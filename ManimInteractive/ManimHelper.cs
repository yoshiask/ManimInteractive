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
        public static string ManimDirectory { get; set; } = ""; //@"C:\Users\jjask\Documents\manim\";

        #region Shapes
        public abstract class IMobject_Shape : Draggable
        {
            public abstract string Fill {
                get; set;
            }
            public string Outline;
            public double OutlineThickness = 0;
            public abstract string MobjType { get; }

            public abstract string GetPyInitializer(string defaultName);

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
                    Background = Common.BrushFromHex(Colors[_fill]);
                }
            }
            public override string MobjType {
                get;
            } = "Rectangle";

            public static void Draw(string name, Panel view, Rect rect, string outline, string fill, int zindex)
            {
                var item = new Mobject_Rectangle()
                {
                    Name = name,
                    Fill = fill,
                    Outline = outline,
                    IsDraggable = true,
                };
                SetRelativeRect(item, rect);
                view.Children.Add(item);
                SetZIndex(item, zindex);
            }
            public override string GetPyInitializer(string defaultName)
            {
                if (String.IsNullOrWhiteSpace(Name))
                    Name = defaultName;

                string init = $"{Name} = Rectangle()\r\n";

                init += $"{PY_TAB}{PY_TAB}{Name}.set_fill({Fill}, opacity=1.0)\r\n";
                //init += $"{PY_TAB}{PY_TAB}{name}.set_outline({Outline}, opacity=1.0)\r\n";

                init += $"{PY_TAB}{PY_TAB}{Name}.set_height({GetRelativeRect(this).Height * FrameHeight})\r\n";
                init += $"{PY_TAB}{PY_TAB}{Name}.stretch_to_fit_width({GetRelativeRect(this).Width * FrameWidth})\r\n";

                // TODO: Calculate vectors for positioning
                init += $"{PY_TAB}{PY_TAB}{Name}.shift({CalculateXPosition(GetRelativeRect(this).X)} + {CalculateYPosition(GetRelativeRect(this).Y)})";
                return init;
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

            public static void Draw(string name, Panel view, Rect rect, string outline, string fill, int zindex)
            {
                var ellipse = new Ellipse()
                {
                    Fill = Common.BrushFromHex(Colors[fill]),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    IsHitTestVisible = false
                };
                var item = new Mobject_Ellipse()
                {
                    Name = name,
                    Children =
                    {
                        ellipse
                    },
                    IsDraggable = true,
                    IsHitTestVisible = true,
                    Background = new SolidColorBrush(System.Windows.Media.Colors.Transparent),
                    InternalEllipse = ellipse,
                    Fill = fill,
                };
                SetRelativeRect(item, rect);
                view.Children.Add(item);
                SetZIndex(item, zindex);
            }
            public override string GetPyInitializer(string defaultName)
            {
                if (String.IsNullOrWhiteSpace(Name))
                    Name = defaultName;

                string init = $"{Name} = Ellipse()\r\n";

                init += $"{PY_TAB}{PY_TAB}{Name}.set_fill({Fill}, opacity=1.0)\r\n";
                //init += $"{PY_TAB}{PY_TAB}{name}.set_outline({Outline}, opacity=1.0)\r\n";

                init += $"{PY_TAB}{PY_TAB}{Name}.set_height({GetRelativeRect(this).Height * FrameHeight})\r\n";
                init += $"{PY_TAB}{PY_TAB}{Name}.stretch_to_fit_width({GetRelativeRect(this).Width * FrameWidth})\r\n";

                // TODO: Calculate vectors for positioning
                init += $"{PY_TAB}{PY_TAB}{Name}.shift({CalculateXPosition(GetRelativeRect(this).X)} + {CalculateYPosition(GetRelativeRect(this).Y)})";
                return init;
            }
        }
        #endregion
    }
}
