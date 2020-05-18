using ManimLib.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Svg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using ManimLib;

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

        [TestMethod]
        public void CreateSVGMobject()
        {
            var svg = new ManimLib.Mobject.Svg.SvgMobject(@"C:\Users\jjask\Pictures\Icons\SVG\Windows 10\Accept.svg");
            svg.GeneratePoints();
        }
    }

    public static class TestClass
    {

    }
}