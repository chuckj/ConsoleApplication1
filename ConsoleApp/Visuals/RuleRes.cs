using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class RuleRes : VizRes
    {
        public RuleRes() : base()
        {
        }

        public StrokeStyle Ruler_MeasureStrokeStyle;
        public TextFormat Ruler_TextFormat;

        public Brush Ruler_LineBrush;
        public Brush Ruler_FontBrush;
        public Brush Ruler_MinuteBrush;
        public Brush Ruler_MeasureBrush;
        public Brush Ruler_CursorBrush;
        public Brush Ruler_SecondsBrush;

        public override void DevIndepAcquire()
        {
            Ruler_TextFormat = TextFormat("Arial", 10);
            Ruler_MeasureStrokeStyle = StrokeStyle(new StrokeStyleProperties() { DashStyle = DashStyle.Custom }, new float[] { 4.0f, 1.0f });
        }

        public override void DevIndepRelease()
        {
            Ruler_TextFormat = null;
            Ruler_MeasureStrokeStyle = null;
        }

        public override void DevDepAcquire(RenderTarget target)
        {
            Ruler_MinuteBrush = SolidColorBrush(target, Global.Ruler_MinuteColor);
            Ruler_MeasureBrush = SolidColorBrush(target, Global.Ruler_MeasureColor);
            Ruler_FontBrush = SolidColorBrush(target, Global.Ruler_FontColor);
            Ruler_LineBrush = SolidColorBrush(target, Global.Ruler_LineColor);
            Ruler_CursorBrush = SolidColorBrush(target, Global.Ruler_CursorColor);
            Ruler_SecondsBrush = SolidColorBrush(target, Global.Ruler_SecondsColor);

            //Ruler_MinutePen.DashPattern = new float[] { 1.0f, 4.0f };
        }

        public override void DevDepRelease()
        {
            Ruler_MinuteBrush = null;
            Ruler_MeasureBrush = null;
            Ruler_FontBrush = null;
            Ruler_LineBrush = null;
            Ruler_CursorBrush = null;
            Ruler_SecondsBrush = null;
        }
    }
}
