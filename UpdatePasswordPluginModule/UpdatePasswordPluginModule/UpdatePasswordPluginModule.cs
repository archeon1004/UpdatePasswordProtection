using System;
using System.Management.Automation;
using Microsoft.Win32;
using System.Diagnostics;

namespace UpdatePasswordPluginModule
{
    [Cmdlet(VerbsCommon.Get, "UPLockedOutUser")]
    public class LockedOutUser : Cmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string Key { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        public string Value { get; set; }

        [Parameter(Position = 2, Mandatory = true)]
        public string Path { get; set; }

        protected override void ProcessRecord()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(Path, true);
            registryKey.SetValue(Key, Value);
            WriteObject("Registry key added successfully.");
        }
    }
    [Cmdlet("Invoke", "UPPluginLogConfiguration")]
    public class PluginLogConfiguration : Cmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                const string eventsource = "ADFSUpdatePasswordPlugin";
                const string eventLog = "Application";
                if (!EventLog.SourceExists(eventsource))
                {
                    EventLog.CreateEventSource(source: eventsource, logName: eventLog);
                }
                try
                {
                    EventLog ev =new EventLog(eventLog, ".", eventsource);
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
}