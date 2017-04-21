using FTD2XX_NET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    public class USBCntrl : Cntrl
    {
        public List<GECEStrand> strands = new List<GECEStrand>();

        public string Serial { get; set; }

        // Create new instance of the FTDI device class
        private FTDI myFtdiDevice;

        private byte avl = 0x73;
        private byte maxavl = 0;
        private byte nxtseq = 0;

        private BlockingCollection<USBMsg> bufrPool = new BlockingCollection<USBMsg>(150);
        private BlockingCollection<USBMsg> pendMsgs = new BlockingCollection<USBMsg>(50);
        private ConcurrentQueue<USBMsg> sentMsgs = new ConcurrentQueue<USBMsg>();

        private CancellationTokenSource cts = null;
        private CancellationToken tkn;
        private Task rxTsk = null;
        private Task txTsk = null;

        //private FileStream rdrfs = null;
        //private BinaryReader rdrbr = null;
        //private CancellationTokenSource rdrcts = null;
        //private CancellationToken rdrtkn;
        //private Task rdrtsk = null;
        //private int rdrState = 0;

        //private FileStream wtrfs = null;
        //private BinaryWriter wtrbw = null;
        //private int wtrState = 0;

        private int state = -1;
        private static log4net.ILog logger = log4net.LogManager.GetLogger(nameof(USBCntrl));

        public USBCntrl(XElement el)
        {
            Name = (string)el.Attribute("nm");
            Serial = (string)el.Attribute("serial");
            strands.AddRange(from strnd in el.Elements("Strand") select new GECEStrand(strnd, this));

            if (!Global.Instance.Settings.BypassUSBControllers)
            {
                myFtdiDevice = new FTDI();

                do
                {
                    // Open first device by serial number
                    FTDI.FT_STATUS ftStatus = myFtdiDevice.OpenBySerialNumber(Serial);
                    if (ftStatus != FTDI.FT_STATUS.FT_OK)
                    {
                        // Wait for a key press
                        logger.Error("Failed to open device:" + Name + " (error " + ftStatus.ToString() + ")");
                        break;
                    }

                    ftStatus = myFtdiDevice.Purge(FTDI.FT_PURGE.FT_PURGE_RX + FTDI.FT_PURGE.FT_PURGE_TX);
                    if (ftStatus != FTDI.FT_STATUS.FT_OK)
                    {
                        // Wait for a key press
                        logger.Error("Failed to purge device:" + Name + " (error " + ftStatus.ToString() + ")");
                        break;
                    }

                    ftStatus = myFtdiDevice.InTransferSize(64);
                    if (ftStatus != FTDI.FT_STATUS.FT_OK)
                    {
                        // Wait for a key press
                        logger.Error("Failed to set InTransferSize device:" + Name + " (error " + ftStatus.ToString() + ")");
                        break;
                    }

                    Console.WriteLine("Set to set timeouts");
                    // Set read timeout to 5 seconds, write timeout to infinite
                    ftStatus = myFtdiDevice.SetTimeouts(5000, 0);
                    if (ftStatus != FTDI.FT_STATUS.FT_OK)
                    {
                        // Wait for a key press
                        logger.Error("Failed to set timeouts:" + Name + " (error " + ftStatus.ToString() + ")");
                        break;
                    }

                    Console.WriteLine("clear the data");
                    UInt32 numBytesAvailable = 0;
                    ftStatus = myFtdiDevice.GetRxBytesAvailable(ref numBytesAvailable);
                    if (ftStatus != FTDI.FT_STATUS.FT_OK)
                    {
                        // Wait for a key press
                        logger.Error("Failed to get number of bytes available to read:" + Name + " (error " + ftStatus.ToString() + ")");
                        break;
                    }

                    if (numBytesAvailable > 0)
                    {
                        byte[] readData = new byte[numBytesAvailable];
                        UInt32 numBytesRead = 0;

                        ftStatus = myFtdiDevice.Read(readData, numBytesAvailable, ref numBytesRead);
                        if (ftStatus != FTDI.FT_STATUS.FT_OK)
                        {
                            // Wait for a key press
                            logger.Error("Failed to read data:" + Name + " (error " + ftStatus.ToString() + ")");
                            break;
                        }
                        for (int i = 0; i < numBytesRead; i++)
                        {
                            Console.Write(readData[i].ToString("x") + " ");
                        }
                    }

                    byte[] dataToWrite =
                    {
                // command    sequence      length      chksum
                (byte)0xff, (byte)0xff, (byte)0xff, (byte)0xff,     //Resync
                (byte)0xff, (byte)0xff, (byte)0xff, (byte)0xff,     //
                (byte)0xff, (byte)0xff, (byte)0xff, (byte)0xff,     //
                (byte)0xff, (byte)0xff, (byte)0xff, (byte)0xff,     //
                (byte)0x84, (byte)0x00, (byte)0x01, (byte)0xff,     //Stop
                (byte)0x81, (byte)0x00, (byte)0x02, (byte)0xff,     //Set time
                (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00,     //  <the time>
                (byte)0x85, (byte)0x00, (byte)0x01, (byte)0xff,     //ReSeq
                (byte)0x82, (byte)0x00, (byte)0x01, (byte)0xff};    //Run

                    uint numBytesWritten = 0;
                    ftStatus = myFtdiDevice.Write(dataToWrite, dataToWrite.Length, ref numBytesWritten);


                    cts = new CancellationTokenSource();
                    tkn = cts.Token;
                    rxTsk = Task.Factory.StartNew(() => ShowRxData(), tkn);
                    txTsk = Task.Factory.StartNew(() => SendTxData(), tkn);

                    state = 0;

                } while (false);
            }

            for (int ndx = 0; ndx < bufrPool.BoundedCapacity; ndx++)
                bufrPool.Add(new USBMsg());
        }

        ~USBCntrl()
        {
            Close();
        }


        public override void Close()
        {
            if (cts != null)
            {
                cts.Cancel();
                try
                {
                    txTsk.Wait();
                }
                catch (Exception)
                {
                }
                try
                {
                    rxTsk.Wait();
                }
                catch (Exception)
                {
                }
                cts = null;
            }

            if (myFtdiDevice != null)
                myFtdiDevice.Close();
        }


        public override void Send(int tim)
        {
            //if (state < 0)
            //    throw new InvalidOperationException("Open failed: " + Name);

            USBMsg msg = null;
            foreach (GECEStrand strand in strands)
            {
                if (msg == null)
                {
                    msg = bufrPool.Take();

                    msg.seq = 0x00;
                    msg.chksum = 0xff;
                    msg.time = tim;
                    msg.size = 8;
                }

                if (strand.gather(msg, tim))
                {
                    msg.port = strand.Port;
                    if (state < 0)
                    {
                        string hex = "Send: " + Name + ": ";
                        int siz = msg.size;
                        for (int ptr = 0; ptr < siz; ptr++)
                            hex += " " + msg.msg[ptr].ToString("x2");
                        logger.Info(hex);
                    }
                    else
                    {
                        pendMsgs.Add(msg);
                        msg = null;
                    }
                }
            }
            if (msg != null) bufrPool.Add(msg);
        }


        private void SendTxData()
        {
            logger.Info(">SendTx Ready:" + Name);

            FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OTHER_ERROR;
            USBMsg msg = null;
            UInt32 numBytesWritten = 0;

            while (!cts.IsCancellationRequested)
            {
                if ((pendMsgs.Count > 0) && ((avl - sentMsgs.Count) > 10))
                {
                    pendMsgs.TryTake(out msg);
                    msg.seq = ++nxtseq;
                    sentMsgs.Enqueue(msg);

                    string hex = "Send: " + Name + ": ";
                    int siz = msg.size;
                    for (int ptr = 0; ptr < siz; ptr++)
                        hex += " " + msg.msg[ptr].ToString("x2");
                    logger.Info(hex);

                    ftStatus = myFtdiDevice.Write(msg.msg, msg.size, ref numBytesWritten);
                    if (ftStatus != FTDI.FT_STATUS.FT_OK)
                    {
                        logger.Error("<failure: " + Name + ":" + ftStatus.ToString());
                    }
                    else
                    {
                        logger.Info(string.Format("< {0:x2}", msg.seq));
                    }
                }
                else
                    Thread.Sleep(5);
            }
            logger.Info(">SendTx Closed:" + Name);
        }


        private void ShowRxData()
        {
            logger.Info(">ShowRx Ready: " + Name);

            FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OTHER_ERROR;
            UInt32 numBytesAvailable = 0;
            byte[] resp = new byte[4];
            byte cod = 0;
            byte seq = 0;
            byte cnt = 0;
            byte chk = 0;
            USBMsg msg = null;
            UInt32 numBytesRead = 0;

            try
            {
                ManualResetEventSlim hdl = new ManualResetEventSlim(false);

                while (!cts.IsCancellationRequested)
                {
                    ftStatus = myFtdiDevice.GetRxBytesAvailable(ref numBytesAvailable);
                    if (ftStatus != FTDI.FT_STATUS.FT_OK)
                    {
                        // Wait for a key press
                        logger.Error("Failed to get number of bytes available to read:" + Name + " (error " + ftStatus.ToString() + ")");
                        return;
                    }

                    if (numBytesAvailable > 3)
                    {

                        ftStatus = myFtdiDevice.Read(resp, numBytesAvailable, ref numBytesRead);
                        if (ftStatus != FTDI.FT_STATUS.FT_OK)
                        {
                            // Wait for a key press
                            logger.Error("Failed to read data:" + Name + " (error " + ftStatus.ToString() + ")");
                        }

                        cod = resp[0];
                        seq = resp[1];
                        cnt = resp[2];
                        chk = resp[3];
                        logger.Info(string.Format("Recv: {4}: {0:x2} {1:x2} {2:x2} {3:x2}", cod, seq, cnt, chk, Name));

                        switch (cod)
                        {
                            case 0x80:
                                if (sentMsgs.TryPeek(out msg))
                                    if (msg.seq == seq)
                                    {
                                        if (sentMsgs.TryDequeue(out msg))
                                            bufrPool.Add(msg);
                                    }
                                avl = cnt;
                                if (avl > maxavl) maxavl = avl;
                                break;

                            case 0x90:
                                avl = cnt;
                                if (avl > maxavl) maxavl = avl;
                                break;

                            default:
                                logger.Error(string.Format(">Invalid resp: {4}: {0:x2} {1:x2} {2:x2} {3:x2}", cod, seq, cnt, chk, Name));
                                break;
                        }
                    }
                    else
                    {
                        //using (EventWaitHandle hdl = new EventWaitHandle(false, EventResetMode.ManualReset))
                        //{
                        //    myFtdiDevice.SetEventNotification(0, hdl);
                        //    hdl.WaitOne(500);
                        //}
                        hdl.Reset();
                        //myFtdiDevice.SetEventNotification(FTD2XX_NET.FTDI.FT_EVENTS.FT_EVENT_RXCHAR, hdl.WaitHandle);
                        hdl.Wait(-1, tkn);
                        WaitHandle.WaitAny(new WaitHandle[] { hdl.WaitHandle, tkn.WaitHandle }, -1);
                    }
                }
            }
            finally
            {
            }
        }

        //public void Flush()
        //{
        //    if (state < 0)
        //        return;

        //    USBMsg msg;
        //    while (pendMsgs.TryTake(out msg))
        //        bufrPool.Add(msg);
        //}

        //public override void WriteToFile(string flnm, Snapshot snap)
        //{
        //    USBMsg msg = new USBMsg();
        //    msg.msg[1] = (byte)(0);
        //    msg.msg[2] = (byte)(0);
        //    msg.msg[3] = (byte)(0xff);

        //    using (FileStream fs = new FileStream(flnm + "." + Serial, FileMode.Create))
        //    using (BinaryWriter bw = new BinaryWriter(fs))
        //    {
        //        for (int timndx = 0; timndx < snap.bufr.GetUpperBound(1); timndx++)
        //        {
        //            msg.time = timndx * snap.step;

        //            foreach (Strand strand in strands)
        //            {
        //                if (strand.gather(snap, msg, timndx))
        //                {
        //                    msg.port = strand.Port;
        //                    bw.Write(msg.msg, 0, msg.size);
        //                }
        //            }
        //        }
        //        bw.Close();
        //    }
        //}

        //public override void PrepareToRecord(string flnm)
        //{
        //    if (rdrState != 0)
        //        throw new InvalidOperationException("Reader is busy");
        //    if (wtrState != 0)
        //        throw new InvalidOperationException("Writer is already busy");

        //    wtrfs = new FileStream(flnm + "." + Serial, FileMode.Create);
        //    wtrbw = new BinaryWriter(wtrfs);
        //    wtrState = 1;
        //}

        //public override void Record(int tim)
        //{
        //    if (wtrState != 1)
        //        throw new InvalidOperationException("Nor prepared");

        //    USBMsg msg = new USBMsg();
        //    msg.msg[1] = 0x00;
        //    msg.msg[3] = 0xff;
        //    msg.time = tim;

        //    foreach (Strand strand in strands)
        //    {
        //        if (strand.gather(msg, tim))
        //        {
        //            msg.port = strand.Port;
        //            wtrbw.Write(msg.msg, 0, msg.size);
        //        }
        //    }
        //}

        //public override void PrepareToPlay(string flnm)
        //{
        //    if (state < 0) return;

        //    if (wtrState != 0)
        //        throw new InvalidOperationException("Writer busy: " + wtrState.ToString());

        //    if (rdrState != 0)
        //        throw new InvalidOperationException("Wrong state for Prepare: " + rdrState.ToString());

        //    rdrfs = new FileStream(flnm + "." + Serial, FileMode.Open);
        //    rdrbr = new BinaryReader(rdrfs);
        //    rdrState = 2;
        //}

        //public override void Seek(int tim)
        //{
        //    if (state < 0) return;

        //    if (rdrState != 2)
        //        throw new InvalidOperationException("Wrong state for Seek: " + rdrState.ToString());

        //    rdrfs.Seek(0, SeekOrigin.Begin);

        //    USBMsg msg = new USBMsg();
        //    long siz = 0;

        //    do
        //    {
        //        rdrfs.Seek(siz, SeekOrigin.Current);

        //        if (rdrbr.Read(msg.msg, 0, 8) == 0)
        //            throw new ArgumentOutOfRangeException("EOF");

        //        siz = msg.size - 8;
        //    } while (msg.time < tim);

        //    rdrfs.Seek(-8, SeekOrigin.Current);

        //    rdrState = 2;
        //}


        //public override void Play()
        //{
        //    if (state < 0) return;

        //    if (rdrState != 2)
        //        throw new InvalidOperationException("Wrong state for Play: " + rdrState.ToString());

        //    CancellationTokenSource rdrcts = new CancellationTokenSource();
        //    rdrtkn = cts.Token;
        //    rdrtsk = Task.Factory.StartNew(() => play(tkn), tkn);
        //}

        //private void play(CancellationToken tkn)
        //{
        //    while (!tkn.IsCancellationRequested)
        //    {
        //        USBMsg msg = bufrPool.Take();
        //        if (rdrbr.Read(msg.msg, 0, 8) == 0) break;
        //        int siz = msg.size - 8;
        //        if (rdrbr.Read(msg.msg, 8, siz) != siz) break;
        //        pendMsgs.Add(msg);
        //    }
        //    rdrState = 3;
        //}

        //public override void Stop()
        //{
        //    if (state < 0) return;

        //    if (rdrState == 2)
        //    {
        //        rdrcts.Cancel();
        //        rdrtsk.Wait(500);
        //        Flush();
        //    }
        //    rdrfs.Seek(0, SeekOrigin.Begin);
        //    rdrState = 1;

        //    rdrtsk = null;
        //    rdrcts = null;
        //}

        //public override void Finis()
        //{
        //    if (state < 0) return;

        //    if (rdrState == 2)
        //        Stop();

        //    Flush();

        //    if (rdrbr != null)
        //    {
        //        rdrbr.Close();
        //        rdrbr.Dispose();
        //    }
        //    if (rdrfs != null)
        //    {
        //        rdrfs.Close();
        //        rdrfs.Dispose();
        //    }

        //    rdrState = 0;

        //    if (wtrbw != null)
        //    {
        //        wtrbw.Close();
        //        wtrbw.Dispose();
        //    }
        //    if (wtrfs != null)
        //    {
        //        wtrfs.Close();
        //        wtrfs.Dispose();
        //    }

        //    wtrState = 0;
        //}

        //public override void gather(int time)
        //{
        //    Console.WriteLine();
        //    Console.WriteLine("Time:" + time.ToString());

        //    strands.ForEach(s => s.gather());

        //    Console.WriteLine(".");
        //}

    }

    public class USBMsg
    {
        public const int MaxSize = 63 * 4 + 8;

        public byte[] msg = new byte[MaxSize];

        public int time
        {
            get
            {
                return (((((msg[7] << 8) | msg[6]) << 8) | msg[5]) << 8) | msg[4];
            }
            set
            {
                int ms = value % 1000;
                int s = value / 1000;
                msg[4] = (byte)(ms);
                msg[5] = (byte)(ms >> 8);
                msg[6] = (byte)(s);
                msg[7] = (byte)(s >> 8);

            }
        }

        public int port
        {
            get
            {
                return msg[0] & 0x0f;
            }
            set
            {
                msg[0] = (byte)(0x90 | (value & 0x0f));
            }
        }

        public int seq
        {
            get
            {
                return msg[1];
            }
            set
            {
                msg[1] = (byte)value;
            }
        }

        public int size
        {
            get
            {
                return msg[2] << 2;
            }
            set
            {
                msg[2] = (byte)(value >> 2);
            }
        }

        public byte chksum
        {
            get
            {
                return msg[3];
            }
            set
            {
                msg[3] = value;
            }
        }

        public void bld(int ptr, RGBLit lit, Clr clr)
        {
            msg[ptr + 0] = (byte)((clr.G & 0xf0) | (clr.B >> 4));
            msg[ptr + 1] = (byte)(clr.R >> 4);
            msg[ptr + 2] = (byte)(0xcc);
            msg[ptr + 3] = (byte)lit.Index;
        }
    }
}
