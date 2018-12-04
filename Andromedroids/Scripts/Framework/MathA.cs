using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Andromedroids
{
    static class MathA
    {
        /// <summary>Accelerating sine. Equation that from 0-1 accelerates according to a sine wave</summary>
        public static float SineA(float value)
            => (float)Math.Sin(value * Math.PI * 0.5);

        /// <summary>Decellerating sine. Equation that from 0-1 decellerates according to a sine wave</summary>
        public static float SineD(float value)
            => (float)Math.Sin((value - 1) * Math.PI * 0.5) + 1;

        public static float Lerp(this float min, float max, float value)
            => min + (max - min) * value;

        public static Vector2 Rotate(this Vector2 vector, float radian)
        {
            float sin = (float)Math.Sin(radian);
            float cos = (float)Math.Cos(radian);

            vector = new Vector2()
            {
                X = (cos * vector.X) - (sin * vector.Y),
                Y = (sin * vector.X) + (cos * vector.Y)
            };
            
            return vector;
        }

        public static float Clamp(this float value, float min, float max)
        {
            if (value > max)
                return max;

            if (value < min)
                return min;

            return value;
        }
    }
}
