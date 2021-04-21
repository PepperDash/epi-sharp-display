using System;
using Crestron.SimplSharp;

namespace Epi.Display.Sharp.CommandTimer
{
    public class SharpCommandTimer
    {

        private CTimer CommandTimer;
        private ushort CountDownTime;


        public bool Running;

        
        //events
        public event EventHandler TimerCompleted;
        public event EventHandler TimerOneMinute;
        public event EventHandler TimerUpdate;



        //eventHandler
        public void RaiseEvent_TimerCompleted()
        {
            if (TimerCompleted != null)
                TimerCompleted(this, EventArgs.Empty);
        }

        public void RaiseEvent_TimerOneMinute()
        {
            if (TimerOneMinute != null)
                TimerOneMinute(this, EventArgs.Empty);
        }

        public void RaiseEvent_TimerUpdate()
        {
            if (TimerUpdate != null)
                TimerUpdate(this, EventArgs.Empty);
        }

        public ushort ResumeTimer()
        {
            if (!(CommandTimer.Disposed))
            {
                CommandTimer.Reset(1000, 1000);           
            }
            return (0);
        }


        public ushort StartTimer(ushort timeInSeconds)
        {

            CountDownTime = timeInSeconds;  // set count down time.
            if (CommandTimer != null)      // Check if timer is null - not defined first time called 
            {
                if (CommandTimer.Disposed)    // if timer has been disposed because it was stopped need to new
                {
                    CommandTimer = new CTimer(QueueTimerCallBack, null, 1000, 1000);
                    
                }
                else
                {
                    CommandTimer.Reset(1000, 1000); // timer in progress reset    
                }
            }
            else
            {
                CommandTimer = new CTimer(QueueTimerCallBack, null, 1000, 1000);               
            }

            Running = true;
            return (1);

        }


        public ushort StopTimer()
        {
            CommandTimer.Stop();
            CommandTimer.Dispose();
            CountDownTime = 0;

            Running = false;

            return (0);
        
        }

        public void SkipTimer()
        {
            CountDownTime = 0;
            CommandTimer.Reset();

        }


        public ushort PauseTimer()
        {
            CommandTimer.Stop();
            return (1);
            
        }

        public string CurrentTime()
        {
            return (String.Format("{0}:{1}", (CountDownTime / 60).ToString("D2"), (CountDownTime % 60).ToString("D2")));
        }


        public void QueueTimerCallBack(object state)
        {
            if(CountDownTime > 0)
                CountDownTime--;
            RaiseEvent_TimerUpdate();
            //CrestronConsole.PrintLine("Time Remaining: {0}:{1}", (countDownTime / 60).ToString("D2"), (countDownTime % 60).ToString("D2"));
            switch (CountDownTime)
            {
                case 60:
                    RaiseEvent_TimerOneMinute();
                    break;
                case 0:
                    StopTimer();
                    RaiseEvent_TimerCompleted();
                    break;
            }
        }
    }

}