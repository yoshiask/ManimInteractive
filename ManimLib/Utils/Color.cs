using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace ManimLib.Utils
{
    public static class Color
    {
        public static List<RL.Color> ColorGradient(int lengthOfOutput, params RL.Color[] referenceColors)
        {
            if (lengthOfOutput == 0)
                return new List<RL.Color>() { referenceColors[0] };

            IEnumerable<int[]> rgbs = referenceColors.Select(c => new int[] { c.R, c.G, c.B });
            List<int> alphas = Iterables.LinSpace(0, rgbs.Count() - 1, lengthOfOutput).ToList();
            alphas.ForEach(num => num %= 1);
            alphas[alphas.Count - 1] = 1;
            //...
            throw new NotImplementedException();
        }

        public static RL.Color AverageColor(params RL.Color[] colors)
        {
            IEnumerable<double[]> rgbs = colors.Select(c => ColorToRgb(c));
            double[] rgb = new double[3];
            rgb[0] = rgbs.GetColumn(0).Average();
            rgb[1] = rgbs.GetColumn(1).Average();
            rgb[2] = rgbs.GetColumn(2).Average();
            return ColorFromRgb(rgb);
        }

        public static RL.Color RandomBrightColor()
        {
            return RandomColor().Interpolate(new RL.Color(255, 255, 255), 0.5);
        }

        public static RL.Color RandomColor()
        {
            Random rnd = new Random();
            return Constants.COLORS[(Constants.Colors)rnd.Next(0, 57)];
        }

        public static object GetShadedColor(RL.Color color, Vector<double> point, Vector<double> unitNormalVect, Vector<double> lightSource)
        {
            Vector<double> toSun = SpaceOps.Normalize(lightSource - point);
            var factor = 0.5 * Pow(unitNormalVect.DotProduct(toSun), 3);
            if (factor < 0)
                factor *= 0.5;
            return ApplyFunctionToRGBA(color, d => d + factor);
        }


        // The following functions are not included in the Python Manim library

        public static RL.Color ClipColor(RL.Color color)
        {
            RL.Color item = color;
            item = ApplyFunctionToRGBA(item, d =>
            {
                return d > 255 ? 255 : d;
            });
            item = ApplyFunctionToRGBA(item, d =>
            {
                return d < 0 ? 0 : d;
            });
            return item;
        }

        /// <summary>
        /// Converts a double to a byte that can be used as an R, G, B, or alpha value
        /// </summary>
        public static byte ByteFromDouble(double value)
        {
            var intmed = value * 255;
            if (intmed > 255)
                return 255;
            else if (intmed < 0)
                return 0;
            else
                return (byte)intmed;
        }
        /// <summary>
        /// Converts a byte from a Color to a double value
        /// </summary>
        public static double DoubleFromByte(byte value)
        {
            return value / 255.0;
        }

        public static double[] ColorToRgba(RL.Color color)
        {
            return new double[] {
                DoubleFromByte(color.R),
                DoubleFromByte(color.G),
                DoubleFromByte(color.B),
                DoubleFromByte(color.A),
            };
        }
        public static double[] ColorToRgb(RL.Color color)
        {
            return new double[] {
                DoubleFromByte(color.R),
                DoubleFromByte(color.G),
                DoubleFromByte(color.B),
            };
        }
        public static byte[] ColorToByteRgba(RL.Color color)
        {
            return new byte[] {
                ByteFromDouble(color.R),
                ByteFromDouble(color.G),
                ByteFromDouble(color.B),
                ByteFromDouble(color.A)
            };
        }
        public static byte[] ColorToByteRgb(RL.Color color)
        {
            return new byte[] {
                ByteFromDouble(color.R),
                ByteFromDouble(color.G),
                ByteFromDouble(color.B)
            };
        }

        public static RL.Color ColorFromRgba(double[] rgba)
        {
            return new RL.Color(
                ByteFromDouble(rgba[3]),
                ByteFromDouble(rgba[0]),
                ByteFromDouble(rgba[1]),
                ByteFromDouble(rgba[2])
            );
        }
        public static RL.Color ColorFromRgb(double[] rgb)
        {
            return new RL.Color(
                ByteFromDouble(rgb[0]),
                ByteFromDouble(rgb[1]),
                ByteFromDouble(rgb[2])   
            );
        }
        public static RL.Color ColorFromRgba(byte[] rgba)
        {
            return new RL.Color(
                rgba[3],
                rgba[0],
                rgba[1],
                rgba[2]
            );
        }
        public static RL.Color ColorFromRgb(byte[] rgb)
        {
            return new RL.Color(
                rgb[0],
                rgb[1],
                rgb[2]
            );
        }

        /// <summary>
        /// Returns a Color where each of the components of the input color (ARGB) is transformed
        /// by the given function
        /// </summary>
        public static RL.Color ApplyFunctionToRGBA(RL.Color color, Func<int, int> func)
        {
            return new RL.Color(
                func(color.A),
                func(color.R),
                func(color.G),
                func(color.B)
            );
        }
        public static RL.Color ApplyFunctionToRGBA(RL.Color color, Func<double, double> func)
        {
            return new RL.Color(
                ByteFromDouble(func(DoubleFromByte(color.A))),
                ByteFromDouble(func(DoubleFromByte(color.R))),
                ByteFromDouble(func(DoubleFromByte(color.G))),
                ByteFromDouble(func(DoubleFromByte(color.B)))
            );
        }

        /// <summary>
        /// Returns a Color where each of the components of the input color (ARGB) is transformed
        /// by the given function
        /// </summary>
        public static RL.Color ApplyFunctionToRGB(RL.Color color, Func<int, int> func)
        {
            return new RL.Color(
                color.A,
                func(color.R),
                func(color.G),
                func(color.B)
            );
        }
        public static RL.Color ApplyFunctionToRGB(RL.Color color, Func<double, double> func)
        {
            return new RL.Color(
                color.A,
                ByteFromDouble(func(DoubleFromByte(color.R))),
                ByteFromDouble(func(DoubleFromByte(color.G))),
                ByteFromDouble(func(DoubleFromByte(color.B)))
            );
        }
        public static RL.Color ApplyFunctionToRGB(int[] color, Func<int, int> func)
        {
            return new RL.Color(
                color[3],
                func(color[0]),
                func(color[1]),
                func(color[2])
            );
        }
        public static RL.Color ApplyFunctionToRGB(double[] color, Func<double, double> func)
        {
            return new RL.Color(
                ByteFromDouble(color[3]),
                ByteFromDouble(func(color[0])),
                ByteFromDouble(func(color[1])),
                ByteFromDouble(func(color[2]))
            );
        }

    }
}
