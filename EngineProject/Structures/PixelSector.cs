using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Structures
{
    public class PixelSector
    {
        public List<PixelInfo> PixelsData = new List<PixelInfo>();

        private PixelsStats pixelsStats = null;

        public PixelsStats GetPixelsStats()
        {
            if (pixelsStats != null) return pixelsStats;
            pixelsStats = new PixelsStats();

            var pixelsCount = PixelsData.Count;
            double avR = PixelsData.Sum(s => s.R) / pixelsCount;
            double avG = PixelsData.Sum(s => s.G) / pixelsCount;
            double avB = PixelsData.Sum(s => s.B) / pixelsCount;
            double avBr = PixelsData.Sum(s => s.Brightness) / pixelsCount;

            double dispSumR = PixelsData.Sum(s => Math.Pow((s.R - avR), 2));
            double dispSumG = PixelsData.Sum(s => Math.Pow((s.G - avG), 2));
            double dispSumB = PixelsData.Sum(s => Math.Pow((s.B - avB), 2));
            double dispSumBr = PixelsData.Sum(s => Math.Pow((s.Brightness - avBr), 2));

            pixelsStats.absoluteQDiffR = Math.Sqrt(dispSumR);
            pixelsStats.absoluteQDiffG = Math.Sqrt(dispSumG);
            pixelsStats.absoluteQDiffB = Math.Sqrt(dispSumB);
            pixelsStats.absoluteQDiffBr = Math.Sqrt(dispSumBr);

            PixelsData = null;
            return pixelsStats;
        }
    }
}
