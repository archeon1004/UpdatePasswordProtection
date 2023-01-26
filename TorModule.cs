using System;
using System.Net;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ThreatDetectionModule;

internal static class TorModule
{
    private static HashSet<IPAddress> torExitNodes = new HashSet<IPAddress>();

    internal static void LoadTorExitNodes()
    {
        string url = "https://check.torproject.org/exit-addresses";
        using (WebClient client = new WebClient())
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
    }

    internal static bool IsTorExitNode(IPAddress address)
    {
        return torExitNodes.Contains(address);
    }
}