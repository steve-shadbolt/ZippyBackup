using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Security.Authentication;
using System.Net;
using Alphaleonis.Win32.Filesystem;         // Replaces System.IO entities such as FileInfo with more advanced versions.
using ZippyBackup.IO;

namespace ZippyBackup.User_Interface
{
    public partial class BackupProjectForm : Form
    {
        /// <summary>
        /// m_Project contains a reference to the original project, and must be updated if OK is
        /// selected.
        /// </summary>
        private BackupProject m_Project;

        private List<string> ExcludeSubfolders = new List<string>();
        private List<string> ExcludeExtensions = new List<string>();
        private List<string> ExcludeFiles = new List<string>();

        /// <summary>
        /// Set Creating to true when this form is being used in a "New..." operation.  Set to
        /// false when being used in a "Edit..." operation.
        /// </summary>
        public bool Creating = true;

        private static long BytesPerGB = 1073741824 /*bytes/GB*/;
        private static long BytesPerMB = 1048576 /*bytes/MB*/;

        /// <summary>
        /// Project should be set to the backup project prior to calling ShowDialog().  After
        /// the user selects OK Project will contain the modified or final project.  If the
        /// user selects Cancel Project is not modified.
        /// </summary>
        public BackupProject Project
        {
            get { return m_Project; }
            set
            {
                m_Project = value;
                tbProjectName.Text = value.Name;
                tbSrcFolder.Text = value.SourceFolder;
                tbBackupFolder.Text = value.BackupFolder;

                ExcludeSubfolders.Clear();
                foreach (string ss in value.ExcludeSubfolders) ExcludeSubfolders.Add(ss);
                labelExcludedSubfolders.Text = "Exclude " + Utility.ListToEnglish(ExcludeSubfolders, Utility.ListToEnglishStyle.SingleLine, 43);

                ExcludeExtensions.Clear();
                foreach (string ss in value.ExcludeExtensions) ExcludeExtensions.Add(ss);
                labelExcludedExtensions.Text = Utility.ListToEnglish(ExcludeExtensions, Utility.ListToEnglishStyle.SingleLine, 50);

                ExcludeFiles.Clear();
                foreach (string ss in value.ExcludeFiles) ExcludeFiles.Add(ss);
                labelExcludedFiles.Text = Utility.ListToEnglish(ExcludeFiles, Utility.ListToEnglishStyle.SingleLine, 50);

                if (value.ExcludeFileSize == long.MaxValue) { cbExcludeSize.Checked = false; tbExcludeSize.Text = "1000.0"; }
                else { cbExcludeSize.Checked = true; tbExcludeSize.Text = (value.ExcludeFileSize / (double)BytesPerMB).ToString("F01"); }
                tbExcludeSize.Enabled = cbExcludeSize.Checked;

                if (string.IsNullOrEmpty(value.SafePassword.Password)) { rbNoPassword.Checked = true; rbAES.Checked = false; tbPassword.Text = ""; }
                else { rbNoPassword.Checked = false; rbAES.Checked = true; tbPassword.Text = value.SafePassword.Password; }

                cbUseVSS.Checked = value.UseVolumeShadowService;
                cbDoNotRemind.Checked = value.DoNotRemind;
            }
        }

