using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZippyBackup.Diagnostics
{
    class DiagnosticStreamWriter : TextWriter
    {
        public string FileName;

        public DiagnosticStreamWriter(string FileName)
        {
            this.FileName = FileName;
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override void Write(string text)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(FileName, true, Encoding)) sw.Write(text);
            }
            catch (Exception) { }
        }

        public override void WriteLine(string text)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(FileName, true, Encoding)) sw.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ": " + text);
            }
            catch (Exception) { }
        }
    }
}
