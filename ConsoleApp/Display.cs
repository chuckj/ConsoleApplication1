using System;
using System.Text;

namespace ConsoleApplication1
{
    public class DisplayUpdate : Update
    {
        private uint[] _bits = new uint[10];
        private string name;


        public uint[] Bits => _bits;

        public DisplayUpdate(int name)
        {
			this.name = name.ToString("x");
			Clear();
        }

        public DisplayUpdate()
        {
            Clear();
        }

        public void Clear()
        {
			for (int ndx = 0; ndx < _bits.Length; ndx++)
				_bits[ndx] = 0;
        }

        public void Set(MonoLit td)
        {
			_bits[td.BitNdx] |= td.BitMask;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Global.pxpersec);
			sb.Append("d" + name + ",");
			for (int ndx = 0; ndx < _bits.Length; ndx++)
			{
				sb.Append(_bits[ndx].ToString("x8"));
			}
			return sb.ToString();
        }

        public static DisplayUpdate Get() => new DisplayUpdate();

        public void Return()
		{
		}

		public bool this[MonoLit td]
		{
			get
			{
				if (td == null) return false;
				return (_bits[td.BitNdx] & td.BitMask) != 0;
			}
			set
			{
				if (value)
					_bits[td.BitNdx] |= td.BitMask;
				else
					_bits[td.BitNdx] &= ~td.BitMask;
			}
		}

		public void Set(MonoLit td, bool bit)
		{
			if (bit)
				_bits[td.BitNdx] |= td.BitMask;
		}

		public DisplayUpdate FromHex(string hex)
		{
			uint byt;
			for (int ndx = 0; ndx < _bits.Length; ndx++)
			{
				if (!uint.TryParse(hex.Substring(ndx * 8, 8), System.Globalization.NumberStyles.AllowHexSpecifier, null, out byt))
					throw new ArgumentException("Display.Value must be hex string:" + hex);
				_bits[ndx] = byt;
			}
			return this;
		}

		public DisplayUpdate Not()
		{
			for (int ndx = 0; ndx < _bits.Length; ndx++)
			{
				_bits[ndx] = ~_bits[ndx];
			}
			return this;
		}

		public DisplayUpdate Or(DisplayUpdate other)
		{
			for (int ndx = 0; ndx < _bits.Length; ndx++)
			{
				_bits[ndx] |= other._bits[ndx];
			}
			return this;
		}

		public DisplayUpdate And(DisplayUpdate other)
		{
			for (int ndx = 0; ndx < _bits.Length; ndx++)
			{
				_bits[ndx] &= other._bits[ndx];
			}
			return this;
		}

        public DisplayUpdate Clone()
        {
            var disp = Get();
			for (int ndx = 0; ndx < _bits.Length; ndx++)
			{
				disp._bits[ndx] = _bits[ndx];
			}
			return disp;
        }
    }
}
