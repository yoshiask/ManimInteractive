using static System.Math;
using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using static ManimLib.Common;
using static ManimLib.Constants;

namespace ManimLib.Utils
{
    public static class SpaceOps
    {
        public const int X = 0;
        public const int Y = 1;
        public const int Z = 2;
        public const int W = 3;

        [Obsolete("Use v.L2Norm()", true)]
        public static double GetNorm(Vector<double> v)
        {
            return v.L2Norm();
        }

        public static Vector<double> Normalize(Vector<double> v)
        {
            return v.Normalize(v.L2Norm());
        }

        public static Quaternion QuaternionMultiply(params Quaternion[] quats)
        {
            if (quats.Length <= 0)
                return new Quaternion(0, 0, 0, 1);

            Quaternion result = quats[0];
            foreach (Quaternion q in quats.Slice(1, quats.Length - 1))
            {
                result *= q;
            }
            return result;
        }

        public static Quaternion QuaternionFromAngleAxis(double angle, Vector<double> axis)
        {
            double s = Sin(angle / 2);
            return new Quaternion(axis[X] * s, axis[Y] * s, axis[Z] * s, Cos(angle / 2));
        }

        public static (double, Vector<double>) AngleAxisFromQuaternion(Quaternion q)
        {
            var axis = Vector.Build.DenseOfArray(new double[] { q.ImagX, q.ImagY, q.ImagZ });
            var axisNorm = axis.Normalize(axis.L2Norm());
            var angle = 2 * Acos(q.Real);
            if (angle > PI)
                angle = 2 * PI - angle;
            return (angle, axisNorm);
        }

        public static Vector<double> RotateVector(Vector<double> v, double angle)
        {
            var newV = new System.Numerics.Complex(v[0], v[1]) *
                System.Numerics.Complex.Exp(new System.Numerics.Complex(0, angle));
            return NewVector(new double[] { newV.Real, newV.Imaginary });
        }

        public static Vector<double> RotateVector(Vector<double> v, double angle, Vector<double> axis = null)
        {
            if (axis == null)
                axis = Vector.Build.SparseOfArray(new double[] { 0, 0, 1 });

            var quat = System.Numerics.Quaternion.CreateFromAxisAngle(
                new System.Numerics.Vector3((float)axis[0], (float)axis[1], (float)axis[2]), (float)angle);
            var quatInv = System.Numerics.Quaternion.Conjugate(quat);
            var product = quat * new System.Numerics.Quaternion(
                new System.Numerics.Vector3((float)v[0], (float)v[1], (float)v[2]), 0) * quatInv;
            return Vector.Build.DenseOfArray(new double[] { product.X, product.Y, product.Z });
        }

        // The following function is defined in manimpy, but it has literally zero
        // references and is causing issues, so bye-bye!
        //public static Matrix<double> ThickDiagonal(int dimension, int thickness = 2)
        //{
        //    int[,] rowIndicies = ArrayUtilities.Arange(dimension).ToList().Repeat(dimension).Reshape((dimension, dimension));
        //    int[,] colIndicies = rowIndicies.Transpose();
        //    var final = rowIndicies.Combine(colIndicies,
        //        (r, c) =>
        //        {
        //            return (Abs(r - c) < thickness) ? 1 : 0;
        //        }
        //    ).CastToDoubleArray();
        //    return NewMatrix(final);
        //}

        //public static Matrix<double> GetMatrixIdentity(int dimension)
        //{
        //    return ThickDiagonal(dimension, 1);
        //}

        public static Matrix<double> RotationMatrix(double angle, Vector<double> axis)
        {
            Matrix<double> aboutZ = RotationAboutZAxis(angle);
            Matrix<double> ZtoAxis = ZToVector(axis);
            Matrix<double> axisToZ = ZtoAxis.Inverse();
            return ZtoAxis * aboutZ * axisToZ;
        }

        public static Matrix<double> RotationAboutZAxis(double angle)
        {
            return NewMatrix(new double[,] {
                { Cos(angle), -Sin(angle), 0 },
                { Sin(angle), Cos(angle), 0 },
                { 0, 0, 1 }
            });
        }

