using Microsoft.UI.Xaml;
using System.Linq;

namespace Fluent_Ricing
{
    public partial class App : Application
    {
        private static MainWindow? _window;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Closed += (_, _) => WidgetManager.CloseAll();
            _window.Activate();
        }

        public static void ShowSettingsWindow(string page = "Dashboard")
        {
            if (_window is MainWindow main)
            {
                main.Activate();
                main.NavigateTo(page);
            }
        }
    }
}