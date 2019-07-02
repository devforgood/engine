﻿using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY
using UnityEngine;

public static partial class Vector3Extensions
{
    public static Vector3 Round(this Vector3 vec)
    {
        return new Vector3()
        {
            x = (short)Math.Round(vec.x),
            y = (short)Math.Round(vec.y),
            z = (short)Math.Round(vec.z),
        };
    }

    public static bool IsZero(this Vector3 vec)
    {
        return vec.x == 0 && vec.y == 0 && vec.z == 0;
    }

    public static ref BEPUutilities.Vector3 CopyTo(this Vector3 vec, ref BEPUutilities.Vector3 toVec)
    {
        toVec.X = vec.x;
        toVec.Y = vec.y;
        toVec.Z = vec.z;
        return ref toVec;
    }
}

#else

public struct Vector3
{
    public static readonly Vector3 zero = new Vector3(0.0f, 0.0f, 0.0f);
    public static readonly Vector3 UnitX = new Vector3(1.0f, 0.0f, 0.0f);
    public static readonly Vector3 UnitY = new Vector3(0.0f, 1.0f, 0.0f);
    public static readonly Vector3 UnitZ = new Vector3(0.0f, 0.0f, 1.0f);

    public static readonly Vector3 forward = new Vector3(0, 0, 1);
    public static readonly Vector3 back = new Vector3(0, 0, -1);
    public static readonly Vector3 right = new Vector3(1, 0, 0);
    public static readonly Vector3 left = new Vector3(-1, 0, 0);
    public static readonly Vector3 up = new Vector3(0, 1, 0);
    public static readonly Vector3 down = new Vector3(0, -1, 0);

    public float x;
    public float y;
    public float z;

    public Vector3(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    public void Set(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    public override string ToString()
    {
        return "(" + x + "," + y + "," + z + ")";
    }

    public static Vector3 operator +(Vector3 inLeft, Vector3 inRight)
    {
        return new Vector3(inLeft.x + inRight.x, inLeft.y + inRight.y, inLeft.z + inRight.z);
    }

    public static Vector3 operator -(Vector3 inLeft, Vector3 inRight)
    {
        return new Vector3(inLeft.x - inRight.x, inLeft.y - inRight.y, inLeft.z - inRight.z);
    }

    // Component-wise multiplication
    public static Vector3 operator *(Vector3 inLeft, Vector3 inRight)
    {
        return new Vector3(inLeft.x * inRight.x, inLeft.y * inRight.y, inLeft.z * inRight.z);
    }

    // Scalar multiply
    public static Vector3 operator *(float inScalar, Vector3 inVec)
    {
        return new Vector3(inVec.x * inScalar, inVec.y * inScalar, inVec.z * inScalar);
    }

    public static Vector3 operator *(Vector3 inVec, float inScalar)
    {
        return new Vector3(inVec.x * inScalar, inVec.y * inScalar, inVec.z * inScalar);
    }

    public static Vector3 operator +(Vector3 inVec, float inScalar)
    {
        return new Vector3(inVec.x + inScalar, inVec.y + inScalar, inVec.z + inScalar);
    }

    public static Vector3 operator -(Vector3 inVec, float inScalar)
    {
        return new Vector3(inVec.x - inScalar, inVec.y - inScalar, inVec.z - inScalar);
    }

    public float Length()
    {
        return (float)Math.Sqrt(x * x + y * y + z * z);
    }

    public float magnitude
    {
        get
        {
            if (IsZero())
                return 0;

            return (float)Math.Sqrt(x * x + y * y + z * z);
        }
    }

    public float sqrMagnitude
    {
        get
        {
            return x * x + y * y + z * z;
        }
    }

    public bool IsZero()
    {
        return x == 0 && y == 0 && z == 0;
    }

    public float LengthSq()
    {
        return x * x + y * y + z * z;
    }

    public float Length2D()
    {
        return (float)Math.Sqrt(x * x + y * y);
    }

    public float LengthSq2D()
    {
        return x * x + y * y;
    }

    public void Normalize()
    {
        if (x == 0 && y == 0 && z == 0)
            return;

        float length = Length();
        x /= length;
        y /= length;
        z /= length;
    }

    public void Normalize2D()
    {
        if (x == 0 && y == 0)
            return;

        float length = Length2D();
        x /= length;
        y /= length;
    }

    public static float Dot(Vector3 inLeft, Vector3 inRight)
    {
        return (inLeft.x * inRight.x + inLeft.y * inRight.y + inLeft.z * inRight.z);
    }

    public static float Dot2D(Vector3 inLeft, Vector3 inRight)
    {
        return (inLeft.x * inRight.x + inLeft.y * inRight.y);
    }

    public static Vector3 Cross(Vector3 inLeft, Vector3 inRight)
    {
        Vector3 temp = default(Vector3);
        temp.x = inLeft.y * inRight.z - inLeft.z * inRight.y;
        temp.y = inLeft.z * inRight.x - inLeft.x * inRight.z;
        temp.z = inLeft.x * inRight.y - inLeft.y * inRight.x;
        return temp;
    }

    public static Vector3 Lerp(Vector3 inA, Vector3 inB, float t)
    {
        return inA + t * (inB - inA);
    }

    public void Copy(Vector3 fromVec)
    {
        x = fromVec.x;
        y = fromVec.y;
        z = fromVec.z;
    }

#if _USE_BEPU_PHYSICS
    public ref BEPUutilities.Vector3 CopyTo(ref BEPUutilities.Vector3 toVec)
    {
        toVec.X = x;
        toVec.Y = y;
        toVec.Z = z;
        return ref toVec;
    }
#endif

    public Vector3 Round()
    {
        return new Vector3()
        {
            x = (short)Math.Round(this.x),
            y = (short)Math.Round(this.y),
            z = (short)Math.Round(this.z),
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
        return (this.x == other.x && this.y == other.y && this.z == other.z);
    }



}

#endif