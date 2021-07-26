using System;
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Sharp.SharpDisplayProtocolCmdStyleClasses
{
    public class SharpDisplayProtocolCmdStylePnUh501 : SharpDisplayProtocolCmdStyleBase
    {
        //readonly SharpDisplayPluginDevice DisplayDevice;

        public SharpDisplayProtocolCmdStylePnUh501(SharpDisplayPluginDevice device) : base(device)
        {
            ParamMatchRegexPattern = @"\d+";
            PollString = "POWR   ?\x0D";
            Pad = '\x20';

            PowerParams = new Dictionary<EPowerParams, string>
            {
                {EPowerParams.On, "1"},
                {EPowerParams.Off, "0"},
                {EPowerParams.Poll, "?"},
            };


            CommandSettingParams = new Dictionary<ECommMethod, string>
            {
                {ECommMethod.Lan, "2"},
                {ECommMethod.Rs232, "1"},
                {ECommMethod.Off, "0"}
            };


            Commands = new Dictionary<ECommands, string>
            {
                {ECommands.Power, "POWR"},
                {ECommands.Input, "INPS"},
                {ECommands.CommandSetting, ""}
            };

            InitLocalPorts();
        }

        protected override void InitLocalPorts()
        {
            Debug.Console(2, "Initializing Ports");
            AddRoutingInputPort(
                new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.AudioVideo,
                    eRoutingPortConnectionType.Hdmi,
                    new Action(
                        () => DisplayDevice.InputSelect(new SharpDisplayCommand(ECommands.Input, EInputParams.Hdmi1))),
                    this), new SharpDisplayCommand(ECommands.Input, EInputParams.Hdmi1));
            AddRoutingInputPort(
                new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.AudioVideo,
                    eRoutingPortConnectionType.Hdmi,
                    new Action(
                        () => DisplayDevice.InputSelect(new SharpDisplayCommand(ECommands.Input, EInputParams.Hdmi2))),
                    this), new SharpDisplayCommand(ECommands.Input, EInputParams.Hdmi2));
            AddRoutingInputPort(
                new RoutingInputPort(RoutingPortNames.DisplayPortIn1, eRoutingSignalType.AudioVideo,
                    eRoutingPortConnectionType.Hdmi,
                    new Action(
                        () => DisplayDevice.InputSelect(new SharpDisplayCommand(ECommands.Input, EInputParams.Dvi1))),
                    this), new SharpDisplayCommand(ECommands.Input, EInputParams.Dvi1));
            AddRoutingInputPort(
                new RoutingInputPort(RoutingPortNames.RgbIn, eRoutingSignalType.AudioVideo,
                    eRoutingPortConnectionType.Hdmi,
                    new Action(
                        () => DisplayDevice.InputSelect(new SharpDisplayCommand(ECommands.Input, EInputParams.Rgb1))),
                    this), new SharpDisplayCommand(ECommands.Input, EInputParams.Rgb1));
            AddRoutingInputPort(
                new RoutingInputPort(RoutingPortNames.AntennaIn, eRoutingSignalType.AudioVideo,
                    eRoutingPortConnectionType.Hdmi,
                    new Action(
                        () => DisplayDevice.InputSelect(new SharpDisplayCommand(ECommands.Input, EInputParams.Tv1))),
                    this), new SharpDisplayCommand(ECommands.Input, EInputParams.Tv1));
        }

        public override string FormatParameter(string parameter)
        {
            return parameter.PadLeft(Len, Pad);
        }
    }
}