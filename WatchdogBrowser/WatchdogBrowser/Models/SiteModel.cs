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
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Watched { get; set; }
        public string Message { get; set; }
        public string WarningSoundPath { get; set; }
        public string ErrorSoundPath { get; set; }

        public List<string> Mirrors { get; set; }

        public List<string> Whitelist { get; set; }
    }
}
