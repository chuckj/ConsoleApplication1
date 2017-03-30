using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    public class LevelSeq
    {
        public static List<LevelSeq> LevelSeqList = new List<LevelSeq>();
        public static Dictionary<string, LevelSeq> LevelSeqDict = new Dictionary<string, LevelSeq>();

        public static void Load(XElement root)
        {
            XElement xpn = root.Element("LevelSeqs");
            foreach (XElement cur in xpn.Elements("LevelSeq"))
            {
                string nm = (string)cur.Attribute(@"nm");
                LevelSeq lvlseq = new LevelSeq(nm);
                LevelSeq.LevelSeqDict.Add(nm, lvlseq);

                lvlseq.Type = ((string)cur.Attribute(@"typ")).ToLower();
                switch (lvlseq.Type)
                {
                    //case "mono":
                    //    string wrk = (string)xpngl.Attribute(@"stop");
                    //    string stop = (wrk.Length > 0) ? int.Parse(wrk) : strt;

                    //    bgn = (Int32)xpngl.Attribute(@"bgn");
                    //    wrk = (string)xpngl.Attribute(@"end");
                    //    end = (wrk.Length > 0) ? int.Parse(wrk) : bgn;
                    //    wrk = (string)xpngl.Attribute(@"grp");
                    //    Grp grp1 = null;
                    //    if (wrk != null)
                    //        Grp.GrpDict.TryGetValue(wrk, out grp1);
                    //    ani = new MonoAnim(strt, stop, bgn, end, grp1);
                    //    break;

                    //case "rgb":
                    //    wrk = (string)xpngl.Attribute(@"stop");
                    //    stop = (wrk.Length > 0) ? int.Parse(wrk) : strt;

                    //    bgn = (Int32)xpngl.Attribute(@"bgn");
                    //    wrk = (string)xpngl.Attribute(@"end");
                    //    end = (wrk.Length > 0) ? int.Parse(wrk) : bgn;
                    //    ani = new MonoAnim(strt, stop, bgn, end, null);
                    //    break;

                    case "rgbi":
                        lvlseq.Levels = new List<Level>();
                        foreach (XElement lvl in cur.Elements("Level"))
                            lvlseq.Levels.Add(Level.RGBIParsex((string)lvl.Attribute(@"val")));
                        break;

                    //case "seq":
                    //    nm = (string)xpngl.Attribute(@"nm");
                    //    SeqAnim seqa = new SeqAnim(strt, nm);
                    //    ani = seqa;
                    //    wrk = (string)xpngl.Attribute(@"grp");
                    //    Grp grp2 = null;
                    //    if (Grp.GrpDict.TryGetValue(wrk, out grp2))
                    //        seqa.Grp = grp2;
                    //    break;

                    default:
                        throw new Exception("Invalid Level typ: " + lvlseq.Type);
                }
            }
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public List<Level> Levels { get; set; }

        public LevelSeq(string nm)
        {
            Name = nm;
            Levels = new List<Level>();
        }
    }
}
