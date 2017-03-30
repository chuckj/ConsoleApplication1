﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace ConsoleApplication1
{
    public class RunTime
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(nameof(RunTime));

        public CancellationToken tkn;
        public int CurTime = 0;
        public int NxtTime = 999999999;
        public Dictionary<int, Pgm> PgmDict = new Dictionary<int, Pgm>();

        private int displayOffset = 0;

        private static IProgress<int> progress;

        private FileStream newFile;
        private BinaryWriter accessor;

        private Color[] colors;
        private volatile bool updated = false;

        public Color this[int ndx]
        {
            set
            {
                if (ndx < Global.Instance.dta.Length)
                {
                    value = Global.Instance.dta[ndx].Coerse(value);

                    colors[ndx] = value;
                }
                else
                {
                    colors[ndx] = value;
                }
                updated = true;
            }

            get
            {
                return colors[ndx];
            }
        }

        public bool Updated => updated;

        public Color[] Colors
        {
            get
            {
                updated = false;
                return colors;
            }
        }



        public static void Progress(IProgress<int> _progress)
        {
            progress = _progress;

            //int workerThreads;
            //int portThreads;
            //ThreadPool.GetMinThreads(out workerThreads, out portThreads);
            //ThreadPool.SetMinThreads(10, portThreads);

        }


        public static Task RunNow(Song song)
        {
            var cts = new CancellationTokenSource();
            var tkn = cts.Token;
            var runtm = new RunTime();
            var tsk = new Task(() => runtm.Run(tkn, song), tkn, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
            tsk.Start();
            return tsk;
        }


        public async void Run(CancellationToken tkn, Song song)
        {
            await Task.Delay(1);

            logger.Info("Run");

            CurTime = 0;
            NxtTime = 999999999;

            var filname = string.Empty;
            var fullpath = string.Empty;
            bool newFileOK = false;
            Helper[] steps = null;

            try
            {
                filname = Path.GetRandomFileName();
                fullpath = Path.Combine(Path.GetDirectoryName(song.FileName), filname);

                using (newFile = File.Create(fullpath, 64*1024, FileOptions.RandomAccess))
                using (accessor = new BinaryWriter(newFile))
                {
                    var mapsize = song.DisplayMapSize;

                    //  Clear l'map and map
                    // Make changes to the view.
                    for (long i = 0; i <= mapsize; i += 4)
                    {
                        accessor.Write(0);
                    }

                    var lits = Global.Instance.VuDict["tree"].LitArray.Count + Global.Instance.LitDict.Values.OfType<RGBLit>().Count();

                    colors = new Color[lits];
                    for (int ndx = 0; ndx < lits; ndx++)
                        colors[ndx] = Color.Black;

                    displayOffset = mapsize + 4;

                    steps = song.Vizs.Where(v => v is IRunTime).OrderBy(v => v.StartPoint.X)
                        .Select(v => new Helper() { time = v.StartPoint.X, iter = ((IRunTime)v).Xeq(this).GetEnumerator() }).ToArray();
                    CurTime = steps.Min(v => v.time);

                    while (CurTime != 99999999)
                    {
                        tkn.ThrowIfCancellationRequested();

                        CurTime = (int)Math.Ceiling(CurTime * 30.0 / Global.pxpersec) * Global.pxpersec / 30;
                        //logger.Info($"NewPhase:{CurTime}");

                        //  process all current runners
                        NxtTime = 99999999;
                        foreach (var step in steps)
                        {
                            if (step.time <= CurTime)
                                step.time = step.iter.MoveNext() ? step.iter.Current : 99999999;

                            NxtTime = Math.Min(NxtTime, step.time);
                        }

                        //logger.Info($"InterPhase: {CurTime}:{updated}");

                        progress.Report((int)(CurTime * 100 / song.MusicEndPx));

                        saveUpdates();

                        CurTime = NxtTime;
                    };

                    progress.Report(100);

                    //  Set l'map to indicate we're complete
                    accessor.Seek(0, SeekOrigin.Begin);
                    accessor.Write(mapsize);

                    accessor.Close();

                    newFileOK = true;
                }
            }
            catch (Exception e)
            {
                logger.Debug("Failed", e);
            }
            finally
            {
                if (steps != null)
                    foreach (var step in steps)
                        step.iter.Dispose();
            }

            if (!newFileOK)
            {
                if (!string.IsNullOrEmpty(fullpath))
                    File.Delete(fullpath);
            }
            else
            {
                lock (song.DisplayInfoLock)
                {
                    if (song.DisplayInfo != null)
                    {
                        var oldDispInfo = song.DisplayInfo;
                        if (oldDispInfo != null)
                        {
                            song.DisplayInfo = null;
                            oldDispInfo.Dispose();
                        }
                    }


                    //
                    File.Delete(song.DisplayInfoFileName);

                    // rename
                    File.Move(fullpath, song.DisplayInfoFileName);

                    var dspInfo = new DisplayInfo();
                    dspInfo.FileStream = File.Open(song.DisplayInfoFileName, FileMode.Open, FileAccess.Read);
                    dspInfo.BinaryReader = new BinaryReader(dspInfo.FileStream);
                    var mapsize = dspInfo.BinaryReader.ReadInt32() / 4;
                    dspInfo.Index = new int[mapsize];
                    for (int ndx = 0; ndx < mapsize; ndx++)
                        dspInfo.Index[ndx] = dspInfo.BinaryReader.ReadInt32();

                    song.DisplayInfo = dspInfo;
                }
            }

            logger.Info("RunComplete");

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                progress.Report(-1);
            });
        }

   
        private void saveUpdates()
        {
            if (updated)
            {
                int offset = (CurTime * 30 / Global.pxpersec) * 4 /* l'int */ + 4 /* l'mapsize */;
                accessor.Seek(offset, SeekOrigin.Begin);
                accessor.Write(displayOffset);

                accessor.Seek(displayOffset, SeekOrigin.Begin);
                for (int ndx = 0; ndx < colors.Length; ndx++)
                {
                    var color = colors[ndx];
                    uint clr = (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | color.B);
                    accessor.Write(clr);
                }

                displayOffset += colors.Length * 4;
            }
        }

        public class Helper
        {
            public int time;
            public IEnumerator<int> iter;
        }
    }
}
