using ManimLib.Mathematics;
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
            public Shapes.Mobject_Shape Mobject { get; set; }
            public Shapes.Mobject_Shape StartingMobject { get; set; }

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
                    scene.RemoveShape(Mobject);
            }

            public void Interpolate(double alpha)
            {
                alpha = Functions.Clip(alpha, 0, 1);
                RateFunction(alpha);
            }

            public void InterpolateMobject(Shapes.Mobject_Shape obj, Shapes.Mobject_Shape startingObj, double alpha)
            {
                return;
            }

            public double GetSubAlpha(double alpha, int index, int numSubMobjects)
            {
                double fullLength = (numSubMobjects - 1) * LagRatio + 1;
                double value = alpha * fullLength;
                double lower = index * LagRatio;
                return Functions.Clip(value - lower, 0, 1);
            }

            public string GetManimType()
            {
                return Name;
            }
        }
    }
}
