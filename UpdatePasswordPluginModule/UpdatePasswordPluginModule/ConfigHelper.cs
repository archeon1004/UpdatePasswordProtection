using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections;

namespace UpdatePasswordPluginModule
{
    public class ConfigHelper
    {
        public bool BypassPasswordUpdateProtection { get; set; }
        public int RequestThreshold { get; set; }
        public bool TorAuditOnly { get; set; }
        public bool Enabled { get; set; }
        public bool CheckIfTorNode { get; set; }
        public bool DebugFileLogEnabled { get; set; }
        public string FileLogPath { get; set; } = string.Empty;
        public string DatabaseFilePath { get; set; } = string.Empty;

        private const string regPath = "SOFTWARE\\ADFSUpdatePasswordPlugin";

        public ConfigHelper()
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
                if (oCheckIfTorNode != null)
                {
                    DatabaseFilePath = oDatabaseFilePath.ToString();
                }
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }
    }
}
