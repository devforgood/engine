using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using uint32_t = System.UInt32;

public class NetworkManagerClient : core.NetworkManager
{
    enum NetworkClientState
    {
        NCS_Uninitialized,
        NCS_SayingHello,
        NCS_Welcomed
    };

    static readonly float kTimeBetweenHellos = 1.0f;
    static readonly float kTimeBetweenInputPackets = 0.033f;


    /// <summary>
    /// Global instance of NetworkManagerClient
    /// </summary>
    public static NetworkManagerClient sInstance = new NetworkManagerClient();

    public static void StaticInit(System.Net.IPEndPoint inServerAddress, string inName)
    {
        sInstance = new NetworkManagerClient();
        sInstance.Init(inServerAddress, inName);
    }

    core.DeliveryNotificationManager mDeliveryNotificationManager;
    ReplicationManagerClient mReplicationManagerClient = new ReplicationManagerClient();

    System.Net.IPEndPoint mServerAddress;

    NetworkClientState mState;

    float mTimeOfLastHello;
    float mTimeOfLastInputPacket;

    string mName;
    int mPlayerId;

    float mLastMoveProcessedByServerTimestamp;

    WeightedTimedMovingAverage mAvgRoundTripTime;
    float mLastRoundTripTime;

    public void SendOutgoingPackets()
    {
        switch (mState)
        {
            case NetworkClientState.NCS_SayingHello:
                UpdateSayingHello();
                break;
            case NetworkClientState.NCS_Welcomed:
                UpdateSendingInputPacket();
                break;
        }
    }

    public override void ProcessPacket(NetIncomingMessage inInputStream, System.Net.IPEndPoint inFromAddress)
    {
        var packetType = (core.PacketType)inInputStream.ReadUInt32();
        switch (packetType)
        {
            case core.PacketType.kWelcomeCC:
                HandleWelcomePacket(inInputStream);
                break;
            case core.PacketType.kStateCC:
                if (mDeliveryNotificationManager.ReadAndProcessState(inInputStream))
                {
                    HandleStatePacket(inInputStream);
                }
                break;
        }
    }

    public WeightedTimedMovingAverage GetAvgRoundTripTime() { return mAvgRoundTripTime; }
    public float GetRoundTripTime() { return mAvgRoundTripTime.GetValue(); }
    public int GetPlayerId() { return mPlayerId; }
    public float GetLastMoveProcessedByServerTimestamp() { return mLastMoveProcessedByServerTimestamp; }


    NetworkManagerClient()
    {
        mState = NetworkClientState.NCS_Uninitialized;
        mDeliveryNotificationManager = new core.DeliveryNotificationManager(true, false);
        mLastRoundTripTime = 0.0f;
    }
    void Init(System.Net.IPEndPoint inServerAddress, string inName)
    {
        base.Init(0);

        mServerAddress = inServerAddress;
        mState = NetworkClientState.NCS_SayingHello;
        mTimeOfLastHello = 0.0f;
        mName = inName;

        mAvgRoundTripTime = new WeightedTimedMovingAverage(1.0f);

        mSocket.Connect(mServerAddress);
    }

    void UpdateSayingHello()
    {

        float time = core.Timing.sInstance.GetTimef();

        if (time > mTimeOfLastHello + kTimeBetweenHellos)
        {
            SendHelloPacket();
            mTimeOfLastHello = time;
        }

    }
    void SendHelloPacket()
    {
        NetOutgoingMessage helloPacket = new NetOutgoingMessage();

        helloPacket.Write((UInt32)core.PacketType.kHelloCC);
        helloPacket.Write(mName);

        SendPacket(helloPacket, mServerAddress);
    }

