using ManimLibTest.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Svg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using System.Text;
using System.Drawing;

namespace ManimLibTest
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestSubsetSlice()
        {
            double[] array = new double[] { 0, 1, 2, 3, 4, 5 };

            var slice = array.Slice(start: -1);
            CollectionAssert.IsSubsetOf(slice, array);
            CollectionAssert.AreEqual(new double[] { 5 }, slice, "Failed to get last elements\r\narray[-1:]");

            slice = array.Slice(start: 1, end: 3);
            CollectionAssert.IsSubsetOf(slice, array);
            CollectionAssert.AreEqual(new double[] { 1, 2 }, slice, "Failed to get subset (+ indexes)\r\narray[1:3]");

            slice = array.Slice(end: -1);
            CollectionAssert.IsSubsetOf(slice, array);
            CollectionAssert.AreEqual(new double[] { 0, 1, 2, 3, 4 }, slice, "Failed to get all but last element\r\narray[:-1]");

            slice = array.Slice(start: -2, end: -1);
            CollectionAssert.IsSubsetOf(slice, array);
            CollectionAssert.AreEqual(new double[] { 4 }, slice, "Failed to get second-to-last element\r\narray[-2:-1]");
        }

        [TestMethod]
        public void TestStepSlice()
        {
            double[] array = new double[] { 0, 1, 2, 3, 4, 5 };

            var slice = array.Slice(step: 1);
            CollectionAssert.IsSubsetOf(slice, array);
            CollectionAssert.AreEqual(array, slice, "Failed to evaluate array[::1]");

            slice = array.Slice(step: 2);
            CollectionAssert.IsSubsetOf(slice, array);
            CollectionAssert.AreEqual(new double[] { 0, 2, 4 }, slice, "Failed to evaluate array[::2]");

            slice = array.Slice(start: -4, step: 2);
            CollectionAssert.IsSubsetOf(slice, array);
            CollectionAssert.AreEqual(new double[] { 2, 4 }, slice, "Failed to evaluate array[-4::2]");

            slice = array.Slice(start: 1, end: -2, step: 2);
            CollectionAssert.IsSubsetOf(slice, array);
            CollectionAssert.AreEqual(new double[] { 1, 3 }, slice, "Failed to evaluate array[1:-2:2]");
        }

        [TestMethod]
        public void TestSubsetSetSlice()
        {
            double[] array = new double[]    {  0,  1,  2,  3 , 4,  5 };
            double[] insert = new double[] { 10, 11, 12, 13, 14, 15 };

            var slice = array.SetSlice(insert, start: 2);
            CollectionAssert.AreEqual(new double[] { 0, 1, 10, 11, 12, 13 }, slice, "Failed to set slice\r\narray[2:] = insert");
            slice = array.SetSlice(insert, start: 1, step: 2);
            CollectionAssert.AreEqual(new double[] { 0, 10, 2, 11, 4, 12 }, slice, "Failed to set slice\r\narray[1::2] = insert");

            slice = array.SetSlice(insert, start: 1, end: -1, step: 2);
            CollectionAssert.AreEqual(new double[] { 0, 10, 2, 11, 4, 5 }, slice, "Failed to set slice\r\narray[1:-1:2] = insert");
        }

        [TestMethod]
        public void TestSubsetChangeSlice()
        {
            double[] array = new double[] { 0, 1, 2, 3, 4, 5 };
            double[] insert1 = new double[] { 10, 11, 12, 13, 14, 15 };
            double[] insert2 = new double[] { 20, 21, 22, 23, 24, 25 };
            double[] insert3 = new double[] { 30, 31, 32, 33, 34, 35 };

            array.ChangeSlice(insert1, start: 2);
            CollectionAssert.AreEqual(new double[] { 0, 1, 10, 11, 12, 13 }, array, "Failed to set slice\r\narray[2:] = insert");

            array.ChangeSlice(insert2, start: 1, step: 2);
            CollectionAssert.AreEqual(new double[] { 0, 20, 10, 21, 12, 22 }, array, "Failed to set slice\r\narray[1::2] = insert");

            array.ChangeSlice(insert3, start: 1, end: -1, step: 2);
            CollectionAssert.AreEqual(new double[] { 0, 30, 10, 31, 12, 22 }, array, "Failed to set slice\r\narray[1:-1:2] = insert");
        }

        [TestMethod]
        public void TestTwoDimChangeSlice()
        {
            double[,] defaultArray = new double[,]
            {
                { 00, 10 },
                { 01, 11 },
                { 02, 12 },
                { 03, 13 },
            };
            double[,] array = defaultArray;
            double[,] array2 = new double[,]
            {
                { 00, 10, 20 },
                { 01, 11, 21 },
                { 02, 12, 22 },
                { 03, 13, 23 },
            };
            double[] insert = new double[] { 0.1, 0.2, 0.3, 0.4 };
            var equal2 = new double[,]
            {
                { 00, 10, 0.1 },
                { 01, 11, 21  },
                { 02, 12, 0.2 },
                { 03, 13, 23  },
            };

            // ColumnSlice()
            var slice = array.SetColumnSlice(insert, 1);
            Debug.WriteLine(slice.ToMatrixString());
            CollectionAssert.AreEqual(new double[,] { { 00, 0.1 }, { 01, 0.2 }, { 02, 0.3 }, { 03, 0.4 }, },
                slice, "Failed array[1, ::]");

            slice = array.SetColumnSlice(insert, -1);
            Debug.WriteLine(slice.ToMatrixString());
            CollectionAssert.AreEqual(new double[,] { { 00, 0.1 }, { 01, 0.2 }, { 02, 0.3 }, { 03, 0.4 }, },
                slice, "Failed array[1, -1]");

            slice = array2.SetColumnSlice(insert, 2, start: 0, end: -1, step: 2);
            Debug.WriteLine(slice.ToMatrixString());
            CollectionAssert.AreEqual(equal2, slice, "Failed array[2, 0:-1:2]");

            // RowSlice()
            slice = array.SetRowSlice(insert, 1);
            Debug.WriteLine(slice.ToMatrixString());
            CollectionAssert.AreEqual(new double[,] { { 00, 10 }, { 0.1, 0.2 }, { 02, 12 }, { 03, 13 }, },
                slice, "Failed array[::, 1]");

            slice = array.SetRowSlice(insert, -1);
            Debug.WriteLine(slice.ToMatrixString());
            CollectionAssert.AreEqual(new double[,] { { 00, 10 }, { 01, 11 }, { 02, 12 }, { 0.1, 0.2 }, },
                slice, "Failed array[-1, 1]");

            equal2 = new double[,]
            {
                { 00, 10, 20 },
                { 01, 11, 21 },
                { 0.1, 12, 22 },
                { 03, 13, 23 },
            };
            slice = array2.SetRowSlice(insert, 2, start: 0, end: -1, step: 2);
            Debug.WriteLine(slice.ToMatrixString());
            CollectionAssert.AreEqual(equal2, slice, "Failed array[0:-1:2, 2]");
        }

        [TestMethod]
        public void ReadSVGFile()
        {
            var doc = SvgDocument.Open(@"C:\Users\jjask\Pictures\Icons\SVG\Windows 10\Settings.svg");
            var paths = doc.Children.FindSvgElementsOf<SvgPath>();
            List<List<Vector<double>>> MobjectPoints = new List<List<Vector<double>>>();
            foreach (SvgPath path in paths)
            {
                if (path.ShouldWriteElement())
                {
                    Debug.WriteLine("> " + path.ID);
                    List<Vector<double>> points = new List<Vector<double>>();
                    points.Add(path.PathData[0].Start.ToVector());
                    foreach (var data in path.PathData)
                    {
                        var vPoint = data.End.ToVector();
                        points.Add(vPoint);
                        Debug.WriteLine($"<{vPoint[0]}, {vPoint[1]}>");
                    }
                    Debug.WriteLine("----------------------");
                    MobjectPoints.Add(points);
                }
            }
            //foreach (Vector<double> point in doc.Path.PathPoints.Select(pf => Vector<double>.Build.DenseOfArray(
            //    new double[] { pf.X, pf.Y }
            //)))
            //{
            //    Debug.WriteLine($"<{point[0]}, {point[1]}>");
            //}
        }
    }

    public static class TestClass
    {
        /// <summary>
        /// Slices a given <see cref="IList{T}"/>, just like <c>array[start:end:step]</c>.
        /// In theory, this is more efficient than <see cref="Slice{T}(IEnumerable{T}, int, int, int)"/>,
        /// because it doesn't use any LINQ
        /// </summary>
        /// <param name="start">Inclusive start index</param>
        /// <param name="end">Exclusive end index</param>
        /// <param name="step">Takes every item that is <c><paramref name="start"/>+k*<paramref name="step"/> lessthan <paramref name="end"/></c></param>
        public static List<T> Slice<T>(this IList<T> array, int start = 0, int? end = null, int step = 1)
        {
            var slice = new List<T>();
            int arrayLength = array.Count;
            // This is to allow negative indexes to work as they might in Python
            end = WrapEndIndex(end, arrayLength);
            start = WrapStartIndex(start, arrayLength);

            for (int i = start; i < end.Value; i += step)
                slice.Add(array[i]);
            return slice;
        }

        /// <summary>
        /// Converts raw user input to a valid array index, to be used as an end index.
        /// Used by <see cref="Slice{T}(IList{T}, int, int?, int)"/> to handle negative indexes.
        /// </summary>
        public static int WrapEndIndex(int? end, int arrayLength)
        {
            if (end.HasValue)
            {
                return end.Value < 0 ? Utils.SimpleFunctions.Mod(end.Value, arrayLength) : end.Value;
            }
            else
            {
                return arrayLength;
            }
        }
        /// <summary>
        /// Converts raw user input to a valid array index, to be used as an start index.
        /// Used by <see cref="Slice{T}(IList{T}, int, int?, int)"/> to handle negative indexes.
        /// </summary>
        public static int WrapStartIndex(int start, int arrayLength)
        {
            return start < 0 ? Utils.SimpleFunctions.Mod(start, arrayLength) : start;
        }

        /// <summary>
        /// Sets each element in a slice to the corresponding element in <paramref name="array"/>.
        /// Functionally identical to <see cref="SetSlice{T}(IList{T}, IList{T}, int, int?, int)"/>,
        /// but modifies <paramref name="array"/> instead of creating a new <see cref="List{T}"/>.
        /// </summary>
        /// <param name="array">The array to set a slice from</param>
        /// <param name="insert">The array to pull set elements from.</param>
        /// <param name="start">Inclusive start index</param>
        /// <param name="end">Exclusive end index</param>
        /// <param name="step">Takes every item that is <c><paramref name="start"/>+k*<paramref name="step"/> lessthan <paramref name="end"/></c></param>
        public static void ChangeSlice<T>(this IList<T> array, IList<T> insert, int start = 0, int? end = null, int step = 1)
        {
            int arrayLength = array.Count;
            // This is to allow negative indexes to work as they might in Python
            end = WrapEndIndex(end, arrayLength);
            start = WrapStartIndex(start, arrayLength);

            int j = 0;
            for (int i = start; i < end; i += step)
            {
                array[i] = insert[j];
                j++;
            }
        }

        /// <summary>
        /// Sets each element in a slice to the corresponding element in <paramref name="array"/>.
        /// Functionally identical to <see cref="ChangeSlice{T}(IList{T}, IList{T}, int, int?, int)"/>,
        /// but returns a new <see cref="List{T}"/> instead of modifying the existing one.
        /// </summary>
        /// <param name="array">The array to set a slice from</param>
        /// <param name="insert">The array to pull set elements from.</param>
        /// <param name="start">Inclusive start index</param>
        /// <param name="end">Exclusive end index</param>
        /// <param name="step">Takes every item that is <c><paramref name="start"/>+k*<paramref name="step"/> lessthan <paramref name="end"/></c></param>
        public static List<T> SetSlice<T>(this IList<T> array, IList<T> insert, int start = 0, int? end = null, int step = 1)
        {
            var output = new List<T>(array);
            output.ChangeSlice(insert, start, end, step);
            return output;
        }

        public static void ChangeRowSlice<T>(this T[,] array, IList<T> insert, int row, int start = 0, int? end = null, int step = 1)
        {
            int arrayLength = array.GetLength(1);
            // This is to allow negative indexes to work as they might in Python
            end = WrapEndIndex(end, arrayLength);
            start = WrapStartIndex(start, arrayLength);
            row = WrapStartIndex(row, array.GetLength(0));

            int i = 0;
            for (int j = start; j < end; j += step)
            {
                array[row, j] = insert[i];
                i++;
            }
        }
        public static void ChangeColumnSlice<T>(this T[,] array, IList<T> insert, int col, int start = 0, int? end = null, int step = 1)
        {
            int arrayLength = array.GetLength(0);
            // This is to allow negative indexes to work as they might in Python
            end = WrapEndIndex(end, arrayLength);
            start = WrapStartIndex(start, arrayLength);
            col = WrapStartIndex(col, array.GetLength(1));

            int i = 0;
            for (int j = start; j < end; j += step)
            {
                array[j, col] = insert[i];
                i++;
            }
        }

        /// <summary>
        /// Sets each element in the slice of a row to the corresponding element in <paramref name="array"/>.
        /// Functionally identical to <see cref="ChangeRowSlice{T}(T[,], IList{T}, int, int, int?, int)"/>,
        /// but returns a new <c>T[,]</c> instead of modifying the existing one.
        /// </summary>
        /// <param name="array">The array to set a slice from</param>
        /// <param name="insert">The array to pull set elements from.</param>
        /// <param name="start">Inclusive start index</param>
        /// <param name="end">Exclusive end index</param>
        /// <param name="step">Takes every item that is <c><paramref name="start"/>+k*<paramref name="step"/> lessthan <paramref name="end"/></c></param>
        public static T[,] SetRowSlice<T>(this T[,] array, IList<T> insert, int row, int start = 0, int? end = null, int step = 1)
        {
            T[,] output = new T[array.GetLength(0), array.GetLength(1)];
            Array.Copy(array, output, array.Length);
            output.ChangeRowSlice(insert, row, start, end, step);
            return output;
        }
        /// <summary>
        /// Sets each element in the slice of a row to the corresponding element in <paramref name="array"/>.
        /// Functionally identical to <see cref="ChangeColumnSlice{T}(T[,], IList{T}, int, int, int?, int)"/>,
        /// but returns a new <c>T[,]</c> instead of modifying the existing one.
        /// </summary>
        /// <param name="array">The array to set a slice from</param>
        /// <param name="insert">The array to pull set elements from.</param>
        /// <param name="start">Inclusive start index</param>
        /// <param name="end">Exclusive end index</param>
        /// <param name="step">Takes every item that is <c><paramref name="start"/>+k*<paramref name="step"/> lessthan <paramref name="end"/></c></param>
        public static T[,] SetColumnSlice<T>(this T[,] array, IList<T> insert, int col, int start = 0, int? end = null, int step = 1)
        {
            T[,] output = new T[array.GetLength(0), array.GetLength(1)];
            Array.Copy(array, output, array.Length);
            output.ChangeColumnSlice(insert, col, start, end, step);
            return output;
        }
    }
}

namespace ManimLibTest.Utils
{
    public static class SimpleFunctions
    {
        public static int Mod(int a, int b)
        {
            return Convert.ToInt32(a - b * Math.Floor((double)a / b));
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
    }

    public static class SpaceOps
    {
        public static Vector<double> ToVector(this PointF pf)
        {
            return Vector<double>.Build.DenseOfArray(new double[] { pf.X, pf.Y });
        }
    }
}