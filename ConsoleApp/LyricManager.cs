using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication2
{
	public class LyricManager
	{
		private List<Lyric> lyrics = new List<Lyric>();
		private Font font;
		private Brush brush;
		private int top;


		public LyricManager(Font font, Brush brush, int top)
		{
			this.font = font;
			this.brush = brush;
			this.top = top;
			lyrics = new List<Lyric>();
		}

		public Lyric Create(string text, TimeMark timemark)
		{
			var lyric = new Lyric(text, timemark);
			lyrics.Add(lyric);
			return lyric;
		}

		public List<Lyric> Lyrics { get { return lyrics; } }

		public void SetLocations()
		{
			foreach (var lyric in lyrics)
				lyric.StartPoint = new PointF(lyric.timemark.Time, top);
		}

		public void Order()
		{
			lyrics = lyrics.OrderBy(l => l.timemark.Time).ToList();
		}
	}
}
