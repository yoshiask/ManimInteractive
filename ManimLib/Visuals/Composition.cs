using System;
using System.Collections.Generic;
using System.Text;

namespace ManimLib.Visuals
{
    public class AnimationGroup
    {
        public double? RunTime { get; set; }
        public Func<double, double> RateFunction { get; set; }
        public double LagRatio { get; set; }
    }
}
