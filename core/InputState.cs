using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    class InputState
    {
        float mDesiredRightAmount, mDesiredLeftAmount;
        float mDesiredForwardAmount, mDesiredBackAmount;
        bool mIsShooting;

        public InputState()
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
}
