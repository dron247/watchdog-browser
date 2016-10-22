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

        public List<SiteMirrorModel> Mirrors { get; set; } = new List<SiteMirrorModel>(1);
    }
}
