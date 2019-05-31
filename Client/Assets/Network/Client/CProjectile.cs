﻿using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class CProjectile : core.Projectile
{
    public static new core.NetGameObject StaticCreate(byte worldId) { return new CProjectile(); }
    public override void Read(NetIncomingMessage inInputStream )
    {
        bool stateBit;

        stateBit = inInputStream.ReadBoolean();
        if (stateBit)
        {
            core.Vector3 location = new core.Vector3();
            inInputStream.Read(location);


            core.Vector3 velocity = new core.Vector3();
            inInputStream.Read(velocity);
            SetVelocity(velocity);

            //dead reckon ahead by rtt, since this was spawned a while ago!
            SetLocation(location + velocity * NetworkManagerClient.sInstance.GetRoundTripTime());


            inInputStream.Read(mDirection);
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
