using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
	public class TreeTest
	{
		private static List<string> font = new List<string>(96);
		private static List<string> cmds = new List<string>(0);
		private static ConcurrentQueue<string> conStrm = new ConcurrentQueue<string>();
		private static Timer t = new Timer(tcb, null, Timeout.Infinite, Timeout.Infinite);

		public async static void Run()
		{
			//Global.Instance.doc = XDocument.Load(@".\\..\\..\\XMLfile1.xml");

			////var pgm = doc.Descendants("steps").FirstOrDefault();
			////var stp = new Step(pgm);
			////stp.RunChildren();
			////Console.ReadLine();
			////return;

			//Global.Instance.dta = Global.Instance.doc.Descendants("lites").Descendants("lite").Select(n => new TreeData()
			//{
			//	row = (int)n.Attribute("row"),
			//	ctr = (int)n.Attribute("cir"),
			//	col = (int)n.Attribute("col"),
			//	ndx = (int)n.Attribute("ndx"),
			//	color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), (string)n.Attribute("color")),
			//}).OrderBy(t => t.ndx).ToArray();
			////byte[] mask = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

			//Global.Instance.dict = Global.Instance.dta.ToDictionary(d => Tuple.Create<int, int>(d.row, d.ctr), d => d);

			Global.Instance.curr = DisplayUpdate.Get();
			foreach (var td in Global.Instance.dta)
				if (td.Row >= 2  && (td.Row == Global.Instance.dta.Max(x => x.Row) || td.Circle == Global.Instance.dta.Where(x => x.Row == td.Row).Max(x => x.Circle) || td.Circle == Global.Instance.dta.Where(x => x.Row == td.Row).Min(x => x.Circle)))
					Global.Instance.curr.Set(td);

			var prv = Global.Instance.curr;

			//Global.Instance.curr.ToConsole();

			var pgm = Global.Instance.Model.Descendants("steps").FirstOrDefault();
			var stp = new Step(pgm);
			stp.RunChildren();
			Console.ReadLine();


			Console.ReadLine();
			CancellationTokenSource cts = new CancellationTokenSource();
			CancellationToken tkn = cts.Token;
			var rdr = Task.Factory.StartNew(() => ConRead(tkn), tkn);

			//var nu = dta.Select(d => d.color == ConsoleColor.Red || d.color == ConsoleColor.Yellow).ToArray();
			//var nu = Global.Instance.dta.Select(d => d.row + Math.Abs(d.ctr) < 32).ToArray();
			//TreeData td;
			//for (int x = 2; x <= 34; x += 2)
			//{
			//	//    var nxt = dta.Select(i => i.row < x ? nu[i.ndx] : prv[i.ndx]).ToArray();
			//	var nxt = Global.Instance.dta
			//	  .Select(d => new { i = d, x = Global.Instance.dict.TryGetValue(Tuple.Create(d.row + 32 - x, d.ctr), out td), t = td })
			//	  .Select(i => i.i.row < x ? (i.t != null ? nu[i.t.ndx] : false) : prv[i.i.ndx])
			//	  .ToArray();
			//	DisplayIt(nxt);
			//	Thread.Sleep(400);

			//	string cmd;
			//	if (conStrm.TryDequeue(out cmd))
			//	{
			//		lock (conLock)
			//		{
			//			if (cmd == null)
			//			{
			//			}
			//			else
			//			{
			//				Console.ForegroundColor = ConsoleColor.White;
			//				Console.WriteLine("You pressed '{0}'.", cmd);
			//				if (cmd == "x" || cmd == "X") break;
			//				sendNextMessage();
			//			}
			//		}
			//	}
			//}

			cts.Cancel();
			//await rdr;
		}

		private static void tcb(object state)
		{
			lock (Global.Instance.conLock)
			{
				t.Change(Timeout.Infinite, Timeout.Infinite);
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write("Timeout");
				conStrm.Enqueue("TIMEOUT");
			}
		}
		private static int ConRead(CancellationToken tkn)
		{
			do
			{
				while (!Console.KeyAvailable)
				{
					if (tkn.IsCancellationRequested)
						return 1;
					Thread.Sleep(100);
				}
				var cki = Console.ReadKey(true);
				conStrm.Enqueue(cki.KeyChar + string.Empty);
				t.Change(Timeout.Infinite, Timeout.Infinite);
			} while (true);

			return 0;
		}


		private void loadFonts()
		{
			font.Clear();

			font.AddRange(Global.Instance.Model.Descendants("fonts").Descendants("font")
				.Select(n => string.Format("f{0},{1}\r", (string)n.Attribute("char"), (string)n.Attribute("value"))));
		}

		private static void sendNextMessage()
		{
			if (font.Count > 0)
			{
				//      serial.Send(font[0]);
				font.RemoveAt(0);
			}
			else if (cmds.Count > 0)
			{
				//      serial.Send(cmds[0]);
				cmds.RemoveAt(0);
			}
			else
			{
				lock (Global.Instance.conLock)
				{
					Console.ForegroundColor = ConsoleColor.White;
					Console.Write("q");
				}
			}
			t.Change(1000, Timeout.Infinite);
		}
	}
}
