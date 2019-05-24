using core;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uint16_t = System.UInt16;
using uint32_t = System.UInt32;

namespace Server
{
    public class NetworkManagerServer : NetworkManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Global instance of NetworkManagerServer
        /// </summary>
        public static NetworkManagerServer sInstance = new NetworkManagerServer();
        public static bool StaticInit(uint16_t inPort)
        {
            sInstance = new NetworkManagerServer();
            return sInstance.Init(inPort);
        }

        public NetPeer GetServer() { return (NetServer)mNetPeer; }


        int mNewPlayerId;
        int mNewNetworkId;

        float mTimeOfLastSatePacket;
        float mTimeBetweenStatePackets;
        float mClientDisconnectTimeout;


        Dictionary<System.Net.IPEndPoint, ClientProxy> mAddressToClientMap = new Dictionary<System.Net.IPEndPoint, ClientProxy>();
        Dictionary<int, ClientProxy> mPlayerIdToClientMap = new Dictionary<int, ClientProxy>();



        NetworkManagerServer()
        {
            mNewPlayerId = 1;
            mNewNetworkId = 1;
            mTimeBetweenStatePackets = 0.033f;
            mClientDisconnectTimeout = 3.0f;
        }

        public NetGameObject RegisterAndReturn(NetGameObject inGameObject)
        {
            RegisterGameObject(inGameObject);
            return inGameObject;
        }
        public override void HandleConnectionReset(System.Net.IPEndPoint inFromAddress)
        {
            //just dc the client right away...
            ClientProxy c = null;
            if (mAddressToClientMap.TryGetValue(inFromAddress, out c) == true)
            {
                HandleClientDisconnected(c);
            }
        }

        public override void ProcessPacket(NetIncomingMessage inInputStream, System.Net.IPEndPoint inFromAddress)
        {
            //try to get the client proxy for this address
            //pass this to the client proxy to process
            ClientProxy c = null;
            if (mAddressToClientMap.TryGetValue(inFromAddress, out c) == false)
            {
                //didn't find one? it's a new cilent..is the a HELO? if so, create a client proxy...
                HandlePacketFromNewClient(inInputStream, inFromAddress);
            }
            else
            {
                ProcessPacket(c, inInputStream);
            }
        }

        public void SendOutgoingPackets()
        {
            float time = Timing.sInstance.GetTimef();

            //let's send a client a state packet whenever their move has come in...
            foreach (var c in mAddressToClientMap)
            {
                var clientProxy = c.Value;
                //process any timed out packets while we're going through the list
                clientProxy.GetDeliveryNotificationManager().ProcessTimedOutPackets();

                if (clientProxy.IsLastMoveTimestampDirty())
                {
                    SendStatePacketToClient(clientProxy);
                }
            }
        }
        public void CheckForDisconnects()
        {
            List<ClientProxy> clientsToDC = new List<ClientProxy>();

            float minAllowedLastPacketFromClientTime = Timing.sInstance.GetTimef() - mClientDisconnectTimeout;
            foreach (var pair in mAddressToClientMap)
            {
                if (pair.Value.GetLastPacketFromClientTime() < minAllowedLastPacketFromClientTime)
                {
                    //can't remove from map while in iterator, so just remember for later...
                    clientsToDC.Add(pair.Value);
                }
            }

            foreach (var client in clientsToDC)
            {
                HandleClientDisconnected(client);
            }
        }

        public void RegisterGameObject(NetGameObject inGameObject)
        {
            //assign network id
            int newNetworkId = GetNewNetworkId();
            inGameObject.SetNetworkId(newNetworkId);

            //add mapping from network id to game object
            mNetworkIdToGameObjectMap[newNetworkId] = inGameObject;

            //tell all client proxies this is new...
            foreach (var pair in mAddressToClientMap)
            {
                pair.Value.GetReplicationManagerServer().ReplicateCreate(newNetworkId, inGameObject.GetAllStateMask());
            }
        }
        public void UnregisterGameObject(NetGameObject inGameObject)
        {
            int networkId = inGameObject.GetNetworkId();
            mNetworkIdToGameObjectMap.Remove(networkId);

            //tell all client proxies to STOP replicating!
            //tell all client proxies this is new...
            foreach (var pair in mAddressToClientMap)
            {
                pair.Value.GetReplicationManagerServer().ReplicateDestroy(networkId);
            }
        }
        public void SetStateDirty(int inNetworkId, uint32_t inDirtyState)
        {
            //tell everybody this is dirty
            foreach (var pair in mAddressToClientMap)
            {
                pair.Value.GetReplicationManagerServer().SetStateDirty(inNetworkId, inDirtyState);
            }
        }

