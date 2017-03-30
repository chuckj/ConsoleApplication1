using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SD = System.Drawing;

namespace ConsoleApplication1
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class StepTransition_Fade : StepTransition_Base
    {
        public static StepTransition_Fade Factory(XElement xml)
        {
            int hold, fade = 0;
            string wrk = (string)xml.Attribute("fade");
            if ((wrk == null) || !int.TryParse(wrk, out fade))
                throw new Exception($"fade transition must have fade value.");
            if ((fade < 0) || (fade > 100))
                throw new Exception($"fade transition fade should be between 0 and 100.");
            wrk = (string)xml.Attribute("hold");
            if ((wrk == null) || !int.TryParse(wrk, out hold))
                hold = 0;
            
            return new StepTransition_Fade(fade, hold);
        }

        public int Fade { get; set; }

        public StepTransition_Fade() : base() { }

        public StepTransition_Fade(int fade, int hold) : base(hold)
        {
            Fade = fade;
        }

        protected new string DebuggerDisplay => $"fade:{Fade.ToString()}";
    }
}
