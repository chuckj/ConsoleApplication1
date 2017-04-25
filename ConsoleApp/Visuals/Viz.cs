using Newtonsoft.Json;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    #region VizFeatures
    [Flags]
	public enum VizFeatures
	{
		None              = 0,
		IsSelectable      = 1 << 0,
		IsMultiSelectable = 1 << 1,
		IsDraggable       = 1 << 2,
		HasContextMenu    = 1 << 3,
		IsFixedSize       = 1 << 5,
		IsKeyFrame        = 1 << 6,
		HasToolTip        = 1 << 7,
		HasHilites        = 1 << 8,
		HasProperties     = 1 << 9,
		CanInsertDelete   = 1 << 10,
		HasRegex          = 1 << 11,

	}
	#endregion


	public abstract class Viz
	{
		//protected static VizRes resources;
		public Point StartPoint;
		public Point EndPoint;
		public int ZOrder;

		[JsonIgnore]
		public virtual string DebuggerDisplay => $"({StartPoint.X},{StartPoint.Y})-({EndPoint.X},{EndPoint.Y})";

		[JsonIgnore]
		public Size2 Size => new Size2(EndPoint.X - StartPoint.X, EndPoint.Y - StartPoint.Y);

		[JsonIgnore]
		public virtual Rectangle Rectangle => new Rectangle(StartPoint.X, StartPoint.Y, Math.Max(Size.Width, 1), Size.Height);

		//[JsonIgnore]
		//public Rectangle Box {  get { return new SD.Rectangle(StartPoint.X, StartPoint.Y, Size.Width, Size.Height); } }

		//[JsonIgnore]
		//public RectangleF RectangleF { get { return new RectangleF(StartPoint.X, StartPoint.Y, Size.Width, Size.Height); } }

		[JsonIgnore]
		public virtual VizFeatures Features => VizFeatures.None;

		public abstract void ResetPoints(Song song);

		public virtual void DrawMove(DrawData dd) { }

		public abstract void Draw(DrawData dd);

		public virtual void DrawHilites(DrawData dd) { }

		public virtual void DrawSelect(DrawData dd, bool primary) { }

		public virtual IEnumerable<CntxtMenuItem> GetContentMenuItems() => null;

		public virtual void ShowDialog(Song Song)
		{
		}

		public Viz Clone() => (Viz)this.MemberwiseClone();

		//public virtual void KeyPress(KeyPressEventArgs e, ToolStripStatusLabel lbl) { }

		public virtual string ToolTip() => null;

		public virtual XElement Serialize(Song song) { return null; }

		public virtual VizCmd TranslateToCmd(ref Message msg, Keys keyData) => null;

		public virtual VizCmd XeqCmd(VizCmd vizCmd)
		{
			switch (vizCmd.cmd)
			{
				case Cmd.HorzRel: return CmdHorzRel(vizCmd);
				case Cmd.Insert: return CmdInsert(vizCmd);
				case Cmd.Delete: return CmdDelete(vizCmd);
				case Cmd.Copy: return CmdCopy(vizCmd);
				case Cmd.Paste: return CmdPaste(vizCmd);
			}
			return null;
		}

		//public bool CmdLogAndDo(int cmd)
		//{
		//	return CmdDo(cmd);
		//}

		#region Command helpers
		protected virtual VizCmd CmdHorzRel(VizCmd vizCmd)
		{
			int offset = (int)vizCmd.obj;
			StartPoint.X += offset;
			EndPoint.X += offset;

			vizCmd.obj = -offset;
			return vizCmd;
		}

		protected virtual VizCmd CmdInsert(VizCmd vizCmd)
		{
			//Viz viz = (Viz)JsonConvert.DeserializeObject((string)(vizCmd.obj), typeof(Lyric));
			Viz viz = vizCmd.viz;
			viz.ResetPoints(Global.Instance.Song);
			Global.Instance.Song.Vizs.Add(viz);
			Global.Instance.Song.Selected.Add(viz);
			vizCmd.cmd = Cmd.Delete;
			return vizCmd;
		}

		protected virtual VizCmd CmdDelete(VizCmd vizCmd)
		{
			var viz = vizCmd.viz;
			Global.Instance.Song.Vizs.Remove(viz);
			//vizCmd.viz = this;
			//vizCmd.obj = JsonConvert.SerializeObject(this);
			vizCmd.cmd = Cmd.Insert;
			Global.Instance.Song.Selected.Remove(viz);
			return vizCmd;
		}

		protected virtual VizCmd CmdCopy(VizCmd vizCmd)
		{
			//Viz viz = (Viz)JsonConvert.DeserializeObject((string)(vizCmd.obj), typeof(Lyric));
			Viz viz = vizCmd.viz;
			viz.ResetPoints(Global.Instance.Song);
			Global.Instance.Song.Vizs.Add(viz);
			vizCmd.cmd = Cmd.Delete;
			return vizCmd;
		}

		protected virtual VizCmd CmdPaste(VizCmd vizCmd)
		{
			Global.Instance.Song.Vizs.Remove(vizCmd.viz);
			//vizCmd.viz = this;
			//vizCmd.obj = JsonConvert.SerializeObject(this);
			//Global.Song.Selected.Remove(viz);
			vizCmd.cmd = Cmd.Insert;
			return vizCmd;
		}

		protected VizCmd MoveBeatLeft()
		{
			var x = StartPoint.X;
			if (x == 0) return null;
			var tp = Global.Instance.Song.TimePoints;
			var ndx = tp.FindFirstIndexGreaterThanOrEqualTo(x);
			if (ndx < 7) return null;
			var offset = tp.Keys[ndx - 6] - tp.Keys[ndx];
			return new VizCmd(this, Cmd.HorzRel, offset);
		}


		protected VizCmd MoveBeatRight()
		{
			var x = StartPoint.X;
			var tp = Global.Instance.Song.TimePoints;
			var ndx = tp.FindFirstIndexGreaterThanOrEqualTo(x);
			if ((ndx < 0) || (ndx + 6 >= tp.Count)) return null;
			var offset = tp.Keys[ndx + 6] - tp.Keys[ndx];
			return new VizCmd(this, Cmd.HorzRel, offset);
		}


		protected VizCmd MoveTimePointLeft()
		{
			var x = StartPoint.X;
			if (x == 0) return null;
			var tp = Global.Instance.Song.TimePoints;
			var ndx = tp.FindFirstIndexGreaterThanOrEqualTo(x);
			if (tp.Keys[ndx] == x) ndx--;
			if (ndx < 0) return null;
			var offset = tp.Keys[ndx] - x;
			return new VizCmd(this, Cmd.HorzRel, offset);
		}

		protected VizCmd MoveTimePointRight()
		{
			var x = StartPoint.X;
			var tp = Global.Instance.Song.TimePoints;
			var ndx = tp.FindFirstIndexGreaterThanOrEqualTo(x);
			if (ndx < 0) return null;
			if (tp.Keys[ndx] == x)
			{
				ndx++;
				if (ndx >= tp.Count) return null;
			}
			var offset = tp.Keys[ndx] - x;
			return new VizCmd(this, Cmd.HorzRel, offset);
		}
		#endregion


		#region Features helpers
		[JsonIgnore]
		public bool IsSelectable => (Features & VizFeatures.IsSelectable) != 0;
		[JsonIgnore]
		public bool IsMultiSelectable => (Features & VizFeatures.IsMultiSelectable) != 0;
		[JsonIgnore]
		public bool IsDraggable => (Features & VizFeatures.IsDraggable) != 0;
		[JsonIgnore]
		public bool HasContextMenu => (Features & VizFeatures.HasContextMenu) != 0;
		[JsonIgnore]
		public bool IsFixedSize => (Features & VizFeatures.IsFixedSize) != 0;
		[JsonIgnore]
		public bool IsKeyFrame => (Features & VizFeatures.IsKeyFrame) != 0;
		[JsonIgnore]
		public bool HasToolTip => (Features & VizFeatures.HasToolTip) != 0;
		[JsonIgnore]
		public bool HasHilites => (Features & VizFeatures.HasHilites) != 0;
		[JsonIgnore]
		public bool HasRegex => (Features & VizFeatures.HasRegex) != 0;
		[JsonIgnore]
		public bool HasProperties => (Features & VizFeatures.HasProperties) != 0;
		public bool CanInsertDelete => (Features & VizFeatures.CanInsertDelete) != 0;

		#endregion

		#region Eval
		public enum TknTyp
		{
			Err, Opr, Hex, Int, Str, Var, End
		}


		public static Stack<Vari> varistack = new Stack<Vari>();


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
		#endregion

	}
}
