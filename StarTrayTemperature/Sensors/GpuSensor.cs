using System;
using System.Collections.Generic;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;

namespace StarTrayTemperature
{
    public class GpuSensor : HardwareSensor
    {
        public GpuSensor() : base("GPU") { }

        public override void FindSensor(Computer computer)
        {
            string targetGpu = Properties.Settings.Default.GPU_SelectedName;
            int firstHardwareID = -1;
            int firstSensorID = -1;

            for (int i = 0; i < computer.Hardware.Count; i++)
            {
                var hardware = computer.Hardware[i];
                if (!IsGpu(hardware)) continue;

                hardware.Update();

                int sensorID = FindTemperatureSensor(hardware);
                if (sensorID == -1) continue;

                if (firstHardwareID == -1)
                {
                    firstHardwareID = i;
                    firstSensorID = sensorID;
                }

                if (hardware.Name == targetGpu)
                {
                    HardwareID = i;
                    SensorID = sensorID;
                    return;
                }
            }

            if (firstHardwareID != -1)
            {
                HardwareID = firstHardwareID;
                SensorID = firstSensorID;
            }
        }

        private static bool IsGpu(IHardware hardware)
        {
            return hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuIntel;
        }

        // "GPU Core" is preferred, but some integrated GPUs never expose it and only report something like "GPU VR SoC"
        private static int FindTemperatureSensor(IHardware hardware)
        {
            int firstTemperature = -1;

            for (int j = 0; j < hardware.Sensors.Length; j++)
            {
                var sensor = hardware.Sensors[j];
                if (sensor == null || sensor.SensorType != SensorType.Temperature) continue;

                if (sensor.Name == "GPU Core") return j;
                if (firstTemperature == -1) firstTemperature = j;
            }

            return firstTemperature;
        }

        public override void InitializeContextMenu(IconTray tray)
        {
            GetCommonContextMenu(tray);
        }

        public override void AddInfoMenuHardware(IconTray tray, MenuItem information)
        {
            List<IHardware> gpus = new List<IHardware>();
            foreach (var compHardware in tray.computer.Hardware)
            {
                if (IsGpu(compHardware))
                {
                    gpus.Add(compHardware);
                }
            }

            if (gpus.Count > 0)
            {
                List<IHardware> selectable = new List<IHardware>();
                foreach (var gpu in gpus)
                {
                    if (FindTemperatureSensor(gpu) != -1) selectable.Add(gpu);
                }

                information.MenuItems.Add(new MenuItem(selectable.Count > 1 ? "Target GPU:" : "Graphics card:") { Enabled = false });

                foreach (var gpu in gpus)
                {
                    MenuItem gpuItem = new MenuItem(gpu.Name);

                    if (HardwareID != -1 && tray.computer.Hardware[HardwareID] == gpu)
                    {
                        gpuItem.Checked = true;
                    }

                    if (selectable.Count > 1 && selectable.Contains(gpu))
                    {
                        gpuItem.Click += (s, e) =>
                        {
                            Properties.Settings.Default.GPU_SelectedName = gpu.Name;
                            Properties.Settings.Default.Save();
                            tray.RestartSensor("GPU");
                        };
                    }
                    else
                    {
                        gpuItem.Enabled = false;
                    }
                    information.MenuItems.Add(gpuItem);
                }
            }
            else
            {
                information.MenuItems.Add(new MenuItem("GPU not detected") { Enabled = false });
            }
        }

        public override bool ShouldIgnoreTemp(int newTemp)
        {
            return newTemp == 0 && CurrentTemp != 0;
        }

