namespace ZippyBackup.User_Interface
{
    partial class BackupProjectForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;        

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackupProjectForm));
            this.label1 = new System.Windows.Forms.Label();
            this.tbProjectName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbSrcFolder = new System.Windows.Forms.TextBox();
            this.btnBrowseSrc = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbBackupFolder = new System.Windows.Forms.TextBox();
            this.btnBrowseBackup = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.labelExcludedExtensions = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnFileExtensions = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.rbNoPassword = new System.Windows.Forms.RadioButton();
            this.rbAES = new System.Windows.Forms.RadioButton();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.labelExcludedSubfolders = new System.Windows.Forms.Label();
            this.btnSubfolders = new System.Windows.Forms.Button();
            this.linkEncryptionInfo = new System.Windows.Forms.LinkLabel();
            this.gbExclusions = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.labelExcludedFiles = new System.Windows.Forms.Label();
            this.btnFiles = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.tbExcludeSize = new System.Windows.Forms.TextBox();
            this.cbExcludeSize = new System.Windows.Forms.CheckBox();
            this.gbSecurity = new System.Windows.Forms.GroupBox();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cbUseVSS = new System.Windows.Forms.CheckBox();
            this.cbDoNotRemind = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.tabSecurity = new System.Windows.Forms.TabPage();
            this.gbFolderAccess = new System.Windows.Forms.GroupBox();
            this.btnManageBackupCredentials = new System.Windows.Forms.Button();
            this.btnManageSourceCredentials = new System.Windows.Forms.Button();
            this.tabExclusions = new System.Windows.Forms.TabPage();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.gbExclusions.SuspendLayout();
            this.gbSecurity.SuspendLayout();
            this.gbOptions.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabSecurity.SuspendLayout();
            this.gbFolderAccess.SuspendLayout();
            this.tabExclusions.SuspendLayout();
            this.tabAdvanced.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Project Name:";
            // 
            // tbProjectName
            // 
            this.tbProjectName.Location = new System.Drawing.Point(192, 17);
            this.tbProjectName.Name = "tbProjectName";
            this.tbProjectName.Size = new System.Drawing.Size(432, 20);
            this.tbProjectName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Folder to be backed up:";
            // 
            // tbSrcFolder
            // 
            this.tbSrcFolder.Location = new System.Drawing.Point(192, 51);
            this.tbSrcFolder.Name = "tbSrcFolder";
            this.tbSrcFolder.Size = new System.Drawing.Size(353, 20);
            this.tbSrcFolder.TabIndex = 3;
            // 
            // btnBrowseSrc
            // 
            this.btnBrowseSrc.Location = new System.Drawing.Point(549, 49);
            this.btnBrowseSrc.Name = "btnBrowseSrc";
            this.btnBrowseSrc.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseSrc.TabIndex = 4;
            this.btnBrowseSrc.Text = "Browse...";
            this.btnBrowseSrc.UseVisualStyleBackColor = true;
            this.btnBrowseSrc.Click += new System.EventHandler(this.btnBrowseSrc_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 127);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(128, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Location to store backup:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(189, 74);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(322, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "* All subfolders will also be backed up, unless named in Exclusions.";
            // 
            // tbBackupFolder
            // 
            this.tbBackupFolder.Location = new System.Drawing.Point(192, 124);
            this.tbBackupFolder.Name = "tbBackupFolder";
            this.tbBackupFolder.Size = new System.Drawing.Size(353, 20);
            this.tbBackupFolder.TabIndex = 7;
            // 
            // btnBrowseBackup
            // 
            this.btnBrowseBackup.Location = new System.Drawing.Point(549, 122);
            this.btnBrowseBackup.Name = "btnBrowseBackup";
            this.btnBrowseBackup.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseBackup.TabIndex = 8;
            this.btnBrowseBackup.Text = "Browse...";
            this.btnBrowseBackup.UseVisualStyleBackColor = true;
            this.btnBrowseBackup.Click += new System.EventHandler(this.btnBrowseBackup_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(498, 350);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 9;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(582, 350);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // labelExcludedExtensions
            // 
            this.labelExcludedExtensions.AutoSize = true;
            this.labelExcludedExtensions.Location = new System.Drawing.Point(177, 26);
            this.labelExcludedExtensions.Name = "labelExcludedExtensions";
            this.labelExcludedExtensions.Size = new System.Drawing.Size(124, 13);
            this.labelExcludedExtensions.TabIndex = 13;
            this.labelExcludedExtensions.Text = "Excluded File Extensions";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 26);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(127, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Excluded File Extensions:";
            // 
            // btnFileExtensions
            // 
            this.btnFileExtensions.Location = new System.Drawing.Point(516, 21);
            this.btnFileExtensions.Name = "btnFileExtensions";
            this.btnFileExtensions.Size = new System.Drawing.Size(75, 23);
            this.btnFileExtensions.TabIndex = 15;
            this.btnFileExtensions.Text = "Edit...";
            this.btnFileExtensions.UseVisualStyleBackColor = true;
            this.btnFileExtensions.Click += new System.EventHandler(this.btnFileExtensions_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Encryption Security:";
            // 
            // rbNoPassword
            // 
            this.rbNoPassword.AutoSize = true;
            this.rbNoPassword.Location = new System.Drawing.Point(187, 23);
            this.rbNoPassword.Name = "rbNoPassword";
            this.rbNoPassword.Size = new System.Drawing.Size(95, 17);
            this.rbNoPassword.TabIndex = 17;
            this.rbNoPassword.TabStop = true;
            this.rbNoPassword.Text = "No Encryption.";
            this.rbNoPassword.UseVisualStyleBackColor = true;
            this.rbNoPassword.CheckedChanged += new System.EventHandler(this.rbNoPassword_CheckedChanged);
            // 
            // rbAES
            // 
            this.rbAES.AutoSize = true;
            this.rbAES.Location = new System.Drawing.Point(187, 46);
            this.rbAES.Name = "rbAES";
            this.rbAES.Size = new System.Drawing.Size(154, 17);
            this.rbAES.TabIndex = 18;
            this.rbAES.TabStop = true;
            this.rbAES.Text = "AES 256-bit with password:";
            this.rbAES.UseVisualStyleBackColor = true;
            this.rbAES.CheckedChanged += new System.EventHandler(this.rbAES_CheckedChanged);
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(359, 45);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(232, 20);
            this.tbPassword.TabIndex = 19;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 84);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 13);
            this.label8.TabIndex = 21;
            this.label8.Text = "Subfolders:";
            // 
            // labelExcludedSubfolders
            // 
            this.labelExcludedSubfolders.AutoSize = true;
            this.labelExcludedSubfolders.Location = new System.Drawing.Point(177, 84);
            this.labelExcludedSubfolders.Name = "labelExcludedSubfolders";
            this.labelExcludedSubfolders.Size = new System.Drawing.Size(104, 13);
            this.labelExcludedSubfolders.TabIndex = 22;
            this.labelExcludedSubfolders.Text = "Excluded Subfolders";
            // 
            // btnSubfolders
            // 
            this.btnSubfolders.Location = new System.Drawing.Point(516, 80);
            this.btnSubfolders.Name = "btnSubfolders";
            this.btnSubfolders.Size = new System.Drawing.Size(75, 23);
            this.btnSubfolders.TabIndex = 23;
            this.btnSubfolders.Text = "Edit...";
            this.btnSubfolders.UseVisualStyleBackColor = true;
            this.btnSubfolders.Click += new System.EventHandler(this.btnSubfolders_Click);
            // 
            // linkEncryptionInfo
            // 
            this.linkEncryptionInfo.AutoSize = true;
            this.linkEncryptionInfo.Location = new System.Drawing.Point(206, 66);
            this.linkEncryptionInfo.Name = "linkEncryptionInfo";
            this.linkEncryptionInfo.Size = new System.Drawing.Size(93, 13);
            this.linkEncryptionInfo.TabIndex = 24;
            this.linkEncryptionInfo.TabStop = true;
            this.linkEncryptionInfo.Text = "* More Information";
            this.linkEncryptionInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkEncryptionInfo_LinkClicked);
            // 
            // gbExclusions
            // 
            this.gbExclusions.Controls.Add(this.label10);
            this.gbExclusions.Controls.Add(this.labelExcludedFiles);
            this.gbExclusions.Controls.Add(this.btnFiles);
            this.gbExclusions.Controls.Add(this.label7);
            this.gbExclusions.Controls.Add(this.tbExcludeSize);
            this.gbExclusions.Controls.Add(this.cbExcludeSize);
            this.gbExclusions.Controls.Add(this.label5);
            this.gbExclusions.Controls.Add(this.labelExcludedExtensions);
            this.gbExclusions.Controls.Add(this.btnSubfolders);
            this.gbExclusions.Controls.Add(this.btnFileExtensions);
            this.gbExclusions.Controls.Add(this.labelExcludedSubfolders);
            this.gbExclusions.Controls.Add(this.label8);
            this.gbExclusions.Location = new System.Drawing.Point(14, 23);
            this.gbExclusions.Name = "gbExclusions";
            this.gbExclusions.Size = new System.Drawing.Size(607, 139);
            this.gbExclusions.TabIndex = 25;
            this.gbExclusions.TabStop = false;
            this.gbExclusions.Text = "Exclusions";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 55);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(78, 13);
            this.label10.TabIndex = 28;
            this.label10.Text = "Excluded Files:";
            // 
            // labelExcludedFiles
            // 
            this.labelExcludedFiles.AutoSize = true;
            this.labelExcludedFiles.Location = new System.Drawing.Point(177, 55);
            this.labelExcludedFiles.Name = "labelExcludedFiles";
            this.labelExcludedFiles.Size = new System.Drawing.Size(75, 13);
            this.labelExcludedFiles.TabIndex = 27;
            this.labelExcludedFiles.Text = "Excluded Files";
            // 
            // btnFiles
            // 
            this.btnFiles.Location = new System.Drawing.Point(516, 50);
            this.btnFiles.Name = "btnFiles";
            this.btnFiles.Size = new System.Drawing.Size(75, 23);
            this.btnFiles.TabIndex = 29;
            this.btnFiles.Text = "Edit...";
            this.btnFiles.UseVisualStyleBackColor = true;
            this.btnFiles.Click += new System.EventHandler(this.btnFiles_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(568, 115);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(23, 13);
            this.label7.TabIndex = 26;
            this.label7.Text = "MB";
            // 
            // tbExcludeSize
            // 
            this.tbExcludeSize.Location = new System.Drawing.Point(462, 112);
            this.tbExcludeSize.Name = "tbExcludeSize";
            this.tbExcludeSize.Size = new System.Drawing.Size(100, 20);
            this.tbExcludeSize.TabIndex = 25;
            // 
            // cbExcludeSize
            // 
            this.cbExcludeSize.AutoSize = true;
            this.cbExcludeSize.Location = new System.Drawing.Point(11, 107);
            this.cbExcludeSize.Name = "cbExcludeSize";
            this.cbExcludeSize.Size = new System.Drawing.Size(141, 17);
            this.cbExcludeSize.TabIndex = 24;
            this.cbExcludeSize.Text = "Exclude files larger than:";
            this.cbExcludeSize.UseVisualStyleBackColor = true;
            this.cbExcludeSize.CheckedChanged += new System.EventHandler(this.cbExcludeSize_CheckedChanged);
            // 
            // gbSecurity
            // 
            this.gbSecurity.Controls.Add(this.tbPassword);
            this.gbSecurity.Controls.Add(this.label6);
            this.gbSecurity.Controls.Add(this.linkEncryptionInfo);
            this.gbSecurity.Controls.Add(this.rbNoPassword);
            this.gbSecurity.Controls.Add(this.rbAES);
            this.gbSecurity.Location = new System.Drawing.Point(15, 21);
            this.gbSecurity.Name = "gbSecurity";
            this.gbSecurity.Size = new System.Drawing.Size(607, 93);
            this.gbSecurity.TabIndex = 26;
            this.gbSecurity.TabStop = false;
            this.gbSecurity.Text = "Archive Security";
            // 
            // gbOptions
            // 
            this.gbOptions.Controls.Add(this.label9);
            this.gbOptions.Controls.Add(this.cbUseVSS);
            this.gbOptions.Location = new System.Drawing.Point(14, 20);
            this.gbOptions.Margin = new System.Windows.Forms.Padding(2);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Padding = new System.Windows.Forms.Padding(2);
            this.gbOptions.Size = new System.Drawing.Size(607, 146);
            this.gbOptions.TabIndex = 27;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = "Volume Shadow Service";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 41);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(559, 65);
            this.label9.TabIndex = 26;
            this.label9.Text = resources.GetString("label9.Text");
            // 
            // cbUseVSS
            // 
            this.cbUseVSS.AutoSize = true;
            this.cbUseVSS.Location = new System.Drawing.Point(12, 21);
            this.cbUseVSS.Name = "cbUseVSS";
            this.cbUseVSS.Size = new System.Drawing.Size(468, 17);
            this.cbUseVSS.TabIndex = 25;
            this.cbUseVSS.Text = "[BETA] Use Windows Volume Shadow Service (VSS) to capture a snapshot for each bac" +
    "kup.";
            this.cbUseVSS.UseVisualStyleBackColor = true;
            this.cbUseVSS.CheckedChanged += new System.EventHandler(this.cbUseVSS_CheckedChanged);
            // 
            // cbDoNotRemind
            // 
            this.cbDoNotRemind.AutoSize = true;
            this.cbDoNotRemind.Location = new System.Drawing.Point(21, 273);
            this.cbDoNotRemind.Name = "cbDoNotRemind";
            this.cbDoNotRemind.Size = new System.Drawing.Size(310, 17);
            this.cbDoNotRemind.TabIndex = 28;
            this.cbDoNotRemind.Text = "Do not remind me if this project isn\'t backed up on schedule.";
            this.cbDoNotRemind.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabSecurity);
            this.tabControl1.Controls.Add(this.tabExclusions);
            this.tabControl1.Controls.Add(this.tabAdvanced);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(645, 332);
            this.tabControl1.TabIndex = 28;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.cbDoNotRemind);
            this.tabGeneral.Controls.Add(this.label1);
            this.tabGeneral.Controls.Add(this.label3);
            this.tabGeneral.Controls.Add(this.tbBackupFolder);
            this.tabGeneral.Controls.Add(this.label4);
            this.tabGeneral.Controls.Add(this.btnBrowseSrc);
            this.tabGeneral.Controls.Add(this.tbProjectName);
            this.tabGeneral.Controls.Add(this.btnBrowseBackup);
            this.tabGeneral.Controls.Add(this.label2);
            this.tabGeneral.Controls.Add(this.tbSrcFolder);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(637, 306);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // tabSecurity
            // 
            this.tabSecurity.Controls.Add(this.gbFolderAccess);
            this.tabSecurity.Controls.Add(this.gbSecurity);
            this.tabSecurity.Location = new System.Drawing.Point(4, 22);
            this.tabSecurity.Name = "tabSecurity";
            this.tabSecurity.Padding = new System.Windows.Forms.Padding(3);
            this.tabSecurity.Size = new System.Drawing.Size(637, 306);
            this.tabSecurity.TabIndex = 1;
            this.tabSecurity.Text = "Security";
            this.tabSecurity.UseVisualStyleBackColor = true;
            // 
            // gbFolderAccess
            // 
            this.gbFolderAccess.Controls.Add(this.btnManageBackupCredentials);
            this.gbFolderAccess.Controls.Add(this.btnManageSourceCredentials);
            this.gbFolderAccess.Location = new System.Drawing.Point(15, 129);
            this.gbFolderAccess.Name = "gbFolderAccess";
            this.gbFolderAccess.Size = new System.Drawing.Size(607, 78);
            this.gbFolderAccess.TabIndex = 27;
            this.gbFolderAccess.TabStop = false;
            this.gbFolderAccess.Text = "Folder Access";
            // 
            // btnManageBackupCredentials
            // 
            this.btnManageBackupCredentials.Location = new System.Drawing.Point(325, 19);
            this.btnManageBackupCredentials.Name = "btnManageBackupCredentials";
            this.btnManageBackupCredentials.Size = new System.Drawing.Size(166, 45);
            this.btnManageBackupCredentials.TabIndex = 1;
            this.btnManageBackupCredentials.Text = "Manage Backup Folder Credentials...";
            this.btnManageBackupCredentials.UseVisualStyleBackColor = true;
            this.btnManageBackupCredentials.Click += new System.EventHandler(this.btnManageBackupCredentials_Click);
            // 
            // btnManageSourceCredentials
            // 
            this.btnManageSourceCredentials.Location = new System.Drawing.Point(91, 19);
            this.btnManageSourceCredentials.Name = "btnManageSourceCredentials";
            this.btnManageSourceCredentials.Size = new System.Drawing.Size(166, 45);
            this.btnManageSourceCredentials.TabIndex = 0;
            this.btnManageSourceCredentials.Text = "Manage Source Folder Credentials...";
            this.btnManageSourceCredentials.UseVisualStyleBackColor = true;
            this.btnManageSourceCredentials.Click += new System.EventHandler(this.btnManageSourceCredentials_Click);
            // 
            // tabExclusions
            // 
            this.tabExclusions.Controls.Add(this.gbExclusions);
            this.tabExclusions.Location = new System.Drawing.Point(4, 22);
            this.tabExclusions.Name = "tabExclusions";
            this.tabExclusions.Size = new System.Drawing.Size(637, 306);
            this.tabExclusions.TabIndex = 2;
            this.tabExclusions.Text = "Exclusions";
            this.tabExclusions.UseVisualStyleBackColor = true;
            // 
            // tabAdvanced
            // 
            this.tabAdvanced.Controls.Add(this.gbOptions);
            this.tabAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabAdvanced.Name = "tabAdvanced";
            this.tabAdvanced.Size = new System.Drawing.Size(637, 306);
            this.tabAdvanced.TabIndex = 3;
            this.tabAdvanced.Text = "Advanced";
            this.tabAdvanced.UseVisualStyleBackColor = true;
            // 
            // BackupProjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(677, 395);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BackupProjectForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Backup Project";
            this.Load += new System.EventHandler(this.BackupProjectForm_Load);
            this.gbExclusions.ResumeLayout(false);
            this.gbExclusions.PerformLayout();
            this.gbSecurity.ResumeLayout(false);
            this.gbSecurity.PerformLayout();
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.tabSecurity.ResumeLayout(false);
            this.gbFolderAccess.ResumeLayout(false);
            this.tabExclusions.ResumeLayout(false);
            this.tabAdvanced.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbProjectName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbSrcFolder;
        private System.Windows.Forms.Button btnBrowseSrc;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbBackupFolder;
        private System.Windows.Forms.Button btnBrowseBackup;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label labelExcludedExtensions;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnFileExtensions;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RadioButton rbNoPassword;
        private System.Windows.Forms.RadioButton rbAES;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label labelExcludedSubfolders;
        private System.Windows.Forms.Button btnSubfolders;
        private System.Windows.Forms.LinkLabel linkEncryptionInfo;
        private System.Windows.Forms.GroupBox gbExclusions;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbExcludeSize;
        private System.Windows.Forms.CheckBox cbExcludeSize;
        private System.Windows.Forms.GroupBox gbSecurity;
        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox cbUseVSS;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label labelExcludedFiles;
        private System.Windows.Forms.Button btnFiles;
        private System.Windows.Forms.CheckBox cbDoNotRemind;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabSecurity;
        private System.Windows.Forms.GroupBox gbFolderAccess;
        private System.Windows.Forms.Button btnManageBackupCredentials;
        private System.Windows.Forms.Button btnManageSourceCredentials;
        private System.Windows.Forms.TabPage tabExclusions;
        private System.Windows.Forms.TabPage tabAdvanced;
    }
}