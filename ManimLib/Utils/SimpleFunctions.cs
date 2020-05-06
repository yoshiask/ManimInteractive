﻿using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace ManimLib.Utils
{
    public static class SimpleFunctions
    {
        public static double Clip(double n, double lower, double upper)
        {
            if (n < lower)
                return lower;
            else if (n > upper)
                return upper;
            else
                return n;
        }
        public static int Clip(int n, int lower, int upper)
        {
            if (n < lower)
                return lower;
            else if (n > upper)
                return upper;
            else
                return n;
        }

        public static double Sigmoid(double x)
        {
            return 1.0 / (1 + Exp(-x));
        }

        public static double Sum(IList<double> values, double start = 0)
        {
            if (values.Count < 1)
                return start;

            double total = 0;
            foreach (double d in values)
            {
                total += d;
            }
            return total;
        }

        /// <summary>
        /// Calculates n choose r
        /// </summary>
        public static double Choose(int n, int r)
        {
            // The implementation here is different than the Python one,
            // but this should be more efficient (and does not need a cache)
            double result = 1;
            for (int i = 1; i <= r; i++)
            {
                result *= n - (r - i);
                result /= i;
            }
            return result;
        }

        public static IEnumerable<T> ClipInPlace<T>(IList<T> array, T minVal = default(T), T maxVal = default(T)) where T : IComparable
        {
            for (int i = 0; i < array.Count; i++)
            {
                T item = array[i];
                if (maxVal.CompareTo(default(T)) != 0)
                {
                    if (item.CompareTo(maxVal) > 0)
                        array[i] = maxVal;
                }
                if (minVal.CompareTo(default(T)) != 0)
                {
                    if (item.CompareTo(minVal) < 0)
                        array[i] = minVal;
                }
            }
            return array;
        }
        public static IEnumerable<RL.Color> ClipInPlace(IList<RL.Color> array, double minVal, double maxVal)
        {
            for (int i = 0; i < array.Count; i++)
            {
                RL.Color item = array[i];
                if (maxVal != default)
                {
                    item = Color.ApplyFunctionToRGBA(item, d =>
                    {
                        return d > maxVal ? maxVal : d;
                    });
                }
                if (minVal != default)
                {
                    item = Color.ApplyFunctionToRGBA(item, d =>
                    {
                        return d < minVal ? minVal : d;
                    });
                }
                array[i] = item;
            }
            return array;
        }

        /// <summary>
        /// Element-wise divides array a by array b. Will throw if b contains 0.
        /// </summary>
        public static IEnumerable<double> FDiv(IList<double> a, IList<double> b, double? zeroOverZeroValue = null)
        {
            return a.Zip(b, (i1, i2) => i1 / i2);
        }
    
        public static double BinarySearch(Func<object> function, double target, double lowerBound, double upperBound, double tolerance = 1E-4)
        {
            throw new NotImplementedException();
        }

        // The following functions are not part of manimpy

        public static int Mod(int a, int b)
        {
            return (int)Mod((double)a, b);
        }
        public static double Mod(double a, double b)
        {
            return a - b * Floor(a / b);
        }
    }
}