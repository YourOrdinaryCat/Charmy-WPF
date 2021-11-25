using System.Windows;

namespace Charmy
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsViewModel ViewModel => App.SViewModel;

        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
