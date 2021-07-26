using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Sharp
{
    public class SharpDisplayCommand
    {
        private ECommands CommandType;
        private EInputParams InputParameter;

        public string Command { get; set; }
        public string Parameter { get; set; }

        private Action<string> UserAction;

        public SharpDisplayCommand(ECommands eCommand)
        {
            CommandType = eCommand;
        }

        public SharpDisplayCommand(ECommands eCommand, EInputParams eParameter)
            : this(eCommand)
        {
            InputParameter = eParameter;

            Command = InputParameter.ToString();
        }

        public SharpDisplayCommand(ECommands eCommand, EPowerParams eParameter)
            : this(eCommand)
        {
            Command = eParameter.ToString();
        }

        public SharpDisplayCommand(string command, string parameter)
        {
            Command = command;
            Parameter = parameter;
        }

        public void SetResponseAction(Action<string> action)
        {
            Debug.Console(2, "SetResponseAction: {0}::{1}", action.ToString());
            UserAction = action;
        }

        public void InvokeAction(string param)
        {
            Debug.Console(2, "InvokeAction {0}", param);
            if (UserAction != null)
                UserAction.Invoke(param);
        }

        public string SendCommand()
        {
            return string.Format("{0}{1}", Command, Parameter);
        }


        public ECommands GetCommand()
        {
            Debug.Console(2, "Get Command: {0}",CommandType);
            return CommandType;
        }

        public EInputParams GetInput()
        {
            Debug.Console(2, "Send Command: {0}", InputParameter.ToString());
            return InputParameter;
        }

        public override string ToString()
        {
            var returnString = string.Format("CommandQueued: {0}", Command);
            return returnString;
        }
    }

    public class SharpDisplayPluginResponse
    {
        public string ResponseString { get; private set; }

        public SharpDisplayPluginResponse(string response)
        {
            ResponseString = response;
        }
    }
}