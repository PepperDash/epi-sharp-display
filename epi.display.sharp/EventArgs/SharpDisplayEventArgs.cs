using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

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
}