using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class SliderRes : VizRes
    {
        public SliderRes() : base()
        {
        }

        public Brush Slider_SliderBrush;
        public StrokeStyle Slider_StrokeStyle;
        public Brush Slider_LoopBeginBrush;
        public Brush Slider_LoopEndBrush;
        public Brush SliderCursor_CursorBrush;

        public override void DevIndepAcquire()
        {
            Slider_StrokeStyle = StrokeStyle(new StrokeStyleProperties() { DashStyle = DashStyle.Custom }, new float[] { 1.0f, 3.0f });
        }

        public override void DevIndepRelease()
        {
            Slider_StrokeStyle = null;
        }

        public override void DevDepAcquire(RenderTarget target)
        {
            Slider_SliderBrush = SolidColorBrush(target, Global.Slider_SliderColor);
            Slider_LoopBeginBrush = SolidColorBrush(target, Global.Slider_LoopBeginColor);
            Slider_LoopEndBrush = SolidColorBrush(target, Global.Slider_LoopEndColor);
            SliderCursor_CursorBrush = SolidColorBrush(target, Global.SliderCursor_CursorColor);
        }

        public override void DevDepRelease()
        {
            Slider_SliderBrush = null;
            Slider_LoopBeginBrush = null;
            Slider_LoopEndBrush = null;
            SliderCursor_CursorBrush = null;
        }
    }
}
