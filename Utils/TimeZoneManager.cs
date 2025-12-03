using System.Collections.Generic;

namespace DaysCounter2.Utils
{
    public class TimeZoneData(int delta, string name, string description)
    {
        public int delta { get; set; } = delta;
        public string name { get; set; } = name;
        public string description { get; set; } = description;
    }

    public class TimeZoneManager
    {
        public static List<TimeZoneData> timeZoneData =
        [
            new(-720, "UTC-12:00", Lang.Resources.timeZone_neg1200),
            new(-660, "UTC-11:00", Lang.Resources.timeZone_neg1100),
            new(-600, "UTC-10:00", Lang.Resources.timeZone_neg1000),
            new(-570, "UTC-09:30", Lang.Resources.timeZone_neg0930),
            new(-540, "UTC-09:00", Lang.Resources.timeZone_neg0900),
            new(-480, "UTC-08:00", Lang.Resources.timeZone_neg0800),
            new(-420, "UTC-07:00", Lang.Resources.timeZone_neg0700),
            new(-360, "UTC-06:00", Lang.Resources.timeZone_neg0600),
            new(-300, "UTC-05:00", Lang.Resources.timeZone_neg0500),
            new(-240, "UTC-04:00", Lang.Resources.timeZone_neg0400),
            new(-210, "UTC-03:30", Lang.Resources.timeZone_neg0330),
            new(-180, "UTC-03:00", Lang.Resources.timeZone_neg0300),
            new(-150, "UTC-02:30", Lang.Resources.timeZone_neg0230),
            new(-120, "UTC-02:00", Lang.Resources.timeZone_neg0200),
            new(- 60, "UTC-01:00", Lang.Resources.timeZone_neg0100),
            new(+  0, "UTC+00:00", Lang.Resources.timeZone_pos0000),
            new(+ 48, "UTC+00:48", Lang.Resources.timeZone_pos0048),
            new(+ 60, "UTC+01:00", Lang.Resources.timeZone_pos0100),
            new(+108, "UTC+01:48", Lang.Resources.timeZone_pos0148),
            new(+120, "UTC+02:00", Lang.Resources.timeZone_pos0200),
            new(+180, "UTC+03:00", Lang.Resources.timeZone_pos0300),
            new(+210, "UTC+03:30", Lang.Resources.timeZone_pos0330),
            new(+240, "UTC+04:00", Lang.Resources.timeZone_pos0400),
            new(+270, "UTC+04:30", Lang.Resources.timeZone_pos0430),
            new(+300, "UTC+05:00", Lang.Resources.timeZone_pos0500),
            new(+330, "UTC+05:30", Lang.Resources.timeZone_pos0530),
            new(+345, "UTC+05:45", Lang.Resources.timeZone_pos0545),
            new(+360, "UTC+06:00", Lang.Resources.timeZone_pos0600),
            new(+390, "UTC+06:30", Lang.Resources.timeZone_pos0630),
            new(+420, "UTC+07:00", Lang.Resources.timeZone_pos0700),
            new(+480, "UTC+08:00", Lang.Resources.timeZone_pos0800),
            new(+525, "UTC+08:45", Lang.Resources.timeZone_pos0845),
            new(+540, "UTC+09:00", Lang.Resources.timeZone_pos0900),
            new(+570, "UTC+09:30", Lang.Resources.timeZone_pos0930),
            new(+600, "UTC+10:00", Lang.Resources.timeZone_pos1000),
            new(+630, "UTC+10:30", Lang.Resources.timeZone_pos1030),
            new(+660, "UTC+11:00", Lang.Resources.timeZone_pos1100),
            new(+720, "UTC+12:00", Lang.Resources.timeZone_pos1200),
            new(+765, "UTC+12:45", Lang.Resources.timeZone_pos1245),
            new(+780, "UTC+13:00", Lang.Resources.timeZone_pos1300),
            new(+825, "UTC+13:45", Lang.Resources.timeZone_pos1345),
            new(+840, "UTC+14:00", Lang.Resources.timeZone_pos1400),
        ];
    }
}
