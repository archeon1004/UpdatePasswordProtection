using System;
using System.Management.Automation;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

namespace UpdatePasswordPluginModule
{
    [Cmdlet(VerbsCommon.Get, "UPLockedOutUser")]
    public class LockedOutUser : Cmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Username { get; set; }
        protected override void ProcessRecord()
        {
            ConfigHelper config = new ConfigHelper();
            try
            {
                int retValue = SQLHelper.GetUserCounter(UserName: Username);
                if(retValue == -1) {
                    WriteObject($"User: '{Username}' has not been found as lockedout by update password plugin");
                }
                else if(retValue == 0)
                {
                    WriteObject($"User: '{Username}' has not been found as lockedout by update password plugin");
                }
                else if(retValue >= config.RequestThreshold)
                {
                    WriteWarning($"User: '{Username}' is locked out");
                    WriteObject($"User: '{Username}' has been lockedout due to {retValue} requests failed");
                }
                else
                {
                    WriteObject($"User: '{Username}' has been registered {retValue} times by update password plugin");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }        
        }
    }
    [Cmdlet("Invoke", "UPLogConfig")]
    public class PluginLogConfiguration : Cmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                const string eventsource = "ADFSUpdatePasswordPlugin";
                const string eventLog = "Application";
                WriteObject($"Configuring event log source {eventsource} for AD FS Update Password Plugin in '{eventLog}' log");
                if (!EventLog.SourceExists(eventsource))
                {
                    WriteWarning("Source not exist. Creating event source.");
                    EventLog.CreateEventSource(source: eventsource, logName: eventLog);
                }
                try
                {
                    EventLog ev =new EventLog(logName: eventLog, machineName: ".",source: eventsource);
                    ev.Dispose();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
    [Cmdlet(VerbsCommon.Get,"UPconfig")]
    [CmdletBinding]
    public class PluginConfig : Cmdlet
    {
        protected override void ProcessRecord()
        {
            ConfigHelper config = new ConfigHelper();
            WriteObject(config);
        }
    }
    [Cmdlet(VerbsCommon.Clear,"UPLockedOutUser")]
    [CmdletBinding]
    public class ClearLockoutUser : Cmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Username { get; set; }
        protected override void ProcessRecord()
        {
            try
            {
                SQLHelper.ResetUserCounter(UserName: Username);
                WriteObject($"User: '{Username}' has been unlocked on update password endpoint");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

/*
protected override void ProcessRecord()
{
    RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Path, true);
    registryKey.SetValue(Key, Value);
    WriteObject("Registry key added successfully.");
}*/