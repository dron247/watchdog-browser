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

namespace WatchdogBrowser.Models {
    public class TabItemModel : ObservableObject {
        public TabItemModel() {
            CloseTabCommand = new RelayCommand(CloseMethod);
        }


        public event EventHandler Close;

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
                Debug.WriteLine("Browser in tab " + browser?.Address);
            }
        }

        





        #endregion
    }
}
