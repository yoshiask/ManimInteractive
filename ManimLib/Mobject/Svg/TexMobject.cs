using Color = RL.Color;
using System;
using System.Collections.Generic;
using System.Text;
using static ManimLib.Constants;
using ManimLib.Mobject.Types;

namespace ManimLib.Mobject.Svg
{
    public class TexSymbol
    {
        
    }

    public class SingleStringTexMobject : SvgMobject
    {
        #region Properties
        public const double TEX_MOB_SCALE_FACTOR = 0.05;

        public string TemplateTexFileBody { get; set; } = GetTemplateTexFileBody();
        public double StrokeWidth { get; set; } = 0;
        public double FillOpacity { get; set; } = 1.0;
        public double BackgroundStrokeWidth { get; set; } = 1.0;
        public Color BackgroundStrokeColor { get; set; } = COLORS[Colors.BLACK];
        public bool ShouldCenter { get; set; } = true;
        public double Height { get; set; } = 0;
        public bool OrganizeLeftToRight { get; set; } = false;
        public string Alignment { get; set; } = "";

        public string TexString { get; set; }
        #endregion

        public SingleStringTexMobject(string texString)
        {
            TexString = texString;
            base.FileName = TexToSvgFile(
                GetModifiedExpression(texString),
                TemplateTexFileBody
            );
            EnsureValidFile();
            MoveIntoPosition();
            if (Height == 0)
                Scale(TEX_MOB_SCALE_FACTOR);
            if (OrganizeLeftToRight)
                OrganizeSubmobjectsLeftToRight();
        }
    }
}
