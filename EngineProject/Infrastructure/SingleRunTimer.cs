using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace EngineProject.Infrastructure
{
    //Base timer runs new thread with set frequency
    //But doesn't check if previously started process is finished
    public class SingleRunTimer
    {
        private Timer timer;
        private bool isRunning = false;

        public SingleRunTimer(int millisecondsFrequency)
        {
            timer = new Timer(millisecondsFrequency);
            timer.Enabled = false;
        }

        public Action<object, ElapsedEventArgs> Elapsed
        {
            set
            {
                timer.Elapsed += (sender, e) =>
                {
                    if (!isRunning)
                    {
                        try
                        {
                            isRunning = true;
                            value(sender, e);
                            isRunning = false;
                        }
                        catch (Exception ex)
                        {
                            isRunning = false;
                            throw ex;
                        }
                    }
                };
            }
            get { return null; }
        }

        public void Start()
        {
            timer.Enabled = true;
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
            timer.Enabled = false;
        }
    }
}
