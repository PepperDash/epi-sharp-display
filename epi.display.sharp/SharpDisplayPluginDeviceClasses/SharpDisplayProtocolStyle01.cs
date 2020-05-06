using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using System.Text.RegularExpressions;

using PepperDash.Core;
using PepperDash.Essentials.Core;


namespace Epi.Display.Sharp.SharpDisplayPluginDeviceClasses
{
    public class SharpDisplayProtocolStyle01 : SharpDisplayBase, IHasPower, IHasPowerToggle, IHasInput
    {
        private char[] SplitChar = { '\x0D' };

        private SharpDisplayPluginDevice DisplayDevice;
        private SharpDisplayPluginConfigObject DisplayConfig;



        private enum eInput
        {
            Toggle = 0,
            DsubRgb = 1,
            DsubComponent = 2,
            DsubVideo = 3,
            Hdmi1 = 9,
            Hdmi2 = 10,
            Hdmi3 = 11,
            Hdmi4 = 12,
        }

        static class InputParam
        {
            public static string DsubRgb { get { return "   1"; } }
            public static string DsubComponent { get { return "   2"; } }
            public static string DsubVideo { get { return "   3"; } }
            public static string Hdmi1 { get { return "   9"; } }
            public static string Hdmi2 { get { return "  10"; } }
            public static string Hdmi3 { get { return "  11"; } }
            public static string Hdmi4 { get { return "  12"; } }
        }

        public SharpDisplayProtocolStyle01(SharpDisplayPluginDevice device, SharpDisplayPluginConfigObject config)
            : base(device, config)
        {
            DisplayDevice = device;
            DisplayConfig = config;

            Delimiter = "\x0D";

            DisplayDevice.SendLine("RSPW   1");

        }

        public override void ParseFeedback(string feedback)
        {
            Regex ResponseType = new Regex("[a-zA-Z]{4}");
            Regex ResponseParam = new Regex(@"\d+");

            Debug.Console(2, "Response Recieved: {0}", feedback);
            
            
            var Response = feedback.TrimEnd(SplitChar);
            

            Match Command = ResponseType.Match(Response);
            Match Parameters = ResponseParam.Match(Response);

            Debug.Console(2, "Regex Command: {0} : {1}", Command.ToString(), Command.Success);
            Debug.Console(2, "Regex Parameters: {0} : {1}", Parameters.ToString(), Parameters.Success);

            if (Command.Success && Parameters.Success)
            {          
                if (Command.ToString().Contains("POWR"))
                {
                    if (Parameters.ToString().Contains("1"))
                    {
                        PowerIsOn = true;
                        PowerIsOnFeedback.FireUpdate();
                        Debug.Console(2, "Power Is ON");
                    }
                    else
                    {
                        PowerIsOn = false;
                        PowerIsOnFeedback.FireUpdate();
                        Debug.Console(2, "Power Is OFF");
                    }
                }
                else if (Command.ToString().Contains("IAVD"))
                {
                    if (Parameters.ToString() == InputParam.Hdmi1)
                        InputActive = (int)eInput.Hdmi1;
                    else if (Parameters.ToString() == InputParam.Hdmi2)
                        InputActive = (int)eInput.Hdmi2;
                    else if (Parameters.ToString() == InputParam.Hdmi3)
                        InputActive = (int)eInput.Hdmi3;
                    else if (Parameters.ToString() == InputParam.Hdmi4)
                        InputActive = (int)eInput.Hdmi4;
                    else if (Parameters.ToString() == InputParam.DsubComponent)
                        InputActive = (int)eInput.DsubComponent;
                    else if (Parameters.ToString() == InputParam.DsubRgb)
                        InputActive = (int)eInput.DsubRgb;
                    else if (Parameters.ToString() == InputParam.DsubVideo)
                        InputActive = (int)eInput.DsubVideo;

                    Debug.Console(2, "Input is: {0}", InputActive);

                    InputActiveFeedback.FireUpdate();
                    InputActiveNameFeedback.FireUpdate();
                }
                
            }

        }

        public override void PowerOn()
        {
            DisplayDevice.SendLine("POWR   1");
        }

        public override void PowerOff()
        {
            CommandSettingOn();
            DisplayDevice.SendLine("POWR   0");
        }

        public override void PowerToggle()
        {
            if (PowerIsOn)
            {
                CommandSettingOn();
                DisplayDevice.SendLine("POWR   0");
            }
            else
            {
                DisplayDevice.SendLine("POWR   1");
            }
        }

        public override void SelectInput(ushort input)
        {
            DisplayDevice.SendLine(string.Format("IAVD   {0}", input));
        }

        public override void PollPower()
        {
            DisplayDevice.SendLine("POWR   ?");
        }

        public override void PollInput()
        {
            DisplayDevice.SendLine("IAVD   ?");
        }



        public override void CommandSettingOn()
        {
            DisplayDevice.SendLine(string.Format("RSPW   {0}", DisplayDevice.CommMethod));
        }

        public override string ToString()
        {
            string type = this.GetType().Name.ToString();
            return "Device Type: " + type + ", Device Name: " + Name;
        }
    }
}