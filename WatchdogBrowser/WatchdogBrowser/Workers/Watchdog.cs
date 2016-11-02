using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WatchdogBrowser.Workers {
    public class Watchdog {
        public Watchdog() { }

        public void StartWatch() {
            LastHeartbeat = DateTime.Now;
            if (timer == null) {
                timer = new Timer(1000);
                timer.AutoReset = true;
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            } else {
                timer.Stop();
                timer.Start();
            }
        }

        public void StopWatch() {
            timer?.Stop();
        }


        private void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            lock (locker) {
                currentTime = DateTime.Now;
                var interval = (int)currentTime.Subtract(LastHeartbeat).TotalSeconds;
                //Debug.WriteLine($"interval = {interval}");
                if (interval > HeartbeatTimeout) {
                    if (interval > SwitchMirrorTimeout) {
                        NeedChangeMirror?.Invoke(this, EventArgs.Empty);
                        LastHeartbeat = DateTime.Now;
                    } else {
                        if (ReloadAttempts == 0) {
                            NeedReload?.Invoke(this, EventArgs.Empty);
                            ReloadAttempts++;
                        } else {
                            ReloadAttempts++;
                            if (ReloadAttempts == 10) {
                                ReloadAttempts = 0;
                            }
                        }
                    }
                }
            }
        }

        public void DoHeartbeat() {
            LastHeartbeat = DateTime.Now;
        }

        /// <summary>
        /// Интервал, в котором ожидется heartbeat
        /// </summary>
        public int HeartbeatTimeout { get; set; }



        /// <summary>
        /// Таймаут heartbeat по зеркалу, необходима смена зеркала
        /// </summary>
        public int SwitchMirrorTimeout { get; set; }

        object raLocker = new object();//блокировщик попыток перезагрузки
        object locker = new object();//блокировщик доступа к последнему heartbeat

        int reloadAttempts = 0;
        DateTime lastHeartbeat;
        DateTime currentTime;
        Timer timer;


        int ReloadAttempts {
            get {
                return reloadAttempts;
            }
            set {
                lock (raLocker) {
                    reloadAttempts = value;
                }
            }
        }

        DateTime LastHeartbeat {
            get {
                return lastHeartbeat;
            }
            set {
                lock (locker) {
                    lastHeartbeat = value;
                }
                ReloadAttempts = 0;
            }
        }

        public event EventHandler NeedReload;
        public event EventHandler NeedChangeMirror;
    }
}
