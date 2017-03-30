using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class LoopBeginRes : VizRes
    {
        public LoopBeginRes() : base()
        {
        }

        public Brush LoopBeginBrush;
        public Brush HiliteBrush;
        public Brush Selected_PrimaryBrush;

        public override void DevDepAcquire(RenderTarget target)
        {
            LoopBeginBrush = SolidColorBrush(target, Global.LoopBeginColor);
            HiliteBrush = SolidColorBrush(target, Global.HiliteColor);
            Selected_PrimaryBrush = SolidColorBrush(target, Global.Selected_PrimaryColor);
        }

        public override void DevDepRelease()
        {
            LoopBeginBrush = null;
            HiliteBrush = null;
            Selected_PrimaryBrush = null;
        }
    }
}
