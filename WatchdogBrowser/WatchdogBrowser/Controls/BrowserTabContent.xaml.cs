using CefSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WatchdogBrowser.JSBoundObjects;
using WatchdogBrowser.Models;

namespace WatchdogBrowser.Controls {


    /// <summary>
    /// Interaction logic for BrowserTabContent.xaml
    /// </summary>
    public partial class BrowserTabContent : UserControl {
        MonitorJSBound bound = null;
        public BrowserTabContent() {
            InitializeComponent();
            var crd = Credntials.CredentialsManager.DefaultInstance;
            bound = new MonitorJSBound(crd.Username, crd.Password);            
            browser.RegisterJsObject("cobraMonitor", bound);
        }

        

        

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            var model = (TabItemModel) DataContext;
            model.JsBinding = bound;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            //browser?.GetFocusedFrame().Dispose();
        }
    }
}
