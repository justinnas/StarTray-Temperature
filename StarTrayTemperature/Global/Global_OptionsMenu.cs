using Microsoft.Win32.TaskScheduler;
using System;
using System.Linq;
using System.Windows.Forms;

namespace StarTrayTemperature
{
    public partial class IconTray : Form
    {
        private void LoadGlobalSettings()
        {
            useFahrenheit = Properties.Settings.Default.UseFahrenheit;
            showCPU = Properties.Settings.Default.ShowCPU;
            showGPU = Properties.Settings.Default.ShowGPU;
        }

        internal void ToggleGPU(object sender, EventArgs e)
        {
            if (showCPU == false && showGPU == true) { return; }

            showGPU = !showGPU;

            if (showGPU == false) StopSensor("GPU");
            else StartSensor("GPU");

            foreach (var state in ActiveSensors.Values)
            {
                if (state.ShowGPUMenuItem != null)
                {
                    state.ShowGPUMenuItem.Checked = showGPU;
                }
            }

            GC.Collect();
            Properties.Settings.Default.ShowGPU = showGPU;
            Properties.Settings.Default.Save();
        }

        internal void ToggleCPU(object sender, EventArgs e)
        {
            if (showCPU == true && showGPU == false) { return; }

            showCPU = !showCPU;

            if (showCPU == false) StopSensor("CPU");
            else StartSensor("CPU");

            foreach (var state in ActiveSensors.Values)
            {
                if (state.ShowCPUMenuItem != null)
                {
                    state.ShowCPUMenuItem.Checked = showCPU;
                }
            }

            GC.Collect();
            Properties.Settings.Default.ShowCPU = showCPU;
            Properties.Settings.Default.Save();
        }

        internal void RunOnStartup_Click(object sender, EventArgs e)
        {
            foreach (var state in ActiveSensors.Values)
            {
                if (state.StartupMenuItem != null)
                {
                    state.StartupMenuItem.Checked = !state.StartupMenuItem.Checked;
                }
            }

            if (!IsTaskScheduled()) CreateTask();
            else RemoveTask();
        }

        internal bool IsTaskScheduled()
        {
            Task task = taskService.GetTask(TaskName);
            return task != null;
        }

        private void CreateTask()
        {
            TaskDefinition taskDefinition = taskService.NewTask();
            taskDefinition.RegistrationInfo.Description = "Start StarTray on system startup.";
            taskDefinition.Triggers.Add(new LogonTrigger());
            taskDefinition.Actions.Add(new ExecAction(Application.ExecutablePath));
            taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;
            taskDefinition.Settings.DisallowStartIfOnBatteries = false;
            taskDefinition.Settings.StopIfGoingOnBatteries = false;
            taskDefinition.Settings.RunOnlyIfIdle = false;
            taskDefinition.Settings.IdleSettings.StopOnIdleEnd = false;
            taskDefinition.Settings.RunOnlyIfNetworkAvailable = false;
            taskDefinition.Settings.ExecutionTimeLimit = TimeSpan.Zero;
            taskDefinition.Settings.StartWhenAvailable = true;

            taskService.RootFolder.RegisterTaskDefinition(TaskName, taskDefinition);
        }

        private void RemoveTask()
        {
            if (taskService == null)
            {
                taskService = new TaskService();
            }

            taskService.RootFolder.DeleteTask(TaskName, false);
        }

        internal void ChangeScale_Click(object sender, EventArgs e)
        {
            useFahrenheit = !useFahrenheit;
            string newText = useFahrenheit ? "Change to Celsius" : "Change to Fahrenheit";

            foreach (var state in ActiveSensors.Values)
            {
                if (state.ChangeScaleMenuItem != null)
                {
                    state.ChangeScaleMenuItem.Text = newText;
                }

                RefreshIcon(state);
            }

            Properties.Settings.Default.UseFahrenheit = useFahrenheit;
            Properties.Settings.Default.Save();
        }

        internal void ExitMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var type in ActiveSensors.Keys.ToList())
            {
                StopSensor(type);
            }

            computer?.Close();
            Application.Exit();
        }
    }
}