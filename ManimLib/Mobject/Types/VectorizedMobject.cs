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
        public new List<VMobject> Submobjects { get; internal set; }

        public VMobjectStyle Style { get; set; }

        // Indicates that it will not be displayed, but
        // that it should count in parent mobject's path
        public bool CloseNewPoints { get; set; } = false;

        public double PreFunctionHandleToAnchorScaleFactor { get; set; } = 0.01;
        public bool MakeSmoothAfterApplyingFunctions { get; set; } = false;
        public Color[,] BackgroundImage { get; set; }
        public bool ShadeIn3D { get; set; } = false;

        // This is within a pixel
        // TODO: Do we care about accounting for
        // varying zoom levels?
        public double ToleranceForPointEquality { get; set; } = 1E-6;
        public int NPointsPerCubicCurve { get; set; } = 4;

        public VMobject ZIndexGroup { get; set; }

        // TODO: This sucks. Find a better way to do this.
        /// <summary>
        /// This is mostly used for colors, which are of type <see cref="List<double[]>"/>
        /// </summary>
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
        #endregion

        public VMobject(string name = null, Color color = default, int dim = 3, Mobject target = null)
            : base(name, color, dim, target) { }

        #region Colors
        public override Mobject InitColors()
        {
            SetFill(Style.FillColor != null ? Style.FillColor : new Color[] { Color }, Style.FillOpacity);
            SetStroke(Style.StrokeColor != null ? Style.StrokeColor : new Color[] { Color }, Style.StrokeWidth, Style.StrokeOpacity);
            SetBackgroundStroke(Style.BackgroundStrokeColor, Style.BackgroundStrokeWidth, Style.BackgroundStrokeOpacity);
            SetSheen(Style.SheenFactor, Style.SheenDirection);
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
        /// If self.sheen_factor is not zero, and only
        /// one color was passed in, a second slightly light color
        /// will automatically be added for the gradient
        /// </summary>
        public List<double[]> GenerateRGBAsArray(List<Color> colors, List<double> opacities)
        {
            (IEnumerable<Color> colorList, IEnumerable<double> opacityList) = Utils.Iterables.MakeEven(colors, opacities);
            IEnumerable<double[]> rgbas = colorList.Zip(opacityList, (c, o) =>
            {
                c.A *= (byte)o;
                return Utils.Color.ColorToRgba(c);
            });

            if (Style.SheenFactor != 0 && rgbas.Count() == 1)
            {
                Color lightColor = Utils.Color.ApplyFunctionToRGB(rgbas.First(), d => d + Style.SheenFactor);
                rgbas = rgbas.Concat(new double[1][] { Utils.Color.ColorToRgba(lightColor) });
            }
            return rgbas.ToList();
        }
        /// <summary>
        /// If self.sheen_factor is not zero, and only
        /// one color was passed in, a second slightly light color
        /// will automatically be added for the gradient
        /// </summary>
        public List<double[]> GenerateRGBAsArray(Color[] colors, double[] opacities)
        {
            (IEnumerable<Color> colorList, IEnumerable<double> opacityList) = Utils.Iterables.MakeEven(colors, opacities);
            IEnumerable<double[]> rgbas = colorList.Zip(opacityList, (c, o) =>
            {
                c.A *= (byte)o;
                return Utils.Color.ColorToRgba(c);
            });

            if (Style.SheenFactor != 0 && rgbas.Count() == 1)
            {
                Color lightColor = Utils.Color.ApplyFunctionToRGB(rgbas.First(), d => d + Style.SheenFactor);
                rgbas = rgbas.Concat(new double[1][] { Utils.Color.ColorToRgba(lightColor) });
            }
            return rgbas.ToList();
        }

        public VMobject UpdateRGBAsArray(string arrayName, Color color = null, double? opacity = null)
        {
            Color passedColor = color == null ? COLORS[Colors.BLACK] : color;
            double passedOpacity = opacity.HasValue ? opacity.Value : 0;

            List<double[]> rgbas = GenerateRGBAsArray(passedColor, passedOpacity);
            if (!Attributes.ContainsKey(arrayName))
            {
                Attributes.Add(arrayName, rgbas);
                return this;
            }
            // Match up current rgbas array with the newly calculated
            // one. 99% of the time they'll be the same.
            var currRgbas = (List<double[]>)Attributes[arrayName];
            if (currRgbas.Count < rgbas.Count)
                currRgbas = Utils.Iterables.StretchArrayToLength(currRgbas, rgbas.Count).ToList();
            else if (currRgbas.Count > rgbas.Count)
                rgbas = Utils.Iterables.StretchArrayToLength(rgbas, currRgbas.Count).ToList();

            // Only update rgb if color was not null, and only
            // update alpha channel if opacity was passed in
            for (int i = 0; i < currRgbas.Count; i++)
            {
                if (color != null)
                    currRgbas[i] = rgbas[i][0..2].Concat(currRgbas[3]).ToArray();
                if (opacity.HasValue)
                    currRgbas[i][3] = rgbas[i][3];
            }
            Attributes[arrayName] = currRgbas;
            return this;
        }
        public VMobject UpdateRGBAsArray(string arrayName, Color[] color = null, double[] opacity = null)
        {
            // This feels a bit hacky. Find a better way to do this
            List<double[]> rgbas;
            if (color == null && opacity == null)
                rgbas = GenerateRGBAsArray(COLORS[Colors.BLACK], 0);
            else if (color == null && opacity != null)
                rgbas = GenerateRGBAsArray(COLORS[Colors.BLACK], opacity[0]);
            else if (color != null && opacity == null)
                rgbas = GenerateRGBAsArray(color, (0.0).Repeat(color.Length).ToArray());
            else
                rgbas = GenerateRGBAsArray(color, opacity);

            if (!Attributes.ContainsKey(arrayName))
            {
                Attributes.Add(arrayName, rgbas);
                return this;
            }
            // Match up current rgbas array with the newly calculated
            // one. 99% of the time they'll be the same.
            var currRgbas = (List<double[]>)Attributes[arrayName];
            if (currRgbas.Count < rgbas.Count)
                currRgbas = Utils.Iterables.StretchArrayToLength(currRgbas, rgbas.Count).ToList();
            else if (currRgbas.Count > rgbas.Count)
                rgbas = Utils.Iterables.StretchArrayToLength(rgbas, currRgbas.Count).ToList();

            // Only update rgb if color was not null, and only
            // update alpha channel if opacity was passed in
            for (int i = 0; i < currRgbas.Count; i++)
            {
                if (color != null)
                    currRgbas[i] = rgbas[i][0..2].Concat(currRgbas[3]).ToArray();
                if (opacity != null)
                    currRgbas[i][3] = rgbas[i][3];
            }
            Attributes[arrayName] = currRgbas;
            return this;
        }

        public VMobject SetFill(Color[] color = null, double[] opacity = null, bool family = true)
        {
            if (family)
                foreach (VMobject submobj in Submobjects)
                    submobj.SetFill(color, opacity, family);
            UpdateRGBAsArray("fill_rgbas", color, opacity);
            return this;
        }

        public VMobject SetStroke(Color[] color = null, double? width = null, double? opacity = null, bool background = true, bool family = true)
        {
            if (family)
                foreach (VMobject submobj in Submobjects)
                    submobj.SetStroke(color, width, opacity, background, family);

            string arrayName, widthName;
            if (background)
            {
                arrayName = "background_stroke_rgbas";
                widthName = "background_stroke_width";
            }
            else
            {
                arrayName = "stroke_rgbas";
                widthName = "stroke_width";
            }
            UpdateRGBAsArray(arrayName, color,
                opacity.HasValue ? new double[] { opacity.Value } : new double[] { 0 });
            if (width.HasValue)
                Attributes[widthName] = width;
            return this;
        }

        public VMobject SetBackgroundStroke(Color[] color = null, double? width = null, double? opacity = null, bool family = true)
        {
            return SetStroke(color, width, opacity, true, family);
        }

        public VMobject SetStyle(Color[] fillColor = null, double[] fillOpacity = null, Color[] strokeColor = null,
                  double? strokeWidth = null, double? strokeOpacity = null, Color[] backgroundStrokeColor = null,
                  double? backgroundStrokeWidth = null, double? backgroundStrokeOpacity = null,
                  double? sheenFactor = null, Vector<double> sheenDirection = null,
                  string backgroundImageFile = null, bool family = true)
        {
            SetFill(fillColor, fillOpacity, family);
            SetStroke(strokeColor, strokeWidth, strokeOpacity, family);
            SetBackgroundStroke(backgroundStrokeColor, backgroundStrokeWidth, backgroundStrokeOpacity, family);
            if (sheenFactor.HasValue)
                SetSheen(sheenFactor.Value, sheenDirection, family);
            if (!String.IsNullOrEmpty(backgroundImageFile))
                ColorUsingBackgroundImage(backgroundImageFile);
            return this;
        }
        public VMobject SetStyle(Dictionary<string, object> style, bool family = true)
        {
            return SetStyle((VMobjectStyle)style, family);
        }
        public VMobject SetStyle(VMobjectStyle style, bool family = true)
        {
            return SetStyle(
                style.FillColor,
                style.FillOpacity,
                style.StrokeColor,
                style.StrokeWidth,
                style.StrokeOpacity,
                style.BackgroundStrokeColor,
                style.BackgroundStrokeWidth,
                style.BackgroundStrokeOpacity,
                style.SheenFactor,
                style.SheenDirection,
                style.BackgroundImageFile,
                family
            );
        }

        public VMobjectStyle GetStyle()
        {
            return new VMobjectStyle()
            {
                FillColor = GetFillColors(),
                FillOpacity = GetFillOpacities(),
                StrokeColor = GetStrokeColors(),
                StrokeWidth = GetStrokeWidth(),
                StrokeOpacity = GetStrokeOpacity(),
                BackgroundStrokeColor = GetStrokeColors(background: true),
                BackgroundStrokeWidth = GetStrokeWidth(background: true),
                BackgroundStrokeOpacity = GetStrokeOpacity(background: true),
                SheenFactor = Style.SheenFactor,
                SheenDirection = Style.SheenDirection,
                BackgroundImageFile = Style.BackgroundImageFile
            };

        }

        public VMobject MatchStyle(VMobject vmobj, bool family = true)
        {
            SetStyle(vmobj.GetStyle(), family);

            if (family)
            {
                // Does its best to match up submobject lists, and match styles accordingly
                if (Submobjects.Count == 0)
                    return this;
                else if (vmobj.Submobjects.Count == 0)
                    vmobj.Submobjects = new List<VMobject> { vmobj };
                foreach ((VMobject sm1, VMobject sm2) in Utils.Iterables.MakeEvenTuples(Submobjects, vmobj.Submobjects))
                    sm1.MatchStyle(sm2);
            }
            return this;
        }

        public new VMobject SetColor(Color color, bool family = true)
        {
            SetFill(new Color[] { color }, family: family);
            SetStroke(new Color[] { color }, family: family);
            return this;
        }
        public new VMobject SetColor(Colors color, bool family = true)
        {
            return SetColor(COLORS[color], family);
        }

        public VMobject SetOpacity(double opacity, bool family = true)
        {
            SetFill(opacity: new double[] { opacity }, family: family);
            SetStroke(opacity: opacity, family: family);
            SetStroke(opacity: opacity, family: family, background: true);
            return this;
        }

        public new VMobject Fade(double darkness = 0.5, bool family = true)
        {
            double factor = 1.0 - darkness;
            SetFill(
                opacity: new double[] { factor * GetFillOpacity() },
                family: false
            );
            SetStroke(
                opacity: factor * GetStrokeOpacity(),
                family: false
            );
            SetBackgroundStroke(
                opacity: factor * GetStrokeOpacity(true),
                family: false
            );
            base.Fade(darkness, family);
            return this;
        }

        public double[][] GetFillRgbas()
        {
            // TODO: The original function refers to self.fill_rgbas,
            // but it doesn't seem to be defined anywhere
            return Style.FillColor != null ? Style.FillColor.Select(c => Utils.Color.ColorToRgba(c)).ToArray()
                : new double[][] { new double[] { 0, 0, 0, 0 } };
        }

        /// <summary>
        /// If there are multiple colors (for gradient) this returns the first one
        /// </summary>
        /// <returns></returns>
        public double[] GetFillRgba()
        {
            return GetFillRgbas()[0];
        }

        public double GetFillOpacity()
        {
            return GetFillOpacities()[0];
        }

        public double[] GetFillOpacities()
        {
            return GetFillRgbas().Select(c => c[3]).ToArray();
        }

        public Color[] GetFillColors()
        {
            return Style.FillColor != null ? Style.FillColor
                : new Color[] { new Color(0, 0, 0) };
        }

        public Color GetFillColor()
        {
            return GetFillColors()[0];
        }

        public double[][] GetStrokeRgbas(bool background = false)
        {
            if (background)
                return Style.BackgroundStrokeColor != null ? Style.BackgroundStrokeColor.Select(c => Utils.Color.ColorToRgba(c)).ToArray()
                    : new double[][] { new double[] { 0, 0, 0, 0 } };
            else
                return Style.StrokeColor != null ? Style.StrokeColor.Select(c => Utils.Color.ColorToRgba(c)).ToArray()
                    : new double[][] { new double[] { 0, 0, 0, 0 } };
        }

        public Color GetStrokeColor(bool background = false)
        {
            return GetStrokeColors(background)[0];
        }

        public double GetStrokeWidth(bool background = false)
        {
            if (background)
                return System.Math.Max(0, Style.BackgroundStrokeWidth);
            else
                return System.Math.Max(0, Style.StrokeWidth);
        }

        public double GetStrokeOpacity(bool background = false)
        {
            return GetStrokeOpacities(background)[0];
        }

        public double[] GetStrokeOpacities(bool background = false)
        {
            return GetStrokeRgbas(background).Select(c => c[3]).ToArray();
        }

        public Color[] GetStrokeColors(bool background = false)
        {
            if (background)
                return Style.BackgroundStrokeColor != null ? Style.BackgroundStrokeColor
                    : new Color[] { new Color(0, 0, 0) };
            else
                return Style.StrokeColor != null ? Style.StrokeColor
                : new Color[] { new Color(0, 0, 0) };
        }

        public new Color GetColor()
        {
            if (GetFillOpacities().All(o => o == 0))
                return GetStrokeColor();
            else
                return GetFillColor();
        }

        public VMobject SetSheenDirection(Vector<double> direction, bool family = true)
        {
            if (family)
                foreach (VMobject submob in Submobjects)
                    submob.Style.SheenDirection = direction;
            else
                Style.SheenDirection = direction;
            return this;
        }

        public VMobject SetSheen(double factor, Vector<double> direction = null, bool family = true)
        {
            if (family)
                foreach (VMobject submobj in Submobjects)
                    submobj.SetSheen(factor, direction, family);
            Style.SheenFactor = factor;
            if (direction != null)
                // family set to false because recursion will already be handled above
                SetSheenDirection(direction, false);
            // Reset color to put sheen_factor into effect
            if (factor != 0)
            {
                SetStroke(GetStrokeColors(), family: family);
                SetFill(GetFillColors(), family: family);
            }
            return this;
        }

        public (Vector<double> Start, Vector<double> End) GetGradientStartAndEndPoints()
        {
            if (ShadeIn3D)
                return Get3DGradientStartAndEndPoints();
            else
            {
                Vector<double> direction = Style.SheenDirection;
                Vector<double> c = GetCenter();
                List<Vector<double>> bases = new Vector<double>[]
                {
                    GetEdgeCenter(RIGHT) - c,
                    GetEdgeCenter(UP) - c,
                    GetEdgeCenter(OUT) - c
                }.Transpose();
                Vector<double> offset = Common.NewVector(bases.Select(v => v.DotProduct(direction)).ToArray());
                return (c - offset, c + offset);
            }
        }

        public VMobject ColorUsingBackgroundImage(string path)
        {
            Style.BackgroundImageFile = path;
            SetColor(Colors.WHITE);
            foreach (VMobject submobj in Submobjects)
                submobj.ColorUsingBackgroundImage(path);
            return this;
        }

        public VMobject MatchBackgroundImageFile(VMobject vmobj)
        {
            return ColorUsingBackgroundImage(vmobj.Style.BackgroundImageFile);
        }

        public VMobject SetShadeIn3D(bool value = true, bool zIndexAsGroup = false)
        {
            foreach (VMobject submobj in Submobjects)
            {
                submobj.ShadeIn3D = value;
                if (zIndexAsGroup)
                    submobj.ZIndexGroup = this;
            }
            return this;
        }
        #endregion

        #region Points
        public new VMobject SetPoints(params Vector<double>[] points)
        {
            Points = points?.ToList();
            return this;
        }

        public VMobject SetAnchorsAndHandles(IList<Vector<double>> anchors1, IList<Vector<double>> handles1,
            IList<Vector<double>> anchors2, IList<Vector<double>> handles2)
        {
            if (anchors1.Count == handles1.Count && anchors2.Count == handles2.Count && anchors1.Count == anchors2.Count)
                throw new ArgumentException("Number of anchors and handles are not equal");
            int nppcc = NPointsPerCubicCurve;
            int totalLength = nppcc * anchors1.Count;
            Points = Utils.SpaceOps.GetZeroVector(Dimension).Repeat(totalLength);
            IList<Vector<double>>[] arrays = new IList<Vector<double>>[]
            {
                anchors1, handles1, anchors2, handles2
            };
            for (int i = 0; i < 4; i++)
            {
                Points.ChangeSlice(arrays[i], start: i, step: nppcc);
            }
            return this;
        }

        public VMobject AppendPoints(params Vector<double>[] points)
        {
            // TODO: Check that number new points is a multiple of 4?
            // or else that if points.Count % 4 == 1, then
            // points.Count % 4 == 3?
            Points.AddRange(points);
            return this;
        }
        public VMobject AppendPoints(IEnumerable<Vector<double>> points)
        {
            // TODO: Check that number new points is a multiple of 4?
            // or else that if points.Count % 4 == 1, then
            // points.Count % 4 == 3?
            Points.AddRange(points);
            return this;
        }

        /// <summary>
        /// This is functionally similar to <see cref="AppendPoints(Vector{double}[])"/>
        /// </summary>
        public VMobject StartNewPath(Vector<double> point)
        {
            // TODO: Make sure that len(self.points) % 4 == 0?
            Points.Add(point);
            return this;
        }

        public VMobject AddCubicBezierCurve(Vector<double> anchor1, Vector<double> handle1,
            Vector<double> anchor2, Vector<double> handle2)
        {
            // TODO, check the len(self.points) % 4 == 0?
            AppendPoints(anchor1, handle1, handle2, anchor2);
            return this;
        }

        /// <summary>
        /// Add cubic bezier curve to the path.
        /// </summary>
        public VMobject AddCubicBezierCurveTo(Vector<double> handle1,
            Vector<double> handle2, Vector<double> anchor)
        {
            if (Points.Count == 0)
                throw new Exception("There must be at least one point");
            if (HasNewPathStarted())
                AppendPoints(handle1, handle2, anchor);
            else
                AppendPoints(Points[^1], handle1, handle2, anchor);
            return this;
        }

        public VMobject AddLineTo(Vector<double> point)
        {
            List<Vector<double>> newPoints = new List<Vector<double>>(3);
            foreach (double a in Utils.Iterables.LinSpace(0, 1, NPointsPerCubicCurve).Skip(1))
            {
                newPoints.Add(Utils.BezierUtil.Interpolate(Points[^1], point, a));
            }
            AddCubicBezierCurveTo(newPoints[0], newPoints[1], newPoints[2]);
            return this;
        }

        /// <summary>
        /// If two points are passed in, the first is intepreted as a handle,
        /// the second as an anchor
        /// </summary>
        public VMobject AddSmoothCurveTo(params Vector<double>[] points)
        {
            Vector<double> handle2, newAnchor;
            if (points.Length == 1)
            {
                handle2 = null;
                newAnchor = points[0];
            }
            else if (points.Length == 2)
            {
                handle2 = points[0];
                newAnchor = points[1];
            }
            else
            {
                throw new ArgumentException("AddSmoothCurveTo can only be called with one or two points");
            }

            if (HasNewPathStarted())
                AddLineTo(newAnchor);
            else
            {
                if (Points.Count == 0)
                    throw new Exception("There must be at least one point");
                Vector<double> lastH2 = Points[Points.Count - 2];
                Vector<double> lastA2 = Points[Points.Count - 1];
                var lastTangent = lastA2 - lastH2;
                var handle1 = lastA2 + lastTangent;
                if (handle2 == null)
                {
                    var toAnchorVect = newAnchor - lastA2;
                    Vector<double> newTangent = Utils.SpaceOps.RotateVector(
                        lastTangent, System.Math.PI, toAnchorVect
                    );
                    handle2 = newAnchor - newTangent;
                }
                AppendPoints(lastA2, handle1, handle2, newAnchor);
            }
            return this;
        }

        public bool HasNewPathStarted()
        {
            return Points.Count % NPointsPerCubicCurve == 1;
        }

        public bool IsClosed()
        {
            return ConsiderPointsEquals(Points[0], Points[^1]);
        }

        public Vector<double>[] AddPointsAsCorners(params Vector<double>[] points)
        {
            foreach (Vector<double> point in points)
                AddLineTo(point);
            return points;
        }

        public VMobject SetPointsAsCorners(params Vector<double>[] points)
        {
            List<List<Vector<double>>> newPoints = new List<List<Vector<double>>>(4);
            foreach (double a in Utils.Iterables.LinSpace(0, 1, NPointsPerCubicCurve))
            {
                newPoints.Add(
                    Points.Take(Points.Count - 1).Zip(Points.Skip(1),
                        (p1, p2) => Utils.BezierUtil.Interpolate(p1, p2, a)).ToList()
                );
            }
            SetAnchorsAndHandles(newPoints[0], newPoints[1], newPoints[2], newPoints[3]);
            return this;
        }

        public VMobject SetPointsSmoothly(params Vector<double>[] points)
        {
            SetPointsAsCorners(points);
            MakeSmooth();
            return this;
        }

        public VMobject ChangeAnchorMode(AnchorMode mode)
        {
            foreach (VMobject submobj in GetFamilyMembersWithPoints())
            {
                var subpaths = submobj.GetSubpaths();
                submobj.Points.Clear();
                foreach (List<Vector<double>> subpath in subpaths)
                {
                    // Is it okay to set the last element as null?
                    // In manimpy, it's set to 0, but that doesn't work
                    // here because it's a list of vectors
                    var anchors = subpath.Where((x, i) => i % NPointsPerCubicCurve == 0).ToList()
                        .Concat(new List<Vector<double>>() { subpath.Last(), null });
                    List<Vector<double>> h1, h2;
                    switch (mode)
                    {
                        case AnchorMode.Jagged:
                            var smoothHandles = Utils.BezierUtil.GetSmoothHandlePoints(anchors.ToList());
                            h1 = smoothHandles.Item1;
                            h2 = smoothHandles.Item2;
                            break;

                        case AnchorMode.Smooth:
                            // Find something better to take all but the last item.
                            // This results in two enumerations of the list;
                            var a1 = anchors.Take(anchors.Count());
                            var a2 = anchors.Skip(1);
                            h1 = Utils.BezierUtil.Interpolate(a1, a2, 1.0 / 3).ToList();
                            h2 = Utils.BezierUtil.Interpolate(a1, a2, 2.0 / 3).ToList();
                            break;

                        default:
                            throw new ArgumentException("mode must be an anchor mode");
                    }
                    submobj.AppendPoints(subpath
                        .SetSlice(h1, start: 1, step: NPointsPerCubicCurve)
                        .SetSlice(h2, start: 2, step: NPointsPerCubicCurve));
                }
            }
            return this;
        }

        public VMobject MakeSmooth()
        {
            return ChangeAnchorMode(AnchorMode.Smooth);
        }

        public VMobject MakeJagged()
        {
            return ChangeAnchorMode(AnchorMode.Jagged);
        }

        public VMobject AddSubpath(params Vector<double>[] points)
        {
            if (points.Length % 4 != 0)
                throw new ArgumentException("points.Length must be divisible by four");
            Points.AddRange(points);
            return this;
        }

        public VMobject AppendVectorizedMobject(VMobject vmobj)
        {
            List<Vector<double>> newPoints = vmobj.Points;
            // Remove last point, which is starting a new path
            if (HasNewPathStarted())
                Points = Points.Slice(end: -1);
            AppendPoints(newPoints);
            return this;
        }

        public new VMobject ApplyFunction(Func<Vector<double>, Vector<double>> func, Vector<double> aboutPoint = null, Vector<double> aboutEdge = null)
        {
            double factor = PreFunctionHandleToAnchorScaleFactor;
            ScaleHandleToAnchorDistances(factor);
            // TODO: Should this be passing aboutPoint and aboutEdge?
            base.ApplyFunction(func, aboutPoint, aboutEdge);
            ScaleHandleToAnchorDistances(1 / factor);
            if (MakeSmoothAfterApplyingFunctions)
                MakeSmooth();
            return this;
        }

        /// <summary>
        /// If the distance between a given handle point H and its associated
        /// anchor point A is d, then it changes H to be a distances factor* d
        /// away from A, but so that the line from A to H doesn't change.
        /// This is mostly useful in the context of applying a (differentiable)
        /// function, to preserve tangency properties.One would pull all the
        /// handles closer to their anchors, apply the function then push them out
        /// again.
        /// </summary>
        public VMobject ScaleHandleToAnchorDistances(double factor)
        {
            foreach (VMobject submobj in GetFamilyMembersWithPoints())
            {
                if (submobj.Points.Count < NPointsPerCubicCurve)
                    continue;
                var AAH = GetAnchorsAndHandlesAsTuple();
                var a1Toh1 = AAH.handles1.Zip(AAH.anchors1, (h1, a1) => h1 - a1).ToArray();
                var a2Toh2 = AAH.handles2.Zip(AAH.anchors2, (h2, a2) => h2 - a2).ToArray();
                var newH1 = AAH.anchors1.Zip(a1Toh1, (a1, ah1) => a1 + factor * ah1).ToArray();
                var newH2 = AAH.anchors2.Zip(a2Toh2, (a2, ah2) => a2 + factor * ah2).ToArray();
                submobj.SetAnchorsAndHandles(AAH.anchors1, newH1, AAH.anchors2, newH2);
            }
            return this;
        }

        /// <summary>
        /// Compares each components of the two vectors and determines if
        /// they are equal within the VMobject's <see cref="ToleranceForPointEquality"/>.
        /// </summary>
        public bool ConsiderPointsEquals(Vector<double> p1, Vector<double> p2)
        {
            return (p1 - p2).All(d => System.Math.Abs(d) < ToleranceForPointEquality);
        }
        #endregion

        #region Line info
        public IEnumerable<List<Vector<double>>> GetCubicBezierTuplesFromPoints(IList<Vector<double>> points)
        {
            int remainder = points.Count % NPointsPerCubicCurve;
            points = points.Slice(step: points.Count - remainder);
            for (int i = 0; i < points.Count; i += NPointsPerCubicCurve)
            {
                yield return points.Slice(start: i, end: i + NPointsPerCubicCurve);
            }
        }

        public IEnumerable<List<Vector<double>>> GetCubicBezierTuples()
        {
            return GetCubicBezierTuplesFromPoints(Points);
        }

        public IEnumerable<List<Vector<double>>> GetSubpathsFromPoints(params Vector<double>[] points)
        {
            IEnumerable<int> splitIndexes = Utils.Iterables.CreateRange(NPointsPerCubicCurve, Points.Count, NPointsPerCubicCurve)
                .Where(n => ConsiderPointsEquals(Points[n - 1], Points[n]));
            splitIndexes = (new int[] { 0 }).Concat(splitIndexes).Concat(new int[] { points.Length });
            foreach ((int i1, int i2) in splitIndexes.Zip(splitIndexes.Skip(1), (a, b) => (a, b)))
            {
                if (i2 - i1 >= NPointsPerCubicCurve)
                    yield return points[i1..i2].ToList();
            }
        }

        public IEnumerable<List<Vector<double>>> GetSubpaths()
        {
            return GetSubpathsFromPoints(Points.ToArray());
        }

        public List<Vector<double>> GetNthCurvePoints(int n)
        {
            if (n >= GetNumCurves())
                throw new ArgumentException("n must be less than the number of curves");
            return Points.GetRange(NPointsPerCubicCurve * n, NPointsPerCubicCurve * (n + 1));
        }

        public Func<double, Vector<double>> GetNthCurveFunction(int n)
        {
            return Utils.BezierUtil.Bezier(GetNthCurvePoints(n));
        }

        public int GetNumCurves()
        {
            return Points.Count / NPointsPerCubicCurve;
        }

        public override Vector<double> PointFromProportion(double alpha)
        {
            (int n, double residue) = Utils.BezierUtil.InterpolateInteger(0, GetNumCurves(), alpha);
            return GetNthCurveFunction(n)(residue);
        }

        /// <summary>
        /// Returns anchors1, handles1, handles2, anchors2,
        /// where (anchors1[i], handles1[i], handles2[i], anchors2[i])
        /// will be four points defining a cubic bezier curve
        /// for any i in range(0, anchors1.Count)
        /// </summary>
        public IEnumerable<List<Vector<double>>> GetAnchorsAndHandles()
        {
            for (int i = 0; i < NPointsPerCubicCurve; i++)
            {
                yield return Points.Slice(start: i, step: NPointsPerCubicCurve);
            }
            //throw new NotImplementedException("This function may be incorrect. Please review.");
        }
        public (List<Vector<double>> anchors1, List<Vector<double>> anchors2, List<Vector<double>> handles2, List<Vector<double>> handles1)
            GetAnchorsAndHandlesAsTuple()
        {
            // TODO: Double check that this works properly
            var anchorsAndHandles = GetAnchorsAndHandles();
            return (anchorsAndHandles.ElementAt(0), anchorsAndHandles.ElementAt(1),
                anchorsAndHandles.ElementAt(2), anchorsAndHandles.ElementAt(3));
        }

        public List<Vector<double>> GetStartAnchors()
        {
            return Points.Slice(start: 0, step: NPointsPerCubicCurve);
        }

        public List<Vector<double>> GetEndAnchors()
        {
            return Points.Slice(start: NPointsPerCubicCurve - 1, step: NPointsPerCubicCurve);
        }

        public List<Vector<double>> GetAnchors()
        {
            return Utils.Iterables.Interleave(GetStartAnchors(), GetEndAnchors());
        }

        public List<Vector<double>> GetPointsDefiningBoundary()
        {
            var boundary = new List<Vector<double>>();
            // TODO: Shouldn't this be GetFamilyMembersWithPoints, not get_family()?
            foreach (VMobject vmobj in Submobjects)
                boundary.AddRange(vmobj.GetAnchors());
            return boundary;
        }

        public double GetArcLength(int nSamplePoints = -1)
        {
            if (nSamplePoints < 0)
                nSamplePoints = 4 * GetNumCurves() + 1;
            var points = Utils.Iterables.LinSpace(0, 1, nSamplePoints)
                .Select(a => PointFromProportion(a)).ToArray();
            var diffs = points.Skip(1).Zip(points.Slice(end: -1), (p1, p2) => p1 - p2);
            var norms = diffs.Select(v => v.L2Norm());
            return norms.Sum();
        }
        #endregion

        #region Alignment
        public VMobject AlignPoints(VMobject vmobj)
        {
            AlignRgbas(vmobj);
            if (Points.Count == vmobj.Points.Count)
                // For some reason, manimpy does "return", without returning itself
                return this;
            foreach (VMobject submobj in Submobjects.Concat(vmobj.Submobjects))
            {
                // If there are no points, add one to wherever the "center" is
                if (submobj.Points.Count == 0)
                    submobj.StartNewPath(submobj.GetCenter());
                // If there's only one point, turn it into a null curve
                if (submobj.HasNewPathStarted())
                    submobj.AddLineTo(submobj.Points[^1]);
            }
            // Figure out what the subpaths are, and align
            List<List<Vector<double>>> subpaths1 = GetSubpaths().ToList();
            List<List<Vector<double>>> subpaths2 = vmobj.GetSubpaths().ToList();
            int nSubpaths = System.Math.Max(subpaths1.Count(), subpaths2.Count());
            // Start building new ones
            List<Vector<double>> newPath1 = new List<Vector<double>>();
            List<Vector<double>> newPath2 = new List<Vector<double>>();

            List<Vector<double>> GetNthSubpath(List<List<Vector<double>>> pathList, int n)
            {
                if (n >= pathList.Count())
                    return pathList[^1][^1].Repeat(n);
                return pathList[n];
            }

            for (int n = 0; n < nSubpaths; n++)
            {
                List<Vector<double>> sp1 = GetNthSubpath(subpaths1, n);
                List<Vector<double>> sp2 = GetNthSubpath(subpaths2, n);
                int diff1 = System.Math.Max(0, (sp2.Count - sp1.Count) / NPointsPerCubicCurve);
                int diff2 = System.Math.Max(0, (sp1.Count - sp2.Count) / NPointsPerCubicCurve);
                sp1 = InsertNCurvesToPointList(diff1, sp1);
                sp2 = InsertNCurvesToPointList(diff2, sp2);
                newPath1.AddRange(sp1);
                newPath2.AddRange(sp2);
            }
            SetPoints(newPath1);
            vmobj.SetPoints(newPath2);
            return this;
        }

        public VMobject InsertNCurves(int n)
        {
            Vector<double> newPathPoint = null;
            if (HasNewPathStarted())
                newPathPoint = Points[^1];

            SetPoints(InsertNCurvesToPointList(n, Points));

            if (newPathPoint != null)
                Points.Add(newPathPoint);
            return this;
        }

        public List<Vector<double>> InsertNCurvesToPointList(int n, List<Vector<double>> points)
        {
            if (points.Count == 1)
                return points[0].Repeat(NPointsPerCubicCurve * n);
            var bezierQuads = GetCubicBezierTuplesFromPoints(points).ToList();
            int currNum = bezierQuads.Count;
            int targetNum = currNum + n;
            // This is an array with values ranging from 0
            // up to currNum, with repeats such that
            // it's total length is targetNum. For example,
            // with currNum = 10, targetNum = 15, this would
            // be [0, 0, 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 8, 8, 9]
            IEnumerable<int> repeatIndicies = Utils.Iterables.CreateRange(targetNum)
                .Select(a => a * currNum / NPointsPerCubicCurve);

            // If the nth term of this list is k, it means
            // that the nth curve of our path should be split
            // into k pieces. In the above example, this would
            // be [2, 1, 2, 1, 2, 1, 2, 1, 2, 1]
            List<int> splitFactors = new List<int>();
            for (int i = 0; i < currNum; i++)
                splitFactors.Add(repeatIndicies.Sum(d => (d == i) ? 1 : 0));
            List<Vector<double>> newPoints = new List<Vector<double>>();
            foreach ((List<Vector<double>> quads, int sf) in bezierQuads.Zip(splitFactors, (q, s) => (q, s)))
            {
                // What was once a single cubic curve defined
                // by "quad" will now be broken into sf
                // smaller cubic curves
                // TODO: Are these actually ints, or should they be doubles?
                int[] alphas = Utils.Iterables.LinSpace(0, 1, sf + 1).ToArray();
                foreach ((int a1, int a2) in alphas.Zip(alphas, (b, c) => (b, c)))
                {
                    newPoints.AddRange(Utils.BezierUtil.PartialBezierPoints(quads, a1, a2));
                }
            }
            return newPoints;
        }

        public VMobject AlignRgbas(VMobject vmobj)
        {
            string[] attrs = new string[] { "fill_rgbas", "stroke_rgbas", "background_stroke_rgbas" };
            foreach (string attr in attrs)
            {
                var a1 = Attributes[attr] as List<double[]>;
                var a2 = vmobj.Attributes[attr] as List<double[]>;
                if (a1.Count > a2.Count)
                {
                    List<double[]> newA2 = Utils.Iterables.StretchArrayToLength(a2, a1.Count).ToList();
                    vmobj.Attributes[attr] = newA2;
                }
                else if (a2.Count > a1.Count)
                {
                    List<double[]> newA1 = Utils.Iterables.StretchArrayToLength(a1, a2.Count).ToList();
                    vmobj.Attributes[attr] = newA1;
                }
            }
            return this;
        }

        public new VectorizedPoint GetPointMobject(Vector<double> center = null)
        {
            if (center == null)
                center = GetCenter();
            VectorizedPoint point = new VectorizedPoint(center);
            point.MatchStyle(this);
            return point;
        }

        public void InterpolateColor(VMobject mobj1, VMobject mobj2, double alpha)
        {
            string[] attrs = new string[]
            {
                "fill_rgbas",
                "stroke_rgbas",
                "background_stroke_rgbas",
                "stroke_width",
                "background_stroke_width",
                "sheen_direction",
                "sheen_factor"
            };
            foreach (string attr in attrs)
            {
                Attributes[attr] = Utils.BezierUtil.Interpolate(
                    mobj1.Attributes[attr] as IEnumerable<double>,
                    mobj1.Attributes[attr] as IEnumerable<double>,
                    alpha
                );
                if (alpha == 1.0)
                    Attributes[attr] = mobj2.Attributes[attr];
            }
        }

        public VMobject PointwiseBecomePartial(VMobject vmobj, int a, int b)
        {
            // Partial curve includes three portions:
            // - A middle section, which matches the curve exactly
            // - A start, which is some ending portion of an inner cubic
            // - An end, which is the starting portion of a later inner cubic
            if (a <= 0 && b >= 1)
            {
                SetPoints(vmobj.Points);
                return this;
            }
            var bezierQuads = vmobj.GetCubicBezierTuples();
            int numCubics = bezierQuads.Count();
            (int lowerIndex, double lowerResidue) = Utils.BezierUtil.InterpolateInteger(0, numCubics, a);
            (int upperIndex, double upperResidue) = Utils.BezierUtil.InterpolateInteger(0, numCubics, b);

            Points.Clear();
            if (numCubics == 0)
                return this;
            if (lowerIndex == upperIndex)
            {
                AppendPoints(Utils.BezierUtil.PartialBezierPoints(
                    bezierQuads.ElementAt(lowerIndex),
                    lowerResidue, upperResidue)
                );
            }
            else
            {
                AppendPoints(Utils.BezierUtil.PartialBezierPoints(
                    bezierQuads.ElementAt(lowerIndex),
                    lowerResidue, 1)
                );
                // Equivalent to "for quad in bezier_quads[lower_index + 1:upper_index]: append_points(quad)"
                foreach (var quad in bezierQuads.Skip(lowerIndex + 1).Take(upperIndex - lowerIndex + 1))
                    AppendPoints(quad);
                AppendPoints(Utils.BezierUtil.PartialBezierPoints(
                    bezierQuads.ElementAt(upperIndex),
                    0, upperResidue)
                );
            }
            return this;
        }

        public VMobject GetSubcurve(double a, double b)
        {
            // This function is fishy...
            // TODO: Double check that Mobject can be cast to VMobject,
            // and that PointwiseBecomePartial is supposed to take ints and
            // not doubles.
            var vmobj = (VMobject)Copy();
            vmobj.PointwiseBecomePartial(this, (int)a, (int)b);
            return vmobj;
        }
        #endregion

        public sealed class VMobjectStyle
        {
            public Color[] FillColor { get; set; }
            public double[] FillOpacity { get; set; } = new double[] { 0.0 };

            public Color[] StrokeColor { get; set; }
            public double StrokeOpacity { get; set; } = 1.0;
            public double StrokeWidth { get; set; } = 0.0;

            // The purpose of background stroke is to have
            // something that won't overlap the fill, e.g.
            // For text against some textured background
            public Color[] BackgroundStrokeColor { get; set; } = new Color[] { COLORS[Colors.BLACK] };
            public double BackgroundStrokeOpacity { get; set; } = 1.0;
            public double BackgroundStrokeWidth { get; set; } = 0.0;

            // When a color c is set, there will be a second color
            // computed based on interpolating c to WHITE by with
            // sheen_factor, and the display will gradient to this
            // secondary color in the direction of sheen_direction.
            public double SheenFactor { get; set; } = 0.0;
            public Vector<double> SheenDirection { get; set; } = UL;

            public string BackgroundImageFile { get; set; }


            public static explicit operator Dictionary<string, object>(VMobjectStyle style)
            {
                return new Dictionary<string, object>()
                {
                    { "fill_color", style.FillColor },
                    { "fill_opacity", style.FillOpacity },
                    { "stroke_color", style.StrokeColor },
                    { "stroke_width", style.StrokeWidth },
                    { "stroke_opacity", style.StrokeOpacity},
                    { "background_stroke_color", style.BackgroundStrokeColor},
                    { "background_stroke_width", style.BackgroundStrokeWidth},
                    { "background_stroke_opacity", style.BackgroundStrokeOpacity},
                    { "sheen_factor", style.SheenFactor },
                    { "sheen_direction", style.SheenDirection },
                    { "background_image_file", style.BackgroundStrokeOpacity},
                };
            }

            public static explicit operator VMobjectStyle(Dictionary<string, object> dict)
            {
                return new VMobjectStyle()
                {
                    FillColor = (Color[])dict["fill_color"],
                    FillOpacity = (double[])dict["fillOpacity"],
                    StrokeColor = (Color[])dict["strokeColor"],
                    StrokeWidth = (double)dict["strokeWidth"],
                    StrokeOpacity = (double)dict["strokeOpacity"],
                    BackgroundStrokeColor = (Color[])dict["backgroundStrokeColor"],
                    BackgroundStrokeWidth = (double)dict["backgroundStrokeWidth"],
                    BackgroundStrokeOpacity = (double)dict["backgroundStrokeOpacity"],
                    SheenFactor = (double)dict["sheenFactor"],
                    SheenDirection = (Vector<double>)dict["sheenDirection"],
                    BackgroundImageFile = (string)dict["backgroundStrokeOpacity"],
                };
            }
        }

        public enum AnchorMode
        {
            Jagged,
            Smooth
        }
    }

    // TODO: The following classes should really be in their own file

    public class VGroup : VMobject
    {
        public VGroup(params VMobject[] vmobjs) : base()
        {
            Add(vmobjs);
        }

        public VGroup(string name = null, Color color = default, int dim = 3, Mobject target = null, params VMobject[] vmobjs)
            : base(name, color, dim, target)
        {
            Add(vmobjs);
        }

    }

    public class VectorizedPoint : VMobject
    {
        #region Properties
        //public Color Color { get; set; }
        public double FillOpacity { get; set; }
        public double StrokeWidth { get; set; }
        public double ArtificialWidth { get; set; }
        public double ArtificialHeight { get; set; }
        #endregion

        public VectorizedPoint(Vector<double> location = null) : base()
        {
            SetPoints(location);
        }
        public VectorizedPoint(string name = null, Color color = default, int dim = 3, Mobject target = null, Vector<double> location = null)
            : base(name, color, dim, target)
        {
            SetPoints(location);
        }

        public new double GetWidth()
        {
            return ArtificialWidth;
        }

        public new double GetHeight()
        {
            return ArtificialHeight;
        }

        public Vector<double> GetLocation()
        {
            return Points[0];
        }

        public void SetLocation(Vector<double> location)
        {
            SetPoints(location);
        }
    }

    public class CurvesAsSubmobjects : VGroup
    {
        public CurvesAsSubmobjects(VMobject vmobj, string name = null, Color color = default, int dim = 3, Mobject target = null)
            : base(name, color, dim, target)
        {
            IEnumerable<List<Vector<double>>> tuples = vmobj.GetCubicBezierTuples();
            foreach (List<Vector<double>> tup in tuples)
            {
                var part = new VMobject();
                part.SetPoints(tup);
                part.MatchStyle(vmobj);
                Add(part);
            }
        }
    }

    public class DashedVMobject : VMobject
    {
        public int NumDashes { get; set; } = 15;
        public double PositiveSpaceRatio { get; set; } = 0.5;
        //public Color Color { get; set; } = COLORS[Colors.WHITE];

        public DashedVMobject(VMobject vmobj, int numDashes = 15, double positiveSpaceRatio = 0.5,
            string name = null, Color color = default, int dim = 3, Mobject target = null)
            : base(name, color, dim, target)
        {
            NumDashes = numDashes;
            PositiveSpaceRatio = positiveSpaceRatio;
            if (NumDashes > 0)
            {
                // End points of the unit interval for division
                IEnumerable<double> alphas = Utils.Iterables.LinSpace(0.0, 1.0, NumDashes + 1);

                // This determines the length of each "dash"
                double fullDAlpha = 1.0 / NumDashes;
                double partialDAlpha = fullDAlpha * PositiveSpaceRatio;

                // Rescale so that the last point of vmobject will
                // be the end of the last dash
                alphas = alphas.Select(a => a / (1 - fullDAlpha + partialDAlpha));

                foreach (double alpha in alphas.ToArray().Slice(end: -1))
                {
                    Add(vmobj.GetSubcurve(alpha, alpha + partialDAlpha));
                }
            }
            // Family is already taken care of by GetSubcurve implementation
            MatchStyle(vmobj, false);
        }
    }

}
