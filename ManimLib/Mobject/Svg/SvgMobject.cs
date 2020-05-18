using ManimLib.Mobject.Types;
using Color = RL.Color;
using System;
using System.Collections.Generic;
using static ManimLib.Constants;
using System.IO;
using Svg;
using MathNet.Numerics.LinearAlgebra;
using ManimLib.Utils;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;
using MathNet.Numerics;
using ManimLib.Math;

namespace ManimLib.Mobject.Svg
{
    public class SvgMobject : VMobject
    {
        #region Properties
        public bool ShouldCenter { get; set; } = true;
        public double Height { get; set; } = 2.0;
        public double Width { get; set; } = 0;
        // Must be filled in a subclass, or when called
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string SvgText { get; set; }
        public bool UnpackGroups { get; set; } = true; // If false, creates a hierarchy of VGroups
        public double StrokeWidth { get; set; } = DEFAULT_STROKE_WIDTH;
        public double FillOpacity { get; set; } = 1.0;
        //public Color FillColor { get; set; } = COLORS[Colors.LIGHT_GREY];

        public Dictionary<string, XmlElement> RefToElement { get; set; }
        #endregion

        #region Helpers
        public static List<Vector<double>> StringToPoints(string coordString)
        {
            double[] numbers = StringToNumbers(coordString);
            List<Vector<double>> points = new List<Vector<double>>(numbers.Length + 1);
            for (int i = 0; i < numbers.Length; i += 2)
            {
                double x = numbers[i];
                double y = i == numbers.Length ? 0 : numbers[i];
                points.Add(Vector<double>.Build.DenseOfArray(new double[] { x, y }));
            }
            return points;
        }

        /// <summary>
        /// Parses a string containing a comma- and space-delimited list of numbers
        /// </summary>
        public static double[] StringToNumbers(string numString)
        {
            numString = numString.Replace("-", ",-").Replace("e,-", "e-");
            var splitString = numString.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            double[] nums = new double[splitString.Length];
            for (int i = 0; i < nums.Length; i++)
            {
                nums[i] = Double.Parse(splitString[i]);
            }
            return nums;
        }

        public static Color ProcessColor(string colorString, Color defaultColor = null)
        {
            if (String.IsNullOrWhiteSpace(colorString) || colorString == "none")
            {
                return Color.Transparent;
            }
            else if (colorString == "#FFF")
            {
                return COLORS[Colors.WHITE];
            }
            else if (colorString == "#000")
            {
                return COLORS[Colors.BLACK];
            }
            else
            {
                try
                {
                    return new Color(colorString);
                }
                catch
                {
                    return defaultColor ?? COLORS[Colors.WHITE];
                }
            }
        }
        #endregion

        public SvgMobject(string fileName = null, string svgText = null, string name = null, Color color = null, int dim = 3, Mobject target = null) : base(name, color, dim, target)
        {
            if (fileName != null)
            {
                FileName = fileName;
                EnsureValidFile();
                MoveIntoPosition();
            }
            else if (svgText != null)
            {
                SvgText = svgText;
            }
        }

        public void EnsureValidFile()
        {
            if (String.IsNullOrWhiteSpace(FileName) || SvgText == null)
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
            // TODO: Some platforms, like UWP, don't support the vvvv.SVG library
            // because they don't have System.Drawing. Looks like I have to resort
            // back to walking the XML DOM manually...
            //SvgDocument doc = FilePath != null ? SvgDocument.Open(FilePath) : SvgDocument.FromSvg<SvgDocument>(SvgText);
            XmlDocument doc = new XmlDocument();
            if (FilePath != null)
                doc.Load(FilePath);
            else
                doc.LoadXml(SvgText);

            var mobjects = GetMobjectsFrom(doc.DocumentElement);
            if (UnpackGroups)
                Add(mobjects);
            else
                Add(mobjects[0].Submobjects);
        }

        public List<VMobject> GetMobjectsFrom(XmlElement element)
        {
            List<VMobject> mobjects = new List<VMobject>();
            switch (element.Name)
            {
                case "defs":
                    UpdateRefToElement(element);
                    break;

                case "style":
                    // TODO: Handle style
                    break;

                case "g":
                case "svg":
                case "symbol":
                    foreach (XmlElement child in element.ChildNodes)
                    {
                        mobjects.AddRange(GetMobjectsFrom(child));
                    }
                    break;

                case "path":
                    mobjects.Add(new VMobjectFromSvgPathstring(element.Attributes["d"].Value));
                    break;

                case "use":
                    mobjects.AddRange(UseToMobjects(element));
                    break;

                case "rect":
                    mobjects.Add(RectToMobject(element));
                    break;

                case "circle":
                    mobjects.Add(CircleToMobject(element));
                    break;

                case "ellipse":
                    mobjects.Add(EllipseToMobject(element));
                    break;

                case "polyline":
                case "polygon":
                    mobjects.Add(PolygonToMobject(element));
                    break;

                default:
                    Debug.WriteLine("Unknown element type: " + element.Name);
                    break;
            }

            //foreach (SvgPolygon polygon in doc.SelectNodes("polygon"))
            //{
            //    // Handle polygon & polyline
            //    VMobject mobj = new VMobject(polygon.ID, new Color(polygon.Fill?.ToString()), dim: 2);
            //    mobj.Points.Clear();

            //    for (int i = 0; i+1 < polygon.Points.Count; i+=2)
            //    {
            //        mobj.Points.Add(Vector<double>.Build.DenseOfArray(new double[] {
            //            polygon.Points[i], polygon.Points[i + 1]
            //        }));
            //    }
            //    mobjects.Add(mobj);
            //}
            //foreach (SvgPath path in doc.SelectNodes("path"))
            //{
            //    // TODO: Recognize circles, rectangles, and other primitives that
            //    // have a Manim equivalent.
            //    VMobject mobj = new VMobject(path.ID, new Color(path.Fill?.ToString()), dim: 2);
            //    mobj.Points.Clear();
            //    mobj.Points.Add(path.PathData[0].Start.ToVector());
            //    foreach (var data in path.PathData)
            //    {
            //        var vPoint = data.End.ToVector();
            //        mobj.Points.Add(vPoint);
            //    }
            //    mobjects.Add(mobj);
            //}
            return mobjects;
        }

