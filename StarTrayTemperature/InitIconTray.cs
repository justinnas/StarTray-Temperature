using Microsoft.Win32;
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
using LibreHardwareMonitor.Hardware;
using System.Diagnostics;

namespace StarTrayTemperature
{
    public partial class IconTray : Form
    {
        internal string AppLabel = "StarTray";
        internal string VersionLabel = "v1.2";
        internal string CopyrightLabel = "© justinnas";
        internal string WebpageURL = "https://github.com/justinnas/StarTray-Temperature";


        private string resourcesFolder = Path.Combine(Application.StartupPath, "Resources");

        // --==+

        internal Computer computer;

        // -+

        internal bool useFahrenheit = false;
        internal bool showCPU = true;
        internal bool showGPU = true;

        // +=-

        private TaskService taskService = new TaskService();
        private const string TaskName = "StarTray_RunOnStartup";

        // --==+

        internal int iconWidth = 32;
        internal int iconHeight = 32;
        internal FontFamily customFontFamily = FontFamily.GenericSansSerif;


        public IconTray()
        {
            InitializeComponent();
            LoadGlobalSettings();

            PawnIOManager.CheckAndInstallPawnIO();

            computer = new Computer {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
            };

            computer.Open();

            // Load Themes
            ThemeManager.LoadThemes();

            // Initialize Fonts
            PrivateFontCollection fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(Path.Combine(resourcesFolder, "font.ttf"));
            customFontFamily = fontCollection.Families[0];

            // Initialize the icons
            if (showCPU)
            {
                StartSensor("CPU");
            }

            if (showGPU)
            {
                StartSensor("GPU");
            }

            // Start CPU icon if both of the icons are somehow turned off
            else if (!showCPU && !showGPU)
            {
                StartSensor("CPU");
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
    }
}