    void HandleWelcomePacket(NetIncomingMessage inInputStream)
    {
        if (mState == NetworkClientState.NCS_SayingHello)
        {
            //if we got a player id, we've been welcomed!
            int playerId = (int)inInputStream.ReadUInt32();
            mPlayerId = playerId;
            mState = NetworkClientState.NCS_Welcomed;
            //LOG("'%s' was welcomed on client as player %d", mName.c_str(), mPlayerId);
        }
    }
    void HandleStatePacket(NetIncomingMessage inInputStream)
    {
        if (mState == NetworkClientState.NCS_Welcomed)
        {
            ReadLastMoveProcessedOnServerTimestamp(inInputStream);

            //old
            //HandleGameObjectState( inPacketBuffer );
            HandleScoreBoardState(inInputStream);

            //tell the replication manager to handle the rest...
            mReplicationManagerClient.Read(inInputStream);
        }
    }
    void ReadLastMoveProcessedOnServerTimestamp(NetIncomingMessage inInputStream)
    {
        bool isTimestampDirty = inInputStream.ReadBoolean();
        if (isTimestampDirty)
        {
            mLastMoveProcessedByServerTimestamp = inInputStream.ReadFloat();

            float rtt = core.Timing.sInstance.GetFrameStartTime() - mLastMoveProcessedByServerTimestamp;
            mLastRoundTripTime = rtt;
            mAvgRoundTripTime.Update(rtt);

            InputManager.sInstance.GetMoveList().RemovedProcessedMoves(mLastMoveProcessedByServerTimestamp);

        }
    }

    void HandleGameObjectState(NetIncomingMessage inInputStream)
    {
        //copy the mNetworkIdToGameObjectMap so that anything that doesn't get an updated can be destroyed...
        var objectsToDestroy = mNetworkIdToGameObjectMap.ToDictionary(x => x.Key, x => x.Value);

        int stateCount = inInputStream.ReadInt32();
        if (stateCount > 0)
        {
            for (int stateIndex = 0; stateIndex < stateCount; ++stateIndex)
            {
                int networkId;
                uint32_t fourCC;

                networkId = inInputStream.ReadInt32();
                fourCC = inInputStream.ReadUInt32();
                core.NetGameObject go = null;
                //didn't find it, better create it!
                if (mNetworkIdToGameObjectMap.TryGetValue(networkId, out go) == false)
                {
                    go = core.GameObjectRegistry.sInstance.CreateGameObject(fourCC);
                    go.SetNetworkId(networkId);
                    AddToNetworkIdToGameObjectMap(go);
                }


                //now we can update into it
                go.Read(inInputStream);
                objectsToDestroy.Remove(networkId);
            }
        }

        //anything left gets the axe
        DestroyGameObjectsInMap(objectsToDestroy);
    }
    void HandleScoreBoardState(NetIncomingMessage inInputStream)
    {
        core.ScoreBoardManager.sInstance.Read(inInputStream);
    }

    void UpdateSendingInputPacket()
    {
        float time = core.Timing.sInstance.GetTimef();

        if (time > mTimeOfLastInputPacket + kTimeBetweenInputPackets)
        {
            SendInputPacket();
            mTimeOfLastInputPacket = time;
        }
    }
    void SendInputPacket()
    {
        //only send if there's any input to sent!
        var moveList = InputManager.sInstance.GetMoveList();

        if (moveList.HasMoves())
        {
            NetOutgoingMessage inputPacket = new NetOutgoingMessage();

            inputPacket.Write((UInt32)core.PacketType.kInputCC);

            mDeliveryNotificationManager.WriteState(inputPacket);

            //eventually write the 3 latest moves so they have three chances to get through...
            int moveCount = moveList.GetMoveCount();
            int firstMoveIndex = moveCount - 3;
            if (firstMoveIndex < 3)
            {
                firstMoveIndex = 0;
            }
            //auto move = moveList.begin() + firstMoveIndex;

            //only need two bits to write the move count, because it's 0, 1, 2 or 3
            inputPacket.Write(moveCount - firstMoveIndex, 2);

            for (; firstMoveIndex < moveCount; ++firstMoveIndex)
            {
                ///would be nice to optimize the time stamp...
                moveList.Moves[firstMoveIndex].Write(inputPacket);
            }

            SendPacket(inputPacket, mServerAddress);
        }
    }

    void DestroyGameObjectsInMap(Dictionary<int, core.NetGameObject> inObjectsToDestroy)
    {
        foreach (var pair in inObjectsToDestroy)
        {
            pair.Value.SetDoesWantToDie(true);
            //and remove from our map!
            mNetworkIdToGameObjectMap.Remove(pair.Key);
        }

    }
}
