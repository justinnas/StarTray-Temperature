using System;
using System.IO;
using System.Windows.Forms;

namespace StarTrayTemperature
{
    public static class ErrorHandler
    {
        private const string Title = "StarTray";

        private static string LogPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "StarTray",
            "error.log");

        // Something the app cannot recover from: tell the user, then close
        public static void HandleFatal(Exception ex)
        {
            Log(ex);
            MessageBox.Show(GetUserMessage(ex), Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }

        // Something went wrong but the app can keep running (e.g. a menu action)
        public static void HandleNonFatal(Exception ex)
        {
            Log(ex);
            MessageBox.Show(GetUserMessage(ex), Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowMessage(string message)
        {
            MessageBox.Show(message, Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string GetUserMessage(Exception ex)
        {
            if (ex is StarTrayException)
            {
                return ex.Message;
            }

            if (ex is UnauthorizedAccessException)
            {
                return "StarTray doesn't have the permissions it needs for this action.\n\n" +
                       "Try running the application as administrator.";
            }

            if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                return "Some StarTray files are missing or damaged.\n\n" +
                       "Reinstalling the application should fix this.";
            }

            return "StarTray ran into an unexpected problem.\n\n" +
                   "Please try restarting the application.";
        }

        private static void Log(Exception ex)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath));
                File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}\n\n");
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}
