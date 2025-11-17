using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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

        long GetLoopDestJulian(MyDateTime now, long nowJulian)
        {
            MyDateTime destDateTime = dateTime.Clone();
            long destJulian;
            if (loopType == LoopTypes.Years)
            {
                int loopsCount = (now.year - dateTime.year + loopValue - 1) / loopValue;
                destDateTime.year += loopsCount * loopValue;
                if (destDateTime.EarlierThan(now))
                {
                    destDateTime.year += loopValue;
                }
            }
            else if (loopType == LoopTypes.Months)
            {
                int dateTimeMonths = dateTime.year * 12 + dateTime.month - 1;
                int nowMonths = now.year * 12 + now.month - 1;
                int loopsCount = (nowMonths - dateTimeMonths + loopValue - 1) / loopValue;
                int destDateTimeMonths = dateTimeMonths + loopsCount * loopValue;
                destDateTime.year = destDateTimeMonths / 12;
                destDateTime.month = destDateTimeMonths % 12 + 1;
                if (destDateTime.EarlierThan(now))
                {
                    destDateTimeMonths += 1;
                    destDateTime.year = destDateTimeMonths / 12;
                    destDateTime.month = destDateTimeMonths % 12 + 1;
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

        public static long displayRangeStart = 199190707200L; // 1600/1/1 0:00 UTC
        public static long displayRangeEnd = 464269017600L; // 9999/12/31 0:00 UTC

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
