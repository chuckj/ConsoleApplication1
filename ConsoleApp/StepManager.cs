using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication2
{
	public class StepManager
	{
		private List<Step> steps = new List<Step>();
		private int top;


		public StepManager(int top)
		{
			this.top = top;
			steps = new List<Step>();
		}

		public Step Create(string text, TimeMark startTimeMark, TimeMark endTimeMark)
		{
			var step = new Step(text, startTimeMark, endTimeMark);
			steps.Add(step);
			return step;
		}

		public List<Step> Steps { get { return steps; } }

		public void Order()
		{
			steps = steps.OrderBy(l => l.startTimeMark.Time).ToList();
		}


	}
}
