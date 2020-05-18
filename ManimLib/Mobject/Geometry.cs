using ManimLib.Mobject.Types;
using ManimLib.Utils;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using static ManimLib.Constants;
using Color = RL.Color;
using MathNet.Spatial.Euclidean;
using MathNet.Numerics.Integration;

namespace ManimLib.Mobject
{
    /// <summary>
    /// Meant for shared functionality between Arc and Line.
    /// Functionality can be classified broadly into these groups:
    /// 
    /// * Adding, Creating, Modifying tips
    ///     - add_tip calls create_tip, before pushing the new tip
    ///     into the TipableVMobject's list of submobjects
    ///     - stylistic and positional configuration
    ///
    /// * Checking for tips
    ///     - Boolean checks for whether the TipableVMobject has a tip
    ///         and a starting tip
    /// 
    /// * Getters
    ///     - Straightforward accessors, returning information pertaining
    ///         to the TipableVMobject instance's tip(s), its length etc
    /// </summary>
    public class TipableVMobject : VMobject
    {
        #region Properties
        public const double DEFAULT_DOT_RADIUS = 0.08;
        public const double DEFAULT_SMALL_DOT_RADIUS = 0.04;
        public const double DEFAULT_DASH_LENGTH = 0.05;
        public const double DEFAULT_ARROW_TIP_LENGTH = 0.35;

        public double TipLength { get; set; } = DEFAULT_ARROW_TIP_LENGTH;
        public Vector<double> NormalVector { get; set; } = OUT;
        public VMobjectStyle TipStyle { get; set; } = new VMobjectStyle()
        {
            FillOpacity = new double[] { 1 },
            StrokeWidth = 0
        };
        public ArrowTip Tip { get; set; }
        public ArrowTip StartTip { get; set; }
        #endregion

        /// <summary>
        /// Adds a tip to the TipableVMobject instance, recognising
        /// that the endpoints might need to be switched if it's
        /// a 'starting tip' or not.
        /// </summary>
        public TipableVMobject AddTip(double tipLength = DEFAULT_ARROW_TIP_LENGTH, bool atStart = false)
        {
            ArrowTip tip = CreateTip(tipLength, atStart);
            ResetEndpointsBasedOnTip(tip, atStart);
            AssignTipAttribute(tip, atStart);
            Add(tip);
            return this;
        }

        /// <summary>
        /// Stylises the tip, positions it spacially, and returns
        /// the newly instantiated tip to the caller.
        /// </summary>
        public ArrowTip CreateTip(double tipLength = DEFAULT_ARROW_TIP_LENGTH, bool atStart = false)
        {
            ArrowTip tip = GetUnpositionedTip(tipLength);
            PositionTip(tip, atStart);
            return tip;
        }

        /// <summary>
        /// Returns a tip that has been stylistically configured,
        /// but has not yet been given a position in space.
        /// </summary>
        public ArrowTip GetUnpositionedTip(double tipLength = DEFAULT_ARROW_TIP_LENGTH)
        {
            Color[] color = new Color[] { GetColor() };
            VMobjectStyle style = Style;
            style.FillColor = color;
            style.StrokeColor = color;
            return new ArrowTip(tipLength, style);
        }

        public ArrowTip PositionTip(ArrowTip tip, bool atStart = false)
        {
            // Last two control points, defining both
            // the end, and the tangency direction
            Vector<double> anchor, handle;
            if (atStart)
            {
                anchor = GetStart();
                handle = GetFirstHandle();
            }
            else
            {
                anchor = GetEnd();
                handle = GetLastHandle();
            }
            tip.Rotate(
                SpaceOps.GetVectorAngle(handle - anchor) - System.Math.PI - tip.GetAngle()
            );
            tip.Shift(anchor - tip.GetTipPoint());
            return tip;
        }

        public TipableVMobject ResetEndpointsBasedOnTip(ArrowTip tip, bool atStart)
        {
            if (GetLength() == 0)
            {
                // Zero length, PutStartAndEndOn() wouldn't work
                return this;
            }

            if (atStart)
            {
                PutStartAndEndOn(tip.GetBase(), GetEnd());
            }
            else
            {
                PutStartAndEndOn(GetStart(), tip.GetBase());
            }
            return this;
        }

        public TipableVMobject AssignTipAttribute(ArrowTip tip, bool atStart)
        {
            if (atStart)
                StartTip = tip;
            else
                Tip = tip;
            return this;
        }

        public bool HasTip()
        {
            return Tip != null;
        }

        public bool HasStartTip()
        {
            return StartTip != null;
        }

