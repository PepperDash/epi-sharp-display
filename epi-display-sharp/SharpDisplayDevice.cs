using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Queues;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Displays;

namespace PepperDash.Plugins.SharpDisplay
{
	public class SharpDisplayController : TwoWayDisplayBase, IBasicVolumeWithFeedback, ICommunicationMonitor, 
		IInputHdmi1, IInputHdmi2, IInputHdmi3, IInputDisplayPort1, IInputVga1,
		IBridgeAdvanced
	{

		GenericQueue ReceiveQueue;

		public const int InputPowerOn = 101;

		public const int InputPowerOff = 102;
		public static List<string> InputKeys = new List<string>();
		public List<BoolFeedback> InputFeedback;
		public IntFeedback InputNumberFeedback;
		private RoutingInputPort _currentInputPort;
		private List<bool> _inputFeedback;
		private int _inputNumber;
		private bool _isCoolingDown;
		private bool _isMuted;
		private bool _isSerialComm;
		private bool _isWarmingUp;
		private bool _lastCommandSentWasVolume;
		private int _lastVolumeSent;
		private CTimer _pollRing;
		private bool _powerIsOn;
		private ActionIncrementer _volumeIncrementer;
		private bool _volumeIsRamping;
		private ushort _volumeLevelForSig;
		private bool _pollVolume;
		private string _lastCommandSent;

		public SharpDisplayController(string key, string name, SharpDisplayPropertiesConfig config, IBasicCommunication comms)
			: base(key, name)
		{
			Communication = comms;

			ReceiveQueue = new GenericQueue(key + "-queue");

			var props = config;
			if (props == null)
			{
				Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Display configuration must be included");
				return;
			}
			ZeroPadCommands = props.ZeroPadCommands;
			_upperLimit = props.volumeUpperLimit;
			_lowerLimit = props.VolumeLowerLimit;
			_pollIntervalMs = props.PollIntervalMs > 30000 ? props.PollIntervalMs : 30000;
			_coolingTimeMs = props.CoolingTimeMs > 9999 ? props.CoolingTimeMs : 15000;
			_warmingTimeMs = props.WarmingTimeMs > 9999 ? props.WarmingTimeMs : 15000;

			_pollVolume = props.PollVolume;

			InputNumberFeedback = new IntFeedback(() => _inputNumber);

			Init();
		}

		public IBasicCommunication Communication { get; private set; }
		public CommunicationGather PortGather { get; private set; }

		public bool ZeroPadCommands { get; private set; }

		public bool PowerIsOn
		{
			get { return _powerIsOn; }
			set
			{
				if (_powerIsOn == value)
				{
					return;
				}

				_powerIsOn = value;

				if (_powerIsOn)
				{
					IsWarmingUp = true;

					WarmupTimer = new CTimer(o =>
					{
						IsWarmingUp = false;
					}, WarmupTime);
				}
				else
				{
					IsCoolingDown = true;

					CooldownTimer = new CTimer(o =>
					{
						IsCoolingDown = false;
					}, CooldownTime);
				}

				PowerIsOnFeedback.FireUpdate();
			}
		}

		public bool IsWarmingUp
		{
			get { return _isWarmingUp; }
			set
			{
				_isWarmingUp = value;
				IsWarmingUpFeedback.FireUpdate();
			}
		}

		public bool IsCoolingDown
		{
			get { return _isCoolingDown; }
			set
			{
				_isCoolingDown = value;
				IsCoolingDownFeedback.FireUpdate();
			}
		}

		public bool IsMuted
		{
			get { return _isMuted; }
			set
			{
				_isMuted = value;
				MuteFeedback.FireUpdate();
			}
		}

		private readonly int _lowerLimit;
		private readonly int _upperLimit;
		private readonly uint _coolingTimeMs;
		private readonly uint _warmingTimeMs;
		private readonly long _pollIntervalMs;

		public int InputNumber
		{
			get { return _inputNumber; }
			private set
			{
				if (_inputNumber == value) return;

				_inputNumber = value;
				InputNumberFeedback.FireUpdate();
				UpdateBooleanFeedback(value);
			}
		}

		private bool ScaleVolume { get; set; }

		protected override Func<bool> PowerIsOnFeedbackFunc
		{
			get { return () => PowerIsOn; }
		}

		protected override Func<bool> IsCoolingDownFeedbackFunc
		{
			get { return () => IsCoolingDown; }
		}

		protected override Func<bool> IsWarmingUpFeedbackFunc
		{
			get { return () => IsWarmingUp; }
		}

		protected override Func<string> CurrentInputFeedbackFunc
		{
			get { return () => _currentInputPort.Key; }
		}

		#region IBasicVolumeWithFeedback Members

		/// <summary>
		/// Volume Level Feedback Property
		/// </summary>
		public IntFeedback VolumeLevelFeedback { get; private set; }

		/// <summary>
		/// Volume Mute Feedback Property
		/// </summary>
		public BoolFeedback MuteFeedback { get; private set; }

		/// <summary>
		/// Scales the level to the range of the display and sends the command
		/// Set: "kf [SetID] [Range 0x00 - 0x64]"
		/// </summary>
		/// <param name="level"></param>
		public void SetVolume(ushort level)
		{
			int scaled;
			_lastVolumeSent = level;
			if (!ScaleVolume)
			{
				scaled = (int)NumericalHelpers.Scale(level, 0, 65535, 0, 100);
			}
			else
			{
				scaled = (int)NumericalHelpers.Scale(level, 0, 65535, _lowerLimit, _upperLimit);
			}

			SendData("VOLM", scaled.ToString(CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Set Mute On
		/// </summary>
		public void MuteOn()
		{
			SendData("MUTE", "1");
		}

		/// <summary>
		/// Set Mute Off
		/// </summary>
		public void MuteOff()
		{
			SendData("MUTE", "0");
		}

		/// <summary>
		/// Toggle Current Mute State
		/// </summary>
		public void MuteToggle()
		{
			if (IsMuted)
			{
				MuteOff();
			}
			else
			{
				MuteOn();
			}
		}

		/// <summary>
		/// Decrement Volume on Press
		/// </summary>
		/// <param name="pressRelease"></param>
		public void VolumeDown(bool pressRelease)
		{
			if (pressRelease)
			{
				_volumeIncrementer.StartDown();
				_volumeIsRamping = true;
			}
			else
			{
				_volumeIsRamping = false;
				_volumeIncrementer.Stop();
			}
		}

		/// <summary>
		/// Increment Volume on press
		/// </summary>
		/// <param name="pressRelease"></param>
		public void VolumeUp(bool pressRelease)
		{
			if (pressRelease)
			{
				_volumeIncrementer.StartUp();
				_volumeIsRamping = true;
			}
			else
			{
				_volumeIsRamping = false;
				_volumeIncrementer.Stop();
			}
		}

		#endregion

		#region IBridgeAdvanced Members

		public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
		}

		#endregion

		#region ICommunicationMonitor Members

		public StatusMonitorBase CommunicationMonitor { get; private set; }

		#endregion

		private void Init()
		{
			WarmupTime = _warmingTimeMs > 0 ? _warmingTimeMs : 15000;
			CooldownTime = _coolingTimeMs > 0 ? _coolingTimeMs : 15000;

			_inputFeedback = new List<bool>();
			InputFeedback = new List<BoolFeedback>();

			if (_upperLimit != _lowerLimit && _upperLimit > _lowerLimit)
			{
				ScaleVolume = true;
			}

			PortGather = new CommunicationGather(Communication, "\x0D");
			PortGather.LineReceived += PortGather_LineReceived;

			var socket = Communication as ISocketStatus;
			if (socket != null)
			{
				//This Instance Uses IP Control
				Debug.Console(2, this, "The Sharp Display Plugin does NOT support IP Control currently");
			}
			else
			{
				// This instance uses RS-232 Control
				_isSerialComm = true;
			}

			var pollInterval = _pollIntervalMs > 0 ? _pollIntervalMs : 45000;
			CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, pollInterval, 180000, 300000,
				StatusGet);
			CommunicationMonitor.StatusChange += CommunicationMonitor_StatusChange;

			DeviceManager.AddDevice(CommunicationMonitor);

			if (!ScaleVolume)
			{
				_volumeIncrementer = new ActionIncrementer(655, 0, 65535, 800, 80,
					v => SetVolume((ushort)v),
					() => _lastVolumeSent);
			}
			else
			{
				var scaleUpper = NumericalHelpers.Scale(_upperLimit, 0, 100, 0, 65535);
				var scaleLower = NumericalHelpers.Scale(_lowerLimit, 0, 100, 0, 65535);

				_volumeIncrementer = new ActionIncrementer(655, (int)scaleLower, (int)scaleUpper, 800, 80,
					v => SetVolume((ushort)v),
					() => _lastVolumeSent);
			}

			MuteFeedback = new BoolFeedback(() => IsMuted);
			VolumeLevelFeedback = new IntFeedback(() => _volumeLevelForSig);

			AddRoutingInputPort(
				new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
					eRoutingPortConnectionType.Hdmi, new Action(InputHdmi1), this), "9");
			AddRoutingInputPort(
				new RoutingInputPort(RoutingPortNames.HdmiIn1PC, eRoutingSignalType.Audio | eRoutingSignalType.Video,
					eRoutingPortConnectionType.Hdmi, new Action(InputHdmi2), this), "10");

			AddRoutingInputPort(
				new RoutingInputPort(RoutingPortNames.DviIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
					eRoutingPortConnectionType.Dvi, new Action(InputDvi1), this), "7");

			AddRoutingInputPort(
				new RoutingInputPort(RoutingPortNames.VgaIn1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
					eRoutingPortConnectionType.Vga, new Action(InputVga1), this), "2");
		}

		public override bool CustomActivate()
		{
			Communication.Connect();

			if (_isSerialComm)
			{
				CommunicationMonitor.Start();
			}

			return base.CustomActivate();
		}

		private void CommunicationMonitor_StatusChange(object sender, MonitorStatusChangeEventArgs e)
		{
			CommunicationMonitor.IsOnlineFeedback.FireUpdate();
		}

		private void PortGather_LineReceived(object sender, GenericCommMethodReceiveTextArgs args)
		{
			ReceiveQueue.Enqueue(new ProcessStringMessage(args.Text, ProcessResponse));
		}

		private void ProcessResponse(string s)
		{
			// ex poll tx/rx
			// Tx: "POWR   ?\x0D\x0A"
			// Rx: "{POWR_STATE:0,1} 001"
			// ex command tx/rx
			// Tx: "POWR   1\x0D\x0A"
			// Rx: "OK 001"

			// get current last command in case the string has changed
			var last = _lastCommandSent;

			Debug.Console(1, this, "ProcessResponse: {0} | last: {1}", s, last);
			var data = s.Trim().Split(' ');

			//Debug.Console(2, this, "ProcessResponse data.Count(): {0}", data.Count());
			//for (var i = 0; i < data.Count(); i++)
			//    Debug.Console(2, this, "ProcessResponse data[{0}]: {1}", i, data[i]);

			if (data[0].Contains("ERR") || data[0].Contains("WAIT"))
			{
				Debug.Console(2, this, "ProcessResponse data[0] = {0}, exiting", data[0]);
				return;
			}

			switch (last)
			{
				case "POWR":
					{
						if (data[0].Contains("OK"))
							PowerGet();
						else
							UpdatePowerFb(data[0]);
						break;
					}
				case "INPS":
					{
						if (data.Contains("OK"))
							InputGet();
						else
							UpdateInputFb(data[0]);
						break;
					}
				case "VOLM":
					{
						if (data.Contains("OK"))
							VolumeGet();
						else
							UpdateVolumeFb(data[0]);
						break;
					}
				case "MUTE":
					{
						if (data.Contains("OK"))
							MuteGet();
						else
							UpdateMuteFb(data[0]);
						break;
					}
			}
		}


		/// <summary>
		/// Formats an outgoing message
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="cmdValue"></param>
		private void SendData(string cmd, string cmdValue)
		{
			if (string.IsNullOrEmpty(cmd)) return;

			if (string.IsNullOrEmpty(cmdValue))
				cmdValue = "?";

			var data = ZeroPadCommands ?
				string.Format("{0}{1,4:0}", cmd, cmdValue) :
				string.Format("{0}{1,4:#}", cmd, cmdValue);

			Communication.SendText(data + "\x0D\x0A");

			_lastCommandSent = cmd;
			Debug.Console(1, this, "SendData _lastCommandSent: {0}", _lastCommandSent);
		}

		/// <summary>
		/// Executes a switch, turning on display if necessary.
		/// </summary>
		/// <param name="selector"></param>
		public override void ExecuteSwitch(object selector)
		{
			//if (!(selector is Action))
			//    Debug.Console(1, this, "WARNING: ExecuteSwitch cannot handle type {0}", selector.GetType());

			if (PowerIsOn)
			{
				var action = selector as Action;
				if (action != null)
				{
					action();
				}
			}
			else // if power is off, wait until we get on FB to send it. 
			{
				// One-time event handler to wait for power on before executing switch
				EventHandler<FeedbackEventArgs> handler = null; // necessary to allow reference inside lambda to handler
				handler = (o, a) =>
				{
					if (_isWarmingUp)
					{
						return;
					}

					IsWarmingUpFeedback.OutputChange -= handler;

					var action = selector as Action;
					if (action != null)
					{
						action();
					}
				};
				IsWarmingUpFeedback.OutputChange += handler; // attach and wait for on FB
				PowerOn();
			}
		}

		#region Volume

		/// <summary>
		/// Poll Mute State
		/// </summary>
		public void MuteGet()
		{
			SendData("MUTE", "?");
		}

		/// <summary>
		/// Poll Volume
		/// </summary>
		public void VolumeGet()
		{
			SendData("VOLM", "?");
		}

		/// <summary>
		/// Process Volume Feedback from Response
		/// </summary>
		/// <param name="s">response from device</param>
		public void UpdateVolumeFb(string s)
		{
			try
			{
				ushort newVol;
				if (!ScaleVolume)
				{
					newVol = (ushort)NumericalHelpers.Scale(Convert.ToDouble(s), 0, 100, 0, 65535);
				}
				else
				{
					newVol = (ushort)NumericalHelpers.Scale(Convert.ToDouble(s), _lowerLimit, _upperLimit, 0, 65535);
				}
				if (!_volumeIsRamping)
				{
					_lastVolumeSent = newVol;
				}

				if (newVol == _volumeLevelForSig)
				{
					return;
				}
				_volumeLevelForSig = newVol;
				VolumeLevelFeedback.FireUpdate();
			}
			catch (Exception e)
			{
				Debug.Console(2, this, "Error updating volumefb for value: {1}: {0}", e, s);
			}
		}

		/// <summary>
		/// Process Mute Feedback from Response
		/// </summary>
		/// <param name="s">response from device</param>
		public void UpdateMuteFb(string s)
		{
			try
			{
				var state = Convert.ToInt32(s);

				if (state == 0)
				{
					IsMuted = true;
				}
				else if (state == 1)
				{
					IsMuted = false;
				}
			}
			catch (Exception e)
			{
				Debug.Console(2, this, "Unable to parse {0} to Int32 {1}", s, e);
			}
		}

		#endregion

		#region Inputs

		private void AddRoutingInputPort(RoutingInputPort port, string fbMatch)
		{
			port.FeedbackMatchObject = fbMatch;
			InputPorts.Add(port);
		}

		public void ListRoutingInputPorts()
		{
			foreach (var inputPort in InputPorts)
			{
				Debug.Console(0, this, "inputPort key: {0}, connectionType: {1}, feedbackMatchObject: {2}",
					inputPort.Key, inputPort.ConnectionType, inputPort.FeedbackMatchObject);
			}
		}

		/// <summary>
		/// Select Hdmi 1 Input (AV HDMI)
		/// </summary>
		public void InputHdmi1()
		{
			SendData("INPS", "9");
		}

		/// <summary>
		/// Select Hdmi 2 Input (PC HDMI)
		/// </summary>
		public void InputHdmi2()
		{
			SendData("INPS", "10");
		}

		/// <summary>
		/// Select Hdmi 3 (AV DVI-D)
		/// </summary>
		public void InputHdmi3()
		{
			SendData("INPS", "7");
		}

		/// <summary>
		/// Select display port 1 (PC DVI-D)
		/// </summary>
		public void InputDisplayPort1()
		{
			SendData("INPS", "1");
		}

		/// <summary>
		/// Select DVI 1 Input (AV)
		/// </summary>
		public void InputDvi1()
		{
			SendData("INPS", "7");
		}

		/// <summary>
		/// Select DVI 2 Input (PC DVI-D)
		/// </summary>
		public void InputDvi2()
		{
			SendData("INPS", "1");
		}

		/// <summary>
		/// Select VGA 1 Input (PC D-Sub)
		/// </summary>
		public void InputVga1()
		{
			SendData("INPS","2");
		}

		/// <summary>
		/// Toggles the display input
		/// </summary>
		public void InputToggle()
		{
			SendData("INPS", "0");
		}

		/// <summary>
		/// Poll input
		/// </summary>
		public void InputGet()
		{
			SendData("INPS", "?");
		}

		/// <summary>
		/// Process Input Feedback from Response
		/// </summary>
		/// <param name="s">response from device</param>
		public void UpdateInputFb(string s)
		{
			var newInput = InputPorts.FirstOrDefault(i => i.FeedbackMatchObject.Equals(s.ToLower()));
			if (newInput == null) return;
			if (newInput == _currentInputPort)
			{
				Debug.Console(1, this, "UpdateInputFb _currentInputPort ({0}) == newInput ({1})", _currentInputPort.Key, newInput.Key);
				return;
			}

			Debug.Console(1, this, "UpdateInputFb newInput key: {0}, connectionType: {1}, feedbackMatchObject: {2}",
				newInput.Key, newInput.ConnectionType, newInput.FeedbackMatchObject);

			_currentInputPort = newInput;
			CurrentInputFeedback.FireUpdate();

			var key = newInput.Key;
			Debug.Console(1, this, "UpdateInputFb key: {0}", key);
			switch (key)
			{
				case "hdmiIn1":
					InputNumber = 1;
					break;
				case "hdmiIn2":
					InputNumber = 2;
					break;
				case "dvi":
					InputNumber = 3;
					break;
				case "dvi1":
					InputNumber = 5;
					break;
				case "vga1":
					InputNumber = 4;
					break;
			}
		}

		/// <summary>
		/// Updates Digital Route Feedback for Simpl EISC
		/// </summary>
		/// <param name="data">currently routed source</param>
		private void UpdateBooleanFeedback(int data)
		{
			try
			{
				if (_inputFeedback[data])
				{
					return;
				}

				for (var i = 1; i < InputPorts.Count + 1; i++)
				{
					_inputFeedback[i] = false;
				}

				_inputFeedback[data] = true;
				foreach (var item in InputFeedback)
				{
					var update = item;
					update.FireUpdate();
				}
			}
			catch (Exception e)
			{
				Debug.Console(0, this, "{0}", e.Message);
			}
		}

		#endregion

		#region Power

		/// <summary>
		/// Set Power On For Device
		/// </summary>
		public override void PowerOn()
		{
			if (_isSerialComm)
			{
				SendData("POWR", "1");
			}
		}

		/// <summary>
		/// Set Power Off for Device
		/// </summary>
		public override void PowerOff()
		{
			SendData("POWR", "0");
		}

		/// <summary>
		/// Poll Power
		/// </summary>
		public void PowerGet()
		{
			SendData("POWR", "?");
		}


		/// <summary>
		/// Toggle current power state for device
		/// </summary>
		public override void PowerToggle()
		{
			if (PowerIsOn)
			{
				PowerOff();
			}
			else
			{
				PowerOn();
			}
		}

		/// <summary>
		/// Process Power Feedback from Response
		/// </summary>
		/// <param name="s">response from device</param>
		public void UpdatePowerFb(string s)
		{
			PowerIsOn = s.Contains("1");

		}

		#endregion

		/// <summary>
		/// Starts the Poll Ring
		/// </summary>
		public void StatusGet()
		{
			CrestronInvoke.BeginInvoke((o) =>
			{
				PowerGet();
				CrestronEnvironment.Sleep(2000);
				InputGet();

				if (!_pollVolume) return;

				CrestronEnvironment.Sleep(2000);
				VolumeGet();
				CrestronEnvironment.Sleep(2000);
				MuteGet();
			});
		}
	}
}