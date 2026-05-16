using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;

namespace StarTrayTemperature
{
    public class GpuSensor : HardwareSensor
    {
        public GpuSensor() : base("GPU") { }

        public override void FindSensor(Computer computer)
        {
            for (int i = 0; i < computer.Hardware.Count; i++)
            {
                var hardware = computer.Hardware[i];
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuIntel)
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

        public override void GetHardwareInfo(MenuItem information)
        {
            List<string> gpuNames = GetGpuNames();
            if (gpuNames.Count > 0)
            {
                information.MenuItems.Add(new MenuItem(gpuNames.Count > 1 ? "Graphics cards:" : "Graphics card:") { Enabled = false });
                foreach (string gpuName in gpuNames)
                {
                    information.MenuItems.Add(new MenuItem(gpuName) { Enabled = false });
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

        public override void HandleMissingSensor(IconTray tray)
        {
            tray.showGPU = false;
            if (tray.showCPU)
            {
                if (tray.ActiveSensors.TryGetValue("CPU", out var cpu))
                {
                    if (cpu.ShowGPUMenuItem != null)
                    {
                        cpu.ShowGPUMenuItem.Enabled = false;
                        cpu.ShowGPUMenuItem.Checked = false;
                        cpu.ShowGPUMenuItem.Text = "Show GPU icon (disabled)";
                    }
                }
                Properties.Settings.Default.showGPU = false;
                Properties.Settings.Default.Save();
            }
        }

        protected override int GetIconOffsetX(bool highTemp) => highTemp ? 2 : 3;
        
        protected override int GetIconOffsetY(bool highTemp) => highTemp ? 1 : 0;

        private static List<string> GetGpuNames()
        {
            List<string> gpuNames = new List<string>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select Name from Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    gpuNames.Add(obj["Name"].ToString());
                }
            }
            return gpuNames;
        }
    }
}
