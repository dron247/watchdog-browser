﻿using CefSharp;
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
                return Closeable ? Visibility.Visible : Visibility.Collapsed;
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
                    browser.FrameLoadStart += Browser_FrameLoadStart;//начало загрузки
                    browser.FrameLoadEnd += Browser_FrameLoadEnd;//конец загрузки

                    browser.MenuHandler = new WatchdogMenuHandler();
                    var lHandler = new WatchdogLifespanHandler();
                    lHandler.NewTabRequest += (s, e) => {
                        NewTabRequest?.Invoke(this, e);
                    };
                    lHandler.CloseTabRequest += (s, e) => {
                        var brwsr = (IBrowser)s;
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

        private void Browser_FrameLoadStart(object sender, FrameLoadStartEventArgs e) {
            //throw new NotImplementedException();
        }

        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e) {
            //throw new NotImplementedException();
        }

        private void Browser_LoadError(object sender, CefSharp.LoadErrorEventArgs e) {
            //MessageBox.Show($"Ошибка загрузки страницыю код: {e.ErrorCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //TODO: Логика умного апдейта
            var b = (IWpfWebBrowser)sender;
            b.Reload();
            
            Debug.WriteLine($"Перезагрузка после {e.ErrorCode}; РЕАЛИЗУЙ УМНУЮ ПЕРЕЗАГРУЗКУ", "LOAD LIFECYCLE");
        }









        #endregion
    }
}
