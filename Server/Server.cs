using core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Server : Engine
    {

        public static bool StaticInit()
        {
            sInstance = new Server();
            return true;
        }

        public override void DoFrame()
        {
            NetworkManagerServer.sInstance.ProcessIncomingPackets();

            NetworkManagerServer.sInstance.CheckForDisconnects();

            NetworkManagerServer.sInstance.RespawnCats();

            base.DoFrame();

            NetworkManagerServer.sInstance.SendOutgoingPackets();
        }

	public override  int Run()
        {
            SetupWorld();
            return base.Run();
        }

        public void HandleNewClient(ClientProxyPtr inClientProxy)
        {

            int playerId = inClientProxy->GetPlayerId();

            ScoreBoardManager::sInstance->AddEntry(playerId, inClientProxy->GetName());
            SpawnCatForPlayer(playerId);
        }
        public void HandleLostClient(ClientProxyPtr inClientProxy);

        public RoboCat GetCatForPlayer(int inPlayerId);
        public void SpawnCatForPlayer(int inPlayerId);


        private:
	Server();

        bool InitNetworkManager();
        void SetupWorld();

    }
}
