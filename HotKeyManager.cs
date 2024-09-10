using System.Runtime.InteropServices;
using System.Windows.Forms;
using System;

namespace ScreenshotApp
{
    public static partial class HotKeyManager
    {
        [LibraryImport("user32.dll")]
        private static partial int RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;
        private static readonly int hotKeyId = 0x1234;

        public static void RegisterHotKey(Keys key, KeyModifiers modifiers)
        {
            _ = RegisterHotKey(IntPtr.Zero, hotKeyId, (uint)modifiers, (uint)key);
            Application.AddMessageFilter(new HotKeyMessageFilter());
        }

        public static void UnregisterHotKey()
        {
            UnregisterHotKey(IntPtr.Zero, hotKeyId);
        }

        private class HotKeyMessageFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_HOTKEY && (int)m.WParam == hotKeyId)
                {
                    CaptureScreen();
                    return true;
                }
                return false;
            }

            private static void CaptureScreen()
            {
                var captureForm = CaptureForm.GetInstance();
                if (!captureForm.Visible)
                {
                    captureForm.ShowDialog();
                }
            }
        }
    }


    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }
}