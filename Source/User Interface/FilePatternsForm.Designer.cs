namespace ZippyBackup.User_Interface
{
    partial class FilePatternsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilePatternsForm));
            this.btnRemove = new System.Windows.Forms.Button();
            this.listPatterns = new System.Windows.Forms.ListBox();
            this.textNewPattern = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.labelPrompt = new System.Windows.Forms.Label();
            this.linkBuiltin = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // btnRemove
            // 
            this.btnRemove.Enabled = false;
            this.btnRemove.Location = new System.Drawing.Point(164, 47);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.TabIndex = 12;
            this.btnRemove.Text = "&Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // listPatterns
            // 
            this.listPatterns.FormattingEnabled = true;
            this.listPatterns.Location = new System.Drawing.Point(12, 47);
            this.listPatterns.Name = "listPatterns";
            this.listPatterns.Size = new System.Drawing.Size(146, 238);
            this.listPatterns.TabIndex = 11;
            this.listPatterns.SelectedIndexChanged += new System.EventHandler(this.listPatterns_SelectedIndexChanged);
            // 
            // textNewPattern
            // 
            this.textNewPattern.Location = new System.Drawing.Point(13, 292);
            this.textNewPattern.Name = "textNewPattern";
            this.textNewPattern.Size = new System.Drawing.Size(145, 20);
            this.textNewPattern.TabIndex = 13;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(164, 292);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 14;
            this.btnAdd.Text = "&Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnOK.Location = new System.Drawing.Point(163, 335);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 15;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // labelPrompt
            // 
            this.labelPrompt.AutoSize = true;
            this.labelPrompt.Location = new System.Drawing.Point(12, 13);
            this.labelPrompt.Name = "labelPrompt";
            this.labelPrompt.Size = new System.Drawing.Size(62, 13);
            this.labelPrompt.TabIndex = 16;
            this.labelPrompt.Text = "labelPrompt";
            // 
            // linkBuiltin
            // 
            this.linkBuiltin.AutoSize = true;
            this.linkBuiltin.Location = new System.Drawing.Point(15, 335);
            this.linkBuiltin.Name = "linkBuiltin";
            this.linkBuiltin.Size = new System.Drawing.Size(83, 13);
            this.linkBuiltin.TabIndex = 17;
            this.linkBuiltin.TabStop = true;
            this.linkBuiltin.Text = "View Built-in List";
            this.linkBuiltin.Visible = false;
            this.linkBuiltin.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkBuiltin_LinkClicked);
            // 
            // FilePatternsForm
            // 
            this.AcceptButton = this.btnAdd;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.btnOK;
            this.ClientSize = new System.Drawing.Size(252, 374);
            this.Controls.Add(this.linkBuiltin);
            this.Controls.Add(this.labelPrompt);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.textNewPattern);
            this.Controls.Add(this.listPatterns);
            this.Controls.Add(this.btnRemove);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FilePatternsForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Excluded File Extensions";
            this.Load += new System.EventHandler(this.FilePatternsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.ListBox listPatterns;
        private System.Windows.Forms.TextBox textNewPattern;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label labelPrompt;
        private System.Windows.Forms.LinkLabel linkBuiltin;

    }
}