using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleApplication1
{

    public class Change
	{
		public List<VizCmd> vizCmds { get; set; }
	}

	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class VizCmd
	{
        public string DebuggerDisplay => string.Format("Cmv: {0} {1}", cmd, viz.DebuggerDisplay);

        public Viz viz { get; set; }
		public Cmd cmd { get; set; }
		public object obj { get; set; }

		public VizCmd(Viz viz, Cmd cmd, object obj = null)
		{
			this.viz = viz;
			this.cmd = cmd;
			this.obj = obj;
		}
	}

	public enum Cmd
	{
		HorzRel = 1,
        Insert,
        Delete,
        UnSelectItem,
        Copy,
        Paste,

	}
}
