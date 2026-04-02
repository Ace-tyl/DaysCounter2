using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaysCounter2.Utils.Hebrew
{
    public enum YearPattern
    {
        P2D3 = 0x100,
        P2C5 = 0x102,
        P2D5 = 0x110,
        P2C7 = 0x112,
        P3R5 = 0x201,
        P3R7 = 0x211,
        P5R7 = 0x401,
        P5C1 = 0x402,
        P5D1 = 0x410,
        P5C3 = 0x412,
        P7D1 = 0x600,
        P7C3 = 0x602,
        P7D3 = 0x610,
        P7C5 = 0x612,
    }

    internal class HebrewCalendarHelper
    {
        const double MonthLength = 29.5 + 793.0 / 1080 / 24;
        public const int JulianDayOffset = 347997;

        // The weekday of a julian day is its remainder modulo 7 plus 1
        // Returns 0 to 6, where 0 stands for Sunday
        static int GetWeekdayOfJd(int jd)
        {
            return (jd + 1) % 7;
        }

        static double GetWeekdayOfJd(double jd)
        {
            return (jd + 1) % 7;
        }

        // The 3rd, 6th, 8th, 11th, 14th, 17th, 19th year of a Machzor (loop of 19 years) is leap year
        public static bool IsLeapYear(int year)
        {
            const int leapYearPattern = 0x24949;
            return ((leapYearPattern >> ((year + 19000) % 19)) & 1) == 1; // Avoid negative results
        }

        static int GetMonthId(int year, int month)
        {
            int[] monthCount = [0, 13, 25, 37, 50, 62, 74, 87, 99, 112, 124, 136, 149, 161, 173, 186, 198, 210, 223];
            int MachzorId = (year + 19000) / 19 - 1000;
            return MachzorId * 235 + monthCount[(year + 19000) % 19] + month - 1;
        }

        // Get Molad of specific year and month
        // To get the Molad Tishrei, set month to 1
        public static double GetMoladJd(int year, int month = 1)
        {
            int N = GetMonthId(year, month) - 13;
            // The benchmark of this value is based on Jerusalem time
            // So IsraelTimeOffset is not required to be added
            return JulianDayOffset + (23 + 34.0 / 3 / 60) / 24 + MonthLength * N;
        }

        public static int GetPartIndexOfTime(int d, int h, int p)
        {
            return (d * 24 + h) * 1080 + p;
        }

        public static int GetPartIndexOfWeek(double jd)
        {
            // The new day starts in 18:00 of previous day in Hebrew Calendar
            double njd = jd + 0.25;
            double weekday = GetWeekdayOfJd(njd);
            return (int)((weekday + 1) * 24 * 1080);
        }

        static YearPattern[] patternList1 = [
            YearPattern.P2D3,
            YearPattern.P2C5,
            YearPattern.P2C5,
            YearPattern.P3R5,
            YearPattern.P3R5,
            YearPattern.P5R7,
            YearPattern.P5R7,
            YearPattern.P5R7,
            YearPattern.P5C1,
            YearPattern.P7D1,
            YearPattern.P7C3,
            YearPattern.P7C3,
            YearPattern.P7C3,
        ];
        static YearPattern[] patternList2 = [
            YearPattern.P2D3,
            YearPattern.P2C5,
            YearPattern.P2C5,
            YearPattern.P3R5,
            YearPattern.P3R5,
            YearPattern.P5R7,
            YearPattern.P5R7,
            YearPattern.P5R7,
            YearPattern.P5C1,
            YearPattern.P7D1,
            YearPattern.P7D1,
            YearPattern.P7C3,
            YearPattern.P7C3,
        ];
        static YearPattern[] patternList3 = [
            YearPattern.P2D3,
            YearPattern.P2C5,
            YearPattern.P2C5,
            YearPattern.P2C5,
            YearPattern.P3R5,
            YearPattern.P5R7,
            YearPattern.P5R7,
            YearPattern.P5R7,
            YearPattern.P5C1,
            YearPattern.P7D1,
            YearPattern.P7D1,
            YearPattern.P7C3,
            YearPattern.P7C3,
        ];
        static YearPattern[] patternList4 = [
            YearPattern.P2D5,
            YearPattern.P2D5,
            YearPattern.P2C7,
            YearPattern.P2C7,
            YearPattern.P3R7,
            YearPattern.P3R7,
            YearPattern.P5D1,
            YearPattern.P5C3,
            YearPattern.P5C3,
            YearPattern.P7D3,
            YearPattern.P7D3,
            YearPattern.P7D3,
            YearPattern.P7C5,
        ];
        const int mask1 = 0x9212, mask2 = 0x40080, mask3 = 0x12424;

        public static YearPattern GetYearPattern(int year, double _molad = double.NaN)
        {
            double[] patternSplitters = [
                GetPartIndexOfTime(1, 9, 204),
                GetPartIndexOfTime(1, 20, 491),
                GetPartIndexOfTime(2, 15, 589),
                GetPartIndexOfTime(2, 18, 0),
                GetPartIndexOfTime(3, 9, 204),
                GetPartIndexOfTime(3, 18, 0),
                GetPartIndexOfTime(4, 11, 695),
                GetPartIndexOfTime(5, 9, 204),
                GetPartIndexOfTime(5, 18, 0),
                GetPartIndexOfTime(6, 0, 408),
                GetPartIndexOfTime(6, 9, 204),
                GetPartIndexOfTime(6, 20, 491),
                GetPartIndexOfTime(7, 18, 0)
            ];
            double molad = double.IsNaN(_molad) ? GetMoladJd(year) : _molad;
            double index = GetPartIndexOfWeek(molad);
            int yearInMachzor = (year + 19000) % 19;
            int patternIndex = 0;
            for (int i = 1; i <= 12; i++)
            {
                if (index >= patternSplitters[i - 1] && index < patternSplitters[i])
                {
                    patternIndex = i;
                    break;
                }
            }
            if (((mask1 >> yearInMachzor) & 1) == 1)
            {
                return patternList1[patternIndex];
            }
            else if (((mask2 >> yearInMachzor) & 1) == 1)
            {
                return patternList2[patternIndex];
            }
            else if (((mask3 >> yearInMachzor) & 1) == 1)
            {
                return patternList3[patternIndex];
            }
            else
            {
                return patternList4[patternIndex];
            }
        }

        public static int GetFirstDayOfYear(int year)
        {
            double molad = GetMoladJd(year);
            YearPattern pattern = GetYearPattern(year, molad);
            int weekday = ((int)pattern & 0xF00) >> 8;
            int moladDay = (int)Math.Floor(molad);
            return moladDay + weekday - GetWeekdayOfJd(moladDay);
        }
    }
}
