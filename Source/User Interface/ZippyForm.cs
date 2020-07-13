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
using System.Runtime.InteropServices;

namespace ZippyBackup.User_Interface
{
    public partial class ZippyForm : Form
    {
        public ZippyForm()
        {
            InitializeComponent();
        }

        static XmlSerializer Serializer = new XmlSerializer(typeof(BackupList));

        protected void LoadConfigFromFile(string Filename)
        {
            using (FileStream fs = new FileStream(Filename, FileMode.Open))
                MainList = Serializer.Deserialize(fs) as BackupList;

            foreach (BackupProject bp in MainList.Projects)
            {
                bp.AfterXmlLoad();
                bp.Refresh();
            }
        }

        protected void LoadConfig(string Configuration, SplashForm Splash)
        {
            string ConfigStr = Configuration;
            if (ConfigStr != null)
            {
                Utility.Debug(ConfigStr);

                using (StringReader sr = new StringReader(ConfigStr))
                    MainList = Serializer.Deserialize(sr) as BackupList;                

                foreach (BackupProject bp in MainList.Projects)
                {
                    if (Splash != null) Splash.Status = "Loading Backup Project '" + bp.Name + "'...";

                    bp.AfterXmlLoad();
                    bp.Refresh();
                }

                if (Splash != null) Splash.Status = "Almost done loading configuration...";
            }
            else MainList = BackupList.CreateDefault();
        }

        protected void SaveConfigToFile(string Filename)
        {
            using (FileStream fs = new FileStream(Filename, FileMode.Create))
                lock (MainList.Projects)
                    Serializer.Serialize(fs, MainList);
        }

        protected string SaveConfig()
        {
            string Configuration;
            using (StringWriter sw = new StringWriter())
            {
                lock (MainList.Projects)
                    Serializer.Serialize(sw, MainList);
                Configuration = sw.ToString();
            }
            return Configuration;
        }

        public static BackupList MainList;

        public static void SendEMail(string Subject, string Body)
        {
            if (MainList.Schedule.EMailUserIfBackupPastDue)
            {
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                message.To.Add(MainList.Schedule.EMailSettings.EMailTo);
                message.Subject = Subject;
                message.From = new System.Net.Mail.MailAddress(MainList.Schedule.EMailSettings.EMailFrom);
                message.Body = Body;
                System.Net.Mail.SmtpClient smtp
                    = new System.Net.Mail.SmtpClient(MainList.Schedule.EMailSettings.SMTPServer, MainList.Schedule.EMailSettings.SMTPPort);
                smtp.Credentials = new System.Net.NetworkCredential(MainList.Schedule.EMailSettings.Username, MainList.Schedule.EMailSettings.Password.Password);
                smtp.Send(message);
            }
        }

        static object LogLock = new object();

        public static void LogWriteLine(LogLevel MinLevel, string text)
        {
            if ((int)MainList.Logging < (int)MinLevel) return;
            try
            {
                lock (LogLock)
                    using (StreamWriter sw = new StreamWriter(MainList.Logfile, true, Encoding.UTF8)) sw.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": " + text);
            }
            catch (Exception) { }
        }

        public static void LogNextLine(LogLevel MinLevel, string text)
        {            
            if ((int)MainList.Logging < (int)MinLevel) return;
            try
            {
                lock (LogLock)
                    using (StreamWriter sw = new StreamWriter(MainList.Logfile, true, Encoding.UTF8)) sw.WriteLine("\t" + text);
            }
            catch (Exception) { }
        }

        public static bool IsLoggingAt(LogLevel MinLevel)
        {
            return (int)MainList.Logging >= (int)MinLevel;
        }

        public static string LogFileName { get { return MainList.Logfile; } }
    }
}
