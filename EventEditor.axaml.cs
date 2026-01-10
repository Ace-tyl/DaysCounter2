using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using DaysCounter2.Utils;
using DaysCounter2.Utils.ChineseLunisolar;

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
            CalendarSelector.SelectedIndex = ev.calendar;
            if (ev.calendar == 1)
            {
                LunisolarDateTime lunar = LunisolarDateTime.FromGregorian(ev.dateTime, ev.dateTime.GetJulianDay());
                YearValue.Value = lunar.year;
                MonthValue.Value = lunar.month;
                DayValue.Value = lunar.day;
                HourValue.Value = lunar.hour;
                MinuteValue.Value = lunar.minute;
                SecondValue.Value = lunar.second;
            }
            else
            {
                YearValue.Value = ev.dateTime.year;
                MonthValue.Value = ev.dateTime.month;
                DayValue.Value = ev.dateTime.day;
                HourValue.Value = ev.dateTime.hour;
                MinuteValue.Value = ev.dateTime.minute;
                SecondValue.Value = ev.dateTime.second;
            }
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
            if (CalendarSelector.SelectedIndex == 1)
            {
                if (year == -4713 && month == 10)
                {
                    DayValue.Minimum = 22;
                }
                else
                {
                    DayValue.Minimum = 1;
                }
                if (year == 9999 && month == 12)
                {
                    DayValue.Maximum = 3;
                }
                else
                {
                    DayValue.Maximum = LunisolarDateTime.GetDayCountOfMonth(year, month);
                }
            }
            else
            {
                DayValue.Minimum = 1;
                DayValue.Maximum = MyDateTime.GetDayCountOfMonth(year, month);
            }
        }

        void ModifyDayValue()
        {
            if (DayValue.Value > DayValue.Maximum)
            {
                DayValue.Value = DayValue.Maximum;
            }
            if (DayValue.Value < DayValue.Minimum)
            {
                DayValue.Value = DayValue.Minimum;
            }
            if (YearValue.Value == null || MonthValue.Value == null)
            {
                return;
            }
            // To address October 1582
            int year = (int)YearValue.Value;
            int month = (int)MonthValue.Value;
            if (CalendarSelector.SelectedIndex == 0 && year == 1582 && month == 10)
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

        void ModifyMonthLimits()
        {
            if (YearValue.Value == null)
            {
                return;
            }
            int year = (int)YearValue.Value;
            if (CalendarSelector.SelectedIndex == 1)
            {
                if (year == -4713)
                {
                    MonthValue.Minimum = 10;
                }
                else if (LunisolarDateTime.GetLeapMonth(year) != 0)
                {
                    MonthValue.Minimum = 0;
                }
                else
                {
                    MonthValue.Minimum = 1;
                }
                MonthValue.Maximum = 12;
            }
            else
            {
                MonthValue.Minimum = 1;
                MonthValue.Maximum = 12;
            }
        }

        void ModifyMonthValue()
        {
            if (MonthValue.Value < MonthValue.Minimum)
            {
                MonthValue.Value = MonthValue.Minimum;
            }
            if (MonthValue.Value > MonthValue.Maximum)
            {
                MonthValue.Value = MonthValue.Maximum;
            }
        }

        void ModifyYearLimits()
        {
            if (CalendarSelector.SelectedIndex == 1)
            {
                YearValue.Minimum = -4713;
                YearValue.Maximum = 9999;
            }
            else
            {
                YearValue.Minimum = -4712;
                YearValue.Maximum = 9999;
            }
        }

        void ModifyYearValue()
        {
            if (YearValue.Value < YearValue.Minimum)
            {
                YearValue.Value = YearValue.Minimum;
            }
            if (YearValue.Value > YearValue.Maximum)
            {
                YearValue.Value = YearValue.Maximum;
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
            if (CalendarSelector.SelectedIndex == 1)
            {
                double JulianDay = new LunisolarDateTime(year, month, day, hour, minute, second, timeZoneDelta).GetJulianDay();
                return MyDateTime.FromJulianDay(JulianDay, timeZoneDelta);
            }
            else
            {
                return new MyDateTime(year, month, day, hour, minute, second, timeZoneDelta);
            }
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
            ModifyMonthLimits();
            ModifyMonthValue();
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
            if (CalendarSelector.SelectedIndex == 1 && MonthValue.Value == 0 && YearValue.Value != null)
            {
                int year = (int)YearValue.Value;
                int leap = LunisolarDateTime.GetLeapMonth(year);
                MonthText.Text = Lang.Resources.editor_date_month + string.Format(Lang.Resources.editor_month_leap, leap);
            }
            else
            {
                MonthText.Text = Lang.Resources.editor_date_month;
            }
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
            savedEvent = new Event
            {
                name = EventNameValue.Text,
                dateTime = dateTime,
                calendar = CalendarSelector.SelectedIndex
            };
            if (LoopCheck.IsChecked == true)
            {
                savedEvent.loopType = (LoopTypes)LoopType.SelectedIndex;
                savedEvent.loopValue = LoopValue.Value == null ? 1 : (int)LoopValue.Value;
            }
            Close();
        }

        int lastSelectedIndex = 0;

        private void CalendarSelector_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (YearValue == null || MonthValue == null || DayValue == null || HourValue == null || MinuteValue == null || SecondValue == null)
            {
                return;
            }
            if (YearValue.Value == null || MonthValue.Value == null || DayValue.Value == null || HourValue.Value == null || MinuteValue.Value == null || SecondValue.Value == null)
            {
                return;
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
            double julian;
            MyDateTime gregorian;
            if (lastSelectedIndex == 0)
            {
                gregorian = new MyDateTime(year, month, day, hour, minute, second, timeZoneDelta);
                julian = gregorian.GetJulianDay();
            }
            else if (lastSelectedIndex == 1)
            {
                julian = new LunisolarDateTime(year, month, day, hour, minute, second, timeZoneDelta).GetJulianDay();
                gregorian = MyDateTime.FromJulianDay(julian, timeZoneDelta);
            }
            else
            {
                gregorian = new MyDateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                julian = gregorian.GetJulianDay();
            }
            ModifyYearLimits();
            ModifyYearValue();
            ModifyMonthLimits();
            ModifyMonthValue();
            ModifyDayLimits();
            ModifyDayValue();
            int newSelectedIndex = CalendarSelector.SelectedIndex;
            if (newSelectedIndex == 0)
            {
                YearValue.Value = gregorian.year;
                MonthValue.Value = gregorian.month;
                DayValue.Value = gregorian.day;
                HourValue.Value = gregorian.hour;
                MinuteValue.Value = gregorian.minute;
                SecondValue.Value = gregorian.second;
            }
            else if (newSelectedIndex == 1)
            {
                LunisolarDateTime lunar = LunisolarDateTime.FromGregorian(gregorian, julian);
                YearValue.Value = lunar.year;
                MonthValue.Value = lunar.month > 100 ? 0 : lunar.month;
                DayValue.Value = lunar.day;
                HourValue.Value = lunar.hour;
                MinuteValue.Value = lunar.minute;
                SecondValue.Value = lunar.second;
            }
            lastSelectedIndex = newSelectedIndex;
        }
    }
}
