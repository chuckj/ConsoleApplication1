using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
//using System.Xml;
//using System.Xml.XPath;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Drawing;

namespace ConsoleApplication1
{
    public class Grp : List<Lit>
    {
        public static Dictionary<string, Grp> GrpDict = new Dictionary<string, Grp>();

        public static void Load(XElement root)
        {
            XElement xpN = root.Element("Groups");
            foreach (XElement cur in xpN.Elements("Group"))
            {
                Grp grp = new Grp((string)cur.Attribute(@"nm"));

                foreach (XElement xpngl in cur.Elements("Lite"))
                {
                    grp.Add((string)xpngl.Attribute(@"nm"));
                }
            }
        }

        public static Grp Group(params object[] args)
        {
            Grp grp = new Grp();

            for (int ndx = 0; ndx < args.Length; ndx++)
            {
                object prm = args[ndx];
                if (prm is string)
                {
                    grp.Add((string)prm);
                }
                else if (prm is Lit)
                {
                    grp.Add((Lit)prm);
                }
                else if (prm is Grp)
                {
                    grp = new Grp(grp.Union((Grp)prm));
                }
                else if (prm is Func<Lit, int, bool>)
                {
                    grp.Add(Lit.LitDict.Values.Where((Func<Lit, int, bool>)prm));
                }
            }
            return grp;
        }

        public static void Build(XElement vu, string Name, Lit[] LitArray)
        {
            XElement grps = vu.Element("Groups");
            if (grps != null)
            {
                foreach (XElement xgrp in grps.Elements("Group"))
                {
                    Grp grp = new Grp(((string)xgrp.Attribute("nm")).Replace("$", Name));
                    if (xgrp.Attribute("row") != null)
                    {
                        int rw = (int)xgrp.Attribute("row");
                        grp.AddRange(from l in LitArray where l.Row == rw select l);
                    }
                    else if (xgrp.Attribute("col") != null)
                    {
                        int cl = (int)xgrp.Attribute("col");
                        grp.AddRange(from l in LitArray where l.Column == cl select l);
                    }
                }
            }
        }

        public string Name { get; private set; }

        public Grp()
        {
        }

        public Grp(Lit lit)
        {
            Add(lit);
        }

        public Grp(IEnumerable<Lit> lit)
            : this()
        {
            Add(lit);
        }

        public Grp(String nm)
            : this()
        {
            if (GrpDict.ContainsKey(nm))
                throw new Exception("Duplicate Group key: " + nm);

            Name = nm;
            GrpDict.Add(nm, this);
        }

        public Grp(string nm, IEnumerable<Lit> lit)
            : this(nm)
        {
            Add(lit);
        }

        public void Add(IEnumerable<Lit> lit)
        {
            foreach (Lit l in lit)
                base.Add(l);
        }

        public void Add(string nm)
        {
            Lit lit = null;
            if (!Lit.LitDict.TryGetValue(nm, out lit))
                throw new Exception("Missing Lit key: " + nm);

            Add(lit);
        }

        public static Grp operator *(Grp A, Grp B)
        {
            return new Grp(A.Intersect(B));
        }

        public static Grp operator +(Grp A, Grp B)
        {
            return new Grp(A.Union(B));
        }

        public static Grp operator -(Grp A, Grp B)
        {
            return new Grp(A.Except(B));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Lit l in this)
                sb.Append(l.ToString() + ",");
            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);
            return Name + ":{" + sb.ToString() + "}";
        }

        public void Set(Level lvl, int pgmId)
        {
            foreach (Lit lit in this)
                lit.Set(lvl, pgmId);
        }
    }
}
