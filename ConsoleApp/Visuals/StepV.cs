using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    [Step("stepv")]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class StepV : StepBase, IRunTime
    {
        #region Statics
        private static StepVRes resources;

        static StepV()
        {
            resources = new StepVRes();
        }
        #endregion

        #region Debugger
        public override string DebuggerDisplay => $"StepV: {base.DebuggerDisplay}";
        #endregion

        #region Ctors
        public override Viz Factory(XElement xml, Song song, int measureOffset) => new StepV(xml, song, measureOffset);

        public StepV(XElement xml, Song song, int measureOffset) : base(xml, song, measureOffset)
        {
        }
        #endregion

        public IEnumerable<int> Xeq(RunTime context)
        {
            context[0] = System.Drawing.Color.White;
            Task.Delay(500).Wait();
            yield return EndPoint.X;
            Task.Delay(500).Wait();
            context[0] = System.Drawing.Color.Red;
        }
    }
}
