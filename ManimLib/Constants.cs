using RL;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static ManimLib.Common;

namespace ManimLib
{
    public static class Constants
    {
        // TODO: Set these to PRODUCTION_QUALITY_CAMERA_CONFIG["pixel_height"]
        public const double DEFAULT_PIXEL_HEIGHT = 1440;
        public const double DEFAULT_PIXEL_WIDTH = 2560;
        public const double DEFAULT_FRAME_RATE = 60;

        public const double DEFAULT_STROKE_WIDTH = 4;

        public const double FRAME_HEIGHT = 8.0;
        public const double FRAME_WIDTH = FRAME_HEIGHT * DEFAULT_PIXEL_WIDTH / DEFAULT_PIXEL_HEIGHT;
        public const double FRAME_Y_RADIUS = FRAME_HEIGHT / 2;
        public const double FRAME_X_RADIUS = FRAME_WIDTH / 2;

        public const double SMALL_BUFF = 0.1;
        public const double MED_SMALL_BUFF = 0.25;
        public const double MED_LARGE_BUFF = 0.5;
        public const double LARGE_BUFF = 1.0;

        public const double DEFAULT_MOBJECT_TO_EDGE_BUFFER = MED_LARGE_BUFF;
        public const double DEFAULT_MOBJECT_TO_MOBJECT_BUFFER = MED_SMALL_BUFF;

        /// <summary>
        /// Default point-wise function run time, in seconds
        /// </summary>
        public const double DEFAULT_POINTWISE_FUNCTION_RUN_TIME = 3.0;
        /// <summary>
        /// Default animation wait time, in seconds
        /// </summary>
        public const double DEFAULT_WAIT_TIME = 1.0;

        public static readonly Vector<double> ORIGIN = NewVector(new double[] { 0, 0, 0 });
        public static readonly Vector<double> UP = NewVector(new double[] { 0, 1, 0 });
        public static readonly Vector<double> DOWN = NewVector(new double[] { 0, -1, 0 });
        public static readonly Vector<double> RIGHT = NewVector(new double[] { 1, 0, 0 });
        public static readonly Vector<double> LEFT = NewVector(new double[] { -1, 0, 0 });
        public static readonly Vector<double> IN = NewVector(new double[] { 0, 0, -1 });
        public static readonly Vector<double> OUT = NewVector(new double[] { 0, 0, 1 });
        public static readonly Vector<double> X_AXIS = NewVector(new double[] { 1, 0, 0 });
        public static readonly Vector<double> Y_AXIS = NewVector(new double[] { 0, 1, 0 });
        public static readonly Vector<double> Z_AXIS = NewVector(new double[] { 0, 0, 1 });
        public static readonly Vector<double> ONE_VECTOR = NewVector(new double[] { 1, 1, 1 });

        public static readonly Vector<double> UL = UP + LEFT;
        public static readonly Vector<double> UR = UP + RIGHT;
        public static readonly Vector<double> DL = DOWN + LEFT;
        public static readonly Vector<double> DR = DOWN + RIGHT;

        public static readonly Vector<double> TOP = FRAME_Y_RADIUS * UP;
        public static readonly Vector<double> BOTTOM = FRAME_Y_RADIUS * DOWN;
        public static readonly Vector<double> LEFT_SIDE = FRAME_X_RADIUS * LEFT;
        public static readonly Vector<double> RIGHT_SIDE = FRAME_X_RADIUS * RIGHT;

        //public const double PI = System.Math.PI; // Removed because of type clashes
        public const double TAU = 2 * System.Math.PI;
        public const double DEGREES = TAU / 360;

        public const string FFMPEG_BIN = "ffmpeg";

