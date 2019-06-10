using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY
using UnityEngine;
#endif 

using uint32_t = System.UInt32;

namespace core
{
    public class Projectile : NetGameObject
    {
        public override uint32_t GetClassId() { return (uint32_t)GameObjectClassId.kProjectile; }


        enum EYarnReplicationState
        {
            EYRS_Pose = 1 << 0,
            EYRS_Color = 1 << 1,
            EYRS_PlayerId = 1 << 2,

            EYRS_AllState = EYRS_Pose | EYRS_Color | EYRS_PlayerId
        };

        public static NetGameObject StaticCreate(byte worldId) { return new Projectile(); }

        public override uint32_t GetAllStateMask() { return (uint32_t)EYarnReplicationState.EYRS_AllState; }


        protected Vector3 mVelocity;

        protected float mMuzzleSpeed;
        protected int mPlayerId;

        protected Projectile()
        {
            mMuzzleSpeed = 3.0f;
            mVelocity = Vector3.zero;
            mPlayerId = 0;
            SetScale(GetScale() * 0.25f);
            SetCollisionRadius(0.125f);
        }


        public void SetVelocity(Vector3 inVelocity) { mVelocity = inVelocity; }
        public ref Vector3 GetVelocity() { return ref mVelocity; }

        public void SetPlayerId(int inPlayerId) { mPlayerId = inPlayerId; }
        public int GetPlayerId() { return mPlayerId; }

        public override uint32_t Write(NetOutgoingMessage inOutputStream, uint32_t inDirtyState)
        {
            uint32_t writtenState = 0;

            if ((inDirtyState & (uint32_t)EYarnReplicationState.EYRS_Pose) != 0)
            {
                inOutputStream.Write((bool)true);

                inOutputStream.Write(ref GetLocation());

                inOutputStream.Write(ref GetVelocity());

                inOutputStream.Write(ref mDirection);

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

            return writtenState;
        }


        public override bool HandleCollisionWithActor(Actor inActor)
        {
            //you hit a cat, so look like you hit a cat



            return false;
        }


        public void InitFromShooter(Actor inShooter)
        {
            SetColor(inShooter.GetColor());
            SetPlayerId((int)inShooter.GetPlayerId());

            Vector3 forward = inShooter.GetForwardVector();
            SetVelocity(inShooter.GetVelocity() + forward * mMuzzleSpeed);
            SetLocation(inShooter.GetLocation());

            mDirection = inShooter.GetRotation();
        }

        public override void Update()
        {

            float deltaTime = Timing.sInstance.GetDeltaTime();

            SetLocation(GetLocation() + mVelocity * deltaTime);


            //we'll let the cats handle the collisions
        }



    }
}
