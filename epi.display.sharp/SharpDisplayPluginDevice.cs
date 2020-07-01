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
using PepperDash.Essentials.Core.Bridges;

using PepperDash.Core;
using Epi.Display.Sharp.SharpDisplayProtocolCmdStyleClasses;
using Crestron.SimplSharpPro.DeviceSupport;
using Epi.Display.Sharp.DisplayEventArgs;
using Epi.Display.Sharp.SharpPluginQueue;
using PepperDash.Essentials.Bridges;

namespace Epi.Display.Sharp
{

    public enum eInputParams
    {
        Poll,
        Hdmi1,
        Hdmi2,
        Hdmi3,
        Hdmi4,
        DsubRgb,
        DsubComponent,
        DsubVideo,
    }

    public enum ePowerParams
    {
        Poll,
        Off,
        On
    }

    public enum eCommands
    {
        Power,
        Input,
        CommandSetting
    }

    public enum eCommMethod
    {
        Off,
        Rs232,
        Lan
    }

	public class SharpDisplayPluginDevice : EssentialsDevice, PepperDash.Essentials.Core.Bridges.IBridgeAdvanced
	{
        private SharpDisplayPluginConfigObject Config;

        public IBasicCommunication Communication { get; private set; }
        public CommunicationGather PortGather { get; set; }
        public GenericCommunicationMonitor CommunicationMonitor { get; private set; }

        public BoolFeedback PowerIsOnFeedback { get; protected set; }
        public BoolFeedback PollIsStartedFeedback { get; protected set; }
        public IntFeedback InputActiveFeedback { get; protected set; }
        public StringFeedback InputActiveNameFeedback { get; protected set; }

        private SharpDisplayPluginPoll DisplayPoll;

        private Epi.SharpCommandTimer.SharpCommandTimer CommandTimer;
        private const ushort CommandTimeOut = 300;

        SharpDisplayPluginQueue CommandQueue;

        SharpDisplayProtocolCmdStyleBase Display;

        private char[] SplitChar = {',' , '\x0D'};
        private SharpDisplayPluginMessage Command;

        public eCommMethod CommMethod { get; protected set; }

        public string Delimiter;

        public int InputActive { get; set; }
        public string InputName { get; set; }
        public bool PowerIsOn { get; set; }
   

		public SharpDisplayPluginDevice(string key, string name, SharpDisplayPluginConfigObject config, IBasicCommunication comms)
			: base(key, name)
		{
            Debug.Console(0, this, "Constructing new SharpDisplayPluginDevice instance");

            Config = config;
            Communication = comms;
            
            // Check config for protocol type and get correct protocol
            var RetreiveSharpDisplay = SharpDisplayPluginProtocolCmdStyleFactory.BuildSharpDislplay(this, Config);
            if (RetreiveSharpDisplay != null)
            {
                // A valid protocol retreived.
                Display = RetreiveSharpDisplay;
                Debug.Console(2, this, "Added Display Name: {0}, Type: {1}, Comm: {2}", Config.Name, Display.GetType().ToString(),Communication.GetType().ToString());

                Delimiter = Display.Delimiter;
                PortGather = new CommunicationGather(Communication, Delimiter);
                PortGather.LineReceived += PortGather_LineReceived;

                // if Comm type is com it is serial, otherwise it is tcp or ssh... sets the correct value for RSPW command
                if (Communication.GetType().ToString().Contains("ComPort"))
                    CommMethod = eCommMethod.Rs232;
                else
                    CommMethod = eCommMethod.Lan;

                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 20000, 120000, 300000, Display.PollString);
                CommunicationMonitor.Start();

                // Attach to display feedbacks here
                PowerIsOnFeedback = new BoolFeedback(() => PowerIsOn);
                InputActiveFeedback = new IntFeedback(() => InputActive);
                InputActiveNameFeedback = new StringFeedback(() => InputName);

                // Set up polling
                DisplayPoll = new SharpDisplayPluginPoll(60000);

                PollIsStartedFeedback = DisplayPoll.PollIsEnabledFeedback;

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
            // Subscribe to poll event
            DisplayPoll.Poll += OnPoll;

            //Set Up Command Queue
            CommandQueue = new SharpDisplayPluginQueue();
            CommandTimer = new Epi.SharpCommandTimer.SharpCommandTimer();

            //Set up Command Queue processed event
            CommandQueue.MessageProcessed += OnCommandProcessed;
            CommandTimer.TimerCompleted += OnCommandTimerCompleted;
        }

