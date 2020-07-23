using System;

namespace Epi.Display.Sharp.DisplayEventArgs
{
    public class SharpDisplayPollEventArgs : EventArgs
    {
        public string CurrentEvent { get; set; }

        public SharpDisplayPollEventArgs(string currentEvent)
        {
            CurrentEvent = currentEvent;
        }
    }

    public class SharpDisplayMessageEventArgs : EventArgs
    {
        public string Response { get; set; }

        public SharpDisplayMessageEventArgs(string response)
        {
            Response = response;
        }
    }
}