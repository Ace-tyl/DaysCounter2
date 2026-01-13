using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Media;
using DaysCounter2.Utils;

namespace DaysCounter2
{
    public partial class SettingsWindow : Window
    {
        public bool savedSettings = false;
        public bool languageModified = false;
        public bool fontModified = false;

        Color futureColor { get; set; }
        Color pastColor { get; set; }
        Color distantColor { get; set; }

        string fontName = "";

        IBrush? defaultBrush;

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
            DateTimeFormatInput.Text = App.settings.dateTimeFormat;
            DateTimeCalendarSelector.SelectedIndex = App.settings.dateTimeCalendar;
            defaultBrush = DateTimeFormatInput.Foreground;
            DestModeSelector.SelectedIndex = App.settings.destinationShowingMode;
            var systemFonts = FontManager.Current.SystemFonts;
            int index = 0;
            foreach (var font in systemFonts)
            {
                var name = font.Name;
                if (name == App.settings.windowFont)
                {
                    index = FontSelector.Items.Count;
                }
                FontSelector.Items.Add(new ComboBoxItem()
                {
                    Content = name,
                    FontFamily = font
                });
            }
            FontSelector.SelectedIndex = index;
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
            App.settings.dateTimeFormat = DateTimeFormatInput.Text ?? "yyyy-MM-dd HH:mm:ss";
            App.settings.dateTimeCalendar = (byte)DateTimeCalendarSelector.SelectedIndex;
            App.settings.destinationShowingMode = (byte)DestModeSelector.SelectedIndex;
            string oldFamilyName = App.settings.windowFont;
            if (FontSelector.SelectedItem != null)
            {
                if (FontSelector.SelectedIndex > 0)
                {
                    App.settings.windowFont = ((ComboBoxItem)FontSelector.SelectedItem).FontFamily.Name;
                }
                else
                {
                    App.settings.windowFont = "";
                }
                if (oldFamilyName != App.settings.windowFont)
                {
                    fontModified = true;
                }
            }
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

        private void DateTimeFormatInput_TextChanged(object? sender, TextChangedEventArgs e)
        {
            bool formatFailed = false;
            if (DateTimeFormatInput.Text == "")
            {
                formatFailed = true;
            }
            try
            {
                string _ = DateTime.Now.ToString(DateTimeFormatInput.Text);
            }
            catch (FormatException)
            {
                formatFailed = true;
            }
            if (formatFailed)
            {
                DateTimeFormatInput.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                SaveButton.IsEnabled = false;
            }
            else
            {
                DateTimeFormatInput.Foreground = defaultBrush;
                SaveButton.IsEnabled = true;
            }
        }

        private void DateTimeFormatText_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Process.Start(new ProcessStartInfo() { FileName = Lang.Resources.settings_dateTimeFormat_helpUrl, UseShellExecute = true });
        }

        private void LangDefault_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LanguageSelector.SelectedIndex = 0;
        }

        private void TimeFormatDefault_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            DateTimeFormatInput.Text = "yyyy-MM-dd HH:mm:ss";
            DateTimeCalendarSelector.SelectedIndex = 0;
        }

        private void ColorDefault_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            futureColor = new Color(160, 102, 204, 255);
            pastColor = new Color(160, 238, 0, 0);
            distantColor = new Color(0, 0, 0, 0);
            PickFutureColor.Background = new SolidColorBrush(futureColor);
            PickPastColor.Background = new SolidColorBrush(pastColor);
            PickDistantColor.Background = new SolidColorBrush(distantColor);
        }

        private void DestModeDefault_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            DestModeSelector.SelectedIndex = 0;
        }

        private void FontDefault_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            FontSelector.SelectedIndex = 0;
        }
    }
}
