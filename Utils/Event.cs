using System;
using DaysCounter2.Utils.AlHijri;
using DaysCounter2.Utils.ChineseLunisolar;

namespace DaysCounter2.Utils
{
    public enum LoopTypes
    {
        None = -1,
        Seconds = 0,
        Minutes = 1,
        Hours = 2,
        Days = 3,
        Months = 4,
        Years = 5,
    }

    public class Event
    {
        public string uuid = Guid.NewGuid().ToString(), name = "Unnamed";
        public MyDateTime dateTime = new MyDateTime();
        public LoopTypes loopType = LoopTypes.None;
        public int loopValue = 1;
        public int calendar = 0;

        long GetLoopDestJulian(MyDateTime now, long nowJulian)
        {
            MyDateTime destDateTime = dateTime.Clone();
            destDateTime.InitializeTimeZone();
            now.InitializeTimeZone();
            if (destDateTime.timeZoneDelta == null) { return -1; } // Impossible
            if (now.timeZoneDelta == null) { return -1; } // Impossible
            long destJulian;
            if (loopType == LoopTypes.Years)
            {
                if (calendar == 1)
                {
                    LunisolarDateTime lunar = LunisolarDateTime.FromGregorian(destDateTime, destDateTime.GetJulianDay());
                    LunisolarDateTime nowLunar = LunisolarDateTime.FromGregorian(now, nowJulian / 86400.0);
                    int loopsCount = (nowLunar.year - lunar.year + loopValue - 1) / loopValue;
                    int destYear = lunar.year + loopsCount * loopValue;
                    int destMonth = lunar.month;
                    int destDay = lunar.day;
                    if (destMonth > 100)
                    {
                        if (LunisolarDateTime.GetLeapMonth(destYear) != destMonth - 100)
                        {
                            destMonth -= 100;
                        }
                    }
                    int dayCountOfMonth = LunisolarDateTime.GetDayCountOfMonth(destYear, destMonth);
                    if (destDay > dayCountOfMonth)
                    {
                        destDay = dayCountOfMonth;
                    }
                    LunisolarDateTime destLunar = new LunisolarDateTime(destYear, destMonth, destDay, lunar.hour, lunar.minute, lunar.second, lunar.timeZoneDelta);
                    if (destLunar.EarlierThan(nowLunar))
                    {
                        destYear = lunar.year + loopsCount * loopValue + loopValue;
                        destMonth = lunar.month;
                        destDay = lunar.day;
                        if (destMonth > 100)
                        {
                            if (LunisolarDateTime.GetLeapMonth(destYear) != destMonth - 100)
                            {
                                destMonth -= 100;
                            }
                        }
                        dayCountOfMonth = LunisolarDateTime.GetDayCountOfMonth(destYear, destMonth);
                        if (destDay > dayCountOfMonth)
                        {
                            destDay = dayCountOfMonth;
                        }
                        destLunar = new LunisolarDateTime(destYear, destMonth, destDay, lunar.hour, lunar.minute, lunar.second, lunar.timeZoneDelta);
                    }
                    destDateTime = MyDateTime.FromJulianDay(destLunar.GetJulianDay(), lunar.timeZoneDelta);
                }
                else if (calendar == 2)
                {
                    AlHijriDateTime alHijri = AlHijriDateTime.FromJulianDay(destDateTime.GetJulianDay(), (int)destDateTime.timeZoneDelta);
                    AlHijriDateTime nowAlHijri = AlHijriDateTime.FromJulianDay(nowJulian / 86400.0, (int)now.timeZoneDelta);
                    int loopsCount = (nowAlHijri.year - alHijri.year + loopValue - 1) / loopValue;
                    AlHijriDateTime alHijri2 = alHijri.Clone();
                    alHijri2.year += loopsCount * loopValue;
                    alHijri2.AdjustData();
                    if (alHijri2.EarlierThan(nowAlHijri))
                    {
                        alHijri.year += alHijri2.year + loopValue;
                        alHijri.AdjustData();
                    }
                    else
                    {
                        alHijri = alHijri2;
                    }
                    destDateTime = MyDateTime.FromJulianDay(alHijri.GetJulianDay(), alHijri.timeZoneDelta);
                }
                else
                {
                    int loopsCount = (now.year - dateTime.year + loopValue - 1) / loopValue;
                    MyDateTime destDateTime2 = destDateTime.Clone();
                    destDateTime2.year += loopsCount * loopValue;
                    destDateTime2.AdjustData();
                    if (destDateTime2.EarlierThan(now))
                    {
                        destDateTime.year = destDateTime2.year + loopValue;
                    }
                    else
                    {
                        destDateTime = destDateTime2;
                    }
                }
            }
            else if (loopType == LoopTypes.Months)
            {
                if (calendar == 1)
                {
                    int count = 0;
                    LunisolarDateTime destLunar = LunisolarDateTime.FromGregorian(destDateTime, destDateTime.GetJulianDay());
                    int day = destLunar.day;
                    LunisolarDateTime nowLunar = LunisolarDateTime.FromGregorian(now, nowJulian / 86400.0);
                    while (count % loopValue != 0 || destLunar.EarlierThan(nowLunar))
                    {
                        count++;
                        var nextMonthResult = LunisolarDateTime.NextMonth(destLunar.year, destLunar.month);
                        int year = nextMonthResult.Item1, month = nextMonthResult.Item2;
                        int nday = Math.Min(day, LunisolarDateTime.GetDayCountOfMonth(year, month));
                        destLunar.year = year;
                        destLunar.month = month;
                        destLunar.day = nday;
                    }
                    destDateTime = MyDateTime.FromJulianDay(destLunar.GetJulianDay(), destLunar.timeZoneDelta);
                }
                else if (calendar == 2)
                {
                    AlHijriDateTime alHijri = AlHijriDateTime.FromJulianDay(destDateTime.GetJulianDay(), (int)destDateTime.timeZoneDelta);
                    AlHijriDateTime nowAlHijri = AlHijriDateTime.FromJulianDay(nowJulian / 86400.0, (int)now.timeZoneDelta);
                    int dateTimeMonths = alHijri.year * 12 + alHijri.month - 1;
                    int nowMonths = nowAlHijri.year * 12 + nowAlHijri.month - 1;
                    int loopsCount = (nowMonths - dateTimeMonths + loopValue - 1) / loopValue;
                    int destDateTimeMonths = dateTimeMonths + loopsCount * loopValue;
                    AlHijriDateTime alHijri2 = alHijri.Clone();
                    alHijri2.year = destDateTimeMonths / 12;
                    alHijri2.month = destDateTimeMonths % 12 + 1;
                    alHijri2.AdjustData();
                    if (alHijri2.EarlierThan(nowAlHijri))
                    {
                        destDateTimeMonths += 1;
                        alHijri.year = destDateTimeMonths / 12;
                        alHijri.month = destDateTimeMonths % 12 + 1;
                        alHijri.AdjustData();
                    }
                    else
                    {
                        alHijri = alHijri2;
                    }
                    destDateTime = MyDateTime.FromJulianDay(alHijri.GetJulianDay(), alHijri.timeZoneDelta);
                }
                else
                {
                    int dateTimeMonths = dateTime.year * 12 + dateTime.month - 1;
                    int nowMonths = now.year * 12 + now.month - 1;
                    int loopsCount = (nowMonths - dateTimeMonths + loopValue - 1) / loopValue;
                    int destDateTimeMonths = dateTimeMonths + loopsCount * loopValue;
                    MyDateTime destDateTime2 = destDateTime.Clone();
                    destDateTime2.year = destDateTimeMonths / 12;
                    destDateTime2.month = destDateTimeMonths % 12 + 1;
                    destDateTime2.AdjustData();
                    if (destDateTime.EarlierThan(now))
                    {
                        destDateTimeMonths += 1;
                        destDateTime.year = destDateTimeMonths / 12;
                        destDateTime.month = destDateTimeMonths % 12 + 1;
                        destDateTime.AdjustData();
                    }
                    else
                    {
                        destDateTime = destDateTime2;
                    }
                }
            }
            destDateTime.AdjustData();
            destJulian = destDateTime.GetJulianSecond();
            long loopSeconds = -1;
            if (loopType == LoopTypes.Days)
            {
                loopSeconds = loopValue * 86400;
            }
            else if (loopType == LoopTypes.Hours)
            {
                loopSeconds = loopValue * 3600;
            }
            else if (loopType == LoopTypes.Minutes)
            {
                loopSeconds = loopValue * 60;
            }
            else if (loopType == LoopTypes.Seconds)
            {
                loopSeconds = loopValue;
            }
            if (loopSeconds != -1)
            {
                long loopsCount = (nowJulian - destJulian + loopSeconds - 1) / loopSeconds;
                destJulian += loopsCount * loopSeconds;
            }
            return destJulian;
        }

        public long GetDestinationJulian(MyDateTime now, long nowJulian)
        {
            if (loopType != LoopTypes.None && dateTime.EarlierThan(now))
            {
                return GetLoopDestJulian(now, nowJulian);
            }
            else
            {
                return dateTime.GetJulianSecond();
            }
        }

        public static long displayRangeStart = 199190664000L; // 1600/1/1 0:00 UTC
        public static long displayRangeEnd = 464268974400L; // 9999/12/31 0:00 UTC

        public long GetDelta(MyDateTime now, long nowJulian)
        {
            return GetDestinationJulian(now, nowJulian) - nowJulian;
        }
        
        public DateTime? GetDestinationDateTime(MyDateTime now, long nowJulian)
        {
            long destinationJulian = GetDestinationJulian(now, nowJulian);
            if (destinationJulian < displayRangeStart || destinationJulian > displayRangeEnd)
            {
                return null;
            }
            DateTime destDateTime = new DateTime(1600, 1, 1, 0, 0, 0);
            long timeDelta = destinationJulian - displayRangeStart;
            destDateTime = destDateTime.AddSeconds(timeDelta).ToLocalTime();
            return destDateTime;
        }
    }
}
