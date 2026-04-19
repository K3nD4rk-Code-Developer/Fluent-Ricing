using Microsoft.UI.Composition;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.Runtime.InteropServices;
using Windows.Graphics;
using Windows.Storage;
using WinRT;

namespace Fluent_Ricing.Widgets
{
    public sealed partial class ClockWidget : Window
    {
        [DllImport("user32.dll")] static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")] static extern bool ReleaseCapture();
        [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("dwmapi.dll")] static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
        [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref uint attrValue, int attrSize);

        [StructLayout(LayoutKind.Sequential)]
        struct MARGINS { public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight; }

        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWCP_DONOTROUND = 1;
        private const uint DWMWA_COLOR_NONE = 0xFFFFFFFE;

        private const int GWL_EXSTYLE = -20;
        private const uint WS_EX_NOACTIVATE = 0x08000000;
        private const uint WS_EX_TOOLWINDOW = 0x00000080;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint WM_NCLBUTTONDOWN = 0x00A1;
        private const uint HTCAPTION = 2;
        private static readonly IntPtr HWND_BOTTOM = new(1);

        private readonly DispatcherTimer _clockTimer = new();
        private readonly DispatcherTimer _pinTimer = new();

        private bool _use24Hour = false;
        private bool _showSeconds = false;
        private bool _showDate = true;
        private string _dateFormat = "ddd, MMM d";
        private string _label = "Local Time";
        private string _timeZoneId = "";

        private IntPtr _hwnd;

        public ClockWidget()
        {
            InitializeComponent();
            LoadSettings();
            ApplySettings();
            SetupWindow();
            StartTimers();
        }

        private void SetupWindow()
        {
            ExtendsContentIntoTitleBar = true;
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
            AppWindow.IsShownInSwitchers = false;

            _hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            if (AppWindow.Presenter is OverlappedPresenter p)
            {
                p.IsMaximizable = false;
                p.IsMinimizable = false;
                p.IsResizable = false;
                p.SetBorderAndTitleBar(false, false);
            }

            int doNotRound = DWMWCP_DONOTROUND;
            DwmSetWindowAttribute(_hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref doNotRound, sizeof(int));

            uint noBorder = DWMWA_COLOR_NONE;
            DwmSetWindowAttribute(_hwnd, DWMWA_BORDER_COLOR, ref noBorder, sizeof(uint));

            var margins = new MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
            DwmExtendFrameIntoClientArea(_hwnd, ref margins);

            uint exStyle = (uint)(long)GetWindowLong(_hwnd, GWL_EXSTYLE);
            exStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
            SetWindowLong(_hwnd, GWL_EXSTYLE, (IntPtr)exStyle);

            // Remove the WinUI 3 input island border
            var nonClientSource = InputNonClientPointerSource.GetForWindowId(AppWindow.Id);
            nonClientSource.SetRegionRects(NonClientRegionKind.Caption, null);

            WindowBlur.EnableBlur(_hwnd, 0x20FFFFFF);

            var compositor = new Windows.UI.Composition.Compositor();
            var transparentBrush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
            this.As<ICompositionSupportsSystemBackdrop>().SystemBackdrop = transparentBrush;

            AppWindow.Resize(new SizeInt32(240, 130));
            AppWindow.Move(LoadPosition());

            this.Activate();
            PinToDesktop();

            AppWindow.Changed += (_, args) => { if (args.DidPositionChange) SavePosition(); };
        }

        private void PinToDesktop() =>
            SetWindowPos(_hwnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

        private void StartTimers()
        {
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (_, _) => UpdateClock();
            _clockTimer.Start();
            UpdateClock();

            _pinTimer.Interval = TimeSpan.FromSeconds(3);
            _pinTimer.Tick += (_, _) => PinToDesktop();
            _pinTimer.Start();

            Closed += (_, _) => { _clockTimer.Stop(); _pinTimer.Stop(); };
        }

        private void UpdateClock()
        {
            var tz = string.IsNullOrEmpty(_timeZoneId)
                ? TimeZoneInfo.Local
                : TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId);
            var now = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, tz);

            TimeText.Text = _use24Hour ? now.ToString("HH:mm") : now.ToString("h:mm");
            AmPmText.Text = _use24Hour ? "" : now.ToString("tt");
            SecondsText.Text = _showSeconds ? now.ToString(":ss") : "";
            DateText.Text = now.ToString(_dateFormat);
        }

        private void ApplySettings()
        {
            LabelText.Text = _label;
            SecondsText.Visibility = _showSeconds ? Visibility.Visible : Visibility.Collapsed;
            DateText.Visibility = _showDate ? Visibility.Visible : Visibility.Collapsed;
            AmPmText.Visibility = _use24Hour ? Visibility.Collapsed : Visibility.Visible;
        }

        public void Configure(bool use24Hour, bool showSeconds, bool showDate,
                              string dateFormat, string label, string timeZoneId = "")
        {
            _use24Hour = use24Hour;
            _showSeconds = showSeconds;
            _showDate = showDate;
            _dateFormat = dateFormat;
            _label = label;
            _timeZoneId = timeZoneId;
            ApplySettings();
            SaveSettings();
            UpdateClock();
        }

        private ApplicationDataContainer Settings => ApplicationData.Current.LocalSettings;

        private void SaveSettings()
        {
            Settings.Values["Clock.Use24Hour"] = _use24Hour;
            Settings.Values["Clock.ShowSeconds"] = _showSeconds;
            Settings.Values["Clock.ShowDate"] = _showDate;
            Settings.Values["Clock.DateFormat"] = _dateFormat;
            Settings.Values["Clock.Label"] = _label;
            Settings.Values["Clock.TimeZoneId"] = _timeZoneId;
        }

        private void LoadSettings()
        {
            _use24Hour = Settings.Values["Clock.Use24Hour"] is bool b1 ? b1 : false;
            _showSeconds = Settings.Values["Clock.ShowSeconds"] is bool b2 ? b2 : false;
            _showDate = Settings.Values["Clock.ShowDate"] is bool b3 ? b3 : true;
            _dateFormat = Settings.Values["Clock.DateFormat"] as string ?? "ddd, MMM d";
            _label = Settings.Values["Clock.Label"] as string ?? "Local Time";
            _timeZoneId = Settings.Values["Clock.TimeZoneId"] as string ?? "";
        }

        private void SavePosition()
        {
            Settings.Values["Clock.PosX"] = AppWindow.Position.X;
            Settings.Values["Clock.PosY"] = AppWindow.Position.Y;
        }

        private PointInt32 LoadPosition() => new(
            Settings.Values["Clock.PosX"] is int x ? x : 40,
            Settings.Values["Clock.PosY"] is int y ? y : 40);

        private void DragHandle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ReleaseCapture();
            SendMessage(_hwnd, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
        }

        private void DragHandle_PointerEntered(object sender, PointerRoutedEventArgs e) =>
            ContextBar.Visibility = Visibility.Visible;

        private void DragHandle_PointerExited(object sender, PointerRoutedEventArgs e) =>
            ContextBar.Visibility = Visibility.Collapsed;

        private void EditButton_Click(object sender, RoutedEventArgs e) =>
            App.ShowSettingsWindow("Widgets");

        private void CloseButton_Click(object sender, RoutedEventArgs e) =>
            WidgetManager.Remove("Clock");
    }
}