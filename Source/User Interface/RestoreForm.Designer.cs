namespace ZippyBackup.User_Interface
{
    partial class RestoreForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RestoreForm));
            this.labelSearchMsg = new System.Windows.Forms.Label();
            this.cbSearchFolderNames = new System.Windows.Forms.CheckBox();
            this.cbSearchArchiveNames = new System.Windows.Forms.CheckBox();
            this.tbSearch = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnViewBackup = new System.Windows.Forms.Button();
            this.listRestoreDates = new ZippyBackup.User_Interface.SortedListBox();
            this.listRestoreBackups = new System.Windows.Forms.ListBox();
            this.GUIFastRefreshTimer = new System.Windows.Forms.Timer(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnBrowseFolder = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.tbProjectFolder = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelSearchMsg
            // 
            this.labelSearchMsg.AutoSize = true;
            this.labelSearchMsg.Location = new System.Drawing.Point(10, 75);
            this.labelSearchMsg.Name = "labelSearchMsg";
            this.labelSearchMsg.Size = new System.Drawing.Size(83, 13);
            this.labelSearchMsg.TabIndex = 19;
            this.labelSearchMsg.Text = "labelSearchMsg";
            // 
            // cbSearchFolderNames
            // 
            this.cbSearchFolderNames.AutoSize = true;
            this.cbSearchFolderNames.Checked = true;
            this.cbSearchFolderNames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSearchFolderNames.Location = new System.Drawing.Point(175, 54);
            this.cbSearchFolderNames.Name = "cbSearchFolderNames";
            this.cbSearchFolderNames.Size = new System.Drawing.Size(91, 17);
            this.cbSearchFolderNames.TabIndex = 18;
            this.cbSearchFolderNames.Text = "Folder Names";
            this.cbSearchFolderNames.UseVisualStyleBackColor = true;
            this.cbSearchFolderNames.CheckedChanged += new System.EventHandler(this.cbSearchFolderNames_CheckedChanged);
            // 
            // cbSearchArchiveNames
            // 
            this.cbSearchArchiveNames.AutoSize = true;
            this.cbSearchArchiveNames.Checked = true;
            this.cbSearchArchiveNames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSearchArchiveNames.Location = new System.Drawing.Point(57, 54);
            this.cbSearchArchiveNames.Name = "cbSearchArchiveNames";
            this.cbSearchArchiveNames.Size = new System.Drawing.Size(98, 17);
            this.cbSearchArchiveNames.TabIndex = 17;
            this.cbSearchArchiveNames.Text = "Archive Names";
            this.cbSearchArchiveNames.UseVisualStyleBackColor = true;
            this.cbSearchArchiveNames.CheckedChanged += new System.EventHandler(this.cbSearchArchiveNames_CheckedChanged);
            // 
            // tbSearch
            // 
            this.tbSearch.Location = new System.Drawing.Point(57, 28);
            this.tbSearch.Name = "tbSearch";
            this.tbSearch.Size = new System.Drawing.Size(216, 20);
            this.tbSearch.TabIndex = 16;
            this.tbSearch.TextChanged += new System.EventHandler(this.tbSearch_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Search:";
            // 
            // btnViewBackup
            // 
            this.btnViewBackup.Enabled = false;
            this.btnViewBackup.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnViewBackup.Location = new System.Drawing.Point(6, 281);
            this.btnViewBackup.Name = "btnViewBackup";
            this.btnViewBackup.Size = new System.Drawing.Size(268, 37);
            this.btnViewBackup.TabIndex = 14;
            this.btnViewBackup.Text = "&View...";
            this.btnViewBackup.UseVisualStyleBackColor = true;
            this.btnViewBackup.Click += new System.EventHandler(this.btnViewBackup_Click);
            // 
            // listRestoreDates
            // 
            this.listRestoreDates.FormattingEnabled = true;
            this.listRestoreDates.Location = new System.Drawing.Point(6, 93);
            this.listRestoreDates.Name = "listRestoreDates";
            this.listRestoreDates.Size = new System.Drawing.Size(267, 186);
            this.listRestoreDates.TabIndex = 13;
            this.listRestoreDates.SelectedIndexChanged += new System.EventHandler(this.listRestoreDates_SelectedIndexChanged);
            this.listRestoreDates.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listRestoreDates_MouseDoubleClick);
            // 
            // listRestoreBackups
            // 
            this.listRestoreBackups.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listRestoreBackups.FormattingEnabled = true;
            this.listRestoreBackups.ItemHeight = 20;
            this.listRestoreBackups.Location = new System.Drawing.Point(17, 40);
            this.listRestoreBackups.Name = "listRestoreBackups";
            this.listRestoreBackups.Size = new System.Drawing.Size(308, 184);
            this.listRestoreBackups.TabIndex = 12;
            this.listRestoreBackups.SelectedIndexChanged += new System.EventHandler(this.listRestoreBackups_SelectedIndexChanged);
            // 
            // GUIFastRefreshTimer
            // 
            this.GUIFastRefreshTimer.Tick += new System.EventHandler(this.GUIFastRefreshTimer_Tick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnBrowseFolder);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.tbProjectFolder);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.listRestoreBackups);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(343, 324);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Backup Projects";
            // 
            // btnBrowseFolder
            // 
            this.btnBrowseFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBrowseFolder.Location = new System.Drawing.Point(227, 294);
            this.btnBrowseFolder.Name = "btnBrowseFolder";
            this.btnBrowseFolder.Size = new System.Drawing.Size(110, 24);
            this.btnBrowseFolder.TabIndex = 21;
            this.btnBrowseFolder.Text = "&Browse...";
            this.btnBrowseFolder.UseVisualStyleBackColor = true;
            this.btnBrowseFolder.Click += new System.EventHandler(this.btnBrowseFolder_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 271);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 23;
            this.label4.Text = "Folder:";
            // 
            // tbProjectFolder
            // 
            this.tbProjectFolder.Enabled = false;
            this.tbProjectFolder.Location = new System.Drawing.Point(82, 268);
            this.tbProjectFolder.Name = "tbProjectFolder";
            this.tbProjectFolder.Size = new System.Drawing.Size(255, 20);
            this.tbProjectFolder.TabIndex = 21;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 248);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(114, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "Other Backup Projects";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "Active Backup Projects";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.listRestoreDates);
            this.groupBox2.Controls.Add(this.btnViewBackup);
            this.groupBox2.Controls.Add(this.labelSearchMsg);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.cbSearchFolderNames);
            this.groupBox2.Controls.Add(this.tbSearch);
            this.groupBox2.Controls.Add(this.cbSearchArchiveNames);
            this.groupBox2.Location = new System.Drawing.Point(361, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(284, 324);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Backups";
            // 
            // RestoreForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 348);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "RestoreForm";
            this.Text = "ZippyBackup Restore";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnLoad);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelSearchMsg;
        private System.Windows.Forms.CheckBox cbSearchFolderNames;
        private System.Windows.Forms.CheckBox cbSearchArchiveNames;
        private System.Windows.Forms.TextBox tbSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnViewBackup;
        private ZippyBackup.User_Interface.SortedListBox listRestoreDates;
        private System.Windows.Forms.ListBox listRestoreBackups;
        private System.Windows.Forms.Timer GUIFastRefreshTimer;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnBrowseFolder;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbProjectFolder;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}