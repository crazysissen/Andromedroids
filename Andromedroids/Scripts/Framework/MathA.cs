using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andromedroids
{
    static class MathA
    {
        /// <summary>Accelerating sine. Equation that from 0-1 accelerates according to a sine wave</summary>
        public static float SineA(float value)
            => (float)Math.Sin((double)value * Math.PI * 0.5);

        /// <summary>Decellerating sine. Equation that from 0-1 decellerates according to a sine wave</summary>
        public static float SineD(float value)
            => (float)Math.Sin((value - 1) * Math.PI * 0.5) + 1;
    }
}
