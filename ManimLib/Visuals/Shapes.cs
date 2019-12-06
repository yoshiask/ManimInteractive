using System;
using System.Collections.Generic;
using System.Linq;

namespace ManimLib.Visuals
{
    public class Shapes
    {
        public abstract class Mobject_Shape : IManimElement
        {
            #region Properties
            private string _name = "Shape";
            public string Name {
                get {
                    return _name;
                }
                set {
                    var old = this;
                    _name = value;
                    OnShapeChanged(this, old);
                }
            }

            private string _fill = Common.Colors["BLACK"];
            public string Fill {
                get {
                    return _fill;
                }
                set {
                    var old = this;
                    _fill = value;
                    OnShapeChanged(this, old);
                }
            }

            private string _outline = Common.Colors["WHITE"];
            public string Outline {
                get {
                    return _outline;
                }
                set {
                    var old = this;
                    _outline = value;
                    OnShapeChanged(this, old);
                }
            }

            private double _outlineThickness = 0;
            public double OutlineThickness {
                get {
                    return _outlineThickness;
                }
                set {
                    var old = this;
                    _outlineThickness = value;
                    OnShapeChanged(this, old);
                }

            }

            private Rect _size = new Rect(100, 100);
            public Rect Size {
                get {
                    return _size;
                }
                set {
                    var old = this;
                    _size = value;
                    OnSizeChanged(value, old.Size);
                    OnShapeChanged(this, old);
                }

            }

            private Point _location = new Point(0, 0);
            public Point Location {
                get {
                    return _location;
                }
                set {
                    var old = this;
                    _location = value;
                    OnLocationChanged(value, old.Location);
                    OnShapeChanged(this, old);
                }

            }

            public abstract string MobjType { get; }
            #endregion

            #region Events
            public event ShapeChangedHandler OnShapeChanged;
            public delegate void ShapeChangedHandler(Mobject_Shape current, Mobject_Shape old);

            public event SizeChangedHandler OnSizeChanged;
            public delegate void SizeChangedHandler(Rect current, Rect old);

            public event LocationChangedHandler OnLocationChanged;
            public delegate void LocationChangedHandler(Point current, Point old);
            #endregion

            public Dictionary<string, AnimationMethod> AvailableAnimations = new Dictionary<string, AnimationMethod>();
            public Dictionary<string, Type[]> AnimationArgs = new Dictionary<string, Type[]>();
            public delegate string AnimationMethod(object[] args);
            public abstract void LoadAnimations();

            /// <summary>
            /// Returns a string that initializes the mobject in manim for Python scenes.
            /// </summary>
            /// <returns></returns>
            public string GetPyInitializer(string AddToEachLine)
            {
                string init = $"{AddToEachLine}{Name} = {MobjType}()\r\n";

                init += $"{AddToEachLine}{Name}.set_fill({Fill}, opacity=1.0)\r\n";
                init += $"{AddToEachLine}{Name}.set_outline({Outline}, opacity=1.0)\r\n";

                init += $"{AddToEachLine}{Name}.set_height({Size.GetHeight()})\r\n";
                init += $"{AddToEachLine}{Name}.stretch_to_fit_width({Size.GetWidth()})\r\n";

                // Calculate vectors for positioning
                init += $"{AddToEachLine}{Name}.shift({Vector.PointToPythonVector(Location)})";
                return init;
            }

            public string GetManimType()
            {
                return MobjType;
            }
        }

        public class Mobject_Rectangle : Mobject_Shape
        {
            public override string MobjType {
                get;
            } = "Rectangle";

            public override void LoadAnimations()
            {
                AvailableAnimations.Add("ShowCreation", GetShowCreationAnim);
                AvailableAnimations.Add("FadeIn", GetFadeInAnim);
                AvailableAnimations.Add("FadeOut", GetFadeOutAnim);
            }
            public string GetShowCreationAnim(object arg = null)
            {
                return $"self.play(ShowCreation({Name}))";
            }
            public string GetDrawBorderThenFillAnim(object arg = null)
            {
                return $"self.play(DrawBorderThenFill({Name}))";
            }
            public string GetFadeInAnim(object arg = null)
            {
                return $"self.play(FadeIn({Name}))";
            }
            public string GetFadeOutAnim(object arg = null)
            {
                return $"self.play(FadeOut({Name}))";
            }
        }

        public class Mobject_Ellipse : Mobject_Shape
        {
            public override string MobjType {
                get;
            } = "Ellipse";

