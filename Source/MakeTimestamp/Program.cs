/** ZippyBackup
 *  Copyright (C) 2012-2013 by Wiley Black.  All rights reserved.
 *  See License.txt for licensing rules.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;

namespace MakeTimestamp
{
    class Program
    {
        public static string UpdateMessage =
            "ZipBackup is now called ZippyBackup!  A new version is available!  Would you like to download and install it now?"
            /**
            + "\n\nNew Features:"
            + "\n\t- Bugfix for automatic update."
            + "\n\t- New search filenames option available."
            + "\n\t- Empty backups create a note file instead of a zip file."
             */
             ;

        public static string Executable = "ZippyBackup.exe";
        public static string TimeStampFile = "ZippyBackup.Version.xml";
        public static string BuildVersionFile = "BuildVersion.cs";

        // The current executable will be compared to the timestamp to determine if an update is required.  Giving it
        // 30 seconds into the past is a good idea.
        public static TimeSpan Past = new TimeSpan(0, 0, 30);

        static int Main(string[] args)
        {
            try
            {                
                string TargetFolder = null;
                if (args != null && args.Length > 0) TargetFolder = FindTarget(args[0], Executable);
                if (TargetFolder == null) TargetFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (TargetFolder == null)
                {
                    Console.Write("\n\nFatal Error: Unable to locate target executable '" + Executable + "'.\n\n");
                    return -1;
                }

                string TargetExecutable = TargetFolder + "\\" + Executable;
                string TargetTimestamp = TargetFolder + "\\" + TimeStampFile;
                DateTime TimeStampUtc = File.GetLastWriteTimeUtc(TargetExecutable) - Past;

                XmlDocument Doc = new XmlDocument();
                Doc.InsertBefore(Doc.CreateXmlDeclaration("1.0", "UTF-8", null), Doc.DocumentElement);
                XmlElement AutoUpdateInfoXml = Doc.CreateElement("AutoUpdateInfo");
                Doc.AppendChild(AutoUpdateInfoXml);
                AutoUpdateInfoXml.SetAttribute("CurrentVersion", TimeStampUtc.ToString("o"));
                AutoUpdateInfoXml.SetAttribute("Message", UpdateMessage);
                Doc.Save(TargetTimestamp);
                
                string CodeFolder = FindTarget(Path.GetDirectoryName(TargetFolder), BuildVersionFile);
                if (CodeFolder == null) CodeFolder = FindTarget(Path.GetDirectoryName(Path.GetDirectoryName(TargetFolder)), BuildVersionFile);
                if (CodeFolder == null) CodeFolder = FindTarget(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(TargetFolder))), BuildVersionFile);
                if (CodeFolder == null)
                {
                    Console.Write("\n\nFatal Error: Unable to locate build version file '" + BuildVersionFile + "'\n\n");
                    return -2;
                }
                using (StreamWriter sw = new StreamWriter(CodeFolder + "\\" + BuildVersionFile))
                {
                    sw.WriteLine("using System;");
                    sw.WriteLine("");
                    sw.WriteLine("namespace ZippyBackup.User_Interface {");
                    sw.WriteLine("\tpublic class BuildVersion { public static DateTime Version = DateTime.Parse(\"" + TimeStampUtc.ToString("o") + "\"); }");
                    sw.WriteLine("}");
                }

                Console.Write("MakeTimestamp: Success.\n");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Write("\n\nFatal Error: " + ex.Message + "\n\n" + ex.ToString() + "\n");
                return -2;
            }
        }

        static string FindTarget(string Folder, string TargetName)
        {
            Folder = Folder.Replace("\"", "");
            TargetName = TargetName.Replace("\"", "");
            //Console.WriteLine("Folder: " + Folder);
            //Console.WriteLine("TargetName: " + TargetName);
            DirectoryInfo di = new DirectoryInfo(Folder);
            foreach (FileInfo fi in di.GetFiles())
                if (fi.Name.ToLower() == TargetName.ToLower()) return Folder;
            foreach (DirectoryInfo diSub in di.GetDirectories())
            {
                string res = FindTarget(diSub.FullName, TargetName);
                if (res != null) return res;
            }
            return null;
        }
    }
}
