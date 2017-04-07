using SharpDX;
using SharpDX.Direct3D11;
//using SharpDX.DXGI;
using SharpHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Colors = System.Drawing.Color;
using Buffer11 = SharpDX.Direct3D11.Buffer;
//using SharpDX.Direct3D;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using System.Timers;
using SD = System.Drawing;
using SWF = System.Windows.Forms;
using System.Threading;
using Microsoft.ConcurrencyVisualizer.Instrumentation;

namespace ConsoleApplication1
{
    public class RC2 : SharpDX.Windows.RenderControl
    {
        //private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel ssHoverSep;
        private ToolStripStatusLabel ssHover;
        private ToolStripStatusLabel ssSelectSep;
        private ToolStripStatusLabel ssSelect;
        //private ToolStripLabel tsppTime;
        //private readonly SynchronizationContext synchronizationContext;
        private IProgress<string> tsppTimeProg, tsLbl4Prog;

        private TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip toolTip1;

        private bool initialized = false;

        //target of rendering
        WindowRenderTarget target;

        private SharpFPS fpsCounter = null;
        private DisplayUpdate dsp = null;

        private int updateds = 0;

        private IntPtr handle;

        private int prevLFT;
        private int prevRIT;
        private bool mouseOver;
        private int mousex, mousey;


        public RC2()
        {
            InitializeComponent();

            handle = this.Handle;
        }

        public void WinInit(IProgress<string> tsppTimeProg, IProgress<string> tsLbl4Prog,
            ToolStripStatusLabel ssHoverSep, ToolStripStatusLabel ssHover,
            ToolStripStatusLabel ssSelectSep, ToolStripStatusLabel ssSelect)
        {
            this.tsppTimeProg = tsppTimeProg;
            this.tsLbl4Prog = tsLbl4Prog;

            this.ssHoverSep = ssHoverSep;
            this.ssHover = ssHover;
            this.ssSelectSep = ssSelectSep;
            this.ssSelect = ssSelect;
            //this.tsppTime = tsppTime;

            this.toolTip1 = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            // toolTip1
            // 
            this.toolTip1.AllowLinksHandling = true;
            this.toolTip1.BaseStylesheet = null;
            this.toolTip1.MaximumSize = new System.Drawing.Size(0, 0);
            this.toolTip1.OwnerDraw = true;
            this.toolTip1.TooltipCssClass = "htmltooltip";

            toolTip1.BaseStylesheet = ".htmltooltip { border:dashed 2px #767676; background-color:aqua; background-gradient:#E4E5F0; padding: 8px; Font: 9pt Tahoma;}"
                + ".hr{border:2px solid red;} .c1{color: red} .b1{background-color: #8dd}";
            toolTip1.BackColor = SD.Color.BlueViolet;
        }

        #region Direct2D

        public void D3DInit()
        {
            RenderTargetProperties renderProp = new RenderTargetProperties()
            {
                DpiX = 0,
                DpiY = 0,
                MinLevel = FeatureLevel.Level_10,
                PixelFormat = new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
                Type = RenderTargetType.Hardware,
                Usage = RenderTargetUsage.None
            };

            //set hwnd target properties (permit to attach Direct2D to window)
            HwndRenderTargetProperties winProp = new HwndRenderTargetProperties()
            {
                Hwnd = handle,
                PixelSize = new Size2(this.ClientSize.Width, this.ClientSize.Height),
                PresentOptions = PresentOptions.Immediately
            };

            //target creation
            target = new WindowRenderTarget(Global.Instance.factory2D1, renderProp, winProp);

            fpsCounter = new SharpFPS();
            fpsCounter.Reset();
            dsp = new DisplayUpdate();

            initialized = true;
        }

        private int prevPosition = -1;
        private volatile bool resizeNeeded = false;

