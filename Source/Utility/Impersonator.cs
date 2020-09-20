using System;
using System.IO;
using System.Net; 
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace ZippyBackup
{   
    /** IMPORTANT:
     * 
     *  Neither Impersonator nor NetworkConnection seems to really work.  The workaround I have for myself is that it seems to
     *  accept no credentials (which in these classes becomes just a pass-thru) and instead ZippyBackup seems to be relying on the
     *  user's account having access to the network store.
     */


#if true
    /// <summary>
    /// Allows code to be executed under the security context of a specified user account.
    /// </summary>
    /// <remarks> 
    ///
    /// Implements IDispose, so can be used via a using-directive or method calls;
    ///  ...
    ///
    ///  var imp = new Impersonator( "myUsername", "myDomainname", "myPassword" );
    ///  imp.UndoImpersonation();
    ///
    ///  ...
    ///
    ///   var imp = new Impersonator();
    ///  imp.Impersonate("myUsername", "myDomainname", "myPassword");
    ///  imp.UndoImpersonation();
    ///
    ///  ...
    ///
    ///  using ( new Impersonator( "myUsername", "myDomainname", "myPassword" ) )
    ///  {
    ///   ...
    ///   [code that executes under the new context]
    ///   ...
    ///  }
    ///
    ///  ...
    /// </remarks>
    public class NetworkConnection : IDisposable
    {
        public enum LogonType
        {
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK = 3,
            LOGON32_LOGON_BATCH = 4,
            LOGON32_LOGON_SERVICE = 5,
            LOGON32_LOGON_UNLOCK = 7,
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8, // Win2K or higher
            LOGON32_LOGON_NEW_CREDENTIALS = 9 // Win2K or higher
        };

        public enum LogonProvider
        {
            LOGON32_PROVIDER_DEFAULT = 0,
            LOGON32_PROVIDER_WINNT35 = 1,
            LOGON32_PROVIDER_WINNT40 = 2,
            LOGON32_PROVIDER_WINNT50 = 3
        };

        public enum ImpersonationLevel
        {
            SecurityAnonymous = 0,
            SecurityIdentification = 1,
            SecurityImpersonation = 2,
            SecurityDelegation = 3
        }

        class Win32NativeMethods
        {
            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern int LogonUser(string lpszUserName,
                string lpszDomain,
                string lpszPassword,
                int dwLogonType,
                int dwLogonProvider,
                ref IntPtr phToken);

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int DuplicateToken(IntPtr hToken,
                int impersonationLevel,
                ref IntPtr hNewToken);

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool RevertToSelf();

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern bool CloseHandle(IntPtr handle);
        }

        private WindowsImpersonationContext _wic;

        public NetworkConnection(string NetworkLocation, StoredNetworkCredentials Credentials)
        {
            if (Credentials.Provided) Impersonate(NetworkLocation, Credentials.UserName, Credentials.Domain, Credentials.Password);
            // If no credentials provided, do nothing - it's not an error.
        }

        public NetworkConnection(string NetworkLocation, System.Net.NetworkCredential Credentials)
        {
            Impersonate(NetworkLocation, Credentials.UserName, Credentials.Domain, Credentials.Password);
        }

        /// <summary>
        /// Begins impersonation with the given credentials, Logon type and Logon provider.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="domainName">Name of the domain.</param>
        /// <param name="password">The password. <see cref="System.String"/></param>
        /// <param name="logonType">Type of the logon.</param>
        /// <param name="logonProvider">The logon provider. <see cref="Mit.Sharepoint.WebParts.EventLogQuery.Network.LogonProvider"/></param>
        public NetworkConnection(string NetworkLocation, string userName, string domainName, string password, LogonType logonType, LogonProvider logonProvider)
        {
            Impersonate(NetworkLocation, userName, domainName, password, logonType, logonProvider);
        }

        /// <summary>
        /// Begins impersonation with the given credentials.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="domainName">Name of the domain.</param>
        /// <param name="password">The password. <see cref="System.String"/></param>
        public NetworkConnection(string NetworkLocation, string userName, string domainName, string password)
        {
            Impersonate(NetworkLocation, userName, domainName, password, LogonType.LOGON32_LOGON_INTERACTIVE, LogonProvider.LOGON32_PROVIDER_DEFAULT);
        }        

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            UndoImpersonation();
        }

        /// <summary>
        /// Impersonates the specified user account.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="domainName">Name of the domain.</param>
        /// <param name="password">The password. <see cref="System.String"/></param>
        private void Impersonate(string NetworkLocation, string userName, string domainName, string password)
        {
            Impersonate(NetworkLocation, userName, domainName, password, LogonType.LOGON32_LOGON_INTERACTIVE, LogonProvider.LOGON32_PROVIDER_DEFAULT);
            //Impersonate(NetworkLocation, userName, domainName, password, LogonType.LOGON32_LOGON_NEW_CREDENTIALS, LogonProvider.LOGON32_PROVIDER_WINNT50);
        }

        /// <summary>
        /// Impersonates the specified user account.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="domainName">Name of the domain.</param>
        /// <param name="password">The password. <see cref="System.String"/></param>
        /// <param name="logonType">Type of the logon.</param>
        /// <param name="logonProvider">The logon provider. <see cref="Mit.Sharepoint.WebParts.EventLogQuery.Network.LogonProvider"/></param>
        private void Impersonate(string NetworkLocation, string userName, string domainName, string password, LogonType logonType, LogonProvider logonProvider)
        {
            try
            {
                UndoImpersonation();

                /*
    if (userName.Contains("\\") || userName.Contains("/"))
    {
        string[] tokens = userName.Split(new char[] { '\\', '/' });
        if (tokens.Length != 2) throw new Exception("Expected user name to contain at most one / or \\ character.  User name: " + userName);
        if (domainName.Trim().Length != 0) throw new Exception("Cannot specify a / or \\ in user name when domain is also given.  User name: " + userName + "  Domain: " + domainName);
        domainName = tokens[0];
        userName = tokens[1];
    }
                 */

                IntPtr logonToken = IntPtr.Zero;
                IntPtr logonTokenDuplicate = IntPtr.Zero;
                try
                {
                    // revert to the application pool identity, saving the identity of the current requestor
                    _wic = WindowsIdentity.Impersonate(IntPtr.Zero);

                    // do logon & impersonate
                    if (Win32NativeMethods.LogonUser(userName,
                        domainName,
                        password,
                        (int)logonType,
                        (int)logonProvider,
                        ref logonToken) != 0)
                    {
                        if (Win32NativeMethods.DuplicateToken(logonToken, (int)ImpersonationLevel.SecurityImpersonation, ref logonTokenDuplicate) != 0)
                        {
                            var wi = new WindowsIdentity(logonTokenDuplicate);
                            wi.Impersonate(); // discard the returned identity context (which is the context of the application pool)
                        }
                        else
                            ThrowSpecificException();
                    }
                    else
                        ThrowSpecificException();
                }
                finally
                {
                    if (logonToken != IntPtr.Zero)
                        Win32NativeMethods.CloseHandle(logonToken);

                    if (logonTokenDuplicate != IntPtr.Zero)
                        Win32NativeMethods.CloseHandle(logonTokenDuplicate);
                }
            }
            catch (Exception ex)
            {
                throw new IOException("Unable to access path:\n" + NetworkLocation + "\nAs username: " + userName + "\nOn domain: " + domainName + "\nError: " + ex.ToString());
            }
        }

        private const int ERROR_LOGON_FAILURE = 0x52E;

        private void ThrowSpecificException()
        {
            int LastError = Marshal.GetLastWin32Error();
            switch (LastError)
            {
                case ERROR_LOGON_FAILURE: throw new System.Security.Authentication.InvalidCredentialException();
                default:
                    throw new Win32Exception(LastError);
            }
        }

        /// <summary>
        /// Stops impersonation.
        /// </summary>
        private void UndoImpersonation()
        {
            // restore saved requestor identity
            if (_wic != null)
                _wic.Undo();
            _wic = null;
        }
    }

