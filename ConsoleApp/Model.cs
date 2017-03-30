using NAudio.Wave;
using Sample_NAudio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Model : INotifyPropertyChanged, IDisposable
    {
        #region Upper
        public int UpBfrLeft { get; set; }
        public int UpBfrHeight { get; set; }
        public int UpBfrWidth { get; set; }

        public int UpperLeft { get; set; }
        public int UpperHeight { get; set; }
        public int UpperWidth { get; set; }

        public int LowerLeft { get; set; }
        public int LowerHeight { get; set; }
        public int LowerXScale { get; set; }
        #endregion

        #region Fields
        private readonly BackgroundWorker waveformGenerateWorker = new BackgroundWorker();
        private int frameLength = 440 * 4 * 2; //fftDataSize;

        private bool canPlay;
        private bool canPause;
        private bool canStop;
		private bool canSerialize;
        private bool isPlaying;
        private bool isOpen;

        private WaveOut waveOutDevice;
        private WaveStream activeStream;
        private WaveChannel32 inputStream;
        private SampleAggregator sampleAggregator;
        private string pendingWaveformPath;

        #endregion

        #region Constants
        private const int waveformCompressedPointCount = 2000;
        private const int repeatThreshold = 200;

        #endregion

        #region Singleton Pattern
        private static Model instance = null;

        public static Model Instance
        {
            get
            {
                if (instance == null)
                    instance = new Model();
                return instance;
            }
        }
        #endregion

        #region Constructor
        private Model()
        {
            //positionTimer.Interval = TimeSpan.FromMilliseconds(10);
            //positionTimer.Tick += positionTimer_Tick;

            waveformGenerateWorker.DoWork += waveformGenerateWorker_DoWork;
            waveformGenerateWorker.RunWorkerCompleted += waveformGenerateWorker_RunWorkerCompleted;
            waveformGenerateWorker.WorkerSupportsCancellation = true;
        }

        private void positionTimer_Tick(object sender, EventArgs e)
        {
            inChannelTimerUpdate = true;
            ChannelPosition = ((double)ActiveStream.Position / (double)ActiveStream.Length) * ActiveStream.TotalTime.TotalSeconds;
            inChannelTimerUpdate = false;
        }

        #endregion

        #region IDisposable
        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    StopAndCloseStream();
                }

                disposed = true;
            }
        }
        #endregion

        #region IWaveformPlayer
        private TimeSpan repeatStart;
        private TimeSpan repeatStop;
        private bool inRepeatSet;

        public TimeSpan SelectionBegin
        {
            get { return repeatStart; }
            set
            {
                if (!inRepeatSet)
                {
                    inRepeatSet = true;
                    TimeSpan oldValue = repeatStart;
                    repeatStart = value;
                    if (oldValue != repeatStart)
                        NotifyPropertyChanged("SelectionBegin");
                    inRepeatSet = false;
                }
            }
        }

        public TimeSpan SelectionEnd
        {
            get { return repeatStop; }
            set
            {
                if (!inRepeatSet)
                {
                    inRepeatSet = true;
                    TimeSpan oldValue = repeatStop;
                    repeatStop = value;
                    if (oldValue != repeatStop)
                        NotifyPropertyChanged("SelectionEnd");
                    inRepeatSet = false;
                }
            }
        }

        private byte[] waveformData;
        public byte[] WaveformData
        {
            get { return waveformData; }
            protected set
            {
                byte[] oldValue = waveformData;
                waveformData = value;
                if (oldValue != waveformData)
                    NotifyPropertyChanged("WaveformData");
            }
        }

        private double channelLength;
        public double ChannelLength
        {
            get { return channelLength; }
            protected set
            {
                double oldValue = channelLength;
                channelLength = value;
                if (oldValue != channelLength)
                    NotifyPropertyChanged("ChannelLength");
            }
        }

        private double channelPosition;
        private bool inChannelSet;
        private bool inChannelTimerUpdate = false;
        public double ChannelPosition
        {
            get { return channelPosition; }
            set
            {
                if (!inChannelSet)
                {
                    inChannelSet = true; // Avoid recursion
                    double oldValue = channelPosition;
                    double position = Math.Max(0, Math.Min(value, ChannelLength));
                    if (!inChannelTimerUpdate && ActiveStream != null)
                        ActiveStream.Position = (long)((position / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
                    channelPosition = position;
                    if (oldValue != channelPosition)
                        NotifyPropertyChanged("ChannelPosition");
                    inChannelSet = false;
                }
            }
        }

        public Song song;

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region Waveform Generation
        private class WaveformGenerationParams
        {
            public WaveformGenerationParams(int points, string path)
            {
                Points = points;
                Path = path;
            }

            public int Points { get; protected set; }
            public string Path { get; protected set; }
        }

        private void GenerateWaveformData(string path)
        {
            if (waveformGenerateWorker.IsBusy)
            {
                pendingWaveformPath = path;
                waveformGenerateWorker.CancelAsync();
                return;
            }

            if (!waveformGenerateWorker.IsBusy && waveformCompressedPointCount != 0)
                waveformGenerateWorker.RunWorkerAsync(new WaveformGenerationParams(waveformCompressedPointCount, path));
        }

        private void waveformGenerateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                if (!waveformGenerateWorker.IsBusy && waveformCompressedPointCount != 0)
                    waveformGenerateWorker.RunWorkerAsync(new WaveformGenerationParams(waveformCompressedPointCount, pendingWaveformPath));
            }
        }

        private SampleAggregator waveformAggregator;
        private void waveformGenerateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            WaveformGenerationParams waveformParams = e.Argument as WaveformGenerationParams;
            Mp3FileReader waveformMp3Stream = new Mp3FileReader(waveformParams.Path);
            WaveChannel32 waveformInputStream = new WaveChannel32(waveformMp3Stream);
            waveformInputStream.Sample += waveStream_Sample;

            int frameCount = (int)((double)waveformInputStream.Length / (double)frameLength);
            byte[] readBuffer = new byte[frameLength];
            waveformAggregator = new SampleAggregator(frameLength);

            int currentPointIndex = 0;

            float[] waveformArray = new float[frameCount * 2];
            float waveformLeftMax = 0;
            float waveformRightMax = 0;
            int readCount = 0;
            while (currentPointIndex < frameCount * 2)
            {
                waveformInputStream.Read(readBuffer, 0, readBuffer.Length);

                waveformArray[currentPointIndex++] = waveformAggregator.LeftMaxVolume;
                waveformArray[currentPointIndex++] = waveformAggregator.RightMaxVolume;

                if (waveformAggregator.LeftMaxVolume > waveformLeftMax)
                    waveformLeftMax = waveformAggregator.LeftMaxVolume;
                if (waveformAggregator.RightMaxVolume > waveformRightMax)
                    waveformRightMax = waveformAggregator.RightMaxVolume;

                waveformAggregator.Clear();

                if (waveformGenerateWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                readCount++;
            }
            byte[] waveformBytes = new byte[waveformArray.Length];
            float factor = 31f / Math.Max(Math.Abs(waveformLeftMax), Math.Abs(waveformRightMax));
            for (int ndx = 0; ndx < waveformArray.Length; ndx++)
                waveformBytes[ndx] = (byte)Math.Abs(Math.Abs(waveformArray[ndx]) * factor);

            //UI.Invoke(new Action(() => { WaveformData = waveformBytes; }));
            waveformData = waveformBytes;

            waveformInputStream.Close();
            waveformInputStream.Dispose();
            waveformInputStream = null;
            waveformMp3Stream.Close();
            waveformMp3Stream.Dispose();
            waveformMp3Stream = null;
        }
        #endregion

        #region Private Utility Methods
        private void StopAndCloseStream()
        {
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
        }
        #endregion

        #region Public Methods
        public void Stop()
        {
            if (waveOutDevice != null)
            {
                waveOutDevice.Stop();
            }
            IsPlaying = false;
            CanStop = false;
            CanPlay = true;
            CanPause = false;
        }

        public void Pause()
        {
            if (IsPlaying && CanPause)
            {
                waveOutDevice.Pause();
                IsPlaying = false;
                CanPlay = true;
                CanPause = false;
            }
        }

        public void Play()
        {
            if (CanPlay)
            {
                waveOutDevice.Play();
                IsPlaying = true;
                CanPause = true;
                CanPlay = false;
                CanStop = true;
            }
        }

        public void OpenFile(string path)
        {
            Stop();
            IsOpen = false;

            if (ActiveStream != null)
            {
                SelectionBegin = TimeSpan.Zero;
                SelectionEnd = TimeSpan.Zero;
                ChannelPosition = 0;
            }

            StopAndCloseStream();

            if (System.IO.File.Exists(path))
            {
                try
                {
                    waveOutDevice = new WaveOut()
                    {
                        DesiredLatency = 100
                    };
                    ActiveStream = new Mp3FileReader(path);
                    inputStream = new WaveChannel32(ActiveStream);
                    Console.WriteLine(inputStream.TotalTime.ToString());
                    sampleAggregator = new SampleAggregator(frameLength);
                    inputStream.Sample += inputStream_Sample;
                    waveOutDevice.PlaybackStopped += waveOutDevice_PlaybackStopped;
                    waveOutDevice.Init(inputStream);
                    ChannelLength = inputStream.TotalTime.TotalSeconds;

                    GenerateWaveformData(path);
                    IsOpen = true;
                    CanPlay = true;
                }
                catch (Exception ex)
                {
                    ActiveStream = null;
                    CanPlay = false;
                }
            }
        }
        #endregion

        #region Event Handlers
        private void inputStream_Sample(object sender, SampleEventArgs e)
        {
            sampleAggregator.Add(e.Left, e.Right);
            long repeatStartPosition = (long)((SelectionBegin.TotalSeconds / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
            long repeatStopPosition = (long)((SelectionEnd.TotalSeconds / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
            if (((SelectionEnd - SelectionBegin) >= TimeSpan.FromMilliseconds(repeatThreshold)) && ActiveStream.Position >= repeatStopPosition)
            {
                sampleAggregator.Clear();
                ActiveStream.Position = repeatStartPosition;
            }
        }

        void waveStream_Sample(object sender, SampleEventArgs e)
        {
            waveformAggregator.Add(e.Left, e.Right);
        }

        void waveOutDevice_PlaybackStopped(object sender, EventArgs e)
        {
            //IsPlaying = false;
            //CanStop = false;
            //CanPlay = true;
            //CanPause = false;
        }
        #endregion

        #region Public Properties

        private string fileName = string.Empty;
        public string FileName
        {
            get { return fileName; }
            protected set
            {
                string oldValue = fileName;
                fileName = value;
                if (oldValue != fileName)
                    NotifyPropertyChanged("FileName");
            }
        }

        public WaveStream ActiveStream
        {
            get { return activeStream; }
            protected set
            {
                WaveStream oldValue = activeStream;
                if (activeStream == value) return;

                activeStream = value;
                NotifyPropertyChanged("ActiveStream");
            }
        }

        public bool CanPlay
        {
            get { return canPlay; }
            protected set
            {
                if (canPlay == value) return;

                canPlay = value;
                NotifyPropertyChanged("CanPlay");
            }
        }

        public bool CanPause
        {
            get { return canPause; }
            protected set
            {
                if (canPause == value) return;

                canPause = value;
                NotifyPropertyChanged("CanPause");
            }
        }

        public bool CanStop
        {
            get { return canStop; }
            protected set
            {
                if (canStop == value) return;

                canStop = value;
                NotifyPropertyChanged("CanStop");
            }
        }


        public bool IsOpen
        {
            get { return isOpen; }
            protected set
            {
                if (isOpen == value) return;

                isOpen = value;
                NotifyPropertyChanged("IsOpen");
            }
        }

		public bool CanSerialize
		{
			get { return canSerialize; }
			protected set
			{
				if (canSerialize == value) return;

				canSerialize = value;
				NotifyPropertyChanged("CanSerialize");
			}
		}

		public bool IsPlaying
        {
            get { return isPlaying; }
            protected set
            {
                if (isPlaying == value) return;

                isPlaying = value;
                NotifyPropertyChanged("IsPlaying");
                //positionTimer.IsEnabled = value;
            }
        }
        #endregion
    }
}
