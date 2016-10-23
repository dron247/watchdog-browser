using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchdogBrowser.Models {
    public class SiteModel {
        public string Name { get; set; }
        public int UpdateInterval { get; set; }
        public int SwitchMirrorTimeout { get; set; }
        public int UpdateTimeout { get; set; }

        public List<string> Mirrors { get; set; }

        public List<string> Whitelist { get; set; }
    }
}
