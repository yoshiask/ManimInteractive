using static System.Math;
using System.Collections.Generic;
using System.Numerics;
using System;
using System.Runtime.InteropServices;
using System.Linq;

namespace ManimLib.Math
{
    public static class SpaceOps
    {
        public static double[,] RotationMatrixTranspose(double angle, Vector3 axis)
        {
            if (axis.X == 0 && axis.Y == 0)
            {
                // axis = [0, 0, z] case is common enough it's worth having a shortcut
                int sign = Sign(axis.Z);
                double cos_a = Cos(angle);
                double sin_a = Sin(angle) * sign;
                return new double[,]
                {
                    { cos_a, sin_a, 0 },
                    { -sin_a, cos_a, 0 },
                    { 0, 0, 1 },
                };
            }

            //var quat = quaternion_from_angle_axis(angle, axis)
            //var quat_inv = quaternion_conjugate(quat)
            //return new double[,]
            //{
            //    quaternion_mult(quat, [0, *basis], quat_inv)[1:]
            //    for basis in
            //    {
            //        { 1, 0, 0 },
            //        { 0, 1, 0 },
            //        { 0, 0, 1 },
            //    }
            //};

            throw new NotImplementedException();
        }

        public static Quaternion QuaternionMultiply(params Quaternion[] quats)
        {
            if (quats.Length <= 0)
                return new Quaternion(0, 0, 0, 1);

            Quaternion result = quats[0];
            foreach (Quaternion q in quats.RangeSubset(1, quats.Length - 1))
            {
                result *= q;
            }
            return result;
        }

        public static (double, Vector3) AngleAxisFromQuaternion(Quaternion q)
        {
            var axis = Vector3.Normalize(new Vector3(q.X, q.Y, q.Z));
            var angle = 2 * Acos(q.W);
            if (angle > PI)
                angle = 2 * PI - angle;
            return (angle, axis);
        }

        public static Vector2 RotateVector(Vector2 v, double angle)
        {
            var newV = new Complex(v.X, v.Y) * Complex.Exp(new Complex(0, angle));
            return new Vector2((float)newV.Real, (float)newV.Imaginary);
        }

        public static Vector3 RotateVector(Vector3 v, double angle, [Optional] Vector3 axis)
        {
            if (axis == null)
                axis = Vector3.UnitZ;

            var quat = Quaternion.CreateFromAxisAngle(axis, (float)angle);
            var quatInv = Quaternion.Conjugate(quat);
            var product = quat * new Quaternion(v, 0) * quatInv;
            return new Vector3(product.X, product.Y, product.Z);
        }

        public static byte[,] ThickDiagonal(int dimension, int thickness = 2)
        {
            int[,] rowIndicies = ArrayUtilities.Arange(dimension).ToList().Repeat(dimension).Reshape((dimension, dimension));
            int[,] colIndicies = rowIndicies.Transpose();
            var final = rowIndicies.Combine(colIndicies,
                (r, c) =>
                {
                    return (Abs(r - c) < thickness) ? 1 : 0;
                }
            ).CastToByteArray();
            return final;
        }

        public static double[,] RotationMatrix(double angle, Vector3 axis)
        {
            return RotationMatrixTranspose(angle, axis).Transpose();
        }

        public static double[,] RotationAboutZAxis(double angle)
        {
            return new double[,] {
                { Cos(angle), -Sin(angle), 0 },
                { Sin(angle), Cos(angle), 0 },
                { 0, 0, 1 }
            };
        }
    }
}
