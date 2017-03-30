using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
	[Step("for")]
	public class ForStep : Step
	{
		private string _varname;
		private int _init;
		private int _final;
		private int _step;

        public override Step Factory(XElement nod) => new ForStep(nod, (string)nod.Attribute("varname"), (string)nod.Attribute("init"), (string)nod.Attribute("final"), (string)nod.Attribute("step"));

        public ForStep(XElement nod, string varname, string init, string final, string step)
			: base(nod)
		{
			_varname = varname;
			_init = (int)Eval(init);
			_final = (int)Eval(final);
			_step = 1;
			if (!string.IsNullOrEmpty(step))
				_step = (int)Eval(step);
		}

		public override int Run()
		{
			var vari = new Vari<int>(_varname);

			varistack.Push(vari);

			for (int val = _init; val <= _final; val += _step)
			{
				vari.Value = val;
				RunChildren();
				ContinuePending = false;
				if (BreakPending)
				{
					BreakPending = false;
					break;
				}
			}

			varistack.Pop();

			return 0;
		}
	}
}
