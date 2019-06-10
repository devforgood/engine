using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY
using UnityEngine;
#endif 

using uint32_t = System.UInt32;
using uint8_t = System.Byte;

namespace core
{
    public enum GameObjectClassId
    {
        kGameObject = 1196376650,
        kProp = 1297044819,
        kActor = 1380139348,
        kProjectile = 1497453134,
        kBomb = 1222222222,
    }

    public partial class NetGameObject
    {
        Vector3 mLocation = new Vector3();
        Vector3 mColor = new Vector3();
        public Vector3 mDirection = new Vector3();
        public Vector3 GetRotation() { return mDirection; }

        float mCollisionRadius;

        float mScale;
        int mIndexInWorld;

        bool mDoesWantToDie;

        int mNetworkId;


        public int NetworkId { get { return mNetworkId; } }

        public byte WorldId { get; set; }

        public virtual uint32_t GetClassId() { return (uint32_t)GameObjectClassId.kGameObject; }


        public virtual Actor GetAsActor() { return null; }
        public virtual uint32_t GetAllStateMask() { return 0; }

        public virtual bool HandleCollisionWithActor(Actor inActor) { return true; }

        public virtual void Update() { }
        public virtual void LateUpdate() { }
        public virtual void HandleDying() { }

        public void SetIndexInWorld(int inIndex) { mIndexInWorld = inIndex; }
        public int GetIndexInWorld() { return mIndexInWorld; }



        public void SetScale(float inScale) { mScale = inScale; }
        public float GetScale() { return mScale; }


        public ref Vector3 GetLocation() { return ref mLocation; }
        public void SetLocation(Vector3 inLocation)
        {
            //if (inLocation.Equals(mLocation) == false)
            //{
            //    World.Instance(WorldId).mWorldMap.ChangeLocation(this, mLocation, inLocation);
            //}
            mLocation = inLocation;
        }

        public float GetCollisionRadius() { return mCollisionRadius; }
        public void SetCollisionRadius(float inRadius) { mCollisionRadius = inRadius; }

        public Vector3 GetForwardVector()
        {
            return mDirection;
        }


        public void SetColor(Vector3 inColor) { mColor = inColor; }
        public ref Vector3 GetColor() { return ref mColor; }

        public bool DoesWantToDie() { return mDoesWantToDie; }
        public void SetDoesWantToDie(bool inWants) { mDoesWantToDie = inWants; }

        public int GetNetworkId() { return mNetworkId; }
        public void SetNetworkId(int inNetworkId)
        {
            //this doesn't put you in the map or remove you from it
            mNetworkId = inNetworkId;

        }

        public virtual uint32_t Write(NetOutgoingMessage inOutputStream, uint32_t inDirtyState)
        {
            return 0;
        }
        public virtual void Read(NetIncomingMessage inInputStream) { }


        public virtual void CompleteCreate()
        {

        }

        public virtual void CompleteRemove()
        {

        }

        public virtual int OnExplode(int player_id, int parentNetworkId, int damage)
        {
            return 0;
        }
    }
}
