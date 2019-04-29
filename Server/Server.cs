using core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uint32_t = System.UInt32;
using uint16_t = System.UInt16;


namespace Server
{
    public class Server : Engine
    {

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

            NetworkManagerServer.sInstance.SendOutgoingPackets();
        }

        public override int Run()
        {
            SetupWorld();
            return base.Run();
        }

        public void HandleNewClient(ClientProxy inClientProxy)
        {

            int playerId = inClientProxy.GetPlayerId();

            ScoreBoardManager.sInstance.AddEntry((uint32_t)playerId, inClientProxy.GetName());
            SpawnActorForPlayer(playerId);
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

        public void SpawnActorForPlayer(int inPlayerId)
        {
            Actor actor = (Actor)GameObjectRegistry.sInstance.CreateGameObject((uint32_t)GameObjectClassId.kActor);
            actor.SetColor(ScoreBoardManager.sInstance.GetEntry((uint32_t)inPlayerId).GetColor());
            actor.SetPlayerId((uint32_t)inPlayerId);
            //gotta pick a better spawn location than this...
            actor.SetLocation(new Vector3(1.0f - (float)(inPlayerId), 0.0f, 0.0f));
        }


        Server(uint16_t port)
        {
            GameObjectRegistry.sInstance.RegisterCreationFunction((uint32_t)GameObjectClassId.kActor, SActor.StaticCreate);
            GameObjectRegistry.sInstance.RegisterCreationFunction((uint32_t)GameObjectClassId.kProp, SProp.StaticCreate);
            GameObjectRegistry.sInstance.RegisterCreationFunction((uint32_t)GameObjectClassId.kProjectile, SProjectile.StaticCreate);

            InitNetworkManager(port);

            //NetworkManagerServer::sInstance->SetDropPacketChance( 0.8f );
            //NetworkManagerServer::sInstance->SetSimulatedLatency( 0.25f );
            //NetworkManagerServer::sInstance->SetSimulatedLatency( 0.5f );
            //NetworkManagerServer::sInstance->SetSimulatedLatency( 0.1f );
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
                go = GameObjectRegistry.sInstance.CreateGameObject((uint32_t)GameObjectClassId.kProp);
                Vector3 mouseLocation = RoboMath.GetRandomVector(mouseMin, mouseMax);
                go.SetLocation(mouseLocation);
            }
        }

    }
}
