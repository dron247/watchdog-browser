using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchdogBrowser.Models;

namespace WatchdogBrowser.CustomEventArgs {
    public class ConfigReadyEventArgs : System.EventArgs {
        public ConfigReadyEventArgs(List<SiteModel> sites) { Sites = sites; }
        public List<SiteModel> Sites;
    }
}
