namespace ZippyBackup.User_Interface
{
    partial class GlobalSettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GlobalSettingsForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbLogLevel = new System.Windows.Forms.ComboBox();
            this.btnLogBrowse = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.tbLogfile = new System.Windows.Forms.TextBox();
            this.lblLogfile = new System.Windows.Forms.Label();
            this.btnImportConfiguration = new System.Windows.Forms.Button();
            this.btnExportConfiguration = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbConstrainArchiveSizeInGB = new System.Windows.Forms.TextBox();
            this.cbConstrainArchiveSize = new System.Windows.Forms.CheckBox();
            this.btnCompressedFileExtensions = new System.Windows.Forms.Button();
            this.btnExcludeFileExtensions = new System.Windows.Forms.Button();
            this.labelCompressedFileExtensions = new System.Windows.Forms.Label();
            this.labelExcludedFileExtensions = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.labelExcludedFolderPatterns = new System.Windows.Forms.Label();
            this.btnExcludedFolderPatterns = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnExcludedFolderPatterns);
            this.groupBox1.Controls.Add(this.labelExcludedFolderPatterns);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.cbLogLevel);
            this.groupBox1.Controls.Add(this.btnLogBrowse);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.tbLogfile);
            this.groupBox1.Controls.Add(this.lblLogfile);
            this.groupBox1.Controls.Add(this.btnImportConfiguration);
            this.groupBox1.Controls.Add(this.btnExportConfiguration);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.tbConstrainArchiveSizeInGB);
            this.groupBox1.Controls.Add(this.cbConstrainArchiveSize);
            this.groupBox1.Controls.Add(this.btnCompressedFileExtensions);
            this.groupBox1.Controls.Add(this.btnExcludeFileExtensions);
            this.groupBox1.Controls.Add(this.labelCompressedFileExtensions);
            this.groupBox1.Controls.Add(this.labelExcludedFileExtensions);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(560, 321);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "All Backups";
            // 
            // cbLogLevel
            // 
            this.cbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLogLevel.FormattingEnabled = true;
            this.cbLogLevel.Location = new System.Drawing.Point(125, 211);
            this.cbLogLevel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cbLogLevel.Name = "cbLogLevel";
            this.cbLogLevel.Size = new System.Drawing.Size(426, 21);
            this.cbLogLevel.TabIndex = 28;
            // 
            // btnLogBrowse
            // 
            this.btnLogBrowse.Location = new System.Drawing.Point(454, 234);
            this.btnLogBrowse.Name = "btnLogBrowse";
            this.btnLogBrowse.Size = new System.Drawing.Size(97, 23);
            this.btnLogBrowse.TabIndex = 27;
            this.btnLogBrowse.Text = "&Browse...";
            this.btnLogBrowse.UseVisualStyleBackColor = true;
            this.btnLogBrowse.Click += new System.EventHandler(this.btnLogBrowse_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 239);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 26;
            this.label6.Text = "Logfile:";
            // 
            // tbLogfile
            // 
            this.tbLogfile.Location = new System.Drawing.Point(125, 236);
            this.tbLogfile.Name = "tbLogfile";
            this.tbLogfile.Size = new System.Drawing.Size(309, 20);
            this.tbLogfile.TabIndex = 25;
            // 
            // lblLogfile
            // 
            this.lblLogfile.AutoSize = true;
            this.lblLogfile.Location = new System.Drawing.Point(18, 213);
            this.lblLogfile.Name = "lblLogfile";
            this.lblLogfile.Size = new System.Drawing.Size(76, 13);
            this.lblLogfile.TabIndex = 24;
            this.lblLogfile.Text = "Logging detail:";
            // 
            // btnImportConfiguration
            // 
            this.btnImportConfiguration.Location = new System.Drawing.Point(283, 282);
            this.btnImportConfiguration.Name = "btnImportConfiguration";
            this.btnImportConfiguration.Size = new System.Drawing.Size(169, 23);
            this.btnImportConfiguration.TabIndex = 23;
            this.btnImportConfiguration.Text = "Import Configuration...";
            this.btnImportConfiguration.UseVisualStyleBackColor = true;
            this.btnImportConfiguration.Click += new System.EventHandler(this.btnImportConfiguration_Click);
            // 
            // btnExportConfiguration
            // 
            this.btnExportConfiguration.Location = new System.Drawing.Point(108, 282);
            this.btnExportConfiguration.Name = "btnExportConfiguration";
            this.btnExportConfiguration.Size = new System.Drawing.Size(169, 23);
            this.btnExportConfiguration.TabIndex = 22;
            this.btnExportConfiguration.Text = "Export Configuration...";
            this.btnExportConfiguration.UseVisualStyleBackColor = true;
            this.btnExportConfiguration.Click += new System.EventHandler(this.btnExportConfiguration_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(37, 168);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(371, 26);
            this.label5.TabIndex = 21;
            this.label5.Text = "When limit is exceeded, multiple backup archive files will be generated.  Limit \r" +
    "\napplies to uncompressed size of data to be contained in an archive.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(532, 152);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(22, 13);
            this.label4.TabIndex = 20;
            this.label4.Text = "GB";
            // 
            // tbConstrainArchiveSizeInGB
            // 
            this.tbConstrainArchiveSizeInGB.Location = new System.Drawing.Point(454, 146);
            this.tbConstrainArchiveSizeInGB.Name = "tbConstrainArchiveSizeInGB";
            this.tbConstrainArchiveSizeInGB.Size = new System.Drawing.Size(69, 20);
            this.tbConstrainArchiveSizeInGB.TabIndex = 19;
            this.tbConstrainArchiveSizeInGB.Text = "10.0";
            // 
            // cbConstrainArchiveSize
            // 
            this.cbConstrainArchiveSize.AutoSize = true;
            this.cbConstrainArchiveSize.Location = new System.Drawing.Point(21, 148);
            this.cbConstrainArchiveSize.Name = "cbConstrainArchiveSize";
            this.cbConstrainArchiveSize.Size = new System.Drawing.Size(245, 17);
            this.cbConstrainArchiveSize.TabIndex = 18;
            this.cbConstrainArchiveSize.Text = "Limit maximum size of a single backup archive:";
            this.cbConstrainArchiveSize.UseVisualStyleBackColor = true;
            this.cbConstrainArchiveSize.CheckedChanged += new System.EventHandler(this.cbConstrainArchiveSize_CheckedChanged);
            // 
            // btnCompressedFileExtensions
            // 
            this.btnCompressedFileExtensions.Location = new System.Drawing.Point(454, 60);
            this.btnCompressedFileExtensions.Name = "btnCompressedFileExtensions";
            this.btnCompressedFileExtensions.Size = new System.Drawing.Size(97, 23);
            this.btnCompressedFileExtensions.TabIndex = 17;
            this.btnCompressedFileExtensions.Text = "Edit...";
            this.btnCompressedFileExtensions.UseVisualStyleBackColor = true;
            this.btnCompressedFileExtensions.Click += new System.EventHandler(this.btnCompressedFileExtensions_Click);
            // 
            // btnExcludeFileExtensions
            // 
            this.btnExcludeFileExtensions.Location = new System.Drawing.Point(454, 26);
            this.btnExcludeFileExtensions.Name = "btnExcludeFileExtensions";
            this.btnExcludeFileExtensions.Size = new System.Drawing.Size(97, 23);
            this.btnExcludeFileExtensions.TabIndex = 16;
            this.btnExcludeFileExtensions.Text = "Edit...";
            this.btnExcludeFileExtensions.UseVisualStyleBackColor = true;
            this.btnExcludeFileExtensions.Click += new System.EventHandler(this.btnExcludeFileExtensions_Click);
            // 
            // labelCompressedFileExtensions
            // 
            this.labelCompressedFileExtensions.AutoSize = true;
            this.labelCompressedFileExtensions.Location = new System.Drawing.Point(194, 65);
            this.labelCompressedFileExtensions.Name = "labelCompressedFileExtensions";
            this.labelCompressedFileExtensions.Size = new System.Drawing.Size(36, 13);
            this.labelCompressedFileExtensions.TabIndex = 4;
            this.labelCompressedFileExtensions.Text = "None.";
            // 
            // labelExcludedFileExtensions
            // 
            this.labelExcludedFileExtensions.AutoSize = true;
            this.labelExcludedFileExtensions.Location = new System.Drawing.Point(194, 31);
            this.labelExcludedFileExtensions.Name = "labelExcludedFileExtensions";
            this.labelExcludedFileExtensions.Size = new System.Drawing.Size(36, 13);
            this.labelExcludedFileExtensions.TabIndex = 3;
            this.labelExcludedFileExtensions.Text = "None.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(398, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "(These files will be included in backup archives, but compression will be turned " +
    "off.)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(141, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Compressed File Extensions:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Excluded File Extensions:";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(417, 351);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(498, 351);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(18, 113);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(166, 13);
            this.label7.TabIndex = 29;
            this.label7.Text = "Excluded Folder Names/Patterns:";
            // 
            // labelExcludedFolderPatterns
            // 
            this.labelExcludedFolderPatterns.AutoSize = true;
            this.labelExcludedFolderPatterns.Location = new System.Drawing.Point(194, 113);
            this.labelExcludedFolderPatterns.Name = "labelExcludedFolderPatterns";
            this.labelExcludedFolderPatterns.Size = new System.Drawing.Size(36, 13);
            this.labelExcludedFolderPatterns.TabIndex = 30;
            this.labelExcludedFolderPatterns.Text = "None.";
            // 
            // btnExcludedFolderPatterns
            // 
            this.btnExcludedFolderPatterns.Location = new System.Drawing.Point(454, 108);
            this.btnExcludedFolderPatterns.Name = "btnExcludedFolderPatterns";
            this.btnExcludedFolderPatterns.Size = new System.Drawing.Size(97, 23);
            this.btnExcludedFolderPatterns.TabIndex = 31;
            this.btnExcludedFolderPatterns.Text = "Edit...";
            this.btnExcludedFolderPatterns.UseVisualStyleBackColor = true;
            this.btnExcludedFolderPatterns.Click += new System.EventHandler(this.btnExcludedFolderPatterns_Click);
            // 
            // GlobalSettingsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(585, 386);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GlobalSettingsForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Global Settings";
            this.Load += new System.EventHandler(this.GlobalSettingsForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelCompressedFileExtensions;
        private System.Windows.Forms.Label labelExcludedFileExtensions;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnExcludeFileExtensions;
        private System.Windows.Forms.Button btnCompressedFileExtensions;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbConstrainArchiveSizeInGB;
        private System.Windows.Forms.CheckBox cbConstrainArchiveSize;
        private System.Windows.Forms.Button btnExportConfiguration;
        private System.Windows.Forms.Button btnImportConfiguration;
        private System.Windows.Forms.Label lblLogfile;
        private System.Windows.Forms.Button btnLogBrowse;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbLogfile;
        private System.Windows.Forms.ComboBox cbLogLevel;
        private System.Windows.Forms.Button btnExcludedFolderPatterns;
        private System.Windows.Forms.Label labelExcludedFolderPatterns;
        private System.Windows.Forms.Label label7;
    }
}