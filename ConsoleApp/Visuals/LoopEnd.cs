using SharpDX;
using SharpDX.Direct2D1;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class LoopEnd : Viz
    {

        public override string DebuggerDisplay => $"LupEnd: {base.DebuggerDisplay}";

        private static LoopEndRes resources;

        static LoopEnd()
        {
            resources = new LoopEndRes();
        }

        public LoopEnd()
        {
        }

        public override VizFeatures Features => VizFeatures.IsSelectable | VizFeatures.IsDraggable | VizFeatures.HasHilites;

        public override void ResetPoints(Song song)
        {
            if (song.LoopBeginPx > song.LoopEndPx)
            {
                song.LoopEndPx = song.LoopBeginPx;
            }
            StartPoint = new Point(song.LoopEndPx, Global.Ruler_Y + 1);
            EndPoint = new Point(StartPoint.X, Global.Instance.Height - Global.Slider_Height);
        }

        public override void Draw(DrawData dd)
        {
            draw(dd, resources.LoopEndBrush);
        }

        public override void DrawHilites(DrawData dd)
        {
            draw(dd, resources.HiliteBrush);
        }

        public override void DrawSelect(DrawData dd, bool primary)
        {
            draw(dd, resources.Selected_PrimaryBrush);
        }

        private void draw(DrawData dd, Brush brush)
        {
            //var savTrans = dd.target.Transform;
            //dd.target.Transform = Matrix3x2.Identity;

            //dd.target.DrawLine(new Vector2(StartPoint.X, dd.Height + 1), new Vector2(StartPoint.X, dd.Height + Global.Slider_Height - 2), brush);

            //dd.target.Transform = savTrans;

            if (StartPoint.X < dd.LFT) return;
            if (StartPoint.X > dd.RIT) return;

            dd.target.DrawLine(StartPoint, EndPoint, brush);
        }

    }
}