        private void BackupProjectForm_Load(object sender, EventArgs e)
        {
            if (!Creating)
            {
                //tbProjectName.Enabled = false;
                //tbBackupFolder.Enabled = false;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                if (Snapshot != null) { Snapshot.Dispose(); Snapshot = null; }
            }
            base.Dispose(disposing);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            double ExcludeFileSize = double.PositiveInfinity;

            try
            {
                btnOK.Enabled = false;
                tbSrcFolder.Text = Utility.StripTrailingSlash(tbSrcFolder.Text);
                tbBackupFolder.Text = Utility.StripTrailingSlash(tbBackupFolder.Text);

                // If specifying a root folder, then we actually need the trailing slash due to some quarks in Windows.  It also looks nice.
                if (tbSrcFolder.Text.Length == 2 && tbSrcFolder.Text[1] == ':') tbSrcFolder.Text = tbSrcFolder.Text + "\\";
                if (tbBackupFolder.Text.Length == 2 && tbBackupFolder.Text[1] == ':') tbBackupFolder.Text = tbBackupFolder.Text + "\\";

                string CompleteBackupFolder;                

                for (; ; )
                {                    
                    try
                    {
                        Path.GetFullPath(tbBackupFolder.Text);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Please provide a valid backup folder path.");
                        return;
                    }
                    
                    CompleteBackupFolder = tbBackupFolder.Text + "\\" + tbProjectName.Text;
                    try
                    {
                        if (tbProjectName.Text.Contains("\\")) throw new Exception();
                        Path.GetFullPath(CompleteBackupFolder);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Please ensure that the project name is a valid filename.  It cannot include any of the following characters:  \\ / : * ? \" < > |");
                        return;
                    }
                    
                    try
                    {
                        if (m_Project.SourceCredentials.Provided)
                            using (Impersonator newself = new Impersonator(m_Project.SourceCredentials))
                            {
                                NativeDirectoryInfo.CheckAccessToDirectory(tbSrcFolder.Text);
                            }
                        else
                            NativeDirectoryInfo.CheckAccessToDirectory(tbSrcFolder.Text);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        if (MessageBox.Show("The source folder does not exist or is offline.  ZippyBackup cannot verify access to the folder at this time.", "Warning", MessageBoxButtons.OKCancel)
                            == System.Windows.Forms.DialogResult.Cancel) return;                        
                    }
                    catch (System.Security.Authentication.InvalidCredentialException)
                    {
                        MessageBox.Show("The credentials provided do not have access to the source folder.  Please use the Security tab to provide proper credentials.");
                        return;
                    }
                    catch (System.IO.IOException ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    try
                    {
                        if (m_Project.BackupCredentials.Provided)
                            using (Impersonator newself = new Impersonator(m_Project.BackupCredentials))
                            {
                                NativeDirectoryInfo.CheckAccessToDirectory(tbBackupFolder.Text);
                            }
                        else
                            NativeDirectoryInfo.CheckAccessToDirectory(tbBackupFolder.Text);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        if (MessageBox.Show("The backup folder does not exist.  Would you like to create it?", "Question", MessageBoxButtons.YesNo) == DialogResult.No)
                            return;
                        Directory.CreateDirectory(tbBackupFolder.Text);
                    }
                    catch (System.Security.Authentication.InvalidCredentialException)
                    {
                        MessageBox.Show("The credentials provided do not have access to the backup folder.  Please use the Security tab to provide proper credentials.");
                        return;
                    }
                    catch (System.IO.IOException ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }                    

#if false
                    if (cbUseVSS.Checked)
                    {
                        if (m_Project.BackupCredentials.Provided)
                        {
                            MessageBox.Show("Cannot support credentials for the backup user (besides the user launching ZippyBackup) and also for Volume Shadow Service access (administrator).");
                            return;
                        }

                        if (!m_Project.SourceCredentials.Provided)
                        {
                            NetworkCredential Credential = CredentialsPrompt.PromptForCredentials(
                                CredentialsPrompt.ServerNameFromPath(new DirectoryInfo(tbSrcFolder.Text).Root.Name),
                                "Credentials are required to take a Volume Shadow Service snapshot.");
                            if (Credential == null)
                            {
                                m_Project.SourceCredentials.Clear();
                                return;
                            }
                            m_Project.SourceCredentials = new StoredNetworkCredentials(Credential);
                        }
                    }
#endif

                    break;
                }

#               if false
                if (!Directory.Exists(tbSrcFolder.Text))
                {
                    MessageBox.Show("The source folder does not exist!  If the folder is offline, please bring it online for the setup process.");
                    return;
                }

                if (!Directory.Exists(tbBackupFolder.Text))
                {
                    if (MessageBox.Show("The backup folder does not exist.  Would you like to create it?", "Question", MessageBoxButtons.YesNo) == DialogResult.No)
                        return;
                    Directory.CreateDirectory(tbBackupFolder.Text);
                }
#               endif

                if (Utility.IsContainedIn(tbSrcFolder.Text, tbBackupFolder.Text))
                {
                    // This isn't acceptable by itself, but there's also the possibility that the user
                    // has excluded the backup folder from the backup.  Let's check...
                    string RelativePath = Utility.GetRelativePath(tbSrcFolder.Text, tbBackupFolder.Text);
                    bool IsExcluded = false;
                    foreach (string ExcludedRelativePath in ExcludeSubfolders)
                        if (Utility.IsContainedIn(ExcludedRelativePath, RelativePath)) { IsExcluded = true; break; }

                    if (!IsExcluded)
                    {
                        MessageBox.Show("The backup folder cannot be part of the backup.  Either change "
                            + "the backup folder so that it is not contained within the source folder "
                            + "or use the \"Edit...\" Subfolders button and uncheck the backup folder.");
                        return;
                    }
                }

                // Check if a backup with the same name already exists here.
                if (Creating && Directory.Exists(CompleteBackupFolder))
                {
                    DialogResult dr = MessageBox.Show("A backup project by this name already exists on disk.  If you are "
                        + "backing up the same content as in the past, this will work without any issues.  If you are backing "
                        + "up new content with the same name, then older backup files may have significantly different content "
                        + "than newer ones.  Nothing will be deleted, however, so the worst that will happen is a discontinuity "
                        + "in your backup content when you view or restore older backups.", "Confirm", MessageBoxButtons.OKCancel);
                    if (dr != DialogResult.OK) return;
                }
                else if (!Creating && (CompleteBackupFolder.ToLower() != m_Project.CompleteBackupFolder.ToLower()))
                {
                    DialogResult dr = MessageBox.Show("This change will relocate your existing backups for this project as follows:\n\n"
                        + "Previous Location:  " + m_Project.CompleteBackupFolder + "\n"
                        + "New Location:       " + CompleteBackupFolder + "\n"
                        + "\nWould you like ZippyBackup to move your existing backups to the new location?", "Question", MessageBoxButtons.YesNoCancel);
                    if (dr == DialogResult.Yes)
                    {
                        if (MoveBackups(m_Project.CompleteBackupFolder, CompleteBackupFolder))
                            MessageBox.Show("Your backups were moved successfully.", "Success");
                        else return;
                    }
                    if (dr == DialogResult.Cancel) return;
                }

                if (cbExcludeSize.Checked)
                {                    
                    if (!double.TryParse(tbExcludeSize.Text, out ExcludeFileSize))
                    {
                        MessageBox.Show("Please provide a valid file size for exclusions.");
                        return;
                    }
                }

                if (rbAES.Checked && tbPassword.Text.Length < 4)
                {
                    MessageBox.Show("Please provide a password of at least 4 characters or disable encryption.");
                    return;
                }

                if (rbAES.Checked && m_Project.SafePassword.Password != tbPassword.Text)
                {
                    PasswordForm pf = new PasswordForm();
                    pf.Prompt = "Please re-enter your password to confirm.";
                    if (pf.ShowDialog() != DialogResult.OK) return;
                    if (pf.Password != tbPassword.Text)
                    {
                        MessageBox.Show("The password confirmation did not match.  Please retype your password.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\nDetailed Info: " + ex.ToString());
                return;
            }
            finally { btnOK.Enabled = true; }

            m_Project.Name = tbProjectName.Text;
            m_Project.SourceFolder = tbSrcFolder.Text;
            m_Project.BackupFolder = tbBackupFolder.Text;
            m_Project.ExcludeSubfolders = ExcludeSubfolders;
            m_Project.ExcludeExtensions = ExcludeExtensions;
            m_Project.ExcludeFiles = ExcludeFiles;

            Directory.CreateDirectory(m_Project.CompleteBackupFolder);

            if (cbExcludeSize.Checked) m_Project.ExcludeFileSize = (long)(ExcludeFileSize * BytesPerMB);
            else m_Project.ExcludeFileSize = long.MaxValue;

            if (rbAES.Checked) m_Project.SafePassword.Password = tbPassword.Text; else m_Project.SafePassword.Password = null;

            m_Project.UseVolumeShadowService = cbUseVSS.Checked;
            m_Project.DoNotRemind = cbDoNotRemind.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        bool MoveBackups(string OldFolder, string NewFolder)
        {
            ProgressForm Progress = new ProgressForm();
            Progress.OverallProgressBar.Maximum = 100;
            Progress.OverallProgressBar.Minimum = 0;
            Progress.OverallProgressBar.Value = 0;
            Progress.CurrentProgressBar.Visible = false;
            Progress.label1.Text = "Relocating backup files for project '" + Project.Name + "'...";
            Progress.label2.Text = "";
            Progress.CancelPrompt = "Cancelling may leave files in a mixed state/location.  Are you sure you wish to cancel this move?";
            Progress.Show();

            try
            {
                for (; ; )
                {
                    try
                    {
                        DirectoryInfo diSource = new DirectoryInfo(OldFolder);
                        System.IO.DirectoryInfo diDest = new System.IO.DirectoryInfo(NewFolder);

                        if (!diSource.Exists)
                            throw new Exception("The source directory '" + diSource.FullName + "' was not found or is offline.");

                        if (!diDest.Exists)
                            diDest.Create();

                        FileInfo[] SourceList = diSource.GetFiles("Backup_*.zip");
                        Progress.OverallProgressBar.Maximum = SourceList.Length;
                        Progress.OverallProgressBar.Value = 0;
                        int Completed = 0;
                        foreach (FileInfo fiSource in SourceList)
                        {
                            Progress.label2.Text = "Moving file:  " + fiSource.Name;
                            Progress.OverallProgressBar.Value = Completed;
                            Application.DoEvents();
                            if (Progress.Cancel) throw new CancelException();

                            string NewName = Utility.StripTrailingSlash(diDest.FullName) + "\\" + fiSource.Name;
                            fiSource.MoveTo(NewName);
                            Completed++;
                        }
                        
                        return true;
                    }
                    catch (CancelException) { return false; }
                    catch (Exception ex)
                    {
                        DialogResult dr = MessageBox.Show(ex.Message, "Error",
                                MessageBoxButtons.RetryCancel);
                        if (dr == DialogResult.Retry) continue;
                        if (dr == DialogResult.Cancel) return false;
                    }
                }
            }
            finally
            {
                Progress.Dispose();
            }
        }        

        public BackupProjectForm()
        {
            InitializeComponent();
        }

        private void btnBrowseSrc_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please select folder to be backed up...";
            if (fbd.ShowDialog() == DialogResult.OK) tbSrcFolder.Text = fbd.SelectedPath;
        }

        private void btnBrowseBackup_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please select the location to store backups...";
            fbd.ShowNewFolderButton = true;
            if (fbd.ShowDialog() == DialogResult.OK) tbBackupFolder.Text = fbd.SelectedPath;
        }

        #region "Subfolder Scan"

        TreeNode StartTreeEntry(DirectoryInfo diFolder, string BasePath, string Path, bool Checked)
        {
            string RelativePath = Utility.GetRelativePath(BasePath, Path);
            string Name = Utility.GetFileName(diFolder.FullName);
            //MessageBox.Show("Adding Folder Name '" + Name + "'\nBasePath= " + BasePath + "\nPath= " + Path + "\nRelative Path= " + RelativePath);
            TreeNode Folder = new TreeNode(Name);
            Folder.Tag = RelativePath;
            Folder.Checked = Checked;
            if (Folder.Checked)
            {
                foreach (string ExcludedRelativePath in ExcludeSubfolders)
                    if (ExcludedRelativePath.ToLower() == RelativePath.ToLower()) { Folder.Checked = false; break; }
            }
            return Folder;
        }

        /// <summary>
        /// Precondition:  Should be enclosed in a using (Impersonator ...) block for access to
        /// the path.
        /// </summary>        
        /// <param name="BasePath"></param>
        /// <param name="Path"></param>
        /// <param name="ParentChecked"></param>
        /// <returns></returns>
        TreeNode GetTree(string BasePath, string Path, bool ParentChecked, int Depth)
        {
            ZippyForm.LogWriteLine(LogLevel.HeavyDebug, "Scanning directory structure at '" + Path + "'.");
            Path = Utility.EnsureTrailingSlash(Path);
            DirectoryInfo diFolder = new DirectoryInfo(Path);
            TreeNode Folder = StartTreeEntry(diFolder, BasePath, Path, ParentChecked);
            DirectoryInfo[] Subfolders = diFolder.GetDirectories();
            for (int ii = 0; ii < Subfolders.Length; ii++)
            {
                DirectoryInfo diSubfolder = Subfolders[ii];
                if (Depth == 0) ScanProgress.OverallProgressBar.Value = ii; 
                ScanProgress.label2.Text = "Scanning '" + Utility.GetRelativePath(BasePath, diSubfolder.FullName) + "'...";                
                DoEvents();

                try
                {
                    if (SymbolicLink.IsLink(diSubfolder.FullName)) continue;
                    Folder.Nodes.Add(GetTree(BasePath, diSubfolder.FullName, Folder.Checked, Depth + 1));
                }
                catch (CancelException ex) { throw ex; }
                catch (UnauthorizedAccessException ex)
                {
                    if (Folder.Checked)
                    {
                        switch (MessageBox.Show("Error while scanning folder '" + diSubfolder.FullName + "': " + ex.Message +
                            "\nThe Volume Shadow Snapshot option can be used to address in-use files.\n\nDo you want to exclude this folder and all subfolders?", "Error while scanning", MessageBoxButtons.YesNoCancel))
                        {
                            case DialogResult.Yes:
                                Folder.Nodes.Add(StartTreeEntry(diSubfolder, BasePath, diSubfolder.FullName, false));
                                continue;
                            case DialogResult.No:
                                MessageBox.Show("Please correct the error preventing ZippyBackup from accessing the folder and try again.");
                                throw new CancelException();
                            case DialogResult.Cancel:
                                throw new CancelException();
                            default: throw ex;
                        }
                    }
                    else
                    {
                        Folder.Nodes.Add(StartTreeEntry(diSubfolder, BasePath, diSubfolder.FullName, false));
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    if (Folder.Checked)
                    {
                        switch (MessageBox.Show("Error while scanning folder '" + diSubfolder.FullName + "': " + ex.Message +
                            "\n\nDo you want to exclude this folder and all subfolders?", "Error while scanning", MessageBoxButtons.YesNoCancel))
                        {
                            case DialogResult.Yes:
                                Folder.Nodes.Add(StartTreeEntry(diSubfolder, BasePath, diSubfolder.FullName, false));
                                continue;
                            case DialogResult.No:
                                MessageBox.Show("Please correct the error preventing ZippyBackup from accessing the folder and try again.");
                                throw new CancelException();
                            case DialogResult.Cancel:
                                throw new CancelException();
                            default: throw ex;
                        }
                    }
                    else
                    {
                        Folder.Nodes.Add(StartTreeEntry(diSubfolder, BasePath, diSubfolder.FullName, false));
                        continue;
                    }
                }
            }
            return Folder;
        }

        void UncheckedTreeToList(List<string> ExcludedList, TreeNode Node)
        {
            // If a folder is unchecked, then none of its subfolders can be included
            // either.  Therefore we don't need to recurse when it is not checked.
            if (!Node.Checked)
            {
                string RelativePath = Node.Tag as string;
                ExcludedList.Add(RelativePath);
            }
            else
            {
                foreach (TreeNode Subnode in Node.Nodes) UncheckedTreeToList(ExcludedList, Subnode);
            }
        }        

        ProgressForm ScanProgress;

        /// <summary>
        /// When a project has the "Use Volume Shadow Service (VSS)" option set, we need to
        /// create a shadow snapshot of the volume at the start of each scan operation.
        /// Files on the shadow can only be accessed via the Alphaleonis.Win32.FileSystem 
        /// namespace, which replaces built-in classes such as FileInfo and DirectoryInfo.  
        /// Some additional replacements happen in ZippyBackup.IO.
        /// </summary>
        ShadowVolume Snapshot;

        /// <summary>
        /// Gives the root folder for enumerating and reading files for the backup.  This
        /// can be equal to Project.SourceFolder, but if VSS is enabled then it is a dynamically
        /// generated location.
        /// </summary>
        string SourceRoot;

        void DoEvents()
        {
            Application.DoEvents();
            if (ScanProgress != null && ScanProgress.Cancel) throw new CancelException();
        }

        void OnVSSStatus(string Message)
        {
            ScanProgress.label3.Text = Message;
        }
        
        private void btnSubfolders_Click(object sender, EventArgs e)
        {
            btnSubfolders.Enabled = false;
            try
            {
                ZippyForm.LogWriteLine(LogLevel.LightDebug, "Starting directory structure scan...");

                ScanProgress = new ProgressForm();
                ScanProgress.Text = "Directory scan in progress";
                ScanProgress.CurrentProgressBar.Visible = false;
                ScanProgress.label1.Text = "Scanning directory structure...";
                ScanProgress.label2.Text = "";
                ScanProgress.label3.Visible = false;
                ScanProgress.OverallProgressBar.Value = 0;      // Will be setup later in the process, after VSS...
                ScanProgress.CancelPrompt = "Are you sure you wish to cancel this directory scan?";
                ScanProgress.Show();
                DoEvents();

                TreeSelectForm tsf = new TreeSelectForm();
                using (Impersonator newself = new Impersonator(m_Project.SourceCredentials))
                {
                    // Capture Volume Shadow Snapshot if applicable...
                    SourceRoot = tbSrcFolder.Text;

                    try
                    {
                        if (Project.UseVolumeShadowService)
                        {
                            ScanProgress.label2.Text = "Capturing VSS shapshot...";
                            DoEvents();
                            try
                            {
                                string Volume = new DirectoryInfo(SourceRoot).Root.FullName;

                                ZippyForm.LogWriteLine(LogLevel.MediumDebug, "Initializing Volume Shadow Service (VSS) Interface...");
                                Snapshot = new ShadowVolume(Volume);
                                Snapshot.OnStatusUpdate += new ShadowVolume.UpdateStatus(OnVSSStatus);
                                ZippyForm.LogWriteLine(LogLevel.LightDebug, "Starting shadow snapshot capture...");
                                Snapshot.TakeSnapshot();

                                string RelativeToVolume = Utility.GetRelativePath(Volume, tbSrcFolder.Text);
                                SourceRoot = Snapshot.ShadowRoot + "\\" + RelativeToVolume;
                                ZippyForm.LogWriteLine(LogLevel.LightDebug, "Shadow snapshot captured at '" + Snapshot.ShadowRoot + "'...");
                                ZippyForm.LogWriteLine(LogLevel.LightDebug, "Working source root at '" + SourceRoot + "'.");
                            }
                            catch (Exception exc)
                            {
                                throw new Exception("While taking VSS snapshot: " + exc + "\nStatus at error: " + ScanProgress.label3.Text, exc);
                            }
                        }

                        ScanProgress.label2.Text = "Starting scan...";
                        ScanProgress.label3.Text = "";

                        // Estimate the number of folders to scan for progress bar...
                        DirectoryInfo diSrc = new DirectoryInfo(SourceRoot);
                        DirectoryInfo[] diSubfolders = diSrc.GetDirectories();
                        ScanProgress.OverallProgressBar.Minimum = 0;
                        ScanProgress.OverallProgressBar.Maximum = diSubfolders.Length;
                        ScanProgress.OverallProgressBar.Value = 0;
                        ScanProgress.Show();
                        DoEvents();

                        tsf.Tree.Nodes.Add(GetTree(SourceRoot, SourceRoot, true, 0));
                        tsf.Tree.Nodes[0].Text = Utility.GetFileName(tbSrcFolder.Text);
                    }
                    finally
                    {
                        if (Snapshot != null) { Snapshot.Dispose(); Snapshot = null; }
                    }
                }
                ScanProgress.Dispose();
                ScanProgress = null;
                tsf.Tree.Nodes[0].Expand();
                tsf.ShowDialog();

                ExcludeSubfolders = new List<string>();
                UncheckedTreeToList(ExcludeSubfolders, tsf.Tree.Nodes[0]);
                labelExcludedSubfolders.Text = "Exclude " + Utility.ListToEnglish(ExcludeSubfolders, Utility.ListToEnglishStyle.SingleLine, 43);
            }
            catch (CancelException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (ScanProgress != null) { ScanProgress.Dispose(); ScanProgress = null; }
                btnSubfolders.Enabled = true;
            }
        }

        #endregion

        private void btnFileExtensions_Click(object sender, EventArgs e)
        {
            FilePatternsForm fef = new FilePatternsForm();
            fef.Prompt = "Select extensions to exclude";
            fef.Extensions = ExcludeExtensions;
            fef.ShowDialog();
            ExcludeExtensions = fef.Extensions;
            labelExcludedExtensions.Text = Utility.ListToEnglish(ExcludeExtensions, Utility.ListToEnglishStyle.SingleLine, 50);
        }

        private void btnFiles_Click(object sender, EventArgs e)
        {
            ExcludedFilesForm eff = new ExcludedFilesForm();
            eff.BasePath = Utility.StripTrailingSlash(tbSrcFolder.Text);
            eff.Prompt = "Select specific file(s) to exclude";
            eff.Files = ExcludeFiles;
            eff.ShowDialog();
            ExcludeFiles = eff.Files;
            labelExcludedFiles.Text = Utility.ListToEnglish(ExcludeFiles, Utility.ListToEnglishStyle.SingleLine, 50);
        }

        private void cbExcludeSize_CheckedChanged(object sender, EventArgs e)
        {
            tbExcludeSize.Enabled = cbExcludeSize.Checked;
        }

        private void rbNoPassword_CheckedChanged(object sender, EventArgs e)
        {
            rbAES.Checked = !rbNoPassword.Checked;
            tbPassword.Enabled = rbAES.Checked;
        }

        private void rbAES_CheckedChanged(object sender, EventArgs e)
        {
            rbNoPassword.Checked = !rbAES.Checked;
            tbPassword.Enabled = rbAES.Checked;
        }

        private void linkEncryptionInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(
                "ZippyBackup supports 256-bit Advanced Encryption Standard (AES) backups.  AES is "
                + "used by the United States National Security Agency for top secret information.\n\n"

                + "The zip file format supports 256-bit AES archives, but there are some limitations.  "
                + "In particular, not all software which can read zip files can read AES.  That "
                + "includes Windows Explorer.  WinZip and ZippyBackup can both access it.\n\n"

                + "ZippyBackup goes a step further than WinZip and also encrypts the file and directory names "
                + "in your archives.  This means that WinZip can access the files but their filenames are "
                + "not apparent.  The information is stored inside of an XML text file within the archive.  "
                + "While other software may not be able to view the encrypted archives, ZippyBackup "
                + "can access your encrypted backup files seemlessly.\n\n",
                "Encryption Information");
        }

        private void cbUseVSS_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnManageSourceCredentials_Click(object sender, EventArgs e)
        {
            NetworkCredential Credential = CredentialsPrompt.PromptForCredentials(
                            CredentialsPrompt.ServerNameFromPath(tbSrcFolder.Text),
                            "Please provide credentials to access this location.");
            if (Credential == null)
            {
                m_Project.SourceCredentials.Clear();
                return;
            }
            m_Project.SourceCredentials = new StoredNetworkCredentials(Credential);
        }

        private void btnManageBackupCredentials_Click(object sender, EventArgs e)
        {
            NetworkCredential Credential = CredentialsPrompt.PromptForCredentials(
                            CredentialsPrompt.ServerNameFromPath(tbBackupFolder.Text),
                            "Please provide credentials to access this location.");
            if (Credential == null)
            {
                m_Project.BackupCredentials.Clear();
                return;
            }
            m_Project.BackupCredentials = new StoredNetworkCredentials(Credential);
        }
    }
}
