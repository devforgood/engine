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
        Dictionary<int, ReplicationCommand> mNetworkIdToReplicationCommand;
        uint32_t WriteCreateAction(NetOutgoingMessage inOutputStream, int inNetworkId, uint32_t inDirtyState)
        {
            //need object
            GameObject gameObject = NetworkManagerServer.sInstance.GetGameObject(inNetworkId);
            //need 4 cc
            inOutputStream.Write(gameObject->GetClassId());
            return gameObject->Write(inOutputStream, inDirtyState);
        }
        uint32_t WriteUpdateAction(NetOutgoingMessage inOutputStream, int inNetworkId, uint32_t inDirtyState);
        uint32_t WriteDestroyAction(NetOutgoingMessage inOutputStream, int inNetworkId, uint32_t inDirtyState);


    }
}
