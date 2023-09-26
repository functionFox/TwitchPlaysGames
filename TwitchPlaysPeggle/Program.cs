using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using static TwitchPlaysPeggle.config;
using static TwitchPlaysPeggle.SendToWin;
using ws = WindowScrape;

namespace TwitchPlaysPeggle
{
    class Program
    {
        static async Task Main(string[] args)
        {
            config configParam = new config();
            Process window = new Process();
            IntPtr iHandle = new();
            MousePoint mousePos = new MousePoint();
            Point xy = new Point();
            GameData.Name = configParam.defaultGameName;
            Process p = GetProcess();

            string title = ws.Static.HwndInterface.GetHwndTitle(iHandle);
            IntPtr Hwnd = p.MainWindowHandle;
            
            
            string password = configParam.oauth;
            string botUsername = configParam.nick;
            string channel = configParam.channel;
            GameData.X = 1203;
            GameData.Y = 941;
            
            mousePos = SendToWin.GetCursorPosition();
            xy = ws.Static.HwndInterface.GetHwndPos(Hwnd);

            var twitchBot = new ChatBot(botUsername, password);
            twitchBot.Start().SafeFireAndForget();
            //We could .SafeFireAndForget() these two calls if we want to
            await twitchBot.JoinChannel(channel);
            await twitchBot.SendMessage(channel, "Beep Boop!");

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
                    
                    string newMsg = "X: " + SendToWin.GetCursorPosition().X.ToString() + "Y: " + SendToWin.GetCursorPosition().Y.ToString();
                    await twitchBot.SendMessage(twitchChatMessage.Channel, $"I'm seeing {newMsg}!");
                    Macros.LeftClick();

                    // SendToWin.SendMessage((int)iHandle, SendToWin.WM_LBUTTONUP, 0, SendToWin.CreateLParam(x,y));
                }
                if (twitchChatMessage.Message.ToLower().StartsWith("!setxy"))
                {
                    string newMsg = "X: 823, Y: 495";
                    await twitchBot.SendMessage(twitchChatMessage.Channel, $"I'm setting {newMsg}");
                    SendToWin.SetCursorPosition(823, 495);
                    Macros.LeftClick();
                }
                if (twitchChatMessage.Message.ToLower().StartsWith("!getxy"))
                {
                    mousePos = SendToWin.GetCursorPosition();
                    xy = ws.Static.HwndInterface.GetHwndPos(Hwnd);
                    string newMsg = $"X: {mousePos.X}, Y: {mousePos.Y}, wX: {xy.X}, xY: {xy.Y}";
                    await twitchBot.SendMessage(twitchChatMessage.Channel, $"{GameData.Name} is at {newMsg}!");
                }
                if (twitchChatMessage.Message.ToLower().StartsWith("!start"))
                {
                    xy = ws.Static.HwndInterface.GetHwndPos(Hwnd);
                    await twitchBot.SendMessage(twitchChatMessage.Channel, $"Let's start a new adventure!");
                    SendToWin.SetCursorPosition(xy.X + 666, xy.Y + 125);
                    Macros.LeftClick();
                    p.ki
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