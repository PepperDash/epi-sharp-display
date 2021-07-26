using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Sharp.DisplayEventArgs;

namespace PepperDash.Essentials.Sharp
{
    public class SharpDisplayPluginPoll
    {
        public List<string> DisplayPollTypesEnabled;

        public EventHandler<SharpDisplayPollEventArgs> Poll;
        private CTimer _pollTimer;
        private string _pollTypeCurrent;

        public SharpDisplayPluginPoll(long pollTime) : this(pollTime, null)
        {
        }

        public SharpDisplayPluginPoll(long pollTime, IEnumerable<string> enable)
        {
            PollingEnabled = false;
            _pollTypeCurrent = "";
            PollTime = pollTime;
            DisplayPollTypesEnabled = new List<string>();

            PollEnabledFeedback = new BoolFeedback(() => PollingEnabled);

            if (enable == null)
            {
                return;
            }

            DisplayPollTypesEnabled.AddRange(enable);
        }

        public BoolFeedback PollEnabledFeedback { get; private set; }
        public bool PollingEnabled { get; set; }

        public long PollTime { get; set; }


        public void StartPoll()
        {
            Debug.Console(2, "Poll Started: Timer is {0}", PollTime);

            if (_pollTimer != null)
            {
                _pollTimer.Dispose();
            }

            PollingEnabled = true;
            PollEnabledFeedback.FireUpdate();

            _pollTimer = new CTimer(PollExecute, PollTime);
        }

        public void StopPoll()
        {
            Debug.Console(2, "Poll Stopped");
            _pollTimer.Stop();
            PollingEnabled = false;
            PollEnabledFeedback.FireUpdate();
        }

        public void PollExecute(object state)
        {
            Debug.Console(2, "Poll Execute");

            //Check if handler has subscriptions
            var handler = Poll;
            if (handler == null)
            {
                return;
            }
            Debug.Console(2, "Poll subscribed");
            //Check current poll type. If not defined, set to first type to poll. Poll type represents a function of display, power, input, etc... add to List as needed
            if (_pollTypeCurrent == "")
            {
                //Set to first type to poll for

                Debug.Console(2, "Polling set to blank");

                _pollTypeCurrent = DisplayPollTypesEnabled[0];

                Debug.Console(2, "Polling set to {0}", _pollTypeCurrent);
            }
            else
            {
                //Check index of current type polled for. 

                var pollTypeCurrentIndex = DisplayPollTypesEnabled.IndexOf(_pollTypeCurrent);
                Debug.Console(2, "Get Poll index = {0}", pollTypeCurrentIndex);

                //Reached end of polling... set to beginning
                if (pollTypeCurrentIndex == DisplayPollTypesEnabled.Count - 1)
                {
                    Debug.Console(2, "Poll index equals count");
                    pollTypeCurrentIndex = 0;
                }
                else
                {
                    // advance to next
                    pollTypeCurrentIndex = DisplayPollTypesEnabled.IndexOf(_pollTypeCurrent) + 1;
                    Debug.Console(2, "Poll Advance to {0}", pollTypeCurrentIndex);
                }
                //Get type to poll for
                _pollTypeCurrent = DisplayPollTypesEnabled.ElementAt(pollTypeCurrentIndex);
                Debug.Console(2, "Polling: Polltype: {0}", _pollTypeCurrent);
            }

            handler(this, new SharpDisplayPollEventArgs(_pollTypeCurrent));
            if (PollingEnabled)
            {
                StartPoll();
            }
        }
    }
}