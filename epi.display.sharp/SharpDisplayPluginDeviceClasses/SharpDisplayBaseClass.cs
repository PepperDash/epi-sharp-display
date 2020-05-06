using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Bridges;

namespace Epi.Display.Sharp.SharpDisplayPluginDeviceClasses
{
    public abstract class SharpDisplayBase : IHasPower, IHasInput, IHasPowerToggle
    {

        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Protocol { get; set; }

        protected SharpDisplayPluginDevice Device;
        protected SharpDisplayPluginConfigObject DeviceConfig;
        
        public IntFeedback InputActiveFeedback { get; protected set; }
        public StringFeedback InputActiveNameFeedback { get; protected set; }
        public BoolFeedback PowerIsOnFeedback { get; protected set; }

        public string PollString { get; protected set; }
        public string Delimiter { get; protected set; }

        public Dictionary<string, JoinDataComplete> DeviceJoinData;

        protected int InputActive;
        protected string InputName;
        protected bool PowerIsOn;

        


        public SharpDisplayBase(SharpDisplayPluginDevice device, SharpDisplayPluginConfigObject deviceConfig)
        {
            this.Device = device;
            this.DeviceConfig = deviceConfig;

            Name = DeviceConfig.Name;
            Enabled = DeviceConfig.Enabled;
            Protocol = DeviceConfig.Protocol;

            InputActiveFeedback = new IntFeedback(() => InputActive);
            InputActiveNameFeedback = new StringFeedback(() => InputName);
            PowerIsOnFeedback = new BoolFeedback(() => PowerIsOn);

            
            
            Debug.Console(2,"Constructed Sharp Display Device. Name: {0}, Type: {1}, Enabled: {2}", Name, Protocol, Enabled);
           
        }


        public abstract void ParseFeedback(string feedback);

        protected void PortGather_LineReceived(object sender, PepperDash.Core.GenericCommMethodReceiveTextArgs e)
        {
            ParseFeedback(e.ToString());
        }

        public void PollDevice(string pollType)
        {
            if (pollType.ToLower() == "power")
                PollPower();
            else if (pollType.ToLower() == "input")
                PollInput();
        }


        #region IHasPower Members

        public abstract void PowerOn();

        public abstract void PowerOff();

        #endregion

        #region IHasInput Members

        public abstract void SelectInput(ushort input);

        #endregion

        #region IHasPowerToggle Members

        public abstract void PowerToggle();

        #endregion

        public abstract void PollPower();

        public abstract void PollInput();

        public abstract void CommandSettingOn();

    }

}
