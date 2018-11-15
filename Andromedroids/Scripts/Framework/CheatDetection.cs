using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Andromedroids
{
    class CheatDetection
    {
        static bool cheat = false;
        static string message = "";
        static int targetThread = 0;

        public static void ReportCheat(string format, params object[] arg)
        {
            cheat = true;
            message = string.Format(format, arg);
            targetThread = Thread.CurrentThread.ManagedThreadId;
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
