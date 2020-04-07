using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ManimLib.Math
{
    public struct Point : IPoint
    {
        public decimal X { get; set; }
        public decimal Y { get; set; }

        public Point(int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }
        public Point(double x = 0, double y = 0)
        {
            X = (decimal)x;
            Y = (decimal)y;
        }
        public Point(float x = 0, float y = 0)
        {
            X = (decimal)x;
            Y = (decimal)y;
        }
        public Point(decimal x = 0, decimal y = 0)
        {
            X = x;
            Y = y;
        }
        public Point(Vector2 v)
        {
            X = (decimal)v.X;
            Y = (decimal)v.Y;
        }

        public static Point operator +(Point p, Vector2 v)
        {
            return new Point(p.X + (decimal)v.X, p.Y + (decimal)v.Y);
        }

        public static Point operator -(Point p, Vector2 v)
        {
            return new Point(p.X - (decimal)v.X, p.Y - (decimal)v.Y);
        }

        public static Point operator *(Point p, decimal s)
        {
            return new Point(p.X * s, p.Y * s);
        }
        public static Point operator *(Point p, double s)
        {
            return new Point(p.X * (decimal)s, p.Y * (decimal)s);
        }
        public static Point operator *(Point p, float s)
        {
            return new Point(p.X * (decimal)s, p.Y * (decimal)s);
        }

        public static bool operator ==(Point a, Point b)
        {
            return (a.X == b.X) && (a.Y == b.Y);
        }
        public static bool operator !=(Point a, Point b)
        {
            return (a.X != b.X) || (a.Y != b.Y);
        }

        public static Point[] Multiply(Point[] points, double s)
        {
            Point[] newPoints = new Point[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                newPoints[i] = points[i] * s;
            }
            return newPoints;
        }
    }

    public interface IPoint
    {
        decimal X { get; set; }
        decimal Y { get; set; }
    }
}
