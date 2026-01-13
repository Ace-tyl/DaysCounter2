using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DaysCounter2.Utils.ChineseLunisolar;

namespace DaysCounter2.Utils
{
    internal class Astronomy
    {
        // Mean length of synodic month
        public const double MSM = 29.530588853;

        // Benchmark New Moon: Jan 6, 2000 14:20:37
        public const double BNM = 2451550.0976504628;

        // Delta time between Earth time and UTC
        public static double DeltaTSeconds(double year, double month)
        {
            // Reference: https://eclipse.gsfc.nasa.gov/SEhelp/deltatpoly2004.html

            double y = year + (month - 0.5) / 12;
            double dt;

            if (year < -500)
            {
                double u = (y - 1820) / 100;
                dt = -20 + 32 * u * u;
            }
            else if (year < 500)
            {
                double u = y / 100;
                dt = 10583.6 - 1014.41 * u
                    + 33.78311 * u * u
                    - 5.952053 * u * u * u
                    - 0.1798452 * u * u * u * u
                    + 0.022174192 * u * u * u * u * u
                    + 0.0090316521 * u * u * u * u * u * u;
            }
            else if (year < 1600)
            {
                double u = (y - 1000) / 100;
                dt = 1574.2 - 556.01 * u
                    + 71.23472 * u * u
                    + 0.319781 * u * u * u
                    - 0.8503463 * u * u * u * u
                    - 0.005050998 * u * u * u * u * u
                    + 0.0083572073 * u * u * u * u * u * u;
            }
            else if (year < 1700)
            {
                double t = y - 1600;
                dt = 120 - 0.9808 * t
                    - 0.01532 * t * t
                    + t * t * t / 7129;
            }
            else if (year < 1800)
            {
                double t = y - 1700;
                dt = 8.83 + 0.1603 * t
                    - 0.0059285 * t * t
                    + 0.00013336 * t * t * t
                    - t * t * t * t / 1174000;
            }
            else if (year < 1860)
            {
                double t = y - 1800;
                dt = 13.72 - 0.332447 * t
                    + 0.0068612 * t * t
                    + 0.0041116 * t * t * t
                    - 0.00037436 * t * t * t * t
                    + 0.0000121272 * t * t * t * t * t
                    - 0.0000001699 * t * t * t * t * t * t
                    + 0.000000000875 * t * t * t * t * t * t * t;
            }
            else if (year < 1900)
            {
                double t = y - 1860;
                dt = 7.62 + 0.5737 * t
                    - 0.251754 * t * t
                    + 0.01680668 * t * t * t
                    - 0.0004473624 * t * t * t * t
                    + t * t * t * t * t / 233174;
            }
            else if (year < 1920)
            {
                double t = y - 1900;
                dt = -2.79 + 1.494119 * t
                    - 0.0598939 * t * t
                    + 0.0061966 * t * t * t
                    - 0.000197 * t * t * t * t;
            }
            else if (year < 1941)
            {
                double t = y - 1920;
                dt = 21.2 + 0.84493 * t
                    - 0.0761 * t * t
                    + 0.0020936 * t * t * t;
            }
            else if (year < 1961)
            {
                double t = y - 1950;
                dt = 29.07 + 0.407 * t
                    - t * t / 233
                    + t * t * t / 2547;
            }
            else if (year < 1986)
            {
                double t = y - 1975;
                dt = 45.45 + 1.067 * t
                    - t * t / 260
                    - t * t * t / 718;
            }
            else if (year < 2005)
            {
                double t = y - 2000;
                dt = 63.86
                    + 0.3345 * t
                    - 0.060374 * t * t
                    + 0.0017275 * t * t * t
                    + 0.000651814 * t * t * t * t
                    + 0.00002373599 * t * t * t * t * t;
            }
            else if (year < 2050)
            {
                double t = y - 2000;
                dt = 62.92 + 0.32217 * t
                    + 0.005589 * t * t;
            }
            else if (year < 2150)
            {
                double u = (y - 1820) / 100;
                dt = -20 + 32 * u * u
                    - 0.5628 * (2150 - y);
            }
            else
            {
                double u = (y - 1820) / 100;
                dt = -20 + 32 * u * u;
            }

            // All values of ΔT based on Morrison and Stephenson [2004] assume a value for the Moon's secular acceleration of -26 arcsec/cy^2.
            // However, the ELP-2000/82 lunar ephemeris employed in the Canon uses a slightly different value of -25.858 arcsec/cy^2.
            // Thus, a small correction "c" must be added to the values derived from the polynomial expressions for ΔT before they can be used in the Canon
            // Since the values of ΔT for the interval 1955 to 2005 were derived independent of any lunar ephemeris, no correction is needed for this period.
            if (year < 1955 || year >= 2005)
            {
                double c = -0.000012932 * (y - 1955) * (y - 1955);
                dt += c;
            }

            return dt;
        }

        public static double DeltaTDays(double year, double month)
        {
            return DeltaTSeconds(year, month) / 86400;
        }

