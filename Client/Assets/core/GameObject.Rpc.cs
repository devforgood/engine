using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    public partial class NetGameObject
    {
        protected bool IsClient => IsRunning && Engine.sInstance.IsClient;
        private bool IsRunning => Engine.sInstance.IsRunning;

        protected bool IsServer => IsRunning && Engine.sInstance.IsServer;

        int OwnerClientId => Engine.sInstance.ServerClientId;

        public virtual void Send(int clientId, NetBuffer inOutputStream)
        {
        }


        #region MESSAGING_SYSTEM
        private static readonly Dictionary<NetGameObject, Dictionary<ulong, ClientRPC>> CachedClientRpcs = new Dictionary<NetGameObject, Dictionary<ulong, ClientRPC>>();
        private static readonly Dictionary<NetGameObject, Dictionary<ulong, ServerRPC>> CachedServerRpcs = new Dictionary<NetGameObject, Dictionary<ulong, ServerRPC>>();
        private static readonly Dictionary<Type, MethodInfo[]> Methods = new Dictionary<Type, MethodInfo[]>();
        private static readonly Dictionary<ulong, string> HashResults = new Dictionary<ulong, string>();
        private static readonly Dictionary<MethodInfo, ulong> methodInfoHashTable = new Dictionary<MethodInfo, ulong>();
        private static readonly StringBuilder methodInfoStringBuilder = new StringBuilder();

        private ulong HashMethodName(string name)
        {
            HashSize mode = HashSize.VarIntTwoBytes;

            if (mode == HashSize.VarIntTwoBytes)
                return name.GetStableHash16();
            if (mode == HashSize.VarIntFourBytes)
                return name.GetStableHash32();
            if (mode == HashSize.VarIntEightBytes)
                return name.GetStableHash64();

            return 0;
        }

        private ulong HashMethod(MethodInfo method)
        {
            if (methodInfoHashTable.ContainsKey(method))
            {
                return methodInfoHashTable[method];
            }
            else
            {
                ulong val = HashMethodName(GetHashableMethodSignature(method));

                methodInfoHashTable.Add(method, val);

                return val;
            }
        }

        private string GetHashableMethodSignature(MethodInfo method)
        {
            methodInfoStringBuilder.Length = 0;
            methodInfoStringBuilder.Append(method.Name);

            ParameterInfo[] parameters = method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                methodInfoStringBuilder.Append(parameters[i].ParameterType.Name);
            }

            return methodInfoStringBuilder.ToString();
        }

        private MethodInfo[] GetNetworkedBehaviorChildClassesMethods(Type type, List<MethodInfo> list = null)
        {
            if (list == null)
            {
                list = new List<MethodInfo>();
                list.AddRange(type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            }
            else
            {
                list.AddRange(type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance));
            }

            if (type.BaseType != null && type.BaseType != typeof(NetGameObject))
            {
                return GetNetworkedBehaviorChildClassesMethods(type.BaseType, list);
            }
            else
            {
                return list.ToArray();
            }
        }

        protected void CacheAttributes()
        {
            Type type = GetType();

            CachedClientRpcs[this] = new Dictionary<ulong, ClientRPC>();
            CachedServerRpcs[this] = new Dictionary<ulong, ServerRPC>();

            MethodInfo[] methods;
            if (Methods.ContainsKey(type)) methods = Methods[type];
            else
            {
                methods = GetNetworkedBehaviorChildClassesMethods(type);
                Methods.Add(type, methods);
            }

            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].IsDefined(typeof(ServerRPC), true))
                {
                    ServerRPC[] attributes = (ServerRPC[])methods[i].GetCustomAttributes(typeof(ServerRPC), true);
                    if (attributes.Length > 1)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Having more than 1 ServerRPC attribute per method is not supported.");
                    }

                    ParameterInfo[] parameters = methods[i].GetParameters();
                    if (parameters.Length == 2 && parameters[0].ParameterType == typeof(ulong) && parameters[1].ParameterType == typeof(NetBuffer) && methods[i].ReturnType == typeof(void))
                    {
                        //use delegate
                        attributes[0].rpcDelegate = (RpcDelegate)Delegate.CreateDelegate(typeof(RpcDelegate), this, methods[i].Name);
                    }
                    else
                    {
                        if (methods[i].ReturnType != typeof(void)
                            //&& !SerializationHelper.IsTypeSupported(methods[i].ReturnType)
                            )
                        {
                            if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogWarning("Invalid return type of RPC. Has to be either void or RpcResponse<T> with a serializable type");
                        }

                        attributes[0].reflectionMethod = new ReflectionMethod(methods[i]);
                    }

                    ulong nameHash = HashMethodName(methods[i].Name);

                    if (HashResults.ContainsKey(nameHash) && HashResults[nameHash] != methods[i].Name)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError($"Hash collision detected for RPC method. The method \"{methods[i].Name}\" collides with the method \"{HashResults[nameHash]}\". This can be solved by increasing the amount of bytes to use for hashing in the NetworkConfig or changing the name of one of the conflicting methods.");
                    }
                    else if (!HashResults.ContainsKey(nameHash))
                    {
                        HashResults.Add(nameHash, methods[i].Name);
                    }
                    CachedServerRpcs[this].Add(nameHash, attributes[0]);


                    if (methods[i].GetParameters().Length > 0)
                    {
                        // Alloc justification: This is done only when first created. We are still allocing a whole NetworkedBehaviour. Allocing a string extra is NOT BAD
                        // As long as we dont alloc the string every RPC invoke. It's fine
                        string hashableMethodSignature = GetHashableMethodSignature(methods[i]);

                        ulong methodHash = HashMethodName(hashableMethodSignature);

                        if (HashResults.ContainsKey(methodHash) && HashResults[methodHash] != hashableMethodSignature)
                        {
                            if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError($"Hash collision detected for RPC method. The method \"{hashableMethodSignature}\" collides with the method \"{HashResults[methodHash]}\". This can be solved by increasing the amount of bytes to use for hashing in the NetworkConfig or changing the name of one of the conflicting methods.");
                        }
                        else if (!HashResults.ContainsKey(methodHash))
                        {
                            HashResults.Add(methodHash, hashableMethodSignature);
                        }
                        CachedServerRpcs[this].Add(methodHash, attributes[0]);
                    }
                }

                if (methods[i].IsDefined(typeof(ClientRPC), true))
                {
                    ClientRPC[] attributes = (ClientRPC[])methods[i].GetCustomAttributes(typeof(ClientRPC), true);
                    if (attributes.Length > 1)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Having more than 1 ClientRPC attribute per method is not supported.");
                    }

                    ParameterInfo[] parameters = methods[i].GetParameters();
                    if (parameters.Length == 2 && parameters[0].ParameterType == typeof(ulong) && parameters[1].ParameterType == typeof(NetBuffer) && methods[i].ReturnType == typeof(void))
                    {
                        //use delegate
                        attributes[0].rpcDelegate = (RpcDelegate)Delegate.CreateDelegate(typeof(RpcDelegate), this, methods[i].Name);
                    }
                    else
                    {
                        if (methods[i].ReturnType != typeof(void)
                            //&& !SerializationHelper.IsTypeSupported(methods[i].ReturnType)
                            )
                        {
                            if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogWarning("Invalid return type of RPC. Has to be either void or RpcResponse<T> with a serializable type");
                        }

                        attributes[0].reflectionMethod = new ReflectionMethod(methods[i]);
                    }


                    ulong nameHash = HashMethodName(methods[i].Name);

                    if (HashResults.ContainsKey(nameHash) && HashResults[nameHash] != methods[i].Name)
                    {
                        if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError($"Hash collision detected for RPC method. The method \"{methods[i].Name}\" collides with the method \"{HashResults[nameHash]}\". This can be solved by increasing the amount of bytes to use for hashing in the NetworkConfig or changing the name of one of the conflicting methods.");
                    }
                    else if (!HashResults.ContainsKey(nameHash))
                    {
                        HashResults.Add(nameHash, methods[i].Name);
                    }
                    CachedClientRpcs[this].Add(nameHash, attributes[0]);


                    if (methods[i].GetParameters().Length > 0)
                    {
                        // Alloc justification: This is done only when first created. We are still allocing a whole NetworkedBehaviour. Allocing a string extra is NOT BAD
                        // As long as we dont alloc the string every RPC invoke. It's fine
                        string hashableMethodSignature = GetHashableMethodSignature(methods[i]);

                        ulong methodHash = HashMethodName(hashableMethodSignature);

                        if (HashResults.ContainsKey(methodHash) && HashResults[methodHash] != hashableMethodSignature)
                        {
                            if (LogHelper.CurrentLogLevel <= LogLevel.Error) LogHelper.LogError($"Hash collision detected for RPC method. The method \"{hashableMethodSignature}\" collides with the method \"{HashResults[methodHash]}\". This can be solved by increasing the amount of bytes to use for hashing in the NetworkConfig or changing the name of one of the conflicting methods.");
                        }
                        else if (!HashResults.ContainsKey(methodHash))
                        {
                            HashResults.Add(methodHash, hashableMethodSignature);
                        }
                        CachedClientRpcs[this].Add(methodHash, attributes[0]);
                    }
                }
            }
        }

        public object OnRemoteServerRPC(ulong hash, int senderClientId, NetBuffer stream)
        {
            if (!CachedServerRpcs.ContainsKey(this) || !CachedServerRpcs[this].ContainsKey(hash))
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("ServerRPC request method not found");
                return null;
            }

            return InvokeServerRPCLocal(hash, senderClientId, stream);
        }

        public object OnRemoteClientRPC(ulong hash, int senderClientId, NetBuffer stream)
        {
            if (!CachedClientRpcs.ContainsKey(this) || !CachedClientRpcs[this].ContainsKey(hash))
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("ClientRPC request method not found");
                return null;
            }

            return InvokeClientRPCLocal(hash, senderClientId, stream);
        }

        private object InvokeServerRPCLocal(ulong hash, int senderClientId, NetBuffer stream)
        {
            if (!CachedServerRpcs.ContainsKey(this) || !CachedServerRpcs[this].ContainsKey(hash))
                return null;

            ServerRPC rpc = CachedServerRpcs[this][hash];

            if (rpc.RequireOwnership && senderClientId != mNetworkId)
            {
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only owner can invoke ServerRPC that is marked to require ownership");
                return null;
            }


            if (rpc.reflectionMethod != null)
            {

                return rpc.reflectionMethod.Invoke(this, stream);
            }

            if (rpc.rpcDelegate != null)
            {
                rpc.rpcDelegate(senderClientId, stream);
            }

            return null;

        }

        private object InvokeClientRPCLocal(ulong hash, int senderClientId, NetBuffer stream)
        {
            if (!CachedClientRpcs.ContainsKey(this) || !CachedClientRpcs[this].ContainsKey(hash))
                return null;

            ClientRPC rpc = CachedClientRpcs[this][hash];

            if (rpc.reflectionMethod != null)
            {
                return rpc.reflectionMethod.Invoke(this, stream);
            }

            if (rpc.rpcDelegate != null)
            {
                rpc.rpcDelegate(senderClientId, stream);
            }

            return null;

        }

        //Technically boxed writes are not needed. But save LOC for the non performance sends.
        internal void SendServerRPCBoxed(ulong hash, string channel, SecuritySendFlags security, params object[] parameters)
        {
            var writer = new NetBuffer();
            writer.Write((UInt32)PacketType.kRPC);
            writer.Write(NetworkId);
            writer.Write(hash);


            for (int i = 0; i < parameters.Length; i++)
            {
                writer.WriteObjectPacked(parameters[i]);
            }

            SendServerRPCPerformance(hash, writer, channel, security);
        }

        internal RpcResponse<T> SendServerRPCBoxedResponse<T>(ulong hash, string channel, SecuritySendFlags security, params object[] parameters)
        {
            var writer = new NetBuffer();
            writer.Write((UInt32)PacketType.kRPC);
            writer.Write(NetworkId);
            writer.Write(hash);

            for (int i = 0; i < parameters.Length; i++)
            {
                writer.WriteObjectPacked(parameters[i]);
            }

            return SendServerRPCPerformanceResponse<T>(hash, writer, channel, security);

        }

        internal void SendClientRPCBoxedToClient(ulong hash, int clientId, string channel, SecuritySendFlags security, params object[] parameters)
        {
            var writer = new NetBuffer();
            writer.Write((UInt32)PacketType.kRPC);
            writer.Write(NetworkId);
            writer.Write(hash);

            for (int i = 0; i < parameters.Length; i++)
            {
                writer.WriteObjectPacked(parameters[i]);
            }
            SendClientRPCPerformance(hash, clientId, writer, channel, security);

        }

        internal RpcResponse<T> SendClientRPCBoxedResponse<T>(ulong hash, int clientId, string channel, SecuritySendFlags security, params object[] parameters)
        {
            var writer = new NetBuffer();
            writer.Write((UInt32)PacketType.kRPC);
            writer.Write(NetworkId);
            writer.Write(hash);

            for (int i = 0; i < parameters.Length; i++)
            {
                writer.WriteObjectPacked(parameters[i]);
            }

            return SendClientRPCPerformanceResponse<T>(hash, clientId, writer, channel, security);

        }

        internal void SendClientRPCBoxed(ulong hash, List<int> clientIds, string channel, SecuritySendFlags security, params object[] parameters)
        {
            var writer = new NetBuffer();
            writer.Write((UInt32)PacketType.kRPC);
            writer.Write(NetworkId);
            writer.Write(hash);

            for (int i = 0; i < parameters.Length; i++)
            {
                writer.WriteObjectPacked(parameters[i]);
            }
            SendClientRPCPerformance(hash, clientIds, writer, channel, security);

        }

        internal void SendClientRPCBoxedToEveryoneExcept(int clientIdToIgnore, ulong hash, string channel, SecuritySendFlags security, params object[] parameters)
        {
            var writer = new NetBuffer();
            writer.Write((UInt32)PacketType.kRPC);
            writer.Write(NetworkId);
            writer.Write(hash);

            for (int i = 0; i < parameters.Length; i++)
            {
                writer.WriteObjectPacked(parameters[i]);
            }
            SendClientRPCPerformance(hash, writer, clientIdToIgnore, channel, security);

        }

        internal void SendServerRPCPerformance(ulong hash, NetBuffer messageStream, string channel, SecuritySendFlags security)
        {
            if (!IsClient && IsRunning)
            {
                //We are ONLY a server.
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only server and host can invoke ServerRPC");
                return;
            }



            Send(Engine.sInstance.ServerClientId, messageStream);
        }

        internal RpcResponse<T> SendServerRPCPerformanceResponse<T>(ulong hash, NetBuffer messageStream, string channel, SecuritySendFlags security)
        {
            if (!IsClient && IsRunning)
            {
                //We are ONLY a server.
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only server and host can invoke ServerRPC");
                return null;
            }

            Send(Engine.sInstance.ServerClientId, messageStream);

            return null;
        }

        internal void SendClientRPCPerformance(ulong hash, List<int> clientIds, NetBuffer messageStream, string channel, SecuritySendFlags security)
        {
            if (!IsServer && IsRunning)
            {
                //We are NOT a server.
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only clients and host can invoke ClientRPC");
                return;
            }

            Send(Engine.sInstance.ServerClientId, messageStream);
        }

        internal void SendClientRPCPerformance(ulong hash, NetBuffer messageStream, int clientIdToIgnore, string channel, SecuritySendFlags security)
        {
            if (!IsServer && IsRunning)
            {
                //We are NOT a server.
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only clients and host can invoke ClientRPC");
                return;
            }


            Send(Engine.sInstance.ServerClientId, messageStream);
        }

        internal void SendClientRPCPerformance(ulong hash, int clientId, NetBuffer messageStream, string channel, SecuritySendFlags security)
        {
            if (!IsServer && IsRunning)
            {
                //We are NOT a server.
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only clients and host can invoke ClientRPC");
                return;
            }

            Send(Engine.sInstance.ServerClientId, messageStream);
        }

        internal RpcResponse<T> SendClientRPCPerformanceResponse<T>(ulong hash, int clientId, NetBuffer messageStream, string channel, SecuritySendFlags security)
        {
            if (!IsServer && IsRunning)
            {
                //We are NOT a server.
                if (LogHelper.CurrentLogLevel <= LogLevel.Normal) LogHelper.LogWarning("Only clients and host can invoke ClientRPC");
                return null;
            }

            Send(Engine.sInstance.ServerClientId, messageStream);

            return null;
        }
        #endregion

    }
}
