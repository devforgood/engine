using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
#if USE_INPUT_STATE_OLD
        float mRotation;

        public void SetRotation(float inRotation)
        {
            //should we normalize using fmodf?
            mRotation = inRotation;
        }

        public float GetRotation() { return mRotation; }


#else 
        public Vector3 mDirection = new Vector3();
        public Vector3 GetRotation() { return mDirection; }
#endif

        float mCollisionRadius;


        float mScale;
        int mIndexInWorld;

        bool mDoesWantToDie;

        int mNetworkId;

        public int NetworkId { get { return mNetworkId; } }

        public virtual uint32_t GetClassId() { return (uint32_t)GameObjectClassId.kGameObject; }

        public static NetGameObject CreateInstance() { return new NetGameObject(); }

        public virtual Actor GetAsActor() { return null; }
        public virtual uint32_t GetAllStateMask() { return 0; }

        public virtual bool HandleCollisionWithActor(Actor inActor) { return true; }

        public virtual void NetUpdate() { }
        public virtual void LateUpdate() { }
        public virtual void HandleDying() { }

        public void SetIndexInWorld(int inIndex) { mIndexInWorld = inIndex; }
        public int GetIndexInWorld() { return mIndexInWorld; }



        public void SetScale(float inScale) { mScale = inScale; }
        public float GetScale() { return mScale; }


        public Vector3 GetLocation() { return mLocation; }
        public void SetLocation(Vector3 inLocation) { mLocation = inLocation; }

        public float GetCollisionRadius() { return mCollisionRadius; }
        public void SetCollisionRadius(float inRadius) { mCollisionRadius = inRadius; }

        public Vector3 GetForwardVector()
        {
#if USE_INPUT_STATE_OLD
            //should we cache this when you turn?
            return new Vector3((float)Math.Sin(mRotation), (float)-Math.Cos(mRotation), 0.0f);
#else
            return mDirection;
#endif
        }


        public void SetColor(Vector3 inColor) { mColor = inColor; }
        public Vector3 GetColor() { return mColor; }

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

    }
}
