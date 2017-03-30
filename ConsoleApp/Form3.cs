using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleApplication1
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            btnRun.Enabled = false;
            btnRun.BackColor = Color.FromKnownColor(KnownColor.ControlLight);

            Run();

            btnStop.Enabled = true;
            btnStop.BackColor = Color.Red;

            tsStatus.Text = "Running...";
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            btnStop.BackColor = Color.FromKnownColor(KnownColor.ControlLight);

            Stop();

            btnRun.Enabled = true;
            btnRun.BackColor = Color.Lime;
        }

        private CancellationTokenSource TknSrc = null;
        private Task display = null;
        private Task runner = null;
        private Task player = null;

        private async void Run()
        {

            Global.Reallocate();
          
            TknSrc = new CancellationTokenSource();
            Global.Instance.Tkn = TknSrc.Token;

            runner = new Task(() => Step.Runner(), Global.Instance.Tkn);
            runner.Start();

            await runner;

            Global.Instance.readyList.Sort();

            //render form
            var play = new Player();
            player = new Task(() => play.Runner(), Global.Instance.Tkn);
            player.Start();

            await Task.Delay(1);

            var form = new Form2();
            display = new Task(() => form.Runner(), Global.Instance.Tkn);
            display.Start();

            await Task.Delay(1);

        }

        private void Stop()
        {
            tsStatus.Text = "stopping";

            TknSrc.Cancel();

            try
            {
                display.Wait();
            }
            catch (AggregateException e)
            {

            }
            display.Dispose();
            display = null;
            tsStatus.Text = "Display stopped";

            try
            { 
                runner.Wait();
            }
            catch (AggregateException e)
            {

            }
            runner.Dispose();
            runner = null;
            tsStatus.Text = "Display stopped";

            try
            { 
                player.Wait();
            }
            catch (AggregateException e)
            {

            }
            player.Dispose();
            player = null;
            tsStatus.Text = "";

            reset();
        }

        private void reset()
        {

        }
    }
}
