using ManimLib.Visuals;
using RL;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManimLib.Rendering
{
    public class Renderer
    {
        /// TODO: This class should be able to take in an instance of a scene. It will have
        /// functions for stepping to the next animation, along with other information for
        /// rendering, including the video quality and save PNGs.

        public Scene Scene { get; set; }

        public Renderer(Scene scene)
        {

        }
    }

    public class ExportOptions
    {
        public bool Preview = false;
        public bool LowQuality = false;
        public bool MediumQuality = false;
        public bool HideProgress = false;
        public bool SkipToLastFrame = false;
        public bool SavePNG = false;
        public bool UseTransparency = false;
        public int StartAtAnimation = 0;
        public Color BackgroundColor = Constants.COLORS[Constants.Colors.BLACK];
        public CameraConfig Camera = CameraConfig.Production;
    }
}
