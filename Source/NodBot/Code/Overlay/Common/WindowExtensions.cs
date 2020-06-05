using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace NodBot.Code.Overlay
{
    public static class WindowExtensions
    {
        private const int GwlExstyle = -20;
        private const int WsExTransparent = 0x00000020;

        public static void MakeWindowTransparent(this Window wnd)
        {
            if (!wnd.IsInitialized)
                throw new Exception("The extension method MakeWindowTransparent can not be called prior to the window being initialized.");
            var hwnd = new WindowInteropHelper(wnd).Handle;
            var extendedStyle = Native.GetWindowLongPtr(hwnd, GwlExstyle);
            Native.SetWindowLongPtr(hwnd, GwlExstyle, new IntPtr(extendedStyle.ToInt32() | WsExTransparent));
        }
    }
}
