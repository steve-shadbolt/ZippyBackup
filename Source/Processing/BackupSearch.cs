using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ZippyBackup.User_Interface;

namespace ZippyBackup
{
    public class BackupSearch : IDisposable
    {
        #region "Search Control Properties"

        /// <summary>
        /// CurrentProject controls the project being used in the current search.  CurrentProject
        /// is thread-safe, and setting it will restart the current search.  CurrentProject can
        /// be set to null to stop any active search activities.
        /// </summary>
        public BackupProject CurrentProject
        {
            get
            {
                lock (m_CurrentProjectLock)
                    return m_CurrentProject;
            }
            set
            {
                lock (m_CurrentProjectLock)
                {
                    m_CurrentProject = value;
                }
                StartNewSearch();
            }
        }
        private object m_CurrentProjectLock = new object();
        private BackupProject m_CurrentProject;

        /// <summary>
        /// CurrentSearchWords controls the current search.  CurrentSearchWords
        /// is thread-safe, and setting it will restart the current search.
        /// </summary>
        public string[] CurrentSearchWords
        {
            get
            {
                lock (m_CurrentSearchWordsLock)
                {
                    if (m_CurrentSearchWords == null) return null;
                    return (string[])m_CurrentSearchWords.Clone();
                }
            }
            set
            {
                lock (m_CurrentSearchWordsLock)
                {
                    m_CurrentSearchWords = value;
                }
                StartNewSearch();
            }
        }
        private object m_CurrentSearchWordsLock = new object();
        private string[] m_CurrentSearchWords;

        public bool IncludeArchiveNames
        {
            get
            {
                lock (m_IncludeArchiveNamesLock) return m_IncludeArchiveNames;
            }
            set
            {
                lock (m_IncludeArchiveNamesLock) m_IncludeArchiveNames = value;
                StartNewSearch();
            }
        }
        private object m_IncludeArchiveNamesLock = new object();
        private bool m_IncludeArchiveNames = true;

        public bool IncludeFolderNames
        {
            get
            {
                lock (m_IncludeFolderNamesLock) return m_IncludeFolderNames;
            }
            set
            {
                lock (m_IncludeFolderNamesLock) m_IncludeFolderNames = value;
                StartNewSearch();
            }
        }
        private object m_IncludeFolderNamesLock = new object();
        private bool m_IncludeFolderNames = true;

        /// <summary>
        /// NewResults contains a list of any archives which have been found to meet
        /// the search criteria.  NewResults should be lock()'d before access.  Call
        /// Clear() on NewResults after retrieving the results (within the same lock
        /// block) to ensure that NewResults only contains results since the last
        /// access.  Alternatively, never clear NewResults and NewResults will contain
        /// all results.  NewResults is cleared internally whenever a new search is
        /// initiated.
        /// </summary>
        public List<ArchiveFilename> NewResults = new List<ArchiveFilename>();

        /// <summary>
        /// IsSearchComplete returns true if the current search operation is
        /// completed.
        /// </summary>
        public bool IsSearchComplete
        {
            get
            {
                return (IsArchiveNamesComplete || !IncludeArchiveNames)
                    && (IsFolderNamesComplete || !IncludeFolderNames);
            }
        }

        /// <summary>
        /// If PasswordBlockedSearch is true, then it indicates that the search could
        /// not include all archives because a different password was utilized on
        /// some archives.  Archives utilizing the current password are all included.
        /// </summary>
        public bool PasswordBlockedSearch = false;

        #endregion

        #region "Search State"

        void StartNewSearch()
        {
            lock (SearchStateLock)
            {
                lock (NewResults)
                {
                    NewResults.Clear();
                    AllResults.Clear();
                    IsArchiveNamesComplete = false;
                    IsFolderNamesComplete = false;

                    if (OnNewSearch != null) OnNewSearch();
                }
            }
        }

        public delegate void OnNewSearchHandler();
        public event OnNewSearchHandler OnNewSearch;

        object SearchStateLock = new object();
        bool IsArchiveNamesComplete = false;
        bool IsFolderNamesComplete = false;
        List<ArchiveFilename> AllResults = new List<ArchiveFilename>();        
        List<ArchiveFilename> NegFolderNameResults = new List<ArchiveFilename>();        

        #endregion

        #region "Error handling"

        object ExceptionLock = new object();
        Exception WorkerException = null;

        /// <summary>
        /// CheckForErrors() should be called on a GUI thread routinely to check for any exception conditions
        /// that occurred in the worker thread.  If an error occurred, it is thrown by CheckForErrors().
        /// </summary>
        public void CheckForErrors()
        {
            Exception exCopy = null;
            lock (ExceptionLock)
            {
                if (WorkerException != null)
                {
                    exCopy = WorkerException;
                    WorkerException = null;
                }
            }
            if (exCopy != null) throw exCopy;
        }

        #endregion

        #region "Initialization / Shutdown"

        Thread thWorker;
        bool Closing = false;

        public BackupSearch()
        {
            thWorker = new Thread(new ThreadStart(WorkerThread));
            thWorker.Start();
        }

        public void Dispose()
        {
            if (thWorker != null)
            {
                Closing = true;
                thWorker.Join();
                thWorker = null;
            }

            GC.SuppressFinalize(this);
        }

        ~BackupSearch() { Dispose(); }

        #endregion

        #region "Worker Thread"

