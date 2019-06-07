using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
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
    static readonly float kTimeBetweenInputPackets = 0.033f; // 30 FPS


    /// <summary>
    /// Global instance of NetworkManagerClient
    /// </summary>
    public static NetworkManagerClient sInstance = new NetworkManagerClient();

    public static void StaticInit(System.Net.IPEndPoint inServerAddress, string inName)
    {
        if (sInstance == null)
        {
            sInstance = new NetworkManagerClient();
        }
        sInstance.Init(inServerAddress, inName);
    }

    core.DeliveryNotificationManager mDeliveryNotificationManager;
    public core.DeliveryNotificationManager DeliveryNotificationManager { get { return mDeliveryNotificationManager; } }

    ReplicationManagerClient mReplicationManagerClient = new ReplicationManagerClient();

    public NetClient GetClient() { return (NetClient)mNetPeer; }

    System.Net.IPEndPoint mServerAddress;
    public System.Net.IPEndPoint ServerAddress { get { return mServerAddress; } }


    NetworkClientState mState;

    float mTimeOfLastHello;
    float mTimeOfLastInputPacket;

    string mName;
    int mPlayerId;
    byte mWorldId;

    float mLastMoveProcessedByServerTimestamp;

    core.WeightedTimedMovingAverage mAvgRoundTripTime;
    float mLastRoundTripTime;

    public void SetWorldId(byte worldId) { mWorldId = worldId;}

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
            case core.PacketType.kRPC:
                HandleRPCPacket(inInputStream);
                break;
        }
    }

    public core.WeightedTimedMovingAverage GetAvgRoundTripTime() { return mAvgRoundTripTime; }
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

        mAvgRoundTripTime = new core.WeightedTimedMovingAverage(1.0f);

        NetOutgoingMessage hail = GetClient().CreateMessage("hail");
        GetClient().Connect(mServerAddress, hail);
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
        NetOutgoingMessage helloPacket = GetClient().CreateMessage();

        helloPacket.Write((UInt32)core.PacketType.kHelloCC);
        helloPacket.Write(mName);
        helloPacket.Write(mWorldId);

        Debug.Log(string.Format("Send hello {0}, {1}", mName, mWorldId));
        GetClient().SendMessage(helloPacket, NetDeliveryMethod.Unreliable);
    }

    void HandleWelcomePacket(NetIncomingMessage inInputStream)
    {
        if (mState == NetworkClientState.NCS_SayingHello)
        {
            //if we got a player id, we've been welcomed!
            int playerId = (int)inInputStream.ReadUInt32();
            mPlayerId = playerId;
            mState = NetworkClientState.NCS_Welcomed;
            core.Engine.sInstance.ServerClientId = mPlayerId;
            //LOG("'%s' was welcomed on client as player %d", mName.c_str(), mPlayerId);
        }
    }
    void HandleStatePacket(NetIncomingMessage inInputStream)
    {
        if (mState == NetworkClientState.NCS_Welcomed)
        {
            ReadLastMoveProcessedOnServerTimestamp(inInputStream);

            HandleScoreBoardState(inInputStream);

            //tell the replication manager to handle the rest...
            mReplicationManagerClient.Read(inInputStream);
        }
    }

    void HandleRPCPacket(NetIncomingMessage inInputStream)
    {
        Debug.Log("Handle RPC Packet ");

        int networkId = inInputStream.ReadInt32();
        ulong hash = inInputStream.ReadUInt64();

        core.NetGameObject obj;
        if (mNetworkIdToGameObjectMap.TryGetValue(networkId, out obj) == true)
        {
            obj.OnRemoteClientRPC(hash, 0, inInputStream);
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

    void HandleScoreBoardState(NetIncomingMessage inInputStream)
    {
        core.GameMode.sInstance.Read(inInputStream);
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

    static int debug_index = 0;

    void SendInputPacket()
    {
        //only send if there's any input to sent!
        var moveList = InputManager.sInstance.GetMoveList();

        if (moveList.HasMoves())
        {
            NetOutgoingMessage inputPacket = GetClient().CreateMessage();

            inputPacket.Write((UInt32)core.PacketType.kInputCC);

            mDeliveryNotificationManager.WriteState(inputPacket);

            //eventually write the 3 latest moves so they have three chances to get through...
            int moveCount = moveList.GetMoveCount();
            int firstMoveIndex = moveCount - 3;
            if (firstMoveIndex < 0)
            {
                firstMoveIndex = 0;
            }
            //auto move = moveList.begin() + firstMoveIndex;

            //only need two bits to write the move count, because it's 0, 1, 2 or 3
            inputPacket.Write((uint32_t)(moveCount - firstMoveIndex), 2);

            ++debug_index;
            for (; firstMoveIndex < moveCount; ++firstMoveIndex)
            {
                //Debug.Log("send move info " + debug_index + ", moveCount : " +  moveCount + ", firstMoveIndex :" + firstMoveIndex + ", timestamp:" + moveList.mMoves[firstMoveIndex].GetTimestamp() + ", delta:" + moveList.mMoves[firstMoveIndex].GetDeltaTime());
                ///would be nice to optimize the time stamp...
                moveList.mMoves[firstMoveIndex].Write(inputPacket);
            }

            GetClient().SendMessage(inputPacket, NetDeliveryMethod.Unreliable);
        }
    }
}
