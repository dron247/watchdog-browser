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
using WatchdogBrowser.Handlers;

namespace WatchdogBrowser.Models {
    public class TabItemModel : ObservableObject {
        public TabItemModel() {
            CloseTabCommand = new RelayCommand(CloseMethod);
            ShowDevtoolsCommand = new RelayCommand(ShowDevtoolsMethod);
        }


        public event EventHandler Close;
        public event EventHandler<TabRequestEventArgs> NewTabRequest;
        public event EventHandler CloseTabRequest;

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


        public ICommand ShowDevtoolsCommand { get; private set; }

        void ShowDevtoolsMethod() {
            browser?.ShowDevTools();
        }



        #region BROWSER
        IWpfWebBrowser browser = null;
        public IWpfWebBrowser WebBrowser {
            get { return browser; }
            set {
                browser = value;
                if (browser != null) {
                    browser.LoadError += Browser_LoadError;
                    browser.MenuHandler = new WatchdogMenuHandler();
                    var lHandler = new WatchdogLifespanHandler();
                    lHandler.NewTabRequest += (s, e) => {
                        NewTabRequest?.Invoke(this, e);
                    };
                    lHandler.CloseTabRequest += (s, e) => {
                        var brwsr = (IBrowser)s;
                        //Debug.WriteLine("-----------CLOSE request--------");

                        //Debug.WriteLine(brwsr.IsPopup);

                        //Debug.WriteLine("-----------CLOSE request--------");
                        try {
                            //Так как обработка идёт из потока, плюс ко всему от биндинга неродного контрола, tst нужны чтобы вызвать exception
                            //он вызывается если был открыт попап типа девтулзов, если всё норм, значит вкладка
                            //да, костыль, но не критичный
                            var tst = brwsr.IsPopup;
                            var tst2 = brwsr.HasDocument;
                            CloseTabRequest?.Invoke(this, e);
                        } catch {
                            //MessageBox.Show("Disposed");
                        }

                        
                    };
                    browser.LifeSpanHandler = lHandler;
                }
            }
        }

        private void Browser_LoadError(object sender, CefSharp.LoadErrorEventArgs e) {
            MessageBox.Show($"Ошибка загрузки страницыю код: {e.ErrorCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }









        #endregion
    }
}
