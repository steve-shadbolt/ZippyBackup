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
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
using Microsoft.Win32;
using Ionic.Zip;

namespace ZippyBackup
{
    [XmlRoot("backup-list")]
    public class BackupList
    {
        /// <summary>
        /// Projects is lock-protected and should only be accessed inside of
        /// a lock(Projects) statement.
        /// </summary>
        public List<BackupProject> Projects = new List<BackupProject>();

        /// <summary>
        /// Provides a list of file extensions which should not be included in the backup.  The period (.) should be included.
        /// For example, to exclude audio-video interleaved files, add ".avi" to the list.
        /// </summary>
        public List<string> ExcludeExtensions = new List<string>();

        /// <summary>
        /// Provides a list of file extensions which should not be compressed in the backup.  The period (.) should be included.
        /// For example, to avoid compressing audio-video interleaved files, add ".avi" to the list.
        /// </summary>
        public List<string> CompressedExtensions = new List<string>();

        /// <summary>
        /// Provides a list of folder names/patterns that should not be included in the backup.        
        /// </summary>
        public List<string> ExcludeFolderPatterns = new List<string>();

        /// <summary>
        /// Constrains the allowable size of an archive to some maximum.  If the size is exceeded, then multiple archives are
        /// generated to accomplish the backup.  Measured in bytes.  If given as 'long.MaxValue', no limit is given.
        /// </summary>
        public long ConstrainArchiveSize = 10 /*GB*/ * BytesPerGB;
        private static long BytesPerGB = 1073741824 /*bytes/GB*/;

        [XmlAttribute("logfile")]
        public string Logfile;

        [XmlAttribute("log-level")]
        public LogLevel Logging = LogLevel.None;

        public BackupSchedule Schedule = new BackupSchedule();

        public BackupList() { }

        public static BackupList CreateDefault()
        {
            BackupList ret = new BackupList();            
            return ret;
        }
    }

    public enum LogLevel
    {
        None = 0,
        Errors = 10,
        Warnings = 20,
        Information = 30,
        LightDebug = 40,
        MediumDebug = 50,
        HeavyDebug = 60
    }

    public class BackupProject : User_Interface.IDisplayStates, IDisposable
    {
        public string Name;

        /// <summary>
        /// BackupFolder is the name of the folder where backup archives are stored.  See
        /// CompleteBackupFolder for the full path to this folder.
        /// </summary>
        public string BackupFolder;

        public string CompleteBackupFolder
        {
            get { return BackupFolder + "\\" + Name; }
        }

        /// <summary>
        /// SourceFolder is the complete path to the folder which the user wants backed up.  For
        /// example, "C:\Users\Person\Documents" might be a SourceFolder.
        /// </summary>
        public string SourceFolder;

        /// <summary>
        /// If Password is provided, then all archives generated with this project should be
        /// encrypted with 256-bit AES.  If Password is null, then all archives generated in 
        /// this project are unencrypted.
        /// </summary>        
        public string Password;     // Deprecated, used SafePassword instead.

        /// <summary>
        /// In Rev 16 of ZippyBackup, we moved the Password into a somewhat safer location.  SafePassword
        /// is preferred to Password.  In later revisions, remove Password.
        /// </summary>
        public StoredPassword SafePassword = new StoredPassword();

        /// <summary>
        /// Provides a list of subfolders to be excluded from the backup.  The subfolders may be multiple levels deep and
        /// are specified as a relative path.  For example, "Subfolder/Deeper/Last" would exclude the folder Last contained
        /// in the folder Deeper contained in Subfolder, which is contained in the archive's SourceFolder.
        /// </summary>
        public List<string> ExcludeSubfolders = new List<string>();

        /// <summary>
        /// Provides a list of file extensions which should not be included in the backup.  The period (.) should be included.
        /// For example, to exclude audio-video interleaved files, add ".avi" to the list.
        /// </summary>
        public List<string> ExcludeExtensions = new List<string>();

        /// <summary>
        /// Provides a list of specific files which should not be included in the backup.  The path should be specified relative
        /// to SourceFolder.  For example, if the source folder is @"C:\", then an ExcludeFiles entry might be "Pagefile.sys".
        /// </summary>
        public List<string> ExcludeFiles = new List<string>();

