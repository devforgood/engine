using core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uint32_t = System.UInt32;
using uint16_t = System.UInt16;
using StackExchange.Redis;

namespace Server
{
    public class Server : Engine
    {
        public ServerCommon.ServerInfo server_info = new ServerCommon.ServerInfo();
        public ConnectionMultiplexer redis = null;
        public float set_server_info_time = 0.0f;
        public TimeSpan server_info_expire = new TimeSpan(0, 1, 0);

        public static bool StaticInit(uint16_t port)
        {
            sInstance = new Server(port);
            return true;
        }

        public override void DoFrame()
        {
            NetworkManagerServer.sInstance.ProcessIncomingPackets();

            NetworkManagerServer.sInstance.CheckForDisconnects();

            NetworkManagerServer.sInstance.RespawnActors();

            base.DoFrame();

 
            World.sInstance.LateUpdate();

            NetworkManagerServer.sInstance.SendOutgoingPackets();

            set_server_info_time += Timing.sInstance.GetDeltaTime();
            if (set_server_info_time > 3.0f)
            {
                Task.Run(()=> 
                {
                    var db = redis.GetDatabase();
                    db.StringSet(server_info.server_id, NetworkManagerServer.sInstance.GetPlayerCount(), server_info_expire);
                });

                set_server_info_time = 0.0f;
            }

        }

        public override int Run()
        {
            SetupWorld();
            return base.Run();
        }

        public void HandleNewClient(ClientProxy inClientProxy)
        {

            int playerId = inClientProxy.GetPlayerId();
            byte worldId = inClientProxy.GetWorldId();

            ScoreBoardManager.sInstance.AddEntry((uint32_t)playerId, inClientProxy.GetName());
            SpawnActorForPlayer(playerId, worldId);
        }
        public void HandleLostClient(ClientProxy inClientProxy)
        {
            //kill client's actor
            //remove client from scoreboard
            int playerId = inClientProxy.GetPlayerId();

            ScoreBoardManager.sInstance.RemoveEntry((uint32_t)playerId);
            Actor actor = GetActorForPlayer(playerId);
            if (actor != null)
            {
                actor.SetDoesWantToDie(true);
            }
        }

        public Actor GetActorForPlayer(int inPlayerId)
        {
            //run through the objects till we find the actor...
            //it would be nice if we kept a pointer to the actor on the clientproxy
            //but then we'd have to clean it up when the actor died, etc.
            //this will work for now until it's a perf issue
            var gameObjects = World.sInstance.GetGameObjects();
            foreach (var go in gameObjects)
            {
                Actor actor = go.GetAsActor();
                if (actor != null && actor.GetPlayerId() == inPlayerId)
                {
                    return (Actor)go;
                }
            }

            return null;
        }

        public void SpawnActorForPlayer(int inPlayerId, byte worldId)
        {
            Actor actor = (Actor)GameObjectRegistry.sInstance.CreateGameObject((uint32_t)GameObjectClassId.kActor, worldId);
            actor.SetColor(ScoreBoardManager.sInstance.GetEntry((uint32_t)inPlayerId).GetColor());
            actor.SetPlayerId((uint32_t)inPlayerId);
            actor.SetWorldId(worldId);
            //gotta pick a better spawn location than this...
            actor.SetLocation(core.Utility.GetRandomVector(-10, 10));
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
        void SetupWorld()
        {
            //spawn some random mice
            CreateRandomMice(10);

            //spawn more random mice!
            CreateRandomMice(10);
        }
        void CreateRandomMice(int inMouseCount)
        {
            Vector3 mouseMin = new Vector3(-5.0f, -3.0f, 0.0f);
            Vector3 mouseMax = new Vector3(5.0f, 3.0f, 0.0f);
            NetGameObject go;

            //make a mouse somewhere- where will these come from?
            for (int i = 0; i < inMouseCount; ++i)
            {
                go = GameObjectRegistry.sInstance.CreateGameObject((uint32_t)GameObjectClassId.kProp, 0);
                Vector3 mouseLocation = core.Utility.GetRandomVector(mouseMin, mouseMax);
                go.SetLocation(mouseLocation);
            }
        }

    }
}
