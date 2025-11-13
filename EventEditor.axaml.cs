using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DaysCounter2.Utils;

namespace DaysCounter2
{
    public partial class EventEditor : Window
    {
        public Event? savedEvent;
        List<TimeZoneData> timeZoneData { get; set; } = TimeZoneManager.timeZoneData;

        void SelectTimeZone(int timeZoneDeltaValue)
        {
            TimeZoneSelector.SelectedIndex = timeZoneData.FindIndex(x => x.delta == timeZoneDeltaValue);
        }

        public EventEditor()
        {
            InitializeComponent();
            TimeZoneSelector.ItemsSource = timeZoneData;
            DateTime now = DateTime.Now;
            YearValue.Value = now.Year;
            MonthValue.Value = now.Month;
            DayValue.Value = now.Day;
            int timeZoneDeltaValue = (int)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes;
            SelectTimeZone(timeZoneDeltaValue);
        }

        public EventEditor(Event ev)
        {
            InitializeComponent();
            TimeZoneSelector.ItemsSource = timeZoneData;
            EventNameValue.Text = ev.name;
            YearValue.Value = ev.dateTime.year;
            MonthValue.Value = ev.dateTime.month;
            DayValue.Value = ev.dateTime.day;
            HourValue.Value = ev.dateTime.hour;
            MinuteValue.Value = ev.dateTime.minute;
            SecondValue.Value = ev.dateTime.second;
            ev.dateTime.InitializeTimeZone();
            if (ev.dateTime.timeZoneDelta == null) { return; } // Impossible
            SelectTimeZone((int)ev.dateTime.timeZoneDelta);
            LoopCheck.IsChecked = (ev.loopType != LoopTypes.None);
            if (ev.loopType != LoopTypes.None)
            {
                LoopType.SelectedIndex = (int)ev.loopType;
                LoopValue.Value = ev.loopValue;
            }
        }

        void ModifyDayLimits()
        {
            if (YearValue.Value == null || MonthValue.Value == null)
            {
                return;
            }
            int year = (int)YearValue.Value;
            int month = (int)MonthValue.Value;
            DayValue.Maximum = MyDateTime.GetDayCountOfMonth(year, month);
        }

        void ModifyDayValue()
        {
            if (DayValue.Value > DayValue.Maximum)
            {
                DayValue.Value = DayValue.Maximum;
            }
            if (YearValue.Value == null || MonthValue.Value == null)
            {
                return;
            }
            // To address October 1582
            int year = (int)YearValue.Value;
            int month = (int)MonthValue.Value;
            if (year == 1582 && month == 10)
            {
                if (DayValue.Value >= 5 && DayValue.Value < 14)
                {
                    DayValue.Value = 15;
                }
                if (DayValue.Value == 14)
                {
                    DayValue.Value = 4;
                }
            }
        }

        MyDateTime? GetInputDateTime()
        {
            if (YearValue.Value == null || MonthValue.Value == null || DayValue.Value == null || HourValue.Value == null || MinuteValue.Value == null || SecondValue.Value == null)
            {
                return null;
            }
            int year = (int)YearValue.Value;
            int month = (int)MonthValue.Value;
            int day = (int)DayValue.Value;
            int hour = (int)HourValue.Value;
            int minute = (int)MinuteValue.Value;
            int second = (int)SecondValue.Value;
            int timeZoneDelta;
            if (TimeZoneSelector.SelectedItem != null)
            {
                timeZoneDelta = ((TimeZoneData)TimeZoneSelector.SelectedItem).delta;
            }
            else
            {
                timeZoneDelta = (int)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes;
            }
            return new MyDateTime(year, month, day, hour, minute, second, timeZoneDelta);
        }

        void ModifySaveButton()
        {
            if (EventNameValue.Text == null || EventNameValue.Text == "")
            {
                SaveButton.IsEnabled = false;
                return;
            }
            MyDateTime? inputDateTime = GetInputDateTime();
            if (inputDateTime == null || !inputDateTime.IsValidData())
            {
                SaveButton.IsEnabled = false;
                return;
            }
            SaveButton.IsEnabled = true;
        }

        private void YearValue_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            if (YearValue.Value == null)
            {
                return;
            }
            ModifyDayLimits();
            ModifyDayValue();
            ModifySaveButton();
            if (YearValue.Value <= 0)
            {
                YearText.Text = Lang.Resources.editor_date_year + string.Format(Lang.Resources.editor_year_bc, 1 - YearValue.Value);
            }
            else
            {
                YearText.Text = Lang.Resources.editor_date_year;
            }
        }

        private void MonthValue_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            ModifyDayLimits();
            ModifyDayValue();
            ModifySaveButton();
        }

        private void DayValue_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            ModifyDayValue();
            ModifySaveButton();
        }

        private void LoopCheck_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (LoopCheck.IsChecked == null) { return; }
            LoopWarn.IsVisible = (bool)LoopCheck.IsChecked;
            LoopValue.IsVisible = (bool)LoopCheck.IsChecked;
            LoopType.IsVisible = (bool)LoopCheck.IsChecked;
        }

        private void EventNameValue_TextChanged(object? sender, TextChangedEventArgs e)
        {
            ModifySaveButton();
        }

        private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MyDateTime? dateTime = GetInputDateTime();
            if (EventNameValue.Text == null || dateTime == null || LoopCheck.IsChecked == null) { return; }
            savedEvent = new Event();
            savedEvent.name = EventNameValue.Text;
            savedEvent.dateTime = dateTime;
            if (LoopCheck.IsChecked == true)
            {
                savedEvent.loopType = (LoopTypes)LoopType.SelectedIndex;
                savedEvent.loopValue = LoopValue.Value == null ? 1 : (int)LoopValue.Value;
            }
            Close();
        }
    }
}
