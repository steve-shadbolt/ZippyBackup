/** ZippyBackup
 *  Copyright (C) 2012-2013 by Wiley Black.  All rights reserved.
 *  See License.txt for licensing rules.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace ZippyBackup
{     
    [XmlRoot("backup-schedule")]
    public class BackupSchedule
    {
        [XmlAttribute("schedule-on-user-idle")]
        public bool ScheduleOnUserIdle = false;

        [XmlAttribute("user-idle-minutes")]
        public int UserIdleMinutes = 60;

        [XmlAttribute("schedule-routine")]        
        public bool ScheduleRoutine = false;

        [XmlAttribute("schedule-routine-minutes")]
        public int RoutineMinutes = 60;

        [XmlAttribute("warn-user-if-backup-past-due")]
        public bool WarnUserIfBackupPastDue = true;

        [XmlAttribute("backup-past-due-after-days")]
        public int BackupPastDueAfterDays = 5;

        [XmlAttribute("verification-frequency-days")]
        public int VerificationFrequencyInDays = 30;

        [XmlAttribute("email-user-if-backup-past-due")]
        public bool EMailUserIfBackupPastDue = false;

        [XmlAttribute("email-frequency")]
        public int EMailFrequencyInDays = 4;

        [XmlElement("email-settings")]
        public EMailSettings EMailSettings = new EMailSettings();
    }

    public class EMailSettings
    {
        [XmlAttribute("email-from")]
        public string EMailFrom;

        [XmlAttribute("email-to")]
        public string EMailTo;

        [XmlAttribute("smtp-server")]
        public string SMTPServer;

        [XmlAttribute("smtp-port")]
        public int SMTPPort;

        [XmlAttribute("username")]
        public string Username;
        
        public StoredPassword Password = new StoredPassword();
    }
}
