using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct Point3D
    {
        private float x;
        private float y;
        private float z;

        public Point3D(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public float X => x;
        public float Y => y;
        public float Z => z;

        public static explicit operator Point3D(XAttribute attrb)
        {
            string data = (string)attrb;
            string[] parts = data.Split(',');
            float x, y, z;
            if (!float.TryParse(parts[0], out x))
                throw new FormatException("x");
            if (!float.TryParse(parts[1], out y))
                throw new FormatException("y");
            if (!float.TryParse(parts[2], out z))
                throw new FormatException("z");
            return new Point3D(x, y, z);
        }

        public static Point3D operator *(Point3D pt, float scale) => new Point3D(pt.x * scale, pt.y * scale, pt.z * scale);
        public static Point3D operator /(Point3D pt, float scale) => new Point3D(pt.x / scale, pt.y / scale, pt.z / scale);
        public static Point3D operator +(Point3D pt1, Point3D pt2) => new Point3D(pt1.x + pt2.x, pt1.y + pt2.y, pt1.z + pt2.z);
        public static Point3D operator -(Point3D pt1, Point3D pt2) => new Point3D(pt1.x - pt2.x, pt1.y - pt2.y, pt1.z - pt2.z);

        public string DebuggerDisplay => $"({x},{y},{z})";
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct ColorPoint3D
    {
        private Point3D pt;
        private Clr clr;

        public ColorPoint3D(float x, float y, float z, Clr clr)
        {
            this.pt = new Point3D(x, y, z);
            this.clr = clr; 
        }

        public ColorPoint3D(Point3D pt, Clr clr)
        {
            this.pt = pt;
            this.clr = clr;
        }
        public float X => pt.X;
        public float Y => pt.Y;
        public float Z => pt.Z;
        public Clr Clr => clr;

        public string DebuggerDisplay => $"({pt.X},{pt.Y},{pt.Z}):{clr.Name}";

    }

    public struct IndexPoint3D
    {
        private Point3D pt;
        private int ndx;

        public IndexPoint3D(float x, float y, float z, int ndx)
        {
            this.pt = new Point3D(x, y, z);
            this.ndx = ndx;
        }

        public IndexPoint3D(Point3D pt, int ndx)
        {
            this.pt = pt;
            this.ndx = ndx;
        }
        public float X => pt.X;
        public float Y => pt.Y;
        public float Z => pt.Z;
        public int Ndx => ndx;

        public string DebuggerDisplay => $"({pt.X},{pt.Y},{pt.Z}):{ndx}";

    }
}