        #region SVG Element to Mobject
        public List<VMobject> GToMobjects(XmlElement gElement)
        {
            VMobject mobj = new VGroup(GetMobjectsFrom(gElement));
            HandleTransforms(gElement, mobj);
            return mobj.Submobjects;
        }

        /// <summary>
        /// Returns a new VMobjectFromSvgPathstring using the provided pathString.
        /// Use <see cref="VMobjectFromSvgPathstring.VMobjectFromSvgPathstring(string, string, Color, int, Mobject)"/>.
        /// </summary>
        [Obsolete]
        public VMobject PathStringToVMobject(string pathString)
        {
            return new VMobjectFromSvgPathstring(pathString);
        }

        public List<VMobject> UseToMobjects(XmlElement useElement)
        {
            // Remove initial "#" character
            string refName = useElement.Attributes["href"].Value.Substring(1);
            if (!RefToElement.ContainsKey(refName))
            {
                // TODO: Find a way to reasonably handle the reference
                // not being recognized
                //return new List<VMobject>() { new VMobject() };
                throw new KeyNotFoundException($"The reference \"{refName}\" was not recognized");
            }
            return GetMobjectsFrom(RefToElement[refName]);
        }

        public VMobject PolygonToMobject(XmlElement polygonElement)
        {
            string pathString = polygonElement.Attributes["points"].Value;
            return new VMobject
            {
                Points = StringToPoints(pathString)
            };
        }

        public Circle CircleToMobject(XmlElement circleElement)
        {
            double x, y, r;
            string xVal = circleElement.GetAttribute("cx");
            string yVal = circleElement.GetAttribute("cy");
            string rVal = circleElement.GetAttribute("r");
            Double.TryParse(xVal, out x);
            Double.TryParse(yVal, out y);
            Double.TryParse(rVal, out r);

            return (Circle)(new Circle(radius: r).Shift(x * RIGHT + y * DOWN));
        }

        public Circle EllipseToMobject(XmlElement ellipseElement)
        {
            string xVal = ellipseElement.GetAttribute("cx");
            string yVal = ellipseElement.GetAttribute("cy");
            string rxVal = ellipseElement.GetAttribute("rx");
            string ryVal = ellipseElement.GetAttribute("ry");
            Double.TryParse(xVal, out double x);
            Double.TryParse(yVal, out double y);
            Double.TryParse(rxVal, out double rx);
            Double.TryParse(ryVal, out double ry);

            return (Circle)(new Circle().Scale(rx, aboutEdge: RIGHT).Scale(ry, aboutEdge: UP)
                .Shift(x * RIGHT + y * DOWN));
        }

        public VMobject RectToMobject(XmlElement rectElement)
        {
            Color fillColor = ProcessColor(rectElement.GetAttribute("fill"));
            Color strokeColor = ProcessColor(rectElement.GetAttribute("stroke"));
            bool hasStrokeWidth = Double.TryParse(rectElement.GetAttribute("stroke-width"), out double strokeWidth);
            bool hasCornerRadius = Double.TryParse(rectElement.GetAttribute("corner-radius"), out double cornerRadius);
            Double.TryParse(rectElement.GetAttribute("width"), out double width);
            Double.TryParse(rectElement.GetAttribute("height"), out double height);

            VMobject vmobj;
            if (!hasCornerRadius || cornerRadius == 0)
            {
                vmobj = new Rectangle(
                    width: width,
                    height: height
                );
                vmobj.SetStyle(strokeWidth: strokeWidth,
                    strokeColor: new Color[] { strokeColor },
                    fillColor: new Color[] { fillColor });
            }
            else
            {
                vmobj = new RoundedRectangle(
                    width: width,
                    height: height,
                    cornerRadius: cornerRadius
                );
                vmobj.SetStyle(strokeWidth: strokeWidth,
                    strokeColor: new Color[] { strokeColor },
                    fillColor: new Color[] { fillColor });
            }
            return (VMobject)vmobj.Shift(vmobj.GetCenter() - vmobj.GetCorner(UP + LEFT));
        }
        #endregion