        // Calculate the perturbation of Earth by other planets given specified Julian day jd
        public static double Perturbation(double jd)
        {
            // Reference: Jean Meeus. Astronomical Algorithms. 1991. Page 177

            int[] A = [485, 203, 199, 182, 156, 136, 77, 74, 70, 58, 52, 50, 45, 44, 29, 18, 17, 16, 14, 12, 12, 12, 9, 8];
            double[] B = [324.96, 337.23, 342.08, 27.85, 73.14, 171.52, 222.54, 296.72, 243.58, 119.81, 297.17, 21.02, 247.54,
                325.15,60.93, 155.12, 288.79, 198.04, 199.76, 95.39, 287.11, 320.81, 227.73, 15.45];
            double[] C = [1934.136, 32964.467, 20.186, 445267.112, 45036.886, 22518.443, 65928.934, 3034.906, 9037.513,
                33718.147, 150.678, 2281.226, 29929.562, 31555.956, 4443.417, 67555.328, 4562.452, 62894.029, 31436.921,
                14577.848, 31931.756, 34777.259, 1222.114, 16859.074];

            double T = Julian.JulianCentury(jd);

            double s = 0;
            for (int k = 0; k <= 23; k++)
            {
                s += A[k] * Math.Cos(Mathematics.Deg2Rad(B[k] + C[k] * T));
            }

            double W = Mathematics.Deg2Rad(35999.373 * T - 2.47);
            double l = 1 + 0.0334 * Math.Cos(W) + 0.0007 * Math.Cos(2 * W);
            return 0.00001 * s / l;
        }

        // Calculate the JD of Vernal Equinox (春分) in specific year
        public static double VernalEquinox(int year)
        {
            // Reference: Jean Meeus. Astronomical Algorithms. 1991. Page 177

            if (year >= 1000 && year <= 3000)
            {
                double m = (year - 2000) / 1000.0;
                return 2451623.80984 + 365242.37404 * m + 0.05169 * m * m - 0.00411 * m * m * m - 0.00057 * m * m * m * m;
            }
            else
            {
                double m = year / 1000.0;
                return 1721139.29189 + 365242.1374 * m + 0.06134 * m * m + 0.00111 * m * m * m - 0.00071 * m * m * m * m;
            }
        }

        // Calculate the length of tropical year of specific year
        public static double TropicalYearDays(int year)
        {
            return VernalEquinox(year + 1) - VernalEquinox(year);
        }

        // 24 solar terms of specific year
        // To cover a whole year, 4 more solar terms are fetched
        public static double[] MeanSolarTerms(int year)
        {
            // Vernal Equinox jd
            double ve = VernalEquinox(year);

            // Tropical year length
            double ty = VernalEquinox(year + 1) - ve;

            double ath = Math.PI / 12;

            double T = Julian.JulianThousandYear(ve);
            double e = 0.0167086342 - 0.0004203654 * T
                - 0.0000126734 * T * T
                + 0.0000001444 * T * T * T
                - 0.0000000002 * T * T * T * T
                + 0.0000000003 * T * T * T * T * T;

            double TT = year / 1000.0;
            // The angle between the vernal equinox and the perihelion (degree)
            double d = 111.25586939 - 17.0119934518333 * TT
                - 0.044091890166673 * TT * TT
                - 4.37356166661345E-04 * TT * TT * TT
                + 8.16716666602386E-06 * TT * TT * TT * TT;
            double rvp = Mathematics.Deg2Rad(d);

            double[] peri = new double[28];
            for (int i = 0; i < 28; i++)
            {
                int flag = 0;
                double th = ath * i + rvp;
                if (th > Math.PI && th <= 3 * Math.PI)
                {
                    th = 2 * Math.PI - th;
                    flag = 1;
                }
                if (th > 3 * Math.PI)
                {
                    th = 4 * Math.PI - th;
                    flag = 2;
                }

                double f1 = 2 * Math.Atan(Math.Sqrt((1 - e) / (1 + e)) * Math.Tan(th / 2));
                double f2 = e * Math.Sqrt(1 - e * e) * Math.Sin(th) / (1 + e * Math.Cos(th));
                double f = (f1 - f2) * ty / 2 / Math.PI;
                if (flag == 1)
                {
                    f = ty - f;
                }
                if (flag == 2)
                {
                    f = 2 * ty - f;
                }
                peri[i] = f;
            }

            double[] mst = new double[28];
            for (int i = 0; i < 28; i++)
            {
                mst[i] = ve + peri[i] - peri[0];
            }
            return mst;
        }

