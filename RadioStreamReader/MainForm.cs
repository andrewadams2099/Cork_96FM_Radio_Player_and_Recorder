using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Composition;

namespace RadioStreamReader
{
    [Export]
    public partial class MainForm : Form
    {
        [ImportingConstructor]
        public MainForm(MP3StreamingPanel demo)
        {
            InitializeComponent();
            this.Controls.Add(demo);

            this.Text = this.Text + ((System.Runtime.InteropServices.Marshal.SizeOf(IntPtr.Zero) == 8) ? " (x64)" : " (x86)");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            GC.Collect();
            Application.Exit();
            return;
        }
    }
}