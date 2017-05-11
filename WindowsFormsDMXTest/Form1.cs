using System;
using System.Windows.Forms;
using System.IO.Ports;

namespace WindowsFormsDMXTest
{
    public partial class Form1 : Form
    {
        private MultimediaTimer mmTimer = null;

        public Form1()
        {
            InitializeComponent();

            serialPort1.WriteBufferSize = 10;
            var names = SerialPort.GetPortNames();
            if (names.Length == 0)
                throw new IndexOutOfRangeException("serial ports");

            var port = names[0];
            this.Text = port;
            serialPort1.PortName = port;
            serialPort1.Open();
            //timer1.Tick += timerTick;
            mmTimer = new MultimediaTimer(1);
            mmTimer.Elapsed += mmTimerTick;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            label2.Text = trackBar2.Value.ToString();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            label3.Text = trackBar3.Value.ToString();
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            label4.Text = trackBar4.Value.ToString();
        }

        private int step = 0;
        private int loop = 0;
        private void nextstep()
        {
            if (mmTimer == null) return;

            switch (step)
            {
                case 0:
                    var rem = serialPort1.BytesToWrite;
                    if (rem > 0)
                    {

                    }

                    serialPort1.BreakState = true;
                    //timer1.Interval = 1;
                    //timer1.Start();
                    mmTimer.Interval = 1;
                    mmTimer.Start();

                    loop++;
                    label5.Invoke((MethodInvoker)(() => label5.Text = loop.ToString()));

                    step++;
                    break;

                case 1:
                    serialPort1.BreakState = false;
                    //timer1.Interval = 1;
                    //timer1.Start();
                    mmTimer.Interval = 1;
                    mmTimer.Start();

                    step++;
                    break;

                case 2:
                    //byte[] msg = new byte[] { 0, (byte)trackBar1.Value, (byte)trackBar2.Value, (byte)trackBar3.Value, (byte)trackBar4.Value };
                    //serialPort1.Write(msg, 0, msg.Length);

                    //serialPort1.BreakState = false;
                    byte[] msg = null;

                    this.Invoke((MethodInvoker)(() => { msg = sendit(); }));
                    serialPort1.Write(msg, 0, msg.Length);

                    serialPort1.BreakState = false;

                    //timer1.Interval = 1;
                    //timer1.Start();
                    mmTimer.Interval = 1;
                    mmTimer.Start();

                    step = 0;
                    break;
            }
        }

        private byte[] sendit()
        {
            return new byte[] { 0, (byte)trackBar1.Value, (byte)trackBar2.Value, (byte)trackBar3.Value, (byte)trackBar4.Value };
        }

        //private void timerTick(object source, EventArgs e)
        //{
        //    timer1.Stop();
        //    nextstep();
        //}

        private void mmTimerTick(object source, MultimediaElapsedEventArgs e)
        {
            mmTimer.Stop();
            nextstep();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            nextstep();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var tmp = mmTimer;
            if (tmp != null)
            {
                mmTimer = null;
                tmp.Dispose();
            }
        }
    }
}
