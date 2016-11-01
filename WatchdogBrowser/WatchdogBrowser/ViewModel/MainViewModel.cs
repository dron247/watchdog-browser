using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using WatchdogBrowser.Config;
using WatchdogBrowser.CustomEventArgs;
using WatchdogBrowser.Models;

namespace WatchdogBrowser.ViewModel {
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase {

        SitesConfig config = new SitesConfig();
        object locker = new object();

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel() {
            ////if (IsInDesignMode) {
            ////    // Code runs in Blend --> create design time data.
            ////} else {
            ////    // Code runs "for real"
            ////}            
            config.Ready += Config_Ready;
            try {
                config.Initialize();
            } catch (Exception e) {
                MessageBox.Show($"Ошибка чтения файла конфигурации {e.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region СВОЙСТВА ПРИВЯЗКИ

        private ObservableCollection<TabItemModel> tabs = new ObservableCollection<TabItemModel>();

        public ObservableCollection<TabItemModel> Tabs {
            get {
                return tabs;
            }
            set {
                lock (locker) {
                    tabs = value;
                }
                RaisePropertyChanged(nameof(Tabs));
            }
        }

        private TabItemModel selectedTab;
        public TabItemModel SelectedTab {
            get {
                return selectedTab;
            }
            set {
                Set<TabItemModel>(nameof(this.SelectedTab), ref selectedTab, value);
                lock (locker) {
                    foreach (var tab in tabs) {
                        tab.ZIndex = 0;
                    }
                }
                try {
                    selectedTab.ZIndex = 1000;
                } catch { }
            }
        }


        #endregion


        #region СОБЫТИЯ

        private void Config_Ready(object sender, CustomEventArgs.ConfigReadyEventArgs e) {
            var sitesList = e.Sites;

            var prepTabs = new List<TabItemModel>();
            foreach (var site in sitesList) {
                Credntials.CredentialsManager.DefaultInstance.Username = site.Username;
                Credntials.CredentialsManager.DefaultInstance.Password = site.Password;

                var prepTab = new TabItemModel();
                prepTab.Title = site.Name;
                prepTab.Watched = site.Watched;
                prepTab.Url = site.Mirrors[0];
                prepTab.Mirrors = site.Mirrors;
                prepTab.Closeable = !site.Watched;                
                prepTab.ErrorMessage = site.Message;
                prepTab.WarningSoundPath = site.WarningSoundPath;
                prepTab.HeartbeatTimeout = site.UpdateInterval;
                prepTab.PageLoadTimeout = site.UpdateTimeout;
                prepTab.SwitchMirrorTimeout = site.SwitchMirrorTimeout;
                prepTab.Close += TabClosed;
                prepTab.NewTabRequest += Tab_NewTabRequest;
                lock (locker) {
                    Tabs.Add(prepTab);
                }
            }
            RaisePropertyChanged(nameof(Tabs));
            SelectedTab = Tabs[0];
        }

        

        private void Tab_NewTabRequest(object sender, CustomEventArgs.TabRequestEventArgs e) {
            Application.Current.Dispatcher.Invoke(() => {
                var tab = new TabItemModel {
                    Title = "",
                    Watched = false,
                    Url = e.URL,
                    Closeable = true                    
                };
                tab.Close += TabClosed;
                lock (locker) {
                    Tabs.Add(tab);
                }
                RaisePropertyChanged(nameof(Tabs));
                SelectedTab = tab;
            });
        }
        

        private void TabClosed(object sender, StringMessageEventArgs e) {
            //find tab
            if (e.Message != string.Empty) {
                TabItemModel tab = null;
                lock (locker) {
                    foreach(var t in Tabs) {
                        if (t.Url.Contains(e.Message)) {
                            tab = t;
                            break;
                        }
                    }
                }
                if (tab != null) {
                    CloseTab(tab);
                }
            } else {
                var prey = (TabItemModel)sender;
                CloseTab(prey);
            }
        }

        #endregion

        #region МЕТОДЫ

        /// <summary>
        /// Метод закрытия указаной вкладки
        /// </summary>
        /// <param name="prey">Модель вкладки под закрытие</param>
        private void CloseTab(TabItemModel prey) {
            if (Tabs.Count == 1) {
                App.Current.Shutdown();
            } else {
                lock (locker) {
                    Tabs.Remove(prey);
                }
                RaisePropertyChanged(nameof(Tabs));
                prey?.DisposeTab();
            }
        }

        #endregion
    }
}