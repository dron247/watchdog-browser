using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
            var config = Config.Config.DefaultInstance;
            var mainWindow = new MainWindow();
            mainWindow.Closing += (sndr, eventData) => {
                if (MessageBox.Show("Вы действительно хотите завершить работу приложения?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No) {
                    eventData.Cancel = true;
                    return;
                }
            };
            mainWindow.Show();
        }
    }
}
