﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace NodBot.Code
{
     public class InputService
    {
        // Mouse click values
        private const int WM_RBUTTON_DOWN = 0x204;
        private const int WM_RBUTTON_UP = 0x205;
        private const int WM_LBUTTON_DOWN = 0x201;
        private const int WM_LBUTTON_UP = 0x202;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;

        //
        private IntPtr game_hwnd;
        private Logger mLogger;
        //
        public IntPtr GAME { get { return game_hwnd; } }

        public InputService(String handleTitle, Logger aLogger)
        {
            game_hwnd = FindWindow(null, handleTitle);
            mLogger = aLogger;
        }

        public static IntPtr getNodiatisWindowHandle()
        {
            return FindWindow(null, Settings.WINDOW_NAME);
        }

        // Moves mouse
        public void moveMouse(int x, int y)
        {
            Rectangle rect = new Rectangle();
            GetWindowRect(game_hwnd, ref rect);
            SetCursorPos(rect.X + x, rect.Y + y);
        }

        /// <summary>
        /// This function sends a mouse click to the provided x,y coordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void sendLeftMouseClickWithWindowHandler(int x, int y, bool aLeftClick)
        {
            String coord = "(" + x + ", " + y + ")";
            mLogger.sendMessage("Sending left mouse click: " + coord, LogType.DEBUG);

            // Convert x,y coordinates to int pointer
            // IntPtr coords = new IntPtr((y << 16) | x); // lparam

            // Retrieve the rectangle of game handle window
            Rectangle rect = new Rectangle();
            GetWindowRect(game_hwnd, ref rect);

            // Retrieves current window and cursor state, so it can be restored
            IntPtr lCurrentWindow = GetForegroundWindow();
            POINT lCurrentCursor;
            GetCursorPos(out lCurrentCursor);

            // Bring the game handle window to the foreground for mouse click
            SetForegroundWindow(game_hwnd);

            // Set curosr to x,y coords + offset of the game handle window
            SetCursorPos(rect.X + x, rect.Y + y);

            Task.Delay(50).ContinueWith(_ =>
            {
                if (aLeftClick)
                {
                    SendMessage(game_hwnd, WM_LBUTTON_DOWN, IntPtr.Zero, IntPtr.Zero);
                    SendMessage(game_hwnd, WM_LBUTTON_UP, IntPtr.Zero, IntPtr.Zero);
                }
                else
                {
                    SendMessage(game_hwnd, WM_RBUTTON_DOWN, IntPtr.Zero, IntPtr.Zero);
                    SendMessage(game_hwnd, WM_RBUTTON_UP, IntPtr.Zero, IntPtr.Zero);
                }


                // Brings back the previous forground window, if not game handle window
                if (lCurrentWindow != game_hwnd)
                    SetForegroundWindow(lCurrentWindow);

                // Restore Cursor Position
                SetCursorPos(lCurrentCursor.X, lCurrentCursor.Y);
            });
        }

        public async Task dragTo(int x, int y, int x2, int y2, bool withShiftKey = true)
        {
            // Retrieve the rectangle of game handle window
            Rectangle rect = new Rectangle();
            GetWindowRect(game_hwnd, ref rect);

            // Retrieves current window and cursor state, so it can be restored
            IntPtr lCurrentWindow = GetForegroundWindow();
            POINT lCurrentCursor;
            GetCursorPos(out lCurrentCursor);

            // Bring the game handle window to the foreground for mouse click
            SetForegroundWindow(game_hwnd);

            // Set curosr to x,y coords + offset of the game handle window
            keybd_event(0x10, 0, 0, 0); // Shift Down

            SetCursorPos(rect.X + x, rect.Y + y);
            Task.Delay(250).Wait();
            SendMessage(game_hwnd, WM_LBUTTON_DOWN, IntPtr.Zero, IntPtr.Zero);
            SendMessage(game_hwnd, WM_LBUTTON_UP, IntPtr.Zero, IntPtr.Zero);
            Task.Delay(500).Wait();

            SetCursorPos(rect.X + x2, rect.Y + y2);
            Task.Delay(250).Wait();
            SendMessage(game_hwnd, WM_LBUTTON_DOWN, IntPtr.Zero, IntPtr.Zero);
            SendMessage(game_hwnd, WM_LBUTTON_UP, IntPtr.Zero, IntPtr.Zero);
            Task.Delay(250).Wait();
            
            keybd_event(0x10, 0, 0x02, 0); // Shift Up
        }

        /// <summary>
        /// This function sends a provided keyboard action the game window handle
        /// </summary>
        /// <param name="action"></param>
        public void sendKeyboardClick(Keyboard_Actions action)
        {
            String lInput = Enum.GetName(typeof(Keyboard_Actions_Keys), action);
            mLogger.sendMessage("Sending keyboard input: [" + lInput + "]", LogType.DEBUG);

            SendMessage(game_hwnd, WM_KEYDOWN, new IntPtr((uint)action), IntPtr.Zero);
            SendMessage(game_hwnd, WM_KEYUP, new IntPtr((uint)action), IntPtr.Zero);
        }

        /// <summary>
        /// This enumerator holds the user friendly values that are used to decide what input to use.
        /// </summary>
        public enum Keyboard_Actions
        {
            GEM_SLOT_1 = 0X31,   // 1
            GEM_SLOT_2 = 0X32,   // 2
            GEM_SLOT_3 = 0X33,   // 3
            GEM_SLOT_4 = 0X34,   // 4
            GEM_SLOT_5 = 0X35,   // 5
            GEM_SLOT_6 = 0X36,   // 6
            AUTO_ATTACK = 0x41,  // A
            AUTO_SHOOT = 0x53,   // S
            LOOT = 0x4C,         // L
            CA_PRIMARY = 0x44,   // D
            CA_SECONDARY = 0x46, // F
            START_FIGHT = 0x46,  // F
            EXIT = 0x45,         // E
            ESC = 0x1B,          // Esc
            MOVE_UP = 0X57,      // Arrow Up
            MOVE_DOWN = 0X53,    // Arrow Down
            MOVE_LEFT = 0X41,    // Arrow Left
            MOVE_RIGHT = 0X44,   // Arrow Right
        }

        private enum Keyboard_Actions_Keys
        {
            A = 0x41,
            S = 0x53,
            L = 0x4C,
            D = 0x44,
            F = 0x46,
            E = 0x45,
            ESC = 0x1B,
            W = 0X57,
        }

        /// <summary>
        /// This function is used to retrieve the handle window using the provided class name and window name
        /// </summary>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// This function sends a message to the to the provided handle window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// This function retrieves the window rectangle of the provided handle window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpRect"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern long GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        /// <summary>
        /// This function sets the windows curosr position to the provided x and y coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        /// <summary>
        /// This function brings the provided window handle to the foreground.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// This function returns the window handle of the current forground window.
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// This function retrieves the current cursor position.
        /// </summary>
        /// <param name="lpPoint"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern bool keybd_event(int bVk, int bScan, int dwFlags, int dwExtraInfo);


        /// <summary>
        /// This struct is used to hold the cursor x,y position returned by the above function 'GetCursorPos'
        /// </summary>
        public struct POINT
        {
            public int X;
            public int Y;
        }

    }
}