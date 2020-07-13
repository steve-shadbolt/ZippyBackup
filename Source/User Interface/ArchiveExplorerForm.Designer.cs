namespace ZippyBackup.User_Interface
{
    partial class ArchiveExplorerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArchiveExplorerForm));
            this.ArchiveTree = new System.Windows.Forms.TreeView();
            this.CurrentView = new System.Windows.Forms.ListView();
            this.btnExtract = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ArchiveTree
            // 
            this.ArchiveTree.Location = new System.Drawing.Point(12, 12);
            this.ArchiveTree.Name = "ArchiveTree";
            this.ArchiveTree.Size = new System.Drawing.Size(395, 377);
            this.ArchiveTree.TabIndex = 0;
            this.ArchiveTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ArchiveTree_AfterSelect);
            // 
            // CurrentView
            // 
            this.CurrentView.Location = new System.Drawing.Point(413, 12);
            this.CurrentView.Name = "CurrentView";
            this.CurrentView.Size = new System.Drawing.Size(388, 377);
            this.CurrentView.TabIndex = 1;
            this.CurrentView.UseCompatibleStateImageBehavior = false;
            this.CurrentView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.CurrentView_MouseClick);
            this.CurrentView.SelectedIndexChanged += new System.EventHandler(this.CurrentView_SelectedIndexChanged);
            this.CurrentView.DoubleClick += new System.EventHandler(this.CurrentView_DoubleClick);
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(681, 395);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(120, 32);
            this.btnExtract.TabIndex = 2;
            this.btnExtract.Text = "Extract...";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // ArchiveExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(813, 433);
            this.Controls.Add(this.btnExtract);
            this.Controls.Add(this.CurrentView);
            this.Controls.Add(this.ArchiveTree);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ArchiveExplorerForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Archive Explorer";
            this.Load += new System.EventHandler(this.ArchiveExplorerForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView ArchiveTree;
        private System.Windows.Forms.ListView CurrentView;
        private System.Windows.Forms.Button btnExtract;
    }
}