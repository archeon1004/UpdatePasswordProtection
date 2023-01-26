using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace ThreatDetectionModule
{
    internal static class WindowsLogger
    {
        private static EventLog _winLog;
        internal static void CreateWinLog()
        {
            const string eventsource = "ADFSUpdatePasswordPlugin";
            const string eventLog = "Application";
            if(!EventLog.SourceExists(eventsource))
            {
                EventLog.CreateEventSource(source: eventsource, logName: eventLog);
            }
            try
            {
                _winLog = new EventLog(eventLog, ".", eventsource);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message,ex);
            }
        }
        internal static void WriteWinLogEvent(string message, EventLogEntryType entryType)
        {
            int EventID;
            switch (entryType)
            {
                case EventLogEntryType.Information:
                    EventID = 3000;
                    break;
                case EventLogEntryType.Error:
                    EventID = 3002;
                    break;
                case EventLogEntryType.Warning:
                    EventID = 3001;
                    break;
                default:
                    EventID = 3000;
                    break;
            }
            try
            {
                _winLog?.WriteEntry(message, entryType, EventID);
            }
            catch (Exception e)
            {
                throw new Exception("Event Log writing exception",e);
            }
            
        }
    }
}
