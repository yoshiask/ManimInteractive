using ManimLib.Mobject.Types;
using Color = RL.Color;
using System;
using System.Collections.Generic;
using System.Text;
using static ManimLib.Constants;
using System.IO;
using System.Security.Cryptography;
using Svg;
using MathNet.Numerics.LinearAlgebra;
using ManimLib.Utils;
using Svg.Transforms;
using System.Linq;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;

namespace ManimLib.Mobject.Svg
{
    public class SvgMobject : VMobject
    {
        //region Properties
        public bool ShouldCenter { get; set; } = true;
        public double Height { get; set; } = 2.0;
        public double Width { get; set; } = 0;
        // Must be filled in a subclass, or when called
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public bool UnpackGroups { get; set; } = true; // If false, creates a hierarchy of VGroups
        public double StrokeWidth { get; set; } = DEFAULT_STROKE_WIDTH;
        public double FillOpacity { get; set; } = 1.0;
        //public Color FillColor { get; set; } = COLORS[Colors.LIGHT_GREY];

        public Dictionary<string, SvgElement> RefToElement { get; set; }
        //endregion

        public SvgMobject(string fileName = null, string name = null, Color color = null, int dim = 3, Mobject target = null) : base(name, color, dim, target)
        {
            FileName = fileName;
            EnsureValidFile();
            MoveIntoPosition();
        }

