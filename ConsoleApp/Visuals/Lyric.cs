using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;
using ConsoleApplication1.Dialogs;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Direct2D1;

namespace ConsoleApplication1
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Lyric : Viz
	{
        [JsonIgnore]
        public override string DebuggerDisplay => $"Lyr: {timemark.ToString()}:{text}:{base.DebuggerDisplay}";

        private static LyricRes resources;

        static Lyric()
        {
            resources = new LyricRes();
        }

        public string text;
		public TimeMark timemark;
        public Size2 fixedSize;


        public Lyric(string text, TimeMark timemark)
        {
            this.text = text;
            this.timemark = timemark;

            using (TextLayout tl = new TextLayout(Global.Instance.factoryWrite, text, resources.Lyric_TextFormat, 100, 15, 1, false))
            {
                fixedSize = new Size2((int)(tl.Metrics.Width + 5), (int)tl.Metrics.Height);
            }
            EndPoint = StartPoint.Add(fixedSize);

            resetPoints();
		}

        [JsonConstructor]
        public Lyric()
        {
        }

        [JsonIgnore]
        public override VizFeatures Features => VizFeatures.IsSelectable | VizFeatures.IsMultiSelectable
                    | VizFeatures.IsFixedSize | VizFeatures.HasHilites | VizFeatures.HasContextMenu
                    | VizFeatures.CanInsertDelete;

        public override IEnumerable<CntxtMenuItem> GetContentMenuItems()
        {
            Song song = Global.Instance.Song;
            bool mayCopyDel = ((song.Selected.Count == 0) || (song.Selected.Contains(this)));
            bool mayPaste = ((song.Copied.Count > 0) && (song.Copied[0] is Lyric));
            return new List<CntxtMenuItem>(4)
            {
                new CntxtMenuItem("Copy", cmCopy, mayCopyDel),
                new CntxtMenuItem("Paste", cmPaste, mayPaste),
                new CntxtMenuItem("Duplicate", cmDuplicate, mayCopyDel),
                new CntxtMenuItem("Delete", cmDelete),
            };
        }


        private static IEnumerable<VizCmd> cmCopy(Viz viz, System.Drawing.Point MouseLocation) => Global.Instance.Song.cmCopy(viz);

        private static IEnumerable<VizCmd> cmPaste(Viz viz, System.Drawing.Point MouseLocation) => null;

        private static IEnumerable<VizCmd> cmDuplicate(Viz viz, System.Drawing.Point MouseLocation) => Global.Instance.Song.cmDuplicate(viz);

        private static IEnumerable<VizCmd> cmDelete(Viz viz, System.Drawing.Point MouseLocation) => Global.Instance.Song.cmDelete(viz);

        public static IEnumerable<VizCmd> AddDialog(string txt, TimeMark tm)
        {
            var dlg = new dlgProps();
            var txtTm = dlg.AddNewTextBox("Time:", tm.ToString());
            var txtTxt = dlg.AddNewTextBox("Text:", txt);

            dlg.MaximizeBox = false;
            dlg.MinimizeBox = false;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            var result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (Global.Instance.Song.TimeMarkTryParse(txtTm.Text, out tm))
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>()
                    {
                        {"type", typeof(Lyric) },
                        {"text", txtTxt.Text },
                        {"tm", tm }
                    };
                    return new[] { new VizCmd(null, Cmd.Insert, dict) };
                }
            }
            return null;
        }

        public void EditDialog(Song Song)
        {

        }

        public static PropDialogItem[] GetPropDataItems() => null;

        public override void ResetPoints(Song song)
        {
            resetPoints();
        }

        private void resetPoints()
        { 
			StartPoint = new Point((int)timemark.Time, Global.Lyric_Y);
			EndPoint = StartPoint.Add(fixedSize);
		}

        public override void Draw(DrawData dd)
		{
            Rectangle rect = Rectangle;

            if (rect.Left > dd.RIT) return;
            if (rect.Right < dd.LFT) return;

            //PointF[] wrk = new[] { StartPoint };
            //dd.Transform.TransformPoints(wrk);
            dd.target.DrawText(text, resources.Lyric_TextFormat, rect, resources.Lyric_FontBrush);
		}

		public override void DrawHilites(DrawData dd)
		{
            Rectangle rect = Rectangle;

            if (rect.Left > dd.RIT) return;
            if (rect.Right < dd.LFT) return;

            dd.target.DrawRectangle(rect, resources.HiliteBrush);
		}

		public override void DrawSelect(DrawData dd, bool primary)
		{
            Rectangle rect = Rectangle;

            if (rect.Left > dd.RIT) return;
            if (rect.Right < dd.LFT) return;

            dd.target.DrawRectangle(rect, primary ? resources.Selected_PrimaryBrush : resources.Selected_SecondaryBrush);
		}

        public override XElement Serialize(Song song) => new XElement("lyric",
    new XAttribute("time", timemark.ToString()),
    new XAttribute("text", text));

        public override VizCmd TranslateToCmd(ref Message msg, Keys keyData)
		{
            switch (keyData)
            {
                case Keys.Left:
                    if (StartPoint.X <= 0) return null;
                    return new VizCmd(this, Cmd.HorzRel, -1);

                case Keys.Right:
                    return new VizCmd(this, Cmd.HorzRel, 1);

                case Keys.Left | Keys.Control:
                    if (StartPoint.X <= 9) return null;
                    return new VizCmd(this, Cmd.HorzRel, -10);

                case Keys.Right | Keys.Control:
                    return new VizCmd(this, Cmd.HorzRel, 10);

                case Keys.Left | Keys.Shift:
                    return MoveBeatLeft();

                case Keys.Right | Keys.Shift:
                    return MoveBeatRight();

                case Keys.Left | Keys.Shift | Keys.Control:
                    return MoveTimePointLeft();

                case Keys.Right | Keys.Shift | Keys.Control:
                    return MoveTimePointRight();

                case Keys.Insert:
                    //return new VizCmd(this, Cmd.Insert);
                    throw new Exception("Insert not implemented.");

                case Keys.Delete:
                case Keys.X | Keys.Control:
                    return new VizCmd(this, Cmd.Delete);

                case Keys.C | Keys.Control:
                    return new VizCmd(this, Cmd.Copy);

                case Keys.V | Keys.Control:
                    return new VizCmd(this, Cmd.Paste);
            }

            return null;
		}

		protected override VizCmd CmdHorzRel(VizCmd vizCmd)
		{
			var cmd = base.CmdHorzRel(vizCmd);
            timemark = Global.Instance.Song.NormalizedTimeMark(StartPoint.X);
			return cmd;
		}
    }
}
