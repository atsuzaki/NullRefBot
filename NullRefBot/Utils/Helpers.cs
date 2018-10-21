using System;

namespace NullRefBot.Utils {
    public class Helpers {
        public static void SetTimeout(Action callback, int delayInMs) {
            var timer = new System.Timers.Timer(delayInMs);
            timer.Elapsed += (source, e) => {
                callback.Invoke();

                timer.Stop();
                timer.Dispose();
            };

            timer.Enabled = true;
            timer.Start();
        }
    }
}