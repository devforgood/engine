﻿using Lidgren.Network;
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
    public class Prop : NetGameObject
    {
        public override uint32_t GetClassId() { return (uint32_t)GameObjectClassId.kProp; }


        enum EMouseReplicationState
        {
            EMRS_Pose = 1 << 0,
            EMRS_Color = 1 << 1,

            EMRS_AllState = EMRS_Pose | EMRS_Color
        };

        public Prop()
        {

            SetScale(GetScale() * 0.5f);

            SetCollisionRadius(0.25f);
        }


        public static NetGameObject StaticCreate(byte worldId) { return new Prop(); }

        public override uint32_t GetAllStateMask() { return (uint32_t)EMouseReplicationState.EMRS_AllState; }

        public override uint32_t Write(NetOutgoingMessage inOutputStream, uint32_t inDirtyState)
        {
            uint32_t writtenState = 0;

            if ((inDirtyState & (uint32_t)EMouseReplicationState.EMRS_Pose) != 0)
            {
                inOutputStream.Write((bool)true);

                inOutputStream.Write(ref GetLocation());

                inOutputStream.Write(ref mDirection);

                writtenState |= (uint32_t)EMouseReplicationState.EMRS_Pose;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }

            if ((inDirtyState & (uint32_t)EMouseReplicationState.EMRS_Color) != 0)
            {
                inOutputStream.Write((bool)true);

                inOutputStream.Write(ref GetColor());

                writtenState |= (uint32_t)EMouseReplicationState.EMRS_Color;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }


            return writtenState;
        }
        public override void Read(NetIncomingMessage inInputStream )
        {
            bool stateBit = inInputStream.ReadBoolean();

            if (stateBit)
            {
                Vector3 location = default(Vector3);
                inInputStream.Read(ref location);
                SetLocation(location);

                inInputStream.Read(ref mDirection);
            }


            stateBit = inInputStream.ReadBoolean();
            if (stateBit)
            {
                Vector3 color = default(Vector3);
                inInputStream.Read(ref color);
                SetColor(color);
            }
        }
    }
}
