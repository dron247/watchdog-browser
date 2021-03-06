﻿using CefSharp;
using CefSharp.Wpf;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WatchdogBrowser.CustomEventArgs;
using WatchdogBrowser.Handlers;
using WatchdogBrowser.JSBoundObjects;
using WatchdogBrowser.Workers;

namespace WatchdogBrowser.ViewModel {
    public class TabItemViewModel : ObservableObject {
        public TabItemViewModel() {
            CloseTabCommand = new RelayCommand(CloseMethod);
            ShowDevtoolsCommand = new RelayCommand(ShowDevtoolsMethod);
            SwitchMirrorCommand = new RelayCommand(SwitchMirrorMethod);
        }

        static readonly SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
        static readonly SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        static readonly SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);


        public event EventHandler<StringMessageEventArgs> Close;
        public event EventHandler<TabRequestEventArgs> NewTabRequest;
        public event EventHandler<StringMessageEventArgs> UrlChanged;
        //public event EventHandler CloseTabRequest;

        Watchdog watchdog = new Watchdog();
        bool pageLoadFinished = false;
        readonly object locker = new object();


        string title = "Без имени";
        string url = "#";
        bool closeable = false;
        int alertStatus = -1;
        int activeMirror = 0;

        /// <summary>
        /// Заголовок вкладки
        /// </summary>
        public string Title {
            get {
                return title;
            }
            set {
                Set<string>(nameof(this.Title), ref title, value);
            }
        }

        int zindex = 0;
        /// <summary>
        /// Нужно для реализации своих вкладок
        /// </summary>
        public int ZIndex {
            get {
                return zindex;
            }
            set {
                Set<int>(nameof(this.ZIndex), ref zindex, value);
            }
        }

        /// <summary>
        /// Текущий адрес вкладки
        /// </summary>
        public string Url {
            get {
                return url;
            }
            set {
                Set<string>(nameof(this.Url), ref url, value);
                if (Watched) StartPageLoadTimer();
            }
        }

        public string CurrentUrl {
            get {
                return browser?.Address ?? string.Empty;
            }
        }



        /// <summary>
        /// Список зеркал сайта
        /// </summary>
        public List<string> Mirrors { get; set; }

        /// <summary>
        /// Можно ли закрыть вкладку
        /// </summary>
        public bool Closeable {
            get {
                return closeable;
            }
            set {
                Set<bool>(nameof(this.Closeable), ref closeable, value);
            }
        }

        /// <summary>
        /// Наблюдается ли вкладка системой мёртвой руки
        /// </summary>
        public bool Watched {
            get; set;
        }


