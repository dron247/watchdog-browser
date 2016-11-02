using System.Windows;
using System.Windows.Controls;
using WatchdogBrowser.JSBoundObjects;
using WatchdogBrowser.ViewModel;

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
            var model = (TabItemViewModel) DataContext;
            model.JsBinding = bound;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            //browser?.GetFocusedFrame().Dispose();
        }
    }
}
