using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ManimLib.Rendering;
using MathNet.Numerics.LinearAlgebra;
using static ManimLib.Constants;
using static ManimLib.Common;
using ManimLib.Utils;
using Color = RL.Color;

namespace ManimLib.Mobject
{
    public class Mobject : IList<Mobject>, IManimElement
    {
        #region Properties
        public Color Color { get; set; }
        public string Name { get; set; }

        public int Dimension { get; set; } = 3;
        public Mobject Target { get; set; }
        public List<Mobject> Submobjects { get; set; }
        public List<Mobject> Parents { get; internal set; }
        public List<Mobject> Family { get; internal set; }

        public List<Func<Mobject, double, Mobject>> Updaters { get; internal set; }
        public bool IsUpdatingSuspended { get; set; }

        public List<Vector<double>> Points { get; set; }

        public int Count => Family.Count;

        public bool IsReadOnly => throw new NotImplementedException();

        public Mobject this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        public Mobject(string name = null, Color color = default, int dim = 3, Mobject target = null)
        {
            Submobjects = new List<Mobject>();
            Parents = new List<Mobject>();
            Family = new List<Mobject>() { this };
            Color = color == null ? COLORS[Colors.WHITE] : color;
            if (name == null)
                Name = this.GetType().Name;
            Updaters = new List<Func<Mobject, double, Mobject>>();
            IsUpdatingSuspended = false;

            ResetPoints();
            InitPoints();
            InitColors();
        }

        public void ResetPoints()
        {
            //Points = ArrayUtils.Zeros(typeof(Math.Point), (0, Dimension));
            //Array.Clear(Points, 0, Dimension);
            Points = new List<Vector<double>>();
        }

        public virtual Mobject InitPoints()
        {
            return null;
        }

        public Mobject SetPoints(params Vector<double>[] points)
        {
            Points = points.ToList();
            return this;
        }

        public virtual Mobject InitColors()
        {
            return null;
        }

        #region Updaters
        public Mobject Update(double dt = 0, bool recursive = true)
        {
            if (IsUpdatingSuspended)
                return this;
            foreach (Func<Mobject, double, Mobject> updater in Updaters)
                updater(this, dt);
            if (recursive)
                foreach (Mobject submobj in Submobjects)
                    submobj.Update(dt, recursive);
            return this;
        }

        public Mobject AddUpdater(Func<Mobject, double, Mobject> updater, int index = -1, bool call = true)
        {
            if (index < 0)
                Updaters.Add(updater);
            else
                Updaters.Insert(index, updater);

            if (call)
                Update(0);
            return this;
        }

        public Mobject RemoveUpdater(Func<Mobject, double, Mobject> updater)
        {
            while (Updaters.Contains(updater))
                Updaters.Remove(updater);
            return this;
        }

        public Mobject ClearUpdaters(bool recursive = true)
        {
            Updaters.Clear();
            if (recursive)
                foreach (Mobject submobj in Submobjects)
                    submobj.ClearUpdaters();
            return this;
        }

        public Mobject MatchUpdaters(Mobject mobj)
        {
            ClearUpdaters();
            foreach (Func<Mobject, double, Mobject> updater in mobj.Updaters)
                AddUpdater(updater);
            return this;
        }

        public Mobject SuspendUpdating(bool recursive = true)
        {
            IsUpdatingSuspended = true;
            if (recursive)
                foreach (Mobject submobj in Submobjects)
                    SuspendUpdating(recursive);
            return this;
        }

        public Mobject ResumeUpdating(bool recursive = true)
        {
            IsUpdatingSuspended = true;
            if (recursive)
                foreach (Mobject submobj in Submobjects)
                    ResumeUpdating(recursive);
            return this;
        }
        #endregion

        public Mobject Copy()
        {
            var parents = Parents;
            Parents = new List<Mobject>();
            var copyMobj = (Mobject)MemberwiseClone();
            Parents = parents;

            copyMobj.Points = Points;
            copyMobj.Submobjects = new List<Mobject>();
            foreach (Mobject submobj in Submobjects)
                copyMobj.Add(submobj.Copy());
            copyMobj.MatchUpdaters(this);
            return copyMobj;
        }

        public Mobject Deepcopy()
        {
            var parents = Parents;
            Parents = new List<Mobject>();
            Mobject result = (Mobject)MemberwiseClone();
            result.Color = new Color(Color.A, Color);
            result.Name = String.Copy(Name);
            result.Points = ((Vector<double>[])Points.ToArray().Clone()).ToList();
            result.Submobjects = ((Mobject[])Submobjects.ToArray().Clone()).ToList();
            result.Updaters = ((Func<Mobject, double, Mobject>[])Updaters.ToArray().Clone()).ToList();
            return result;
        }

        public Mobject GenerateTarget(bool useDeepcopy = false)
        {
            Target = null;  // Prevent exponential explosion
            if (useDeepcopy)
                Target = Deepcopy();
            else
                Target = Copy();
            return Target;
        }

        #region Display
        public byte[,] GetImage(Camera camera = null)
        {
            throw new NotImplementedException();
            //camera.Clear();
            //camera.Capture(this);
            //return camera.GetImage();
        }

        public void Show(Camera camera)
        {
            throw new NotImplementedException();
            //GetImage(camera).Show();
        }
        #endregion

        #region Transforming operations
        public Mobject SetPoints(IEnumerable<Vector<double>> points)
        {
            Points = points.ToList();
            return this;
        }

        public Mobject ApplyToFamily(Func<Mobject, Mobject> func)
        {
            foreach (Mobject mobj in GetFamilyMembersWithPoints())
                func(mobj);
            return this;
        }

