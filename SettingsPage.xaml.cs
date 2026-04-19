using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Fluent_Ricing
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void StartupToggle_Toggled(object sender, RoutedEventArgs e)
        {
            bool enabled = StartupToggle.IsOn;
            // TODO: Register/unregister startup task
            // StartupManager.SetStartupEnabled(enabled);
        }
    }
}