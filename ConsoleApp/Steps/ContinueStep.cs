using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
	[Step("continue")]
	public class ContinueStep : Step
	{
        public override Step Factory(XElement nod) => new ContinueStep(nod);

        public ContinueStep(XElement nod)
			: base(nod)
		{
		}

		public override int Run()
		{
			ContinuePending = true;

			return 0;
		}
	}
}
