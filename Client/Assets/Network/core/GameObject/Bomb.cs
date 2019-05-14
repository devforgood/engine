using Lidgren.Network;
using uint32_t = System.UInt32;

namespace core
{
    public class Bomb : NetGameObject
    {
        public override uint32_t GetClassId() { return (uint32_t)GameObjectClassId.kBomb; }

        public static new NetGameObject CreateInstance() { return new Bomb(); }

        enum EYarnReplicationState
        {
            EYRS_Pose = 1 << 0,
            EYRS_Color = 1 << 1,
            EYRS_PlayerId = 1 << 2,
            EYRS_Parent = 1 << 3,

            EYRS_AllState = EYRS_Pose | EYRS_Color | EYRS_PlayerId | EYRS_Parent
        };

        public static NetGameObject StaticCreate() { return new Bomb(); }

        public override uint32_t GetAllStateMask() { return (uint32_t)EYarnReplicationState.EYRS_AllState; }


        protected int mPlayerId;
        public int mParentNetworkId;

        protected Bomb()
        {
            mPlayerId = 0;
            mParentNetworkId = 0;
            SetScale(GetScale() * 0.25f);
            SetCollisionRadius(0.125f);
        }



        public void SetPlayerId(int inPlayerId) { mPlayerId = inPlayerId; }
        public int GetPlayerId() { return mPlayerId; }

        public override uint32_t Write(NetOutgoingMessage inOutputStream, uint32_t inDirtyState)
        {
            uint32_t writtenState = 0;

            if ((inDirtyState & (uint32_t)EYarnReplicationState.EYRS_Pose) != 0)
            {
                inOutputStream.Write((bool)true);

                inOutputStream.Write(GetLocation().mX);
                inOutputStream.Write(GetLocation().mY);

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

            return writtenState;
        }


        public override bool HandleCollisionWithActor(Actor inActor)
        {
            //you hit a cat, so look like you hit a cat



            return false;
        }


        public void InitFrom(Actor inShooter)
        {
            SetColor(inShooter.GetColor().Clone());
            SetPlayerId((int)inShooter.GetPlayerId());
            mParentNetworkId = inShooter.GetNetworkId();

            SetLocation(inShooter.GetLocation().Round());

            mDirection = inShooter.GetRotation().Clone();
        }

        public override void NetUpdate()
        {

            float deltaTime = Timing.sInstance.GetDeltaTime();

            //SetLocation(GetLocation() + mVelocity * deltaTime);


            //we'll let the cats handle the collisions
        }
    }
}
