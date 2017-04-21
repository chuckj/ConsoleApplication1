using System;

namespace ConsoleApplication1
{
    public class DMXLit : RGBLit
    {
        public DMXLit(string nm) : base(nm) {
        }

        private float minx = float.MaxValue, miny = float.MaxValue, minz = float.MaxValue;
        private float maxx = float.MinValue, maxy = float.MinValue, maxz = float.MinValue;

        public void AddPoint(Point3D pt)
        {
            minx = Math.Min(minx, pt.X);
            miny = Math.Min(miny, pt.Y);
            minz = Math.Min(minz, pt.Z);
            maxx = Math.Max(maxx, pt.X);
            maxy = Math.Max(maxy, pt.Y);
            maxz = Math.Max(maxz, pt.Z);
        }
        public override HitResult Hit(HitData data)
        {
            if (minx == maxx)
            {
                var t = (minx - data.P0.X) / data.PD.X;
                if (t < 0) return null;
                var tz = data.P0.Z + data.PD.Z * t;
                var ty = data.P0.Y + data.PD.Y * t;
                var d = (tz >= minz) && (ty >= miny) && (tz <= maxz) && (ty <= maxy);
                if (!d) return null;
                return new HitResult(this, minx, ty, tz, data);

            }
            else
            {
                var tz = (minz + maxz) / 2;
                var t = (tz - data.P0.Z) / data.PD.Z;
                if (t < 0) return null;
                var tx = data.P0.X + data.PD.X * t;
                var ty = data.P0.Y + data.PD.Y * t;
                var d = (tx >= minx) && (ty >= miny) && (tx <= maxx) && (ty <= maxy);
                if (!d) return null;
                return new HitResult(this, tx, ty, tz, data);
            }
        }
    }
}
