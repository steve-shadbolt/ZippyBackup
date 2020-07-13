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
    public partial class FilePatternsForm : Form
    {
        public bool FilesMode = true;           // True: Files, False: Folders
        public bool ExtensionsMode = true;      // True: Extensions, False: Filenames/Patterns
                
        public List<string> Extensions
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (string Entry in listPatterns.Items) ret.Add(Entry);
                return ret;
            }

            set
            {
                ExtensionsMode = true;
                listPatterns.Items.Clear();
                foreach (string Entry in value)
                {
                    listPatterns.Items.Add(Entry);
                }
            }
        }

        public List<string> Patterns
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (string Entry in listPatterns.Items) ret.Add(Entry);
                return ret;
            }

            set
            {
                ExtensionsMode = false;
                listPatterns.Items.Clear();
                foreach (string Entry in value)
                {
                    listPatterns.Items.Add(Entry);
                }
            }
        }

        public List<string> m_Builtin = new List<string>();
        public List<string> Builtin
        {
            get { return m_Builtin; }
            set
            {
                m_Builtin = value;
                linkBuiltin.Visible = true;
            }
        }

        string OriginalPrompt;
        public string Prompt
        {
            get { return labelPrompt.Text; }
            set { OriginalPrompt = value; labelPrompt.Text = value; SetSize(); }
        }

        public FilePatternsForm()
        {
            InitializeComponent();
            labelPrompt.Text = Text;
        }
        
        void SetSize()
        {
            listPatterns.Top = labelPrompt.Bottom + 25;
            btnRemove.Top = listPatterns.Top;
            textNewPattern.Top = listPatterns.Bottom + 15;
            btnAdd.Top = textNewPattern.Top;
            btnOK.Top = btnAdd.Bottom + 25;
            linkBuiltin.Top = btnOK.Top;
        }

        private string GetItemLabel(bool Plural)
        {
            if (Plural)
                return (FilesMode ? "file" : "folder") + " " + (ExtensionsMode ? "extensions" : "names or patterns");
            else
                return (FilesMode ? "file" : "folder") + " " + (ExtensionsMode ? "extension" : "name or pattern");
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {            
            string NewExt = textNewPattern.Text.Trim();
            if (ExtensionsMode)
            {
                if (NewExt.StartsWith("*.")) NewExt.Substring(1);
                if (!NewExt.StartsWith(".")) NewExt = "." + NewExt;
            }
            if (NewExt.Length < 1)
            {
                MessageBox.Show("Please type a " + GetItemLabel(false) + " in the box to the left before clicking Add.");
                return;
            }            

            textNewPattern.Text = "";
            textNewPattern.Focus();

            foreach (string ExistingExt in listPatterns.Items)
            {
                if (ExistingExt.ToLower() == NewExt.ToLower())
                {
                    labelPrompt.Text = ExistingExt + " is already on the list.";
                    labelPrompt.ForeColor = System.Drawing.Color.Red;
                    return;
                }
            }

            foreach (string BuiltinExt in Builtin)
            {
                if (BuiltinExt.ToLower() == NewExt.ToLower())
                {                    
                    labelPrompt.Text = BuiltinExt + " is on the built-in list.";
                    labelPrompt.ForeColor = System.Drawing.Color.Red;
                    return;
                }
            }

            listPatterns.Items.Add(NewExt);
            labelPrompt.Text = OriginalPrompt;
            labelPrompt.ForeColor = System.Drawing.Color.Black;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listPatterns.SelectedItem == null) return;
            listPatterns.Items.Remove(listPatterns.SelectedItem);
        }

        private void listPatterns_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemove.Enabled = (listPatterns.SelectedItem != null);
        }        

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void linkBuiltin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("The following " + GetItemLabel(true) + " are built-in and automatically included in the list:\n\n" +
                Utility.ListToEnglish(Builtin, Utility.ListToEnglishStyle.Multiline, 60));
        }

        private void FilePatternsForm_Load(object sender, EventArgs e)
        {

        }        
    }
}