        /// <summary>
        /// Specifies the maximum file size to include in a backup.  Any files larger than this size will not be backed up.
        /// If the value long.MaxValue is used, then file size will not be considered.
        /// </summary>
        public long ExcludeFileSize = long.MaxValue;        

        /// <summary>
        /// SourceCredentials provides optional network credentials to be utilized when accessing
        /// the source folder by ZippyBackup.
        /// </summary>
        public StoredNetworkCredentials SourceCredentials = new StoredNetworkCredentials();

        /// <summary>
        /// BackupCredentials provides optional network credentials to be utilized when accessing
        /// the backup folder by ZippyBackup.
        /// </summary>
        public StoredNetworkCredentials BackupCredentials = new StoredNetworkCredentials();

        /// <summary>
        /// If enabled, then the Windows Volume Shadow Service is asked to take a shadow snapshot of the drive
        /// containing the source folder at the start of every backup, and that snapshot is used in place of
        /// the original file structure.  This is useful if there is the possibility that files will be in-use,
        /// such as a Microsoft Outlook PST (mail) file is in use or a Microsoft Exchange Server or SQL Server
        /// is running, both of which keep files in use at all times.
        /// </summary>
        public bool UseVolumeShadowService = false;

        /// <summary>If set, then this project will not cause "nag" warnings via e-mail or balloon tips.</summary>
        public bool DoNotRemind = false;

        /// <summary>
        /// If not null, this string indicates the last folder scanned in the most recent backup.
        /// For an automated backup, this is the "continuation path" where the next scan should
        /// pickup.  This value is not stored with the project configuration, but is instead stored in the
        /// Backup_Status.xml file and is loaded by the Refresh() call.
        /// </summary>
        [XmlIgnore]
        public string LastScanRelativePath;

        /// <summary>
        /// If not null, this string indicates the last folder scanned in the most recent verification.
        /// For an automated backup, this is the "continuation path" where the next verification should
        /// pickup.  This value is not stored with the project configuration, but is instead stored in the
        /// Backup_Status.xml file and is loaded by the Refresh() call.
        /// </summary>
        [XmlIgnore]
        public string LastVerifyRelativePath;

        /// <summary>
        /// Last time a Refresh() call was made.
        /// </summary>
        [XmlIgnore]
        public DateTime LastRefresh = DateTime.MinValue;

        [XmlIgnore]
        private Thread StatusThread;

        [XmlIgnore]
        private bool Closing = false;

        public BackupProject()
        {
            StatusThread = new Thread(new ThreadStart(StatusMonitor));
            StatusThread.Start();
        }

        public void OnClosing()
        {
            Closing = true;
        }

        public void OnClosed()
        {
            if (StatusThread != null) { Closing = true; StatusThread.Join(); }
        }

        public void Dispose()
        {
            OnClosed();
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return Name;
        }

#if false
        public BackupProject Clone()
        {
            BackupProject cp = new BackupProject();
            cp.Name = Name;
            cp.BackupFolder = BackupFolder;
            cp.SourceFolder = SourceFolder;
            cp.Password = Password;
            cp.SafePassword = SafePassword.Clone();
            cp.ExcludeExtensions = new List<string>();
            foreach (string ss in ExcludeExtensions) cp.ExcludeExtensions.Add(ss);
            cp.ExcludeFiles = new List<string>();
            foreach (string ss in ExcludeFiles) cp.ExcludeFiles.Add(ss);
            cp.SourceCredentials = SourceCredentials.Clone();
            cp.BackupCredentials = BackupCredentials.Clone();
            cp.UseVolumeShadowService = UseVolumeShadowService;
            cp.DoNotRemind = DoNotRemind;

            cp.AlternativePassword = AlternativePassword;
            lock (ArchiveFileList)
                cp.ArchiveFileList = ArchiveFileList.Clone();
            cp.MostRecentBackup = MostRecentBackup;
            // The Manifest Cache is not cloned.
            return cp;
        }
#endif

        public void AfterXmlLoad()
        {
            if (!string.IsNullOrEmpty(Password))
            {
                SafePassword.Password = Password;
                Password = "";
            }
        }        

        public void Refresh() { Refresh(false); }

