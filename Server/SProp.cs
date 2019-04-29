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

        public new static NetGameObject StaticCreate() { return NetworkManagerServer.sInstance.RegisterAndReturn(new SProp()); }

        public override void HandleDying()
        {
            NetworkManagerServer.sInstance.UnregisterGameObject(this);

        }
        public override bool HandleCollisionWithCat(Actor inCat)
        {
            //kill yourself!
            SetDoesWantToDie(true);

            ScoreBoardManager.sInstance.IncScore(inCat.GetPlayerId(), 1);

            return false;
        }

    }


}

