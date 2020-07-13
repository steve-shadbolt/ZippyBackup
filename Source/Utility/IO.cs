// ZippyBackup utilizes the Alphaleonis.Win32.Filesystem namespace instead of System.IO.  However, AlphaFS does not provide the
// entire System.IO functionality, so here we provide cross-references to patch the problem.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alphaleonis.Win32.Vss;
using Alphaleonis.Win32.Filesystem;

namespace ZippyBackup.IO
{
    public class FileNotFoundException : System.IO.FileNotFoundException
    {
        public FileNotFoundException() : base() { }
        public FileNotFoundException(string message) : base(message) { }
        public FileNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class DirectoryNotFoundException : System.IO.DirectoryNotFoundException
    {
        public DirectoryNotFoundException() : base() { }
        public DirectoryNotFoundException(string message) : base(message) { }
        public DirectoryNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class FileStream : System.IO.FileStream
    {
        public FileStream(Microsoft.Win32.SafeHandles.SafeFileHandle handle, System.IO.FileAccess access) : base(handle, (System.IO.FileAccess)(int)access) { }
        public FileStream(string path, System.IO.FileMode mode) : base(path, (System.IO.FileMode)(int)mode) { }
        public FileStream(string path, System.IO.FileMode mode, System.IO.FileAccess access) : base(path, (System.IO.FileMode)(int)mode, (System.IO.FileAccess)(int)access) { }
        public FileStream(string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share) : base(path, (System.IO.FileMode)(int)mode, (System.IO.FileAccess)(int)access, (System.IO.FileShare)(int)share) { }
    }    

    public class MemoryStream : System.IO.MemoryStream
    {
        public MemoryStream() : base() { }
        public MemoryStream(int capacity) : base(capacity) { }
        public MemoryStream(byte[] buffer) : base(buffer) { }
    }    

    public class PendingAlphaleonisFileStream : System.IO.Stream
    {
        string FullName;

        private System.IO.FileStream m_Underlying;
        protected System.IO.FileStream Underlying
        {
            get
            {
                if (m_Underlying != null) return m_Underlying;
                throw new Exception("Cannot access Pending FileStream before opening stream.  Pending FileStream File: " + FullName);
            }
        }

        public virtual void Open()
        {
            Exception LastError = null;
            for (int Retry = 0; Retry < 10; Retry++)
            {
                try
                {
                    m_Underlying = Alphaleonis.Win32.Filesystem.File.OpenRead(FullName);
                    return;
                }
                catch (Exception exc)
                {
                    ZippyBackup.User_Interface.ZippyForm.LogWriteLine(ZippyBackup.LogLevel.HeavyDebug, "\tException while opening file: " + exc.ToString());
                    LastError = exc;
                    System.Threading.Thread.Sleep(100);
                }
            }
            ZippyBackup.User_Interface.ZippyForm.LogWriteLine(ZippyBackup.LogLevel.HeavyDebug, "\tTen failures detected.  Giving up on file.");
            throw LastError;
        }

        public override void Close()
        {
            if (m_Underlying != null)
            {
                m_Underlying.Close();
                m_Underlying.Dispose();
                m_Underlying = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { if (m_Underlying != null) { m_Underlying.Dispose(); m_Underlying = null; } }
        }

        public PendingAlphaleonisFileStream(string FullName)
        {
            this.FullName = FullName;
        }

        #region "Pass-thru to Underlying stream"

        public override bool CanRead
        {
            get { return Underlying.CanRead; }
        }

        public override bool CanSeek
        {
            get { return Underlying.CanSeek; }
        }

        public override bool CanTimeout
        {
            get { return Underlying.CanTimeout; }
        }

        public override bool CanWrite
        {
            get { return Underlying.CanWrite; }
        }                

        public override void Flush()
        {
            Underlying.Flush();
        }

        public override long Length
        {
            get { return Underlying.Length; }
        }

        public override long Position
        {
            get
            {
                return Underlying.Position;
            }
            set
            {
                Underlying.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Underlying.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return Underlying.ReadByte();
        }

        public override int ReadTimeout
        {
            get
            {
                return Underlying.ReadTimeout;
            }
            set
            {
                Underlying.ReadTimeout = value;
            }
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            return Underlying.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Underlying.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Underlying.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            Underlying.WriteByte(value);
        }

        public override int WriteTimeout
        {
            get
            {
                return Underlying.WriteTimeout;
            }
            set
            {
                Underlying.WriteTimeout = value;
            }
        }

        #endregion
    }

    /// <summary>
    /// ZippyFileStream provides I/O access for all files read for archiving in ZippyBackup.  It provides the following modifications
    /// over ordinary IO FileStream:
    /// 1. Based on the Alphaleonis I/O system, which is compatible with VSS and other special system filenames.
    /// 2. Based on PendingAlphaleonisFileStream, which stores the filename and holds off on actually opening the file until requested.
    /// 3. Provides a buffering scheme that allows efficient access using a set of credentials specific to the file.
    /// </summary>
    public class ZippyFileStream : PendingAlphaleonisFileStream
    {
        private const int BufferSize = 1048576;         // 1MiB
        private RingBuffer m_ReadBuffer;
        private RingBuffer m_WriteBuffer;
        StoredNetworkCredentials m_Credentials;

        public ZippyFileStream(string FullName, StoredNetworkCredentials Credentials)
            : base(FullName)
        {
            m_Credentials = Credentials;
        }

        public override void Open()
        {
            m_ReadBuffer = new RingBuffer(BufferSize);
            m_WriteBuffer = new RingBuffer(BufferSize);
            // Note that the Impersonator class (part of ZippyBackup), will just "pass-through" if m_Credentials.Provided is false.
            using (Impersonator im = new Impersonator(m_Credentials)) base.Open();
        }

        public override void Close()
        {
            Flush();
            using (Impersonator im = new Impersonator(m_Credentials)) base.Close();
            if (m_ReadBuffer != null) { m_ReadBuffer.Dispose(); m_ReadBuffer = null; }
            if (m_WriteBuffer != null) { m_WriteBuffer.Dispose(); m_WriteBuffer = null; }            
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { 
                Close();
                if (m_ReadBuffer != null) { m_ReadBuffer.Dispose(); m_ReadBuffer = null; }
                if (m_WriteBuffer != null) { m_WriteBuffer.Dispose(); m_WriteBuffer = null; }
                base.Dispose(disposing); 
            }
        }

        public override void Flush()
        {
            if (m_ReadBuffer != null && m_ReadBuffer.Length > 0) m_ReadBuffer.Clear();
            if (m_WriteBuffer != null && m_WriteBuffer.Length > 0)
            {
                using (Impersonator im = new Impersonator(m_Credentials))
                {
                    byte[] tmp = new byte[m_WriteBuffer.Length];
                    base.Write(tmp, 0, tmp.Length);
                    tmp = null;
                }
                m_WriteBuffer.Clear();
            }
        }

        #region "Basic I/O Operations"        

        public override bool CanRead
        {
            get { return base.CanRead; }
        }

        public override bool CanSeek
        {
            get { return base.CanSeek; }
        }

        public override bool CanTimeout
        {
            get { return base.CanTimeout; }
        }

        public override bool CanWrite
        {
            get { return base.CanWrite; }
        }        

        public override long Length
        {
            get {
                long BaseLength;
                using (Impersonator im = new Impersonator(m_Credentials)) { BaseLength = base.Length; }
                if (m_WriteBuffer.Length == 0) return BaseLength;
                long FromEnd = BaseLength - base.Position;

                return (FromEnd <= m_WriteBuffer.Length) ? (BaseLength + m_WriteBuffer.Length - FromEnd) : BaseLength;
            }
        }

        public override long Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                Flush();
                base.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int AlreadyRead = 0;
            if (m_ReadBuffer.Length > 0)
            {
                int FromQueue = Math.Min(m_ReadBuffer.Length, count);
                m_ReadBuffer.Dequeue(buffer, offset, FromQueue);
                offset += FromQueue;
                count -= FromQueue;
                AlreadyRead += FromQueue;                
            }

            if (count == 0) return AlreadyRead;

            using (Impersonator im = new Impersonator(m_Credentials))
            {
                if (count >= m_ReadBuffer.CapacityAvailable) return Underlying.Read(buffer, offset, count) + AlreadyRead;

                byte[] tmp = new byte[m_ReadBuffer.CapacityAvailable];
                int Read = Underlying.Read(tmp, 0, tmp.Length);

                if (Read < count) count = Read;
                Buffer.BlockCopy(tmp, 0, buffer, offset, count);
                AlreadyRead += count;

                m_ReadBuffer.Enqueue(tmp, count, Read - count);
                tmp = null;
                return AlreadyRead;
            }
        }

        private byte[] tiny = new byte[1];
        public override int ReadByte()
        {            
            if (Read(tiny, 0, 1) < 1) return -1;
            return tiny[0];
        }

        public override int ReadTimeout
        {
            get
            {
                return Underlying.ReadTimeout;
            }
            set
            {
                Underlying.ReadTimeout = value;
            }
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            Flush();
            return Underlying.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Underlying.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (m_WriteBuffer.CapacityAvailable >= count)
            {
                m_WriteBuffer.Enqueue(buffer, offset, count);
                return;
            }

            using (Impersonator im = new Impersonator(m_Credentials))
            {
                byte[] tmp = new byte[m_WriteBuffer.Length];
                m_WriteBuffer.Dequeue(tmp, 0, tmp.Length);
                Underlying.Write(tmp, 0, tmp.Length);
                Underlying.Write(buffer, 0, count);
            }
        }

        public override void WriteByte(byte value)
        {
            tiny[0] = value;
            Write(tiny, 0, 1);
        }

        public override int WriteTimeout
        {
            get
            {
                return Underlying.WriteTimeout;
            }
            set
            {
                Underlying.WriteTimeout = value;
            }
        }        

        #endregion
    }

    public class ShadowVolume : IDisposable
    {
        string Volume;
        bool IncludeBootableSystemState = false;

        public ShadowVolume(string Volume) { this.Volume = Volume; }
        public ShadowVolume(string Volume, bool IncludeBootableSystemState) { this.Volume = Volume; this.IncludeBootableSystemState = IncludeBootableSystemState; }

        public void Dispose()
        {
            Dispose(true);                  // Dispose of unmanaged resources.            
            GC.SuppressFinalize(this);      // Suppress finalization.
        }

        void Dispose(bool disposing)
        {            
            if (disposing)
            {
                if (backup != null) { backup.Dispose(); backup = null; }
            }            
        }

        IVssImplementation vss;

        /// <summary>
        /// When the backup object is released, the shadow volume/snapshot will also be deleted.
        /// </summary>
        IVssBackupComponents backup;

        public string ShadowRoot;

        public delegate void UpdateStatus(string Message);
        public event UpdateStatus OnStatusUpdate;

        public void TakeSnapshot()
        {
            Volume = Volume.Trim();
            if (!Volume.EndsWith("\\")) Volume = Volume + "\\";

            //string filename = @"C:\Windows\system32\config\sam";
            //FileInfo fiSource = new FileInfo(filename);
            //String Volume = fiSource.Directory.Root.Name;

            // VSS step 1: Initialize
            if (OnStatusUpdate != null) OnStatusUpdate("Initializing VSS...");
            vss = VssUtils.LoadImplementation();
            if (OnStatusUpdate != null) OnStatusUpdate("Creating VSS backup components...");
            backup = vss.CreateVssBackupComponents();

            if (OnStatusUpdate != null) OnStatusUpdate("Creating backup...");
            backup.InitializeForBackup(null);

            // VSS step 2: Getting Metadata from all the VSS writers
            if (OnStatusUpdate != null) OnStatusUpdate("Gathering metadata...");
            backup.GatherWriterMetadata();

            // VSS step 3: VSS Configuration
            if (OnStatusUpdate != null) OnStatusUpdate("Configuring VSS backup...");
            backup.SetContext((VssVolumeSnapshotAttributes)0);
            backup.SetBackupState(false, IncludeBootableSystemState, Alphaleonis.Win32.Vss.VssBackupType.Full, false);

            // VSS step 4: Declaring the Volumes that we need to use in this beckup.
            // The Snapshot is a volume element (hence the name "Volume Shadow-Copy").
            // For each file that we nee to copy we have to make sure that the proper volume is included in the "Snapshot Set".
            if (OnStatusUpdate != null) OnStatusUpdate("Configuring VSS snapshot...");
            Guid SetGuid = backup.StartSnapshotSet();
            Guid VolumeGuid = backup.AddToSnapshotSet(Volume, Guid.Empty);

            // VSS step 5: Preparation (Writers & Provaiders need to start preparation)
            if (OnStatusUpdate != null) OnStatusUpdate("Preparing for VSS backup...");
            backup.PrepareForBackup();
            // VSS step 6: Create a Snapshot For each volume in the "Snapshot Set"
            if (OnStatusUpdate != null) OnStatusUpdate("Capturing VSS snapshot (takes up to 60 seconds)...");
            backup.DoSnapshotSet();

            /***********************************
            /* At this point we have a snapshot!
            /* This action should not take more then 60 second, regardless of file or disk size.
            /* The snapshot is not a backup or any copy!
            /* please more information at http://technet.microsoft.com/en-us/library/ee923636.aspx
            /***********************************/

            // VSS step 7: Expose Snapshot
            /***********************************
            /* Snapshot path look like:
             * \\?\Volume{011682bf-23d7-11e2-93e7-806e6f6e6963}\
             * The build in method System.IO.File.Copy do not work with path like this,
             * Therefore, we are going to Expose the Snapshot to our application,
             * by mapping the Snapshot to new virtual volume
             * - Make sure that you are using a volume that is not already exist
             * - This is only for learning purposes. usually we will use the snapshot directly as i show in the next example in the blog
            /***********************************/

            if (OnStatusUpdate != null) OnStatusUpdate("Retrieving VSS snapshot properties...");
            VssSnapshotProperties SnapshotProperties = backup.GetSnapshotProperties(VolumeGuid);
            ShadowRoot = SnapshotProperties.SnapshotDeviceObject;

            if (OnStatusUpdate != null) OnStatusUpdate("VSS snapshot complete.");
        }
    }
}
