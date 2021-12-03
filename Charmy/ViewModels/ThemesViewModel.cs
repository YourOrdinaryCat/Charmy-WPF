using Charmy.Enums;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.Management;
using System.Security.Principal;
using System.Windows;

namespace Charmy.ViewModels
{
    public class ThemesViewModel : ViewModel
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string RegistryValueName = "AppsUseLightTheme";

        private WindowsThemes _currentTheme;
        /// <summary>
        /// Gets or sets the current theme.
        /// </summary>
        public WindowsThemes CurrentTheme
        {
            get { return _currentTheme; }
            set
            {
                if (Set(ref _currentTheme, value))
                {
                    CurrentThemeChanged?.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// This is raised whenever the current theme changes.
        /// </summary>
        public event EventHandler<WindowsThemes> CurrentThemeChanged;

        public ThemesViewModel()
        {
            var currentUser = WindowsIdentity.GetCurrent();
            string query = string.Format(
                CultureInfo.InvariantCulture,
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                currentUser.User.Value,
                RegistryKeyPath.Replace(@"\", @"\\"),
                RegistryValueName);

            // Watch high contrast
            SystemParameters.StaticPropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SystemParameters.HighContrast))
                {
                    if (SystemParameters.HighContrast)
                    {
                        CurrentTheme = WindowsThemes.HighContrast;
                    }
                    else
                    {
                        CurrentTheme = GetWindowsTheme();
                    }
                }
            };

            try
            {
                var watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += (sender, args) =>
                {
                    CurrentTheme = GetWindowsTheme();
                };

                // Start listening for events
                watcher.Start();
            }
            catch (Exception)
            {
                // This can fail on older Windows versions
            }

            CurrentTheme = GetWindowsTheme();
        }

        private WindowsThemes GetWindowsTheme()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                object registryValueObject = key?.GetValue(RegistryValueName);
                if (registryValueObject == null)
                {
                    return WindowsThemes.Light;
                }

                int registryValue = (int)registryValueObject;

                return registryValue > 0 ? WindowsThemes.Light : WindowsThemes.Dark;
            }
        }
    }
}
