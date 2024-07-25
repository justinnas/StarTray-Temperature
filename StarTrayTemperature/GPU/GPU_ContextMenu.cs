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
        private ContextMenu contextMenu_GPU;
        private MenuItem startupMenuItem_GPU;
        private MenuItem showCPUMenuItem_GPU;
        private MenuItem showGPUMenuItem_GPU;
        private MenuItem changeScale_GPU;

        private void InitializeGPUContextMenu()
        {
            contextMenu_GPU = new ContextMenu();

            List<string> gpuNames = GetGpuNames();

            // -- Header --
            contextMenu_GPU.MenuItems.Add(new MenuItem(AppLabel + " (GPU)") { Enabled = false });
            contextMenu_GPU.MenuItems.Add("-");

            // ------------ Themes ------------
            MenuItem colorModes = new MenuItem("GPU theme");

            MenuItem lightMode = new MenuItem("Light Theme");
            lightMode.Click += LightMode_GPU_Click;
            colorModes.MenuItems.Add(lightMode);

            MenuItem darkMode = new MenuItem("Dark Theme");
            darkMode.Click += DarkMode_GPU_Click;
            colorModes.MenuItems.Add(darkMode);

            MenuItem blue11Mode = new MenuItem("Blue11 Theme");
            blue11Mode.Click += Blue11Mode_GPU_Click;
            colorModes.MenuItems.Add(blue11Mode);

            colorModes.MenuItems.Add("-");

            MenuItem greenMode = new MenuItem("Green Theme");
            greenMode.Click += GreenMode_GPU_Click;
            colorModes.MenuItems.Add(greenMode);

            MenuItem redMode = new MenuItem("Red Theme");
            redMode.Click += RedMode_GPU_Click;
            colorModes.MenuItems.Add(redMode);

            MenuItem blueMode = new MenuItem("Blue Theme");
            blueMode.Click += BlueMode_GPU_Click;
            colorModes.MenuItems.Add(blueMode);

            contextMenu_GPU.MenuItems.Add(colorModes);

            // ------------ Global Options ------------

            MenuItem globalOptions = new MenuItem("Options");

            // -- Startup --
            startupMenuItem_GPU = new MenuItem("Run on Startup");
            startupMenuItem_GPU.Checked = IsTaskScheduled();
            startupMenuItem_GPU.Click += RunOnStartup_Click;
            globalOptions.MenuItems.Add(startupMenuItem_GPU);

            // -- Show GPU --
            showGPUMenuItem_GPU = new MenuItem("Show GPU icon");
            showGPUMenuItem_GPU.Checked = showGPU;
            showGPUMenuItem_GPU.Click += ToggleGPU;
            globalOptions.MenuItems.Add(showGPUMenuItem_GPU);

            // -- Show CPU --
            showCPUMenuItem_GPU = new MenuItem("Show CPU icon");
            showCPUMenuItem_GPU.Checked = showCPU;
            showCPUMenuItem_GPU.Click += ToggleCPU;
            globalOptions.MenuItems.Add(showCPUMenuItem_GPU);

            // -- Change Scale --
            changeScale_GPU = new MenuItem("Change to Fahrenheit");
            if (useFahrenheit)
            {
                changeScale_GPU.Text = "Change to Celsius";
            }
            changeScale_GPU.Click += ChangeScale_Click;
            globalOptions.MenuItems.Add(changeScale_GPU);

            contextMenu_GPU.MenuItems.Add(globalOptions);

            // -------------- More info --------------

            MenuItem information = new MenuItem("Info");

            if (gpuNames.Count > 0)
            {
                if (gpuNames.Count > 1)
                    information.MenuItems.Add(new MenuItem("Graphics cards:") { Enabled = false });
                else
                    information.MenuItems.Add(new MenuItem("Graphics card:") { Enabled = false });

                foreach (string gpuName in gpuNames)
                {
                    information.MenuItems.Add(new MenuItem(gpuName) { Enabled = false });
                }
            }
            else
            {
                information.MenuItems.Add(new MenuItem("GPU not detected") { Enabled = false });
            }   

            information.MenuItems.Add("-");
            information.MenuItems.Add(new MenuItem(AppLabel + " " + VersionLabel + " " + CopyrightLabel) { Enabled = false });

            contextMenu_GPU.MenuItems.Add(information);

            // ------------ Exit ------------
            contextMenu_GPU.MenuItems.Add("-");
            MenuItem exitMenuItem = new MenuItem("Exit");
            exitMenuItem.Click += ExitMenuItem_Click;
            contextMenu_GPU.MenuItems.Add(exitMenuItem);
        }

        static List<string> GetGpuNames()
        {
            List<string> gpuNames = new List<string>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select Name from Win32_VideoController");

            foreach (ManagementObject obj in searcher.Get())
            {
                gpuNames.Add(obj["Name"].ToString());
            }

            return gpuNames;
        }
    }
}
