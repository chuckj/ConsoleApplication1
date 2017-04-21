using SharpDX.Direct2D1;

namespace ConsoleApplication1
{
    public class SliderThumbRes : VizRes
    {
        public SliderThumbRes() : base()
        {
        }

        public Brush SliderThumb_ThumbBrush;
        public Brush SliderThumb_ThumbFillBrush;
        public StrokeStyle SliderThumb_StrokeStyle;

        public override void DevIndepAcquire()
        {
            SliderThumb_StrokeStyle = StrokeStyle(new StrokeStyleProperties() { DashStyle = DashStyle.Solid });
        }

        public override void DevIndepRelease()
        {
            SliderThumb_StrokeStyle = null;
        }

        public override void DevDepAcquire(RenderTarget target)
        {
            SliderThumb_ThumbBrush = SolidColorBrush(target, Global.SliderThumb_ThumbColor);
            SliderThumb_ThumbFillBrush = SolidColorBrush(target, Global.SliderThumb_ThumbFillColor);
        }

        public override void DevDepRelease()
        {
            SliderThumb_ThumbBrush = null;
            SliderThumb_ThumbFillBrush = null;
        }
    }
}
