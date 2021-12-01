using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

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

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
