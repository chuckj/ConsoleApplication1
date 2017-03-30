using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
	[Step("leftright")]
	public class LeftRightStep : Step
	{
		private string _trgt;
		private string _exit;
		private string _enter;
		private int _step = 2;

		public override Step Factory(XElement nod)
		{
			return new LeftRightStep(nod, (string)nod.Attribute("display"), (string)nod.Attribute("exit"), (string)nod.Attribute("enter"), (string)nod.Attribute("step"));
		}

		public LeftRightStep(XElement nod, string trgt, string exit, string enter, string step)
			: base(nod)
		{
			_trgt = trgt;
			_exit = exit;
			_enter = enter;
			if (!string.IsNullOrEmpty(step))
			{
				_step = (int)Eval(step);
				if (_step != 1 && _step != 2 && _step != 4)
					throw new ArgumentException("step must be 2 or 4:" + step);
			}
		}

		public override int Run()
		{
			List<Display> steps = new List<Display>(35);
			var curr = Global.Instance.curr;
			var trgt = (Display)Eval(_trgt);
			if (!(trgt is Display))
				throw new Exception("Invalid target:" + _trgt);
			Display nxt = (Display)trgt;
			Func<TreeData, TreeData> _curr, _nxt;
			int ctr = 0;
			if (_enter == "slide")
				_nxt = (td) => { TreeData tmp; Global.Instance.dict.TryGetValue(Tuple.Create(td.row, 17 - ctr + td.ctr), out tmp); return tmp; };
			else
				_nxt = (td) => td;

			if (_exit == "slide")
				_curr = (td) => { TreeData tmp; Global.Instance.dict.TryGetValue(Tuple.Create(td.row, td.ctr - 19 - ctr), out tmp); return tmp; };
			else
				_curr = (td) => td;

			for (ctr = _step - 19; ctr < 17; ctr += _step)
			{
				var d = Display.Get();

				Array.ForEach(Global.Instance.dta, t => d[t] = ((t.ctr <= ctr) ? nxt[_nxt(t)] : curr[_curr(t)]));
				steps.Add(d);
			}
			steps.Add(nxt);

            return PlaySteps(steps);
		}
	}
}
