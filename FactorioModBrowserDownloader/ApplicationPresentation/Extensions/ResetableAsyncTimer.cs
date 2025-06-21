using System.Windows.Threading;

namespace FactorioNexus.ApplicationPresentation.Extensions
{
    public class ResetableAsyncTimer
    {
        private readonly DispatcherTimer timer = new DispatcherTimer();

        public ResetableAsyncTimer(TimeSpan interval, Action callback)
        {
            timer.Interval = interval;
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                callback.Invoke();
            };
        }

        public void Reset()
        {
            timer.Start();
        }
    }
}
