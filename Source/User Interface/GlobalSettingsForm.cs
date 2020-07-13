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
    public partial class GlobalSettingsForm : Form
    {
        /// <summary>
        /// m_MainList contains a reference to the primary ArchiveFileList and should only
        /// be modified when the user pressed OK.  Note: BackupList.Projects is lock-protected
        /// and should only be accessed through a lock.
        /// </summary>
        private BackupList m_MainList;

        public MainForm MainForm;

        private List<string> ExcludeExtensions;
        private List<string> CompressedExtensions;
        private List<string> ExcludeFolderPatterns;

        private static long BytesPerGB = 1073741824 /*bytes/GB*/;

        public BackupList GlobalSettings
        {
            set
            {
                m_MainList = value;
                ExcludeExtensions = m_MainList.ExcludeExtensions;
                CompressedExtensions = m_MainList.CompressedExtensions;
                ExcludeFolderPatterns = m_MainList.ExcludeFolderPatterns;
                labelExcludedFileExtensions.Text = Utility.ListToEnglish(ExcludeExtensions, Utility.ListToEnglishStyle.SingleLine, 48);
                labelCompressedFileExtensions.Text = Utility.ListToEnglish(CompressedExtensions, Utility.ListToEnglishStyle.SingleLine, 48);
                labelExcludedFolderPatterns.Text = Utility.ListToEnglish(ExcludeFolderPatterns, Utility.ListToEnglishStyle.SingleLine, 48);
                if (double.IsInfinity(m_MainList.ConstrainArchiveSize))
                {
                    cbConstrainArchiveSize.Checked = false;
                    tbConstrainArchiveSizeInGB.Text = "10.0";
                }
                else
                {
                    cbConstrainArchiveSize.Checked = true;
                    tbConstrainArchiveSizeInGB.Text = (m_MainList.ConstrainArchiveSize / (double)BytesPerGB).ToString("F01");
                }
                tbLogfile.Text = m_MainList.Logfile;
                cbLogLevel.SelectedItem = m_MainList.Logging;
            }
        }

        public GlobalSettingsForm()
        {
            InitializeComponent();

            foreach (LogLevel level in (LogLevel[])Enum.GetValues(typeof(LogLevel)))
            {
                cbLogLevel.Items.Add(level);
            }
        }

        private void btnExcludeFileExtensions_Click(object sender, EventArgs e)
        {
            FilePatternsForm fef = new FilePatternsForm();
            fef.Text = "Select extensions to exclude";
            fef.Prompt = "Select file extensions which will be\n"
                       + "excluded from all backups.";
            fef.Extensions = ExcludeExtensions;
            fef.ShowDialog();
            ExcludeExtensions = fef.Extensions;
            labelExcludedFileExtensions.Text = Utility.ListToEnglish(ExcludeExtensions, Utility.ListToEnglishStyle.SingleLine, 48);
        }

        private void btnCompressedFileExtensions_Click(object sender, EventArgs e)
        {
            FilePatternsForm fef = new FilePatternsForm();
            fef.Text = "Identify already compressed files";
            fef.Prompt = "Identify file extensions which represent\n"
                       + "compressed files.  These files will be\n"
                       + "included in backup archives, but no\n"
                       + "attempt to further compress them will\n"
                       + "be made.";
            fef.Extensions = CompressedExtensions;
            fef.Builtin = Globals.BuiltinCompressedExtensions;
            fef.ShowDialog();
            CompressedExtensions = fef.Extensions;
            labelCompressedFileExtensions.Text = Utility.ListToEnglish(CompressedExtensions, Utility.ListToEnglishStyle.SingleLine, 48);
        }

        private void btnExcludedFolderPatterns_Click(object sender, EventArgs e)
        {
            FilePatternsForm fef = new FilePatternsForm();
            fef.FilesMode = false;
            fef.Text = "Identify folder name patterns to exclude";
            fef.Prompt = "Identify folder filenames or patterns\n"
                       + "that should be excluded from all\n"
                       + "backups.";

            fef.Patterns = ExcludeFolderPatterns;
            fef.Builtin = Globals.BuiltinExcludeFolderPatterns;
            fef.ShowDialog();
            ExcludeFolderPatterns = fef.Patterns;
            labelExcludedFolderPatterns.Text = Utility.ListToEnglish(ExcludeFolderPatterns, Utility.ListToEnglishStyle.SingleLine, 48);
        }        

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                LogLevel PrevLogLevel = m_MainList.Logging;

                m_MainList.ExcludeExtensions = ExcludeExtensions;
                m_MainList.CompressedExtensions = CompressedExtensions;
                m_MainList.ExcludeFolderPatterns = ExcludeFolderPatterns;
                m_MainList.ConstrainArchiveSize = cbConstrainArchiveSize.Checked
                    ? (long)(double.Parse(tbConstrainArchiveSizeInGB.Text) * BytesPerGB) : long.MaxValue;                
                m_MainList.Logging = (LogLevel)cbLogLevel.SelectedItem;
                m_MainList.Logfile = tbLogfile.Text;

                if (PrevLogLevel != m_MainList.Logging)
                    ZippyForm.LogWriteLine(LogLevel.Information, "Log detail level changed to " + m_MainList.Logging.ToString() + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cbConstrainArchiveSize_CheckedChanged(object sender, EventArgs e)
        {
            tbConstrainArchiveSizeInGB.Enabled = cbConstrainArchiveSize.Checked;
        }

        private void btnExportConfiguration_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "ZippyBackup Configuration Files (*.xml)|*.xml";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            MainForm.ExportConfig(sfd.FileName);
        }

        private void btnImportConfiguration_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Importing a configuration will overwrite ALL settings in your current configuration.  Do you wish to continue?", "Confirm", MessageBoxButtons.OKCancel)
                != DialogResult.OK) return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "ZippyBackup Configuration Files (*.xml)|*.xml";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            MainForm.ImportConfig(ofd.FileName);
        }

        private void GlobalSettingsForm_Load(object sender, EventArgs e)
        {
        }

        private void btnLogBrowse_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.OverwritePrompt = false;
            sfd.Filter = "Text log file (*.log)|*.log|All files (*.*)|*.*";
            if (sfd.ShowDialog() != DialogResult.OK) return;
            tbLogfile.Text = sfd.FileName;
        }        
    }
}
