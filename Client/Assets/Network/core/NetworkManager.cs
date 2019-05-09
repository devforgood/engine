using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using uint16_t = System.UInt16;

namespace core
{
    public enum PacketType
    {
        kHelloCC    = 1212501071,
        kWelcomeCC  = 1464615757,
        kStateCC    = 1398030676,
        kInputCC    = 1229869140,
        kRPC        = 1111111111,
    }

    public class NetworkManager
    {
        static readonly int kMaxPacketsPerFrameCount = 10;
        static readonly int kMaxBufferSize = 1500;



        protected NetPeer mNetPeer;

        WeightedTimedMovingAverage mBytesReceivedPerSecond;
        WeightedTimedMovingAverage mBytesSentPerSecond;


        int mBytesSentThisFrame;

        float mDropPacketChance;
        float mSimulatedLatency;

        class ReceivedPacket
        {
            public ReceivedPacket(float inReceivedTime, NetIncomingMessage inInputMemoryBitStream)
            {
                mReceivedTime = inReceivedTime;
                mPacketBuffer = inInputMemoryBitStream;
            }

            public float GetReceivedTime() { return mReceivedTime; }
            public NetIncomingMessage GetPacketBuffer() { return mPacketBuffer; }

            float mReceivedTime;
            NetIncomingMessage mPacketBuffer;
        };

        Queue<ReceivedPacket> mPacketQueue = new Queue<ReceivedPacket>();
        //Stack<ReceivedPacket> mPacketQueue = new Stack<ReceivedPacket>();

        protected Dictionary<int, NetGameObject> mNetworkIdToGameObjectMap = new Dictionary<int, NetGameObject>();



        public NetworkManager()
        {
            mBytesSentThisFrame = 0;
            mDropPacketChance = 0.0f;
            mSimulatedLatency = 0.0f;

        }
        ~NetworkManager()
        {

        }

        public bool Init(uint16_t inPort, bool reBind = false)
        {


            if(inPort == 0)
            {
                // client
                NetPeerConfiguration config = new NetPeerConfiguration("game");
                //config.AutoFlushSendQueue = false;
                mNetPeer = new NetClient(config);
            }
            else
            {
                // server
                NetPeerConfiguration config = new NetPeerConfiguration("game");
                config.MaximumConnections = 1000;
                config.Port = inPort;
                mNetPeer = new NetServer(config);
            }

            mNetPeer.Start();


            //LOG("Initializing NetworkManager at port %d", inPort);

            mBytesReceivedPerSecond = new WeightedTimedMovingAverage(1.0f);
            mBytesSentPerSecond = new WeightedTimedMovingAverage(1.0f);


            return true;
        }
        public virtual void ProcessPacket(NetIncomingMessage inInputStream, System.Net.IPEndPoint inFromAddress) { }
        public virtual void HandleConnectionReset(System.Net.IPEndPoint inFromAddress) { }

        public void ProcessIncomingPackets()
        {
             ProcessQueuedPackets();

            UpdateBytesSentLastFrame();

        }


        public void ProcessQueuedPackets()
        {

            int totalReadByteCount = 0;

            NetIncomingMessage im;
            while ((im = mNetPeer.ReadMessage()) != null)
            {

                // handle incoming message
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        string text = im.ReadString();
                        LogHelper.LogInfo(text);
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                        string reason = im.ReadString();
                        LogHelper.LogInfo("status " + NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason);

                        if (status == NetConnectionStatus.Connected)
                        {
                            //LogHelper.LogInfo("Remote hail: " + im.SenderConnection.RemoteHailMessage.ReadString());
                        }

                        //UpdateConnectionsList();
                        break;
                    case NetIncomingMessageType.Data:
                        totalReadByteCount += im.LengthBytes;

                        ProcessPacket(im, im.SenderEndPoint);

                        break;
                    default:
                        LogHelper.LogInfo("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);
                        break;
                }
                mNetPeer.Recycle(im);
            }

            if (totalReadByteCount > 0)
            {
                mBytesReceivedPerSecond.UpdatePerSecond((float)(totalReadByteCount));
            }
        }

        public void UpdateBytesSentLastFrame()
        {
            if (mBytesSentThisFrame > 0)
            {
                mBytesSentPerSecond.UpdatePerSecond((float)(mBytesSentThisFrame));

                mBytesSentThisFrame = 0;
            }
        }

        public void AddToNetworkIdToGameObjectMap(NetGameObject inGameObject)
        {
            mNetworkIdToGameObjectMap[inGameObject.GetNetworkId()] = inGameObject;
        }

        public void RemoveFromNetworkIdToGameObjectMap(NetGameObject inGameObject)
        {
            mNetworkIdToGameObjectMap.Remove(inGameObject.GetNetworkId());
        }

        public WeightedTimedMovingAverage GetBytesReceivedPerSecond() { return mBytesReceivedPerSecond; }
        public WeightedTimedMovingAverage GetBytesSentPerSecond() { return mBytesSentPerSecond; }

        public void SetDropPacketChance(float inChance) { mDropPacketChance = inChance; }
        public void SetSimulatedLatency(float inLatency) { mSimulatedLatency = inLatency; }

        public NetGameObject GetGameObject(int inNetworkId)
        {
            NetGameObject gameObjectIt = null;
            if (mNetworkIdToGameObjectMap.TryGetValue(inNetworkId, out gameObjectIt) == true)
            {
                return gameObjectIt;
            }
            else
            {
                return null;
            }
        }


    }
}

