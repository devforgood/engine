using core;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using uint32_t = System.UInt32;


public class CActor : core.Actor
{
    public static new core.NetGameObject StaticCreate() { return new CActor(); }

    float mTimeLocationBecameOutOfSync;
    float mTimeVelocityBecameOutOfSync;

    public bool IsLocalPlayer()
    {
        return GetPlayerId() == NetworkManagerClient.sInstance.GetPlayerId();
    }

    public override void NetUpdate()
    {
        if (GetPlayerId() == NetworkManagerClient.sInstance.GetPlayerId())
        {
            Move pendingMove = InputManager.sInstance.GetAndClearPendingMove();
            //in theory, only do this if we want to sample input this frame / if there's a new move ( since we have to keep in sync with server )
            if (pendingMove != null) //is it time to sample a new move...
            {
                float deltaTime = pendingMove.GetDeltaTime();

                //apply that input

                ProcessInput(deltaTime, pendingMove.GetInputState());

                //and simulate!

                SimulateMovement(deltaTime);

                //Debug.Log( "Local Client Move Time: " + pendingMove.GetTimestamp()  +" deltaTime: "+ deltaTime + " left rot at " + GetRotation() + " location: " + GetLocation() );
            }
        }
        else
        {
            SimulateMovement(core.Timing.sInstance.GetDeltaTime());

            if (core.RoboMath.Is2DVectorEqual(GetVelocity(), core.Vector3.Zero.Clone()))
            {
                //we're in sync if our velocity is 0
                mTimeLocationBecameOutOfSync = 0.0f;
            }

            //Debug.Log("Remote Client Location : " + GetLocation() + ", player_id : " + GetPlayerId());

        }
    }
    public override void HandleDying()
    {
        base.HandleDying();

        //and if we're local, tell the hud so our health goes away!
        if (GetPlayerId() == NetworkManagerClient.sInstance.GetPlayerId())
        {
            //HUD::sInstance->SetPlayerHealth(0);
        }
    }

    public override void Read(NetIncomingMessage inInputStream)
    {
        bool stateBit = inInputStream.ReadBoolean();

        uint32_t readState = 0;

        if (stateBit)
        {
            uint32_t playerId = inInputStream.ReadUInt32();
            SetPlayerId(playerId);
            readState |= (uint32_t)EActorReplicationState.ECRS_PlayerId;
        }

#if USE_INPUT_STATE_OLD
        float oldRotation = GetRotation();
        float replicatedRotation;
#else
        core.Vector3 oldRotation = GetRotation();
#endif
        core.Vector3 oldLocation = GetLocation();
        core.Vector3 oldVelocity = GetVelocity();

        core.Vector3 replicatedLocation = new core.Vector3();
        core.Vector3 replicatedVelocity = new core.Vector3();

        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            replicatedVelocity.mX = inInputStream.ReadFloat();
            replicatedVelocity.mY = inInputStream.ReadFloat();

            SetVelocity(replicatedVelocity);

            replicatedLocation.mX = inInputStream.ReadFloat();
            replicatedLocation.mY = inInputStream.ReadFloat();

            //Debug.Log("replicatedLocation : " + replicatedLocation + ", player_id :" + GetPlayerId());


            SetLocation(replicatedLocation);

#if USE_INPUT_STATE_OLD
            replicatedRotation = inInputStream.ReadFloat();
            SetRotation(replicatedRotation);
#else
            mDirection.mX = inInputStream.ReadFloat();
            mDirection.mY = inInputStream.ReadFloat();
            mThrustDir = 1.0f;
#endif
            readState |= (uint32_t)EActorReplicationState.ECRS_Pose;
        }


#if USE_INPUT_STATE_OLD
        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            stateBit = inInputStream.ReadBoolean();
            mThrustDir = stateBit ? 1.0f : -1.0f;
        }
        else
        {
            mThrustDir = 0.0f;
        }
