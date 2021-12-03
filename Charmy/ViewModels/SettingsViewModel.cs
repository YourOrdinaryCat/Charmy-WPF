using System.Runtime.InteropServices;

namespace Charmy.ViewModels
{
    public class SettingsViewModel : ViewModel
    {
        [DllImport("HotCorners")]
        private static extern uint GetDelay();

        [DllImport("HotCorners")]
        private static extern void SetDelay(uint delay);

        public uint HotDelay
        {
            get { return GetDelay(); }
            set { SetDelay(value); }
        }
    }
}
