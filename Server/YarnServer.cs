using core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class YarnServer : Yarn
    {
        public new static NetGameObject StaticCreate() { return NetworkManagerServer.sInstance.RegisterAndReturn(new YarnServer()); }

        public override void HandleDying()
        {

            NetworkManagerServer.sInstance.UnregisterGameObject(this);
        }

        public override bool HandleCollisionWithCat(RoboCat inCat)
        {
            if (inCat.GetPlayerId() != GetPlayerId())
            {
                //kill yourself!
                SetDoesWantToDie(true);

                ((RoboCatServer)(inCat)).TakeDamage(GetPlayerId());

            }

            return false;
        }

        public override void NetUpdate()
        {
            base.NetUpdate();

            if (Timing.sInstance.GetFrameStartTime() > mTimeToDie)
            {

                SetDoesWantToDie(true);
            }
        }

        protected YarnServer()
        {
            mTimeToDie = Timing.sInstance.GetFrameStartTime() + 1.0f;

        }

        private float mTimeToDie;
    }
}
