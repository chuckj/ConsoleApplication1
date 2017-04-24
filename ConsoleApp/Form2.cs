using SharpHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SD = System.Drawing;
using SWF = System.Windows.Forms;
using SLD = System.Linq.Dynamic;

namespace ConsoleApplication1
{
    public class Form2 : Form
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(nameof(Form2));
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel ssLabel1;
        private ToolStripStatusLabel ssLabel2;
        private ToolStripStatusLabel ssLabel3;
        private ToolStripStatusLabel ssLabel4;
        private ToolStripStatusLabel ssSpring;
        private ToolStripStatusLabel ssHoverSep;
        private ToolStripStatusLabel ssHover;
        private ToolStripStatusLabel ssSelectedSep;
        private ToolStripStatusLabel ssSelected;

        //private static System.Windows.Forms.Timer myTimer;
        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip toolTip1;
        private ToolStripContainer toolStripContainer1;
        private SplitContainer splitContainer1;
        private RC1 rc1;
        private RC2 rc2;
        private ToolStrip tspPlayer;
        private ToolStripButton tsppPlay;
        private ToolStripButton tsppStop;
        private ToolStripButton tsppPause;
        private ToolStripButton tsppReplay;
        private ToolStripLabel tsppTime;
        private ToolStrip tspFile;
        private ToolStripButton tspfNew;
        private ToolStripButton tspfOpen;
        private ToolStripButton tspFSave;
        private MenuStrip tspMenu;
        private ToolStripMenuItem tsmiFile;
        private ToolStripMenuItem tsmiNew;
        private ToolStripMenuItem tsmiOpen;
        private ToolStripMenuItem tsmiSave;
        private ToolStripMenuItem tsmiClose;
        private ToolStripSeparator tsmiSplit1;
        private ToolStripMenuItem tsmiRecent;
        private ToolStripSeparator tsmiSplit2;
        private ToolStripMenuItem tsmiExit;
        private ToolStripMenuItem tsmiGenerate;
        private SplitContainer splitContainer2;
        private ToolStripButton tsppBackToStart;
        private ToolStripMenuItem runToolStripMenuItem;
        private ToolStripProgressBar ssProgressBar1;
        private ToolStripStatusLabel ssClickOn;
        private ToolStrip tspSearch;
        private ToolStripLabel tspsLabel1;
        private ToolStripTextBox tspsRegex;
        private ToolStripButton tspsSearch;
        private ToolStripLabel tspsLabel2;
        private ToolStripTextBox tspsSelector;
        private SWF.Timer progbartimer;

