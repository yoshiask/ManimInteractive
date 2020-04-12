using MathNet.Spatial.Euclidean;
using static System.Math;

namespace ManimLib.Math
{
    public struct Rect
    {
        private double Width, Height;

        #region Constructors
        public Rect(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public Rect(double width, double height)
        {
            Width = width;
            Height = height;
        }
        public Rect(float width, float height)
        {
            Width = width;
            Height = height;
        }
        public Rect(decimal width, decimal height)
        {
            Width = (double)width;
            Height = (double)height;
        }

        public Rect(int x1, int y1, int x2, int y2)
        {
            Width = Abs(x2 - x1);
            Height = Abs(y2 - y1);
        }
        public Rect(double x1, double y1, double x2, double y2)
        {
            Width = Abs(x2 - x1);
            Height = Abs(y2 - y1);
        }
        public Rect(float x1, float y1, float x2, float y2)
        {
            Width = Abs(x2 - x1);
            Height = Abs(y2 - y1);
        }
        public Rect(decimal x1, decimal y1, decimal x2, decimal y2)
        {
            Width = (double)Abs(x2 - x1);
            Height = (double)Abs(y2 - y1);
        }

        public Rect(Point2D a, Point2D b)
        {
            Width = Abs(b.X - a.X);
            Height = Abs(a.X - a.Y);
        }
        #endregion

        public void SetWidth(int w)
        {
            Width = w;
        }
        public void SetWidth(float w)
        {
            Width = w;
        }
        public void SetWidth(double w)
        {
            Width = w;
        }
        public void SetWidth(decimal w)
        {
            Width = (double)w;
        }
        public double GetWidth()
        {
            return Width;
        }

        public void SetHeight(int h)
        {
            Height = h;
        }
        public void SetHeight(float h)
        {
            Height = h;
        }
        public void SetHeight(double h)
        {
            Height = h;
        }
        public void SetHeight(decimal h)
        {
            Height = (double)h;
        }
        public double GetHeight()
        {
            return Height;
        }
    }
}
