using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    public class TreeTransition
    {
        //private static Regex rx = new Regex(@"(\[(?<obgn>\d{1,3})(-(?<oend>\d{1,3}))?\])|(\{(?<nbgn>\d{1,3})(-(?<nend>\d{1,3}))?\})|(@(?<at>\d{1,3}))|(\*(?<star>\d{0,3}))", RegexOptions.Compiled);
        //private static int rxObgn = rx.GroupNumberFromName("obgn");
        //private static int rxOend = rx.GroupNumberFromName("oend");
        //private static int rxNbgn = rx.GroupNumberFromName("nbgn");
        //private static int rxNend = rx.GroupNumberFromName("nend");
        //private static int rxAt = rx.GroupNumberFromName("at");
        //private static int rxStar = rx.GroupNumberFromName("star");
        private static Regex rx = new Regex(@"\d\d", RegexOptions.Compiled);

        public static TreeTransition FromEnumerable(IEnumerable<short> trn) => new TreeTransition() { Steps = trn.ToArray() };

        public static TreeTransition FromString(string trn)
        {
            var matchs = rx.Matches(trn);
            if (matchs.Count != 307)
                throw new ArgumentException("invalid transition:");

            return FromEnumerable(Global.Instance.tdOrder
                .Zip(matchs.Cast<Match>(), (a, b) => new { ndx = a, val = (short)(short.Parse(b.Value) - 1) })
                .OrderBy(z => z.ndx).Select(z => z.val));
        }

        private static bool fill(short[] ts, ref int pos, string sbgn, string send, short factor)
        {
            short bgn, end;
            if (!short.TryParse(sbgn, out bgn) || bgn < 0 || bgn > 306)
                return false;
            if (string.IsNullOrEmpty(send))
                end = bgn;
            else
                if (!short.TryParse(send, out end) || end < 0 || end > 306)
                    return false;

            if (bgn <= end)
            {
                for (; bgn <= end && pos < 307; pos++, bgn++)
                    ts[pos] = (short)((bgn + 1) * factor);
            }
            else
            {
               for (; bgn >= end && pos < 307; bgn--, pos++)
                    ts[pos] = (short)((bgn + 1) * factor);
            }
            return true;
        }

        public short[] Steps = new short[307];
    }
}
