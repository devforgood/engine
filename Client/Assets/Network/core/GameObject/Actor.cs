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
    public class Actor : NetGameObject
    {
        //static readonly float HALF_WORLD_HEIGHT = 3.6f;
        //static readonly float HALF_WORLD_WIDTH = 6.4f;

        public bool IsForward = false;
        public bool IsBack = false;
        public bool IsRight = false;
        public bool IsLeft = false;

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

            //mMaxRotationSpeed = 5.0f;
            mMaxLinearSpeed = 60.0f;
            mVelocity = Vector3.zero;
            mWallRestitution = 0.1f;

            mActorRestitution = 0.1f;

            mThrustDir = 0.0f;

            mPlayerId = 0;

            mIsShooting = false;
            mIsBomb = false;

            mHealth = 10;
            SetCollisionRadius(0.45f);

            CacheAttributes();


#if _USE_BEPU_PHYSICS
            mCharacterController = new BEPUphysics.Character.CharacterController(new BEPUutilities.Vector3(0, 3, 0), 1.0f, 1.0f * 0.7f, 1.0f * 0.3f, 0.5f, 0.001f, 10f, 0.8f, 1.3f, 8.0f
                , 3f, 1.5f, 1000, 0f, 0f, 0f, 0f
                );

            World.Instance(worldId).space.Add(mCharacterController);
#endif

        }

        public void ProcessInput(float inDeltaTime, InputState inInputState)
        {
            //process our input....
            Vector3 direction = default(Vector3);

            IsForward = false;
            IsBack = false;
            IsRight = false;
            IsLeft = false;
            if (inInputState.mIsForward)
            {
                direction += Vector3.forward;
                IsForward = true;
            }
            if (inInputState.mIsBack)
            {
                direction += Vector3.back;
                IsBack = true;
            }
            if (inInputState.mIsRight)
            {
                direction += Vector3.right;
                IsRight = true;
            }
            if (inInputState.mIsLeft)
            {
                direction += Vector3.left;
                IsLeft = true;
            }

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

            ProcessCollisions();
        }


        public void ProcessCollisions()
        {

            float sourceRadius = GetCollisionRadius();
            Vector3 sourceLocation = GetLocation();

            //now let's iterate through the world and see what we hit...
            //note: since there's a small number of objects in our game, this is fine.
            //but in a real game, brute-force checking collisions against every other object is not efficient.
            //it would be preferable to use a quad tree or some other structure to minimize the
            //number of collisions that need to be tested.
            foreach (var target in World.Instance(WorldId).GetGameObjects())
            {
                if (target.GetNetworkId() != this.GetNetworkId() && !target.DoesWantToDie())
                {
                    //simple collision test for spheres- are the radii summed less than the distance?
                    Vector3 targetLocation = target.GetLocation();
                    float targetRadius = target.GetCollisionRadius();

                    Vector3 delta = targetLocation - sourceLocation;
                    float distSq = delta.sqrMagnitude;
                    float collisionDist = (sourceRadius + targetRadius);
                    if (distSq < (collisionDist * collisionDist))
                    {
                        //first, tell the other guy there was a collision with a cat, so it can do something...

                        if (target.HandleCollisionWithActor(this))
                        {
                            //okay, you hit something!
                            //so, project your location far enough that you're not colliding
                            Vector3 dirToTarget = delta;
                            dirToTarget.Normalize();
                            Vector3 acceptableDeltaFromSourceToTarget = dirToTarget * collisionDist;
                            //important note- we only move this cat. the other cat can take care of moving itself
                            SetLocation(targetLocation - acceptableDeltaFromSourceToTarget);


                            Vector3 relVel = mVelocity;

                            //if other object is a cat, it might have velocity, so there might be relative velocity...
                            Actor targetActor = target.GetAsActor();
                            if (targetActor != null)
                            {
                                relVel -= targetActor.mVelocity;
                            }

                            //got vel with dir between objects to figure out if they're moving towards each other
                            //and if so, the magnitude of the impulse ( since they're both just balls )
                            float relVelDotDir = Vector3.Dot(relVel, dirToTarget);

                            if (relVelDotDir > 0.0f)
                            {
                                Vector3 impulse = relVelDotDir * dirToTarget;

                                if (targetActor != null)
                                {
                                    mVelocity -= impulse;
                                    mVelocity *= mActorRestitution;
                                }
                                else
                                {
                                    mVelocity -= impulse * 2.0f;
                                    mVelocity *= mWallRestitution;
                                }

                            }
                        }
                    }
                }
            }

        }


        public static NetGameObject StaticCreate(byte worldId) { return new Actor(worldId); }

        public override uint32_t GetAllStateMask() { return (uint32_t)EActorReplicationState.ECRS_AllState; }

        public override Actor GetAsActor() { return this; }
        public override void Update()
        {

        }

        public override void CompleteRemove()
        {
#if _USE_BEPU_PHYSICS
            mCharacterController.OnRemovalFromSpace(World.Instance(WorldId).space);
#endif
        }

        public override void LateUpdate()
        {
#if _USE_BEPU_PHYSICS
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
#endif
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

                inOutputStream.Write(ref mVelocity);

                inOutputStream.Write(ref GetLocation());

                inOutputStream.Write(IsRight);
                inOutputStream.Write(IsLeft);
                inOutputStream.Write(IsForward);
                inOutputStream.Write(IsBack);

                writtenState |= (uint32_t)EActorReplicationState.ECRS_Pose;
            }
            else
            {
                inOutputStream.Write((bool)false);
            }

            if ((inDirtyState & (uint32_t)EActorReplicationState.ECRS_Color) != 0)
            {
                inOutputStream.Write((bool)true);
                inOutputStream.Write(ref GetColor());

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
        public ref Vector3 GetVelocity() { return ref mVelocity; }

        public int GetHealth() { return mHealth; }


        Vector3 mVelocity = new Vector3();


        float mMaxLinearSpeed;
        //float mMaxRotationSpeed;

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

#if _USE_BEPU_PHYSICS
        public BEPUphysics.Character.CharacterController mCharacterController = null;
#endif


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
