using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace WatchdogBrowser.Handlers {
    public class WatchdogBrowserProcessHandler : BrowserProcessHandler {
        private Timer timer;
        private Dispatcher dispatcher;

        public WatchdogBrowserProcessHandler(Dispatcher dispatcher) {
            timer = new Timer { Interval = MaxTimerDelay, AutoReset = true };
            timer.Start();
            timer.Elapsed += TimerTick;

            this.dispatcher = dispatcher;
            this.dispatcher.ShutdownStarted += DispatcherShutdownStarted;
        }

        private void DispatcherShutdownStarted(object sender, EventArgs e) {
            //If the dispatcher is shutting down then we stop the timer
            if (timer != null) {
                timer.Stop();
            }
        }

        private void TimerTick(object sender, EventArgs e) {
            //Basically execute Cef.DoMessageLoopWork 30 times per second, on the UI Thread
            dispatcher.BeginInvoke((Action)(() => Cef.DoMessageLoopWork()), DispatcherPriority.Render);
        }

        protected override void OnScheduleMessagePumpWork(int delay) {
            //When delay <= 0 we'll execute Cef.DoMessageLoopWork immediately
            // if it's greater than we'll just let the Timer which fires 30 times per second
            // care of the call
            if (delay <= 0) {
                dispatcher.BeginInvoke((Action)(() => Cef.DoMessageLoopWork()), DispatcherPriority.Normal);
            }
        }

        public override void Dispose() {
            if (dispatcher != null) {
                dispatcher.ShutdownStarted -= DispatcherShutdownStarted;
                dispatcher = null;
            }

            if (timer != null) {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }
    }
}
