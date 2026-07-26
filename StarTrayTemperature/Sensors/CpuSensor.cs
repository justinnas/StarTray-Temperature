using System;
using System.Management;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;

namespace StarTrayTemperature
{
    public class CpuSensor : HardwareSensor
    {
        public CpuSensor() : base("CPU") { }

        public override void FindSensor(Computer computer)
        {
            for (int i = 0; i < computer.Hardware.Count; i++)
            {
                var hardware = computer.Hardware[i];
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    hardware.Update();
                    HardwareID = i;
                    for (int j = 0; j < hardware.Sensors.Length; j++)
                    {
                        var sensor = hardware.Sensors[j];
                        if (sensor != null && sensor.SensorType == SensorType.Temperature)
                        {
                            SensorID = j;
                            return;
                        }
                    }
                }
            }
        }

        public override void InitializeContextMenu(IconTray tray)
        {
            GetCommonContextMenu(tray);
        }

        public override void AddInfoMenuHardware(IconTray tray, MenuItem information)
        {
            information.MenuItems.Add(new MenuItem("Processor:") { Enabled = false });
            information.MenuItems.Add(new MenuItem(GetCpuName()) { Enabled = false });
        }

        public override string GetTooltipText(IHardware hardware, bool useFahrenheit)
        {
            string tooltipText = "";
            if (Properties.Settings.Default.CPU_HoverShowTemperature)
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

            if (Properties.Settings.Default.CPU_HoverShowLoad)
            {
                var loadSensor = Array.Find(hardware.Sensors, s => s != null && s.SensorType == SensorType.Load && s.Name == "CPU Total");
                if (loadSensor != null && loadSensor.Value.HasValue) tooltipText += $"🧠  {loadSensor.Value.Value:F1}%\n";
            }
            if (Properties.Settings.Default.CPU_HoverShowPower)
            {
                var powerSensor = Array.Find(hardware.Sensors, s => s != null && s.SensorType == SensorType.Power && s.Name == "Package");
                if (powerSensor != null && powerSensor.Value.HasValue) tooltipText += $"⚡  {powerSensor.Value.Value:F1}W\n";
            }
            if (Properties.Settings.Default.CPU_HoverShowClock)
            {
                var clockSensor = Array.Find(hardware.Sensors, s => s != null && s.SensorType == SensorType.Clock && s.Name == "Cores (Average)");
                if (clockSensor != null && clockSensor.Value.HasValue) 
                {
                    tooltipText += $"⏱️  {clockSensor.Value.Value / 1000f:F2}GHz\n";
                }
            }

            if (tooltipText != "")
            {
                string tempText = tooltipText;
                tooltipText = "CPU\n" + tempText;
            }

            return tooltipText;
        }

        public override void HandleMissingSensor(IconTray tray)
        {
            throw new StarTrayException("CPU temperature sensors could not be found.\n\nMake sure you are running StarTray with administrator rights.");
        }

        protected override int GetIconOffsetX(bool highTemp) => highTemp ? 1 : 1;
        
        protected override int GetIconOffsetY(bool highTemp) => highTemp ? 1 : 1;

        private static string GetCpuName()
        {
            string cpuName = string.Empty;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select Name from Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    cpuName = obj["Name"].ToString();
                }
            }
            return cpuName;
        }

        public override void AddCustomMenuItems(IconTray tray)
        {
            MenuItem hoverOptions = new MenuItem("Hover display");
            
            MenuItem showTemp = new MenuItem("Show Temperature (🌡️)");
            showTemp.Checked = Properties.Settings.Default.CPU_HoverShowTemperature;
            showTemp.Click += (s, e) => {
                showTemp.Checked = !showTemp.Checked;
                Properties.Settings.Default.CPU_HoverShowTemperature = showTemp.Checked;
                Properties.Settings.Default.Save();
            };

            MenuItem showLoad = new MenuItem("Show Load (🧠)");
            showLoad.Checked = Properties.Settings.Default.CPU_HoverShowLoad;
            showLoad.Click += (s, e) => {
                showLoad.Checked = !showLoad.Checked;
                Properties.Settings.Default.CPU_HoverShowLoad = showLoad.Checked;
                Properties.Settings.Default.Save();
            };

            MenuItem showPower = new MenuItem("Show Power Usage (⚡)");
            showPower.Checked = Properties.Settings.Default.CPU_HoverShowPower;
            showPower.Click += (s, e) => {
                showPower.Checked = !showPower.Checked;
                Properties.Settings.Default.CPU_HoverShowPower = showPower.Checked;
                Properties.Settings.Default.Save();
            };

            MenuItem showClock = new MenuItem("Show Clock Speed (⏱️)");
            showClock.Checked = Properties.Settings.Default.CPU_HoverShowClock;
            showClock.Click += (s, e) => {
                showClock.Checked = !showClock.Checked;
                Properties.Settings.Default.CPU_HoverShowClock = showClock.Checked;
                Properties.Settings.Default.Save();
            };

            hoverOptions.MenuItems.Add(showTemp);
            hoverOptions.MenuItems.Add(showLoad);
            hoverOptions.MenuItems.Add(showPower);
            hoverOptions.MenuItems.Add(showClock);
            
            ContextMenu.MenuItems.Add(hoverOptions);
        }
    }
}