        public void Refresh(bool PromptForPassword)
        {                        
            try
            {
                LastRefresh = DateTime.Now;

                ArchiveFilename MostRecent;
                lock (ArchiveFileList)
                {
                    using (Impersonator newself = new Impersonator(BackupCredentials))
                        ArchiveFileList.LoadAll(this);

                    // Locate most recent backup...
                    MostRecent = ArchiveFileList.FindMostRecent();
                }

                if (MostRecent == ArchiveFilename.MaxValue) MostRecentBackup = DateTime.MinValue;
                else
                {
                    try
                    {
                        Manifest Manifest;
                        using (Impersonator newself = new Impersonator(BackupCredentials))
                            Manifest = MostRecent.LoadArchiveManifest(this, PromptForPassword);
                        MostRecentBackup = Manifest.BackupStartTime;
                    }
                    catch (Ionic.Zip.BadPasswordException bpe)
                    {
                        if (PromptForPassword) throw bpe;
                        else MostRecentBackup = MostRecent.BackupDate;
                    }
                }

                // Also check for an Backup_Status.xml file...
                // This file is created with each backup, and is particularly helpful when we have an 'empty backup'
                // where no archive need be created (because nothing has changed).  We need to display to the user that
                // the project was backed up recently, but we needn't create an archive.  The Backup_Status.xml file
                // accomplishes this.
                FileInfo[] FileList = new DirectoryInfo(CompleteBackupFolder).GetFiles("Backup_Status.xml");
                if (FileList.Length > 0)
                {
                    BackupStatus Status = BackupStatus.Load(FileList[0].FullName);
                    if (Status != null)
                    {
                        if (MostRecent != ArchiveFilename.MaxValue
                            && Status.LastArchive == MostRecent.ToString()
                            && Status.LastBackup.ToUniversalTime() > MostRecentBackup.ToUniversalTime())
                            MostRecentBackup = Status.LastBackup;
                        MostRecentCompleteBackup = Status.LastCompletedBackup;
                        LastScanRelativePath = Status.LastScanRelativePath;
                        LastVerifyRelativePath = Status.LastVerifyRelativePath;
                        MostRecentVerify = Status.LastVerify;
                        MostRecentCompleteVerify = Status.LastCompletedVerify;
                    }
                }

                // Clear out the manifest cache...
                lock (ManifestCache)
                    ManifestCache.Clear();

                LoadIssue = false;
            }
            catch (IOException) { LoadIssue = true; }
        }

        /// <summary>
        /// OnNewBackup is similar to Refresh(), but is called whenever a backup run has completed
        /// that generated a new backup archive (i.e. files had changed).  It is called immediately
        /// after the backup run is completed.
        /// </summary>
        public void OnNewBackup()
        {
            lock (ArchiveFileList)
                using (Impersonator newself = new Impersonator(BackupCredentials))
                    ArchiveFileList.LoadAll(this);
        }

        public void OnNewBackup(ArchiveFilename NewArchive)
        {
            lock (ArchiveFileList)
                ArchiveFileList.Archives.Add(NewArchive);
        }

        [XmlIgnore]
        public bool IsNoBackups
        {
            get
            {
                lock (ArchiveFileList)
                    return ArchiveFileList.Archives.Count == 0;
            }
        }

        /// <summary>
        /// When calling Refresh() and performing incremental backups, sometimes a situation arrises where the user
        /// has changed the backup project's password since a previous backup.  When this happens, we prompt the
        /// user for the password.  In order to avoid asking for it twice (or more) times, we retain the older
        /// password once the user enters is and attempt to re-use it anytime the older password is needed.
        /// </summary>
        [XmlIgnore]
        public string AlternativePassword;

        /// <summary>
        /// ArchiveFileList contains a listing of all archive files associated with the backup project.
        /// A lock must be placed on ArchiveFileList before accessing.
        /// </summary>
        [XmlIgnore]
        public ArchiveFileList ArchiveFileList = new ArchiveFileList();

        /// <summary>
        /// ManifestCache contains a cache of information about each archive.  When an archive's manifest
        /// is loaded during a search, it is stored here by the search background thread so that the
        /// search can quickly access the archive in memory.  A lock must be placed on ManifestCache
        /// before accessing.
        /// </summary>
        [XmlIgnore]
        public Dictionary<ArchiveFilename, Manifest> ManifestCache = new Dictionary<ArchiveFilename,Manifest>();

        /// <summary>
        /// MostRecentBackup is calculated by the Refresh() function.  MostRecentBackup takes on the
        /// value DateTime.MinValue if no backups have been made in this folder.  If a password is
        /// unavailable, then MostRecentBackup may have reduced accuracy (within 24hrs).
        /// </summary>
        [XmlIgnore]
        public DateTime MostRecentBackup = DateTime.MinValue;

