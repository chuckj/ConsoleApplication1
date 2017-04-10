using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Drawing;
using SharpDX.Direct2D1;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using SharpDX.DirectWrite;
using SharpDX;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using SD = System.Drawing;

namespace ConsoleApplication1
{
    public class Global : INotifyPropertyChanged
    {

        public XElement Model { get; set; }
        public AppSettings Settings;

        public MonoLit[] dta { get; set; }
        public DisplayUpdate curr { get; set; }
        public volatile DisplayUpdate currDisplay;
        public volatile MarqueUpdate currMarque;

        //public Dictionary<Tuple<int, int>, MonoLit> dict = new Dictionary<Tuple<int, int>, MonoLit>();
		public object conLock = new object();
        public Dictionary<string, TreeTransition> TreeTransitionDict = new Dictionary<string, TreeTransition>();
        public short[] tdOrder;
        public Dictionary<char, FontUpdate> FontDict = new Dictionary<char, FontUpdate>();

        public SortedList<Update, Update> TransitionQueue = new SortedList<Update, Update>();

        public volatile bool Updated = false;

        public Dictionary<string, View> VuDict = new Dictionary<string, View>();
        public List<View> vus = new List<View>();

        public Dictionary<string, GECEStrand> StrandDict = new Dictionary<string, GECEStrand>();
        public List<GECEStrand> Strands = new List<GECEStrand>(24);

        public List<Cntrl> Cntrlrs = new List<Cntrl>();

        public Dictionary<string, Lit> LitDict = new Dictionary<string, Lit>(1500);
        public Lit[] LitArray = null;

        public Dictionary<string, FeatureLit> FeatureLitDict = new Dictionary<string, FeatureLit>();

        public List<IndexPoint3D> LineVertices = new List<IndexPoint3D>(100);
        public List<short> LineIndices = new List<short>(100);
        public List<IndexPoint3D> TriVertices = new List<IndexPoint3D>(100);
        public List<short> TriIndices = new List<short>(100);

        public List<IndexPoint3D> CandleVertices = new List<IndexPoint3D>(100);

        private Song song = null;
        public Song Song
        {
            get
            {
                return this.song;
            }
            set
            {
                SetPropertyField(nameof(Song), ref this.song, value);
            }
        }

        //factory for creating 2D elements
        public SharpDX.Direct2D1.Factory factory2D1 = new SharpDX.Direct2D1.Factory(SharpDX.Direct2D1.FactoryType.SingleThreaded, DebugLevel.None); //.Information);
        //this one is for creating DirectWrite Elements
        public SharpDX.DirectWrite.Factory factoryWrite = new SharpDX.DirectWrite.Factory();


        public const int HoverDragSensitivity = 2;

        public const int PixelsPerSecond = 120;

        public int Height = 1000;
        public int Width = 1000;

        //public static Brush FontBrush;
        //public static Brush CursorBrush;
        public static int Lft = -1;

        public static Brush RangeBrush;

        public static Color RedColor;
        public static Color GreenColor;
        public static Color GhostWhiteColor;
        public static Color BlackColor;

        public const int ZOrder_Drag = 255;
        public const int ZOrder_LoopBegin = 250;
        public const int ZOrder_LoopEnd = 251;
        public const int ZOrder_LoopRange = 4;
        public const int ZOrder_Step = 100;
        public const int ZOrder_StepBg = 1;
        public const int ZOrder_Lyric = 10;
        public const int ZOrder_LyricBg = 9;
        public const int ZOrder_Rule = 5;
        public const int ZOrder_Wave = 2;
        public const int ZOrder_Measure = 110;
        public const int ZOrder_Beat = 101;
        public const int ZOrder_Slider = 104;
        public const int ZOrder_SliderThumb = 106;

        public const int Field_Width = 60;

        public const int Wave_Height = 40;
        public const int Wave_Channels = 135 - Wave_Height / 2;
        public static Color Wave_RedColor;
        public const int Wave_RedChannel = 166;
        public static Color Wave_GreenColor;
        public const int Wave_GreenChannel = 164;

        public const int Ruler_majorTick = 5;
        public const int Ruler_minorTick = 3;
        public static int Ruler_Y = 15;
        public static Color Ruler_MinuteColor;
        public static Color Ruler_MeasureColor;
        public static Color Ruler_LineColor;
        public static Color Ruler_FontColor;
        public static Color Ruler_CursorColor;
        public static Color Ruler_SecondsColor;

        public const int Slider_Height = 20;
        public static Color Slider_SliderColor;
        public static Color SliderThumb_ThumbColor;
        public static Color SliderThumb_ThumbFillColor;
        public static Color SliderCursor_CursorColor;
        public static Color Slider_LoopBeginColor;
        public static Color Slider_LoopEndColor;

        public static Color Beat_BeatColor;

        public static Color Drag_DragColor;

