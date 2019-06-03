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
        public new static NetGameObject StaticCreate(byte worldId) { return NetworkManagerServer.sInstance.RegisterAndReturn(new SBomb(), worldId); }

        public override void HandleDying()
        {
            NetworkManagerServer.sInstance.UnregisterGameObject(this);
        }

        private void Explode()
        {
            //World.Instance(WorldId).
        }

        public override void NetUpdate()
        {
            base.NetUpdate();

            if (Timing.sInstance.GetFrameStartTime() > mTimeToBomb )
            {
                // 첫 폭발만 클라와 동기화를 맞춘다.
                if (mExplodeCount == 0)
                {
                    mIsExplode = true;
                    NetworkManagerServer.sInstance.SetStateDirty(GetNetworkId(), WorldId, (uint)EYarnReplicationState.EYRS_Explode);
                }

                // 해당 시간 뒤에 추가 폭발이 일어난다.
                mTimeToBomb += 0.05f;
                ++mExplodeCount;
                Explode();
            }


            if (Timing.sInstance.GetFrameStartTime() > mTimeToDie)
            {
                SetDoesWantToDie(true);
            }
        }

        protected SBomb()
        {
            mTimeToDie = Timing.sInstance.GetFrameStartTime() + 4.0f;
            mTimeToBomb = Timing.sInstance.GetFrameStartTime() + 3.0f;

        }

        private float mTimeToDie;
        private float mTimeToBomb;
        private int mExplodeCount = 0;

    }
}
