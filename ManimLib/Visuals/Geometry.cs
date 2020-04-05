using System;
using System.Collections.Generic;
using System.Text;

namespace ManimLib.Visuals
{
    public struct Rect
    {
        private decimal Width, Height;

        #region Constructors
        public Rect (int width, int height)
        {
            Width = width;
            Height = height;
        }
        public Rect(double width, double height)
        {
            Width = (decimal)width;
            Height = (decimal)height;
        }
        public Rect(float width, float height)
        {
            Width = (decimal)width;
            Height = (decimal)height;
        }
        public Rect(decimal width, decimal height)
        {
            Width = width;
            Height = height;
        }

        public Rect(int x1, int y1, int x2, int y2)
        {
            Width = Math.Abs(x2 - x1);
            Height = Math.Abs(y2 - y1);
        }
        public Rect(double x1, double y1, double x2, double y2)
        {
            Width = (decimal)Math.Abs(x2 - x1);
            Height = (decimal)Math.Abs(y2 - y1);
        }
        public Rect(float x1, float y1, float x2, float y2)
        {
            Width = (decimal)Math.Abs(x2 - x1);
            Height = (decimal)Math.Abs(y2 - y1);
        }
        public Rect(decimal x1, decimal y1, decimal x2, decimal y2)
        {
            Width = Math.Abs(x2 - x1);
            Height = Math.Abs(y2 - y1);
        }
        #endregion

        public void SetWidth(int w)
        {
            Width = w;
        }
        public void SetWidth(float w)
        {
            Width = (decimal)w;
        }
        public void SetWidth(double w)
        {
            Width = (decimal)w;
        }
        public void SetWidth(decimal w)
        {
            Width = w;
        }
        public decimal GetWidth()
        {
            return Width;
        }

        public void SetHeight(int h)
        {
            Height = h;
        }
        public void SetHeight(float h)
        {
            Height = (decimal)h;
        }
        public void SetHeight(double h)
        {
            Height = (decimal)h;
        }
        public void SetHeight(decimal h)
        {
            Height = h;
        }
        public decimal GetHeight()
        {
            return Height;
        }
    }

    public struct Point
    {
        public decimal X, Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Point(double x, double y)
        {
            X = (decimal)x;
            Y = (decimal)y;
        }
        public Point(float x, float y)
        {
            X = (decimal)x;
            Y = (decimal)y;
        }
        public Point(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }
        public Point(Vector v)
        {
            X = v.X;
            Y = v.Y;
        }
    }

    public struct Vector
    {
        public decimal X, Y;

        public Vector(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Vector(double x, double y)
        {
            X = (decimal)x;
            Y = (decimal)y;
        }
        public Vector(float x, float y)
        {
            X = (decimal)x;
            Y = (decimal)y;
        }
        public Vector(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }
        public Vector(Point p)
        {
            X = p.X;
            Y = p.Y;
        }

        public void SetAngle(decimal theta)
        {
            SetAngle((double)theta);
        }
        public void SetAngle(double theta)
        {
            // Before doing anything, calculate the magnitude of the vector
            decimal magnitude = GetMagnitude();

            Y = (decimal)Math.Sin(theta) * magnitude;
            X = (decimal)Math.Cos(theta) * magnitude;
        }
        public void GetAngle(double theta)
        {
            Y = (decimal)Math.Sin(theta);
            X = (decimal)Math.Cos(theta);
        }

        public decimal GetMagnitude()
        {
            return (decimal)Math.Sqrt(Math.Pow((double)X, 2) + Math.Pow((double)Y, 2));
        }

        public string ToPythonVector()
        {
            string result = "";
            result += X.ToString();
            result += (X < 0) ? "*LEFT" : "*RIGHT";

            result += " + ";
            result += Y.ToString();
            result += (Y < 0) ? "*UP" : "*DOWN";

            return result;
        }
        public static string PointToPythonVector(Point p)
        {
            return new Vector(p).ToPythonVector();
        }
        public static string PointToPythonVector(decimal x, decimal y)
        {
            return new Vector(x, y).ToPythonVector();
        }
    }

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
