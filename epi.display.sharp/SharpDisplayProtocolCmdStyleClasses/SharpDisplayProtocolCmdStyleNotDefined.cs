using System.Collections.Generic;
using Epi.Display.Sharp.Inputs;
using PepperDash.Core;


namespace Epi.Display.Sharp.SharpDisplayProtocolCmdStyleClasses
{
    public class SharpDisplayProtocolCmdStyleNotDefined : SharpDisplayProtocolCmdStyleBase
    {

        public SharpDisplayProtocolCmdStyleNotDefined(SharpDisplayPluginDevice device)
            : base(device)
        {
            ParamMatchRegexPattern = @"\d+";
            Pad = '\x20';

            InputList = new Dictionary<ushort, SharpDisplayPluginInput>();

            PowerParams = new Dictionary<EPowerParams, string>();
            CommandSettingParams = new Dictionary<ECommMethod, string>();
            Commands = new Dictionary<ECommands, string>();
        }

        protected override void InitLocalPorts()
        {

        }

        public override string FormatParameter(string parameter)
        {
            return parameter.PadLeft(Len, Pad);
        }

        public override void FormatCommand(ECommands command, EPowerParams parameter)
        {
            Debug.Console(2, Debug.ErrorLogLevel.Error, "Protocol Style Not Defined");
        }

        public override void FormatCommand(ECommands command, EInputParams parameter)
        {
            Debug.Console(2, Debug.ErrorLogLevel.Error, "Protocol Style Not Defined");
        }



    }
}