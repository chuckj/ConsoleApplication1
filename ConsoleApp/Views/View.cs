using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Colors = System.Drawing.Color;

namespace ConsoleApplication1
{
    public class View
    {
        public static List<View> ViewList = new List<View>();

        //public class ViewMaskEq : EqualityComparer<ViewMask>
        //{
        //    public override int GetHashCode(ViewMask tm)
        //    {
        //        uint cod = 0;
        //        for (int ndx = 0; ndx < tm.Mask.Length; ndx++) cod ^= tm.Mask[ndx];
        //        return cod.GetHashCode();
        //    }

        //    public override bool Equals(ViewMask tm1, ViewMask tm2)
        //    {
        //        for (int ndx = 0; ndx < tm1.Mask.Length; ndx++)
        //            if (tm1.Mask[ndx] != tm2.Mask[ndx]) return false;
        //        return true;
        //    }
        //}

        public string Name { get; private set; }

        public List<Lit> LitArray = new List<Lit>();

        public XElement ViewXML { get; set; }

        protected View()
        {
            ViewList.Add(this);
            Scale = 1;
        }

        public static void Load(XElement root)
        {
            var vus = root.Element("Views").Elements("View").Where(x => (x.Attribute("main") != null) && (bool)x.Attribute("main"));
            foreach (XElement vu in vus)
                Global.Instance.vus.Add(View.Load(root, vu));

        }

        public static View Load(XElement root, string nm)
        {
            XElement vu = root.Element("Views").Elements("View").Where(x => (string)x.Attribute("nm") == nm).FirstOrDefault();
            if (vu == null)
                throw new ArgumentException("View not found: " + nm);

            return Load(root, vu);
        }

        public static View Load(XElement root, XElement vu)
        {
            string nm = (string)vu.Attribute("nm");

            View view = new View();
            view.Name = nm;
            view.ViewXML = vu;

            Global.Instance.VuDict.Add(nm, view);

            List<GECEStrand> strndnu = null;
            //if (vu.Attribute("strand") != null)
            //{
            //    string strndnm = (string)vu.Attribute("strand");
            //    if (!Strand.StrandDict.TryGetValue(strndnm, out strndnu))
            //        throw new Exception("Strand does not exist: " + strndnm);
            //}

            //string xexp = string.Empty, yexp = string.Empty, zexp = string.Empty;
            XAttribute attrb;
            //attrb = vu.Attribute("x");
            //if (!string.IsNullOrEmpty((string)attrb))
            //    xexp = (string)attrb;
            //attrb = vu.Attribute("y");
            //if (!string.IsNullOrEmpty((string)attrb))
            //    yexp = (string)attrb;
            //attrb = vu.Attribute("z");
            //if (!string.IsNullOrEmpty((string)attrb))
            //    zexp = (string)attrb;

            Point3D nuoffset = new Point3D();

            attrb = vu.Attribute("offset");
            if (!string.IsNullOrEmpty((string)attrb))
                nuoffset = (Point3D)attrb;

            Func<Lit, float> nuxexp = null, nuyexp = null, nuzexp = (x) => 0f;
            attrb = vu.Attribute("xexp");
            if (!string.IsNullOrEmpty((string)attrb))
                nuxexp = (Func<Lit, float>)System.Linq.Dynamic.DynamicExpression.ParseLambda(typeof(Lit), typeof(float), (string)attrb, vu).Compile();
            attrb = vu.Attribute("yexp");
            if (!string.IsNullOrEmpty((string)attrb))
                nuyexp = (Func<Lit, float>)System.Linq.Dynamic.DynamicExpression.ParseLambda(typeof(Lit), typeof(float), (string)attrb, vu).Compile();
            attrb = vu.Attribute("zexp");
            if (!string.IsNullOrEmpty((string)attrb))
                nuzexp = (Func<Lit, float>)System.Linq.Dynamic.DynamicExpression.ParseLambda(typeof(Lit), typeof(float), (string)attrb, vu).Compile();

            bldView(root, vu, strndnu, 0, 0, nuoffset, nuxexp, nuyexp, nuzexp, nm, view);

            view.InitDone();
            return view;
        }


