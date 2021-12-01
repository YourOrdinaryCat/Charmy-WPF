using System.Runtime.InteropServices;

namespace Charmy
{
    public class SettingsViewModel : ViewModel
    {
        [DllImport("HotCorners")]
        private static extern ulong GetDelay();

        [DllImport("HotCorners")]
        private static extern void SetDelay(ulong delay);

        public ulong HotDelay
        {
            get { return GetDelay(); }
            set
            {
                SetDelay(value);
            }
        }
    }
}
