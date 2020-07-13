/** ExtractionRun.cs
 *  Copyright (C) 2012-2015 by Wiley Black.  All rights reserved.
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
    public class ExtractionRun
    {
        BackupProject Project;
        ProgressForm Progress;
        long TotalBytes = 0;
        long BytesCompleted = 0;

        Exception ZipError;

        public ExtractionRun(BackupProject Project)
        {
            this.Project = Project;
        }

        public void Run(List<Manifest.Entry> Entries, string DestinationFolder)
        {
            // Setup progress UI...
            Progress = new ProgressForm();
            Progress.Text = "Extracting archived files";
            Progress.OverallProgressBar.Maximum = 10000;
            Progress.OverallProgressBar.Minimum = 0;
            Progress.OverallProgressBar.Value = 0;
            Progress.CancelPrompt = "Are you sure you wish to cancel this extraction operation?";
            Progress.Show();

            try
            {
                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;

                ZippyForm.LogWriteLine(LogLevel.Information, "Starting extraction from project " + Project.Name + " of " + Entries.Count + " files and folders to destination folder '" + DestinationFolder + "'.");

                /** Operate on a single zip file at a time.  This requires planning which files are
                 *  coming from where.  Also, we count how many total bytes we'll be extracting.  **/
                Progress.label1.Text = "Identifying required archive files...";
                List<ArchiveFilename> ArchivesRequired = new List<ArchiveFilename>();
                foreach (Manifest.Entry Entry in Entries) IdentifyRequiredArchives(ArchivesRequired, Entry);

                // Then, begin actual extraction...
                foreach (ArchiveFilename Archive in ArchivesRequired)
                {
                    ZippyForm.LogWriteLine(LogLevel.LightDebug, "Starting extractions from archive '" + Archive.ToString() + "'.");
                    Progress.label1.Text = "Extracting from archive:  " + Archive.ToString();
                    for (; ; )
                    {
                        try
                        {
                            using (ZipFile zip = ZipFile.Read(Project.CompleteBackupFolder + "\\" + Archive.ToString()))
                            {
                                zip.ZipError += new EventHandler<ZipErrorEventArgs>(OnZipError);
                                zip.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(OnZipExtractProgress);
                                foreach (Manifest.Entry Entry in Entries)
                                {
                                    ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "Extracting '" + Entry.RelativePath + "'.");
                                    RunEntry(zip, Archive, Entry, Utility.StripTrailingSlash(DestinationFolder));
                                }
                            }
                            break;
                        }
                        catch (Ionic.Zip.ZipException ze)
                        {
                            ZippyForm.LogWriteLine(LogLevel.Information, "Zip error during extraction from archive '" + Archive.ToString() + "': " + ze.Message);
                            ZippyForm.LogWriteLine(LogLevel.LightDebug, "Detailed error: " + ze.ToString());

                            StringBuilder Msg = new StringBuilder();
                            Msg.Append("Error extracting from archive '" + Archive.ToString() + "': " + ze.Message);
                            if (Entries.Count < 15)
                            {
                                Msg.AppendLine();
                                Msg.AppendLine("The following entries to be restored are affected:");
                                foreach (Manifest.Entry Entry in Entries)
                                {
                                    if (Entry is Manifest.File) Msg.AppendLine(Entry.Name);
                                    if (Entry is Manifest.Folder) Msg.AppendLine(Entry.Name + " folder");
                                }
                            }
                            else
                                Msg.Append("  More than 15 files to be restored are affected.");
                            switch (MessageBox.Show(Msg.ToString(), "Error", MessageBoxButtons.AbortRetryIgnore))
                            {
                                case DialogResult.Abort: throw new CancelException();
                                case DialogResult.Retry: continue;
                                case DialogResult.Ignore: break;
                                default: throw new NotSupportedException();
                            }
                        }
                    }
                }

                ZippyForm.LogWriteLine(LogLevel.Information, "Extraction operation completed.");
            }
            catch (CancelException ex)
            {
                ZippyForm.LogWriteLine(LogLevel.Information, "Extraction operation canceled.");
                throw ex;
            }
            finally
            {
                Progress.Dispose();
                Progress = null;

                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
            }
        }

        void IdentifyRequiredArchives(List<ArchiveFilename> ArchivesRequired, Manifest.Entry Entry)
        {
            try
            {
                if (Entry is Manifest.File)
                {
                    Manifest.File File = (Manifest.File)Entry;
                    TotalBytes += (long)File.Length;
                    ArchiveFilename Archive = ArchiveFilename.Parse(File.ArchiveFile);
                    if (!ArchivesRequired.Contains(Archive)) ArchivesRequired.Add(Archive);
                }
                else if (Entry is Manifest.Folder)
                {
                    Manifest.Folder Folder = (Manifest.Folder)Entry;
                    foreach (Manifest.File File in Folder.Files) IdentifyRequiredArchives(ArchivesRequired, File);
                    foreach (Manifest.Folder Subfolder in Folder.Folders) IdentifyRequiredArchives(ArchivesRequired, Subfolder);
                }
                else throw new ArgumentException();
            }
            catch (CancelException ce) { throw ce; }
            catch (Exception ex)
            {                
                try
                {
                    ZippyForm.LogWriteLine(LogLevel.Information, "Error while identifying required archive for '" + Entry.RelativePath + "': " + ex.Message);
                    ZippyForm.LogWriteLine(LogLevel.LightDebug, "Detailed error: " + ex.ToString());

                    throw new Exception(ex.Message + "\nWhile analyzing archives for entry '" + Entry.RelativePath + "'.", ex);
                }
                catch (Exception exc)
                {
                    throw new Exception(exc.Message + "\nWhile generating error message for: \n\n" + ex.Message, exc);
                }
            }
        }

        void OnZipError(object sender, ZipErrorEventArgs e)
        {
            try
            {
                ZippyForm.LogWriteLine(LogLevel.Information, "Error while extracting '" + e.FileName + "': " + e.Exception.Message);
                ZippyForm.LogWriteLine(LogLevel.LightDebug, "Detailed error: " + e.Exception.ToString());

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

        void RunEntry(ZipFile zip, ArchiveFilename Archive, Manifest.Entry Entry, string DestinationFolder)
        {
            if (Entry == null || String.IsNullOrEmpty(DestinationFolder)) throw new ArgumentException("Invalid manifest entry or processing error for manifest entry.", "Entry");
            bool IsArchiveRoot = (String.IsNullOrEmpty(Entry.Name)) && Entry is Manifest.Folder;

            Progress.label2.Text = "Extracting:  " + Entry.RelativePath;
            DoEvents();

            string Path = Utility.StripTrailingSlash(DestinationFolder) + "\\" + Entry.Name;

            if (Entry is Manifest.File)
            {
                Manifest.File File = (Manifest.File)Entry;
                try
                {
                    ArchiveFilename NeededArchive = ArchiveFilename.Parse(File.ArchiveFile);
                    if (NeededArchive == Archive)
                    {
                        Directory.CreateDirectory(DestinationFolder);
                        ExtractFile(zip, File, Path);
                        ExtractProperties(File, Path);
                        BytesCompleted += (long)File.Length;
                    }
                }
                catch (CancelException ce) { throw ce; }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + "\nWhile extracting file '" + Entry.RelativePath + "'.", ex);
                }
            }
            else if (Entry is Manifest.Folder)
            {
                Manifest.Folder Folder = (Manifest.Folder)Entry;
                try
                {
                    Directory.CreateDirectory(Path);
                    foreach (Manifest.File File in Folder.Files)
                        RunEntry(zip, Archive, File, Path);
                    foreach (Manifest.Folder Subfolder in Folder.Folders)
                        RunEntry(zip, Archive, Subfolder, Path);
                    if (!IsArchiveRoot) ExtractProperties(Folder, Path);
                }
                catch (CancelException ce) { throw ce; }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + "\nWhile extracting folder '" + Entry.RelativePath + "'.", ex);
                }
            }
            else throw new ArgumentException();

            Progress.OverallProgressBar.Value = (int)(10000L * BytesCompleted / TotalBytes);
            return;
        }

        bool OverwriteAll = false;
        bool OverwriteNone = false;

        void ExtractFile(ZipFile zip, Manifest.File FileM, string DestinationPath)
        {
            bool CanOverwrite = false;

            // NeedRollback is set to true as soon as we've created the new file.  If an error occurs before we complete
            // the new file, we need to delete the file we've created.  Once we finish the new file successfully, we no 
            // longer need a rollback (delete) operation.
            bool NeedRollback = false;

            for (; ; )
            {
                try
                {
                    ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "Extracting file '" + FileM.RelativePath + "' from archive path '" + FileM.PathInArchive + "' in archive '" + FileM.ArchiveFile + "'...");

                    if (File.Exists(DestinationPath))
                    {
                        if (OverwriteAll) CanOverwrite = true;
                        if (OverwriteNone) return;

                        if (!CanOverwrite)
                        {
                            OverwritePromptForm.Result res = OverwritePromptForm.Show("The file '" + DestinationPath + "' already exists.  Overwrite?");
                            if (res == OverwritePromptForm.Result.Cancel) throw new CancelException();
                            else if (res == OverwritePromptForm.Result.Yes) CanOverwrite = true;
                            else if (res == OverwritePromptForm.Result.No) return;
                            else if (res == OverwritePromptForm.Result.YesToAll) { CanOverwrite = true; OverwriteAll = true; }
                            else if (res == OverwritePromptForm.Result.NoToAll) { OverwriteNone = true; return; }
                            else throw new Exception();
                        }

                        // In case the file is marked read-only, unmark it.  We'll correctly set the attributes after we create the file.
                        File.SetAttributes(DestinationPath, FileM.WindowsAttributes & ~System.IO.FileAttributes.ReadOnly & ~System.IO.FileAttributes.Hidden & ~System.IO.FileAttributes.System);
                    }

                    ZipEntry ze;
                    try
                    {
                        string PathInArchive = FileM.PathInArchive.Replace('\\', '/').Replace('’','\'');
                        ze = zip[PathInArchive];
                        if (ze == null)
                        {
                            /*
                            if ((int)ZippyForm.MainList.Logging >= (int)LogLevel.HeavyDebug)
                            {
                                ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "All entries in this archive:");
                                foreach (ZipEntry zeDiag in zip)
                                    ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t" + zeDiag.FileName);
                            }
                             */

                            // I've had this exception trigger before when a special character had been silently replaced in the zip file
                            // archive name.  In that case, it was a special apostrophe character that had been silently replaced with an
                            // ordinary apostrophe.  So, maybe the zip directoy abuses unicode characters?

                            throw new FileNotFoundException();
                        }
                    }
                    catch (CancelException ce) { throw ce; }
                    catch (Exception ex)
                    {
                        throw new FileNotFoundException(ex.Message + "\nThe file '" + FileM.RelativePath + "' is missing from archive '" + FileM.ArchiveFile + "'.", ex);
                    }

                    using (FileStream Dest = new FileStream(DestinationPath, CanOverwrite ? FileMode.Create : FileMode.CreateNew,
                        FileAccess.ReadWrite, FileShare.Read))
                    {
                        NeedRollback = true;

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
                    NeedRollback = false;
                    return;
                }
                catch (CancelException ce) { throw ce; }
                catch (Exception ex)
                {
                    ZippyForm.LogWriteLine(LogLevel.Information, "Error extracting file '" + FileM.RelativePath + "' from archive path '" + FileM.PathInArchive + "' in archive '" + FileM.ArchiveFile + "': " + ex.Message);
                    ZippyForm.LogWriteLine(LogLevel.LightDebug, "Detailed Error: " + ex.ToString());

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
                    if (NeedRollback) { File.Delete(DestinationPath); NeedRollback = false; }
                }
            }
        }

        void ExtractProperties(Manifest.Entry Entry, string Path)
        {
            for (; ; )
            {
                try
                {
                    if (Entry is Manifest.File)
                    {
                        File.SetCreationTimeUtc(Path, Entry.CreationTimeUtc);
                        File.SetLastWriteTimeUtc(Path, Entry.LastWriteTimeUtc);
                        File.SetLastAccessTimeUtc(Path, Entry.LastAccessTimeUtc);
                        File.SetAttributes(Path, Entry.WindowsAttributes);
                    }
                    else if (Entry is Manifest.Folder)
                    {
                        System.IO.Directory.SetCreationTimeUtc(Path, Entry.CreationTimeUtc);
                        System.IO.Directory.SetLastWriteTimeUtc(Path, Entry.LastWriteTimeUtc);
                        System.IO.Directory.SetLastAccessTimeUtc(Path, Entry.LastAccessTimeUtc);
                        File.SetAttributes(Path, Entry.WindowsAttributes);
                    }
                    else throw new ArgumentException();
                    return;
                }
                catch (CancelException ce) { throw ce; }
                catch (Exception ex)
                {
#                   if DEBUG
                    string ErrMsg = "Error extracting attributes for '" + Path + "': " + ex.Message + "\n\nDetails:\n" + ex.ToString();
#                   else
                    string ErrMsg = "Error extracting attributes for '" + Path + "': " + ex.Message;
#                   endif

                    switch (MessageBox.Show(ErrMsg, "Error", MessageBoxButtons.AbortRetryIgnore))
                    {
                        case DialogResult.Abort: throw ex;
                        case DialogResult.Retry: continue;
                        case DialogResult.Ignore: return;
                        default: throw new NotSupportedException();
                    }
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
