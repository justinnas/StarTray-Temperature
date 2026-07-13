using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace StarTrayTemperature
{
    public partial class IconTray : Form
    {
        public Dictionary<string, HardwareSensor> ActiveSensors = new Dictionary<string, HardwareSensor>();

        private HardwareSensor GetState(string type)
        {
            if (!ActiveSensors.ContainsKey(type))
            {
                if (type == "CPU") ActiveSensors[type] = new CpuSensor();
                else if (type == "GPU") ActiveSensors[type] = new GpuSensor();
                else throw new Exception("Unknown sensor type: " + type);
            }
            return ActiveSensors[type];
        }

        private void StartSensor(string type)
        {
            var state = GetState(type);
            LoadSettings_Sensor(type);

            state.FindSensor(computer);

            if (state.HardwareID == -1 || state.SensorID == -1)
            {
                state.HandleMissingSensor(this);
                if (!ActiveSensors.ContainsKey(type)) return; // If HandleMissingSensor didn't throw, it might mean we just gracefully cancel start
                if (state.HardwareID == -1 || state.SensorID == -1) return; // double check it didn't find something or is meant to abort
            }

            state.BaseIcon = Image.FromFile(state.IconPath);

            state.InitializeContextMenu(this);
            state.NotifyIcon = new NotifyIcon();
            state.NotifyIcon.ContextMenu = state.ContextMenu;
            state.NotifyIcon.Text = $"{type} Temperature: {state.CurrentTemp}°C";
            state.NotifyIcon.Icon = state.CreateIcon(state.CurrentTemp, this);
            state.NotifyIcon.Visible = true;
            state.NotifyIcon.DoubleClick += (s, e) =>
            {
                try { System.Diagnostics.Process.Start("taskmgr"); } catch { }
            };

            state.Timer = new Timer();
            state.Timer.Interval = 1000;
            state.Timer.Tick += (s, e) => Timer_Tick(type);
            state.Timer.Start();

            GC.Collect();
        }
        private void StopSensor(string type)
        {
            if (!ActiveSensors.ContainsKey(type)) return;
            var state = ActiveSensors[type];
            state.BaseIcon?.Dispose();
            state.Timer?.Stop();
            state.Timer?.Dispose();

            if (state.NotifyIcon?.Icon != null)
            {
                DynamicIconRenderer.DisposeIcon(state.NotifyIcon.Icon);
            }

            state.NotifyIcon?.ContextMenu?.Dispose();
            state.NotifyIcon?.Dispose();

            state.StartupMenuItem = null;
            state.ShowCPUMenuItem = null;
            state.ShowGPUMenuItem = null;
            state.ChangeScaleMenuItem = null;
            state.ContextMenu = null;

            ActiveSensors.Remove(type);
            GC.Collect();
        }

        internal void RestartSensor(string type)
        {
            StopSensor(type);
            StartSensor(type);
        }

        private void Timer_Tick(string type)
        {
            if (!ActiveSensors.ContainsKey(type)) return;
            try
            {
                var state = ActiveSensors[type];
                var hardware = computer.Hardware[state.HardwareID];
                hardware.Update();
                int newTemp = Convert.ToInt32(hardware.Sensors[state.SensorID].Value);

                if (state.ShouldIgnoreTemp(newTemp)) return;

                bool tempChanged = newTemp != state.CurrentTemp;
                state.CurrentTemp = newTemp;

                string tooltipText = state.GetTooltipText(hardware, useFahrenheit);

                tooltipText = tooltipText.TrimEnd('\n');
                if (string.IsNullOrEmpty(tooltipText))
                {
                    tooltipText = $"StarTray ({type})";
                }

                if (tooltipText.Length > 63) tooltipText = tooltipText.Substring(0, 63);

                if (state.NotifyIcon.Text != tooltipText)
                {
                    state.NotifyIcon.Text = tooltipText;
                }

                if (tempChanged || state.NotifyIcon.Icon == null)
                {
                    RefreshIcon(state);
                }
            }
            catch { }
        }

        internal void ApplyTheme(string type, string theme)
        {
            if (!ActiveSensors.ContainsKey(type)) return;
            var state = ActiveSensors[type];

            SetColorsFromTheme(state, theme);
            RefreshIcon(state);
            SaveSettings_Sensor(type);
        }

        internal void RefreshIcon(HardwareSensor state)
        {
            if (state.NotifyIcon == null) return;

            int displayTemp = state.CurrentTemp;
            if (useFahrenheit)
            {
                displayTemp = Convert.ToInt32(displayTemp * 1.8 + 32);
            }

            Icon oldIcon = state.NotifyIcon.Icon;
            state.NotifyIcon.Icon = state.CreateIcon(displayTemp, this);

            if (oldIcon != null)
            {
                DynamicIconRenderer.DisposeIcon(oldIcon);
            }
        }

        private void SetColorsFromTheme(HardwareSensor state, string themeId)
        {
            Theme theme = ThemeManager.GetThemeById(themeId);
            try
            {
                state.IconColorStart = theme.GetColor1();
                state.IconColorEnd = theme.GetColor2();
                state.TextColor = theme.GetTextColor();
            }
            catch
            {
                // Fallback
                state.IconColorStart = Color.White;
                state.IconColorEnd = Color.White;
                state.TextColor = Color.White;
            }
        }

        private void SaveSettings_Sensor(string type)
        {
            var state = ActiveSensors[type];
            if (type == "CPU")
            {
                Properties.Settings.Default.CPU_IconColor1 = state.IconColorStart;
                Properties.Settings.Default.CPU_IconColor2 = state.IconColorEnd;
                Properties.Settings.Default.CPU_TextColor = state.TextColor;
            }
            else if (type == "GPU")
            {
                Properties.Settings.Default.GPU_IconColor1 = state.IconColorStart;
                Properties.Settings.Default.GPU_IconColor2 = state.IconColorEnd;
                Properties.Settings.Default.GPU_TextColor = state.TextColor;
            }
            Properties.Settings.Default.Save();
        }

        private void LoadSettings_Sensor(string type)
        {
            var state = GetState(type);

            string prefix = type.ToLower();
            state.IconPath = Path.Combine(Application.StartupPath, "Resources", $"{prefix}icon.ico");

            Color color1 = Color.White;
            Color color2 = Color.White;
            Color textCol = Color.White;

            if (type == "CPU")
            {
                color1 = Properties.Settings.Default.CPU_IconColor1;
                color2 = Properties.Settings.Default.CPU_IconColor2;
                textCol = Properties.Settings.Default.CPU_TextColor;
            }
            else if (type == "GPU")
            {
                color1 = Properties.Settings.Default.GPU_IconColor1;
                color2 = Properties.Settings.Default.GPU_IconColor2;
                textCol = Properties.Settings.Default.GPU_TextColor;
            }
            

            if (color1.A != 0)
            {
                state.IconColorStart = color1;
                state.IconColorEnd = color2;
                state.TextColor = textCol;
            }
            else // First boot (no saved settings)
            {
                if (IsWindowsThemeLight())
                {
                    SetColorsFromTheme(state, "dark");
                }
                else
                {
                    SetColorsFromTheme(state, "light");
                }
                SaveSettings_Sensor(type);
            }
        }
    }
}
