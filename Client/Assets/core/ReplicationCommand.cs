using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uint32_t = System.UInt32;

namespace core
{
    public enum ReplicationAction
    {
        RA_Create,
        RA_Update,
        RA_Destroy,
        RA_RPC,
        RA_MAX
    };

    public static partial class NetBufferExtensions
    {
        public static void Write(this NetBuffer buff, ReplicationAction r)
        {
            buff.Write((byte)r, 2);
        }

        public static void Read(this NetBuffer buff, ReplicationAction r)
        {
            r = (ReplicationAction)buff.ReadByte(2);
        }
    }

    public class ReplicationCommand
    {
        uint32_t mDirtyState;
        ReplicationAction mAction;

        public ReplicationCommand() { }
        public ReplicationCommand(uint32_t inInitialDirtyState)
        {
            mAction = ReplicationAction.RA_Create;
            mDirtyState = inInitialDirtyState;
        }
        public void HandleCreateAckd() { if (mAction == ReplicationAction.RA_Create) { mAction = ReplicationAction.RA_Update; } }
        public void AddDirtyState(uint32_t inState) { mDirtyState |= inState; }
        public void SetDestroy() { mAction = ReplicationAction.RA_Destroy; }

        public bool HasDirtyState() { return (mAction == ReplicationAction.RA_Destroy) || (mDirtyState != 0); }

        public ReplicationAction GetAction() { return mAction; }
        public uint32_t GetDirtyState() { return mDirtyState; }
        public void ClearDirtyState(uint32_t inStateToClear)
        {
            mDirtyState &= ~inStateToClear;

            if (mAction == ReplicationAction.RA_Destroy)
            {
                mAction = ReplicationAction.RA_Update;
            }
        }


    }
}
