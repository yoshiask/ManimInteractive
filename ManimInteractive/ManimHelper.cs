using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ManimInteractive
{
    public class ManimHelper
    {
        public const string PY_TAB = @"    ";
        public const string PythonSceneHeader = "#!/usr/bin/env python\r\n\r\nfrom big_ol_pile_of_manim_imports import *\r\n\r\n";

        public const int PixelHeight = 1440;
        public const int PixelWidth = 2560;
        public const double FrameHeight = 8;
        public const double FrameWidth = (8 * PixelWidth / PixelHeight);
        public static readonly Point FrameOrigin = new Point(FrameWidth / 2, FrameHeight / 2);

        public static CameraConfig ActiveCameraConfig = CameraConfig.Production;
        public class CameraConfig
        {
            public static readonly CameraConfig Production= new CameraConfig("Production", 2560, 1440, 1/60, "1440p60");
            public static readonly CameraConfig High = new CameraConfig("High", 1920, 1080, 1/60, "1080p60");
            public static readonly CameraConfig Medium = new CameraConfig("Medium", 1280, 720, 1/30, "720p30");
            public static readonly CameraConfig Low = new CameraConfig("Low", 854, 480, 1/15, "480p15");

            public string Name;
            public int Width;
            public int Height;
            public double FrameDuration;
            public string ExportFolder;
            public CameraConfig(string name, int pixelwidth, int pixelheight, double frameduration, string exportfolder)
            {
                Name = name;
                Width = pixelwidth;
                Height = pixelheight;
                FrameDuration = frameduration;
                ExportFolder = exportfolder;
            }
        }

        public class ExportOptions
        {
            public bool Preview = false;
            public bool LowQuality = false;
            public bool MediumQuality = false;
            public bool HideProgress = false;
            public bool SavePNG = false;
            public bool UseTransparency = false;
            public int StartAtAnimation = 0;
            public string BackgroundColor = Colors["BLACK"];
            //public CameraConfig Camera = CameraConfig.Production;
        }

        public static readonly Dictionary<string, string> Colors = new Dictionary<string, string>() {
            { "DARK_BLUE", "#236B8E" },
            { "DARK_BROWN", "#8B4513" },
            { "LIGHT_BROWN", "#CD853F" },
            { "BLUE_E", "#1C758A" },
            { "BLUE_D", "#29ABCA" },
            { "BLUE_C", "#58C4DD" },
            { "BLUE_B", "#9CDCEB" },
            { "BLUE_A", "#C7E9F1" },
            { "TEAL_E", "#49A88F" },
            { "TEAL_D", "#55C1A7" },
            { "TEAL_C", "#5CD0B3" },
            { "TEAL_B", "#76DDC0" },
            { "TEAL_A", "#ACEAD7" },
            { "GREEN_E", "#699C52" },
            { "GREEN_D", "#77B05D" },
            { "GREEN_C", "#83C167" },
            { "GREEN_B", "#A6CF8C" },
            { "GREEN_A", "#C9E2AE" },
            { "YELLOW_E", "#E8C11C" },
            { "YELLOW_D", "#F4D345" },
            { "YELLOW_C", "#FFFF00" },
            { "YELLOW_B", "#FFEA94" },
            { "YELLOW_A", "#FFF1B6" },
            { "GOLD_E", "#C78D46" },
            { "GOLD_D", "#E1A158" },
            { "GOLD_C", "#F0AC5F" },
            { "GOLD_B", "#F9B775" },
            { "GOLD_A", "#F7C797" },
            { "RED_E", "#CF5044" },
            { "RED_D", "#E65A4C" },
            { "RED_C", "#FC6255" },
            { "RED_B", "#FF8080" },
            { "RED_A", "#F7A1A3" },
            { "MAROON_E", "#94424F" },
            { "MAROON_D", "#A24D61" },
            { "MAROON_C", "#C55F73" },
            { "MAROON_B", "#EC92AB" },
            { "MAROON_A", "#ECABC1" },
            { "PURPLE_E", "#644172" },
            { "PURPLE_D", "#715582" },
            { "PURPLE_C", "#9A72AC" },
            { "PURPLE_B", "#B189C6" },
            { "PURPLE_A", "#CAA3E8" },
            { "WHITE", "#FFFFFF" },
            { "BLACK", "#000000" },
            { "LIGHT_GRAY", "#BBBBBB" },
            { "LIGHT_GREY", "#BBBBBB" },
            { "GRAY", "#888888" },
            { "GREY", "#888888" },
            { "DARK_GREY", "#444444" },
            { "DARK_GRAY", "#444444" },
            { "GREY_BROWN", "#736357" },
            { "PINK", "#D147BD" },
            { "GREEN_SCREEN", "#00FF00" },
            { "ORANGE", "#FF862F" },
        };
        public static string ManimDirectory { get; set; } = @"C:\Users\jjask\Documents\manim\";

        #region Shapes
        public abstract class IMobject_Shape : Draggable
        {
            public abstract string Fill {
                get; set;
            }
            public string Outline;
            public double OutlineThickness = 0;
            public abstract string MobjType { get; }

            public Dictionary<string, AnimationMethod> AvailableAnimations = new Dictionary<string, AnimationMethod>();
            public delegate string AnimationMethod(object[] args);
            public abstract void LoadAnimations();

            /// <summary>
            /// Returns a string that initializes the mobject in manim for Python scenes.
            /// </summary>
            /// <returns></returns>
            public abstract string GetPyInitializer(string AddToEachLine);
            /// <summary>
            /// Draws a red border around the bounds of the shape.
            /// </summary>
            /// <param name="thickness">Thickness of the border</param>
            public abstract void DrawSelectionBorder(double thickness = 5);

            public static string CalculateXPosition(double X)
            {
                double DistanceFromCenter = X - .5;
                double Shift = Math.Abs(DistanceFromCenter) * (FrameWidth / 1);
                string result = Shift.ToString();
                if (DistanceFromCenter < 0)
                {
                    result += "*LEFT";
                }
                else
                {
                    result += "*RIGHT";
                }

                return result;
            }
            public static string CalculateYPosition(double Y)
            {
                double DistanceFromCenter = Y - .5;
                double Shift = Math.Abs(DistanceFromCenter) * (FrameHeight / 1);
                string result = Shift.ToString();
                if (DistanceFromCenter < 0)
                {
                    result += "*UP";
                }
                else
                {
                    result += "*DOWN";
                }

                return result;
            }
        }

        public class Mobject_Rectangle : IMobject_Shape
        {
            private string _fill;
            public override string Fill {
                get {
                    return _fill;
                }
                set {
                    _fill = value;
                    InternalRectangle.Fill = Common.BrushFromHex(Colors[_fill]);
                }
            }
            public override string MobjType {
                get;
            } = "Rectangle";
            private Rectangle InternalRectangle;
            private Border InternalBorder;

            /// <summary>
            /// Draws a Rectangle mobject in the specified view.
            /// </summary>
            /// <param name="name">Name of the mobject</param>
            /// <param name="view">The view to draw the shape in</param>
            /// <param name="rect">Relative position and size of the shape</param>
            /// <param name="outline">Stroke color (manim)</param>
            /// <param name="fill">Fill color (manim)</param>
            /// <param name="zindex">Zindex/Arrange height</param>
            public static void Draw(string name, Panel view, Rect rect, string outline, string fill, int zindex)
            {
                var rectangle = new Rectangle()
                {
                    Fill = Common.BrushFromHex(Colors[fill]),
                    Stroke = Common.BrushFromHex(Colors[outline]),
                    StrokeThickness = 7,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    IsHitTestVisible = false
                };
                var border = new Border()
                {
                    Background = null,
                    BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent),
                    BorderThickness = new Thickness(5),
                    Child = rectangle
                };
                var item = new Mobject_Rectangle()
                {
                    Name = name,
                    Children =
                    {
                        border
                    },
                    IsDraggable = true,
                    IsHitTestVisible = true,
                    Background = new SolidColorBrush(System.Windows.Media.Colors.Transparent),
                    InternalRectangle = rectangle,
                    InternalBorder = border,
                    Fill = fill,
                };
                SetRelativeRect(item, rect);
                view.Children.Add(item);
                SetZIndex(item, zindex);
            }
            public override void DrawSelectionBorder(double thickness = 5)
            {
                if (thickness <= 0)
                {
                    InternalBorder.BorderThickness = new Thickness(5);
                    InternalBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
                }
                else
                {
                    InternalBorder.BorderThickness = new Thickness(thickness);
                    InternalBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
                }
            }

            public override string GetPyInitializer(string AddToEachLine)
            {
                string init = $"{AddToEachLine}{Name} = Rectangle()\r\n";

                init += $"{AddToEachLine}{Name}.set_fill({Fill}, opacity=1.0)\r\n";
                //init += $"{PY_TAB}{PY_TAB}{name}.set_outline({Outline}, opacity=1.0)\r\n";

                init += $"{AddToEachLine}{Name}.set_height({GetRelativeRect(this).Height * FrameHeight})\r\n";
                init += $"{AddToEachLine}{Name}.stretch_to_fit_width({GetRelativeRect(this).Width * FrameWidth})\r\n";

                // Calculate vectors for positioning
                init += $"{AddToEachLine}{Name}.shift({CalculateXPosition(GetRelativeRect(this).X)} + {CalculateYPosition(GetRelativeRect(this).Y)})";
                return init;
            }
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

        public class Mobject_Ellipse : IMobject_Shape
        {
            private string _fill;
            public override string Fill {
                get {
                    return _fill;
                }
                set {
                    _fill = value;
                    InternalEllipse.Fill = Common.BrushFromHex(Colors[_fill]);
                }
            }
            public override string MobjType {
                get;
            } = "Ellipse";
            private Ellipse InternalEllipse;
            private Border InternalBorder;

            /// <summary>
            /// Draws an Ellipse mobject in the specified view.
            /// </summary>
            /// <param name="name">Name of the mobject</param>
            /// <param name="view">The view to draw the shape in</param>
            /// <param name="rect">Relative position and size of the shape</param>
            /// <param name="outline">Stroke color (manim)</param>
            /// <param name="fill">Fill color (manim)</param>
            /// <param name="zindex">Zindex/Arrange height</param>
            public static void Draw(string name, Panel view, Rect rect, string outline, string fill, int zindex)
            {
                var ellipse = new Ellipse()
                {
                    Fill = Common.BrushFromHex(Colors[fill]),
                    Stroke = Common.BrushFromHex(Colors[outline]),
                    StrokeThickness = 7,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    IsHitTestVisible = false
                };
                var border = new Border()
                {
                    Background = null,
                    BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent),
                    BorderThickness = new Thickness(5),
                    Child = ellipse
                };
                var item = new Mobject_Ellipse()
                {
                    Name = name,
                    Children =
                    {
                        border
                    },
                    IsDraggable = true,
                    IsHitTestVisible = true,
                    Background = new SolidColorBrush(System.Windows.Media.Colors.Transparent),
                    InternalEllipse = ellipse,
                    InternalBorder = border,
                    Fill = fill,
                };
                SetRelativeRect(item, rect);
                view.Children.Add(item);
                SetZIndex(item, zindex);
            }
            public override void DrawSelectionBorder(double thickness = 5)
            {
                if (thickness <= 0)
                {
                    InternalBorder.BorderThickness = new Thickness(5);
                    InternalBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
                }
                else
                {
                    InternalBorder.BorderThickness = new Thickness(thickness);
                    InternalBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
                }
            }

            public override void LoadAnimations()
            {
                AvailableAnimations.Add("ShowCreation", GetShowCreationAnim);
                AvailableAnimations.Add("FadeIn", GetFadeInAnim);
                AvailableAnimations.Add("FadeOut", GetFadeOutAnim);
            }
            public override string GetPyInitializer(string AddToEachLine)
            {
                string init = $"{AddToEachLine}{Name} = Ellipse()\r\n";

                init += $"{AddToEachLine}{Name}.set_fill({Fill}, opacity=1.0)\r\n";
                //init += $"{PY_TAB}{PY_TAB}{name}.set_outline({Outline}, opacity=1.0)\r\n";

                init += $"{AddToEachLine}{Name}.set_height({GetRelativeRect(this).Height * FrameHeight})\r\n";
                init += $"{AddToEachLine}{Name}.stretch_to_fit_width({GetRelativeRect(this).Width * FrameWidth})\r\n";

                // Calculate vectors for positioning
                init += $"{AddToEachLine}{Name}.shift({CalculateXPosition(GetRelativeRect(this).X)} + {CalculateYPosition(GetRelativeRect(this).Y)})";
                return init;
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

        public class Mobject_Text : IMobject_Shape
        {
            private string _fill;
            public override string Fill {
                get {
                    return _fill;
                }
                set {
                    _fill = value;
                    InternalBlock.Foreground = Common.BrushFromHex(Colors[_fill]);
                }
            }
            public override string MobjType {
                get;
            } = "TextMobject";
            private string _text = "";
            public string TextContent {
                get {
                    return _text;
                }
                set {
                    _text = value;
                    InternalBlock.Text = value;
                }
            }
            private TextBlock InternalBlock;
            private Border InternalBorder;

            /// <summary>
            /// Draws a Text mobject in the specified view.
            /// </summary>
            /// <param name="name">Name of the mobject</param>
            /// <param name="view">Panel to draw in</param>
            /// <param name="rect">Relative sizing and location</param>
            /// <param name="text">Text content</param>
            /// <param name="fill">Text fill color</param>
            /// <param name="fontsize">Text size</param>
            /// <param name="zindex">Zindex/Arrange height</param>
            public static void Draw(string name, Panel view, Rect rect, string text, string fill, double fontsize, int zindex)
            {
                var textblock = new TextBlock()
                {
                    FontFamily = LaTeXHelper.DefaultFont,
                    FontSize = fontsize,
                    Text = text,
                    Background = null,
                    Foreground = Common.BrushFromHex(Colors[fill]),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    IsHitTestVisible = false
                };
                var border = new Border()
                {
                    Background = null,
                    BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent),
                    BorderThickness = new Thickness(5),
                    Child = textblock
                };
                var item = new Mobject_Text()
                {
                    Name = name,
                    Children =
                    {
                        border
                    },
                    IsDraggable = true,
                    IsHitTestVisible = true,
                    Background = new SolidColorBrush(System.Windows.Media.Colors.Transparent),
                    InternalBlock = textblock,
                    InternalBorder = border,
                    Fill = fill,
                    TextContent = text,
                };
                item.LoadAnimations();
                SetRelativeRect(item, rect);
                view.Children.Add(item);
                SetZIndex(item, zindex);
            }
            public override void DrawSelectionBorder(double thickness = 5)
            {
                if (thickness <= 0)
                {
                    InternalBorder.BorderThickness = new Thickness(5);
                    InternalBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
                }
                else
                {
                    InternalBorder.BorderThickness = new Thickness(thickness);
                    InternalBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
                }
            }

            public override void LoadAnimations()
            {
                AvailableAnimations.Add("Write", GetWriteAnim);
                AvailableAnimations.Add("FadeIn", GetFadeInAnim);
                AvailableAnimations.Add("FadeOut", GetFadeOutAnim);
            }
            public override string GetPyInitializer(string AddToEachLine)
            {
                string init = $"{AddToEachLine}{Name} = TextMobject(\r\n";
                init += $"{AddToEachLine}{PY_TAB}\"{TextContent}\"\r\n";
                //init += $"{PY_TAB}text_to_color_map={{\"{TextContent}\"}}";
                init += $"{AddToEachLine})\r\n";

                init += $"{AddToEachLine}{Name}.set_fill({Fill}, opacity=1.0)\r\n";
                //init += $"{PY_TAB}{PY_TAB}{name}.set_outline({Outline}, opacity=1.0)\r\n";

                init += $"{AddToEachLine}{Name}.set_height({GetRelativeRect(this).Height * FrameHeight})\r\n";
                //init += $"{PY_TAB}{PY_TAB}{Name}.stretch_to_fit_width({GetRelativeRect(this).Width * FrameWidth})\r\n";

                // Calculate vectors for positioning
                init += $"{AddToEachLine}{Name}.shift({CalculateXPosition(GetRelativeRect(this).X)} + {CalculateYPosition(GetRelativeRect(this).Y)})";
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

        public class Mobject_TeX : IMobject_Shape
        {
            private string _fill;
            public override string Fill {
                get {
                    return _fill;
                }
                set {
                    _fill = value;
                    InternalBlock.Formula = ChangeFontColor(TextContent, value);
                }
            }
            public override string MobjType {
                get;
            } = "TexMobject";
            private string _text = "";
            public string TextContent {
                get {
                    return _text;
                }
                set {
                    _text = value;
                    InternalBlock.Formula = ChangeFontColor(value, Fill);
                }
            }
            private WpfMath.Controls.FormulaControl InternalBlock;
            private Border InternalBorder;

            /// <summary>
            /// Draws a TeX mobject in the specified view.
            /// </summary>
            /// <param name="name">Name of the mobject</param>
            /// <param name="view">Panel to draw in</param>
            /// <param name="rect">Relative sizing and location</param>
            /// <param name="text">TeX / LaTeX</param>
            /// <param name="fill">Text fill color</param>
            /// <param name="fontsize">Text size</param>
            /// <param name="zindex">Zindex/Arrange height</param>
            public static void Draw(string name, Panel view, Rect rect, string tex, string fill, int zindex)
            {
                tex = ChangeFontColor(tex, fill);
                var texblock = new WpfMath.Controls.FormulaControl()
                {
                    Formula = tex,
                    Foreground = Common.BrushFromHex(Colors[fill]),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    IsHitTestVisible = false,
                };
                var border = new Border()
                {
                    Background = null,
                    BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent),
                    BorderThickness = new Thickness(5),
                    Child = texblock
                };
                var item = new Mobject_TeX()
                {
                    Name = name,
                    Children =
                    {
                        border
                    },
                    IsDraggable = true,
                    IsHitTestVisible = true,
                    Background = new SolidColorBrush(System.Windows.Media.Colors.Transparent),
                    InternalBlock = texblock,
                    InternalBorder = border,
                    Fill = fill,
                    TextContent = tex,
                };
                item.LoadAnimations();
                SetRelativeRect(item, rect);
                view.Children.Add(item);
                SetZIndex(item, zindex);
            }
            public override void DrawSelectionBorder(double thickness = 5)
            {
                if (thickness <= 0)
                {
                    InternalBorder.BorderThickness = new Thickness(5);
                    InternalBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
                }
                else
                {
                    InternalBorder.BorderThickness = new Thickness(thickness);
                    InternalBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
                }
            }
            public static string ChangeFontColor(string tex, string LaTeXcolor)
            {
                if (LaTeXHelper.Colors.ContainsKey(LaTeXcolor))
                {
                    return @"\colorbox{" + LaTeXcolor + "}{" + tex + "}";
                }
                else if (Colors.ContainsKey(LaTeXcolor))
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
            public override string GetPyInitializer(string AddToEachLine)
            {
                // TODO: Separate lines and add a tab to each one
                string EscapedText = "";
                foreach (string line in TextContent.Lines())
                {
                    EscapedText = AddToEachLine + PY_TAB + TextContent;
                }

                string init = $"{AddToEachLine}{Name} = TexMobject(\r\n";
                init += $"{AddToEachLine}{PY_TAB}@\"{TextContent}\"\r\n";
                //init += $"{PY_TAB}text_to_color_map={{\"{TextContent}\"}}";
                init += $"{AddToEachLine})\r\n";

                init += $"{AddToEachLine}{Name}.set_fill({Fill}, opacity=1.0)\r\n";
                //init += $"{PY_TAB}{PY_TAB}{name}.set_outline({Outline}, opacity=1.0)\r\n";

                init += $"{AddToEachLine}{Name}.set_height({GetRelativeRect(this).Height * FrameHeight})\r\n";
                //init += $"{PY_TAB}{PY_TAB}{Name}.stretch_to_fit_width({GetRelativeRect(this).Width * FrameWidth})\r\n";

                // Calculate vectors for positioning
                init += $"{AddToEachLine}{Name}.shift({CalculateXPosition(GetRelativeRect(this).X)} + {CalculateYPosition(GetRelativeRect(this).Y)})";
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

        public class Mobject_PiCreature : IMobject_Shape
        {
            private string _fill;
            public override string Fill {
                get {
                    return _fill;
                }
                set {
                    _fill = value;
                    InternalSvg.Source = new Uri(ChangeSVGColor(value));
                }
            }
            public override string MobjType {
                get;
            } = "PiCreature";
            private SharpVectors.Converters.SvgViewbox InternalSvg;
            private Border InternalBorder;

            /// <summary>
            /// Draws an Ellipse mobject in the specified view.
            /// </summary>
            /// <param name="name">Name of the mobject</param>
            /// <param name="view">The view to draw the shape in</param>
            /// <param name="rect">Relative position and size of the shape</param>
            /// <param name="outline">Stroke color (manim)</param>
            /// <param name="fill">Fill color (manim)</param>
            /// <param name="zindex">Zindex/Arrange height</param>
            public static void Draw(string name, Panel view, Rect rect, string fill, int zindex)
            {
                var svg = new SharpVectors.Converters.SvgViewbox()
                {
                    Stretch = Stretch.Uniform,
                    Source = new Uri(System.IO.Path.Combine(ManimDirectory, @"files\PiCreatures_plain.svg")),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    IsHitTestVisible = false
                };
                var border = new Border()
                {
                    Background = null,
                    BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent),
                    BorderThickness = new Thickness(5),
                    Child = svg
                };
                var item = new Mobject_PiCreature()
                {
                    Name = name,
                    Children =
                    {
                        border
                    },
                    IsDraggable = true,
                    IsHitTestVisible = true,
                    Background = new SolidColorBrush(System.Windows.Media.Colors.Transparent),
                    InternalSvg = svg,
                    InternalBorder = border,
                    Fill = fill,
                };
                SetRelativeRect(item, rect);
                view.Children.Add(item);
                SetZIndex(item, zindex);
            }
            public override void DrawSelectionBorder(double thickness = 5)
            {
                if (thickness <= 0)
                {
                    InternalBorder.BorderThickness = new Thickness(5);
                    InternalBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
                }
                else
                {
                    InternalBorder.BorderThickness = new Thickness(thickness);
                    InternalBorder.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
                }
            }

            public override string GetPyInitializer(string AddToEachLine)
            {
                string init = $"{AddToEachLine}{Name} = PiCreature()\r\n";

                init += $"{AddToEachLine}{Name}.set_color({Fill})\r\n";

                // Set sizing
                if (GetRelativeRect(this).Height * PixelHeight < GetRelativeRect(this).Width * PixelWidth)
                    init += $"{AddToEachLine}{Name}.set_height({GetRelativeRect(this).Height * FrameHeight})\r\n";
                else
                    init += $"{AddToEachLine}{Name}.set_width({GetRelativeRect(this).Width * FrameWidth})\r\n";

                // Calculate vectors for positioning
                init += $"{AddToEachLine}{Name}.shift({CalculateXPosition(GetRelativeRect(this).X)} + {CalculateYPosition(GetRelativeRect(this).Y)})";
                return init;
            }

            #region Animations
            public override void LoadAnimations()
            {
                AvailableAnimations.Add("FadeIn", GetFadeInAnim);
                AvailableAnimations.Add("FadeOut", GetFadeOutAnim);
                AvailableAnimations.Add("Look", GetLookAnim);
                AvailableAnimations.Add("LookAt", GetLookAtAnim);
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
            /// Returns a string that plays a LookAt animation [Accepts  <see cref="IMobject_Shape"/>]
            /// </summary>
            /// <param name="arg">Shape to look at</param>
            /// <returns></returns>
            public string GetLookAtAnim(object arg)
            {
                var shape = arg as IMobject_Shape;
                // TODO: Check if working
                return $"self.play({Name}.look({shape.Name}))";
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
            #endregion

            public static string ChangeSVGColor(string color)
            {
                string[] SVG = System.IO.File.ReadAllLines(System.IO.Path.Combine(ManimDirectory, @"files\PiCreatures_plain.svg"));
                SVG[6] = @"	.st1{fill:" + Colors[color] + ";}";

                string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $@"ManimInteractive\");
                System.IO.Directory.CreateDirectory(path);
                path += $"PiCreatures_{color}.svg";
                System.IO.File.WriteAllLines(path, SVG);
                return path;
            }
        }
        #endregion

        #region Built-in Drawings
        private static IList<string> _drawings;
        public static IList<string> ManimDrawings {
            get {
                if (_drawings == null && ManimDirectory != "")
                {
                    _drawings = new List<string>();
                    string path = System.IO.Path.Combine(ManimDirectory, @"mobject\svg\drawings.py");
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
        #endregion

        #region Command Line
        public static string RenderVideo(string sceneName)
        {
            return RenderVideo(sceneName, new ExportOptions());
        }
        public static string RenderVideo(string sceneName, ExportOptions options)
        {
            CameraConfig camera = CameraConfig.Production;
            string cmd = $@"py -3 extract_scene.py testing\exported_scenes.py {sceneName}";
            if (options.Preview)
                cmd += " -p";
            if (options.LowQuality)
            {
                cmd += " -l";
                camera = CameraConfig.Low;
            }
            if (options.MediumQuality)
            {
                cmd += "-m" +
                    "";
                camera = CameraConfig.Medium;
            }
            if (options.HideProgress)
                cmd += " -q";
            if (options.SavePNG)
                cmd += " -g";
            if (options.UseTransparency)
                cmd += " -t";
            cmd += $" -c {options.BackgroundColor}";
            cmd += $" -n {options.StartAtAnimation}";

            Common.RunCMD("cmd.exe", cmd, System.Diagnostics.ProcessWindowStyle.Normal);
            return $@"C:\Users\jjask\Videos\Manim Exports\videos\testing\exported_scenes\{camera.ExportFolder}\{sceneName}.mp4";
        }
        #endregion
    }

    public class LaTeXHelper
    {
        public static readonly FontFamily DefaultFont = new FontFamily("BKM-cmr17");
        public static readonly Dictionary<string, string> Colors = new Dictionary<string, string>()
        {
            { "white", "#f8f9fa" },

        };
    }
}
