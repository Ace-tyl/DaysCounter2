using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DaysCounter2.Utils;

namespace DaysCounter2
{
    public partial class SettingsWindow : Window
    {
        public bool savedSettings = false;
        public bool languageModified = false;

        public SettingsWindow()
        {
            InitializeComponent();
            LanguageSelector.ItemsSource = LanguageManager.languages;
            LanguageSelector.SelectedIndex = LanguageManager.languages.FindIndex(x => x.id == App.settings.languageId);
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
            Close();
        }
    }
}
