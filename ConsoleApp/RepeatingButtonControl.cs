using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.ComponentModel;

namespace RepeatButtonControl
{
	public class RepeatingButton : Button
	{
		private Timer timerRepeater;
		private IContainer components;

		public RepeatingButton()
			: base()
		{
			InitializeComponent();

			InitialDelay = 400;
			RepeatInterval = 62;
		}

		[DefaultValue(400)]
		public int InitialDelay { set; get; }

		[DefaultValue(62)]
		public int RepeatInterval { set; get; }

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.timerRepeater = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			this.timerRepeater.Tick += new System.EventHandler(this.timerRepeater_Tick);
			this.ResumeLayout(false);
		}

		MouseEventArgs MouseDownArgs = null;
		protected override void OnMouseDown(MouseEventArgs mevent)
		{
			//Save arguments
			MouseDownArgs = mevent;
			timerRepeater.Enabled = false;
			timerRepeater_Tick(null, EventArgs.Empty);
		}

		private void timerRepeater_Tick(object sender, EventArgs e)
		{

			base.InvokeOnClick(this, e);
			if (timerRepeater.Enabled)
				timerRepeater.Interval = RepeatInterval;
			else
				timerRepeater.Interval = InitialDelay;

			timerRepeater.Enabled = true;
		}

		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			base.OnMouseUp(mevent);
			timerRepeater.Enabled = false;
		}
	}
}