using MathNet.Spatial.Euclidean;

namespace ManimLib.Math
{
    public static class PyVector
    {
        public static string ToPythonVector(Vector2D v)
        {
            string result = "";
            result += v.X.ToString();
            result += (v.X < 0) ? "*LEFT" : "*RIGHT";

            result += " + ";
            result += v.Y.ToString();
            result += (v.Y < 0) ? "*UP" : "*DOWN";

            return result;
        }
        public static string PointToPythonVector(Point2D p)
        {
            return ToPythonVector(new Vector2D(p.X, p.Y));
        }
        public static string PointToPythonVector(decimal x, decimal y)
        {
            return ToPythonVector(new Vector2((float)x, (float)y));
        }

        public static readonly Vector2D UP = new Vector2D(0, 1);
        public static readonly Vector2D DOWN = new Vector2D(0, -1);
        public static readonly Vector2D LEFT = new Vector2D(-1, 0);
        public static readonly Vector2D RIGHT = new Vector2D(1, 0);
        public static readonly Vector2D ORIGIN = new Vector2D(0, 0);
    }
}
