using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;

namespace ManimLib.Math
{
    public static class PyVector
    {
        public static string ToPythonVector(this Vector<double> v)
        {
            string result = "";
            result += v[0].ToString();
            result += (v[0] < 0) ? "*LEFT" : "*RIGHT";

            if (v.Count > 1)
            {
                result += " + ";
                result += v[1].ToString();
                result += (v[1] < 0) ? "*UP" : "*DOWN";
            }

            if (v.Count > 2)
            {
                result += " + ";
                result += v[2].ToString();
                result += (v[2] < 0) ? "*IN" : "*OUT";
            }

            return result;
        }
        public static string PointToPythonVector(this Point2D p)
        {
            return ToPythonVector(Vector<double>.Build.DenseOfArray(new double[] { p.X, p.Y }));
        }
        public static string PointToPythonVector(double x, double y)
        {
            return ToPythonVector(Vector<double>.Build.DenseOfArray(new double[] { x, y }));
        }
        public static string PointToPythonVector(params double[] components)
        {
            return ToPythonVector(Vector<double>.Build.DenseOfArray(components));
        }
    }
}