        public VGroup PopTips()
        {
            (Vector<double> start, Vector<double> end) = GetStartAndEnd();
            VGroup result = new VGroup();
            if (HasTip())
            {
                result.Add(Tip);
                Remove(Tip);
            }
            if (HasStartTip())
            {
                result.Add(StartTip);
                Remove(StartTip);
            }
            PutStartAndEndOn(start, end);
            return result;
        }

        /// <summary>
        /// Returns a <see cref="VGroup"/> (collection of <see cref="VMobject"/>s) containing
        /// the <see cref="TipableVMobject"/> instance's tips.
        /// </summary>
        public VGroup GetTips()
        {
            VGroup result = new VGroup();
            if (HasTip())
                result.Add(Tip);
            if (HasStartTip())
                result.Add(StartTip);
            return result;
        }

        /// <summary>
        /// Returns the <see cref="TipableVMobject"/> instance's (first) tip,
        /// otherwise throws an exception.
        /// </summary>
        /// <returns></returns>
        public ArrowTip GetTip()
        {
            var tips = GetTips();
            if (tips.Count == 0)
                throw new Exception("Tip not found");
            return (ArrowTip)tips[0];
        }

        public Vector<double> GetFirstHandle()
        {
            return Points[1];
        }

        public Vector<double> GetLastHandle()
        {
            return Points[^2];
        }

        public new Vector<double> GetEnd()
        {
            if (HasTip())
                return Tip.GetStart();
            else
                return base.GetEnd();
        }

        public new Vector<double> GetStart()
        {
            if (HasStartTip())
                return StartTip.GetStart();
            else
                return base.GetStart();
        }

        public double GetLength()
        {
            (Vector<double> start, Vector<double> end) = GetStartAndEnd();
            return (start - end).L2Norm();
        }
    }

    public class Arc : TipableVMobject
    {
        public double Radius { get; set; }
        public double StartAngle { get; set; }
        public double Angle { get; set; }
        public int NumComponents { get; set; } = 9;
        public bool AnchorsSpanFullRange { get; set; } = true;
        public Vector<double> ArcCenter { get; set; } = ORIGIN;

        public Arc(double startAngle = 0, double angle = TAU / 4, double radius = 1.0, string name = null, Color color = null, int dim = 3, Mobject target = null)
        {
            Name = name;
            Color = color;
            Dimension = dim;
            Target = target;

            StartAngle = startAngle;
            Angle = angle;
            Radius = radius;
        }

        public void GeneratePoints()
        {
            SetPrePositionedPoints();
            Scale(Radius, aboutPoint: ORIGIN);
            Shift(ArcCenter);
        }

        public void SetPrePositionedPoints()
        {
            IEnumerable<double> linSpace = Iterables.LinSpace(StartAngle, StartAngle + Angle, NumComponents);
            List<Vector<double>> anchors = new List<Vector<double>>(linSpace.Count());
            foreach (double a in linSpace)
            {
                anchors.Add(System.Math.Cos(a) * RIGHT + System.Math.Sin(a) * UP);
            }
            // Figure out which control points will give the
            // appropriate tangent lines to the circle
            double dTheta = Angle / (NumComponents - 1.0);
            List<Vector<double>> tangentVectors = SpaceOps.GetZeroVector(anchors[0].Count).Repeat(anchors.Count);
            // Rotate all 90 degress, via (x, y) => (-y, x)
            for (int i = 0; i < anchors.Count; i++)
            {
                tangentVectors[i][1] = anchors[i][0];
                tangentVectors[i][0] = -anchors[i][1];
            }
            // Use tangent vectors to deduce anchors
            int handlesCount = anchors.Count - 1;
            List<Vector<double>> handles1 = new List<Vector<double>>(handlesCount);
            List<Vector<double>> handles2 = new List<Vector<double>>(handlesCount);
            for (int i = 0; i < handlesCount; i++)
            {
                handles1[i] = anchors[i] + (dTheta / 3) * tangentVectors[i];
                handles2[i] = anchors[i + 1] - (dTheta / 3) * tangentVectors[i + 1];
            }
            List<Vector<double>> anchors1 = new List<Vector<double>>(anchors);
            anchors1.RemoveAt(anchors.Count - 1);
            List<Vector<double>> anchors2 = new List<Vector<double>>(anchors.Skip(1));
            SetAnchorsAndHandles(anchors1, handles1, handles2, anchors2);
        }

