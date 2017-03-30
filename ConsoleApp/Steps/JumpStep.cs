using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
	[Step("jump")]
	public class JumpStep : Step
	{
		private string _trgt;

        public override Step Factory(XElement nod) => new JumpStep(nod, (string)nod.Attribute("display"));

        public JumpStep(XElement nod, string trgt)
			: base(nod)
		{
			_trgt = trgt;
		}

		public override int Run()
		{
			var trgt = Eval(_trgt);
			if (!(trgt is DisplayUpdate))
				throw new Exception("Invalid target:" + _trgt);

            return PlayUpdates((DisplayUpdate)trgt);
		}
	}
}
