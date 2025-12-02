using MaterialDesignThemes.Wpf;
using System.Windows;
using Application = System.Windows.Application;

namespace GymClient.Utils
{
    public static class ThemeManager
    {
        public static event Action? ThemeChanged;

        /// <summary>
        /// Устанавливает базовую тему (Light / Dark) в глобальном BundledTheme.
        /// </summary>
        public static void SetBaseTheme(BaseTheme baseTheme)
        {
            BundledTheme? FindBundledTheme(ResourceDictionary dict)
            {
                foreach (var md in dict.MergedDictionaries)
                {
                    if (md is BundledTheme bundledTheme)
                        return bundledTheme;

                    var found = FindBundledTheme(md);
                    if (found != null)
                        return found;
                }
                return null;
            }

            var bundledTheme = FindBundledTheme(Application.Current.Resources);
            if (bundledTheme != null)
            {
                bundledTheme.BaseTheme = baseTheme;
            }
        }

        /// <summary>
        /// Применяет сохранённую тему из настроек ко всему приложению.
        /// </summary>
        public static void ApplyTheme()
        {
            var themeName = GetCurrentTheme();

            var baseTheme = themeName.Equals("Dark", StringComparison.OrdinalIgnoreCase)
                ? BaseTheme.Dark
                : BaseTheme.Light;

            SetBaseTheme(baseTheme);
        }

        /// <summary>
        /// Меняет тему и сразу сохраняет её в настройки + применяет глобально.
        /// </summary>
        public static void ChangeTheme(string newTheme)
        {
            Properties.Settings.Default.AppTheme = newTheme;
            Properties.Settings.Default.Save();

            ApplyTheme();

            ThemeChanged?.Invoke();
        }

        public static string GetCurrentTheme()
        {
            var saved = Properties.Settings.Default.AppTheme;
            return string.IsNullOrEmpty(saved) ? "Light" : saved;
        }
    }
}
