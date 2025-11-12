using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaysCounter2.Utils
{
    public class TimeZoneData
    {
        public int delta { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public TimeZoneData(int delta, string name, string description)
        {
            this.delta = delta;
            this.name = name;
            this.description = description;
        }
    }

    public class TimeZoneManager
    {
        public static List<TimeZoneData> timeZoneData = new List<TimeZoneData>
        {
            new TimeZoneData(-720, "UTC-12:00", Lang.Resources.timeZone_neg1200),
            new TimeZoneData(-660, "UTC-11:00", Lang.Resources.timeZone_neg1100),
            new TimeZoneData(-600, "UTC-10:00", Lang.Resources.timeZone_neg1000),
            new TimeZoneData(-570, "UTC-09:30", Lang.Resources.timeZone_neg0930),
            new TimeZoneData(-540, "UTC-09:00", Lang.Resources.timeZone_neg0900),
            new TimeZoneData(-480, "UTC-08:00", Lang.Resources.timeZone_neg0800),
            new TimeZoneData(-420, "UTC-07:00", Lang.Resources.timeZone_neg0700),
            new TimeZoneData(-360, "UTC-06:00", Lang.Resources.timeZone_neg0600),
            new TimeZoneData(-300, "UTC-05:00", Lang.Resources.timeZone_neg0500),
            new TimeZoneData(-240, "UTC-04:00", Lang.Resources.timeZone_neg0400),
            new TimeZoneData(-210, "UTC-03:30", Lang.Resources.timeZone_neg0330),
            new TimeZoneData(-180, "UTC-03:00", Lang.Resources.timeZone_neg0300),
            new TimeZoneData(-150, "UTC-02:30", Lang.Resources.timeZone_neg0230),
            new TimeZoneData(-120, "UTC-02:00", Lang.Resources.timeZone_neg0200),
            new TimeZoneData(- 60, "UTC-01:00", Lang.Resources.timeZone_neg0100),
            new TimeZoneData(+  0, "UTC+00:00", Lang.Resources.timeZone_pos0000),
            new TimeZoneData(+ 48, "UTC+00:48", Lang.Resources.timeZone_pos0048),
            new TimeZoneData(+ 60, "UTC+01:00", Lang.Resources.timeZone_pos0100),
            new TimeZoneData(+108, "UTC+01:48", Lang.Resources.timeZone_pos0148),
            new TimeZoneData(+120, "UTC+02:00", Lang.Resources.timeZone_pos0200),
            new TimeZoneData(+180, "UTC+03:00", Lang.Resources.timeZone_pos0300),
            new TimeZoneData(+210, "UTC+03:30", Lang.Resources.timeZone_pos0330),
            new TimeZoneData(+240, "UTC+04:00", Lang.Resources.timeZone_pos0400),
            new TimeZoneData(+270, "UTC+04:30", Lang.Resources.timeZone_pos0430),
            new TimeZoneData(+300, "UTC+05:00", Lang.Resources.timeZone_pos0500),
            new TimeZoneData(+330, "UTC+05:30", Lang.Resources.timeZone_pos0530),
            new TimeZoneData(+345, "UTC+05:45", Lang.Resources.timeZone_pos0545),
            new TimeZoneData(+360, "UTC+06:00", Lang.Resources.timeZone_pos0600),
            new TimeZoneData(+390, "UTC+06:30", Lang.Resources.timeZone_pos0630),
            new TimeZoneData(+420, "UTC+07:00", Lang.Resources.timeZone_pos0700),
            new TimeZoneData(+480, "UTC+08:00", Lang.Resources.timeZone_pos0800),
            new TimeZoneData(+525, "UTC+08:45", Lang.Resources.timeZone_pos0845),
            new TimeZoneData(+540, "UTC+09:00", Lang.Resources.timeZone_pos0900),
            new TimeZoneData(+570, "UTC+09:30", Lang.Resources.timeZone_pos0930),
            new TimeZoneData(+600, "UTC+10:00", Lang.Resources.timeZone_pos1000),
            new TimeZoneData(+630, "UTC+10:30", Lang.Resources.timeZone_pos1030),
            new TimeZoneData(+660, "UTC+11:00", Lang.Resources.timeZone_pos1100),
            new TimeZoneData(+720, "UTC+12:00", Lang.Resources.timeZone_pos1200),
            new TimeZoneData(+765, "UTC+12:45", Lang.Resources.timeZone_pos1245),
            new TimeZoneData(+780, "UTC+13:00", Lang.Resources.timeZone_pos1300),
            new TimeZoneData(+825, "UTC+13:45", Lang.Resources.timeZone_pos1345),
            new TimeZoneData(+840, "UTC+14:00", Lang.Resources.timeZone_pos1400),
        };
    }
}
