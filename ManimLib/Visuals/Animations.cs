using ManimLib.Math;
using ManimLib.Mobject;
using ManimLib.Utils;
using System;

namespace ManimLib.Visuals
{
    public class Animations
    {
        public abstract class Animation : IManimElement
        {
            public Func<double, double> RateFunction {
                get;
                set;
            }
            public abstract bool IsRemover { get; }
            public abstract string AnimationType { get; }
            public string Name { get; set; }
            public double RunTime { get; set; }
            public double LagRatio { get; set; }
            public Mobject.Mobject Mobject { get; set; }
            public Mobject.Mobject StartingMobject { get; set; }

            public Animation()
            {
                RateFunction = RateFunctions.Smooth;
                RunTime = 1.0;
                LagRatio = 0;
            }
            public Animation(Mobject.Mobject mobject)
            {
                Mobject = mobject;
                RateFunction = RateFunctions.Smooth;
                RunTime = 1.0;
                LagRatio = 0;
            }
            public Animation(Mobject.Mobject mobject, Func<double, double> rateFunc, double runTime, double lagRatio)
            {
                Mobject = mobject;
                RateFunction = rateFunc;
                RunTime = runTime;
                LagRatio = lagRatio;
            }

            public void Begin()
            {
                StartingMobject = Mobject;
                Interpolate(0);
            }

            public void Finish()
            {
                Interpolate(1);
            }

            public void CleanUpScene(Scene scene)
            {
                if (IsRemover)
                    scene.RemoveShape(Mobject.Name);
            }

            public void Interpolate(double alpha)
            {
                alpha = SimpleFunctions.Clip(alpha, 0, 1);
                RateFunction(alpha);
            }

            public void InterpolateMobject(Mobject.Mobject obj, Mobject.Mobject startingObj, double alpha)
            {
                // Typically implemented by subclass
                return;
            }

            public double GetSubAlpha(double alpha, int index, int numSubMobjects)
            {
                double fullLength = (numSubMobjects - 1) * LagRatio + 1;
                double value = alpha * fullLength;
                double lower = index * LagRatio;
                return SimpleFunctions.Clip(value - lower, 0, 1);
            }

            public string GetManimType()
            {
                return Name;
            }
        }

        public abstract class ShowPartial : Animation
        {
            public void InterpolateSubmobject(Mobject.Mobject mobject, Mobject.Mobject startMobject, double alpha)
            {

            }

            public abstract Tuple<double, double> GetBounds();
        }
    }
}
