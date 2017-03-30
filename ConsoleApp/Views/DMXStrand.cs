using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class DMXStrand : Strand
    {

        public DMXStrand()
        {
            lites = new List<RGBLit>(32);
        }
    }
}
