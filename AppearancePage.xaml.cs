using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Fluent_Ricing
{
    public sealed partial class AppearancePage : Page
    {
        public AppearancePage()
        {
            InitializeComponent();
        }

        private void ThemeRadio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (ThemeRadio.SelectedItem as RadioButton)?.Tag?.ToString();
            // TODO: Connect to theme manager
            // AppSettings.Theme = selected;
        }

        private void BackdropCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = BackdropCombo.SelectedIndex;
            // TODO: Apply to MainWindow backdrop
            // 0 = Mica, 1 = Mica Alt, 2 = Acrylic
        }

        private void OpacitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (OpacityValueLabel != null)
                OpacityValueLabel.Text = $"{(int)e.NewValue}%";
            // TODO: AppSettings.WidgetOpacity = e.NewValue / 100.0;
        }

        private void FontSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (FontSizeValueLabel != null)
                FontSizeValueLabel.Text = $"{(int)e.NewValue}px";
            // TODO: AppSettings.WidgetFontSize = e.NewValue;
        }

        private void CornerRadiusSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (CornerRadiusValueLabel != null)
                CornerRadiusValueLabel.Text = $"{(int)e.NewValue}px";
            // TODO: AppSettings.WidgetCornerRadius = e.NewValue;
        }
    }
}