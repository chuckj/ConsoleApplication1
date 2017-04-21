using SharpDX;
using System;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Rule : Viz
	{
        public override string DebuggerDisplay => $"Rule";

        private static RuleRes resources;

        static Rule()
        {
            resources = new RuleRes();
        }

        public override void ResetPoints(Song song)
        {
        }

        public override void DrawMove(DrawData dd)
        {

        }

        public override void Draw(DrawData dd)
		{
			dd.target.DrawLine(new Point(dd.LFT, Global.Ruler_Y), new Point(dd.RIT, Global.Ruler_Y), resources.Ruler_LineBrush);

            int lft = Math.Max(dd.Song.MusicBeginPx, dd.LFT);
            int rit = Math.Min(dd.Song.MusicEndPx, dd.RIT);
			dd.target.DrawLine(new Point(lft, Global.Ruler_Y - 1), new Point(rit, Global.Ruler_Y - 1), resources.Ruler_LineBrush);
			dd.target.DrawLine(new Point(lft, Global.Ruler_Y + 1), new Point(rit, Global.Ruler_Y + 1), resources.Ruler_LineBrush);

			for (int s = 1; s <= (int)dd.Song.TrackTime; s++)
			{
				var x = s * Global.pxpersec;
                if (x > dd.RIT) break;
                if ((x + 120) < dd.LFT) continue;

				dd.target.DrawLine(new Point(x, Global.Ruler_Y - Global.Ruler_majorTick), new Point(x, Global.Ruler_Y), resources.Ruler_MinuteBrush, 1, resources.Ruler_MeasureStrokeStyle);
				dd.target.DrawText($"{s/60}:{(s%60).ToString("00")}", resources.Ruler_TextFormat, new RectangleF(x + 2, Global.Ruler_Y - 14, 120, 20), resources.Ruler_SecondsBrush);
			}
		}
	}
}
