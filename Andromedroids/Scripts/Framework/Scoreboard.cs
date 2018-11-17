using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Andromedroids
{
    [Serializable]
    class Scoreboard
    {
        const string
            SAVEDIRECTORY = @"\Scoreboards",
            EXTENSION = ".SCORE";

        static string SaveDirectory => AppDomain.CurrentDomain.BaseDirectory + SAVEDIRECTORY;

        public static Scoreboard ImportFromFile(string scoreboardName)
        {
            if (!Directory.Exists(SaveDirectory) || !File.Exists(FileDirectory(scoreboardName)))
                return null;

            return File.ReadAllBytes(FileDirectory(scoreboardName)).ToObject<Scoreboard>();
        }

        public void WriteToFile(string scoreboardName)
        {
            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            File.WriteAllBytes(FileDirectory(scoreboardName), this.ToBytes());
        }

        static string FileDirectory(string scoreboardName) => SaveDirectory + @"\" + scoreboardName + EXTENSION;
    }
}