        /// <summary>
        /// MostRecentCompleteBackup is calculated by the Refresh() function.  MostRecentCompleteBackup takes 
        /// on the value DateTime.MinValue if no complete backups have been made in this folder.  If a password is
        /// unavailable, then MostRecentCompleteBackup may have reduced accuracy (within 24hrs).
        /// </summary>
        [XmlIgnore]
        public DateTime MostRecentCompleteBackup = DateTime.MinValue;

        /// <summary>
        /// MostRecentVerify is calculated by the Refresh() function.  MostRecentVerify takes on the
        /// value DateTime.MinValue if no verifications have been made in this folder.
        /// </summary>
        [XmlIgnore]
        public DateTime MostRecentVerify = DateTime.MinValue;

        /// <summary>
        /// MostRecentCompleteVerify is calculated by the Refresh() function.  MostRecentCompleteVerify takes on the
        /// value DateTime.MinValue if no completed verifications have been made in this folder.
        /// </summary>
        [XmlIgnore]
        public DateTime MostRecentCompleteVerify = DateTime.MinValue;

        public static BackupProject CreateDefault()
        {
            BackupProject ret = new BackupProject();
            ret.Name = "My Documents";            
            ret.SourceFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            // Determine if the user has DropBox installed...
            string Home = Utility.GetUserHomeDirectory();
            if (Directory.Exists(Home + "\\DropBox")) ret.BackupFolder = Home + "\\DropBox\\My Backups";
            else if (Directory.Exists(Home + "\\Google Drive")) ret.BackupFolder = Home + "\\Google Drive\\My Backups";
            else ret.BackupFolder = Home + "\\Backups";
            return ret;
        }

        [XmlIgnore]
        public bool LoadIssue = false;

        [XmlIgnore]
        public int State = -1;
        public int GetState() { return (LoadIssue ? (int)User_Interface.MainForm.ProjectStateIcon.Alarm : State); }

        [XmlIgnore]
        object StatusLock = new object();
        DateTime LastStatusCheck = DateTime.MinValue;        
        FolderStatus LastStatusCheckResult = FolderStatus.Unknown;
        FolderStatus LastRetrievedStatus = FolderStatus.Unknown;

        private void StatusMonitor()
        {
            Thread.CurrentThread.Name = "Status Monitor for Project '" + Name + "'";
            while (!Closing)
            {
                FolderStatus fsResult = GetProjectStatusAux();
                lock (StatusLock)
                {
                    LastStatusCheckResult = fsResult;
                    LastStatusCheck = DateTime.Now;
                }

                int CummulativeElapsed = 0;
                while (CummulativeElapsed < 15000 && !Closing)
                {
                    Thread.Sleep(100); CummulativeElapsed += 100;
                }
            }
        }

        public FolderStatus GetProjectStatus()
        {
            FolderStatus StatusResult;
            lock (StatusLock) { StatusResult = LastStatusCheckResult; LastRetrievedStatus = StatusResult; }
            return StatusResult;
        }

        public bool HasProjectStatusChanged()
        {
            bool Changed = false;
            lock (StatusLock)
            {
                if (LastRetrievedStatus != LastStatusCheckResult) Changed = true;
            }
            return Changed;
        }

