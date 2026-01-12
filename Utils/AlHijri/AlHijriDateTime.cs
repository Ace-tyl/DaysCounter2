using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaysCounter2.Utils.AlHijri
{
    internal class AlHijriDateTime
    {
        public int year, month, day, hour, minute, second;
        public int timeZoneDelta;

        public AlHijriDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int? timeZoneDelta = null)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.timeZoneDelta = (int)(timeZoneDelta ?? TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
        }

        public AlHijriDateTime Clone()
        {
            return new AlHijriDateTime(year, month, day, hour, minute, second, timeZoneDelta);
        }

        public static bool IsLeapYear(int year)
        {
            const int leapData = 0x252524A4; // leap year data in binary
            int y30 = (year + 30000) % 30; // in order to prevent negative result
            return (leapData & (1 << y30)) != 0;
        }

        public static int GetDayCountOfMonth(int year, int month)
        {
            if (month % 2 == 1)
            {
                return 30;
            }
            else if (month == 12)
            {
                return IsLeapYear(year) ? 30 : 29;
            }
            else
            {
                return 29;
            }
        }

        public bool IsValidData()
        {
            if (year < -5498)
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
            if (year < -5498)
            {
                // This function starts at 4717 BC
                year = -5498;
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

        public static int GetDayCountOfYear(int year)
        {
            return IsLeapYear(year) ? 355 : 354;
        }

        static int GetDayOfYear(int year, int month, int day)
        {
            int[] monthDayCount = [0, 30, 59, 89, 118, 148, 177, 207, 236, 266, 295, 325];
            return monthDayCount[month - 1] + day - 1;
        }

        const int cycleDays = 10631; // Per 30 years
        const int julianOffset = -178115; // Julian day of -6000/1/1 (1 Muharram 6001 BH)

        static int JulianYearOffset(int year)
        {
            int[] cycleOffset = [0, 354, 708, 1063, 1417, 1771, 2126, 2480, 2835, 3189,
                3543, 3898, 4252, 4606, 4961, 5315, 5669, 6024, 6378, 6733,
                7087, 7441, 7796, 8150, 8504, 8859, 9213, 9568, 9922, 10276];
            int yearOffset = year + 6000;
            return julianOffset + cycleDays * (yearOffset / 30) + cycleOffset[yearOffset % 30];
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
            return JulianDay(year, month, day) * 86400L + (hour - 12) * 3600 + (minute - (int)timeZoneDelta) * 60 + second;
        }

        public double GetJulianDay()
        {
            return (GetJulianSecond() + 0.5) / 86400.0;
        }

        static Tuple<int, int> MonthDayFromOffset(int year, int offset)
        {
            int[] monthDay = [0, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29];
            if (IsLeapYear(year))
            {
                monthDay[12]++;
            }
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

        public static AlHijriDateTime FromJulianDay(double jd, int timeZoneDelta = 0)
        {
            double jdn = jd + 0.5 + timeZoneDelta / 1440.0;
            int Z = (int)Math.Floor(jdn);
            double F = jdn - Z;

            int left = (Z - julianOffset) / 355 - 6001, right = (Z - julianOffset) / 354 - 5998, offset = 0;
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
            return new AlHijriDateTime(year, month, day, secondOfDay / 3600, secondOfDay % 3600 / 60, secondOfDay % 60, timeZoneDelta);
        }

        public bool EarlierThan(AlHijriDateTime another)
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
