// For Basic SIMPL#Pro classes

// For Basic SIMPL# Classes

using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Sharp.CommandTimer;
using PepperDash.Essentials.Sharp.DisplayEventArgs;
using PepperDash.Essentials.Sharp.SharpDisplayProtocolCmdStyleClasses;
using PepperDash.Essentials.Sharp.SharpPluginQueue;

namespace PepperDash.Essentials.Sharp
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
        private readonly bool _displayEnabled;
        private readonly SharpDisplayPluginPoll _displayPoll;
        private readonly string _delimiter;
        public List<string> InputLabels;
        private SharpDisplayPluginQueue _commandQueue;
        private SharpCommandTimer _commandTimer;
        private SharpDisplayProtocolCmdStyleBase _display;
        private int _inputActive;
        private bool _powerIsOn;

        public SharpDisplayPluginDevice(string key, string name, SharpDisplayPluginConfigObject config,
            IBasicCommunication comms)
            : base(key, name)
        {
            var deviceConfiguration = config;

            Communication = comms;

            try
            {
                _display = SharpDisplayPluginProtocolCmdStyleFactory.BuildSharpDislplay(this,
                    deviceConfiguration.Protocol);
                // A valid protocol retreived.
                Debug.Console(2, this, "Added Display Name: {0}, Type: {1}, Comm: {2}", deviceConfiguration.Name,
                    _display.GetType().ToString(), Communication.GetType().ToString());

                _delimiter = _display.Delimiter;
                PortGather = new CommunicationGather(Communication, _delimiter);
                PortGather.LineReceived += PortGather_LineReceived;

                _displayEnabled = deviceConfiguration.Enabled;

                var checkCommunicationType = Communication as ISocketStatus;
                CommMethod = checkCommunicationType == null ? ECommMethod.Rs232 : ECommMethod.Lan;


                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 20000, 120000, 300000,
                    _display.PollString);
                DeviceManager.AddDevice(CommunicationMonitor);
                CommunicationMonitor.Start();

                // Attach to display feedbacks here
                PowerIsOnFeedback = new BoolFeedback(() => PowerIsOn);
                InputActiveFeedback = new IntFeedback(() => InputActive);
                InputActiveNameFeedback = new StringFeedback(() => InputName);
                StatusFeedback = new IntFeedback(() => (int) CommunicationMonitor.Status);


                // Set up polling
                _displayPoll = new SharpDisplayPluginPoll(60000);

                PollEnabledFeedback = _displayPoll.PollEnabledFeedback;


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

        protected override Func<string> CurrentInputFeedbackFunc
        {
            get { return () => _display.InputList[(ushort) InputActive].InputCode; }
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

        #region IBridgeAdvanced Members

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            Debug.Console(2, this, "Link To API");
            LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
        }

        #endregion

        #region Polling

        public void PollStart()
        {
            if (_displayEnabled)
            {
                _displayPoll.StartPoll();
            }
        }

        public void PollStop()
        {
            if (_displayEnabled)
            {
                _displayPoll.StopPoll();
            }
        }

        public void PollSetTime(ushort pollTime)
        {
            if (_displayEnabled)
            {
                _displayPoll.PollTime = pollTime*1000;
            }
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
            if (_displayEnabled)
            {
                _display.FormatCommand(new SharpDisplayCommand(ECommands.Power, EPowerParams.On));
            }
        }

        public override void PowerOff()
        {
            if (_displayEnabled)
            {
                _display.FormatCommand(new SharpDisplayCommand(ECommands.Power, EPowerParams.Off));
            }
        }

        public override void PowerToggle()
        {
            if (!_displayEnabled)
            {
                return;
            }
            if (_display == null)
            {
                Debug.Console(2, this, Debug.ErrorLogLevel.Error,
                    "Display not defined: Confirm Protocol configuration");
                return;
            }

            if (PowerIsOn)
            {
                PowerOff();
            }
            else
            {
                PowerOn();
            }
        }

        public void PowerPoll()
        {
            if (_displayEnabled)
            {
                _display.FormatCommand(new SharpDisplayCommand(ECommands.Power, EPowerParams.Poll));
            }
        }

        public void InputSelect(SharpDisplayCommand command)
        {
            if (_displayEnabled)
            {
                _display.FormatCommand(command);
            }
        }

        public void InputPoll()
        {
            if (_displayEnabled)
            {
                _display.FormatCommand(new SharpDisplayCommand(ECommands.Input, EInputParams.Poll));
            }
        }

        public void PollNext()
        {
            if (_displayEnabled)
            {
                _displayPoll.PollExecute(null);
            }
        }

        public void CommandSettingOn()
        {
            if (_displayEnabled)
            {
                _display.FormatCommand(new SharpDisplayCommand(ECommands.CommandSetting, EPowerParams.On));
            }
        }

        public void ExecuteCommand(SharpDisplayCommand command)
        {
            if (!_displayEnabled)
            {
                return;
            }
            if (_commandQueue.GetNextCommand() == null)
            {
                SendLineRaw(command.SendCommand());
                _commandTimer.StartTimer(CommandTimeOut);
            }

            Debug.Console(2, "Command Queued: {0}", command.SendCommand());
            _commandQueue.EnqueueMessage(command);

            _commandQueue.PrintQueue();
        }


        public void SetPowerFb(string param)
        {
            var powerState = false;
            if (String.Compare(param, "1", StringComparison.Ordinal) > 0)
            {
                return;
            }
            Debug.Console(2, this, "Set Power FB: {0}", param);
            if (param.Contains("1"))
            {
                powerState = true;
            }

            PowerIsOn = powerState;
            PowerIsOnFeedback.FireUpdate();
        }

        public void SetInputFb(string param)
        {
            if (param == null)
            {
                return;
            }
            Debug.Console(2, this, "Set Input FB: {0}", param);
            try
            {
                var key =
                    _display.InputList.FirstOrDefault(o => o.Value.InputCode == param);
                Debug.Console(2, this, "InputParams: {0}, as INT: {1}", key, (int) key.Key);
                if (key.IsDefault())
                {
                    return;
                }
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
            if (_display == null)
            {
                return;
            }

            foreach (var input in _display.InputList.Where(input => input.Key > 0))
            {
                Debug.Console(0, "Input: {0}, Name: {1}, InputCode: {2}", input.Key, input.Value.Name,
                    input.Value.InputCode);
                InputLabels.Add(input.Value.Name);
            }
        }

        #endregion

        #region ICommunicationMonitor Members

        public StatusMonitorBase CommunicationMonitor { get; private set; }

        #endregion

        public void Initialize()
        {
            Debug.Console(2, this, "Initializing");

            //InputLabels = _display.GetInputs();
            // Subscribe to poll event

            _displayPoll.Poll += OnPoll;

            //Set Up Command Queue
            _commandQueue = new SharpDisplayPluginQueue();
            _commandTimer = new SharpCommandTimer();

            //Set up Command Queue processed event
            _commandQueue.MessageProcessed += OnCommandProcessed;
            _commandTimer.TimerCompleted += OnCommandTimerCompleted;
        }

        private void PortGather_LineReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            try
            {
                //Send feedback to display protocol style for parsing - check if a response value was received and execute command action

                _commandQueue.PrintQueue();
                _commandTimer.StartTimer(CommandTimeOut);

                if (_commandQueue.GetNextCommand() != null)
                {
                    Debug.Console(2, this, "Command in queue: {0}", _commandQueue.GetNextCommand().Command);
                }
                else
                {
                    Debug.Console(2, this, "Command queue: Empty");
                }

                var responseObject = _display.HandleResponse(e.Text);

                if (responseObject is SharpDisplayPluginResponse)
                {
                    var responseHandler = _commandQueue.DequeueMessage();
                    if (responseHandler == null)
                    {
                        Debug.Console(2, this, "Queue empty, nothing to process");
                        return;
                    }

                    var responseObj = responseObject as SharpDisplayPluginResponse;
                    Debug.Console(2, this, "ResponseHandler Fired: {0} - Invoking on param: {1}",
                        responseHandler.ToString(), responseObj.ResponseString);
                    responseHandler.InvokeAction(responseObj.ResponseString);
                    var nextCommand = _commandQueue.GetNextCommand();
                    if (nextCommand != null)
                    {
                        _commandQueue.RaiseEvent_ProcessMessage(nextCommand.Command);
                    }

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

                if (!(responseObject is SharpDisplayPluginResponseError))
                {
                    return;
                }
                Debug.Console(2, this, "Error received");
                _commandQueue.DequeueMessage();
            }
            catch (NullReferenceException ne)
            {
                Debug.Console(2, this, "Null Reference: {0}", ne.StackTrace);
            }
        }

        public void SetProtocol(string protocol)
        {
            if (!_displayEnabled)
            {
                return;
            }
            try
            {
                _display = SharpDisplayPluginProtocolCmdStyleFactory.BuildSharpDislplay(this, protocol);
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
                    _display.FormatCommand(ECommands.Power, EPowerParams.Poll);
                    break;
                case "input":
                    _display.FormatCommand(ECommands.Input, EInputParams.Poll);
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
            _commandQueue.ClearQueue();
        }

        public override void ExecuteSwitch(object selector)
        {
            Debug.Console(2, "ExecuteSwitch {0}", selector.ToString());
            if (!(selector is Action))
            {
                return;
            }

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
                    if (!PowerIsOn)
                    {
                        return;
                    }
                    PowerIsOnFeedback.OutputChange -= handler;
                    var action = selector as Action;
                    if (action != null)
                    {
                        action();
                    }
                };
                PowerIsOnFeedback.OutputChange += handler;
                PowerOn();
            }
        }
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