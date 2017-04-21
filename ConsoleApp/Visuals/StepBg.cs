using SharpDX;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class StepBg : Viz
	{
        public override string DebuggerDisplay => $"StepBg: {base.DebuggerDisplay}";

        //private static StepBgRes resources;

        static StepBg()
        {
            //resources = new StepBgRes();
        }

        public override VizFeatures Features => VizFeatures.HasContextMenu | VizFeatures.HasProperties;

        public override IEnumerable<CntxtMenuItem> GetContentMenuItems() => new CntxtMenuItem[] {
                new CntxtMenuItem("Move", cmMove),
                new CntxtMenuItem("Insert", cmInsert) };

        private static IEnumerable<VizCmd> cmMove(Viz viz, System.Drawing.Point MouseLocation) => null;
        private static IEnumerable<VizCmd> cmInsert(Viz viz, System.Drawing.Point MouseLocation) => null;


        public override void ResetPoints(Song song)
		{
			StartPoint = new Point((int)song.MusicBeginPx, Global.Step_Y);
			EndPoint = new Point((int)song.TrackPx, Global.Instance.Height - Global.Slider_Height);
		}

        public override void Draw(DrawData dd)
		{
		}
	}
}
