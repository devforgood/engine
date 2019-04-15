using core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uint32_t = System.UInt32;


namespace Server
{
    public class ReplicationManagerTransmissionData : TransmissionData
    {
        class ReplicationTransmission
        {
            public ReplicationTransmission(int inNetworkId, ReplicationAction inAction, uint32_t inState)
            {
                mNetworkId = inNetworkId;
                mAction = inAction;
                mState = inState;
            }
            public int GetNetworkId() { return mNetworkId; }
            public ReplicationAction GetAction() { return mAction; }
            public uint32_t GetState() { return mState; }


            int mNetworkId;
            ReplicationAction mAction;
            uint32_t mState;
        };

        ReplicationManagerServer mReplicationManagerServer;

        List<ReplicationTransmission> mTransmissions = new List<ReplicationTransmission>();
        public ReplicationManagerTransmissionData(ReplicationManagerServer inReplicationManagerServer)
        {
            mReplicationManagerServer = inReplicationManagerServer;
        }

        public void AddTransmission(int inNetworkId, ReplicationAction inAction, uint32_t inState)
        {
            //it would be silly if we already had a transmission for this network id in here...
            //for( const auto& transmission: mTransmissions )
            //{
            //    assert( inNetworkId != transmission.GetNetworkId() );
            //}

            mTransmissions.Add(new ReplicationTransmission(inNetworkId, inAction, inState));

        }

        public override void HandleDeliveryFailure(DeliveryNotificationManager inDeliveryNotificationManager)
        {
            //run through the transmissions
            foreach (var rt in mTransmissions)
            {
                //is it a create? then we have to redo the create.
                int networkId = rt.GetNetworkId();

                switch (rt.GetAction())
                {
                    case ReplicationAction.RA_Create:
                        HandleCreateDeliveryFailure(networkId);
                        break;
                    case ReplicationAction.RA_Update:
                        HandleUpdateStateDeliveryFailure(networkId, rt.GetState(), inDeliveryNotificationManager);
                        break;
                    case ReplicationAction.RA_Destroy:
                        HandleDestroyDeliveryFailure(networkId);
                        break;
                }

            }
        }
        public override void HandleDeliverySuccess(DeliveryNotificationManager inDeliveryNotificationManager)
        {
            //run through the transmissions, if any are Destroyed then we can remove this network id from the map
            foreach (var rt in mTransmissions)
            {
                switch (rt.GetAction())
                {
                    case ReplicationAction.RA_Create:
                        HandleCreateDeliverySuccess(rt.GetNetworkId());
                        break;
                    case ReplicationAction.RA_Destroy:
                        HandleDestroyDeliverySuccess(rt.GetNetworkId());
                        break;
                }
            }
        }

        void HandleCreateDeliveryFailure(int inNetworkId)
        {
            GameObject gameObject = NetworkManagerServer.sInstance.GetGameObject(inNetworkId);
            if (gameObject != null)
            {
                mReplicationManagerServer.ReplicateCreate(inNetworkId, gameObject.GetAllStateMask());
            }
        }
        void HandleUpdateStateDeliveryFailure(int inNetworkId, uint32_t inState, DeliveryNotificationManager inDeliveryNotificationManager)
        {
            //does the object still exist? it might be dead, in which case we don't resend an update
            if (NetworkManagerServer.sInstance.GetGameObject(inNetworkId) != null)
            {
                //look in all future in flight packets, in all transmissions
                //remove written state from dirty state
                foreach (var inFlightPacket in inDeliveryNotificationManager.GetInFlightPackets())
                {
                    ReplicationManagerTransmissionData rmtdp = (ReplicationManagerTransmissionData)(inFlightPacket.GetTransmissionData((int)TransmissionDataType.kReplicationManager));

                    foreach (var otherRT in rmtdp.mTransmissions)
                    {
                        inState &= ~otherRT.GetState();
                    }
                }

                //if there's still any dirty state, mark it
                if (inState != 0)
                {
                    mReplicationManagerServer.SetStateDirty(inNetworkId, inState);
                }
            }
        }
        void HandleDestroyDeliveryFailure(int inNetworkId)
        {
            mReplicationManagerServer.ReplicateDestroy(inNetworkId);
        }
        void HandleCreateDeliverySuccess(int inNetworkId)
        {
            //we've received an ack for the create, so we can start sending as only an update
            mReplicationManagerServer.HandleCreateAckd(inNetworkId);

        }
        void HandleDestroyDeliverySuccess(int inNetworkId)
        {
            mReplicationManagerServer.RemoveFromReplication(inNetworkId);
        }
    }

}
