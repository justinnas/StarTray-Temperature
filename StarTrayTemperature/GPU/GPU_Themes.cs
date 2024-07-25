using Microsoft.Win32;
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
        private void LightMode_GPU_Click(object sender, EventArgs e)
        {
            ApplyGPUTheme("light");
        }

        private void DarkMode_GPU_Click(object sender, EventArgs e)
        {
            ApplyGPUTheme("dark");
        }

        private void Blue11Mode_GPU_Click(object sender, EventArgs e)
        {
            ApplyGPUTheme("blue11");
        }

        private void GreenMode_GPU_Click(object sender, EventArgs e)
        {
            ApplyGPUTheme("green");
        }

        private void RedMode_GPU_Click(object sender, EventArgs e)
        {
            ApplyGPUTheme("red");
        }

        private void BlueMode_GPU_Click(object sender, EventArgs e)
        {
            ApplyGPUTheme("blue");
        }

        private void ApplyGPUTheme(string theme)
        {
            if (GPU_colorMode != theme)
            {
                GPU_colorMode = theme;

                switch (theme)
                {
                    case "light":
                        GPU_Color = Color.FromArgb(255, 255, 255);
                        GPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "gpuicon.ico");
                        break;
                    case "dark":
                        GPU_Color = Color.FromArgb(0, 0, 0);
                        GPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "gpuicon_dark.ico");
                        break;
                    case "blue11":
                        GPU_Color = Color.FromArgb(151, 234, 255);
                        GPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "gpuicon_blue11.ico");
                        break;
                    case "green":
                        GPU_Color = Color.FromArgb(189, 255, 71);
                        GPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "gpuicon_green.ico");
                        break;
                    case "red":
                        GPU_Color = Color.FromArgb(255, 161, 150);
                        GPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "gpuicon_red.ico");
                        break;
                    case "blue":
                        GPU_Color = Color.FromArgb(130, 228, 255);
                        GPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "gpuicon_blue.ico");
                        break;
                }

                notifyIcon_GPU.Icon?.Dispose();
                GPU_Icon = Image.FromFile(GPU_Icon_Path);
                notifyIcon_GPU.Icon = CreateGPUIcon(currentTemp_GPU);
                SaveSettings_GPU();
            }
        }

        private void SaveSettings_GPU()
        {
            Properties.Settings.Default.ColorMode_GPU = GPU_colorMode;
            Properties.Settings.Default.TextColor_GPU = GPU_Color;
            Properties.Settings.Default.IconPath_GPU = GPU_Icon_Path;
            Properties.Settings.Default.Save();
        }

        private void LoadSettings_GPU()
        {
            GPU_colorMode = Properties.Settings.Default.ColorMode_GPU;
            GPU_Color = Properties.Settings.Default.TextColor_GPU;
            GPU_Icon_Path = Properties.Settings.Default.IconPath_GPU;

            // First launch
            if (GPU_Icon_Path == string.Empty)
            {
                if (IsWindowsThemeLight())
                {
                    GPU_colorMode = "dark";
                    GPU_Color = Color.FromArgb(0, 0, 0);
                    GPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "gpuicon_dark.ico");
                }
                else
                {
                    GPU_colorMode = "light";
                    GPU_Color = Color.FromArgb(255, 255, 255);
                    GPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "gpuicon.ico");
                }

                SaveSettings_GPU();
            }
        }
    }
}
