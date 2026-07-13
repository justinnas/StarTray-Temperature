using System;
using System.Windows.Forms;

namespace StarTrayTemperature
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
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
