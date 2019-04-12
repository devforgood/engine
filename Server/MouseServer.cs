using core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class MouseServer : Mouse
    {
        protected MouseServer()
        {

        }

        public new static GameObject StaticCreate() { return NetworkManagerServer.sInstance.RegisterAndReturn(new MouseServer()); }

        public override void HandleDying()
        {
            NetworkManagerServer.sInstance.UnregisterGameObject(this);

        }
        public override bool HandleCollisionWithCat(RoboCat inCat)
        {
            //kill yourself!
            SetDoesWantToDie(true);

            ScoreBoardManager.sInstance.IncScore(inCat.GetPlayerId(), 1);

            return false;
        }

    }


}

