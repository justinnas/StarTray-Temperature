using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace StarTrayTemperature
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            string currentProcessName = Process.GetCurrentProcess().ProcessName;

            var runningProcesses = Process.GetProcessesByName(currentProcessName);

            if (runningProcesses.Length > 1)
            {
                MessageBox.Show("StarTray is already running!\n\nDon't see the icon?\nTry clicking the upward arrow in the system tray to see if it's hidden.", "StarTray", MessageBoxButtons.OK);
                Environment.Exit(0);
                return;
            }

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ErrorHandler.HandleNonFatal(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                ErrorHandler.HandleFatal(e.ExceptionObject as Exception);

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                new IconTray();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleFatal(ex);
            }
        }
    }
}