        public Mobject Shift(params Vector<double>[] vtrs)
        {
            Vector<double> totalVectors = SpaceOps.GetZeroVector(Dimension);
            foreach (Vector<double> v in vtrs)
                totalVectors += v;
            
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] += totalVectors;
            }
            return this;
        }

        public Mobject Scale(double scaleFactor, Vector<double> aboutPoint = null, Vector<double> aboutEdge = null)
        {
            ApplyPointFunctionAboutPoint(
                points => points.Multiply(scaleFactor),
                aboutPoint, aboutEdge
            );
            return this;
        }

        public Mobject RotateAboutOrigin(double angle, Vector<double> axis = null)
        {
            if (axis == null) axis = OUT;
            Rotate(angle, axis, aboutPoint:SpaceOps.GetZeroVector(Dimension));
            return this;
        }

        public Mobject Rotate(double angle, Vector<double> axis = null, Vector<double> aboutPoint = null, Vector<double> aboutEdge = null)
        {
            if (axis == null) axis = OUT;

            Matrix<double> rotationMatrix = SpaceOps.RotationMatrix(angle, axis);
            ApplyPointFunctionAboutPoint(point => {
                return NewVector((point.ToRowMatrix() * rotationMatrix).ToRowMajorArray());
            }, aboutPoint);

            return this;
        }

        public Mobject Flip(Vector<double> axis = null, Vector<double> aboutPoint = null, Vector<double> aboutEdge = null)
        {
            if (axis == null) axis = UP;
            Rotate(System.Math.PI, axis, aboutPoint, aboutEdge);
            return this;
        }

        public Mobject Stretch(double factor, int dimesnion, Vector<double> aboutPoint = null, Vector<double> aboutEdge = null)
        {
            ApplyPointsFunctionAboutPoint(points =>
            {
                List<Vector<double>> subset = points.GetRange(0, dimesnion);
                subset.ForEach(p => p *= factor);
                return subset;
            }, aboutPoint, aboutEdge);
            return this;
        }

        public Mobject ApplyFunction(Func<Vector<double>, Vector<double>> func, Vector<double> aboutPoint = null, Vector<double> aboutEdge = null)
        {
            if (aboutPoint == null) aboutPoint = NewVector(new double[] { 0, 0 });
            ApplyPointFunctionAboutPoint(func, aboutPoint, aboutEdge);
            return this;
        }

        public Mobject ApplyFunctionToPosition(Func<Vector<double>, Vector<double>> func)
        {
            MoveTo(func(GetCenter()));
            return this;
        }

        public Mobject ApplyFunctionToSubmobjectPositions(Func<Vector<double>, Vector<double>> func)
        {
            foreach (Mobject submobj in Submobjects)
                submobj.ApplyFunctionToPosition(func);
            return this;
        }

        public Mobject ApplyMatrix(Matrix<double> matrix, Vector<double> aboutPoint = null, Vector<double> aboutEdge = null)
        {
            if (aboutPoint == null && aboutEdge == null)
                aboutPoint = SpaceOps.GetZeroVector(Dimension);
            Matrix<double> fullMatrix = Matrix<double>.Build.DenseIdentity(Dimension);
            throw new NotImplementedException();

            return this;
        }

        public Mobject ApplyComplexFunction(Func<System.Numerics.Complex, System.Numerics.Complex> func)
        {
            return ApplyFunction(
                v =>
                {
                    System.Numerics.Complex xyComplex = func(new System.Numerics.Complex(v[0], v[1]));
                    return NewVector(new double[] { xyComplex.Real, xyComplex.Imaginary, v[2] });
                }
            );
        }

        public Mobject Wag(Vector<double> direction = null, Vector<double> axis = null, double wagFactor = 1.0)
        {
            foreach (Mobject mobj in GetFamilyMembersWithPoints())
            {
                // TODO: Check if axis needs to be explicitly transposed or not
                List<double> alphas = new List<double>();
                foreach (Vector<double> p in Points)
                    alphas.Add(p.DotProduct(axis));

                alphas.Remove(alphas.Min());
                alphas.ForEach(
                    d => System.Math.Pow(d / alphas.Max(), wagFactor)
                );
                foreach (double d in alphas)
                    mobj.Points.Add(direction.Multiply(wagFactor));
            }
            return this;
        }

        public Mobject ReversePoints()
        {
            foreach (Mobject mobj in GetFamilyMembersWithPoints())
                mobj.Points.Reverse();
            return this;
        }

        public Mobject Repeat(int count)
        {
            foreach (Mobject mobj in GetFamilyMembersWithPoints())
            {
                mobj.Points.Aggregate(
                    SpaceOps.GetZeroVector(Dimension),
                    (acc, x) => acc + x
                );
            }

            return this;
        }

        public Mobject ApplyPointsFunctionAboutPoint(Func<List<Vector<double>>, List<Vector<double>>> func, Vector<double> aboutPoint = null, Vector<double> aboutEdge = null)
        {
            if (aboutPoint == null)
            {
                if (aboutEdge == null)
                    aboutEdge = ORIGIN;
                aboutPoint = GetCriticalPoint(aboutEdge);
            }

            Points.Remove(aboutPoint);
            Points = func(Points);
            Points.Add(aboutPoint);
            return this;
        }
        public Mobject ApplyPointFunctionAboutPoint(Func<Vector<double>, Vector<double>> func, Vector<double> aboutPoint = null, Vector<double> aboutEdge = null)
        {
            if (aboutPoint == null)
            {
                if (aboutEdge == null)
                    aboutEdge = ORIGIN;
                aboutPoint = GetCriticalPoint(aboutEdge);
            }

            Points.Remove(aboutPoint);
            Points.ForEach(v => func(v));
            Points.Add(aboutPoint);
            return this;
        }

        [Obsolete("Use Rotate(double, axis)")]
        public Mobject RotateInPlace(double angle, Vector<double> axis = null)
        {
            return Rotate(angle, axis);
        }

        [Obsolete("Use Scale(scaleFactor, point)")]
        public Mobject ScaleInPlace(double scaleFactor)
        {
            return Scale(scaleFactor, GetCriticalPoint(SpaceOps.GetZeroVector(Dimension)));
        }

        [Obsolete("Use Scale(scaleFactor, point)")]
        public Mobject ScaleAboutPoint(double scaleFactor, Vector<double> point)
        {
            return Scale(scaleFactor, point);
        }
        #endregion

        #region Positioning
        public Mobject Center()
        {
            Shift(-GetCenter());
            return this;
        }

        /// <summary>
        /// Aligns the Mobject with specified edge or corner
        /// </summary>
        /// <param name="direction">A 2D vector pointing towards an edge or corner to align to</param>
        /// <param name="buffer">Margin between edges of the Mobject and the edge/corner</param>
        public Mobject AlignOnBorder(Vector<double> direction, double buffer = DEFAULT_MOBJECT_TO_EDGE_BUFFER)
        {
            Vector<double> targetPoint = direction.PointwiseSign()
                .PointwiseMultiply(NewVector(new double[] { FRAME_X_RADIUS, FRAME_Y_RADIUS, 0 }));
            Vector<double> pointToAlign = GetCriticalPoint(direction);
            Vector<double> shiftVal = targetPoint - pointToAlign - (buffer * direction);
            shiftVal = shiftVal.PointwiseMultiply(Vector<double>.Abs(direction.PointwiseSign()));
            Shift(shiftVal);
            return this;
        }
        
        public Mobject ToCorner(Vector<double> corner = null, double buffer = DEFAULT_MOBJECT_TO_EDGE_BUFFER)
        {
            if (corner == null)
                corner = LEFT + DOWN;
            return AlignOnBorder(corner, buffer);
        }

        public Mobject ToEdge(Vector<double> edge = null, double buffer = DEFAULT_MOBJECT_TO_EDGE_BUFFER)
        {
            if (edge == null)
                edge = LEFT + DOWN;
            return AlignOnBorder(edge, buffer);
        }

        public Mobject NextTo(Mobject mobj, double buffer = DEFAULT_MOBJECT_TO_MOBJECT_BUFFER,
            Mobject submobjectToAlign = null, int indexOfSubmobjectToAlign = -1,
            Vector<double> direction = null, Vector<double> alignedEdge = null,
            Vector<double> coorMask = null)
        {
            Mobject targetAligner;
            if (indexOfSubmobjectToAlign >= 0)
                targetAligner = mobj[indexOfSubmobjectToAlign];
            else
                targetAligner = mobj;
            var targetPoint = targetAligner.GetCriticalPoint(alignedEdge + direction);

            Mobject aligner;
            if (submobjectToAlign != null)
                aligner = submobjectToAlign;
            else if (indexOfSubmobjectToAlign >= 0)
                aligner = this[indexOfSubmobjectToAlign];
            else
                aligner = this;
            Vector<double> pointToAlign = aligner.GetCriticalPoint(alignedEdge - direction);
            Shift(
                (targetPoint - pointToAlign + buffer * direction).PointwiseMultiply(coorMask)
            );

            return this;
        }

        public Mobject ShiftOntoScreen(double buffer = DEFAULT_MOBJECT_TO_MOBJECT_BUFFER)
        {
            var spaceLengths = new double[] { FRAME_X_RADIUS, FRAME_Y_RADIUS };
            foreach (Vector<double> vect in new Vector<double>[] { UP, DOWN, LEFT, RIGHT })
            {
                int dimension = Vector<double>.Abs(vect).MaximumIndex();
                double maxVal = spaceLengths[dimension] - buffer;
                Vector<double> edgeCenter = GetEdgeCenter(vect);
                if (edgeCenter.DotProduct(vect) > maxVal)
                    ToEdge(vect);
            }
            return this;
        }

        public bool IsOffScreen()
        {
            return GetLeft()[0] > FRAME_X_RADIUS ||
                   GetRight()[0] < -FRAME_X_RADIUS ||
                   GetBottom()[1] > FRAME_Y_RADIUS ||
                   GetTop()[1] < -FRAME_Y_RADIUS;
        }

        [Obsolete("Use Stretch(factor, dimension, aboutPoint)")]
        public Mobject StretchAboutPoint(double factor, int dimension, Vector<double> point)
        {
            return Stretch(factor, dimension, point);
        }

        [Obsolete("Use Stretch(factor, dimension)")]
        public Mobject StretchInPlace(double factor, int dimension)
        {
            return Stretch(factor, dimension);
        }

        public Mobject RescaleToFit(double length, int dimension, bool stretch = false)
        {
            double oldLength = LengthOverDimension(dimension);
            if (oldLength == 0)
                return this;
            if (stretch)
                Stretch(length / oldLength, dimension);
            else
                Scale(length / oldLength);
            return this;
        }

        public Mobject StretchToFitWidth(double width)
        {
            return RescaleToFit(width, 0, true);
        }

        public Mobject StretchToFitHeight(double height)
        {
            return RescaleToFit(height, 1, true);
        }

        public Mobject StretchToFitDepth(double depth)
        {
            return RescaleToFit(depth, 2, true);
        }

        public Mobject SetWidth(double width, bool stretch = false)
        {
            return RescaleToFit(width, 0, stretch);
        }

        public Mobject SetHeight(double height, bool stretch = false)
        {
            return RescaleToFit(height, 1, stretch);
        }

        public Mobject SetDepth(double depth, bool stretch = false)
        {
            return RescaleToFit(depth, 2, stretch);
        }

        public Mobject SetCoord(double value, int dimension, Vector<double> direction = null)
        {
            if (direction == null)
                direction = ORIGIN;
            double current = GetCoord(dimension, direction);
            Vector<double> shiftVector = SpaceOps.GetZeroVector(Dimension);
            shiftVector[dimension] = value - current;
            Shift(shiftVector);
            return this;
        }

        public Mobject SetX(double x, Vector<double> direction = null)
        {
            return SetCoord(x, 0, direction);
        }

        public Mobject SetY(double y, Vector<double> direction = null)
        {
            return SetCoord(y, 1, direction);
        }

        public Mobject SetZ(double z, Vector<double> direction = null)
        {
            return SetCoord(z, 2, direction);
        }

        public Mobject SpaceOutMobjects(double factor = 1.5)
        {
            Scale(factor);
            foreach (Mobject submobj in Submobjects)
                submobj.Scale(1 / factor);
            return this;
        }

        public Mobject MoveTo(Mobject mobj, Vector<double> alignedEdge = null, Vector<double> coorMask = null)
        {
            return MoveTo(mobj.GetCriticalPoint(alignedEdge), alignedEdge, coorMask);
        }
        public Mobject MoveTo(Vector<double> point, Vector<double> alignedEdge = null, Vector<double> coorMask = null)
        {
            if (alignedEdge == null)
                alignedEdge = ORIGIN;
            if (coorMask == null)
                coorMask = ONE_VECTOR;

            Vector<double> pointToAlign = GetCriticalPoint(alignedEdge);
            Shift((point - pointToAlign).PointwiseMultiply(coorMask));
            return this;
        }
        
        public Mobject Replace(Mobject mobj, int dimensionToMatch = 0, bool stretch = false)
        {
            if (mobj.Points.Count <= 0 && mobj.Submobjects.Count <= 0)
                throw new ArgumentException("Attempting to replace mobject with no points");

            if (stretch)
            {
                StretchToFitWidth(mobj.GetWidth());
                StretchToFitHeight(mobj.GetHeight());
            }
            else
            {
                RescaleToFit(mobj.LengthOverDimension(dimensionToMatch), dimensionToMatch);
            }
            Shift(mobj.GetCenter() - GetCenter());
            return this;
        }

        public Mobject Surround(Mobject mobj, int dimensionToMatch = 0, bool stretch = false, double buffer = MED_SMALL_BUFF)
        {
            Replace(mobj, dimensionToMatch, stretch);
            double length = mobj.LengthOverDimension(dimensionToMatch);
            ScaleInPlace((length + buffer) / length);
            return this;
        }

        public Mobject PutStartAndEndOn(Vector<double> start, Vector<double> end)
        {
            (Vector<double> start, Vector<double> end) current = GetStartAndEnd();
            Vector<double> currStart = current.start;
            Vector<double> currEnd = current.end;
            Vector<double> currVect = currEnd - currStart;
            if (currVect.All(d => d == 0))
                throw new Exception("Cannot position endpoints of closed loop");
            Vector<double> targetVect = end - start;
            Scale(
                targetVect.L2Norm() / currVect.L2Norm(),
                aboutPoint:currStart
            );
            Rotate(
                SpaceOps.GetVectorAngle(targetVect) -
                SpaceOps.GetVectorAngle(currVect),
                aboutPoint:currStart
            );
            Shift(start - currStart);
            return this;
        }

        public Mobject AddBackgroundRectangle(Color color = null, double opacity = 0.75)
        {
            // TODO: This does not behave well when the mobject has points,
            // since it gets displayed on top
            //from manimlib.mobject.shape_matchers import BackgroundRectangle
            //BackgroundRectangle = new BackgroundRectangle(color, opacity);
            //AddToBack(BackgroundRectangle);
            throw new NotImplementedException();
            return this;
        }

        public Mobject AddBackgroundRectangleToSubmobjects(Color color = null, double opacity = 0.75)
        {
            foreach (Mobject submobj in Submobjects)
                submobj.AddBackgroundRectangle(color, opacity);
            return this;
        }
        
        public Mobject AddBackgroundRectangleToFamilyMembersWithPoints(Color color = null, double opacity = 0.75)
        {
            foreach (Mobject mobj in GetFamilyMembersWithPoints())
                mobj.AddBackgroundRectangle(color, opacity);
            return this;
        }

        #region Color
        public Mobject SetColor(Color color = null, bool applyToSubmobj = true)
        {
            if (color == null)
                color = COLORS[Colors.YELLOW_C];

            foreach (Mobject submobj in Submobjects)
                submobj.SetColor(color);
            Color = color;
            return this;
        }
        public Mobject SetColor(Colors color = Colors.YELLOW_C, bool applyToSubmobj = true)
        {
            return SetColor(COLORS[color], applyToSubmobj);
        }

        public Mobject SetColorsByGradient(params Color[] colors)
        {
            SetSubmobjectColorsByGradient(colors);
            return this;
        }

        public Mobject SetColorsByRadialGradient(Vector<double> center = null, double radius = 1, Color innerColor = null, Color outerColor = null)
        {
            SetSubmobjectColorsByRadialGradient(center, radius, innerColor, outerColor);
            return this;
        }

        public Mobject SetSubmobjectColorsByGradient(params Color[] colors)
        {
            if (colors.Length <= 0)
                throw new ArgumentException("colors must contain at least one color");
            else if (colors.Length == 1)
                SetColor(colors[0]);

            List<Mobject> mobjs = GetFamilyMembersWithPoints();
            var newColors = Utils.Color.ColorGradient(mobjs.Count, colors);
            foreach ((Mobject mobj, Color col) pair in mobjs.Zip(newColors, (m, c) => (m, c))) {
                pair.mobj.SetColor(pair.col, false);
            }
            return this;
        }

        public Mobject SetSubmobjectColorsByRadialGradient(Vector<double> center = null, double radius = 1, Color innerColor = null, Color outerColor = null)
        {
            if (center == null)
                center = GetCenter();

            foreach (Mobject mobj in GetFamilyMembersWithPoints())
            {
                double t = (mobj.GetCenter() - center).L2Norm() / radius;
                t = t < 1 ? t : 1; // Effectively clips t at 1
                Color mobjColor = innerColor.Interpolate(outerColor, t);
                mobj.SetColor(mobjColor, false);
            }

            return this;
        }

        public Mobject ToOriginalColor()
        {
            SetColor(Color);
            return this;
        }

        public Mobject FadeTo(Color color, double alpha, bool applyToSubmobjects = true)
        {
            if (Points.Count > 0)
            {
                Color newColor = GetColor().Interpolate(color, alpha);
                SetColor(newColor, false);
            }
            if (applyToSubmobjects)
            {
                foreach (Mobject submobj in Submobjects)
                    submobj.FadeTo(color, alpha);
            }
            return this;
        }

        public Mobject Fade(double darkness = 0.5, bool applyToSubmobjects = true)
        {
            if (applyToSubmobjects)
                foreach (Mobject submobj in Submobjects)
                    submobj.Fade(darkness, applyToSubmobjects);
            return this;
        }
        #endregion
        #endregion

        /// <summary>
        /// This function needs to be reviewed by someone else.
        /// </summary>
        public double ReduceAcrossDimension(Func<List<double>, double> pointsFunc, Func<double, double> reduceFunc, int dimension)
        {
            if (Points == null || Points.Count == 0)
                // Note, this default means things like empty VGroups
                // will appear to have a center at [0, 0, 0]
                return 0;
            double values = pointsFunc(Points.GetColumn(dimension).ToList());
            return reduceFunc(values);
        }

        public IEnumerable<Mobject> GetNonemptySubmobjects()
        {
            foreach (Mobject submobj in Submobjects)
                if (submobj.Submobjects.Count > 0 || submobj.Points.Count > 0)
                    yield return submobj;
        }

        #region Getters
        public List<Vector<double>> GetAllPoints()
        {
            IEnumerable<Vector<double>> allPoints = Points;
            foreach (Mobject mobj in Family)
            {
                allPoints = allPoints.Concat(mobj.Points);
            }
            return allPoints.ToList();
        }

        public Color GetColor()
        {
            return Color;
        }

        public double GetExtremumAlongDimension(IList<Vector<double>> points = null, int dimension = 0, double key = 0)
        {
            if (points == null)
                points = GetAllPoints();

            IEnumerable<double> values = points.GetColumn(dimension);

            if (key < 0)
                return values.Min();
            else if (key == 0)
                return (values.Min() + values.Max()) / 2;
            else
                return values.Max();
        }

        /// <summary>
        /// Picture a box bounding the mobject. Such a box has 
        /// 9 'critical points': 4 corners, 4 edge center, and the
        /// center. This returns one of them.
        /// </summary>
        public Vector<double> GetCriticalPoint(Vector<double> direction)
        {
            Vector<double> point = SpaceOps.GetZeroVector(Dimension);
            var allPoints = GetAllPoints();
            if (allPoints.Count == 0)
                return point;
            for (int d = 0; d < Dimension; d++)
            {
                point[d] = GetExtremumAlongDimension(allPoints, d, direction[d]);
            }
            return point;
        }

        public Vector<double> GetCenter()
        {
            return GetCriticalPoint(SpaceOps.GetZeroVector(Dimension));
        }
        public Vector<double> GetCorner(Vector<double> direction)
        {
            return GetCriticalPoint(direction);
        }
        public Vector<double> GetEdgeCenter(Vector<double> direction)
        {
            return GetCriticalPoint(direction);
        }

        public Vector<double> GetCenterOfMass()
        {
            // Actual Python code:
            // return np.apply_along_axis(np.mean, 0, self.get_all_points())
            // It looks like all this does is take the average of all vectors
            return SpaceOps.CenterOfMass(Points.ToArray());
        }

        public Vector<double> GetBoundaryPoint(Vector<double> direction)
        {
            List<double> products = new List<double>();
            foreach (Vector<double> point in Points)
            {
                products.Add(point.DotProduct(direction)); 
            }
            int index = products.IndexOf(products.Max());
            return Points[index];
        }

        public Vector<double> GetTop()
        {
            return GetEdgeCenter(UP);
        }

        public Vector<double> GetBottom()
        {
            return GetEdgeCenter(DOWN);
        }

        public Vector<double> GetLeft()
        {
            return GetEdgeCenter(LEFT);
        }

        public Vector<double> GetRight()
        {
            return GetEdgeCenter(RIGHT);
        }

        public Vector<double> GetZenith()
        {
            return GetEdgeCenter(OUT);
        }

        public Vector<double> GetNadir()
        {
            return GetEdgeCenter(IN);
        }

        public double LengthOverDimension(int dimension)
        {
            return ReduceAcrossDimension(points => points.Max(), d => d, dimension)
                - ReduceAcrossDimension(points => points.Min(), d => d, dimension);
        }

        public double GetWidth()
        {
            return LengthOverDimension(0);
        }

        public double GetHeight()
        {
            return LengthOverDimension(1);
        }

        public double GetDepth()
        {
            return LengthOverDimension(2);
        }

        public double GetCoord(int dimension, Vector<double> direction = null)
        {
            if (direction == null)
                direction = ORIGIN;
            return GetExtremumAlongDimension(
                dimension: dimension,
                key: direction[dimension]
            );
        }

        public double GetX(Vector<double> direction = null)
        {
            if (direction == null)
                direction = ORIGIN;
            return GetCoord(0, direction);
        }

        public double GetY(Vector<double> direction = null)
        {
            if (direction == null)
                direction = ORIGIN;
            return GetCoord(1, direction);
        }

        public double GetZ(Vector<double> direction = null)
        {
            if (direction == null)
                direction = ORIGIN;
            return GetCoord(2, direction);
        }

        public Vector<double> GetStart()
        {
            if (Points.Count <= 0)
                throw new Exception("Mobject contains no points");
            return Points[0];
        }

        public Vector<double> GetEnd()
        {
            if (Points.Count <= 0)
                throw new Exception("Mobject contains no points");
            return Points[^1];
        }

        public (Vector<double> start, Vector<double> end) GetStartAndEnd()
        {
            return (GetStart(), GetEnd());
        }

        /// <summary>
        /// Do not call this function directly; call something like <see cref="Types.VMobject.PointFromProportion(double)"/>
        /// </summary>
        public virtual Vector<double> PointFromProportion(double alpha)
        {
            throw new NotImplementedException();
        }

        public Group GetPieces(int nPieces)
        {
            Mobject template = Copy();
            template.Submobjects = new List<Mobject>();
            List<int> alphas = Iterables.LinSpace(0, 1, nPieces + 1).ToList();
            List<Mobject> mobjects = new List<Mobject>();
            for (int i = 0; i < alphas.Count() - 1; i++)
            {
                mobjects.Add(template.Copy().PointwiseBecomePartial(
                    this, alphas[i], alphas[i + 1]
                ));
            }
            return new Group(mobjects);
        }

        public Vector<double> GetZIndexReferencePoint()
        {
            // Actual Python code:
            // z_index_group = getattr(self, "z_index_group", self)
            // return z_index_group.get_center()
            // Problem: z_index_group isn't defined anywhere
            return ORIGIN;
        }
        #endregion

        #region Match
        public Mobject MatchColor(Mobject mobj)
        {
            return SetColor(mobj.GetColor());
        }

        public Mobject MatchDimensionSize(Mobject mobj, int dimension, bool stretch = false)
        {
            return RescaleToFit(mobj.LengthOverDimension(dimension), dimension, stretch);
        }

        public Mobject MatchWidth(Mobject mobj, bool stretch = false)
        {
            return MatchDimensionSize(mobj, 0, stretch);
        }

        public Mobject MatchHeight(Mobject mobj, bool stretch = false)
        {
            return MatchDimensionSize(mobj, 1, stretch);
        }

        public Mobject MatchDepth(Mobject mobj, bool stretch = false)
        {
            return MatchDimensionSize(mobj, 2, stretch);
        }

        public Mobject MatchCoord(Mobject mobj, int dimension, Vector<double> direction = null)
        {
            return SetCoord(
                mobj.GetCoord(dimension, direction),
                dimension, direction
            );
        }

        public Mobject MatchX(Mobject mobj, Vector<double> direction = null)
        {
            return MatchCoord(mobj, 0, direction);
        }

        public Mobject MatchY(Mobject mobj, Vector<double> direction = null)
        {
            return MatchCoord(mobj, 1, direction);
        }

        public Mobject MatchZ(Mobject mobj, Vector<double> direction = null)
        {
            return MatchCoord(mobj, 2, direction);
        }

        public Mobject AlignTo(Vector<double> point, Vector<double> direction = null)
        {
            if (direction == null)
                direction = ORIGIN;
            for (int d = 0; d <= Dimension; d++)
            {
                if (direction[d] != 0)
                    SetCoord(point[d], d, direction);
            }
            return this;
        }
        public Mobject AlignTo(Mobject mobj, Vector<double> direction = null)
        {
            return AlignTo(mobj.GetCriticalPoint(direction), direction);
        }
        #endregion

        #region Family matters
        public Mobject AssembleFamily()
        {
            List<Mobject> subFamilies = new List<Mobject>();
            foreach (Mobject mobj in Submobjects)
            {
                subFamilies = subFamilies.Concat(mobj.Family).ToList();
            }
            Family = new List<Mobject>() { this }.Concat(subFamilies).ToList();
            foreach (Mobject parent in Parents)
            {
                parent.AssembleFamily();
            }
            return this;
        }

        public List<Mobject> GetFamilyMembersWithPoints()
        {
            var output = new List<Mobject>();
            foreach (Mobject mobj in Family)
                if (mobj.Points.Count > 0)
                    output.Add(mobj);
            return output;
        }

        public Mobject Arrange(Vector<double> direction = null, bool center = true,
            double buffer = DEFAULT_MOBJECT_TO_MOBJECT_BUFFER,
            Mobject submobjectToAlign = null, int indexOfSubmobjectToAlign = -1,
            Vector<double> alignedEdge = null, Vector<double> coorMask = null)
        {
            if (direction == null)
                direction = RIGHT;
            for (int i = 0; i < Submobjects.Count - 1; i++)
            {
                Mobject m1 = Submobjects[i];
                Mobject m2 = Submobjects[i + 1];
                m2.NextTo(m1, buffer, submobjectToAlign, indexOfSubmobjectToAlign, direction,
                    alignedEdge, coorMask);
            }
            if (center)
                Center();
            return this;
        }

        public Mobject ArrangeInGrid(int nRows = -1, int nCols = -1)
        {
            if (nRows <= 0 && nCols <= 0)
                nCols = (int)System.Math.Sqrt(Submobjects.Count);

            Vector<double> v1 = ORIGIN, v2 = ORIGIN;
            int n = 0;

            if (nRows > 0)
            {
                v1 = RIGHT;
                v2 = DOWN;
                n = Submobjects.Count / nRows;
            }
            else if (nCols > 0)
            {
                v1 = DOWN;
                v2 = RIGHT;
                n = Submobjects.Count / nCols;
            }
            //throw new NotImplementedException("See the following comments in the source code.");
            // Actual Python code:
            // Group(*[
            //     Group(*submobs[i: i + n]).arrange(v1, **kwargs)
            //     for i in range(0, len(submobs), n)
            // ]).arrange(v2, **kwargs)
            List<Group> groups = new List<Group>();
            for (int i = 0; i < Submobjects.Count; i += n)
            {
                groups.Concat(new Group(
                    Submobjects.ToArray()[i..(i+n)][0].Arrange(v1)
                ));
            }
            new Group(groups).Arrange(v2);
            return this;
        }

        public Mobject Sort(Func<Vector<double>, double> pointToNumFunc = null, Func<Mobject, double> submobjFunc = null)
        {
            if (submobjFunc == null)
                submobjFunc = m => pointToNumFunc(m.GetCenter());
            Submobjects.Sort((m1, m2) =>
            {
                return submobjFunc(m1).CompareTo(submobjFunc(m2));
            });
            return this;
        }

        public void Shuffle(bool recursive = false)
        {
            if (recursive)
                foreach (Mobject submobj in Submobjects)
                    submobj.Shuffle(true);
            var rand = new Random();
            Submobjects.OrderBy(x => rand.Next()).ToList();
        }

        public Mobject Add(params Mobject[] submobjects)
        {
            if (submobjects.Contains(this))
                throw new ArgumentException("Mobject cannot contain self.");
            foreach (Mobject mobj in submobjects)
            {
                if (!Submobjects.Contains(mobj))
                    Submobjects.Add(mobj);
                if (!mobj.Parents.Contains(this))
                    mobj.Parents.Add(this);
            }
            return this;
        }
        public Mobject AddToBack(params Mobject[] submobjects)
        {
            if (submobjects.Contains(this))
                throw new ArgumentException("Mobject cannot contain self.");
            foreach (Mobject mobj in submobjects)
            {
                Submobjects.Remove(mobj);
                Submobjects.Add(mobj);
            }
            return this;
        }

        public Mobject Remove(params Mobject[] submobjects)
        {
            foreach (Mobject mobj in submobjects)
            {
                if (Submobjects.Contains(mobj))
                    Submobjects.Remove(mobj);
                if (mobj.Parents.Contains(mobj))
                    mobj.Parents.Remove(this);
            }
            return this;
        }

        public Mobject Replace(int index, Mobject newSubmob)
        {
            Mobject oldSubmob = Submobjects[index];
            if (oldSubmob.Parents.Contains(this))
                oldSubmob.Parents.Remove(this);
            Submobjects[index] = newSubmob;
            return this;
        }

        public Mobject[] Split()
        {
            Mobject[] result = Points.Count > 0 ? new Mobject[] { this } : new Mobject[] { };
            return result.Concat(Submobjects).ToArray();
        }

        public Mobject SetSubmobjects(IEnumerable<Mobject> submobjects)
        {
            Submobjects = submobjects.ToList();
            return this;
        }

        public int IndexOf(Mobject item)
        {
            return Split().ToList().IndexOf(item);
        }

        public void Insert(int index, Mobject item)
        {
            Submobjects.Insert(index - 1, item);
        }

        public void RemoveAt(int index)
        {
            Remove(Split()[index - 1]);
        }

        public void Add(Mobject item)
        {
            Add(item);
        }

        public void Clear()
        {
            Submobjects.Clear();
        }

        public bool Contains(Mobject item)
        {
            return Submobjects.Contains(item) || item == this;
        }

        public void CopyTo(Mobject[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Mobject item)
        {
            var old = this;
            return Remove(new Mobject[] { item }) != old;
        }

        public IEnumerator<Mobject> GetEnumerator()
        {
            return Split().ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Split().GetEnumerator();
        }
        #endregion

        #region Alignment
        public void AlignData(Mobject mobj)
        {
            NullPointAlign(mobj);
            AlignSubmobjects(mobj);
            AlignPoints(mobj);
            foreach ((Mobject m1, Mobject m2) in Submobjects.Zip(mobj.Submobjects, (s1, s2) => (s1,s1)))
            {
                m1.AlignData(m2);
            }
        }

        public virtual Mobject GetPointMobject(Vector<double> center = null)
        {
            throw new NotImplementedException($"get_point_mobject not implemented for {GetType().Name}");
        }

        public Mobject AlignPoints(Mobject mobj)
        {
            int count1 = Points.Count;
            int count2 = mobj.Points.Count;
            if (count1 < count2)
                AlignPointsWithLarger(mobj);
            else
                mobj.AlignPointsWithLarger(this);
            return this;
        }

        public void AlignPointsWithLarger(Mobject largerMobj)
        {
            throw new NotImplementedException("This function has not yet been implemented in Manim");
        }

        public Mobject AlignSubmobjects(Mobject mobj)
        {
            Mobject mobj1 = this;
            Mobject mobj2 = mobj;
            int n1 = mobj1.Submobjects.Count;
            int n2 = mobj2.Submobjects.Count;
            mobj1.AddNMoreSubmobjects(System.Math.Max(0, n2 - n1));
            mobj2.AddNMoreSubmobjects(System.Math.Max(0, n1 - n2));
            return this;
        }

        /// <summary>
        /// If an mobject with points is being aligned to
        /// one without, treat both as groups, and push
        /// the one with points into its own submobjects
        /// list.
        /// </summary>
        public Mobject NullPointAlign(Mobject mobj)
        {
            foreach ((Mobject m1, Mobject m2) in this.Zip(mobj, (p1, p2) => (p1, p2))) {
                if (m1.Points.Count <= 0 && m2.Points.Count > 0)
                    m2.PushSelfIntoSubmobjects();
            }
            foreach ((Mobject m1, Mobject m2) in mobj.Zip(this, (p1, p2) => (p1, p2)))
            {
                if (m1.Points.Count <= 0 && m2.Points.Count > 0)
                    m2.PushSelfIntoSubmobjects();
            }
            return this;
        }

        public Mobject PushSelfIntoSubmobjects()
        {
            Mobject copy = Copy();
            copy.Submobjects = new List<Mobject>();
            ResetPoints();
            Add(copy);
            return this;
        }

        public Mobject AddNMoreSubmobjects(int n)
        {
            if (n == 0)
                return null;

            int curr = Submobjects.Count;
            if (curr == 0)
            {
                for (int k = 0; k <= n; k++) {
                    GetPointMobject();
                }
                return null;
            }

            int target = curr + n;
            // TODO: factor this out to utils so as to reuse
            // with VMobject.insert_n_curves
            List<int> repeatIndicies = new List<int>();
            for (int i = 0; i < target; i++)
                repeatIndicies.Add((i * curr) / target);
            List<int> splitFactors = new List<int>();
            for (int i = 0; i < curr; i++)
                splitFactors.Add(repeatIndicies[i] == i ? 1 : 0);
            List<Mobject> newSubmobjs = new List<Mobject>();
            foreach ((Mobject submobj, int sf) in Submobjects.Zip(splitFactors, (a,b) => (a,b))) {
                newSubmobjs.Add(submobj);
                for (int k = 0; k < sf; k++)
                    newSubmobjs.Add(submobj.Copy().Fade(1));
            }
            Submobjects = newSubmobjs;
            return this;
        }

        public Mobject RepeatSubmobject(Mobject submobj)
        {
            return submobj.Copy();
        }

        /// <summary>
        /// Turns self into an interpolation between mobj1 and mobj2.
        /// </summary>
        public Mobject Interpolate(Mobject mobj1, Mobject mobj2, double alpha,
            Func<IEnumerable<Vector<double>>, IEnumerable<Vector<double>>, double, IEnumerable<Vector<double>>> pathFunc = null)
        {
            if (pathFunc == null)
                pathFunc = Paths.StraightPath;
            Points = pathFunc(mobj1.Points, mobj2.Points, alpha).ToList();
            InterpolateColor(mobj1, mobj2, alpha);
            return this;
        }

        public Mobject InterpolateColor(Mobject mobj1, Mobject mobj2, double alpha)
        {
            // To implement in subclass
            return this;
        }

        /// <summary>
        /// Set points in such a way as to become only
        /// part of mobject.
        /// Inputs 0 <= a<b <= 1 determine what portion
        /// of mobject to become.
        /// </summary>
        public virtual Mobject BecomePartial(Mobject mobj, int a, int b)
        {
            // To implement in subclass
            return this;
        }

        public virtual Mobject PointwiseBecomePartial(Mobject mobj, int a, int b)
        {
            // To implement in subclass
            return this;
        }

        /// <summary>
        /// Edit points, colors and submobjects to be idential to another mobject
        /// </summary>
        public Mobject Become(Mobject mobj, bool copySubmobjs = true)
        {
            AlignData(mobj);
            foreach ((Mobject submobj1, Mobject submobj2) in Family.Zip(mobj.Family, (c, m) => (c, m)))
            {
                submobj1.Points = submobj2.Points;
                submobj1.InterpolateColor(submobj1, submobj2, 1);
            }
            return this;
        }
        #endregion

        public string GetManimType()
        {
            return "Mobject";
        }
    }

    /// <summary>
    /// An interface to be implemented by any objects that can be placed into a Scene
    /// </summary>
    public interface IManimElement
    {
        string GetManimType();
    }

    public class Group : Mobject, ICollection<Mobject>
    {
        private IList<Mobject> Mobjects;

        public Group(params Mobject[] mobjects)
        {
            Mobjects = mobjects.ToList();
        }
        public Group(IList<Mobject> mobjects)
        {
            Mobjects = mobjects;
        }
        public Group(IEnumerable<Mobject> mobjects)
        {
            Mobjects = mobjects.ToList();
        }

        public new int Count => Mobjects.Count;

        public new bool IsReadOnly => false;

        public new void Add(Mobject item)
        {
            Mobjects.Add(item);
        }

        public new void Clear()
        {
            Mobjects.Clear();
        }

        public new bool Contains(Mobject item)
        {
            return Mobjects.Contains(item);
        }

        public new void CopyTo(Mobject[] array, int arrayIndex)
        {
            Mobjects.CopyTo(array, arrayIndex);
        }

        public new IEnumerator<Mobject> GetEnumerator()
        {
            return Mobjects.GetEnumerator();
        }

        public new bool Remove(Mobject item)
        {
            return Mobjects.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Mobjects.GetEnumerator();
        }
    }

    public class Point : Mobject
    {
        public Vector<double> Location;
        public double ArtificialWidth, ArtificialHeight = 1e-6;

        public Point(params int[] components)
        {
            Location = NewVector(components.Cast<double>().ToArray());
        }
        public Point(params double[] components)
        {
            Location = NewVector(components);
        }
        public Point(params float[] components)
        {
            Location = NewVector(components.Cast<double>().ToArray());
        }
        public Point(params decimal[] components)
        {
            Location = NewVector(components.Cast<double>().ToArray());
        }
        public Point(Vector<double> v)
        {
            Location = v.Clone();
        }

        public double GetWidth()
        {
            return ArtificialWidth;
        }

        public double GetHeight()
        {
            return ArtificialWidth;
        }
    }
}
