using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using static System.Math;
using static ManimLib.Utils.SimpleFunctions;
using System.Linq;
using SliceAndDice;

namespace ManimLib.Utils
{
    public static class BezierUtil
    {
        public static Func<double, Vector<double>> Bezier(List<Vector<double>> points)
        {
            int n = points.Count - 1;
            return (double t) =>
            {
                var items = new List<Vector<double>>();
                for (int k = 0; k < points.Count; k++)
                {
                    items.Add(Pow(1 - t, n - k) * Pow(t, k) * Choose(n, k) * points[k]);
                }
                return ArrayUtilities.Sum(items.ToArray());
            };
        }
        public static Func<double, double> Bezier(List<double> points)
        {
            int n = points.Count - 1;
            return (double t) =>
            {
                var items = new List<double>();
                for (int k = 0; k < points.Count; k++)
                {
                    items.Add(Pow(1 - t, n - k) * Pow(t, k) * Choose(n, k) * points[k]);
                }
                return Enumerable.Sum(items);
            };
        }

        /// <summary>
        /// Given an array of points which define
        /// a bezier curve, and two numbers 0<=a<b<=1,
        /// return an array of the same size, which
        /// describes the portion of the original bezier
        /// curve on the interval[a, b].
        /// </summary>
        public static List<double> PartialBezierPoints(List<double> points, int a, int b)
        {
            //throw new NotImplementedException("This function definitely does not work.");
            // TODO: Check the parameter types of this function.
            // Should points be a list of Vectors or doubles?
            if (a == 1)
                return Enumerable.Repeat(points.Last(), points.Count).ToList();
            List<double> aTo1 = new List<double>();
            for (int i = 0; i < points.Count; i++)
            {
                aTo1.Add(Bezier(new List<double>() { points[i] })(a));
            }
            double endProp = (b - a) / (1.0 - a);
            List<double> output = new List<double>();
            for (int i = 0; i < points.Count; i++)
            {
                aTo1.Add(Bezier(new ArraySlice<double>(points).GetSlice($":{i + 1}").ToList())(a));
            }
            return output;
        }
    
        /// <summary>
        /// Performs a linear interpolation
        /// </summary>
        public static double Interpolate(double start, double end, double alpha)
        {
            return (1 - alpha) * start + alpha * end;
        }
        public static IEnumerable<double> Interpolate(IEnumerable<double> start, IEnumerable<double> end, double alpha)
        {
            foreach ((double s, double e) in start.Zip(end, (a, b) => (a, b)))
                yield return Interpolate(s, e, alpha);
        }
        public static Vector<double> Interpolate(Vector<double> start, Vector<double> end, double alpha)
        {
            return alpha * start + (1 - alpha) * end;
        }
        public static IEnumerable<Vector<double>> Interpolate(IEnumerable<Vector<double>> start, IEnumerable<Vector<double>> end, double alpha)
        {
            foreach ((Vector<double> s, Vector<double> e) in start.Zip(end, (a, b) => (a, b)))
                yield return Interpolate(s, e, alpha);
        }

        /// <summary>
        /// alpha is a float between 0 and 1.  This returns
        /// an integer between start and end(inclusive) representing
        /// appropriate interpolation between them, along with a
        /// "residue" representing a new proportion between the
        /// returned integer and the next one of the
        /// list.
        /// 
        /// For example, if start= 0, end= 10, alpha= 0.46, This
        /// would return (4, 0.6).
        /// </summary>
        public static (int, double) InterpolateInteger(int start, int end, double alpha)
        {
            if (alpha >= 1)
                return (end - 1, 1.0);
            if (alpha <= 0)
                return (start, 0);
            int value = (int)Interpolate(start, end, alpha);
            double residue = ((end - start) * alpha) % 1;
            return (value, residue);
        }

        public static double Mid(double start, double end)
        {
            return (start + end) / 2.0;
        }

        public static double InverseInterpolate(double start, double end, double value)
        {
            return (value - start) / (end - start);
        }

        public static double MatchInterpolate(double newStart, double newEnd, double oldStart, double oldEnd, double oldValue)
        {
            return Interpolate(
                newStart, newEnd,
                InverseInterpolate(oldStart, oldEnd, oldValue)
            );
        }

        public static void GetSmoothHandlePoints(List<double> points)
        {
            throw new NotImplementedException();
            // It looks like this function uses matrices to calculate the first
            // and second derivatives, which it then uses to find the tangent
            // and handle points;
        }

        /// <summary>
        /// Determines whether or not the shape defined by the given points is closed,
        /// within the given tolerance
        /// </summary>
        public static bool IsClosed(List<Vector<double>> points, double tolerance = 1E-5)
        {
            Vector<double> pointA = points.First();
            Vector<double> pointB = points.Last();
            for (int k = 0; k < pointA.Count; k++)
            {
                if (Abs(pointA[k] - pointB[k]) > tolerance)
                    return false;
            }
            return true;
        }
    }
}
