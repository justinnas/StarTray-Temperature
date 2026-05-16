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

        public override void GetHardwareInfo(MenuItem information)
        {
            information.MenuItems.Add(new MenuItem("Processor:") { Enabled = false });
            information.MenuItems.Add(new MenuItem(GetCpuName()) { Enabled = false });
        }

        public override void HandleMissingSensor(IconTray tray)
        {
            throw new Exception("CPU Sensors could not be found. Make sure you have administrator rights!");
        }

        protected override int GetIconOffsetX(bool highTemp) => highTemp ? 0 : 1;
        
        protected override int GetIconOffsetY(bool highTemp) => highTemp ? 2 : 0;

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

        public override void AddSpecificMenuItems(IconTray tray)
        {
            if (!PawnIOManager.IsPawnIoInstalled())
            {
                MenuItem installPawnIO = new MenuItem("Install PawnIO Driver");
                installPawnIO.Click += (s, e) => {
                    PawnIOManager.PromptAndInstallPawnIO(true);
                };
                ContextMenu.MenuItems.Add(installPawnIO);
                ContextMenu.MenuItems.Add("-");
            }
        }
    }
}
