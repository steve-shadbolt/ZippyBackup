namespace ZippyBackup.User_Interface
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.GUIRefreshTimer = new System.Windows.Forms.Timer(this.components);
            this.ScheduleTimer = new System.Windows.Forms.Timer(this.components);
            this.tabAbout = new System.Windows.Forms.TabPage();
            this.linkAlphaFSLicense = new System.Windows.Forms.LinkLabel();
            this.linkAlphaVSSLicense = new System.Windows.Forms.LinkLabel();
            this.btnSourceForge = new System.Windows.Forms.Button();
            this.btnCheckForUpdates = new System.Windows.Forms.Button();
            this.linkZippyBackupLicense = new System.Windows.Forms.LinkLabel();
            this.linkBZip2License = new System.Windows.Forms.LinkLabel();
            this.linkZLibLicense = new System.Windows.Forms.LinkLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.linkDotNetZipDonate = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.linkDotNetZipLicense = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.tabManage = new System.Windows.Forms.TabPage();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.listManageBackups = new System.Windows.Forms.ListBox();
            this.tabSchedule = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tbVerifyDays = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnEMailSettings = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.tbEMailFreq = new System.Windows.Forms.TextBox();
            this.cbEMailReminders = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tbRoutineMinutes = new System.Windows.Forms.TextBox();
            this.rbScheduleRoutine = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.tbAlertDays = new System.Windows.Forms.TextBox();
            this.cbWarnOnAgedBackup = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbIdleMinutes = new System.Windows.Forms.TextBox();
            this.rbScheduleOnUserIdle = new System.Windows.Forms.RadioButton();
            this.rbManualOnly = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.cbLaunchWithWindows = new System.Windows.Forms.CheckBox();
            this.tabBackups = new System.Windows.Forms.TabPage();
            this.btnVerify = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnRunComplete = new System.Windows.Forms.Button();
            this.btnRunAll = new System.Windows.Forms.Button();
            this.labelBackups = new System.Windows.Forms.Label();
            this.btnRun = new System.Windows.Forms.Button();
            this.listBackupsSx = new System.Windows.Forms.ListBox();
            this.TabControl = new System.Windows.Forms.TabControl();
            this.tabAbout.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabManage.SuspendLayout();
            this.tabSchedule.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabBackups.SuspendLayout();
            this.TabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // GUIRefreshTimer
            // 
            this.GUIRefreshTimer.Interval = 55000;
            this.GUIRefreshTimer.Tick += new System.EventHandler(this.GUIRefreshTimer_Tick);
            // 
            // ScheduleTimer
            // 
            this.ScheduleTimer.Interval = 10000;
            this.ScheduleTimer.Tick += new System.EventHandler(this.ScheduleTimer_Tick);
            // 
            // tabAbout
            // 
            this.tabAbout.BackColor = System.Drawing.Color.Transparent;
            this.tabAbout.Controls.Add(this.linkAlphaFSLicense);
            this.tabAbout.Controls.Add(this.linkAlphaVSSLicense);
            this.tabAbout.Controls.Add(this.btnSourceForge);
            this.tabAbout.Controls.Add(this.btnCheckForUpdates);
            this.tabAbout.Controls.Add(this.linkZippyBackupLicense);
            this.tabAbout.Controls.Add(this.linkBZip2License);
            this.tabAbout.Controls.Add(this.linkZLibLicense);
            this.tabAbout.Controls.Add(this.groupBox1);
            this.tabAbout.Controls.Add(this.label2);
            this.tabAbout.Location = new System.Drawing.Point(4, 22);
            this.tabAbout.Name = "tabAbout";
            this.tabAbout.Size = new System.Drawing.Size(563, 347);
            this.tabAbout.TabIndex = 4;
            this.tabAbout.Text = "About";
            this.tabAbout.UseVisualStyleBackColor = true;
            // 
            // linkAlphaFSLicense
            // 
            this.linkAlphaFSLicense.AutoSize = true;
            this.linkAlphaFSLicense.Location = new System.Drawing.Point(209, 196);
            this.linkAlphaFSLicense.Name = "linkAlphaFSLicense";
            this.linkAlphaFSLicense.Size = new System.Drawing.Size(87, 13);
            this.linkAlphaFSLicense.TabIndex = 10;
            this.linkAlphaFSLicense.TabStop = true;
            this.linkAlphaFSLicense.Text = "AlphaFS License";
            this.linkAlphaFSLicense.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAlphaFSLicense_LinkClicked);
            // 
            // linkAlphaVSSLicense
            // 
            this.linkAlphaVSSLicense.AutoSize = true;
            this.linkAlphaVSSLicense.Location = new System.Drawing.Point(209, 172);
            this.linkAlphaVSSLicense.Name = "linkAlphaVSSLicense";
            this.linkAlphaVSSLicense.Size = new System.Drawing.Size(95, 13);
            this.linkAlphaVSSLicense.TabIndex = 9;
            this.linkAlphaVSSLicense.TabStop = true;
            this.linkAlphaVSSLicense.Text = "AlphaVSS License";
            this.linkAlphaVSSLicense.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAlphaVSSLicense_LinkClicked);
            // 
            // btnSourceForge
            // 
            this.btnSourceForge.Location = new System.Drawing.Point(416, 185);
            this.btnSourceForge.Name = "btnSourceForge";
            this.btnSourceForge.Size = new System.Drawing.Size(134, 34);
            this.btnSourceForge.TabIndex = 8;
            this.btnSourceForge.Text = "Visit ZippyBackup @ SourceForge...";
            this.btnSourceForge.UseVisualStyleBackColor = true;
            this.btnSourceForge.Click += new System.EventHandler(this.btnSourceForge_Click);
            // 
            // btnCheckForUpdates
            // 
            this.btnCheckForUpdates.Location = new System.Drawing.Point(416, 16);
            this.btnCheckForUpdates.Name = "btnCheckForUpdates";
            this.btnCheckForUpdates.Size = new System.Drawing.Size(134, 34);
            this.btnCheckForUpdates.TabIndex = 7;
            this.btnCheckForUpdates.Text = "Check for &updates...";
            this.btnCheckForUpdates.UseVisualStyleBackColor = true;
            this.btnCheckForUpdates.Click += new System.EventHandler(this.btnCheckForUpdates_Click);
            // 
            // linkZippyBackupLicense
            // 
            this.linkZippyBackupLicense.AutoSize = true;
            this.linkZippyBackupLicense.Location = new System.Drawing.Point(434, 53);
            this.linkZippyBackupLicense.Name = "linkZippyBackupLicense";
            this.linkZippyBackupLicense.Size = new System.Drawing.Size(110, 13);
            this.linkZippyBackupLicense.TabIndex = 6;
            this.linkZippyBackupLicense.TabStop = true;
            this.linkZippyBackupLicense.Text = "ZippyBackup License";
            this.linkZippyBackupLicense.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkZippyBackupLicense_LinkClicked);
            // 
            // linkBZip2License
            // 
            this.linkBZip2License.AutoSize = true;
            this.linkBZip2License.Location = new System.Drawing.Point(18, 196);
            this.linkBZip2License.Name = "linkBZip2License";
            this.linkBZip2License.Size = new System.Drawing.Size(75, 13);
            this.linkBZip2License.TabIndex = 5;
            this.linkBZip2License.TabStop = true;
            this.linkBZip2License.Text = "BZip2 License";
            this.linkBZip2License.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkBZip2License_LinkClicked);
            // 
            // linkZLibLicense
            // 
            this.linkZLibLicense.AutoSize = true;
            this.linkZLibLicense.Location = new System.Drawing.Point(18, 172);
            this.linkZLibLicense.Name = "linkZLibLicense";
            this.linkZLibLicense.Size = new System.Drawing.Size(68, 13);
            this.linkZLibLicense.TabIndex = 4;
            this.linkZLibLicense.TabStop = true;
            this.linkZLibLicense.Text = "ZLib License";
            this.linkZLibLicense.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkZLibLicense_LinkClicked);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.linkDotNetZipDonate);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.linkDotNetZipLicense);
            this.groupBox1.Location = new System.Drawing.Point(21, 84);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(356, 85);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "DotNetZip";
            // 
            // linkDotNetZipDonate
            // 
            this.linkDotNetZipDonate.AutoSize = true;
            this.linkDotNetZipDonate.Location = new System.Drawing.Point(18, 54);
            this.linkDotNetZipDonate.Name = "linkDotNetZipDonate";
            this.linkDotNetZipDonate.Size = new System.Drawing.Size(55, 13);
            this.linkDotNetZipDonate.TabIndex = 4;
            this.linkDotNetZipDonate.TabStop = true;
            this.linkDotNetZipDonate.Text = "Donations";
            this.linkDotNetZipDonate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkDotNetZipDonate_LinkClicked);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(120, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(226, 39);
            this.label3.TabIndex = 3;
            this.label3.Text = "DotNetZip supports the Boys and Girls Club of \r\nWestern Pennsylvania.  If you app" +
    "reciate this \r\nsoftware, please consider donating.";
            // 
            // linkDotNetZipLicense
            // 
            this.linkDotNetZipLicense.AutoSize = true;
            this.linkDotNetZipLicense.Location = new System.Drawing.Point(18, 28);
            this.linkDotNetZipLicense.Name = "linkDotNetZipLicense";
            this.linkDotNetZipLicense.Size = new System.Drawing.Size(96, 13);
            this.linkDotNetZipLicense.TabIndex = 1;
            this.linkDotNetZipLicense.TabStop = true;
            this.linkDotNetZipLicense.Text = "DotNetZip License";
            this.linkDotNetZipLicense.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkDotNetZipLicense_LinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(232, 65);
            this.label2.TabIndex = 0;
            this.label2.Text = "ZippyBackup \r\nCopyright (C) 2012-2016 by Wiley Black\r\nModified 2023 by Steve Shadbolt\r\n\r\nZippyBackup depends on " +
    "the following libraries.";
            // 
            // tabManage
            // 
            this.tabManage.Controls.Add(this.btnSettings);
            this.tabManage.Controls.Add(this.btnEdit);
            this.tabManage.Controls.Add(this.btnRemove);
            this.tabManage.Controls.Add(this.btnNew);
            this.tabManage.Controls.Add(this.listManageBackups);
            this.tabManage.Location = new System.Drawing.Point(4, 22);
            this.tabManage.Name = "tabManage";
            this.tabManage.Padding = new System.Windows.Forms.Padding(3);
            this.tabManage.Size = new System.Drawing.Size(563, 347);
            this.tabManage.TabIndex = 3;
            this.tabManage.Text = "Manage";
            this.tabManage.UseVisualStyleBackColor = true;
            // 
            // btnSettings
            // 
            this.btnSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettings.Location = new System.Drawing.Point(6, 303);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(550, 38);
            this.btnSettings.TabIndex = 5;
            this.btnSettings.Text = "Global Settings...";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Enabled = false;
            this.btnEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEdit.Location = new System.Drawing.Point(221, 213);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(120, 37);
            this.btnEdit.TabIndex = 4;
            this.btnEdit.Text = "Edit...";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Enabled = false;
            this.btnRemove.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemove.Location = new System.Drawing.Point(95, 213);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(120, 37);
            this.btnRemove.TabIndex = 3;
            this.btnRemove.Text = "Remove...";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnNew
            // 
            this.btnNew.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNew.Location = new System.Drawing.Point(347, 213);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(120, 37);
            this.btnNew.TabIndex = 2;
            this.btnNew.Text = "New...";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // listManageBackups
            // 
            this.listManageBackups.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listManageBackups.FormattingEnabled = true;
            this.listManageBackups.ItemHeight = 20;
            this.listManageBackups.Location = new System.Drawing.Point(6, 6);
            this.listManageBackups.Name = "listManageBackups";
            this.listManageBackups.Size = new System.Drawing.Size(550, 204);
            this.listManageBackups.TabIndex = 1;
            this.listManageBackups.SelectedIndexChanged += new System.EventHandler(this.listManageBackups_SelectedIndexChanged);
            // 
            // tabSchedule
            // 
            this.tabSchedule.Controls.Add(this.groupBox2);
            this.tabSchedule.Controls.Add(this.label4);
            this.tabSchedule.Controls.Add(this.cbLaunchWithWindows);
            this.tabSchedule.Location = new System.Drawing.Point(4, 22);
            this.tabSchedule.Name = "tabSchedule";
            this.tabSchedule.Padding = new System.Windows.Forms.Padding(3);
            this.tabSchedule.Size = new System.Drawing.Size(563, 347);
            this.tabSchedule.TabIndex = 5;
            this.tabSchedule.Text = "Schedule";
            this.tabSchedule.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.tbVerifyDays);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.btnEMailSettings);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.tbEMailFreq);
            this.groupBox2.Controls.Add(this.cbEMailReminders);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.tbRoutineMinutes);
            this.groupBox2.Controls.Add(this.rbScheduleRoutine);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.tbAlertDays);
            this.groupBox2.Controls.Add(this.cbWarnOnAgedBackup);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.tbIdleMinutes);
            this.groupBox2.Controls.Add(this.rbScheduleOnUserIdle);
            this.groupBox2.Controls.Add(this.rbManualOnly);
            this.groupBox2.Location = new System.Drawing.Point(7, 74);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(550, 230);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Automatic Backup Schedule";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(411, 202);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(29, 13);
            this.label10.TabIndex = 18;
            this.label10.Text = "days";
            // 
            // tbVerifyDays
            // 
            this.tbVerifyDays.Location = new System.Drawing.Point(352, 199);
            this.tbVerifyDays.Name = "tbVerifyDays";
            this.tbVerifyDays.Size = new System.Drawing.Size(53, 20);
            this.tbVerifyDays.TabIndex = 17;
            this.tbVerifyDays.Text = "30";
            this.tbVerifyDays.TextChanged += new System.EventHandler(this.tbVerifyDays_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(5, 202);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(207, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Perform verification of each backup every:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 156);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(300, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "if a project goes without successful backup or an error occurs.";
            // 
            // btnEMailSettings
            // 
            this.btnEMailSettings.Location = new System.Drawing.Point(446, 134);
            this.btnEMailSettings.Margin = new System.Windows.Forms.Padding(2);
            this.btnEMailSettings.Name = "btnEMailSettings";
            this.btnEMailSettings.Size = new System.Drawing.Size(99, 19);
            this.btnEMailSettings.TabIndex = 14;
            this.btnEMailSettings.Text = "&E-Mail Settings";
            this.btnEMailSettings.UseVisualStyleBackColor = true;
            this.btnEMailSettings.Click += new System.EventHandler(this.btnEMailSettings_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(411, 136);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "days";
            // 
            // tbEMailFreq
            // 
            this.tbEMailFreq.Location = new System.Drawing.Point(352, 134);
            this.tbEMailFreq.Name = "tbEMailFreq";
            this.tbEMailFreq.Size = new System.Drawing.Size(53, 20);
            this.tbEMailFreq.TabIndex = 12;
            this.tbEMailFreq.Text = "5";
            this.tbEMailFreq.TextChanged += new System.EventHandler(this.tbEMailFreq_TextChanged);
            // 
            // cbEMailReminders
            // 
            this.cbEMailReminders.AutoSize = true;
            this.cbEMailReminders.Location = new System.Drawing.Point(7, 136);
            this.cbEMailReminders.Name = "cbEMailReminders";
            this.cbEMailReminders.Size = new System.Drawing.Size(103, 17);
            this.cbEMailReminders.TabIndex = 11;
            this.cbEMailReminders.Text = "E-mail me every:";
            this.cbEMailReminders.UseVisualStyleBackColor = true;
            this.cbEMailReminders.CheckedChanged += new System.EventHandler(this.cbEMailReminders_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(411, 68);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(43, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "minutes";
            // 
            // tbRoutineMinutes
            // 
            this.tbRoutineMinutes.Location = new System.Drawing.Point(352, 66);
            this.tbRoutineMinutes.Name = "tbRoutineMinutes";
            this.tbRoutineMinutes.Size = new System.Drawing.Size(53, 20);
            this.tbRoutineMinutes.TabIndex = 9;
            this.tbRoutineMinutes.Text = "60";
            this.tbRoutineMinutes.TextChanged += new System.EventHandler(this.tbRoutineMinutes_TextChanged);
            // 
            // rbScheduleRoutine
            // 
            this.rbScheduleRoutine.AutoSize = true;
            this.rbScheduleRoutine.Location = new System.Drawing.Point(7, 67);
            this.rbScheduleRoutine.Name = "rbScheduleRoutine";
            this.rbScheduleRoutine.Size = new System.Drawing.Size(211, 17);
            this.rbScheduleRoutine.TabIndex = 8;
            this.rbScheduleRoutine.TabStop = true;
            this.rbScheduleRoutine.Text = "Automatically backup all projects every:";
            this.rbScheduleRoutine.UseVisualStyleBackColor = true;
            this.rbScheduleRoutine.CheckedChanged += new System.EventHandler(this.rbScheduleRoutine_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(411, 114);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "days";
            // 
            // tbAlertDays
            // 
            this.tbAlertDays.Location = new System.Drawing.Point(352, 111);
            this.tbAlertDays.Name = "tbAlertDays";
            this.tbAlertDays.Size = new System.Drawing.Size(53, 20);
            this.tbAlertDays.TabIndex = 6;
            this.tbAlertDays.Text = "5";
            this.tbAlertDays.TextChanged += new System.EventHandler(this.tbAlertDays_TextChanged);
            // 
            // cbWarnOnAgedBackup
            // 
            this.cbWarnOnAgedBackup.AutoSize = true;
            this.cbWarnOnAgedBackup.Location = new System.Drawing.Point(7, 112);
            this.cbWarnOnAgedBackup.Name = "cbWarnOnAgedBackup";
            this.cbWarnOnAgedBackup.Size = new System.Drawing.Size(339, 17);
            this.cbWarnOnAgedBackup.TabIndex = 5;
            this.cbWarnOnAgedBackup.Text = "Alert me if a project goes without successful backup for more than:";
            this.cbWarnOnAgedBackup.UseVisualStyleBackColor = true;
            this.cbWarnOnAgedBackup.CheckedChanged += new System.EventHandler(this.cbWarnOnAgedBackup_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(411, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "minutes";
            // 
            // tbIdleMinutes
            // 
            this.tbIdleMinutes.Location = new System.Drawing.Point(352, 42);
            this.tbIdleMinutes.Name = "tbIdleMinutes";
            this.tbIdleMinutes.Size = new System.Drawing.Size(53, 20);
            this.tbIdleMinutes.TabIndex = 2;
            this.tbIdleMinutes.Text = "60";
            this.tbIdleMinutes.TextChanged += new System.EventHandler(this.tbIdleMinutes_TextChanged);
            // 
            // rbScheduleOnUserIdle
            // 
            this.rbScheduleOnUserIdle.AutoSize = true;
            this.rbScheduleOnUserIdle.Location = new System.Drawing.Point(7, 43);
            this.rbScheduleOnUserIdle.Name = "rbScheduleOnUserIdle";
            this.rbScheduleOnUserIdle.Size = new System.Drawing.Size(339, 17);
            this.rbScheduleOnUserIdle.TabIndex = 1;
            this.rbScheduleOnUserIdle.TabStop = true;
            this.rbScheduleOnUserIdle.Text = "Automatically backup all projects whenever my computer is idle for:";
            this.rbScheduleOnUserIdle.UseVisualStyleBackColor = true;
            this.rbScheduleOnUserIdle.CheckedChanged += new System.EventHandler(this.rbScheduleOnUserIdle_CheckedChanged);
            // 
            // rbManualOnly
            // 
            this.rbManualOnly.AutoSize = true;
            this.rbManualOnly.Location = new System.Drawing.Point(7, 20);
            this.rbManualOnly.Name = "rbManualOnly";
            this.rbManualOnly.Size = new System.Drawing.Size(168, 17);
            this.rbManualOnly.TabIndex = 0;
            this.rbManualOnly.TabStop = true;
            this.rbManualOnly.Text = "Only perform manual backups.";
            this.rbManualOnly.UseVisualStyleBackColor = true;
            this.rbManualOnly.CheckedChanged += new System.EventHandler(this.rbManualOnly_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(25, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(394, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "ZippyBackup must be running (in the system tray) for scheduled backups to occur.";
            // 
            // cbLaunchWithWindows
            // 
            this.cbLaunchWithWindows.AutoSize = true;
            this.cbLaunchWithWindows.Location = new System.Drawing.Point(7, 7);
            this.cbLaunchWithWindows.Name = "cbLaunchWithWindows";
            this.cbLaunchWithWindows.Size = new System.Drawing.Size(236, 17);
            this.cbLaunchWithWindows.TabIndex = 0;
            this.cbLaunchWithWindows.Text = "Launch &ZippyBackup when I start Windows.";
            this.cbLaunchWithWindows.UseVisualStyleBackColor = true;
            this.cbLaunchWithWindows.CheckedChanged += new System.EventHandler(this.cbLaunchWithWindows_CheckedChanged);
            // 
            // tabBackups
            // 
            this.tabBackups.Controls.Add(this.btnVerify);
            this.tabBackups.Controls.Add(this.btnRestore);
            this.tabBackups.Controls.Add(this.btnRunComplete);
            this.tabBackups.Controls.Add(this.btnRunAll);
            this.tabBackups.Controls.Add(this.labelBackups);
            this.tabBackups.Controls.Add(this.btnRun);
            this.tabBackups.Controls.Add(this.listBackupsSx);
            this.tabBackups.Location = new System.Drawing.Point(4, 22);
            this.tabBackups.Name = "tabBackups";
            this.tabBackups.Padding = new System.Windows.Forms.Padding(3);
            this.tabBackups.Size = new System.Drawing.Size(563, 347);
            this.tabBackups.TabIndex = 0;
            this.tabBackups.Text = "Backups";
            this.tabBackups.UseVisualStyleBackColor = true;
            // 
            // btnVerify
            // 
            this.btnVerify.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVerify.Location = new System.Drawing.Point(462, 268);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new System.Drawing.Size(92, 33);
            this.btnVerify.TabIndex = 8;
            this.btnVerify.Text = "Verify All";
            this.btnVerify.UseVisualStyleBackColor = true;
            this.btnVerify.Click += new System.EventHandler(this.btnVerify_Click);
			// 
            // btnRestore
            // 
            this.btnRestore.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRestore.Location = new System.Drawing.Point(364, 268);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(92, 33);
            this.btnRestore.TabIndex = 7;
            this.btnRestore.Text = "Restore";
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // btnRunComplete
            // 
            this.btnRunComplete.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRunComplete.Image = global::ZippyBackup.Properties.Resources.ButtonRunFull;
            this.btnRunComplete.Location = new System.Drawing.Point(334, 307);
            this.btnRunComplete.Name = "btnRunComplete";
            this.btnRunComplete.Size = new System.Drawing.Size(122, 34);
            this.btnRunComplete.TabIndex = 6;
            this.btnRunComplete.UseVisualStyleBackColor = true;
            this.btnRunComplete.Click += new System.EventHandler(this.btnRunComplete_Click);
            // 
            // btnRunAll
            // 
            this.btnRunAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRunAll.Image = global::ZippyBackup.Properties.Resources.ButtonRunAll;
            this.btnRunAll.Location = new System.Drawing.Point(7, 307);
            this.btnRunAll.Name = "btnRunAll";
            this.btnRunAll.Size = new System.Drawing.Size(126, 34);
            this.btnRunAll.TabIndex = 5;
            this.btnRunAll.UseVisualStyleBackColor = true;
            this.btnRunAll.Click += new System.EventHandler(this.btnRunAll_Click);
            // 
            // labelBackups
            // 
            this.labelBackups.AutoSize = true;
            this.labelBackups.Location = new System.Drawing.Point(6, 234);
            this.labelBackups.Name = "labelBackups";
            this.labelBackups.Size = new System.Drawing.Size(71, 13);
            this.labelBackups.TabIndex = 4;
            this.labelBackups.Text = "labelBackups";
            // 
            // btnRun
            // 
            this.btnRun.Image = ((System.Drawing.Image)(resources.GetObject("btnRun.Image")));
            this.btnRun.Location = new System.Drawing.Point(462, 307);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(92, 34);
            this.btnRun.TabIndex = 3;
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // listBackupsSx
            // 
            this.listBackupsSx.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBackupsSx.FormattingEnabled = true;
            this.listBackupsSx.ItemHeight = 20;
            this.listBackupsSx.Location = new System.Drawing.Point(7, 7);
            this.listBackupsSx.Name = "listBackupsSx";
            this.listBackupsSx.Size = new System.Drawing.Size(550, 224);
            this.listBackupsSx.TabIndex = 0;
            this.listBackupsSx.SelectedIndexChanged += new System.EventHandler(this.listBackupsSx_SelectedIndexChanged);
            // 
            // TabControl
            // 
            this.TabControl.Controls.Add(this.tabBackups);
            this.TabControl.Controls.Add(this.tabSchedule);
            this.TabControl.Controls.Add(this.tabManage);
            this.TabControl.Controls.Add(this.tabAbout);
            this.TabControl.Location = new System.Drawing.Point(12, 12);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new System.Drawing.Size(571, 373);
            this.TabControl.TabIndex = 0;
            this.TabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(595, 397);
            this.Controls.Add(this.TabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "ZippyBackup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.tabAbout.ResumeLayout(false);
            this.tabAbout.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabManage.ResumeLayout(false);
            this.tabSchedule.ResumeLayout(false);
            this.tabSchedule.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabBackups.ResumeLayout(false);
            this.tabBackups.PerformLayout();
            this.TabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer GUIRefreshTimer;
        private System.Windows.Forms.Timer ScheduleTimer;
        private System.Windows.Forms.TabPage tabAbout;
        private System.Windows.Forms.Button btnSourceForge;
        private System.Windows.Forms.Button btnCheckForUpdates;
        private System.Windows.Forms.LinkLabel linkZippyBackupLicense;
        private System.Windows.Forms.LinkLabel linkBZip2License;
        private System.Windows.Forms.LinkLabel linkZLibLicense;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.LinkLabel linkDotNetZipDonate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkDotNetZipLicense;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabManage;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.ListBox listManageBackups;
        private System.Windows.Forms.TabPage tabSchedule;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnEMailSettings;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbEMailFreq;
        private System.Windows.Forms.CheckBox cbEMailReminders;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbRoutineMinutes;
        private System.Windows.Forms.RadioButton rbScheduleRoutine;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbAlertDays;
        private System.Windows.Forms.CheckBox cbWarnOnAgedBackup;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbIdleMinutes;
        private System.Windows.Forms.RadioButton rbScheduleOnUserIdle;
        private System.Windows.Forms.RadioButton rbManualOnly;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbLaunchWithWindows;
        private System.Windows.Forms.TabPage tabBackups;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnRunComplete;
        private System.Windows.Forms.Button btnRunAll;
        private System.Windows.Forms.Label labelBackups;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.ListBox listBackupsSx;
        private System.Windows.Forms.TabControl TabControl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkAlphaVSSLicense;
        private System.Windows.Forms.LinkLabel linkAlphaFSLicense;
        private System.Windows.Forms.Button btnVerify;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tbVerifyDays;
        private System.Windows.Forms.Label label9;
    }
}

