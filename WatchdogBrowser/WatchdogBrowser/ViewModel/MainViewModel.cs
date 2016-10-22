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
                RaisePropertyChanged("Tabs");
            }
        }


        private void CreateMockupTabs() {
            Tabs.Add(new TabItemModel { Title = "Супер сайт 1", Url = "http://yandex.ru/", Closeable = false });
            Tabs.Add(new TabItemModel { Title = "Отдых", Url = "http://bash.im/", Closeable = true });
            Tabs.Add(new TabItemModel { Title = "Просто так", Url = "http://9gag.com/", Closeable = true });
            RaisePropertyChanged("Tabs");
        }
    }
}