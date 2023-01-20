using System;
using System.DirectoryServices;

namespace ThreatDetectionModule
{
    internal static class ADHelper //TODO currently not needed to decide if
    {
        public static string GetUPNoutOfSAMAccountNAme(string SamAccountName)
        {
            string ldapPath = "LDAP://dc=ad,dc=ignastech,dc=cloud";
            string username = SamAccountName.Split('\\')[1];
            try
            {
                using (DirectoryEntry root = new DirectoryEntry(ldapPath))
                {
                    DirectorySearcher searcher = new DirectorySearcher(root)
                    {
                        Filter = string.Format($"(&(objectClass=user)(samaccountname={username}))"),
                        SearchScope = SearchScope.Subtree
                    };
                    SearchResult result = searcher.FindOne();
                    if (result != null)
                    {
                        string val = result.GetDirectoryEntry().Properties["UserPrincipalName"].Value.ToString();
                        return val;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                System.Exception e = new Exception("LDAP lookup Error");
                throw e;
            }
        }
    }
}
