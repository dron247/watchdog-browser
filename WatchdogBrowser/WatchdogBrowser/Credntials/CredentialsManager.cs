using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchdogBrowser.Credntials {
    public class CredentialsManager {
        private static CredentialsManager instance;
        private CredentialsManager() { }
        public static CredentialsManager DefaultInstance {
            get {
                if(instance == null) {
                    instance = new CredentialsManager();
                }
                return instance;
            }
        }

        public string Username { get; set; }

        public string Password { get; set; }

    }
}
