using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TwitchPlaysGames
{
    public static class Macros
    {
        public static void LeftClick()
        {
            SendToWin.MouseEvent(SendToWin.MouseEventFlags.LeftDown);
            SendToWin.MouseEvent(SendToWin.MouseEventFlags.LeftUp);
        }

        public static void RightClick()
        {
            SendToWin.MouseEvent(SendToWin.MouseEventFlags.RightDown);
            SendToWin.MouseEvent(SendToWin.MouseEventFlags.RightUp);
        }
    }
}
