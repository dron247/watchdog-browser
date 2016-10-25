using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using WatchdogBrowser.Config;
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
                MessageBox.Show($"������ ������ ����� ������������ {e.Message}", "������", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region �������� ��������

        private ObservableCollection<TabItemModel> tabs = new ObservableCollection<TabItemModel>();

        public ObservableCollection<TabItemModel> Tabs {
            get {
                return tabs;
            }
            set {
                tabs = value;
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

            }
        }


        #endregion


        #region �������

        private void Config_Ready(object sender, CustomEventArgs.ConfigReadyEventArgs e) {
            var sitesList = e.Sites;

            var prepTabs = new List<TabItemModel>();
            foreach (var site in sitesList) {
                var prepTab = new TabItemModel();
                prepTab.Title = site.Name;
                prepTab.Url = site.Mirrors[0];
                prepTab.Closeable = false;
                prepTab.Close += TabClosed;
                prepTab.CloseTabRequest += PrepTab_SelfCloseRequest;
                prepTab.NewTabRequest += Tab_NewTabRequest;
                Tabs.Add(prepTab);
            }
            RaisePropertyChanged(nameof(Tabs));
        }

        private void PrepTab_SelfCloseRequest(object sender, EventArgs e) {
            Application.Current.Dispatcher.Invoke(() => {
                CloseTab(SelectedTab);
            });
        }

        private void Tab_NewTabRequest(object sender, CustomEventArgs.TabRequestEventArgs e) {
            Application.Current.Dispatcher.Invoke(() => {
                var tab = new TabItemModel { Title = "������� ����", Url = e.URL, Closeable = true };
                tab.Close += TabClosed;
                Tabs.Add(tab);
                SelectedTab = tab;
                RaisePropertyChanged(nameof(Tabs));
            });
        }

        private void TabClosed(object sender, System.EventArgs e) {
            CloseTab((TabItemModel)sender);
        }

        #endregion

        #region ������

        /// <summary>
        /// ����� �������� �������� �������
        /// </summary>
        /// <param name="prey">������ ������� ��� ��������</param>
        private void CloseTab(TabItemModel prey) {
            if (Tabs.Count == 1) {
                App.Current.Shutdown();
            } else {
                Tabs.Remove(prey);
                RaisePropertyChanged(nameof(Tabs));

                Debug.WriteLine(Tabs.Count);
            }
        }

        #endregion
    }
}