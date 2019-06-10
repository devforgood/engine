using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class CProjectile : core.Projectile
{
    public static new core.NetGameObject StaticCreate(byte worldId) { return new CProjectile(); }
    public override void Read(NetIncomingMessage inInputStream )
    {
        bool stateBit;

        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            Vector3 location = default(Vector3);
            inInputStream.Read(ref location);


            Vector3 velocity = default(Vector3);
            inInputStream.Read(ref velocity);
            SetVelocity(velocity);

            //dead reckon ahead by rtt, since this was spawned a while ago!
            SetLocation(location + velocity * NetworkManagerClient.sInstance.GetRoundTripTime());


            inInputStream.Read(ref mDirection);
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

    }
    public override bool HandleCollisionWithActor(core.Actor inActor)
    {
        if (GetPlayerId() != inActor.GetPlayerId())
        {
            //RenderManager.sInstance.RemoveComponent(mSpriteComponent.get());
        }
        return false;
    }

}