        private static void bldView(XElement root, XElement vu, List<GECEStrand> strndlst, int rowoff, int coloff, Point3D offset, 
            Func<Lit, float> xexp, Func<Lit, float> yexp, Func<Lit, float> zexp, string nm, View view)
        {
            int blb = 0;
            int rgbt = 0;

            foreach (XElement lit in vu.Elements())
            {
                string el = lit.Name.LocalName.ToLower();
                switch (el)
                {
                    case "view":
                        {
                            XAttribute attrb;
                            int nurowoff = rowoff, nucoloff = coloff;
                            attrb = lit.Attribute("rowoff");
                            if (!string.IsNullOrEmpty((string)attrb))
                                nurowoff += (Int32)attrb;
                            attrb = lit.Attribute("coloff");
                            if (!string.IsNullOrEmpty((string)attrb))
                                nucoloff += (Int32)attrb;

                            string nunm = nm + ":";
                            if (lit.Attribute("nm") != null)
                                nunm += (string)lit.Attribute("nm");

                            Point3D nuoffset = new Point3D();
                            float nuscale = 1.0f;

                            attrb = lit.Attribute("offset");
                            if (!string.IsNullOrEmpty((string)attrb))
                                nuoffset = (Point3D)attrb;
                            attrb = lit.Attribute("scale");
                            if (!string.IsNullOrEmpty((string)attrb))
                                nuscale = (float)attrb;

                            nuoffset = nuoffset + offset;

                            string vunm = (string)lit.Attribute("view");
                            XElement vunu = root.Element("Views").Elements("View").Where(x => (string)x.Attribute("nm") == vunm).First();
                            if (vunu == null)
                                throw new Exception("view required");

                            Func<Lit, float> nuxexp = xexp, nuyexp = yexp, nuzexp = zexp;
                            object a = xexp;

                            attrb = vunu.Attribute("xexp") ?? lit.Attribute("xexp");
                            if (!string.IsNullOrEmpty((string)attrb))
                                nuxexp = (Func<Lit, float>)System.Linq.Dynamic.DynamicExpression.ParseLambda(typeof(Lit), typeof(float), (string)attrb, vu).Compile();
                            attrb = vunu.Attribute("yexp") ?? lit.Attribute("yexp");
                            if (!string.IsNullOrEmpty((string)attrb))
                                nuyexp = (Func<Lit, float>)System.Linq.Dynamic.DynamicExpression.ParseLambda(typeof(Lit), typeof(float), (string)attrb, vu).Compile();
                            attrb = vunu.Attribute("zexp") ?? lit.Attribute("zexp");
                            if (!string.IsNullOrEmpty((string)attrb))
                                nuzexp = (Func<Lit, float>)System.Linq.Dynamic.DynamicExpression.ParseLambda(typeof(Lit), typeof(float), (string)attrb, vu).Compile();

                            List<GECEStrand> strndnu = strndlst;
                            if (lit.Attribute("strand") != null)
                            {
                                string strnds = (string)lit.Attribute("strand");
                                strndnu = new List<GECEStrand>();
                                foreach (string strndnm in strnds.Split(','))
                                {
                                    GECEStrand strand;
                                    if (!Global.Instance.StrandDict.TryGetValue(strndnm, out strand))
                                        throw new Exception("Strand does not exist: " + strndnm);
                                    strndnu.Add(strand);
                                }
                            }

                            bldView(root, vunu, strndnu, nurowoff, nucoloff, nuoffset, nuxexp, nuyexp, nuzexp, nunm, view);

                        }
                        break;

                    case "rgb":
                        {
                            if (strndlst == null)
                                throw new Exception("Strand is required before lite: " + nm + ":" + blb.ToString());

                            bool hasexp = (xexp != null) && (yexp != null) && (zexp != null);
                            int cnt = 0, rowinc = 0, colinc = 0, cirinc = 0;
                            //float xinc = 0, yinc = 0, zinc = 0;
                            XAttribute attrb;
                            Point3D ptoffset = new Point3D();
                            if (lit.Attribute("repeat") != null)
                            {
                                cnt = (Int32)lit.Attribute("repeat");

                                attrb = lit.Attribute("rowinc");
                                if (!string.IsNullOrEmpty((string)attrb))
                                    rowinc = (Int32)attrb;
                                attrb = lit.Attribute("colinc");
                                if (!string.IsNullOrEmpty((string)attrb))
                                    colinc = (Int32)attrb;
                                if ((colinc == 0) && (rowinc == 0))
                                    throw new Exception("rowinc or colinc in required with cnt");
                                attrb = lit.Attribute("cirinc");
                                if (!string.IsNullOrEmpty((string)attrb))
                                    cirinc = (Int32)attrb;
                                attrb = lit.Attribute("ptoffset");
                                if (string.IsNullOrEmpty((string)attrb))
                                {
                                    if (!hasexp)
                                        throw new ArgumentNullException("ptoffset");
                                }
                                else
                                    ptoffset = (Point3D)attrb;
                            }
                            int rw = (Int32)lit.Attribute("row");
                            int cl = (Int32)lit.Attribute("col");
                            int cir = 0;
                            if ((string)lit.Attribute("cir") != null)
                                cir = (Int32)lit.Attribute("cir");

                            Point3D point = new Point3D();
                            attrb = lit.Attribute("pt");
                            if (string.IsNullOrEmpty((string)attrb))
                            {
                                if (!hasexp)
                                    throw new MissingFieldException("pt");
                            }
                            else
                                point = (Point3D)attrb;

                            GECEStrand strand;
                            if (strndlst.Count == 1)
                                strand = strndlst[0];
                            else
                            {
                                if (lit.Attribute("strandndx") == null)
                                    throw new Exception("strandndx is required.");
                                int strandndx = (Int32)lit.Attribute("strandndx");
                                strand = strndlst[strandndx];
                            }

                            Point3D repoffset = new Point3D();
                            do
                            {
                                Clr clr = Colors.DarkGray;
                                if (lit.Attribute("color") != null)
                                    clr = Clr.FromName((string)lit.Attribute("color"));
                                RGBLit rgb = new GECELit(nm + ":" + blb.ToString())
                                {
                                    Strnd = strand,
                                    Index = strand.lites.Count,
                                    Row = rw + rowoff,
                                    Column = cl + coloff,
                                    Circle = cir,
                                    Clr = clr,

                                };

                                if (hasexp)
                                { 
                                    point = new Point3D((xexp != null) ? xexp(rgb) : 0, (yexp != null) ? yexp(rgb) : 0, (zexp != null) ? zexp(rgb) : 0);
                                }
                                rgb.Pt = point + offset + repoffset;

                                if ((string)lit.Attribute("ndx") != null)
                                    rgb.Index = (Int32)lit.Attribute("ndx");

                                strand.lites.Add(rgb);
                                view.LitArray.Add(rgb);

                                blb++;
                                rw += rowinc;
                                cl += colinc;
                                cir += cirinc;
                                repoffset += ptoffset;
                                cnt--;
                            } while (cnt > 0);
                        }
                        break;

                    case "mono":
                        {
                            int rw = (Int32)lit.Attribute("row");
                            int cl = (Int32)lit.Attribute("col");
                            int cir = 0;
                            if ((string)lit.Attribute("cir") != null)
                                cir = (Int32)lit.Attribute("cir");

                            Clr clr = Colors.DarkGray;
                            if (lit.Attribute("color") != null)
                                clr = Clr.FromName((string)lit.Attribute("color"));
                            MonoLit mono = new MonoLit(nm + ":" + blb.ToString(), clr)
                            {
                                //Strnd = strnd, 
                                Row = rw + rowoff,
                                Column = cl + coloff,
                                Circle = cir,
                                MarqueNdx = string.IsNullOrEmpty((string)lit.Attribute("mrqrow")) ? -1 : (int)lit.Attribute("mrqrow"),
                                MarqueMask = (uint)(string.IsNullOrEmpty((string)lit.Attribute("mrqcol")) ? 0 : (1 << (int)lit.Attribute("mrqcol"))),
                            };
                            if ((string)lit.Attribute("ndx") != null)
                                mono.Index = (Int32)lit.Attribute("ndx");

                            Point3D point = new Point3D();
                            XAttribute attrb = lit.Attribute("point");
                            if (!string.IsNullOrEmpty((string)attrb))
                            {
                                point = (Point3D)attrb;
                            }
                            else if (xexp != null && yexp != null && zexp != null)
                            {
                                point = new Point3D(xexp(mono), yexp(mono), zexp(mono));
                            }
                            else
                                throw new MissingFieldException("point");
                            mono.Pt = point + offset;

                            view.LitArray.Add(mono);

                            blb++;
                        }
                        break;

                    case "line":
                        {

                            XAttribute attrb;
                            attrb = lit.Attribute("color");
                            var clr = FeatureLit.FromName(string.IsNullOrEmpty((string)attrb) ? "Red" : (string)attrb);

                            int repeat = 1;
                            Point3D ptoffset = new Point3D();
                            attrb = lit.Attribute("repeat");
                            if (!string.IsNullOrEmpty((string)attrb))
                            {
                                repeat = (int)attrb;
                                attrb = lit.Attribute("ptoffset");
                                if (string.IsNullOrEmpty((string)attrb))
                                    throw new ArgumentNullException("ptoffset");
                                ptoffset = (Point3D)attrb;
                            }

                            List<Point3D> pts = new List<Point3D>();
                            for (int ndx = 1; ; ndx++)
                            {
                                attrb = lit.Attribute("pt" + ndx.ToString());
                                if (string.IsNullOrEmpty((string)attrb)) break;
                                pts.Add((Point3D)attrb);
                            }

                            attrb = lit.Attribute("close");
                            bool close = (!string.IsNullOrEmpty((string)attrb));

                            var repoffset = new Point3D();
                            for (int ndx = 0; ndx < repeat; ndx++)
                            {
                                short startNdx = (short)Global.Instance.LineVertices.Count;
                                foreach (var pt in pts)
                                {
                                    Global.Instance.LineIndices.Add((short)Global.Instance.LineVertices.Count);
                                    Global.Instance.LineVertices.Add(new IndexPoint3D(pt + offset + repoffset, clr.GlobalIndex));
                                }
                                if (close)
                                    Global.Instance.LineIndices.Add(startNdx);

                                Global.Instance.LineIndices.Add(-1);

                                repoffset += ptoffset;
                            }
                        }
                        break;

                    case "triangle":
                        {
                            XAttribute attrb;
                            attrb = lit.Attribute("color");
                            var clr = FeatureLit.FromName(string.IsNullOrEmpty((string)attrb) ? "Red" : (string)attrb);

                            for (int ndx = 1; ; ndx++)
                            {
                                attrb = lit.Attribute("pt" + ndx.ToString());
                                if (string.IsNullOrEmpty((string)attrb)) break;
                                Point3D pt = (Point3D)attrb + offset;
                                Global.Instance.TriIndices.Add((short)Global.Instance.TriVertices.Count);
                                Global.Instance.TriVertices.Add(new IndexPoint3D(pt, clr.GlobalIndex));
                            }

                            Global.Instance.TriIndices.Add(-1);
                        }
                        break;

                    case "dmx":
                        {
                            var dmxStrand = Global.Instance.DMXStrand;

                            DMXLit dmx = new DMXLit(nm + ":dmx:" + rgbt++)
                            {
                                Strnd = dmxStrand,
                                Index = dmxStrand.lites.Count,
                                Clr = Clr.FromName("darkred"),
                            };

                            dmxStrand.lites.Add(dmx);
                            view.LitArray.Add(dmx);

                            foreach (XElement trig in lit.Elements())
                            {
                                string tri = trig.Name.LocalName.ToLower();
                                switch (tri)
                                {
                                    case "triangle":
                                        {
                                            XAttribute attrb;
                                            for (int ndx = 1; ; ndx++)
                                            {
                                                attrb = trig.Attribute("pt" + ndx);
                                                if (string.IsNullOrEmpty((string)attrb)) break;
                                                Point3D pt = (Point3D)attrb + offset;

                                                //if (ndx == 3)
                                                //    dmx.LiteNdx.Add(Global.Instance.TriVertices.Count);
                                                //else
                                                //    dmx.DimNdx.Add(Global.Instance.TriVertices.Count);

                                                Global.Instance.TriIndices.Add((short)Global.Instance.TriVertices.Count);
                                                Global.Instance.TriVertices.Add(new IndexPoint3D(pt, (ndx == 3) ? dmx.GlobalIndex : dmx.GlobalIndex | 0x00010000 ));
                                            }

                                            Global.Instance.TriIndices.Add(-1);
                                        }
                                        break;
                                }
                            }

                        }
                        break;

                    //case "rgbtriangle":
                    //    {
                    //        XAttribute attrb;
                    //        string distance = string.Empty;

                    //        attrb = lit.Attribute("color");
                    //        if (string.IsNullOrEmpty((string)attrb))
                    //            throw new Exception("rgbTriangle requires 'color' attrb.");
                    //        Clr clr = Clr.FromName((string)attrb);


                    //        DMXLit rgb = new DMXLit(nm + ":rgbt:" + rgbt++)
                    //        {
                    //            Strnd = dmxStrand,
                    //            Index = dmxStrand.lites.Count,
                    //            Clr = clr,
                    //        };

                    //        if ((string)lit.Attribute("ndx") != null)
                    //            rgb.Index = (Int32)lit.Attribute("ndx");

                    //        dmxStrand.lites.Add(rgb);
                    //        view.LitArray.Add(rgb);

                    //        for (int ndx = 1; ; ndx++)
                    //        {
                    //            attrb = lit.Attribute("pt" + ndx);
                    //            if (string.IsNullOrEmpty((string)attrb)) break;
                    //            Point3D pt = (Point3D)attrb + offset;

                    //            attrb = lit.Attribute("clr" + ndx);
                    //            if (attrb != null)
                    //                rgb.LiteNdx.Add(Global.Instance.TriVertices.Count);
                    //            else
                    //                rgb.DimNdx.Add(Global.Instance.TriVertices.Count);

                    //            Global.Instance.TriIndices.Add((short)Global.Instance.TriVertices.Count);
                    //            Global.Instance.TriVertices.Add(new ColorPoint3D(pt, clr));
                    //        }

                    //        Global.Instance.TriIndices.Add(-1);
                    //    }
                    //    break;
                }
            }
            //LitGrp.Build(vu, nm, rowoff, coloff);

            //LitGrpSeq.Build(vu, nm);
        }