        public override string GetTooltipText(IHardware hardware, bool useFahrenheit)
        {
            string tooltipText = "";

            if (Properties.Settings.Default.GPU_HoverShowTemperature)
            {
                int displayTemp = CurrentTemp;
                string scale = "°C";
                if (useFahrenheit)
                {
                    displayTemp = Convert.ToInt32(displayTemp * 1.8 + 32);
                    scale = "°F";
                }
                tooltipText += $"🌡️  {displayTemp}{scale}\n";
            }
            
            if (Properties.Settings.Default.GPU_HoverShowLoad)
            {
                var loadSensor = Array.Find(hardware.Sensors, s => s != null && s.SensorType == SensorType.Load && s.Name == "GPU Core");
                if (loadSensor != null && loadSensor.Value.HasValue) tooltipText += $"🧠  {loadSensor.Value.Value:F1}%\n";
            }
            if (Properties.Settings.Default.GPU_HoverShowPower)
            {
                var powerSensor = Array.Find(hardware.Sensors, s => s != null && s.SensorType == SensorType.Power && s.Name == "GPU Package");
                if (powerSensor != null && powerSensor.Value.HasValue) tooltipText += $"⚡  {powerSensor.Value.Value:F1}W\n";
            }
            if (Properties.Settings.Default.GPU_HoverShowClock)
            {
                var clockSensor = Array.Find(hardware.Sensors, s => s != null && s.SensorType == SensorType.Clock && s.Name == "GPU Core");
                if (clockSensor != null && clockSensor.Value.HasValue) tooltipText += $"⏱️  {clockSensor.Value.Value / 1000f:F2}GHz\n";
            }
            if (Properties.Settings.Default.GPU_HoverShowMemory)
            {
                var memUsedSensor = Array.Find(hardware.Sensors, s => s != null && s.SensorType == SensorType.SmallData && s.Name == "GPU Memory Used");
                if (memUsedSensor != null && memUsedSensor.Value.HasValue) 
                {
                    tooltipText += $"💾  {memUsedSensor.Value.Value / 1024f:F2}GB\n";
                }
            }

            if (tooltipText != "")
            {
                string tempText = tooltipText;
                tooltipText = "GPU\n" + tempText;
            }

            return tooltipText;
        }

        public override void HandleMissingSensor(IconTray tray)
        {
            tray.showGPU = false;
            tray.ActiveSensors.Remove(Type);

            if (tray.ActiveSensors.TryGetValue("CPU", out var cpu))
            {
                if (cpu.ShowGPUMenuItem != null)
                {
                    cpu.ShowGPUMenuItem.Enabled = false;
                    cpu.ShowGPUMenuItem.Checked = false;
                    cpu.ShowGPUMenuItem.Text = "Show GPU icon (disabled)";
                }
            }

            Properties.Settings.Default.ShowGPU = false;
            Properties.Settings.Default.Save();
        }

        protected override int GetIconOffsetX(bool highTemp) => highTemp ? 3 : 3;
        
        protected override int GetIconOffsetY(bool highTemp) => highTemp ? 1 : 1;

        public override void AddCustomMenuItems(IconTray tray)
        {
            MenuItem hoverOptions = new MenuItem("Hover display");
            
            MenuItem showTemp = new MenuItem("Show Temperature (🌡️)");
            showTemp.Checked = Properties.Settings.Default.GPU_HoverShowTemperature;
            showTemp.Click += (s, e) => {
                showTemp.Checked = !showTemp.Checked;
                Properties.Settings.Default.GPU_HoverShowTemperature = showTemp.Checked;
                Properties.Settings.Default.Save();
            };

            MenuItem showLoad = new MenuItem("Show Load (🧠)");
            showLoad.Checked = Properties.Settings.Default.GPU_HoverShowLoad;
            showLoad.Click += (s, e) => {
                showLoad.Checked = !showLoad.Checked;
                Properties.Settings.Default.GPU_HoverShowLoad = showLoad.Checked;
                Properties.Settings.Default.Save();
            };

            MenuItem showPower = new MenuItem("Show Power Usage (⚡)");
            showPower.Checked = Properties.Settings.Default.GPU_HoverShowPower;
            showPower.Click += (s, e) => {
                showPower.Checked = !showPower.Checked;
                Properties.Settings.Default.GPU_HoverShowPower = showPower.Checked;
                Properties.Settings.Default.Save();
            };

            MenuItem showClock = new MenuItem("Show Clock Speed (⏱️)");
            showClock.Checked = Properties.Settings.Default.GPU_HoverShowClock;
            showClock.Click += (s, e) => {
                showClock.Checked = !showClock.Checked;
                Properties.Settings.Default.GPU_HoverShowClock = showClock.Checked;
                Properties.Settings.Default.Save();
            };

            MenuItem showMemory = new MenuItem("Show Memory Usage (💾)");
            showMemory.Checked = Properties.Settings.Default.GPU_HoverShowMemory;
            showMemory.Click += (s, e) => {
                showMemory.Checked = !showMemory.Checked;
                Properties.Settings.Default.GPU_HoverShowMemory = showMemory.Checked;
                Properties.Settings.Default.Save();
            };

            hoverOptions.MenuItems.Add(showTemp);
            hoverOptions.MenuItems.Add(showLoad);
            hoverOptions.MenuItems.Add(showPower);
            hoverOptions.MenuItems.Add(showClock);
            hoverOptions.MenuItems.Add(showMemory);
            
            ContextMenu.MenuItems.Add(hoverOptions);
        }
    }
}