            public override void LoadAnimations()
            {
                AvailableAnimations.Add("ShowCreation", GetShowCreationAnim);
                AvailableAnimations.Add("FadeIn", GetFadeInAnim);
                AvailableAnimations.Add("FadeOut", GetFadeOutAnim);
            }
            public string GetShowCreationAnim(object[] args = null)
            {
                return $"self.play(ShowCreation({Name}))";
            }
            public string GetDrawBorderThenFillAnim(object arg = null)
            {
                return $"self.play(DrawBorderThenFill({Name}))";
            }
            public string GetFadeInAnim(object[] args = null)
            {
                return $"self.play(FadeIn({Name}))";
            }
            public string GetFadeOutAnim(object[] args = null)
            {
                return $"self.play(FadeOut({Name}))";
            }
        }

        public class Mobject_Text : Mobject_Shape
        {
            public override string MobjType {
                get;
            } = "TextMobject";

            public event TextContentChangedHandler OnTextContentChanged;
            public delegate void TextContentChangedHandler(string current, string old);
            private string _text = "";
            public string TextContent {
                get {
                    return _text;
                }
                set {
                    OnTextContentChanged(value, _text);
                    _text = value;
                }
            }

            public override void LoadAnimations()
            {
                AvailableAnimations.Add("Write", GetWriteAnim);
                AvailableAnimations.Add("FadeIn", GetFadeInAnim);
                AvailableAnimations.Add("FadeOut", GetFadeOutAnim);
            }
            public new string GetPyInitializer(string AddToEachLine)
            {
                string init = $"{AddToEachLine}{Name} = TextMobject(\r\n";
                init += $"{AddToEachLine}{Common.PY_TAB}\"{TextContent}\"\r\n";
                //init += $"{Common.PY_TAB}text_to_color_map={{\"{TextContent}\"}}";
                init += $"{AddToEachLine})\r\n";

                init += $"{AddToEachLine}{Name}.set_fill({Fill}, opacity=1.0)\r\n";
                init += $"{AddToEachLine}{Name}.set_outline({Outline}, opacity=1.0)\r\n";

                init += $"{AddToEachLine}{Name}.set_height({Size.GetHeight()})\r\n";
                //init += $"{Common.PY_TAB}{Common.PY_TAB}{Name}.stretch_to_fit_width({GetRelativeRect().Width * FrameWidth})\r\n";

                // Calculate vectors for positioning
                init += $"{AddToEachLine}{Name}.shift({Vector.PointToPythonVector(Location)})";
                return init;
            }
            public string GetWriteAnim(object[] args = null)
            {
                return $"self.play(Write({Name}))";
            }
            public string GetFadeInAnim(object[] args = null)
            {
                return $"self.play(FadeIn({Name}))";
            }
            public string GetFadeOutAnim(object[] args = null)
            {
                return $"self.play(FadeOut({Name}))";
            }
        }

        public class Mobject_TeX : Mobject_Shape
        {
            public override string MobjType {
                get;
            } = "TexMobject";

            public event TeXContentChangedHandler OnTeXContentChanged;
            public delegate void TeXContentChangedHandler(string current, string old);
            private string _tex = "x^2=0";
            public string TeXContent {
                get {
                    return _tex;
                }
                set {
                    OnTeXContentChanged(value, _tex);
                    _tex = value;
                }
            }

            public static string ChangeFontColor(string tex, string LaTeXcolor)
            {
                if (LaTeXHelper.Colors.ContainsKey(LaTeXcolor))
                {
                    return @"\colorbox{" + LaTeXcolor + "}{" + tex + "}";
                }
                else if (Common.Colors.ContainsKey(LaTeXcolor))
                {
                    return @"\usepackage[dvipsnames]{xcolor}" + "\r\n" + @"\color{" + LaTeXcolor + "}{" + tex + "}";
                }
                else
                {
                    return tex;
                }
            }

