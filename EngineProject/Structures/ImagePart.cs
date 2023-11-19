using EngineProject.Infrastructure;
using EngineProject.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Structures
{
    //Class to store bitmap parameters of area for comparing (target is set checking)
    //Store only parameters of pixels, not bitmaps, to reduce memory loss
    public class ImagePart : IDisposable
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        private DisposableBitmap ImagePartBitmap { get; set; }

        public PixelInfo[][] PixelsData { get; set; }
        public PixelSector[] PixelSectors { get; set; }

        public ImagePart(string filePath, int sectorsCount)
        {
            var bitmapFromFile = new Bitmap(filePath);
            SetupImage(new DisposableBitmap(bitmapFromFile), sectorsCount);
        }

        public ImagePart(DisposableBitmap originalBitmap, int sectorsCount)
        {
            SetupImage(originalBitmap, sectorsCount);
        }

        private void SetupImage(DisposableBitmap originalBitmap, int sectorsCount)
        {
            try
            {
                ImagePartBitmap = new DisposableBitmap(originalBitmap.Clone(new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height),
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb));
                originalBitmap.Dispose();

                int width = ImagePartBitmap.Width;
                int height = ImagePartBitmap.Height;

                PixelSectors = new PixelSector[sectorsCount];
                PixelsData = new PixelInfo[ImagePartBitmap.Width][];

                if ((double)Math.Sqrt(sectorsCount) % 1 != 0) throw new Exception($"Number of sectors ({sectorsCount}) must be a perfect square (4, 9, 16...)");

                int sectorsInRowCount = (int)Math.Sqrt(sectorsCount);

                if (sectorsInRowCount > width + 1) throw new Exception($"Image has small resolution ({width}x{height}) for sectors count {sectorsInRowCount}");
                if (sectorsInRowCount > height + 1) throw new Exception($"Image has small resolution ({width}x{height}) for sectors count {sectorsInRowCount}");

                int sectorWidth = width / sectorsInRowCount;
                int sectorHeight = height / sectorsInRowCount;
                for (int x = 0; x < width; x++)
                {
                    PixelInfo[] PixelsColumn = new PixelInfo[ImagePartBitmap.Height];
                    for (int y = 0; y < height; y++)
                    {
                        var color = ImagePartBitmap.GetPixel(x, y);
                        var pixel = new PixelInfo()
                        {
                            R = color.R,
                            G = color.G,
                            B = color.B,
                            Brightness = color.GetBrightness(),
                            X = x,
                            Y = y
                        };
                        PixelsColumn[y] = pixel;
                        var sectorNumber = GetSectorNumber(x, y, sectorWidth, sectorHeight, sectorsInRowCount);
                        if (PixelSectors[sectorNumber - 1] == null) PixelSectors[sectorNumber - 1] = new PixelSector();
                        PixelSectors[sectorNumber - 1].PixelsData.Add(pixel);
                    }
                    PixelsData[x] = PixelsColumn;
                }
                ImagePartBitmap.Dispose();
                for (int i = 0; i < PixelSectors.Count(); i++)
                {
                    PixelSectors[i].GetPixelsStats();
                }

                PixelsData = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, $"Image setup error");
                throw ex;
            }
        }

        public double GetDupCoef(ImagePart dupImage)
        {
            double diff = 0;
            int pixelSectorsCount = dupImage.PixelSectors.Count();
            for (int i = 0; i < pixelSectorsCount; i++)
            {
                var stats = this.PixelSectors[i].GetPixelsStats();
                var dupStats = dupImage.PixelSectors[i].GetPixelsStats();
                diff += PixelsStats.GetSectorDifferenceInProcent(stats, dupStats);
            }
            return diff / pixelSectorsCount;
        }

        private int GetSectorNumber(int x, int y, int sectorWidth, int sectorHeight, int sectorsInRowCount)
        {
            int xIndex = (int)Math.Ceiling((double)x / sectorWidth);
            int yIndex = (int)Math.Ceiling((double)y / sectorHeight);

            Func<int, int> normalize = (int a) => { return a == 0 ? 1 : a > sectorsInRowCount ? sectorsInRowCount : a; };
            xIndex = normalize(xIndex);
            yIndex = normalize(yIndex);

            return (yIndex - 1) * sectorsInRowCount + xIndex;
        }

        public void Dispose()
        {
            PixelSectors = null;
            FileName = null;
            FilePath = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
