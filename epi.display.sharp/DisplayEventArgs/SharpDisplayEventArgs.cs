namespace Epi.Display.Sharp.DisplayEventArgs
{
    public class SharpDisplayPollEventArgs : System.EventArgs
    {
        public string CurrentEvent { get; set; }

        public SharpDisplayPollEventArgs(string currentEvent)
        {
            CurrentEvent = currentEvent;
        }
    }

    public class SharpDisplayMessageEventArgs : System.EventArgs
    {
        public string Response { get; set; }

        public SharpDisplayMessageEventArgs(string response)
        {
            Response = response;
        }
    }
}