using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SD = System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Threading;
using System.ComponentModel;
using Colors = System.Drawing.Color;
using NAudio.Wave;
using Sample_NAudio;
using System.IO.MemoryMappedFiles;


namespace ConsoleApplication1
{
    public enum PlayerMode { closed, stopped, paused, playing }

    public class Song : INotifyPropertyChanged, IDisposable
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(nameof(Song));

        private Measure[] measures = new Measure[0];
        private List<Viz> vizs = new List<Viz>();
        private System.Collections.ObjectModel.ObservableCollection<Viz> selected = new ObservableCollection<Viz>();
        private List<Change> changes = new List<Change>();
        private int changePtr = -1;
        private SortedList<int, TimeMarkShort> timePoints = new SortedList<int, TimeMarkShort>();
        private List<Viz> copied = new List<Viz>();
        private Drag drag;

        private float bgnmTm;
        private float endmTm;
        private float endtTm;
        
        private int musicBeginPx;
        private int musicEndPx;

        private int loopBeginPx;
        private int loopEndPx;

        private static Regex regex = null;
        private static int regexGrpMeasure = 0;
        private static int regexGrpBeat = 0;
        private static int regexGrpNum = 0;
        private static int regexGrpDenom = 0;
        private static int regexGrpSecs = 0;

        private WaveOut waveOutDevice;
        private WaveStream activeStream;
        private WaveChannel32 inputStream;


        public ToolStripStatusLabel ssHoverSep;
        public ToolStripStatusLabel ssHover;
        public ToolStripStatusLabel ssSelectedSep;
        public ToolStripStatusLabel ssSelected;

        public bool IsDevIndepResourcesAcquired;
        public bool IsDevDepResourcesAcquired;

        //public ObservableCollection<Viz> Selected = null;


        public int LFT = 0;
        public int RIT = 0;

        private float trackTime;
        public byte[] WaveformData = null;


        public string DisplayInfoFileName { get; set; }
        public int DisplayMapSize = 0;
        public DisplayInfo DisplayInfo { get; set; }
        public object DisplayInfoLock = new object();

        private IProgress<int> progress = null;

        public int Position
        {
            get
            {
                return offset;
            }
            set
            {
                if (SetPropertyField(nameof(Position), ref offset, value))
                {
                    if (PlayerMode != PlayerMode.playing)
                        inputStream.Position = offset;
                }
            }
        }

        private int offset = 0;

        private DragMode dragMode = DragMode.Off;

        public DragMode DragMode { get; set; }

        public SD.Point DragBegin { get; set; }
        public SD.Point DragEnd { get; set; }

        public Rectangle DragBox => new Rectangle(Math.Min(DragBegin.X, DragEnd.X), Math.Min(DragBegin.Y, DragEnd.Y), Math.Abs(DragBegin.X - DragEnd.X), Math.Abs(DragBegin.Y - DragEnd.Y));

        //public SDX.Rectangle DragRectangle {  get { return new SDX.Rectangle(DrawBox); } }

