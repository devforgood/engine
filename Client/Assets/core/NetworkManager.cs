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
        kHelloCC,
        kWelcomeCC,
        kStateCC,
        kInputCC,
    }

    public class NetworkManager
    {
        static readonly int kMaxPacketsPerFrameCount = 10;
        static readonly int kMaxBufferSize = 1500;

        protected Socket mSocket;
        byte[] receiveBuffer = new byte[kMaxBufferSize];
        System.Net.EndPoint senderRemote = new System.Net.IPEndPoint(0, 0);

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
            NetPeerConfiguration config = new NetPeerConfiguration("chat");
            config.MaximumConnections = 100;
            config.Port = inPort;

            try
            {

                if (mSocket == null)
                    mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                if (reBind)
                    mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, (int)1);

                mSocket.ReceiveBufferSize = kMaxBufferSize;
                mSocket.SendBufferSize = kMaxBufferSize;
                mSocket.Blocking = false;

                if (inPort != 0)
                {
                    var ep = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 65000);
                    mSocket.Bind(ep);
                }
            }
            catch( SocketException ex)
            {

            }


            //LOG("Initializing NetworkManager at port %d", inPort);

            mBytesReceivedPerSecond = new WeightedTimedMovingAverage(1.0f);
            mBytesSentPerSecond = new WeightedTimedMovingAverage(1.0f);


            return true;
        }
        public virtual void ProcessPacket(NetIncomingMessage inInputStream, System.Net.IPEndPoint inFromAddress) { }
        public virtual void HandleConnectionReset(System.Net.IPEndPoint inFromAddress) { }

        public void ProcessIncomingPackets()
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

                int bytesReceived = 0;
                try
                {
                    if (!mSocket.Poll(1, SelectMode.SelectRead))
                        return;

                    bytesReceived = mSocket.ReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref senderRemote);
                }
                catch (SocketException sx)
                {
                    switch (sx.SocketErrorCode)
                    {
                        case SocketError.ConnectionReset:
                            // connection reset by peer, aka connection forcibly closed aka "ICMP port unreachable"
                            // we should shut down the connection; but m_senderRemote seemingly cannot be trusted, so which connection should we shut down?!
                            // So, what to do?
                            //LogWarning("ConnectionReset");
                            return;

                        case SocketError.NotConnected:
                            // socket is unbound; try to rebind it (happens on mobile when process goes to sleep)
                            //BindSocket(true);
                            return;
                        case SocketError.WouldBlock:


                            return;
                        default:
                            //LogWarning("Socket exception: " + sx.ToString());
                            return;
                    }
                }

                var inputStream = new NetIncomingMessage();
                inputStream.Data = receiveBuffer;
                inputStream.LengthBytes = bytesReceived;
                inputStream.SenderEndPoint = (System.Net.IPEndPoint)senderRemote;

                // realloc
                receiveBuffer = new byte[kMaxBufferSize];

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
            try
            {
                int bytesSent = mSocket.SendTo(inOutputStream.Data, 0, inOutputStream.Data.Length, SocketFlags.None, inFromAddress);

                if (bytesSent > 0)
                {
                    mBytesSentThisFrame += inOutputStream.LengthBytes;
                }
            }
            catch (SocketException ex)
            {

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

