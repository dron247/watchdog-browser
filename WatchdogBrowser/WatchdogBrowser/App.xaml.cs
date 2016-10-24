using CefSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WatchdogBrowser.Handlers;

namespace WatchdogBrowser {
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application {
        /// <summary>
        /// Логика загрузки приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartup(object sender, StartupEventArgs e) {
            var mainWindow = new MainWindow();




            mainWindow.Closing += (sndr, eventData) => {
                if (MessageBox.Show("Вы действительно хотите завершить работу приложения?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No) {
                    eventData.Cancel = true;
                    return;
                }
            };

            var settings = new CefSettings();
            settings.CachePath = "cache";
            bool multithread = true;
            settings.MultiThreadedMessageLoop = multithread;
            settings.ExternalMessagePump = !multithread;

            if (!Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: new BrowserProcessHandler())) {
                throw new Exception("Unable to Initialize Cef");
            }


            mainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            Cef.Shutdown();
        }
    }
}