        private void Selected_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (selected.Count == 0)
            {
                ssSelectedSep.Visible = ssSelected.Visible = false;
            }
            else
            {
                ssSelected.Text = this.ssSelectedDD;
                ssSelectedSep.Visible = ssSelected.Visible = true;
            }
        }




        public static Song New(ToolStripStatusLabel ssHoverSep, ToolStripStatusLabel ssHover, ToolStripStatusLabel ssSelectedSep, ToolStripStatusLabel ssSelected)
        {
            var song = new Song();
            song.ssHoverSep = ssHoverSep;
            song.ssHover = ssHover;
            song.ssSelectedSep = ssSelectedSep;
            song.ssSelected = ssSelected;

            //	load ruler
            song.CreateRule();

            song.CreateWave();

            song.CreateLoopBegin();
            song.CreateLoopEnd();
            song.CreateLoopRange();

            song.CreateSlider();
            song.CreateSliderThumb();

            //	load lyrics
            song.CreateLyricBg();

            song.drag = song.CreateDrag();

            song.Order();
            song.ResetPoints();

            return song;
        }

        public static Song Open(ToolStripStatusLabel ssHoverSep, ToolStripStatusLabel ssHover, ToolStripStatusLabel ssSelectedSep, ToolStripStatusLabel ssSelected, string xmlname)
        {
            var song = Song.New(ssHoverSep, ssHover, ssSelectedSep, ssSelected);

            song.Selected.CollectionChanged += song.Selected_CollectionChanged;

            var xdoc = XDocument.Load(xmlname);

            var root = xdoc.Element("song");
            string path = Path.GetPathRoot(Application.StartupPath).Replace("\\", "");
            song.fileName = ((string)root.Attribute("filename")).Replace("@@path@@", path);
            song.DisplayInfoFileName = song.FileName + ".bin";

            song.KeyFrames = root.Element("keyframes").Elements("keyframe")
                .Select(xml => new KeyFrame()
                {
                    Measure = (int)xml.Attribute("measure"),
                    BeatsPerMeasure = (xml.Attribute("beatspermeasure") == null ? 0 : (int)xml.Attribute("beatspermeasure")),
                    Time = (float)xml.Attribute("time")
                }).ToList();

            foreach (var xml in root.Element("lyrics").Elements("lyric"))
            {
                song.CreateLyric((string)xml.Attribute("text"), song.TimeMarkParse((string)xml.Attribute("time")));
            }

            //	load steps
            song.CreateStepBg();

            foreach (var elm in root.Element("steps").Elements())
            {
                if (elm.Name == "repeat")
                {
                    for (int ndx = 1; ; ndx++)
                    {
                        var attrb = elm.Attribute("tm" + ndx);
                        if (attrb == null) break;
                        var measureOffset = (int)attrb;
                        foreach (var sub in elm.Elements())
                        {
                            stepHelper(song, sub, measureOffset);
                        }
                    }
                }
                else
                {
                    stepHelper(song, elm, 0);
                }
            }

            var lup = root.Element("loop");
            if (lup != null)
            {
                if (lup.Attribute("begin") != null)
                    song.LoopBeginPx = (int)lup.Attribute("begin");
                if (lup.Attribute("end") != null)
                    song.LoopEndPx = (int)lup.Attribute("end");
            }

            song.Order();
            song.ResetPoints();

            song.ArrangeSteps();

            song.ResetPoints();

            //Recalculate();

            //bool y = false;
            //foreach (var step in stepManager.Steps)
            //{
            //	step.Brush = (y) ? Brushes.LightBlue : Brushes.Goldenrod;
            //	y = !y;
            //}


            try
            {
                song. waveOutDevice = new WaveOut()
                {
                    DesiredLatency = 100
                };
                song.ActiveStream = new Mp3FileReader(song.fileName);
                song.inputStream = new WaveChannel32(song.activeStream);
                logger.Info($"Ttl time:{song.inputStream.TotalTime}");
                song.waveOutDevice.PlaybackStopped += song.waveOutDevice_PlaybackStopped;
                song.waveOutDevice.Init(song.inputStream);
                song.endmTm = (float)song.inputStream.TotalTime.TotalSeconds;
                song.musicEndPx = (int)(song.endmTm * Global.pxpersec);
                song.trackTime = (float)song.inputStream.TotalTime.TotalSeconds;

                var tkn = new CancellationTokenSource().Token;
                new WaveformSampler(song, tkn).Sample();

                song.PlayerMode = PlayerMode.stopped;
            }
            catch (Exception ex)
            {
                song.dispose();
                return null;
            }

            Colors[] clrtbl = new[] { Colors.Red, Colors.Blue, Colors.Yellow, Colors.Green, Colors.Orange };
            var dict = Global.Instance.LitDict;
            var lits = new Vector4[5][];
            for (int ndx = 0; ndx < 5; ndx++)
            {
                lits[ndx] =
                     Global.Instance.dta.Select((x, n) => new Vector4(x.Clr.R / 255.0f, x.Clr.G / 255.0f, x.Clr.B / 255.0f, 0))
                     .Concat(Global.Instance.LitDict.Values.OfType<RGBLit>().Select((x, n) => (Vector4)(Clr)clrtbl[(n + ndx) % 5])).ToArray();
            }

            song.litValues = lits;

            song.DisplayMapSize = (int)Math.Ceiling(song.TrackTime * 30) * 4;

            var tsk = RunTime.RunNow(song);
            tsk.Wait();

            return song;
        }

        private static void stepHelper(Song song, XElement elm, int measureOffset)
        {
            Func<XElement, Song, int, Viz> fact;
            if (!Global.StepFactoryDict.TryGetValue(elm.Name.LocalName, out fact))
                throw new ArgumentException("Unknown xml command: " + elm.Name.LocalName);
            var step = fact(elm, song, measureOffset);
            step.ZOrder = Global.ZOrder_Step;
            song.vizs.Add(step);
        }

        //private void inputStream_Sample(object sender, SampleEventArgs e)
        //{
        //    sampleAggregator.Add(e.Left, e.Right);
        //    long repeatStartPosition = (long)((SelectionBegin.TotalSeconds / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
        //    long repeatStopPosition = (long)((SelectionEnd.TotalSeconds / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
        //    if (((SelectionEnd - SelectionBegin) >= TimeSpan.FromMilliseconds(repeatThreshold)) && ActiveStream.Position >= repeatStopPosition)
        //    {
        //        sampleAggregator.Clear();
        //        ActiveStream.Position = repeatStartPosition;
        //    }
        //}



        void waveOutDevice_PlaybackStopped(object sender, EventArgs e)
        {
            PlayerMode = PlayerMode.stopped;
        }

        private List<KeyFrame> keyFrames;


        static Song()
        {
            regex = new Regex("^(?<measure>\\d+)m(?<beat>\\d+)(\\+(?<num>\\d+)/(?<denom>\\d+))?(\\+(?<secs>\\d{0,3}\\.\\d{1,3}))?$",
                RegexOptions.ExplicitCapture | RegexOptions.Compiled);
            regexGrpMeasure = regex.GroupNumberFromName("measure");
            regexGrpBeat = regex.GroupNumberFromName("beat");
            regexGrpNum = regex.GroupNumberFromName("num");
            regexGrpDenom = regex.GroupNumberFromName("denom");
            regexGrpSecs = regex.GroupNumberFromName("secs");
        }

        public Song()
        {
            keyFrames = new KeyFrameList();

            vizs = new List<Viz>();

            GeneratePercentage = null;

        }

        public SortedList<int, TimeMarkShort> TimePoints => timePoints;

        public Drag Drag { get; set; }

        public bool IsChanged { get; set; }

        public float TrackTime => trackTime;

        public bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            VizCmd chain;

            if (keyData == (Keys.Z | Keys.Control))
            {
                if (changePtr < 0) return false;
                var change = changes[changePtr--];
                var vizCmds = change.vizCmds;
                change.vizCmds = new List<VizCmd>(vizCmds.Count);

                if (vizCmds.Any(c => c.cmd == Cmd.Insert))
                    Global.Instance.Song.Selected.Clear();

                for (int ndx = 0; ndx < vizCmds.Count; ndx++)
                {
                    var vizCmd = vizCmds[ndx];
                    if ((chain = vizCmd.viz.XeqCmd(vizCmd)) != null)
                        change.vizCmds.Add(chain);
                }
                return true;
            }

            else if (keyData == (Keys.Z | Keys.Control | Keys.Shift))
            {
                if (changePtr >= changes.Count - 1) return false;
                var change = changes[++changePtr];
                var vizCmds = change.vizCmds;
                change.vizCmds = new List<VizCmd>(vizCmds.Count);

                if (vizCmds.Any(c => c.cmd == Cmd.Insert))
                    Global.Instance.Song.Selected.Clear();

                for (int ndx = 0; ndx < vizCmds.Count; ndx++)
                {
                    var vizCmd = vizCmds[ndx];
                    if ((chain = vizCmd.viz.XeqCmd(vizCmd)) != null)
                    {
                        change.vizCmds.Add(chain);
                    }
                }
                return true;
            }

            else if (selected.Count > 0)
            {
                List<VizCmd> vizCmds = new List<VizCmd>();
                VizCmd cmd;
                bool valid = true;
                foreach (Viz viz in selected)
                {
                    if ((cmd = viz.TranslateToCmd(ref msg, keyData)) != null)
                        vizCmds.Add(cmd);
                    else
                        valid = false;
                }

                if (valid && (vizCmds.Count > 0))
                {
                    var undoes = new List<VizCmd>(vizCmds.Count);

                    for (int ndx = 0; ndx < vizCmds.Count; ndx++)
                    {
                        var vizCmd = vizCmds[ndx];
                        if ((chain = vizCmd.viz.XeqCmd(vizCmd)) != null)
                            undoes.Add(chain);
                    }

                    Remember(undoes);
                    return true;
                }
            }
            return false;
        }

        public void Remember(List<VizCmd> undoes)
        {
            if (changes.Count - 1 > changePtr)
                changes.RemoveRange(changePtr + 1, changes.Count - changePtr - 1);
            changes.Add(new Change() { vizCmds = undoes });
            changePtr = changes.Count - 1;
        }

        public void Select(Viz viz, bool ShiftKey)
        {
            if (viz.IsSelectable)
            {
                if (ShiftKey
                    || (!viz.IsMultiSelectable)
                    || ((Selected.Count > 0) && (viz.GetType() != Selected[0].GetType())))
                {
                    Selected.Clear();
                }
                if (Selected.Contains(viz))
                    Selected.Remove(viz);
                else
                    Selected.Add(viz);
            }
            else
            {
                Selected.Clear();
            }

        }
        private int cursorPx = 0;
        public int CursorPx
        {
            get
            {
                return cursorPx;
            }
            set
            {
                cursorPx = value;
            }
        }

        public List<KeyFrame> KeyFrames
        {
            set
            {
                if (value.Count < 2)
                    throw new ArgumentOutOfRangeException("At least 2 KeyFrames are required.");

                keyFrames = value.OrderBy(k => k.Measure).ToList();
                bgnmTm = keyFrames[0].Time;
                endmTm = keyFrames[keyFrames.Count - 1].Time;

                musicBeginPx = (int)(bgnmTm * Global.pxpersec);
                musicEndPx = (int)(endmTm * Global.pxpersec);

                measures = new Measure[keyFrames[keyFrames.Count - 1].Measure + 1];

                var kfEnum = keyFrames.GetEnumerator();
                if (kfEnum.MoveNext())
                {
                    var kf = kfEnum.Current;
                    float tm = kf.Time;
                    while (kfEnum.MoveNext())
                    {
                        var kfNxt = kfEnum.Current;
                        var span = kfNxt.Measure - kf.Measure;
                        float tpm = (kfNxt.Time - kf.Time) / (span);
                        float tpb = tpm / kf.BeatsPerMeasure;
                        bool isKeyFrame = true;
                        for (int m = kf.Measure; m < kfNxt.Measure; m++)
                        {
                            CreateMeasure(m, kf.BeatsPerMeasure, tm, tpb, isKeyFrame);

                            isKeyFrame = false;

                            float xbeat = tm;
                            //	add beats
                            for (int beat = 2; beat <= kf.BeatsPerMeasure; beat++)
                            {
                                xbeat += tpb;
                                CreateBeat(m, beat, xbeat, tpb);
                            }

                            tm += tpm;
                        }
                        kf = kfNxt;
                    }
                    CreateMeasure(kf.Measure, 0, kf.Time, 0, true);
                }

                //				NotifyPropertyChanged("Collection");
            }
            get
            {
                return keyFrames;
            }
        }

        public void AddTimePoints(int measure, int beat, float startingTime, float timePerBeat)
        {
            var x = (int)(startingTime * Global.pxpersec);
            AddTimePoint(x, measure, beat, PartialBeats.None);

            if ((measure > 0) && (timePerBeat > 0))
            {
                x = (int)((startingTime + timePerBeat * 1 / 4) * Global.pxpersec);
                AddTimePoint(x, measure, beat, PartialBeats.TwoOfFour);

                x = (int)((startingTime + timePerBeat * 1 / 3) * Global.pxpersec);
                AddTimePoint(x, measure, beat, PartialBeats.TwoOfThree);

                x = (int)((startingTime + timePerBeat * 1 / 2) * Global.pxpersec);
                AddTimePoint(x, measure, beat, PartialBeats.TwoOfTwo);

                x = (int)((startingTime + timePerBeat * 2 / 3) * Global.pxpersec);
                AddTimePoint(x, measure, beat, PartialBeats.ThreeOfThree);

                x = (int)((startingTime + timePerBeat * 3 / 4) * Global.pxpersec);
                AddTimePoint(x, measure, beat, PartialBeats.FourOfFour);
            }
        }

        public void AddTimePoint(int x, int measure, int beat, PartialBeats partial)
        {
            timePoints.Add(x, new TimeMarkShort(measure, beat, partial));
        }

        public Measure[] Measures { get { return measures; } set { measures = value; } }
        public int MeasureCount => measures.Length - 1;
        public float MusicBeginTime => bgnmTm;
        public float MusicEndTime => endmTm;