        /// <summary>
        /// Looks at the normals to the first two
        /// anchors, and finds their intersection points
        /// </summary>
        public Vector<double> GetArcCenter()
        {
            // First two anchors and handles
            Vector<double> a1 = Points[0];
            Vector<double> h1 = Points[1];
            Vector<double> a2 = Points[2];
            Vector<double> h2 = Points[3];
            // Tangent vectors
            Vector<double> t1 = h1 - a1;
            Vector<double> t2 = h2 - a2;
            // Normals
            Vector<double> n1 = SpaceOps.RotateVector(t1, TAU / 4);
            Vector<double> n2 = SpaceOps.RotateVector(t2, TAU / 4);
            try
            {
                return SpaceOps.LineIntersection((a1, a1 + n1), (a2, a2 + n2));
            }
            catch
            {
                // Can't find Arc center, using ORIGIN instead
                return ORIGIN;
            }
        }

        public Arc MoveArcCenterTo(Vector<double> point)
        {
            Shift(point - GetArcCenter());
            return this;
        }

        public double StopAngle()
        {
            return SpaceOps.GetVectorAngle(Points[^1] - GetArcCenter()) % TAU;
        }
    }

    public class ArcBetweenPoints : Arc
    {
        public ArcBetweenPoints(Vector<double> start, Vector<double> end, double angle = TAU / 4, string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(0, angle, 1, name, color, dim, target)
        {
            if (angle == 0)
                SetPointsAsCorners(LEFT, RIGHT);
            PutStartAndEndOn(start, end);
        }
    }

    public class CurvedArrow : ArcBetweenPoints
    {
        public CurvedArrow(Vector<double> start, Vector<double> end, double angle = TAU / 4, string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(start, end, angle, name, color, dim, target)
        {
            AddTip();
        }
    }

    public class CurvedDoubleArrow : CurvedArrow
    {
        public CurvedDoubleArrow(Vector<double> start, Vector<double> end, double angle = TAU / 4, string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(start, end, angle, name, color, dim, target)
        {
            AddTip(atStart: true);
        }
    }

    public class Circle : Arc
    {
        public Circle(double radius = 1, string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(0, TAU, radius, name, color, dim, target)
        {
            Color = COLORS[Colors.RED_A];
            CloseNewPoints = true;
            AnchorsSpanFullRange = false;
        }

        public new void Surround(Mobject mobject, int dimToMatch = 0, bool stretch = false, double bufferFactor = 1.2)
        {
            // Ignores dim_to_match and stretch; result will always be a circle
            // TODO: Perhaps create an ellipse class to handle singele-dimension stretching

            // Something goes wrong here when surrounding lines?
            // TODO: Figure out and fix
            Replace(mobject, dimToMatch, stretch);

            SetWidth(System.Math.Sqrt(
                System.Math.Pow(GetWidth(), 2) + System.Math.Pow(GetHeight(), 2)
            ));
            Scale(bufferFactor);
        }
    
        public Vector<double> PointAtAngle(double angle)
        {
            double startAngle = SpaceOps.GetVectorAngle(Points[0] - GetCenter());
            return PointFromProportion((angle - startAngle) / TAU);
        }
    }

    public class Dot : Circle
    {
        public Dot(Vector<double> point = null, string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(DEFAULT_DOT_RADIUS, name, color, dim, target)
        {
            ArcCenter = point ?? ORIGIN;
        }
    }

    public class SmallDot : Dot
    {
        public SmallDot(Vector<double> point = null, string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(point, name, color, dim, target)
        {
            Radius = DEFAULT_SMALL_DOT_RADIUS;
        }
    }

    public class Ellipse : Circle
    {
        public Ellipse(double width = 2, double height = 1, string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(DEFAULT_DOT_RADIUS, name, color, dim, target)
        {
            SetWidth(width, true);
            SetHeight(height, true);
        }
    }

    // AnnularSector(Arc)

    // Sector(AnnularSector)

    // Annulus(Circle)

    public class Line : TipableVMobject
    {
        public double Buff { get; set; } = 0;
        public double? PathArc { get; set; }
        public Vector<double> Start { get; set; }
        public Vector<double> End { get; set; }

        public Line(Vector<double> start, Vector<double> end, double angle = TAU / 4, string name = null, Color color = null, int dim = 3, Mobject target = null)
        {
            Name = name;
            Color = color;
            Dimension = dim;
            Target = target;

            SetStartAndEndAttrs(start, end);
        }
    
        public void GeneratePoints()
        {
            if (PathArc.HasValue)
            {
                ArcBetweenPoints arc = new ArcBetweenPoints(Start, End, PathArc.Value);
                SetPoints(arc.Points);
            }
            else
            {
                SetPointsAsCorners(Start, End);
            }
            AccountForBuff();
        }

        public void SetPathArc(double newPathArc)
        {
            PathArc = newPathArc;
            GeneratePoints();
        }

