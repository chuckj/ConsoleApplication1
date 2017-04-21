using SharpDX.Direct2D1;

namespace ConsoleApplication1
{
    public class BeatRes : VizRes
    {
        public BeatRes() : base()
        {
        }

        public Brush Beat_BeatBrush;
        public StrokeStyle Beat_StrokeStyle;

        public override void DevIndepAcquire()
        {
            Beat_StrokeStyle = StrokeStyle(new StrokeStyleProperties() { DashStyle = DashStyle.Custom }, new float[] { 1.0f, 3.0f });
        }

        public override void DevIndepRelease()
        {
            Beat_StrokeStyle = null;
        }

        public override void DevDepAcquire(RenderTarget target)
        {
            Beat_BeatBrush = SolidColorBrush(target, Global.Beat_BeatColor);
        }

        public override void DevDepRelease()
        {
            Beat_BeatBrush = null;
        }
    }
}
