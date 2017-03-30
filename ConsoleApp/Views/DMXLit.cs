using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class DMXLit : RGBLit
    {
        public DMXLit(string nm) : base(nm) {
            LiteNdx = new List<int>();
            DimNdx = new List<int>();
        }
        public List<int> LiteNdx { get; set; }
        public List<int> DimNdx { get; set; }
    }
}
