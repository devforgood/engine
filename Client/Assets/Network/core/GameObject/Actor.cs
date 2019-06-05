using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using uint32_t = System.UInt32;

namespace core
{
    public class Actor : NetGameObject
    {
        static readonly float HALF_WORLD_HEIGHT = 3.6f;
        static readonly float HALF_WORLD_WIDTH = 6.4f;

        public override uint32_t GetClassId() { return (uint32_t)GameObjectClassId.kActor; }


        public enum EActorReplicationState
        {
            ECRS_Pose = 1 << 0,
            ECRS_Color = 1 << 1,
            ECRS_PlayerId = 1 << 2,
            ECRS_Health = 1 << 3,

            ECRS_AllState = ECRS_Pose | ECRS_Color | ECRS_PlayerId | ECRS_Health
        };

        public Actor(byte worldId)
        {
            WorldId = worldId;

            mMaxRotationSpeed = 5.0f;
            mMaxLinearSpeed = 60.0f;
            mVelocity = Vector3.Zero.Clone();
            mWallRestitution = 0.1f;

            mActorRestitution = 0.1f;

            mThrustDir = 0.0f;

            mPlayerId = 0;

            mIsShooting = false;
            mIsBomb = false;

            mHealth = 10;
            SetCollisionRadius(0.5f);

            CacheAttributes();

            /*
            body = new BEPUphysics.Entities.Prefabs.Cylinder(new BEPUutilities.Vector3(0, 0, 0), 1.0f, 0.5f, 10f);
            body.IgnoreShapeChanges = true;
            body.CollisionInformation.Shape.CollisionMargin = 0.1f;
            //Making the character a continuous object prevents it from flying through walls which would be pretty jarring from a player's perspective.
            body.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            body.LocalInertiaTensorInverse = new BEPUutilities.Matrix3x3();
            //TODO: In v0.16.2, compound bodies would override the material properties that get set in the CreatingPair event handler.
            //In a future version where this is changed, change this to conceptually minimally required CreatingPair.
            //body.CollisionInformation.Events.DetectingInitialCollision += RemoveFriction;
            body.LinearDamping = 0;

            Engine.sInstance.world.Add(body);
            */

            mCharacterController = new BEPUphysics.Character.CharacterController(new BEPUutilities.Vector3(0, 3, 0), 1.0f, 1.0f * 0.7f, 1.0f * 0.3f, 0.5f, 0.001f, 10f, 0.8f, 1.3f, 8.0f
                , 3f, 1.5f, 1000, 0f, 0f, 0f, 0f
                );

            World.Instance(worldId).space.Add(mCharacterController);

        }

        public void ProcessInput(float inDeltaTime, InputState inInputState)
        {
            //process our input....
            Vector3 direction = new Vector3();
            if (inInputState.mIsForward)
                direction += Vector3.forward;
            if (inInputState.mIsBack)
                direction += Vector3.back;
            if (inInputState.mIsRight)
                direction += Vector3.right;
            if (inInputState.mIsLeft)
                direction += Vector3.left;

            direction.Normalize();
            mDirection = direction;

            //LogHelper.LogInfo("direction " + mDirection);
            //turning...


            //moving...
            //float inputForwardDelta = inInputState.GetDesiredVerticalDelta();
            mThrustDir = 1.0f;


            mIsShooting = inInputState.IsShooting();
            mIsBomb = inInputState.mIsBomb;

        }


        public void AdjustVelocityByThrust(float inDeltaTime)
        {
            //just set the velocity based on the thrust direction -- no thrust will lead to 0 velocity
            //simulating acceleration makes the client prediction a bit more complex
            Vector3 forwardVector = GetForwardVector();
            mVelocity = forwardVector * (mThrustDir * inDeltaTime * mMaxLinearSpeed);

            //LogHelper.LogInfo("mVelocity " + mVelocity);

        }

        public void SimulateMovement(float inDeltaTime)
        {
            //simulate us...
            AdjustVelocityByThrust(inDeltaTime);


            SetLocation(GetLocation() + mVelocity * inDeltaTime);

            //ProcessCollisions();
        }

