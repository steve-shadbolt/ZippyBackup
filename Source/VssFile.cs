using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alphaleonis.Win32.Vss;
using System.IO;
using System.Diagnostics;
using Ionic.Zip;

namespace ZippyBackup
{
    namespace Vss
    {
        public class ShadowVolume
        {
            public ShadowVolume(string Volume) { Initialize(Volume, false); }
            public ShadowVolume(string Volume, bool IncludeBootableSystemState) { Initialize(Volume, IncludeBootableSystemState); }

            private void Initialize(string Volume, bool IncludeBootableSystemState)
            {
                //string filename = @"C:\Windows\system32\config\sam";
                //FileInfo fiSource = new FileInfo(filename);
                //String Volume = fiSource.Directory.Root.Name;

                // VSS step 1: Initialize
                IVssImplementation vss = VssUtils.LoadImplementation();
                IVssBackupComponents backup = vss.CreateVssBackupComponents();
                backup.InitializeForBackup(null);

                // VSS step 2: Getting Metadata from all the VSS writers
                backup.GatherWriterMetadata();

                // VSS step 3: VSS Configuration
                backup.SetContext((VssVolumeSnapshotAttributes)0);
                backup.SetBackupState(false, IncludeBootableSystemState, Alphaleonis.Win32.Vss.VssBackupType.Full, false);

                // VSS step 4: Declaring the Volumes that we need to use in this beckup.
                // The Snapshot is a volume element (hence the name "Volume Shadow-Copy").
                // For each file that we nee to copy we have to make sure that the proper volume is included in the "Snapshot Set".
                Guid SetGuid = backup.StartSnapshotSet();
                Guid VolumeGuid = backup.AddToSnapshotSet(Volume, Guid.Empty);

                // VSS step 5: Preparation (Writers & Provaiders need to start preparation)
                backup.PrepareForBackup();
                // VSS step 6: Create a Snapshot For each volume in the "Snapshot Set"
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

                VssSnapshotProperties SnapshotProperties = backup.GetSnapshotProperties(VolumeGuid);                
                DirectoryInfo diShadowRoot = new DirectoryInfo(SnapshotProperties.SnapshotDeviceObject);
                DirectoryInfo[] Folders = diShadowRoot.GetDirectories();
            }
        }
    }
}
