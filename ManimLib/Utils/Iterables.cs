using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
