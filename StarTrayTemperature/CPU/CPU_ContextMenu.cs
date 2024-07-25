using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarTrayTemperature
{
    public partial class IconTray : Form
    {
        private ContextMenu contextMenu_CPU;
        private MenuItem startupMenuItem_CPU;
        private MenuItem showCPUMenuItem_CPU;
        private MenuItem showGPUMenuItem_CPU;
        private MenuItem changeScale_CPU;

        private void InitializeCPUContextMenu()
        {
            contextMenu_CPU = new ContextMenu();

            string cpuName = GetCpuName();

            // -- Header --
            contextMenu_CPU.MenuItems.Add(new MenuItem(AppLabel + " (CPU)") { Enabled = false });
            contextMenu_CPU.MenuItems.Add("-");

            // ------------ Themes ------------
            MenuItem colorModes = new MenuItem("CPU theme");

            MenuItem lightMode = new MenuItem("Light Theme");
            lightMode.Click += LightMode_CPU_Click;
            colorModes.MenuItems.Add(lightMode);

            MenuItem darkMode = new MenuItem("Dark Theme");
            darkMode.Click += DarkMode_CPU_Click;
            colorModes.MenuItems.Add(darkMode);

            MenuItem blue11Mode = new MenuItem("Blue11 Theme");
            blue11Mode.Click += Blue11Mode_CPU_Click;
            colorModes.MenuItems.Add(blue11Mode);

            colorModes.MenuItems.Add("-");

            MenuItem greenMode = new MenuItem("Green Theme");
            greenMode.Click += GreenMode_CPU_Click;
            colorModes.MenuItems.Add(greenMode);

            MenuItem redMode = new MenuItem("Red Theme");
            redMode.Click += RedMode_CPU_Click;
            colorModes.MenuItems.Add(redMode);

            MenuItem blueMode = new MenuItem("Blue Theme");
            blueMode.Click += BlueMode_CPU_Click;
            colorModes.MenuItems.Add(blueMode);

            contextMenu_CPU.MenuItems.Add(colorModes);

            // ------------ Global Options ------------

            MenuItem globalOptions = new MenuItem("Options");

            // -- Startup --
            startupMenuItem_CPU = new MenuItem("Run on Startup");
            startupMenuItem_CPU.Checked = IsTaskScheduled();
            startupMenuItem_CPU.Click += RunOnStartup_Click;
            globalOptions.MenuItems.Add(startupMenuItem_CPU);

            // -- Show GPU --
            showGPUMenuItem_CPU = new MenuItem("Show GPU icon");
            showGPUMenuItem_CPU.Checked = showGPU;
            showGPUMenuItem_CPU.Click += ToggleGPU;
            globalOptions.MenuItems.Add(showGPUMenuItem_CPU);

            // -- Show CPU --
            showCPUMenuItem_CPU = new MenuItem("Show CPU icon");
            showCPUMenuItem_CPU.Checked = showCPU;
            showCPUMenuItem_CPU.Click += ToggleCPU;
            globalOptions.MenuItems.Add(showCPUMenuItem_CPU);

            // -- Change Scale --
            changeScale_CPU = new MenuItem("Change to Fahrenheit");
            if (useFahrenheit)
            {
                changeScale_CPU.Text = "Change to Celsius";
            }
            changeScale_CPU.Click += ChangeScale_Click;
            globalOptions.MenuItems.Add(changeScale_CPU);

            contextMenu_CPU.MenuItems.Add(globalOptions);

            // -------------- More info --------------

            MenuItem information = new MenuItem("Info");

            information.MenuItems.Add(new MenuItem("Processor:") { Enabled = false });
            information.MenuItems.Add(new MenuItem(cpuName) { Enabled = false });
            information.MenuItems.Add("-");
            information.MenuItems.Add(new MenuItem(AppLabel + " " + VersionLabel + " " + CopyrightLabel) { Enabled = false });

            contextMenu_CPU.MenuItems.Add(information);

            // ------------ Exit ------------
            contextMenu_CPU.MenuItems.Add("-");
            MenuItem exitMenuItem = new MenuItem("Exit");
            exitMenuItem.Click += ExitMenuItem_Click;
            contextMenu_CPU.MenuItems.Add(exitMenuItem);
        }

        static string GetCpuName()
        {
            string cpuName = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select Name from Win32_Processor");

            foreach (ManagementObject obj in searcher.Get())
            {
                cpuName = obj["Name"].ToString();
            }

            return cpuName;
        }
    }
}
