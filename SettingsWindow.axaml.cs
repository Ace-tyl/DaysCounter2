using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DaysCounter2.Utils;

namespace DaysCounter2
{
    public partial class SettingsWindow : Window
    {
        public bool savedSettings = false;
        public bool languageModified = false;

        Color futureColor { get; set; }
        Color pastColor { get; set; }
        Color distantColor { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();
            LanguageSelector.ItemsSource = LanguageManager.languages;
            LanguageSelector.SelectedIndex = LanguageManager.languages.FindIndex(x => x.id == App.settings.languageId);
            futureColor = App.settings.futureColor;
            pastColor = App.settings.pastColor;
            distantColor = App.settings.distantColor;
            PickFutureColor.BorderBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
            PickPastColor.BorderBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
            PickDistantColor.BorderBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
            PickFutureColor.Background = new SolidColorBrush(futureColor);
            PickPastColor.Background = new SolidColorBrush(pastColor);
            PickDistantColor.Background = new SolidColorBrush(distantColor);
        }

        private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            savedSettings = true;
            if (LanguageSelector.SelectedItem != null)
            {
                string selectedLanguageId = ((LanguageData)LanguageSelector.SelectedItem).id;
                if (App.settings.languageId != selectedLanguageId)
                {
                    languageModified = true;
                }
                App.settings.languageId = selectedLanguageId;
            }
            App.settings.futureColor = futureColor;
            App.settings.pastColor = pastColor;
            App.settings.distantColor = distantColor;
            Close();
        }

        private async void PickFutureColor_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ColorSelector colorSelector = new(futureColor);
            await colorSelector.ShowDialog(this);
            if (colorSelector.selectedColor != null)
            {
                futureColor = (Color)colorSelector.selectedColor;
                PickFutureColor.Background = new SolidColorBrush(futureColor);
            }
        }

        private async void PickPastColor_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ColorSelector colorSelector = new(pastColor);
            await colorSelector.ShowDialog(this);
            if (colorSelector.selectedColor != null)
            {
                pastColor = (Color)colorSelector.selectedColor;
                PickPastColor.Background = new SolidColorBrush(pastColor);
            }
        }

        private async void PickDistantColor_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ColorSelector colorSelector = new(distantColor);
            await colorSelector.ShowDialog(this);
            if (colorSelector.selectedColor != null)
            {
                pastColor = (Color)colorSelector.selectedColor;
                PickPastColor.Background = new SolidColorBrush(pastColor);
            }
        }
    }
}
