using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using DaysCounter2.Utils;
using MsBox.Avalonia;

namespace DaysCounter2
{
    public class DisplayedEvent
    {
        public string uuid { get; set; } = "";
        public string name { get; set; } = "";
        public long delta { get; set; }
        public string timerText { get; set; } = "";
        public IBrush brush { get; set; } = new SolidColorBrush();
    }

    public partial class MainWindow : Window
    {
        Thread refreshThread;
        List<Event> events = [];
        List<DisplayedEvent> displayedEvents = [];
        ObservableCollection<DisplayedEvent> Displayed { get; set; } = [];
        string languageId;

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

        public void RefreshWindow(DateTime now)
        {
            try
            {
                CurrentTimeText.Text = Lang.Resources.ui_currentTime + now.ToString(App.settings.dateTimeFormat, CultureInfo.CreateSpecificCulture(languageId));
            }
            catch (FormatException)
            {
                App.settings.dateTimeFormat = "yyyy/MM/dd HH:mm:ss";
                CurrentTimeText.Text = Lang.Resources.ui_currentTime + now.ToString(App.settings.dateTimeFormat, CultureInfo.CreateSpecificCulture(languageId));
            }
            MyDateTime myNow = new(now, TimeZoneInfo.Local);
            long myNowJulian = myNow.GetJulianSecond();
            displayedEvents = [];
            string searchText = SearchBox.Text ?? "";
            foreach (Event ev in events)
            {
                if (!ev.name.ToLower().Contains(searchText.ToLower()))
                {
                    continue;
                }
                long delta = ev.GetDelta(myNow, myNowJulian);
                string timerText = "";
                if (delta > 0)
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
                displayedEvents.Add(new DisplayedEvent
                {
                    uuid = ev.uuid,
                    name = ev.name,
                    delta = delta,
                    timerText = timerText,
                    brush = new SolidColorBrush(ColorSelector.Interpolate(delta > 0 ? App.settings.futureColor : App.settings.pastColor, App.settings.distantColor, Math.Log(days, 1000)))
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
                RefreshWindow();
                SaveEvents();
            }
        }

        private void TimerList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = (TimerList.SelectedIndex != -1);
            EditButton.IsEnabled = (TimerList.SelectedIndex != -1);
        }

        private async void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (TimerList.SelectedItem == null)
            {
                return;
            }
            var msgbox = MessageBoxManager.GetMessageBoxStandard(Lang.Resources.ui_delete_warn_title, Lang.Resources.ui_delete_warn, MsBox.Avalonia.Enums.ButtonEnum.YesNo);
            var result = await msgbox.ShowWindowDialogAsync(this);
            if (result != MsBox.Avalonia.Enums.ButtonResult.Yes)
            {
                return;
            }
            string selectedUuid = ((DisplayedEvent)TimerList.SelectedItem).uuid;
            foreach (Event ev in events)
            {
                if (ev.uuid == selectedUuid)
                {
                    events.Remove(ev);
                    break;
                }
            }
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
                    var msgbox = MessageBoxManager.GetMessageBoxStandard(Lang.Resources.ui_settings_language_title, Lang.Resources.ui_settings_language, MsBox.Avalonia.Enums.ButtonEnum.Ok);
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