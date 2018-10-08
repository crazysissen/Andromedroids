using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Andromedroids
{
    class HashKey
    {
        public static Int64 PublicKey { get; private set; } = 0;

        private static Random r = new Random();

        private Int64 key;

        public HashKey()
        {
            key = (Int64)(r.NextDouble() * 0xFFFFFFFF);

            if (PublicKey == 0)
                PublicKey = key;
        }

        public bool Validate()
        {
            if (key == PublicKey)
                return true;

            CheatDetection.ReportCheat("Attempt to use an invalid key. Used key: {0}, Desired key: {1}", key.ToString("X8"), PublicKey.ToString("X8"));
            return false;
        }

        public bool Validate(string targetMethod)
        {
            if (key == PublicKey)
                return true;

            CheatDetection.ReportCheat("Attempt to use an invalid key. Used key: {0}, Desired key: {1}. Target method was: {2}.", key.ToString("X8"), PublicKey.ToString("X8"), targetMethod);
            return false;
        }
    }
}
