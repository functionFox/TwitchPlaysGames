using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TwitchPlaysGames
{
    public class config // Insert your details below, then rename this file to config.cs
    {
        public string oauth = "oauth:";
        public string nick = "YourBotName";
        public string channel = "YourChannel";
        public string defaultGameName = "Peggle Deluxe";

        public void readFile()
        {
            try
            {
                string? path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                
                Dictionary<string, string> dic = File.ReadAllLines($@"{path}\settings.txt")
                  .Select(l => l.Split(new[] { '=' }))
                  .ToDictionary(s => s[0].Trim(), s => s[1].Trim());
                oauth = "oauth:" + dic["oauth"];
                nick = dic["nick"];
                channel = dic["channel"];
                defaultGameName = dic["game"];
            }
            catch
            {
                Console.WriteLine("Error: Unable to locate settings file.");
                return;
            }
        }
    }
}
