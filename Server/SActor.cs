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

        void HandleShooting()
        {
            float time = Timing.sInstance.GetFrameStartTime();
            if (mIsShooting && Timing.sInstance.GetFrameStartTime() > mTimeOfNextShot)
            {
                //not exact, but okay
                mTimeOfNextShot = time + mTimeBetweenShots;

                //fire!
                Projectile yarn = (Projectile)GameObjectRegistry.sInstance.CreateGameObject((uint)GameObjectClassId.kProjectile);
                yarn.InitFromShooter(this);
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

            Vector3 oldLocation = GetLocation();
            Vector3 oldVelocity = GetVelocity();
#if USE_INPUT_STATE_OLD
            float oldRotation = GetRotation();
#else
            Vector3 oldRotation = GetRotation();
#endif 

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

                        log.InfoFormat( "Server Move Time: {0} deltaTime: {1} location:{2}, player_id{3}", unprocessedMove.GetTimestamp(), deltaTime, GetLocation(), GetPlayerId() );

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

            if (!RoboMath.Is2DVectorEqual(oldLocation, GetLocation()) ||
                !RoboMath.Is2DVectorEqual(oldVelocity, GetVelocity()) ||
                oldRotation != GetRotation())
            {
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

            CacheAttributes();

        }

        public override void Send(int clientId, NetBuffer inOutputStream)
        {
            NetworkManagerServer.sInstance.SendPacket(clientId, inOutputStream);
        }

        [ServerRPC(RequireOwnership = false)]
        public override void PingServer(int number)
        {
        }
    }
}
