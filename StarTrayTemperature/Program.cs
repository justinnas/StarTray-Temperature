using System;
using System.Threading;
using System.Windows.Forms;

namespace StarTrayTemperature
{
    internal static class Program
    {
        [STAThread]

        static void Main()
        {
            // Prevent the exception from being displayed and simply exit the application without any notification
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                new IconTray();
            }
            catch
            {
                Environment.Exit(0);
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
