using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Epi.Display.Sharp.DisplayEventArgs;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace Epi.Display.Sharp
{
    public class SharpDisplayPluginPoll
    {
        public long PollTime { get; set; }
        private CTimer PollTimer;
        public bool PollingEnabled;
        private string PollTypeCurrent;

        public BoolFeedback PollEnabledFeedback;

        public List<string> DisplayPollTypesEnabled;

        public EventHandler<SharpDisplayPollEventArgs> Poll;

        public SharpDisplayPluginPoll(long pollTime)
        {
            PollingEnabled = false;
            PollTypeCurrent = "";
            PollTime = pollTime;
            DisplayPollTypesEnabled = new List<string> { "power", "input", "commandSetting" };

            PollEnabledFeedback = new BoolFeedback(() => PollingEnabled);
        }

        public SharpDisplayPluginPoll(long pollTime, string[] enable)
        {
            PollingEnabled = false;
            PollTypeCurrent = "";
            PollTime = pollTime;
            DisplayPollTypesEnabled = new List<string>();

            DisplayPollTypesEnabled.AddRange(enable);

            PollEnabledFeedback = new BoolFeedback(() => PollingEnabled);
        }


        public void StartPoll()
        {
            Debug.Console(2, "Poll Started: Timer is {0}",PollTime);

            if (PollTimer != null)
                PollTimer.Dispose();

            PollingEnabled = true;
            PollEnabledFeedback.FireUpdate();

            PollTimer = new CTimer(PollExecute, PollTime);
        }

        public void StopPoll()
        {
            Debug.Console(2, "Poll Stopped");
            PollTimer.Stop();
            PollingEnabled = false;
            PollEnabledFeedback.FireUpdate();
        }

        public void PollExecute(object state)
        {
            Debug.Console(2,"Poll Execute");

            //Check if handler has subscriptions
            var handler = Poll;
            if (handler == null)
                return;
            Debug.Console(2, "Poll subscribed");
            //Check current poll type. If not defined, set to first type to poll. Poll type represents a function of display, power, input, etc... add to List as needed
            if (PollTypeCurrent == "")
            {
                //Set to first type to poll for

                Debug.Console(2, "Polling set to blank");

                PollTypeCurrent = DisplayPollTypesEnabled[0];

                Debug.Console(2, "Polling set to {0}", PollTypeCurrent);
            }
            else
            {
                //Check index of current type polled for. 

                int PollTypeCurrentIndex = DisplayPollTypesEnabled.IndexOf(PollTypeCurrent);
                Debug.Console(2, "Get Poll index = {0}", PollTypeCurrentIndex);

                //Reached end of polling... set to beginning
                if (PollTypeCurrentIndex == DisplayPollTypesEnabled.Count - 1)
                {
                    Debug.Console(2, "Poll index equals count");
                    PollTypeCurrentIndex = 0;
                }
                else
                {
                    // advance to next
                    PollTypeCurrentIndex = DisplayPollTypesEnabled.IndexOf(PollTypeCurrent) + 1;
                    Debug.Console(2, "Poll Advance to {0}", PollTypeCurrentIndex);
                }
                //Get type to poll for
                PollTypeCurrent = DisplayPollTypesEnabled.ElementAt<string>(PollTypeCurrentIndex);
                Debug.Console(2,"Polling: Polltype: {0}", PollTypeCurrent);
            }

            handler(this, new SharpDisplayPollEventArgs(PollTypeCurrent));
            if (PollingEnabled)
                StartPoll();
        }
    }
}