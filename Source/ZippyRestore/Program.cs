using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ZippyRestore
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] Args)
        {
Args = new string[] { "C:\\Users\\Wiley\\Desktop\\ZippyBackup Configuration.xml" };
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                ZippyBackup.User_Interface.RestoreForm rf = new ZippyBackup.User_Interface.RestoreForm(Args);
                Application.Run(rf);
            }
            catch (Exception exc) { MessageBox.Show(exc.Message, "ZippyRestore"); }
        }
    }
}
