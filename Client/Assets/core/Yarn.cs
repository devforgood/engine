﻿using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using uint32_t = System.UInt32;

namespace core
{
    public class Yarn : GameObject
    {
        public override uint32_t GetClassId() { return (uint32_t)GameObjectClassId.kYarn; }

        public static new GameObject CreateInstance() { return new Yarn(); }

        enum EYarnReplicationState
        {
            EYRS_Pose = 1 << 0,
            EYRS_Color = 1 << 1,
            EYRS_PlayerId = 1 << 2,

            EYRS_AllState = EYRS_Pose | EYRS_Color | EYRS_PlayerId
        };

        public static GameObject StaticCreate() { return new Yarn(); }

        public override uint32_t GetAllStateMask() { return (uint32_t)EYarnReplicationState.EYRS_AllState; }


        Vector3 mVelocity;

        float mMuzzleSpeed;
        int mPlayerId;

        protected Yarn()
        {
            mMuzzleSpeed = 3.0f;
            mVelocity = Vector3.Zero.Clone();
            mPlayerId = 0;
            SetScale(GetScale() * 0.25f);
            SetCollisionRadius(0.125f);
        }


        public void SetVelocity(Vector3 inVelocity) { mVelocity = inVelocity; }
        public Vector3 GetVelocity() { return mVelocity; }

        public void SetPlayerId(int inPlayerId) { mPlayerId = inPlayerId; }
        public int GetPlayerId() { return mPlayerId; }

        public override uint32_t Write(NetOutgoingMessage inOutputStream, uint32_t inDirtyState)
        {
            uint32_t writtenState = 0;

            if ((inDirtyState & (uint32_t)EYarnReplicationState.EYRS_Pose) != 0)
            {
                inOutputStream.Write((bool)true);

                Vector3 location = GetLocation();
                inOutputStream.Write(location.mX);
                inOutputStream.Write(location.mY);

                Vector3 velocity = GetVelocity();
                inOutputStream.Write(velocity.mX);
                inOutputStream.Write(velocity.mY);

                inOutputStream.Write(GetRotation());

                writtenState |= (uint32_t)EYarnReplicationState.EYRS_Pose;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }

            if ((inDirtyState & (uint32_t)EYarnReplicationState.EYRS_Color) != 0)
            {
                inOutputStream.Write((bool)true);

                inOutputStream.Write(GetColor());

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


        public override bool HandleCollisionWithCat(RoboCat inCat)
        {
            //you hit a cat, so look like you hit a cat



            return false;
        }


        public void InitFromShooter(RoboCat inShooter)
        {
            SetColor(inShooter.GetColor());
            SetPlayerId((int)inShooter.GetPlayerId());

            Vector3 forward = inShooter.GetForwardVector();
            SetVelocity(inShooter.GetVelocity() + forward * mMuzzleSpeed);
            SetLocation(inShooter.GetLocation() /* + forward * 0.55f */ );

            SetRotation(inShooter.GetRotation());
        }

        public override void Update()
        {

            float deltaTime = Timing.sInstance.GetDeltaTime();

            SetLocation(GetLocation() + mVelocity * deltaTime);


            //we'll let the cats handle the collisions
        }



    }
}
