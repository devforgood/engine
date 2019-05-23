using core;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public enum EActorControlType
    {
        ESCT_Human,
        ESCT_AI
    };

    public class SActor : Actor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        EActorControlType mActorControlType;


        float mTimeOfNextShot;
        float mTimeBetweenShots;

        float mTimeOfNextBomb;
        float mTimeBetweenBomb;

        void HandleShooting()
        {
            float time = Timing.sInstance.GetFrameStartTime();
            if (mIsShooting && Timing.sInstance.GetFrameStartTime() > mTimeOfNextShot)
            {
                //not exact, but okay
                mTimeOfNextShot = time + mTimeBetweenShots;

                //fire!
                Projectile bullet = (Projectile)GameObjectRegistry.sInstance.CreateGameObject((uint)GameObjectClassId.kProjectile);
                bullet.InitFromShooter(this);
            }
        }

        void HandleBomb()
        {
            float time = Timing.sInstance.GetFrameStartTime();
            if (mIsBomb && Timing.sInstance.GetFrameStartTime() > mTimeOfNextBomb)
            {
                //not exact, but okay
                mTimeOfNextBomb = time + mTimeBetweenBomb;

                //install bomb
                Bomb bomb = (Bomb)GameObjectRegistry.sInstance.CreateGameObject((uint)GameObjectClassId.kBomb);
                bomb.InitFrom(this);
            }
        }

        public static new NetGameObject StaticCreate() { return NetworkManagerServer.sInstance.RegisterAndReturn(new SActor()); }
        public override void HandleDying()
        {
            NetworkManagerServer.sInstance.UnregisterGameObject(this);

        }

        public override void NetUpdate()
        {
            base.NetUpdate();

            Vector3 oldLocation = GetLocation().Clone();
            Vector3 oldVelocity = GetVelocity().Clone();
            Vector3 oldRotation = GetRotation().Clone();

            //are you controlled by a player?
            //if so, is there a move we haven't processed yet?
            if (mActorControlType == EActorControlType.ESCT_Human)
            {
                ClientProxy client = NetworkManagerServer.sInstance.GetClientProxy((int)GetPlayerId());
                if (client != null)
                {
                    MoveList moveList = client.GetUnprocessedMoveList();
                    foreach (var unprocessedMove in moveList.mMoves)
                    {
                        var currentState = unprocessedMove.GetInputState();

                        float deltaTime = unprocessedMove.GetDeltaTime();

                        ProcessInput(deltaTime, currentState);

                        SimulateMovement(deltaTime);

                        //log.InfoFormat( "Server Move Time: {0} deltaTime: {1} location:{2}, old_location{3}, player_id{4}", unprocessedMove.GetTimestamp(), deltaTime, GetLocation(), oldLocation, GetPlayerId() );
                        log.Info("Location:" + GetLocation() + ", Velocity:" + GetVelocity() + ", player_id:" + GetPlayerId());

                    }

                    moveList.Clear();
                }
            }
            else
            {
                //do some AI stuff
                SimulateMovement(Timing.sInstance.GetDeltaTime());
            }

            HandleShooting();
            HandleBomb();

            mCharacterController.Body.Position = new BEPUutilities.Vector3(GetLocation().mX, GetLocation().mY, GetLocation().mZ);
            mCharacterController.HorizontalMotionConstraint.LastDirection = new BEPUutilities.Vector3(mDirection.mX, mDirection.mY, mDirection.mZ);
            if (mCharacterController.HorizontalMotionConstraint.MovementMode != BEPUphysics.Character.MovementMode.Floating)
            {
                if (GetVelocity().IsZero() == false)
                    mCharacterController.Body.LinearVelocity = new BEPUutilities.Vector3(GetVelocity().mX, GetVelocity().mY, GetVelocity().mZ);
            }


            //body.Position = new BEPUutilities.Vector3(GetLocation().mX, GetLocation().mY, GetLocation().mZ);
            //body.LinearVelocity = new Jitter.LinearMath.JVector(GetVelocity().mX, GetLocation().mY, GetVelocity().mZ);

            if (!oldLocation.Equals(GetLocation()) ||
                !oldVelocity.Equals(GetVelocity()) ||
                !oldRotation.Equals(GetRotation())
                )
            {
                //log.InfoFormat("ol {0} cl {1} ov {2} cv {3} or{4} cr{5}", oldLocation, GetLocation(), oldVelocity, GetVelocity(), oldRotation, GetRotation());
                NetworkManagerServer.sInstance.SetStateDirty(GetNetworkId(), (uint)EActorReplicationState.ECRS_Pose);
            }
        }

        public void SetActorControlType(EActorControlType inActorControlType) { mActorControlType = inActorControlType; }

        public void TakeDamage(int inDamagingPlayerId)
        {
            mHealth--;
            if (mHealth <= 0.0f)
            {
                //score one for damaging player...
                ScoreBoardManager.sInstance.IncScore((uint)inDamagingPlayerId, 1);

                //and you want to die
                SetDoesWantToDie(true);

                //tell the client proxy to make you a new cat
                ClientProxy clientProxy = NetworkManagerServer.sInstance.GetClientProxy((int)GetPlayerId());
                if (clientProxy != null)
                {
                    clientProxy.HandleActorDied();
                }
            }

            //tell the world our health dropped
            NetworkManagerServer.sInstance.SetStateDirty(GetNetworkId(), (uint)EActorReplicationState.ECRS_Health);
        }
        protected SActor()
        {

            mActorControlType = EActorControlType.ESCT_Human;
            mTimeOfNextShot = 0.0f;
            mTimeBetweenShots = 0.2f;

            mTimeOfNextBomb = 0.0f;
            mTimeBetweenBomb = 0.2f;

            //CacheAttributes();

            

        }

        public override NetBuffer CreateRpcPacket(int clientId)
        {
            //build state packet
            var rpcPacket = NetworkManagerServer.sInstance.GetServer().CreateMessage();

            //it's rpc!
            rpcPacket.Write((UInt32)PacketType.kRPC);

            return rpcPacket;
        }

        public override void Send(int clientId, NetBuffer inOutputStream)
        {
            NetworkManagerServer.sInstance.SendPacket(clientId, inOutputStream);
        }

        [ServerRPC(RequireOwnership = false)]
        public override void PingServer(int number)
        {
            InvokeClientRpcOnClient(PingClient, (int)GetPlayerId(), number);
        }

        [ServerRPC(RequireOwnership = false)]
        public override void JumpServer(int power)
        {
            var location = GetLocation();
            location.mY += power;
            location += mDirection * power;
            SetLocation(location);
        }
    }
}
