using System.Collections.Generic;
using Epi.Display.Sharp.Inputs;
using PepperDash.Core;


namespace Epi.Display.Sharp.SharpDisplayProtocolCmdStyleClasses
{
    public class SharpDisplayProtocolCmdStyle_PN_UH701 : SharpDisplayProtocolCmdStyleBase
    {

        public SharpDisplayProtocolCmdStyle_PN_UH701(SharpDisplayPluginDevice device) : base(device)
        {
            ParamMatchRegexPattern = @"\d+";
            PollString = "POWR????\x0D";
            Pad = '\x20';

            InputList = new Dictionary<ushort, SharpDisplayPluginInput>
            {
                {1,new SharpDisplayPluginInput("HDMI1","1")},                
                {2,new SharpDisplayPluginInput("HDMI2","2")},
                {3,new SharpDisplayPluginInput("USB","4")},
                {4,new SharpDisplayPluginInput("TV","0")},
                {0, new SharpDisplayPluginInput("Poll","????")}
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
            return parameter.PadRight(Len,Pad);
        }
    }
}