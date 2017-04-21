using SharpDX;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class LyricBg : Viz
	{
        public override string DebuggerDisplay => $"LyrBg: {base.DebuggerDisplay}";

        public override VizFeatures Features => VizFeatures.HasContextMenu;

        public override IEnumerable<CntxtMenuItem> GetContentMenuItems()
        {
            bool mayPaste = ((Global.Instance.Song.Copied.Count > 0) && (Global.Instance.Song.Copied[0] is Lyric));
            return new List<CntxtMenuItem>(2)
            {
                new CntxtMenuItem("Insert...", cmInsert),
                new CntxtMenuItem("Paste", cmPaste, mayPaste),
            };
        }

        private static IEnumerable<VizCmd> cmPaste(Viz viz, System.Drawing.Point MouseLocation)
        {
            Song song = Global.Instance.Song;
            Viz frst = song.Copied[0];
            int newBeat = song.Beat(MouseLocation.X + Global.Lft);
            int oldBeat =  song.Beat(frst.StartPoint.X);
            int offset = newBeat - oldBeat;

            List<VizCmd> cmds = new List<VizCmd>(song.Copied.Count);
            foreach (var src in song.Copied)
            {
                Lyric dst = (Lyric)src.Clone();
                int beat = song.Beat(dst.StartPoint.X);
                float newTime = dst.StartPoint.X + (Global.Instance.Song.TimePoints.Keys[beat + offset] - Global.Instance.Song.TimePoints.Keys[beat]);
                dst.timemark = Global.Instance.Song.NormalizedTimeMark((int)newTime);
                cmds.Add(new VizCmd(dst, Cmd.Insert));
            }
            return cmds;
        }

        private static IEnumerable<VizCmd> cmInsert(Viz viz, System.Drawing.Point MouseLocation)
        {
            TimeMark tm = Global.Instance.Song.NormalizedTimeMark(Global.Lft + MouseLocation.X);
            return Lyric.AddDialog("", tm);
        }

        //switch (keyData)
        //{
        //    case Keys.Insert:
        //        //return new VizCmd(this, Cmd.Insert);
        //        throw new Exception("Insert not implemented.");

        //    case Keys.Delete:
        //    case Keys.X | Keys.Control:
        //        return new VizCmd(this, Cmd.Delete);

        //    case Keys.C | Keys.Control:
        //        //return new VizCmd(this, Cmd.Copy);
        //        throw new Exception("Insert not implemented.");

        //    case Keys.V | Keys.Control:
        //        //return new VizCmd(this, Cmd.Paste);
        //        throw new Exception("Insert not implemented.");

        //}

        public override VizCmd TranslateToCmd(ref Message msg, Keys keyData) => null;

        public override void ShowDialog(Song Song)
        {
        }

        public override void ResetPoints(Song song)
		{
			StartPoint = new Point(song.MusicBeginPx, Global.Lyric_Y);
			EndPoint = new Point(song.MusicEndPx, Global.Lyric_Y + Global.Lyric_Height);
		}

        public override void Draw(DrawData dd)
		{
		}
	}
}
