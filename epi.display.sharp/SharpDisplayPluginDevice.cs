using System;
using System.Collections.Generic;
// For Basic SIMPL#Pro classes


using System.Linq;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharpPro.DeviceSupport;
using Epi.Display.Sharp.DisplayEventArgs;
using Epi.Display.Sharp.SharpDisplayProtocolCmdStyleClasses;
using Epi.Display.Sharp.SharpPluginQueue;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace Epi.Display.Sharp
{
    
    public enum eInputParams
    {
        Poll,
        Input1,
        Input2,
        Input3,
        Input4,
        Input5,
        Input6,
        Input7,
        Input8,
        Input9,
        Input10
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
        public BoolFeedback PollEnabledFeedback { get; protected set; }
        public IntFeedback InputActiveFeedback { get; protected set; }
        public StringFeedback InputActiveNameFeedback { get; protected set; }
        public StringFeedback[] InputNameListFeedback { get; protected set; }

        public List<string> InputLabels;

        private SharpDisplayPluginPoll DisplayPoll;

        private Epi.SharpCommandTimer.SharpCommandTimer CommandTimer;
        private const ushort CommandTimeOut = 300;

        SharpDisplayPluginQueue CommandQueue;

        SharpDisplayProtocolCmdStyleBase Display;

        public eCommMethod CommMethod { get; protected set; }

        public string Delimiter;

        private bool DisplayEnabled;

        public int InputActive { get; set; }
        public string InputName { get; set; }
        public bool PowerIsOn { get; set; }
   

		public SharpDisplayPluginDevice(string key, string name, SharpDisplayPluginConfigObject config, IBasicCommunication comms)
			: base(key, name)
		{
            Debug.Console(0, this, "Constructing new SharpDisplayPluginDevice instance");

            Config = config;
            Communication = comms;

            try
            {
                Display = SharpDisplayPluginProtocolCmdStyleFactory.BuildSharpDislplay(this, Config.Protocol);
                // A valid protocol retreived.
                Debug.Console(2, this, "Added Display Name: {0}, Type: {1}, Comm: {2}", Config.Name, Display.GetType().ToString(), Communication.GetType().ToString());

                Delimiter = Display.Delimiter;
                PortGather = new CommunicationGather(Communication, Delimiter);
                PortGather.LineReceived += PortGather_LineReceived;

                DisplayEnabled = Config.Enabled;

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

                PollEnabledFeedback = DisplayPoll.PollEnabledFeedback;
                

                Initialize();
            }
            catch (NullReferenceException nEx)
            {
                Debug.Console(2, Debug.ErrorLogLevel.Error, "Invalid Protocol, please check configuration file: {0}", nEx.ToString());
            }       
		}

        public void Initialize()
        {
            Debug.Console(2, this, "Initializing");

            InputLabels = Display.GetInputs();
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
                    var ResponseHandler = CommandQueue.DequeueMessage();
                    if (ResponseHandler == null)
                    {
                        Debug.Console(2, this, "Queue empty, nothing to process");
                        return;
                    }

                    var ResponseObj = ResponseObject as SharpDisplayPluginResponse;
                    Debug.Console(2, this, "ResponseHandler Fired: {0} - Invoking on param: {1}", ResponseHandler.ToString(), ResponseObj.ResponseString);
                    ResponseHandler.InvokeAction(ResponseObj.ResponseString);
                    var NextCommand = CommandQueue.GetNextCommand();
                    if (NextCommand != null)
                        CommandQueue.RaiseEvent_ProcessMessage(NextCommand.Command);

                    return;
                }

                if (ResponseObject is SharpDisplayPluginResponseOk)
                {
                    Debug.Console(2, this, "OK received");
                    return;
                }

                if (ResponseObject is SharpDisplayPluginResponseWait)
                {
                    Debug.Console(2, this, "Wait received");
                    return;
                }

                if (ResponseObject is SharpDisplayPluginResponseError)
                {
                    Debug.Console(2, this, "Error received");
                    CommandQueue.DequeueMessage();
                    return;
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
            if(DisplayEnabled)
                DisplayPoll.StartPoll();
        }

        public void PollStop()
        {
            if(DisplayEnabled)
                DisplayPoll.StopPoll();
        }

        public void PollSetTime(ushort pollTime)
        {
            if(DisplayEnabled)
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
            if (DisplayEnabled) 
                Display.FormatCommand(eCommands.Power, ePowerParams.On);
        }

        public void PowerOff()
        {
            if (DisplayEnabled)
                Display.FormatCommand(eCommands.Power, ePowerParams.Off);
        }

        public void PowerToggle()
        {
            if (DisplayEnabled)
            {
                if (Display == null)
                {
                    Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Display not defined: Confirm Protocol configuration");
                    return;
                }

                if (PowerIsOn)
                    PowerOff();
                else
                    PowerOn();
            }
        }

        public void PowerPoll()
        {
            if (DisplayEnabled)
                Display.FormatCommand(eCommands.Power, ePowerParams.Poll);
        }

        public void SelectInput(ushort input)
        {
            if (DisplayEnabled)
                Display.FormatCommand(eCommands.Input, (eInputParams)input);
        }


        public void InputPoll()
        {
            if (DisplayEnabled)
                Display.FormatCommand(eCommands.Input, eInputParams.Poll);
        }

        public void PollNext()
        {
            if (DisplayEnabled)
                DisplayPoll.PollExecute(null);
        }

        public void CommandSettingOn()
        {
            if (DisplayEnabled)
                Display.FormatCommand(eCommands.CommandSetting, ePowerParams.On);
        }

        public void ExecuteCommand(SharpDisplayPluginMessage command)
        {
            if (DisplayEnabled)
            {
                if (CommandQueue.GetNextCommand() == null)
                {
                    SendLineRaw(command.SendCommand());
                    CommandTimer.StartTimer(CommandTimeOut);
                }

                Debug.Console(2, "Command Queued: {0}", command.SendCommand());
                CommandQueue.EnqueueMessage(command);

                CommandQueue.PrintQueue();
            }
        }

        public void SetPowerFb(string param)
        {
            var PowerState = false;
            if (param.CompareTo("1") <= 0)
            {
                Debug.Console(2, this, "Set Power FB: {0}", param);
                if (param.Contains("1"))
                    PowerState = true;

                PowerIsOn = PowerState;
                PowerIsOnFeedback.FireUpdate();
            }
        }

        public void SetInputFb(string param)
        {
            var Input = Int32.Parse(param);
            Debug.Console(2, this, "Set Input FB: {0}", param);
            try
            {
                    var Key = Display.InputList.FirstOrDefault(o => o.Value.InputCode == param);
                    Debug.Console(2, this, "InputParams: {0}, as INT: {1}", Key, (int)Key.Key);
                    if (!Key.IsDefault())
                    {
                        InputActive = (int)Key.Key;
                        InputActiveFeedback.FireUpdate();
                    }
            }
            catch
            {
                CrestronConsole.PrintLine("No compatible Input found");
            }

        }

        public void PrintInputs()
        {
            if (Display == null)
                return;

            foreach (var input in Display.InputList)
            {
                if (input.Key > 0)
                {
                    Debug.Console(0, "Input: {0}, Name: {1}, InputCode: {2}", input.Key, input.Value.Name, input.Value.InputCode);
                    InputLabels.Add(input.Value.Name);
                }
            }
        }

        #endregion

        public void SetProtocol(string protocol)
        {
            if (DisplayEnabled)
            {
                try
                {
                    Display = SharpDisplayPluginProtocolCmdStyleFactory.BuildSharpDislplay(this, protocol);
                }
                catch (NullReferenceException nEx)
                {
                    Debug.Console(2, Debug.ErrorLogLevel.Error, "Invalid Protocol, please check configuration file: {0}", nEx.ToString());
                }
            }
        }

        #region IBridgeAdvanced Members




        #endregion

        public void OnPoll(object sender, SharpDisplayPollEventArgs e)
        {
            Debug.Console(2, this, "OnPoll {0}", e.CurrentEvent);
            switch (e.CurrentEvent)
            {
                case "power":
                    Display.FormatCommand(eCommands.Power, ePowerParams.Poll);
                    break;
                case "input":
                    Display.FormatCommand(eCommands.Input, eInputParams.Poll);
                    break;
                case "commandSetting":
                    Display.FormatCommand(eCommands.CommandSetting, (int)CommMethod);
                    break;
            }
            /*
            if (e.CurrentEvent.Contains("power"))
            {
                Display.FormatCommand(eCommands.Power, ePowerParams.Poll);
            }
            else if (e.CurrentEvent.Contains("input"))
            {
                Display.FormatCommand(eCommands.Input, eInputParams.Poll);
            }
            else if (e.CurrentEvent.Contains("commandSetting"))
            {
                Display.FormatCommand(eCommands.CommandSetting, (int) CommMethod);
            }
            */
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

    public static class Extensions
    {
        public static bool IsDefault<T>(this T value) where T : struct
        {
            bool isDefault = value.Equals(default(T));

            return isDefault;
        }
    }
}

