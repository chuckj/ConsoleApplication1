using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{

    public class MarqueUpdate : Update
    {
        private uint[] _bits = new uint[16];

        public MarqueUpdate()
        {
            Clear();
        }

        public uint[] Bits
        {
            get
            {
                return _bits;
            }
            set
            {

            }
        }

        public void Clear()
        {
            for (int ndx = 0; ndx < _bits.Length; ndx++)
                _bits[ndx] = 0;
        }

        public bool this[MonoLit td]
        {
            get
            {
                if (td == null) return false;
                return (_bits[td.MarqueNdx] & td.MarqueMask) != 0;
            }
            set
            {
                if (value)
                    _bits[td.MarqueNdx] |= td.MarqueMask;
                else
                    _bits[td.MarqueNdx] &= ~td.MarqueMask;
            }
        }

        public bool Even { get; set; }

        public MarqueUpdate Clone()
        {
            var disp = new MarqueUpdate();
            for (int ndx = 0; ndx < _bits.Length; ndx++)
            {
                disp._bits[ndx] = _bits[ndx];
            }
            return disp;
        }
    }
}
