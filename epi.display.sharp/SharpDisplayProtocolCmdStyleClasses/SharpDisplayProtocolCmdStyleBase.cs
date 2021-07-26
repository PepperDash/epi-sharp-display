using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Sharp.Inputs;

namespace PepperDash.Essentials.Sharp.SharpDisplayProtocolCmdStyleClasses
{
    public abstract class SharpDisplayProtocolCmdStyleBase : IRoutingInputs
    {
        protected readonly SharpDisplayPluginDevice DisplayDevice;
        protected string Command;
        public Dictionary<ECommMethod, string> CommandSettingParams;
        public Dictionary<ECommands, string> Commands;
        protected SharpDisplayPluginConfigObject Config;
        public string Delimiter = "\x0D";

        public Dictionary<ushort, SharpDisplayPluginInput> InputList;
        protected int Len = 4;
        protected char Pad;

        protected string ParamMatchRegexPattern = string.Empty;

        protected string Parameter;

        public string PollString = string.Empty;
        public Dictionary<EPowerParams, string> PowerParams;

        protected SharpDisplayProtocolCmdStyleBase(SharpDisplayPluginDevice device)
        {
            DisplayDevice = device;
            InputPorts = DisplayDevice.InputPorts;
            Key = device.Key;
        }

        protected SharpDisplayProtocolCmdStyleBase(SharpDisplayPluginDevice device,
            SharpDisplayPluginConfigObject config) : this(device)
        {
            Config = config;
        }

        #region IRoutingInputs Members

        public string Key { get; private set; }
        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        #endregion

        protected void AddRoutingInputPort(RoutingInputPort port, SharpDisplayCommand command)
        {
            port.FeedbackMatchObject = command;
            InputPorts.Add(port);
        }

        protected abstract void InitLocalPorts();

        public abstract string FormatParameter(string parameter);

        public Object HandleResponse(string response)
        {
            var paramRegex = new Regex(ParamMatchRegexPattern);

            Debug.Console(2, "Handling Response: {0}", response);

            if (response.ToUpper().Contains("OK"))
            {
                return new SharpDisplayPluginResponseOk();
            }

            if (response.ToUpper().Contains("ERR"))
            {
                return new SharpDisplayPluginResponseError();
            }

            if (response.ToUpper().Contains("WAIT"))
            {
                return new SharpDisplayPluginResponseWait();
            }

            var param = paramRegex.Match(response);
            Debug.Console(2, "Param match: {0}", param.Value);
            if (param.Success)
            {
                return new SharpDisplayPluginResponse(param.ToString());
            }
            Debug.Console(2, "Param !success");
            return new SharpDisplayPluginResponseError();
        }

        public virtual void FormatCommand(ECommands command, EPowerParams parameter)
        {
            var cmd = Commands[command];

            if (cmd == "")
            {
                Debug.Console(1, "Command Not Supported");
                return;
            }

            var displayCommand = new SharpDisplayCommand(command, parameter);
            displayCommand.SetResponseAction(DisplayDevice.SetPowerFb);
            DisplayDevice.ExecuteCommand(displayCommand);
        }


        public virtual void FormatCommand(ECommands command, EInputParams parameter)
        {
            var cmd = Commands[command];

            if (cmd == "")
            {
                return;
            }

            var input = InputList[(ushort) parameter].InputCode;

            if (input == null)
            {
                Debug.Console(2, "Invalid Input Selected");
                return;
            }

            var displayCommand = new SharpDisplayCommand(command, parameter);
            displayCommand.SetResponseAction(DisplayDevice.SetInputFb);
            DisplayDevice.ExecuteCommand(displayCommand);
        }

        public virtual void FormatCommand(SharpDisplayCommand command)
        {
            var cmd = Commands[command.GetCommand()];

            if (cmd == "")
            {
                return;
            }

            var input = InputList[(ushort) command.GetInput()].InputCode;

            if (input == null)
            {
                Debug.Console(2, "Invalid Input Selected");
                return;
            }

            command.SetResponseAction(DisplayDevice.SetInputFb);
            DisplayDevice.ExecuteCommand(command);
        }

        public List<string> GetInputs()
        {
            var inputList = new List<string>(InputList.Count);
            inputList.AddRange(from input in InputList where input.Key > 0 select input.Value.Name);

            return inputList;
        }

        public override string ToString()
        {
            var type = GetType().Name;
            return "Device Type: " + type;
        }
    }
}