        /// <summary>
        /// Видно ли кнопку закрытия вкладки, только для интерфейса
        /// </summary>
        public Visibility CloseButtonVisibility {
            get {
                return Closeable ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        /// <summary>
        /// Цвет индикатора статуса, только дял интерфейса
        /// </summary>
        public SolidColorBrush AlertColor {
            get {
                if (alertStatus < 0) {
                    return blackBrush;
                }
                return alertStatus == 0 ? greenBrush : redBrush;
            }
        }

        public string ErrorTime {
            get; private set;
        } = string.Empty;


        public Visibility SwitchMirrorVisibility {
            get {
                return Watched ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        private bool reloadingMessageVisible = false;
        /// <summary>
        /// Открыто ли сообщение о загрузке страницы
        /// </summary>
        public bool ReloadingMessageVisible {
            get {
                return reloadingMessageVisible;
            }
            set {
                Set<bool>(nameof(ReloadingMessageVisible), ref reloadingMessageVisible, value);
                RaisePropertyChanged(nameof(ReloadingMessageVisibility));
            }
        }
        /// <summary>
        /// Открыто ли сообщение о загрузке страницы, только для интерфейса
        /// </summary>
        public Visibility ReloadingMessageVisibility {
            get {
                return ReloadingMessageVisible ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private bool loadErrorVisible = false;
        /// <summary>
        /// Видно ли сообщение об ошибке загрузки
        /// </summary>
        public bool LoadErrorVisible {
            get {
                return loadErrorVisible;
            }
            set {
                loadErrorVisible = value;
                RaisePropertyChanged(nameof(LoadErrorVisible));
                FillErrorMessage();
            }
        }



        /// <summary>
        /// Видно ли сообщение об ошибке загрузки, только для интерфейса
        /// </summary>
        public Visibility LoadErrorVisibility {
            get { return LoadErrorVisible ? Visibility.Visible : Visibility.Hidden; }
        }


        /// <summary>
        /// Интервал, в котором ожидется heartbeat
        /// </summary>
        public int HeartbeatTimeout { get; set; }


        /// <summary>
        /// Таймаут heartbeat, необходима перезагрузка страницы
        /// </summary>
        public int PageLoadTimeout { get; set; }


        /// <summary>
        /// Таймаут heartbeat по зеркалу, необходима смена зеркала
        /// </summary>
        public int SwitchMirrorTimeout { get; set; }

        /// <summary>
        /// Время задержки звукового сигналя после появления сообщения об ошибке
        /// </summary>
        public int AlertDelayTime { get; set; } = 0;

        /// <summary>
        /// Дополнительное сообщение от администратора, которое будет выведено в окне ошибки
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Путь к звуковому файлу для предупреждения
        /// </summary>
        public string WarningSoundPath { get; set; } = string.Empty;


        /// <summary>
        /// Путь к звуковому файлу для ошибки
        /// </summary>
        public string ErrorSoundPath { get; set; } = string.Empty;


        #region COMMANDS
        public ICommand SwitchMirrorCommand { get; private set; }

        private void SwitchMirrorMethod() {
            SwitchMirror();
        }



        public ICommand CloseTabCommand { get; private set; }

        /// <summary>
        /// Вызывает закрытие текущей вкладки с проверкой адреса
        /// </summary>
        /// <param name="url">адрес для проверки</param>
        void CloseMethod(string url) {
            Close?.Invoke(this, new StringMessageEventArgs { Message = url });
        }

        /// <summary>
        /// Вызывает закрытие текущей вкладки, случай с нажатием кнопки
        /// </summary>
        void CloseMethod() {
            Close?.Invoke(this, new StringMessageEventArgs { Message = string.Empty });
        }


        public ICommand ShowDevtoolsCommand { get; private set; }
        /// <summary>
        /// Открывает инструменты разработчика
        /// </summary>
        void ShowDevtoolsMethod() {
            browser?.ShowDevTools();
        }

        #endregion

        #region BROWSER

        private MonitorJSBound jsBinding = null;
        /// <summary>
        /// Свойство для привязки js примеси браузера, задаётся в BrowserTabContent.xaml.cs
        /// </summary>
        public MonitorJSBound JsBinding {
            set {
                jsBinding = value;
                if (Watched) {
                    jsBinding.Heartbeat += JsBinding_Heartbeat;
                    jsBinding.AlarmStateUpdated += JsBinding_AlarmStateUpdated;
                }
                jsBinding.CloseTab += JsBinding_CloseTab;
            }
            get {
                return jsBinding;
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
            //Debug.WriteLine("HEARTBEAT");
            lock (locker) {
                pageLoadFinished = true;
            }

            PokeWatchdog();
            watchdog?.DoHeartbeat();

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
        /// <summary>
        /// Свойство для привязки компонента веб-браузера к модели, привязка в BrowserTabContent.xaml
        /// </summary>
        public IWpfWebBrowser WebBrowser {
            get { return browser; }
            set {
                browser = value;
                if (browser != null) {

                    browser.LoadError += Browser_LoadError;
                    //browser.FrameLoadStart += Browser_FrameLoadStart;//начало загрузки
                    browser.FrameLoadEnd += Browser_FrameLoadEnd;//конец загрузки
                    //browser.LoadingStateChanged += Browser_LoadingStateChanged;

                    browser.MenuHandler = new WatchdogMenuHandler();
                    var lHandler = new WatchdogLifespanHandler();
                    lHandler.NewTabRequest += (s, e) => {
                        NewTabRequest?.Invoke(this, e);
                    };
                    browser.LifeSpanHandler = lHandler;
                }
            }
        }



        //Обработчик события конца загрузки документа
        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e) {
            var url = e.Url;
            if (url != "data:text/html,chromewebdata") {
                lock (locker) {
                    pageLoadFinished = true;
                }
            }
            try {
                Application.Current.Dispatcher.Invoke(() => {
                    if (ReloadingMessageVisible) {
                        ReloadingMessageVisible = false;
                    }
                    if (url.ToLower().Contains("login")) {
                        LoadErrorVisible = false;
                    }
                    if (pageLoadFinished) {
                        PokeWatchdog();
                    }
                });
            } catch { }
            RaisePropertyChanged(nameof(CurrentUrl));
            RaisePropertyChanged("SelectedTab.CurrentUrl");
            UrlChanged?.Invoke(this, new StringMessageEventArgs { Message = Url });
        }

        //Обработчик события ошибки загрузки документа
        private void Browser_LoadError(object sender, CefSharp.LoadErrorEventArgs e) {
            //MessageBox.Show($"Ошибка загрузки страницыю код: {e.ErrorCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            if (e.ErrorCode == CefErrorCode.Aborted) return;
            try {
                Application.Current.Dispatcher.Invoke(() => {
                    HandleLoadError((IWpfWebBrowser)sender);
                });
            } catch { }

            // Debug.WriteLine($"Перезагрузка после {e.ErrorCode}; РЕАЛИЗУЙ УМНУЮ ПЕРЕЗАГРУЗКУ", "LOAD LIFECYCLE");
        }

        //Обработчик события "требуется перезагрузка страницы" от следящего таймера
        private void Watchdog_NeedReload(object sender, EventArgs e) {
            Application.Current.Dispatcher.Invoke(() => {
                //Debug.WriteLine("Need reload");
                try {
                    if (browser.Address.ToLower().Contains("login")) {
                        PlayErrorSound();
                    } else {
                        LoadErrorVisible = true;//показываем страшное сообщение об ошибке
                    }
                } catch {
                    LoadErrorVisible = true;//показываем страшное сообщение об ошибке
                }
                ReloadBrowser();
            });
        }


        //Обработчик события "требуется смена зеркала" от следящего таймера
        private void Watchdog_NeedChangeMirror(object sender, EventArgs e) {
            Application.Current.Dispatcher.Invoke(() => {
                //Debug.WriteLine("Need mirror change");
                SwitchMirror();
            });
        }













        #endregion

        /// <summary>
        /// Метод обработки ошибки страницы
        /// </summary>
        /// <param name="browser">ссылка на компонент браузера</param>
        private void HandleLoadError(IWpfWebBrowser browser) {
            var tsk = Task.Run(async () => {
                LoadErrorVisible = true;//показываем страшное сообщение об ошибке              
                ReloadBrowser();
                await Task.Delay(3000);//задержка перезагрузки, чтобы снизить нагрузку на систему, реально можно BSOD поймать
            });
        }

        bool firstbeat = true;
        private void PokeWatchdog() {
            if (Watched && firstbeat) {
                watchdog.HeartbeatTimeout = HeartbeatTimeout;
                watchdog.SwitchMirrorTimeout = SwitchMirrorTimeout;
                watchdog.NeedChangeMirror += Watchdog_NeedChangeMirror;
                watchdog.NeedReload += Watchdog_NeedReload;
                watchdog.StartWatch();
                firstbeat = false;
            }
        }


        /// <summary>
        /// Метод запуска таймера для отслеживания настраиваемого таймаута страницы
        /// </summary>
        private void StartPageLoadTimer() {
            lock (locker) {
                if (Watched && !pageLoadTimerActive) {
                    pageLoadTimerActive = true;
                    pageLoadFinished = false;
                    var timer = new Timer();
                    timer.Interval = PageLoadTimeout > 0 ? PageLoadTimeout * 1000 : 20000;
                    timer.AutoReset = false;
                    timer.Elapsed += PageLoadTimer_Elapsed;
                    timer.Start();
                }
            }
        }

        bool pageLoadTimerActive = false;
        private void PageLoadTimer_Elapsed(object sender, ElapsedEventArgs e) {
            bool switchMirror = false;
            lock (locker) {
                switchMirror = !pageLoadFinished;
                pageLoadTimerActive = false;
            }
            if (switchMirror) {
                Application.Current.Dispatcher.Invoke(() => {
                    SwitchMirror();
                });
            }
            (sender as Timer).Stop();
            (sender as Timer).Dispose();

        }

        private void ReloadBrowser() {
            //LoadErrorVisible = true;//показываем страшное сообщение об ошибке
            browser?.Reload();//перезагружаем страницу

        }

        private void SwitchMirror() {
            if (Mirrors.Count == 0) return;
            activeMirror++;
            if (activeMirror > Mirrors.Count - 1) {
                activeMirror = 0;
            }
            Url = Mirrors[activeMirror];
            PlayWarningSound();
            UrlChanged?.Invoke(this, new StringMessageEventArgs { Message = Url });
        }


        bool isErrorShown = false;
        private void FillErrorMessage() {
            if (loadErrorVisible) {
                var t = DateTime.Now;
                ErrorTime = ErrorTime == string.Empty ? t.ToString("HH:mm:ss") : ErrorTime;
                if (!isErrorShown) {
                    PlayErrorSound();
                }
                isErrorShown = true;
            } else {
                ErrorTime = string.Empty;
                isErrorShown = false;
            }
            RaisePropertyChanged(nameof(ErrorTime));
            RaisePropertyChanged(nameof(LoadErrorVisibility));
        }

        bool isAlertInSchedule = false;
        private void PlayErrorSound() {
            if (isAlertInSchedule) return;
            var tsk = Task.Run(async () => {
                isAlertInSchedule = true;
                await Task.Delay(AlertDelayTime * 1000);//задержка воспроизведения звука ошибки
                if (LoadErrorVisible) {
                    if (ErrorSoundPath != string.Empty) {
                        var player = new System.Media.SoundPlayer(ErrorSoundPath);
                        player.Play();
                        isAlertInSchedule = false;
                        player.Dispose();
                    }
                }
            });
        }


        private void PlayWarningSound() {
            var tsk = Task.Run(async () => {
                if (WarningSoundPath != string.Empty) {
                    var player = new System.Media.SoundPlayer(WarningSoundPath);
                    player.Play();
                    player.Dispose();
                }
                await Task.Delay(3000);//задержка повтора
            });
        }

        public void DisposeTab() {
            jsBinding.Heartbeat -= JsBinding_Heartbeat;
            jsBinding.CloseTab -= JsBinding_CloseTab;
            jsBinding.AlarmStateUpdated -= JsBinding_AlarmStateUpdated;
            ZIndex = 0;
            var tsk = Task.Run(async () => {
                await Task.Delay(3000);//задержка уничтожения, чтобы не подвисало при закрытии вкладки
                WebBrowser?.Dispose();
            });
        }

    }//end class
}//end namespace
