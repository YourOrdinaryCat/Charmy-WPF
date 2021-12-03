using Charmy.ViewModels;
using Microsoft.Win32;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Charmy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public readonly static SettingsViewModel SViewModel = new SettingsViewModel();
        public readonly static ThemesViewModel TViewModel = new ThemesViewModel();

        protected override void OnStartup(StartupEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Assembly curAssembly = Assembly.GetExecutingAssembly();

            if (key.GetValue(curAssembly.GetName().Name) == null)
            {
                key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            }

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                BeginTracking();
            }).Start();

            Resources.MergedDictionaries[0].Source =
                new Uri($"../Themes/{TViewModel.CurrentTheme}.xaml", UriKind.Relative);

            TViewModel.CurrentThemeChanged += ThemeChangedHandler;
            base.OnStartup(e);
        }

        private void ThemeChangedHandler(object sender, Enums.WindowsThemes e)
        {
            Resources.MergedDictionaries[0].Source =
                new Uri($"../Themes/{e}.xaml", UriKind.Relative);
        }

        [DllImport("HotCorners")]
        private static extern int BeginTracking();
    }
}
