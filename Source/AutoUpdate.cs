/** ZippyBackup
 *  Copyright (C) 2012-2013 by Wiley Black.  All rights reserved.
 *  See License.txt for licensing rules.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;
using Ionic.Zip;
using System.Net;
using System.Diagnostics;

namespace ZippyBackup
{
    public class AutoUpdate
    {
        public static string UpdateVersionUrl = "http://fp.optics.arizona.edu/asl/People/Wiley%20Black/ZippyBackup.Version.xml";

        public static string UpdatePackageName = "ZippyBackup Update Package.zip";
        public static string UpdatePackageUrl = "http://fp.optics.arizona.edu/asl/People/Wiley%20Black/" + UpdatePackageName;

        public static void ShowDebug()
        {
#           if DEBUG
            DateTime FilestampUtc = File.GetLastWriteTimeUtc(Assembly.GetExecutingAssembly().Location);
            // Go one minute into the future because the debug exe and release exe may not be quite the same.
            FilestampUtc += new TimeSpan(0, 1, 0);
            System.Diagnostics.Debug.Write("ZippyBackup.Version.xml time stamp should be:\n");
            System.Diagnostics.Debug.Write("\t" + FilestampUtc.ToString("o") + "\n\n");
#           endif
        }

        /// <summary>
        /// CheckForNewerVersion() determines if a new version is available on the web.  If one is found, the user is
        /// asked whether they would like to download and install it.  If a new version is being installed, true is returned
        /// and the calling code should exit the application.  The updater executable will have already been launched.
        /// If false is returned, then no new version is available, the user did not wish to install it, or an error
        /// occurred.  Until the user is prompted to install a new version, no errors are displayed if SilentOnError is
        /// true.
        /// </summary>
        /// <param name="Silent"></param>
        /// <returns></returns>
        public static bool CheckForNewerVersion(bool Silent)
        {  
            DateTime ExecutingVersionDate;
            DateTime CurrentVersionDate;
            string UpdateMessage;
            try
            {
                string ExeLocation = Assembly.GetExecutingAssembly().Location;
                ExecutingVersionDate = File.GetLastWriteTimeUtc(ExeLocation);

                WebRequest request = WebRequest.Create(UpdateVersionUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();                
                XmlDocument Xml = new XmlDocument();
                Xml.Load(response.GetResponseStream());
                XmlNodeList AutoUpdateInfoXmls = Xml.GetElementsByTagName("AutoUpdateInfo");
                if (AutoUpdateInfoXmls.Count != 1) 
                    throw new FormatException("Xml AutoUpdateInfo format invalid.  Multiple or missing AutoUpdateInfo nodes.");
                XmlElement AutoUpdateInfoXml = AutoUpdateInfoXmls[0] as XmlElement;
                if (AutoUpdateInfoXml == null)
                    throw new FormatException("Xml AutoUpdateInfo format invalid.");
                if (AutoUpdateInfoXml.Attributes["CurrentVersion"] == null)
                    throw new FormatException("Xml AutoUpdateInfo format did not include CurrentVersion attribute.");
                string CurrentVersion = AutoUpdateInfoXml.Attributes["CurrentVersion"].Value;
                CurrentVersionDate = DateTime.Parse(CurrentVersion, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
                UpdateMessage = "A new version of ZippyBackup is available!  Would you like to download and install it now?";
                if (AutoUpdateInfoXml.Attributes["Message"] != null)
                    UpdateMessage = AutoUpdateInfoXml.Attributes["Message"].Value;                
            }
            catch (Exception ex) 
            {
                if (Silent) return false;
                MessageBox.Show("Unable to check for software updates: " + ex.Message);
                return false;
            }

            if (CurrentVersionDate > ExecutingVersionDate)
            {
                if (MessageBox.Show(UpdateMessage, "New Version", MessageBoxButtons.YesNo) != DialogResult.Yes) return false;
                return DownloadAndInstall();
            }
            else if (!Silent) MessageBox.Show("No software updates are available."
                    /**
                                        + "\n\n"
                                        + "Current Installed Version: " + ExecutingVersionDate.ToLocalTime().ToString() + "\n"
                                        + "Latest Available Version: " + CurrentVersionDate.ToLocalTime().ToString());**/
                , "Update Status");

            return false;
        }

        private static bool DownloadAndInstall()
        {  
            for (; ; )
            {
                try
                {
#if false
                    string TemporaryDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#else
                    string TemporaryDirectory = Path.GetTempPath() + "\\ZippyBackup_Update";
                    DirectoryInfo diTemp = new DirectoryInfo(TemporaryDirectory);
                    if (diTemp.Exists)                    
                        foreach (FileInfo fi in diTemp.GetFiles()) fi.Delete();                    
                    Directory.CreateDirectory(TemporaryDirectory);
                    
                    string UpdatePackagePath = TemporaryDirectory + "\\" + UpdatePackageName;
                    if (!DownloadNewVersion(UpdatePackagePath)) return false;
                    using (ZipFile zip = ZipFile.Read(UpdatePackagePath))
                    {
                        ZipEntry ze = zip["Updater.exe"];
                        ze.Extract(TemporaryDirectory, ExtractExistingFileAction.OverwriteSilently);
                        ze = zip["Ionic.Zip.dll"];
                        ze.Extract(TemporaryDirectory, ExtractExistingFileAction.OverwriteSilently);
                    }
#endif
                    string IonicPath = TemporaryDirectory + "\\Ionic.Zip.dll";
                    if (!File.Exists(IonicPath)) throw new Exception("Package did not contain a valid Ionic.Zip.dll or it was not found after extraction.");
                    string UpdaterPath = TemporaryDirectory + "\\Updater.exe";
                    if (!File.Exists(UpdaterPath)) throw new Exception("Package did not contain a valid Updater.exe or it was not found after extraction.");
                                        
                    Process UpdateProcess = new Process();
                    string ExecutableDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    UpdateProcess.StartInfo = new ProcessStartInfo(UpdaterPath, "\"" + ExecutableDirectory + "\"");
                    UpdateProcess.StartInfo.UseShellExecute = true;
                    UpdateProcess.StartInfo.CreateNoWindow = true;
                    if (System.Environment.OSVersion.Version.Major >= 6)
                    {
                        UpdateProcess.StartInfo.Verb = "runas";
                    }
                    UpdateProcess.Start();
                    return true;
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show("An error occurred while installing the latest version: " + ex.Message, "Error", MessageBoxButtons.RetryCancel)
                        == DialogResult.Cancel)
                        return false;
                }
            }
        }

        private static bool DownloadNewVersion(string ToUpdatePackagePath)
        {
            for (; ; )
            {
                try
                {
                    WebRequest request = WebRequest.Create(UpdatePackageUrl);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    using (Stream source = response.GetResponseStream())
                    using (FileStream dest = new FileStream(ToUpdatePackagePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        byte[] Block = new byte[4096];
                        for (; ; )
                        {
                            int nBytes = source.Read(Block, 0, Block.Length);
                            if (nBytes == 0) break;
                            dest.Write(Block, 0, nBytes);
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show("An error occurred while downloading the latest version: " + ex.Message, "Error", MessageBoxButtons.RetryCancel)
                        == DialogResult.Cancel)
                        return false;
                }
            }
        }
    }
}
