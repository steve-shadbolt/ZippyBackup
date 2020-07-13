using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZippyBackup.User_Interface
{
    public partial class RestoreForm : ZippyForm
    {
        public BackupSearch Searcher;

        /// <summary>
        /// If we are accessing a project that is not part of the main list we will create a temporary
        /// project for it.  This will need to be cleaned up, so we keep a reference to it for disposal.
        /// </summary>
        private BackupProject TemporaryProject;

        #region "Initialization / Shutdown"

        public RestoreForm()
        {
            InitializeComponent();
        }

        public RestoreForm(string ConfigFile)
        {
            LoadConfigFromFile(ConfigFile);
            InitializeComponent();
        }

        public RestoreForm(string[] CommandLineArgs)
        {
            if (CommandLineArgs.Length < 1)            
                throw new Exception("Please specify a configuration file for ZippyRestore.");                            

            try
            {
                LoadConfigFromFile(CommandLineArgs[0]);
            }
            catch (Exception exc)
            {
                throw new Exception("Unable to load configuration:  " + exc.Message);                
            }

            InitializeComponent();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            UpdateGUI();
            Searcher = new BackupSearch();
            Searcher.OnNewSearch += new BackupSearch.OnNewSearchHandler(OnNewSearchStarted);

            labelSearchMsg.Text = "";

            GUIFastRefreshTimer.Enabled = true;
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            if (Searcher != null)
            {
                Searcher.Dispose();
                Searcher = null;
            }

            if (TemporaryProject != null)
            {
                TemporaryProject.Dispose();
                TemporaryProject = null;
            }
        }

        #endregion

        void OnNewSearchStarted()
        {
            ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "A new search has started...");
            listRestoreDates.Items.Clear();
        }

        public void UpdateGUI()
        {
            Populate(listRestoreBackups);
        }

        void Populate(ListBox lb)
        {
            ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "RestoreForm populating projects list...");

            lb.Sorted = false;
            lb.Items.Clear();
            lock (MainList.Projects)
            {
                foreach (BackupProject bp in MainList.Projects)
                {
                    lb.Items.Add(bp);
                }
            }
        }

        private void listRestoreBackups_SelectedIndexChanged(object sender, EventArgs e)
        {
            BackupProject Project = listRestoreBackups.SelectedItem as BackupProject;
            Searcher.CurrentProject = Project;      // null is an acceptable value here.  This assignment triggers OnNewSearchStarted().
        }

        private void btnViewBackup_Click(object sender, EventArgs e)
        {
            //BackupProject Project = listRestoreBackups.SelectedItem as BackupProject;
            BackupProject Project = Searcher.CurrentProject;
            if (Project == null) return;

            ArchiveFilename Archive = listRestoreDates.SelectedItem as ArchiveFilename;
            if (Archive == null) return;

            ArchiveExplorerForm aef = new ArchiveExplorerForm(Project, Archive);
            aef.Show();
        }

        bool UpdateSearchResults_InProgress = false;
        void UpdateSearchResults()
        {
            // Called routinely by GUIFastRefreshTimer.

            // Prevent re-entry...
            if (UpdateSearchResults_InProgress) return;
            UpdateSearchResults_InProgress = true;
            try
            {
                try
                {
                    Searcher.CheckForErrors();
                }
                catch (Exception ex)
                {
                    #if DEBUG
                    MessageBox.Show("An error occurred while performing search: " + ex.ToString());
                    #else
                    MessageBox.Show("An error occurred while performing search: " + ex.Message);
                    #endif
                    GUIFastRefreshTimer.Enabled = false;            // Prevent further errors from appearing, though this shouldn't happen.
                }

                try
                {
                    lock (Searcher.NewResults)
                    {
                        foreach (ArchiveFilename Archive in Searcher.NewResults)
                            //listRestoreDates.Items.Add(Archive);                    
                            listRestoreDates.AddSorted(Archive);

                        Searcher.NewResults.Clear();
                    }

                    string Status;
                    if (Searcher.CurrentProject == null) Status = "Select a project.";
                    else if (Searcher.IsSearchComplete)
                    {
                        if (Searcher.PasswordBlockedSearch)
                            Status = "Search complete.  Password blocked some archives.";
                        else
                            Status = "Search complete.";
                    }
                    else Status = "Search in progress.";
                    labelSearchMsg.Text = Status;
                }
                catch (Exception ex)
                {
                    #if DEBUG
                    MessageBox.Show("An error occurred while updating search results: " + ex.ToString());
                    #else
                    MessageBox.Show("An error occurred while updating search results: " + ex.Message);
                    #endif
                    GUIFastRefreshTimer.Enabled = false;            // Prevent further errors from appearing, though this shouldn't happen.
                }                
            }
            finally
            {
                UpdateSearchResults_InProgress = false;
            }
        }

        private void GUIFastRefreshTimer_Tick(object sender, EventArgs e)
        {
            UpdateSearchResults();
        }

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            //ArchiveFilename Selected = listRestoreDates.SelectedItem as ArchiveFilename;            
            string[] SearchWords = tbSearch.Text.ToLower().Split(new char[] { ' ' });
            Searcher.CurrentSearchWords = SearchWords;      // Triggers a new search.
        }

        private void cbSearchArchiveNames_CheckedChanged(object sender, EventArgs e)
        {
            Searcher.IncludeArchiveNames = cbSearchArchiveNames.Checked;    // Triggers new search.
        }

        private void cbSearchFolderNames_CheckedChanged(object sender, EventArgs e)
        {
            Searcher.IncludeFolderNames = cbSearchFolderNames.Checked;      // Triggers new search.
        }

        private void listRestoreDates_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnViewBackup.Enabled = (listRestoreDates.SelectedItem != null);
        }

        private void listRestoreDates_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            btnViewBackup_Click(null, null);
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            tbProjectFolder.Text = fbd.SelectedPath;

            // Create a temporary, in-memory backup project with just enough information for our restoration purposes...
            BackupProject Project = new BackupProject();
            Project.Name = System.IO.Path.GetFileName(fbd.SelectedPath);
            Project.BackupFolder = System.IO.Path.GetDirectoryName(fbd.SelectedPath);
            Cursor.Current = Cursors.WaitCursor;            // Set cursor as hourglass
            Project.Refresh(true);
            Cursor.Current = Cursors.Default;               // Set cursor as default arrow

            Searcher.CurrentProject = Project;      // null is an acceptable value here.  This assignment triggers OnNewSearchStarted().

            // Store a reference to this temporary project for later disposal.
            if (TemporaryProject != null) TemporaryProject.Dispose();
            TemporaryProject = Project;
        }
    }
}
