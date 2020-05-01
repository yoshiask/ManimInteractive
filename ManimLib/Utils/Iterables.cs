﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ManimLib.Utils
{
    public static class Iterables
    {
        public static IEnumerable<Tuple<T, T>> AdjacentPairs<T>(IList<T> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (i + 1 < items.Count)
                    yield return new Tuple<T, T>(items[i], items[i + 1]);
                else
                    yield return new Tuple<T, T>(items[i], items[0]);
            }
        }

        public static IEnumerable<IEnumerable<T>> AdjacentNTuples<T>(IList<T> items, int n)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This is the equivalent of items[:, dim] in Python.
        /// It returns a 'column' of what's effectively a 2D array
        /// </summary>
        public static IEnumerable<T> GetColumn<T>(this IEnumerable<IList<T>> items, int index)
        {
            foreach (IList<T> item in items)
                yield return item[index];
        }

        /// <summary>
        /// Returns a list of evenly-spaced numbers over a specified interval.
        /// Equivalent to np.linspace()
        /// </summary>
        public static IEnumerable<double> LinSpace(double start, double end, int partitions)
        {
            return Enumerable.Range(0, partitions + 1).Select(idx => idx != partitions
                    ? start + (end - start) / partitions * idx
                    : end);
        }
        /// <summary>
        /// Returns a list of evenly-spaced numbers over a specified interval.
        /// Equivalent to np.linspace()
        /// </summary>
        public static IEnumerable<int> LinSpace(int start, int end, int partitions)
        {
            return Enumerable.Range(0, partitions + 1).Select(idx => idx != partitions
                    ? start + (end - start) / partitions * idx
                    : end);
        }

        public static Tuple<IEnumerable<T1>, IEnumerable<T2>> MakeEven<T1, T2>(IEnumerable<T1> iter1, IEnumerable<T2> iter2)
        {
            int length = System.Math.Max(iter1.Count(), iter2.Count());
            T1[] newList1 = new T1[length];
            T2[] newList2 = new T2[length];
            for (int n = 0; n < length; n++)
            {
                newList1[n] = iter1.ElementAt((n * iter1.Count()) / length);
                newList2[n] = iter2.ElementAt((n * iter2.Count()) / length);
            }
            return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(newList1, newList2);
        }

        public static Tuple<IEnumerable<T>, IEnumerable<T>> MakeEvenByCycling<T>(IEnumerable<T> iter1, IEnumerable<T> iter2)
        {
            int length = System.Math.Max(iter1.Count(), iter2.Count());
            T[] newList1 = new T[length];
            T[] newList2 = new T[length];
            for (int n = 0; n < length; n++)
            {
                newList1[n] = Cycle(iter1, n);
                newList2[n] = Cycle(iter2, n);
            }
            return new Tuple<IEnumerable<T>, IEnumerable<T>>(newList1, newList2);
        }

        public static T Cycle<T>(IEnumerable<T> iter, int index)
        {
            return iter.ElementAt(index % iter.Count());
        }
    }
}
