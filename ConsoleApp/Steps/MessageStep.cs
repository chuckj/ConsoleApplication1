using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
	[Step("msg")]
	public class MessageStep : Step
	{
		private string _msg;
		private static Regex regex = new Regex("\\{([a-z_][a-z0-9_]*)\\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static MatchEvaluator matchEval = new MatchEvaluator((Match match) =>
		{
			string varname = match.ToString();
			varname = varname.Substring(1, varname.Length - 2);
			var vari = varistack.FirstOrDefault(v => v.VarName == varname);
			return (vari != null) ? vari.ToString() : "???";
		});

        private int _startTime = 0;

        public override Step Factory(XElement nod) => new MessageStep(nod, (string)nod.Attribute("start"), (string)nod.Attribute("stop"), nod.Value);

        public MessageStep(XElement nod, string _start, string _stop, string msg)
			: base(nod)
		{
			_msg = msg;
            _startTime = editTime(_start, Global.Instance.ParseTime);

        }

        public override int Run()
		{
            //Send("o" + regex.Replace(_msg, matchEval));
            //ReceiveOk();
            //ReceiveI();
            //return 0;
            MarqueUpdate prv = new MarqueUpdate();
            List<MarqueUpdate> steps = new List<MarqueUpdate>();
            uint[] shift = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int steptime = 50;

            for (int ndx = 0; ndx < _msg.Length; ndx++)
            {
                char chr = _msg[ndx];

                FontUpdate font;
                Global.Instance.FontDict.TryGetValue(chr, out font);


                // clearance

                int maxBits = 0;
                maxBits = smear(maxBits, font.Bits[0] | font.Bits[15],            shift[0] | shift[1] | shift[15]);
                maxBits = smear(maxBits, font.Bits[1] | font.Bits[15], shift[0] | shift[1] | shift[2] | shift[15]);
                maxBits = smear(maxBits, font.Bits[2] | font.Bits[15], shift[1] | shift[2] | shift[3] | shift[15]);
                maxBits = smear(maxBits, font.Bits[3] | font.Bits[15], shift[2] | shift[3] | shift[4] | shift[15]);
                maxBits = smear(maxBits, font.Bits[4] | font.Bits[15], shift[3] | shift[4] | shift[5] | shift[15]);
                maxBits = smear(maxBits, font.Bits[5] | font.Bits[15], shift[4] | shift[5] | shift[6] | shift[15]);
                maxBits = smear(maxBits, font.Bits[6] | font.Bits[15], shift[5] | shift[6] | shift[7] | shift[15]);
                maxBits = smear(maxBits, font.Bits[7] | font.Bits[15], shift[6] | shift[7] | shift[8] | shift[15]);
                maxBits = smear(maxBits, font.Bits[8] | font.Bits[15], shift[7] | shift[8] | shift[9] | shift[15]);
                maxBits = smear(maxBits, font.Bits[9] | font.Bits[15], shift[8] | shift[9] | shift[10] | shift[15]);
                maxBits = smear(maxBits, font.Bits[10] | font.Bits[15], shift[9] | shift[10] | shift[11] | shift[15]);
                maxBits = smear(maxBits, font.Bits[11] | font.Bits[15], shift[10] | shift[11] | shift[12] | shift[15]);
                maxBits = smear(maxBits, font.Bits[12] | font.Bits[15], shift[11] | shift[12] | shift[13] | shift[15]);
                maxBits = smear(maxBits, font.Bits[13] | font.Bits[15], shift[12] | shift[13] | shift[14] | shift[15]);
                maxBits = smear(maxBits, font.Bits[14] | font.Bits[15], shift[13] | shift[14]             | shift[15]);
                Console.WriteLine(chr + ":" + maxBits);

                if (ndx > 0)
                {
                    for (int shifts = 0; shifts <= maxBits; shifts++)
                    {
                        prv = new MarqueUpdate();
                        prv.Time = _startTime;
                        _startTime += steptime;

                        prv.Bits[0] = (shift[0]) & 0xffff00;
                        prv.Bits[1] = (shift[1] | (shift[1] << 1)) & 0xffff00;
                        prv.Bits[2] = (shift[2]) & 0xffff00;
                        prv.Bits[3] = (shift[3] | (shift[3] << 1)) & 0xffff00;
                        prv.Bits[4] = (shift[4]) & 0xffff00;
                        prv.Bits[5] = (shift[5] | (shift[5] << 1)) & 0xffff00;
                        prv.Bits[6] = (shift[6]) & 0xffff00;
                        prv.Bits[7] = (shift[7] | (shift[7] << 1)) & 0xffff00;
                        prv.Bits[8] = (shift[8]) & 0xffff00;
                        prv.Bits[9] = (shift[9] | (shift[9] << 1)) & 0xffff00;
                        prv.Bits[10] = (shift[10]) & 0xffff00;
                        prv.Bits[11] = (shift[11] | (shift[11] << 1)) & 0xffff00;
                        prv.Bits[12] = (shift[12]) & 0xffff00;
                        prv.Bits[13] = (shift[13] | (shift[13] << 1)) & 0xffff00;
                        prv.Bits[14] = (shift[14]) & 0xffff00;

                        steps.Add(prv);

                        shift[1] = (shift[1] << 1) & 0xffffff;
                        shift[3] = (shift[3] << 1) & 0xffffff;
                        shift[5] = (shift[5] << 1) & 0xffffff;
                        shift[7] = (shift[7] << 1) & 0xffffff;
                        shift[9] = (shift[9] << 1) & 0xffffff;
                        shift[11] = (shift[11] << 1) & 0xffffff;
                        shift[13] = (shift[13] << 1) & 0xffffff;
                        shift[15] = (shift[15] << 1) & 0xffffff;

                        prv = new MarqueUpdate();
                        prv.Time = _startTime;
                        _startTime += steptime;

                        prv.Bits[0] = (shift[0] | (shift[0] << 1)) & 0xffff00;
                        prv.Bits[1] = (shift[1]) & 0xffff00;
                        prv.Bits[2] = (shift[2] | (shift[2] << 1)) & 0xffff00;
                        prv.Bits[3] = (shift[3]) & 0xffff00;
                        prv.Bits[4] = (shift[4] | (shift[4] << 1)) & 0xffff00;
                        prv.Bits[5] = (shift[5]) & 0xffff00;
                        prv.Bits[6] = (shift[6] | (shift[6] << 1)) & 0xffff00;
                        prv.Bits[7] = (shift[7]) & 0xffff00;
                        prv.Bits[8] = (shift[8] | (shift[8] << 1)) & 0xffff00;
                        prv.Bits[9] = (shift[9]) & 0xffff00;
                        prv.Bits[10] = (shift[10] | (shift[10] << 1)) & 0xffff00;
                        prv.Bits[11] = (shift[11]) & 0xffff00;
                        prv.Bits[12] = (shift[12] | (shift[12] << 1)) & 0xffff00;
                        prv.Bits[13] = (shift[13]) & 0xffff00;
                        prv.Bits[14] = (shift[14] | (shift[14] << 1)) & 0xffff00;

                        steps.Add(prv);

                        shift[0] = (shift[0] << 1) & 0xffffff;
                        shift[2] = (shift[2] << 1) & 0xffffff;
                        shift[4] = (shift[4] << 1) & 0xffffff;
                        shift[6] = (shift[6] << 1) & 0xffffff;
                        shift[8] = (shift[8] << 1) & 0xffffff;
                        shift[10] = (shift[10] << 1) & 0xffffff;
                        shift[12] = (shift[12] << 1) & 0xffffff;
                        shift[14] = (shift[14] << 1) & 0xffffff;

                    }
                }

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

            uint bits = shift[0] | shift[1] | shift[2] | shift[3] | shift[4] | shift[5] |
                shift[7] | shift[8] | shift[9] | shift[10] | shift[11] | shift[12] |
                shift[13] | shift[14];
            while ((bits & 0xffffff) > 0)
            {
                prv = new MarqueUpdate();
                prv.Time = _startTime;
                _startTime += steptime;

                prv.Bits[0] = (shift[0]) & 0xffff00;
                prv.Bits[1] = (shift[1] | (shift[1] << 1)) & 0xffff00;
                prv.Bits[2] = (shift[2]) & 0xffff00;
                prv.Bits[3] = (shift[3] | (shift[3] << 1)) & 0xffff00;
                prv.Bits[4] = (shift[4]) & 0xffff00;
                prv.Bits[5] = (shift[5] | (shift[5] << 1)) & 0xffff00;
                prv.Bits[6] = (shift[6]) & 0xffff00;
                prv.Bits[7] = (shift[7] | (shift[7] << 1)) & 0xffff00;
                prv.Bits[8] = (shift[8]) & 0xffff00;
                prv.Bits[9] = (shift[9] | (shift[9] << 1)) & 0xffff00;
                prv.Bits[10] = (shift[10]) & 0xffff00;
                prv.Bits[11] = (shift[11] | (shift[11] << 1)) & 0xffff00;
                prv.Bits[12] = (shift[12]) & 0xffff00;
                prv.Bits[13] = (shift[13] | (shift[13] << 1)) & 0xffff00;
                prv.Bits[14] = (shift[14]) & 0xffff00;

                steps.Add(prv);

                shift[1] = (shift[1] << 1) & 0xffffff;
                shift[3] = (shift[3] << 1) & 0xffffff;
                shift[5] = (shift[5] << 1) & 0xffffff;
                shift[7] = (shift[7] << 1) & 0xffffff;
                shift[9] = (shift[9] << 1) & 0xffffff;
                shift[11] = (shift[11] << 1) & 0xffffff;
                shift[13] = (shift[13] << 1) & 0xffffff;
                shift[15] = (shift[15] << 1) & 0xffffff;

                prv = new MarqueUpdate();
                prv.Time = _startTime;
                _startTime += steptime;

                prv.Bits[0] = (shift[0] | (shift[0] << 1)) & 0xffff00;
                prv.Bits[1] = (shift[1]) & 0xffff00;
                prv.Bits[2] = (shift[2] | (shift[2] << 1)) & 0xffff00;
                prv.Bits[3] = (shift[3]) & 0xffff00;
                prv.Bits[4] = (shift[4] | (shift[4] << 1)) & 0xffff00;
                prv.Bits[5] = (shift[5]) & 0xffff00;
                prv.Bits[6] = (shift[6] | (shift[6] << 1)) & 0xffff00;
                prv.Bits[7] = (shift[7]) & 0xffff00;
                prv.Bits[8] = (shift[8] | (shift[8] << 1)) & 0xffff00;
                prv.Bits[9] = (shift[9]) & 0xffff00;
                prv.Bits[10] = (shift[10] | (shift[10] << 1)) & 0xffff00;
                prv.Bits[11] = (shift[11]) & 0xffff00;
                prv.Bits[12] = (shift[12] | (shift[12] << 1)) & 0xffff00;
                prv.Bits[13] = (shift[13]) & 0xffff00;
                prv.Bits[14] = (shift[14] | (shift[14] << 1)) & 0xffff00;

                steps.Add(prv);

                shift[0] = (shift[0] << 1) & 0xffffff;
                shift[2] = (shift[2] << 1) & 0xffffff;
                shift[4] = (shift[4] << 1) & 0xffffff;
                shift[6] = (shift[6] << 1) & 0xffffff;
                shift[8] = (shift[8] << 1) & 0xffffff;
                shift[10] = (shift[10] << 1) & 0xffffff;
                shift[12] = (shift[12] << 1) & 0xffffff;
                shift[14] = (shift[14] << 1) & 0xffffff;

                bits <<= 1;
            }

            return PlayUpdates(steps);
        }

        private int smear(int maxBits, uint onDeck, uint atBat)
        {
            uint overlap = (atBat | (atBat << 1) | (atBat << 2) | (atBat << 3) | (atBat << 4) | (atBat << 5) | (atBat << 6) | (atBat << 7)) & 0xff & onDeck;
            int bits = 0;
            if ((atBat & 0x01) > 0) bits = 8;
            else if ((atBat & 0x02) > 0) bits = 7;
            else if ((atBat & 0x04) > 0) bits = 6;
            else if ((atBat & 0x08) > 0) bits = 5;
            else if ((atBat & 0x10) > 0) bits = 4;
            else if ((atBat & 0x20) > 0) bits = 3;
            else if ((atBat & 0x40) > 0) bits = 2;
            else if ((atBat & 0x80) > 0) bits = 1;
            return Math.Max(maxBits, bits + 1);
        }
	}
}
