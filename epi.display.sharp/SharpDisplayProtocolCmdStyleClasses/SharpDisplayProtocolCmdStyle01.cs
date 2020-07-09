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
    public class SharpDisplayProtocolCmdStyle01 : SharpDisplayProtocolCmdStyleBase
    {

        public SharpDisplayProtocolCmdStyle01(SharpDisplayPluginDevice device) : base(device)
        {
            ParamMatchRegexPattern = @"\d+";
            PollString = "POWR   ?\x0D";
            Pad = '\x20';

            InputList = new Dictionary<ushort, SharpDisplayPluginInput>
            {
                {1,new SharpDisplayPluginInput("HDMI1","9")},                
                {2,new SharpDisplayPluginInput("HDMI2","10")},
                {3,new SharpDisplayPluginInput("HDMI3","12")},
                {4,new SharpDisplayPluginInput("HDMI4","13")},
                {5,new SharpDisplayPluginInput("RGB","1")},
                {6,new SharpDisplayPluginInput("Component","2")},
                {7,new SharpDisplayPluginInput("Video","4")},
                {0, new SharpDisplayPluginInput("Poll","?")}
            };


            PowerParams = new Dictionary<ePowerParams, string>{
                {ePowerParams.On, "1"},
                {ePowerParams.Off,"0"},
                {ePowerParams.Poll,"?"},
            };


            CommandSettingParams = new Dictionary<eCommMethod, string>{
                {eCommMethod.Lan, "2"},
                {eCommMethod.Rs232, "1"},
                {eCommMethod.Off, "0"}
            };


            Commands = new Dictionary<eCommands, string>()
            {
                {eCommands.Power,  "POWR"},
                {eCommands.Input,"IAVD"},
                {eCommands.CommandSetting, "RSPW"}
            };

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