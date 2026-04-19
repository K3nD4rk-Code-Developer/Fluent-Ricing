using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using Windows.Graphics;
using Windows.Storage;

namespace Fluent_Ricing.Widgets
{
    public sealed partial class SystemStatsWidget : Window
    {
        [DllImport("user32.dll")] static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const int GWL_EXSTYLE = -20;
        private const uint WS_EX_NOACTIVATE = 0x08000000;
        private const uint WS_EX_TOOLWINDOW = 0x00000080;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private static readonly IntPtr HWND_BOTTOM = new(1);

        private readonly DispatcherTimer _pinTimer = new();
        private IntPtr _hwnd;

        public SystemStatsWidget()
        {
            InitializeComponent();
            SetupWindow();
        }

        private void SetupWindow()
        {
            ExtendsContentIntoTitleBar = true;
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
            AppWindow.IsShownInSwitchers = false;

            _hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            uint exStyle = (uint)(long)GetWindowLong(_hwnd, GWL_EXSTYLE);
            exStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
            SetWindowLong(_hwnd, GWL_EXSTYLE, (IntPtr)exStyle);

            AppWindow.Resize(new SizeInt32(260, 180));
            AppWindow.Move(LoadPosition());
            PinToDesktop();

            _pinTimer.Interval = System.TimeSpan.FromSeconds(3);
            _pinTimer.Tick += (_, _) => PinToDesktop();
            _pinTimer.Start();

            AppWindow.Changed += (s, a) => { if (a.DidPositionChange) SavePosition(); };
            Closed += (_, _) => _pinTimer.Stop();
        }

        private void PinToDesktop() =>
            SetWindowPos(_hwnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

        private ApplicationDataContainer Settings => ApplicationData.Current.LocalSettings;
        private void SavePosition() { Settings.Values["SystemStats.PosX"] = AppWindow.Position.X; Settings.Values["SystemStats.PosY"] = AppWindow.Position.Y; }
        private PointInt32 LoadPosition() => new(Settings.Values["SystemStats.PosX"] is int x ? x : 40, Settings.Values["SystemStats.PosY"] is int y ? y : 560);
    }
}