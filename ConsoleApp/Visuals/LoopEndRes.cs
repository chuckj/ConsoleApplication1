using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class LoopEndRes : VizRes
    {
        public LoopEndRes() : base()
        {
        }

        public Brush LoopEndBrush;
        public Brush HiliteBrush;
        public Brush Selected_PrimaryBrush;

        public override void DevDepAcquire(RenderTarget target)
        {
            LoopEndBrush = SolidColorBrush(target, Global.LoopEndColor);
            HiliteBrush = SolidColorBrush(target, Global.HiliteColor);
            Selected_PrimaryBrush = SolidColorBrush(target, Global.Selected_PrimaryColor);
        }

        public override void DevDepRelease()
        {
            LoopEndBrush = null;
            HiliteBrush = null;
            Selected_PrimaryBrush = null;
        }
    }
}
