using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Andromedroids
{
    class CheatDetection
    {
        static bool cheat = false;
        static string message = "";

        public static void ReportCheat(string format, params object[] arg)
        {
            cheat = true;
            message = string.Format(format, arg);
        }

        public void Execute()
        {
            if (cheat)
            {
                cheat = false;

                Debug.WriteLine("Cheat detected: " + message);
            }
        }
    }
}
