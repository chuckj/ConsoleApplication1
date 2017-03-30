using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication2
{
	public class VizManager
	{
		private List<Viz> vizs = new List<Viz>();

		public VizManager()
		{
			vizs = new List<Viz>();
		}

		public Step CreateStep(string text, TimeMark startTimeMark, TimeMark endTimeMark)
		{
			var step = new Step(text, startTimeMark, endTimeMark);
			step.ZOrder = Global.ZOrder_Step;
			vizs.Add(step);
			return step;
		}

		public StepBg CreateStepBg()
		{
			var step = new StepBg();
			step.ZOrder = Global.ZOrder_StepBg;
			vizs.Add(step);
			return step;
		}


		public Lyric CreateLyric(Graphics dc, string text, TimeMark timemark)
		{
			var lyric = new Lyric(dc, text, timemark);
			lyric.ZOrder = Global.ZOrder_Lyric;
			vizs.Add(lyric);
			return lyric;
		}

		public LyricBg CreateLyricBg()
		{
			var lyric = new LyricBg();
			lyric.ZOrder = Global.ZOrder_LyricBg;
			vizs.Add(lyric);
			return lyric;
		}

		public Wave CreateWave()
		{
			var wave = new Wave();
			wave.ZOrder = Global.ZOrder_Wave;
			vizs.Add(wave);
			return wave;
		}

		public Rule CreateRule()
		{
			var rule = new Rule();
			rule.ZOrder = Global.ZOrder_Rule;
			vizs.Add(rule);
			return rule;
		}

		public Measure CreateMeasure(Song song, int startingMeasure, int beatsPerMeasure, float startingTime, float timePerBeat)
		{
			var measure = new Measure(startingMeasure, beatsPerMeasure, startingTime, timePerBeat);
			measure.ZOrder = Global.ZOrder_Measure;
			vizs.Add(measure);
			song.Measures[startingMeasure] = measure;
			return measure;
		}


		public List<Viz> Vizs { get { return vizs; } }

		public void Order()
		{
			vizs = vizs.OrderBy(l => l.ZOrder).ThenBy(l => l.StartPoint.X).ToList();
		}


		public void Draw(Graphics dc, DrawData dd)
		{
			foreach (var viz in vizs)
			{
				viz.Draw(dc, dd);
			}
		}

		//public void SetLocations()
		//{
		//	Order();

		//	float[] stopTime = new [] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
		//	foreach (var viz in vizs)
		//	{
		//		int row = 0;
		//		for (row = 0; row < 8; row++)
		//		{
		//			if (stopTime[row] < viz.startTimeMark.Time)
		//			{
		//				stopTime[row] = viz.endTimeMark.Time;
		//				break;
		//			}
		//		}
		//		viz.row = row;
		//		viz.StartPoint = new PointF(viz.startTimeMark.Time, top + row * 9);
		//		viz.EndPoint = new PointF(viz.endTimeMark.Time, top + row * 9 + 7);
		//		//step.Location = new Point((int)(step.startTimeMark.Time * Global.PixelsPerSecond), top + row * 12);
		//		//step.Size = new Size((int)(step.endTimeMark.Time * Global.PixelsPerSecond) - step.Location.X, 7);
		//		//step.rectangle = new RectangleF(step.startTimeMark.Time, row, step.endTimeMark.Time - step.startTimeMark.Time, .8f);
		//	}
		//}
	}
}
