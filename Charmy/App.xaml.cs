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

            Resources.MergedDictionaries[0].Source =
                new Uri($"../Themes/{TViewModel.CurrentAppTheme}/{TViewModel.CurrentSystemTheme}.xaml", UriKind.Relative);

            Resources.MergedDictionaries[1].Source =
                new Uri($"../Themes/{TViewModel.CurrentAppTheme}/Styles.xaml", UriKind.Relative);

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                BeginTracking();
            }).Start();

            TViewModel.CurrentThemeChanged += ThemeChangedHandler;
            base.OnStartup(e);
        }

        private void ThemeChangedHandler(object sender, ThemeChangedEventArgs e)
        {
            Resources.MergedDictionaries[0].Source =
                new Uri($"../Themes/{e.NewAppTheme}/{e.NewSystemTheme}.xaml", UriKind.Relative);

            Resources.MergedDictionaries[1].Source =
                new Uri($"../Themes/{e.NewAppTheme}/Styles.xaml", UriKind.Relative);
        }

        [DllImport("HotCorners")]
        private static extern int BeginTracking();
    }
}
