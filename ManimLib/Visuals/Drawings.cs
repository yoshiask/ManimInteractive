using System;
using System.Collections.Generic;
using System.Linq;
using static ManimLib.Constants;

namespace ManimLib.Visuals
{
    public static class Drawings
    {
        private static IList<string> _drawings;
        public static IList<string> ManimDrawings {
            get {
                if (_drawings == null)// &&  != "")
                {
                    _drawings = new List<string>();
                    string path = System.IO.Path.Combine(@"C:\Users\jjask\Documents\Manim Archive\manim 4-7-20\manimlib", @"mobject\svg\drawings.py");
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
}
