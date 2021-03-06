﻿using Lidgren.Network;
using uint32_t = System.UInt32;

namespace core
{
    public class Bomb : NetGameObject
    {
        public override uint32_t GetClassId() { return (uint32_t)GameObjectClassId.kBomb; }


        public enum EYarnReplicationState
        {
            EYRS_Pose = 1 << 0,
            EYRS_Color = 1 << 1,
            EYRS_PlayerId = 1 << 2,
            EYRS_Parent = 1 << 3,
            EYRS_Explode = 1 << 4,


            EYRS_AllState = EYRS_Pose | EYRS_Color | EYRS_PlayerId | EYRS_Parent | EYRS_Explode
        };

        public static NetGameObject StaticCreate(byte worldId) { return new Bomb(); }

        public override uint32_t GetAllStateMask() { return (uint32_t)EYarnReplicationState.EYRS_AllState; }


        protected int mPlayerId;
        public int mParentNetworkId;
        public bool mIsExplode;

        protected Bomb()
        {
            mPlayerId = 0;
            mParentNetworkId = 0;
            mIsExplode = false;


        }



        public void SetPlayerId(int inPlayerId) { mPlayerId = inPlayerId; }
        public int GetPlayerId() { return mPlayerId; }

        public override uint32_t Write(NetOutgoingMessage inOutputStream, uint32_t inDirtyState)
        {
            uint32_t writtenState = 0;

            if ((inDirtyState & (uint32_t)EYarnReplicationState.EYRS_Pose) != 0)
            {
                inOutputStream.Write((bool)true);

                inOutputStream.Write(ref GetLocation());

                writtenState |= (uint32_t)EYarnReplicationState.EYRS_Pose;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }

            if ((inDirtyState & (uint32_t)EYarnReplicationState.EYRS_Color) != 0)
            {
                inOutputStream.Write((bool)true);

                inOutputStream.Write(ref GetColor());

                writtenState |= (uint32_t)EYarnReplicationState.EYRS_Color;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }

            if ((inDirtyState & (uint32_t)EYarnReplicationState.EYRS_PlayerId) != 0)
            {
                inOutputStream.Write((bool)true);

                inOutputStream.Write(mPlayerId, 8);

                writtenState |= (uint32_t)EYarnReplicationState.EYRS_PlayerId;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }

            if ((inDirtyState & (uint32_t)EYarnReplicationState.EYRS_Parent) != 0)
            {
                inOutputStream.Write((bool)true);

                inOutputStream.Write(mParentNetworkId);

                writtenState |= (uint32_t)EYarnReplicationState.EYRS_Parent;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }

            if ((inDirtyState & (uint32_t)EYarnReplicationState.EYRS_Explode) != 0)
            {
                inOutputStream.Write((bool)true);

                inOutputStream.Write(mIsExplode);

                writtenState |= (uint32_t)EYarnReplicationState.EYRS_Explode;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }

            return writtenState;
        }


        public override bool HandleCollisionWithActor(Actor inActor)
        {
            //you hit a cat, so look like you hit a cat



            return false;
        }


        public void InitFrom(Actor inShooter)
        {
            SetColor(inShooter.GetColor());
            SetPlayerId((int)inShooter.GetPlayerId());
            mParentNetworkId = inShooter.GetNetworkId();

            SetLocation(inShooter.GetLocation().Round());

            mDirection = inShooter.GetRotation();
        }

        public override void Update()
        {

            float deltaTime = Timing.sInstance.GetDeltaTime();

            //SetLocation(GetLocation() + mVelocity * deltaTime);


            //we'll let the cats handle the collisions
        }

    }
}
