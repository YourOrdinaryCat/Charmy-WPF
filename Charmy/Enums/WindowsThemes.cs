namespace Charmy.Enums
{
    /// <summary>
    /// All the possible base themes the user could enable.
    /// </summary>
    public enum WindowsThemes
    {
        /// <summary>
        /// Light theme, most Windows versions will fallback to this
        /// unless High Contrast is enabled.
        /// </summary>
        Light,

        /// <summary>
        /// Dark theme, supported in Windows 10 and up.
        /// </summary>
        Dark,

        /// <summary>
        /// High contrast themes, most Windows versions should
        /// support this.
        /// </summary>
        HighContrast
    }
}
