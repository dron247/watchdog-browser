using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchdogBrowser.CustomEventArgs;

namespace WatchdogBrowser.JSBoundObjects {
    public class MonitorJSBound {
        public MonitorJSBound(string username = "", string password = "") {
            Username = username;
            Password = password;
        }

        private string Username, Password;

        public string getUsername() {
            return Username;
        }

        public string getPassword() {
            return Password;
        }

        public void setStatus(string status) {
            //ok, alert
            StatusReport?.Invoke(this, new StringMessageEventArgs { Message = status });
        }

        public void reportUpdateProgress(string progress) {
            //started, completed, failed
            UpdateProgress?.Invoke(this, new StringMessageEventArgs { Message = progress });
        }

        public event EventHandler<StringMessageEventArgs> StatusReport;
        public event EventHandler<StringMessageEventArgs> UpdateProgress;
    }
}
