using System;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class StepTransition_Position : StepTransition_Base
    {
        public static StepTransition_Position Factory(XElement xml)
        {
            int x, xstep, y, ystep, steps, hold;
            string wrk = (string)xml.Attribute("x");
            if ((wrk == null) || !int.TryParse(wrk, out x))
                throw new Exception($"position transition must have x value.");
            wrk = (string)xml.Attribute("xstep");
            if ((wrk == null) || !int.TryParse(wrk, out xstep))
                xstep = 0;
            wrk = (string)xml.Attribute("y");
            if ((wrk == null) || !int.TryParse(wrk, out y))
                throw new Exception($"position transition must have y value.");
            wrk = (string)xml.Attribute("ystep");
            if ((wrk == null) || !int.TryParse(wrk, out ystep))
                ystep = 0;
            wrk = (string)xml.Attribute("steps");
            if ((wrk == null) || !int.TryParse(wrk, out steps))
                steps = 0;
            wrk = (string)xml.Attribute("hold");
            if ((wrk == null) || !int.TryParse(wrk, out hold))
                hold = 0;

            if ((steps > 0) && (xstep == 0) && (ystep == 0))
                throw new Exception($"xstep and/or ystep should be present with steps.");

            return new StepTransition_Position(x, xstep, y, ystep, steps, hold);
        }

        public int X { get; set; }
        public int XStep { get; set; }
        public int Y { get; set; }
        public int YStep { get; set; }
        public int Steps { get; set; }

        public StepTransition_Position() : base() { }

        public StepTransition_Position(int x, int xstep, int y, int ystep, int steps, int hold) : base(hold)
        {
            X = x;
            XStep = xstep;
            Y = y;
            YStep = ystep;
            Steps = steps;
        }

        protected new string DebuggerDisplay => $"x,y:{X}{(XStep > 0 ? "+" : "")}{XStep},{Y}{(YStep > 0 ? "+" : "")}{YStep} stps:{Steps} {base.DebuggerDisplay}";
    }
}
