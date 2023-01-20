using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityServer.Public.ThreatDetectionFramework;
using Newtonsoft.Json;

namespace ThreatDetectionModule
{
    public class UpdatePasswordProtectionCustom : Microsoft.IdentityServer.Public.ThreatDetectionFramework.ThreatDetectionModule, IPreAuthenticationThreatDetectionModule, IPostAuthenticationThreatDetectionModule
    {
        public override string VendorName => "IGN";
        public override string ModuleIdentifier => "UpdatePasswordProtectionCustom";

        internal Config _config;

        public string InternalVersion => "0.0.1.3";

        private static readonly Logger fileLogger = new Logger(@"C:\dev\debug.log"); //TODO hardcoded string
        private readonly string EndpointName = "/adfs/portal/updatepassword";
        private Logger Logger = fileLogger;

        public override void OnAuthenticationPipelineLoad(ThreatDetectionLogger adfslogger, ThreatDetectionModuleConfiguration configData)
        {
            try
            {
                Logger.WriteLine($"Started Plugin {ModuleIdentifier} - {InternalVersion}");
                ReadConfigFile(adfslogger, configData);
            }
            catch (Exception ex)
            {
                adfslogger.WriteAdminLogErrorMessage(ex.ToString());
                Logger.WriteLine($"Could not start plugin - {ex}");
                throw;
            }
        }

        private void ReadConfigFile(ThreatDetectionLogger adfslogger, ThreatDetectionModuleConfiguration configData)
        {
            Logger.WriteLine("Reading JSON configuration");
            adfslogger.WriteDebugMessage("Reading JSON configuration");
            string jsonString;
            Config PluginConfig = new Config();
            try
            {
                using (StreamReader sr = new StreamReader(configData.ReadData()))
                {
                    StringBuilder sb = new StringBuilder();
                    string line = null;

                    while ((line = sr.ReadLine()) != null)
                    {
                        sb.Append(line);
                    }
                    jsonString = sb.ToString();
                    sb.Clear();
                }
                Logger.WriteLine($"config string content: {jsonString}");
                try
                {
                    PluginConfig = JsonConvert.DeserializeObject<Config>(jsonString);
                }
                catch (Exception e)
                {

                    Logger.WriteLine($"{e}");
                }
                Logger.WriteLine(JsonConvert.DeserializeObject(jsonString).ToString());
                Logger.WriteLine($"Configured Settings:{PluginConfig.ToString()}");
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Got Exception during reading of Config file. Will use default.Details:\n{ex}");
            }
            _config = PluginConfig;//TODO fix config
            _config = new Config();
            Logger.WriteLine($"Configured Settings:{_config.ToString()}");
        }

        public override void OnAuthenticationPipelineUnload(ThreatDetectionLogger adfslogger)
        {
            Logger.WriteLine("Closing Plugin");
        }

        public override void OnConfigurationUpdate(ThreatDetectionLogger adfslogger, ThreatDetectionModuleConfiguration configData)
        {
            adfslogger.WriteAdminLogErrorMessage("Updating Configuration");
            ReadConfigFile(adfslogger, configData);
        }

        public Task<ThrottleStatus> EvaluatePreAuthentication(ThreatDetectionLogger adfslogger, RequestContext requestContext, SecurityContext securityContext, ProtocolContext protocolContext, IList<Claim> additionalClams)
        {   
            if (requestContext.LocalEndPointAbsolutePath == EndpointName)
            {
                Logger.WriteLine("Entered PreAuthentication. Checking if plugin is enabled");
                if (_config.Enabled == true)
                {
                    string UserName = securityContext.UserIdentifier.Split('\\')[1];
                    Logger.WriteLine($"Checking update password request for a user: '{UserName}'");
                    if (!string.IsNullOrEmpty(UserName))
                    {
                        Logger.WriteLine("Evaluating UserPolicy for Update Passowrd");
                        try
                        {
                            var counterTest = SQLLiteHandlerClass.CheckCounter(UserName);
                            Logger.WriteLine($"Current Lockout Value:{counterTest}");
                            if (counterTest >= _config.RequestThreshold)
                            {
                                if(!(_config.RequestThreshold == 0))
                                {
                                    Logger.WriteLine($"Detected user hit the limit of {_config.RequestThreshold} for a user '{UserName}'. Will deny request");
                                    adfslogger.WriteAdminLogErrorMessage($"User '{UserName}' got blocked on the Update Password endpoint due to too many failed password update requests.");
                                    if (_config.BypassPasswordUpdateProtection == true)
                                    {
                                        Logger.WriteLine($"WARNING: BYPASS is enabled, request will be allowed. Normally this request would be rejected");
                                        return Task.FromResult<ThrottleStatus>(ThrottleStatus.Allow);
                                    }
                                    else
                                    {
                                        return Task.FromResult<ThrottleStatus>(ThrottleStatus.Block);
                                    }
                                }
                                else
                                {
                                    Logger.WriteLine($"Threshold set to 0. Will approve request");
                                }
                            }
                            else
                            {
                                Logger.WriteLine($"User '{UserName}' has not been detected as locked out. Will Allow request");
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.WriteLine($"Exception occured\n\r{e}");
                            adfslogger.WriteAdminLogErrorMessage($"User lockout database exception occurred.\n\rDetails:\n\r{e}");
                            return Task.FromResult<ThrottleStatus>(ThrottleStatus.Allow);
                        }
                    }
                    else
                    {
                        Logger.WriteLine($"Got empty user name for: '{securityContext.UserIdentifier}'");
                    }
                }
                else
                {
                    Logger.WriteLine("Plugin Enforcement Disabled. Allowing Request");
                }
            }
            else
            {
                Logger.WriteLine("Request is not targeting update password endpoint. Skipping");
            }
            return Task.FromResult<ThrottleStatus>(ThrottleStatus.Allow);
        }
        public Task<RiskScore> EvaluatePostAuthentication(ThreatDetectionLogger adfslogger, RequestContext requestContext, SecurityContext securityContext, ProtocolContext protocolContext, AuthenticationResult authenticationResult, IList<Claim> additionalClams)
        { 
            if (requestContext.LocalEndPointAbsolutePath == EndpointName)
            {
                Logger.WriteLine("Entered PostAuthentication. Checking if plugin is enabled");
                if (_config.Enabled == true)
                {
                    string UserName = securityContext.UserIdentifier.Split('\\')[1];
                    Logger.WriteLine($"Checking update password request results for a user: '{UserName}'");
                    if (authenticationResult == AuthenticationResult.Failure)
                    {
                        Logger.WriteLine($"User '{UserName}' Failed Authentication. Will Increase counter");
                        adfslogger.WriteAdminLogErrorMessage($"User '{UserName}' Failed Authentication. Will Increase counter");
                        try
                        {
                            SQLLiteHandlerClass.IncreaseCounter(UserName);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteLine($"Exception cought during Increasing Counter phase.\n\rDetails:\n\r{ex}");
                        }
                        if(_config.EnableRiskClaim == true)
                        {
                            return Task.FromResult<RiskScore>(RiskScore.Medium);
                        }
                    }
                    else
                    {
                        Logger.WriteLine("User Authentication succeeded. Counter won't be increased and update password will be allowed.");
                    }
                }
                else
                {
                    Logger.WriteLine("Plugin Enforcement Disabled. Allowing Request.");
                }

            }
            else
            {
                Logger.WriteLine("Request is not targeting update password endpoint. Skipping");
            }
            return Task.FromResult<RiskScore>(RiskScore.NotEvaluated);
        }
    }
}