        void PortGather_LineReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {

            try
            {
                //Send feedback to display protocol style for parsing - check if a response value was received and execute command action

                Debug.Console(2, this, "Port Line Received: {0}", e.Text);

                CommandQueue.PrintQueue();

                
                CommandTimer.StartTimer(CommandTimeOut);

                if (CommandQueue.GetNextCommand() != null)
                    Debug.Console(2, this, "Command in queue: {0}", CommandQueue.GetNextCommand().Command.ToString());
                else
                    Debug.Console(2, this, "Command queue: Empty");

                var ResponseObject = Display.HandleResponse(e.Text);
                if (ResponseObject is SharpDisplayPluginResponse)
                {
                    var ResponseObj = ResponseObject as SharpDisplayPluginResponse;
                    if (CommandQueue.GetNextCommand() != null)
                    {
                        var ResponseHandler = CommandQueue.DequeueMessage();
                        Debug.Console(2, this, "ResponseHandler Fired: {0}", ResponseHandler.ToString());
                        ResponseHandler.InvokeAction(ResponseObj.ResponseString);
                        var NextCommand = CommandQueue.GetNextCommand();
                        if(NextCommand != null)
                            CommandQueue.RaiseEvent_ProcessMessage(NextCommand.Command);
                    }
                    else
                    {
                        Debug.Console(2, this, "Queue empty, nothing to process");
                    }
                }
                else if (ResponseObject is SharpDisplayPluginResponseOk)
                {
                    Debug.Console(2, this, "OK received");
                }
                else if (ResponseObject is SharpDisplayPluginResponseError)
                {
                    Debug.Console(2, this, "Error received");
                }
            }
            catch (NullReferenceException ne)
            {
                Debug.Console(2, this, "Null Reference: {0}", ne.StackTrace);
            }
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

        public void PollSetTime(ushort pollTime)
        {
            DisplayPoll.PollTime = pollTime * 1000;
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
            //TODO: Set up Command to send to Display protocol style for correct command format.

            //var Cmd = "POWR   1";
            var Cmd = Display.FormatPowerCommand(eCommands.Power, ePowerParams.On);
            Debug.Console(2, "PowerOn: {0}", Cmd);
            Command = new SharpDisplayPluginMessage(this, Cmd);
            Command.SetResponseAction(SetPowerFb);
            ExecuteCommand(Command);
        }

        public void PowerOff()
        {
            //var Cmd = "POWR   0";
            var Cmd = Display.FormatPowerCommand(eCommands.Power, ePowerParams.Off);
            Debug.Console(2, "PowerOff: {0}", Cmd);
            Command = new SharpDisplayPluginMessage(this, Cmd);
            Command.SetResponseAction(SetPowerFb);
            ExecuteCommand(Command);
        }

        public void SelectInput(ushort input)
        {
            //var Cmd = "INPS   ";
            var Cmd = Display.FormatInputCommand(eCommands.Input, (eInputParams) input);
            Debug.Console(2, "Input: {0}{1}, {2}", Cmd,input, (eInputParams) input);
            Command = new SharpDisplayPluginMessage(this, string.Format("{0}{1}",Cmd, input));
            Command.SetResponseAction(SetInputFb);
            ExecuteCommand(Command);
        }

        public void CommandSettingOn()
        {
            //var Cmd = "RSPW   1";
            var Cmd = Display.FormatCommandSettingCommand(eCommands.CommandSetting, CommMethod);
            Debug.Console(2, "Command Set: {0}", Cmd);
            Command = new SharpDisplayPluginMessage(this, Cmd);
            Command.SetResponseAction(null);
            ExecuteCommand(Command);
        }

        public void ExecuteCommand(SharpDisplayPluginMessage command)
        {
            

            if (CommandQueue.GetNextCommand() == null)
            {
                SendLine(command.SendCommand());
                CommandTimer.StartTimer(CommandTimeOut);
            }
            
            Debug.Console(2, "Command Queued: {0} , {1}", this.Communication.GetType(), command.SendCommand());
            CommandQueue.EnqueueMessage(command);

            CommandQueue.PrintQueue();
        }

        private void SetPowerFb(string param)
        {
            var PowerState = false;

            Debug.Console(2, this, "Set Power FB: {0}", param);
            if(param.Contains("1"))
                PowerState = true;

            PowerIsOn = PowerState;
            PowerIsOnFeedback.FireUpdate();

        }

        private void SetInputFb(string param)
        {
            var Input = Int32.Parse(param);
            Debug.Console(2, this, "Set Input FB: {0}", param);
            try
            {
                eInputParams inputE = (eInputParams)Input;
                if (Display.InputParams.ContainsKey(inputE))
                {
                    InputActive = Int32.Parse(Display.InputParams[inputE]);
                    InputActiveFeedback.FireUpdate();
                }
            }
            catch
            {
                CrestronConsole.PrintLine("No compatible Input found");
            }

        }

        #endregion

        #region IBridgeAdvanced Members




        #endregion

        public void OnPoll(object sender, SharpDisplayPollEventArgs e)
        {
            Debug.Console(2, this, "OnPoll {0}", e.CurrentEvent);
            SharpDisplayPluginMessage Cmd;
            if (e.CurrentEvent.Contains("power"))
            {
                Cmd = new SharpDisplayPluginMessage(this, "POWR   ?");
                Cmd.SetResponseAction(SetPowerFb);
            }
            else if (e.CurrentEvent.Contains("input"))
            {
                Cmd = new SharpDisplayPluginMessage(this, "INPS   ?");
                Cmd.SetResponseAction(SetInputFb);
            }
            else
            {
                Cmd = null;
                Cmd.SetResponseAction(null);
            }

            if(Cmd != null)
                ExecuteCommand(Cmd);
        }

        public void OnCommandProcessed(object sender, SharpDisplayMessageEventArgs e)
        {
            SendLine(e.Response);
        }

        public void OnCommandTimerCompleted(object sender, EventArgs e)
        {
            Debug.Console(2, "Device Timeout");
            CommandQueue.ClearQueue();
            
        }

        #region IBridgeAdvanced Members

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            Debug.Console(2, this, "Link To API");
            this.LinkToApiExt(trilist, joinStart, joinMapKey, bridge);
        }

        #endregion
    }
}

