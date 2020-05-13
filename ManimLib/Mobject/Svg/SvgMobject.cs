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
        public bool UnpackGroups { get; set; } = true; // If false, creates a hierarchy of VGroups
        public double StrokeWidth { get; set; } = DEFAULT_STROKE_WIDTH;
        public double FillOpacity { get; set; } = 1.0;
        //public Color FillColor { get; set; } = COLORS[Colors.LIGHT_GREY];
        #endregion

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
            List<Mobject> mobjects = new List<Mobject>();
            foreach (SvgPath path in doc.Children.FindSvgElementsOf<SvgPath>())
            {
                // TODO: This should really be an SvgMobject
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
            return mobjects;
        }


    }
}
