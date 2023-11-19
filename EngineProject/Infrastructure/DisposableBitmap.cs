using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Infrastructure
{
    public class DisposableBitmap : IDisposable
    {
        private Bitmap Bitmap { get; set; }
        private bool isDisposed = false;

        public DisposableBitmap(Bitmap bitmap)
        {
            Bitmap = bitmap;
        }

        public int Width { get { return Bitmap.Width; } }
        public int Height { get { return Bitmap.Height; } }

        public Color GetPixel(int x, int y)
        {
            return Bitmap.GetPixel(x, y);
        }

        public void SetPixel(int x, int y, Color color)
        {
            Bitmap.SetPixel(x, y, color);
        }

        public Bitmap Clone(Rectangle rectangle, PixelFormat pixelFormat)
        {
            return Bitmap.Clone(rectangle, pixelFormat);
        }

        public Size Size { get { return Bitmap.Size; } }

        public PixelFormat PixelFormat { get { return Bitmap.PixelFormat; } }

        public Bitmap GetBitmap()
        {
            return Bitmap;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                Bitmap.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
