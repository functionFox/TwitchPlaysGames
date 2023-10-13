using System;
using System.Windows.Input;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Emgu.CV;
using Emgu.CV.Structure;
using static TwitchPlaysGames.SendToWin;
using ws = WindowScrape;
using System.Net.WebSockets;

namespace TwitchPlaysGames
{
    class Program
    {
        static async Task Main(string[] args)
        {
            config configParam = new config();
            configParam.readFile(); // If you don't want to use a settings file, comment this line out

            Process window = new Process();
            IntPtr iHandle = new();
            MousePoint mousePos = new MousePoint();
            Point xy = new Point();

            GameData.Name = configParam.defaultGameName;
            GameData.UpgradeMode = false;

            Process p = GetProcess();
            string? path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (path == null)
            {
                Console.WriteLine("Error! Path does not exist!");
                return;
            }
            string tempPath = Path.GetTempPath();

            Image<Bgr, byte> imgNovaA = new Image<Bgr, byte>(@$"{path}\images\NovaA.png");
            
            string title = ws.Static.HwndInterface.GetHwndTitle(iHandle);

            IntPtr Hwnd;
            try
            {
                Hwnd = p.MainWindowHandle;
            }
            catch
            {
                Hwnd = (IntPtr)0;
                Console.WriteLine("Error: Window not available!");
            }

            string password = configParam.oauth;
            string botUsername = configParam.nick;
            string channel = configParam.channel;
            
            mousePos = GetCursorPosition();

            xy = ws.Static.HwndInterface.GetHwndPos(Hwnd);
            Size size = ws.Static.HwndInterface.GetHwndSize(Hwnd);

            ws.Static.HwndInterface.ActivateWindow(Hwnd);

            var twitchBot = new ChatBot(botUsername, password);
            twitchBot.Start().SafeFireAndForget();
            //We could .SafeFireAndForget() these two calls if we want to
            await twitchBot.JoinChannel(channel);
            await twitchBot.SendMessage(channel, $"Hello! {configParam.nick} is now online! Beep boop!");

            twitchBot.OnMessage += async (sender, twitchChatMessage) =>
            {
                Console.WriteLine($"{twitchChatMessage.Sender} said '{twitchChatMessage.Message}'");
                //Listen for !hey command
                if (twitchChatMessage.Message.StartsWith("!hey"))
                {
                    await twitchBot.SendMessage(twitchChatMessage.Channel, $"Hey there {twitchChatMessage.Sender}");
                }

                if (twitchChatMessage.Message.StartsWith("!continue"))
                {
                    
                    string newMsg = "X: " + GetCursorPosition().X.ToString() + "Y: " + GetCursorPosition().Y.ToString();
                    await twitchBot.SendMessage(twitchChatMessage.Channel, $"I'm seeing {newMsg}!");
                    Macros.LeftClick();

                    // SendToWin.SendMessage((int)iHandle, SendToWin.WM_LBUTTONUP, 0, SendToWin.CreateLParam(x,y));
                }
                if (twitchChatMessage.Message.ToLower().StartsWith("!setxy"))
                {
                    string newMsg = "X: 823, Y: 495";
                    await twitchBot.SendMessage(twitchChatMessage.Channel, $"I'm setting {newMsg}");
                    SetCursorPosition(823, 495);
                    Macros.LeftClick();
                }
                if (twitchChatMessage.Message.ToLower().StartsWith("!getxy"))
                {
                    mousePos = GetCursorPosition();
                    xy = ws.Static.HwndInterface.GetHwndPos(Hwnd);
                    string newMsg = $"X: {mousePos.X}, Y: {mousePos.Y}, wX: {xy.X}, xY: {xy.Y}";
                    await twitchBot.SendMessage(twitchChatMessage.Channel, $"{GameData.Name} is at {newMsg}! Size is {size}!");
                }
                if (twitchChatMessage.Message.ToLower().StartsWith("!start") && GameData.Name == "Peggle Deluxe")
                {
                    
                    await twitchBot.SendMessage(twitchChatMessage.Channel, $"Let's start a new adventure!");
                    Macros.SetMousePos(xy.X + 666, xy.Y + 170);
                    Macros.LeftClick();
                }

                if (twitchChatMessage.Message.ToLower().StartsWith("!reroll") && GameData.Name == "Nova Drift" && GameData.UpgradeMode)
                {
                    await twitchBot.SendMessage(twitchChatMessage.Channel, "Understood! Procssing reroll attempt...");
                    ws.Static.HwndInterface.ActivateWindow(Hwnd);
                    Macros.PressR();
                }
                if (twitchChatMessage.Message.ToLower().StartsWith("!upgrade") && GameData.Name == "Nova Drift" && !GameData.UpgradeMode)
                {
                    GameData.UpgradeMode = true;
                    Point matchLocation = Macros.Snap(Hwnd, tempPath, imgNovaA);
                    GameData.UpgradeOnLeft = matchLocation.X > size.Width * 0.6 ? false : true;
                    if (GameData.UpgradeOnLeft)
                    {
                        await twitchBot.SendMessage(twitchChatMessage.Channel, $"Upgrade mode engaged! Type !up# (1-7) to upgrade slot, or (8, 9, 0) to exchange ship mods. Use !reroll to expend a reroll. Use !cancel to cancel upgrading and resume gameplay.");
                    }
                    else
                    {
                        await twitchBot.SendMessage(twitchChatMessage.Channel, $"Upgrade mode engaged! Type !up# (1-7) to upgrade slot. Use !reroll to expend a reroll. Use !cancel to cancel upgrading and resume gameplay.");
                    }
                    Macros.PressSpace();
                }
                if ((twitchChatMessage.Message.ToLower().StartsWith("!up") || twitchChatMessage.Message.ToLower().StartsWith("!cancel")) 
                        && !twitchChatMessage.Message.ToLower().StartsWith("!upgrade") && GameData.Name == "Nova Drift" && GameData.UpgradeMode)
                {
                    string message = twitchChatMessage.Message.ToLower();
                    Point matchLocation = Macros.Snap(Hwnd, tempPath, imgNovaA);

                    // await twitchBot.SendMessage(twitchChatMessage.Channel, $"{matchLocation}"); // uncomment to see match location value

                    GameData.UpgradeOnLeft = matchLocation.X > size.Width * 0.6 ? false : true;

                    Point relativeCursorPos = new Point(GetCursorPosition().X - ws.Static.HwndInterface.GetHwndPos(Hwnd).X, GetCursorPosition().Y - ws.Static.HwndInterface.GetHwndPos(Hwnd).Y);
                    Point relativeClickPos = new Point();
                    int plusRight = (int)(0.15 * size.Width);
                    
                    string start = message.Substring(0, Math.Min(4, message.Length)); // Get the first 4 characters
                    switch (start)
                    {
                        case "!up1":
                            relativeClickPos.X = (int)(0.32 * size.Width);
                            relativeClickPos.X = GameData.UpgradeOnLeft ? relativeClickPos.X : relativeClickPos.X + plusRight;
                            relativeClickPos.Y = (int)(0.24 * size.Height);
                            break;
                        case "!up2":
                            relativeClickPos.X = (int)(0.38 * size.Width);
                            relativeClickPos.X = GameData.UpgradeOnLeft ? relativeClickPos.X : relativeClickPos.X + plusRight;
                            relativeClickPos.Y = (int)(0.24 * size.Height);
                            break;
                        case "!up3":
                            relativeClickPos.X = (int)(0.28 * size.Width);
                            relativeClickPos.X = GameData.UpgradeOnLeft ? relativeClickPos.X : relativeClickPos.X + plusRight;
                            relativeClickPos.Y = (int)(0.35 * size.Height);
                            break;
                        case "!up4":
                            relativeClickPos.X = (int)(0.35 * size.Width);
                            relativeClickPos.X = GameData.UpgradeOnLeft ? relativeClickPos.X : relativeClickPos.X + plusRight;
                            relativeClickPos.Y = (int)(0.35 * size.Height);
                            break;
                        case "!up5":
                            relativeClickPos.X = (int)(0.40 * size.Width);
                            relativeClickPos.X = GameData.UpgradeOnLeft ? relativeClickPos.X : relativeClickPos.X + plusRight;
                            relativeClickPos.Y = (int)(0.35 * size.Height);
                            break;
                        case "!up6":
                            relativeClickPos.X = (int)(0.32 * size.Width);
                            relativeClickPos.X = GameData.UpgradeOnLeft ? relativeClickPos.X : relativeClickPos.X + plusRight;
                            relativeClickPos.Y = (int)(0.42 * size.Height);
                            break;
                        case "!up7":
                            relativeClickPos.X = (int)(0.38 * size.Width);
                            relativeClickPos.X = GameData.UpgradeOnLeft ? relativeClickPos.X : relativeClickPos.X + plusRight;
                            relativeClickPos.Y = (int)(0.42 * size.Height);
                            break;
                        case "!up8":
                            relativeClickPos.X = (int)(0.32 * size.Width);
                            relativeClickPos.X = GameData.UpgradeOnLeft ? relativeClickPos.X : 0;
                            relativeClickPos.Y = (int)(0.63 * size.Height);
                            break;
                        case "!up9":
                            relativeClickPos.X = (int)(0.38 * size.Width);
                            relativeClickPos.X = GameData.UpgradeOnLeft ? relativeClickPos.X : 0;
                            relativeClickPos.Y = (int)(0.63 * size.Height);
                            break;
                        case "!up0":
                            relativeClickPos.X = (int)(0.35 * size.Width);
                            relativeClickPos.X = GameData.UpgradeOnLeft ? relativeClickPos.X : 0;
                            relativeClickPos.Y = (int)(0.69 * size.Height);
                            break;
                        case "!can":
                            GameData.UpgradeMode = false;
                            await twitchBot.SendMessage(twitchChatMessage.Channel, $"Upgrade Canceled. Resuming play.");
                            ws.Static.HwndInterface.ActivateWindow(Hwnd);
                            Macros.PressEscape();
                            break;
                        default:
                            await twitchBot.SendMessage(twitchChatMessage.Channel, $"Upgrade command not recognized. Type !up# (1-7) to upgrade, !up# (8, 9, or 0) to exchange gear options, or !cancel to cancel.");
                            break;
                    }
                    relativeClickPos.X = relativeClickPos.X + xy.X;
                    relativeClickPos.Y = relativeClickPos.Y + xy.Y;
                    if ( relativeClickPos.X - xy.X != 0 )
                    {
                        await twitchBot.SendMessage(twitchChatMessage.Channel, $"Processing {message}... Sending input to game... Disengaging Upgrade Mode.");
                        Macros.SetMousePos(relativeClickPos.X, relativeClickPos.Y);
                        ws.Static.HwndInterface.ActivateWindow(Hwnd);
                        Thread.Sleep(250);
                        Macros.LeftClick();
                        Macros.LeftClick();
                        GameData.UpgradeMode = false;
                    }
                    else if (start != "!can")
                    {
                        await twitchBot.SendMessage(twitchChatMessage.Channel, "Please try another selection or !cancel.");
                    }
                }
            };
            await Task.Delay(-1);
        }

        private static Process GetProcess()
        {
            Process p = new Process();
            foreach (Process process in Process.GetProcesses())
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle) && process.MainWindowTitle.Contains(GameData.Name))
                {
                    p = process;
                }
            }
            return p;
        }
    }
}