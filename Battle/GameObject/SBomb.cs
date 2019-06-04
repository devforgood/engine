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


        public static readonly float BombPeriod = 3.0f;
        public static readonly int MaxExplodeCount = 5;
        public static readonly float NextExplodePeriod = 0.05f;

        private float mTimeToBomb;
        private int mExplodeCount = 0;




        public override void HandleDying()
        {
            NetworkManagerServer.sInstance.UnregisterGameObject(this);
        }

        void Explode(Tile tile)
        {
            if (tile == null)
                return;

            foreach(var game_object in tile.gameObjects)
            {
                switch((GameObjectClassId)game_object.GetClassId())
                {
                    case GameObjectClassId.kActor:
                        ((SActor)game_object).TakeDamage(this.mPlayerId, ((SActor)game_object).GetHealth());
                        break;
                }
            }
        }

        void Explode()
        {
            Explode(World.Instance(WorldId).mWorldMap.GetTile(GetLocation() + (Vector3.forward * mExplodeCount)));
            Explode(World.Instance(WorldId).mWorldMap.GetTile(GetLocation() + (Vector3.back * mExplodeCount)));
            Explode(World.Instance(WorldId).mWorldMap.GetTile(GetLocation() + (Vector3.right * mExplodeCount)));
            Explode(World.Instance(WorldId).mWorldMap.GetTile(GetLocation() + (Vector3.left * mExplodeCount)));
        }

        public override void NetUpdate()
        {
            base.NetUpdate();

            if (Timing.sInstance.GetFrameStartTime() > mTimeToBomb && mExplodeCount <= MaxExplodeCount)
            {
                // 해당 시간 뒤에 추가 폭발이 일어난다.
                mTimeToBomb += NextExplodePeriod;

                // 첫 폭발만 클라와 동기화를 맞춘다.
                if (mExplodeCount == 0)
                {
                    mIsExplode = true;
                    NetworkManagerServer.sInstance.SetStateDirty(GetNetworkId(), WorldId, (uint)EYarnReplicationState.EYRS_Explode);

                    Explode(World.Instance(WorldId).mWorldMap.GetTile(GetLocation()));
                }
                else
                {
                    Explode();
                }

                ++mExplodeCount;
            }


            if (mExplodeCount == MaxExplodeCount+1)
            {
                SetDoesWantToDie(true);
            }
        }

        protected SBomb()
        {
            mTimeToBomb = Timing.sInstance.GetFrameStartTime() + BombPeriod;

        }



    }
}
