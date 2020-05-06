using System;
using System.Collections;
using System.Collections.Generic;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       				// For Basic SIMPL#Pro classes

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Linq;

using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Bridges;
using PepperDash.Core;
using Epi.Display.Sharp.SharpDisplayPluginDeviceClasses;
using Crestron.SimplSharpPro.DeviceSupport;
using Epi.Display.Sharp.DisplayEventArgs;

namespace Epi.Display.Sharp
{

	public class SharpDisplayPluginDevice : EssentialsDevice, IBridgeAdvanced
	{
        private SharpDisplayPluginConfigObject Config;

        public IBasicCommunication Communication { get; private set; }
        public CommunicationGather PortGather { get; set; }
        public GenericCommunicationMonitor CommunicationMonitor { get; private set; }

        public BoolFeedback PowerIsOnFeedback { get; protected set; }
        public IntFeedback InputActiveFeedback { get; protected set; }
        public StringFeedback InputActiveNameFeedback { get; protected set; }

        private SharpDisplayPluginPoll DisplayPoll;

        SharpDisplayBase Display;

        private char[] SplitChar = {',' , '\x0D'};

        public int CommMethod { get; protected set; }




		public SharpDisplayPluginDevice(string key, string name, SharpDisplayPluginConfigObject config, IBasicCommunication comms)
			: base(key, name)
		{
            Debug.Console(0, this, "Constructing new SharpDisplayPluginDevice instance");

            Config = config;
            Communication = comms;
            
            var RetreiveSharpDisplay = SharpDisplayPluginProtocolStyleFactory.BuildSharpDislplay(this, Config);
            if (RetreiveSharpDisplay != null)
            {
                Display = RetreiveSharpDisplay;
                Debug.Console(2, this, "Added Display Name: {0}, Type: {1}, Comm: {2}", Display.Name, Display.GetType().ToString(),Communication.GetType().ToString());


                PortGather = new CommunicationGather(Communication, Display.Delimiter);
                PortGather.LineReceived += PortGather_LineReceived;

                if (Communication.GetType().ToString().Contains("com"))
                    CommMethod = 1;
                else
                    CommMethod = 2;

                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 20000, 120000, 300000, Display.PollString);
                CommunicationMonitor.Start();

                PowerIsOnFeedback = Display.PowerIsOnFeedback;
                InputActiveFeedback = Display.InputActiveFeedback;

                

                DisplayPoll = new SharpDisplayPluginPoll(300000);

                Initialize();
            }
            else
            {
                Debug.Console(2, this, "Sharp Protocol Type not valid");
            }

            
		}

        public void Initialize()
        {
            Debug.Console(2, this, "Initializing");
            DisplayPoll.Poll += OnPoll;
        }

        void PortGather_LineReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            Display.ParseFeedback(e.Text);    
        }

        public void SendTestFeedback(string testResponse)
        {
            Display.ParseFeedback(testResponse);
        }

        #region Polling

        public void PollStart()
        {
            DisplayPoll.StartPoll();
        }

        public void PollStop()
        {
            DisplayPoll.StopPoll();
        }

        public void PollSetTime(long pollTime)
        {
            DisplayPoll.PollTime = pollTime;
        }
        #endregion

        #region System Commands
        public void SendLine(string s)
        {
            Debug.Console(2, this, "TX: {0}", s);
            Communication.SendText(s + "\x0D");
        }

        public void SendLineRaw(string s)
        {
            Debug.Console(2, this, "TX: {0}", s);
            Communication.SendText(s);
        }

        #endregion

        #region DisplayControl
       
       
        public void PowerOn()
        {
            Display.PowerOn();
        }

        public void PowerOff()
        {
            Display.PowerOff();
        }

        public void PowerToggle()
        {
            Display.PowerToggle();
        }

        public void SelectInput(ushort input)
        {
            Display.SelectInput(input);
        }

        public void CommandSettingOn()
        {
            Display.CommandSettingOn();
        }


        #endregion

        #region IBridgeAdvanced Members

        void IBridgeAdvanced.LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApi bridge)
        {
            Debug.Console(2, this, "Link To API");
            this.LinkToApiExt(trilist, joinStart, joinMapKey, bridge);
        }



        #endregion

        public void OnPoll(object sender, SharpDisplayPollEventArgs e)
        {
            Display.PollDevice(e.CurrentEvent);
        }
    }
}

