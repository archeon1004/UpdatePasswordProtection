using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdatePasswordPluginModule
{
    internal class UserHelper
    {
        internal string Username;
        internal bool IsLockedOut;
        internal int FailedChangeCount;

        internal UserHelper() {
            Username = null;
            IsLockedOut = false;
            FailedChangeCount = -1 ;
        }

        internal UserHelper(string username,bool islocked, int count) {
            Username = username;
            IsLockedOut = islocked;
            FailedChangeCount = count;
        }
        internal void CheckIfLockedOut()
        {
            ConfigHelper config= new ConfigHelper();
            if(FailedChangeCount >= config.RequestThreshold)
            {
                IsLockedOut = true;
            }
            else 
            {
                IsLockedOut = false;
            }    
        }
    }
}
