namespace ZippyBackup.User_Interface
{
    partial class OverwritePromptForm
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
            this.labelMsg = new System.Windows.Forms.Label();
            this.btnYes = new System.Windows.Forms.Button();
            this.btnNo = new System.Windows.Forms.Button();
            this.btnYesAll = new System.Windows.Forms.Button();
            this.btnNoAll = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelMsg
            // 
            this.labelMsg.AutoSize = true;
            this.labelMsg.Location = new System.Drawing.Point(21, 22);
            this.labelMsg.Name = "labelMsg";
            this.labelMsg.Size = new System.Drawing.Size(35, 13);
            this.labelMsg.TabIndex = 0;
            this.labelMsg.Text = "label1";
            // 
            // btnYes
            // 
            this.btnYes.Location = new System.Drawing.Point(459, 100);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(75, 23);
            this.btnYes.TabIndex = 1;
            this.btnYes.Text = "Yes";
            this.btnYes.UseVisualStyleBackColor = true;
            this.btnYes.Click += new System.EventHandler(this.btnYes_Click);
            // 
            // btnNo
            // 
            this.btnNo.Location = new System.Drawing.Point(540, 100);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(75, 23);
            this.btnNo.TabIndex = 2;
            this.btnNo.Text = "No";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // btnYesAll
            // 
            this.btnYesAll.Location = new System.Drawing.Point(621, 100);
            this.btnYesAll.Name = "btnYesAll";
            this.btnYesAll.Size = new System.Drawing.Size(75, 23);
            this.btnYesAll.TabIndex = 3;
            this.btnYesAll.Text = "Yes to All";
            this.btnYesAll.UseVisualStyleBackColor = true;
            this.btnYesAll.Click += new System.EventHandler(this.btnYesAll_Click);
            // 
            // btnNoAll
            // 
            this.btnNoAll.Location = new System.Drawing.Point(702, 100);
            this.btnNoAll.Name = "btnNoAll";
            this.btnNoAll.Size = new System.Drawing.Size(75, 23);
            this.btnNoAll.TabIndex = 4;
            this.btnNoAll.Text = "No to All";
            this.btnNoAll.UseVisualStyleBackColor = true;
            this.btnNoAll.Click += new System.EventHandler(this.btnNoAll_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(378, 100);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // OverwritePromptForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(789, 134);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnNoAll);
            this.Controls.Add(this.btnYesAll);
            this.Controls.Add(this.btnNo);
            this.Controls.Add(this.btnYes);
            this.Controls.Add(this.labelMsg);
            this.Name = "OverwritePromptForm";
            this.Text = "Warning";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnYes;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.Button btnYesAll;
        private System.Windows.Forms.Button btnNoAll;
        public System.Windows.Forms.Label labelMsg;
        private System.Windows.Forms.Button btnCancel;
    }
}