        void WorkerThread()
        {
            try
            {
                while (!Closing)
                {
                    Thread.Sleep(0);

                    lock (SearchStateLock)
                    {
                        BackupProject Project = CurrentProject;
                        if (Project == null)
                        {
                            Thread.Sleep(100);
                            IsArchiveNamesComplete = true;
                            IsFolderNamesComplete = true;
                            continue;
                        }
                        string[] SearchWords = CurrentSearchWords;

                        if (SearchWords == null || SearchWords.Length == 0)
                        {
                            // In the case where the user has selected a project but no search details,
                            // we want to show all available backup archives.

                            if (!IsArchiveNamesComplete)
                            {
                                lock (Project.ArchiveFileList)
                                {
                                    foreach (ArchiveFilename Backup in Project.ArchiveFileList.Archives)
                                    {
                                        if (!AllResults.Contains(Backup))
                                        {
                                            AllResults.Add(Backup);
                                            lock (NewResults) NewResults.Add(Backup);
                                        }
                                    }
                                }

                                IsArchiveNamesComplete = true;
                            }
                            IsFolderNamesComplete = true;
                            Thread.Sleep(100);
                            continue;
                        }

                        if (!IsArchiveNamesComplete && IncludeArchiveNames)
                        {
                            lock (Project.ArchiveFileList)
                            {
                                foreach (ArchiveFilename Backup in Project.ArchiveFileList.Archives)
                                {
                                    string BackupFile = Backup.ToString().ToLower();

                                    bool Match = true;
                                    foreach (string Word in SearchWords) if (!BackupFile.Contains(Word)) { Match = false; break; }
                                    if (!Match) continue;

                                    if (!AllResults.Contains(Backup))
                                    {
                                        AllResults.Add(Backup);
                                        lock (NewResults) NewResults.Add(Backup);
                                    }
                                }
                            }
                            IsArchiveNamesComplete = true;
                            continue;
                        }

                        if (!IsFolderNamesComplete && IncludeFolderNames)
                        {
                            ArchiveFilename ExamineNext = null;
                            lock (Project.ArchiveFileList)
                            {
                                foreach (ArchiveFilename Backup in Project.ArchiveFileList.Archives)
                                {
                                    bool AlreadyDone = false;
                                    foreach (ArchiveFilename PosRes in AllResults)
                                    {
                                        if (Backup == PosRes) { AlreadyDone = true; break; }
                                    }
                                    if (AlreadyDone) continue;
                                    foreach (ArchiveFilename NegRes in NegFolderNameResults)
                                    {
                                        if (Backup == NegRes) { AlreadyDone = true; break; }
                                    }
                                    if (AlreadyDone) continue;
                                    ExamineNext = Backup;
                                    break;
                                }
                            }
                            if (ExamineNext == null)
                            {
                                IsFolderNamesComplete = true;
                                continue;
                            }

                            /** Examine a single archive on this pass of the loop **/

                            Manifest Manifest;
                            lock (Project.ManifestCache)
                            {
                                if (!Project.ManifestCache.TryGetValue(ExamineNext, out Manifest))
                                {
                                    try
                                    {
                                        using (Impersonator newself = new Impersonator(Project.BackupCredentials))
                                            Manifest = ExamineNext.LoadArchiveManifest(Project, false);
                                        Project.ManifestCache.Add(ExamineNext, Manifest);
                                    }
                                    catch (Ionic.Zip.BadPasswordException)
                                    {
                                        PasswordBlockedSearch = true;
                                        NegFolderNameResults.Add(ExamineNext);
                                        continue;
                                    }
                                }
                            }

                            if (SearchManifestForFolderName(ExamineNext, Manifest.ArchiveRoot, SearchWords))
                            {
                                if (!AllResults.Contains(ExamineNext))
                                {
                                    AllResults.Add(ExamineNext);
                                    lock (NewResults) NewResults.Add(ExamineNext);
                                }
                            }
                            else
                            {
                                NegFolderNameResults.Add(ExamineNext);
                            }

                            continue;
                        }

                        // If we reach this point, the search has already completed.  Idle time.
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                lock (ExceptionLock)
                {
                    WorkerException = ex;
                }
            }
        }

        bool SearchManifestForFolderName(ArchiveFilename Archive, Manifest.Folder ManifestFolder, string[] SearchWords)
        {            
            foreach (Manifest.Folder Subfolder in ManifestFolder.Folders)
            {
                bool Match = true;
                foreach (string Word in SearchWords)
                {
                    if (!Subfolder.Name.ToLowerInvariant().Contains(Word)) { Match = false; break; }
                }
                if (Match && WasUpdatedInArchive(Archive, Subfolder)) return true;
                if (SearchManifestForFolderName(Archive, Subfolder, SearchWords)) return true;                 
            }
            return false;
        }

        /// <summary>
        /// WasUpdatedInArchive() examines whether any files or subfolders of a specified folder
        /// within an archive actually contained any files within this archive.  Since a manifest
        /// contains a complete listing of all files and folders, including ones which are backed
        /// up by "external reference" (incremental backup), we must figure out if the file is
        /// actually present inside this archive when searching for it.
        /// </summary>
        /// <param name="Archive"></param>
        /// <param name="ManifestFolder"></param>
        /// <returns></returns>
        bool WasUpdatedInArchive(ArchiveFilename Archive, Manifest.Folder ManifestFolder)
        {
            foreach (Manifest.File File in ManifestFolder.Files)
            {
                if (File.ArchiveFile == Archive.ToString()) return true;
            }
            foreach (Manifest.Folder Subfolder in ManifestFolder.Folders)
            {
                if (WasUpdatedInArchive(Archive, Subfolder)) return true;
            }
            return false;
        }

        #endregion
    }
}