        public void D3DRender(bool updated)
        {
#if (MARKERS)
            var span = Markers.EnterSpan($"{nameof(RC2)} render");
#endif

            Song song = Global.Instance.Song;

            if (resizeNeeded)
            {
                resizeNeeded = false;

                target.Resize(new Size2(Global.Instance.Width, Global.Instance.Height));

                if (song != null)
                {
                    song.DevDepReacquireAll(target);
                    song.ResetPoints();
                }
            }

            target.BeginDraw();
            target.Clear(SharpDX.Color.Black);

            if (initialized)
            {
                if (true) //Global.Instance.Updated)
                {
                    if (song != null)
                    {
                        if (!song.IsDevDepResourcesAcquired)
                        {
                            VizRes.DevDepAcquireAll(target);
                            song.IsDevDepResourcesAcquired = true;
                        }

                        if (song.PlayerMode == PlayerMode.playing)
                        {
                            if (song.Position < song.TrackPx)
                            {
                                song.Position = Math.Min(song.Position + 2, song.TrackPx - 1);
                            }
                            else
                            {
                                song.Position = 0;
                                song.PlayerMode = PlayerMode.stopped;
                            }
                        }

                        var half = target.PixelSize.Width / 2;
                        if (song.Position < half)
                            song.LFT = 0;
                        else if (song.Position > song.TrackPx - half)
                            song.LFT = song.TrackPx - target.PixelSize.Width;
                        else
                            song.LFT = song.Position - half;

                        song.RIT = song.LFT + target.PixelSize.Width;

                        target.Transform = Matrix3x2.Translation(-song.LFT, 0);

                        bool moving = ((song.LFT != prevLFT) || (song.RIT != prevRIT));
                        if (moving)
                        {
                            prevLFT = song.LFT;
                            prevRIT = song.RIT;
                        }

                        DrawData dd = new DrawData()
                        {
                            target = target,
                            Song = song,
                            Height = target.PixelSize.Height - Global.Slider_Height,
                            Width = target.PixelSize.Width,
                            LFT = prevLFT,
                            RIT = prevRIT,
                            Offset = song.Position
                        };

                        if (moving)
                        {
                            foreach (var viz in song.Vizs)
                            {
                                viz.DrawMove(dd);
                            }
                        }

                        foreach (var viz in song.Vizs)
                        {
                            if (viz is Slider || viz is Rule || (viz.StartPoint.X < prevRIT && viz.EndPoint.X > prevLFT))
                                viz.Draw(dd);
                        }

                        foreach (Viz viz in song.Vizs)
                        {
                            if (viz.StartPoint.X < prevRIT && viz.EndPoint.X > prevLFT)
                                if ((viz.IsSelectable) && (song.Selected.Contains(viz)))
                                {
                                    viz.DrawSelect(dd, viz == song.Selected[0]);
                                }
                        }

                        if ((CurrOver != null) && (CurrOver.HasHilites))
                        {
                            CurrOver.DrawHilites(dd);
                        }

                        //if (song.Drag.DragMode == DragMode.Active)
                        //{
                        //    target.DrawRectangle(Pens.AliceBlue, Math.Min(dragBegin.X, dragEnd.X), Math.Min(dragBegin.Y, dragEnd.Y), Math.Abs(dragBegin.X - dragEnd.X), Math.Abs(dragBegin.Y - dragEnd.Y));
                        //}
                    }
                }
            }

            if (mouseOver)
            {
                using (var brush = new SolidColorBrush(target, Color.LightGoldenrodYellow))
                {
                    target.DrawEllipse(new Ellipse(new Vector2(mousex, mousey), 2, 2), brush);
                }
            }

            //end drawing
            target.EndDraw();

#if (MARKERS)
            span.Leave();
#endif

            fpsCounter.Update();

#if (MARKERS)
            span = Markers.EnterSpan($"{nameof(RC2)} progress");
#endif

            string text = $"FPS:{fpsCounter.FPS}:{Global.Instance.RealTime}  Updates:{updateds} W/H:{ClientSize.Width}/{ClientSize.Height} MO:{mouseOver}";
            tsLbl4Prog.Report(text);

            string time = string.Empty;
            if ((song != null) && (prevPosition != (song.Position / (Global.pxpersec / 10))))
            {
                prevPosition = song.Position / (Global.pxpersec / 10);
                time = $"{(prevPosition / 600):00}:{(prevPosition / 10 % 60):00}.{(prevPosition % 10):0}";
                tsppTimeProg.Report(time);
            }

#if (MARKERS)
            span.Leave();
#endif
        }


        public void D3DRelease()
        {

        }

        #endregion

        #region IDisposed

        bool disposed = false;

        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected new virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //target of rendering
                dispose(target);

                //factory for creating 2D elements
                dispose(Global.Instance.factory2D1);
                //this one is for creating DirectWrite Elements
                dispose(Global.Instance.factoryWrite);
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        #endregion

