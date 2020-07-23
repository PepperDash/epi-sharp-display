using System.Collections.Generic;
using Epi.Display.Sharp.Inputs;
using PepperDash.Core;


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