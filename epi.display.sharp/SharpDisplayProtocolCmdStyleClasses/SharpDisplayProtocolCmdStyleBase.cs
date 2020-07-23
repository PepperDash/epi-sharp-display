using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Epi.Display.Sharp.Inputs;
using PepperDash.Core;


namespace Epi.Display.Sharp.SharpDisplayProtocolCmdStyleClasses
{
    public abstract class SharpDisplayProtocolCmdStyleBase
    {


        public Dictionary<eCommands, string> Commands;
        public Dictionary<ePowerParams, string> PowerParams;
        public Dictionary<eCommMethod, string> CommandSettingParams;

        public Dictionary<ushort, SharpDisplayPluginInput> InputList;

        protected string ParamMatchRegexPattern = string.Empty;

        protected string Command;
        protected string Parameter;

        protected SharpDisplayPluginDevice Device;
        protected SharpDisplayPluginConfigObject Config;
        
        public string PollString = string.Empty;
        public string Delimiter = "\x0D";
        protected char Pad;
        protected int Len = 4;

        public SharpDisplayProtocolCmdStyleBase(SharpDisplayPluginDevice device)
        {
            Device = device;
        }

        public SharpDisplayProtocolCmdStyleBase(SharpDisplayPluginDevice device, SharpDisplayPluginConfigObject config)
        {
            Device = device;
            Config = config;        
        }

        #region IHasProtocolStyle Members


        public abstract string FormatParameter(string parameter);

        #endregion

        public Object HandleResponse(string response)
        {
            Regex ParamRegex = new Regex(ParamMatchRegexPattern);

            Debug.Console(2, "Handling Response: {0}", response);

            if (response.ToUpper().Contains("OK"))
                return new SharpDisplayPluginResponseOk();

            if (response.ToUpper().Contains("ERR"))
                return new SharpDisplayPluginResponseError();

            if (response.ToUpper().Contains("WAIT"))
                return new SharpDisplayPluginResponseWait();

            Match Param = ParamRegex.Match(response);
            Debug.Console(2, "Param match: {0}", Param.Value);
            if (!Param.Success)
            {
                Debug.Console(2, "Param !success");
                return new SharpDisplayPluginResponseError();
            }

            return new SharpDisplayPluginResponse(Param.ToString());
        }

        public virtual void FormatCommand(eCommands command, ePowerParams parameter)
        {
            var Cmd = Commands[command];

            if (Cmd == "")
                return;

            var FormattedParameter = FormatParameter(PowerParams[parameter]);

            SharpDisplayPluginMessage Command = new SharpDisplayPluginMessage(Device, string.Format("{0}{1}", Cmd, FormattedParameter));
            Command.SetResponseAction(Device.SetPowerFb);
            Device.ExecuteCommand(Command);
        }


        public virtual void FormatCommand(eCommands command, eInputParams parameter)
        {
            var Cmd = Commands[command];

            if (Cmd == "")
                return;

            var Input = InputList[(ushort)parameter].InputCode;

            if (Input == null)
            {
                Debug.Console(2, "Invalid Input Selected");
                return;
            }

            var FormattedParameter = FormatParameter(Input);

            SharpDisplayPluginMessage Command = new SharpDisplayPluginMessage(Device, string.Format("{0}{1}", Cmd, FormattedParameter));
            Command.SetResponseAction(Device.SetInputFb);
            Device.ExecuteCommand(Command);
        }

        public virtual void FormatCommand(eCommands command, int parameter)
        {
            var Cmd = Commands[command];

            if (Cmd == "")
                return;

            var FormatedParameter = FormatParameter(parameter.ToString());
            SharpDisplayPluginMessage Command = new SharpDisplayPluginMessage(Device, string.Format("{0}{1}", Cmd, parameter));
            Command.SetResponseAction(Device.SetPowerFb);
            Device.ExecuteCommand(Command);
        }

        public virtual void FormatCommand(eCommands command, string parameter)
        {
            var Cmd = Commands[command];

            if (Cmd == "")
                return;

            var FormatedParameter = FormatParameter(parameter);
            SharpDisplayPluginMessage Command = new SharpDisplayPluginMessage(Device, string.Format("{0}{1}", Cmd, parameter));
            Command.SetResponseAction(Device.SetPowerFb);
            Device.ExecuteCommand(Command);
        }


        public List<string> GetInputs()
        {
            List<string> inputList = new List<string>(InputList.Count);
            foreach (var input in InputList)
                if(input.Key > 0)
                    inputList.Add(input.Value.Name);

            return inputList;
              
        }

        public override string ToString()
        {
            string type = this.GetType().Name.ToString();
            return "Device Type: " + type;
        }
    }

    public class SharpDisplayPluginResponseOk
    {

    }

    public class SharpDisplayPluginResponseError
    {
    }

    public class SharpDisplayPluginResponseWait
    {
    }
}