        #region Initialize Component

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // RC3
            // 
            this.Name = "RC3";
            this.Size = new System.Drawing.Size(333, 150);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RC3_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RC3_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RC3_KeyUp);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RC3_MouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RC3_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RC3_MouseDown);
            this.MouseEnter += new System.EventHandler(this.RC3_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.RC3_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RC3_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RC3_MouseUp);
            this.Resize += new System.EventHandler(this.RC3_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private void RC3_Resize(object sender, EventArgs e)
        {
            Global.Instance.Height = this.ClientSize.Height;
            Global.Instance.Width = this.ClientSize.Width;
            resizeNeeded = true;
        }

        #region Mouse

        private void RC3_MouseEnter(object sender, EventArgs e)
        {
            mouseOver = true;
        }

        private void RC3_MouseLeave(object sender, EventArgs e)
        {
            mouseOver = false;
        }

        //private enum DragMode
        //{
        //    Off,
        //    Predrag,
        //    Active
        //}

        private enum HoverMode
        {
            Off,
            Prehover,
            Active
        }

        private Viz prvover = null;

        private HoverMode hoverMode = HoverMode.Off;
//        private Viz hover = null;
        private System.Timers.Timer hovertimer = null;
        private SD.Point hoverpos;

        private bool dragShift;
        private int dragCount = 0;
        private int prioroffset;

        private Viz currOver;
        private Viz CurrOver
        {
            get
            {
                return currOver;
            }
            set
            {
                currOver = value;

                if (currOver == null)
                {
                    ssHoverSep.Visible = ssHover.Visible = false;
                }
                else
                {
                    ssHover.Text = currOver.DebuggerDisplay;
                    ssHoverSep.Visible = ssHover.Visible = true;
                }
            }
        }


        private void RC3_MouseMove(object sender, MouseEventArgs e)
        {
            if (hovertimer == null)
            {
                hovertimer = new System.Timers.Timer(1000);
                hovertimer.AutoReset = false;
                hovertimer.Enabled = false;
                hovertimer.SynchronizingObject = this;
                hovertimer.Elapsed += beginhover;
            }

            var song = Global.Instance.Song;
            if (song != null)
            {
                if (song.DragMode == DragMode.Predrag)
                {
                    if (!((Math.Abs(e.X - hoverpos.X) < Global.HoverDragSensitivity) && (Math.Abs(e.Y - hoverpos.Y) < Global.HoverDragSensitivity)))
                    {
                        song.DragMode = DragMode.Active;

                        if (!dragShift)
                        {
                            song.Selected.Clear();
                        }

                        dragCount = song.Selected.Count;

                        if ((CurrOver != null) && (CurrOver.IsSelectable))
                        {
                            if ((!song.Selected.Contains(CurrOver))
                                && ((song.Selected.Count == 0)
                                    || ((CurrOver.IsMultiSelectable)
                                        //&& (song.Selected.Count > 0)
                                        && (CurrOver.GetType() == song.Selected[0].GetType()))))
                            {
                                song.Selected.Add(CurrOver);
                            }
                        }
                    }
                }
                if (song.DragMode == DragMode.Active)
                {
                    song.DragEnd = e.Location;
                    var dragBox = song.DragBox;
                    foreach (Viz viz in song.Vizs)
                    {
                        if (dragBox.Contains(viz.Rectangle))
                        {
                            if (!song.Selected.Contains(viz) && ((song.Selected.Count == 0) || (viz.GetType() == song.Selected[0].GetType())))
                                song.Selected.Add(viz);
                        }
                        else
                        {
                            if (song.Selected.IndexOf(viz) >= dragCount)
                                song.Selected.Remove(viz);
                        }
                    }
                }




                // toolStripStatusLabel2.Text = e.Location.ToString();

                int x = e.X;
                int y = e.Y;
                //int x1 = x + hScrollBar1.Value;
                int x1 = x + prioroffset;

                if (song != null && song.Vizs != null)
                {
                    var over = Enumerable.Reverse(song.Vizs).Where(s => s.StartPoint.X <= x1 && x1 <= s.EndPoint.X && s.StartPoint.Y <= y && y <= s.EndPoint.Y).FirstOrDefault();

                    if (over != CurrOver)
                    {
                        CurrOver = over;

                        //if (dragging)
                        //{
                        //	if ((dragOver != null) && dragOver.IsSelectable)
                        //	{
                        //		dragOver = CurrOver;
                        //		if ((!CurrOver.IsMultiSelectable) || (CurrOver.GetType() != song.Selected[0].GetType()))
                        //		{
                        //			song.Selected.Clear();
                        //		}
                        //		if (song.Selected.Contains(CurrOver))
                        //			song.Selected.Remove(CurrOver);
                        //		else
                        //			song.Selected.Add(CurrOver);
                        //	}
                        //}
                    }

                    if (over != null)
                    {
                        //over = x1 / 100 * 100;

                        if (prvover != over)
                        {
                            if (prvover != null)
                            {
                                //toolStripStatusLabel1.Text = "Leave:" + prvover.ToString();
                                endhover();
                            }
                        }
                        else
                        {
                            if (hoverMode == HoverMode.Prehover)
                            {
                                if ((Math.Abs(e.X - hoverpos.X) < Global.HoverDragSensitivity) && (Math.Abs(e.Y - hoverpos.Y) < Global.HoverDragSensitivity))
                                    return;

                                endhover();
                            }
                        }

                        //toolStripStatusLabel1.Text = "Over:" + over.ToString() + ":" + hoverstat.ToString();
                        if (hoverMode == HoverMode.Off)
                        {
                            if (over.HasToolTip)
                            {
                                //toolStripStatusLabel3.Text = "pre hover";
                                prvover = over;
                                hoverMode = HoverMode.Prehover;
                                hoverpos = e.Location;
                                hovertimer.Start();
                            }
                        }
                    }
                    else        // not/no longer over
                    {
                        if (prvover == null) return;

                        endhover();

                        //toolStripStatusLabel3.Text = "leave:" + prvover.ToString();
                    }
                }
            }
        }

        private void beginhover(object sender, ElapsedEventArgs e)
        {
            if (prvover != null)
            {
                hovertimer.Enabled = false;
                //toolStripStatusLabel3.Text = "hover";
                hoverMode = HoverMode.Active;
                string msg = "<b>" + prvover.DebuggerDisplay + "</b><hr class='hr'/>this is a <span class='c1'>test</span> of the <b>tooltip</b> window.  <span class='b1'>THis is only a test.</span>";

                //toolTip1.ToolTipTitle = prvover.ToString();
                toolTip1.Show(msg, this, this.PointToClient(Control.MousePosition + new SD.Size(0, 50)));
            }
        }

        private void endhover()
        {
            switch (hoverMode)
            {
                case HoverMode.Prehover:     // pre
                    hovertimer.Stop();
                    //toolStripStatusLabel3.Text = "stop timer";
                    hoverMode = HoverMode.Off;
                    break;

                case HoverMode.Active:
                    toolTip1.Hide(this);
                    //toolStripStatusLabel3.Text = "end hover";
                    hoverMode = HoverMode.Off;
                    break;
            }
            prvover = null;
        }

        private void endselection()
        {
            Global.Instance.Song.Selected.Clear();
        }

        private void RC3_MouseDown(object sender, MouseEventArgs e)
        {
            Song song = Global.Instance.Song;
            if (song == null) return;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                song.DragMode = DragMode.Predrag;
                song.DragEnd = song.DragBegin = e.Location;
                dragShift = ((Control.ModifierKeys & Keys.Shift) != 0);
            }
        }

        private void RC3_MouseUp(object sender, MouseEventArgs e)
        {
            Song song = Global.Instance.Song;
            if (song == null) return;

            if (song.DragMode != DragMode.Off)
            {
                if (song.DragMode == DragMode.Active)
                {
                    song.DragMode = DragMode.Off;
                }
                else
                    song.DragMode = DragMode.Off;
            }
        }

        private void RC3_MouseClick(object sender, MouseEventArgs e)
        {
            Song song = Global.Instance.Song;
            if (song == null) return;

            if (song.DragMode == DragMode.Active) return;

            //toolStripStatusLabel1.Text += "/MC";

            if (CurrOver != null)
            {
                endhover();

                if ((CurrOver.HasContextMenu) && (e.Button == MouseButtons.Right))
                {
                    CntxtMenuMouseLocation = e.Location;

                    var menu = CurrOver.GetContentMenuItems();
                    if (menu == null)
                        MessageBox.Show("ContextMenu not yet implemented.", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    else
                    {
                        var cms = new ContextMenuStrip();
                        cms.Tag = CurrOver;

                        foreach (CntxtMenuItem item in menu)
                        {
                            var cmi = new ToolStripMenuItem(item.Name, null, CntxtMenu_Click, item.Name);
                            cmi.Tag = item;
                            cmi.Enabled = item.Enabled;
                            cms.Items.Add(cmi);
                        }

                        cms.Show((Control)sender, e.Location);
                    }

                }

                if (e.Button == MouseButtons.Left)
                {
                    Global.Instance.Song.Select(CurrOver, (Control.ModifierKeys & Keys.Shift) == 0);
                }
            }
        }

        #endregion

        #region Keyboard

        protected override bool ProcessCmdKey(ref SWF.Message msg, Keys keyData)
        {
            Song song = Global.Instance.Song;
            if (song != null)
            {
                ////////////toolStripStatusLabel1.Text = keyData.ToString("X");
                if (keyData == Keys.Escape)
                {
                    song.Selected.Clear();
                    //Recalculate(true);
                    return true;
                }

                if (song.ProcessCmdKey(ref msg, keyData))
                {
                    //ssSelectedDD.Text = song.ssSelectedDD;
                    //Recalculate(true);
                    //pictureBox1.Invalidate();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private SD.Point CntxtMenuMouseLocation;

        private void CntxtMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = (ToolStripMenuItem)sender;
            CntxtMenuItem cmi = (CntxtMenuItem)tsmi.Tag;
            Viz viz = (Viz)tsmi.Owner.Tag;

            var does = cmi.Handler(viz, CntxtMenuMouseLocation);
            if (does != null)
            {
                if (does.Any(c => c.cmd == Cmd.Insert))
                    Global.Instance.Song.Selected.Clear();
                var undoes = new List<VizCmd>();
                foreach (VizCmd cmd in does)
                {
                    if (cmd != null)
                    {
                        VizCmd chain;
                        if (cmd.viz != null)
                        {
                            if ((chain = cmd.viz.XeqCmd(cmd)) != null)
                                undoes.Add(chain);
                        }
                        else if (cmd.obj is Dictionary<string, object>)
                        {
                            Dictionary<string, object> dict = cmd.obj as Dictionary<string, object>;
                            object obj;
                            if ((dict != null) && (dict.TryGetValue("type", out obj)) && (obj is Type))
                            {
                                Type typ = obj as Type;
                                if (typ == typeof(Lyric))
                                {
                                    Lyric lyr = Global.Instance.Song.CreateLyric((string)dict["text"], (TimeMark)dict["tm"]);
                                    cmd.cmd = Cmd.Delete;
                                    cmd.viz = lyr;
                                    undoes.Add(cmd);
                                }
                            }
                        }
                    }
                }
                if (undoes.Count > 0)
                {
                    Global.Instance.Song.Remember(undoes);
                    //Recalculate(true);
                }
            }
        }


        private void RC3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Song song = Global.Instance.Song;
            if (song == null) return;

            if ((CurrOver != null) && (CurrOver.HasProperties))
            {
                endselection();
                endhover();
                prvover = null;

                CurrOver.ShowDialog(Global.Instance.Song);

                //  show propr dialog
                //var dlg = CurrOver.PropertyWindow();
                ////dlg.FormBorderStyle = FormBorderStyle.None;
                //dlg.MaximizeBox = false;
                //dlg.MinimizeBox = false;
                //dlg.StartPosition = FormStartPosition.CenterScreen;

                //dlg.ShowDialog();
            }
        }

        private void RC3_KeyDown(object sender, KeyEventArgs e)
        {
            var song = Global.Instance.Song;
            if (song == null) return;
            var msg = new SWF.Message();
            song.ProcessCmdKey(ref msg, e.KeyData);
        }

        private void RC3_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void RC3_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private List<Keys> keys = new List<Keys>(10);
        private void showkeys()
        {
            //ssSpring.Text = keys.Select(k => k.ToString()).Aggregate((a, k) => a + ":" + k);
        }

        private void dispose(IDisposable obj)
        {
            if (obj != null) obj.Dispose();
        }

        #endregion

    }
}