        public void HandleTransforms(XmlElement element, VMobject vmobj)
        {
            double x, y;
            string xVal = element.GetAttribute("x");
            string yVal = element.GetAttribute("y");
            Double.TryParse(xVal, out x);
            Double.TryParse(yVal, out y);

            MatchCollection transforms = Regex.Matches(element.Attributes["transforms"].Value,
                @"(?<command>[A-Za-z]*)\((?<params>[\d\s-.,]*)\)");

            foreach (Match transform in transforms)
            {
                string command = transform.Groups["command"].Value;
                double[] parameters = StringToNumbers(transform.Groups["params"].Value);
                
                switch (command)
                {
                    case "matrix":
                        // TODO: This is ugly, and probably not performant (since ToArray()
                        // iterates over all nine values unnecesarily.
                        var matrix = Matrix<double>.Build.DenseOfArray(
                            parameters.Concat(new double[] { 0, 0, 1 }).ToArray().Reshape((3, 3)));
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
                        break;

                    case "translate":
                        // Shouldn't these add to the current X and Y values, not overwrite?
                        x = parameters[0];
                        y = parameters.Length > 1 ? parameters[1] : 0;
                        vmobj.Shift(x * RIGHT + (-y) * UP);
                        break;

                    case "scale":
                        vmobj.Scale(new double[] {
                            parameters[0],
                            parameters.Length > 1 ? parameters[1] : 1.0
                        });
                        break;

                    case "rotate":
                        Vector<double> aboutPoint = Vector<double>.Build.DenseOfArray(new double[] {
                            parameters.Length == 3 ? parameters[1] : 0,
                            parameters.Length == 3 ? parameters[2] : 0
                        });
                        vmobj.Rotate(Common.DegreesToRadians(parameters[0]), aboutPoint);
                        break;

                    case "skewX":
                        double mX = 1 / System.Math.Tan(Common.DegreesToRadians(parameters[0]));
                        for (int i = 0; i < vmobj.Points.Count; i++)
                        {
                            // See https://en.wikipedia.org/wiki/Shear_mapping#Definition
                            Matrix<double> skewX = Matrix<double>.Build.DenseOfArray(new double[,] {
                                { 1, mX },
                                { 0, 1 },
                            });
                            vmobj.Points[i] = skewX * vmobj.Points[i];
                        }
                        break;

                    case "skewY":
                        double mY = 1 / System.Math.Tan(Common.DegreesToRadians(parameters[0]));
                        for (int i = 0; i < vmobj.Points.Count; i++)
                        {
                            // See https://en.wikipedia.org/wiki/Shear_mapping#Definition
                            Matrix<double> skewY = Matrix<double>.Build.DenseOfArray(new double[,] {
                                { 1, 0 },
                                { mY, 1 },
                            });
                            vmobj.Points[i] = skewY * vmobj.Points[i];
                        }
                        break;
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
        public List<XmlElement> Flatten(params XmlElement[] inputList)
        {
            List<XmlElement> outputList = new List<XmlElement>();
            foreach (XmlElement svgmobj in inputList)
            {
                if (inputList.Length > 1)
                    outputList.AddRange(Flatten(svgmobj));
                else
                    outputList.Add(svgmobj);
            }
            return outputList;
        }

        public List<XmlElement> GetAllNamedChildNodes(XmlElement element)
        {
            if (!String.IsNullOrEmpty(element.GetAttribute("id")))
                return new List<XmlElement>() { element };

            List<XmlElement> namedChildNodes = new List<XmlElement>();
            foreach (XmlElement e in element.ChildNodes)
                namedChildNodes.AddRange(GetAllNamedChildNodes(e));
            return Flatten(namedChildNodes.ToArray());
        }
        /// <summary>
        /// This function is deprecated, please use <see cref="GetAllNamedChildNodes(SvgElement)"/>
        /// </summary>
        [Obsolete("Use GetAllNamedChildNodes()")]
        public List<XmlElement> GetAllChildNodesHaveId(XmlElement element)
        {
            return GetAllNamedChildNodes(element);
        }

        public void UpdateRefToElement(params XmlElement[] defs)
        {
            foreach (XmlElement element in defs)
            {
                foreach (XmlElement e in GetAllNamedChildNodes(element))
                {
                    string id = e.GetAttribute("id");
                    if (RefToElement.ContainsKey(id))
                        RefToElement[id] = e;
                    else
                        RefToElement.Add(id, e);
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

        public VMobjectFromSvgPathstring(string pathString, string name = null, Color color = null, int dim = 2, Mobject target = null)
            : base(name, color, dim, target)
        {
            PathString = pathString;
            GeneratePoints();
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
            //Rotate(System.Math.PI, RIGHT, ORIGIN);
        }

        public void HandleCommand(char cmd, string coordString)
        {
            bool isLower = char.IsLower(cmd);
            cmd = char.ToUpper(cmd);
            // newPoints are the points that will be added to the currPoints
            // list. This variable may get modified in the conditionals below.
            List<Vector<double>> newPoints = SvgMobject.StringToPoints(coordString);

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
    }
}
