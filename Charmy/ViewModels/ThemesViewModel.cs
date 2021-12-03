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

        private WindowsThemes _currentSystemTheme;
        /// <summary>
        /// Gets or sets the current system wide theme.
        /// </summary>
        public WindowsThemes CurrentSystemTheme
        {
            get { return _currentSystemTheme; }
            set
            {
                if (Set(ref _currentSystemTheme, value))
                {
                    var args = new ThemeChangedEventArgs(value, CurrentAppTheme);
                    OnCurrentThemeChanged(args);
                }
            }
        }

        private AppThemes _currentAppTheme;
        /// <summary>
        /// Gets or sets the current custom app theme.
        /// </summary>
        public AppThemes CurrentAppTheme
        {
            get { return _currentAppTheme; }
            set
            {
                if (Set(ref _currentAppTheme, value))
                {
                    var args = new ThemeChangedEventArgs(CurrentSystemTheme, value);
                    OnCurrentThemeChanged(args);
                }
            }
        }

        private bool _darkModeSupported;
        /// <summary>
        /// true if system-wide dark mode is supported, false otherwise.
        /// </summary>
        public bool DarkModeSupported
        {
            get { return _darkModeSupported; }
            set { Set(ref _darkModeSupported, value); }
        }

        /// <summary>
        /// This is raised whenever the current theme changes.
        /// </summary>
        public event EventHandler<ThemeChangedEventArgs> CurrentThemeChanged;

        protected virtual void OnCurrentThemeChanged(ThemeChangedEventArgs e)
        {
            CurrentThemeChanged?.Invoke(this, e);
        }

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
                        CurrentSystemTheme = WindowsThemes.HighContrast;
                    }
                    else
                    {
                        CurrentSystemTheme = GetWindowsTheme();
                    }
                }
            };

            try
            {
                var watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += (sender, args) =>
                {
                    CurrentSystemTheme = GetWindowsTheme();
                };

                // Start listening for events
                watcher.Start();
                DarkModeSupported = true;
            }
            catch (Exception)
            {
                // This can fail on older Windows versions
                DarkModeSupported = false;
            }

            CurrentSystemTheme = GetWindowsTheme();
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

    public class ThemeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The selected system theme.
        /// </summary>
        public WindowsThemes NewSystemTheme { get; private set; }

        /// <summary>
        /// The selected in-app theme.
        /// </summary>
        public AppThemes NewAppTheme { get; private set; }

        public ThemeChangedEventArgs(WindowsThemes sysTheme, AppThemes appTheme)
        {
            NewSystemTheme = sysTheme;
            NewAppTheme = appTheme;
        }

    }
}
