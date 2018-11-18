using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Andromedroids
{
    /// <summary>
    /// Single valid key is generated at startup
    /// </summary>
    class HashKey
    {
        public static uint PublicKey { get; private set; } = 0;

        private static Random r = new Random();

        private uint key;

        public HashKey()
        {
            key = (uint)(r.NextDouble() * 0xFFFFFFFF);

            Debug.WriteLine(key.ToString("X8"));

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

        public static implicit operator bool (HashKey key) => key.Validate();
    }
}