        //private Point[] poly = new Point[0];
        //private GraphicsPath gp;
        //public float skewAngle = 45;
        //public float skewLength = .5f;
        private Point loc = new Point(0, 0);
        //private Size size = new Size(0, 0);
        //private Size orgSize;
        //private Point orgLoc;
        //private GraphicsPath orgGp;

        public void InitDone()
        {
            //List<Lit> litArray = this.LitArray;
            //int minX = litArray.Min(b => b.Loc.X) - 5;
            //int minY = litArray.Min(b => b.Loc.Y) - 5;
            //int maxX = litArray.Max(b => b.Loc.X);
            //int maxY = litArray.Max(b => b.Loc.Y);
            //size = new Size(maxX + 7 - minX, maxY + 7 - minY);
            //Size offset = new Size(minX, minY);
            //foreach (Lit lit in LitArray)
            //    lit.Loc -= offset;
            //poly = ConvexHull(litArray);
            //gp = new GraphicsPath();
            ////if (poly.Length > 0)
            ////    gp.AddPolygon(poly);
            //int lng = poly.Length;

            //Point p0, p1, p2;
            //p0 = poly[lng - 2];
            //p1 = poly[lng - 1];
            //float f1, f2;
            //f1 = 270 - (float)Angle(p0, p1);
            //for (int ndx = 0; ndx < lng; ndx++)
            //{
            //    p2 = poly[ndx];
            //    f2 = 270 - (float)Angle(p1, p2);
            //    float alng = f2 - f1;
            //    if (alng < 0) alng += 360;
            //    gp.AddArc(p1.X - 5, p1.Y - 5, 11, 11, f1, alng);

            //    p0 = p1;
            //    p1 = p2;
            //    f1 = f2;
            //}
            //gp.CloseFigure();
            //resetPolyAddLoc(new Point(0, 0), loc);
        }


