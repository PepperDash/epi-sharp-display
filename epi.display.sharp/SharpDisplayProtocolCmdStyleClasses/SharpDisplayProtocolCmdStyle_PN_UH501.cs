using System.Collections.Generic;
using Epi.Display.Sharp.Inputs;
using PepperDash.Core;


namespace Epi.Display.Sharp.SharpDisplayProtocolCmdStyleClasses
{
    public class SharpDisplayProtocolCmdStyle_PN_UH501 : SharpDisplayProtocolCmdStyleBase
    {

        public SharpDisplayProtocolCmdStyle_PN_UH501(SharpDisplayPluginDevice device) : base(device)
        {
            ParamMatchRegexPattern = @"\d+";
            PollString = "POWR   ?\x0D";
            Pad = '\x20';

            InputList = new Dictionary<ushort, SharpDisplayPluginInput>
            {
                {1,new SharpDisplayPluginInput("HDMI1","10")},                
                {2,new SharpDisplayPluginInput("HDMI2","13")},
                {3,new SharpDisplayPluginInput("DSub","2")},
                {4,new SharpDisplayPluginInput("USB","11")},
                {5,new SharpDisplayPluginInput("RGB","1")},
                {6,new SharpDisplayPluginInput("TV","25")},
                {7,new SharpDisplayPluginInput("","")},
                {8,new SharpDisplayPluginInput("","")},
                {9,new SharpDisplayPluginInput("","")},
                {10,new SharpDisplayPluginInput("","")},
                {0, new SharpDisplayPluginInput("Poll","????")}
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
                {eCommands.Input,"INPS"},
                {eCommands.CommandSetting, ""}
            };

        }

        public override string FormatParameter(string parameter)
        {
            return parameter.PadLeft(Len,Pad);
        }
    }
}