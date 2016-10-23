using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel() {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}            
            CreateMockupTabs();
        }


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
                selectedTab = value;
                Debug.WriteLine(selectedTab?.Title);
                RaisePropertyChanged(nameof(SelectedTab));
            }
        }


        private void CreateMockupTabs() {
            Tabs.Add(new TabItemModel { Title = "Вкладка тест", Url = "https://github.com/", Closeable = true });
            //Tabs.Add(new TabItemModel { Title = "Проверка", Url = "http://yandex.ru/", Closeable = true });
            //Tabs.Add(new TabItemModel { Title = "Отдых", Url = "http://bash.im/", Closeable = true });
            //Tabs.Add(new TabItemModel { Title = "Просто так", Url = "http://9gag.com/", Closeable = true });
            foreach (var tab in Tabs) {
                tab.Close += (sender, args) => {
                    if (Tabs.Count == 1) {
                        App.Current.Shutdown();
                    } else {
                        Tabs.Remove((TabItemModel)sender);
                        RaisePropertyChanged(nameof(Tabs));

                        Debug.WriteLine(Tabs.Count);
                    }
                };
            }
            Debug.WriteLine(Tabs.Count);
            RaisePropertyChanged(nameof(Tabs));
        }
    }
}