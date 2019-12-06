using System;
using System.Collections.Generic;
using System.Text;

namespace ManimLib.Mathematics
{
    public static class Functions
    {
        #region Helpers
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
            return 1.0 / (1 + Math.Exp(-x));
        }

        // TODO: Implement the Bezier function

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


        #endregion

        public static double Linear(double t)
        {
            return t;
        }

        public static double Smooth(double t, double inflection = 10)
        {
            double error = Sigmoid(-inflection / 2);
            return Clip(
                (Sigmoid(inflection * (t - 0.5)) - error) / (1 - 2 * error),
                0, 1
            );
        }

        public static double RushInto(double t, double inflection = 10)
        {
            return 2 * Smooth(t / 2.0, inflection);
        }

        public static double RushFrom(double t, double inflection = 10)
        {
            return 2 * Smooth(t / 2.0 + 0.5, inflection) - 1;
        }

        public static double SlowInto(double t)
        {
            return Math.Sqrt(1 - (1 - t) * (1 - t));
        }

        public static double DoubleSmooth(double t)
        {
            if (t < 0.5)
                return 0.5 * Smooth(2 * t);
            else
                return 0.5 * (1 + Smooth(2 * t - 1));
        }

        public static double ThereAndBack(double t, double inflection = 10)
        {
            double new_t;
            if (t < 0.5)
                new_t = 2 * t;
            else
                new_t = 2 * (1 - t);
            return Smooth(new_t, inflection);
        }

        public static double ThereAndBackWithPause(double t, double pauseRatio = 1.0 / 3)
        {
            double a = 1.0 / pauseRatio;
            if (0.5 - (pauseRatio / 2) > t)
                return Smooth(a * t);
            else if (t < 0.5 + pauseRatio / 2)
                return 1;
            else
                return Smooth(a - a * t);
        }

        // TODO: Implement the running_start function

        public static Func<double, double> NotQuiteThere(Func<double, double> func, double proportion = 0.7)
        {
            return (double t) => func(t) * proportion;
        }

        public static double Wiggle(double t, int numWiggles = 2)
        {
            return ThereAndBack(t) * Math.Sin(numWiggles * Math.PI * t);
        }

        public static Func<double, double> SquishRateFunction(Func<double, double> func, double a = 0.4, double b = 0.6)
        {
            return (double t) =>
            {
                if (a == b)
                    return a;
                else if (t < a)
                    return func(0);
                else if (t > b)
                    return func(1);
                else
                    return func((t - a) / (b - a));
            };
        }

        public static Func<double, double> Lingering(double t)
        {
            return SquishRateFunction((double n) => n, 0, 0.8);
        }

        public static double ExponentialDecay(double t, double halfLife = 0.1)
        {
            return 1 - Math.Exp(-t / halfLife);
        }
    }
}
