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
        private bool showCPU = true;

        private Timer timerCPU;
        private int hardwareID_CPU = -1;
        private int sensorID_CPU = -1;
        private int currentTemp_CPU = 0;

        private NotifyIcon notifyIcon_CPU;
        private string CPU_colorMode = "light";
        private Color CPU_Color = Color.White;
        private string CPU_Icon_Path = Path.Combine(Application.StartupPath, "Resources", "cpuicon.ico");
        private Image CPU_Icon = null;

        private void StartCPU()
        {
            LoadSettings_CPU();
            FindCPUSensor();
            if (hardwareID_CPU == -1 || sensorID_CPU == -1)
            {
                throw new Exception("CPU Sensors could not be found. Make sure you have administrator rights!");
            }

            CPU_Icon = Image.FromFile(CPU_Icon_Path);

            InitializeCPUContextMenu();
            notifyIcon_CPU = new NotifyIcon();
            notifyIcon_CPU.ContextMenu = contextMenu_CPU;
            notifyIcon_CPU.Text = $"CPU Temperature: {currentTemp_CPU}°C";
            notifyIcon_CPU.Icon = CreateCPUIcon(currentTemp_CPU);
            notifyIcon_CPU.Visible = true;

            timerCPU = new Timer();
            timerCPU.Interval = 1000;
            timerCPU.Tick += timerCPU_Tick;
            timerCPU.Start();

            GC.Collect();
        }

        private void StopCPU()
        {
            CPU_Icon?.Dispose();
            timerCPU.Stop();
            timerCPU.Dispose();
            notifyIcon_CPU.Icon?.Dispose();
            NativeMethods.DestroyIcon(notifyIcon_CPU.Icon.Handle);
            notifyIcon_CPU.ContextMenu.Dispose();
            notifyIcon_CPU.Dispose();
            startupMenuItem_CPU = null;
            showCPUMenuItem_CPU = null;
            showGPUMenuItem_CPU = null;
            changeScale_CPU = null;
            contextMenu_CPU = null;
            GC.Collect();
        }


        private void FindCPUSensor()
        {
            for (int i = 0; i < computer.Hardware.Count; i++)
            {
                var hardware = computer.Hardware[i];
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    hardware.Update();
                    hardwareID_CPU = i;
                    for (int j = 0; j < hardware.Sensors.Length; j++)
                    {
                        var sensor = hardware.Sensors[j];
                        if (sensor != null && sensor.SensorType == SensorType.Temperature)
                        {
                            sensorID_CPU = j;
                            return;
                        }
                    }
                }
            }
        }

        private void timerCPU_Tick(object sender, EventArgs e)
        {
            try
            {
                computer.Hardware[hardwareID_CPU].Update();
                currentTemp_CPU = Convert.ToInt32(computer.Hardware[hardwareID_CPU].Sensors[sensorID_CPU].Value);

                string temperatureText = $"CPU Temperature: {currentTemp_CPU}°C";

                if (useFahrenheit)
                {
                    currentTemp_CPU = Convert.ToInt32(currentTemp_CPU * 1.8 + 32);
                    temperatureText = $"CPU Temperature: {currentTemp_CPU}°F";
                }

                notifyIcon_CPU.Text = temperatureText;

                notifyIcon_CPU.Icon?.Dispose();
                NativeMethods.DestroyIcon(notifyIcon_CPU.Icon.Handle);
                notifyIcon_CPU.Icon = CreateCPUIcon(currentTemp_CPU);
            }

            catch {}
        }

        private Icon CreateCPUIcon(int temperature)
        {
            string temperatureText = temperature.ToString();

            Bitmap bitmap = new Bitmap(iconWidth, iconHeight);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);

                graphics.DrawImage(CPU_Icon, new Rectangle(0, 0, iconWidth, iconHeight));

                int fontSize = 18;
                int moveX = 1;
                int moveY = 0;

                if (temperature >= 100)
                {
                    fontSize = 14;
                    moveX = 0;
                    moveY = 2;
                }

                using (Font font = new Font(customFontFamily, fontSize))
                {
                    using (Brush brush = new SolidBrush(CPU_Color))
                    {
                        if (CPU_Color == Color.Black)
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
