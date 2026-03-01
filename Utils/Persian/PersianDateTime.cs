using DaysCounter2.Utils.AlHijri;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaysCounter2.Utils.Persian
{
    internal class PersianDateTime
    {
        public int year, month, day, hour, minute, second;
        public int timeZoneDelta;

        public PersianDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int? timeZoneDelta = null)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.timeZoneDelta = (int)(timeZoneDelta ?? TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
        }

        public PersianDateTime Clone()
        {
            return new PersianDateTime(year, month, day, hour, minute, second, timeZoneDelta);
        }

        const double IranianTimeOffset = 3.5 / 24;

        static int JulianYearOffset(int year)
        {
            int yearGregorian = year + 621;
            double vernalEquinox = SolarTerm.AdjustedSolarTerms(yearGregorian, 0, 0)[0] + IranianTimeOffset;
            return (int)Math.Ceiling(vernalEquinox);
        }

        public static int GetDayCountOfYear(int year)
        {
            return JulianYearOffset(year + 1) - JulianYearOffset(year);
        }

        public static int GetDayCountOfMonth(int year, int month)
        {
            if (month <= 6)
            {
                return 31;
            }
            else if (month < 12)
            {
                return 30;
            }
            else
            {
                return GetDayCountOfYear(year) - 336;
            }
        }

        static int GetDayOfYear(int year, int month, int day)
        {
            int[] monthDayCount = [0, 31, 62, 93, 124, 155, 186, 216, 246, 276, 306, 336];
            return monthDayCount[month - 1] + day - 1;
        }

        public bool IsValidData()
        {
            if (year < -5338)
            {
                return false;
            }
            if (month < 1 || month > 12)
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
            if (year < -5338)
            {
                // This function starts at 4717 BC
                year = -5338;
            }
            if (month < 1 || month > 12)
            {
                // Invalid month value
                month = month < 1 ? 1 : 12;
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
            int[] monthDay = [0, 31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 30];
            int month = -1, day = offset;
            for (int i = 1; i <= 12; i++)
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

        public static PersianDateTime FromJulianDay(double jd, int timeZoneDelta = 0)
        {
            double jdn = jd + 0.5 + timeZoneDelta / 1440.0;
            int Z = (int)Math.Floor(jdn);
            double F = jdn - Z;

            int left = Z / 366 - 5336, right = Z / 365 - 5330, offset = 0;
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
            return new PersianDateTime(year, month, day, secondOfDay / 3600, secondOfDay % 3600 / 60, secondOfDay % 60, timeZoneDelta);
        }

        public bool EarlierThan(PersianDateTime another)
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
    }
}