        public Line AccountForBuff()
        {
            if (Buff == 0)
                return this;

            double length;
            if (!PathArc.HasValue || PathArc.Value == 0)
                length = GetLength();
            else
                length = GetArcLength();

            if (length < Buff * 2)
                return this;

            double buffProportion = Buff / length;
            PointwiseBecomePartial(this, (int)buffProportion, (int)(1 - buffProportion));
            return this;
        }

        public void SetStartAndEndAttrs(Vector<double> start, Vector<double> end)
        {
            Start = start;
            End = end;
        }
        public void SetStartAndEndAttrs(Mobject start, Mobject end)
        {
            Vector<double> roughStart = Pointify(start);
            Vector<double> roughEnd = Pointify(end);
            Vector<double> vect = SpaceOps.Normalize(roughEnd - roughStart);
            // Now that we know the direction between them,
            // we can the appropriate boundary point from
            // start and end, if they're mobjects
            Start = Pointify(start, vect);
            End = Pointify(end, -vect);
        }

        public static Vector<double> Pointify(Mobject mobj, Vector<double> direction = null)
        {
            if (direction == null)
                return mobj.GetCenter();
            else
                return mobj.GetBoundaryPoint(direction);
        }

        public new Mobject PutStartAndEndOn(Vector<double> start, Vector<double> end)
        {
            (Vector<double> curStart, Vector<double> curEnd) = GetStartAndEnd();
            if (curStart.SequenceEqual(curEnd))
            {
                // TODO, any problems with resetting these attrs?
                Start = start;
                End = end;
                GeneratePoints();
            }
            return base.PutStartAndEndOn(start, end);
        }

        public Vector<double> GetVector()
        {
            return GetEnd() - GetStart();
        }

        public Vector<double> GetUnitVector()
        {
            return SpaceOps.Normalize(GetVector());
        }

        public double GetAngle()
        {
            return SpaceOps.GetVectorAngle(GetVector());
        }

        public double GetSlope()
        {
            return System.Math.Tan(GetAngle());
        }

        public void SetAngle(double angle)
        {
            Rotate(angle - GetAngle(), aboutPoint: GetStart());
        }

        public void SetLength(double length)
        {
            Scale(length / GetLength());
        }

        public new Line SetOpacity(double opacity, bool family = true)
        {
            // Overwrite default, which would set the fill opacity
            SetStroke(opacity: opacity);
            if (family)
                foreach (VMobject submobj in Submobjects)
                    submobj.SetOpacity(opacity, family);
            return this;
        }
    }

    // DashedLine(Line)

    // TangentLine(Line)

    // Elbow(VMobject)

    // Arrow(Line)

    // Vector(Arrow)

    // DoubleArrow(Arrow)

    // CubicBezier(VMobject)

    public class Polygon : VMobject
    {
        public new Color Color { get; set; } = COLORS[Colors.BLUE_A];

        public Polygon(params Vector<double>[] verticies) : base()
        {
            SetVerticies(verticies);
        }
        public Polygon(string name = null, Color color = null, int dim = 3, Mobject target = null, params Vector<double>[] verticies)
            : base(name, color, dim, target)
        {
            List<Vector<double>> allVerts = new List<Vector<double>>(verticies);
            allVerts.Add(verticies[0]);
            SetPointsAsCorners(allVerts);
        }
    
        public void SetVerticies(params Vector<double>[] verticies)
        {
            List<Vector<double>> allVerts = new List<Vector<double>>(verticies);
            allVerts.Add(verticies[0]);
            SetPointsAsCorners(allVerts);
        }
        public void SetVerticies(List<Vector<double>> verticies)
        {
            List<Vector<double>> allVerts = new List<Vector<double>>(verticies);
            allVerts.Add(verticies[0]);
            SetPointsAsCorners(allVerts);
        }
        public void SetVerticies(IEnumerable<Vector<double>> verticies)
        {
            List<Vector<double>> allVerts = new List<Vector<double>>(verticies);
            allVerts.Add(verticies.First());
            SetPointsAsCorners(allVerts);
        }

        public List<Vector<double>> GetVerticies()
        {
            return GetStartAnchors();
        }

