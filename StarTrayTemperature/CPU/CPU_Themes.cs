using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarTrayTemperature
{
    public partial class IconTray : Form
    {
        private void LightMode_CPU_Click(object sender, EventArgs e)
        {
            ApplyCPUTheme("light");
        }

        private void DarkMode_CPU_Click(object sender, EventArgs e)
        {
            ApplyCPUTheme("dark");
        }

        private void Blue11Mode_CPU_Click(object sender, EventArgs e)
        {
            ApplyCPUTheme("blue11");
        }

        private void GreenMode_CPU_Click(object sender, EventArgs e)
        {
            ApplyCPUTheme("green");
        }

        private void RedMode_CPU_Click(object sender, EventArgs e)
        {
            ApplyCPUTheme("red");
        }

        private void BlueMode_CPU_Click(object sender, EventArgs e)
        {
            ApplyCPUTheme("blue");
        }

        private void ApplyCPUTheme(string theme)
        {
            if (CPU_colorMode != theme)
            {
                CPU_colorMode = theme;

                switch (theme)
                {
                    case "light":
                        CPU_Color = Color.FromArgb(255, 255, 255);
                        CPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "cpuicon.ico");
                        break;
                    case "dark":
                        CPU_Color = Color.FromArgb(0, 0, 0);
                        CPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "cpuicon_dark.ico");
                        break;
                    case "blue11":
                        CPU_Color = Color.FromArgb(151, 234, 255);
                        CPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "cpuicon_blue11.ico");
                        break;
                    case "green":
                        CPU_Color = Color.FromArgb(189, 255, 71);
                        CPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "cpuicon_green.ico");
                        break;
                    case "red":
                        CPU_Color = Color.FromArgb(255, 161, 150);
                        CPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "cpuicon_red.ico");
                        break;
                    case "blue":
                        CPU_Color = Color.FromArgb(130, 228, 255);
                        CPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "cpuicon_blue.ico");
                        break;
                }

                notifyIcon_CPU.Icon?.Dispose();
                CPU_Icon = Image.FromFile(CPU_Icon_Path);
                notifyIcon_CPU.Icon = CreateCPUIcon(currentTemp_CPU);
                SaveSettings_CPU();
            }
        }

        private void SaveSettings_CPU()
        {
            Properties.Settings.Default.ColorMode_CPU = CPU_colorMode;
            Properties.Settings.Default.TextColor_CPU = CPU_Color;
            Properties.Settings.Default.IconPath_CPU = CPU_Icon_Path;
            Properties.Settings.Default.Save();
        }

        private void LoadSettings_CPU()
        {
            CPU_Icon_Path = Properties.Settings.Default.IconPath_CPU;
            CPU_colorMode = Properties.Settings.Default.ColorMode_CPU;
            CPU_Color = Properties.Settings.Default.TextColor_CPU;

            // First launch
            if (CPU_Icon_Path == string.Empty)
            {
                if (IsWindowsThemeLight())
                {
                    CPU_colorMode = "dark";
                    CPU_Color = Color.FromArgb(0, 0, 0);
                    CPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "cpuicon_dark.ico");
                }
                else
                {
                    CPU_colorMode = "light";
                    CPU_Color = Color.FromArgb(255, 255, 255);
                    CPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "cpuicon.ico");
                }

                SaveSettings_CPU();
            }
        }
    }
}
