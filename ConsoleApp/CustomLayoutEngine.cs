using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace ConsoleApplication1
{
    class CustomLayoutEngine : LayoutEngine
    {
        public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
        {
            Control parent = container as Control;

            Rectangle parentDisplayRectangle = parent.DisplayRectangle;

            Control[] source = new Control[parent.Controls.Count];
            parent.Controls.CopyTo(source, 0);

            Point nextControlLocation = parentDisplayRectangle.Location;

            foreach (Control c in source)
            {
                if (!c.Visible) continue;

                nextControlLocation.Offset(c.Margin.Left, c.Margin.Top);
                c.Location = nextControlLocation;

                if (c.AutoSize)
                {
                    c.Size = c.GetPreferredSize(parentDisplayRectangle.Size);
                }

                nextControlLocation.Y = parentDisplayRectangle.Y;
                nextControlLocation.X += c.Width + c.Margin.Right + parent.Padding.Horizontal;
            }

            return false;
        }
    }
}