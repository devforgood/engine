using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace core
{
    public static class Utility
    {
        public static float PI = 3.1415926535f;

        private static Random rand = new Random();
        public static float GetRandomFloat(double maximum = 1.0, double minimum = 0.0)
        {
            return (float)(rand.NextDouble() * (maximum - minimum) + minimum);
        }

        public static Vector3 GetRandomVector( Vector3 inMin, Vector3 inMax )
        {
            Vector3 r = new Vector3(GetRandomFloat(), GetRandomFloat(), GetRandomFloat());
            return inMin + (inMax - inMin) * r;
        }

        public static Vector3 GetRandomVector(int minValue, int maxValue)
        {
            return new Vector3(rand.Next(minValue, maxValue), 1, rand.Next(minValue, maxValue));
        }


        public static float ToDegrees(float inRadians)
        {
            return inRadians * 180.0f / PI;
        }
    }

    public static class Colors
    {
        public static readonly Vector3 Black = new Vector3(0.0f, 0.0f, 0.0f);
        public static readonly Vector3 White = new Vector3(1.0f, 1.0f, 1.0f);
        public static readonly Vector3 Red = new Vector3(1.0f, 0.0f, 0.0f);
        public static readonly Vector3 Green = new Vector3(0.0f, 1.0f, 0.0f);
        public static readonly Vector3 Blue = new Vector3(0.0f, 0.0f, 1.0f);
        public static readonly Vector3 LightYellow = new Vector3(1.0f, 1.0f, 0.88f);
        public static readonly Vector3 LightBlue = new Vector3(0.68f, 0.85f, 0.9f);
        public static readonly Vector3 LightPink = new Vector3(1.0f, 0.71f, 0.76f);
        public static readonly Vector3 LightGreen = new Vector3(0.56f, 0.93f, 0.56f);
    }
}
