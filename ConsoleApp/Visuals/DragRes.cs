using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class DragRes : VizRes
    {
        public DragRes() : base()
        {
        }

        public Brush Drag_DragBrush;
        public StrokeStyle Drag_StrokeStyle;

        public override void DevIndepAcquire()
        {
            Drag_StrokeStyle = StrokeStyle(new StrokeStyleProperties() { DashStyle = DashStyle.Solid });
        }

        public override void DevIndepRelease()
        {
            Drag_StrokeStyle = null;
        }

        public override void DevDepAcquire(RenderTarget target)
        {
            Drag_DragBrush = SolidColorBrush(target, Global.Drag_DragColor);
        }

        public override void DevDepRelease()
        {
            Drag_DragBrush = null;
        }
    }
}
