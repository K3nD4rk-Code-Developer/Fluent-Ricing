using Fluent_Ricing.Widgets;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Fluent_Ricing
{
    // Represents one live App Folders instance shown in the list
    public class FolderWidgetEntry
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public sealed partial class DashboardPage : Page
    {
        // ── State ─────────────────────────────────────────────────────────────
        private readonly HashSet<string> _activeWidgets = new();
        private readonly ObservableCollection<FolderWidgetEntry> _folderEntries = new();

        // Maps widget tag → (toggle button, icon, label, active pill)
        private Dictionary<string, (Button btn, FontIcon icon, TextBlock label, Border pill)>
            _widgetControls;

        private Style _defaultButtonStyle;

        // ── Constructor ───────────────────────────────────────────────────────
        public DashboardPage()
        {
            InitializeComponent();

            _defaultButtonStyle = ClockButton.Style;

            _widgetControls = new()
            {
                ["Clock"] = (ClockButton, ClockButtonIcon, ClockButtonText, ClockActivePill),
                ["Weather"] = (WeatherButton, WeatherButtonIcon, WeatherButtonText, WeatherActivePill),
                ["SystemStats"] = (SystemButton, SystemButtonIcon, SystemButtonText, SystemActivePill),
                ["Media"] = (MediaButton, MediaButtonIcon, MediaButtonText, MediaActivePill),
                ["Calendar"] = (CalendarButton, CalendarButtonIcon, CalendarButtonText, CalendarActivePill),
                ["Notes"] = (NotesButton, NotesButtonIcon, NotesButtonText, NotesActivePill),
            };

            // Bind folder instance list
            FolderInstanceList.ItemsSource = _folderEntries;

            // Sync button states for widgets already running (e.g. re-navigating to this page)
            foreach (var name in WidgetManager.ActiveWidgets)
            {
                if (_widgetControls.TryGetValue(name, out var c))
                {
                    _activeWidgets.Add(name);
                    SetButtonActive(c);
                }
            }

            foreach (var entry in WidgetManager.FolderEntries)
                _folderEntries.Add(entry);

            UpdateActiveBanner();

            // Keep in sync if a widget is closed from its own X button
            WidgetManager.WidgetClosed += OnWidgetClosed;

            Unloaded += (_, _) => WidgetManager.WidgetClosed -= OnWidgetClosed;
        }

        // ── Add / remove toggle ───────────────────────────────────────────────
        private async void AddWidget_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string? widgetName = button?.Tag?.ToString();
            if (widgetName is null) return;

            if (widgetName == "AppFolders")
            {
                await AddFolderWidgetAsync();
                return;
            }

            if (!_widgetControls.TryGetValue(widgetName, out var controls)) return;

            bool isActive = _activeWidgets.Contains(widgetName);

            if (isActive)
            {
                _activeWidgets.Remove(widgetName);
                SetButtonInactive(controls);
                WidgetManager.Remove(widgetName);
            }
            else
            {
                _activeWidgets.Add(widgetName);
                SetButtonActive(controls);
                WidgetManager.Add(widgetName);
            }

            UpdateActiveBanner();
        }

        // ── Configure gear button ─────────────────────────────────────────────
        private void ConfigureWidget_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the Widgets settings page and let the user expand
            // the relevant expander — simplest approach without a separate dialog
            App.ShowSettingsWindow("Widgets");
        }

        // ── App Folders ───────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task AddFolderWidgetAsync()
        {
            var nameBox = new TextBox
            {
                PlaceholderText = "e.g. Dev Tools",
                MinWidth = 260
            };

            var dialog = new ContentDialog
            {
                Title = "Name this folder widget",
                Content = nameBox,
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot,
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            string name = nameBox.Text.Trim();
            if (string.IsNullOrEmpty(name)) name = "App Folder";

            var entry = WidgetManager.AddFolderWidget(name);
            _folderEntries.Add(entry);
            UpdateActiveBanner();
        }

        private void RemoveFolderInstance_Click(object sender, RoutedEventArgs e)
        {
            var id = (sender as Button)?.Tag?.ToString();
            if (id is null) return;

            WidgetManager.RemoveFolderWidget(id);

            var entry = _folderEntries.FirstOrDefault(f => f.Id == id);
            if (entry is not null)
                _folderEntries.Remove(entry);

            UpdateActiveBanner();
        }

        // ── WidgetManager callback ────────────────────────────────────────────
        private void OnWidgetClosed(object? sender, string widgetName)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (_activeWidgets.Remove(widgetName) &&
                    _widgetControls.TryGetValue(widgetName, out var controls))
                {
                    SetButtonInactive(controls);
                    UpdateActiveBanner();
                }
            });
        }

        // ── Button state helpers ──────────────────────────────────────────────
        private void SetButtonActive(
            (Button btn, FontIcon icon, TextBlock label, Border pill) c)
        {
            c.icon.Glyph = "\uE74D";
            c.label.Text = "Remove";
            c.btn.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
            c.btn.CornerRadius = new CornerRadius(4);
            c.pill.Visibility = Visibility.Visible;
        }

        private void SetButtonInactive(
            (Button btn, FontIcon icon, TextBlock label, Border pill) c)
        {
            c.icon.Glyph = "\uE710";
            c.label.Text = "Add Widget";
            c.btn.Style = _defaultButtonStyle;
            c.btn.CornerRadius = new CornerRadius(4);
            c.pill.Visibility = Visibility.Collapsed;
        }

        // ── Active banner ─────────────────────────────────────────────────────
        private void UpdateActiveBanner()
        {
            int total = _activeWidgets.Count + _folderEntries.Count;

            if (total == 0)
            {
                ActiveBanner.Visibility = Visibility.Collapsed;
                return;
            }

            ActiveBanner.Visibility = Visibility.Visible;
            ActiveBannerText.Text = total == 1
                ? "1 widget active on your desktop"
                : $"{total} widgets active on your desktop";
        }
    }
}