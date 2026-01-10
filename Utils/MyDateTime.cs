using System;

namespace DaysCounter2.Utils
{
    public class MyDateTime
    {
        public int year, month, day, hour, minute, second;
        public int? timeZoneDelta;

        public MyDateTime() { }

        public MyDateTime(DateTime dateTime, TimeZoneInfo timeZone)
        {
            year = dateTime.Year;
            month = dateTime.Month;
            day = dateTime.Day;
            hour = dateTime.Hour;
            minute = dateTime.Minute;
            second = dateTime.Second;
            timeZoneDelta = (int)timeZone.BaseUtcOffset.TotalMinutes;
        }

        public MyDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int? timeZoneDelta = null)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.timeZoneDelta = (int)(timeZoneDelta ?? TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
        }

        public MyDateTime Clone()
        {
            // Used for deep copy
            return new MyDateTime(year, month, day, hour, minute, second, timeZoneDelta);
        }

        public static bool IsLeapYear(int year)
        {
            if (year <= 1582)
            {
                return year % 4 == 0;
            }
            else
            {
                return year % 4 == 0 && year % 100 != 0 || year % 400 == 0;
            }
        }

        public static int GetDayCountOfMonth(int year, int month)
        {
            if (month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12)
            {
                return 31;
            }
            else if (month == 4 || month == 6 || month == 9 || month == 11)
            {
                return 30;
            }
            else if (month == 2)
            {
                return IsLeapYear(year) ? 29 : 28;
            }
            else
            {
                throw new IndexOutOfRangeException("Month should be between 1 and 12");
            }
        }

        public bool IsValidData()
        {
            if (year < -4716)
            {
                // This function starts at 4717 BC
                return false;
            }
            if (month < 1 || month > 12)
            {
                // Invalid month value
                return false;
            }
            if (year == 1582 && month == 10 && day >= 5 && day <= 14)
            {
                // Dates from 1582/10/5 to 1582/10/14 doesn't exist in history
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
            if (year < -4716)
            {
                // This function starts at 4717 BC
                year = -4716;
            }
            if (month < 1 || month > 12)
            {
                // Invalid month value
                month = month < 1 ? 1 : 12;
            }
            if (year == 1582 && month == 10 && day >= 5 && day <= 14)
            {
                // Dates from 1582/10/5 to 1582/10/14 doesn't exist in history
                day = day < 14 ? 4 : 15;
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
            if (IsLeapYear(year))
            {
                return 366;
            }
            else if (year == 1582)
            {
                // Days from 1582/10/5 to 1582/10/14 are removed
                return 355;
            }
            else
            {
                return 365;
            }
        }

        /*
         * Get the number of day in the specific year (start from 0)
         */
        static int GetDayOfYear(int year, int month, int day)
        {
            int[] monthDayCount = [0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334];
            if (month < 3)
            {
                return monthDayCount[month - 1] + day - 1;
            }
            else if (year == 1582 && (month > 10 || month == 10 && day > 4))
            {
                return monthDayCount[month - 1] + day - 11;
            }
            else
            {
                return monthDayCount[month - 1] + (IsLeapYear(year) ? 1 : 0) + day - 1;
            }
        }

        /*
         * Get the julian day of the first day of the specific year
         */
        static int JulianYearOffset(int year)
        {
            if (year <= 1582)
            {
                int daysPer4Year = 1461;
                int absoluteYear = year + 4716;
                int baseDays = daysPer4Year * (absoluteYear / 4 - 1);
                int remainYear = absoluteYear % 4;
                if (remainYear == 0)
                {
                    return baseDays;
                }
                else
                {
                    return baseDays + 365 * remainYear + 1;
                }
            }
            else
            {
                int offsetYear1583 = 2299239;
                int yearCount = year - 1583;
                int year4Count = (year - 1) / 4 - 395;
                int year100Count = (year - 1) / 100 - 15;
                int year400Count = (year - 1) / 400 - 3;
                return offsetYear1583 + yearCount * 365 + year4Count - year100Count + year400Count;
            }
        }

        static int JulianDay(int year, int month, int day)
        {
            return JulianYearOffset(year) + GetDayOfYear(year, month, day);
        }

        public void InitializeTimeZone()
        {
            if (timeZoneDelta == null)
            {
                timeZoneDelta = (int)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes;
            }
        }

        public long GetJulianSecond()
        {
            if (!IsValidData())
            {
                throw new IndexOutOfRangeException("Invalid datetime");
            }
            if (timeZoneDelta == null)
            {
                timeZoneDelta = (int)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes;
            }
            return JulianDay(year, month, day) * 86400L + (hour - 12) * 3600 + (minute - (int)timeZoneDelta) * 60 + second;
        }

        public double GetJulianDay()
        {
            return GetJulianSecond() / 86400.0;
        }

        static Tuple<int, int> MonthDayFromOffset(int year, int offset)
        {
            int[] monthDay = [0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
            if (IsLeapYear(year))
            {
                monthDay[2]++;
            }
            if (year == 1582)
            {
                monthDay[10] -= 10;
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
            if (year == 1582 && month == 10 && day > 4)
            {
                day += 10;
            }
            return new(month, day);
        }

        // Convert from Julian Day to Date Time
        public static MyDateTime FromJulianDay(double jd, int timeZoneDelta = 0)
        {
            double jdn = jd + 0.5 + timeZoneDelta / 1440.0;
            int Z = (int)Math.Floor(jdn);
            double F = jdn - Z;

            int left = Z / 366 - 4714, right = Z / 365 - 4710, offset = 0;
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
            return new MyDateTime(year, month, day, secondOfDay / 3600, secondOfDay % 3600 / 60, secondOfDay % 60, timeZoneDelta);
        }

        public bool EarlierThan(MyDateTime another)
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

        public DateTime ToDateTime()
        {
            return new DateTime(year, month, day, hour, minute, second);
        }
    }
}
