/** ZippyBackup
 *  Copyright (C) 2012-2013 by Wiley Black.  All rights reserved.
 *  See License.txt for licensing rules.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ZippyBackup.User_Interface;
using Alphaleonis.Win32.Filesystem;         // Replaces System.IO entities such as FileInfo with more advanced versions.
using ZippyBackup.IO;
using ZippyBackup.Diagnostics;

namespace ZippyBackup
{
    public enum BackupTypes
    {
        Any,
        Incremental,
        IncrementalAutomatic,
        Complete
    }

    public class NoCompleteBackupException : Exception
    {
        public NoCompleteBackupException() : base("Cannot perform incremental backup - no existing backups found.") { }
    }

    public class CancelException : Exception 
    {
        public CancelException() : base("User cancelled operation.") { }
    }    
    
    public class BackupRun : IDisposable
    {
        BackupProject Project;

        User_Interface.ProgressForm Progress;

        DirectoryInfo BackupFolder;
        
        /// <summary>
        /// When a project has the "Use Volume Shadow Service (VSS)" option set, we need to
        /// create a shadow snapshot of the volume at the start of each backup operation.
        /// This happens in Initialize(), and then Shadow provides the root folder of the
        /// shadow snapshot for reading.  Files on the shadow can only be accessed via
        /// the Alphaleonis.Win32.FileSystem namespace, which replaces built-in
        /// classes such as FileInfo and DirectoryInfo.  Some additional replacements
        /// happen in ZippyBackup.IO.
        /// </summary>
        ShadowVolume Snapshot;

        /// <summary>
        /// Gives the root folder for enumerating and reading files for the backup.  This
        /// can be equal to Project.SourceFolder, but if VSS is enabled then it is a dynamically
        /// generated location.
        /// </summary>
        string SourceRoot;

        int iNextFilename = 1;

        /// <summary>
        /// AutomaticBackup is true for IncrementalAutomatic backup types, which are those initiated
        /// by the automatic scheduler.  These backups behave similar to user initiated backups, 
        /// except that they have a time-limit for their operation.
        /// </summary>
        bool AutomaticBackup = false;

        /// <summary>
        /// Indicates the time when the current BackupRun was initiated.
        /// </summary>
        int StartTick = Environment.TickCount;

        const int MaxAutomaticDurationInTicks = 30 /*minutes*/ * 60 /*seconds/minute*/ * 1000 /*ticks/second*/;

        public BackupRun(BackupProject Project)
        {
            this.Project = Project;
        }

        public void Dispose()
        {
            Dispose(true);                  // Dispose of unmanaged resources.            
            GC.SuppressFinalize(this);      // Suppress finalization.
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Snapshot != null) { Snapshot.Dispose(); Snapshot = null; }
            }
        }        

        public void Run() { Run(BackupTypes.Any); }        

        BackupTypes LaunchType;
        Thread WorkerThread;
        Exception WorkerException;
        public void Run(BackupTypes BackupType)
        {
            Progress = new User_Interface.ProgressForm();
            try
            {
                Progress.OverallProgressBar.Maximum = 100;
                Progress.OverallProgressBar.Minimum = 0;
                Progress.OverallProgressBar.Value = 0;
                Progress.label1.Text = "Starting backup '" + Project.Name + "'...";
                Progress.label2.Text = "";
                Progress.Show();

                LaunchType = BackupType;
                WorkerThread = new Thread(RunWorker);
                WorkerThread.Start();

                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
                while (WorkerThread.IsAlive)
                {
                    Thread.Sleep(500);
                    Progress.ApplyPending();
                    Application.DoEvents();
                }
                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
                if (WorkerException != null) throw WorkerException;
            }
            catch (CancelException) { }
            finally
            {
                Progress.Dispose();
            }
        }

        public void RunWorker()
        {
            try
            {
                RunWorker(LaunchType);
            }
            catch (Exception ex)
            {
                WorkerException = ex;
            }
            if (Snapshot != null) { Snapshot.Dispose(); Snapshot = null; }
        }

        public void RunWorker(BackupTypes BackupType)
        {
            try
            {
                try
                {
                    BackupFolder = new DirectoryInfo(Project.CompleteBackupFolder);
                    Project.Refresh(true);
                }
                catch (CancelException) { }
                catch (Exception ex)
                {
                    throw new Exception("While analyzing current backup archive list: " + ex.Message, ex);
                }

                switch (BackupType)
                {
                    case BackupTypes.Complete:
                        ZippyForm.LogWriteLine(LogLevel.Information, "Starting complete backup of '" + Project.Name + "'.");
                        RunComplete();
                        break;

                    case BackupTypes.Any:
                        // Decide if we are able to do an incremental backup.  If we can, do so...  If not, do a complete.
                        // Locate latest backup (either complete or incremental.)
                        ArchiveFilename LatestBackup;
                        lock (Project.ArchiveFileList) LatestBackup = Project.ArchiveFileList.FindMostRecent();
                        if (LatestBackup != ArchiveFilename.MaxValue)
                        {
                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "Starting backup of '" + Project.Name + "', determined as incremental backup.");
                            ZippyForm.LogWriteLine(LogLevel.Information, "Starting incremental backup of '" + Project.Name + "'.");
                            RunIncremental();
                        }
                        else
                        {
                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "Starting backup of '" + Project.Name + "', determined as complete backup.");
                            ZippyForm.LogWriteLine(LogLevel.Information, "Starting complete backup of '" + Project.Name + "'.");
                            RunComplete();
                        }
                        break;

                    case BackupTypes.Incremental:
                        ZippyForm.LogWriteLine(LogLevel.Information, "Starting incremental backup of '" + Project.Name + "'.");
                        RunIncremental();
                        break;

                    case BackupTypes.IncrementalAutomatic:
                        AutomaticBackup = true;
                        ZippyForm.LogWriteLine(LogLevel.Information, "Starting incremental backup of '" + Project.Name + "'.");
                        RunIncremental();
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            catch (CancelException ce)
            {
                ZippyForm.LogWriteLine(LogLevel.Information, "Backup cancelled before completion.");
                throw ce;
            }
            catch (NoCompleteBackupException exc)
            {
                ZippyForm.LogWriteLine(LogLevel.Warnings, "Cannot perform automatic/incremental backups because no existing backups were found.");
                throw exc;
            }
            catch (Exception ex)
            {
                ZippyForm.LogWriteLine(LogLevel.Errors, "Error during backup of '" + Project.Name + "':");
                ZippyForm.LogNextLine(LogLevel.Errors, ex.Message + " (96)");
                ZippyForm.LogNextLine(LogLevel.LightDebug, "");
                ZippyForm.LogNextLine(LogLevel.LightDebug, ex.ToString());

                throw new Exception(ex.Message + " (96)", ex);
            }            
        }

        void DoEvents()
        {
            //Application.DoEvents();       // Not from the worker thread.
            if (Progress.Cancel) throw new CancelException();
        }

        ArchiveFilename ArchiveName;
        Manifest Manifest;

        long ArchiveSize = 0;

        /// <summary>
        /// NameMapping is used during the zip process to quickly retrieve the original filename
        /// for a file.  The DotNetZip library only provides the name in the archive, but we've
        /// renamed all the files to File0001, File0002, etc. inside the archive.  To provide a
        /// useful UI, we need to know the filename as the user would recognize it.
        /// 
        /// 5-31-2016 Update: There is no longer a 1:1 name mapping as the identical file reduction
        /// method can now introduce a 1:Many mapping when multiple file contents are identical.
        /// Since NameMapping is only used for UI and the 2nd+ file requires no time to archive,
        /// there is no need to mention the 2nd+ file in the name mapping.
        /// </summary>
        Dictionary<string, string> NameMapping;

        /// <summary>
        /// NameEntryMapping is used during the scanning process to rapidly identify files that
        /// could be duplicates located in different file locations.  In an incremental backup,
        /// all previously archived files are added to the SizeMapping, using the file length
        /// as their key.  Since multiple files can have the same length and not be duplicates,
        /// the mapping points to a list to allow multiple values.
        /// 
        /// During RunFolder(), each new file is checked for duplication using this dictionary.  If
        /// the file is determined to be unique then it is added to the dictionary in case of further
        /// matches later in the scan.  If the file is not unique, and the reference is valid, then
        /// we remap the new manifest entry to the existing or duplicated archive store, eliminating
        /// the need to store the file contents twice.
        /// </summary>
        Dictionary<UInt64, List<Manifest.File>> SizeMapping = new Dictionary<UInt64,List<Manifest.File>>();

        string VSSStatus;
        void OnVSSStatus(string Message)
        {
            Progress.Pending_Label2_Text = Message;
            VSSStatus = Message;
        }

        List<Utility.Wildcard> Wildcards_ExcludedFolderPatterns;

        void Initialize(BackupTypes BackupType)
        {
            try
            {
                NameMapping = new Dictionary<string, string>();
                NameMapping.Add("Manifest.xml", "Manifest.xml");                

                ArchiveSize = 0;

                using (NetworkConnection newself = new NetworkConnection(Project.SourceFolder, Project.SourceCredentials))
                    if (!Directory.Exists(Project.SourceFolder))
                        throw new DirectoryNotFoundException("Source directory '" + Project.SourceFolder + "' does not exist or is offline.");                
                using (NetworkConnection newself = new NetworkConnection(Project.CompleteBackupFolder, Project.BackupCredentials))
                    if (!Directory.Exists(Project.CompleteBackupFolder))
                        throw new DirectoryNotFoundException("Backup directory '" + Project.CompleteBackupFolder + "' does not exist or is offline.");

                Progress.Pending_OverallProgressBar_Value = 0;
                Progress.Pending_CurrentProgressBar_Value = 0;

                // Some optimization, do this once now rather than repeatedly later...
                Wildcards_ExcludedFolderPatterns = new List<Utility.Wildcard>();
                foreach (string Excluded in User_Interface.ZippyForm.MainList.ExcludeFolderPatterns)
                    Wildcards_ExcludedFolderPatterns.Add(new Utility.Wildcard(Excluded, System.Text.RegularExpressions.RegexOptions.IgnoreCase));
                foreach (string Excluded in Globals.BuiltinExcludeFolderPatterns)
                    Wildcards_ExcludedFolderPatterns.Add(new Utility.Wildcard(Excluded, System.Text.RegularExpressions.RegexOptions.IgnoreCase));

                // Capture Volume Shadow Snapshot if applicable...
                SourceRoot = Project.SourceFolder;

                if (Project.UseVolumeShadowService)
                {
                    Progress.Pending_Label1_Text = "Capturing VSS shapshot for backup '" + Project.Name + "'...";
                    Progress.Pending_Label2_Text = "";
                    Progress.Pending_Label3_Text = "";
                    try
                    {
                        string Volume = new DirectoryInfo(Project.SourceFolder).Root.FullName;
                        using (NetworkConnection newself = new NetworkConnection(Project.SourceFolder, Project.SourceCredentials))  // Actually, needs to be LocalSystem in a Service.
                        {
                            ZippyForm.LogWriteLine(LogLevel.MediumDebug, "Initializing Volume Shadow Service (VSS) Interface...");
                            Snapshot = new ShadowVolume(Volume);
                            Snapshot.OnStatusUpdate += new ShadowVolume.UpdateStatus(OnVSSStatus);
                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "Starting shadow snapshot capture...");
                            Snapshot.TakeSnapshot();
                        }
                        string RelativeToVolume = Utility.GetRelativePath(Volume, Project.SourceFolder);
                        SourceRoot = Snapshot.ShadowRoot + "\\" + RelativeToVolume;
                        ZippyForm.LogWriteLine(LogLevel.LightDebug, "Shadow snapshot captured at '" + Snapshot.ShadowRoot + "'...");
                        ZippyForm.LogWriteLine(LogLevel.LightDebug, "Working source root at '" + SourceRoot + "'.");
                    }
                    catch (Exception exc)
                    {
                        throw new Exception("While taking VSS snapshot: " + exc + "\nStatus at error: " + VSSStatus, exc);
                    }                    
                }

                switch (BackupType)
                {
                    default:
                    case BackupTypes.Any: Progress.Pending_Label1_Text = "Starting backup '" + Project.Name + "'..."; break;
                    case BackupTypes.Complete: Progress.Pending_Label1_Text = "Starting complete backup '" + Project.Name + "'..."; break;
                    case BackupTypes.Incremental: Progress.Pending_Label1_Text = "Starting incremental backup '" + Project.Name + "'..."; break;
                }
                Progress.Pending_Label2_Text = "";

                // Choose new archive filename...
                // The most recent backup may not have been on the same day as the backup being made now, so this 
                // search is different than checking for the most recent.
                DateTime Now = DateTime.Now;
                ArchiveFilename LastOfDay;
                lock (Project.ArchiveFileList)
                {
                    LastOfDay = Project.ArchiveFileList.Find(
                        new ArchiveFilename(Now, 0),
                        ArchiveFilename.MaxValue,
                        new ArchiveFilename(Now, int.MaxValue));
                }

                ArchiveName = new ArchiveFilename(DateTime.Now,
                    (LastOfDay == ArchiveFilename.MaxValue ? 0 : LastOfDay.BackupTime + 1),
                    BackupType);

                // Start assembling a new manifest...
                Manifest = new Manifest(Project);
                Manifest.BackupStartTime = DateTime.UtcNow;
                Manifest.BackupSequence = ArchiveName.BackupTime;
                Manifest.BackupMachineId = MachineId.Host;
                Manifest.IncrementalBackup = (ArchiveName.BackupType == BackupTypes.Incremental);
                DirectoryInfo diSourceRoot = new DirectoryInfo(SourceRoot);
                Manifest.ArchiveRoot = new Manifest.Folder("", diSourceRoot);
            }
            catch (CancelException ce) { throw ce; }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "  Error while initializing new backup.", ex);
            }
        }
        
        DiagnosticStreamWriter DiagWriter;        

        void RunComplete()
        {
            Progress.Pending_Label1_Text = "Performing complete backup '" + Project.Name + "'...";
            DoEvents();

            // We sometimes run into the single-archive size limit and have to generate multiple
            // archives to accomplish a backup...
            Continuation Continuation = new Continuation();

            Initialize(BackupTypes.Complete);
            try
            {
                try
                {
                    // Scan through folders and files and build up the new manifest.
                    using (ZipFile zip = new ZipFile())
                    {
                        if (!string.IsNullOrEmpty(Project.SafePassword.Password))
                        {
                            zip.Password = Project.SafePassword.Password;
                            zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                        }

                        using (NetworkConnection newself = new NetworkConnection(Project.SourceFolder, Project.SourceCredentials))
                        {
                            Continuation.StartPass(SourceRoot);
                            RunFolder(zip, Manifest.ArchiveRoot, new DirectoryInfo(SourceRoot), null, ref Continuation);
                        }
                        using (MemoryStream ms = new MemoryStream())
                        {
                            try
                            {
                                ZippyForm.LogWriteLine(LogLevel.LightDebug, "Adding manifest.xml stream to zip archive.");

                                Manifest.ToXml(ms);
                                ms.Seek(0, System.IO.SeekOrigin.Begin);
                                zip.AddEntry("Manifest.xml", ms);
                            }
                            catch (CancelException ce) { throw ce; }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message + "  Error while adding manifest to archive.", ex);
                            }

                            Progress.Pending_Label2_Text = Continuation.Required ? "Generating partial backup archive file..." : "Generating backup archive file...";
                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "Generating " + (Continuation.Required ? "partial " : "") + "backup archive file '" + Project.CompleteBackupFolder + "\\" + ArchiveName.ToString() + "'.");
                            DoEvents();

                            System.Diagnostics.Stopwatch FirstFailure = new System.Diagnostics.Stopwatch();
                            for (int Tries = 0; ; Tries++)
                            {
                                try
                                {
                                    if (ZippyForm.IsLoggingAt(LogLevel.HeavyDebug))
                                    {
                                        string ZipLog = Path.GetDirectoryName(ZippyForm.LogFileName) + "\\DotNetZip.log";
                                        //System.IO.StreamWriter sw = new System.IO.StreamWriter(ZipLog, true);
                                        //sw.AutoFlush = true;
                                        DiagWriter = new DiagnosticStreamWriter(ZipLog);
                                        zip.StatusMessageTextWriter = DiagWriter;
                                    }

                                    zip.ParallelDeflateThreshold = -1;          // Workaround for parallel bug
                                    zip.ZipError += new EventHandler<ZipErrorEventArgs>(OnZipError);
                                    zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(OnZipSaveProgress);
                                    zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                                    using (NetworkConnection newself = new NetworkConnection(Project.CompleteBackupFolder, Project.BackupCredentials))
                                        zip.Save(Project.CompleteBackupFolder + "\\" + ArchiveName.ToString());
                                    if (ZipError != null) throw ZipError;
                                    Project.OnNewBackup(ArchiveName);
                                    break;
                                }
                                catch (CancelException ce) { throw ce; }
                                catch (System.IO.IOException iex)
                                {
                                    // Could mean a network is temporarily unavailble...retry for a few minutes...                                    
                                    if (Tries == 0) { FirstFailure.Reset(); FirstFailure.Start(); }
                                    if (FirstFailure.Elapsed.TotalMinutes > 15)
                                        throw new Exception(iex.Message + "  I/O Error while generating archive.", iex);                                    
                                    Progress.Pending_Label2_Text = "I/O issue while saving archive.  Try #" + (Tries+1).ToString() + ", " + FirstFailure.Elapsed.ToString() + " elapsed since issue appeared.";
                                    continue;
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(ex.Message + "  Error while generating archive.", ex);
                                }
                            }
                        }
                    }

                    // Update Backup Status file.                    
                    using (NetworkConnection newself = new NetworkConnection(Project.CompleteBackupFolder, Project.BackupCredentials))
                    {
                        string BackupStatusFile = Project.CompleteBackupFolder + "\\Backup_Status.xml";
                        BackupStatus bs = new BackupStatus();
                        try { bs = BackupStatus.Load(BackupStatusFile); }
                        catch (Exception) { }
                        bs.LastArchive = ArchiveName.ToString();
                        bs.LastBackup = DateTime.UtcNow;
                        if (!Continuation.Required) bs.LastCompletedBackup = DateTime.UtcNow;
                        if (!Continuation.Required) bs.LastScanRelativePath = ""; else bs.LastScanRelativePath = Continuation.LastRelativePath;
                        bs.Save(BackupStatusFile);
                    }

                    if (Continuation.Required)
                    {
                        ZippyForm.LogWriteLine(LogLevel.Information, "First archive in complete backup of '" + Project.Name + "' completed successfully (more required).");
                        ZippyForm.LogWriteLine(LogLevel.LightDebug, "Continuation marker at '" + Continuation.LastRelativePath + "'.");
                    }
                    else
                        ZippyForm.LogWriteLine(LogLevel.Information, "Complete backup of '" + Project.Name + "' completed successfully.");
                }
                catch (CancelException ce) { throw ce; }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + " (249)", ex);
                }
            }
            finally
            {
                if (Snapshot != null) { Snapshot.Dispose(); Snapshot = null; }
            }
            
            // Setup for next pass, if applicable...
            if (Continuation.Required)
            {
                RunIncremental(ref Continuation);
                ZippyForm.LogWriteLine(LogLevel.Information, "Complete backup of '" + Project.Name + "' completed successfully.");
            }
        }

        void RunIncremental()
        {
            Continuation NewCont = new Continuation();

            if (AutomaticBackup && Project.LastScanRelativePath != null && Project.LastScanRelativePath.Length > 0)
            {                
                NewCont.Required = true;
                NewCont.Starting = true;
                NewCont.LastRelativePath = Project.LastScanRelativePath;
            }

            RunIncremental(ref NewCont);
        }

        void RunIncremental(ref Continuation Continuation)
        {
            try
            {
                Progress.Pending_Label1_Text = "Performing incremental backup '" + Project.Name + "'...";
                DoEvents();

                do
                {
                    ArchiveFilename LatestBackup;
                    Manifest PrevManifest;
                    try
                    {
                        using (NetworkConnection newself = new NetworkConnection(Project.CompleteBackupFolder, Project.BackupCredentials))
                        {
                            // Locate latest backup (either complete or incremental.)
                            lock (Project.ArchiveFileList) LatestBackup = Project.ArchiveFileList.FindMostRecent();
                            if (LatestBackup == ArchiveFilename.MaxValue) throw new NoCompleteBackupException();

                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "Loading most recent manifest from '" + LatestBackup.ToString() + "'.");

                            PrevManifest = LatestBackup.LoadArchiveManifest(Project, true);
                        }
                    }
                    catch (CancelException ce) { throw ce; }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + "  Error while loading last backup for incremental update.", ex);
                    }

                    // For incremental backups, start with the pre-existing manifest.  Load the most recent manifest entries into the
                    // NameEntryMapping dictionary for use in identical file determination later.
                    Progress.Pending_Label1_Text = "Mapping incremental data for backup '" + Project.Name + "'...";
                    DoEvents();
                    SizeMapping.Clear();
                    BuildNameEntryMapping(PrevManifest.ArchiveRoot);

                    Initialize(BackupTypes.Incremental);
                    try
                    {
                        // Scan through folders and files and build up the new manifest.
                        using (ZipFile zip = new ZipFile())
                        {
                            if (!string.IsNullOrEmpty(Project.SafePassword.Password))
                            {
                                zip.Password = Project.SafePassword.Password;
                                zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                            }

                            int ChangedFiles;                            
                            using (NetworkConnection newself = new NetworkConnection(Project.SourceFolder, Project.SourceCredentials))
                            {
                                Continuation.StartPass(SourceRoot);
                                ChangedFiles = RunFolder(zip, Manifest.ArchiveRoot, new DirectoryInfo(SourceRoot), PrevManifest.ArchiveRoot,
                                    ref Continuation);
                            }

                            // We want to avoid creating an archive if nothing has changed.  But we will need to note the new backup date in the
                            // status file, below.
                            if (ChangedFiles > 0)
                            {
                                /** Generate backup archive file **/

                                using (MemoryStream ms = new MemoryStream())
                                {
                                    try
                                    {
                                        ZippyForm.LogWriteLine(LogLevel.LightDebug, "Adding manifest.xml stream to zip archive.");

                                        Manifest.ToXml(ms);
                                        ms.Seek(0, System.IO.SeekOrigin.Begin);
                                        zip.AddEntry("Manifest.xml", ms);
                                    }
                                    catch (CancelException ce) { throw ce; }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message + "  Error while adding manifest to archive.", ex);
                                    }

                                    Progress.Pending_Label2_Text = "Generating backup archive file...";
                                    ZippyForm.LogWriteLine(LogLevel.LightDebug, "Generating backup archive file '" + Project.CompleteBackupFolder + "\\" + ArchiveName.ToString() + "'.");
                                    DoEvents();

                                    try
                                    {
                                        if (ZippyForm.IsLoggingAt(LogLevel.HeavyDebug))
                                        {
                                            string ZipLog = Path.GetDirectoryName(ZippyForm.LogFileName) + "\\DotNetZip.log";
                                            //System.IO.StreamWriter sw = new System.IO.StreamWriter(ZipLog, true);
                                            //sw.AutoFlush = true;
                                            DiagWriter = new DiagnosticStreamWriter(ZipLog);
                                            zip.StatusMessageTextWriter = DiagWriter;
                                        }

                                        zip.ParallelDeflateThreshold = -1;          // Workaround for parallel bug
                                        zip.ZipError += new EventHandler<ZipErrorEventArgs>(OnZipError);
                                        zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(OnZipSaveProgress);
                                        zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                                        using (NetworkConnection newself = new NetworkConnection(Project.CompleteBackupFolder, Project.BackupCredentials))
                                            zip.Save(Project.CompleteBackupFolder + "\\" + ArchiveName.ToString());
                                        if (ZipError != null) throw ZipError;
                                    }
                                    catch (CancelException ce) { throw ce; }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message + "\nError while generating archive.\nComplete Backup Folder: " + Project.CompleteBackupFolder + "\nArchive Name: " + ArchiveName.ToString() + "\nFull Archive Filename: " + Project.CompleteBackupFolder + "\\" + ArchiveName.ToString(), ex);
                                    }

                                    try
                                    {
                                        LatestBackup = ArchiveName;
                                        Project.OnNewBackup(ArchiveName);
                                    }
                                    catch (CancelException ce) { throw ce; }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message + "\nError while post-processing archive.\nComplete Backup Folder: " + Project.CompleteBackupFolder + "\nArchive Name: " + ArchiveName.ToString() + "\nFull Archive Filename: " + Project.CompleteBackupFolder + "\\" + ArchiveName.ToString(), ex);
                                    }
                                }
                            }

                            // Update Backup Status file.                    
                            using (NetworkConnection newself = new NetworkConnection(Project.CompleteBackupFolder, Project.BackupCredentials))
                            {
                                string BackupStatusFile = Project.CompleteBackupFolder + "\\Backup_Status.xml";
                                BackupStatus bs = new BackupStatus();
                                try { bs = BackupStatus.Load(BackupStatusFile); }
                                catch (Exception) { }
                                bs.LastArchive = LatestBackup.ToString();
                                bs.LastBackup = DateTime.UtcNow;
                                if (!Continuation.Required) bs.LastCompletedBackup = DateTime.UtcNow;
                                if (!Continuation.Required) bs.LastScanRelativePath = ""; else bs.LastScanRelativePath = Continuation.LastRelativePath;
                                bs.Save(BackupStatusFile);
                            }
                        }

                        if (Continuation.Required)
                        {
                            ZippyForm.LogWriteLine(LogLevel.Information, "Individual incremental backup of '" + Project.Name + "' completed successfully (more required).");
                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "Continuation marker at '" + Continuation.LastRelativePath + "'.");
                        }
                    }
                    finally
                    {
                        if (Snapshot != null) { Snapshot.Dispose(); Snapshot = null; }
                    }
                }
                while (Continuation.Required && !Continuation.TimeLimitReached);
                
                ZippyForm.LogWriteLine(LogLevel.Information, "Incremental backup of '" + Project.Name + "' completed successfully.");
            }
            catch (CancelException ce) { throw ce; }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " (347)", ex);
            }
        }

        void BuildNameEntryMapping(Manifest.Folder FromFolder)
        {
            foreach (Manifest.Folder folder in FromFolder.Folders)
                BuildNameEntryMapping(folder);

            foreach (Manifest.File file in FromFolder.Files)
            {
                if (!SizeMapping.ContainsKey(file.Length))
                    SizeMapping[file.Length] = new List<ZippyBackup.Manifest.File>();
                SizeMapping[file.Length].Add(file);
            }
        }

        void OnZipError(object sender, ZipErrorEventArgs e)
        {
            try
            {
                switch (MessageBox.Show("Error archiving '" + NameMapping[e.FileName] + "': " + e.Exception, "Error", MessageBoxButtons.AbortRetryIgnore))
                {
                    case DialogResult.Abort: e.Cancel = true; ZipError = new CancelException(); return;
                    case DialogResult.Retry: e.CurrentEntry.ZipErrorAction = ZipErrorAction.Retry; return;
                    case DialogResult.Ignore: e.CurrentEntry.ZipErrorAction = ZipErrorAction.Skip; return;
                    default: throw new NotSupportedException();
                }
            }            
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\nError while handling zip error for file " + e.FileName, ex);
            }
        }                

        Exception ZipError;
        void OnZipSaveProgress(object sender, SaveProgressEventArgs e)
        {
            try
            {
                switch (e.EventType)
                {
                    case ZipProgressEventType.Saving_Started: Progress.Pending_Label3_Text = "Creating archive..."; break;
                    case ZipProgressEventType.Saving_BeforeWriteEntry:                                               
                        if (e.CurrentEntry.CompressionLevel != Ionic.Zlib.CompressionLevel.None)
                        {
                            Progress.Pending_Label3_Text = "Compressing:  " + NameMapping[e.CurrentEntry.FileName];
                            ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "Compressing '" + NameMapping[e.CurrentEntry.FileName] + "' into zip archive...");
                        }
                        else
                        {
                            Progress.Pending_Label3_Text = "Archiving:  " + NameMapping[e.CurrentEntry.FileName];
                            ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "Archiving '" + NameMapping[e.CurrentEntry.FileName] + "' into zip archive...");
                        }
                        if ((e.CurrentEntry.InputStream as PendingAlphaleonisFileStream) != null)
                        {
                            ((PendingAlphaleonisFileStream)e.CurrentEntry.InputStream).Open();
                        }
                        else if (e.CurrentEntry.InputStream != null)
                            ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "File input stream is a '" + e.CurrentEntry.InputStream.GetType().FullName + "' not an PendingAlphaleonisFileStream!");
                        else
                            ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "Stream is null!");
                        Progress.Pending_OverallProgressBar_Maximum = e.EntriesTotal;
                        Progress.Pending_OverallProgressBar_Minimum = 0;
                        Progress.Pending_OverallProgressBar_Value = e.EntriesSaved;
                        Progress.Pending_CurrentProgressBar_Maximum = 10000;
                        Progress.Pending_CurrentProgressBar_Value = 0;
                        break;
                    case ZipProgressEventType.Saving_AfterWriteEntry:
                        if ((e.CurrentEntry.InputStream as PendingAlphaleonisFileStream) != null)
                        {
                            ((PendingAlphaleonisFileStream)e.CurrentEntry.InputStream).Close();
                        }
                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "Finished adding '" + NameMapping[e.CurrentEntry.FileName] + "' to zip archive...");
                        if (e.CurrentEntry.CompressionLevel != Ionic.Zlib.CompressionLevel.None)
                            Progress.Pending_Label3_Text = "Compressing:  " + NameMapping[e.CurrentEntry.FileName];
                        else
                            Progress.Pending_Label3_Text = "Archiving:  " + NameMapping[e.CurrentEntry.FileName];                        
                        Progress.Pending_OverallProgressBar_Maximum = e.EntriesTotal;
                        Progress.Pending_OverallProgressBar_Minimum = 0;
                        Progress.Pending_OverallProgressBar_Value = e.EntriesSaved;
                        Progress.Pending_CurrentProgressBar_Maximum = 10000;
                        Progress.Pending_CurrentProgressBar_Value = 10000;
                        break;
                    case ZipProgressEventType.Saving_EntryBytesRead:
                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "Read " + e.BytesTransferred + " of " + e.TotalBytesToTransfer + " bytes of '" + NameMapping[e.CurrentEntry.FileName] + "'...");
                        Progress.Pending_CurrentProgressBar_Maximum = 10000;
                        Progress.Pending_CurrentProgressBar_Value = (int)(10000L * e.BytesTransferred / e.TotalBytesToTransfer);
                        break;
                    case ZipProgressEventType.Saving_Completed:
                        ZippyForm.LogWriteLine(LogLevel.LightDebug, "Archive completed.");
                        Progress.Pending_Label3_Text = "Archive complete.";
                        Progress.Pending_OverallProgressBar_Value = int.MaxValue;
                        Progress.Pending_CurrentProgressBar_Value = int.MaxValue;
                        break;
                    case ZipProgressEventType.Reading_Started:
                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "Read starting of '" + NameMapping[e.CurrentEntry.FileName] + "'...");
                        Progress.Pending_Label3_Text = "Reading:  " + NameMapping[e.CurrentEntry.FileName];
                        Progress.Pending_CurrentProgressBar_Maximum = 10000;
                        Progress.Pending_CurrentProgressBar_Value = 0;
                        break;
                    case ZipProgressEventType.Saving_BeforeRenameTempArchive:
                        ZippyForm.LogWriteLine(LogLevel.LightDebug, "Starting transfer of temporary archive to destination.");
                        Progress.Pending_Label3_Text = "Transferring temporary archive to destination.";
                        break;
                }

                DoEvents();
            }
            catch (CancelException ce) { ZipError = ce; e.Cancel = true; }
            catch (Exception ex)            
            {
                if (e.CurrentEntry != null && e.CurrentEntry.FileName != null)
                {
                    if (NameMapping.ContainsKey(e.CurrentEntry.FileName))
                        ZipError = new Exception(ex.Message + "\nError while processing compression status for file '" + e.CurrentEntry.FileName + "' from '" + NameMapping[e.CurrentEntry.FileName] + "'.", ex);
                    else
                        ZipError = new Exception(ex.Message + "\nError while processing compression status for file '" + e.CurrentEntry.FileName + "'.", ex);
                }
                else
                    ZipError = new Exception(ex.Message + "\nError while processing compression status.", ex);
                throw ZipError;
            }
        }

        bool NeedsUpdate(FileInfo fi, Manifest.File PrevVersion)
        {
            return (fi.LastWriteTimeUtc != PrevVersion.LastWriteTimeUtc
             || (ulong)fi.Length != PrevVersion.Length
             || fi.Attributes != PrevVersion.WindowsAttributes);
        }

        Crc32 CRCAlgorithm = new Crc32(0xEDB88320, 0xFFFFFFFF);
        UInt32 CalcCRC(string FullName)
        {
            // VSS requires the Alphaleonis stream class to work (.NET FileStream can't ordinarily open a kernel
            // path).  We'll only keep this one around long enough to grab the CRC and then close it.                    
            using (ZippyFileStream fs = new ZippyFileStream(FullName, Project.SourceCredentials))
            {
                fs.Open();
                UInt32 CRC = CRCAlgorithm.Compute(fs);
                fs.Close();
                return CRC;
            }
        }

        /// <summary>
        /// Performs the IdenticalFileReduction() step of RunFolder, which is action B, case 2.
        /// IdenticalFileReduction() first determines whether an identical file exists, and
        /// validates it.  IdenticalFileReduction() also adds an entry for the file to facilitate
        /// future file reduction determinations.
        /// </summary>
        /// <param name="NewEntry">The new file under consideration for duplication.</param>
        /// <returns>True if an already-archived identical file was found and NewEntry
        /// was updated.  False if no match was found and the file must be archived.</returns>
        bool IdenticalFileReduction(FileInfo fi, ref Manifest.File NewEntry)
        {
            string Name = NewEntry.Name;
            if (!SizeMapping.ContainsKey(NewEntry.Length))
            {
                // There have been no files with this length, so there are certainly no duplicates.  Make this
                // the first entry at this length.
                SizeMapping[NewEntry.Length] = new List<ZippyBackup.Manifest.File>();
                SizeMapping[NewEntry.Length].Add(NewEntry);
                return false;
            }

            // Move through the potential list in reverse order.  This will generally correspond
            // to "newest files first", since older versions of the files would have come from the
            // previous manifest and therefore were added during initialization instead of scanning.
            // This isn't strictly necessary and doesn't strictly lead to the newest files, but
            // will help.  Aside from our best chance of a match being the most recent version of
            // the filename and reducing scan time, this will create a preference to reference the latest
            // archives.
            List<Manifest.File> PotentialList = SizeMapping[NewEntry.Length];
            string CurrentArchiveName = ArchiveName.ToString();
            for (int iPot = PotentialList.Count - 1; iPot >= 0; iPot--)
            {
                Manifest.File PrevEntry = PotentialList[iPot];

                // Time to dig in and check if their contents are identical.  This is the computationally 
                // expensive step, so we avoid repeating it by storing the CRCs.

                if (!NewEntry.ValidCRC32)
                {
                    NewEntry.CRC32 = CalcCRC(fi.FullName);
                    NewEntry.ValidCRC32 = true;
                }

                if (!PrevEntry.ValidCRC32)
                {
                    if (PrevEntry.ArchiveFile == null || PrevEntry.ArchiveFile.Equals(CurrentArchiveName))
                    {
                        // The potential file is found within the same archive, so it is also a new file.  It hasn't
                        // been archived yet, and must exist on the file system.
                        PrevEntry.CRC32 = CalcCRC(Utility.EnsureTrailingSlash(SourceRoot) + PrevEntry.RelativePath);
                        PrevEntry.ValidCRC32 = true;
                    }
                    else
                    {
                        // The potential file is found in a previous archive and we don't have the CRC for it.  The quickest
                        // way to calculate the CRC is to grab it from the ZIP archive directory (we are using the same CRC 
                        // polynomial.)  However, we do want to store the CRC's in our manifest as well so that future access
                        // can be made from just the manifest and does not require opening many old archive files.
                        
                        string FullPrevArchiveName = Project.CompleteBackupFolder + "\\" + PrevEntry.ArchiveFile;
                        try
                        {
                            using (ZipFile zip = ZipFile.Read(FullPrevArchiveName))
                            {
                                ZipEntry ze;
                                ze = zip[PrevEntry.PathInArchive.Replace('\\', '/')];
                                PrevEntry.CRC32 = (UInt32)ze.Crc;
                                PrevEntry.ValidCRC32 = true;
                            }
                        }
                        catch (CancelException ce) { throw ce; }
                        catch (Exception)
                        {
                            ZippyForm.LogWriteLine(LogLevel.Warnings, "\t\tWarning: Invalid reference found to archive '" + PrevEntry.ArchiveFile + "' file '" + PrevEntry.PathInArchive + "' during identical file determination.  Excluding this file as a potential identical copy and continuing.");                                
                            // Since the reference is to an invalid or inaccessible file, let's avoid hitting it again.
                            PotentialList.RemoveAt(iPot);
                            if (PotentialList.Count == 0) break;            // We've eliminated the entire potential list.                            
                            continue;
                        }
                    }
                }

                if (NewEntry.CRC32 == PrevEntry.CRC32)
                {
                    // We have found an identical file pair.  Perform reduction.
                    NewEntry.ArchiveFile = PrevEntry.ArchiveFile;
                    NewEntry.PathInArchive = PrevEntry.PathInArchive;
                    NewEntry.DuplicateFile = true;
                    ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t\tIdentical files detected:  '" + NewEntry.RelativePath + "' is a match to '" + PrevEntry.RelativePath + "' with CRC-32 of 0x" + NewEntry.CRC32.ToString("X8") + "'.");
                    return true;
                }
            }

            // There was no identical file found, so we need to add this new entry to the future potential list for this file length.
            PotentialList.Add(NewEntry);
            return false;
        }

        bool IsExcludedFolderPattern(string FolderPathInArchive, string FolderName)
        {
            foreach (Utility.Wildcard search in Wildcards_ExcludedFolderPatterns)
                if (search.IsMatch(FolderName)) 
                    return true;
            return false;
        }

        /// <summary>
        /// The Continuation class stores state about a scan+archive process that could not cover all
        /// files - usually due to the size limit of an individual archive being exceeded.  This addresses
        /// a subtle problem with continuation where a certain large file is frequently changing.  For example,
        /// if ZippyBackup is unable to finish backing up folder XYZ in one archive, then it will setup
        /// a continuation.  It could just start its scan from the beginning, and will notice that most
        /// files haven't changed since their backup.  However, if one large file changes often, the
        /// scan will continually find it updated again and back it up again, never reaching any later files.  
        /// In order to address this, a continuation manages the state in order to start from where the
        /// last archive left off.
        /// </summary>
        class Continuation
        {            
            /// <summary>
            /// Continuations can be initiated for multiple reasons.  Some behaviors only occur when
            /// a time-limit continuation has been reached, and it is possible that a continuation
            /// for archive size limit will also run into a time limit or vice versa.  We mark the
            /// time-limit case to retain the related behaviors.
            /// </summary>
            public bool TimeLimitReached = false;

            /// <summary>
            /// Required is a marker that a new Continuation is needed.  A continuation is setup
            /// when we exceed a limit for a single archive file (usually size).
            /// </summary>
            public bool Required = false;

            /// <summary>
            /// Indicates that RunFolder is presently searching for the location where the previous
            /// scan terminated before beginning its new scan.
            /// </summary>
            public bool Starting = false;

            /// <summary>
            /// Contains the location at which the previous archive's scan terminated.  This is
            /// inclusive, so the next scan should begin immediately after locating this file.
            /// </summary>
            public string LastRelativePath;

            /// <summary>
            /// There is a special case: if the LastRelativePath is a file or folder that no longer
            /// exists, then it will not show up in the scan sequence.  We could try to estimate
            /// from alphabetic order and such, but instead we will just start from scratch in
            /// such a case.
            /// </summary>
            public void StartPass(string SourceRoot)
            {
                Required = false;
                if (!Starting) return;
                string NewTarget = Utility.StripTrailingSlash(SourceRoot) + "\\" + Utility.StripLeadingSlash(LastRelativePath);
                if (!File.Exists(NewTarget)) Starting = false;
            }
        }

        /// <summary>
        /// RunFolder() provides the core of ZippyBackup's backup functionality.  It is used for both complete
        /// and incremental backups (complete backups will provide a null PrevFolder argument).  It recursively
        /// scans through all files and folders within the specified directory, given by the di argument.
        /// It generates the manifest and "adds" files to the zip file - although files are not actually
        /// compressed and written to the zip file until zip.Save() is called.
        /// 
        /// Precondition: Impersonation necessary for access to the SOURCE folder should be applied before
        /// calling.
        /// 
        /// Postcondition: Folder may develop shallow references to PrevFolder or its heirarchy, although
        /// no changes will be made herein.  Changes made to the Manifest.Folder content after RunFolder
        /// could alter the shared state of the previous manifest.  Either no changes should be made to
        /// the manifest after RunFolder(), or the previous manifest should be considered invalidated after
        /// the call.
        /// </summary>
        /// <param name="zip"></param>
        /// <param name="Folder"></param>
        /// <param name="di"></param>
        /// <param name="PrevFolder"></param>
        /// <param name="MoreToDo">If set, indicates that RunFolder() reached or exceeded a constraint on the
        /// archive, and that another pass will be needed to complete the task.</param>
        /// <returns></returns>
        int RunFolder(ZipFile zip, Manifest.Folder Folder, DirectoryInfo di, Manifest.Folder PrevFolder, ref Continuation Continuation)
        {
            // For "Complete" backups PrevFolder is null.
            // Incremental Actions:
            //  A. Add to new manifest with file.
            //      1. When file is newer in the file system.
            //      2. When file is older in the file system.
            //      3. When file does not exist in the old manifest.
            //      4. When the old external reference is not valid.
            //  B. Add to new manifest with the same external reference and file attributes.
            //      1. When file is identical in both AND old external reference is valid.
            //      2. When file has identical name and CRC AND old external reference is valid.
            //  C. Omit from manifest.
            //      1. When file does not exist in file system, regardless of previous existence.
            //      2. When file's extension matches the excluded list (including exceeding size limit).
            //      3. When the filename matches Manifest.xml in the archive's root folder.
            //
            // Action C is handled by iterating only through files that exist on the file system.
            //
            // Action B case 2 includes files that are not necessarily located in the same path but have the
            // same name and CRC-32.  This determination is called the identical file reduction, and is a
            // compression step that is especially helpful for files that have been copied or moved.
            //
            // When a Continuation is in progress (either starting or past the archive limit), most folders
            // are not scanned but their previous manifest content is copied directly.  During the Continuation
            // starting phase, the folder containing the previous marker point is scanned until the marker is
            // located, and then the Continuation.Starting state changes.

            try
            {
                int ChangedFiles = 0;

                if (PrevFolder != null)
                    ZippyForm.LogWriteLine(LogLevel.MediumDebug, "Scanning folder '" + di.FullName + "' with comparison...");
                else
                    ZippyForm.LogWriteLine(LogLevel.MediumDebug, "Scanning folder '" + di.FullName + "'...");
                
                string RelativeFolderPath = Utility.GetRelativePath(SourceRoot, di.FullName);

                if (Continuation.Starting)
                    Progress.Pending_Label2_Text = "Continuing scan in folder: " + RelativeFolderPath;
                else if (PrevFolder != null)
                    Progress.Pending_Label2_Text = "Comparing folder: " + RelativeFolderPath;
                else
                    Progress.Pending_Label2_Text = "Scanning folder: " + RelativeFolderPath;                

                /** Check excluded folder list **/
                                
                foreach (string ExcludedRelativePath in Project.ExcludeSubfolders)
                    if (RelativeFolderPath.ToLower() == ExcludedRelativePath.ToLower()) return 0;

                ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\tNot found on folder exclusion list.");                
                
                /** Check if we are starting a continuation and if this folder is before or after the
                 *  continuation point.  Continuation.Required is cleared by the StartPass() call made
                 *  at the beginning of the continuation backup.  It is set herein if the remainder of
                 *  the "scan" is a continuation.  So in this context, .Starting indicates that we
                 *  are starting up a continuation and searching for the continuation marker and
                 *  .Required means that we have decided we need a continuation for the remainder
                 *  of this backup run.  **/

                bool SkipForContinuation = Continuation.Required;
                if (Continuation.Starting)
                {
                    //ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\tChecking '" + RelativeFolderPath + "' for continuation marker '" + Continuation.LastRelativePath + "'...");
                    if (!Utility.IsContainedIn(RelativeFolderPath, Continuation.LastRelativePath)) SkipForContinuation = true;
                }

                // Check if this folder itself is the continuation marker (marker can be either file or folder).

                if (Continuation.Starting && RelativeFolderPath.Equals(Continuation.LastRelativePath, System.StringComparison.OrdinalIgnoreCase))
                {
                    // Target found, begin the scan from here - but the continuation marker is inclusive to the previous scan,
                    // so skip over this last folder before starting the scan (hence the boolean SkipForContinuation captured
                    // before this test).
                    Continuation.Starting = false;
                    ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\tContinuation folder marker '" + Continuation.LastRelativePath + "' triggered.");
                }

                if (SkipForContinuation)
                {
                    // Transfer previous manifest data in, but don't spend time scanning right now...
                    if (PrevFolder != null)
                    {
                        foreach (Manifest.Folder PrevSubfolder in PrevFolder.Folders) Folder.Folders.Add(PrevSubfolder);
                        foreach (Manifest.File PrevFile in PrevFolder.Files) Folder.Files.Add(PrevFile);
                    }
                    ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\tFolder does not contain continuation marker, transferring manifest only.");
                    return 0;
                }

                /** Scan for any subfolders that no longer exist **/

                DirectoryInfo[] ExistingFolders = di.GetDirectories();

                try
                {
                    if (!Continuation.Required && PrevFolder != null)
                    {
                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\tScanning for removed subfolders...");                        
                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t\tCollecting existing folder map...");

                        // First, convert the list of subfolders in the folder (on file system) to their relative pathnames so
                        // that this doesn't have to be calculated more than once...                        
                        Dictionary<string, DirectoryInfo> ExistingMap = new Dictionary<string, DirectoryInfo>(ExistingFolders.Length);
                        for (int ii = 0; ii < ExistingFolders.Length; ii++)
                        {
                            if (SymbolicLink.IsLink(ExistingFolders[ii].FullName)) continue;
                            ExistingMap.Add(Utility.GetRelativePath(SourceRoot, ExistingFolders[ii].FullName).ToLowerInvariant(), ExistingFolders[ii]);
                        }

                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t\tComparing previous and existing folders...");

                        foreach (Manifest.Folder PrevSubfolder in PrevFolder.Folders)
                        {
                            DoEvents();

                            if (ExistingMap.ContainsKey(PrevSubfolder.RelativePath.ToLowerInvariant())) continue;        // Check if present in both.

                            // We have case C1.  The only impact this has is in adding a count to the number of
                            // changed files, and making sure it didn't contain the continuation marker.
                            ChangedFiles++;

                            if (Continuation.Starting && Utility.IsContainedIn(PrevSubfolder.RelativePath, Continuation.LastRelativePath))
                            {
                                // Target found, begin the scan from here.  Since the marker's folder was deleted or moved, we may have
                                // overlap in the scan but that won't hurt.
                                Continuation.Starting = false;
                                ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t\tFolder '" + PrevSubfolder.RelativePath + "' containing continuation marker '" + Continuation.LastRelativePath + "' was not found on file system, triggering continuation.");
                            }
                        }                        
                    }
                }
                catch (CancelException ce) { throw ce; }
                catch (Exception exc) { throw new Exception(exc.Message + " (672:13)", exc); }                

                /** Scan all subfolders within the folder **/

                // This step can recurse into RunFolder() for subfolders and can potentially generate a 
                // new continuation (by setting Continuation.Required).  When this happens, we must
                // finish out this step on this level because we need to complete the manifest for
                // this folder at this level.  Therefore, we don't react to the Continuation.Required
                // here.

                try
                {
                    ZippyForm.LogWriteLine(LogLevel.MediumDebug, "\tScanning subfolders...");

                    foreach (DirectoryInfo sub in ExistingFolders)
                    {
                        DoEvents();

                        if (SymbolicLink.IsLink(sub.FullName)) continue;

                        string RelativePath = Utility.GetRelativePath(SourceRoot, sub.FullName);

                        if (IsExcludedFolderPattern(RelativePath, sub.Name))
                        {
                            ZippyForm.LogWriteLine(LogLevel.MediumDebug, "\tSkipping folder '" + sub.FullName + "' because it matched the excluded folder patterns list.");
                            continue;
                        }

                        Manifest.Folder Subfolder = new Manifest.Folder(RelativePath, sub);
                        Folder.Folders.Add(Subfolder);

                        ZippyForm.LogWriteLine(LogLevel.MediumDebug, "\tScanning subfolder '" + RelativePath + "'...");

                        if (PrevFolder != null)
                        {                            
                            /** Try to locate the matching directory record in the previous manifest in order to 
                             *  provide it to RunFolder **/
                            bool Done = false;
                            foreach (Manifest.Folder PrevSubfolder in PrevFolder.Folders)
                            {
                                if (PrevSubfolder.RelativePath.ToLowerInvariant() == RelativePath.ToLowerInvariant())
                                {
                                    Done = true;
                                    ChangedFiles += RunFolder(zip, Subfolder, sub, PrevSubfolder, ref Continuation);
                                    break;
                                }
                            }
                            if (Done) continue;
                        }

                        /** Either the record in the previous manifest was not found or we aren't working off of a
                         *  previous manifest.  Proceed to scan the subfolder with no history. **/

                        ChangedFiles += RunFolder(zip, Subfolder, sub, null, ref Continuation);
                    }
                }
                catch (CancelException ce) { throw ce; }
                catch (Exception exc) { throw new Exception(exc.Message + " (531:13)", exc); }                                

                /** Check if a continuation was initiated while we were going through subfolders.  If so, let's grab old manifest data and run **/

                if (Continuation.Required)
                {
                    // Transfer previous manifest data in (for files), but don't spend time scanning right now...
                    if (PrevFolder != null)
                    {                        
                        foreach (Manifest.File PrevFile in PrevFolder.Files) Folder.Files.Add(PrevFile);
                    }
                    ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\tFolder does not contain continuation marker, transferring file manifest only.");
                    return ChangedFiles;
                }

                /** Scan for any files that no longer exist **/                

                FileInfo[] ExistingFiles = di.GetFiles();

                try
                {
                    if (PrevFolder != null)
                    {
                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\tScanning for removed files...");
                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t\tCollecting existing file map...");

                        // First, convert the list of files in the folder (on file system) to their relative pathnames so
                        // that this doesn't have to be calculated more than once...                        
                        Dictionary<string, FileInfo> ExistingMap = new Dictionary<string, FileInfo>(ExistingFiles.Length);
                        for (int ii = 0; ii < ExistingFiles.Length; ii++)                           
                            ExistingMap.Add(Utility.GetRelativePath(SourceRoot, ExistingFiles[ii].FullName).ToLowerInvariant(), ExistingFiles[ii]);                        

                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t\tComparing previous and existing files...");

                        foreach (Manifest.File PrevFile in PrevFolder.Files)
                        {
                            DoEvents();

                            if (ExistingMap.ContainsKey(PrevFile.RelativePath.ToLowerInvariant())) continue;        // Check if present in both.                            

                            // We have case C1.  The only impact this has is in adding a count to the number of
                            // changed files, and making sure it wasn't the continuation marker.
                            ChangedFiles++;

                            if (Continuation.Starting
                             && PrevFile.RelativePath.Equals(Continuation.LastRelativePath, System.StringComparison.OrdinalIgnoreCase))
                            {
                                Continuation.Starting = false;
                                ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t\tContinuation marker file '" + Continuation.LastRelativePath + "' was not found on file system, triggering continuation.");
                            }
                        }
                    }
                }
                catch (CancelException ce) { throw ce; }
                catch (Exception exc) { throw new Exception(exc.Message + " (917:82)", exc); }

                /** Scan all files within the folder **/

                ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\tScanning files within folder...");

                foreach (FileInfo fi in ExistingFiles)
                {
                    DoEvents();

                    string RelativePath = Utility.GetRelativePath(SourceRoot, fi.FullName);

                    bool ContinuationOverFile = Continuation.Required || Continuation.Starting;
                    if (Continuation.Starting
                        && RelativePath.Equals(Continuation.LastRelativePath, System.StringComparison.OrdinalIgnoreCase))
                    {
                        // Target found, begin the scan from here - but the continuation marker is inclusive to the previous scan,
                        // so skip over this last file before starting the scan (hence the boolean ContinuationOverFile captured
                        // before this test).
                        Continuation.Starting = false;
                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\tContinuation file marker '" + Continuation.LastRelativePath + "' triggered.");
                    }
                    if (ContinuationOverFile)
                    {
                        if (PrevFolder != null)
                        {
                            foreach (Manifest.File PrevFile in PrevFolder.Files)
                            {
                                if (PrevFile.RelativePath.Equals(RelativePath, StringComparison.OrdinalIgnoreCase))
                                {
                                    Folder.Files.Add(PrevFile);
                                    ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t[Continuation] Transfering file manifest for '" + fi.Name + "'...");
                                    break;
                                }
                            }
                        }                        
                        continue;
                    }

                    ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\tScanning file '" + fi.Name + "'...");
                     
                    bool ExcludedFile = false;
                    if (Project.ExcludeFileSize < long.MaxValue && fi.Length > Project.ExcludeFileSize) continue;   // Action C2.
                    foreach (string Excluded in Project.ExcludeExtensions)
                        if (Excluded.Equals(fi.Extension, StringComparison.OrdinalIgnoreCase)) { ExcludedFile = true; break; }
                    foreach (string Excluded in Project.ExcludeFiles)
                        if (Excluded.Equals(RelativePath, StringComparison.OrdinalIgnoreCase)) { ExcludedFile = true; break; }
                    foreach (string Excluded in User_Interface.ZippyForm.MainList.ExcludeExtensions)
                        if (Excluded.Equals(fi.Extension, StringComparison.OrdinalIgnoreCase)) { ExcludedFile = true; break; }
					if (fi.Name.StartsWith("NTUSER")) { ExcludedFile = true; break; }

                    if (ExcludedFile) continue;                         // Action C2.                    

                    ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t\tNo file exclusions.");

                    try
                    {
                        // At this point, we are either performing action A or B.  Both of these require a manifest entry.                        

                        if (RelativePath.ToLower() == "manifest.xml")
                        {
                            ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t\tSpecial manifest file identified.");
                            continue;  // Action C3.
                        }

                        Manifest.File File = new Manifest.File(RelativePath, fi);
                        Folder.Files.Add(File);

                        if (PrevFolder != null)
                        {
                            bool Skip = false;
                            foreach (Manifest.File PrevFile in PrevFolder.Files)
                            {
                                if (PrevFile.RelativePath.ToLowerInvariant() == RelativePath.ToLowerInvariant())
                                {
                                    Skip = !NeedsUpdate(fi, PrevFile);

                                    File.ArchiveFile = PrevFile.ArchiveFile;
                                    File.PathInArchive = PrevFile.PathInArchive;

                                    // If Skip is true, we are nearly to action B, but validate the old reference...
                                    if (Skip && !System.IO.File.Exists(Project.CompleteBackupFolder + "\\" + File.ArchiveFile))
                                    {
                                        // The old reference was not found.  Perform action A from case 4 instead.
                                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t\tPrevious file reference was not found or invalid.");
                                        Skip = false;
                                    }
                                    // else, perform action B case 1.
                                    break;
                                }
                            }
                            if (Skip)
                            {
                                ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t\tNo change detected.");
                                continue;         // Action B case 1.
                            }
                        }                                                

                        // Check and handle action B case 2, the identical file reduction.
                        try
                        {
                            if (IdenticalFileReduction(fi, ref File)) { ChangedFiles++; continue; }
                        }
                        catch (CancelException ce) { throw ce; }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message + "\nError while checking identical file reduction for file '" + fi.FullName + "'.", ex);
                        }
                        
                        // Perform action A.
                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t\tArchiving '" + fi.FullName + "'.");

                        bool PrecompressedFile = false;
                        foreach (string Compressed in User_Interface.ZippyForm.MainList.CompressedExtensions)
                            if (Compressed.ToLowerInvariant() == fi.Extension.ToLowerInvariant()) { PrecompressedFile = true; break; }
                        if (!PrecompressedFile)
                            foreach (string Compressed in Globals.BuiltinCompressedExtensions)
                                if (Compressed.ToLowerInvariant() == fi.Extension.ToLowerInvariant()) { PrecompressedFile = true; break; }

                        File.ArchiveFile = ArchiveName.ToString();
                        if (!string.IsNullOrEmpty(Project.SafePassword.Password))
                            File.PathInArchive = "Content\\File" + iNextFilename.ToString("D6");
                        else
                            File.PathInArchive = RelativePath;
                        iNextFilename++;
                        ZipEntry NewZipEntry;

                        // We need to provide a stream so that we can use Alphaleonis to open it, otherwise VSS won't work (the
                        // .NET FileStream can't ordinarily open a kernel path).  We could open it and just leave it open from
                        // here, but that's an awful lot of open files.  Instead, we use a Pending stream that gets opened
                        // just before working on the zip entry and closed after, using the zip event processing.  See 
                        // PendingAlphaleonisFileStream.  Descended from that, we use ZippyFileStream to include credential
                        // access.
                        ZippyFileStream fs = new ZippyFileStream(fi.FullName, Project.SourceCredentials);
                        // PendingAlphaleonisFileStream fs = new PendingAlphaleonisFileStream(fi.FullName);
                        NewZipEntry = zip.AddEntry(fi.FullName, fs);

                        NewZipEntry.FileName = File.PathInArchive;          // Rename the file as it enters the archive.                        
                        if (PrecompressedFile) NewZipEntry.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
                        string PathInArchive = File.PathInArchive.Replace("\\", "/");
                        try
                        {
                            NameMapping.Add(PathInArchive, fi.FullName);
                        }
                        catch (Exception ex) { throw new Exception(ex.Message + "\nWhile attempting to map '" + PathInArchive + "' to '" + fi.FullName + "'.", ex); }
                        ChangedFiles++;
                        ArchiveSize += (long)File.Length;
                        if (ArchiveSize >= User_Interface.ZippyForm.MainList.ConstrainArchiveSize)
                        {
                            if (!Continuation.Required)
                            {
                                Continuation.Required = true;
                                Continuation.Starting = true;
                                Continuation.LastRelativePath = RelativePath;
                                ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "New continuation marker set to '" + RelativePath + "'.");
                                ZippyForm.LogWriteLine(LogLevel.Information, "Scan terminating early due to archive size limit.");

                                // We don't break out as we start a continuation because we have to grab the old manifest data for the rest
                                // of the files.  When we iterate on this loop, the next file will have the ContinuationOverFile flag set
                                // and this will lead to copying the old manifest and a continue statement that will trigger for all
                                // remaining files within this folder.
                            }                            
                        }
                    }
                    catch (CancelException ce) { throw ce; }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + "\nError while processing file '" + fi.FullName + "'.", ex);
                    }
                }

                ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\tSubfolder scan complete for '" + di.FullName + "'.");

                if (AutomaticBackup && (Environment.TickCount - StartTick) > MaxAutomaticDurationInTicks)
                {
                    Continuation.TimeLimitReached = true;
                    if (!Continuation.Required)
                    {
                        Continuation.Required = true;
                        Continuation.Starting = true;
                        Continuation.LastRelativePath = RelativeFolderPath;
                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "New continuation marker set to '" + RelativeFolderPath + "'.");
                        ZippyForm.LogWriteLine(LogLevel.Information, "Scan terminating early due to individual scan time limit.");
                    }
                }

                return ChangedFiles;
            }
            catch (CancelException ce) { throw ce; }
            catch (ExceptionWithDetail ex) { throw ex; }
            catch (Exception ex)
            {
                throw new ExceptionWithDetail(ex.Message + "\nError while processing folder '" + di.FullName + "'.  "
                    + "\n\nIf this subfolder can be excluded from your backup, see the Manage tab|Edit...|Exclude Subfolders option to disable it.", ex);
            }
        }        
    }    
}
