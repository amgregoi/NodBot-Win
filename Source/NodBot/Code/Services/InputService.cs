using NodBot.Code.Model;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NodBot.Code
{
    public class InputService
    {
        public class SendMessageConst{
            // Mouse click values
            public const int WM_RBUTTON_DOWN = 0x204;
            public const int WM_RBUTTON_UP = 0x205;
            public const int WM_LBUTTON_DOWN = 0x201;
            public const int WM_LBUTTON_UP = 0x202;
            public const int WM_KEYDOWN = 0x100;
            public const int WM_KEYUP = 0x101;
        }

        public class MouseEventConst
        {
            public const int WM_RBUTTON_DOWN = 0x0008;
            public const int WM_RBUTTON_UP = 0x0010;
            public const int WM_LBUTTON_DOWN = 0x0002;
            public const int WM_LBUTTON_UP = 0x0004;
        }

        public class KeyBdEventConst
        {
            public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
            public const int KEYEVENTF_KEYUP = 0x0002;
        }

        public class WindowInfo
        {
            public IntPtr window;
            public POINT cursorPos;

            public WindowInfo(IntPtr window, POINT cursor)
            {
                this.window = window;
                this.cursorPos = cursor;
            }
        }

        //
        private IntPtr game_hwnd;
        private Logger mLogger;
        private Semaphore _mutex;

        //
        public IntPtr GAME { get { return game_hwnd; } }

        public InputService(String handleTitle, Logger aLogger)
        {
            game_hwnd = FindWindow(null, handleTitle);
            mLogger = aLogger;
            initMutex();
        }


        private void initMutex()
        {

            try
            {
                _mutex = Semaphore.OpenExisting("nodbot_win_2020");
            }
            catch
            {
                //the specified mutex doesn't exist, we should create it
                _mutex = new Semaphore(1,25, "nodbot_win_2020"); //these names need to match.
            }
        }

        public void Dispose()
        {
            _mutex.Dispose();
            _mutex.Close();
        }

        public void SemaUp()
        {
            _mutex.WaitOne();
        }

        public void SemaDown()
        {
            _mutex.Release();
        }

        public IntPtr getGameWindow()
        {
            return game_hwnd;
        }

        public IntPtr FindWindow(String windowName)
        {
            return FindWindow(null, windowName);
        }

        public void ResetWindowPosition()
        {
            MoveWindow(game_hwnd, 50, 50, 1450, 1050, true);
        }

        /// <summary>
        ///  Use Sparingly
        /// </summary>
        /// <returns></returns>
        public static IntPtr getNodiatisWindowHandle()
        {
            return FindWindow(null, Settings.WINDOW_NAME);
        }

        // Moves mouse
        public UIPoint moveMouse(int x, int y)
        {
            Rectangle rect = new Rectangle();
            GetWindowRect(game_hwnd, ref rect);
            SetCursorPos(rect.X + x, rect.Y + y);
            return new UIPoint(new System.Drawing.Point(x, y), rect);
        }

        public void leftClick(UIPoint point)
        {

            _mutex.WaitOne();
            doLeftClick(point);
            _mutex.Release();
        }

        public void doubleLeftClick(UIPoint point)
        {

            _mutex.WaitOne();
            doDoubleLeftClick(point);
            _mutex.Release();
        }

        public void doLeftClick(UIPoint point)
        {
            if (point != null)
            {
                moveMouse(point.X, point.Y);
                Task.Delay(100).Wait();
            }

            mouse_event(MouseEventConst.WM_LBUTTON_DOWN, point.X, point.Y, 0, 0);
            // SendMessage(game_hwnd, WM_LBUTTON_DOWN, IntPtr.Zero, IntPtr.Zero);
            Task.Delay(100).Wait();
            //SendMessage(game_hwnd, WM_LBUTTON_UP, IntPtr.Zero, IntPtr.Zero);
            mouse_event(MouseEventConst.WM_LBUTTON_UP, point.X, point.Y, 0, 0);
        }

        public void doDoubleLeftClick(UIPoint point)
        {
            if (point != null)
            {
                moveMouse(point.X, point.Y);
                Task.Delay(50).Wait();
            }

            mouse_event(MouseEventConst.WM_LBUTTON_DOWN, point.X, point.Y, 0, 0);
            Task.Delay(50).Wait();
            mouse_event(MouseEventConst.WM_LBUTTON_DOWN, point.X, point.Y, 0, 0);
            // SendMessage(game_hwnd, WM_LBUTTON_DOWN, IntPtr.Zero, IntPtr.Zero);
            Task.Delay(50).Wait();
            //SendMessage(game_hwnd, WM_LBUTTON_UP, IntPtr.Zero, IntPtr.Zero);
            mouse_event(MouseEventConst.WM_LBUTTON_UP, point.X, point.Y, 0, 0);
            Task.Delay(50).Wait();
            mouse_event(MouseEventConst.WM_LBUTTON_UP, point.X, point.Y, 0, 0);
        }





        public void rightClick(UIPoint point)
        {
            _mutex.WaitOne();
            doRightClick(point);
            _mutex.Release();
        }

        private void doRightClick(UIPoint point)
        {
            if (point != null)
            {
                moveMouse(point.X, point.Y);
                Task.Delay(100).Wait();
            }

            mouse_event(MouseEventConst.WM_RBUTTON_DOWN, point.X, point.Y, 0, 0);
            //SendMessage(game_hwnd, WM_RBUTTON_DOWN, IntPtr.Zero, IntPtr.Zero);
            Task.Delay(100).Wait();
            mouse_event(MouseEventConst.WM_RBUTTON_UP, point.X, point.Y, 0, 0);
            //SendMessage(game_hwnd, WM_RBUTTON_UP, IntPtr.Zero, IntPtr.Zero);
        }
        
        /***
         * Sets current foreground window to Game, returns reference to previous foreground window to return to after input
         */
        private WindowInfo setForgroundGame()
        {
            // Retrieves current window and cursor state, so it can be restored
            IntPtr lCurrentWindow = GetForegroundWindow();
            POINT lCurrentCursor;
            GetCursorPos(out lCurrentCursor);
            // Bring the game handle window to the foreground for mouse click
            SetForegroundWindow(game_hwnd);

            return new WindowInfo(lCurrentWindow, lCurrentCursor);
        } 

        private void setForegroundWindow(WindowInfo wInfo)
        {
            SetForegroundWindow(wInfo.window);
            SetCursorPos(wInfo.cursorPos.X, wInfo.cursorPos.Y);
        }

        /// <summary>
        /// This function sends a mouse click to the provided x,y coordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void sendLeftMouseClickWithWindowHandler(int x, int y, bool aLeftClick, bool doubleClick)
        {
            _mutex.WaitOne();
            String coord = "(" + x + ", " + y + ")";
            mLogger.debug("Sending left mouse click: " + coord);

            // Convert x,y coordinates to int pointer
            // IntPtr coords = new IntPtr((y << 16) | x); // lparam

            // Retrieve the rectangle of game handle window
            Rectangle rect = new Rectangle();
            GetWindowRect(game_hwnd, ref rect);

            /*
            // Retrieves current window and cursor state, so it can be restored
            IntPtr lCurrentWindow = GetForegroundWindow();
            POINT lCurrentCursor;
            GetCursorPos(out lCurrentCursor);

            // Bring the game handle window to the foreground for mouse click
            SetForegroundWindow(game_hwnd);
            */

            WindowInfo window = setForgroundGame();

            // Set curosr to x,y coords + offset of the game handle window
            SetCursorPos(rect.X + x, rect.Y + y);
            UIPoint clickPoint = new UIPoint(new System.Drawing.Point(x, y), rect);
            Task.Delay(50).Wait();

            if (aLeftClick)
            {
                if (doubleClick) doDoubleLeftClick(clickPoint);
                else doLeftClick(clickPoint);
            }
            else doRightClick(clickPoint);

            /*
            // Brings back the previous forground window, if not game handle window
            if (lCurrentWindow != game_hwnd)
                SetForegroundWindow(lCurrentWindow);

            // Restore Cursor Position
            SetCursorPos(lCurrentCursor.X, lCurrentCursor.Y);
            */

            setForegroundWindow(window);

            _mutex.Release();
        }

        public void sendLeftMouseClickWithWindowHandlerNoMutex(int x, int y, bool aLeftClick)
        {
            String coord = "(" + x + ", " + y + ")";
            mLogger.debug("Sending left mouse click: " + coord);

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
            UIPoint clickPoint = new UIPoint(new System.Drawing.Point(x, y), rect);
            
            Task.Delay(50).Wait();

            if (aLeftClick) doLeftClick(clickPoint);
            else doRightClick(clickPoint);

            // Brings back the previous forground window, if not game handle window
            if (lCurrentWindow != game_hwnd)
                SetForegroundWindow(lCurrentWindow);

            // Restore Cursor Position
            SetCursorPos(lCurrentCursor.X, lCurrentCursor.Y);
        }

        public Task<bool> dragTo(int x, int y, int x2, int y2, bool withShiftKey = true)
        {
            _mutex.WaitOne();

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

            var test1  = new UIPoint(new System.Drawing.Point(x,y), rect);
            SetCursorPos(rect.X + x, rect.Y + y);
            doLeftClick(test1);


            Task.Delay(100).Wait();

            var test2 = new UIPoint(new System.Drawing.Point(x2, y2), rect);
            SetCursorPos(rect.X + x2, rect.Y + y2);
            doLeftClick(test2);


            keybd_event(0x10, 0, 0x02, 0); // Shift Up

            _mutex.Release();

            return Task.FromResult<bool>(true);
        }

        /// <summary>
        /// This function sends a provided keyboard action the game window handle
        /// </summary>
        /// <param name="action"></param>
        public void sendKeyboardClick(Keyboard_Actions action)
        {
            _mutex.WaitOne();
            String lInput = Enum.GetName(typeof(Keyboard_Actions_Keys), action);
            mLogger.debug("Sending keyboard input: [" + lInput + "]");

            //SendMessage(game_hwnd, WM_KEYDOWN, new IntPtr((uint)action), IntPtr.Zero);
            //SendMessage(game_hwnd, WM_KEYUP, new IntPtr((uint)action), IntPtr.Zero);

            keybd_event((int)action, 0x45, KeyBdEventConst.KEYEVENTF_EXTENDEDKEY | 0, 0);
            keybd_event((int)action, 0x45, KeyBdEventConst.KEYEVENTF_EXTENDEDKEY | KeyBdEventConst.KEYEVENTF_KEYUP, 0);


            _mutex.Release();
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
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);


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

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);


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
