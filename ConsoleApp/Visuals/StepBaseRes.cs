using SharpDX;
using SharpDX.Direct2D1;

namespace ConsoleApplication1
{
    public class StepBaseRes : VizRes
    {
        public StepBaseRes() : base()
        {
        }

        public Brush Selected_PrimaryBrush;
        public Brush Selected_SecondaryBrush;
        public Brush HiliteBrush;
        public Brush Step_GreenBrush;
        public Brush Step_RedBrush;
        public Brush Step_GhostWhiteBrush;
        public Brush Step_BlackBrush;
        public Brush Step_GoldenrodBrush;

        public override void DevDepAcquire(RenderTarget target)
        {
            Selected_PrimaryBrush = SolidColorBrush(target, Global.Selected_PrimaryColor);
            Selected_SecondaryBrush = SolidColorBrush(target, Global.Selected_SecondaryColor);
            HiliteBrush = SolidColorBrush(target, Global.HiliteColor);
            Step_GreenBrush = SolidColorBrush(target, Color.Green);
            Step_RedBrush = SolidColorBrush(target, Color.Red);
            Step_GhostWhiteBrush = SolidColorBrush(target, Color.GhostWhite);
            Step_BlackBrush = SolidColorBrush(target, Color.Black);
            Step_GoldenrodBrush = SolidColorBrush(target, Color.Goldenrod);
        }

        public override void DevDepRelease()
        {
            Selected_PrimaryBrush = null;
            Selected_SecondaryBrush = null;
            HiliteBrush = null;
            Step_GreenBrush = null;
            Step_RedBrush = null;
            Step_GhostWhiteBrush = null;
            Step_BlackBrush = null; 
            Step_GoldenrodBrush = null;
        }

        //public override void DevIndepAcquire()
        //{
        //}

        //public override void DevIndepRelease()
        //{
        //}
    }
}
