using ManimLib.Visuals;
using RL;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManimLib.Rendering
{
    public class Camera
    {
        public string BackgroundImage { get; set; }
        public Math.Rect FrameSize { get; set; }
        public Math.Rect PixelSize { get; set; }
        public int Framerate { get; set; }
        public Color BackgroundColor { get; set; }
    }

    public class CameraConfig
    {
        public static CameraConfig ActiveCameraConfig = Production;
        public static readonly CameraConfig Production = new CameraConfig("Production", 2560, 1440, 1 / 60, "1440p60");
        public static readonly CameraConfig High = new CameraConfig("High", 1920, 1080, 1 / 60, "1080p60");
        public static readonly CameraConfig Medium = new CameraConfig("Medium", 1280, 720, 1 / 30, "720p30");
        public static readonly CameraConfig Low = new CameraConfig("Low", 854, 480, 1 / 15, "480p15");

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
}
