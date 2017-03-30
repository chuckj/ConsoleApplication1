using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
	public class KeyFrame
	{
		public int Measure;
		public int BeatsPerMeasure;
		public float Time;
	}

	public class KeyFrameList : List<KeyFrame>
	{
	}

}
