using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uint32_t = System.UInt32;


namespace core
{
    public class Mouse : GameObject
    {
        public override uint32_t GetClassId() { return 2; }

        public static new GameObject CreateInstance() { return new Mouse(); }

        enum EMouseReplicationState
        {
            EMRS_Pose = 1 << 0,
            EMRS_Color = 1 << 1,

            EMRS_AllState = EMRS_Pose | EMRS_Color
        };

        public Mouse()
        {

            SetScale(GetScale() * 0.5f);

            SetCollisionRadius(0.25f);
        }


        public static GameObject StaticCreate() { return new Mouse(); }

        public override uint32_t GetAllStateMask() { return (uint32_t)EMouseReplicationState.EMRS_AllState; }

        public override uint32_t Write(NetOutgoingMessage inOutputStream, uint32_t inDirtyState)
        {
            uint32_t writtenState = 0;

            if ((inDirtyState & (uint32_t)EMouseReplicationState.EMRS_Pose) != 0)
            {
                inOutputStream.Write((bool)true);

                Vector3 location = GetLocation();
                inOutputStream.Write(location.mX);
                inOutputStream.Write(location.mY);

                inOutputStream.Write(GetRotation());

                writtenState |= (uint32_t)EMouseReplicationState.EMRS_Pose;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }

            if ((inDirtyState & (uint32_t)EMouseReplicationState.EMRS_Color) != 0)
            {
                inOutputStream.Write((bool)true);

                inOutputStream.Write(GetColor());

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
                Vector3 location = new Vector3();
                location.mX = inInputStream.ReadFloat();
                location.mY = inInputStream.ReadFloat();
                SetLocation(location);

                float rotation = inInputStream.ReadFloat();
                SetRotation(rotation);
            }


            stateBit = inInputStream.ReadBoolean();
            if (stateBit)
            {
                Vector3 color = new Vector3();
                inInputStream.Read(color);
                SetColor(color);
            }
        }

        public override bool HandleCollisionWithCat(RoboCat inCat)
        {
            return false;
        }
    }
}
