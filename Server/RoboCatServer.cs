using core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public enum ECatControlType
    {
        ESCT_Human,
        ESCT_AI
    };

    public class RoboCatServer : RoboCat
    {
        ECatControlType mCatControlType;


        float mTimeOfNextShot;
        float mTimeBetweenShots;

        void HandleShooting()
        {

        }

        public static new GameObject StaticCreate() { return NetworkManagerServer.sInstance.RegisterAndReturn(new RoboCatServer()); }
        public override void HandleDying()
        {
            NetworkManagerServer.sInstance.UnregisterGameObject(this);

        }

        public override void Update()
        {
            base.Update();

            Vector3 oldLocation = GetLocation();
            Vector3 oldVelocity = GetVelocity();
            float oldRotation = GetRotation();

            //are you controlled by a player?
            //if so, is there a move we haven't processed yet?
            if (mCatControlType == ECatControlType.ESCT_Human)
            {
                ClientProxy client = NetworkManagerServer.sInstance.GetClientProxy((int)GetPlayerId());
                if (client != null)
                {
                    MoveList  moveList = client.GetUnprocessedMoveList();
                    foreach ( var unprocessedMove in moveList.Moves)
			{
                        var currentState = unprocessedMove.GetInputState();

                        float deltaTime = unprocessedMove.GetDeltaTime();

                        ProcessInput(deltaTime, currentState);
                        SimulateMovement(deltaTime);

                        //LOG( "Server Move Time: %3.4f deltaTime: %3.4f left rot at %3.4f", unprocessedMove.GetTimestamp(), deltaTime, GetRotation() );

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
                NetworkManagerServer.sInstance.SetStateDirty(GetNetworkId(), (uint)ECatReplicationState.ECRS_Pose);
            }
        }

        public void SetCatControlType(ECatControlType inCatControlType) { mCatControlType = inCatControlType; }

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
                    clientProxy.HandleCatDied();
                }
            }

            //tell the world our health dropped
            NetworkManagerServer.sInstance.SetStateDirty(GetNetworkId(), (uint)ECatReplicationState.ECRS_Health);
        }
        protected RoboCatServer()
        {

            mCatControlType = ECatControlType.ESCT_Human;
            mTimeOfNextShot = 0.0f;
            mTimeBetweenShots = 0.2f;
        }

    }
}
