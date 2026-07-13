using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace StarTrayTemperature
{
    public static class PawnIOManager
    {
        public static void CheckAndInstallPawnIO()
        {
            PromptAndInstallPawnIO(false);
        }

        public static void PromptAndInstallPawnIO(bool isUserInitiated)
        {
            if (IsPawnIoInstalled()) return;

            if (!isUserInitiated && Properties.Settings.Default.PawnIODeclined)
                return;

            DialogResult result = MessageBox.Show("PawnIO driver is not installed. It is required to read CPU temperatures.\n\nDo you want to install it now?\n\nPawnIO is a third-party open-source driver, learn more at: https://pawnio.eu/", "StarTray", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                InstallPawnIO();
                
                if (IsPawnIoInstalled())
                {
                    Properties.Settings.Default.PawnIODeclined = false;
                    Properties.Settings.Default.Save();

                    Application.Restart();
                    Environment.Exit(0);
                }
            }
            else if (!isUserInitiated)
            {
                Properties.Settings.Default.PawnIODeclined = true;
                Properties.Settings.Default.Save();
            }
        }

        public static bool IsPawnIoInstalled()
        {
            try
            {
                using (RegistryKey subKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PawnIO"))
                {
                    if (Version.TryParse(subKey?.GetValue("DisplayVersion") as string, out _))
                        return true;
                }

                using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (RegistryKey subKeyWow64 = registryKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PawnIO"))
                    {
                        if (Version.TryParse(subKeyWow64?.GetValue("DisplayVersion") as string, out _))
                            return true;
                    }
                }
            }
            catch { }

            return false;
        }

        private static void InstallPawnIO()
        {
            string path = ExtractPawnIO();
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Could not run PawnIO_setup.exe. Please try again or download and install it manually.", "StarTray", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var process = Process.Start(new ProcessStartInfo(path, "-install") { Verb = "runas" });
                process?.WaitForExit();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // The user dismissed the Windows admin (UAC) prompt
                MessageBox.Show("PawnIO installation was cancelled.", "StarTray", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                try { File.Delete(path); } catch { }
            }
        }

        private static string ExtractPawnIO()
        {
            string destination = Path.Combine(Application.StartupPath, "PawnIO_setup.exe");

            try
            {
                using (Stream resourceStream = typeof(PawnIOManager).Assembly.GetManifestResourceStream("StarTrayTemperature.Resources.PawnIO_setup.exe"))
                {
                    if (resourceStream == null) return null;

                    using (FileStream fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                }
                return destination;
            }
            catch
            {
                return null;
            }
        }
    }
}
