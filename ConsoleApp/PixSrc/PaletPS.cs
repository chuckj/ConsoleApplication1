using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SD = System.Drawing;

namespace ConsoleApplication1.PixSrc
{
    public class PaletPS : IPixSrc
    {
        public SD.Color[] Palet { get; set; }
        public bool Wrap { get; set; }

        public void Init(PSData psdata)
        {

        }
        public SD.Color Get(PSData psdata)
        {
            var ndx = psdata.Index;
            if (!Wrap && ((ndx < 0) || (ndx >= Palet.Length)))
                return Color.Black;
            return Palet[ndx % Palet.Length];
        }
    }
}
