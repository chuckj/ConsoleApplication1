using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SD = System.Drawing;

namespace ConsoleApplication1
{
    public class PaletPS : IPixSrc
    {
        public SD.Color[] Palet { get; set; }
        public bool Wrap { get; set; }

        public PaletPS() { }
        public PaletPS(IEnumerable<SD.Color> palet, bool wrap)
        {
            Wrap = wrap;
            Palet = palet.ToArray();
        }
        public void Init(PSData psdata)
        {

        }

        public SD.Color this[int index]  
        {
            get
            {
                // Check the index limits.
                if (index < 0)
                    throw new ArgumentException("Index");
                if (index < Palet.Length)
                    return Palet[index];
                else if (Wrap)
                    return Palet[index % Palet.Length];
                return SD.Color.Black;
            }
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
