using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace core
{

    public class InputStateOld 
    {
        public float mDesiredRightAmount, mDesiredLeftAmount;
        public float mDesiredForwardAmount, mDesiredBackAmount;
        public bool mIsShooting;

        public override string ToString()
        {
            return "mDesiredRightAmount: " + mDesiredRightAmount
                + ",mDesiredLeftAmount:  " + mDesiredLeftAmount
                + ",mDesiredForwardAmount:  " + mDesiredForwardAmount
                + ",mDesiredBackAmount:  " + mDesiredBackAmount
                + ",mIsShooting:  " + mIsShooting
                ;
        }

        public InputStateOld()
        {
            mDesiredRightAmount = 0;
            mDesiredLeftAmount = 0;
            mDesiredForwardAmount = 0;
            mDesiredBackAmount = 0;
            mIsShooting = false;
        }

        public float GetDesiredHorizontalDelta() { return mDesiredRightAmount - mDesiredLeftAmount; }
        public float GetDesiredVerticalDelta() { return mDesiredForwardAmount - mDesiredBackAmount; }
        public bool IsShooting() { return mIsShooting; }

        void WriteSignedBinaryValue(NetOutgoingMessage inOutputStream, float inValue)
        {
            bool isNonZero = (inValue != 0.0f);
            inOutputStream.Write(isNonZero);
            if (isNonZero)
            {
                inOutputStream.Write(inValue > 0.0f);
            }
        }

        void ReadSignedBinaryValue(NetIncomingMessage inInputStream, out float outValue )
        {
            bool isNonZero = inInputStream.ReadBoolean();
            if (isNonZero)
            {
                bool isPositive = inInputStream.ReadBoolean();
                outValue = isPositive ? 1.0f : -1.0f;
            }
            else
            {
                outValue = 0.0f;
            }
        }

        public bool Write(NetOutgoingMessage inOutputStream )
        {
            WriteSignedBinaryValue(inOutputStream, GetDesiredHorizontalDelta());
            WriteSignedBinaryValue(inOutputStream, GetDesiredVerticalDelta());
            inOutputStream.Write(mIsShooting);

            return false;
        }
        public bool Read(NetIncomingMessage inInputStream )
        {
            ReadSignedBinaryValue(inInputStream, out mDesiredRightAmount);
            ReadSignedBinaryValue(inInputStream, out mDesiredForwardAmount);
            mIsShooting = inInputStream.ReadBoolean();

            return true;
        }
    }

    public class InputState
    {
        public bool mIsForward;
        public bool mIsBack;
        public bool mIsRight;
        public bool mIsLeft;
        public bool mIsShooting;

        public override string ToString()
        {
            return "mIsForward: " + mIsForward
                + ",mIsBack:  " + mIsBack
                + ",mIsRight:  " + mIsRight
                + ",mIsLeft:  " + mIsLeft
                + ",mIsShooting:  " + mIsShooting
                ;
        }

        public InputState()
        {
            mIsForward = false;
            mIsBack = false;
            mIsRight = false;
            mIsLeft = false;
            mIsShooting = false;
        }

        public bool IsShooting() { return mIsShooting; }


        public bool Write(NetOutgoingMessage inOutputStream)
        {
            inOutputStream.Write(mIsForward);
            inOutputStream.Write(mIsBack);
            inOutputStream.Write(mIsRight);
            inOutputStream.Write(mIsLeft);
            inOutputStream.Write(mIsShooting);

            return false;
        }
        public bool Read(NetIncomingMessage inInputStream)
        {
            mIsForward = inInputStream.ReadBoolean();
            mIsBack = inInputStream.ReadBoolean();
            mIsRight = inInputStream.ReadBoolean();
            mIsLeft = inInputStream.ReadBoolean();
            mIsShooting = inInputStream.ReadBoolean();

            return true;
        }
    }
}
