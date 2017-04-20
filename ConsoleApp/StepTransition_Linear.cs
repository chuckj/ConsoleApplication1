using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class StepTransition_Linear : StepTransition_Base
    {
        public static StepTransition_Linear Factory(XElement xml)
        {

            int hold, fade = 0, steps = 0;
           
            string wrk = (string)xml.Attribute("hold");
            if ((wrk == null) || !int.TryParse(wrk, out hold))
                hold = 0;
            wrk = (string)xml.Attribute("steps");
            if ((wrk == null) || !int.TryParse(wrk, out steps))
                steps = 0;

            return new StepTransition_Linear(steps, fade, hold);


            //int x, xstep, y, ystep, steps, hold;
            //string wrk = (string)xml.Attribute("x");
            //if ((wrk == null) || !int.TryParse(wrk, out x))
            //    throw new Exception($"position transition must have x value.");
            //wrk = (string)xml.Attribute("xstep");
            //if ((wrk == null) || !int.TryParse(wrk, out xstep))
            //    xstep = 0;
            //wrk = (string)xml.Attribute("y");
            //if ((wrk == null) || !int.TryParse(wrk, out y))
            //    throw new Exception($"position transition must have y value.");
            //wrk = (string)xml.Attribute("ystep");
            //if ((wrk == null) || !int.TryParse(wrk, out ystep))
            //    ystep = 0;
            //wrk = (string)xml.Attribute("steps");
            //if ((wrk == null) || !int.TryParse(wrk, out steps))
            //    steps = 0;
            //wrk = (string)xml.Attribute("hold");
            //if ((wrk == null) || !int.TryParse(wrk, out hold))
            //    hold = 0;

            //if ((steps > 0) && (xstep == 0) && (ystep == 0))
            //    throw new Exception($"xstep and/or ystep should be present with steps.");

            //return new StepTransition_Linear(x, xstep, y, ystep, steps, hold);
        }

        //public int X { get; set; }
        //public int XStep { get; set; }
        //public int Y { get; set; }
        //public int YStep { get; set; }
        public int Steps { get; set; }
        public int Fade { get; set; }
        public StepTransition_Linear() : base() { }

        public StepTransition_Linear(int steps, int fade, int hold) : base(hold)
        {
            Steps = steps;
            Fade = fade;
        }

        protected new string DebuggerDisplay => $"stps:{Steps} {base.DebuggerDisplay}";
    }
}
