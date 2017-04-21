using SharpDX;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Beat : Viz
    {
        private static BeatRes resources;

        static Beat()
        {
            resources = new BeatRes();
        }

        public override string DebuggerDisplay => $"Beat {StartingMeasure}m{Beats}: {base.DebuggerDisplay}";

        public short StartingMeasure;
		public short Beats;
        private float StartingTime;
        private float TimePerBeat;

		public Beat(int startingMeasure, int beats, float startingTime, float timePerBeat)
		{
			StartingMeasure = (short)startingMeasure;
			Beats = (short)beats;
			StartingTime = startingTime;
            TimePerBeat = timePerBeat;
		}

        public override VizFeatures Features => VizFeatures.None;

        public override void ResetPoints(Song song)
		{
			StartPoint = new Point((int)(StartingTime * Global.pxpersec), Global.Ruler_Y);
			EndPoint = new Point(StartPoint.X, Global.Instance.Height - Global.Slider_Height);
            song.AddTimePoints(StartingMeasure, Beats, StartingTime, TimePerBeat);
        }

        public override void Draw(DrawData dd)
		{
            if (StartPoint.X < dd.LFT) return;
            if (StartPoint.X > dd.RIT) return;

			dd.target.DrawLine(StartPoint, EndPoint, resources.Beat_BeatBrush, 1, resources.Beat_StrokeStyle);
		}
	}
}
