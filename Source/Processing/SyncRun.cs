#if false
/** SyncRun.cs
 *  Copyright (C) 2015 by Wiley Black.  All rights reserved.
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

namespace ZippyBackup
{
    public class SyncRun
    {
        BackupProject Project;
        ProgressForm Progress;
        long TotalBytes = 0;
        long BytesCompleted = 0;

        /// <summary>
        /// Gives the root folder for enumerating and reading files for the backup.  This is equal to Project.SourceFolder
        /// for sync operations.
        /// </summary>
        string SourceRoot;

        Exception ZipError;

        public SyncRun(BackupProject Project)
        {
            this.Project = Project;
        }

        public void Run()
        {
            // Setup progress UI...
            Progress = new ProgressForm();
            Progress.Text = "Synchronizing files";
            Progress.OverallProgressBar.Maximum = 10000;
            Progress.OverallProgressBar.Minimum = 0;
            Progress.OverallProgressBar.Value = 0;
            Progress.Show();

            try
            {
                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;

                ZippyForm.LogWriteLine(LogLevel.Information, "Starting synchronize operation for project '" + Project.Name + "'.");

                /** Retrieve newest manifest **/
                ArchiveFilename LatestBackup;
                Manifest LatestManifest;
                try
                {
                    using (Impersonator newself = new Impersonator(Project.BackupCredentials))
                    {
                        // Locate latest backup (either complete or incremental.)
                        lock (Project.ArchiveFileList) LatestBackup = Project.ArchiveFileList.FindMostRecent();
                        if (LatestBackup == ArchiveFilename.MaxValue) throw new NoCompleteBackupException();

                        ZippyForm.LogWriteLine(LogLevel.LightDebug, "Loading most recent manifest from '" + LatestBackup.ToString() + "'.");

                        LatestManifest = LatestBackup.LoadArchiveManifest(Project, true);
                    }
                }
                catch (CancelException ce) { throw ce; }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + "  Error while loading last backup for sync.", ex);
                }

                SourceRoot = Project.SourceFolder;

                SyncFolder(new DirectoryInfo(Project.SourceFolder), LatestManifest.ArchiveRoot);

                // Update Backup Status file.                    
                using (Impersonator newself = new Impersonator(Project.BackupCredentials))
                {
                    string BackupStatusFile = Project.CompleteBackupFolder + "\\Backup_Status.xml";
                    BackupStatus bs = BackupStatus.Load(BackupStatusFile);
                    bs.LastSync = DateTime.UtcNow;
                    bs.Save(BackupStatusFile);
                }
            }
            finally
            {
                Progress.Dispose();
                Progress = null;

                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
            }
        }
        
        int SyncFolder(DirectoryInfo di, Manifest.Folder BackupFolder)
        {
            // Scan for files and folders that have been updated in the manifest.
            //
            // Each host must track certain changes for synchronization to be
            // effective.  These changes do not need to be included in backups,
            // but are necessary to accomplish syncs.  These include:
            // 1. Deletion of files/folders
            // 2. Modification of files
            // 3. Move of files/folders out of tree (equivalent to #1).
            // 4. Creation of files/folders?
            //
            // Whenever a file or folder is changed (including deletion), we must
            // take a moment to identify where the file was branched from and make
            // a note.  The sync system will need to append this information to a
            // local file (if not already listed).  A simple xml file with an
            // implicit top-level container is ideal.  Entries in the xml file
            // would look like:
            //
            // <Changed filename="Relative Path/Filename" branch-source="Backup123.zip" />
            // <Deleted filename="Relative Path/Gonefile" branch-source="Backup122.zip" />
            // <Created filename="Relative Path/Newfile" />
            //
            // To parse the XML file, initiate a string with <Local-Updates> and append
            // the contents of the file.  Then append a </Local-Updates> and parse.  This
            // allows writes to the file in text mode using a file append.
            //
            // The key information is where a file branched from at the time of the
            // modification.  When we generate a backup archive, all files become
            // branches from that new archive going forward (upon completion of
            // the backup, including any secondary archive files necessary for 
            // completion).  When we receive a backup archive to sync, any files 
            // that are not locally updated develop that archive as the new active 
            // branch source.
            //
            // We can resolve the branch source identification issue from conflicted
            // files by pulling the google drive solution of renaming one of the
            // conflicted copies to [Conflicted] and then it exists as its own
            // new file.  It is still a modified (created) file though, so it has
            // no branch source at all and still must be tracked as an exception to 
            // the current global branch source.
            //
            // Now a sync is simply a matter of verifying that all files, folders,
            // and differences that come from the new archive are in a linear branch 
            // line with the files on disk, and any that aren't are marked as conflicted.  
            //
            // The trick is identifying a linear branch line.  If a file has not been
            // changed locally, this is straightforward - it is automatically a valid
            // update.  Wait, what if the other computer didn't see a previous archive?
            // Do we need to validate that things are synchronized before they are
            // backed up?  It is possible using Google Drive for instance that two
            // computers generated backups at close to the same time, both archives
            // having updates, and one or both archive took quite a while to upload.
            // Computer A and B have no awareness of the other archive when generating
            // their own.  Their sync operations cannot resolve these updates then.
            // I suppose the Manifest could also identify the branch source for a file
            // prior to changes, and that way the conflicts can be detected.  This
            // solution again fails for Complete backups though, as they have no branch
            // source.
            //
            // What happens in these situations?  Computer A uploads file A1 that came
            // from branch 123.  Computer B simultaneously uploads file B1 that came
            // from branch 123.  These files are conflicted.  Computer A receives file
            // B1.  Can it detect that these files both came from branch 123 instead of
            // from the more recent update?  One of the computers should notice that the
            // file timestamp went backwards and is older than its local copy.  What
            // about a delete or a create operation though?
            //
            // Also what happens if the two archives have the same filename?  Well, I suppose
            // google drive would mark one as [Conflicted].  If one of the filenames is
            // older than the other, would the 2nd host even know to sync to the archive
            // since it is older than the latest that it generated itself?
            //
            // I suppose the slave host could have a subfolder where its files go.  What
            // a mess.
            //
            /** OLDER THOUGHTS BELOW **/
            //
            // Now a sync is simplified:
            // A. Any modification from the archive to a file that hasn't been modified
            //    locally is applied.
            // B. Any modification on a local file that is not updated in the archive
            //    is retained.
            // C. Any modification on a local file that has been updated is a conflict.
            //
            // We need only define "updated" and "not updated".  The branch source can
            // help with this.  
            //             
            // Not modified in the archive -> File in archive matches local file timestamp
            // or is stored in an archive that is less than or equal to the file's branch
            // source.

            //
            // Cases:
            //  A. File/Folder updated in backup that isn't on hard drive.
            //      1. Was it locally deleted after the backup time?
            //          Action: Keep it deleted.
            //      2. Was it locally deleted before the backup time?
            //          Action: Conflict, prompt user.
            //      2. Was it created remotely?
            //          Action: If not in exclusion list, sync to hard drive.
            //  B. File/Folder found on hard drive that isn't in backup.
            //      1. Was it deleted?
            //          Action: Delete hard drive file (may want to prompt user).
            //      2. Was it created locally?
            //          Action: Ignore - this is a backup problem not a sync problem.
            //  C. File newer in backup than on hard drive.
            //      1. Does the file have a first modified time?
            //          Action: Conflict, prompt user.
            //      2. If the file does not have a first modified time...
            //          Action: Update file from backup.
            //  D. File newer on hard drive than backup.
            //      1. Is the first modified time for the file later than the backup?
            //    * And we need a last synchronized time?
            //          Action: Ignore - file 
            //      1. Was file backed up by a different computer before changes made?              
            //         Was file sync'd before changes started?
            //      2. Was file backed up by this computer before changes made?
            //          Action: Ignore - this is a backup problem not a sync problem.
            //      3. Was file changed before being backed up by a different computer?  (Conflict)
            //          Action: Conflict, prompt user.
            //  E. File changes in both backup and hard drive.
            //      Detected when first modified time is older than the modification time in backup.
            //      Action: Always a conflict.
            //
            // In all cases of conflict, it would be useful to do a more careful comparison of the files
            // before bothering the user - if they ended up being identical then it doesn't matter.            
            
            // I need to be able to trace out the modification path in order to check that there are no conflicts.
            // Case C is straightforward, there is a definite modification conflict if there is a first modified time.
            // Case D is not so straightforward.
            
            // It helps to draw out timelines:
            /*
             *     Sync'd
             *      |------- A ------->
             *      |                   ^
             *      |------------ B --> |
             *      
             * Case 1. Both A and B have modified the file since last sync.  This is a conflict detected when B makes a backup
             * and A sees the backup dated after the first modified time on its local copy.  This is case E.
             * 
             *     Sync'd
             *      |------------ A --->
             *      |                   ^
             *      |------- B -------> |
             *   
             * Case 2. Both A and B have modified the file since last sync.  The backup modified time is
             * earlier than A's first modified time but later than the backup copy that A had pulled from.  This is a conflict, but
             * how do we detect it?  We could:
             *      1. Keep track of the last sync of all local files.
             * I think that's necessary...we need this "origin time" or "sync'd time" to know if there is a conflict because we need
             * to track back and say whether the file coming out of a backup was sync'd.
             * 
             * Actually, we can figure this out in case 2...let's see.  We have a backup that came in and we see that a file has been
             * modified.  It was modified before our first modified time by B, but after the previous backup time of the file.  Do we
             * assume that the previous backup time of the file had it sync'd between A and B?  That's not a great assumption.  We
             * could keep a single project-wide note of the last time that we were successfully sync'd.  Now it matches the picture, the
             * sync'd marker isn't the last time that it was backed up but the last time we completed a sync.  But now backups can happen
             * asynchronous to sync's.
             * 
             *         Sync'd
             *     ------S------------ A -----S
             *           |                    |
             *     ---W--|------- B -------W--|
             * 
             * New picture: S refers to a sync operation (read) and W refers to a write (backup) operation.  This is a smooth picture, but 
             * there are complexities.  We can't have a W W together because there has to be a "B" that indicates a change.  
             * 
             * Unless there were a complete backup I suppose.  A complete backup is a whole new dimension to the problem.  We should probably 
             * also mark whether a complete backup came from a master copy?  A master copy is one where excluded folders are truly excluded 
             * from the manifest.  A non-master can exclude folders, but in that case it only means that those folders are excluded on that 
             * machine and that the excluded folder that exists in the manifest should be propagated to later manifests.
             * 
             *         Sync'd
             *     ------S------------ A ----S
             *           |                   |
             *     ---W--|---W--- B ---------|
             * 
             * Maybe I should aim lower and setup a system that prompts the user for every sync change made?
             * 
             * Can I do without the notifications?  We should be able to detect a deleted file by comparison.  We know there is always a
             * complete backup somewhere at the root of each archive timeline.  So for any file that is missing, there is a previous
             * copy that exists.  That translates to a deletion.  We can find that from archive comparisons.  What about modifications?
             * Well, those give us the "first modified time" but do we need it to be first modified time?  Can it be last?  Hmm, it does
             * kind of need to be first modified time because the purpose is to identify whether something was modified from the same
             * sync'd copy or if a branch happened.  But the first modified time might only be relevant to sync operations, backups
             * don't need to record it?  That would simplify using "Complete" backups as equivalent to incrementals.
             * 
             * Instead of first modified time, we need to record the branch source when we see a Changed notification.  The entire
             * problem is about detecting conflicts, so let's do it as soon as we notice a change instead of trying to backtrack it.
             * Whenever a file is changed (and we get a notification), we need to record that the file was branched off of backup 123.
             * Then, when a new backup comes in for us to sync against, a conflict detection is easy - if the file was modified after
             * backup 123 then we have a conflict.
             */

            
            

#error Left off here:
#error How do we identify cases B1 vs B2 and cases D1 vs D2?
            
            try
            {
                int ChangedFiles = 0;
                
                ZippyForm.LogWriteLine(LogLevel.MediumDebug, "Scanning folder '" + di.FullName + "'...");

                string RelativePath = Utility.GetRelativePath(SourceRoot, di.FullName);

                Progress.label2.Text = "Scanning folder: " + RelativePath;

                /** Check excluded folder list **/

                foreach (string ExcludedRelativePath in Project.ExcludeSubfolders)
                    if (RelativePath.ToLower() == ExcludedRelativePath.ToLower()) return 0;

                ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\tNot found on folder exclusion list.");
                
                /** Scan for any subfolders that no longer exist **/

                DirectoryInfo[] ExistingFolders = di.GetDirectories();

                try
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
                catch (CancelException ce) { throw ce; }
                catch (Exception exc) { throw new Exception(exc.Message + " (672:13)", exc); }

                /** Scan all subfolders within the folder **/

                try
                {
                    ZippyForm.LogWriteLine(LogLevel.MediumDebug, "\tScanning subfolders...");

                    foreach (DirectoryInfo sub in ExistingFolders)
                    {
                        DoEvents();

                        if (SymbolicLink.IsLink(sub.FullName)) continue;

                        RelativePath = Utility.GetRelativePath(SourceRoot, sub.FullName);

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

                /** Scan for any files that no longer exist **/

                FileInfo[] ExistingFiles = di.GetFiles();

                try
                {
                    if (!Continuation.Required && PrevFolder != null)
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

                    RelativePath = Utility.GetRelativePath(SourceRoot, fi.FullName);

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
                                    // else, perform action B.
                                    break;
                                }
                            }
                            if (Skip)
                            {
                                ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\t\tNo change detected.");
                                continue;         // Action B.
                            }
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
                            Continuation.Required = true;
                            Continuation.Starting = true;
                            Continuation.LastRelativePath = RelativePath;
                            ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "New continuation marker set to '" + RelativePath + "'.");
                            ZippyForm.LogWriteLine(LogLevel.Information, "Scan terminating early due to archive size limit.");
                        }
                    }
                    catch (CancelException ce) { throw ce; }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + "\nError while processing file '" + fi.FullName + "'.", ex);
                    }
                }

                ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "\tSubfolder scan complete for '" + di.FullName + "'.");

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

        void DoEvents()
        {
            Application.DoEvents();
            if (Progress.Cancel) throw new CancelException();
        }
    }
}
#endif
