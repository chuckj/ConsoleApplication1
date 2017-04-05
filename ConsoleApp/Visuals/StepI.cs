using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Threading.Tasks;
using SD = System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApplication1
{
    [Step("stepi")]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class StepI : StepBase, IRunTime
    {
        #region Statics
        private static log4net.ILog logger = log4net.LogManager.GetLogger(nameof(StepI));

        //private static StepBaseRes resources;

        static StepI()
        {
            //resources = new StepBaseRes();
        }
        #endregion

        #region Debugger
        public override string DebuggerDisplay => $"StepI: {base.DebuggerDisplayShort} Mode:{mode} Text:{text}";
        #endregion

        #region Locals
        public int mode;

        private BitMapImg bmi;

        private StepTransition Transition;

        private SD.Color color;

        private Regex regex;
        #endregion

        #region Ctors
        public override Viz Factory(XElement xml, Song song, int measureOffset) => new StepI(xml, song, measureOffset);

        public StepI(XElement xml, Song song, int measureOffset) : base(xml, song, measureOffset)
        {
            this.mode = int.Parse((string)xml.Attribute("mode"));
            string img = (string)xml.Attribute("img");
            if (img != null)
            {
                if (!Global.Instance.BitMapImgs.TryGetValue(Path.GetFileNameWithoutExtension(img), out bmi))
                    throw new Exception($"BMI '{img}' not found.");
            }

            var clr = (string)xml.Attribute("color");
            if (clr != null)
                this.color = SD.Color.FromName(clr);

            StepTransition trans = null;
            var nam = (string)xml.Attribute("transition");
            if (nam != null)
            {
                if (!Global.Instance.StepTransitionDict.TryGetValue(nam, out trans))
                    throw new Exception($"Transition '{nam}' not found.");
            }
            else
            {
            }
            if (trans == null)
                throw new Exception($"Expecting transition name or transitions element.");

            Transition = trans;

            var regx = (string)xml.Attribute("regex");
            if (regx == null)
                throw new Exception($"Regex not found.");

            regex = new Regex(regx, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
        #endregion


        private int[,] bgnndx;
        private RunTime context;
        private SD.Color[,] before;

        public IEnumerable<int> Xeq(RunTime context)
        {
            this.context = context;

            //if (mode == 1)                          //  appear one row @ a time - left to right
            //{
            //    int intr = (EndPoint.X - StartPoint.X) / 20;
            //    for (int row = 0; row < 9; row++)
            //    {
            //        for (int col = 0; col < 11; col++)
            //            context[bgnndx[row, col]] = bmi.Colors[bmi.Indices[row, col]];

            //        yield return StartPoint.X + intr * (row + 1);
            //    }
            //}
            //else if (mode == 2 || mode == 4)        // 2 = slide in from right and then out to left, 4 - slide in from right & stop
            //{
            //    for (int lup = 0; lup < 11; lup+=2)
            //    {
            //        for (int col = 0; col <= lup; col++)
            //            copycol(col, 10 - lup + col);

            //        yield return context.CurTime + 4;
            //    }

            //    if (mode == 2)
            //    {
            //        for (int lup = 2; lup <= 12; lup += 2)
            //        {
            //            int col;
            //            for (col = lup; col < 11 && col - lup < 11; col++)
            //                copycol(col, col - lup);
            //            for (col = 11 - lup; col < 11; col++) 
            //                if (col >= 0)
            //                    restorecol(col); 

            //            yield return context.CurTime + 4;

            //        };
            //    }
            //    yield break;
            //}
            //else if (mode == 3)
            //{
            //    int thrds = (EndPoint.X - StartPoint.X) / 3;
            //    int intr = thrds / 10;
            //    for (int row = 0; row < 9; row++)
            //    {
            //        for (int col = 0; col < 11; col++)
            //            context[bgnndx[row, col]] = bmi.Colors[bmi.Indices[row, col]];

            //        yield return context.CurTime + intr;
            //    }

            //    yield return StartPoint.X + thrds;

            //    int pre = 0;
            //    int nxt = 11;
            //    int starttime = context.CurTime;
            //    int endtime = context.CurTime + thrds;
            //    while (context.CurTime < endtime)
            //    {
            //        float fraction = 2 * (context.CurTime - starttime) / (float)thrds;
            //        if (fraction > 1) break;
            //        for (int row = 0; row < 9; row++)
            //        {
            //            for (int col = 0; col < 11; col++)
            //                context[bgnndx[row, col]] = factor(bmi.Colors[bmi.Indices[row, col + pre]], bmi.Colors[bmi.Indices[row, col + nxt]], fraction);
            //        }
            //        yield return context.CurTime + 4;
            //    }

            //    yield return endtime;

            //    pre = 11;
            //    nxt = 22;
            //    starttime = context.CurTime;
            //    endtime = EndPoint.X;
            //    while (context.CurTime < endtime)
            //    {
            //        float fraction = 2 * (context.CurTime - starttime) / (float)thrds;
            //        if (fraction > 1) break;
            //        for (int row = 0; row < 9; row++)
            //        {
            //            for (int col = 0; col < 11; col++)
            //                context[bgnndx[row, col]] = factor(bmi.Colors[bmi.Indices[row, col + pre]], bmi.Colors[bmi.Indices[row, col + nxt]], fraction);
            //        }
            //        yield return context.CurTime + 4;
            //    }
            //}
            bool first = true;

            foreach (var tran in Transition.Transitions)
            {
                if (tran is StepTransition_Position)
                {
                    if (first)
                    {
                        first = false;
                        bgnndx = new int[9, 11];
                        int ndx = 0;
                        foreach (var itm in Global.Instance.LitDict.Values.OfType<Lit>()
                            .Where(v => regex.IsMatch(v.Name))
                            .OrderByDescending(v => v.Row).ThenBy(v => v.Column))
                        {
                            bgnndx[(ndx / 11), (ndx++ % 11)] = itm.GlobalIndex;
                        }

                        before = new SD.Color[9, 11];
                        for (int row = 0; row < 9; row++)
                            for (int col = 0; col < 11; col++)
                                before[row, col] = context[bgnndx[row, col]];
                    }

                    var poser = (StepTransition_Position)tran;
                    int x = poser.X;
                    int y = poser.Y;
                    int steps = 0;
                    do
                    {
                        for (int col = 0; col < 11; col++)
                            restorecol(col);

                        for (int row = 0, srcrow = row - y; row < 9; row++, srcrow++)
                            if ((srcrow >= 0) && (srcrow < 9))
                                for (int col = 0, srccol = col - x; col < 11; col++, srccol++)
                                    if ((srccol >= 0) && (srccol < 11))
                                        context[bgnndx[row, col]] = bmi.Pixels.Pixels[bmi.Indices[srcrow, srccol]];

                        x += poser.XStep;
                        y += poser.YStep;
                        steps++;

                        yield return context.CurTime + 4;
                    } while (steps <= poser.Steps);

                }
                else if (tran is StepTransition_Fade)
                {
                    var fader = (StepTransition_Fade)tran;

                    switch (fader.Fade)
                    {
                        case 10:
                            {
                                //var fader = (StepTransition_Fade)tran;
                                bgnColor[] initClr = Global.Instance.LitDict.Values.OfType<Lit>()
                                    .Where(v => regex.IsMatch(v.Name))
                                    .Select(l => new bgnColor() { clr = context.Colors[l.GlobalIndex], Ndx = l.GlobalIndex })
                                    .ToArray();
                                float start = this.startTimeMark.Time;
                                float endTime = this.endTimeMark.Time;

                                int fade = 0;
                                do
                                {
                                    float fraction;
                                    if (start >= endTime)
                                        fraction = 1;
                                    else
                                        fraction = Math.Min((context.CurTime - start) / (endTime - start), 1);

                                    foreach (var bgn in initClr)
                                    {
                                        context[bgn.Ndx] = factor(bgn.clr, this.color, fraction);
                                    }

                                    fade++;

                                    yield return context.CurTime + 4;
                                } while (context.CurTime < endTime);
                            }
                            break;

                        case 0:
                            {
                                bgnColor[] initClr = Global.Instance.LitDict.Values.OfType<Lit>()
                                   .Where(v => regex.IsMatch(v.Name))
                                   .Select(l => new bgnColor() { clr = context[l.GlobalIndex], Ndx = l.GlobalIndex })
                                   .ToArray();

                                foreach (var bgn in initClr)
                                    context[bgn.Ndx] = this.color;
                            }
                            break;

                        case 1:
                            {
                                bgnColor[] initClr = Global.Instance.LitDict.Values.OfType<Lit>()
                                    .Where(v => regex.IsMatch(v.Name))
                                    .Select(l => new bgnColor() { clr = context[(int)l.GlobalIndex], Ndx = (int)l.GlobalIndex })
                                    .ToArray();

                                foreach (var bgn in initClr)
                                    context[bgn.Ndx] = this.color;

                                yield return context.CurTime + 4;

                                foreach (var bgn in initClr)
                                    context[bgn.Ndx] = bgn.clr;

                            }
                            break;
                    }
                }
            }

            yield break;

            //yield return EndPoint.X;

            //for (int row = 0; row < 9; row++)
            //    for (int col = 0; col < 11; col++)
            //        context[bgnndx[row, col]] = System.Drawing.Color.Black;
        }

        private SD.Color factor(Clr bgn, SD.Color end, float fraction)
        {
            fraction = Math.Max(Math.Min(fraction, 1), 0);
            int red = Math.Min((int)(((uint)bgn.R) * (1 - fraction) + ((uint)end.R) * fraction), 255);
            int green = Math.Min((int)(((uint)bgn.G) * (1 - fraction) + ((uint)end.G) * fraction), 255);
            int blue = Math.Min((int)(((uint)bgn.B) * (1 - fraction) + ((uint)end.B) * fraction), 255);
            return SD.Color.FromArgb(red, green, blue);
        }

        private class bgnColor
        {
            public SD.Color clr;
            public int Ndx;
        }

        private void copycol(int from, int to)
        {
            for (int row = 0; row < 9; row++)
            {
                context[bgnndx[row, to]] = bmi.Pixels.Pixels[bmi.Indices[row, from]];
            }
        }
        private void restorecol(int col)
        {
            for (int row = 0; row < 9; row++)
            {
                context[bgnndx[row, col]] = before[row, col];
            }
        }
    }
}
