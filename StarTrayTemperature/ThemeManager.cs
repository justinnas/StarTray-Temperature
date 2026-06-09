using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web.Script.Serialization;

namespace StarTrayTemperature
{
    public class Theme
    {
        public string Id { get; internal set; }
        public string DisplayName { get; set; }
        public string IconColor1 { get; set; }
        public string IconColor2 { get; set; }
        public string TextColor { get; set; }

        public Color GetColor1() => ColorTranslator.FromHtml(IconColor1);
        public Color GetColor2() => ColorTranslator.FromHtml(IconColor2);
        public Color GetTextColor() => ColorTranslator.FromHtml(TextColor);
    }

    public static class ThemeManager
    {
        public static List<Theme> AvailableThemes { get; private set; } = new List<Theme>();
        public static string ThemesDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");

        public static void LoadThemes()
        {
            AvailableThemes.Clear();

            // Hardcoded core themes
            AvailableThemes.Add(new Theme { Id = "light", DisplayName = "Light", IconColor1 = "#FFFFFF", IconColor2 = "#FFFFFF", TextColor = "#FFFFFF" });
            AvailableThemes.Add(new Theme { Id = "dark", DisplayName = "Dark", IconColor1 = "#171717", IconColor2 = "#171717", TextColor = "#171717" });

            if (!Directory.Exists(ThemesDirectory))
            {
                Directory.CreateDirectory(ThemesDirectory);
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            foreach (var file in Directory.GetFiles(ThemesDirectory, "*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    Theme theme = serializer.Deserialize<Theme>(json);
                    if (theme != null)
                    {
                        // Use filename as the ID
                        theme.Id = Path.GetFileNameWithoutExtension(file).ToLower();

                        // Avoid conflict with core themes
                        if (theme.Id == "light" || theme.Id == "dark")
                        {
                            theme.Id = theme.Id + "(custom)";
                        }

                        if (!AvailableThemes.Exists(t => t.Id == theme.Id))
                        {
                            AvailableThemes.Add(theme);
                        }
                    }
                }
                catch
                {
                    // Ignore malformed theme files
                }
            }
        }
        
        public static Theme GetThemeById(string id)
        {
            if (string.IsNullOrEmpty(id)) return AvailableThemes[0];
            return AvailableThemes.Find(t => t.Id.ToLower() == id.ToLower()) ?? AvailableThemes[0];
        }
    }
}