        public void RespawnActors()
        {
            foreach (var c in mAddressToClientMap)
            {
                c.Value.RespawnActorIfNecessary();
            }
        }

        public ClientProxy GetClientProxy(int inPlayerId)
        {
            ClientProxy c = null;
            if (mPlayerIdToClientMap.TryGetValue(inPlayerId, out c) == true)
            {
                return c;
            }

            return null;
        }

        public int SendPacket(int inPlayerId, NetBuffer inOutputStream)
        {
            var c = GetClientProxy(inPlayerId);
            if(c!=null)
            {
                return (int)GetServer().SendMessage((NetOutgoingMessage)inOutputStream, c.mConnection, NetDeliveryMethod.ReliableSequenced);
            }
            return -1;
        }


        void ProcessPacket(ClientProxy inClientProxy, NetIncomingMessage inInputStream)
        {
            //remember we got a packet so we know not to disconnect for a bit
            inClientProxy.UpdateLastPacketTime();

            uint32_t packetType;
            packetType = inInputStream.ReadUInt32();
            switch ((PacketType)packetType)
            {
                case PacketType.kHelloCC:
                    //need to resend welcome. to be extra safe we should check the name is the one we expect from this address,
                    //otherwise something weird is going on...
                    SendWelcomePacket(inClientProxy);
                    break;
                case PacketType.kInputCC:
                    if (inClientProxy.GetDeliveryNotificationManager().ReadAndProcessState(inInputStream))
                    {
                        HandleInputPacket(inClientProxy, inInputStream);
                    }
                    break;
                case PacketType.kRPC:
                    HandleRPCPacket(inClientProxy, inInputStream);
                    break;
                default:
                    //LOG("Unknown packet type received from %s", inClientProxy.GetSocketAddress().ToString().c_str());
                    break;
            }
        }
        void HandlePacketFromNewClient(NetIncomingMessage inInputStream, System.Net.IPEndPoint inFromAddress)
        {
            //read the beginning- is it a hello?
            uint32_t packetType = inInputStream.ReadUInt32();
            if (packetType == (uint32_t)PacketType.kHelloCC)
            {
                //read the name
                string name = inInputStream.ReadString();
                ClientProxy newClientProxy = new ClientProxy(inFromAddress, name, mNewPlayerId++);
                newClientProxy.mConnection = inInputStream.SenderConnection;
                mAddressToClientMap[inFromAddress] = newClientProxy;
                mPlayerIdToClientMap[newClientProxy.GetPlayerId()] = newClientProxy;


                log.Info(string.Format("HandlePacketFromNewClient new client {0} as player {1}, addr_map{2}, id_map{3}", newClientProxy.GetName(), newClientProxy.GetPlayerId(), mAddressToClientMap.Count, mPlayerIdToClientMap.Count));


                //tell the server about this client, spawn a cat, etc...
                //if we had a generic message system, this would be a good use for it...
                //instead we'll just tell the server directly
                ((Server)Engine.sInstance).HandleNewClient(newClientProxy);

                //and welcome the client...
                SendWelcomePacket(newClientProxy);

                //and now init the replication manager with everything we know about!
                foreach (var pair in mNetworkIdToGameObjectMap)
                {
                    newClientProxy.GetReplicationManagerServer().ReplicateCreate(pair.Key, pair.Value.GetAllStateMask());
                }
            }
            else
            {
                //bad incoming packet from unknown client- we're under attack!!
                //LOG("Bad incoming packet from unknown client at socket %s", inFromAddress);
            }
        }

        void SendWelcomePacket(ClientProxy inClientProxy)
        {
            var welcomePacket = GetServer().CreateMessage();

            welcomePacket.Write((uint32_t)PacketType.kWelcomeCC);
            welcomePacket.Write(inClientProxy.GetPlayerId());

            log.Info(string.Format("Server Welcoming, new client {0} as player {1}", inClientProxy.GetName(), inClientProxy.GetPlayerId()));

            GetServer().SendMessage(welcomePacket, inClientProxy.mConnection, NetDeliveryMethod.Unreliable);


        }
        void UpdateAllClients()
        {
            foreach (var it in mAddressToClientMap)
            {
                //process any timed out packets while we're going throug hthe list
                it.Value.GetDeliveryNotificationManager().ProcessTimedOutPackets();

                SendStatePacketToClient(it.Value);
            }
        }