//        public float TrackTime { get { return endtTm; } set { endtTm = value; } }
        public int TrackPx => (int)(trackTime * Global.pxpersec);
        public int MusicBeginPx => musicBeginPx;
        public int MusicEndPx => musicEndPx;
        public int LoopBeginPx { get { return loopBeginPx; } set { loopBeginPx = value; } }
        public int LoopEndPx { get { return loopEndPx; } set { loopEndPx = value; } }
        public ObservableCollection<Viz> Selected => selected;
        public List<Viz> Copied => copied;
        public List<Change> Changes => changes;

        public string ssSelectedDD
        {
            get
            {
                var selCnt = (selected.Count > 1) ? "*" + selected.Count.ToString() + " " : "";
                return selCnt + Selected[0].DebuggerDisplay;
            }
        }

        public void DevDepReacquireAll(RenderTarget target)
        {
            VizRes.DevDepReacquireAll(target);
        }

        #region VIZ

        public LoopBegin CreateLoopBegin()
        {
            var step = new LoopBegin();
            step.ZOrder = Global.ZOrder_LoopBegin;
            vizs.Add(step);
            return step;
        }

        public LoopEnd CreateLoopEnd()
        {
            var step = new LoopEnd();
            step.ZOrder = Global.ZOrder_LoopEnd;
            vizs.Add(step);
            return step;
        }

        public LoopRange CreateLoopRange()
        {
            var step = new LoopRange();
            step.ZOrder = Global.ZOrder_LoopRange;
            vizs.Add(step);
            return step;
        }

        //public StepV CreateStep(string text, TimeMark startTimeMark, TimeMark endTimeMark)
        //{
        //    var step = new StepV(text, startTimeMark, endTimeMark);
        //    step.ZOrder = Global.ZOrder_Step;
        //    vizs.Add(step);
        //    return step;
        //}

        //public StepI CreateStepI(string text, TimeMark startTimeMark, TimeMark endTimeMark, string mode)
        //{
        //    var step = new StepI(text, startTimeMark, endTimeMark, int.Parse(mode));
        //    step.ZOrder = Global.ZOrder_Step;
        //    vizs.Add(step);
        //    return step;
        //}

        public StepBg CreateStepBg()
        {
            var step = new StepBg();
            step.ZOrder = Global.ZOrder_StepBg;
            vizs.Add(step);
            return step;
        }


        public Lyric CreateLyric(string text, TimeMark timemark)
        {
            var lyric = new Lyric(text, timemark);
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

        public Drag CreateDrag()
        {
            var drag = new Drag();
            drag.ZOrder = Global.ZOrder_Drag;
            vizs.Add(drag);
            return drag;
        }


        public Measure CreateMeasure(int startingMeasure, int beatsPerMeasure, float startingTime, float timePerBeat, bool isKeyFrame)
        {
            var measure = new Measure(startingMeasure, beatsPerMeasure, startingTime, timePerBeat, isKeyFrame);
            measure.ZOrder = Global.ZOrder_Measure;
            vizs.Add(measure);
            Measures[startingMeasure] = measure;
            return measure;
        }

        public Beat CreateBeat(int startingMeasure, int beats, float startingTime, float timePerBeat)
        {
            var beat = new Beat(startingMeasure, beats, startingTime, timePerBeat);
            beat.ZOrder = Global.ZOrder_Beat;
            vizs.Add(beat);
            return beat;
        }

        public Slider CreateSlider()
        {
            var slider = new Slider();
            slider.ZOrder = Global.ZOrder_Slider;
            vizs.Add(slider);
            return slider;
        }

        public SliderThumb CreateSliderThumb()
        {
            var thumb = new SliderThumb();
            thumb.ZOrder = Global.ZOrder_SliderThumb;
            vizs.Add(thumb);
            return thumb;
        }



        public List<Viz> Vizs => vizs;

        public void Order()
        {
            vizs = vizs.OrderBy(l => l.ZOrder).ThenBy(l => l.StartPoint.X).ToList();
        }

        public void ArrangeSteps()
        {
            float[] stopTime = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (StepBase step in vizs.OfType<StepBase>())
            {
                int row = 0;
                for (row = 0; row < 8; row++)
                {
                    if (stopTime[row] < step.StartPoint.X)
                    {
                        stopTime[row] = step.EndPoint.X + Global.Step_Padding;
                        break;
                    }
                }
                step.row = row * Global.Step_RowHeight;
            }
        }


        public void ResetPoints()
        {
            timePoints = new SortedList<int, TimeMarkShort>(measures.Count() * 4 * 5);
            foreach (var viz in vizs)
                viz.ResetPoints(this);
        }

        public void Draw(DrawData dd)
        {
            foreach (var viz in vizs)
            {
                viz.Draw(dd);
            }
        }

        public XElement Serialize() => new XElement("song",
            new XAttribute("filename", FileName),
            new XElement("keyframes",
                measures.Where(m => m != null && m.IsKeyFrame).Select(m => m.Serialize(this))),
            new XElement("lyrics",
                vizs.Where(v => v is Lyric).Select(v => v.Serialize(this))),
            new XElement("steps",
                vizs.Where(v => v is StepBase).Select(v => v.Serialize(this))));

        public IEnumerable<VizCmd> cmCopy(Viz viz = null)
        {
            copied.Clear();
            if (selected.Count == 0)
            {
                copied.Add(viz);
            }
            else
            {
                copied.AddRange(selected);
            }
            return null;
        }

        public IEnumerable<VizCmd> cmDuplicate(Viz viz = null)
        {
            if (selected.Count == 0)
            {
                return new List<VizCmd>() { new VizCmd(viz.Clone(), Cmd.Insert) };
            }
            else
            {
                return selected.Select(v => new VizCmd(v.Clone(), Cmd.Insert)).ToList();
            }
        }

        public IEnumerable<VizCmd> cmDelete(Viz viz = null)
        {
            if (selected.Count == 0)
            {
                return new List<VizCmd>() { new VizCmd(viz, Cmd.Delete) };
            }
            else
            {
                return selected.Select(v => new VizCmd(v, Cmd.Delete)).ToList();
            }
        }
        #endregion

        #region Timemark
        public bool TimeMarkTryParse(string txt, out TimeMark value, int measureOffset = 0)
        {
            value = null;
            var match = regex.Match(txt);
            if (!match.Success) return false;

            var tm = new TimeMark();
            if (!short.TryParse(match.Groups[regexGrpMeasure].Value, out tm.Measure)) return false;
            tm.Measure += (short)measureOffset;
            Measure curr = measures[tm.Measure];
            if (curr == null) return false;

            if (!short.TryParse(match.Groups[regexGrpBeat].Value, out tm.Beat)) return false;
            if ((tm.Beat < 1) || (tm.Beat > curr.BeatsPerMeasure)) return false;

            if (match.Groups[regexGrpNum].Value.Length > 0)
            {
                short num, denom;
                if (!short.TryParse(match.Groups[regexGrpNum].Value, out num)) return false;
                if (!short.TryParse(match.Groups[regexGrpDenom].Value, out denom)) return false;
                if ((num == 2) && (denom == 4)) tm.PartialBeat = PartialBeats.TwoOfFour;
                else if ((num == 2) && (denom == 3)) tm.PartialBeat = PartialBeats.TwoOfThree;
                else if ((num == 2) && (denom == 2)) tm.PartialBeat = PartialBeats.TwoOfTwo;
                else if ((num == 3) && (denom == 3)) tm.PartialBeat = PartialBeats.ThreeOfThree;
                else if ((num == 3) && (denom == 4)) tm.PartialBeat = PartialBeats.ThreeOfFour;
                else if ((num == 4) && (denom == 4)) tm.PartialBeat = PartialBeats.FourOfFour;
                else return false;
            }

            if (match.Groups[regexGrpSecs].Value.Length > 0)
            {
                float milli;
                if (!float.TryParse(match.Groups[regexGrpSecs].Value, out milli)) return false;
                tm.Milliseconds = (short)(milli * 1000);
            }
            tm.Time = ToTime(tm) * Global.pxpersec;
            value = tm;

            return true;
        }

        public TimeMark TimeMarkParse(string txt, int measureOffset = 0)
        {
            TimeMark tm;
            TimeMarkTryParse(txt, out tm, measureOffset);
            return tm;
        }

        public float ToTime(int measure) => ToTimeHelper((short)measure);

        public float ToTime(short measure, short beat) => ToTimeHelper(measure, beat);

        public float ToTime(TimeMark tm) => ToTimeHelper(tm.Measure, tm.Beat, tm.PartialBeat, tm.Milliseconds);

        public float ToTimeHelper(short measure, short beat = 1, PartialBeats partialBeat = PartialBeats.None, short milliseconds = 0)
        {
            if ((measure <= 0) || (measure >= measures.Length)) return 0f;
            Measure curr = measures[measure];
            if (curr == null) return 0f;

            return curr.StartingTime
               + curr.TimePerBeat
                   * (((measure - curr.StartingMeasure) * curr.BeatsPerMeasure)
                       + ((beat > 1) ? (beat - 1) : 0) + partialBeat.ToFraction())
               + milliseconds / 1000f;
        }

        public TimeMark NormalizedTimeMark(int x)
        {
            if (x < 0) return null;

            var ndx = timePoints.FindFirstIndexGreaterThanOrEqualTo(x);
            float key = timePoints.Keys[ndx];

            if (x < key)
            {
                key = timePoints.Keys[--ndx];
            }
            var tm = new TimeMark(timePoints.Values[ndx], x - key);
            tm.Time = ToTime(tm) * Global.pxpersec;
            return tm;
        }

        public int Beat(int x)
        {
            int ndx = timePoints.FindFirstIndexGreaterThanOrEqualTo(x);
            while ((ndx > 0) && (timePoints.Values[ndx].PartialBeat != PartialBeats.None))
                ndx--;
            return ndx;
        }
        #endregion



        public void Close()
        {
            PlayerMode = PlayerMode.stopped;
            LoopBeginPx = 0;
            loopEndPx = 0;
            CurrentPosition = 0;

            lock (DisplayInfoLock)
            {
                var dspinfo = DisplayInfo;
                DisplayInfo = null;
                if (dspinfo != null)
                {
                    dspinfo.Dispose();
                }
            }
        }


        public int? GeneratePercentage { get; set; }
        private Thread generatorThread;

        public void Generate()
        {
            generatorThread = new Thread(new ThreadStart(Generator));
            GeneratePercentage = 0;
            generatorThread.Start();
        }

        private void Generator()
        {
            try
            {
                for (int pctg = 0; pctg < 101; pctg++)
                {
                    Thread.Sleep(200);
                    GeneratePercentage = pctg;
                }
                Thread.Sleep(200);
            }
            finally
            {
                GeneratePercentage = null;
            }
        }

        private PlayerMode playerMode = PlayerMode.stopped;

        public PlayerMode PlayerMode
        {
            get
            {
                return playerMode;
            }
            set
            {
                SetPropertyField(nameof(PlayerMode), ref playerMode, value);
                switch (value)
                {
                    case PlayerMode.playing:
                        waveOutDevice.Play();
                        break;

                    case PlayerMode.stopped:
                        waveOutDevice.Stop();
                        break;

                    case PlayerMode.paused:
                        waveOutDevice.Pause();
                        break;

                    case PlayerMode.closed:
                        dispose();
                        break;
                }
            }
        }

        private void dispose()
        {
            Dispose();
        }

        public WaveStream ActiveStream
        {
            get { return activeStream; }
            protected set
            {
                SetPropertyField(nameof(ActiveStream), ref activeStream, value);
            }
        }

        private int currentPosition = 0;
        public int CurrentPosition
        {
            get { return currentPosition; }
            protected set
            {
                SetPropertyField(nameof(CurrentPosition), ref currentPosition, value);
            }
        }

        private string fileName;

        public string FileName
        {
            get { return fileName; }
            protected set
            {
                SetPropertyField(nameof(FileName), ref fileName, value);
            }
        }


        private bool repeating = false;
        public bool Repeating
        {
            get
            {
                return repeating;
            }
            set
            {
                SetPropertyField(nameof(Repeating), ref repeating, value);
            }
        }

        private Vector4[][] litValues = null;
        public Vector4[][] LitValues
        {
            get
            {
                return litValues;
            }
            set
            {
                SetPropertyField(nameof(LitValues), ref litValues, value);
            }
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

        protected bool SetPropertyField<T>(string propertyName, ref T field, T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                if (DisplayInfo != null)
                {
                    DisplayInfo.Dispose();
                    DisplayInfo = null;
                }
                if (waveOutDevice != null)
                {
                    waveOutDevice.Stop();
                }
                if (activeStream != null)
                {
                    inputStream.Close();
                    inputStream = null;
                    ActiveStream.Close();
                    ActiveStream = null;
                }
                if (waveOutDevice != null)
                {
                    waveOutDevice.Dispose();
                    waveOutDevice = null;
                }

                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Song()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
