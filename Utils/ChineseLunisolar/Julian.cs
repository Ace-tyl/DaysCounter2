using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaysCounter2.Utils.ChineseLunisolar
{
    internal class Julian
    {
        const double julianDay2000 = 2451545;
        const double daysOfYear = 365.25;

        public static double JulianDayFrom2000(double jd)
        {
            return jd - julianDay2000;
        }

        public static double JulianCentury(double jd)
        {
            return JulianDayFrom2000(jd) / daysOfYear / 100;
        }

        public static double JulianThousandYear(double jd)
        {
            return JulianDayFrom2000(jd) / daysOfYear / 1000;
        }
    }
}
