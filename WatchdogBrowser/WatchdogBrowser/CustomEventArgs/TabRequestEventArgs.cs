using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchdogBrowser.CustomEventArgs {
    public class TabRequestEventArgs {
        public string URL { get; set; }
        public string Title { get; set; }
    }
}
