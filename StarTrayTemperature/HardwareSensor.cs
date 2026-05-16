using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;

namespace StarTrayTemperature
{
    public abstract class HardwareSensor
    {
        public string Type { get; protected set; }
        public Timer Timer { get; set; }
        public int HardwareID { get; set; } = -1;
        public int SensorID { get; set; } = -1;
        public int CurrentTemp { get; set; } = 0;
        public NotifyIcon NotifyIcon { get; set; }
        public string ColorMode { get; set; } = "light";
        public Color IconColor { get; set; } = Color.White;
        public string IconPath { get; set; }
        public Image BaseIcon { get; set; }

        public ContextMenu ContextMenu { get; set; }
        public MenuItem StartupMenuItem { get; set; }
        public MenuItem ShowCPUMenuItem { get; set; }
        public MenuItem ShowGPUMenuItem { get; set; }
        public MenuItem ChangeScaleMenuItem { get; set; }

        protected HardwareSensor(string type)
        {
            Type = type;
        }

        public abstract void FindSensor(Computer computer);
        
        public abstract void GetHardwareInfo(MenuItem information);
        
        public virtual void AddSpecificMenuItems(IconTray tray) { }
        
        public virtual bool ShouldIgnoreTemp(int newTemp) { return false; }

        public abstract void HandleMissingSensor(IconTray tray);

        protected abstract int GetIconOffsetX(bool highTemp);
        
        protected abstract int GetIconOffsetY(bool highTemp);

        public abstract void InitializeContextMenu(IconTray tray);

        protected void GetCommonContextMenu(IconTray tray)
        {
            ContextMenu = new ContextMenu();

            ContextMenu.MenuItems.Add(new MenuItem($"{tray.AppLabel} ({Type})") { Enabled = false });
            ContextMenu.MenuItems.Add("-");


            // +=-=========+====================--=+
            // Theme options
            // +-===+=================+==========--==+
            MenuItem colorModes = new MenuItem($"{Type} theme");
            string[] themes = { "Light", "Dark", "Blue11", "Green", "Red", "Blue" };
            string[] themeKeys = { "light", "dark", "blue11", "green", "red", "blue" };

            for (int i = 0; i < themes.Length; i++)
            {
                MenuItem mode = new MenuItem($"{themes[i]} Theme");
                string themeKey = themeKeys[i];
                mode.Click += (s, e) => tray.ApplyTheme(Type, themeKey);
                colorModes.MenuItems.Add(mode);
                if (i == 2) colorModes.MenuItems.Add("-");
            }

            ContextMenu.MenuItems.Add(colorModes);


            // +=-=========+====================--=+
            // Global options
            // +-===+=================+==========--==+
            MenuItem globalOptions = new MenuItem("Options");

            StartupMenuItem = new MenuItem("Run on Startup");
            StartupMenuItem.Checked = tray.IsTaskScheduled();
            StartupMenuItem.Click += tray.RunOnStartup_Click;
            globalOptions.MenuItems.Add(StartupMenuItem);

            ShowGPUMenuItem = new MenuItem("Show GPU icon");
            ShowGPUMenuItem.Checked = tray.showGPU;
            ShowGPUMenuItem.Click += tray.ToggleGPU;
            globalOptions.MenuItems.Add(ShowGPUMenuItem);

            ShowCPUMenuItem = new MenuItem("Show CPU icon");
            ShowCPUMenuItem.Checked = tray.showCPU;
            ShowCPUMenuItem.Click += tray.ToggleCPU;
            globalOptions.MenuItems.Add(ShowCPUMenuItem);

            ChangeScaleMenuItem = new MenuItem(tray.useFahrenheit ? "Change to Celsius" : "Change to Fahrenheit");
            ChangeScaleMenuItem.Click += tray.ChangeScale_Click;
            globalOptions.MenuItems.Add(ChangeScaleMenuItem);

            ContextMenu.MenuItems.Add(globalOptions);


            // +=-=========+====================--=+
            // Hardware info
            // +-===+=================+==========--==+
            MenuItem information = new MenuItem("Info");
            GetHardwareInfo(information);

            information.MenuItems.Add("-");
            information.MenuItems.Add(new MenuItem($"{tray.AppLabel} {tray.VersionLabel} {tray.CopyrightLabel}") { Enabled = false });

            ContextMenu.MenuItems.Add(information);
            ContextMenu.MenuItems.Add("-");


            // +=-=========+====================--=+
            // (Optional) Extra options per sensor
            // +-===+=================+==========--==+
            AddSpecificMenuItems(tray);


            // +=-=========+====================--=+
            // Exit
            // +-===+=================+==========--==+
            MenuItem exitMenuItem = new MenuItem("Exit");
            exitMenuItem.Click += tray.ExitMenuItem_Click;
            ContextMenu.MenuItems.Add(exitMenuItem);
        }

        public virtual Icon CreateIcon(int temperature, IconTray tray)
        {
            string temperatureText = temperature.ToString();
            Bitmap bitmap = new Bitmap(tray.iconWidth, tray.iconHeight);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.DrawImage(BaseIcon, new Rectangle(0, 0, tray.iconWidth, tray.iconHeight));

                bool highTemp = temperature >= 100;
                int fontSize = highTemp ? 14 : 18;
                int moveX = GetIconOffsetX(highTemp);
                int moveY = GetIconOffsetY(highTemp);

                using (Font font = new Font(tray.customFontFamily, fontSize))
                {
                    using (Brush brush = new SolidBrush(IconColor))
                    {
                        if (IconColor == Color.Black)
                        {
                            graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
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
