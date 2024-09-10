using System;
using System.Drawing;
using System.Windows.Forms;

namespace ScreenshotApp
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            HotKeyManager.RegisterHotKey(Keys.PrintScreen, KeyModifiers.Control);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            NotifyIcon trayIcon = new()
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "ScreenshotApp"
            };

            ContextMenuStrip contextMenu = new();
            contextMenu.Items.Add("Exit", null, (sender, e) => Application.Exit());
            trayIcon.ContextMenuStrip = contextMenu;
            Application.Run();
        }

    }
}
