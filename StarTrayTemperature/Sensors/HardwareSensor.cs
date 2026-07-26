using LibreHardwareMonitor.Hardware;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

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
        public Color IconColorStart { get; set; } = Color.White;
        public Color IconColorEnd { get; set; } = Color.White;
        public Color TextColor { get; set; } = Color.White;
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
        
        public abstract void AddInfoMenuHardware(IconTray tray, MenuItem information);
        
        public abstract string GetTooltipText(IHardware hardware, bool useFahrenheit);
        
        public virtual void AddCustomMenuItems(IconTray tray) { }
        
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

            // +=-======+========-+==========--=+
            // PawnIO installer
            // +-===+====================--==+
            if (!PawnIOManager.IsPawnIoInstalled())
            {
                MenuItem installPawnIO = new MenuItem("Install PawnIO Driver");
                installPawnIO.Click += (s, e) => {
                    PawnIOManager.PromptAndInstallPawnIO(true);
                };
                ContextMenu.MenuItems.Add(installPawnIO);
                ContextMenu.MenuItems.Add("-");
            }

            // +=-=========+====================--=+
            // Theme options
            // +-===+=================+==========--==+
            MenuItem colorModes = new MenuItem($"{Type} theme");

            for (int i = 0; i < ThemeManager.AvailableThemes.Count; i++)
            {
                Theme theme = ThemeManager.AvailableThemes[i];
                MenuItem mode = new MenuItem($"{theme.DisplayName} Theme");
                string themeKey = theme.Id;
                mode.Click += (s, e) => tray.ApplyTheme(Type, themeKey);
                colorModes.MenuItems.Add(mode);
            }

            colorModes.MenuItems.Add("-");
            MenuItem reloadThemes = new MenuItem("Reload themes");
            reloadThemes.Click += (s, e) => tray.ReloadThemes();
            colorModes.MenuItems.Add(reloadThemes);

            MenuItem openThemesDir = new MenuItem("Open Themes folder...");
            openThemesDir.Click += (s, e) => Process.Start("explorer.exe", ThemeManager.ThemesDirectory);
            colorModes.MenuItems.Add(openThemesDir);

            ContextMenu.MenuItems.Add(colorModes);

            // +=-=========+====================--=+
            // (Optional) Extra options per sensor
            // +-===+=================+==========--==+
            AddCustomMenuItems(tray);

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
            MenuItem information = new MenuItem("More");
            AddInfoMenuHardware(tray, information);

            information.MenuItems.Add("-");
            MenuItem copyrightItem = new MenuItem($"{tray.AppLabel} {tray.VersionLabel} {tray.CopyrightLabel}");
            copyrightItem.Click += (s, e) => Process.Start(tray.WebpageURL);
            information.MenuItems.Add(copyrightItem);

            ContextMenu.MenuItems.Add(information);

            // +=-=========+====================--=+
            // Exit
            // +-===+=================+==========--==+
            ContextMenu.MenuItems.Add("-");
            MenuItem exitMenuItem = new MenuItem("Exit");
            exitMenuItem.Click += tray.ExitMenuItem_Click;
            ContextMenu.MenuItems.Add(exitMenuItem);
        }

        public virtual Icon CreateIcon(int temperature, IconTray tray)
        {
            string temperatureText = temperature.ToString();
            bool highTemp = temperature >= 100;
            int fontSize = highTemp ? 15 : 18;
            int moveX = GetIconOffsetX(highTemp);
            int moveY = GetIconOffsetY(highTemp);

            using (Font font = new Font(tray.customFontFamily, fontSize))
            {
                return DynamicIconRenderer.CreateDynamicIcon(
                    BaseIcon, 
                    IconColorStart,
                    IconColorEnd,
                    TextColor, 
                    temperatureText, 
                    tray.iconWidth, 
                    tray.iconHeight, 
                    font, 
                    moveX, 
                    moveY);
            }
        }
    }
}
