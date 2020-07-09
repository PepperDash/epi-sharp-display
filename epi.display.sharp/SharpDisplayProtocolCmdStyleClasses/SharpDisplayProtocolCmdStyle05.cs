using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Epi.Display.Sharp;
using PepperDash.Core;

using System.Text.RegularExpressions;


namespace Epi.Display.Sharp.SharpDisplayProtocolCmdStyleClasses
{
    public class SharpDisplayProtocolCmdStyle05 : SharpDisplayProtocolCmdStyleBase
    {
        public SharpDisplayProtocolCmdStyle05()
        {
            ParamMatchRegexPattern = @"\d+";
            PollString = "POWR????";

            InputParams = new Dictionary<eInputParams, string>{
                {eInputParams.DsubRgb,"1"},
                {eInputParams.DsubComponent,"2"},
                {eInputParams.DsubVideo,"4"},
                {eInputParams.Hdmi1,"9"},
                {eInputParams.Hdmi2,"10"},
                {eInputParams.Hdmi3,"12"},
                {eInputParams.Hdmi4,"13"},
                {eInputParams.Poll,"????"}
            };

            PowerParams = new Dictionary<ePowerParams, string>{
                {ePowerParams.On, "1"},
                {ePowerParams.Off,"0"},
                {ePowerParams.Poll,"????"},
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
            return parameter.PadLeft(4);
        }

        #region IHasProtocolStyle Members
        /*
        public override string FormatCommandFromString(string command, string parameter)
        {
            var Command = command;
            var FormatedParameter = parameter.PadLeft(4);
            return string.Format("{0}{1}", Command, FormatedParameter);
        }

        public override string FormatPowerCommand(eCommands command, ePowerParams parameter)
        {
            var Command = command;

            var FormattedParameter = PowerParams[parameter].PadLeft(4);
            return string.Format("{0}{1}", Commands[Command], FormattedParameter);
        }

        public override string FormatInputCommand(eCommands command, eInputParams parameter)
        {
            var Command = command;
            string FormattedParameter;
            if (InputParams.ContainsKey(parameter))
            {
                FormattedParameter = InputParams[parameter].PadLeft(4);
                return string.Format("{0}{1}", Commands[Command], FormattedParameter);
            }

            return string.Empty;
        }

        public override string FormatCommandSettingCommand(eCommands command, eCommMethod parameter)
        {
            var Command = command;

            var FormattedParameter = CommandSettingParams[parameter].PadLeft(4);
            return string.Format("{0}{1}", Commands[Command], FormattedParameter);
        }
         */

        #endregion

    }
}