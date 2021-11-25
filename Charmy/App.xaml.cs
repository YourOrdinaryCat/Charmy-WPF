using Microsoft.Win32;
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

            base.OnStartup(e);
        }

        [DllImport("HotCorners")]
        private static extern int BeginTracking();
    }
}
