using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Structures
{
    public class OnScreenArea
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        //Empty constructor for json parser for setup loading
        public OnScreenArea() 
        {

        }

        public OnScreenArea(Point point1, Point point2)
        {
            X = point1.X;
            Y = point1.Y;
            Width = point2.X - point1.X;
            Height = point2.Y - point1.Y;
            if (Width <= 0) throw new Exception("Area width can't be negative");
            if (Height <= 0) throw new Exception("Area height can't be negative");
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }
    }
}
