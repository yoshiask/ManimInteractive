using System;
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

        public static List<List<T>> AdjacentNTuples<T>(IList<T> items, int n)
        {
            int totalCount = items.Count + (items.Count % n);
            List<List<T>> tuples = new List<List<T>>(totalCount / n);
            //for (int i = 0; i < totalCount; i+=n)
            //{
            //    List<T> tuple = new List<T>(n);
            //    for (int j = 0; j < n; j++)
            //    {
            //        if ()
            //    }
            //    tuples.Add(tuple);
            //}
            for (int i = 0; i < items.Count; i++)
            {
                int location = items.Count / n;
                tuples[i / location] = new List<T>();
                tuples[i / location][i % location] = items[i];
            }
            return tuples;
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

        public static IEnumerable<T> StretchArrayToLength<T>(IEnumerable<T> array, int length)
        {
            int currLength = array.Count();
            if (currLength > length)
                System.Diagnostics.Debug.WriteLine("Warning: Trying to stretch array to a length shorter than its own");
            IEnumerable<int> indicies = ArrayUtilities.Arange(length).Select(
                i => Convert.ToInt32(i / (float)length * currLength)
            );
            foreach (int index in indicies)
            {
                yield return array.ElementAt(index);
            }
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

        public static IEnumerable<Tuple<T1, T2>> MakeEvenTuples<T1, T2>(IEnumerable<T1> iter1, IEnumerable<T2> iter2)
        {
            int length = System.Math.Max(iter1.Count(), iter2.Count());
            Tuple<T1, T2>[] newList = new Tuple<T1, T2>[length];
            for (int n = 0; n < length; n++)
            {
                newList[n] = new Tuple<T1, T2>(
                    iter1.ElementAt((n * iter1.Count()) / length),
                    iter2.ElementAt((n * iter2.Count()) / length)
                );
            }
            return newList;
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

        /// <summary>
        /// Creates a sequence of numbers, exclusive with <c>end</c>
        /// </summary>
        public static IEnumerable<int> CreateRange(int start, int end, int step)
        {
            for (int i = start; i < end; i += step)
                yield return i;
        }
        /// <summary>
        /// Creates a sequence of numbers starting at zero, exclusive with <c>end</c>
        /// </summary>
        public static IEnumerable<int> CreateRange(int end)
        {
            for (int i = 0; i < end; i++)
                yield return i;
        }

        public static List<T> Interleave<T>(IList<T> first, IList<T> second)
        {
            // I'll assume both have the same length and are
            // not null, simply add more checks/tweaks if needed

            int length = first.Count;
            List<T> result = new List<T>(length * 2);

            for (int i = 0; i < length; i++)
            {
                result.Add(first[i]);
                result.Add(second[i]);
            }

            return result;
        }
    }
}
