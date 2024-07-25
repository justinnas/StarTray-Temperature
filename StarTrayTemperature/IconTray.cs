using Microsoft.Win32;
using LibreHardwareMonitor.Hardware;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32.TaskScheduler;
using System.Collections.Generic;

namespace StarTrayTemperature
{
    public partial class IconTray : Form
    {
        private string AppLabel = "StarTray";
        private string VersionLabel = "v1.1";
        private string CopyrightLabel = "© justinnas";


        private string resourcesFolder = Path.Combine(Application.StartupPath, "Resources");

        // --==+

        private Computer computer;

        // -+

        private bool useFahrenheit = false;

        // +=-

        private TaskService taskService = new TaskService();
        private const string TaskName = "StarTray_RunOnStartup";

        // --==+

        private int iconWidth = 32;
        private int iconHeight = 32;
        private FontFamily customFontFamily = FontFamily.GenericSansSerif;


        public IconTray()
        {
            InitializeComponent();
            LoadGlobalSettings();

            computer = new Computer {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
            };

            computer.Open();

            // Initialize Fonts
            PrivateFontCollection fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(Path.Combine(resourcesFolder, "font.ttf"));
            customFontFamily = fontCollection.Families[0];

            // Initialize the icons
            if (showCPU)
            {
                StartCPU();
            }

            if (showGPU)
            {
                StartGPU();
            }

            // Start CPU icon if both of the icons are somehow turned off
            else if (!showCPU && !showGPU)
            {
                StartCPU();
            }

            Application.Run();
        }

        private bool IsWindowsThemeLight()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object registryValueObject = key.GetValue("SystemUsesLightTheme");
                        if (registryValueObject != null)
                        {
                            int registryValue = (int)registryValueObject;
                            return registryValue == 1;
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        private static class NativeMethods // Used for clearing up GDI's and User's icon handles
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern bool DestroyIcon(IntPtr handle);
        }
    }
}
