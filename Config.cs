using System;
using System.Diagnostics;
using Microsoft.Win32;
namespace ThreatDetectionModule
{
    public class Config
    {
        internal bool BypassPasswordUpdateProtection { get; set; }
        internal int RequestThreshold { get; set; }
        internal bool TorAuditOnly { get; set; }
        internal bool Enabled { get; set;  }
        internal bool CheckIfTorNode { get; set; }
        internal bool DebugFileLogEnabled { get; set; }
        internal string FileLogPath { get; set; } = string.Empty;
        internal string DatabaseFilePath { get; set; } = string.Empty;


        private const string regPath = "SOFTWARE\\ADFSUpdatePasswordPlugin";

        public Config()
        {
            try 
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(regPath);
                if (rk == null)
                {
                    throw new InvalidOperationException("RegistryKeyException");
                }
                object oEnabled = rk.GetValue("Enabled");
                if (oEnabled != null)
                {
                    Enabled = oEnabled.ToString() == "0" ? false : true;
                }
                object oBypassPasswordUpdateProtection = rk.GetValue("BypassPasswordUpdateProtection");
                if (oBypassPasswordUpdateProtection != null)
                {
                    BypassPasswordUpdateProtection = oBypassPasswordUpdateProtection.ToString() == "0" ? false : true;
                }
                object oRequestThreshold = rk.GetValue("RequestThreshold");
                if (oRequestThreshold != null)
                {
                    RequestThreshold = Convert.ToInt32(oRequestThreshold);
                }
                object oTorAuditOnly = rk.GetValue("TorAuditOnly");
                if (oTorAuditOnly != null)
                {
                    TorAuditOnly = oTorAuditOnly.ToString() == "0" ? false : true;
                }
                object oCheckIfTorNode = rk.GetValue("CheckIfTorNode");
                if (oCheckIfTorNode != null)
                {
                    CheckIfTorNode = oCheckIfTorNode.ToString() == "0" ? false : true;
                }
                object oDatabaseFilePath = rk.GetValue("DatabaseFilePath");
                if(oCheckIfTorNode != null)
                {
                    DatabaseFilePath = oDatabaseFilePath.ToString();
                }
            } 
            catch
            {
                BypassPasswordUpdateProtection = false;
                Enabled = false;
                RequestThreshold = -1;
                TorAuditOnly = false;
                CheckIfTorNode = false;
                DatabaseFilePath = @"C:\Windows\ADFS\UpdatePasswordCustomPlugin\";
            } 
        }
        public override string ToString() => $"Enabled: {Enabled};BypassPasswordUpdateProtection: {BypassPasswordUpdateProtection};RequestThreshold: {RequestThreshold};TorAuditOnly: {TorAuditOnly}; CheckIfTorNode: {CheckIfTorNode}";
        public void InitConfig()
        {
            try
            {
                SQLLiteHandlerClass.CreateDatabase(databasePath: DatabaseFilePath);
            }
            catch (Exception ex)
            {
                WindowsLogger.WriteWinLogEvent($"SQL Initialization failed. \nException:\n{ex.Message}", EventLogEntryType.Error);
                throw;
            } 
            WindowsLogger.WriteWinLogEvent("Reading Plugin Registry Configuration Configuration", EventLogEntryType.Information);
            try
            {
                TorModule.LoadTorExitNodes();
            }
            catch
            {
                WindowsLogger.WriteWinLogEvent($"Exception refreshing Tor Nodes. Disabling Tor Check", EventLogEntryType.Error);
            }
        }
    }
}