        public Polygon RoundCorners(double radius = 0.5)
        {
            List<Vector<double>> verts = GetVerticies();
            List<ArcBetweenPoints> arcs = new List<ArcBetweenPoints>();
            foreach (List<Vector<double>> vs in Iterables.AdjacentNTuples(verts, 3))
            {
                Vector<double> v1 = vs[0];
                Vector<double> v2 = vs[1];
                Vector<double> v3 = vs[2];

                Vector<double> vect1 = v2 - v1;
                Vector<double> vect2 = v3 - v2;
                Vector<double> unitVect1 = SpaceOps.Normalize(vect1);
                Vector<double> unitVect2 = SpaceOps.Normalize(vect2);
                double angle = SpaceOps.AngleBetweenVectors(vect1, vect2);
                // Negative radius gives concave curves
                angle *= System.Math.Sign(radius);
                // Distance between vertex and start of the arc
                double cutoffLength = radius * System.Math.Tan(angle / 2);
                // Determines counterclockwise vs. clockwise
                int sign = System.Math.Sign(
                    Vector3D.OfVector(vect1).CrossProduct(Vector3D.OfVector(vect2)).Z);
                ArcBetweenPoints arc = new ArcBetweenPoints(
                    v2 - unitVect1 * cutoffLength,
                    v2 + unitVect2 * cutoffLength,
                    angle: sign * angle
                );
                arcs.Add(arc);
            }

            Points.Clear();
            // To ensure that we loop through starting with last
            // The following is equivalent to this: arcs = [arcs[-1], *arcs[:-1]]
            arcs.Insert(0, arcs[^1]);
            arcs.RemoveAt(arcs.Count - 1);

            foreach (Tuple<ArcBetweenPoints, ArcBetweenPoints> pair in Iterables.AdjacentPairs(arcs))
            {
                ArcBetweenPoints arc1 = pair.Item1;
                ArcBetweenPoints arc2 = pair.Item2;

                AppendPoints(arc1.Points);
                Line line = new Line(arc1.GetEnd(), arc2.GetStart());
                // Make sure anchors are evenly distributed
                double lenRatio = line.GetLength() / arc1.GetArcLength();
                line.InsertNCurves((int)(arc1.GetNumCurves() * lenRatio));
                AppendPoints(line.Points);
            }
            return this;
        }
    }

    public class RegularPolygon : Polygon
    {
        public double StartAngle { get; set; }

        public RegularPolygon(double? startAngle = null, int n = 6, string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(name, color, dim, target)
        {
            if (startAngle == null)
            {
                if (n % 2 == 0)
                    StartAngle = 0;
                else
                    StartAngle = 90 * DEGREES;
            }
            Vector<double> startVect = SpaceOps.RotateVector(RIGHT, StartAngle);
            IEnumerable<Vector<double>> vertices = SpaceOps.CompassDirections(startVect, n);
            base.SetVerticies(vertices);
        }
    }

    public class Triangle : RegularPolygon
    {
        public Triangle(string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(null, 3, name, color, dim, target) { }
    }

    public class ArrowTip : Triangle
    {
        public double FillOpacity { get; set; } = 1.0;
        public double StrokeWidth { get; set; } = 0.0;
        public double Length { get; set; }
        public new double StartAngle { get; set; } = System.Math.PI;

        public ArrowTip(double length = TipableVMobject.DEFAULT_ARROW_TIP_LENGTH, VMobjectStyle style = null, string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(name, color, dim, target)
        {
            Length = length;
            SetWidth(Length);
            SetHeight(Length, true);
            Style = style ?? default;
        }
    
        public Vector<double> GetBase()
        {
            return PointFromProportion(0.5);
        }

        public Vector<double> GetTipPoint()
        {
            return Points[0];
        }

        public Vector<double> GetVector()
        {
            return GetTipPoint() - GetBase();
        }

        public double GetAngle()
        {
            return SpaceOps.GetVectorAngle(GetVector());
        }

        public double GetLength()
        {
            return GetVector().L2Norm();
        }
    }

    public class Rectangle : Polygon
    {
        public new Color Color { get; set; } = COLORS[Colors.WHITE];
        public double Height { get; set; } = 2.0;
        public double Width { get; set; } = 4.0;
        public bool MarkPathsClosed { get; set; } = true;
        public new bool CloseNewPoints { get; set; } = true;

        public Rectangle(double width = 4, double height = 2, string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(name, color, dim, target, UL, UR, DR, DL)
        {
            Width = width;
            Height = height;
            SetWidth(Width, true);
            SetHeight(Height, true);
        }
    }

    public class Square : Rectangle
    {
        public double SideLength { get; set; }

        public Square(double sideLength = 2.0, string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(sideLength, sideLength, name, color, dim, target)
        {
            SideLength = sideLength;
        }
    }

    public class RoundedRectangle : Rectangle
    {
        public double CornerRadius { get; set; }

        public RoundedRectangle(double cornerRadius = 2, double width = 4, double height = 2, string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(width, height, name, color, dim, target)
        {
            CornerRadius = cornerRadius;
            RoundCorners(CornerRadius);
        }
    }
}
