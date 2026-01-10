using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaysCounter2.Utils.ChineseLunisolar
{
    internal class SolarTerm
    {
        // Mean solar terms of specific years
        static Dictionary<int, double[]> mstCache = [];

        // Get adjusted solar terms by Delta T and Perturbation from Vernal Equinox
        public static double[] AdjustedSolarTerms(int year, int start = 0, int end = 25)
        {
            double[] mst;
            if (mstCache.ContainsKey(year))
            {
                mst = mstCache[year];
            }
            else
            {
                mst = Astronomy.MeanSolarTerms(year);
                mstCache.Add(year, mst);
            }

            double[] jieqi = new double[mst.Length]; // Length is 26
            for (int i = 0; i < 26; i++)
            {
                if (i < start || i > end)
                {
                    continue;
                }
                double jd = mst[i];
                double perturbation = Astronomy.Perturbation(jd);
                double deltaT = Astronomy.DeltaTDays(year, (i + 1) / 2 + 3);
                jieqi[i] = jd + perturbation - deltaT;
            }
            return jieqi;
        }

        // Last year solar terms from Winter Solstice (index from 18 to 23)
        public static double[] LastYearSolarTerms(int year)
        {
            return AdjustedSolarTerms(year - 1, 18, 23);
        }
    }
}
