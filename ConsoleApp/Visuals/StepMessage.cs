using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using SD = System.Drawing;

namespace ConsoleApplication1
{
    [Step("stepmsg")]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class StepMessage : StepBase, IRunTime
    {
        #region Statics
        static StepMessage()
        {
        }
        #endregion

        #region Debugger
        public override string DebuggerDisplay => $"StepMsg: msg: {msg} {base.DebuggerDisplayShort}";
        #endregion

        #region Ctors
        public override Viz Factory(XElement xml, Song song, int measureOffset) => new StepMessage(xml, song, measureOffset);

        public StepMessage(XElement xml, Song song, int measureOffset) : base(xml, song, measureOffset)
        {
            this.msg = (string)xml.Attribute("msg");
            var val = (string)xml.Attribute("steptime");
            if (!string.IsNullOrEmpty(val))
                steptime = int.Parse(val);
        }
        #endregion

        private string msg;
        private int steptime = 16;
        private uint[] shift;
        private RunTime context;

        public IEnumerable<int> Xeq(RunTime context)
        {
            this.context = context;

            shift = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            uint _________ = 0;

            for (int ndx = 0; ndx < msg.Length; ndx++)
            {
                FontUpdate font;
                Global.Instance.FontDict.TryGetValue(msg[ndx], out font);

                //            <------ New Character ------>  <------------- Current display -------------->
                //            current         keep-clear     above       current     below       keep-clear
                while (
                    interferes(font.Bits[00] | font.Bits[15], _________ | shift[00] | shift[01] | shift[15]) ||
                    interferes(font.Bits[01] | font.Bits[15], shift[00] | shift[01] | shift[02] | shift[15]) ||
                    interferes(font.Bits[02] | font.Bits[15], shift[01] | shift[02] | shift[03] | shift[15]) ||
                    interferes(font.Bits[03] | font.Bits[15], shift[02] | shift[03] | shift[04] | shift[15]) ||
                    interferes(font.Bits[04] | font.Bits[15], shift[03] | shift[04] | shift[05] | shift[15]) ||
                    interferes(font.Bits[05] | font.Bits[15], shift[04] | shift[05] | shift[06] | shift[15]) ||
                    interferes(font.Bits[06] | font.Bits[15], shift[05] | shift[06] | shift[07] | shift[15]) ||
                    interferes(font.Bits[07] | font.Bits[15], shift[06] | shift[07] | shift[08] | shift[15]) ||
                    interferes(font.Bits[08] | font.Bits[15], shift[07] | shift[08] | shift[09] | shift[15]) ||
                    interferes(font.Bits[09] | font.Bits[15], shift[08] | shift[09] | shift[10] | shift[15]) ||
                    interferes(font.Bits[10] | font.Bits[15], shift[09] | shift[10] | shift[11] | shift[15]) ||
                    interferes(font.Bits[11] | font.Bits[15], shift[10] | shift[11] | shift[12] | shift[15]) ||
                    interferes(font.Bits[12] | font.Bits[15], shift[11] | shift[12] | shift[13] | shift[15]) ||
                    interferes(font.Bits[13] | font.Bits[15], shift[12] | shift[13] | shift[14] | shift[15]) ||
                    interferes(font.Bits[14] | font.Bits[15], shift[13] | shift[14] | _________ | shift[15]))
                {
                    doShift();
                    yield return context.CurTime + steptime;
                }

                doShift();
                yield return context.CurTime + steptime;

                shift[0] |= font.Bits[0];
                shift[1] |= font.Bits[1];
                shift[2] |= font.Bits[2];
                shift[3] |= font.Bits[3];
                shift[4] |= font.Bits[4];
                shift[5] |= font.Bits[5];
                shift[6] |= font.Bits[6];
                shift[7] |= font.Bits[7];
                shift[8] |= font.Bits[8];
                shift[9] |= font.Bits[9];
                shift[10] |= font.Bits[10];
                shift[11] |= font.Bits[11];
                shift[12] |= font.Bits[12];
                shift[13] |= font.Bits[13];
                shift[14] |= font.Bits[14];
                shift[15] |= font.Bits[15];
            }

            while (((shift[0] | shift[1] | shift[2] | shift[3] | shift[4] | shift[5] |
                shift[7] | shift[8] | shift[9] | shift[10] | shift[11] | shift[12] |
                shift[13] | shift[14]) & 0xffffff) != 0)
            {
                doShift();
                yield return context.CurTime + steptime;
            }
        }

        private bool interferes(uint onDeck, uint atBat)
        {
            return ((atBat | (atBat << 1) | (atBat << 2) | (atBat << 3) | (atBat << 4) | (atBat << 5) | (atBat << 6) | (atBat << 7)) & 0xff & onDeck) != 0;
        }

        private void doShift()
        {
            var upd = new[] {
                (shift[0]) & 0xffff00,
                (shift[1]) & 0xffff00,
                (shift[2]) & 0xffff00,
                (shift[3]) & 0xffff00,
                (shift[4]) & 0xffff00,
                (shift[5]) & 0xffff00,
                (shift[6]) & 0xffff00,
                (shift[7]) & 0xffff00,
                (shift[8]) & 0xffff00,
                (shift[9]) & 0xffff00,
                (shift[10]) & 0xffff00,
                (shift[11]) & 0xffff00,
                (shift[12]) & 0xffff00,
                (shift[13]) & 0xffff00,
                (shift[14]) & 0xffff00};

            foreach (var td in Global.Instance.dta)
            {
                if ((td.MarqueNdx >= 0))
                {
                    context[td.GlobalIndex] = ((upd[td.MarqueNdx] & td.MarqueMask) != 0) ?
                        SD.Color.White : SD.Color.Black;
                }
            }

            shift[0] = (shift[0] << 1) & 0xffffff;
            shift[2] = (shift[2] << 1) & 0xffffff;
            shift[4] = (shift[4] << 1) & 0xffffff;
            shift[6] = (shift[6] << 1) & 0xffffff;
            shift[8] = (shift[8] << 1) & 0xffffff;
            shift[10] = (shift[10] << 1) & 0xffffff;
            shift[12] = (shift[12] << 1) & 0xffffff;
            shift[14] = (shift[14] << 1) & 0xffffff;

            shift[1] = (shift[1] << 1) & 0xffffff;
            shift[3] = (shift[3] << 1) & 0xffffff;
            shift[5] = (shift[5] << 1) & 0xffffff;
            shift[7] = (shift[7] << 1) & 0xffffff;
            shift[9] = (shift[9] << 1) & 0xffffff;
            shift[11] = (shift[11] << 1) & 0xffffff;
            shift[13] = (shift[13] << 1) & 0xffffff;
            shift[15] = (shift[15] << 1) & 0xffffff;
        }
    }
}
