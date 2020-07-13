namespace ZippyBackup.User_Interface
{
    partial class TreeSelectForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TreeSelectForm));
            this.Tree = new System.Windows.Forms.TreeView();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Tree
            // 
            this.Tree.CheckBoxes = true;
            this.Tree.Location = new System.Drawing.Point(13, 13);
            this.Tree.Name = "Tree";
            this.Tree.Size = new System.Drawing.Size(383, 325);
            this.Tree.TabIndex = 0;
            this.Tree.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.Tree_AfterCheck);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(321, 344);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // TreeSelectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 378);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.Tree);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TreeSelectForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Select Folders";
            this.Load += new System.EventHandler(this.TreeSelectForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        public System.Windows.Forms.TreeView Tree;
    }
}