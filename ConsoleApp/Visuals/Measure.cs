using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using System.Diagnostics;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Measure : Viz
	{
        public override string DebuggerDisplay => $"Meas {StartingMeasure}: {base.DebuggerDisplay} bpm:{BeatsPerMeasure}";

        private static MeasureRes resources;

        static Measure()
        {
            resources = new MeasureRes();
        }

        public short StartingMeasure;
		public short BeatsPerMeasure;
		public float StartingTime;
		public float TimePerBeat; 
		public new bool IsKeyFrame;

        private int fixedSize;
        private string text;

		public Measure(int startingMeasure, int beatsPerMeasure, float startingTime, float timePerBeat, bool isKeyFrame)
		{
			StartingMeasure = (short)startingMeasure;
			BeatsPerMeasure = (short)beatsPerMeasure;
			StartingTime = startingTime;
			TimePerBeat = timePerBeat;
			IsKeyFrame = isKeyFrame;

            text = $"m{StartingMeasure.ToString()}";

            using (TextLayout tl = new TextLayout(Global.Instance.factoryWrite, text, resources.Measure_TextFormat, 100, 15, 1, false))
            {
                fixedSize = (int)(tl.Metrics.Width + 5);
            }
        }

        public override VizFeatures Features => VizFeatures.IsSelectable | VizFeatures.IsDraggable | VizFeatures.HasHilites | (IsKeyFrame ? VizFeatures.IsKeyFrame : VizFeatures.None);

        public override void ResetPoints(Song song)
		{
			StartPoint = new Point((int)(StartingTime * Global.pxpersec), Global.Ruler_Y);
			EndPoint = new Point(StartPoint.X, Global.Instance.Height - Global.Slider_Height);
            song.AddTimePoints(StartingMeasure, 1, StartingTime, TimePerBeat);
        }

        public override void Draw(DrawData dd)
		{
            if (StartPoint.X > dd.RIT) return;
            if ((StartPoint.X + 100) < dd.LFT) return;

			if (StartingMeasure == 0) return;

			var brush = resources.Measure_NormalBrush;
			if (StartingMeasure == 1)
				brush = resources.Measure_GreenBrush;
			else if (StartingMeasure == dd.Song.MeasureCount)
				brush = resources.Measure_RedBrush;
			
            //	draw measure
			dd.target.DrawText(text, resources.Measure_TextFormat, new RectangleF(StartPoint.X - fixedSize + 2, Global.Ruler_Y + 2, 100, 20), resources.Measure_FontBrush);


            draw(dd, brush);
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
			dd.target.DrawLine(StartPoint, EndPoint, brush);
            if (IsKeyFrame)
            {
                float xbgn = StartPoint.X + 7;
                float ybgn = Global.Ruler_Y + 3;
                dd.target.DrawEllipse(new Ellipse(new Vector2(xbgn - 3, ybgn + 1), 2, 2), brush);
                dd.target.DrawLine(new Vector2(xbgn - 3, ybgn + 3), new Vector2(xbgn - 3, ybgn + 3 + 5), brush);
                dd.target.DrawLine(new Vector2(xbgn - 5, ybgn + 3), new Vector2(xbgn - 3, ybgn + 3), brush);
                dd.target.DrawLine(new Vector2(xbgn - 5, ybgn + 5), new Vector2(xbgn - 3, ybgn + 5), brush);
            }
        }

        public override XElement Serialize(Song song) => new XElement("keyframe",
    new XAttribute("measure", StartingMeasure),
    BeatsPerMeasure != 0 ? new XAttribute("beatspermeasure", BeatsPerMeasure) : null,
    new XAttribute("time", StartingTime));
    }
}
