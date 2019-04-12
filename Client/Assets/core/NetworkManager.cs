using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using uint16_t = System.UInt16;

namespace core
{
    public enum PacketType
    {
        kHelloCC,
        kWelcomeCC,
        kStateCC,
        kInputCC,
    }

    class NetworkManager
    {
        static readonly int kMaxPacketsPerFrameCount = 10;

        NetServer mSocket;

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

        //Queue<ReceivedPacket> mPacketQueue = new Queue<ReceivedPacket>();
        Stack<ReceivedPacket> mPacketQueue = new Stack<ReceivedPacket>();

        protected Dictionary<int, GameObject> mNetworkIdToGameObjectMap = new Dictionary<int, GameObject>();



        public NetworkManager()
        {
            mBytesSentThisFrame = 0;
            mDropPacketChance = 0.0f;
            mSimulatedLatency = 0.0f;

        }
        ~NetworkManager()
        {

        }

        public bool Init(uint16_t inPort)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("chat");
            config.MaximumConnections = 100;
            config.Port = inPort;
            mSocket = new NetServer(config);

            //LOG("Initializing NetworkManager at port %d", inPort);

            mBytesReceivedPerSecond = new WeightedTimedMovingAverage(1.0f);
            mBytesSentPerSecond = new WeightedTimedMovingAverage(1.0f);

            //did we bind okay?
            if (mSocket == null)
            {
                return false;
            }

            return true;
        }
        public virtual void ProcessPacket(NetIncomingMessage inInputStream, System.Net.IPEndPoint inFromAddress) { }
        public virtual void HandleConnectionReset(System.Net.IPEndPoint inFromAddress) { }

        void ProcessIncomingPackets()
        {
            ReadIncomingPacketsIntoQueue();

            ProcessQueuedPackets();

            UpdateBytesSentLastFrame();

        }

        void ReadIncomingPacketsIntoQueue()
        {
            //should we just keep a static one?
            //should we just keep a static one?

            //keep reading until we don't have anything to read ( or we hit a max number that we'll process per frame )
            int receivedPackedCount = 0;
            int totalReadByteCount = 0;

            while (receivedPackedCount < kMaxPacketsPerFrameCount)
            {
                var inputStream = mSocket.ReadMessage();
                if (inputStream == null)
                {
                    //nothing to read
                    break;
                }
                else
                {
                    ++receivedPackedCount;
                    totalReadByteCount += inputStream.LengthBytes;

                    //now, should we drop the packet?
                    if (RoboMath.GetRandomFloat() >= mDropPacketChance)
                    {
                        //we made it
                        //shove the packet into the queue and we'll handle it as soon as we should...
                        //we'll pretend it wasn't received until simulated latency from now
                        //this doesn't sim jitter, for that we would need to.....

                        float simulatedReceivedTime = Timing.sInstance.GetTimef() + mSimulatedLatency;
                        mPacketQueue.Push(new ReceivedPacket(simulatedReceivedTime, inputStream));
                    }
                    else
                    {
                        //LOG("Dropped packet!", 0);
                        //dropped!
                    }
                }
            }

            if (totalReadByteCount > 0)
            {
                mBytesReceivedPerSecond.UpdatePerSecond((float)(totalReadByteCount));
            }
        }

        public void ProcessQueuedPackets()
        {
            //look at the front packet...
            while (mPacketQueue.Count != 0)
            {
                ReceivedPacket nextPacket = mPacketQueue.Peek();
                if (Timing.sInstance.GetTimef() > nextPacket.GetReceivedTime())
                {
                    ProcessPacket(nextPacket.GetPacketBuffer(), nextPacket.GetPacketBuffer().SenderEndPoint);
                    mPacketQueue.Pop();
                }
                else
                {
                    break;
                }
            }
        }


        public void SendPacket(NetOutgoingMessage inOutputStream, System.Net.IPEndPoint inFromAddress)
        {

            mSocket.SendUnconnectedMessage(inOutputStream, inFromAddress);

            if (inOutputStream.LengthBytes > 0)
            {
                mBytesSentThisFrame += inOutputStream.LengthBytes;
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

        public void AddToNetworkIdToGameObjectMap(GameObject inGameObject)
        {
            mNetworkIdToGameObjectMap[inGameObject.GetNetworkId()] = inGameObject;
        }

        public void RemoveFromNetworkIdToGameObjectMap(GameObject inGameObject)
        {
            mNetworkIdToGameObjectMap.Remove(inGameObject.GetNetworkId());
        }

        public WeightedTimedMovingAverage GetBytesReceivedPerSecond() { return mBytesReceivedPerSecond; }
        public WeightedTimedMovingAverage GetBytesSentPerSecond() { return mBytesSentPerSecond; }

        public void SetDropPacketChance(float inChance) { mDropPacketChance = inChance; }
        public void SetSimulatedLatency(float inLatency) { mSimulatedLatency = inLatency; }

        public GameObject GetGameObject(int inNetworkId)
        {
            GameObject gameObjectIt = null;
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

