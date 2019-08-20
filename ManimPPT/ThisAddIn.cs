using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;

namespace ManimPPT
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Application.PresentationNewSlide += Application_PresentationNewSlide;
        }

        private void Application_PresentationNewSlide(PowerPoint.Slide Sld)
        {
            var rectangle = Sld.Shapes.AddShape(Office.MsoAutoShapeType.msoShapeRectangle,
                0, 0, 200, 200);
            rectangle.Name = "HelloWorld";
            Sld.TimeLine.MainSequence.AddEffect(rectangle, MsoAnimEffect.msoAnimEffectWipe);
            //Application.OpenThemeFile(@"C:\Users\jjask\Documents\Manim PPT Theme.xml");

            Application.Presentations[0].NewWindow();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