#else

    // An alternative to the above impersonation-based approach that relies on the mpr/WNetAddConnection2 API.
    public class NetworkConnection : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        class NetResource
        {
            public ResourceScope Scope;
            public ResourceType ResourceType;
            public ResourceDisplaytype DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        public enum ResourceScope : int
        {
            Connected = 1,
            GlobalNetwork,
            Remembered,
            Recent,
            Context
        };

        public enum ResourceType : int
        {
            Any = 0,
            Disk = 1,
            Print = 2,
            Reserved = 8,
        }

        public enum ResourceDisplaytype : int
        {
            Generic = 0x0,
            Domain = 0x01,
            Server = 0x02,
            Share = 0x03,
            File = 0x04,
            Group = 0x05,
            Network = 0x06,
            Root = 0x07,
            Shareadmin = 0x08,
            Directory = 0x09,
            Tree = 0x0a,
            Ndscontainer = 0x0b
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource,
            string password, string username, int flags);
        
        /*
        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection3(IntPtr hWndOwner,
            ref NetResource lpNetResource, string lpPassword,
            string lpUserName, int dwFlags);
         */

        [DllImport("mpr.dll")]
        static extern int WNetCancelConnection2(string lpName, Int32 dwFlags, bool bForce);

        public NetworkConnection(string NetworkLocation, StoredNetworkCredentials Credentials)
        {
            if (Credentials.Provided) Establish(NetworkLocation, Credentials.UserName, Credentials.Domain, Credentials.Password);
            // If no credentials provided, do nothing - it's not an error.
        }

        public NetworkConnection(string NetworkLocation, System.Net.NetworkCredential Credentials)
        {
            Establish(NetworkLocation, Credentials.UserName, Credentials.Domain, Credentials.Password);
        }

        ~NetworkConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool _established = false;
        string _networkName;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            _established = false;
            WNetCancelConnection2(_networkName, 0, true);
        }

        /// <summary>
        /// Impersonates the specified user account.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="domainName">Name of the domain.</param>
        /// <param name="password">The password. <see cref="System.String"/></param>
        public void Establish(string NetworkLocation, string userName, string domainName, string password)
        {
            if (_established) Dispose(false);
            _networkName = NetworkLocation;

            var netResource = new NetResource()
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = _networkName
            };

            var EffUserName = string.IsNullOrEmpty(domainName)
                ? userName
                : string.Format(@"{0}\{1}", domainName, userName);

            var result = WNetAddConnection2(
                netResource,
                password,
                userName,
                0);

            if (result != 0)
            {
                WNetCancelConnection2(_networkName, 0, true);
                try
                {
                    throw new Win32Exception(result);
                }
                catch (Win32Exception we)
                {
                    throw new IOException("Unable to access path:\n" + NetworkLocation + "\nAs username: " + EffUserName + "\nError: " + we.ToString());
                }
            }
            else 
                _established = true;
        }
    }

#endif
}