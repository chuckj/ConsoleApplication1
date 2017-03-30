using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ConsoleApplication1
{
	public class Step
	{
        private static log4net.ILog logger = log4net.LogManager.GetLogger(nameof(Step));

        enum ColorEnum { Red, Yellow, Green, Blue };
        static Color[] Colors = new[] { Color.Red, Color.Yellow, Color.ForestGreen, Color.Blue };

        protected XElement nod;
		public static Stack<Vari> varistack = new Stack<Vari>();
		public static bool BreakPending { get; set; }
		public static bool ContinuePending { get; set; }
		private static Dictionary<string, Func<XElement, Step>> cmds;

		static Step()
		{

			cmds = typeof(Step).Assembly.GetTypes()
				.Where(t => t.IsClass && t.IsSubclassOf(typeof(Step)))
				.Select(t => new
				{
					attr = (StepAttribute)t.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(StepAttribute)),
					mi = t.GetMethod("Factory")
				})
				.Where(x => x.attr != null && !string.IsNullOrEmpty(x.attr.XmlName) && x.mi != null)
				.ToDictionary(x => x.attr.XmlName, x => (Func<XElement, Step>)x.mi.CreateDelegate(typeof(Func<XElement, Step>), null));

			//dict = Global.Instance.dta.ToDictionary(d => Tuple.Create<int, int>(d.row, d.ctr), d => d);
		}

        public static void Runner()
        {
            logger.Info("Starting...");

            //XElement root;
            //Global.Instance.Model = root = XDocument.Load(@".\\..\\..\\XMLfile1.xml").Element("root");

            //Cntrl.Load(root);

            //View.Load(root);

            ////  Displays

            //var tree = Global.Instance.VuDict["tree"];
            //Global.Instance.dta = tree.LitArray.Cast<MonoLit>().OrderBy(t => t.Index).ToArray();

            //Global.Instance.tdOrder = tree.LitArray.Select(n => (short)n.Index).ToArray();

            //Global.Instance.dict = Global.Instance.dta.ToDictionary(d => Tuple.Create<int, int>(d.Row, d.Circle), d => d);

            //// Transitions

            //Global.Instance.Transitions = new Dictionary<string, DisplayTransition>()
            //{ {"topdown", DisplayTransition.FromEnumerable(Global.Instance.dta.Select(x => (short)x.Row)) } };

            //foreach (XElement trans in root.Descendants("transitions").Descendants("transition"))
            //{
            //    Global.Instance.Transitions.Add((string)trans.Attribute("name"), DisplayTransition.FromString((string)trans.Attribute("value")));
            //}

            ////  Font

            //foreach (XElement felm in root.Descendants("fonts").Descendants("font"))
            //{
            //    Global.Instance.FontDict.Add(((string)felm.Attribute("char"))[0], FontUpdate.FromString((string)felm.Attribute("value")));
            //}


            Global.Instance.curr = Global.Instance.currDisplay = DisplayUpdate.Get();
            Global.Instance.ParseTime = 0;
            logger.Info("***: " + Global.Instance.ParseTime);
            Global.Instance.TimeReset();

            //  Views





            var pgm = Global.Instance.Model.Descendants("steps").FirstOrDefault();
            var stp = new Step(pgm);
            stp.RunChildren();
        }

        public Step(XElement nod)
		{
			this.nod = nod;
		}

        public int RunChildren() => RunChildren(nod);

        public int RunChildren(XElement nod)
		{
			if (nod == null) return 0;
			XNode child = nod.FirstNode;
			while (child != null)
			{
                if (Global.Instance.Tkn.IsCancellationRequested) return 0;
                if (child.NodeType == XmlNodeType.Element)
				{
					var step = factory(child);
					step.Run();
				}
				if (BreakPending || ContinuePending) break;
				child = child.NextNode;
			}
			return 0;
		}

		public virtual int Run()
		{
			Send("r");

			ReceiveOk();

			return -1;
		}


        public int PlayUpdates(IEnumerable<Update> steps)
        {
            foreach (var trgt in steps)
            {
                PlayUpdates(trgt);
            }

            return 0;
        }

        public int PlayUpdates(Update trgt)
        {
            Global.Instance.readyList.Add(trgt);
            if (trgt is DisplayUpdate)
                Global.Instance.curr = (DisplayUpdate)trgt;
            Global.Instance.ParseTime = trgt.Time;

            return 0;
        }



        public void Send(string msg)
		{
			Console.WriteLine(msg);
		}

		public void ReceiveOk()
		{
			var resp = Console.ReadLine();
			if (resp == "O" || resp == "o" || resp == "") return;
			throw new Exception("Unexpected respoonse:" + resp);
		}

		public void ReceiveI()
		{
			var resp = Console.ReadLine();
			if (resp == "I" || resp == "i" || resp == "") return;
			throw new Exception("Unexpected respoonse:" + resp);
		}

        public virtual Step Factory(XElement xml) => null;

        public static Step factory(XNode nod)
		{
			var elm = (XElement)nod;
			Func<XElement, Step> fact;
			if (!cmds.TryGetValue(elm.Name.LocalName, out fact))
				throw new ArgumentException("Unknown xml command: " + elm.Name.LocalName);
			return fact(elm);
		}

		public enum TknTyp
		{
			Err, Opr, Hex, Int, Str, Var, End
		}

		private static Regex rx = new Regex(@"(?<hex>0x[a-f0-9]{1,8})"
			+ @"|(?<int>[0-9]{1,10})"
			//          <=  >= != ==  <  > = !  + -  * / &  |  (  )
			+ @"|(?<op>\<=|\>=|!=|==|\<|\>|=|!|\+|-|\*|/|&|\||\(|\))"
			//          <=  >= != ==  <  > = !  + -  * / &  |  (  )
			+ @"|(?<nam>[a-z_][a-z0-9_]*)"
			+ @"|(?<str>""(?:[^""]|"""")*"")", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static int rxHex = rx.GroupNumberFromName("hex");
		private static int rxInt = rx.GroupNumberFromName("int");
		private static int rxOp = rx.GroupNumberFromName("op");
		private static int rxNam = rx.GroupNumberFromName("nam");
		private static int rxStr = rx.GroupNumberFromName("str");
		private static MatchCollection matchs;
		private static int matchptr = 0;
		private static TknTyp tkntyp;
		private static string tknval;
		private static Stack<object> stack;

		public object Eval(string exp)
		{
			matchs = rx.Matches(exp);
			matchptr = 0;
			stack = new Stack<object>();
			getNext();
			return evalOr();
		}


		private object evalOr()
		{
			var left = evalAnd();
			while (tknval == "|")
			{
				if (left is bool)
				{
					getNext();
					var right = evalAnd();
					if (!(right is bool)) throw new ArgumentException("Right operand type invalid:" + right.GetType().ToString());
					left = (object)((bool)left || (bool)right);
				}
				else if (left is DisplayUpdate)
				{
					getNext();
					var right = evalNot();
					if (!(right is DisplayUpdate)) throw new ArgumentException("Right operand type invalid:" + right.GetType().ToString());
					((DisplayUpdate)left).Or((DisplayUpdate)right);
					((DisplayUpdate)right).Return();
				}
				else
					throw new ArgumentException("Left operand type invalid:" + left.GetType().ToString());
			}
			return left;
		}

		private object evalAnd()
		{
			var left = evalNot();
			while (tknval == "&")
			{
				if (left is bool)
				{
					getNext();
					var right = evalNot();
					if (!(right is bool)) throw new ArgumentException("Right operand type invalid:" + right.GetType().ToString());
					left = (object)((bool)left && (bool)right);
				}
				else if (left is DisplayUpdate)
				{
					getNext();
					var right = evalNot();
					if (!(right is DisplayUpdate)) throw new ArgumentException("Right operand type invalid:" + right.GetType().ToString());
					((DisplayUpdate)left).And((DisplayUpdate)right);
					((DisplayUpdate)right).Return();
				}
				else
					throw new ArgumentException("Left operand type invalid:" + left.GetType().ToString());
			}
			return left;
		}

		private object evalNot()
		{
			if (tknval != "!") return evalComp();

			getNext();
			var oprnd = evalComp();

			if (oprnd is bool)
			{
				return (object)(!(bool)oprnd);
			}
			if (oprnd is DisplayUpdate)
			{
				return ((DisplayUpdate)oprnd).Not();
			}
			
			throw new ArgumentException("Operand type invalid:" + oprnd.GetType().ToString());
		}

		private object evalComp()
		{
			var left = evalVal();
			string opr = tknval;
			switch (opr)
			{
				case "<":
				case ">":
				case "=":
				case "!=":
				case "<=":
				case ">=":
					break;
				default:
					return left;
			}
			if (!(left is string || left is int)) throw new ArgumentException("Left operand type must be string/int:" + left.GetType().ToString());

			getNext();
			var right = evalVal();
			if (!(right is string || right is int)) throw new ArgumentException("Left operand type must be string/int:" + right.GetType().ToString());
			if (right.GetType() != left.GetType()) throw new ArgumentException("Left and Right operand must be of same type:");
			bool result = false;

			switch (opr)
			{
				case "<":
					if (left is int)
						result = (int)left < (int)right;
					else
						new ArgumentException("< invalid with strings");
					break;

				case ">":
					if (left is int)
						result = (int)left > (int)right;
					else
						new ArgumentException("> invalid with strings");
					break;

				case "=":
					if (left is int)
						result = (int)left == (int)right;
					else
						result = (string)left == (string)right;
					break;

				case "!=":
					if (left is int)
						result = (int)left != (int)right;
					else
						result = (string)left != (string)right;
					break;

				case "<=":
					if (left is int)
						result = (int)left <= (int)right;
					else
						new ArgumentException("<= invalid with strings");
					break;

				case ">=":
					if (left is int)
						result = (int)left >= (int)right;
					else
						new ArgumentException(">= invalid with strings");
					break;

				default:
					return null;
			}
			return result;
		}

		private object evalAddSub()
		{
			var left = evalMulDiv();
			if (left is int)
			{
				while (tknval == "+" || tknval == "-")
				{
					bool sub = tknval == "-";
					getNext();
					var right = evalMulDiv();
					if (!(right is int)) throw new ArgumentException("Right operand type invalid:" + right.GetType().ToString());
					left = (object)(sub ? (int)left - (int)right : (int)left + (int)right);
				}
			}
			return left;
		}

		private object evalMulDiv()
		{
			var left = evalVal();
			if (left is int)
			{
				while (tknval == "*" || tknval == "/")
				{
					bool div = tknval == "/";
					getNext();
					var right = evalVal();
					if (!(right is int)) throw new ArgumentException("Right operand type invalid:" + right.GetType().ToString());
					left = (object)(div ? (int)left / (int)right : (int)left * (int)right);
				}
			}
			return left;
		}

		private object evalVal()
		{
			if (tkntyp == TknTyp.Int)
			{
				int x;
				if (!Int32.TryParse(tknval, out x)) throw new ArgumentException("Expecting integer:");
				getNext();
				return x;
			}
			if (tkntyp == TknTyp.Hex)
			{
				int x;
				if (!Int32.TryParse(tknval.Substring(2, tknval.Length - 2), System.Globalization.NumberStyles.AllowHexSpecifier, null, out x))
					throw new ArgumentException("Expecting integer:");
				getNext();
				return x;
			}
			if (tkntyp == TknTyp.Str)
			{
				getNext();
				return tknval;
			}
			if (tkntyp == TknTyp.Var)
			{
				var vari = varistack.FirstOrDefault(v => v.VarName == tknval);
				if (vari != null)
				{
					getNext();
					return vari.BoxedValue;
				}

                if (tknval == "curr")
                {
                    getNext();
                    return Global.Instance.curr.Clone();
                }

                var hex = Global.Instance.Model.Descendants("displays").Descendants("display")
					.First(d => d.Attribute("name").Value == tknval).Attribute("value").Value;
				if (hex != null)
				{
					var disp = DisplayUpdate.Get().FromHex(hex);
					getNext();
					return disp;
				}

				throw new ArgumentException("Variable is not known:" + tknval);
			}
			if (tknval == "(")
			{
				getNext();
				var oprnd = evalOr();
				if (tknval != ")")
					throw new ArgumentException("Invalid expression:");
				getNext();
				return oprnd;
			}
			else
				throw new ArgumentException("Expecting operand:");
		}

		private void getNext()
		{
			if (matchptr == matchs.Count)
			{
				tknval = string.Empty;
				tkntyp = TknTyp.End;
			}
			else if (matchptr > matchs.Count)
			{
				throw new ArgumentException("Passed end of expression:");
			}
			else if (matchs[matchptr].Groups[rxHex].Length > 0)
			{
				tknval = matchs[matchptr].Groups[rxHex].Value;
				tkntyp = TknTyp.Hex;
			}
			else if (matchs[matchptr].Groups[rxInt].Length > 0)
			{
				tknval = matchs[matchptr].Groups[rxInt].Value;
				tkntyp = TknTyp.Int;
			}
			else if (matchs[matchptr].Groups[rxOp].Length > 0)
			{
				tknval = matchs[matchptr].Groups[rxOp].Value;
				tkntyp = TknTyp.Opr;
			}
			else if (matchs[matchptr].Groups[rxNam].Length > 0)
			{
				tknval = matchs[matchptr].Groups[rxNam].Value;
				tkntyp = TknTyp.Var;
			}
			else if (matchs[matchptr].Groups[rxStr].Length > 0)
			{
				tknval = matchs[matchptr].Groups[rxStr].Value;
				tkntyp = TknTyp.Str;
			}
			else
				tkntyp = TknTyp.Err;

			matchptr++;
		}


        private static Regex rx1 = new Regex(@"\A(?<plus>\+)?(?<secs>\d{1,5})(\.(?<mils>\d{1,3}))?\z", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private static int rxPlus = rx1.GroupNumberFromName("plus");
        private static int rxSecs = rx1.GroupNumberFromName("secs");
        private static int rxMils = rx1.GroupNumberFromName("mils");

        protected static int editTime(string parm, int basis)
        {
            if (string.IsNullOrEmpty(parm))
                return basis;
            var match = rx1.Match(parm);
            if (!match.Success)
                throw new ArgumentException("invalid start:" + parm);
            int time;
            if (!int.TryParse(match.Groups[rxSecs].Value, out time))
                throw new ArgumentException("invalid start:" + parm);
            time *= 1000;
            if (match.Groups[rxPlus].Length > 0)
                time += basis;
            if (match.Groups[rxMils].Length > 0)
            {
                int mils;
                if (!int.TryParse(match.Groups[rxMils].Value, out mils))
                    throw new ArgumentException("invalid start:" + parm);
                time += mils;
            }
            return time;

        }

    }
}
