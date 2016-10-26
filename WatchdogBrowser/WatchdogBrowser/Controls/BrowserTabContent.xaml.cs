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
            var bound = new MonitorJSBound();
            bound.StatusReport += Bound_StatusReport;
            bound.UpdateProgress += Bound_UpdateProgress;
            browser.RegisterJsObject("cobraMonitor", bound);

        }

        private void Bound_UpdateProgress(object sender, CustomEventArgs.StringMessageEventArgs e) {
            Debug.WriteLine(e.Message);
        }

        private void Bound_StatusReport(object sender, CustomEventArgs.StringMessageEventArgs e) {
            Debug.WriteLine(e.Message);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            //browser?.GetFocusedFrame().Dispose();
        }


    }
}
