using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManimLib
{
    public class Common
    {
        public static string ManimDirectory { get; set; } = Environment.GetEnvironmentVariable("MANIM_PATH", EnvironmentVariableTarget.User);
        public static string ManimLibDirectory { get; } = System.IO.Path.Combine(ManimDirectory, "manimlib");

        public static readonly Dictionary<string, string> Colors = new Dictionary<string, string>() {
            { "DARK_BLUE", "#236B8E" },
            { "DARK_BROWN", "#8B4513" },
            { "LIGHT_BROWN", "#CD853F" },
            { "BLUE_E", "#1C758A" },
            { "BLUE_D", "#29ABCA" },
            { "BLUE_C", "#58C4DD" },
            { "BLUE_B", "#9CDCEB" },
            { "BLUE_A", "#C7E9F1" },
            { "TEAL_E", "#49A88F" },
            { "TEAL_D", "#55C1A7" },
            { "TEAL_C", "#5CD0B3" },
            { "TEAL_B", "#76DDC0" },
            { "TEAL_A", "#ACEAD7" },
            { "GREEN_E", "#699C52" },
            { "GREEN_D", "#77B05D" },
            { "GREEN_C", "#83C167" },
            { "GREEN_B", "#A6CF8C" },
            { "GREEN_A", "#C9E2AE" },
            { "YELLOW_E", "#E8C11C" },
            { "YELLOW_D", "#F4D345" },
            { "YELLOW_C", "#FFFF00" },
            { "YELLOW_B", "#FFEA94" },
            { "YELLOW_A", "#FFF1B6" },
            { "GOLD_E", "#C78D46" },
            { "GOLD_D", "#E1A158" },
            { "GOLD_C", "#F0AC5F" },
            { "GOLD_B", "#F9B775" },
            { "GOLD_A", "#F7C797" },
            { "RED_E", "#CF5044" },
            { "RED_D", "#E65A4C" },
            { "RED_C", "#FC6255" },
            { "RED_B", "#FF8080" },
            { "RED_A", "#F7A1A3" },
            { "MAROON_E", "#94424F" },
            { "MAROON_D", "#A24D61" },
            { "MAROON_C", "#C55F73" },
            { "MAROON_B", "#EC92AB" },
            { "MAROON_A", "#ECABC1" },
            { "PURPLE_E", "#644172" },
            { "PURPLE_D", "#715582" },
            { "PURPLE_C", "#9A72AC" },
            { "PURPLE_B", "#B189C6" },
            { "PURPLE_A", "#CAA3E8" },
            { "WHITE", "#FFFFFF" },
            { "BLACK", "#000000" },
            { "LIGHT_GRAY", "#BBBBBB" },
            { "LIGHT_GREY", "#BBBBBB" },
            { "GRAY", "#888888" },
            { "GREY", "#888888" },
            { "DARK_GREY", "#444444" },
            { "DARK_GRAY", "#444444" },
            { "GREY_BROWN", "#736357" },
            { "PINK", "#D147BD" },
            { "GREEN_SCREEN", "#00FF00" },
            { "ORANGE", "#FF862F" },
        };

        public const int PixelHeight = 1440;
        public const int PixelWidth = 2560;
        public const double FrameHeight = 8;
        public const double FrameWidth = (8 * PixelWidth / PixelHeight);
        //public static readonly Point FrameOrigin = new Point(FrameWidth / 2, FrameHeight / 2);

        public const string PY_TAB = @"    ";
        public const string PythonSceneHeader = "#!/usr/bin/env python\r\n\r\nfrom manimlib.imports import *\r\n\r\n";
    }

    public static class StringExtensions
    {
        public static string[] Lines(this string s)
        {
            string[] sep = { @"\r\n" };
            return s.Split(sep, StringSplitOptions.None);
        }
    }

    public static class ArrayUtilities
    {
        // create a subset from a range of indices
        public static T[] RangeSubset<T>(this T[] array, int startIndex, int length)
        {
            T[] subset = new T[length];
            Array.Copy(array, startIndex, subset, 0, length);
            return subset;
        }
        public static double[] RangeSubset(this Vector<double> array, int startIndex, int length)
        {
            return array.ToArray().RangeSubset(startIndex, length);
        }

        // create a subset from a specific list of indices
        public static T[] Subset<T>(this T[] array, params int[] indices)
        {
            T[] subset = new T[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                subset[i] = array[indices[i]];
            }
            return subset;
        }

        public static Array Zeros(Type dataType, int dim, int length)
        {
            int[] lengths = new int[dim];
            for (int d = 0; d < dim; d++)
            {
                lengths[d] = length;
            }
            return Array.CreateInstance(dataType, lengths);
        }
        public static Array Zeros(Type dataType, (int, int) shape)
        {
            return Zeros(dataType, shape.Item1, shape.Item2);
        }
        public static Array Zeros(Type dataType, Tuple<int, int> shape)
        {
            return Zeros(dataType, shape);
        }

        /// <summary>
        /// Generates an array of ints where the value at index i is i
        /// (e.g. Arange(4) = [ 0, 1, 2, 3 ])
        /// </summary>
        public static IEnumerable<int> Arange(int length)
        {
            for (int i = 0; i < length; i++) yield return i;
        }

        /// <summary>
        /// Repeats every item the specified number of times
        /// </summary>
        public static T[] Repeat<T>(this IList<T> items, int repeat)
        {
            T[] output = new T[items.Count * repeat];
            for (int i = 0; i < items.Count; i++)
            {
                for (int r = 0; r < repeat; r++)
                {
                    output[i * repeat + r] = items[i];
                }
            }
            return output;
        }

        public static T[,] Reshape<T>(this IList<T> items, Tuple<int, int> shape)
        {
            var output = new T[shape.Item1, shape.Item2];

            for (int i = 0; i < items.Count; i++)
            {
                output[i / shape.Item1, i % shape.Item1] = items[i];
            }
            return output;
        }
        public static T[,] Reshape<T>(this IList<T> items, (int, int) shape)
        {
            return items.Reshape(new Tuple<int,int>(shape.Item1, shape.Item2));
        }

        /// <summary>
        /// Swaps the 'rows' and 'columns' with each each other, which reverses the dimensions
        /// </summary>
        public static T[,] Transpose<T>(this T[,] items)
        {
            var output = new T[items.GetLength(1), items.GetLength(0)];
            for (int j = 0; j < items.GetLength(1); j++)
                for (int r = 0; r < items.GetLength(0); r++)
                    output[j, r] = items[r, j];

            return output;
        }

        public static T[,] Combine<T>(this T[,] arrayA, T[,] arrayB, Func<T, T, T> combiningFunc)
        {
            int width = arrayA.GetLength(0);
            int height = arrayA.GetLength(1);

            if (width != arrayB.GetLength(0) || height != arrayB.GetLength(1))
                throw new ArgumentException("Arrays must be the same shape");

            var output = new T[width, height];
            for (int j = 0; j < height; j++)
                for (int r = 0; r < width; r++)
                    output[r, j] = combiningFunc(arrayA[r, j], arrayB[r, j]);

            return output;
        }

        public static int[,] Subtract(this int[,] arrayA, int[,] arrayB)
        {
            int width = arrayA.GetLength(0);
            int height = arrayA.GetLength(1);

            if (width != arrayB.GetLength(0) || height != arrayB.GetLength(1))
                throw new ArgumentException("Arrays must be the same shape");

            var output = new int[width, height];
            for (int j = 0; j < height; j++)
                for (int r = 0; r < width; r++)
                    output[r, j] = arrayA[r, j] - arrayB[r, j];

            return output;
        }
        public static double[,] Subtract(this double[,] arrayA, double[,] arrayB)
        {
            int width = arrayA.GetLength(0);
            int height = arrayA.GetLength(1);

            if (width != arrayB.GetLength(0) || height != arrayB.GetLength(1))
                throw new ArgumentException("Arrays must be the same shape");

            var output = new double[width, height];
            for (int j = 0; j < height; j++)
                for (int r = 0; r < width; r++)
                    output[r, j] = arrayA[r, j] - arrayB[r, j];

            return output;
        }

        public static int[,] Multiply(this int[,] matrix, int scalar)
        {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);

            var output = new int[width, height];
            for (int j = 0; j < height; j++)
                for (int r = 0; r < width; r++)
                    output[r, j] = matrix[r, j] * scalar;

            return output;
        }
        public static double[,] Multiply(this double[,] matrix, double scalar)
        {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);

            var output = new double[width, height];
            for (int j = 0; j < height; j++)
                for (int r = 0; r < width; r++)
                    output[r, j] = matrix[r, j] * scalar;

            return output;
        }
        public static double[] Multiply(this double[] matrix, double scalar)
        {
            var output = new double[matrix.Length];
            for (int j = 0; j < matrix.Length; j++)
                output[j] = matrix[j] * scalar;

            return output;
        }

        public static int[,] Abs(this int[,] nums)
        {
            int width = nums.GetLength(0);
            int height = nums.GetLength(1);
            var output = new int[width, height];
            for (int j = 0; j < height; j++)
                for (int r = 0; r < width; r++)
                    output[r, j] = System.Math.Abs(nums[r, j]);

            return output;
        }

        public static byte[,] CastToByteArray(this int[,] input)
        {
            var output = new byte[input.GetLength(0), input.GetLength(1)];
            for (int j = 0; j < input.GetLength(1); j++)
                for (int r = 0; r < input.GetLength(0); r++)
                    output[r, j] = (byte)input[r, j];

            return output;
        }
        public static double[,] CastToDoubleArray(this int[,] input)
        {
            var output = new double[input.GetLength(0), input.GetLength(1)];
            for (int j = 0; j < input.GetLength(1); j++)
                for (int r = 0; r < input.GetLength(0); r++)
                    output[r, j] = (double)input[r, j];

            return output;
        }

        public static string ToMatrixString<T>(this T[,] matrix, string delimiter = "\t")
        {
            var s = new StringBuilder();

            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(1); j++)
                {
                    s.Append(matrix[i, j]).Append(delimiter);
                }

                s.AppendLine();
            }

            return s.ToString();
        }

        public static int Sum(this int[] nums)
        {
            int sum = 0;
            foreach (int n in nums)
                sum += n;
            return sum;
        }
        public static Vector<double> Sum(this Vector<double>[] vectors)
        {
            Vector<double> sum = Vector<double>.Build.DenseOfArray(new double[] { 0, 0, 0 });
            foreach (Vector<double> n in vectors)
                sum += n;
            return sum;
        }
        public static Vector3D Sum(this Vector3D[] vectors)
        {
            Vector3D sum = new Vector3D( 0, 0, 0 );
            foreach (Vector3D n in vectors)
                sum += n;
            return sum;
        }
        public static Vector2D Sum(this Vector2D[] vectors)
        {
            Vector2D sum = new Vector2D(0, 0);
            foreach (Vector2D n in vectors)
                sum += n;
            return sum;
        }

        public static double[,] DotProduct3x3(this double[,] mA, double[,] mB)
        {
            if (mA.GetLength(0) == 3 && mA.GetLength(1) == 3 &&
                mB.GetLength(0) == 3 && mB.GetLength(1) == 3)
                throw new ArgumentException("Both matrices must be 3x3");

            return new double[,]
            {
                { mA[0,0]*mB[0,0] + mA[0,1]*mB[1,0] + mA[0,2]*mB[2,0],
                  mA[0,0]*mB[0,1] + mA[0,1]*mB[1,1] + mA[0,2]*mB[2,1],
                  mA[0,0]*mB[0,2] + mA[0,1]*mB[1,2] + mA[0,2]*mB[2,2]  },

                { mA[1,0]*mB[0,0] + mA[1,1]*mB[1,0] + mA[1,2]*mB[2,0],
                  mA[1,0]*mB[0,1] + mA[1,1]*mB[1,1] + mA[1,2]*mB[2,1],
                  mA[1,0]*mB[0,2] + mA[1,1]*mB[1,2] + mA[1,2]*mB[2,2]  },

                { mA[2,0]*mB[0,0] + mA[2,1]*mB[1,0] + mA[2,2]*mB[2,0],
                  mA[2,0]*mB[0,1] + mA[2,1]*mB[1,1] + mA[2,2]*mB[2,1],
                  mA[2,0]*mB[0,2] + mA[2,1]*mB[1,2] + mA[2,2]*mB[2,2]  }
            };
        }

        public static double[,][,] OuterProduct(this double[,] mA, double[,] mB)
        {
            int aWidth = mA.GetLength(0);
            int aHeight = mA.GetLength(1);

            var output = new double[aWidth, aHeight][,];
            for (int j = 0; j < aHeight; j++)
                for (int r = 0; r < aWidth; r++)
                    output[r, j] = mB.Multiply(mA[r, j]);

            return output;
        }

        public static double[][] OuterProduct(this double[] mA, double[] mB)
        {
            var output = new double[mA.Length][];
            for (int j = 0; j < mA.Length; j++)
                output[j] = mB.Multiply(mA[j]);

            return output;
        }

        public static T[,] To2D<T>(this T[][] source)
        {
            try
            {
                int FirstDim = source.Length;
                int SecondDim = source.GroupBy(row => row.Length).Single().Key; // throws InvalidOperationException if source is not rectangular

                var result = new T[FirstDim, SecondDim];
                for (int i = 0; i < FirstDim; ++i)
                    for (int j = 0; j < SecondDim; ++j)
                        result[i, j] = source[i][j];

                return result;
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("The given jagged array is not rectangular.");
            }
        }

        /// <summary>
        /// Performs an element-wise multiplication
        /// </summary>
        public static Vector<double> Multiply(this Vector<double> vA, Vector<double> vB)
        {
            double[] newV = vA.ToArray();
            for (int i = 0; i < vA.Count && i < vB.Count; i++)
            {
                newV[i] *= vB[i];
            }
            return Vector<double>.Build.DenseOfArray(newV);
        }
    }
}
