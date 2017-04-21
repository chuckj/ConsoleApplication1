using System;

namespace ConsoleApplication1
{
    public class GECELit : RGBLit
    {
        public GECELit(string nm) : base(nm) { }


        public override HitResult Hit(HitData data)
        {
            var t = (Pt.Z - data.P0.Z) / data.PD.Z;
            if (t < 0) return null;
            var tx = data.P0.X + data.PD.X * t;
            var ty = data.P0.Y + data.PD.Y * t;
            var d = Math.Pow(Pt.X - tx, 2) + Math.Pow(Pt.Y - ty, 2);
            if (d > 3) return null;
            return new HitResult(this, tx, ty, Pt.Z, data);
        }
    }
}