        private Point[] ConvexHull(List<Lit> lits)
        {
            List<Point> pts = lits.Select(b => b.Loc).OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            Point p0 = pts[0];
            pts = pts.Skip(1).OrderBy(p => (p.Y - p0.Y) / ((p.X == p0.X) ? .000001f : (float)(p.X - p0.X))).ToList();
            List<Point> stk = new List<Point>(pts.Count);
            stk.Add(p0);
            int ndx = 0;
            foreach (Point p in pts)
            {
                while ((ndx > 0) && IsRight(stk[ndx - 1], stk[ndx], p))
                    stk.RemoveAt(ndx--);
                stk.Add(p);
                ndx++;
            }
            return stk.ToArray();
        }


        private bool IsRight(Point p1, Point p2, Point p3) => (p2.Y - p1.Y) * (p3.X - p2.X) >= (p3.Y - p2.Y) * (p2.X - p1.X);

        private double Angle(Point p0, Point p1)
        {
            int dY = p0.Y - p1.Y;
            int dX = p1.X - p0.X;
            double ang = Math.Atan(dY / (double)dX) * 180 / Math.PI;
            if (dX < 0) ang += 180;
            if (ang < 0) ang += 360;
            return ang;
        }

        //public GraphicsPath GP
        //{
        //    get
        //    {
        //        return gp;
        //    }
        //}

