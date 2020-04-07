using static System.Math;

namespace ManimLib.Math
{
    public struct Rect
    {
        private decimal Width, Height;

        #region Constructors
        public Rect(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public Rect(double width, double height)
        {
            Width = (decimal)width;
            Height = (decimal)height;
        }
        public Rect(float width, float height)
        {
            Width = (decimal)width;
            Height = (decimal)height;
        }
        public Rect(decimal width, decimal height)
        {
            Width = width;
            Height = height;
        }

        public Rect(int x1, int y1, int x2, int y2)
        {
            Width = Abs(x2 - x1);
            Height = Abs(y2 - y1);
        }
        public Rect(double x1, double y1, double x2, double y2)
        {
            Width = (decimal)Abs(x2 - x1);
            Height = (decimal)Abs(y2 - y1);
        }
        public Rect(float x1, float y1, float x2, float y2)
        {
            Width = (decimal)Abs(x2 - x1);
            Height = (decimal)Abs(y2 - y1);
        }
        public Rect(decimal x1, decimal y1, decimal x2, decimal y2)
        {
            Width = Abs(x2 - x1);
            Height = Abs(y2 - y1);
        }

        public Rect(Point a, Point b)
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
            Width = (decimal)w;
        }
        public void SetWidth(double w)
        {
            Width = (decimal)w;
        }
        public void SetWidth(decimal w)
        {
            Width = w;
        }
        public decimal GetWidth()
        {
            return Width;
        }

        public void SetHeight(int h)
        {
            Height = h;
        }
        public void SetHeight(float h)
        {
            Height = (decimal)h;
        }
        public void SetHeight(double h)
        {
            Height = (decimal)h;
        }
        public void SetHeight(decimal h)
        {
            Height = h;
        }
        public decimal GetHeight()
        {
            return Height;
        }
    }
}
