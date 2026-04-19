using Fluent_Ricing.Widgets;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluent_Ricing
{
    public static class WidgetManager
    {
        // ── Single-instance widget windows ────────────────────────────────────
        private static readonly Dictionary<string, Window> _windows = new();

        // ── Multi-instance folder widgets ─────────────────────────────────────
        private static readonly List<(string Id, Window Window)> _folderWindows = new();
        private static readonly List<FolderWidgetEntry> _folderEntries = new();

        // ── Public read-only views ────────────────────────────────────────────

        // Which single-instance widgets are currently active
        // DashboardPage reads this on navigation to sync button states
        public static IReadOnlyCollection<string> ActiveWidgets =>
            _windows.Keys;

        // Live list of folder widget entries
        // DashboardPage reads this on navigation to populate the folder list
        public static IReadOnlyList<FolderWidgetEntry> FolderEntries =>
            _folderEntries;

        // ── Events ────────────────────────────────────────────────────────────

        // Raised when a single-instance widget is closed from its own X button
        // DashboardPage subscribes to flip the toggle button back to inactive
        public static event EventHandler<string>? WidgetClosed;

        // ── Single-instance add / remove ──────────────────────────────────────
        public static void Add(string widgetName)
        {
            if (_windows.ContainsKey(widgetName))
                return; // already open, nothing to do

            Window window = widgetName switch
            {
                "Clock" => new ClockWidget(),
                "Weather" => new WeatherWidget(),
                "SystemStats" => new SystemStatsWidget(),
                "Media" => new MediaWidget(),
                "Calendar" => new CalendarWidget(),
                "Notes" => new NotesWidget(),
                _ => throw new ArgumentException($"Unknown widget: {widgetName}")
            };

            _windows[widgetName] = window;

            // If the user closes the widget via its own X button rather than
            // the Dashboard toggle, clean up and notify DashboardPage
            window.Closed += (_, _) =>
            {
                _windows.Remove(widgetName);
                WidgetClosed?.Invoke(null, widgetName);
            };

            window.Activate();
        }

        public static void Remove(string widgetName)
        {
            if (!_windows.TryGetValue(widgetName, out var window))
                return;

            // Unsubscribe before closing so we don't double-fire WidgetClosed
            window.Closed -= null;
            window.Close();
            _windows.Remove(widgetName);

            // Still raise the event so any subscribers stay in sync
            WidgetClosed?.Invoke(null, widgetName);
        }

        // ── Multi-instance folder add / remove ────────────────────────────────
        public static FolderWidgetEntry AddFolderWidget(string name)
        {
            var entry = new FolderWidgetEntry
            {
                Id = Guid.NewGuid().ToString(),
                Name = name
            };

            var window = new AppFoldersWidget(entry.Id, name);

            window.Closed += (_, _) =>
            {
                var match = _folderWindows.FirstOrDefault(f => f.Id == entry.Id);
                if (match != default)
                    _folderWindows.Remove(match);

                _folderEntries.RemoveAll(f => f.Id == entry.Id);
            };

            _folderWindows.Add((entry.Id, window));
            _folderEntries.Add(entry);

            window.Activate();

            return entry;
        }

        public static void RemoveFolderWidget(string id)
        {
            var match = _folderWindows.FirstOrDefault(f => f.Id == id);
            if (match == default)
                return;

            match.Window.Closed -= null;
            match.Window.Close();

            _folderWindows.Remove(match);
            _folderEntries.RemoveAll(f => f.Id == id);
        }

        // ── Retrieve a running widget window ──────────────────────────────────

        // Useful for settings pages that want to call Configure() on a live widget
        public static Window? Get(string widgetName)
        {
            _windows.TryGetValue(widgetName, out var window);
            return window;
        }

        public static T? Get<T>(string widgetName) where T : Window
        {
            return Get(widgetName) as T;
        }

        // ── Check if a widget is active ───────────────────────────────────────
        public static bool IsActive(string widgetName) =>
            _windows.ContainsKey(widgetName);

        // ── Shutdown ──────────────────────────────────────────────────────────

        // Called from App.xaml.cs when the main window closes
        public static void CloseAll()
        {
            foreach (var window in _windows.Values)
            {
                window.Closed -= null;
                window.Close();
            }
            _windows.Clear();

            foreach (var (_, window) in _folderWindows)
            {
                window.Closed -= null;
                window.Close();
            }
            _folderWindows.Clear();
            _folderEntries.Clear();
        }
    }
}