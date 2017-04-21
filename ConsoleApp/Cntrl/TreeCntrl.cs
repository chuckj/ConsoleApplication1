using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    class TreeCntrl : Cntrl
    {
        private class TreeMsg
        {
        }

        private static ConcurrentQueue<TreeMsg> sentMsgs = new ConcurrentQueue<TreeMsg>();

        private CancellationTokenSource cts = null;

        private SerialPort port;
        private Task tsk = null;
        private string portName;
        private int baudRate;
        private Parity parity;
        private int dataBits;
        private StopBits stopbits;

        public TreeCntrl(XElement el)
        {
            Name = (string)el.Attribute("nm");
            portName = (string)el.Attribute("portname");
            string wrk = (string)el.Attribute("baudrate");
            if (!int.TryParse(wrk, out baudRate))
                throw new Exception("Baudrate must be int: " + wrk);
            wrk = (string)el.Attribute("parity");
            if (!Enum.TryParse(wrk, out parity))
                throw new Exception("parity must be Parity: " + wrk);
            wrk = (string)el.Attribute("databits");
            if (!int.TryParse(wrk, out dataBits))
                throw new Exception("dataBits must be int: " + wrk);
            wrk = (string)el.Attribute("stopbits");
            if (!Enum.TryParse(wrk, out stopbits))
                throw new Exception("stopbits must be Stopbits: " + wrk);

            if (portName.Length > 0)
            {
                port = new SerialPort(portName, baudRate, parity, dataBits, stopbits);

                port.Open();
            }
        }

        ~TreeCntrl()
        {
            Close();
        }

        public override void Close()
        {
            if (cts != null)
                cts.Cancel();

            if (tsk != null)
                tsk.Wait();

            if (port != null)
            {
                port.Close();
                port.Dispose();
                port = null;
            }
        }

        public override void Send(int time)
        {

        }

        //public override void WriteToFile(string flnm, Snapshot snap)
        //{
        //    byte[] msg = new byte[66 * 4];
        //    using (FileStream fs = new FileStream(flnm + "." + Serial, FileMode.Create))
        //    using (BinaryWriter bw = new BinaryWriter(fs))
        //    {
        //        for (int timndx = 0; timndx < snap.bufr.GetUpperBound(1); timndx++)
        //        {
        //        }
        //        bw.Close();
        //    }
        //}

        //public override void gather(int time)
        //{
        //    Console.WriteLine();
        //    Console.WriteLine("Time:" + time.ToString());

        //    Console.WriteLine(".");
        //}

        //private void ShowRxData(object obj)
        //{
        //    CancellationToken tkn = (CancellationToken)obj;

        //    do
        //    {
        //        if (tkn.IsCancellationRequested) break;

        //        if (true)
        //        {
        //        }
        //        else
        //        {
        //            using (EventWaitHandle hdl = new EventWaitHandle(false, EventResetMode.ManualReset))
        //            {
        //                myFtdiDevice.SetEventNotification(0, hdl);
        //                hdl.WaitOne(5);
        //            }
        //        }
        //    } while (true);
        //}
    }
}
