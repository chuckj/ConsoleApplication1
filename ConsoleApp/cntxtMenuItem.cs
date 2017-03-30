using System;
using System.Collections.Generic;
using System.Drawing;

namespace ConsoleApplication1
{
    public delegate IEnumerable<VizCmd> CntxtMenuItemHandler(Viz viz, Point MouseLocation);

    public class CntxtMenuItem
    {
        public string Name;
        public CntxtMenuItemHandler Handler;
        public bool Enabled;

        public CntxtMenuItem() { }

        public CntxtMenuItem(string Name, CntxtMenuItemHandler Handler, bool Enabled = true)
        {
            this.Name = Name;
            this.Handler = Handler;
            this.Enabled = Enabled;
        }
    }
}
