﻿using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace core
{
    public class Vector3
    {
        public static readonly Vector3 Zero = new Vector3 (0.0f, 0.0f, 0.0f);
        public static readonly Vector3 UnitX = new Vector3(1.0f, 0.0f, 0.0f);
        public static readonly Vector3 UnitY = new Vector3(0.0f, 1.0f, 0.0f);
        public static readonly Vector3 UnitZ = new Vector3(0.0f, 0.0f, 1.0f);

        public static readonly Vector3 forward  =    new Vector3( 0,  0,  1); 
        public static readonly Vector3 back     =    new Vector3( 0,  0, -1); 
        public static readonly Vector3 right    =    new Vector3( 1,  0,  0); 
        public static readonly Vector3 left     =    new Vector3(-1,  0,  0); 

        public float mX;
        public float mY;
        public float mZ;

        public Vector3(float x, float y, float z) 
        {
            mX = x;
            mY = y;
            mZ = z;
        }
        public Vector3()
        {
            mX = 0.0f;
            mY = 0.0f;
            mZ = 0.0f;
        }

        public void Set(float x, float y, float z)
        {
            mX = x;
            mY = y;
            mZ = z;
        }

        public override string ToString()
        {
            return "(" + mX + "," + mY + "," + mZ + ")";
        }

        public static Vector3 operator +(Vector3 inLeft, Vector3 inRight)
        {
            return new Vector3(inLeft.mX + inRight.mX, inLeft.mY + inRight.mY, inLeft.mZ + inRight.mZ);
        }

        public static Vector3 operator -(Vector3 inLeft, Vector3 inRight)
        {
            return new Vector3(inLeft.mX - inRight.mX, inLeft.mY - inRight.mY, inLeft.mZ - inRight.mZ);
        }

        // Component-wise multiplication
        public static Vector3 operator *(Vector3 inLeft, Vector3 inRight)
        {
            return new Vector3(inLeft.mX * inRight.mX, inLeft.mY * inRight.mY, inLeft.mZ * inRight.mZ);
        }

        // Scalar multiply
        public static Vector3 operator *(float inScalar, Vector3 inVec)
        {
            return new Vector3(inVec.mX * inScalar, inVec.mY * inScalar, inVec.mZ * inScalar);
        }

        public static Vector3 operator *(Vector3 inVec, float inScalar)
        {
            return new Vector3(inVec.mX * inScalar, inVec.mY * inScalar, inVec.mZ * inScalar);
        }

        public static Vector3 operator +(Vector3 inVec, float inScalar)
        {
            return new Vector3(inVec.mX + inScalar, inVec.mY + inScalar, inVec.mZ + inScalar);
        }

        public static Vector3 operator -(Vector3 inVec, float inScalar)
        {
            return new Vector3(inVec.mX - inScalar, inVec.mY - inScalar, inVec.mZ - inScalar);
        }

        public float Length()
        {
            return (float)Math.Sqrt(mX * mX + mY * mY + mZ * mZ);
        }

        public float magnitude
        {
            get
            {
                if (IsZero())
                    return 0;

                return (float)Math.Sqrt(mX * mX + mY * mY + mZ * mZ);
            }
        }

        public bool IsZero()
        {
            return mX == 0 && mY == 0 && mZ == 0;
        }

        public float LengthSq()
        {
            return mX * mX + mY * mY + mZ * mZ;
        }

        public float Length2D()
        {
            return (float)Math.Sqrt(mX * mX + mY * mY);
        }

        public float LengthSq2D()
        {
            return mX * mX + mY * mY;
        }

        public void Normalize()
        {
            if (mX == 0 && mY == 0 && mZ == 0)
                return;

            float length = Length();
            mX /= length;
            mY /= length;
            mZ /= length;
        }

        public void Normalize2D()
        {
            if (mX == 0 && mY == 0)
                return;

            float length = Length2D();
            mX /= length;
            mY /= length;
        }

        public static float Dot(Vector3 inLeft, Vector3 inRight)
        {
            return (inLeft.mX * inRight.mX + inLeft.mY * inRight.mY + inLeft.mZ * inRight.mZ);
        }

        public static float Dot2D(Vector3 inLeft, Vector3 inRight)
        {
            return (inLeft.mX * inRight.mX + inLeft.mY * inRight.mY);
        }

        public static Vector3 Cross(Vector3 inLeft, Vector3 inRight)
        {
            Vector3 temp = new Vector3();
            temp.mX = inLeft.mY * inRight.mZ - inLeft.mZ * inRight.mY;
            temp.mY = inLeft.mZ * inRight.mX - inLeft.mX * inRight.mZ;
            temp.mZ = inLeft.mX * inRight.mY - inLeft.mY * inRight.mX;
            return temp;
        }

        public static Vector3 Lerp(Vector3 inA, Vector3 inB, float t)
        {
            return inA + t * (inB - inA);
        }

        public Vector3 Clone()
        {
            return new Vector3()
            {
                mX = this.mX,
                mY = this.mY,
                mZ = this.mZ,
            };
        }

        public Vector3 Round()
        {
            return new Vector3()
            {
                mX = (short)Math.Round(this.mX),
                mY = (short)Math.Round(this.mY),
                mZ = (short)Math.Round(this.mZ),
            };
        }

        //
        // 요약:
        //     이 인스턴스와 다른 벡터가 같은지 여부를 나타내는 값을 반환합니다.
        //
        // 매개 변수:
        //   other:
        //     다른 벡터입니다.
        //
        // 반환 값:
        //     두 벡터가 같으면 true이고, 그렇지 않으면 false입니다.
        public bool Equals(Vector3 other)
        {
            return (this.mX == other.mX && this.mY == other.mY && this.mZ == other.mZ);
        }



    }
}