        public static NetGameObject StaticCreate(byte worldId) { return new Actor(worldId); }

        public override uint32_t GetAllStateMask() { return (uint32_t)EActorReplicationState.ECRS_AllState; }

        public override Actor GetAsActor() { return this; }
        public override void NetUpdate()
        {

        }

        public override void CompleteRemove()
        {
            mCharacterController.OnRemovalFromSpace(World.Instance(WorldId).space);
        }

        public override void LateUpdate()
        {
            Vector3 v = new Vector3(mCharacterController.Body.Position.X, mCharacterController.Body.Position.Y, mCharacterController.Body.Position.Z);
            if(v.Equals(GetLocation()) == false)
            {
                //LogHelper.LogInfo("old location " + GetLocation() + ", new location " + v);
                SetLocation(v);
            }


            //Vector3 v2 = new Vector3(mCharacterController.Body.LinearVelocity.X, mCharacterController.Body.LinearVelocity.Y, mCharacterController.Body.LinearVelocity.Z);
            //if (v2.Equals(GetVelocity()) == false)
            //{
            //    SetVelocity(v2);
            //}

            //mCharacterController.HorizontalMotionConstraint.MovementDirection = BEPUutilities.Vector2.Zero;

        }

        public override uint32_t Write(NetOutgoingMessage inOutputStream, uint32_t inDirtyState)
        {

            uint32_t writtenState = 0;

            if ((inDirtyState & (uint32_t)EActorReplicationState.ECRS_PlayerId) != 0)
            {
                inOutputStream.Write((bool)true);
                inOutputStream.Write(GetPlayerId());

                writtenState |= (uint32_t)EActorReplicationState.ECRS_PlayerId;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }


            if ((inDirtyState & (uint32_t)EActorReplicationState.ECRS_Pose) != 0)
            {
                inOutputStream.Write((bool)true);

                inOutputStream.Write(mVelocity);

                inOutputStream.Write(GetLocation());

                inOutputStream.Write(mDirection);

                writtenState |= (uint32_t)EActorReplicationState.ECRS_Pose;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }

            if ((inDirtyState & (uint32_t)EActorReplicationState.ECRS_Color) != 0)
            {
                inOutputStream.Write((bool)true);
                inOutputStream.Write(GetColor());

                writtenState |= (uint32_t)EActorReplicationState.ECRS_Color;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }

            if ((inDirtyState & (uint32_t)EActorReplicationState.ECRS_Health) != 0)
            {
                inOutputStream.Write((bool)true);
                inOutputStream.Write(mHealth, 4);

                writtenState |= (uint32_t)EActorReplicationState.ECRS_Health;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }
            return writtenState;
        }

        public void SetPlayerId(uint32_t inPlayerId) { mPlayerId = inPlayerId; }
        public void SetWorldId(byte worldId) {  WorldId = worldId; }
        public uint32_t GetPlayerId() { return mPlayerId; }

        public void SetVelocity(Vector3 inVelocity) { mVelocity = inVelocity; }
        public Vector3 GetVelocity() { return mVelocity; }

        public int GetHealth() { return mHealth; }


        Vector3 mVelocity = new Vector3();


        float mMaxLinearSpeed;
        float mMaxRotationSpeed;

        //bounce fraction when hitting various things
        float mWallRestitution;
        float mActorRestitution;


        uint32_t mPlayerId;



        ///move down here for padding reasons...

        protected float mLastMoveTimestamp;

        protected float mThrustDir;
        protected int mHealth;

        protected bool mIsShooting;
        protected bool mIsBomb;

        public BEPUphysics.Character.CharacterController mCharacterController = null;


        [ServerRPC(RequireOwnership = false)]
        public virtual void PingServer(int number)
        {
        }

        [ClientRPC]
        public virtual void PingClient(int number)
        {
        }


        [ServerRPC(RequireOwnership = false)]
        public virtual void JumpServer(int power)
        {
        }
    }
}
