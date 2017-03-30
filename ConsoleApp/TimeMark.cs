using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    public class MeasureCollection : INotifyPropertyChanged
    {
        static MeasureCollection()
        {
        }

        private Measure[] mc;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged( string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        public Measure this[int index] => (index <= 0 || index >= mc.Length) ? null : mc[index];

        public int Length => (mc == null) ? 0 : mc.Length;

        public float GetTime(int Measure) => this[Measure].StartingTime;

    }


    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class TimeMark
	{
		public short Measure;
		public short Beat;
		public PartialBeats PartialBeat;
		public short Milliseconds;
		public float Time;

        [JsonIgnore]
        public string DebuggerDisplay => $"TM {this}";

        public TimeMark()
		{
		}

        public TimeMark(TimeMarkShort tms, float milliseconds = 0)
        {
            this.Measure = tms.Measure;
            this.Beat = tms.Beat;
            this.PartialBeat = tms.PartialBeat;
            this.Milliseconds = (short)milliseconds;
        }

		public TimeMark(short measure, short beat = 1, PartialBeats partialBeat = PartialBeats.None, short milliseconds = 0)
		{
			if (measure < 1) throw new ArgumentOutOfRangeException("Measure must be > 0");
			if (beat < 1) throw new ArgumentOutOfRangeException("Beat must be > 0");
			if (milliseconds < 0) throw new ArgumentOutOfRangeException("Milliseconds must be >= 0");

			this.Measure = measure;
			this.Beat = beat;
			this.PartialBeat = partialBeat;
			this.Milliseconds = milliseconds;
		}

		public override string ToString()
		{
			string pb;
			switch (PartialBeat){
				case PartialBeats.TwoOfFour:		pb = "+2/4";		break;
				case PartialBeats.TwoOfThree:		pb = "+2/3";		break;
				case PartialBeats.TwoOfTwo:			pb = "+2/2";		break;
				case PartialBeats.ThreeOfThree:		pb = "+3/3";		break;
				case PartialBeats.ThreeOfFour:		pb = "+3/4";		break;
				case PartialBeats.FourOfFour:		pb = "+4/4";		break;
				default:							pb = "";			break;
			}
			return $"{Measure}m{Beat}{pb}{(Milliseconds > 0 ? string.Format("+{0:F3}", Milliseconds / 100f) : "")}";
		}
	}

	public enum PartialBeats : byte
	{
		None = 0,
		TwoOfFour = 1,
		TwoOfThree,
		TwoOfTwo,
		ThreeOfThree,
		ThreeOfFour,
		FourOfFour
	}

    public static class PartialBeatsExtensions
    {
        public static string ToString(this PartialBeats PartialBeat)
        {
            switch (PartialBeat)
            {
                case PartialBeats.TwoOfFour:
                    return "+2/4";
                case PartialBeats.TwoOfThree:
                    return "+2/3";
                case PartialBeats.TwoOfTwo:
                    return "+2/2";
                case PartialBeats.ThreeOfThree:
                    return "+3/3";
                case PartialBeats.ThreeOfFour:
                    return "+3/4";
                case PartialBeats.FourOfFour:
                    return "+4/4";
                default:
                    return "";
            }
        }

        public static float ToFraction(this PartialBeats PartialBeat)
        {
            switch (PartialBeat)
            {
                case PartialBeats.TwoOfFour:
                    return .25f;
                case PartialBeats.TwoOfThree:
                    return 1 / 3f;
                case PartialBeats.TwoOfTwo:
                    return .5f;
                case PartialBeats.ThreeOfFour:
                    return .5f;
                case PartialBeats.ThreeOfThree:
                    return 2 / 3f;
                case PartialBeats.FourOfFour:
                    return .75f;
                default:
                    return 0f;
            }
        }
    }


    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct TimeMarkShort
    {
        //public int 
        public short Measure;
        public byte Beat;
        public PartialBeats PartialBeat;

        public TimeMarkShort(int measure, int beat, PartialBeats partial)
        {
            Measure = (short)measure;
            Beat = (byte)beat;
            PartialBeat = partial;
        }

        public string DebuggerDisplay => $"TMS {this.ToString()}";


        public override string ToString() => $"{Measure}m{Beat}{PartialBeat.ToString()}";
    }


    public class ShowEvent
    {
        public TimeMark BeginTimeMark;
        public TimeMark EndTimeMark;
        public double BeginTime;
        public double EndTime;
        public string Text;
        public int slot;
    }


    public class ShowEventCollection : List<ShowEvent>, INotifyPropertyChanged
    {
        private MeasureCollection mc;

        public ShowEventCollection() : base() { }

        public ShowEventCollection(MeasureCollection mc)
            : base()
        {
            this.mc = mc;
            this.mc.PropertyChanged += MeasureCollection_PropertyChanged;
        }

        public ShowEventCollection(MeasureCollection mc, int size)
            : base(size)
        {
            this.mc = mc;
        }

        private void MeasureCollection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Console.WriteLine("A property has changed: " + e.PropertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        public MeasureCollection MeasureCollection
        {
            get
            {
                return mc;
            }
            set
            {
                this.mc = value;
            }
        }
    }
}
