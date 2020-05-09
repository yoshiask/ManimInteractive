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
        public static List<Vector<double>> PartialBezierPoints(List<Vector<double>> points, double a, double b)
        {
            //throw new NotImplementedException("This function definitely does not work.");
            // TODO: Check the parameter types of this function.
            // Should points be a list of Vectors or doubles?
            if (a == 1)
                return Enumerable.Repeat(points[^1], points.Count).ToList();
            List<Vector<double>> aTo1 = new List<Vector<double>>();
            for (int i = 0; i < points.Count; i++)
            {
                aTo1.Add(Bezier(new List<Vector<double>>() { points[i] })(a));
            }
            double endProp = (b - a) / (1.0 - a);
            List<Vector<double>> output = new List<Vector<double>>();
            for (int i = 0; i < points.Count; i++)
            {
                aTo1.Add(Bezier(new ArraySlice<Vector<double>>(points).GetSlice($":{i + 1}").ToList())(a));
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

        // Figuring out which bezier curves most smoothly connect a sequence of points

        public static Tuple<List<Vector<double>>, List<Vector<double>>> GetSmoothHandlePoints(List<Vector<double>> points)
        {
            // It looks like this function uses matrices to calculate the first
            // and second derivatives, which it then uses to find the tangent
            // and handle points.

            int numHandles = points.Count - 1;
            int dim = points[0].Count;
            if (numHandles < 1)
                return new Tuple<List<Vector<double>>, List<Vector<double>>>(
                    SpaceOps.GetZeroVector(dim).Repeat(0), SpaceOps.GetZeroVector(dim).Repeat(0)
                );
            // Must solve 2*numHandles equations to get the handles.
            // l and u are the number of lower and upper diagonal rows
            // in the matrix to solve.
            int l = 2;
            int u = 1;
            // diag is a representation of the matrix in diagonal form
            // See https://www.particleincell.com/2012/bezier-splines/
            // for how to arrive at these equations
            double[,] diagBase = new double[l + u + 1, 2 * numHandles];
            diagBase.ChangeRowSlice(new double[] { -1 }, 0, start: 0, step: 2);
            diagBase.ChangeRowSlice(new double[] { 1 }, 0, start: 2, step: 2);
            diagBase.ChangeRowSlice(new double[] { 2 }, 1, start: 0, step: 2);
            diagBase.ChangeRowSlice(new double[] { 1 }, 1, start: 1, step: 2);
            diagBase.ChangeRowSlice(new double[] { -2 }, 2, start: 1, end: -2, step: 2);
            diagBase.ChangeRowSlice(new double[] { -2 }, 3, start: 0, end: -3, step: 2);
            // Last
            diagBase[2, -2] = -1;
            diagBase[1, -1] = 2;
            var diag = Matrix<double>.Build.DenseOfArray(diagBase);

            // This is the b as in Ax = b, where we are solving for x,
            // and A is represented using diag.  However, think of entries
            // to x and b as being points in space, not numbers
            double[,] bBase = new double[2 * numHandles, dim];
            var pointsx2 = points.Skip(1).Select(v => v * 2).ToList();
            for (int d = 0; d < dim; d++)
            {
                // TODO: Test equivalency with b[1::2] = 2 * points[1:]
                bBase.ChangeRowSlice(pointsx2[d], row: d,
                    start: 1, step: 2);
            }
            bBase.ChangeRowSlice(points[0], 0); // b[0] = points[0]
            bBase.ChangeRowSlice(points[^1], -1); // b[-1] = points[-1]
            var b = Matrix<double>.Build.DenseOfArray(bBase);

            bool useClosedSolveFunction = IsClosed(points);
            if (useClosedSolveFunction)
            {
                // Get equations to relate first and last points

                // Here's where things go really bonkers, so just throw
                // a NotImplementedException until I figure it out
                throw new NotImplementedException("yoshiask doesn't know how to do matrix multiplication");
            }

            double[,] handlePairs = new double[2 * numHandles, dim];
            for (int i = 0; i < dim; i++)
            {
                //if (useClosedSolveFunction)
                //    handle_pairs[:, i] = closedCurveSolveFunc(b[:, i]);
                //else
                //    handle_pairs[:, i] = solve_func(b[:, i]);
            }
            // TODO: This iterates over the list several times because of all the LINQ
            return new Tuple<List<Vector<double>>, List<Vector<double>>>(
                handlePairs.ToJagged().Slice(start: 0, step: 2).Select(a => Common.NewVector(a)).ToList(),
                handlePairs.ToJagged().Slice(start: 1, step: 2).Select(a => Common.NewVector(a)).ToList()
            );
        }

        /// <summary>
        /// Converts array whose rows represent diagonal
        /// entries of a matrix into the matrix itself.
        /// </summary>
        public static double[,] DiagToMatrix(int l, int u, double[,] diag)
        {
            // TODO: Figure this out
            throw new NotImplementedException("yoshiask has not figured out how to implement this");
            int dim = diag.GetLength(1);
            double[,] matrix = new double[dim, dim];
            for (int i = 0; i < l + u + 1; i++)
            {

            }
        }

        public static double[,] FillDiagonal(double[,] a, double value, bool wrap = false)
        {
            for (int i = 0; i < a.GetLength(0); i++)
            {
                a[i, i] = value;
            }
            return a;
        }

        /// <summary>
        /// Determines whether or not the shape defined by the given points is closed,
        /// within the given tolerance
        /// </summary>
        public static bool IsClosed(List<Vector<double>> points, double tolerance = 1E-5)
        {
            Vector<double> pointA = points[0];
            Vector<double> pointB = points[^1];
            for (int k = 0; k < pointA.Count; k++)
            {
                if (Abs(pointA[k] - pointB[k]) > tolerance)
                    return false;
            }
            return true;
        }
    }
}
