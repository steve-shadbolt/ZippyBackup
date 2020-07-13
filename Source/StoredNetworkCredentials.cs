/** ZippyBackup
 *  Copyright (C) 2012-2013 by Wiley Black.  All rights reserved.
 *  See License.txt for licensing rules.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Net;

namespace ZippyBackup
{
    [XmlRoot("network-credential")]
    public class StoredNetworkCredentials
    {
        /// <summary>
        /// This is by no means secure, since a hacker could pull this out of ZippyBackup's executable.  But, it's
        /// a first step away from just storing the credentials in plaintext in the registry.
        /// </summary>
        internal static string ZippyBackupInternalPassword = "aizoi1izEa8z8uagazifi29a";

        [XmlAttribute]
        public string UserName;

        [XmlAttribute]
        public string EncryptedPassword;

        [XmlIgnore]
        public string Password
        {
            get
            {
                if (string.IsNullOrEmpty(EncryptedPassword)) return "";
                return StringAES.DecryptStringAES(EncryptedPassword, ZippyBackupInternalPassword);
            }
            set
            {
                if (string.IsNullOrEmpty(value)) { EncryptedPassword = ""; return; }
                EncryptedPassword = StringAES.EncryptStringAES(value, ZippyBackupInternalPassword);
            }
        }

        [XmlAttribute]
        public string Domain;

        /// <summary>
        /// Provided indicates whether the user provided any credentials in this StoredNetworkCredential
        /// object.
        /// </summary>
        [XmlIgnore]
        public bool Provided
        {
            get
            {
                return !String.IsNullOrEmpty(UserName);
            }
        }

        public StoredNetworkCredentials()
        {
        }

        public StoredNetworkCredentials(string UserName, string Password, string Domain)
        {
            this.UserName = UserName;
            this.Password = Password;
            this.Domain = Domain;
        }

        public StoredNetworkCredentials(NetworkCredential FromCredential)
        {
            this.UserName = FromCredential.UserName;
            this.Password = FromCredential.Password;
            this.Domain = FromCredential.Domain;
        }

        public NetworkCredential ToNetworkCredential()
        {
            return new NetworkCredential(UserName, Password, Domain);
        }

        public StoredNetworkCredentials Clone()
        {
            return new StoredNetworkCredentials(UserName, Password, Domain);
        }

        public void Clear()
        {
            UserName = "";
            Password = "";
            Domain = "";
        }
    }

    [XmlRoot("password")]
    public class StoredPassword
    {
        [XmlAttribute]
        public string EncryptedPassword;

        [XmlIgnore]
        public string Password
        {
            get
            {
                if (string.IsNullOrEmpty(EncryptedPassword)) return "";
                return StringAES.DecryptStringAES(EncryptedPassword, StoredNetworkCredentials.ZippyBackupInternalPassword);
            }
            set
            {
                if (string.IsNullOrEmpty(value)) { EncryptedPassword = ""; return; }
                EncryptedPassword = StringAES.EncryptStringAES(value, StoredNetworkCredentials.ZippyBackupInternalPassword);
            }
        }

        public StoredPassword()
        {
        }

        public StoredPassword(string Password)
        {
            this.Password = Password;
        }

        public StoredPassword Clone()
        {
            return new StoredPassword(Password);
        }

        public void Clear()
        {            
            Password = "";            
        }
    }
}