        void AddWorldStateToPacket(NetOutgoingMessage inOutputStream)
        {
            var gameObjects = World.sInstance.GetGameObjects();

            //now start writing objects- do we need to remember how many there are? we can check first...
            inOutputStream.Write(gameObjects.Count);

            foreach (var gameObject in gameObjects)
            {
                inOutputStream.Write(gameObject.GetNetworkId());
                inOutputStream.Write(gameObject.GetClassId());
                gameObject.Write(inOutputStream, 0xffffffff);
            }
        }
        void AddScoreBoardStateToPacket(NetOutgoingMessage inOutputStream)
        {
            ScoreBoardManager.sInstance.Write(inOutputStream);

        }

        void SendStatePacketToClient(ClientProxy inClientProxy)
        {
            //build state packet
            var statePacket = GetServer().CreateMessage();

            //it's state!
            statePacket.Write((uint32_t)PacketType.kStateCC);

            InFlightPacket ifp = inClientProxy.GetDeliveryNotificationManager().WriteState(statePacket);

            WriteLastMoveTimestampIfDirty(statePacket, inClientProxy);

            AddScoreBoardStateToPacket(statePacket);

            var rmtd = new ReplicationManagerTransmissionData(inClientProxy.GetReplicationManagerServer());
            inClientProxy.GetReplicationManagerServer().Write(statePacket, rmtd);
            ifp.SetTransmissionData((int)TransmissionDataType.kReplicationManager, rmtd);

            var ret = GetServer().SendMessage(statePacket, inClientProxy.mConnection, NetDeliveryMethod.Unreliable);
            //log.InfoFormat("send {0}", ret);
        }


        void WriteLastMoveTimestampIfDirty(NetOutgoingMessage inOutputStream, ClientProxy inClientProxy)
        {
            //first, dirty?
            bool isTimestampDirty = inClientProxy.IsLastMoveTimestampDirty();
            inOutputStream.Write(isTimestampDirty);
            if (isTimestampDirty)
            {
                inOutputStream.Write(inClientProxy.GetUnprocessedMoveList().GetLastMoveTimestamp());
                inClientProxy.SetIsLastMoveTimestampDirty(false);
            }
        }

        void HandleInputPacket(ClientProxy inClientProxy, NetIncomingMessage inInputStream)
        {
            Move move = new Move();
            uint32_t moveCount = inInputStream.ReadUInt32(2);

            for (; moveCount > 0; --moveCount)
            {
                if (move.Read(inInputStream))
                {
                    //log.InfoFormat("recv move {0}, {1}, {2}, {3}", move.GetDeltaTime(), move.GetInputState(), move.GetTimestamp(), moveCount);
                    if (inClientProxy.GetUnprocessedMoveList().AddMoveIfNew(move))
                    {
                        inClientProxy.SetIsLastMoveTimestampDirty(true);
                    }
                }
            }
        }

        void HandleRPCPacket(ClientProxy inClientProxy, NetIncomingMessage inInputStream)
        {
            int networkId = inInputStream.ReadInt32();
            ulong hash = inInputStream.ReadUInt64();
            int senderClientId = inClientProxy.GetPlayerId();

            NetGameObject obj;
            if(mNetworkIdToGameObjectMap.TryGetValue(networkId, out obj)==true)
            {
                obj.OnRemoteServerRPC(hash, senderClientId, inInputStream);
            }
        }

        void HandleClientDisconnected(ClientProxy inClientProxy)
        {
            mPlayerIdToClientMap.Remove(inClientProxy.GetPlayerId());
            mAddressToClientMap.Remove(inClientProxy.GetSocketAddress());
            ((Server)(Engine.sInstance)).HandleLostClient(inClientProxy);

            log.Info(string.Format("HandleClientDisconnected client {0} as player {1}, addr_map{2}, id_map{3}", inClientProxy.GetName(), inClientProxy.GetPlayerId(), mAddressToClientMap.Count, mPlayerIdToClientMap.Count));


            //was that the last client? if so, bye!
            if (mAddressToClientMap.Count == 0)
            {
                //Engine.sInstance.SetShouldKeepRunning(false);
            }
        }

        int GetNewNetworkId()
        {
            int toRet = mNewNetworkId++;
            if (mNewNetworkId < toRet)
            {
                //LOG("Network ID Wrap Around!!! You've been playing way too long...", 0);
            }

            return toRet;
        }




    }
}