        public static Matrix<double> ZToVector(Vector<double> initV)
        {
            double norm = initV.L2Norm();
            if (norm == 0)
                return Matrix<double>.Build.DiagonalIdentity(3);
            Vector<double> v = initV / norm;
            double phi = Acos(v[2]);
            double theta;
            if (v[X] != 0 || v[Y] != 0)
            {
                // Projection of vector on unit circle
                Vector<double> axisProjection = Vector.Build.DenseOfArray(v.RangeSubset(0, 2));
                axisProjection = axisProjection / axisProjection.L2Norm();
                theta = Acos(axisProjection[X]);
                if (axisProjection[Y] < 0)
                    theta = -theta;
            }
            else
            {
                theta = 0;
            }
            Matrix<double> phiDown = NewMatrix(new double[,]
            {
                { Cos(phi),  0, Sin(phi) },
                { 0,         1, 0        },
                { -Sin(phi), 0, Cos(phi) }
            });
            
            return RotationAboutZAxis(theta) * phiDown;
        }

        public static double GetVectorAngle(Vector<double> v)
        {
            return new System.Numerics.Complex(v[X], v[Y]).Phase;
        }

        public static double AngleBetweenVectors(Vector<double> vA, Vector<double> vB)
        {
            double diff = GetVectorAngle(vA) - GetVectorAngle(vB) % (2*PI);
            return Min(diff, 2 * PI - diff);
        }

        public static Vector<double> ProjectAlongVector(Vector<double> point, Vector<double> vector)
        {
            Matrix<double> matrix = Matrix<double>.Build.DiagonalIdentity(3) - vector.OuterProduct(vector);
            return point * matrix;
        }

        public static Vector<double> GetUnitNormal(Vector<double> v1, Vector<double> v2)
        {
            return Vector3D.OfVector(v1).CrossProduct(Vector3D.OfVector(v2)).Normalize().ToVector();
        }

        public static IEnumerable<Vector<double>> CompassDirections(Vector<double> startVector = null, int n = 4)
        {
            if (startVector == null)
                startVector = NewVector(new double[] { 1, 0, 0 });

            double angle = PI * 2 / n;
            for (int k = 0; k < n; k++)
            {
                yield return RotateVector(startVector, k * angle);
            }
        }
    
        public static Vector<double> ComplexToR3(System.Numerics.Complex num)
        {
            return NewVector(new double[] { num.Real, num.Imaginary, 0 });
        }

        public static System.Numerics.Complex R3ToComplex(Vector<double> point)
        {
            return new System.Numerics.Complex(point[0], point[1]);
        }

        public static Func<Vector<double>, Vector<double>> ComplexFuncToR3Func(
            Func<System.Numerics.Complex, System.Numerics.Complex> complexFunc)
        {
            return (vector) =>
            {
                return ComplexToR3(complexFunc(R3ToComplex(vector)));
            };
        }

        public static Vector<double> CenterOfMass(params Vector<double>[] points)
        {
            return points.Sum() / points.Length;
        }
        public static Vector2D CenterOfMass(params Vector2D[] points)
        {
            return points.Sum() / points.Length;
        }
        public static Vector3D CenterOfMass(params Vector3D[] points)
        {
            return points.Sum() / points.Length;
        }

        public static Vector<double> Midpoint(Vector<double> vA, Vector<double> vB)
        {
            return CenterOfMass(vA, vB);
        }
        public static Vector2D Midpoint(Vector2D vA, Vector2D vB)
        {
            return CenterOfMass(vA, vB);
        }
        public static Vector3D Midpoint(Vector3D vA, Vector3D vB)
        {
            return CenterOfMass(vA, vB);
        }

        public static Vector<double> LineIntersection((Vector<double>, Vector<double>) lineA, (Vector<double>, Vector<double>) lineB)
        {
            var xDiff = (lineA.Item1[X] - lineA.Item2[X], lineB.Item1[X] - lineB.Item2[X]);
            var yDiff = (lineA.Item1[Y] - lineA.Item2[Y], lineB.Item1[Y] - lineB.Item2[Y]);
            double div = Det(xDiff, yDiff);
            if (div == 0)
                throw new Exception("Lines do not intersect");
            var d = (Det(lineA.Item1, lineA.Item2), Det(lineB.Item1, lineB.Item2));
            double x = Det(d, xDiff) / div;
            double y = Det(d, yDiff) / div;
            return NewVector(new double[] { x, y, 0 });
        }

