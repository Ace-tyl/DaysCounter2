using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using Avalonia.Controls;
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
    }

    public partial class MainWindow : Window
    {
        Thread refreshThread;
        List<Event> events = new List<Event>();
        List<DisplayedEvent> displayedEvents = new List<DisplayedEvent>();
        ObservableCollection<DisplayedEvent> Displayed { get; set; } = new ObservableCollection<DisplayedEvent>();

        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists(App.appFilePath))
            {
                FileStream stream = File.OpenRead(App.appFilePath);
                try
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Event>));
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
        }

        void SaveEvents()
        {
            FileStream stream = File.Open(App.appFilePath, FileMode.Create, FileAccess.Write);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Event>));
            ser.WriteObject(stream, events);
            stream.Close();
        }

        void SaveSettings()
        {
            FileStream stream = File.Open(App.appSettingsPath, FileMode.Create, FileAccess.Write);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SettingsManager));
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

        public void RefreshWindow(DateTime now)
        {
            if (CurrentTimeText.DataContext != null)
            {
                CurrentTimeText.Text = (string)CurrentTimeText.DataContext + now.ToString("yyyy/MM/dd HH:mm:ss");
            }
            MyDateTime myNow = new MyDateTime(now, TimeZoneInfo.Local);
            long myNowJulian = myNow.GetJulianSecond();
            displayedEvents = new List<DisplayedEvent>();
            foreach (Event ev in events)
            {
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
                displayedEvents.Add(new DisplayedEvent
                {
                    uuid = ev.uuid,
                    name = ev.name,
                    delta = delta,
                    timerText = timerText
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
            EventEditor editor = new EventEditor();
            await editor.ShowDialog(this);
            if (editor.savedEvent != null)
            {
                events.Add(editor.savedEvent);
                DateTime now = DateTime.Now;
                RefreshWindow(now);
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
            DateTime now = DateTime.Now;
            RefreshWindow(now);
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
                    EventEditor editor = new EventEditor(ev);
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
            SettingsWindow settingsWindow = new SettingsWindow();
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
    }
}