        //public Size Size
        //{
        //    get
        //    {
        //        return size;
        //    }
        //}

        //public Rectangle Rectangle
        //{
        //    get
        //    {
        //        return new Rectangle(loc, size);
        //    }
        //}

        public Point Loc
        {
            set
            {
                resetPolyAddLoc(loc, value);
                loc = value;
            }
            get
            {
                return loc;
            }
        }

        public int Entered { get; set; }

        private void resetPolyAddLoc(Point oldLoc, Point newLoc)
        {
        //    if (gp != null)
        //    {
        //        Matrix mat = new Matrix();
        //        mat.Translate(newLoc.X - oldLoc.X, newLoc.Y - oldLoc.Y);
        //        gp.Transform(mat);
        //    }
        }

        public bool IsVisible(Point pt)
        {
            // return gp.IsVisible(pt);
            throw new NotImplementedException();
        }

        //public GraphicsPath GPOffset(Size loc)
        //{
        //    GraphicsPath gp0 = (GraphicsPath)gp.Clone();
        //    Matrix mat = new Matrix();
        //    mat.Translate(loc.Width, loc.Height);
        //    gp0.Transform(mat);
        //    return gp0;
        //}

        //public bool Maximized { get; set; }
        public int Scale { get; set; }

        //public void Maximize(int wid, int ht)
        //{
        //    if (!Maximized)
        //    {
        //        orgLoc = loc;
        //        orgSize = size;
        //        orgGp = (GraphicsPath)gp.Clone();
        //        int scale = (int)Math.Min(wid / (float)size.Width, ht / (float)size.Height);
        //        size = new Size(size.Width * scale, size.Height * scale);
        //        loc = new Point((wid - size.Width) / 2, (ht - size.Height) / 2);
        //        Matrix mat = new Matrix();
        //        mat.Translate(loc.X, loc.Y);
        //        mat.Scale(scale, scale);
        //        mat.Translate(-orgLoc.X, -orgLoc.Y);
        //        gp.Transform(mat);
        //        Scale = scale;
        //        Maximized = true;
        //    }
        //}

        //public void Restore()
        //{
        //    if (Maximized)
        //    {
        //        loc = orgLoc;
        //        size = orgSize;
        //        gp = orgGp;
        //        Scale = 1;
        //        Maximized = false;
        //    }
        //}
    }
}
