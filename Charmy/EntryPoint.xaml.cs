﻿using Charmy.Views;
using Hardcodet.Wpf.TaskbarNotification;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;

namespace Charmy
{
    public partial class EntryPoint : Window
    {
        private SettingsWindow _window = new SettingsWindow();

        public EntryPoint()
        {
            TaskbarIcon tbi = new TaskbarIcon
            {
                Icon = new Icon(SystemIcons.Warning, 40, 40),
                ToolTipText = "Settings",

                LeftClickCommand = new RelayCommand(OpenSettings),
                DoubleClickCommand = new RelayCommand(CeaseTracking)
            };

            Hide();
        }

        [DllImport("HotCorners")]
        private static extern void StopTracking();

        private void OpenSettings()
        {
            bool act = _window.Activate();
            if (!act)
            {
                _window = new SettingsWindow();
            }

            _window.Show();
        }

        private void CeaseTracking()
        {
            StopTracking();
            Close();

            bool act = _window.Activate();
            if (act)
            {
                _window.Close();
            }
        }
    }
}
