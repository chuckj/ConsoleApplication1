using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
	[Step("break")]
	public class BreakStep : Step
	{
        public override Step Factory(XElement nod) => new BreakStep(nod);

        public BreakStep(XElement nod)
			: base(nod)
		{
		}

		public override int Run()
		{
			BreakPending = true;

			return 0;
		}
	}
}
