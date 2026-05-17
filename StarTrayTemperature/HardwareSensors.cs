using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;

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
                if (!ActiveSensors.ContainsKey(type)) return; // If HandleMissingSensor didn't throw, it might mean we just gracefully cancel start.
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
                NativeMethods.DestroyIcon(state.NotifyIcon.Icon.Handle);
                state.NotifyIcon.Icon.Dispose();
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

                state.CurrentTemp = newTemp;
                int displayTemp = state.CurrentTemp;

                if (useFahrenheit)
                {
                    displayTemp = Convert.ToInt32(displayTemp * 1.8 + 32);
                }

                string tooltipText = state.GetTooltipText(hardware, useFahrenheit);

                tooltipText = tooltipText.TrimEnd('\n');
                if (string.IsNullOrEmpty(tooltipText))
                {
                    tooltipText = $"StarTray ({type})";
                }
                
                if (tooltipText.Length > 63) tooltipText = tooltipText.Substring(0, 63);

                state.NotifyIcon.Text = tooltipText;

                if (state.NotifyIcon.Icon != null)
                {
                    NativeMethods.DestroyIcon(state.NotifyIcon.Icon.Handle);
                    state.NotifyIcon.Icon.Dispose();
                }
                
                state.NotifyIcon.Icon = state.CreateIcon(displayTemp, this);
            }
            catch { }
        }

        internal void ApplyTheme(string type, string theme)
        {
            if (!ActiveSensors.ContainsKey(type)) return;
            var state = ActiveSensors[type];
            if (state.ColorMode != theme)
            {
                state.ColorMode = theme;
                string prefix = type.ToLower();

                switch (theme)
                {
                    case "light":
                        state.IconColor = Color.FromArgb(255, 255, 255);
                        state.IconPath = Path.Combine(Application.StartupPath, "Resources", $"{prefix}icon.ico");
                        break;
                    case "dark":
                        state.IconColor = Color.FromArgb(0, 0, 0);
                        state.IconPath = Path.Combine(Application.StartupPath, "Resources", $"{prefix}icon_dark.ico");
                        break;
                    case "blue11":
                        state.IconColor = Color.FromArgb(151, 234, 255);
                        state.IconPath = Path.Combine(Application.StartupPath, "Resources", $"{prefix}icon_blue11.ico");
                        break;
                    case "green":
                        state.IconColor = Color.FromArgb(189, 255, 71);
                        state.IconPath = Path.Combine(Application.StartupPath, "Resources", $"{prefix}icon_green.ico");
                        break;
                    case "red":
                        state.IconColor = Color.FromArgb(255, 161, 150);
                        state.IconPath = Path.Combine(Application.StartupPath, "Resources", $"{prefix}icon_red.ico");
                        break;
                    case "blue":
                        state.IconColor = Color.FromArgb(130, 228, 255);
                        state.IconPath = Path.Combine(Application.StartupPath, "Resources", $"{prefix}icon_blue.ico");
                        break;
                }

                if (state.NotifyIcon.Icon != null)
                {
                    state.NotifyIcon.Icon.Dispose();
                }
                state.BaseIcon = Image.FromFile(state.IconPath);
                state.NotifyIcon.Icon = state.CreateIcon(state.CurrentTemp, this);
                SaveSettings_Sensor(type);
            }
        }

        private void SaveSettings_Sensor(string type)
        {
            var state = ActiveSensors[type];
            Properties.Settings.Default[$"ColorMode_{type}"] = state.ColorMode;
            Properties.Settings.Default[$"TextColor_{type}"] = state.IconColor;
            Properties.Settings.Default[$"IconPath_{type}"] = state.IconPath;
            Properties.Settings.Default.Save();
        }

        private void LoadSettings_Sensor(string type)
        {
            var state = GetState(type);
            
            if (Properties.Settings.Default[$"ColorMode_{type}"] != null)
                state.ColorMode = (string)Properties.Settings.Default[$"ColorMode_{type}"];
                
            if (Properties.Settings.Default[$"TextColor_{type}"] != null)
                state.IconColor = (Color)Properties.Settings.Default[$"TextColor_{type}"];
                
            if (Properties.Settings.Default[$"IconPath_{type}"] != null)
                state.IconPath = (string)Properties.Settings.Default[$"IconPath_{type}"];

            if (string.IsNullOrEmpty(state.IconPath))
            {
                string prefix = type.ToLower();
                if (IsWindowsThemeLight())
                {
                    state.ColorMode = "dark";
                    state.IconColor = Color.FromArgb(0, 0, 0);
                    state.IconPath = Path.Combine(Application.StartupPath, "Resources", $"{prefix}icon_dark.ico");
                }
                else
                {
                    state.ColorMode = "light";
                    state.IconColor = Color.FromArgb(255, 255, 255);
                    state.IconPath = Path.Combine(Application.StartupPath, "Resources", $"{prefix}icon.ico");
                }
                SaveSettings_Sensor(type);
            }
        }
    }
}
