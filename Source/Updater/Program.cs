/** ZippyBackup
 *  Copyright (C) 2012-2013 by Wiley Black.  All rights reserved.
 *  See License.txt for licensing rules.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Reflection;
using System.Threading;
using Ionic.Zip;

namespace Updater
{
    static class Program
    {
        public static string AltExecutable = "ZipBackup.exe";
        public static string MainExecutable = "ZippyBackup.exe";
        public static string UpdatePackageName = "ZippyBackup Update Package.zip";
        public static int Timeout = 30000 /*ms*/;

        public static bool IsRunning(string ProcessName1, string ProcessName2)
        {
            Process[] procs = Process.GetProcesses();
            foreach (Process proc in procs)
                if (proc.ProcessName.ToLower().Contains(ProcessName1.ToLower())
                 || proc.ProcessName.ToLower().Contains(ProcessName2.ToLower())) return true;
            return false;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string UpdateLogPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Update.log";
            using (StreamWriter log = new StreamWriter(UpdateLogPath))
            {
                log.WriteLine(DateTime.Now + ": Updater.exe launched."); log.Flush();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                for (; ; )
                {
                    try
                    {
#                       if false
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Updater.exe");
                        if (args != null)
                        {
                            foreach (string ss in args) sb.Append(" " + ss);
                        }
                        MessageBox.Show(sb.ToString());
#                       endif

                        // Pause a moment so that the calling process can finish cleaning up...
                        long StartTick = Environment.TickCount;
                        while (IsRunning(MainExecutable, AltExecutable))
                        {
                            Thread.Sleep(100);

                            if (Environment.TickCount - StartTick > Timeout)
                                throw new Exception("Unable to complete update!  The primary process has not exited.");
                        }

                        // Locate the folder where we will be updating ZippyBackup.  This is usually passed on the command-line
                        // as an argument, but failing that we may be able to guess.
                        string ZippyBackupDirectory;
                        if (args != null && args.Length > 0) ZippyBackupDirectory = args[0];
                        else ZippyBackupDirectory = "C:\\Program Files (x86)\\ZipBackup";
                        if (!Directory.Exists(ZippyBackupDirectory))
                            ZippyBackupDirectory = "C:\\Program Files (x86)\\ZipBackup";
                        if (!Directory.Exists(ZippyBackupDirectory))
                            ZippyBackupDirectory = "C:\\Program Files\\ZipBackup";
                        if (!Directory.Exists(ZippyBackupDirectory))
                            ZippyBackupDirectory = "C:\\Program Files (x86)\\ZippyBackup";
                        if (!Directory.Exists(ZippyBackupDirectory))
                            ZippyBackupDirectory = "C:\\Program Files\\ZippyBackup";
                        if (!Directory.Exists(ZippyBackupDirectory))
                        {
                            MessageBox.Show("Unable to locate the ZippyBackup Program Files directory!  You may need to uninstall "
                                + "ZippyBackup and install the newest version.", "Error");
                            return;
                        }

                        // Locate the update package we need...
                        string TemporaryFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        string UpdatePackagePath = TemporaryFolder + "\\" + UpdatePackageName;
                        if (!File.Exists(UpdatePackagePath))
                        {
                            MessageBox.Show("Unable to complete update!  Expected update package to be downloaded and stored as '" + UpdatePackagePath + "' prior to launching the Updater!", "Update Error");
                            return;
                        }

                        // MessageBox.Show("TemporaryFolder: " + TemporaryFolder + "\n" + "UpdatePackagePath: " + UpdatePackagePath + "\n");

                        // To perform the actual update, we extract the contents of the zipfile, overwriting everything we find -
                        // except this executable and the DotNetZip library that it depends on.
                        using (ZipFile zip = ZipFile.Read(UpdatePackagePath))
                        {
                            foreach (ZipEntry ze in zip)
                            {
                                if (ze.FileName.ToLower().Contains("updater.exe")
                                 || ze.FileName.ToLower().Contains("ionic.zip.dll")) continue;
                                try
                                {
                                    log.WriteLine(DateTime.Now + ": Extracting file '" + ze.FileName + "' to '" + ZippyBackupDirectory + "'."); log.Flush();
                                    ze.Extract(ZippyBackupDirectory, ExtractExistingFileAction.OverwriteSilently);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("While extracting file '" + ze.FileName + "': " + ex.Message, ex);
                                }
                            }
                        }

                        string MainExecutablePath = ZippyBackupDirectory + "\\" + MainExecutable;
                        if (!File.Exists(MainExecutablePath))
                            throw new Exception("After update, unable to locate main executable.");

                        log.WriteLine(DateTime.Now + ": Update completed successfully."); log.Flush();
                        MessageBox.Show("ZippyBackup has been successfully updated!", "New Version");

                        // Relaunch the new ZippyBackup executable.
                        Process RestartProcess = new Process();
                        RestartProcess.StartInfo = new ProcessStartInfo(MainExecutablePath);
                        RestartProcess.StartInfo.UseShellExecute = false;
                        RestartProcess.Start();
                        return;
                    }
                    catch (Exception ex)
                    {
                        DialogResult dr = MessageBox.Show("The following error occurred while attempting to update: " + ex.Message, "Update Error", MessageBoxButtons.RetryCancel);
                        if (dr == DialogResult.Cancel) return;
                    }
                }
            }
        }
    }
}
