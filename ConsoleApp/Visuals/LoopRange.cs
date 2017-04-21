using SharpDX;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class LoopRange : Viz
    {
        public override string DebuggerDisplay => $"LupRng: {base.DebuggerDisplay}";

        private static LoopRangeRes resources;

        static LoopRange()
        {
            resources = new LoopRangeRes();
        }

        public LoopRange()
        {
        }

        public override VizFeatures Features => VizFeatures.IsSelectable | VizFeatures.IsDraggable | VizFeatures.HasHilites;

        public override void ResetPoints(Song song)
        {
            StartPoint = new Point(song.LoopBeginPx, Global.Ruler_Y + 1);
            EndPoint = new Point(song.LoopEndPx, Global.Instance.Height - Global.Slider_Height);
        }

        public override void Draw(DrawData dd)
        {
            if (StartPoint.X < dd.LFT) return;
            if (StartPoint.X > dd.RIT) return;

            //	draw measure
            dd.target.FillRectangle(Rectangle, resources.LoopRangeBrush);
        }

        public override void DrawHilites(DrawData dd)
        {
            if (StartPoint.X < dd.LFT) return;
            if (StartPoint.X > dd.RIT) return;

            dd.target.FillRectangle(Rectangle, resources.RangeBrush);
        }

        public override void DrawSelect(DrawData dd, bool primary)
        {
            if (StartPoint.X < dd.LFT) return;
            if (StartPoint.X > dd.RIT) return;

            dd.target.DrawRectangle(Rectangle, resources.Selected_PrimaryBrush);
        }
    }
}