            public override void LoadAnimations()
            {
                AvailableAnimations.Add("Write", GetWriteAnim);
                AvailableAnimations.Add("FadeIn", GetFadeInAnim);
                AvailableAnimations.Add("FadeOut", GetFadeOutAnim);
            }
            public new string GetPyInitializer(string AddToEachLine)
            {
                // TODO: Separate lines and add a tab to each one
                string EscapedText = "";
                foreach (string line in TeXContent.Lines())
                {
                    EscapedText = AddToEachLine + Common.PY_TAB + TeXContent;
                }

                string init = $"{AddToEachLine}{Name} = TexMobject(\r\n";
                init += $"{AddToEachLine}{Common.PY_TAB}\"{LaTeXHelper.EscapeLaTeX(TeXContent)}\"\r\n";
                //init += $"{Common.PY_TAB}text_to_color_map={{\"{TextContent}\"}}";
                init += $"{AddToEachLine})\r\n";

                init += $"{AddToEachLine}{Name}.set_fill({Fill}, opacity=1.0)\r\n";
                init += $"{AddToEachLine}{Name}.set_outline({Outline}, opacity=1.0)\r\n";

                init += $"{AddToEachLine}{Name}.set_height({Size.GetHeight()})\r\n";
                //init += $"{Common.PY_TAB}{Common.PY_TAB}{Name}.stretch_to_fit_width({GetRelativeRect().Width * FrameWidth})\r\n";

                // Calculate vectors for positioning
                init += $"{AddToEachLine}{Name}.shift({Vector.PointToPythonVector(Location)})";
                return init;
            }
            public string GetWriteAnim(object[] args = null)
            {
                return $"self.play(Write({Name}))";
            }
            public string GetFadeInAnim(object[] args = null)
            {
                return $"self.play(FadeIn({Name}))";
            }
            public string GetFadeOutAnim(object[] args = null)
            {
                return $"self.play(FadeOut({Name}))";
            }
        }

        public class Mobject_PiCreature : Mobject_Shape
        {
            public override string MobjType {
                get;
            } = "PiCreature";

            public new string GetPyInitializer(string AddToEachLine)
            {
                string init = $"{AddToEachLine}{Name} = {MobjType}()\r\n";

                init += $"{AddToEachLine}{Name}.set_color({Fill})\r\n";

                // Set sizing
                if (Size.GetHeight() < Size.GetWidth())
                    init += $"{AddToEachLine}{Name}.set_height({Size.GetHeight()})\r\n";
                else
                    init += $"{AddToEachLine}{Name}.set_width({Size.GetWidth()})\r\n";

                // Calculate vectors for positioning
                init += $"{AddToEachLine}{Name}.shift({Vector.PointToPythonVector(Location)})";
                return init;
            }

            #region Animations
            public override void LoadAnimations()
            {
                AvailableAnimations.Add("FadeIn", GetFadeInAnim);
                AvailableAnimations.Add("FadeOut", GetFadeOutAnim);
                AvailableAnimations.Add("Look", GetLookAnim);
                AvailableAnimations.Add("LookAt", GetLookAtAnim);
                AvailableAnimations.Add("Blink", GetBlinkAnim);
                AvailableAnimations.Add("MakeEyeContact", GetMakeEyeContactAnim);
                AvailableAnimations.Add("Shrug", GetShrugAnim);
                AvailableAnimations.Add("Flip", GetFlipAnim);
            }
            public string GetFadeInAnim(object arg = null)
            {
                return $"self.play(FadeIn({Name}))";
            }
            public string GetFadeOutAnim(object arg = null)
            {
                return $"self.play(FadeOut({Name}))";
            }
            /// <summary>
            /// Returns a string that plays a Look animation [Accepts  <see cref="Point"/>]
            /// </summary>
            /// <param name="arg">Point to look at</param>
            /// <returns></returns>
            public string GetLookAnim(object arg)
            {
                var point = (Point)arg;
                // TODO: Check if working
                return $"self.play({Name}.look({point}))";
            }
            /// <summary>
            /// Returns a string that plays a LookAt animation [Accepts  <see cref="Mobject_Shape"/>]
            /// </summary>
            /// <param name="arg">Shape to look at</param>
            /// <returns></returns>
            public string GetLookAtAnim(object arg)
            {
                var shape = arg as Mobject_Shape;
                // TODO: Check if working
                return $"self.play({Name}.look_at({shape.Name}))";
            }
            public string GetBlinkAnim(object arg = null)
            {
                return $"self.play({Name}.blink())";
            }
            /// <summary>
            /// Returns a string that plays an animation that makes eye contact with another pi creature [Accepts  <see cref="Mobject_PiCreature"/>]
            /// </summary>
            /// <param name="arg">Pi creature to look at</param>
            /// <returns></returns>
            public string GetMakeEyeContactAnim(object arg)
            {
                var shape = arg as Mobject_PiCreature;
                // TODO: Check if working
                return $"self.play({Name}.make_eye_contact({shape.Name}))";
            }
            public string GetShrugAnim(object arg = null)
            {
                return $"self.play({Name}.shrug())";
            }
            /// <summary>
            /// Flips the shape
            /// </summary>
            /// <param name="arg">Direction to flip (as string)</param>
            /// <returns></returns>
            public string GetFlipAnim(object arg)
            {
                return $"self.play({Name}.flip({arg}))";
            }
            #endregion

