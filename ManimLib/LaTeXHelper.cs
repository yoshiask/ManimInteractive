using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManimLib
{
    public static class LaTeXHelper
    {
        //public static readonly FontFamily DefaultFont = new FontFamily("BKM-cmr17");
        public static readonly Dictionary<string, string> Colors = new Dictionary<string, string>()
        {
            { "white", "#f8f9fa" },

        };

        /// <summary>
        /// Escapes a LaTeX string for use in Python
        /// </summary>
        /// <param name="latex">LaTeX string to escape</param>
        public static string EscapeLaTeX(string latex)
        {
            string esc = "";
            foreach (char ch in latex.ToArray())
            {
                if (ch == '\\')
                    esc += @"\\";
                else
                    esc += ch;
            }
            return esc;
        }
    }
}
