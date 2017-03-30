using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class MeasureRes : VizRes
    {
        public MeasureRes() : base()
        {
        }

        public Brush Measure_NormalBrush;
        public Brush Measure_GreenBrush;
        public Brush Measure_RedBrush;
        public TextFormat Measure_TextFormat;
        public Brush Selected_PrimaryBrush;
        public Brush HiliteBrush;
        public Brush Measure_FontBrush;

        public override void DevIndepAcquire()
        {
            Measure_TextFormat = TextFormat("Arial", 10);
        }

        public override void DevIndepRelease()
        {
            Measure_TextFormat = null;
        }

        public override void DevDepAcquire(RenderTarget target)
        {
            Measure_NormalBrush = SolidColorBrush(target, Global.Measure_NormalColor);
            Measure_GreenBrush = SolidColorBrush(target, Global.Measure_GreenColor);
            Measure_RedBrush = SolidColorBrush(target, Global.Measure_RedColor);
            HiliteBrush = SolidColorBrush(target, Global.HiliteColor);
            Selected_PrimaryBrush = SolidColorBrush(target, Global.Selected_PrimaryColor);
            Measure_FontBrush = SolidColorBrush(target, Global.Measure_FontColor);
        }

        public override void DevDepRelease()
        {
            Measure_NormalBrush = null;
            Measure_GreenBrush = null;
            Measure_RedBrush = null;
            HiliteBrush = null;
            Selected_PrimaryBrush = null;
            Measure_FontBrush = null;
        }
    }
}
