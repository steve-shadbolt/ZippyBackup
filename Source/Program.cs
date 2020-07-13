/** ZippyBackup
 *  Copyright (C) 2012-2013 by Wiley Black.  All rights reserved.
 *  See License.txt for licensing rules.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ZippyBackup.User_Interface;
using System.IO;

namespace ZippyBackup
{
    static class Program
    {
        public static SplashForm Splash = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            #if false
            using (FileStream fsi = new FileStream(@"C:\Users\Wiley\Desktop\Zippy.log", FileMode.Open, FileAccess.Read))
            {
                Int64 OriginalLength = fsi.Length;
                Int64 Start = 3 * OriginalLength / 4;
                fsi.Seek(Start, SeekOrigin.Begin);

                using (FileStream fso = new FileStream(@"C:\Users\Wiley\Desktop\Zippy2.log", FileMode.Create, FileAccess.ReadWrite))
                {
                    byte[] buffer = new byte [4096];
                    for (; ; )
                    {
                        int Count = fsi.Read(buffer, 0, buffer.Length);
                        if (Count <= 0) break;
                        fso.Write(buffer, 0, Count);
                    }
                }
            }
            MessageBox.Show("Done");
            #endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                System.Threading.Thread.CurrentThread.Name = "Main Thread";

                // Change to the application's directory so that we can find the DLLs we need.
                string AppFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                Directory.SetCurrentDirectory(AppFolder);

                if (args.Length > 0 && args[0].ToLowerInvariant() == "/tray")
                    Application.Run(new MainForm(true));
                else
                {
                    Splash = new SplashForm();
                    Splash.Show();
                    Application.DoEvents();
                    Application.Run(new MainForm(false));
                }
            }
            catch (Exception ex)
            {
                #if DEBUG
                MessageBox.Show("An application-level error has occurred: \n" + ex.ToString());
                #endif
            }
        }
    }
}
 