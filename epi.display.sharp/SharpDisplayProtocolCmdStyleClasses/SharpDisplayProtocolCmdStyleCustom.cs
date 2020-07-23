using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Epi.Display.Sharp;
using PepperDash.Core;
using Epi.Display.Sharp.Inputs;
using System.Text.RegularExpressions;


namespace Epi.Display.Sharp.SharpDisplayProtocolCmdStyleClasses
{
    public class SharpDisplayProtocolCmdStyleCustom : SharpDisplayProtocolCmdStyleBase
    {
        private Dictionary<string, CustomCommand> Commands;
        public SharpDisplayProtocolCmdStyleCustom(SharpDisplayPluginDevice device, SharpDisplayPluginConfigObject config) : base(device, config)
        {
            ParamMatchRegexPattern = @"\d+";
            PollString = "POWR   ?\x0D";
            Pad = '\x20';

            Commands = new Dictionary<string, CustomCommand>();

            foreach (var cmd in Config.CustomCommands)
            {
                Commands.Add(cmd.Key, cmd);
            }
            

        }

        public override string FormatParameter(string parameter)
        {
            return parameter.PadLeft(Len,Pad);
        }

        public override void FormatCommandFromString(string command, string parameter)
        {
            var FormatedParameter = FormatParameter(parameter);
            SharpDisplayPluginMessage Command = new SharpDisplayPluginMessage(Device, string.Format("{0}{1}", command, parameter));
            Command.SetResponseAction(Device.SetPowerFb);
            Device.ExecuteCommand(Command);

        }

        public override void FormatPowerCommand(eCommands command, ePowerParams parameter)
        {

            var FormattedParameter = FormatParameter(PowerParams[parameter]);

            SharpDisplayPluginMessage Command = new SharpDisplayPluginMessage(Device, string.Format("{0}{1}", Commands[command], FormattedParameter));
            Command.SetResponseAction(Device.SetPowerFb);
            Device.ExecuteCommand(Command);
        }


        public override void FormatInputCommand(eCommands command, eInputParams parameter)
        {

            var FormattedParameter = FormatParameter(InputList[(ushort) parameter].InputCode);

            if (FormattedParameter == null)
                return;

            SharpDisplayPluginMessage Command = new SharpDisplayPluginMessage(Device, string.Format("{0}{1}", Commands[command], FormattedParameter));
            Command.SetResponseAction(Device.SetPowerFb);
            Device.ExecuteCommand(Command);
        }

        public override void FormatCommand(eCommands command, ePowerParams parameter)
        {
            var FormattedParameter = FormatParameter(PowerParams[parameter]);

            SharpDisplayPluginMessage Command = new SharpDisplayPluginMessage(Device, string.Format("{0}{1}", Commands[command], FormattedParameter));
            Command.SetResponseAction(Device.SetPowerFb);
            Device.ExecuteCommand(Command);
        }

        public override void FormatCommand(eCommands command, eInputParams parameter)
        {
            var Input = InputList[(ushort) parameter].InputCode;

            if (Input == null)
            {
                Debug.Console(2, "Invalid Input Selected");
                return;
            }

            var FormattedParameter = FormatParameter(Input);

            SharpDisplayPluginMessage Command = new SharpDisplayPluginMessage(Device, string.Format("{0}{1}", Commands[command], FormattedParameter));
            Command.SetResponseAction(Device.SetPowerFb);
            Device.ExecuteCommand(Command);
        }

        public override void FormatCommand(eCommands command, string parameter)
        {
            var FormatedParameter = FormatParameter(parameter);
            SharpDisplayPluginMessage Command = new SharpDisplayPluginMessage(Device, string.Format("{0}{1}", command, parameter));
            Command.SetResponseAction(Device.SetPowerFb);
            Device.ExecuteCommand(Command);
        }

    }
}