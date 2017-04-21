using SharpDX;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class SliderThumb : Viz
    {
        private static SliderThumbRes resources;

        static SliderThumb()
        {
            resources = new SliderThumbRes();
        }

        public override string DebuggerDisplay => $"SliderThumb: {base.DebuggerDisplay}";


		public SliderThumb()
		{
		}

        public override VizFeatures Features => VizFeatures.None;

        public override void ResetPoints(Song song)
		{
        }

        public override void Draw(DrawData dd)
		{
            //  save transform and install identity

            var savTrans = dd.target.Transform;
            dd.target.Transform = Matrix3x2.Identity;

            //  draw slider and thumb
            int ht = Global.Slider_Height;
            int top = dd.Height;
            var scale = (float)dd.Width / dd.Song.TrackPx;

            var pt = ((float)dd.LFT) * scale;
            var pt1 = ((float)dd.RIT) * scale;
            dd.target.FillRectangle(new RectangleF(pt, top + 2, pt1 - pt, ht - 4), resources.SliderThumb_ThumbFillBrush);
            dd.target.DrawRectangle(new RectangleF(pt, top + 2, pt1 - pt, ht - 4), resources.SliderThumb_ThumbBrush);

            //  restore transform

            dd.target.Transform = savTrans;
        }
    }
}