        public static readonly ReadOnlyDictionary<Colors, Color> COLOR_MAP = COLORS;
        public static readonly ReadOnlyDictionary<Colors, Color> COLORS = new ReadOnlyDictionary<Colors, Color>(
            new Dictionary<Colors, Color>() {
                { Colors.DARK_BLUE, new Color("#236B8E") },
                { Colors.DARK_BROWN, new Color("#8B4513") },
                { Colors.LIGHT_BROWN, new Color("#CD853F") },
                { Colors.BLUE_E, new Color("#1C758A") },
                { Colors.BLUE_D, new Color("#29ABCA") },
                { Colors.BLUE_C, new Color("#58C4DD") },
                { Colors.BLUE_B, new Color("#9CDCEB") },
                { Colors.BLUE_A, new Color("#C7E9F1") },
                { Colors.TEAL_E, new Color("#49A88F") },
                { Colors.TEAL_D, new Color("#55C1A7") },
                { Colors.TEAL_C, new Color("#5CD0B3") },
                { Colors.TEAL_B, new Color("#76DDC0") },
                { Colors.TEAL_A, new Color("#ACEAD7") },
                { Colors.GREEN_E, new Color("#699C52") },
                { Colors.GREEN_D, new Color("#77B05D") },
                { Colors.GREEN_C, new Color("#83C167") },
                { Colors.GREEN_B, new Color("#A6CF8C") },
                { Colors.GREEN_A, new Color("#C9E2AE") },
                { Colors.YELLOW_E, new Color("#E8C11C") },
                { Colors.YELLOW_D, new Color("#F4D345") },
                { Colors.YELLOW_C, new Color("#FFFF00") },
                { Colors.YELLOW_B, new Color("#FFEA94") },
                { Colors.YELLOW_A, new Color("#FFF1B6") },
                { Colors.GOLD_E, new Color("#C78D46") },
                { Colors.GOLD_D, new Color("#E1A158") },
                { Colors.GOLD_C, new Color("#F0AC5F") },
                { Colors.GOLD_B, new Color("#F9B775") },
                { Colors.GOLD_A, new Color("#F7C797") },
                { Colors.RED_E, new Color("#CF5044") },
                { Colors.RED_D, new Color("#E65A4C") },
                { Colors.RED_C, new Color("#FC6255") },
                { Colors.RED_B, new Color("#FF8080") },
                { Colors.RED_A, new Color("#F7A1A3") },
                { Colors.MAROON_E, new Color("#94424F") },
                { Colors.MAROON_D, new Color("#A24D61") },
                { Colors.MAROON_C, new Color("#C55F73") },
                { Colors.MAROON_B, new Color("#EC92AB") },
                { Colors.MAROON_A, new Color("#ECABC1") },
                { Colors.PURPLE_E, new Color("#644172") },
                { Colors.PURPLE_D, new Color("#715582") },
                { Colors.PURPLE_C, new Color("#9A72AC") },
                { Colors.PURPLE_B, new Color("#B189C6") },
                { Colors.PURPLE_A, new Color("#CAA3E8") },
                { Colors.WHITE, new Color("#FFFFFF") },
                { Colors.BLACK, new Color("#000000") },
                { Colors.LIGHT_GRAY, new Color("#BBBBBB") },
                { Colors.LIGHT_GREY, new Color("#BBBBBB") },
                { Colors.GRAY, new Color("#888888") },
                { Colors.GREY, new Color("#888888") },
                { Colors.DARK_GREY, new Color("#444444") },
                { Colors.DARK_GRAY, new Color("#444444") },
                { Colors.DARKER_GREY, new Color("#222222") },
                { Colors.DARKER_GRAY, new Color("#222222") },
                { Colors.GREY_BROWN, new Color("#736357") },
                { Colors.PINK, new Color("#D147BD") },
                { Colors.LIGHT_PINK, new Color("#DC75CD") },
                { Colors.GREEN_SCREEN, new Color("#00FF00") },
                { Colors.ORANGE, new Color("#FF862F") },
            }
        );
        public enum Colors
        {
            DARK_BLUE,
            DARK_BROWN,
            LIGHT_BROWN,
            BLUE_E,
            BLUE_D,
            BLUE_C,
            BLUE_B,
            BLUE_A,
            TEAL_E,
            TEAL_D,
            TEAL_C,
            TEAL_B,
            TEAL_A,
            GREEN_E,
            GREEN_D,
            GREEN_C,
            GREEN_B,
            GREEN_A,
            YELLOW_E,
            YELLOW_D,
            YELLOW_C,
            YELLOW_B,
            YELLOW_A,
            GOLD_E,
            GOLD_D,
            GOLD_C,
            GOLD_B,
            GOLD_A,
            RED_E,
            RED_D,
            RED_C,
            RED_B,
            RED_A,
            MAROON_E,
            MAROON_D,
            MAROON_C,
            MAROON_B,
            MAROON_A,
            PURPLE_E,
            PURPLE_D,
            PURPLE_C,
            PURPLE_B,
            PURPLE_A,
            WHITE,
            BLACK,
            LIGHT_GRAY,
            LIGHT_GREY,
            GRAY,
            GREY,
            DARK_GREY,
            DARK_GRAY,
            DARKER_GREY,
            DARKER_GRAY,
            GREY_BROWN,
            PINK,
            LIGHT_PINK,
            GREEN_SCREEN,
            ORANGE,
        }
    }
}