        public Form2()
        {
            logger.Info($"FormThreadId:{Thread.CurrentThread.ManagedThreadId} OS:{AppDomain.GetCurrentThreadId() }");

            InitializeComponent();

            var settings = Global.Instance.Settings;
            Global.Instance.PropertyChanged += globalPropertyChanged;

            var windowTop = settings.WindowTop;
            var windowLeft = settings.WindowLeft;
            var windowWidth = settings.WindowWidth;
            var windowHeight = settings.WindowHeight;

            if ((windowWidth > 0) && (windowHeight > 0))
            {

                SD.Rectangle window = new SD.Rectangle(windowLeft, windowTop, windowWidth, windowHeight);

                var screen = Screen.AllScreens.FirstOrDefault(s => s.WorkingArea.Contains(window));
                if (screen == null)
                {
                    screen = Screen.AllScreens.First(s => s.Primary);
                    windowLeft = screen.WorkingArea.Left;
                    windowTop = screen.WorkingArea.Top;
                }

                User32Support.MoveWindow(this.Handle, windowLeft, windowTop, windowWidth, windowHeight, false);
            }

            this.statusStrip1.Renderer = new CustomStripRenderer();
            this.tspMenu.Renderer = new CustomMenuStripRenderer();

            this.Show();

            List<Control> ctls = new List<Control>(4);
            while (toolStripContainer1.TopToolStripPanel.Controls.Count > 0)
            {
                var ctl = toolStripContainer1.TopToolStripPanel.Controls[0];
                ctls.Add(ctl);
                toolStripContainer1.TopToolStripPanel.Controls.Remove(ctl);
            }

            if (settings.tspMenu.X != 0)
                tspMenu.Location = settings.tspMenu;
            if (settings.tspFile.X != 0)
                tspFile.Location = settings.tspFile;
            if (settings.tspPlayer.X != 0)
                tspPlayer.Location = settings.tspPlayer;
            if (settings.tspSearch.X != 0)
                tspSearch.Location = settings.tspSearch;

            foreach (var ctl in ctls.OrderBy(c => c.Location.X).ThenBy(c => c.Location.Y))
            {
                toolStripContainer1.TopToolStripPanel.Controls.Add(ctl);
            }

            this.splitContainer1.SplitterDistance = settings.SplitterDistance;
            //this.splitContainer2.SplitterDistance = 40;


            this.tspMenu.LocationChanged += this.tspMenu_LocationChanged;
            this.tspFile.LocationChanged += this.tspFile_LocationChanged;
            this.tspPlayer.LocationChanged += this.tspPlayer_LocationChanged;
            this.tspSearch.LocationChanged += this.tspSearch_LocationChanged;
            this.splitContainer1.SplitterMoved += this.splitContainer1_SplitterMoved;


            updatePlayer();

            toolTip1.BaseStylesheet = ".htmltooltip { border:dashed 2px #767676; background-color:aqua; background-gradient:#E4E5F0; padding: 8px; Font: 9pt Tahoma;}"
                    + ".hr{border:2px solid red;} .c1{color: red} .b1{background-color: #8dd}";
            toolTip1.BackColor = SD.Color.BlueViolet;

            if (!SharpDevice.IsDirectX11Supported())
            {
                logger.Info("DirectX11 Not Supported");
                Console.ReadLine();
                Application.Exit();
            }

            Global.Instance.Height = rc2.ClientSize.Height;
            Global.Instance.Width = rc2.ClientSize.Width;

            //  r1c init

            var rc1Prog = new Progress<Tuple<string, string, string>>((parms) =>
            {
                ssLabel1.Text = parms.Item1;
                ssLabel2.Text = parms.Item2;
                ssLabel3.Text = parms.Item3;
            });
            rc1.WinInit(rc1Prog);

            var rc1ClickOn = new Progress<string>((parms) =>
            {
                ssClickOn.Text = parms;
            });
            rc1.ClickOnInit(rc1ClickOn);
            //  rc3 init

            var tsppTimeProg = new Progress<string>((t) => tsppTime.Text = t);
            var ssLbl4Prog = new Progress<string>((t) => ssLabel4.Text = t);
            rc2.WinInit(tsppTimeProg, ssLbl4Prog, ssHoverSep, ssHover, ssSelectedSep, ssSelected);

            var d3dThread = new D3DThread();
            d3dThread.Form = this;

            var cts = new CancellationTokenSource();
            d3dThread.Token = cts.Token;
            d3dThread.Form = this;

            var d3dTask = Task.Factory.StartNew(() => d3dThread.Runner(), d3dThread.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

            rc1.Select();
            progbartimer = new SWF.Timer();
            progbartimer.Tick += hideprogbar;

            //  RunTime init
            var progress = new Progress<int>((parms) =>
            {
                if (parms == -1)
                {
                    ssProgressBar1.Visible = false;
                }
                else
                {
                    ssProgressBar1.Visible = true;
                    ssProgressBar1.Value = parms;
                }
            });
            RunTime.Progress(progress);
        }

        private void globalPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Song":
                    updatePlayer();
                    break;
            }
        }

        private void hideprogbar(Object myObject, EventArgs myEventArgs)
        {
            progbartimer.Stop();
            ssProgressBar1.Visible = false;
        }

        #region Initialize Component

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.ssLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.ssSpring = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssClickOn = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssHoverSep = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssHover = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssSelectedSep = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssSelected = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTip1 = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tspPlayer = new System.Windows.Forms.ToolStrip();
            this.tsppPlay = new System.Windows.Forms.ToolStripButton();
            this.tsppPause = new System.Windows.Forms.ToolStripButton();
            this.tsppStop = new System.Windows.Forms.ToolStripButton();
            this.tsppBackToStart = new System.Windows.Forms.ToolStripButton();
            this.tsppReplay = new System.Windows.Forms.ToolStripButton();
            this.tsppTime = new System.Windows.Forms.ToolStripLabel();
            this.tspMenu = new System.Windows.Forms.MenuStrip();
            this.tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNew = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSave = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiClose = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSplit1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiRecent = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSplit2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiExit = new System.Windows.Forms.ToolStripMenuItem();
            this.runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiGenerate = new System.Windows.Forms.ToolStripMenuItem();
            this.tspFile = new System.Windows.Forms.ToolStrip();
            this.tspfNew = new System.Windows.Forms.ToolStripButton();
            this.tspfOpen = new System.Windows.Forms.ToolStripButton();
            this.tspFSave = new System.Windows.Forms.ToolStripButton();
            this.tspSearch = new System.Windows.Forms.ToolStrip();
            this.tspsLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tspsRegex = new System.Windows.Forms.ToolStripTextBox();
            this.tspsSearch = new System.Windows.Forms.ToolStripButton();
            this.tspsSelector = new System.Windows.Forms.ToolStripTextBox();
            this.tspsLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.rc1 = new ConsoleApplication1.RC1();
            this.rc2 = new ConsoleApplication1.RC2();
            this.statusStrip1.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tspPlayer.SuspendLayout();
            this.tspMenu.SuspendLayout();
            this.tspFile.SuspendLayout();
            this.tspSearch.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ssLabel1,
            this.ssLabel2,
            this.ssLabel3,
            this.ssLabel4,
            this.ssProgressBar1,
            this.ssSpring,
            this.ssClickOn,
            this.ssHoverSep,
            this.ssHover,
            this.ssSelectedSep,
            this.ssSelected});
            this.statusStrip1.Location = new System.Drawing.Point(0, 686);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1087, 24);
            this.statusStrip1.TabIndex = 5;
            // 
            // ssLabel1
            // 
            this.ssLabel1.BackColor = System.Drawing.Color.Black;
            this.ssLabel1.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.ssLabel1.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.ssLabel1.ForeColor = System.Drawing.Color.White;
            this.ssLabel1.Name = "ssLabel1";
            this.ssLabel1.Size = new System.Drawing.Size(4, 19);
            // 
            // ssLabel2
            // 
            this.ssLabel2.BackColor = System.Drawing.Color.Black;
            this.ssLabel2.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.ssLabel2.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.ssLabel2.ForeColor = System.Drawing.Color.White;
            this.ssLabel2.Name = "ssLabel2";
            this.ssLabel2.Size = new System.Drawing.Size(4, 19);
            // 
            // ssLabel3
            // 
            this.ssLabel3.BackColor = System.Drawing.Color.Black;
            this.ssLabel3.ForeColor = System.Drawing.Color.White;
            this.ssLabel3.Name = "ssLabel3";
            this.ssLabel3.Size = new System.Drawing.Size(0, 19);
            // 
            // ssLabel4
            // 
            this.ssLabel4.BackColor = System.Drawing.Color.Black;
            this.ssLabel4.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.ssLabel4.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.ssLabel4.ForeColor = System.Drawing.Color.White;
            this.ssLabel4.Name = "ssLabel4";
            this.ssLabel4.Size = new System.Drawing.Size(4, 19);
            // 
            // ssProgressBar1
            // 
            this.ssProgressBar1.AutoSize = false;
            this.ssProgressBar1.BackColor = System.Drawing.Color.Black;
            this.ssProgressBar1.ForeColor = System.Drawing.Color.ForestGreen;
            this.ssProgressBar1.Name = "ssProgressBar1";
            this.ssProgressBar1.Size = new System.Drawing.Size(110, 18);
            this.ssProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // ssSpring
            // 
            this.ssSpring.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.ssSpring.Name = "ssSpring";
            this.ssSpring.Size = new System.Drawing.Size(912, 19);
            this.ssSpring.Spring = true;
            // 
            // ssClickOn
            // 
            this.ssClickOn.ForeColor = System.Drawing.Color.White;
            this.ssClickOn.Name = "ssClickOn";
            this.ssClickOn.Size = new System.Drawing.Size(36, 19);
            this.ssClickOn.Text = "Click:";
            // 
            // ssHoverSep
            // 
            this.ssHoverSep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.ssHoverSep.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.ssHoverSep.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.ssHoverSep.ForeColor = System.Drawing.Color.White;
            this.ssHoverSep.Name = "ssHoverSep";
            this.ssHoverSep.Size = new System.Drawing.Size(46, 19);
            this.ssHoverSep.Text = "Hover:";
            this.ssHoverSep.Visible = false;
            // 
            // ssHover
            // 
            this.ssHover.ForeColor = System.Drawing.Color.White;
            this.ssHover.Name = "ssHover";
            this.ssHover.Size = new System.Drawing.Size(0, 19);
            this.ssHover.Visible = false;
            // 
            // ssSelectedSep
            // 
            this.ssSelectedSep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.ssSelectedSep.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.ssSelectedSep.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.ssSelectedSep.ForeColor = System.Drawing.Color.White;
            this.ssSelectedSep.Name = "ssSelectedSep";
            this.ssSelectedSep.Size = new System.Drawing.Size(58, 19);
            this.ssSelectedSep.Text = "Selected:";
            this.ssSelectedSep.Visible = false;
            // 
            // ssSelected
            // 
            this.ssSelected.ForeColor = System.Drawing.Color.White;
            this.ssSelected.Name = "ssSelected";
            this.ssSelected.Size = new System.Drawing.Size(0, 19);
            this.ssSelected.Visible = false;
            // 
            // toolTip1
            // 
            this.toolTip1.AllowLinksHandling = true;
            this.toolTip1.BaseStylesheet = null;
            this.toolTip1.MaximumSize = new System.Drawing.Size(0, 0);
            this.toolTip1.OwnerDraw = true;
            this.toolTip1.TooltipCssClass = "htmltooltip";
            // 
            // toolStripContainer1
            // 
            this.toolStripContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.splitContainer1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1087, 663);
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(1087, 688);
            this.toolStripContainer1.TabIndex = 6;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.tspMenu);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.tspPlayer);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.tspFile);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.tspSearch);
            this.toolStripContainer1.TopToolStripPanel.ForeColor = System.Drawing.Color.White;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BackColor = System.Drawing.Color.Purple;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.ForeColor = System.Drawing.Color.White;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.Black;
            this.splitContainer1.Panel1.Controls.Add(this.rc1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.Black;
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1087, 661);
            this.splitContainer1.SplitterDistance = 536;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 7;
            this.splitContainer1.SizeChanged += new System.EventHandler(this.splitContainer1_SizeChanged);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BackColor = System.Drawing.Color.Purple;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.ForeColor = System.Drawing.Color.White;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.BackColor = System.Drawing.Color.Black;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.Color.Black;
            this.splitContainer2.Panel2.Controls.Add(this.rc2);
            this.splitContainer2.Size = new System.Drawing.Size(1087, 123);
            this.splitContainer2.SplitterDistance = 100;
            this.splitContainer2.SplitterWidth = 2;
            this.splitContainer2.TabIndex = 0;
            // 
            // tspPlayer
            // 
            this.tspPlayer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.tspPlayer.Dock = System.Windows.Forms.DockStyle.None;
            this.tspPlayer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsppPlay,
            this.tsppPause,
            this.tsppStop,
            this.tsppBackToStart,
            this.tsppReplay,
            this.tsppTime});
            this.tspPlayer.Location = new System.Drawing.Point(142, 0);
            this.tspPlayer.Name = "tspPlayer";
            this.tspPlayer.Size = new System.Drawing.Size(205, 25);
            this.tspPlayer.TabIndex = 0;
            // 
            // tsppPlay
            // 
            this.tsppPlay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.tsppPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsppPlay.ForeColor = System.Drawing.Color.White;
            this.tsppPlay.Image = global::ConsoleApplication1.Properties.Resources.Play;
            this.tsppPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsppPlay.Name = "tsppPlay";
            this.tsppPlay.Size = new System.Drawing.Size(23, 22);
            this.tsppPlay.Click += new System.EventHandler(this.tsppPlay_Click);
            // 
            // tsppPause
            // 
            this.tsppPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsppPause.Image = global::ConsoleApplication1.Properties.Resources.Pause;
            this.tsppPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsppPause.Name = "tsppPause";
            this.tsppPause.Size = new System.Drawing.Size(23, 22);
            this.tsppPause.Click += new System.EventHandler(this.tsppPause_Click);
            // 
            // tsppStop
            // 
            this.tsppStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsppStop.ForeColor = System.Drawing.Color.White;
            this.tsppStop.Image = global::ConsoleApplication1.Properties.Resources.Stop;
            this.tsppStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsppStop.Name = "tsppStop";
            this.tsppStop.Size = new System.Drawing.Size(23, 22);
            this.tsppStop.Click += new System.EventHandler(this.tsppStop_Click);
            // 
            // tsppBackToStart
            // 
            this.tsppBackToStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsppBackToStart.Image = global::ConsoleApplication1.Properties.Resources.GotoFirstRow_287_32;
            this.tsppBackToStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsppBackToStart.Name = "tsppBackToStart";
            this.tsppBackToStart.Size = new System.Drawing.Size(23, 22);
            this.tsppBackToStart.Click += new System.EventHandler(this.tsppBackToStart_Click);
            // 
            // tsppReplay
            // 
            this.tsppReplay.Checked = true;
            this.tsppReplay.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsppReplay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsppReplay.Image = global::ConsoleApplication1.Properties.Resources.Repeat;
            this.tsppReplay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsppReplay.Name = "tsppReplay";
            this.tsppReplay.Size = new System.Drawing.Size(23, 22);
            this.tsppReplay.Click += new System.EventHandler(this.tsppReplay_Click);
            // 
            // tsppTime
            // 
            this.tsppTime.Font = new System.Drawing.Font("OCR A Extended", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsppTime.ForeColor = System.Drawing.Color.Red;
            this.tsppTime.Name = "tsppTime";
            this.tsppTime.Size = new System.Drawing.Size(78, 22);
            this.tsppTime.Text = "00:00.0";
            // 
            // tspMenu
            // 
            this.tspMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.tspMenu.Dock = System.Windows.Forms.DockStyle.None;
            this.tspMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiFile,
            this.tsmiGenerate});
            this.tspMenu.Location = new System.Drawing.Point(3, 0);
            this.tspMenu.Name = "tspMenu";
            this.tspMenu.Size = new System.Drawing.Size(139, 24);
            this.tspMenu.Stretch = false;
            this.tspMenu.TabIndex = 3;
            // 
            // tsmiFile
            // 
            this.tsmiFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.tsmiFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiNew,
            this.tsmiOpen,
            this.tsmiSave,
            this.tsmiClose,
            this.tsmiSplit1,
            this.tsmiRecent,
            this.tsmiSplit2,
            this.tsmiExit,
            this.runToolStripMenuItem});
            this.tsmiFile.ForeColor = System.Drawing.Color.White;
            this.tsmiFile.Name = "tsmiFile";
            this.tsmiFile.Size = new System.Drawing.Size(65, 20);
            this.tsmiFile.Text = "&Program";
            this.tsmiFile.DropDownOpening += new System.EventHandler(this.tsmiFile_DropDownOpening);
            // 
            // tsmiNew
            // 
            this.tsmiNew.Name = "tsmiNew";
            this.tsmiNew.Size = new System.Drawing.Size(110, 22);
            this.tsmiNew.Text = "&New";
            this.tsmiNew.Click += new System.EventHandler(this.tsmiNew_Click);
            // 
            // tsmiOpen
            // 
            this.tsmiOpen.Name = "tsmiOpen";
            this.tsmiOpen.Size = new System.Drawing.Size(110, 22);
            this.tsmiOpen.Text = "&Open";
            this.tsmiOpen.Click += new System.EventHandler(this.tsmiOpen_Click);
            // 
            // tsmiSave
            // 
            this.tsmiSave.Name = "tsmiSave";
            this.tsmiSave.Size = new System.Drawing.Size(110, 22);
            this.tsmiSave.Text = "&Save";
            this.tsmiSave.Click += new System.EventHandler(this.tsmiSave_Click);
            // 
            // tsmiClose
            // 
            this.tsmiClose.Name = "tsmiClose";
            this.tsmiClose.Size = new System.Drawing.Size(110, 22);
            this.tsmiClose.Text = "&Close";
            this.tsmiClose.Click += new System.EventHandler(this.tsmiClose_Click);
            // 
            // tsmiSplit1
            // 
            this.tsmiSplit1.Name = "tsmiSplit1";
            this.tsmiSplit1.Size = new System.Drawing.Size(107, 6);
            // 
            // tsmiRecent
            // 
            this.tsmiRecent.Name = "tsmiRecent";
            this.tsmiRecent.Size = new System.Drawing.Size(110, 22);
            this.tsmiRecent.Text = "Recent";
            this.tsmiRecent.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.tsmiRecent_DropDownItemClicked);
            // 
            // tsmiSplit2
            // 
            this.tsmiSplit2.Name = "tsmiSplit2";
            this.tsmiSplit2.Size = new System.Drawing.Size(107, 6);
            // 
            // tsmiExit
            // 
            this.tsmiExit.Name = "tsmiExit";
            this.tsmiExit.Size = new System.Drawing.Size(110, 22);
            this.tsmiExit.Text = "E&xit";
            this.tsmiExit.Click += new System.EventHandler(this.tsmiExit_Click);
            // 
            // runToolStripMenuItem
            // 
            this.runToolStripMenuItem.Name = "runToolStripMenuItem";
            this.runToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.runToolStripMenuItem.Text = "Run";
            this.runToolStripMenuItem.Click += new System.EventHandler(this.tmsiRun_Click);
            // 
            // tsmiGenerate
            // 
            this.tsmiGenerate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.tsmiGenerate.ForeColor = System.Drawing.Color.White;
            this.tsmiGenerate.Name = "tsmiGenerate";
            this.tsmiGenerate.Size = new System.Drawing.Size(66, 20);
            this.tsmiGenerate.Text = "Generate";
            this.tsmiGenerate.Click += new System.EventHandler(this.tsmiGenerate_Click);
            // 
            // tspFile
            // 
            this.tspFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.tspFile.Dock = System.Windows.Forms.DockStyle.None;
            this.tspFile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tspfNew,
            this.tspfOpen,
            this.tspFSave});
            this.tspFile.Location = new System.Drawing.Point(347, 0);
            this.tspFile.Name = "tspFile";
            this.tspFile.Size = new System.Drawing.Size(81, 25);
            this.tspFile.TabIndex = 1;
            // 
            // tspfNew
            // 
            this.tspfNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tspfNew.Image = global::ConsoleApplication1.Properties.Resources.NewDocument;
            this.tspfNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tspfNew.Name = "tspfNew";
            this.tspfNew.Size = new System.Drawing.Size(23, 22);
            this.tspfNew.Click += new System.EventHandler(this.tspfNew_Click);
            // 
            // tspfOpen
            // 
            this.tspfOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tspfOpen.Image = global::ConsoleApplication1.Properties.Resources.OpenFile;
            this.tspfOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tspfOpen.Name = "tspfOpen";
            this.tspfOpen.Size = new System.Drawing.Size(23, 22);
            this.tspfOpen.Click += new System.EventHandler(this.tspfOpen_Click);
            // 
            // tspFSave
            // 
            this.tspFSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tspFSave.Image = global::ConsoleApplication1.Properties.Resources.Save;
            this.tspFSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tspFSave.Name = "tspFSave";
            this.tspFSave.Size = new System.Drawing.Size(23, 22);
            this.tspFSave.Click += new System.EventHandler(this.tspFSave_Click);
            // 
            // tspSearch
            // 
            this.tspSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.tspSearch.Dock = System.Windows.Forms.DockStyle.None;
            this.tspSearch.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tspsLabel1,
            this.tspsRegex,
            this.tspsLabel2,
            this.tspsSelector,
            this.tspsSearch});
            this.tspSearch.Location = new System.Drawing.Point(442, 0);
            this.tspSearch.Name = "tspSearch";
            this.tspSearch.Size = new System.Drawing.Size(363, 25);
            this.tspSearch.TabIndex = 4;
            // 
            // tspsLabel1
            // 
            this.tspsLabel1.ActiveLinkColor = System.Drawing.Color.White;
            this.tspsLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tspsLabel1.ForeColor = System.Drawing.Color.White;
            this.tspsLabel1.Name = "tspsLabel1";
            this.tspsLabel1.Size = new System.Drawing.Size(41, 22);
            this.tspsLabel1.Text = "Regex:";
            // 
            // tspsRegex
            // 
            this.tspsRegex.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.tspsRegex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tspsRegex.ForeColor = System.Drawing.Color.White;
            this.tspsRegex.Name = "tspsRegex";
            this.tspsRegex.Size = new System.Drawing.Size(100, 25);
            this.tspsRegex.Text = "hello";
            this.tspsRegex.Leave += new System.EventHandler(this.tspsRegex_Leave);
            this.tspsRegex.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tspsRegex_KeyPress);
            // 
            // tspsSearch
            // 
            this.tspsSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tspsSearch.Image = ((System.Drawing.Image)(resources.GetObject("tspsSearch.Image")));
            this.tspsSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tspsSearch.Name = "tspsSearch";
            this.tspsSearch.Size = new System.Drawing.Size(23, 22);
            this.tspsSearch.Text = "toolStripButton1";
            this.tspsSearch.Click += new System.EventHandler(this.tspfNew_Click);
            // 
            // tspsSelector
            // 
            this.tspsSelector.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.tspsSelector.ForeColor = System.Drawing.Color.White;
            this.tspsSelector.Name = "tspsSelector";
            this.tspsSelector.Size = new System.Drawing.Size(100, 25);
            this.tspsSelector.Leave += new System.EventHandler(this.tspsSelector_Leave);
            this.tspsSelector.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tspsSelector_KeyPress);
            // 
            // tspsLabel2
            // 
            this.tspsLabel2.Name = "tspsLabel2";
            this.tspsLabel2.Size = new System.Drawing.Size(52, 22);
            this.tspsLabel2.Text = "Selector:";
            // 
            // rc1
            // 
            this.rc1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rc1.BackColor = System.Drawing.Color.Black;
            this.rc1.Location = new System.Drawing.Point(0, 0);
            this.rc1.Name = "rc1";
            this.rc1.Size = new System.Drawing.Size(1087, 541);
            this.rc1.TabIndex = 4;
            this.rc1.TabStop = false;
            // 
            // rc2
            // 
            this.rc2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.rc2.BackColor = System.Drawing.Color.Black;
            this.rc2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rc2.Location = new System.Drawing.Point(0, 0);
            this.rc2.Name = "rc2";
            this.rc2.Size = new System.Drawing.Size(985, 123);
            this.rc2.TabIndex = 5;
            this.rc2.TabStop = false;
            // 
            // Form2
            // 
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1087, 710);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form2_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form2_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form2_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form2_KeyUp);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tspPlayer.ResumeLayout(false);
            this.tspPlayer.PerformLayout();
            this.tspMenu.ResumeLayout(false);
            this.tspMenu.PerformLayout();
            this.tspFile.ResumeLayout(false);
            this.tspFile.PerformLayout();
            this.tspSearch.ResumeLayout(false);
            this.tspSearch.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            closing();
        }

        private void closing()
        {
            Application.Exit();
        }

        #region Menu processing

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            exit();
        }

        private void exit()
        {
            var settings = Global.Instance.Settings;
            settings.WindowTop = this.Top;
            settings.WindowLeft = this.Left;
            settings.WindowWidth = this.Width;
            settings.WindowHeight = this.Height;

            Global.Instance.Settings.SaveAppSettings();

            this.Close();
        }

        private void tsmiNew_Click(object sender, EventArgs e)
        {
            if (readyToRelease())
            {
                var song = Global.Instance.Song = Song.New(ssHoverSep, ssHover, ssSelectedSep, ssSelected);
                song.PropertyChanged += songPropertyChanged;
            }
        }

        private void songPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "PlayerMode":
                case "Repeating":
                    updatePlayer();
                    break;

            }
        }

        private void updatePlayer()
        {
            var song = Global.Instance.Song;
            var mode = (song == null) ? PlayerMode.closed : Global.Instance.Song.PlayerMode;
            tsppPlay.Enabled = mode == PlayerMode.stopped || mode == PlayerMode.paused;
            tsppPause.Enabled = mode == PlayerMode.playing;
            tsppPlay.Visible = !(tsppPause.Visible = mode == PlayerMode.playing);
            tsppStop.Enabled = mode == PlayerMode.playing;
            tsppBackToStart.Enabled = mode != PlayerMode.closed;
            tsppReplay.Enabled = mode != PlayerMode.closed;
            tsppReplay.Checked = mode != PlayerMode.closed;
            tsppReplay.CheckState = (mode != PlayerMode.closed && song.Repeating) ? CheckState.Checked : CheckState.Unchecked;
            tsppTime.Enabled = mode != PlayerMode.closed;
            tsppTime.ForeColor = (mode == PlayerMode.playing) ? SD.Color.Green : SD.Color.Red;
        }

        private void tsmiOpen_Click(object sender, EventArgs e)
        {
            openfile();
        }


        private void tsmiSave_Click(object sender, EventArgs e)
        {
            savefile();
        }

        private void savefile()
        {
            var song = Global.Instance.Song;
            if ((song == null) || (!song.IsChanged)) return;

        }

        private void tsmiClose_Click(object sender, EventArgs e)
        {
            if (readyToRelease())
            {
                //close music files
                //realease music

                //Song.Player.Stop();

                Global.Instance.Song = null;
            }

        }

        private void tsmiFile_DropDownOpening(object sender, EventArgs e)
        {
            tsmiSave.Enabled = (Global.Instance.Song != null);
            tsmiClose.Enabled = (Global.Instance.Song != null);

            tsmiRecent.DropDownItems.Clear();
            var files = Global.Instance.Settings.RecentlyUsed;
            if (files.Count == 0)
            {
                tsmiRecent.Enabled = false;
            }
            else
            {
                tsmiRecent.Enabled = true;
                foreach (var filnam in files)
                {
                    var item = new ToolStripMenuItem()
                    {
                        Text = Path.GetFileName(filnam),
                        ToolTipText = filnam
                    };
                    tsmiRecent.DropDownItems.Add(item);
                }
            }
        }

        private void tsmiRecent_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var filnam = e.ClickedItem.ToolTipText;
            logger.Info("tsmiRecent_DropDownItemClicked:" + filnam);

            if (!File.Exists(filnam))
            {
                var answer = MessageBox.Show("That file no longer exists.  Would you like it removed from the list?", "File not found", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer == DialogResult.Yes)
                {
                    Global.Instance.Settings.RemoveMostRecentlyUsed(filnam);
                }
            }
            else if (readyToRelease())
            {
                openNewSong(filnam);
            }
        }

        #endregion

        protected override bool ProcessCmdKey(ref SWF.Message msg, Keys keyData)
        {
            Song song = Global.Instance.Song;

            if (song != null)
            {
                //toolStripStatusLabel1.Text = keyData.ToString("X");
                if (keyData == Keys.Escape)
                {
                    song.Selected.Clear();
                    //Recalculate(true);
                    return true;
                }

                if (song.ProcessCmdKey(ref msg, keyData))
                {
                    ssSelected.Text = song.ssSelectedDD;
                   // Recalculate(true);
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }


        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            keys.Add(e.KeyCode);
            showkeys();
        }

        private void Form2_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void Form2_KeyUp(object sender, KeyEventArgs e)
        {
            keys.Remove(e.KeyCode);
            showkeys();
        }

        private List<Keys> keys = new List<Keys>(10);
        private void showkeys()
        {
            ssSpring.Text = keys.Select(k => k.ToString()).Aggregate((a, k) => a + ":" + k);
        }

        private void tsmiGenerate_Click(object sender, EventArgs e)
        {
            Global.Instance.Song.Generate();
        }

        private void tspMenu_LocationChanged(object sender, EventArgs e)
        {
            Global.Instance.Settings.tspMenu = tspMenu.Location;
        }

        private void tspPlayer_LocationChanged(object sender, EventArgs e)
        {
            Global.Instance.Settings.tspPlayer = tspPlayer.Location;
        }

        private void tspFile_LocationChanged(object sender, EventArgs e)
        {
            Global.Instance.Settings.tspFile = tspFile.Location;
        }
        private void tspSearch_LocationChanged(object sender, EventArgs e)
        {
            Global.Instance.Settings.tspSearch = tspSearch.Location;
        }

        private void tspPlayerEnable(bool enable)
        {
            tsppPlay.Enabled = enable;
            tsppStop.Enabled = enable;
            tsppPause.Enabled = enable;
            tsppBackToStart.Enabled = enable;
            tsppReplay.Enabled = enable;
            tsppTime.Enabled = enable;
        }

        private void openfile()
        {
            if (readyToRelease())
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                openFileDialog1.InitialDirectory = Global.Instance.Settings.GetDefaultDirectory();
                openFileDialog1.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    closeSong();
                    openNewSong(openFileDialog1.FileName);
                }
            }
        }

        private void closeSong()
        {
            Song song = Global.Instance.Song;
            if (song != null)
            {
                Global.Instance.Song = null;
                song.Close();
            }

        }

        private void openNewSong(string filnam)
        {
            Global.Instance.Settings.SetMostRecentlyUsed(filnam);
            var song = Global.Instance.Song = Song.Open(ssHoverSep, ssHover, ssSelectedSep, ssSelected, filnam);
            this.Text = Path.GetFileNameWithoutExtension(filnam);
            song.PropertyChanged += songPropertyChanged;
        }

        private bool readyToRelease()
        {
            var song = Global.Instance.Song;
            if ((song == null) || (!song.IsChanged)) return true;

            var result = MessageBox.Show("Changes have been made and will be lost.  Proceed without saving?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            return (result == DialogResult.Yes);
        }

        private void tspfOpen_Click(object sender, EventArgs e)
        {
            openfile();
        }

        private void tspFSave_Click(object sender, EventArgs e)
        {
            savefile();
        }

        private void tspfNew_Click(object sender, EventArgs e)
        {
            //newfile();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Global.Instance.Settings.SplitterDistance = splitContainer1.SplitterDistance;
        }

        private void splitContainer1_SizeChanged(object sender, EventArgs e)
        {
        }

        private void tsppPlay_Click(object sender, EventArgs e)
        {
            //Global.Instance.Model.Instance.Play();
            setPlayerMode(PlayerMode.playing);
        }

        private void tsppStop_Click(object sender, EventArgs e)
        {
            setPlayerMode(PlayerMode.stopped);
        }

        private void tsppPause_Click(object sender, EventArgs e)
        {
            setPlayerMode(PlayerMode.paused);
        }

        private void tsppReplay_Click(object sender, EventArgs e)
        {
            if (Global.Instance.Song != null)
                Global.Instance.Song.Repeating = !Global.Instance.Song.Repeating;
        }

        private void setPlayerMode(PlayerMode mode)
        {
            if (Global.Instance.Song != null)
                Global.Instance.Song.PlayerMode = mode;
        }

        private void tsppBackToStart_Click(object sender, EventArgs e)
        {
            setPlayerMode(PlayerMode.stopped);
            Global.Instance.Song.Position = 0;
        }

        private class D3DThread
        {
            public CancellationToken Token;
            public Form2 Form;

            public void Runner()
            {
                logger.Info($"D3DThreadId:{Thread.CurrentThread.ManagedThreadId} OS:{AppDomain.GetCurrentThreadId() }");

                Form.rc1.D3DInit();

                Form.rc2.D3DInit();

                try
                {
                    while (!Token.IsCancellationRequested)
                    {
                        Form.rc1.D3DRender(true);
                        Form.rc2.D3DRender(true);
                    }
                    Token.ThrowIfCancellationRequested();
                }
                finally
                {
                    Form.rc1.D3DRelease();
                    Form.rc2.D3DRelease();
                }
            }
        }

        private void tmsiRun_Click(object sender, EventArgs e)
        {
            //var cts = new CancellationTokenSource();
            //var tkn = cts.Token;
            //var runtm = new RunTime();
            //var tsk = Task.Factory.StartNew(() => runtm.Run(tkn, Global.Instance.Song), tkn);
            //tsk.Wait();
            RunTime.RunNow(Global.Instance.Song);
        }

        private void tspsRegex_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Console.WriteLine("keypress:" + e.KeyChar.ToString());
            if (e.KeyChar == '\r')
            {
                trySelect();
            }
            else
            {
                tspsRegex.ForeColor = SD.Color.White;
                tspsSelector.ForeColor = SD.Color.White;
            }
        }

        private void tspsSelector_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                trySelect();
            }
            else
            {
                tspsRegex.ForeColor = SD.Color.White;
                tspsSelector.ForeColor = SD.Color.White;
            }

        }

        private void tspsSelector_Leave(object sender, EventArgs e)
        {
            trySelect();
        }

        private void tspsRegex_Leave(object sender, EventArgs e)
        {
            trySelect();
        }

        private void trySelect()
        {
            if (!String.IsNullOrEmpty(tspsRegex.Text))
            {
                try
                {
                    var regex = new Regex(tspsRegex.Text, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                    var litx = Global.Instance.LitDict.Values.OfType<Lit>()
                        .Where(v => regex.IsMatch(v.Name));

                    if (!String.IsNullOrEmpty(tspsSelector.Text))
                    {
                        var selector = SLD.DynamicExpression.CompileLambda<Lit, bool>(tspsSelector.Text);
                        litx = litx.Where(lit => selector(lit));
                    }

                    Global.Instance.Selected = litx.Select(lit => lit.GlobalIndex).ToList();
                }
                catch (Exception)
                {
                    tspsRegex.ForeColor = SD.Color.Red;
                    tspsSelector.ForeColor = SD.Color.Red;
                    return;
                }
            }
            tspsRegex.ForeColor = SD.Color.White;
            tspsSelector.ForeColor = SD.Color.White;
        }
    }
}
