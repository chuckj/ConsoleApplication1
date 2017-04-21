using System.Drawing;
using System.Windows.Forms;

namespace ConsoleApplication1
{
    internal class CustomStripRenderer : ToolStripProfessionalRenderer
    {
       
        private static Color borderColor = Color.Black;


        public CustomStripRenderer()
        {
        }

        // This method handles the RenderToolStripBorder event.
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.Black),
                new Rectangle(0, 0, e.ToolStrip.Width, 3));
            e.Graphics.FillRectangle(new SolidBrush(Color.Black),
                new Rectangle(0, 20, e.ToolStrip.Width, 2));
            e.Graphics.FillRectangle(new SolidBrush(Color.Black),
                new Rectangle(0, 3, 1, e.ToolStrip.Height - 4));
            e.Graphics.FillRectangle(new SolidBrush(Color.Black),
                new Rectangle(e.ToolStrip.Width-14, 3, 14, e.ToolStrip.Height - 4));
            base.OnRenderToolStripBorder(e);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            using (Brush brush = new SolidBrush(e.Item.BackColor))
            {
                e.Graphics.FillRectangle(brush, e.TextRectangle);
            }
            if (e.Item.Name == "ssProgressBar1")
            {
                int? pctg = (int?)e.Item.Tag;
                if (pctg.HasValue)
                {
                    int width = (e.Item.Bounds.Width - 6) * pctg.Value / 100;
                    e.Graphics.FillRectangle(Brushes.Lime, 3, 0, width, e.Item.Bounds.Height);
                    e.Graphics.FillRectangle(Brushes.DarkGreen, width + 3, 0, e.Item.Bounds.Width - 6 - width, e.Item.Bounds.Height);
                }
            }
            //else
            //{
                base.OnRenderItemText(e);
            //}
        }

        protected override void OnRenderToolStripStatusLabelBackground(ToolStripItemRenderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(e.Item.BackColor), e.Item.ContentRectangle);
            base.OnRenderToolStripStatusLabelBackground(e);
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderButtonBackground(e);
        }

        protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderItemBackground(e);
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            base.OnRenderToolStripBackground(e);
        }

        protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e)
        {
            base.OnRenderToolStripPanelBackground(e);
        }

        protected override void OnRenderToolStripContentPanelBackground(ToolStripContentPanelRenderEventArgs e)
        {
            base.OnRenderToolStripContentPanelBackground(e);
        }

        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderDropDownButtonBackground(e);
        }

        protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderLabelBackground(e);
        }
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderMenuItemBackground(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            base.OnRenderSeparator(e);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            base.OnRenderImageMargin(e);
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            base.OnRenderItemImage(e);
        }

        protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderSplitButtonBackground(e);
        }
    }

    internal class CustomMenuStripRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            //if (!e.Item.Selected) base.OnRenderMenuItemBackground(e);
            //else {
            if (!e.Item.IsOnDropDown)
            { 
                var rc = new Rectangle(Point.Empty, e.Item.Size);
                e.Graphics.FillRectangle(Brushes.Black, rc);
                if (e.Item.Selected)
                    e.Graphics.DrawRectangle(Pens.AntiqueWhite, 1, 0, rc.Width - 2, rc.Height - 1);
            }
            else
                base.OnRenderMenuItemBackground(e);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            base.OnRenderToolStripBorder(e);

            e.Graphics.FillRectangle(new SolidBrush(Color.White),
                new Rectangle(0, e.ToolStrip.Height - 1, e.ToolStrip.Width, 1));
        }
    }
}
