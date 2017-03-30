using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    public class GrpSeq
    {
        public static Dictionary<string, GrpSeq> GrpSeqDict = new Dictionary<string, GrpSeq>();

        public static void Load(XElement root)
        {
            XElement xpN = root.Element("GrpSeqs");
            foreach (XElement cur in xpN.Elements("GrpSeq"))
            {
                GrpSeq grpseq = new GrpSeq((string)cur.Attribute(@"nm"));

                foreach (XElement stp in cur.Elements("step"))
                {
                    List<Grp> stplst = new List<Grp>();
                    grpseq.GrpList.Add(stplst);

                    foreach (XElement grp in stp.Elements("grp"))
                    {
                        stplst.Add(Pgm.ParseGrp(null, grp));
                    }
                }
            }
        }


        public static void Build(XElement vu, string Name, Lit[] LitArray)
        {
            XElement grpseqs = vu.Element("GrpSeqs");
            if (grpseqs != null)
            {
                foreach (XElement xgrpseq in grpseqs.Elements("GrpSeq"))
                {
                    GrpSeq grpseq = new GrpSeq(((string)xgrpseq.Attribute("nm")).Replace("$", Name));
                    var x = from l in xgrpseq.Elements("Group") select new List<Grp> { Grp.GrpDict[((string)l.Attribute("grp")).Replace("$", Name)] };
                    grpseq.GrpList.AddRange(x);
                }
            }
        }


        public List<List<Grp>> GrpList = new List<List<Grp>>();
        public string Name;

        public GrpSeq()
        {
        }

        public GrpSeq(String nm)
            : this()
        {
            if (GrpSeqDict.ContainsKey(nm))
                throw new Exception("Duplicate GroupSeq name: " + nm);

            Name = nm;
            GrpSeqDict.Add(nm, this);
        }
    }
}
