using System;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace ThreatDetectionModule
{
    internal static class TorModule
    {
        private static HashSet<IPAddress> torExitNodes = new HashSet<IPAddress>();

        internal static void LoadTorExitNodes()
        {
            string url = "https://check.torproject.org/exit-addresses";
            using (WebClient client = new WebClient())
            {
                try
                {
                    string text = client.DownloadString(url);
                    using (StringReader reader = new StringReader(text))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.StartsWith("ExitAddress"))
                            {
                                string[] parts = line.Split(' ');
                                if (parts.Length >= 2)
                                {
                                    string address = parts[1];
                                    try
                                    {
                                        torExitNodes.Add(IPAddress.Parse(address));
                                    }
                                    catch (FormatException)
                                    {
                                        //
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }

        internal static bool IsTorExitNode(IPAddress address)
        {
            return torExitNodes.Contains(address);
        }
    }
}
    