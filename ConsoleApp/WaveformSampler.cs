using Microsoft.ConcurrencyVisualizer.Instrumentation;
using NAudio.Wave;
using Sample_NAudio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class WaveformSampler
    {
        private Song song;

        #region Fields
        private readonly BackgroundWorker waveformGenerateWorker = new BackgroundWorker();
        private int frameLength = 44100 / Global.pxpersec * 4 * 2; //fftDataSize;
        #endregion

        private CancellationToken tkn;
        private SampleAggregator waveformAggregator;


        public WaveformSampler(Song song, CancellationToken tkn)
        {
            this.song = song;
            this.tkn = tkn;
        }

        public void Sample()
        { 
            var tsk = Task.Run(() => waveformGenerateWorker_DoWork(), tkn);
        }


        private void waveformGenerateWorker_DoWork()
        {
#if (MARKERS)
            var span = Markers.EnterSpan("waveformGen");
#endif

            using (Mp3FileReader waveformMp3Stream = new Mp3FileReader(song.FileName))
            using (WaveChannel32 waveformInputStream = new WaveChannel32(waveformMp3Stream))
            {
                waveformInputStream.Sample += waveStream_Sample;

                int frameCount = (int)((float)waveformInputStream.Length / frameLength);
                byte[] readBuffer = new byte[frameLength];
                waveformAggregator = new SampleAggregator(frameLength);

                int currentPointIndex = 0;

                float[] waveformArray = new float[frameCount * 2];
                float waveformLeftMax = 0;
                float waveformRightMax = 0;

                while (currentPointIndex < frameCount * 2)
                {
                    waveformInputStream.Read(readBuffer, 0, readBuffer.Length);

                    var leftMaxVolume = waveformAggregator.LeftMaxVolume;
                    var rightMaxVolume = waveformAggregator.RightMaxVolume;
                    waveformArray[currentPointIndex++] = leftMaxVolume;
                    waveformArray[currentPointIndex++] = rightMaxVolume;

                    if (leftMaxVolume > waveformLeftMax)
                        waveformLeftMax = leftMaxVolume;
                    if (rightMaxVolume > waveformRightMax)
                        waveformRightMax = rightMaxVolume;

                    waveformAggregator.Clear();

                    tkn.ThrowIfCancellationRequested();
                }

                byte[] waveformBytes = new byte[waveformArray.Length];
                float factor = 31f / Math.Max(Math.Abs(waveformLeftMax), Math.Abs(waveformRightMax));
                for (int ndx = 0; ndx < waveformArray.Length; ndx++)
                    waveformBytes[ndx] = (byte)Math.Abs(Math.Abs(waveformArray[ndx]) * factor);

                song.WaveformData = waveformBytes;
            }
#if (MARKERS)
            span.Leave();
#endif
        }

        void waveStream_Sample(object sender, SampleEventArgs e)
        {
            waveformAggregator.Add(e.Left, e.Right);
        }
    }
}
