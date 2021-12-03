using Charmy.ViewModels;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool result = uint.TryParse(DelayBox.Text, out uint val);
            if (!result)
            {
                MessageBox.Show("Not a valid value. The valid range is: " + uint.MinValue + "-" + uint.MaxValue);
                return;
            }

            ViewModel.HotDelay = val;
        }
    }
}
