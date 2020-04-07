using System;
using System.Collections.Generic;
using System.Text;

namespace ManimLib.Visuals
{
    public class Color
    {
        byte A, R, G, B;

        public Color(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }
        public Color(byte r, byte g, byte b)
        {
            A = 0xFF;
            R = r;
            G = g;
            B = b;
        }
        public Color(int a, int r, int g, int b)
        {
            A = Convert.ToByte(a);
            R = Convert.ToByte(r);
            G = Convert.ToByte(g);
            B = Convert.ToByte(b);
        }
        public Color(int r, int g, int b)
        {
            A = 0xFF;
            R = Convert.ToByte(r);
            G = Convert.ToByte(g);
            B = Convert.ToByte(b);
        }
        public Color(string hex)
        {
            A = 0xFF;
            if (hex.Length == 9)
            {
                // With alpha
                hex = hex.Substring(1);
                hex = hex.Substring(0, 2);
                A = Convert.ToByte(hex);
            }
            else if (hex.Length == 8)
            {
                // With alpha, no #
                A = Convert.ToByte(hex.Substring(0, 2));
            }
            else if (hex.Length == 7)
            {
                // No alpha
                hex = hex.Substring(1);
            }
            
            R = Convert.ToByte(hex.Substring(0, 2));
            G = Convert.ToByte(hex.Substring(2, 2));
            B = Convert.ToByte(hex.Substring(4, 2));
        }

        public string ToHex(bool withAlpha = true)
        {
            string output = "#";
            if (withAlpha)
                output += A.ToString("X");
            output += R.ToString("X");
            output += G.ToString("X");
            output += B.ToString("X");
            return output;
        }

        public override string ToString()
        {
            return ToHex();
        }
    }

    public static class Colors
    {
        public static readonly Color White = new Color(255, 255, 255);
        public static readonly Color Black = new Color(0, 0, 0);
    }
}
