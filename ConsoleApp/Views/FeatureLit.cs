using System;
using System.Drawing;

namespace ConsoleApplication1
{
    public class FeatureLit : Lit
    {

        public static FeatureLit FromName(string name)
        {
            FeatureLit lit;
            if (Global.Instance.FeatureLitDict.TryGetValue(name, out lit))
                return lit;

            Clr clr;
            if (name.StartsWith("#"))
            {
                int val = Convert.ToInt32(name.Substring(1), 16);
                clr = Color.FromArgb((val >> 16) & 0xff, (val >> 8) & 0xff, val & 0xff);
                return new FeatureLit(name, clr);
            }
            else
            {
                clr = Color.FromName(name);
                return new FeatureLit(name, clr);
            }
        }

        public FeatureLit(string nm, Clr clr) : base(nm, clr)
        {
            Global.Instance.FeatureLitDict.Add(nm, this);
        }
    }
}
