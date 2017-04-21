using System.Diagnostics;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Drag : Viz
    {
        private static DragRes resources;

        static Drag()
        {
            resources = new DragRes();
        }

        public override string DebuggerDisplay => $"Drag: {base.DebuggerDisplay}";

        public Drag()
		{
		}

        public override VizFeatures Features => VizFeatures.None;

        public override void ResetPoints(Song song)
        {
        }

        public DragMode DragMode { get; set; }

        public override void Draw(DrawData dd)
		{
            Song song = Global.Instance.Song;

            if ((song != null) && (song.DragMode == DragMode.Active))
            {
                dd.target.DrawRectangle(Rectangle, resources.Drag_DragBrush, 1, resources.Drag_StrokeStyle); //, Math.Min(dragBegin.X, dragEnd.X), Math.Min(dragBegin.Y, dragEnd.Y), Math.Abs(dragBegin.X - dragEnd.X), Math.Abs(dragBegin.Y - dragEnd.Y));
            }
        }
	}
}
