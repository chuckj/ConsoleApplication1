﻿using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace ConsoleApplication1
{
    class CustomToolStripPanel : ToolStripPanel
    {
        private LayoutEngine _layoutEngine;

        public override LayoutEngine LayoutEngine
        {
            get
            {
                if (_layoutEngine == null) _layoutEngine = new CustomLayoutEngine();
                return _layoutEngine;
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size size = base.GetPreferredSize(proposedSize);

            foreach (Control control in Controls)
            {
                int newHeight = control.Height + control.Margin.Vertical + Padding.Vertical;
                if (newHeight > size.Height) size.Height = newHeight;
            }

            return size;
        }
    }
}