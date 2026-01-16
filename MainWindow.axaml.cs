using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DaysCounter2.Utils;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace DaysCounter2
{
    public class DisplayedEvent
    {
        public string uuid { get; set; } = "";
        public string name { get; set; } = "";
        public long delta { get; set; }
        public string timerText { get; set; } = "";
        public string destinationText { get; set; } = "";
        public IBrush brush { get; set; } = new SolidColorBrush();
        public InlineCollection nameInlines { get; set; } = [];
        public Event correspondEvent { get; set; } = new Event();
    }

    public partial class MainWindow : Window
    {
        Thread refreshThread, checkUpdateThread;
        List<Event> events = [];
        HashSet<string> eventUuids = [];
        List<DisplayedEvent> displayedEvents = [];
        ObservableCollection<DisplayedEvent> Displayed { get; set; } = [];
        string languageId;
        CultureInfo culture;
        static CultureInfo ArabicCulture = CultureInfo.CreateSpecificCulture("ar-SA");
        UpdateChecker updateChecker = new();
        bool updateFound = false;

        List<Event> ReadEvents(Stream stream)
        {
            List<Event> events = [];
            try
            {
                DataContractJsonSerializer ser = new(typeof(List<Event>));
                var result = ser.ReadObject(stream);
                if (result != null)
                {
                    events = (List<Event>)result;
                }
            }
            catch { }
            stream.Close();
            return events;
        }

        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists(App.appFilePath))
            {
                Stream stream = File.OpenRead(App.appFilePath);
                events = ReadEvents(stream);
                foreach (var ev in events)
                {
                    eventUuids.Add(ev.uuid);
                }
            }
            refreshThread = new Thread(RefreshTimer);
            refreshThread.Start();
            languageId = App.settings.languageId;
            culture = CultureInfo.CreateSpecificCulture(languageId);
            VersionText.Text += Lang.Resources.ui_version + "\n" + updateChecker.currentVersion.ToString();
            UpdateExportButtonContent(events.Count);
            checkUpdateThread = new Thread(CheckForUpdate);
            checkUpdateThread.Start();
        }

        public async void CheckForUpdate()
        {
            await updateChecker.GetNewestVersion();
            if (updateChecker.currentVersion.EarlierThan(updateChecker.newestVersion))
            {
                updateFound = true;
                Dispatcher.UIThread.Invoke(() =>
                {
                    VersionText.Text += "\n" + Lang.Resources.ui_newVersion + "\n" + updateChecker.newestVersion.ToString();
                    VersionText.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                });
            }
            else
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    VersionText.Text += "\n" + Lang.Resources.ui_newestVersion;
                });
            }
        }

        void SaveEvents(List<Event>? exportedEvents = null, Stream? givenStream = null)
        {
            Stream stream = givenStream ?? File.Open(App.appFilePath, FileMode.Create, FileAccess.Write);
            DataContractJsonSerializer ser = new(typeof(List<Event>));
            ser.WriteObject(stream, exportedEvents ?? events);
            stream.Close();
        }

        void SaveSettings()
        {
            Stream stream = File.Open(App.appSettingsPath, FileMode.Create, FileAccess.Write);
            DataContractJsonSerializer ser = new(typeof(SettingsManager));
            ser.WriteObject(stream, App.settings);
            stream.Close();
        }

        public void RefreshTimer()
        {
            DateTime? lastRefreshTime = null;
            try
            {
                while (true)
                {
                    DateTime now = DateTime.Now;
                    if (lastRefreshTime == null || now.Second != lastRefreshTime.Value.Second)
                    {
                        lastRefreshTime = now;
                        Dispatcher.UIThread.Invoke(() => RefreshWindow(now));
                    }
                    Thread.Sleep(1000 - now.Millisecond);
                }
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
        }

        public void RefreshWindow()
        {
            RefreshWindow(DateTime.Now);
        }

        string DateTimeString(DateTime dateTime)
        {
            CultureInfo usedCulture = culture;
            switch (App.settings.dateTimeCalendar)
            {
                case (byte)DisplayCalendarTypes.Gregorian:
                    // Gregorian Calendar
                    usedCulture.DateTimeFormat.Calendar = new GregorianCalendar();
                    break;
                case (byte)DisplayCalendarTypes.AlHijri:
                    // AlHijri Calendar
                    usedCulture = ArabicCulture; // Use Arabic culture
                    usedCulture.DateTimeFormat.Calendar = new HijriCalendar();
                    break;
            }

            try
            {
                return dateTime.ToString(App.settings.dateTimeFormat, usedCulture);
            }
            catch (FormatException)
            {
                App.settings.dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                return dateTime.ToString(App.settings.dateTimeFormat, usedCulture);
            }
            catch (Exception)
            {
                return Lang.Resources.ui_outOfRange;
            }
        }

        public void RefreshWindow(DateTime now)
        {
            CurrentTimeText.Text = Lang.Resources.ui_currentTime + DateTimeString(now);

            if (TimerList.SelectedItems != null && TimerList.SelectedItems.Count > 1)
            {
                return;
            }

            MyDateTime myNow = new(now, TimeZoneInfo.Local);
            long myNowJulian = myNow.GetJulianSecond();
            displayedEvents = [];
            string searchText = SearchBox.Text ?? "";
            foreach (Event ev in events)
            {
                // Try match search text
                int matchPos, matchLength;
                matchPos = ev.name.ToLower().IndexOf(searchText.ToLower());
                matchLength = searchText.Length;
                if (matchPos == -1)
                {
                    try
                    {
                        Match match = Regex.Match(ev.name, searchText, RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            matchPos = match.Index;
                            matchLength = match.Length;
                        }
                    }
                    catch { }
                }
                if (matchPos == -1)
                {
                    continue;
                }
                // Construct name display runs
                InlineCollection runs = [];
                if (searchText != "")
                {
                    runs.Add(new Run(ev.name.Substring(0, matchPos)));
                    runs.Add(new Run { Text = ev.name.Substring(matchPos, matchLength), TextDecorations = TextDecorations.Underline });
                    runs.Add(new Run(ev.name.Substring(matchPos + matchLength)));
                }
                else
                {
                    runs.Add(new Run(ev.name));
                }
                // Construct timer string
                long delta = ev.GetDelta(myNow, myNowJulian);
                string timerText = "";
                if (delta >= 0)
                {
                    timerText = string.Format(Lang.Resources.ui_timerLater, delta / 86400, delta / 3600 % 24, delta / 60 % 60, delta % 60);
                }
                else
                {
                    long fdelta = -delta;
                    timerText = string.Format(Lang.Resources.ui_timerAgo, fdelta / 86400, fdelta / 3600 % 24, fdelta / 60 % 60, fdelta % 60);
                }
                double days = Math.Abs(delta / 86400.0);
                // Construct destination text
                string destinationText = "";
                if (App.settings.destinationShowingMode != (byte)DestinationShowingModes.None
                    && (App.settings.destinationShowingMode != (byte)DestinationShowingModes.FutureOnly || delta >= 0))
                {
                    DateTime? dest = ev.GetDestinationDateTime(myNow, myNowJulian);
                    if (dest == null)
                    {
                        destinationText = Lang.Resources.ui_outOfRange;
                    }
                    else
                    {
                        destinationText = DateTimeString(dest.Value);
                    }
                }
                // Construct background
                Color background;
                if (App.settings.backgroundGradientMode == (byte)BackgroundGradientModes.Logarithm)
                {
                    if (App.settings.backgroundGradientLow == App.settings.backgroundGradientHigh)
                    {
                        background = days < App.settings.backgroundGradientLow
                            ? (delta > 0 ? App.settings.futureColor : App.settings.pastColor)
                            : App.settings.distantColor;
                    }
                    else
                    {
                        background = ColorSelector.Interpolate(delta > 0 ? App.settings.futureColor : App.settings.pastColor, App.settings.distantColor,
                            Math.Log(days / App.settings.backgroundGradientLow, App.settings.backgroundGradientHigh / App.settings.backgroundGradientLow));
                    }
                }
                else if (App.settings.backgroundGradientMode == (byte)BackgroundGradientModes.Linear)
                {
                    if (App.settings.backgroundGradientLow == App.settings.backgroundGradientHigh)
                    {
                        background = days < App.settings.backgroundGradientLow
                            ? (delta > 0 ? App.settings.futureColor : App.settings.pastColor)
                            : App.settings.distantColor;
                    }
                    else
                    {
                        background = ColorSelector.Interpolate(delta > 0 ? App.settings.futureColor : App.settings.pastColor, App.settings.distantColor,
                            (days - App.settings.backgroundGradientLow) / (App.settings.backgroundGradientHigh - App.settings.backgroundGradientLow));
                    }
                }
                else
                {
                    background = delta > 0 ? App.settings.futureColor : App.settings.pastColor;
                }
                if (ev.customizeColor)
                {
                    background = ColorSelector.Overlay(ev.color, background);
                }
                // Add displayed event
                displayedEvents.Add(new DisplayedEvent
                {
                    uuid = ev.uuid,
                    name = ev.name,
                    delta = delta,
                    timerText = timerText,
                    destinationText = destinationText,
                    brush = new SolidColorBrush(background),
                    nameInlines = runs,
                    correspondEvent = ev,
                });
            }
            displayedEvents.Sort((a, b) =>
            {
                if (a.delta != b.delta)
                {
                    if ((a.delta < 0) != (b.delta < 0))
                    {
                        return a.delta < 0 ? 1 : -1;
                    }
                    if (a.delta < 0)
                    {
                        return a.delta < b.delta ? 1 : -1;
                    }
                    else
                    {
                        return a.delta > b.delta ? 1 : -1;
                    }
                }
                return a.name.CompareTo(b.name);
            });
            int selectedIndex = -1;
            if (TimerList.SelectedItem != null)
            {
                string selectedUuid = "";
                selectedUuid = ((DisplayedEvent)TimerList.SelectedItem).uuid;
                selectedIndex = displayedEvents.FindIndex(x => x.uuid == selectedUuid);
            }
            TimerList.ItemsSource = new ObservableCollection<DisplayedEvent>(displayedEvents);
            TimerList.SelectedIndex = selectedIndex;
            if (TimerList.SelectedItems == null || TimerList.SelectedIndex == -1)
            {
                UpdateExportButtonContent(displayedEvents.Count);
            }
        }

        private async void NewEventButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            EventEditor editor = new();
            await editor.ShowDialog(this);
            if (editor.savedEvent != null)
            {
                events.Add(editor.savedEvent);
                eventUuids.Add(editor.savedEvent.uuid);
                TimerList.SelectedIndex = -1;
                RefreshWindow();
                SaveEvents();
            }
        }

        void UpdateExportButtonContent(int items)
        {
            if (items == 0)
            {
                ExportButton.Content = Lang.Resources.ui_export;
                ExportButton.IsEnabled = false;
            }
            else if (items == 1)
            {
                ExportButton.Content = Lang.Resources.ui_export;
                ExportButton.IsEnabled = true;
            }
            else
            {
                ExportButton.Content = string.Format(Lang.Resources.ui_export_number, items);
                ExportButton.IsEnabled = true;
            }
        }

        private void TimerList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (TimerList.SelectedItems == null || TimerList.SelectedIndex == -1)
            {
                DeleteButton.IsEnabled = false;
                EditButton.IsEnabled = false;
                DeleteButton.Content = Lang.Resources.ui_delete;
                UpdateExportButtonContent(displayedEvents.Count);
            }
            else
            {
                DeleteButton.IsEnabled = true;
                if (TimerList.SelectedItems.Count == 1)
                {
                    EditButton.IsEnabled = true;
                    DeleteButton.Content = Lang.Resources.ui_delete;
                    ExportButton.Content = Lang.Resources.ui_export;
                    UpdateExportButtonContent(1);
                }
                else
                {
                    EditButton.IsEnabled = false;
                    DeleteButton.Content = string.Format(Lang.Resources.ui_delete_number, TimerList.SelectedItems.Count);
                    UpdateExportButtonContent(TimerList.SelectedItems.Count);
                }
            }
        }

        private async void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (TimerList.SelectedItems == null)
            {
                return;
            }
            int selectedItemsCount = TimerList.SelectedItems.Count;
            var msgbox = MessageBoxManager.GetMessageBoxStandard(Lang.Resources.ui_delete_warn_title,
                selectedItemsCount == 1 ? Lang.Resources.ui_delete_warn : string.Format(Lang.Resources.ui_delete_warn_multi, selectedItemsCount),
                MsBox.Avalonia.Enums.ButtonEnum.YesNo);
            var result = await msgbox.ShowWindowDialogAsync(this);
            if (result != MsBox.Avalonia.Enums.ButtonResult.Yes)
            {
                return;
            }
            HashSet<string> selectedUuids = [];
            foreach (var item in TimerList.SelectedItems)
            {
                selectedUuids.Add(((DisplayedEvent)item).uuid);
            }
            for (int i = 0; i < events.Count; i++)
            {
                if (selectedUuids.Contains(events[i].uuid))
                {
                    eventUuids.Remove(events[i].uuid);
                    events.Remove(events[i]);
                    i--;
                }
            }
            TimerList.SelectedIndex = -1;
            RefreshWindow();
            SaveEvents();
        }

        private async void EditButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (TimerList.SelectedItem == null)
            {
                return;
            }
            string selectedUuid = ((DisplayedEvent)TimerList.SelectedItem).uuid;
            foreach (Event ev in events)
            {
                if (ev.uuid == selectedUuid)
                {
                    EventEditor editor = new(ev);
                    await editor.ShowDialog(this);
                    if (editor.savedEvent != null)
                    {
                        events.Remove(ev);
                        eventUuids.Remove(ev.uuid);
                        events.Add(editor.savedEvent);
                        eventUuids.Add(editor.savedEvent.uuid);
                        DateTime now = DateTime.Now;
                        RefreshWindow(now);
                        SaveEvents();
                    }
                    break;
                }
            }
        }

        private async void SettingsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new();
            await settingsWindow.ShowDialog(this);
            if (settingsWindow.savedSettings)
            {
                SaveSettings();
                if (settingsWindow.languageModified)
                {
                    var msgbox = MessageBoxManager.GetMessageBoxStandard(Lang.Resources.ui_settings_language_title, Lang.Resources.ui_settings_language,
                        MsBox.Avalonia.Enums.ButtonEnum.Ok);
                    await msgbox.ShowWindowDialogAsync(this);
                }
                if (settingsWindow.fontModified)
                {
                    var msgbox = MessageBoxManager.GetMessageBoxStandard(Lang.Resources.ui_settings_font_title, Lang.Resources.ui_settings_font,
                        MsBox.Avalonia.Enums.ButtonEnum.Ok);
                    await msgbox.ShowWindowDialogAsync(this);
                }
            }
        }

        private void Window_Closing(object? sender, WindowClosingEventArgs e)
        {
            refreshThread.Interrupt();
            refreshThread.Join();
        }

        private void VersionBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            string startUrl = string.Format("https://github.com/{0}", App.githubRepo);
            if (updateFound)
            {
                startUrl = updateChecker.releaseUrl;
            }
            Process.Start(new ProcessStartInfo() { FileName = startUrl, UseShellExecute = true });
        }

        private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            TimerList.SelectedIndex = -1;
            RefreshWindow();
        }

        static FilePickerFileType Dc2eFileType = new("Days Counter 2 Events")
        {
            Patterns = ["*.dc2e"]
        };

        private async void ExportButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            List<Event> exportedEvents = [];
            if (TimerList.SelectedItems == null || TimerList.SelectedIndex == -1)
            {
                foreach (var item in TimerList.Items)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    exportedEvents.Add(((DisplayedEvent)item).correspondEvent);
                }
            }
            else
            {
                foreach (var item in TimerList.SelectedItems)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    exportedEvents.Add(((DisplayedEvent)item).correspondEvent);
                }
            }
            if (exportedEvents.Count == 0)
            {
                return;
            }
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = Lang.Resources.ui_export_to,
                SuggestedFileName = Lang.Resources.ui_export_defaultFilename,
                DefaultExtension = "dc2e",
                FileTypeChoices = [Dc2eFileType]
            });
            if (file != null)
            {
                Stream stream = await file.OpenWriteAsync();
                SaveEvents(exportedEvents, stream);
                file.Dispose();
                var msgbox = MessageBoxManager.GetMessageBoxStandard(Lang.Resources.ui_export_complete_title,
                    string.Format(Lang.Resources.ui_export_complete, exportedEvents.Count));
                await msgbox.ShowWindowDialogAsync(this);
            }
        }

        private async void ImportButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var file = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = Lang.Resources.ui_import_from,
                AllowMultiple = false,
                FileTypeFilter = [Dc2eFileType]
            });
            if (file != null && file.Count > 0)
            {
                Stream stream = await file[0].OpenReadAsync();
                List<Event> newEvents = ReadEvents(stream);
                List<Event> willInsertEvents = [];
                foreach (var ev in newEvents)
                {
                    if (!eventUuids.Contains(ev.uuid))
                    {
                        willInsertEvents.Add(ev);
                    }
                }
                if (willInsertEvents.Count == 0)
                {
                    var msgbox = MessageBoxManager.GetMessageBoxStandard(Lang.Resources.ui_import,
                        string.Format(Lang.Resources.ui_import_wasted, newEvents.Count));
                    await msgbox.ShowWindowDialogAsync(this);
                }
                else
                {
                    var msgbox = MessageBoxManager.GetMessageBoxStandard(Lang.Resources.ui_import,
                        string.Format(Lang.Resources.ui_import_warn, willInsertEvents.Count, newEvents.Count - willInsertEvents.Count), MsBox.Avalonia.Enums.ButtonEnum.OkCancel);
                    var msgboxResult = await msgbox.ShowWindowDialogAsync(this);
                    if (msgboxResult == MsBox.Avalonia.Enums.ButtonResult.Ok)
                    {
                        foreach (var ev in willInsertEvents)
                        {
                            events.Add(ev);
                            eventUuids.Add(ev.uuid);
                        }
                        RefreshWindow();
                        SaveEvents();
                    }
                }
            }
        }
    }
}