using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchdogBrowser.Models;

namespace WatchdogBrowser.Config {
    public class Config {
        
        static Config instance = null;
        public static Config DefaultInstance {
            get {
                if (instance == null) {
                    instance = new Config();
                }
                return instance;
            }
        }

        public event EventHandler Ready;

        bool configIsReady = false;
        string configPath;

        private Config() {
            configPath = Properties.Settings.Default.ConfigFilename;
            Initialize();
        }


        public bool ConfigIsReady {
            get { return configIsReady; }
        }


        async void Initialize() {
            var configText = await ReadConfig();
            configIsReady = true;
            ParseConfig(configText);
            Ready?.Invoke(this, EventArgs.Empty);
        }

        async Task<string> ReadConfig() {
            if (!File.Exists(configPath)) {
                return string.Empty;
            }
            using(var reader = File.OpenText(configPath)) {
                return await reader.ReadToEndAsync();
            }
        }


        void ParseConfig(string xmlString) {
            //
        }


        List<SiteModel> sites = new List<SiteModel>();
    }

}
/*
 * Пример xml
 * <?xml version="1.1" encoding="UTF-8" ?>
 * <sites>
 *  <site name="Сторожевая собака" updateOk="10" updateFail="60" updateTimeout="20" uri=">
 *      <mirrors>
 *          <mirror domain="ex1.example.com"/>
 *          <mirror domain="ex2.example.com"/>
 *      </mirrors>
 *      <tabs>
 *          <tab uri="index.php?r=site%2Findex"/>
 *          <tab uri="index.php"/>
 *      </tabs>
 *  </site>
 * </sites>
 */
