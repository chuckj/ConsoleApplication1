using SharpDX;
using SharpDX.Direct2D1;

namespace ConsoleApplication1
{
    public class WaveRes : VizRes
    {
        public WaveRes() : base()
        {
        }

        public Brush Wave_GreenBrush;
        public Brush Wave_RedBrush;
        public Vector2[] pointsLeft;
        public Vector2[] pointsRight;
        public SharpDX.Direct2D1.Bitmap[] Wave_Bmps;

        //public override void DevIndepAcquire()
        //{
        //}

        //public override void DevIndepRelease()
        //{
        //}

        public override void DevDepAcquire(RenderTarget target)
        {
            Wave_GreenBrush = SolidColorBrush(target, Global.Wave_GreenColor);
            Wave_RedBrush = SolidColorBrush(target, Global.Wave_RedColor);
            pointsLeft = new Vector2[(int)target.Size.Width];
            pointsRight = new Vector2[(int)target.Size.Width];
            int bmps = (Global.Instance.Song.TrackPx + Global.pxpersec - 1) / Global.pxpersec;
            Wave_Bmps = new SharpDX.Direct2D1.Bitmap[bmps];
        }

        public override void DevDepRelease()
        {
            Wave_GreenBrush = null;
            Wave_RedBrush = null;
            pointsLeft = null;
            pointsRight = null;
            if (Wave_Bmps != null)
            {
                for (int ndx = 0; ndx < Wave_Bmps.Length; ndx++)
                {
                    if (Wave_Bmps[ndx] != null)
                    {
                        Wave_Bmps[ndx].Dispose();
                        Wave_Bmps[ndx] = null;
                    }
                }
            }
        }
    }
}