        private FolderStatus GetProjectStatusAux()
        {
            // Can occur on "temporary" projects that are setup as part of a Restore operation and not part of the
            // active backup project list.
            if (SourceFolder == null || CompleteBackupFolder == null) return FolderStatus.Unknown;

            try
            {
                Uri SrcUri;
                try
                {
                    SrcUri = new Uri(SourceFolder);
                }
                catch (Exception)
                {
                    SrcUri = new Uri(Utility.StripTrailingSlash(SourceFolder) + Path.DirectorySeparatorChar);
                }

                if (SrcUri.IsUnc)
                {
                    // Network status checks can take a while.  The timeout can be quite long, 60 seconds or so, and then if you have multiple
                    // network projects this can lead to some nasty cycles.  We shouldn't be *trying* to check project status that often, but
                    // let's also prvent it here.  UPDATE: now running on our own thread.  Still, don't hit it too often.
                    //if ((DateTime.Now - LastStatusCheck).TotalSeconds < 15) return LastStatusCheckResult;

                    if (!Directory.Exists(SourceFolder)) return FolderStatus.Offline;
                }
                else
                {
                    if (!Directory.Exists(SourceFolder)) return FolderStatus.MissingSource; 
                }

                Uri BUri;
                try
                {
                    BUri = new Uri(CompleteBackupFolder);
                }
                catch (Exception)
                {
                    BUri = new Uri(Utility.StripTrailingSlash(CompleteBackupFolder) + Path.DirectorySeparatorChar);
                }

                if (BUri.IsUnc)
                {
                    // Network status checks can take a while.  The timeout can be quite long, 60 seconds or so, and then if you have multiple
                    // network projects this can lead to some nasty cycles.  We shouldn't be *trying* to check project status that often, but
                    // let's also prvent it here.  UPDATE: now running on our own thread.  Still, don't hit it too often.
                    //if ((DateTime.Now - LastStatusCheck).TotalSeconds < 15) return LastStatusCheckResult;

                    if (!Directory.Exists(CompleteBackupFolder)) return FolderStatus.Offline;
                }
                else
                {
                    if (!Directory.Exists(CompleteBackupFolder)) return FolderStatus.MissingBackup;
                }

                return FolderStatus.Online;
            }
            catch (IOException) { return FolderStatus.AccessError; }
        }
    }

    public class BackupStatus
    {
        [XmlAttribute]
        public DateTime LastBackup = DateTime.MinValue;

        [XmlAttribute]
        public DateTime LastCompletedBackup = DateTime.MinValue;

        [XmlAttribute]
        public DateTime LastSync = DateTime.MinValue;

        [XmlAttribute]
        public DateTime LastVerify = DateTime.MinValue;

        [XmlAttribute]
        public DateTime LastCompletedVerify = DateTime.MinValue;

        [XmlAttribute]
        public string LastArchive;

        [XmlAttribute]
        public string LastScanRelativePath;         // Stores continuation information, when applicable.

        [XmlAttribute]
        public string LastVerifyRelativePath;

        static XmlSerializer Serializer = new XmlSerializer(typeof(BackupStatus));

        public static BackupStatus Load(string Path)
        {
            try
            {
                for (int Retry = 0; ; Retry++)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read))
                            return (BackupStatus)Serializer.Deserialize(fs);
                    }
                    catch (Exception exc)
                    {
                        if (Retry >= 10) throw exc;
                        System.Threading.Thread.Sleep(10);
                    }
                }
            }
            catch (IOException exc)
            {
                List<System.Diagnostics.Process> Processes = FileLockInfo.Win32Processes.GetProcessesLockingFile(Path);
                if (Processes.Count == 0) throw new IOException(exc.Message + "\nNo processes presently lock the file.", exc);
                StringBuilder sb = new StringBuilder("\nThe file is locked by the following processes:\n");
                foreach (System.Diagnostics.Process proc in Processes) sb.AppendLine("\t" + proc.ProcessName);
                throw new IOException(exc.Message + sb.ToString(), exc);
            }
        }

        public void Save(string Path)
        {
            using (FileStream fs = new FileStream(Path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                Serializer.Serialize(fs, this);
        }
    }

    public enum FolderStatus
    {
        Unknown,
        Online,
        MissingSource,
        MissingBackup,
        Offline,
        AccessError
    }

    /*
    public class PendingUpdates
    {        
    }
     */

    public static class Globals
    {        
        public static List<string> BuiltinCompressedExtensions = new List<string>(
            new string[] {
                /** Common formats **/
                ".avi", ".mpg", ".mpeg", ".mpg", ".mp4", ".mkv", 
                ".mp3", ".wma", 
                ".jpg", ".png",
                ".docx", ".xlsx", ".pptx", ".ppsx",
                ".zip", ".rar", ".gz", ".gzip", ".7z", ".lzma",

                /** Uncommon formats **/
                ".asf", ".rm", ".3gp", ".3gpp", ".flv", ".mov",
                ".m4a", ".m4v", ".m4p", ".aac", ".m1v", ".m2v",
                ".mp2", ".flac", ".ra", 
                ".tif", ".tiff", ".gif", 
                ".arj", ".bz2", ".tgz", ".cab"
            });

        public static List<string> BuiltinExcludeFolderPatterns = new List<string>(
            new string[] {
                ".svn"
            });
    }
}
