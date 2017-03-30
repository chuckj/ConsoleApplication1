using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsControlLibrary1;

namespace ConsoleApplication1.Dialogs
{
    public partial class dlgProps : Form
    {
        int cLeft = 1;
        int cTop = 10;

        public dlgProps()
        {
            InitializeComponent();
        }

        //public dlgProps(PropDialogData pdd)
        //{
        //    InitializeComponent();


        //}

        private void button1_Click(object sender, EventArgs e)
        {
            AddNewTextBox();
        }

        public System.Windows.Forms.TextBox AddNewTextBox(string caption = null, string txt = "")
        {
            var lbl = new System.Windows.Forms.Label();
            var self = new System.Windows.Forms.TextBox();
            self.Top = cTop + panel3.AutoScrollPosition.Y;
            lbl.Top = self.Top + 2;
            cTop += 25;
            lbl.Left = 0;
            lbl.AutoSize = false;
            lbl.Width = 50; 
            lbl.Name = "Label " + this.cLeft.ToString();
            lbl.Text = caption;
            self.Left = 55;
            self.Name = "TextBox " + this.cLeft.ToString();
            if (txt == null) txt = self.Name;
            self.Text = txt;
            cLeft = cLeft + 1;
            self.TabIndex = cLeft;
            this.panel3.Controls.Add(lbl);
            this.panel3.Controls.Add(self);
            return self;
        }

        public EditableLabel AddNewEditableLabel()
        {
            var self = new EditableLabel();
            self.AutoSize = true;
            self.Name = "editableLabel" + cLeft.ToString();
            self.Size = new System.Drawing.Size(103, 53);
            self.Text = "editableLabel" + cLeft.ToString();
            self.Left = 0;
            self.Top = cTop + panel3.AutoScrollPosition.Y;
            cTop += 60;
            cLeft++;
            self.TabIndex = cLeft;
            this.panel3.Controls.Add(self);
            return self;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AddNewEditableLabel();
        }
    }
}
