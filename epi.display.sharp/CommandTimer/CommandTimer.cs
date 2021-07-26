using System;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Sharp.CommandTimer
{
    public class SharpCommandTimer
    {
        public bool Running;
        private CTimer _commandTimer;
        private ushort _countDownTime;


        //events
        public event EventHandler TimerCompleted;
        public event EventHandler TimerOneMinute;
        public event EventHandler TimerUpdate;


        //eventHandler
        public void RaiseEvent_TimerCompleted()
        {
            if (TimerCompleted != null)
            {
                TimerCompleted(this, EventArgs.Empty);
            }
        }

        public void RaiseEvent_TimerOneMinute()
        {
            if (TimerOneMinute != null)
            {
                TimerOneMinute(this, EventArgs.Empty);
            }
        }

        public void RaiseEvent_TimerUpdate()
        {
            if (TimerUpdate != null)
            {
                TimerUpdate(this, EventArgs.Empty);
            }
        }

        public ushort ResumeTimer()
        {
            if (!(_commandTimer.Disposed))
            {
                _commandTimer.Reset(1000, 1000);
            }
            return (0);
        }


        public ushort StartTimer(ushort timeInSeconds)
        {
            _countDownTime = timeInSeconds; // set count down time.
            if (_commandTimer != null) // Check if timer is null - not defined first time called 
            {
                if (_commandTimer.Disposed) // if timer has been disposed because it was stopped need to new
                {
                    _commandTimer = new CTimer(QueueTimerCallBack, null, 1000, 1000);
                }
                else
                {
                    _commandTimer.Reset(1000, 1000); // timer in progress reset    
                }
            }
            else
            {
                _commandTimer = new CTimer(QueueTimerCallBack, null, 1000, 1000);
            }

            Running = true;
            return (1);
        }


        public ushort StopTimer()
        {
            _commandTimer.Stop();
            _commandTimer.Dispose();
            _countDownTime = 0;

            Running = false;

            return (0);
        }

        public void SkipTimer()
        {
            _countDownTime = 0;
            _commandTimer.Reset();
        }


        public ushort PauseTimer()
        {
            _commandTimer.Stop();
            return (1);
        }

        public string CurrentTime()
        {
            return (String.Format("{0}:{1}", (_countDownTime/60).ToString("D2"), (_countDownTime%60).ToString("D2")));
        }


        public void QueueTimerCallBack(object state)
        {
            if (_countDownTime > 0)
            {
                _countDownTime--;
            }
            RaiseEvent_TimerUpdate();
            //CrestronConsole.PrintLine("Time Remaining: {0}:{1}", (countDownTime / 60).ToString("D2"), (countDownTime % 60).ToString("D2"));
            switch (_countDownTime)
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