        // True new moon jd
        public static double TrueNewMoon(double k)
        {
            double nme = BNM + MSM * k;

            double t = Julian.JulianCentury(nme);
            double t2 = t * t;
            double t3 = t * t * t;
            double t4 = t * t * t * t;

            // mean time of phase
            double mnm = nme + 0.0001337 * t2 - 0.00000015 * t3 + 0.00000000073 * t4;

            // Sun's mean anomaly
            double m = 2.5534 + 29.10535669 * k - 0.0000218 * t2 - 0.00000011 * t3;

            // Moon's mean anomaly
            double ms = 201.5643 + 385.81693528 * k + 0.0107438 * t2 + 0.00001239 * t3 - 0.000000058 * t4;

            // Moon's argument of latitude
            double f = 160.7108 + 390.67050274 * k - 0.0016341 * t2 - 0.00000227 * t3 + 0.000000011 * t4;

            // Longitude of the ascending node of the lunar orbit
            double omega = 124.7746 - 1.5637558 * k + 0.0020691 * t2 + 0.00000215 * t3;

            double e = 1 - 0.002516 * t - 0.0000074 * t2;

            double apt1 = -0.4072 * Math.Sin(Math.PI / 180 * ms);
            apt1 += 0.17241 * e * Math.Sin(Math.PI / 180 * m);
            apt1 += 0.01608 * Math.Sin(Math.PI / 180 * 2 * ms);
            apt1 += 0.01039 * Math.Sin(Math.PI / 180 * 2 * f);
            apt1 += 0.00739 * e * Math.Sin(Math.PI / 180 * (ms - m));
            apt1 -= 0.00514 * e * Math.Sin(Math.PI / 180 * (ms + m));
            apt1 += 0.00208 * e * e * Math.Sin(Math.PI / 180 * (2 * m));
            apt1 -= 0.00111 * Math.Sin(Math.PI / 180 * (ms - 2 * f));
            apt1 -= 0.00057 * Math.Sin(Math.PI / 180 * (ms + 2 * f));
            apt1 += 0.00056 * e * Math.Sin(Math.PI / 180 * (2 * ms + m));
            apt1 -= 0.00042 * Math.Sin(Math.PI / 180 * 3 * ms);
            apt1 += 0.00042 * e * Math.Sin(Math.PI / 180 * (m + 2 * f));
            apt1 += 0.00038 * e * Math.Sin(Math.PI / 180 * (m - 2 * f));
            apt1 -= 0.00024 * e * Math.Sin(Math.PI / 180 * (2 * ms - m));
            apt1 -= 0.00017 * Math.Sin(Math.PI / 180 * omega);
            apt1 -= 0.00007 * Math.Sin(Math.PI / 180 * (ms + 2 * m));
            apt1 += 0.00004 * Math.Sin(Math.PI / 180 * (2 * ms - 2 * f));
            apt1 += 0.00004 * Math.Sin(Math.PI / 180 * (3 * m));
            apt1 += 0.00003 * Math.Sin(Math.PI / 180 * (ms + m - 2 * f));
            apt1 += 0.00003 * Math.Sin(Math.PI / 180 * (2 * ms + 2 * f));
            apt1 -= 0.00003 * Math.Sin(Math.PI / 180 * (ms + m + 2 * f));
            apt1 += 0.00003 * Math.Sin(Math.PI / 180 * (ms - m + 2 * f));
            apt1 -= 0.00002 * Math.Sin(Math.PI / 180 * (ms - m - 2 * f));
            apt1 -= 0.00002 * Math.Sin(Math.PI / 180 * (3 * ms + m));
            apt1 += 0.00002 * Math.Sin(Math.PI / 180 * (4 * ms));

            double apt2 = 0.000325 * Math.Sin(Math.PI / 180 * (299.77 + 0.107408 * k - 0.009173 * t2));
            apt2 += 0.000165 * Math.Sin(Math.PI / 180 * (251.88 + 0.016321 * k));
            apt2 += 0.000164 * Math.Sin(Math.PI / 180 * (251.83 + 26.651886 * k));
            apt2 += 0.000126 * Math.Sin(Math.PI / 180 * (349.42 + 36.412478 * k));
            apt2 += 0.00011 * Math.Sin(Math.PI / 180 * (84.66 + 18.206239 * k));
            apt2 += 0.000062 * Math.Sin(Math.PI / 180 * (141.74 + 53.303771 * k));
            apt2 += 0.00006 * Math.Sin(Math.PI / 180 * (207.14 + 2.453732 * k));
            apt2 += 0.000056 * Math.Sin(Math.PI / 180 * (154.84 + 7.30686 * k));
            apt2 += 0.000047 * Math.Sin(Math.PI / 180 * (34.52 + 27.261239 * k));
            apt2 += 0.000042 * Math.Sin(Math.PI / 180 * (207.19 + 0.121824 * k));
            apt2 += 0.00004 * Math.Sin(Math.PI / 180 * (291.34 + 1.844379 * k));
            apt2 += 0.000037 * Math.Sin(Math.PI / 180 * (161.72 + 24.198154 * k));
            apt2 += 0.000035 * Math.Sin(Math.PI / 180 * (239.56 + 25.513099 * k));
            apt2 += 0.000023 * Math.Sin(Math.PI / 180 * (331.55 + 3.592518 * k));

            return mnm + apt1 + apt2;
        }

        // Mean time of phase
        public static double MeanTimeOfPhase(double k)
        {
            double nme = BNM + k * MSM;
            double t = Julian.JulianCentury(nme);

            return nme + 0.0001337 * t * t - 0.00000015 * t * t * t + 0.00000000073 * t * t * t * t;
        }

        // Reference lunar month number
        public static int ReferenceLunarMonthNumber(double jd)
        {
            return (int)Math.Floor((jd - BNM) / MSM);
        }
    }
}
