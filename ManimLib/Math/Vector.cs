using System.Numerics;

namespace ManimLib.Math
{
    public static class PyVector
    {
        public static string ToPythonVector(Vector2 v)
        {
            string result = "";
            result += v.X.ToString();
            result += (v.X < 0) ? "*LEFT" : "*RIGHT";

            result += " + ";
            result += v.Y.ToString();
            result += (v.Y < 0) ? "*UP" : "*DOWN";

            return result;
        }
        public static string PointToPythonVector(Point p)
        {
            return ToPythonVector(new Vector2((float)p.X, (float)p.Y));
        }
        public static string PointToPythonVector(decimal x, decimal y)
        {
            return ToPythonVector(new Vector2((float)x, (float)y));
        }

        public static readonly Vector2 UP = new Vector2(0, 1);
        public static readonly Vector2 DOWN = new Vector2(0, -1);
        public static readonly Vector2 LEFT = new Vector2(-1, 0);
        public static readonly Vector2 RIGHT = new Vector2(1, 0);
        public static readonly Vector2 ORIGIN = new Vector2(0, 0);
    }
}
