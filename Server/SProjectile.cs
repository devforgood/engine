using core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class SProjectile : Projectile
    {
        public new static NetGameObject StaticCreate() { return NetworkManagerServer.sInstance.RegisterAndReturn(new SProjectile()); }

        public override void HandleDying()
        {

            NetworkManagerServer.sInstance.UnregisterGameObject(this);
        }

        public override bool HandleCollisionWithCat(Actor inCat)
        {
            if (inCat.GetPlayerId() != GetPlayerId())
            {
                //kill yourself!
                SetDoesWantToDie(true);

                ((SActor)(inCat)).TakeDamage(GetPlayerId());

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

        protected SProjectile()
        {
            mTimeToDie = Timing.sInstance.GetFrameStartTime() + 1.0f;

        }

        private float mTimeToDie;
    }
}
