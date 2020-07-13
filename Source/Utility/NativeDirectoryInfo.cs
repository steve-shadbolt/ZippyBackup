using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.ComponentModel;

namespace ZippyBackup
{
    public class NativeDirectoryInfo
    {
        public const int MAX_PATH = 260;
        public const int MAX_ALTERNATE = 14;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ALTERNATE)]
            public string cAlternate;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFindHandle FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FindClose(SafeHandle hFindFile);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool FindNextFile(SafeHandle hFindFile, out WIN32_FIND_DATA lpFindFileData);

        public sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
        {            
            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            private SafeFindHandle()
                : base(true)
            {
            }

            private SafeFindHandle(IntPtr preExistingHandle, bool ownsHandle)
                : base(ownsHandle)
            {
                base.SetHandle(preExistingHandle);
            }

            protected override bool ReleaseHandle()
            {
                if (!(IsInvalid || IsClosed))
                {
                    return FindClose(this);
                }
                return (IsInvalid || IsClosed);
            }

            protected override void Dispose(bool disposing)
            {
                if (!(IsInvalid || IsClosed))
                {
                    FindClose(this);
                }
                base.Dispose(disposing);
            }
        }



        private const int ERROR_FILE_NOT_FOUND = 0x2;
        private const int ERROR_PATH_NOT_FOUND = 0x3;
        private const int ERROR_LOGON_FAILURE = 0x52E;

        public static void CheckAccessToDirectory(string Directory)
        {
            Directory = Directory.TrimEnd();
            if (Directory.Contains("*")) throw new ArgumentException("Directory name cannot contain wildcards.");
            if (Directory.EndsWith("\\")) Directory = Directory.Substring(0, Directory.Length - 1);
            Directory = Directory + "\\*.*";

            WIN32_FIND_DATA FindData;
            using (SafeFindHandle FindHandle = FindFirstFile(Directory, out FindData))
            {
                if (FindHandle.IsInvalid)
                {
                    int LastError = Marshal.GetLastWin32Error();
                    switch (LastError)
                    {
                        case ERROR_FILE_NOT_FOUND: throw new DirectoryNotFoundException();
                        case ERROR_PATH_NOT_FOUND: throw new DirectoryNotFoundException();
                        case ERROR_LOGON_FAILURE: throw new System.Security.Authentication.InvalidCredentialException();                        
                        default:
                            {
                                Win32Exception InnerException = new Win32Exception(LastError);
                                throw new IOException("Unable to enumerate path '" + Directory + "':  " + InnerException.Message + "\n\nError Code: " + LastError.ToString(), InnerException);
                            }
                    }
                }
                // Access was granted. 
                return;
            }
        }
    }
}
