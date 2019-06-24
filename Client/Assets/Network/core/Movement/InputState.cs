using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace core
{
    public class InputState
    {
        public bool mIsForward;
        public bool mIsBack;
        public bool mIsRight;
        public bool mIsLeft;
        public bool mIsShooting;
        public bool mIsBomb;
        public float mYaxis;

        public override string ToString()
        {
            return "mIsForward: " + mIsForward
                + ",mIsBack:  " + mIsBack
                + ",mIsRight:  " + mIsRight
                + ",mIsLeft:  " + mIsLeft
                + ",mIsShooting:  " + mIsShooting
                + ",mIsBomb:  " + mIsBomb
                + ",mYaxis:  " + mYaxis
                ;
        }

        public InputState()
        {
            mIsForward = false;
            mIsBack = false;
            mIsRight = false;
            mIsLeft = false;
            mIsShooting = false;
            mIsBomb = false;
            mYaxis = 0.0f;
        }

        public bool IsShooting() { return mIsShooting; }


        public bool Write(NetOutgoingMessage inOutputStream)
        {
            inOutputStream.Write(mIsForward);
            inOutputStream.Write(mIsBack);
            inOutputStream.Write(mIsRight);
            inOutputStream.Write(mIsLeft);
            inOutputStream.Write(mIsShooting);
            inOutputStream.Write(mIsBomb);
            inOutputStream.Write(mYaxis);

            return false;
        }
        public bool Read(NetIncomingMessage inInputStream)
        {
            mIsForward = inInputStream.ReadBoolean();
            mIsBack = inInputStream.ReadBoolean();
            mIsRight = inInputStream.ReadBoolean();
            mIsLeft = inInputStream.ReadBoolean();
            mIsShooting = inInputStream.ReadBoolean();
            mIsBomb = inInputStream.ReadBoolean();
            mYaxis = inInputStream.ReadFloat();

            return true;
        }
    }
}
