using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaysCounter2.Utils.Hebrew
{
    internal class HebrewDateTime
    {
        public int year, month, day, hour, minute, second;
        public int timeZoneDelta;

        public HebrewDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int? timeZoneDelta = null)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.timeZoneDelta = (int)(timeZoneDelta ?? TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
        }

        public HebrewDateTime Clone()
        {
            return new HebrewDateTime(year, month, day, hour, minute, second, timeZoneDelta);
        }

        public static bool IsLeapYear(int year)
        {
            return HebrewCalendarHelper.IsLeapYear(year);
        }

        static int[] GetDayCountArray(int year)
        {
            YearPattern pattern = HebrewCalendarHelper.GetYearPattern(year);
            int[] dayCount;
            if (((int)pattern & 0x010) != 0)
            {
                dayCount = [0, 30, 29, 30, 29, 30, 30, 29, 30, 29, 30, 29, 30, 29];
            }
            else
            {
                dayCount = [0, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29];
            }
            if (((int)pattern & 0x00F) == 0)
            {
                dayCount[3]--;
            }
            if (((int)pattern & 0x00F) == 2)
            {
                dayCount[2]++;
            }
            return dayCount;
        }

        public static int GetDayCountOfMonth(int year, int month)
        {
            int[] dayCount = GetDayCountArray(year);
            return dayCount[month];
        }

        public bool IsValidData()
        {
            if (year < -952)
            {
                return false;
            }
            if (month < 1 || month > (IsLeapYear(year) ? 13 : 12))
            {
                // Invalid month value
                return false;
            }
            if (day < 1 || day > GetDayCountOfMonth(year, month))
            {
                // Invalid day value
                return false;
            }
            if (hour < 0 || hour > 23)
            {
                // Invalid hour value
                return false;
            }
            if (minute < 0 || minute > 59)
            {
                // Invalid minute value
                return false;
            }
            if (second < 0 || second > 59)
            {
                // Invalid second value
                return false;
            }
            return true;
        }

        public void AdjustData()
        {
            if (year < -952)
            {
                year = -952;
            }
            if (month < 1 || month > (IsLeapYear(year) ? 13 : 12))
            {
                // Invalid month value
                month = month < 1 ? 1 : IsLeapYear(year) ? 13 : 12;
            }
            if (day < 1 || day > GetDayCountOfMonth(year, month))
            {
                // Invalid day value
                day = day < 1 ? 1 : GetDayCountOfMonth(year, month);
            }
            if (hour < 0 || hour > 23)
            {
                // Invalid hour value
                hour = hour < 0 ? 0 : 23;
            }
            if (minute < 0 || minute > 59)
            {
                // Invalid minute value
                minute = minute < 0 ? 0 : 59;
            }
            if (second < 0 || second > 59)
            {
                // Invalid second value
                second = second < 0 ? 0 : 59;
            }
        }

        public static int GetDayCountOfYear(int year)
        {
            YearPattern pattern = HebrewCalendarHelper.GetYearPattern(year);
            return (((int)pattern & 0x010) == 0x010 ? 353 : 383) + ((int)pattern & 0x00F);
        }

        static int GetDayOfYear(int year, int month, int day)
        {
            int[] dayCount = GetDayCountArray(year);
            int count = 0;
            for (int i = 1; i < month; i++)
            {
                count += dayCount[i];
            }
            return count + day - 1;
        }

        static int JulianYearOffset(int year)
        {
            return HebrewCalendarHelper.GetFirstDayOfYear(year);
        }

        static int JulianDay(int year, int month, int day)
        {
            return JulianYearOffset(year) + GetDayOfYear(year, month, day);
        }

        public long GetJulianSecond()
        {
            if (!IsValidData())
            {
                throw new IndexOutOfRangeException("Invalid datetime");
            }
            return JulianDay(year, month, day) * 86400L + (hour - 12) * 3600 + (minute - timeZoneDelta) * 60 + second;
        }

        public double GetJulianDay()
        {
            return (GetJulianSecond() + 0.5) / 86400.0;
        }

        static Tuple<int, int> MonthDayFromOffset(int year, int offset)
        {
            int[] monthDay = GetDayCountArray(year);
            int month = -1, day = offset;
            for (int i = 1; i < monthDay.Length; i++)
            {
                if (day < monthDay[i])
                {
                    month = i;
                    break;
                }
                day -= monthDay[i];
            }
            if (month < 0 || day < 0)
            {
                throw new IndexOutOfRangeException("Invalid day offset");
            }
            day++;
            return new(month, day);
        }

        public static HebrewDateTime FromJulianDay(double jd, int timeZoneDelta = 0)
        {
            double jdn = jd + 0.5 + timeZoneDelta / 1440.0;
            int Z = (int)Math.Floor(jdn);
            double F = jdn - Z;

            int left = (Z - HebrewCalendarHelper.JulianDayOffset) / 385 - 1000, right = (Z - HebrewCalendarHelper.JulianDayOffset) / 353 + 1000, offset = 0;
            while (right - left > 1)
            {
                int mid = (left + right) / 2;
                int temp_offset = Z - JulianYearOffset(mid);
                if (temp_offset >= 0)
                {
                    left = mid;
                    offset = temp_offset;
                }
                else
                {
                    right = mid;
                }
            }
            int year = left;
            var monthDay = MonthDayFromOffset(year, offset);
            int month = monthDay.Item1, day = monthDay.Item2;

            int secondOfDay = (int)Math.Floor(F * 86400);
            return new HebrewDateTime(year, month, day, secondOfDay / 3600, secondOfDay % 3600 / 60, secondOfDay % 60, timeZoneDelta);
        }

        public bool EarlierThan(HebrewDateTime another)
        {
            if (timeZoneDelta != another.timeZoneDelta)
            {
                return GetJulianSecond() < another.GetJulianSecond();
            }
            if (year != another.year)
            {
                return year < another.year;
            }
            if (month != another.month)
            {
                return month < another.month;
            }
            if (day != another.day)
            {
                return day < another.day;
            }
            if (hour != another.hour)
            {
                return hour < another.hour;
            }
            if (minute != another.minute)
            {
                return minute < another.minute;
            }
            return second < another.second;
        }

        public static Tuple<int, int> NextMonth(int year, int month)
        {
            if (month < (IsLeapYear(year) ? 13 : 12))
            {
                return new Tuple<int, int>(year, month + 1);
            }
            else
            {
                return new Tuple<int, int>(year + 1, month);
            }
        }
    }
}
