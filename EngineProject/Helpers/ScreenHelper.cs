using EngineProject.Managers;
using EngineProject.Structures;
using EngineProject.Enums;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using System.Drawing.Drawing2D;
using EngineProject.Infrastructure;

namespace EngineProject.Helpers
{
    //Methods to get information from screen
    public static class ScreenHelper
    {
        //Component for reading text from image (check target)
        private static TesseractEngine TesseractEngine;
        public static void SetupScreenHelper()
        {
            TesseractEngine = new TesseractEngine(Path.Combine(Environment.CurrentDirectory, "tessdata"), Languages.Russian, EngineMode.Default);
        }

        //Screenshot area and get bitmap
        public static DisposableBitmap GetScreenAreaBitmap(OnScreenArea onScreenArea, bool toBitColors = false)
        {
            try
            {
                using (var screenBitmap = new DisposableBitmap(new Bitmap(SettingsManager.ScreenWidth, SettingsManager.ScreenHeight)))
                {
                    using (var graphics = Graphics.FromImage(screenBitmap.GetBitmap() as Image))
                    {
                        graphics.CopyFromScreen(0, 0, 0, 0, screenBitmap.Size);

                        var cloneRect = onScreenArea.GetRectangle();
                        if (toBitColors) return new DisposableBitmap(screenBitmap.Clone(cloneRect, PixelFormat.Format8bppIndexed));
                        return new DisposableBitmap(screenBitmap.Clone(cloneRect, screenBitmap.PixelFormat));
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, "Getting screen area bitmap error");
                throw ex;
            }
        }

        //Get pixels colors list in bitmap
        private static List<int> GetColors(DisposableBitmap bitmap, OnScreenArea onScreenArea, IEnumerable<TrackableColors> ignoredColors = null)
        {
            try
            {
                var colors = new List<int>() { (int)TrackableColors.Unknown };
                int pixelColor = (int)TrackableColors.Unknown;

                for (int x = 0; x < onScreenArea.Width; x++)
                {
                    for (int y = 0; y < onScreenArea.Height; y++)
                    {
                        pixelColor = (int)TrackableColor.GetTrackableColor(bitmap.GetPixel(x, y));

                        if (ignoredColors == null || !ignoredColors.Select(s => (int)s).Contains(pixelColor))
                            colors.Add(pixelColor);
                    }
                }

                return colors;
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, "Getting list of area colors error");
                throw ex;
            }
        }

        //Compare screen area to sample (for target checking)
        public static bool AreAreasEqual(OnScreenArea onScreenArea, string sampleName, int sectorsCount)
        {
            try
            {
                double dupCoef = 1;
                using (var imagePartFromScreen = new ImagePart(GetScreenAreaBitmap(onScreenArea), sectorsCount))
                {
                    using (var sample = new ImagePart(Path.Combine(Environment.CurrentDirectory, @"Samples\", sampleName), sectorsCount))
                    {
                        dupCoef = imagePartFromScreen.GetDupCoef(sample);
                    }
                }
                return dupCoef <= SettingsManager.MaxDupDifferenceInProcent;
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, "Comparing areas error");
                throw ex;
            }
        }

        //Check color in area (for overweight checking)
        public static int GetMostPopularColorInArea(OnScreenArea onScreenArea, IEnumerable<TrackableColors> ignoredColors = null)
        {
            try
            {
                using (var screenAreaBitmap = GetScreenAreaBitmap(onScreenArea, toBitColors: true))
                {
                    var colors = GetColors(screenAreaBitmap, onScreenArea, ignoredColors);
                    var groupedColors = colors.GroupBy(colorCode => colorCode);
                    return groupedColors.Where(w => w.Count() == groupedColors.Max(gr => gr.Count())).First().Key;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, "Getting most popular color in area error");
                throw ex;
            }
        }

        //Calculate percent of color in area (for target HP checking)
        public static double GetPercentOfColor(OnScreenArea onScreenArea, int colorCode)
        {
            try
            {
                using (var screenAreaBitmap = GetScreenAreaBitmap(onScreenArea, true))
                {
                    var colors = GetColors(screenAreaBitmap, onScreenArea);

                    int colorCount = colors.Where(c => c == colorCode).Count();
                    int totalCount = colors.Count();

                    //in case of error
                    if (totalCount <= 0) return 0;

                    return Math.Round((float)colorCount / (float)totalCount * 100f, 2);
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, "Getting percent of color error");
                throw ex;
            }
        }

        //Zoom bitmap (before text reading)
        public static DisposableBitmap ResizeImage(DisposableBitmap bitmap, double zoom)
        {
            var width = (int)((double)bitmap.Width * zoom);
            var height = (int)((double)bitmap.Height * zoom);
            using (var image = (Image)bitmap.GetBitmap())
            {
                var destRect = new Rectangle(0, 0, width, height);
                var destImage = new Bitmap(width, height);

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }
                bitmap.Dispose();
                return new DisposableBitmap(destImage);
            }
        }

        //Prepare area for white text reading
        public static DisposableBitmap GetTextForReading(DisposableBitmap bitmap, int colorCodeToRead)
        {
            int pixelColor = (int)TrackableColors.Unknown;
            var newBitmap = new DisposableBitmap(new Bitmap(bitmap.Width, bitmap.Height));
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    pixelColor = (int)TrackableColor.GetTrackableColor(bitmap.GetPixel(x, y));
                    if (pixelColor == colorCodeToRead)
                    {
                        newBitmap.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                        newBitmap.SetPixel(x, y, Color.White);
                    }
                }
            }
            bitmap.Dispose();
            return newBitmap;
        }

        //Get text in area
        public static string GetTextInArea(OnScreenArea area, int colorCodeToRead)
        {
            var screenAreaBitmap = GetScreenAreaBitmap(area);
            screenAreaBitmap = ResizeImage(screenAreaBitmap, 2);
            screenAreaBitmap = GetTextForReading(screenAreaBitmap, colorCodeToRead);
            string onScreenText = string.Empty;
            try
            {
                //Tesseract reader can't read multiple images at the same time
                lock (TesseractEngine)
                {
                    using (var pix = PixConverter.ToPix(screenAreaBitmap.GetBitmap()))
                    {
                        using (var page = TesseractEngine.Process(pix))
                        {
                            onScreenText = page.GetText();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, "Can't read text from image");
            }
            screenAreaBitmap.Dispose();
            return onScreenText.Trim();
        }

        //Methods for testing
        private static void SaveBitmap(Bitmap bitmap, string path = "", string fileName = "")
        {
            if (string.IsNullOrWhiteSpace(fileName)) fileName = $"{DateTime.Now.ToString("T").Replace(":", "-")}.jpg";
            if (!string.IsNullOrWhiteSpace(path))
            {
                bitmap.Save(Path.Combine(path, fileName));
            }
            else
            {
                bitmap.Save(fileName);
            }
        }
        private static String HexConverter(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
    }
}
