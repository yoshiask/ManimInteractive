using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;

namespace ManimPPT
{
    public partial class ManimRibbon
    {
        private void ManimTab_Load(object sender, RibbonUIEventArgs e)
        {
            //GenerateButton.Click += GenerateButton_Click;
        }

        private void GenerateButton_Click(object sender, RibbonControlEventArgs e)
        {
            Globals.ThisAddIn.Application.ShowStartupDialog = Office.MsoTriState.msoTrue;

            string output = "";
            var console = Globals.ThisAddIn.Application.Presentations[0].Slides[0].Shapes.AddPlaceholder(PpPlaceholderType.ppPlaceholderSubtitle);
            /*for (int i = 0; i<Globals.ThisAddIn.Application.CurShapes.Count; i++)
            {
                var shape = ThisAddIn.CurSlide.Shapes[i];
                shape.Name = "ManimShape";
                output += shape.Type.ToString();
            }*/
            console.Name = output;
        }
    }
}
