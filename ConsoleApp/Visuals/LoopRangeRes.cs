using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class LoopRangeRes : VizRes
    {
        public LoopRangeRes() : base()
        {
        }

        public Brush LoopRangeBrush;
        public Brush RangeBrush;
        public Brush Selected_PrimaryBrush;

        public override void DevDepAcquire(RenderTarget target)
        {
            LoopRangeBrush = SolidColorBrush(target, Global.LoopRangeColor);
            RangeBrush = SolidColorBrush(target, Global.RangeColor);
            Selected_PrimaryBrush = SolidColorBrush(target, Global.Selected_PrimaryColor);
        }

        public override void DevDepRelease()
        {
            LoopRangeBrush = null;
            RangeBrush = null;
            Selected_PrimaryBrush = null;
        }
    }
}
