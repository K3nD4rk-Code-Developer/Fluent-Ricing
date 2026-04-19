using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Fluent_Ricing
{
    public sealed partial class WidgetsPage : Page
    {
        public WidgetsPage()
        {
            InitializeComponent();
        }

        private async void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Add App Folder",
                Content = new TextBox { PlaceholderText = "Folder name..." },
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
            // TODO: Connect to folder widget manager
        }
    }
}