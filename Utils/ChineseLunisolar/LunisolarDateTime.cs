using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaysCounter2.Utils.ChineseLunisolar
{
    internal class LunisolarDateTime
    {
        // Month value: 1~12 indicats ordinary month, 101~112 indicates leap month
        public int year, month, day, hour, minute, second;
        public int timeZoneDelta;

        public LunisolarDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int? timeZoneDelta = null)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            if (this.month == 0)
            {
                this.month = 100 + GetLeapMonth(year);
            }
            this.timeZoneDelta = (int)(timeZoneDelta ?? TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
        }

        // Chinese Time Zone (UTC + 8)
        const double ChineseTimeOffset = 8.0 / 24;

        // Zhongqi since previous Winter Solstice
        static Dictionary<int, double[]> zhongqiCache = [];

        // Get 17 zhongqis from Winter Solstice
        static double[] ZhongqiSinceWinterSolstice(int year)
        {
            if (zhongqiCache.ContainsKey(year))
            {
                return zhongqiCache[year];
            }

            double[] zhongqi = new double[17];
            var lastYearSt = SolarTerm.LastYearSolarTerms(year);

            // 3 items: Winter Solstice, Great Cold, Rain Water
            for (int i = 18; i < 24; i += 2)
            {
                zhongqi[(i - 18) / 2] = lastYearSt[i] + ChineseTimeOffset;
            }

            var st = SolarTerm.AdjustedSolarTerms(year);

            // 13 items
            for (int i = 0; i < 28; i += 2)
            {
                zhongqi[i / 2 + 3] = st[i] + ChineseTimeOffset;
            }

            zhongqiCache.Add(year, zhongqi);
            return zhongqi;
        }

        // Get 17 new moon points from previous 11th month
        static double[] NewMoonPointsSinceMonth11(int year, double winterSolstice)
        {
            double[] newMoonPoints = new double[21];
            double november = new MyDateTime(year - 1, 11, 1).GetJulianDay(); // Previous November
            int kn = Astronomy.ReferenceLunarMonthNumber(november); // Index of new moon point before previous November

            // Get 21 new moon points
            for (int i = 0; i < 21; i++)
            {
                int k = kn + i;
                newMoonPoints[i] = Astronomy.TrueNewMoon(k) + ChineseTimeOffset;
                newMoonPoints[i] -= Astronomy.DeltaTDays(year, i - 1);
            }

            int begin = 0;
            for (int i = 0; i < 20; i++)
            {
                if (Math.Floor(newMoonPoints[i] + 0.5) > Math.Floor(winterSolstice + 0.5))
                {
                    // The first new moon point after Winter Solstice
                    begin = i;
                    break;
                }
            }

            double[] result = new double[17];
            for (int i = 0; i < 17; i++)
            {
                result[i] = newMoonPoints[begin - 1 + i];
            }
            return result;
        }

        // Find leap month in codes since previous month 11
        // Returns month index + 2 (1 and 2 indicates previous month 11, 12)
        // Returns 0 if no leap month found
        static int LeapMonthPos(int[] monthCodes)
        {
            int leap = 0;
            for (int i = 1; i <= 15; i++)
            {
                if (monthCodes[i] > 100)
                {
                    leap = monthCodes[i] - 100;
                    break;
                }
            }
            return leap;
        }

        public static int GetLeapMonth(int year)
        {
            int[] monthCodes = LunarMonthInfo(year).Item3;
            int pos = LeapMonthPos(monthCodes);
            if (pos >= 3)
            {
                return pos - 2;
            }
            else
            {
                return 0;
            }
        }

        public static Tuple<int, int> NextMonth(int year, int month)
        {
            int leap = GetLeapMonth(year);
            int nmonth = month + 1;
            if (month > 100)
            {
                nmonth = month - 100 + 1;
            }
            if (leap == month)
            {
                nmonth = month + 100;
            }
            if (nmonth == 13)
            {
                year++;
                nmonth = 1;
            }
            return new(year, nmonth);
        }

        // Lunar Month Information
        // Item1: 17 Zhongqis from previous Winter Solstice
        // Item2: 17 New Moon Points from previous 11th month
        // Item3: Moon codes, larger than 100 indecates leap month
        static Tuple<double[], double[], int[]> LunarMonthInfo(int year)
        {
            int[] monthCodes = new int[17];
            double[] zhongqis = ZhongqiSinceWinterSolstice(year);
            double[] newMoons = NewMoonPointsSinceMonth11(year, zhongqis[0]);

            int flag = 0; // Have met leap month?
            if (Math.Floor(zhongqis[12] + 0.5) >= Math.Floor(newMoons[13] + 0.5))
            {
                // The 13th zhongqi is later than the 14th new moon
                // A leap month is required
                // The first month without zhongqi is leap month
                monthCodes[0] = 1;
                for (int i = 1; i <= 14; i++)
                {
                    // First month without zhongqi
                    if (Math.Floor(newMoons[i] + 0.5) > Math.Floor(zhongqis[i - 1 - flag] + 0.5)
                        && Math.Floor(newMoons[i + 1] + 0.5) <= Math.Floor(zhongqis[i - flag] + 0.5))
                    {
                        monthCodes[i] = i + 100;
                        flag = 1;
                    }
                    else
                    {
                        monthCodes[i] = i - flag + 1;
                    }
                }
            }
            else
            {
                // No leap month until month 11
                for (int i = 0; i <= 12; i++)
                {
                    monthCodes[i] = i + 1;
                }
                // Leap month may occur after month 11
                for (int i = 13; i <= 14; i++)
                {
                    if (Math.Floor(newMoons[i] + 0.5) > Math.Floor(zhongqis[i - 1 - flag] + 0.5)
                        && Math.Floor(newMoons[i + 1] + 0.5) <= Math.Floor(zhongqis[i - flag] + 0.5))
                    {
                        monthCodes[i] = i + 100;
                        flag = 1;
                    }
                    else
                    {
                        monthCodes[i] = i - flag + 1;
                    }
                }
            }
            return new(zhongqis, newMoons, monthCodes);
        }

        public static int GetDayCountOfMonth(int year, int month, double[]? _newMoons = null, int? _leap = null)
        {
            if (month < 0 || (month > 12 && month <= 100) || month > 112)
            {
                throw new IndexOutOfRangeException("Month should be between 1 and 12 or between 101 and 112");
            }

            double[] newMoons;
            int leap;
            if (_newMoons == null || _leap == null)
            {
                var lunarMonthInfo = LunarMonthInfo(year);
                newMoons = lunarMonthInfo.Item2;
                int[] monthCodes = lunarMonthInfo.Item3;
                leap = LeapMonthPos(monthCodes);
            }
            else
            {
                newMoons = _newMoons;
                leap = (int)_leap;
            }

            if (month > 100 && leap != month - 98)
            {
                throw new IndexOutOfRangeException("Invalid leap month position");
            }

            int nmonth = month + 2;
            if (leap != 0 && nmonth > leap)
            {
                nmonth++;
            }
            if (nmonth > 100)
            {
                nmonth -= 100;
            }
            return (int)(Math.Floor(newMoons[nmonth + 1] + 0.5) - Math.Floor(newMoons[nmonth] + 0.5));
        }

        public bool IsValidData(double[]? _newMoons = null, int? _leap = null)
        {
            if (year < -4713 || (year == -4713 && month < 10) || (year == -4713 && month == 10 && day < 22))
            {
                // -4712/1/1 is -4713/10/22 in lunisolar calendar
                return false;
            }
            try
            {
                if (day < 1 || day > GetDayCountOfMonth(year, month, _newMoons, _leap))
                {
                    // Invalid day value
                    return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                // Invalid month value
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

        // Convert from LunisolarDateTime to Julian Day
        public double GetJulianDay()
        {
            var lunarMonthInfo = LunarMonthInfo(year);
            double[] newMoons = lunarMonthInfo.Item2;
            int[] monthCodes = lunarMonthInfo.Item3;
            int leap = LeapMonthPos(monthCodes);

            if (!IsValidData(newMoons, leap))
            {
                throw new IndexOutOfRangeException("Invalid datetime");
            }

            int nmonth = month + 2;

            double jd = 0;
            if (month > 100)
            {
                // Since validity check is performed
                // The month is a valid leap month
                jd = newMoons[nmonth - 100] + day - 1;
            }
            else
            {
                if (leap == 0)
                {
                    jd = newMoons[nmonth - 1] + day - 1;
                }
                else
                {
                    jd = newMoons[nmonth + (nmonth > leap ? 1 : 0) - 1] + day - 1;
                }
            }

            // Convert to time at 0:00:00
            jd = Math.Floor(jd + 0.5);
            jd += ((hour - 12) * 3600 + minute * 60 + second + 0.5) / 86400.0;
            // Previous data is calculated based on UTC+8
            // Time Zone Convert
            jd -= timeZoneDelta / 1440.0;
            return jd;
        }

        // Convert from Julian Day to LunisolarDateTime
        public static LunisolarDateTime FromGregorian(MyDateTime g, double jd)
        {
            int g_timeZoneDelta = (int)(g.timeZoneDelta ?? TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
            var lunarMonthInfo = LunarMonthInfo(g.year);
            double[] newMoons = lunarMonthInfo.Item2;
            int[] monthCodes = lunarMonthInfo.Item3;
            int njd = (int)Math.Floor(jd + ChineseTimeOffset + 0.5);
            bool previous = false;
            if (njd < Math.Floor(newMoons[0] + 0.5))
            {
                previous = true;
                lunarMonthInfo = LunarMonthInfo(g.year - 1);
                newMoons = lunarMonthInfo.Item2;
                monthCodes = lunarMonthInfo.Item3;
            }
            int monthIndex = 0;
            for (int i = 0; i <= 15; i++)
            {
                if (njd >= Math.Floor(newMoons[i] + 0.5) && njd < Math.Floor(newMoons[i + 1] + 0.5))
                {
                    monthIndex = i;
                    break;
                }
            }
            int year = g.year;
            if (monthCodes[monthIndex] % 100 < 3 || previous)
            {
                year--;
            }
            int month = (monthCodes[monthIndex] % 100 + 9) % 12 + 1;
            if (monthCodes[monthIndex] > 100)
            {
                // Leap month
                month += 100;
            }
            int day = njd - (int)Math.Floor(newMoons[monthIndex] + 0.5) + 1;
            return new LunisolarDateTime(year, month, day, g.hour, g.minute, g.second, g_timeZoneDelta);
        }

        public bool EarlierThan(LunisolarDateTime another)
        {
            return GetJulianDay() < another.GetJulianDay();
        }
    }
}
