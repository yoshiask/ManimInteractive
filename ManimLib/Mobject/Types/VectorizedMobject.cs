using Color = RL.Color;
using System;
using System.Collections.Generic;
using static ManimLib.Constants;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;

namespace ManimLib.Mobject.Types
{
    public class VMobject : Mobject
    {
        #region Properties
        public Color FillColor { get; set; }
        public double FillOpacity { get; set; } = 0.0;

        public Color StrokeColor { get; set; }
        public double StrokeOpacity { get; set; } = 1.0;
        public double StrokeWidth { get; set; } = 0.0;

        // The purpose of background stroke is to have
        // something that won't overlap the fill, e.g.
        // For text against some textured background
        public Color BackgroundStrokeColor { get; set; } = COLORS[Colors.BLACK];
        public double BackgroundStrokeOpacity { get; set; } = 1.0;
        public double BackgroundStrokeWidth { get; set; } = 0.0;

        // When a color c is set, there will be a second color
        // computed based on interpolating c to WHITE by with
        // sheen_factor, and the display will gradient to this
        // secondary color in the direction of sheen_direction.
        public double SheenFactor { get; set; } = 0.0;
        public Vector<double> SheenDirection { get; set; } = UL;

        // Indicates that it will not be displayed, but
        // that it should count in parent mobject's path
        public bool CloseNewPoints { get; set; } = false;

        public double PreFunctionHandleToAnchorScaleFactor { get; set; } = 0.01;
        public bool MakeSmoothAfterApplyingFunctions { get; set; } = false;
        public Color[,] BackgroundImage { get; set; }
        public string BackgroundImageFile { get; set; }
        public bool ShadeIn3D { get; set; } = false;

        // This is within a pixel
        // TODO: Do we care about accounting for
        // varying zoom levels?
        public double ToleranceForPointEquality { get; set; } = 1E-6;
        public int NPointsPerCubicCurve { get; set; } = 4;

        // TODO: This sucks. Find a better way to do this.
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
        #endregion

        #region Colors
        public override Mobject InitColors()
        {
            SetFill(FillColor != null ? FillColor : Color, FillOpacity);
            SetStroke(StrokeColor != null ? StrokeColor : Color, StrokeWidth, StrokeOpacity);
            SetBackgroundStroke(BackgroundStrokeColor, BackgroundStrokeWidth, BackgroundStrokeOpacity);
            SetSheen(SheenFactor, SheenDirection);
            return this;
        }

        /// <summary>
        /// If self.sheen_factor is not zero a second slightly light color
        /// will automatically be added for the gradient
        /// </summary>
        public List<double[]> GenerateRGBAsArray(Color color, double opacity)
        {
            return GenerateRGBAsArray(new List<Color>() { color }, new List<double>() { opacity });
        }
        /// <summary>
        /// First arg can be either a color, or a tuple/list of colors.
        /// Likewise, opacity can either be a float, or a tuple of floats.
        /// If self.sheen_factor is not zero, and only
        /// one color was passed in, a second slightly light color
        /// will automatically be added for the gradient
        /// </summary>
        public List<double[]> GenerateRGBAsArray(List<Color> colors, List<double> opacities)
        {
            (IEnumerable<Color> colorList, IEnumerable<double> opacityList) = Utils.Iterables.MakeEven(colors, opacities);
            IEnumerable<double[]> rgbas = colorList.Zip(opacityList, (c, o) => {
                c.A *= (byte)o;
                return Utils.Color.ColorToRgba(c);
            });

            if (SheenFactor != 0 && rgbas.Count() == 1)
            {
                Color lightColor = Utils.Color.ApplyFunctionToRGB(rgbas.First(), d => d + SheenFactor);
                rgbas.Concat(new double[1][] { Utils.Color.ColorToRgba(lightColor) });
            }
            return rgbas;
        }

        public VMobject UpdateRGBAsArray(string arrayName, Color color = null, double opacity = 0)
        {
            if (color == null)
                color = COLORS[Colors.BLACK];
            List<double[]> rgbas = GenerateRGBAsArray(color, opacity);
            if (!Attributes.ContainsKey(arrayName))
            {
                Attributes.Add(arrayName, rgbas);
                return this;
            }
            // Match up current rgbas array with the newly calculated
            // one. 99% of the time they'll be the same.
            var currRgbas = Attributes[arrayName];
            if ()
        }
        #endregion
    }
}
