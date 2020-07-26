/** VerificationRun.cs
 *  Copyright (C) 2012-2016 by Wiley Black.  All rights reserved.
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
using System.Diagnostics;
using Microsoft.Win32;
using Ionic.Zip;
using ZippyBackup.User_Interface;
using Alphaleonis.Win32.Filesystem;         // Replaces System.IO entities such as FileInfo with more advanced versions.
using ZippyBackup.IO;
using ZippyBackup.Diagnostics;
using FileAttributes = System.IO.FileAttributes;
using FileMode = System.IO.FileMode;
using FileAccess = System.IO.FileAccess;
using FileShare = System.IO.FileShare;

namespace ZippyBackup
{
    public class VerificationRun
    {
        BackupProject Project;
        ProgressForm Progress;
        long TotalBytes = 0;
        long BytesCompleted = 0;

        Exception ZipError;

        DirectoryInfo TempRoot;

        /// <summary>
        /// AutomaticVerify is true if this verification was initiated by the automatic 
        /// scheduler.  These runs behave similar to user-initiated verifications, except 
        /// that they have a time-limit for their operation.
        /// </summary>
        bool AutomaticVerify = false;

        /// <summary>
        /// Indicates the time when the current BackupRun was initiated.
        /// </summary>
        int StartTick = Environment.TickCount;

        const int MaxMappingDurationInTicks = 60 /*seconds*/ * 1000 /*ticks/sec*/;
        const int MaxAutomaticDurationInTicks = 30 /*minutes*/ * 60 /*seconds/minute*/ * 1000 /*ticks/second*/;

        /// <summary>
        /// The Continuation class stores state about a verification process that could not cover all
        /// files - usually due to a time limit for the verification.
        /// </summary>
        class Continuation
        {
            /// <summary>
            /// Required is a marker that a new Continuation is needed.  A continuation is setup
            /// when we exceed a time limit.
            /// </summary>
            public bool Required = false;

            /// <summary>
            /// Indicates that the verification is presently searching for the location where the previous
            /// scan terminated before beginning its new scan.
            /// </summary>
            public bool Starting = false;

            /// <summary>
            /// Contains the location at which the previous verification terminated.  This is
            /// inclusive, so the next verification should begin immediately after locating this file.
            /// </summary>
            public string LastRelativePath;

            public void StartPass()
            {
                Required = false;
                if (!Starting) return;
            }
        }

        public VerificationRun(BackupProject Project)
        {
            this.Project = Project;
        }

        void MapManifest(Manifest.Folder Folder, ref List<Manifest.File> Results, ref List<ArchiveFilename> ArchivesRequired, ref long TotalSize, ref Continuation Continuation, ref int MapStartTick)
        {
            if (Continuation.Required) return;

            if (Continuation.Starting)
            {
                if (!Utility.IsContainedIn(Folder.RelativePath, Continuation.LastRelativePath)) return;
            }                        

            foreach (Manifest.Folder Subfolder in Folder.Folders)
            {
                ZippyForm.LogWriteLine(LogLevel.MediumDebug, "\tMapping manifest folder '" + Subfolder.RelativePath + "'...");
                MapManifest(Subfolder, ref Results, ref ArchivesRequired, ref TotalSize, ref Continuation, ref MapStartTick);
                DoEvents();
                if (Continuation.Required) return;
            }

            long FilesAdded = 0, ArchivesAdded = 0;
            foreach (Manifest.File File in Folder.Files) 
            {
                if (Continuation.Starting)
                {
                    if (File.RelativePath.Equals(Continuation.LastRelativePath, System.StringComparison.OrdinalIgnoreCase))
                    {
                        ZippyForm.LogWriteLine(LogLevel.LightDebug, "Found continuation marker '" + Continuation.LastRelativePath + ".");
                        Continuation.Starting = false;
                        MapStartTick = Environment.TickCount;
                    }
                    continue;               // The marker file itself was included in the previous verification run, not this one.
                }                
                Results.Add(File);
                TotalSize += (long)File.Length;
                FilesAdded++;
                ArchiveFilename Archive = ArchiveFilename.Parse(File.ArchiveFile);
                if (!ArchivesRequired.Contains(Archive)) { ArchivesRequired.Add(Archive); ArchivesAdded++; }

                if (AutomaticVerify && (Environment.TickCount - MapStartTick) > MaxMappingDurationInTicks)
                {
                    Continuation.Required = true;
                    Continuation.LastRelativePath = File.RelativePath;
                    ZippyForm.LogWriteLine(LogLevel.MediumDebug, "\tMapping time limit exceeded.  Marking mapping continuation at '" + Continuation.LastRelativePath + "'.");
                    return;
                }
            }
            ZippyForm.LogWriteLine(LogLevel.MediumDebug, "\t\tFound " + FilesAdded + " files and " + ArchivesAdded + " new archives to be verified.");
        }

        public void Run(bool Automatic)
        {
            AutomaticVerify = Automatic;

            Continuation Continuation = new Continuation();

            if (Automatic && Project.LastVerifyRelativePath != null && Project.LastVerifyRelativePath.Length > 0)
            {
                Continuation.Starting = true;
                Continuation.LastRelativePath = Project.LastVerifyRelativePath;
                Continuation.StartPass();
            }

            Run(ref Continuation);
        }

        void Run(ref Continuation Continuation)
        {
            // Setup progress UI...
            Progress = new ProgressForm();            
            Progress.Text = "Verifying archived files for project '" + Project.Name + "'.";
            Progress.OverallProgressBar.Maximum = 10000;
            Progress.OverallProgressBar.Minimum = 0;
            Progress.OverallProgressBar.Value = 0;
            Progress.CancelPrompt = "Are you sure you wish to cancel this verification operation?";
            Progress.Show();

            try
            {
                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;

                ZippyForm.LogWriteLine(LogLevel.Information, "Starting verification of backup '" + Project.Name + "'.");

                string UserTempFolder = Path.GetTempPath();
                TempRoot = new DirectoryInfo(Utility.StripTrailingSlash(UserTempFolder) + "\\ZippyBackup_Verification");
                Directory.CreateDirectory(TempRoot.FullName);

                try
                {
                    Progress.label1.Text = "Retrieving latest archive manifest...";
                    DoEvents();
                    ArchiveFilename LatestBackup;
                    Manifest LatestManifest;
                    try
                    {
                        using (Impersonator newself = new Impersonator(Project.BackupCredentials))
                        {
                            // Locate latest backup (either complete or incremental.)
                            lock (Project.ArchiveFileList) LatestBackup = Project.ArchiveFileList.FindMostRecent();
                            if (LatestBackup == ArchiveFilename.MaxValue) throw new NoCompleteBackupException();

                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "Loading most recent manifest from '" + LatestBackup.ToString() + "' of project '" + Project.Name + "'.");

                            LatestManifest = LatestBackup.LoadArchiveManifest(Project, true);
                        }
                    }
                    catch (CancelException ce) { throw ce; }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + "  Error while loading last backup for incremental update.", ex);
                    }

                    int StartTick = Environment.TickCount;

                    if (Continuation.Starting)
                    {
                        ZippyForm.LogWriteLine(LogLevel.LightDebug, "Continuing from previous run.  Searching for starting point '" + Continuation.LastRelativePath + "'.");
                    }

                    Continuation MapContinuation = new Continuation();                    
                    MapContinuation.Starting = Continuation.Starting;
                    MapContinuation.LastRelativePath = Continuation.LastRelativePath;

                    Continuation.Required = false;
                    while (!Continuation.Required)
                    {
                        /** Operate on a single zip file at a time.  This requires planning which files are
                         *  coming from where.  Also, we count how many total bytes we'll be extracting.  **/
                        Progress.label1.Text = "Identifying required archive files...";
                        DoEvents();
                        List<Manifest.File> FileList = new List<Manifest.File>();
                        List<ArchiveFilename> ArchivesRequired = new List<ArchiveFilename>();
                        TotalBytes = 0;

                        /** For extremely large archives, it can happen that even just scanning through the manifest can be very time consuming.  We don't want
                         *  to spend too much time on this step, so we have a time limit for coming up with file/archive list.  But we also have a time limit
                         *  for the overall verify operation.  We use a pattern of spending N minutes coming up with filenames and then processing that bunch,
                         *  then repeating as long as the overall M minute limit hasn't yet been reached.  This requires keeping track of "where we left off"
                         *  for both the scanning operation and the overall verify operation.  If a file hasn't yet been extracted and verified and we have to
                         *  quit because of time or cancellation, then we need the overall "where we left off" to point to the last extracted file, potentially
                         *  repeating a little bit of the mapping time when we startup again.
                         */                        

                        int MapStartTick = Environment.TickCount;
                        // If the last loop ran into a mapping time limit, initiate a continuation on the mapping.  If the last verification ran into a time limit,
                        // we are also doing a mapping continuation because there is no need to map anything before the first file of the new set.
                        if (MapContinuation.Required) MapContinuation.Starting = true;
                        MapContinuation.Required = false;

                        if (MapContinuation.Starting)
                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "Identifying required archive files, beginning at '" + MapContinuation.LastRelativePath + "'...");
                        else
                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "Identifying required archive files...");

                        MapManifest(LatestManifest.ArchiveRoot, ref FileList, ref ArchivesRequired, ref TotalBytes, ref MapContinuation, ref MapStartTick);

                        if (MapContinuation.Starting)
                        {
                            // We have the case where we never found the continuation marker.  This can happen if the latest archive mismatches
                            // the one where the continuation marker was made.  We just restart from the beginning of the archive.
                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "Continuation marker not found.  Starting verification from beginning.");
                            MapContinuation.Starting = false;
                            Debug.Assert(FileList.Count == 0 && ArchivesRequired.Count == 0);
                            MapStartTick = Environment.TickCount;
                            MapManifest(LatestManifest.ArchiveRoot, ref FileList, ref ArchivesRequired, ref TotalBytes, ref MapContinuation, ref MapStartTick);
                        }

                        ZippyForm.LogWriteLine(LogLevel.LightDebug, "Archives requiring verification (" + ArchivesRequired.Count + "):");
                        foreach (ArchiveFilename Archive in ArchivesRequired)
                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "\t" + Archive.ToString());
                        ZippyForm.LogWriteLine(LogLevel.LightDebug, "Starting individual verifications of " + FileList.Count + " files...");

                        // Begin extracting each file and then immediately deleting if successful.
                        int FilesVerified = 0;
                        foreach (ArchiveFilename Archive in ArchivesRequired)
                        {
                            Progress.label1.Text = "Extracting from archive:  " + Archive.ToString();
                            DoEvents();
                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "Verifying archive: " + Archive.ToString());
                            try
                            {
                                using (ZipFile zip = ZipFile.Read(Project.CompleteBackupFolder + "\\" + Archive.ToString()))
                                {
                                    zip.ZipError += new EventHandler<ZipErrorEventArgs>(OnZipError);
                                    zip.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(OnZipExtractProgress);
                                    foreach (Manifest.File Entry in FileList)
                                    {
                                        ArchiveFilename NeededArchive = ArchiveFilename.Parse(Entry.ArchiveFile);
                                        if (NeededArchive != Archive) continue;

                                        RunEntry(zip, Archive, Entry, Utility.StripTrailingSlash(TempRoot.FullName));
                                        FilesVerified++;

                                        if (AutomaticVerify && (Environment.TickCount - StartTick) > MaxAutomaticDurationInTicks)
                                        {
                                            Continuation.Required = true;
                                            Continuation.Starting = true;
                                            Continuation.LastRelativePath = Entry.RelativePath;
                                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "New verification continuation marker set to '" + Continuation.LastRelativePath + "'.");
                                            ZippyForm.LogWriteLine(LogLevel.Information, "Verify terminating early due to individual scan time limit.");
                                            break;
                                        }
                                    }
                                }
                            }
                            catch (FileNotFoundException fe)
                            {
                                StringBuilder Msg = new StringBuilder();
                                Msg.Append("Unable to locate file(s) in archive '" + Archive.ToString() + "'.  Do you want to delete this archive so that it will be reconstructed on your next backup?\n\nThe error was: " + fe.Message);
                                switch (MessageBox.Show(Msg.ToString(), "Error", MessageBoxButtons.YesNoCancel))
                                {
                                    case DialogResult.Yes: File.Delete(Project.CompleteBackupFolder + "\\" + Archive.ToString()); break;
                                    case DialogResult.No: break;
                                    case DialogResult.Cancel: throw new CancelException();
                                    default: throw new NotSupportedException();
                                }
                            }
                            catch (Ionic.Zip.ZipException ze)
                            {
                                StringBuilder Msg = new StringBuilder();
                                Msg.Append("Unable to extract file(s) from archive '" + Archive.ToString() + "'.  Do you want to delete this archive so that it will be reconstructed on your next backup?\n\nThe error was: " + ze.Message);
                                switch (MessageBox.Show(Msg.ToString(), "Error", MessageBoxButtons.YesNoCancel))
                                {
                                    case DialogResult.Yes: File.Delete(Project.CompleteBackupFolder + "\\" + Archive.ToString()); break;
                                    case DialogResult.No: break;
                                    case DialogResult.Cancel: throw new CancelException();
                                    default: throw new NotSupportedException();
                                }
                            }
                            catch (Ionic.Zlib.ZlibException ze)
                            {
                                StringBuilder Msg = new StringBuilder();
                                Msg.Append("Unable to extract file(s) from archive '" + Archive.ToString() + "'.  Do you want to delete this archive so that it will be reconstructed on your next backup?\n\nThe error was: " + ze.Message);
                                switch (MessageBox.Show(Msg.ToString(), "Error", MessageBoxButtons.YesNoCancel))
                                {
                                    case DialogResult.Yes: File.Delete(Project.CompleteBackupFolder + "\\" + Archive.ToString()); break;
                                    case DialogResult.No: break;
                                    case DialogResult.Cancel: throw new CancelException();
                                    default: throw new NotSupportedException();
                                }
                            }

                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "Verified " + FilesVerified + " of " + FileList.Count + " files.");

                            if (Continuation.Required) break;
                        }

                        if (!MapContinuation.Required) break;
                    }

                    string BackupStatusFile = Project.CompleteBackupFolder + "\\Backup_Status.xml";
                    BackupStatus bs = new BackupStatus();
                    try { bs = BackupStatus.Load(BackupStatusFile); }
                    catch (Exception) { }
                    bs.LastVerify = DateTime.UtcNow;
                    if (!Continuation.Required) bs.LastCompletedVerify = DateTime.UtcNow;
                    if (!Continuation.Required) bs.LastVerifyRelativePath = ""; else bs.LastVerifyRelativePath = Continuation.LastRelativePath;
                    bs.Save(BackupStatusFile);

                    ZippyForm.LogWriteLine(LogLevel.Information, "Verification complete.");
                }
                finally
                {
                    Directory.Delete(TempRoot.FullName);
                }
            }                
            catch (CancelException ce)
            {
                ZippyForm.LogWriteLine(LogLevel.Information, "Verification cancelled by user before completion.");
                throw ce;
            }
            catch (Exception ex)
            {
                ZippyForm.LogWriteLine(LogLevel.LightDebug, "Verification interrupted by error: " + ex.Message);
                throw ex;
            }
            finally
            {
                Progress.Dispose();
                Progress = null;

                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
            }
        }
        
        void OnZipError(object sender, ZipErrorEventArgs e)
        {
            try
            {
                switch (MessageBox.Show("Error extracting '" + e.FileName + "': " + e.Exception, "Error", MessageBoxButtons.AbortRetryIgnore))
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

        void OnZipExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            try
            {
                switch (e.EventType)
                {
                    case ZipProgressEventType.Extracting_BeforeExtractEntry:
                        Progress.CurrentProgressBar.Maximum = 10000;
                        Progress.CurrentProgressBar.Value = 0;
                        break;
                    case ZipProgressEventType.Extracting_EntryBytesWritten:
                        Progress.CurrentProgressBar.Maximum = 10000;
                        Progress.CurrentProgressBar.Value = (int)(10000L * e.BytesTransferred / e.TotalBytesToTransfer);
                        break;
                    case ZipProgressEventType.Extracting_AfterExtractEntry:
                        Progress.CurrentProgressBar.Maximum = 10000;
                        Progress.CurrentProgressBar.Value = 10000;
                        break;
                }

                DoEvents();
            }
            catch (CancelException ce) { ZipError = ce; e.Cancel = true; }
            catch (Exception ex)
            {
                if (e.CurrentEntry != null && e.CurrentEntry.FileName != null)
                    ZipError = new Exception(ex.Message + "\nError while processing extraction status for file '" + e.CurrentEntry.FileName + "'.", ex);
                else
                    ZipError = new Exception(ex.Message + "\nError while processing extraction status.", ex);
                e.Cancel = true;
            }
        }

        void RunEntry(ZipFile zip, ArchiveFilename Archive, Manifest.File File, string TempFolder)
        {
            if (File == null || String.IsNullOrEmpty(TempFolder)) throw new ArgumentException("Invalid manifest entry or processing error for manifest entry.", "Entry");            

            Progress.label2.Text = "Extracting:  " + File.RelativePath;
            DoEvents();

            string TempPath = Utility.StripTrailingSlash(TempFolder) + "\\" + File.Name;

            try
            {
                ZippyForm.LogWriteLine(LogLevel.MediumDebug, "Verifying (extracting) file '" + File.RelativePath + "' to temporary path '" + TempPath + "'.");
                ExtractAndDeleteFile(zip, File, TempPath);
                BytesCompleted += (long)File.Length;
            }
            catch (CancelException ce) { throw ce; }
            catch (Ionic.Zip.ZipException ze) { throw ze; }
            catch (Ionic.Zlib.ZlibException ze) { throw ze; }
            catch (FileNotFoundException fe) { throw fe; }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\nWhile verifying file '" + File.RelativePath + "' by trial extraction.", ex);
            }

            Progress.OverallProgressBar.Value = (int)(10000L * BytesCompleted / TotalBytes);
            return;
        }

        void ExtractAndDeleteFile(ZipFile zip, Manifest.File FileM, string DestinationPath)
        {
            for (; ; )
            {
                try
                {
                    if (File.Exists(DestinationPath))
                    {
                        // In case the file is marked read-only, unmark it.  We'll correctly set the attributes after we create the file.
                        File.SetAttributes(DestinationPath, FileM.WindowsAttributes & ~System.IO.FileAttributes.ReadOnly & ~System.IO.FileAttributes.Hidden & ~System.IO.FileAttributes.System);
                    }

                    ZipEntry ze;
                    try
                    {
                        string PathInArchive = FileM.PathInArchive.Replace('\\', '/').Replace('’', '\'');
                        ze = zip[PathInArchive];
                        if (ze == null) throw new FileNotFoundException();
                    }
                    catch (CancelException ce) { throw ce; }
                    catch (Exception ex)
                    {
                        throw new FileNotFoundException(ex.Message + "\nThe file '" + FileM.RelativePath + "' is missing from archive '" + FileM.ArchiveFile + "'.", ex);
                    }

                    using (FileStream Dest = new FileStream(DestinationPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                    {
                        bool FirstPasswordPrompt = true;
                        for (; ; )
                        {
                            try
                            {
                                if (String.IsNullOrEmpty(Project.SafePassword.Password))
                                    ze.Extract(Dest);
                                else
                                    ze.ExtractWithPassword(Dest, Project.SafePassword.Password);
                                if (ZipError != null) throw ZipError;
                                break;
                            }
                            catch (Ionic.Zip.BadPasswordException bpe)
                            {
                                Dest.SetLength(0); Dest.Seek(0, System.IO.SeekOrigin.Begin);
                                try
                                {
                                    if (!String.IsNullOrEmpty(Project.AlternativePassword))
                                        ze.ExtractWithPassword(Dest, Project.AlternativePassword);
                                    else throw bpe;
                                    if (ZipError != null) throw ZipError;
                                    break;
                                }
                                catch (Ionic.Zip.BadPasswordException)
                                {
                                    Dest.SetLength(0); Dest.Seek(0, System.IO.SeekOrigin.Begin);

                                    PasswordForm pf = new PasswordForm();
                                    if (FirstPasswordPrompt)
                                        pf.Prompt = "The archive '" + FileM.ArchiveFile + "' was created with a different password.  (121)";
                                    else
                                        pf.Prompt = "That was not a valid password for the archive '" + FileM.ArchiveFile + "'.";
                                    FirstPasswordPrompt = false;
                                    if (pf.ShowDialog() != DialogResult.OK) throw new CancelException();
                                    Project.AlternativePassword = pf.Password;
                                    continue;
                                }
                            }
                        }                        
                    }
                    ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "Success extracting file '" + FileM.RelativePath + "'.");
                    break;
                }
                catch (CancelException ce) { throw ce; }
                catch (Ionic.Zip.ZipException ze) { throw ze; }
                catch (Ionic.Zlib.ZlibException ze) { throw ze; }
                catch (FileNotFoundException fe) { throw fe; }
                catch (Exception ex)
                {
#                   if DEBUG
                    DialogResult dr = MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.AbortRetryIgnore);
#                   else
                    DialogResult dr = MessageBox.Show(ex.Message, "Error", MessageBoxButtons.AbortRetryIgnore);
#                   endif
                    if (dr == DialogResult.Abort) throw new CancelException();
                    else if (dr == DialogResult.Retry) continue;
                    else if (dr == DialogResult.Ignore) return;
                    else throw ex;
                }
                finally
                {
                    try
                    {
                        ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "Removing temporary file '" + DestinationPath + "'.");
                        File.Delete(DestinationPath);
                    }
                    catch (Exception) { }
                }
            }
        }        

        void DoEvents()
        {
            Application.DoEvents();
            if (Progress.Cancel) throw new CancelException();
        }
    }
}
