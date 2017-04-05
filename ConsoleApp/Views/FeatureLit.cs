﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class FeatureLit : Lit
    {

        public static FeatureLit FromName(string name)
        {
            FeatureLit lit;

            Clr clr;
            if (name.StartsWith("#"))
            {
                if (Global.Instance.FeatureLitDict.TryGetValue(name, out lit))
                    return lit;

                int val = Convert.ToInt32(name.Substring(1), 16);
                clr = Color.FromArgb((val >> 16) & 0xff, (val >> 8) & 0xff, val & 0xff);
            }
            else
            {
                name = "@" + name;
                if (Global.Instance.FeatureLitDict.TryGetValue(name, out lit))
                    return lit;
                clr = Color.FromName(name);
            }
            return new FeatureLit(name, clr);
        }

        public FeatureLit(string nm, Clr clr) : base(nm, clr)
        {
            Global.Instance.FeatureLitDict.Add(nm, this);
        }
    }
}
