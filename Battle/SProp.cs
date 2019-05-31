using core;
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

        }

        public new static NetGameObject StaticCreate(byte worldId) { return NetworkManagerServer.sInstance.RegisterAndReturn(new SProp(), worldId); }

        public override void HandleDying()
        {
            NetworkManagerServer.sInstance.UnregisterGameObject(this);

        }
        public override bool HandleCollisionWithActor(Actor inActor)
        {
            //kill yourself!
            SetDoesWantToDie(true);

            ScoreBoardManager.sInstance.IncScore(inActor.GetPlayerId(), 1);

            return false;
        }

    }


}

