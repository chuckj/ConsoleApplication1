using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
	[Step("if")]
	public class IfStep : Step
	{
		private string _cond;

        public override Step Factory(XElement nod) => new IfStep(nod, (string)nod.Attribute("cond"));

        public IfStep(XElement nod, string cond)
			: base(nod)
		{
			_cond = cond;
		}

		public override int Run()
		{

			object obj = Eval(_cond);
			if (!(obj is bool)) throw new Exception("Expecting bool:");
			var nxt = nod.Descendants((bool)obj ? "then" : "else").FirstOrDefault();
			RunChildren(nxt);

			return 0;
		}


	}
}
