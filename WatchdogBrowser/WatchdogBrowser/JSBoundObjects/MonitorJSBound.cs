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

        public void heartbeat() {
            //ok, alert
            Heartbeat?.Invoke(this, EventArgs.Empty);
        }

        public void closeTab(string url) {
            CloseTab?.Invoke(this, new StringMessageEventArgs { Message = url });
        }

        public void setAlarm(string code) {
            AlarmStateUpdated?.Invoke(this, new StringMessageEventArgs { Message = code });
        }



        public event EventHandler Heartbeat;
        public event EventHandler<StringMessageEventArgs> CloseTab;
        public event EventHandler<StringMessageEventArgs> AlarmStateUpdated;
    }
}
