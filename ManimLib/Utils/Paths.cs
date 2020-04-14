using ManimLib.Visuals;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;
using static ManimLib.Constants;

namespace ManimLib.Utils
{
    public static class Paths
    {
        public const double STRAIGHT_PATH_THRESHOLD = 0.01;

        /// <summary>
        /// Same function as interpolate, but renamed to reflect
        /// intent of being used to determine how a set of points move
        /// to another set.For instance, it should be a specific case
        /// of path_along_arc
        /// </summary>
        public static IEnumerable<Vector<double>> StraightPath(IEnumerable<Vector<double>> startPoints, IEnumerable<Vector<double>> endPoints, double alpha)
        {
            return BezierUtil.Interpolate(startPoints, endPoints, alpha);
        }
    
        public static Func<IEnumerable<Vector<double>>, IEnumerable<Vector<double>>, double, IEnumerable<Vector<double>>>
            PathAlongArc(double arcAngle, Vector<double> axis = null)
        {

            if (Abs(arcAngle) < STRAIGHT_PATH_THRESHOLD)
                return StraightPath;
            if (axis == null || axis.L2Norm() == 0)
                axis = OUT;
            Vector<double> unitAxis = axis / axis.L2Norm();
            return (IEnumerable<Vector<double>> startPoints, IEnumerable<Vector<double>> endPoints, double alpha) =>
            {
                IEnumerable<Vector<double>> vects = endPoints.Zip(startPoints, (e, s) => e - s);
                IEnumerable<Vector<double>> centers = startPoints.Zip(vects, (s, v) => s + 0.5 * v);
                if (arcAngle != PI)
                {
                    for (int i = 0; i < centers.Count(); i++)
                    {
                        Vector<double> center = centers.ElementAt(i);
                        center +=
                            MathNet.Spatial.Euclidean.Vector2D.OfVector(unitAxis).CrossProduct(
                                MathNet.Spatial.Euclidean.Vector2D.OfVector(vects.ElementAt(i) / 2.0)) / Tan(arcAngle / 2);
                    }
                }
                Matrix<double> rotMatrix = SpaceOps.RotationMatrix(alpha * arcAngle, unitAxis);
                throw new NotImplementedException();
                /*return Common.NewVector(centers.Zip(
                    startPoints, (c, s) => (s - c).ToRowMatrix().Multiply(rotMatrix).ToRowMajorArray()
                ));*/
            };
        }

        public static Func<IEnumerable<Vector<double>>, IEnumerable<Vector<double>>, double, IEnumerable<Vector<double>>>
            ClockwisePath()
        {
            return PathAlongArc(-PI);
        }

        public static Func<IEnumerable<Vector<double>>, IEnumerable<Vector<double>>, double, IEnumerable<Vector<double>>>
            CounterClockwisePath()
        {
            return PathAlongArc(PI);
        }
    }
}
