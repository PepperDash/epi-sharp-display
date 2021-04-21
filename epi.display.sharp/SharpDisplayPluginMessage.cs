using System;

namespace Epi.Display.Sharp
{
    public class SharpDisplayPluginMessage
    {
        public string Command { get; set; }
        public string Parameter { get; set; }

        private SharpDisplayPluginDevice Device;

        private enum eResponseType
        {
            Ok,
            Wait,
            Err
        };

        private Action<string> UserAction;

        public SharpDisplayPluginMessage(SharpDisplayPluginDevice device, string command)
        {
            Device = device;

            Command = command;

        }

        public void SetResponseAction(Action<string> action)
        {
            UserAction = action;
        }

        public void InvokeAction(string param)
        {
            if(UserAction != null)
                UserAction.Invoke(param);
        }

        public string SendCommand()
        {
            return Command + Device.Delimiter;
        }


        public override string  ToString()
        {
            var ReturnString = string.Format("CommandQueued: {0}", Command);
 	         return ReturnString;
        }
    }
}