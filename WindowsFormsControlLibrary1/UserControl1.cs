using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsControlLibrary1
{
    public partial class EditableLabel: UserControl
    {
        private TextBox editableTextBox;

        public EditableLabel()
        {
            InitializeComponent();

            editableTextBox = new TextBox();
            this.groupBox1.Controls.Add(editableTextBox);
            editableTextBox.KeyDown += new KeyEventHandler(editableTextBox_KeyDown);
            editableTextBox.Hide();
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get { return labelDisplay.Text; }
            set { labelDisplay.Text = value; }
        }

        private void labelDisplay_DoubleClick(object sender, EventArgs e)
        {
            editableTextBox.Size = labelDisplay.Size;
            editableTextBox.Location = labelDisplay.Location;
            editableTextBox.Text = labelDisplay.Text;
            labelDisplay.Hide();
            editableTextBox.Show();
            editableTextBox.Focus();
        }

        void editableTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                labelDisplay.Text = editableTextBox.Text;
                editableTextBox.Hide();
                labelDisplay.Show();
            }
        }

        private void labelDisplay_Resize(object sender, EventArgs e)
        {
            this.Size = labelDisplay.Size;
        }
    }
}
