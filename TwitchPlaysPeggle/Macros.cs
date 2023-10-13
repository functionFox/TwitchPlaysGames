using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ws = WindowScrape;
using System.Drawing;
using InputSimulatorEx;
using Emgu.CV.Structure;
using Emgu.CV;
using InputSimulatorEx.Native;
using static TwitchPlaysGames.ChatBot;
using System.Drawing.Imaging;

namespace TwitchPlaysGames
{
    public static class Macros
    {
        public static void LeftClick()
        {
            SendToWin.MouseEvent(SendToWin.MouseEventFlags.LeftDown);
            Thread.Sleep(250);
            SendToWin.MouseEvent(SendToWin.MouseEventFlags.LeftUp);
        }

        public static void RightClick()
        {
            SendToWin.MouseEvent(SendToWin.MouseEventFlags.RightDown);
            Thread.Sleep(250);
            SendToWin.MouseEvent(SendToWin.MouseEventFlags.RightUp);
        }

        public static void SetMousePos(int x, int y)
        {
            SendToWin.SetCursorPosition(x, y);
        }

        public static void PressSpace()
        {
            InputSimulator sim = new();
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
        }

        public static void PressEscape()
        {
            InputSimulator sim = new();
            sim.Keyboard.KeyPress(VirtualKeyCode.ESCAPE);
        }

        public static void PressR()
        {
            InputSimulator sim = new();
            sim.Keyboard.KeyPress(VirtualKeyCode.VK_R);
        }

        public static void TakeScreenshot(string tempPath, IntPtr Hwnd)
        {
            ws.Static.HwndInterface.ActivateWindow(Hwnd);
            Image image = ImgProcess.CaptureWindow(Hwnd);
            image.Save(@$"{tempPath}\shoop.png");
        }
        public static Point Snap(IntPtr Hwnd, string tempPath, Image<Bgr, byte> imgNovaA)
        {
            ImgProcess.CaptureWindowToFile(Hwnd, @$"{tempPath}\upgrade.png", ImageFormat.Png);
            Image<Bgr, byte> image = new Image<Bgr, byte>($@"{tempPath}\upgrade.png");
            Point matchLocation = ImgProcess.compareImg(image, imgNovaA);
            return matchLocation;
        }
    }
}
