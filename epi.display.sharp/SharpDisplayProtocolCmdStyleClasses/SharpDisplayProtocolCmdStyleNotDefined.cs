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
    public class SharpDisplayProtocolCmdStyleNotDefined : SharpDisplayProtocolCmdStyleBase
    {

        public SharpDisplayProtocolCmdStyleNotDefined(SharpDisplayPluginDevice device) : base(device)
        {
            ParamMatchRegexPattern = @"\d+";
            Pad = '\x20';

            InputList = new Dictionary<ushort, SharpDisplayPluginInput>
            {

            };


            PowerParams = new Dictionary<ePowerParams, string>{

            };


            CommandSettingParams = new Dictionary<eCommMethod, string>{

            };


            Commands = new Dictionary<eCommands, string>()
            {

            };

        }

        public override string FormatParameter(string parameter)
        {
            return parameter.PadLeft(Len, Pad);
        }

        public override void FormatPowerCommand(eCommands command, ePowerParams parameter)
        {
            Debug.Console(2,Debug.ErrorLogLevel.Error, "Protocol Style Not Defined");
        }

        public override void FormatInputCommand(eCommands command, eInputParams parameter)
        {
            Debug.Console(2, Debug.ErrorLogLevel.Error, "Protocol Style Not Defined");
        }
        
        public override void FormatCommandFromString(string command, string parameter)
        {
            Debug.Console(2, Debug.ErrorLogLevel.Error, "Protocol Style Not Defined");
        }

        public override void FormatCommand(eCommands command, ePowerParams parameter)
        {
            Debug.Console(2, Debug.ErrorLogLevel.Error, "Protocol Style Not Defined");
        }

        public override void FormatCommand(eCommands command, eInputParams parameter)
        {
            Debug.Console(2, Debug.ErrorLogLevel.Error, "Protocol Style Not Defined");
        }

        public override void FormatCommand(eCommands command, string parameter)
        {
            Debug.Console(2, Debug.ErrorLogLevel.Error, "Protocol Style Not Defined");
        }


    }
}