        private static double Det((double, double) a, (double, double) b)
        {
            return a.Item1 * b.Item2 - a.Item2 * b.Item1;
        }
        /// <summary>
        /// Also known as cross2d() for some reason
        /// </summary>
        private static double Det(Vector<double> a, Vector<double> b)
        {
            return a[X] * b[Y] - a[Y] * b[X];
        }
        private static double Det(Vector2D a, Vector2D b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        /// <summary>
        /// Return the intersection of a line passing through p0 in direction v0
        /// with one passing through p1 in direction v1. (Or array of intersections
        /// from arrays of such points/directions).
        /// For 3D values, it returns the point on the ray p0 + v0* t closest to the
        /// ray p1 + v1 * t
        /// </summary>
        [Obsolete("This function is not complete (and likely never will be). Use Line2D.IntersectWith()", true)]
        public static Vector<double> FindIntersection(Vector<double> p0, Vector<double> v0, Vector<double> p1, Vector<double> v1)
        {
            int n = p0.ToRowMatrix().ColumnCount;
            if (n != 2 || n != 3)
                throw new ArgumentException("p0 must be a two- or three-dimensional vector");

            double numerator, denominator;

            if (n == 2)
            {
                numerator = Vector2D.OfVector(v1).CrossProduct(Vector2D.OfVector(p1 - p0));
                denominator = Vector2D.OfVector(v1).CrossProduct(Vector2D.OfVector(v0));
            }
            else if (n == 3)
            {
                Vector3D preNumerator = Vector3D.OfVector(v1).CrossProduct(Vector3D.OfVector(p1 - p0));
                Vector3D preDenominator = Vector3D.OfVector(v1).CrossProduct(Vector3D.OfVector(v0));

                int d = 1; // In the og code, this line is "d = len(np.shape(numer))"
                numerator = preNumerator.ToVector().Multiply(preNumerator.ToVector()).Sum();
                numerator = preDenominator.ToVector().Multiply(preNumerator.ToVector()).Sum();
            }

            throw new Exception("This function is not complete (and likely never will be). Use Line2D.IntersectWith()");
        }
    
        public static double TriangleArea(Vector<double> a, Vector<double> b, Vector<double> c)
        {
            return 0.5 * Abs(
                a[0] * (b[1] - c[1]) +
                b[0] * (c[1] - a[1]) +
                c[0] * (a[1] - b[1])
            );
        }
    
        public static bool IsInsideTriangle(Vector<double> point, Vector<double> a, Vector<double> b, Vector<double> c)
        {
            double[] crosses =
            {
                Det(point - a, b - point),
                Det(point - b, c - point),
                Det(point - c, a - point)
            };
            return crosses.All(cross => cross > 0);
        }
    
        public static double GetWindingNumber(IList<Vector<double>> points)
        {
            double totalAngle = 0;
            foreach (Tuple<Vector<double>, Vector<double>> pair in Iterables.AdjacentPairs(points))
            {
                double dAngle = GetVectorAngle(pair.Item2) - GetVectorAngle(pair.Item1);
                dAngle = ((dAngle + PI) % TAU) - PI;
                totalAngle += dAngle;
            }
            return totalAngle / TAU;
        }

        // Every function after this point is not built into the Python Manim library

        public static Vector<double> GetZeroVector(int dimension)
        {
            return NewVector(Enumerable.Range(0, dimension).Cast<double>().ToArray());
        }

        public static IEnumerable<Vector<double>> SubdivideLine(IList<Vector<double>> points, int numOfSubdivisions = 1)
        {
            if (numOfSubdivisions <= 0)
                return points;
            if (points.Count < 2)
                throw new ArgumentException("Cannot subdivide a line with no length");

            var lineSegments = Iterables.AdjacentPairs(points);
            List<Vector<double>> newPoints = new List<Vector<double>>();
            foreach (Tuple<Vector<double>, Vector<double>> segment in lineSegments)
            {
                newPoints.Add(segment.Item1);
                newPoints.Add(Midpoint(segment.Item1, segment.Item2));
                newPoints.Add(segment.Item2);
            }
            return SubdivideLine(newPoints, numOfSubdivisions - 1);
        }
        public static IEnumerable<Vector<double>> SubdivideLine(int numOfSubdivisions = 1, params Vector<double>[] points)
        {
            return SubdivideLine(points.ToList(), numOfSubdivisions);
        }
    }
}
