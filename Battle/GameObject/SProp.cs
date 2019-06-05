using core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class SProp : Prop
    {
        protected SProp()
        {
            Log.Information(string.Format("create prop {0}", NetworkId));
        }

        public new static NetGameObject StaticCreate(byte worldId) { return NetworkManagerServer.sInstance.RegisterAndReturn(new SProp(), worldId); }

        public override void HandleDying()
        {
            NetworkManagerServer.sInstance.UnregisterGameObject(this);
            Log.Information(string.Format("remove prop {0}", NetworkId));
        }

        public override int OnExplode(int player_id, int parentNetworkId, int damage)
        {
            //kill yourself!
            SetDoesWantToDie(true);

            return 0;
        }
    }


}

