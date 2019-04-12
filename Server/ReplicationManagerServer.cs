using core;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uint32_t = System.UInt32;

namespace Server
{
    public class ReplicationManagerServer
    {
        Dictionary<int, ReplicationCommand> mNetworkIdToReplicationCommand = new Dictionary<int, ReplicationCommand>();
        public void ReplicateCreate(int inNetworkId, uint32_t inInitialDirtyState)
        {
            mNetworkIdToReplicationCommand[inNetworkId] = new ReplicationCommand(inInitialDirtyState);
        }

        public void ReplicateDestroy(int inNetworkId)
        {
            //it's broken if we don't find it...
            mNetworkIdToReplicationCommand[inNetworkId].SetDestroy();
        }

        public void RemoveFromReplication(int inNetworkId)
        {
            mNetworkIdToReplicationCommand.Remove(inNetworkId);
        }

        public void SetStateDirty(int inNetworkId, uint32_t inDirtyState)
        {
            mNetworkIdToReplicationCommand[inNetworkId].AddDirtyState(inDirtyState);
        }

        public void HandleCreateAckd(int inNetworkId)
        {
            mNetworkIdToReplicationCommand[inNetworkId].HandleCreateAckd();
        }

        uint32_t WriteCreateAction(NetOutgoingMessage inOutputStream, int inNetworkId, uint32_t inDirtyState)
        {
            //need object
            GameObject gameObject = NetworkManagerServer.sInstance.GetGameObject(inNetworkId);
            //need 4 cc
            inOutputStream.Write(gameObject.GetClassId());
            return gameObject.Write(inOutputStream, inDirtyState);
        }
        uint32_t WriteUpdateAction(NetOutgoingMessage inOutputStream, int inNetworkId, uint32_t inDirtyState)
        {
            //need object
            GameObject gameObject = NetworkManagerServer.sInstance.GetGameObject(inNetworkId);

            //if we can't find the gameObject on the other side, we won't be able to read the written data ( since we won't know which class wrote it )
            //so we need to know how many bytes to skip.


            //this means we need byte sand each new object needs to be byte aligned

            uint32_t writtenState = gameObject.Write(inOutputStream, inDirtyState);

            return writtenState;
        }
        uint32_t WriteDestroyAction(NetOutgoingMessage inOutputStream, int inNetworkId, uint32_t inDirtyState)
        {
            //don't have to do anything- action already written

            return inDirtyState;
        }


    }
}
