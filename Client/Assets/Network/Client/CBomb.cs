using Lidgren.Network;
using UnityEngine;

class CBomb : core.Bomb
{
    public GameObject mTarget = null;
    public BombBehaviour mBombBehaviour = null;

    public static new core.NetGameObject StaticCreate(byte worldId) { return new CBomb(); }
    public override void Read(NetIncomingMessage inInputStream)
    {
        bool stateBit;

        stateBit = inInputStream.ReadBoolean();
        if (stateBit) 
        {
            Vector3 location = default(Vector3);
            inInputStream.Read(ref location);

            //dead reckon ahead by rtt, since this was spawned a while ago!
            SetLocation(location);
        }

        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            Vector3 color = default(Vector3);
            inInputStream.Read(ref color);
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

        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            mIsExplode = inInputStream.ReadBoolean();
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

        GameObject bomb = GameObject.Instantiate(go, GetLocation(), go.transform.rotation);
        mTarget = bomb;
        mBombBehaviour = bomb.GetComponent<BombBehaviour>();

        var parent = NetworkManagerClient.sInstance.GetGameObject(mParentNetworkId);
        if (parent != null)
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

    public override void Update()
    {
        if(mIsExplode)
        {
            if (mBombBehaviour != null)
                mBombBehaviour.Explode();

            mIsExplode = false;
        }
    }
}
