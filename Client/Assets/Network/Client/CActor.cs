using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using uint32_t = System.UInt32;


public class CActor : core.Actor
{
    public static new core.NetGameObject StaticCreate(byte worldId) { return new CActor(worldId); }
    public GameObject mTarget = null;
    public ActorBehaviour mActorBehaviour = null;

    float mTimeLocationBecameOutOfSync;
    float mTimeVelocityBecameOutOfSync;

    BEPUutilities.Vector3 physicsLocation = new BEPUutilities.Vector3();
    BEPUutilities.Vector3 physicsVelocity = new BEPUutilities.Vector3();

    public bool IsLocalPlayer()
    {
        return GetPlayerId() == NetworkManagerClient.sInstance.GetPlayerId();
    }

    public override void NetUpdate()
    {
        if (GetPlayerId() == NetworkManagerClient.sInstance.GetPlayerId())
        {
            core.Move pendingMove = InputManager.sInstance.GetAndClearPendingMove();
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

            if (GetVelocity().IsZero())
            {
                //we're in sync if our velocity is 0
                mTimeLocationBecameOutOfSync = 0.0f;
            }

            //Debug.Log("Remote Client Location : " + GetLocation() + ", player_id : " + GetPlayerId());

        }

        mCharacterController.Body.Position = GetLocation().CopyTo(ref physicsLocation);
        mDirection.CopyTo(ref mCharacterController.HorizontalMotionConstraint.LastDirection);
        if (mCharacterController.HorizontalMotionConstraint.MovementMode != BEPUphysics.Character.MovementMode.Floating)
        {
            if (GetVelocity().IsZero() == false)
                mCharacterController.Body.LinearVelocity = GetVelocity().CopyTo(ref physicsVelocity);
        }


        //body.Position = new BEPUutilities.Vector3(GetLocation().mX, GetLocation().mY, GetLocation().mZ);

    }
    public override void HandleDying()
    {
        base.HandleDying();
        if (mTarget != null)
            GameObject.Destroy(mTarget, 0.3f);

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

        Vector3 oldRotation = GetRotation();
        Vector3 oldLocation = GetLocation();
        Vector3 oldVelocity = GetVelocity();

        Vector3 replicatedLocation = default(Vector3);
        Vector3 replicatedVelocity = default(Vector3);

        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            inInputStream.Read(ref replicatedVelocity);
            SetVelocity(replicatedVelocity);
            //Debug.Log("replicatedVelocity : " + replicatedVelocity + ", player_id :" + GetPlayerId());

            inInputStream.Read(ref replicatedLocation);
            SetLocation(replicatedLocation);
            //Debug.Log("replicatedLocation : " + replicatedLocation + ", player_id :" + GetPlayerId());

            mDirection.x = 0.0f;
            mDirection.z = 0.0f;
            mDirection.x += inInputStream.ReadBoolean() ? Vector3.right.x : 0.0f;
            mDirection.x += inInputStream.ReadBoolean() ? Vector3.left.x : 0.0f;
            mDirection.z += inInputStream.ReadBoolean() ? Vector3.forward.z : 0.0f;
            mDirection.z += inInputStream.ReadBoolean() ? Vector3.back.z : 0.0f;
            mDirection.Normalize();

            //Debug.Log("mDirection : " + mDirection + ", player_id :" + GetPlayerId());

            mThrustDir = 1.0f;
            readState |= (uint32_t)EActorReplicationState.ECRS_Pose;
        }


        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            Vector3 color = default(Vector3);
            inInputStream.Read(ref color);
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

    protected CActor(byte worldId): base(worldId)
    {
        mTimeLocationBecameOutOfSync = 0.0f;
        mTimeVelocityBecameOutOfSync = 0.0f;
    }

    void InterpolateClientSidePrediction(Vector3 inOldRotation, Vector3 inOldLocation, Vector3 inOldVelocity, bool inIsForRemoteActor)
    {
        if (inOldRotation != GetRotation() && !inIsForRemoteActor)
        {

            //LOG( "ERROR! Move replay ended with incorrect rotation!", 0 );
        }

        float roundTripTime = NetworkManagerClient.sInstance.GetRoundTripTime();

        if (!inOldLocation.Equals(GetLocation()))
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
                if(inIsForRemoteActor)
                {
                    SetLocation(Vector3.Lerp(inOldLocation, GetLocation(), (durationOutOfSync / roundTripTime)));
                    //Debug.Log("location " + GetLocation().ToString());

                }
                else
                {
                    SetLocation(Vector3.Lerp(inOldLocation, GetLocation(), 0.1f));

                }

            }
        }
        else
        {
            //we're in sync
            mTimeLocationBecameOutOfSync = 0.0f;
        }


        if (!inOldVelocity.Equals(GetVelocity()))
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

                SetVelocity(Vector3.Lerp(inOldVelocity, GetVelocity(), inIsForRemoteActor ? (durationOutOfSync / roundTripTime) : 0.1f));
            }
            //otherwise, fine...

        }
        else
        {
            //we're in sync
            mTimeVelocityBecameOutOfSync = 0.0f;
        }


    }

    public override void CompleteCreate()
    {

        GameObject prefab = Resources.Load("Ralph") as GameObject;

        GameObject actor = MonoBehaviour.Instantiate(prefab, GetLocation(), Quaternion.Euler(mDirection)) as GameObject;
        mTarget = actor;

        //GameObject instance = Instantiate(Resources.Load("Brick", typeof(GameObject))) as GameObject;
        mActorBehaviour = actor.GetComponent<ActorBehaviour>();

        mActorBehaviour.actor = this;

        if(IsLocalPlayer())
        {
            GameObject go = GameObject.Find("Main Camera");
            if (go != null)
            {
                go.GetComponent<CompleteCameraController>().SetPlayer(actor);
            }
        }
    }


    public override NetBuffer CreateRpcPacket(int clientId)
    {
        //build state packet
        NetOutgoingMessage rpcPacket = NetworkManagerClient.sInstance.GetClient().CreateMessage();

        //it's rpc!
        rpcPacket.Write((UInt32)core.PacketType.kRPC);

        return rpcPacket;
    }

    public override void Send(int clientId, NetBuffer inOutputStream)
    {
        NetworkManagerClient.sInstance.GetClient().SendMessage((NetOutgoingMessage)inOutputStream, NetDeliveryMethod.ReliableSequenced);
    }

    [core.ClientRPC]
    public override void PingClient(int number)
    {
        Debug.Log("Ping " + number);
    }

}
