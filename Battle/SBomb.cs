using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using core;

namespace Server
{
    class SBomb : Bomb
    {
        public new static NetGameObject StaticCreate() { return NetworkManagerServer.sInstance.RegisterAndReturn(new SBomb()); }

        public override void HandleDying()
        {

            NetworkManagerServer.sInstance.UnregisterGameObject(this);
        }

        public override void NetUpdate()
        {
            base.NetUpdate();

            if (Timing.sInstance.GetFrameStartTime() > mTimeToDie)
            {

                SetDoesWantToDie(true);
            }
        }

        protected SBomb()
        {
            mTimeToDie = Timing.sInstance.GetFrameStartTime() + 3.0f;

        }

        private float mTimeToDie;

    }
}
