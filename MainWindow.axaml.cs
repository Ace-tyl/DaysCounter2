using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using DaysCounter2.Utils;
using DaysCounter2.Utils.AlHijri;
using MsBox.Avalonia;

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
    }

    public partial class MainWindow : Window
    {
        Thread refreshThread;
        List<Event> events = [];
        List<DisplayedEvent> displayedEvents = [];
        ObservableCollection<DisplayedEvent> Displayed { get; set; } = [];
        string languageId;
        CultureInfo culture;
        static CultureInfo ArabicCulture = CultureInfo.CreateSpecificCulture("ar-SA");

        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists(App.appFilePath))
            {
                FileStream stream = File.OpenRead(App.appFilePath);
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
            }
            refreshThread = new Thread(RefreshTimer);
            refreshThread.Start();
            languageId = App.settings.languageId;
            culture = CultureInfo.CreateSpecificCulture(languageId);
            var versionAttribute = (AssemblyFileVersionAttribute?)Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyFileVersionAttribute));
            if (versionAttribute != null)
            {
                VersionText.Text += Lang.Resources.ui_version + "\n" + versionAttribute.Version;
            }
        }

        void SaveEvents()
        {
            FileStream stream = File.Open(App.appFilePath, FileMode.Create, FileAccess.Write);
            DataContractJsonSerializer ser = new(typeof(List<Event>));
            ser.WriteObject(stream, events);
            stream.Close();
        }

        void SaveSettings()
        {
            FileStream stream = File.Open(App.appSettingsPath, FileMode.Create, FileAccess.Write);
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
                days = Math.Clamp(days, 1, 1000);
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
                // Construct background brush
                SolidColorBrush brush;
                if (App.settings.backgroundGradientMode == (byte)BackgroundGradientModes.Logarithm)
                {
                    brush = new SolidColorBrush(
                        ColorSelector.Interpolate(delta > 0 ? App.settings.futureColor : App.settings.pastColor, App.settings.distantColor,
                        Math.Log(days / App.settings.backgroundGradientLow, App.settings.backgroundGradientHigh / App.settings.backgroundGradientLow)));
                }
                else if (App.settings.backgroundGradientMode == (byte)BackgroundGradientModes.Linear)
                {
                    brush = new SolidColorBrush(
                        ColorSelector.Interpolate(delta > 0 ? App.settings.futureColor : App.settings.pastColor, App.settings.distantColor,
                        (days - App.settings.backgroundGradientLow) / (App.settings.backgroundGradientHigh - App.settings.backgroundGradientLow)));
                }
                else
                {
                    brush = new SolidColorBrush(delta > 0 ? App.settings.futureColor : App.settings.pastColor);
                }
                // Add displayed event
                displayedEvents.Add(new DisplayedEvent
                {
                    uuid = ev.uuid,
                    name = ev.name,
                    delta = delta,
                    timerText = timerText,
                    destinationText = destinationText,
                    brush = brush,
                    nameInlines = runs
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
        }

        private async void NewEventButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            EventEditor editor = new();
            await editor.ShowDialog(this);
            if (editor.savedEvent != null)
            {
                events.Add(editor.savedEvent);
                TimerList.SelectedIndex = -1;
                RefreshWindow();
                SaveEvents();
            }
        }

        private void TimerList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = (TimerList.SelectedIndex != -1);
            EditButton.IsEnabled = TimerList.SelectedItems == null ? false : (TimerList.SelectedIndex != -1 && TimerList.SelectedItems.Count == 1);
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
                        events.Add(editor.savedEvent);
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
            Process.Start(new ProcessStartInfo() { FileName = "https://github.com/Ace-tyl/DaysCounter2", UseShellExecute = true });
        }

        private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            RefreshWindow();
        }
    }
}