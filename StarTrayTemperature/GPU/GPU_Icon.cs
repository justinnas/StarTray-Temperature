using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace StarTrayTemperature
{
    public partial class IconTray : Form
    {
        private bool showGPU = true;

        private Timer timerGPU;
        private int hardwareID_GPU = -1;
        private int sensorID_GPU = -1;
        private int currentTemp_GPU = 0;

        private NotifyIcon notifyIcon_GPU;
        private string GPU_colorMode = "light";
        private Color GPU_Color = Color.White;
        private string GPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "gpuicon.ico");
        private Image GPU_Icon = null;

        private void StartGPU()
        {
            LoadSettings_GPU();
            FindGPUSensor();
            if (hardwareID_GPU == -1 || sensorID_GPU == -1)
            {
                showGPU = false;

                if (showCPU)
                {
                    showGPUMenuItem_CPU.Enabled = false;
                    showGPUMenuItem_CPU.Checked = false;
                    showGPUMenuItem_CPU.Text = "Show GPU icon (disabled)";
                    Properties.Settings.Default.showGPU = showGPU;
                    Properties.Settings.Default.Save();
                }

                return;
            }

            GPU_Icon = Image.FromFile(GPU_Icon_Path);

            InitializeGPUContextMenu();
            notifyIcon_GPU = new NotifyIcon();
            notifyIcon_GPU.Text = $"GPU Temperature: {currentTemp_GPU}°C";
            notifyIcon_GPU.Icon = CreateGPUIcon(currentTemp_GPU);
            notifyIcon_GPU.Visible = true;

            timerGPU = new Timer();
            timerGPU.Interval = 1000;
            timerGPU.Tick += timerGPU_Tick;
            timerGPU.Start();

            notifyIcon_GPU.ContextMenu = contextMenu_GPU;

            GC.Collect();
        }

        private void StopGPU()
        {
            GPU_Icon?.Dispose();
            timerGPU.Stop();
            timerGPU.Dispose();
            notifyIcon_GPU.Icon?.Dispose();
            NativeMethods.DestroyIcon(notifyIcon_GPU.Icon.Handle);
            notifyIcon_GPU.ContextMenu.Dispose();
            notifyIcon_GPU.Dispose();
            startupMenuItem_GPU = null;
            showCPUMenuItem_GPU = null;
            showGPUMenuItem_GPU = null;
            changeScale_GPU = null;
            contextMenu_GPU = null;
            GC.Collect();
        }


        private void FindGPUSensor()
        {
            for (int i = 0; i < computer.Hardware.Count; i++)
            {
                var hardware = computer.Hardware[i];
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuIntel)
                {
                    hardware.Update();
                    hardwareID_GPU = i;
                    for (int j = 0; j < hardware.Sensors.Length; j++)
                    {
                        var sensor = hardware.Sensors[j];
                        if (sensor != null && sensor.SensorType == SensorType.Temperature)
                        {
                            sensorID_GPU = j;
                            return;
                        }
                    }
                }
            }
        }

        private void timerGPU_Tick(object sender, EventArgs e)
        {
            try
            {
                computer.Hardware[hardwareID_GPU].Update();
                int newTemp = Convert.ToInt32(computer.Hardware[hardwareID_GPU].Sensors[sensorID_GPU].Value);

                if (newTemp == 0 && currentTemp_GPU != 0) return;
                // Fix for devices with a bug causing temperature to switch between normal and zero every second

                currentTemp_GPU = newTemp;

                string temperatureText = $"GPU Temperature: {currentTemp_GPU}°C";

                if (useFahrenheit)
                {
                    currentTemp_GPU = Convert.ToInt32(currentTemp_GPU * 1.8 + 32);
                    temperatureText = $"GPU Temperature: {currentTemp_GPU}°F";
                }

                notifyIcon_GPU.Text = temperatureText;

                notifyIcon_GPU.Icon?.Dispose();
                NativeMethods.DestroyIcon(notifyIcon_GPU.Icon.Handle);
                notifyIcon_GPU.Icon = CreateGPUIcon(currentTemp_GPU);

            }

            catch { }
        }

        private Icon CreateGPUIcon(int temperature)
        {
            string temperatureText = temperature.ToString();

            Bitmap bitmap = new Bitmap(iconWidth, iconHeight);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);

                graphics.DrawImage(GPU_Icon, new Rectangle(0, 0, iconWidth, iconHeight));

                int fontSize = 18;
                int moveX = 3;
                int moveY = 0;

                if (temperature >= 100)
                {
                    fontSize = 14;
                    moveX = 2;
                    moveY = 1;
                }

                using (Font font = new Font(customFontFamily, fontSize))
                {
                    using (Brush brush = new SolidBrush(GPU_Color))
                    {
                        if (GPU_Color == Color.Black)
                        {
                            graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit; // Disable anti-aliasing
                            graphics.SmoothingMode = SmoothingMode.HighQuality;
                        }

                        SizeF textSize = graphics.MeasureString(temperatureText, font);
                        float x = (bitmap.Width - textSize.Width) / 2 + moveX;
                        float y = (bitmap.Height - textSize.Height) / 2 + moveY;

                        graphics.DrawString(temperatureText, font, brush, new PointF(x, y));
                    }
                }
            }

            Icon icon = Icon.FromHandle(bitmap.GetHicon());
            bitmap.Dispose();

            return icon;
        }
    }
}
