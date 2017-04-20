using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class StepTransition_Base
    {

        public int Hold { get; set; }

        public StepTransition_Base()
        {
            Hold = 0;
        }

        public StepTransition_Base(int hold)
        {
            this.Hold = hold;
        }

        protected string DebuggerDisplay => $"hld:{Hold}";

        //public virtual IEnumerable<int> Xeq(RunTime context)
        //{
        //    yield break;
        //}
    }
}
