using core;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ClientProxy
    {
        static readonly float kRespawnDelay = 3.0f;

        public NetConnection mConnection;

        DeliveryNotificationManager mDeliveryNotificationManager;
        ReplicationManagerServer mReplicationManagerServer = new ReplicationManagerServer();

        System.Net.IPEndPoint mSocketAddress;
        string mName;
        int mPlayerId;
        byte mWorldId;

        //going away!
        InputState mInputState;

        float mLastPacketFromClientTime;
        float mTimeToRespawn;

        MoveList mUnprocessedMoveList = new MoveList();
        bool mIsLastMoveTimestampDirty;

        public ClientProxy(System.Net.IPEndPoint inSocketAddress, string inName, int inPlayerId, byte worldId)
        {
            mSocketAddress = inSocketAddress;
            mName = inName;
            mPlayerId = inPlayerId;
            mDeliveryNotificationManager = new DeliveryNotificationManager(false, true);
            mIsLastMoveTimestampDirty = false;
            mTimeToRespawn = 0.0f;
            mWorldId = worldId;

            UpdateLastPacketTime();
        }


        public void UpdateLastPacketTime()
        {
            mLastPacketFromClientTime = Timing.sInstance.GetTimef();
        }

        public void HandleActorDied()
        {
            mTimeToRespawn = Timing.sInstance.GetFrameStartTime() + kRespawnDelay;
        }

        public void RespawnActorIfNecessary()
        {
            if (mTimeToRespawn != 0.0f && Timing.sInstance.GetFrameStartTime() > mTimeToRespawn)
            {
                ((Server)(Engine.sInstance)).SpawnActorForPlayer(mPlayerId, mWorldId);
                mTimeToRespawn = 0.0f;
            }
        }
        public System.Net.IPEndPoint GetSocketAddress() { return mSocketAddress; }
        public int GetPlayerId() { return mPlayerId; }
        public byte GetWorldId() { return mWorldId; }
        public string GetName() { return mName; }

        public void SetInputState(InputState inInputState) { mInputState = inInputState; }
        public InputState GetInputState() { return mInputState; }

        public float GetLastPacketFromClientTime() { return mLastPacketFromClientTime; }

        public DeliveryNotificationManager GetDeliveryNotificationManager() { return mDeliveryNotificationManager; }
        public ReplicationManagerServer GetReplicationManagerServer() { return mReplicationManagerServer; }

        public MoveList GetUnprocessedMoveList() { return mUnprocessedMoveList; }

        public void SetIsLastMoveTimestampDirty(bool inIsDirty) { mIsLastMoveTimestampDirty = inIsDirty; }
        public bool IsLastMoveTimestampDirty() { return mIsLastMoveTimestampDirty; }
    }
}
