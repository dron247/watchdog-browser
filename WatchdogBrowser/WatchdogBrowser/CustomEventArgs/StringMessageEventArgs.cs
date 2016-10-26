using System;

namespace WatchdogBrowser.CustomEventArgs {
    public class StringMessageEventArgs :EventArgs {
        public string Message { get; set; }
    }
}
