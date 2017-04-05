using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class FeatureLit : RGBLit
    {
        public FeatureLit(string nm, Clr clr) : base(nm) {
            LiteNdx = new List<int>();
            FeatureNdx = new List<int>();
            Clr = clr;
        }
        public List<int> LiteNdx { get; set; }
        public List<int> FeatureNdx { get; set; }
    }
}
