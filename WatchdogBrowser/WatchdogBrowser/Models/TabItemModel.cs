using CefSharp;
using CefSharp.Wpf;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WatchdogBrowser.CustomEventArgs;

namespace WatchdogBrowser.Models {
    public class TabItemModel : ObservableObject {
        public TabItemModel() {
            CloseTabCommand = new RelayCommand(CloseMethod);
        }


        public event EventHandler Close;
        public event EventHandler<TabRequestEventArgs> NewTabRequest;

        string title = "Без имени";
        string url = "#";
        bool closeable = false;

        public string Title {
            get {
                return title;
            }
            set {
                Set<string>(nameof(this.Title), ref title, value);
            }
        }

        public string Url {
            get {
                return url;
            }
            set {
                Set<string>(nameof(this.Url), ref url, value);
            }
        }

        public bool Closeable {
            get {
                return closeable;
            }
            set {
                Set<bool>(nameof(this.Closeable), ref closeable, value);
            }
        }

        public Visibility CloseButtonVisibility {
            get {
                return Closeable ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public ICommand CloseTabCommand { get; private set; }

        void CloseMethod() {
            Close?.Invoke(this, EventArgs.Empty);
        }

        #region BROWSER
        IWpfWebBrowser browser = null;
        public IWpfWebBrowser WebBrowser {
            set {
                browser = value;
                if (browser != null) {
                    browser.LoadError += Browser_LoadError;
                    var lHandler =  new CustomLifespanHandler();
                    lHandler.NewTabRequest += (s, e) => {
                        NewTabRequest?.Invoke(this, e);
                    };
                    browser.LifeSpanHandler = lHandler;
                }
            }
        }

        private void Browser_LoadError(object sender, CefSharp.LoadErrorEventArgs e) {
            MessageBox.Show($"Ошибка загрузки страницыю код: {e.ErrorCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }



        private class CustomLifespanHandler : ILifeSpanHandler {

            public event EventHandler<TabRequestEventArgs> NewTabRequest;

            public bool DoClose(IWebBrowser browserControl, IBrowser browser) {
                return false;
            }

            public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser) {
            }

            public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser) {
            }

            public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser) {
                //return true;
                //return parent.OnBeforePopup(browserControl, browser, frame, targetUrl, targetFrameName, targetDisposition, userGesture, popupFeatures, windowInfo, browserSettings, ref noJavascriptAccess, out newBrowser);
                newBrowser = null;
                NewTabRequest?.Invoke(this, new TabRequestEventArgs { URL = targetUrl });
                return true;
            }
        }



        #endregion
    }
}
