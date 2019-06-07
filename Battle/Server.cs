using core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uint32_t = System.UInt32;
using uint16_t = System.UInt16;
using StackExchange.Redis;
using Serilog;

namespace Server
{
    public class Server : Engine
    {
        public static bool StaticInit(uint16_t port, byte world_count)
        {
            sInstance = new Server(port);
            World.StaticInit(world_count);
            return true;
        }

        public override void DoFrame()
        {
            NetworkManagerServer.sInstance.ProcessIncomingPackets();

            NetworkManagerServer.sInstance.CheckForDisconnects();

            NetworkManagerServer.sInstance.RespawnActors();

            base.DoFrame();

 
            World.LateUpdate();

            NetworkManagerServer.sInstance.SendOutgoingPackets();
        }

        public override int Run()
        {
            return base.Run();
        }

        public void HandleNewClient(ClientProxy inClientProxy)
        {

            int playerId = inClientProxy.GetPlayerId();
            byte worldId = inClientProxy.GetWorldId();

            GameMode.sInstance.AddEntry((uint32_t)playerId, inClientProxy.GetName());
            SpawnActorForPlayer(playerId, worldId);

            ServerMonitor.sInstance.AddUser(worldId);
        }
        public void HandleLostClient(ClientProxy inClientProxy)
        {
            //kill client's actor
            //remove client from scoreboard
            int playerId = inClientProxy.GetPlayerId();

            GameMode.sInstance.RemoveEntry((uint32_t)playerId);
            SActor actor = GetActorForPlayer(playerId, inClientProxy.GetWorldId());
            if (actor != null)
            {
                actor.Unpossess();
                actor.SetDoesWantToDie(true);
            }

            if(ServerMonitor.sInstance.DelUser(inClientProxy.GetWorldId())== 0)
            {
                Log.Information(string.Format("Close World {0}", inClientProxy.GetWorldId()));
                NetworkManagerServer.sInstance.Clear(inClientProxy.GetWorldId());
                World.Instance(inClientProxy.GetWorldId()).Clear();
            }
        }

        public SActor GetActorForPlayer(int inPlayerId, byte worldId)
        {
            //run through the objects till we find the actor...
            //it would be nice if we kept a pointer to the actor on the clientproxy
            //but then we'd have to clean it up when the actor died, etc.
            //this will work for now until it's a perf issue
            var gameObjects = World.Instance(worldId).GetGameObjects();
            foreach (var go in gameObjects)
            {
                SActor actor = (SActor)go.GetAsActor();
                if (actor != null && actor.GetPlayerId() == inPlayerId)
                {
                    return (SActor)go;
                }
            }

            return null;
        }

        public void SpawnActorForPlayer(int inPlayerId, byte worldId)
        {
            SActor actor = (SActor)GameObjectRegistry.sInstance.CreateGameObject((uint32_t)GameObjectClassId.kActor, worldId);
            actor.SetColor(GameMode.sInstance.GetEntry((uint32_t)inPlayerId).GetColor());
            actor.SetPlayerId((uint32_t)inPlayerId);
            actor.Possess(inPlayerId);
            actor.SetWorldId(worldId);
            //gotta pick a better spawn location than this...
            actor.SetLocation(core.Utility.GetRandomVector(-10, 10));

            Log.Information(string.Format("SpawnActorForPlayer player_id{0}, world{1}", inPlayerId, worldId));
        }


        Server(uint16_t port)
        {
            GameObjectRegistry.sInstance.RegisterCreationFunction((uint32_t)GameObjectClassId.kActor, SActor.StaticCreate);
            GameObjectRegistry.sInstance.RegisterCreationFunction((uint32_t)GameObjectClassId.kProp, SProp.StaticCreate);
            GameObjectRegistry.sInstance.RegisterCreationFunction((uint32_t)GameObjectClassId.kProjectile, SProjectile.StaticCreate);
            GameObjectRegistry.sInstance.RegisterCreationFunction((uint32_t)GameObjectClassId.kBomb, SBomb.StaticCreate);

            InitNetworkManager(port);

            //NetworkManagerServer::sInstance->SetDropPacketChance( 0.8f );
            //NetworkManagerServer::sInstance->SetSimulatedLatency( 0.25f );
            //NetworkManagerServer::sInstance->SetSimulatedLatency( 0.5f );
            //NetworkManagerServer::sInstance->SetSimulatedLatency( 0.1f );
            IsClient = false;
            IsServer = true;
        }

        bool InitNetworkManager(uint16_t port)
        {
            return NetworkManagerServer.StaticInit(port);
        }
    }
}
