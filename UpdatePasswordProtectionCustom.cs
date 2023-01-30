﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityServer.Public.ThreatDetectionFramework;
using System.Net;
using System.Diagnostics;

namespace ThreatDetectionModule
{
    public class UpdatePasswordProtectionCustom : Microsoft.IdentityServer.Public.ThreatDetectionFramework.ThreatDetectionModule, IPreAuthenticationThreatDetectionModule, IPostAuthenticationThreatDetectionModule, IRequestReceivedThreatDetectionModule
    {
        public override string VendorName => "Ignastech";
        public override string ModuleIdentifier => "UpdatePasswordProtectionCustom";

        internal Config _config = new Config();

        public string InternalVersion => "0.0.2.14";

        private readonly string EndpointName = "/adfs/portal/updatepassword";

        public override void OnAuthenticationPipelineLoad(ThreatDetectionLogger adfslogger, ThreatDetectionModuleConfiguration configData)
        {
            try
            {
                WindowsLogger.CreateWinLog();
                WindowsLogger.WriteWinLogEvent($"Initiating AD FS {ModuleIdentifier} plugin. Internal Version - {InternalVersion}", EventLogEntryType.Information);
                _config.InitConfig();
                WindowsLogger.WriteWinLogEvent($"Configured Plugin settings:{_config}",EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                adfslogger.WriteAdminLogErrorMessage(ex.ToString());
                throw;
            }
        }

        public override void OnAuthenticationPipelineUnload(ThreatDetectionLogger adfslogger)
        {
            WindowsLogger.WriteWinLogEvent($"OnAuthenticationPipelineUnload: Unloading plugin", EventLogEntryType.Information);
        }

        public override void OnConfigurationUpdate(ThreatDetectionLogger adfslogger, ThreatDetectionModuleConfiguration configData)
        {
            try
            {
                Config PluginConfig = new Config();
                _config = PluginConfig;
                _config.InitConfig();
            }
            catch (Exception ex)
            {
                adfslogger.WriteAdminLogErrorMessage(ex.ToString());
                throw;
            }
        }

        public Task<ThrottleStatus> EvaluateRequest(ThreatDetectionLogger adfslogger, RequestContext requestContext)
        {
            if(_config.CheckIfTorNode)
            {
                foreach (IPAddress ipAddress in requestContext.ClientIpAddresses)
                {
                    if (TorModule.IsTorExitNode(ipAddress))
                    {
                        string detailsMessage = $"{ipAddress} has been found to be from Tor network. Request will be denied\n" +
                            $"request details:\n" +
                            $"Local endpoint: {requestContext.LocalEndPointAbsolutePath}\n" +
                            $"Client Location: {requestContext.ClientLocation}\n" +
                            $"User Agent: {requestContext.UserAgentString}" +
                            $"Incoming Request Type: {requestContext.IncomingRequestType}";
                        WindowsLogger.WriteWinLogEvent($"{detailsMessage}",EventLogEntryType.Warning);
                        if(_config.TorAuditOnly == true)
                        {
                            WindowsLogger.WriteWinLogEvent($"AUDIT MODE: Tor Audit is enabled.Request has been allowed", EventLogEntryType.Warning);
                        }
                        else
                        {
                            return Task.FromResult(ThrottleStatus.Block);
                        }
                    }
                }
            }
            return Task.FromResult(ThrottleStatus.NotEvaluated);
        }
        public Task<ThrottleStatus> EvaluatePreAuthentication(ThreatDetectionLogger adfslogger, RequestContext requestContext, SecurityContext securityContext, ProtocolContext protocolContext, IList<Claim> additionalClams)
        {   
            if (requestContext.LocalEndPointAbsolutePath == EndpointName)
            {
                if (_config.Enabled == true)
                {
                    string [] userProps = securityContext.UserIdentifier.Split('\\');
                    string userName = userProps[1];
                    string userDomain = userProps[0];
                    if (!string.IsNullOrEmpty(userName))
                    {
                        try
                        {
                            WindowsLogger.WriteWinLogEvent($"Got Update Password request. Checking update password lockout status for user '{userName}'", EventLogEntryType.Information);
                            var counterTest = SQLLiteHandlerClass.CheckCounter(userName);
                            if (counterTest >= _config.RequestThreshold)
                            {
                                if(!(_config.RequestThreshold == 0))
                                {
                                    WindowsLogger.WriteWinLogEvent($"Detected user '{userName}'from '{userDomain}' hit the limit of '{_config.RequestThreshold}'. Request will be denied",EventLogEntryType.Error);
                                    if (_config.BypassPasswordUpdateProtection == true)
                                    {
                                        WindowsLogger.WriteWinLogEvent($"WARNING: BYPASS is enabled, request will be allowed. Normally this request would be rejected",EventLogEntryType.Warning);
                                        return Task.FromResult<ThrottleStatus>(ThrottleStatus.Allow);
                                    }
                                    else
                                    {
                                        return Task.FromResult<ThrottleStatus>(ThrottleStatus.Block);
                                    }
                                }
                                else
                                {
                                    WindowsLogger.WriteWinLogEvent($"Threshold set to 0. Will approve request", EventLogEntryType.Information);
                                }
                            }
                            else
                            {
                                WindowsLogger.WriteWinLogEvent($"User '{userName}' has not been detected as locked out. Will Allow request", EventLogEntryType.Information);
                            }
                        }
                        catch (Exception e)
                        {
                            WindowsLogger.WriteWinLogEvent($"Exception occured\n\r{e}", EventLogEntryType.Error);
                            return Task.FromResult<ThrottleStatus>(ThrottleStatus.Allow);
                        }
                    }
                    else
                    {
                        //WindowsLogger.WriteWinLogEvent($"Got empty user name for: '{securityContext.UserIdentifier}'");
                    }
                }
                else
                {
                    //WindowsLogger.WriteWinLogEvent("Plugin Enforcement Disabled. Allowing Request");
                }
            }
            else
            {
                //.WriteWinLogEvent("Request is not targeting update password endpoint. Skipping");
            }
            return Task.FromResult<ThrottleStatus>(ThrottleStatus.Allow);
        }
        public Task<RiskScore> EvaluatePostAuthentication(ThreatDetectionLogger adfslogger, RequestContext requestContext, SecurityContext securityContext, ProtocolContext protocolContext, AuthenticationResult authenticationResult, IList<Claim> additionalClams)
        {
            if (requestContext.LocalEndPointAbsolutePath == EndpointName)
            {
                if (_config.Enabled == true)
                {
                    string[] userProps = securityContext.UserIdentifier.Split('\\');
                    string userName = userProps[1];
                    string userDomain = userProps[0];
                    if (authenticationResult == AuthenticationResult.Failure)
                    {
                        WindowsLogger.WriteWinLogEvent($"User '{userName}' from '{userDomain}' Failed Authentication. Will Increase counter", EventLogEntryType.Warning);
                        try
                        {
                            SQLLiteHandlerClass.IncreaseCounter(userName);
                        }
                        catch (Exception ex)
                        {
                            WindowsLogger.WriteWinLogEvent($"Exception cought during Increasing Counter phase.\n\rDetails:\n\r{ex}", EventLogEntryType.Error);
                        }
                    }
                    else
                    {
                        //WindowsLogger.WriteWinLogEvent("User Authentication succeeded. Counter won't be increased and update password will be allowed.");
                    }
                }
                else
                {
                    //WindowsLogger.WriteWinLogEvent("Plugin Enforcement Disabled. Allowing Request.");
                }

            }
            else
            {
                //WindowsLogger.WriteWinLogEvent("Request is not targeting update password endpoint. Skipping");
            }
            return Task.FromResult<RiskScore>(RiskScore.NotEvaluated);
        }
    }
}