            public static string ChangeSVGColor(string color)
            {
                string[] SVG = System.IO.File.ReadAllLines(System.IO.Path.Combine(Common.ManimLibDirectory, @"files\PiCreatures_plain.svg"));
                SVG[6] = @"	.st1{fill:" + Common.Colors[color] + ";}";

                string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $@"ManimInteractive\");
                System.IO.Directory.CreateDirectory(path);
                path += $"PiCreatures_{color}.svg";
                System.IO.File.WriteAllLines(path, SVG);
                return path;
            }
        }

        public class Mobject_Graph : Mobject_Shape
        {
            public override string MobjType {
                get;
            } = "Rectangle";
            public Dictionary<string, object> Config { get; set; } = new Dictionary<string, object>();

            public new string GetPyInitializer(string AddToEachLine)
            {
                string init = $"{AddToEachLine}CONFIG = {{\r\n";
                init += $"{AddToEachLine}{Common.PY_TAB}" + "\"function\" : " + Config["function"] + ",\r\n";
                init += $"{AddToEachLine}{Common.PY_TAB}" + "\"function_color\" : " + Config["function_color"] + ",\r\n";
                init += $"{AddToEachLine}{Common.PY_TAB}" + "\"center_point\" : " + Config["center_point"] + ",\r\n";
                init += $"{AddToEachLine}{Common.PY_TAB}" + "\"x_min\" : " + Config["x_min"] + ",\r\n";
                init += $"{AddToEachLine}{Common.PY_TAB}" + "\"x_max\" : " + Config["x_max"] + ",\r\n";
                init += $"{AddToEachLine}{Common.PY_TAB}" + "\"y_min\" : " + Config["y_min"] + ",\r\n";
                init += $"{AddToEachLine}{Common.PY_TAB}" + "\"y_max\" : " + Config["y_max"] + ",\r\n";
                init += $"{AddToEachLine}{Common.PY_TAB}" + "\"graph_origin\" : " + Config["graph_origin"] + ",\r\n";
                init += $"{AddToEachLine}}}\r\n";
                return init;
            }
            public override void LoadAnimations()
            {
                AvailableAnimations.Add("ShowCreation", GetShowCreationAnim);
                AvailableAnimations.Add("Transform", GetTransformAnim);
            }
            public string GetShowCreationAnim(object arg)
            {
                string init = $"{(string)arg}self.setup_axes()\r\n";
                init += $"{(string)arg}func_graph = self.get_graph(\r\n";
                init += $"{(string)arg}{Common.PY_TAB}self.function,\r\n";
                init += $"{(string)arg}{Common.PY_TAB}self.function_color,\r\n";
                init += $"{(string)arg})\r\n";
                init += $"{(string)arg}self.play(\r\n";
                init += $"{(string)arg}{Common.PY_TAB}ShowCreation(func_graph, run_time = 2)\r\n";
                init += $"{(string)arg})";
                return init;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="args">Graph to morph into</param>
            /// <returns></returns>
            public string GetTransformAnim(object arg)
            {
                return $"self.play(Transform({Name}, {arg}))";
            }
        }
    }

    public static class Drawings
    {
        private static IList<string> _drawings;
        public static IList<string> ManimDrawings {
            get {
                if (_drawings == null && Common.ManimDirectory != "")
                {
                    _drawings = new List<string>();
                    string path = System.IO.Path.Combine(Common.ManimLibDirectory, @"mobject\svg\drawings.py");
                    var script = System.IO.File.ReadAllLines(path).ToList();
                    foreach (string line in script)
                    {
                        if (line.StartsWith("class "))
                        {
                            string classname;
                            // Remove "class "
                            classname = line.Remove(0, 6);

                            // Get class name
                            classname = classname.Split('(')[0];

                            // Add to Drawings list
                            _drawings.Add(classname);
                        }
                    }
                }
                return _drawings;
            }
        }
    }

    public interface IManimElement
    {
        string GetManimType();
    }
}
