using System;
using System.Diagnostics;
using Microsoft.Win32;
namespace ThreatDetectionModule
{
    public class Config
    {
        internal bool BypassPasswordUpdateProtection { get; set; }
        internal int RequestThreshold { get; set; }
        internal bool EnableRiskClaim { get; set; }
        internal bool Enabled { get; set;  }
        internal bool CheckIfTorNode { get; set; }
        internal bool FileLogEnabled { get; set; }
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
                object oEnableRiskClaim = rk.GetValue("EnableRiskClaim");
                if (oEnableRiskClaim != null)
                {
                    EnableRiskClaim = oEnableRiskClaim.ToString() == "0" ? false : true;
                }
                object oCheckIfTorNode = rk.GetValue("CheckIfTorNode");
                if (oCheckIfTorNode != null)
                {
                    CheckIfTorNode = oCheckIfTorNode.ToString() == "0" ? false : true;
                }
            } 
            catch
            {
                BypassPasswordUpdateProtection = false;
                Enabled = false;
                RequestThreshold = -1;
                EnableRiskClaim = false;
                CheckIfTorNode = false;
            } 
        }
        public override string ToString() => $"Enabled: {Enabled};BypassPasswordUpdateProtection: {BypassPasswordUpdateProtection};RequestThreshold: {RequestThreshold};EnableRiskClaim: {EnableRiskClaim}; CheckIfTorNode: {CheckIfTorNode}";
        public void InitConfig()
        {
            WindowsLogger.CreateWinLog();
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
