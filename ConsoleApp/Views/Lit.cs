using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1
{
    public class Lit
    {
        private static int globalIndex = 0;
        private Clr initVal;
        public static List<Lit> LitList
        {
            get { return Global.Instance.LitDict.Values.ToList(); }
            private set { }
        }

 
        private int index;

        public string Name { get; private set; }
        //public byte[] Program { get; set; }
        private Clr val;
        public int Row { get; set; }
        public int Column { get; set; }
        public Point3D Pt { get; set; }
        public int Circle { get; set; }
        public Point Loc { get; set; }
        public int Index { get { return index; } set { index = value; setIndex(value); } }

        public int GlobalIndex { get; }

        protected Lit()
        {
            GlobalIndex = globalIndex++;
        }

        public Lit(string nm)
            : this()
        {
            if (Global.Instance.LitDict.ContainsKey(nm))
                throw new Exception("Duplicate Lit key: " + nm);

            Name = nm;
            Global.Instance.LitDict.Add(nm, this);
        }

        public Lit(string nm, Clr clr)
            : this(nm)
        {
            initVal = val = clr;
        }

        public virtual string Attributes() => string.Empty;

        public virtual void Set(Clr val, Pgm pgm)
        {
            this.val = val;
        }

        public Clr InitVal { get { return initVal; } }

        public virtual Clr Get() => val;

        public virtual Clr Parse(string txt) => Clr.Parsex(txt);

        private int snapshowrow = -1;

        public int SnapshotRow
        {
            get { return snapshowrow; }
            set { snapshowrow = value; }
        }

        public override string ToString()
        {
            string attrb = Attributes();
            if (attrb.Length > 0)
                attrb = "{" + attrb.Remove(0, 1) + "}";
            return Name + attrb;
        }

        public virtual void setIndex(int value)
        {

        }
        //public virtual void Draw(Graphics g, Rectangle rect)
        //{
        //    g.FillEllipse(new SolidBrush(Get()), rect);
        //}

        public virtual HitResult Hit(HitData data)
        {
            return null;
        }
    }
}