        public void EnsureValidFile()
        {
            if (String.IsNullOrWhiteSpace(FileName))
                throw new ArgumentNullException("fileName", "Must specify file for SVGMobject");

            string[] possiblePaths = new string[]
            {
                Path.Combine("assets", "svg_images", FileName),
                Path.Combine("assets", "svg_images", FileName + ".svg"),
                Path.Combine("assets", "svg_images", FileName + ".xdv"),
                FileName
            };
            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    FilePath = path;
                    return;
                }
            }
            throw new IOException($"No file matching {FileName} in image directory");
        }

        public void GeneratePoints()
        {
            var doc = SvgDocument.Open(FilePath);
            var mobjects = GetMobjectsFrom(doc);
            if (UnpackGroups)
                Add(mobjects);
            else
                Add(mobjects[0].Submobjects);
        }

        // TODO: Test this
        public List<Mobject> GetMobjectsFrom(SvgDocument doc)
        {
            // Investigate using SvgVisualElement and use a single foreach loop
            // TODO: Is it faster to loop through the children once and use
            // conditionals?
            List<Mobject> mobjects = new List<Mobject>();
            foreach (SvgCircle circle in doc.Children.FindSvgElementsOf<SvgCircle>())
            {
                // Handle cirlce
            }
            foreach (SvgEllipse ellipse in doc.Children.FindSvgElementsOf<SvgEllipse>())
            {
                // Handle ellipse
            }
            foreach (SvgRectangle rectangle in doc.Children.FindSvgElementsOf<SvgRectangle>())
            {
                // Handle rectangle
            }
            foreach (SvgPolygon polygon in doc.Children.FindSvgElementsOf<SvgPolygon>())
            {
                // Handle polygon & polyline
                VMobject mobj = new VMobject(polygon.ID, new Color(polygon.Fill.ToString()), dim: 2);
                mobj.Points.Clear();
                for (int i = 0; i+1 < polygon.Points.Count; i+=2)
                {
                    mobj.Points.Add(Vector<double>.Build.DenseOfArray(new double[] {
                        polygon.Points[i], polygon.Points[i + 1]
                    }));
                }
                mobjects.Add(mobj);
            }
            foreach (SvgPath path in doc.Children.FindSvgElementsOf<SvgPath>())
            {
                // TODO: Recognize circles, rectangles, and other primitives that
                // have a Manim equivalent.
                VMobject mobj = new VMobject(path.ID, new Color(path.Fill.ToString()), dim: 2);
                mobj.Points.Clear();
                mobj.Points.Add(path.PathData[0].Start.ToVector());
                foreach (var data in path.PathData)
                {
                    var vPoint = data.End.ToVector();
                    mobj.Points.Add(vPoint);
                }
                mobjects.Add(mobj);
            }
            foreach (SvgUse reference in doc.Children.FindSvgElementsOf<SvgUse>())
            {
                // Handle links to other objects
            }
            return mobjects;
        }

        public void HandleTransforms(SvgElement element, VMobject vmobj)
        {
            double x, y;
            element.TryGetAttribute("x", out string xVal);
            element.TryGetAttribute("y", out string yVal);
            Double.TryParse(xVal, out x);
            Double.TryParse(yVal, out y);

            foreach (SvgTransform transform in element.Transforms)
            {
                if (transform as SvgMatrix != null)
                {
                    var matrixTransform = transform as SvgMatrix;
                    var matrix = Matrix<double>.Build.DenseOfArray(matrixTransform.Points.Reshape((3, 2)).CastToDoubleArray());
                    for (int i = 0; i < vmobj.Points.Count; i++)
                    {
                        // matrix[:2, :2] = transform[:2, :]
                        // matrix[1] *= -1
                        // matrix[:, 1] *= -1

                        vmobj.Points[i] *= matrix;
                    }
                    x = matrix[2, 0];
                    y = matrix[2, 1];
                    vmobj.Shift(x * RIGHT + (-y) * UP);
                }
                else if (transform as SvgTranslate != null)
                {
                    var translateTransform = transform as SvgTranslate;
                    // Shouldn't these be added to the current X and Y values, not overwrite?
                    x = translateTransform.X;
                    y = translateTransform.Y;
                    vmobj.Shift(x * RIGHT + (-y) * UP);
                }
                else if (transform as SvgScale != null)
                {
                    var scaleTransform = transform as SvgScale;
                    vmobj.Scale(new double[] { scaleTransform.X, scaleTransform.Y });
                }
                else if (transform as SvgRotate != null)
                {
                    var rotateTransform = transform as SvgRotate;
                    Vector<double> aboutPoint = Vector<double>.Build.DenseOfArray(
                        new double[] { rotateTransform.CenterX, rotateTransform.CenterY }    
                    );
                    vmobj.Rotate(Common.DegreesToRadians(rotateTransform.Angle), aboutPoint);
                }
                else if (transform as SvgSkew != null)
                {
                    var skewTransform = transform as SvgSkew;
                    double mX = 1 / System.Math.Tan(Common.DegreesToRadians(skewTransform.AngleX));
                    double mY = 1 / System.Math.Tan(Common.DegreesToRadians(skewTransform.AngleY));
                    for (int i = 0; i < vmobj.Points.Count; i++)
                    {
                        // See https://en.wikipedia.org/wiki/Shear_mapping//Definition
                        Matrix<double> skewX = Matrix<double>.Build.DenseOfArray(new double[,] {
                               { 1, mX },
                               { 0, 1 },
                        });
                        Matrix<double> skewY = Matrix<double>.Build.DenseOfArray(new double[,] {
                               { 1, 0 },
                               { mY, 1 },
                        });
                        Vector<double> newPoint = skewX * vmobj.Points[i];
                        newPoint = skewY * newPoint;
                        vmobj.Points[i] = newPoint;
                    }
                }
            }
        }

        public List<SvgMobject> Flatten(params SvgMobject[] inputList)
        {
            List<SvgMobject> outputList = new List<SvgMobject>();
            foreach (SvgMobject svgmobj in inputList)
            {
                if (inputList.Length > 1)
                    outputList.AddRange(Flatten(svgmobj));
                else
                    outputList.Add(svgmobj);
            }
            return outputList;
        }
        public List<SvgElement> Flatten(params SvgElement[] inputList)
        {
            List<SvgElement> outputList = new List<SvgElement>();
            foreach (SvgElement svgmobj in inputList)
            {
                if (inputList.Length > 1)
                    outputList.AddRange(Flatten(svgmobj));
                else
                    outputList.Add(svgmobj);
            }
            return outputList;
        }

        public List<SvgElement> GetAllNamedChildNodes(SvgElement element)
        {
            if (!String.IsNullOrEmpty(element.ID))
                return new List<SvgElement>() { element };

            List<SvgElement> namedChildNodes = new List<SvgElement>();
            foreach (SvgElement e in element.Children)
                namedChildNodes.AddRange(GetAllNamedChildNodes(e));
            return Flatten(namedChildNodes.ToArray());
        }
        /// <summary>
        /// This function is deprecated, please use <see cref="GetAllNamedChildNodes(SvgElement)"/>
        /// </summary>
        [Obsolete("Use GetAllNamedChildNodes()")]
        public List<SvgElement> GetAllChildNodesHaveId(SvgElement element)
        {
            return GetAllNamedChildNodes(element);
        }

        public void UpdateRefToElement(params SvgElement[] defs)
        {
            foreach (SvgElement element in defs)
            {
                foreach (SvgElement e in GetAllNamedChildNodes(element))
                {
                    if (RefToElement.ContainsKey(e.ID))
                        RefToElement[e.ID] = e;
                    else
                        RefToElement.Add(e.ID, e);
                }
            }
        }

        public void MoveIntoPosition()
        {
            if (ShouldCenter)
                Center();
            if (Height >= 0)
                SetHeight(Height);
            if (Width >= 0)
                SetWidth(Width);
        }
    }

    public class VMobjectFromSvgPathstring : VMobject
    {
        public string PathString { get; set; }

        public VMobjectFromSvgPathstring(string pathString, string name = null, Color color = null, int dim = 3, Mobject target = null)
            : base(name, color, dim, target)
        {
            PathString = pathString;
        }

        [Obsolete]
        /// <summary>
        /// Returns a list of all standard SVG path commands.
        /// </summary>
        public static List<string> GetPathCommands()
        {
            // TODO: This function should probably be replaced with s
            // const string, so that the regex can be compiled, which
            // saves us performance
            List<string> commands = new List<string>()
            {
                "M",  // moveto
                "L",  // lineto
                "H",  // horizontal lineto
                "V",  // vertical lineto
                "C",  // curveto
                "S",  // smooth curveto
                "Q",  // quadratic Bezier curve
                "T",  // smooth quadratic Bezier curveto
                "A",  // elliptical Arc
                "Z",  // closepath
            };
            commands.Capacity = commands.Count * 2;
            foreach (string comm in commands)
                commands.Add(comm.ToLower());
            return commands;
        }

        public void GeneratePoints()
        {
            //string pattern = "[" + String.Join("", GetPathCommands()) + "]";
            var parts = Regex.Matches(PathString, "(?<command>[MLHVCSQTAZ]){1}(?<param>[\\d.,]*)", RegexOptions.IgnoreCase);
            foreach (Match part in parts)
                HandleCommand(part.Groups["command"].Value[0], part.Groups["param"].Value);

            // People treat y-coordinate differently
            Rotate(System.Math.PI, RIGHT, ORIGIN);
        }

        public void HandleCommand(char cmd, string coordString)
        {
            bool isLower = char.IsLower(cmd);
            cmd = char.ToUpper(cmd);
            // newPoints are the points that will be added to the currPoints
            // list. This variable may get modified in the conditionals below.
            List<Vector<double>> newPoints = StringToPoints(coordString);

            if (isLower && Points.Count > 0)
                newPoints.Add(Points[^1]);

            if (cmd == 'M') // moveto
            {
                StartNewPath(newPoints[0]);
                if (newPoints.Count <= 1)
                    return;
                // Draw relative line-to values.
                newPoints = newPoints.Slice(1);
                cmd = 'L';

                foreach (Vector<double> p in newPoints)
                {
                    if (isLower)
                    {
                        // Treat everything as relative line-to until empty
                        p[0] += Points[^1][0];
                        p[1] += Points[^1][1];
                    }
                    AddLineTo(p);
                }
                return;
            }

            else if (cmd == 'L' || cmd == 'H' || cmd == 'V') // lineto
            {
                if (cmd == 'H')
                    newPoints[0][1] = Points[^1][1];
                else if (cmd == 'V')
                {
                    if (isLower)
                    {
                        newPoints[0][0] -= Points[^1][1];
                        newPoints[0][0] += Points[^1][1];
                    }
                    newPoints[0][1] = newPoints[0][0];
                    newPoints[0][0] = Points[^1][0];
                }
                AddLineTo(newPoints[0]);
                return;
            }

            if (cmd == 'C') // curveto
            {
                // Yay! No action required
                // pass
            }
            else if (cmd == 'S' || cmd == 'T') // smooth curveto
            {
                AddSmoothCurveTo(newPoints);
                // handle1 = points[-1] + (points[-1] - points[-2])
                // new_points = np.append([handle1], new_points, axis=0)
                return;
            }
            else if (cmd == 'Q') // quadratic Bezier curve
            {
                // TODO, this is a suboptimal approximation
                newPoints.Insert(0, newPoints[0]);
            }
            else if (cmd == 'A') // elliptical Arc
            {
                throw new NotImplementedException();
            }
            else if (cmd == 'Z') // close path
            {
                return;
            }

            // Add first three points
            AddCubicBezierCurveTo(newPoints[0], newPoints[1], newPoints[2]);

            // Handle situations where there's multiple relative control points
            if (newPoints.Count > 3)
            {
                // Add subsequent offset points relatively.
                for (int i = 3; i < newPoints.Count; i+=3)
                {
                    if (isLower)
                    {
                        newPoints[i] -= Points[^1];
                        newPoints[i+1] -= Points[^1];
                        newPoints[i+2] -= Points[^1];
                        newPoints[i] += newPoints[i - 1];
                        newPoints[i+1] += newPoints[i - 1];
                        newPoints[i+2] += newPoints[i - 1];
                    }
                    AddCubicBezierCurveTo(newPoints[0], newPoints[1], newPoints[2]);
                }
            }
        }

        public static List<Vector<double>> StringToPoints(string coordString)
        {
            List<double> numbers = StringToNumbers(coordString).ToList();
            if (numbers.Count % 2 == 1)
                numbers.Add(0);
            int numPoints = numbers.Count; // 2
            return numbers.Reshape((numPoints, 2)).ToJagged()
                .Select(ds => Vector<double>.Build.DenseOfArray(ds)).ToList();
        }

        /// <summary>
        /// Parses a string containing a comma- and space-delimited list of numbers
        /// </summary>
        public static List<double> StringToNumbers(string numString)
        {
            numString = numString.Replace("-", ",-").Replace("e,-", "e-");
            var splitString = numString.Split(new []{ ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<double> nums = new List<double>(splitString.Length);
            for (int i = 0; i < nums.Count; i++)
            {
                nums[i] = Double.Parse(splitString[i]);
            }
            return nums;
        }
    }
}
