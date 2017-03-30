using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace ConsoleApplication1
{
    public class Tree
    {
        private Timer timer = null;
        private bool pending = false;
        private bool waiting = false;
        private SerialPort sp = null;

        public void Run()
        {
            var sps = SerialPort.GetPortNames();
            if (sps.Length == 0)
            {
                Console.WriteLine("No com ports.");
                return;
            }
            using (sp = new SerialPort()
            {
                PortName = sps[0],
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReceivedBytesThreshold = 1
            })
            {
                sp.DataReceived += sp_DataReceived;
                sp.Open();

                timer = new Timer(timerCallBack);
                send("q");

                do
                {
                    string cmd = Console.ReadLine();
                    if (cmd == "x") break;
                    send(cmd);
                } while (true);

                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
                sp.Close();
            }
        }

        private List<string> initCmds = new List<string>(100);
        private int initNdx = 0;

        private void send(string cmd)
        {
            pending = true;
            timer.Change(1000, 1000);
            sp.Write(cmd + "\r");
        }

        private void stopTimer()
        {
            pending = waiting = false;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        private void timerCallBack(object o)
        {
            sp.Write("q\r");
        }

        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            while (sp.BytesToRead > 0)
            {
                Char[] chr = { ' ' };
                sp.Read(chr, 0, 1);
                char rsp = chr[0];
                Console.WriteLine(rsp);
                switch (rsp)
                {
                    case 'R':
                        stopTimer();
                        initCmds.Clear();
                        using (StreamReader rdr = new StreamReader("..\\..\\tree.txt"))
                            initCmds.Add(rdr.ReadLine());
                        initNdx = 0;

                        send("r");
                        break;

                    case 'O':
                        pending = false;
                        if (initCmds.Count > 0)
                        {
                            send(initCmds[initNdx]);
                            initNdx++;
                            if (initNdx == initCmds.Count)
                                initCmds.Clear();
                        }
                        break;

                    case 'I':
                        waiting = false;
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