        public static int Lyric_Y = 30;
        public const int Lyric_Height = 13;
        public static Color Lyric_FontColor;

        public static Color Measure_NormalColor;
        public static Color Measure_GreenColor;
        public static Color Measure_RedColor;
        public static Color Measure_FontColor;

        public static Color LoopBeginColor;
        public static Color LoopEndColor;
        public static Color LoopRangeColor;

        public static int Step_Y = 50;
        public const int Step_RowHeight = 10;
        public const int Step_Padding = 10;

        public static Color Selected_PrimaryColor;
        public static Color Selected_SecondaryColor;
        public static Color HiliteColor;
        public static Color RangeColor;

        public const int pxpersec = 120;
        public const int fontSize = 8;

        public List<Action> DevIndepAcquire = new List<Action>();
        List<Action> DevIndepRelease = new List<Action>();
        List<Action> DevDepAcquire = new List<Action>();
        List<Action> DevDepRelease = new List<Action>();

        public static Dictionary<string, Func<XElement, Song, int, Viz>> StepFactoryDict;

        public Dictionary<string, BitMapImg> BitMapImgs = new Dictionary<string, BitMapImg>(32);

        public Dictionary<string, StepTransition> StepTransitionDict = new Dictionary<string, StepTransition>();

        public DMXStrand DMXStrand = new DMXStrand();

        static Global()
        {


            //  https://social.msdn.microsoft.com/Forums/en-US/eb6966f6-1f54-4c53-a6d4-39798c93ef9a/how-to-determine-the-number-of-characters-that-fit-into-a-directwrite-textlayout-out-or-drawtext?forum=winappswithnativecode
            //  https://english.r2d2rigo.es/2014/03/04/drawing-text-with-direct2d-and-directwrite-with-sharpdx/




            RedColor = Color.Red;
            GreenColor = Color.Green;
            GhostWhiteColor = Color.GhostWhite;
            BlackColor = Color.Black;

            Wave_RedColor = Color.Red;
            Wave_GreenColor = Color.Green;

            Ruler_MeasureColor = Color.Gray;
            Ruler_MinuteColor = Color.Gray;
            Ruler_LineColor = Color.Gray;
            Ruler_FontColor = Color.AntiqueWhite;
            Ruler_CursorColor = Color.White;
            Ruler_SecondsColor = Color.DarkGoldenrod;

            Slider_SliderColor = Color.AntiqueWhite;
            SliderThumb_ThumbColor = Color.AntiqueWhite;
            SliderThumb_ThumbFillColor = new Color(80, 80, 80, 80);
            SliderCursor_CursorColor = Color.White;
            Slider_LoopBeginColor = Color.Green;
            Slider_LoopEndColor = Color.Red;

            Beat_BeatColor = Color.Gray;

            Drag_DragColor = Color.AliceBlue;

            Lyric_FontColor = Color.AntiqueWhite;

            Measure_NormalColor = Color.Gray;
            Measure_GreenColor = Color.Green;
            Measure_RedColor = Color.Red;
            Measure_FontColor = Color.Goldenrod;

            RangeColor = new Color(64, 64, 64, 64);
            HiliteColor = Color.White;


            LoopBeginColor = Color.Green;
            LoopEndColor = Color.Red;
            LoopRangeColor = new Color(80, 80, 80, 80);

            Selected_PrimaryColor = Color.Cyan;
            Selected_SecondaryColor = Color.LightBlue;

            StepFactoryDict = typeof(StepBase).Assembly.GetTypes()
                .Where(t => t.IsClass && t.IsSubclassOf(typeof(StepBase)))
                .Select(t => new
                {
                    attr = (StepAttribute)t.GetCustomAttributes(true).FirstOrDefault(a => a.GetType() == typeof(StepAttribute)),
                    mi = t.GetMethod("Factory")
                })
                .Where(x => x.attr != null && !string.IsNullOrEmpty(x.attr.XmlName) && x.mi != null)
                .ToDictionary(x => x.attr.XmlName, x => (Func<XElement, Song, int, Viz>)x.mi.CreateDelegate(typeof(Func<XElement, Song, int, Viz>), null));


        }


        public static void Reallocate()
        {
            instance = null;
        }

        public int RealTime => (int)stopwatch.ElapsedMilliseconds;

        private int _parseTime = 0;

        public int ParseTime
        {
            get
            {
                return _parseTime;
            }
            set
            {
                _parseTime = value;
            }
        }

        public CancellationToken Tkn;

        public List<Update> readyList = new List<Update>();

        private Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        Global()
        {
        }

        private static object locker = new object();

        private static Global instance = null;

        public static Global Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new Global();
                        }
                    }
                }
                return instance;
            }
        }

        public void TimeReset()
        {
            stopwatch.Restart();
            ParseTime = 0;
        }

        #region PropertyChanged

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetPropertyField<T>(string propertyName, ref T field, T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
