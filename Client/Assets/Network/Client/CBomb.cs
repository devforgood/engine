﻿using Lidgren.Network;
using UnityEngine;

class CBomb : core.Bomb
{
    public GameObject mTarget = null;

    public static new core.NetGameObject StaticCreate() { return new CBomb(); }
    public override void Read(NetIncomingMessage inInputStream)
    {
        bool stateBit;

        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            core.Vector3 location = new core.Vector3();
            location.mX = inInputStream.ReadFloat();
            location.mY = inInputStream.ReadFloat();

            //dead reckon ahead by rtt, since this was spawned a while ago!
            SetLocation(location);
        }

        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            core.Vector3 color = new core.Vector3();
            inInputStream.Read(color);
            SetColor(color);
        }

        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            mPlayerId = inInputStream.ReadInt32(8);
        }

        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            mParentNetworkId = inInputStream.ReadInt32();
        }

    }
    public override bool HandleCollisionWithActor(core.Actor inActor)
    {
        if (GetPlayerId() != inActor.GetPlayerId())
        {
            //RenderManager.sInstance.RemoveComponent(mSpriteComponent.get());
        }
        return false;
    }

    public override void CompleteCreate()
    {
        GameObject go = GameObject.Find("Bomb" + 2);
        if (go == null)
            return;

        var location = new Vector3((short)Mathf.RoundToInt(GetLocation().mX), (short)Mathf.RoundToInt(GetLocation().mZ), (short)Mathf.RoundToInt(GetLocation().mY));

        GameObject bomb =GameObject.Instantiate(go, location, go.transform.rotation);
        mTarget = bomb;

        var parent = NetworkManagerClient.sInstance.GetGameObject(mParentNetworkId);
        if(parent != null)
        {
            ((CActor)parent)?.mActorBehaviour?.PlayAnimation("Bomb");
        }

    }

    public override void HandleDying()
    {
        base.HandleDying();
        if (mTarget != null)
            GameObject.Destroy(mTarget, 0.3f);
    }
}
