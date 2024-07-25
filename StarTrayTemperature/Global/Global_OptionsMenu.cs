using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StarTrayTemperature
{
    public partial class IconTray : Form
    {
        private void LoadGlobalSettings()
        {
            useFahrenheit = Properties.Settings.Default.UseFahrenheit;
            showCPU = Properties.Settings.Default.showCPU;
            showGPU = Properties.Settings.Default.showGPU;
        }

        private void ToggleGPU(object sender, EventArgs e)
        {
            if (showCPU == false && showGPU == true) { return; }

            showGPU = !showGPU;

            if (showGPU == false)
            {
                StopGPU();
            }
            else
            {
                StartGPU();
            }

            if (showGPU)
            {
                showGPUMenuItem_CPU.Checked = showGPU;
                showGPUMenuItem_GPU.Checked = showGPU;
            }
            else
            {
                showGPUMenuItem_CPU.Checked = showGPU;
            }


            GC.Collect();

            Properties.Settings.Default.showGPU = showGPU;
            Properties.Settings.Default.Save();
        }

        private void ToggleCPU(object sender, EventArgs e)
        {
            if (showCPU == true && showGPU == false) { return; }

            showCPU = !showCPU;

            if (showCPU == false)
            {
                StopCPU();
            }
            else
            {
                StartCPU();
            }

            if (showCPU)
            {
                showCPUMenuItem_CPU.Checked = showGPU;
                showCPUMenuItem_GPU.Checked = showGPU;
            }
            else
            {
                showCPUMenuItem_GPU.Checked = showCPU;
            }

            GC.Collect();

            Properties.Settings.Default.showCPU = showCPU;
            Properties.Settings.Default.Save();
        }

        private void RunOnStartup_Click(object sender, EventArgs e)
        {
            if (showCPU) startupMenuItem_CPU.Checked = !startupMenuItem_CPU.Checked;
            if (showGPU) startupMenuItem_GPU.Checked = !startupMenuItem_GPU.Checked;

            if (!IsTaskScheduled())
            {
                CreateTask();
            }
            else
            {
                RemoveTask();
            }
        }

        private bool IsTaskScheduled()
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

        private void ChangeScale_Click(object sender, EventArgs e)
        {
            if (showCPU)
            {
                if (useFahrenheit)
                {
                    changeScale_CPU.Text = "Change to Fahrenheit";
                }
                else
                {
                    changeScale_CPU.Text = "Change to Celsius";
                }
            }

            if (showGPU)
            {
                if (useFahrenheit)
                {
                    changeScale_GPU.Text = "Change to Fahrenheit";
                }
                else
                {
                    changeScale_GPU.Text = "Change to Celsius";
                }
            }

            useFahrenheit = !useFahrenheit;

            Properties.Settings.Default.UseFahrenheit = useFahrenheit;
            Properties.Settings.Default.Save();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
            Close();
        }
    }
}
