using SharpDX;
using System;

namespace ConsoleApplication1
{
    public class HitData
    {
        public HitData() { }
        public HitData(float x, float y, float z, Vector3 direction)
        {
            P0 = new Point3D(x, y, z);
            PD = direction;
        }
        public Point3D P0 { get; set; }
        public Vector3 PD { get; set; }
    }

    public class HitResult
    {
        public Lit Lit { get; set; }
        public double Distance { get; set; }
        public HitResult() { }
        public HitResult(Lit lit, float tx, float ty, float tz, HitData data)
        {
            this.Lit = lit;
            this.Distance = Math.Sqrt(Math.Pow(data.P0.X - tx, 2) + Math.Pow(data.P0.Y - ty, 2) + Math.Pow(data.P0.Z - tz, 2));
        }
    }
}
