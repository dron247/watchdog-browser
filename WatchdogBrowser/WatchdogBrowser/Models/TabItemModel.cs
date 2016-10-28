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
using System.Windows.Media;
using WatchdogBrowser.CustomEventArgs;
using WatchdogBrowser.Handlers;
using WatchdogBrowser.JSBoundObjects;

namespace WatchdogBrowser.Models {
    public class TabItemModel : ObservableObject {
        public TabItemModel() {
            CloseTabCommand = new RelayCommand(CloseMethod);
            ShowDevtoolsCommand = new RelayCommand(ShowDevtoolsMethod);
        }


        public event EventHandler<StringMessageEventArgs> Close;
        public event EventHandler<TabRequestEventArgs> NewTabRequest;
        public event EventHandler CloseTabRequest;

        string title = "Без имени";
        string url = "#";
        bool closeable = false;
        int alertStatus = -1;

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

        static readonly SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
        static readonly SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        static readonly SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);
        public SolidColorBrush AlertColor {
            get {
                if (alertStatus < 0) {
                    return blackBrush;
                }
                return alertStatus == 0 ? greenBrush : redBrush;
            }
        }

        

        public ICommand CloseTabCommand { get; private set; }

        /// <summary>
        /// Вызывает закрытие текущей вкладки
        /// </summary>
        void CloseMethod(string url) {
            Close?.Invoke(this, new StringMessageEventArgs { Message = url });
        }

        void CloseMethod() {
            Close?.Invoke(this, new StringMessageEventArgs { Message = string.Empty });
        }


        public ICommand ShowDevtoolsCommand { get; private set; }

        void ShowDevtoolsMethod() {
            browser?.ShowDevTools();
        }

        private bool reloadingMessageVisible = false;

        public bool ReloadingMessageVisible {
            get {
                return reloadingMessageVisible;
            }
            set {
                Set<bool>(nameof(ReloadingMessageVisible), ref reloadingMessageVisible, value);
                RaisePropertyChanged(nameof(ReloadingMessageVisibility));
            }
        }

        public Visibility ReloadingMessageVisibility {
            get {
                return ReloadingMessageVisible ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private bool loadErrorVisible = false;

        public bool LoadErrorVisible {
            get {
                return loadErrorVisible;
            }
            set {
                Set<bool>(nameof(LoadErrorVisible), ref loadErrorVisible, value);
                RaisePropertyChanged(nameof(LoadErrorVisibility));
            }
        }

        public Visibility LoadErrorVisibility {
            get { return LoadErrorVisible ? Visibility.Visible : Visibility.Hidden; }
        }



        #region BROWSER

        private MonitorJSBound jsBinding = null;
        public MonitorJSBound JsBinding {
            set {
                jsBinding = value;
                jsBinding.Heartbeat += JsBinding_Heartbeat;
                jsBinding.CloseTab += JsBinding_CloseTab;
                jsBinding.AlarmStateUpdated += JsBinding_AlarmStateUpdated;
            }
        }


        //Обработчик сообщения о состоянии тревоги на объекте
        private void JsBinding_AlarmStateUpdated(object sender, StringMessageEventArgs e) {
            alertStatus = e.Message == "0" ? 0 : 1;
            RaisePropertyChanged(nameof(AlertColor));
        }

        //Обработчик сообщения со страницы о необходимости закрыть текущую вкладку
        private void JsBinding_CloseTab(object sender, StringMessageEventArgs e) {
            try {
                Application.Current.Dispatcher.Invoke(() => {
                    CloseMethod(e.Message);
                });
            } catch { }
        }

        //Обработчик сообщения о том, что страница жива, если сообщения нет, то нужно принять меры
        private void JsBinding_Heartbeat(object sender, EventArgs e) {

            try {
                Application.Current.Dispatcher.Invoke(() => {
                    //Debug.WriteLine($"HEARTBEAT");
                    if (LoadErrorVisible) {
                        LoadErrorVisible = false;
                    }
                });
            } catch { }
        }

        IWpfWebBrowser browser = null;
        public IWpfWebBrowser WebBrowser {
            get { return browser; }
            set {
                browser = value;
                if (browser != null) {

                    browser.LoadError += Browser_LoadError;
                    browser.FrameLoadStart += Browser_FrameLoadStart;//начало загрузки
                    browser.FrameLoadEnd += Browser_FrameLoadEnd;//конец загрузки
                    browser.LoadingStateChanged += Browser_LoadingStateChanged;



                    browser.MenuHandler = new WatchdogMenuHandler();
                    var lHandler = new WatchdogLifespanHandler();
                    lHandler.NewTabRequest += (s, e) => {
                        NewTabRequest?.Invoke(this, e);
                    };
                    lHandler.CloseTabRequest += (s, e) => {
                        //var brwsr = (IBrowser)s;
                        //try {
                        //    //Так как обработка идёт из потока, плюс ко всему от биндинга неродного контрола, tst нужны чтобы вызвать exception
                        //    //он вызывается если был открыт попап типа девтулзов, если всё норм, значит вкладка
                        //    //да, костыль, но не критичный
                        //    if (!brwsr.IsPopup) {
                        //        CloseTabRequest?.Invoke(this, e);
                        //    }
                        //} catch {
                        //    //MessageBox.Show("Disposed");
                        //}

                    };
                    browser.LifeSpanHandler = lHandler;
                }
            }
        }

        //Обработчик события состояния загрузки документа, не используется, нет смысла
        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e) {
            //
        }

        //Обработчик события начала загрузки, не используется из-за бага в CEF
        private void Browser_FrameLoadStart(object sender, FrameLoadStartEventArgs e) {
            //
        }

        //Обработчик события конца загрузки документа
        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e) {
            //try {
            //    Application.Current.Dispatcher.Invoke(() => {
            //        if (ReloadingMessageVisible) {
            //            ReloadingMessageVisible = false;
            //        }
            //    });
            //} catch { }
        }

        //Обработчик события ошибки загрузки документа
        private void Browser_LoadError(object sender, CefSharp.LoadErrorEventArgs e) {
            //MessageBox.Show($"Ошибка загрузки страницыю код: {e.ErrorCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //TODO: Логика умного апдейта

            try {
                Application.Current.Dispatcher.Invoke(() => {
                    HandleLoadError((IWpfWebBrowser)sender);
                });
            } catch { }

            Debug.WriteLine($"Перезагрузка после {e.ErrorCode}; РЕАЛИЗУЙ УМНУЮ ПЕРЕЗАГРУЗКУ", "LOAD LIFECYCLE");
        }

        /// <summary>
        /// Метод обработки ошибки страницы
        /// </summary>
        /// <param name="browser">ссылка на компонент браузера</param>
        private void HandleLoadError(IWpfWebBrowser browser) {
            var tsk = Task.Run(async () => {
                await Task.Delay(3000);//задержка перезагрузки, чтобы снизить нагрузку на систему, реально можно BSOD поймать
                LoadErrorVisible = true;//показываем страшное сообщение об ошибке
                browser.Reload();//перезагружаем страницу
            });
        }









        #endregion

        public void DisposeTab() {
            WebBrowser?.Dispose();
        }
    }
}
