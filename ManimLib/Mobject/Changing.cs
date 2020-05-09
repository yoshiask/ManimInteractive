using ManimLib.Mobject.Types;
using static ManimLib.Constants;
using Color = RL.Color;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManimLib.Mobject
{
    public class AnimatedBoundary : VGroup
    {
        #region Properties
        public Color[] Colors { get; set; } = new Color[]
        {
            COLORS[Constants.Colors.BLUE_D], COLORS[Constants.Colors.BLUE_B],
            COLORS[Constants.Colors.BLUE_E], COLORS[Constants.Colors.GREY_BROWN]
        };
        public double MaxStrokeWidth { get; set; } = 3;
        public bool BackAndForth { get; set; } = false;
        public Func<double, double> DrawRateFunction { get; set; } = Utils.RateFunctions.Smooth;
        public Func<double, double> FadeRateFunction { get; set; } = Utils.RateFunctions.Smooth;
        #endregion

        public AnimatedBoundary()
        {

        }
    }
}
