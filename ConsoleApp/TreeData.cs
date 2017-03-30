using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class TreeData
    {
        public int row { get; set; }
        public int ctr { get; set; }
        public int col { get; set; }
        public Color color { get; set; }
        public int marqueNdx { get; set; }
        public uint marqueMsk { get; set;  }

		private int _ndx;
        public int ndx 
		{ 
			set 
			{ 
				_ndx = value; 
				bitndx = value >> 5;
				bitmask = 0x80000000 >> (value & 0x1f);
			} 
			get
			{ return _ndx;
			}
		}

		public int bitndx { get; set; }
		public uint bitmask { get; set; }

        private string DebuggerDisplay { get { return string.Format("row:{0} ctr:{1} Clr:{2}", row, ctr, color);  } }
    }
}
