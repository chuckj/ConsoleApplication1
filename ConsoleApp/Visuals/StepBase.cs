using SharpDX;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class StepBase : Viz
    {
        private static StepBaseRes resources;
        private static int ID = 0;
        private XElement xml;

        static StepBase()
        {
            resources = new StepBaseRes();
        }

        public override string DebuggerDisplay => $"StepBase: {xml} {base.DebuggerDisplay}";
        public string DebuggerDisplayShort => $"{xml} {base.DebuggerDisplay}";

        public TimeMark startTimeMark;
        public TimeMark endTimeMark;
        public string text;
        public int row;
        public int id;

        public StepBase(string text, TimeMark startTimeMark, TimeMark endTimeMark)
        {
            this.text = text;
            this.startTimeMark = startTimeMark;
            this.endTimeMark = endTimeMark;
            this.id = ++ID;
        }

        public StepBase(XElement xml, Song song, int measureOffset)
        {
            this.xml = xml;
            this.text = (string)xml.Attribute("text");
            this.startTimeMark = song.TimeMarkParse((string)xml.Attribute("begin"), measureOffset);
            this.endTimeMark = song.TimeMarkParse((string)xml.Attribute("end"), measureOffset);
        }


        public override VizFeatures Features => VizFeatures.IsSelectable | VizFeatures.IsMultiSelectable | VizFeatures.HasToolTip
            | VizFeatures.HasHilites | VizFeatures.HasContextMenu | VizFeatures.HasProperties;

        public override void ResetPoints(Song song)
        {
            StartPoint = new Point((int)startTimeMark.Time, row + Global.Step_Y);
            EndPoint = new Point((int)endTimeMark.Time, row + 7 + Global.Step_Y);
        }

        public override void Draw(DrawData dd)
        {
            dd.target.FillRectangle(Rectangle, resources.Step_GoldenrodBrush);
        }

        public override void DrawHilites(DrawData dd)
        {
            var rect = new RectangleF(EndPoint.X - 7, EndPoint.Y - 7, 6, 6);
            dd.target.FillRectangle(rect, resources.Step_RedBrush);
            dd.target.DrawRectangle(rect, resources.Step_GhostWhiteBrush);
            rect = new RectangleF(EndPoint.X - 8, EndPoint.Y - 8, 8, 8);
            dd.target.DrawRectangle(rect, resources.Step_BlackBrush);

            rect = new RectangleF(StartPoint.X, StartPoint.Y, 6, 6);
            dd.target.FillRectangle(rect, resources.Step_GreenBrush);
            dd.target.DrawRectangle(rect, resources.Step_GhostWhiteBrush);
            rect = new RectangleF(StartPoint.X - 1, StartPoint.Y - 1, 8, 8);
            dd.target.DrawRectangle(rect, resources.Step_BlackBrush);
        }

        public override void DrawSelect(DrawData dd, bool primary)
        {
            dd.target.DrawRectangle(Rectangle, primary ? resources.Selected_PrimaryBrush : resources.Selected_SecondaryBrush);
        }

        public override IEnumerable<CntxtMenuItem> GetContentMenuItems() => new CntxtMenuItem[] {
                new CntxtMenuItem("Exit", cmExit) };

        private static IEnumerable<VizCmd> cmExit(Viz viz, System.Drawing.Point MouseLocation)
		{
            return null;
		}

        //public override Form PropertyWindow()
        //{
        //	return new Dialogs.dlgProps(); 
        //}

        public override string ToolTip() => DebuggerDisplay;

        public override XElement Serialize(Song song)
		{
			return new XElement("step",
				new XAttribute("begin", startTimeMark.ToString()),
				new XAttribute("end", endTimeMark.ToString()),
				new XAttribute("text", text));
		}

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
                    break;

                case Keys.Delete:
                    break;
            }
			return null;
		}

        protected override VizCmd CmdHorzRel(VizCmd vizCmd) => base.CmdHorzRel(vizCmd);

        public virtual Viz Factory(XElement nod, Song song, int measureOffset) => null;

        public StepBaseRes Resources => resources;

    }
}
