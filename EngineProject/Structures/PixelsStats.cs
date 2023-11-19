using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Structures
{
    public class PixelsStats
    {
        public double absoluteQDiffR { get; set; }
        public double absoluteQDiffG { get; set; }
        public double absoluteQDiffB { get; set; }
        public double absoluteQDiffBr { get; set; }

        private const short ARGBSize = 255;
        private const short BrightnessSize = 1;

        public static double GetSectorDifferenceInProcent(PixelsStats target, PixelsStats dup)
        {
            var statsToAverage = new List<double>();

            statsToAverage.Add(target.absoluteQDiffR > dup.absoluteQDiffR
                ? dup.absoluteQDiffR / target.absoluteQDiffR
                : target.absoluteQDiffR / dup.absoluteQDiffR);

            statsToAverage.Add(target.absoluteQDiffG > dup.absoluteQDiffG
                ? dup.absoluteQDiffG / target.absoluteQDiffG
                : target.absoluteQDiffG / dup.absoluteQDiffG);

            statsToAverage.Add(target.absoluteQDiffB > dup.absoluteQDiffB
                ? dup.absoluteQDiffB / target.absoluteQDiffB
                : target.absoluteQDiffB / dup.absoluteQDiffB);

            statsToAverage.Add(target.absoluteQDiffBr > dup.absoluteQDiffBr
                ? dup.absoluteQDiffBr / target.absoluteQDiffBr
                : target.absoluteQDiffBr / dup.absoluteQDiffBr);

            return Math.Abs((statsToAverage.Sum(s => s) / statsToAverage.Count()) - 1);
        }
    }
}