#endif

        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            core.Vector3 color = new core.Vector3();
            inInputStream.Read(color);
            SetColor(color);
            readState |= (uint32_t)EActorReplicationState.ECRS_Color;
        }

        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            mHealth = 0;
            mHealth = inInputStream.ReadInt32(4);
            readState |= (uint32_t)EActorReplicationState.ECRS_Health;
        }

        if (GetPlayerId() == NetworkManagerClient.sInstance.GetPlayerId())
        {
            //did we get health? if so, tell the hud!
            if ((readState & (uint32_t)EActorReplicationState.ECRS_Health) != 0)
            {
                //HUD::sInstance->SetPlayerHealth(mHealth);
            }

            DoClientSidePredictionAfterReplicationForLocalActor(readState);

            //if this is a create packet, don't interpolate
            if ((readState & (uint32_t)EActorReplicationState.ECRS_PlayerId) == 0)
            {
                InterpolateClientSidePrediction(oldRotation, oldLocation, oldVelocity, false);
            }
        }
        else
        {
            DoClientSidePredictionAfterReplicationForRemoteActor(readState);

            //will this smooth us out too? it'll interpolate us just 10% of the way there...
            if ((readState & (uint32_t)EActorReplicationState.ECRS_PlayerId) == 0)
            {
                InterpolateClientSidePrediction(oldRotation, oldLocation, oldVelocity, true);
            }

        }
    }

    public void DoClientSidePredictionAfterReplicationForLocalActor(uint32_t inReadState)
    {
        if ((inReadState & (uint32_t)EActorReplicationState.ECRS_Pose) != 0)
        {
            //simulate pose only if we received new pose- might have just gotten thrustDir
            //in which case we don't need to replay moves because we haven't warped backwards

            //all processed moves have been removed, so all that are left are unprocessed moves
            //so we must apply them...
            var moveList = InputManager.sInstance.GetMoveList().mMoves;

            foreach (var move in moveList)
            {
                float deltaTime = move.GetDeltaTime();
                ProcessInput(deltaTime, move.GetInputState());

                SimulateMovement(deltaTime);
            }
        }
    }
    public void DoClientSidePredictionAfterReplicationForRemoteActor(uint32_t inReadState)
    {
        if ((inReadState & (uint32_t)EActorReplicationState.ECRS_Pose) != 0)
        {

            //simulate movement for an additional RTT
            float rtt = NetworkManagerClient.sInstance.GetRoundTripTime();
            //LOG( "Other cat came in, simulating for an extra %f", rtt );

            //let's break into framerate sized chunks though so that we don't run through walls and do crazy things...
            float deltaTime = 1.0f / 30.0f;

            while (true)
            {
                if (rtt < deltaTime)
                {
                    SimulateMovement(rtt);
                    break;
                }
                else
                {
                    SimulateMovement(deltaTime);
                    rtt -= deltaTime;
                }
            }
        }
    }

    protected CActor()
    {
        mTimeLocationBecameOutOfSync = 0.0f;
        mTimeVelocityBecameOutOfSync = 0.0f;

        GameObject prefab = Resources.Load("Prefabs/Brick") as GameObject;
        GameObject brick = MonoBehaviour.Instantiate(prefab) as GameObject;
        //GameObject instance = Instantiate(Resources.Load("Brick", typeof(GameObject))) as GameObject;
        var cube = brick.GetComponent<CubeNetwork>();
        cube.actor = this;
    }

    void InterpolateClientSidePrediction(
#if USE_INPUT_STATE_OLD
        float inOldRotation, 
#else
        core.Vector3 inOldRotation,
#endif 
        core.Vector3 inOldLocation, core.Vector3 inOldVelocity, bool inIsForRemoteCat)
    {
        if (inOldRotation != GetRotation() && !inIsForRemoteCat)
        {

            //LOG( "ERROR! Move replay ended with incorrect rotation!", 0 );
        }

        float roundTripTime = NetworkManagerClient.sInstance.GetRoundTripTime();

        if (!core.RoboMath.Is2DVectorEqual(inOldLocation, GetLocation()))
        {
            //LOG( "ERROR! Move replay ended with incorrect location!", 0 );

            //have we been out of sync, or did we just become out of sync?
            float time = core.Timing.sInstance.GetFrameStartTime();
            if (mTimeLocationBecameOutOfSync == 0.0f)
            {
                mTimeLocationBecameOutOfSync = time;
            }

            float durationOutOfSync = time - mTimeLocationBecameOutOfSync;
            if (durationOutOfSync < roundTripTime)
            {

                SetLocation(core.Vector3.Lerp(inOldLocation, GetLocation(), inIsForRemoteCat ? (durationOutOfSync / roundTripTime) : 0.1f));
            }
        }
        else
        {
            //we're in sync
            mTimeLocationBecameOutOfSync = 0.0f;
        }


        if (!core.RoboMath.Is2DVectorEqual(inOldVelocity, GetVelocity()))
        {
            //LOG( "ERROR! Move replay ended with incorrect velocity!", 0 );

            //have we been out of sync, or did we just become out of sync?
            float time = core.Timing.sInstance.GetFrameStartTime();
            if (mTimeVelocityBecameOutOfSync == 0.0f)
            {
                mTimeVelocityBecameOutOfSync = time;
            }

            //now interpolate to the correct value...
            float durationOutOfSync = time - mTimeVelocityBecameOutOfSync;
            if (durationOutOfSync < roundTripTime)
            {

                SetVelocity(core.Vector3.Lerp(inOldVelocity, GetVelocity(), inIsForRemoteCat ? (durationOutOfSync / roundTripTime) : 0.1f));
            }
            //otherwise, fine...

        }
        else
        {
            //we're in sync
            mTimeVelocityBecameOutOfSync = 0.0f;
        }


    }

    public override NetBuffer CreateRpcPacket(int clientId)
    {
        //build state packet
        NetOutgoingMessage rpcPacket = NetworkManagerClient.sInstance.GetClient().CreateMessage();


        //it's rpc!
        rpcPacket.Write((UInt32)PacketType.kRPC);

        InFlightPacket ifp = NetworkManagerClient.sInstance.DeliveryNotificationManager.WriteState(rpcPacket);
        var rmtd = new RpcTransmissionData();
        if(ifp!=null)
            ifp.SetTransmissionData((int)TransmissionDataType.kRpcManager, rmtd);
        rmtd.mOutputStream = rpcPacket;

        return rpcPacket;
    }

    public override void Send(int clientId, NetBuffer inOutputStream)
    {
        NetworkManagerClient.sInstance.GetClient().SendMessage((NetOutgoingMessage)inOutputStream, NetDeliveryMethod.ReliableSequenced);
    }

}
