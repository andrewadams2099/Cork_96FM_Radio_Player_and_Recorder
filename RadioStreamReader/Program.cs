using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel.Composition.Hosting;

namespace RadioStreamReader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        //[MTAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mainForm = new MainForm(new MP3StreamingPanel() );
            Application.Run(mainForm);
        }
    }
}