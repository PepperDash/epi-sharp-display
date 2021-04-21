// For Basic SIMPL#Pro classes

// For Basic SIMPL# Classes
using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Epi.Display.Sharp.CommandTimer;
using Epi.Display.Sharp.DisplayEventArgs;
using Epi.Display.Sharp.SharpDisplayProtocolCmdStyleClasses;
using Epi.Display.Sharp.SharpPluginQueue;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;


namespace Epi.Display.Sharp
{
    public enum EParams
    {
    }

    public enum EInputParams
    {
        Poll,
        Hdmi1,
        Hdmi2,
        Hdmi3,
        Input4,
        Dvi1,
        Input6,
        Rgb1,
        Input8,
        Input9,
        Tv1
    }

    public enum EPowerParams
    {
        Poll,
        Off,
        On
    }

    public enum ECommMethod
    {
        Off,
        Rs232,
        Lan
    }

    public class SharpDisplayPluginDevice : TwoWayDisplayBase, IBridgeAdvanced, ICommunicationMonitor
        //public class SharpDisplayPluginDevice : EssentialsDevice, PepperDash.Essentials.Core.Bridges.IBridgeAdvanced
    {
        private const ushort CommandTimeOut = 300;
        private readonly bool DisplayEnabled;
        private readonly SharpDisplayPluginPoll DisplayPoll;
        public string Delimiter;
        public List<string> InputLabels;
        private SharpDisplayPluginQueue CommandQueue;
        private SharpCommandTimer CommandTimer;
        private SharpDisplayProtocolCmdStyleBase Display;
        private int _inputActive;
        private bool _powerIsOn;

        public SharpDisplayPluginDevice(string key, string name, SharpDisplayPluginConfigObject config,
                                        IBasicCommunication comms)
            : base(key, name)
        {
            Debug.Console(0, this, "Constructing new SharpDisplayPluginDevice instance");

            var deviceConfiguration = config;
            if (deviceConfiguration == null) throw new ArgumentNullException("config1");
            Communication = comms;

            try
            {
                Display = SharpDisplayPluginProtocolCmdStyleFactory.BuildSharpDislplay(this, deviceConfiguration.Protocol);
                // A valid protocol retreived.
                Debug.Console(2, this, "Added Display Name: {0}, Type: {1}, Comm: {2}", deviceConfiguration.Name,
                              Display.GetType().ToString(), Communication.GetType().ToString());

                Delimiter = Display.Delimiter;
                PortGather = new CommunicationGather(Communication, Delimiter);
                PortGather.LineReceived += PortGather_LineReceived;

                DisplayEnabled = deviceConfiguration.Enabled;

                var checkCommunicationType = Communication as ISocketStatus;
                CommMethod = checkCommunicationType == null ? ECommMethod.Rs232 : ECommMethod.Lan;


                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 20000, 120000, 300000,
                                                                       Display.PollString);
                DeviceManager.AddDevice(CommunicationMonitor);
                CommunicationMonitor.Start();

                // Attach to display feedbacks here
                PowerIsOnFeedback = new BoolFeedback(() => PowerIsOn);
                InputActiveFeedback = new IntFeedback(() => InputActive);
                InputActiveNameFeedback = new StringFeedback(() => InputName);
                StatusFeedback = new IntFeedback(() => (int) CommunicationMonitor.Status);


                // Set up polling
                DisplayPoll = new SharpDisplayPluginPoll(60000);

                PollEnabledFeedback = DisplayPoll.PollEnabledFeedback;


                Initialize();
            }
            catch (NullReferenceException nEx)
            {
                Debug.Console(2, Debug.ErrorLogLevel.Error, "Invalid Protocol, please check configuration file: {0}",
                              nEx.ToString());
            }
        }

        public override sealed BoolFeedback PowerIsOnFeedback
        {
            get { return base.PowerIsOnFeedback; }
            protected set { base.PowerIsOnFeedback = value; }
        }

        public IBasicCommunication Communication { get; private set; }
        public CommunicationGather PortGather { get; set; }
        //public GenericCommunicationMonitor CommunicationMonitor { get; private set; }

        public BoolFeedback PollEnabledFeedback { get; protected set; }
        public IntFeedback InputActiveFeedback { get; protected set; }
        public StringFeedback InputActiveNameFeedback { get; protected set; }
        public StringFeedback[] InputNameListFeedback { get; protected set; }
        public IntFeedback StatusFeedback { get; set; }

        public ECommMethod CommMethod { get; protected set; }

        public int InputActive
        {
            get { return _inputActive; }
            set
            {
                _inputActive = value;
                InputActiveFeedback.FireUpdate();
            }
        }

