using ManimLib.Mobject.Types;
using static ManimLib.Constants;
using Color = RL.Color;
using System;
using System.Collections.Generic;
using ManimLib.Math;
using MathNet.Numerics.LinearAlgebra;

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
        public double CycleRate { get; set; } = 0.5;
        public bool BackAndForth { get; set; } = false;
        public Func<double, double> DrawRateFunction { get; set; } = Utils.RateFunctions.Smooth;
        public Func<double, double> FadeRateFunction { get; set; } = Utils.RateFunctions.Smooth;

        public VMobject VMobject { get; set; }
        public List<VMobject> BoundaryCopies { get; set; }
        public double TotalTime { get; set; } = 0;
        #endregion

        public AnimatedBoundary(VMobject vmobj, string name = null, Color color = default, int dim = 3, Mobject target = null)
            : base()
        {
            VMobject = vmobj;
            BoundaryCopies = new List<VMobject>(2);
            for (int i = 0; i < 2; i++)
            {
                var newVMobj = vmobj.Copy().SetStyle(
                    strokeWidth: 0,
                    fillOpacity: new double[] { 0 }
                );
                BoundaryCopies.Add(newVMobj);
                Add(newVMobj);
            }
            AddUpdater((m, dt) => UpdateBoundaryCopies(dt));
        }

        public Mobject UpdateBoundaryCopies(double dt)
        {
            // Not actual time, but something which passes at
            // an altered rate to make the implementation below
            // cleaner
            double time = TotalTime * CycleRate;
            VMobject growing = BoundaryCopies[0];
            VMobject fading = BoundaryCopies[1];
            Color[] colors = Colors;
            double msw = MaxStrokeWidth;
            VMobject vmobj = VMobject;

            int index = (int)(time % colors.Length);
            double alpha = time % 1;
            double drawAlpha = DrawRateFunction(alpha);
            double fadeAlpha = FadeRateFunction(alpha);

            double boundsX, boundsY;
            if (BackAndForth && time % 2 == 1)
            {
                boundsX = 1 - drawAlpha;
                boundsY = 1;
            }
            else
            {
                boundsX = 0;
                boundsY = drawAlpha;
            }
            FullFamilyBecomePartial(growing, vmobj, (int)boundsX, (int)boundsY);
            growing.SetStroke(new Color[] { colors[index] }, width: msw);

            if (time >= 1)
            {
                FullFamilyBecomePartial(fading, vmobj, 0, 1);
                fading.SetStroke(new Color[] { colors[index - 1] }, width: (1 - fadeAlpha) * msw);
            }

            TotalTime += dt;

            // Manimpy does not actually return anything here
            return this;
        }

        public AnimatedBoundary FullFamilyBecomePartial(Mobject mobj1, Mobject mobj2, int a, int b)
        {
            List<Mobject> family1 = mobj1.GetFamilyMembersWithPoints();
            List<Mobject> family2 = mobj2.GetFamilyMembersWithPoints();
            for (int i = 0; i < family1.Count; i++)
            {
                family1[i].PointwiseBecomePartial(family2[i], a, b);
            }
            return this;
        }
    }

    public class TracedPath : VMobject
    {
        public double StrokeWidth { get; set; } = 2;
        public Color StrokColor { get; set; } = COLORS[Colors.WHITE];
        public double MinDistanceToNewPoint { get; set; } = 0.1;

        Func<Vector<double>> TracedPointFunc { get; set; }

        public TracedPath(Func<Vector<double>> tracedPointFunc, string name = null, Color color = default, int dim = 3, Mobject target = null)
            : base(name, color, dim, target)
        {
            TracedPointFunc = tracedPointFunc;
            AddUpdater((m, dt) => (m as TracedPath).UpdatePath());
        }

        public Mobject UpdatePath()
        {
            Vector<double> newPoint = TracedPointFunc();
            if (Points.Count == 0)
            {
                StartNewPath(newPoint);
                AddLineTo(newPoint);
            }
            else
            {
                // Set the end to the last point
                Points[^1] = newPoint;

                // Second to last point
                double dist = (newPoint - Points[ArrayUtilities.WrapStartIndex(NPointsPerCubicCurve, Points.Count)]).L2Norm();
                if (dist >= MinDistanceToNewPoint)
                {
                    AddLineTo(newPoint);
                }
            }
            return this;
        }
    } 
}
