using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Windows.Graphics;

namespace Fluent_Ricing
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupTitleBar();
        }

        private void SetupTitleBar()
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(1080, 680));
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(DashboardPage));
        }

        private void NavView_SelectionChanged(NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
                return;
            }

            var item = args.SelectedItem as NavigationViewItem;
            switch (item?.Tag?.ToString())
            {
                case "Dashboard": ContentFrame.Navigate(typeof(DashboardPage)); break;
                case "Widgets": ContentFrame.Navigate(typeof(WidgetsPage)); break;
                case "Appearance": ContentFrame.Navigate(typeof(AppearancePage)); break;
            }
        }

        // Called by App.ShowSettingsWindow() from widget close/edit buttons
        public void NavigateTo(string tag)
        {
            if (tag == "Settings")
            {
                NavView.SelectedItem = NavView.SettingsItem;
                ContentFrame.Navigate(typeof(SettingsPage));
                return;
            }

            var item = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(i => i.Tag?.ToString() == tag);

            if (item is null) return;

            NavView.SelectedItem = item;

            switch (tag)
            {
                case "Dashboard": ContentFrame.Navigate(typeof(DashboardPage)); break;
                case "Widgets": ContentFrame.Navigate(typeof(WidgetsPage)); break;
                case "Appearance": ContentFrame.Navigate(typeof(AppearancePage)); break;
            }
        }
    }
}