        public string InputName { get; set; }

        public bool PowerIsOn
        {
            get { return _powerIsOn; }
            set
            {
                _powerIsOn = value;
                PowerIsOnFeedback.FireUpdate();
            }
        }

        public StatusMonitorBase CommunicationMonitor { get; private set; }


        public void Initialize()
        {
            Debug.Console(2, this, "Initializing");

            //InputLabels = _display.GetInputs();
            // Subscribe to poll event

            DisplayPoll.Poll += OnPoll;

            //Set Up Command Queue
            CommandQueue = new SharpDisplayPluginQueue();
            CommandTimer = new SharpCommandTimer();

            //Set up Command Queue processed event
            CommandQueue.MessageProcessed += OnCommandProcessed;
            CommandTimer.TimerCompleted += OnCommandTimerCompleted;
        }

        private void PortGather_LineReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            try
            {
                //Send feedback to display protocol style for parsing - check if a response value was received and execute command action

                Debug.Console(2, this, "Port Line Received: {0}", e.Text);

                CommandQueue.PrintQueue();
                CommandTimer.StartTimer(CommandTimeOut);

                if (CommandQueue.GetNextCommand() != null)
                    Debug.Console(2, this, "Command in queue: {0}", CommandQueue.GetNextCommand().Command);
                else
                    Debug.Console(2, this, "Command queue: Empty");

                var responseObject = Display.HandleResponse(e.Text);

                if (responseObject is SharpDisplayPluginResponse)
                {
                    var responseHandler = CommandQueue.DequeueMessage();
                    if (responseHandler == null)
                    {
                        Debug.Console(2, this, "Queue empty, nothing to process");
                        return;
                    }

                    var responseObj = responseObject as SharpDisplayPluginResponse;
                    Debug.Console(2, this, "ResponseHandler Fired: {0} - Invoking on param: {1}",
                                  responseHandler.ToString(), responseObj.ResponseString);
                    responseHandler.InvokeAction(responseObj.ResponseString);
                    var nextCommand = CommandQueue.GetNextCommand();
                    if (nextCommand != null)
                        CommandQueue.RaiseEvent_ProcessMessage(nextCommand.Command);

                    return;
                }

                if (responseObject is SharpDisplayPluginResponseOk)
                {
                    Debug.Console(2, this, "OK received");
                    return;
                }

                if (responseObject is SharpDisplayPluginResponseWait)
                {
                    Debug.Console(2, this, "Wait received");
                    return;
                }

                if (!(responseObject is SharpDisplayPluginResponseError)) return;
                Debug.Console(2, this, "Error received");
                CommandQueue.DequeueMessage();
            }
            catch (NullReferenceException ne)
            {
                Debug.Console(2, this, "Null Reference: {0}", ne.StackTrace);
            }
        }

        public void SetProtocol(string protocol)
        {
            if (!DisplayEnabled) return;
            try
            {
                Display = SharpDisplayPluginProtocolCmdStyleFactory.BuildSharpDislplay(this, protocol);
            }
            catch (NullReferenceException nEx)
            {
                Debug.Console(2, Debug.ErrorLogLevel.Error, "Invalid Protocol, please check configuration file: {0}",
                              nEx.ToString());
            }
        }

        public void OnPoll(object sender, SharpDisplayPollEventArgs e)
        {
            Debug.Console(2, this, "OnPoll {0}", e.CurrentEvent);

            switch (e.CurrentEvent)
            {
                case "power":
                    Display.FormatCommand(ECommands.Power, EPowerParams.Poll);
                    break;
                case "input":
                    Display.FormatCommand(ECommands.Input, EInputParams.Poll);
                    break;
            }
        }

        public void OnCommandProcessed(object sender, SharpDisplayMessageEventArgs e)
        {
            SendLine(e.Response);
        }


        public void OnCommandTimerCompleted(object sender, EventArgs eventArgs)
        {
            Debug.Console(2, "Device Timeout");
            CommandQueue.ClearQueue();
        }

        #region TwoWayDisplayBase Members

        protected override Func<string> CurrentInputFeedbackFunc
        {
            get { return () => Display.InputList[(ushort) InputActive].InputCode; }
        }

        protected override Func<bool> PowerIsOnFeedbackFunc
        {
            get { return () => PowerIsOn; }
        }

        protected override Func<bool> IsCoolingDownFeedbackFunc
        {
            get { return null; }
        }

        protected override Func<bool> IsWarmingUpFeedbackFunc
        {
            get { return null; }
        }

        public override void ExecuteSwitch(object selector)
        {
            Debug.Console(2, "ExecutSwitch {0}", selector.ToString());
            if (!(selector is Action))
                return;

            if (PowerIsOn)
            {
                Debug.Console(2, "ExecuteSwitch Power is On");
                (selector as Action)();
            }
            else
            {
                Debug.Console(2, "ExecuteSwitch Power is Off");
                EventHandler<FeedbackEventArgs> handler = null;
                handler = (o, a) =>
                              {
                                  if (!PowerIsOn) return;
                                  PowerIsOnFeedback.OutputChange -= handler;
                                  var action = selector as Action;
                                  if (action != null) action();
                              };
                PowerIsOnFeedback.OutputChange += handler;
                PowerOn();
            }
        }

        #endregion

        #region IBridgeAdvanced Members

        void IBridgeAdvanced.LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            Debug.Console(2, this, "Link To API");
            LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
        }

        #endregion

        #region IBridgeAdvanced Members

        #endregion

        #region Polling

        public void PollStart()
        {
            if (DisplayEnabled)
                DisplayPoll.StartPoll();
        }

        public void PollStop()
        {
            if (DisplayEnabled)
                DisplayPoll.StopPoll();
        }

        public void PollSetTime(ushort pollTime)
        {
            if (DisplayEnabled)
                DisplayPoll.PollTime = pollTime*1000;
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

        public override void PowerOn()
        {
            if (DisplayEnabled)
                Display.FormatCommand(new SharpDisplayCommand(ECommands.Power, EPowerParams.On));
        }

        public override void PowerOff()
        {
            if (DisplayEnabled)
                Display.FormatCommand(new SharpDisplayCommand(ECommands.Power, EPowerParams.Off));
        }

        public override void PowerToggle()
        {
            if (!DisplayEnabled) return;
            if (Display == null)
            {
                Debug.Console(2, this, Debug.ErrorLogLevel.Error,
                              "Display not defined: Confirm Protocol configuration");
                return;
            }

            if (PowerIsOn)
                PowerOff();
            else
                PowerOn();
        }

        public void PowerPoll()
        {
            if (DisplayEnabled)
                Display.FormatCommand(new SharpDisplayCommand(ECommands.Power, EPowerParams.Poll));
        }

        public void InputSelect(SharpDisplayCommand command)
        {
            if (DisplayEnabled)
                Display.FormatCommand(command);
        }

        public void InputPoll()
        {
            if (DisplayEnabled)
                Display.FormatCommand(new SharpDisplayCommand(ECommands.Input, EInputParams.Poll));
        }

        public void PollNext()
        {
            if (DisplayEnabled)
                DisplayPoll.PollExecute(null);
        }

        public void CommandSettingOn()
        {
            if (DisplayEnabled)
                Display.FormatCommand(new SharpDisplayCommand(ECommands.CommandSetting, EPowerParams.On));
        }

        public void ExecuteCommand(SharpDisplayCommand command)
        {
            if (!DisplayEnabled) return;
            if (CommandQueue.GetNextCommand() == null)
            {
                SendLineRaw(command.SendCommand());
                CommandTimer.StartTimer(CommandTimeOut);
            }

            Debug.Console(2, "Command Queued: {0}", command.SendCommand());
            CommandQueue.EnqueueMessage(command);

            CommandQueue.PrintQueue();
        }


        public void SetPowerFb(string param)
        {
            var powerState = false;
            if (param.CompareTo("1") > 0) return;
            Debug.Console(2, this, "Set Power FB: {0}", param);
            if (param.Contains("1"))
                powerState = true;

            PowerIsOn = powerState;
            PowerIsOnFeedback.FireUpdate();
        }

        public void SetInputFb(string param)
        {
            if (param == null) return;
            Debug.Console(2, this, "Set Input FB: {0}", param);
            try
            {
                var key =
                    Display.InputList.FirstOrDefault(o => o.Value.InputCode == param);
                Debug.Console(2, this, "InputParams: {0}, as INT: {1}", key, (int) key.Key);
                if (key.IsDefault()) return;
                InputActive = key.Key;
                InputActiveFeedback.FireUpdate();
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

            foreach (var input in Display.InputList.Where(input => input.Key > 0))
            {
                Debug.Console(0, "Input: {0}, Name: {1}, InputCode: {2}", input.Key, input.Value.Name,
                              input.Value.InputCode);
                InputLabels.Add(input.Value.Name);
            }
        }

        #endregion
    }

    public static class Extensions
    {
        public static bool IsDefault<T>(this T value) where T : struct
        {
            var isDefault = value.Equals(default(T));

            return isDefault;
        }
    }
}