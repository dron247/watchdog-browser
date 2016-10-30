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
            lastHeartbeat = DateTime.Now;
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

        object locker = new object();
        private void Timer_Elapsed(object sender, ElapsedEventArgs e) {            
            lock (locker) {
                currentTime = DateTime.Now;
                var interval = currentTime.Subtract(lastHeartbeat).Seconds;
                Debug.WriteLine($"interval = {interval}");
                if (interval > HeartbeatTimeout) {
                    if (interval > SwitchMirrorTimeout) {
                        NeedChangeMirror?.Invoke(this, EventArgs.Empty);
                        lastHeartbeat = DateTime.Now;
                    } else  {
                        NeedReload?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public void DoHeartbeat() {
            lock (locker) {
                lastHeartbeat = DateTime.Now;
            }            
        }

        /// <summary>
        /// Интервал, в котором ожидется heartbeat
        /// </summary>
        public int HeartbeatTimeout { get; set; }



        /// <summary>
        /// Таймаут heartbeat по зеркалу, необходима смена зеркала
        /// </summary>
        public int SwitchMirrorTimeout { get; set; }



        DateTime lastHeartbeat;
        DateTime currentTime;

        Timer timer;

        public event EventHandler NeedReload;
        public event EventHandler NeedChangeMirror;
    }
}
