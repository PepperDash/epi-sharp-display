using System;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;

using PepperDash.Essentials.Core.Bridges;


namespace Epi.Display.Sharp
{
	public static class SharpDisplayPluginBridge
	{
        

        public static void LinkToApiExt(this SharpDisplayPluginDevice device, BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            try
            {
                SharpDisplayPluginBridgeJoinMap joinMap = new SharpDisplayPluginBridgeJoinMap(joinStart);
                joinMap.Init();

                // This adds the join map to the collection on the bridge
                if (bridge != null)
                    bridge.AddJoinMap(device.Key, joinMap);

                var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);

                if (customJoins != null)
                {
                    joinMap.SetCustomJoinData(customJoins);
                }

                Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
                Debug.Console(0, "Linking to Bridge Type {0}", device.GetType().Name.ToString());


                Debug.Console(2, "Linking device functions");

                trilist.SetSigTrueAction(joinMap.PowerOn.JoinNumber, () => device.PowerOn());
                trilist.SetSigTrueAction(joinMap.PowerOff.JoinNumber, () => device.PowerOff());
                trilist.SetSigTrueAction(joinMap.PowerToggle.JoinNumber, () => device.PowerToggle());
                trilist.SetSigTrueAction(joinMap.PollStart.JoinNumber, () => device.PollStart());
                trilist.SetSigTrueAction(joinMap.PollStop.JoinNumber, () => device.PollStop());
                trilist.SetSigTrueAction(joinMap.PollNext.JoinNumber, () => device.PollNext());
                trilist.SetUShortSigAction(joinMap.Input.JoinNumber, (value) => device.SelectInput(value));
                trilist.SetUShortSigAction(joinMap.PollTime.JoinNumber, (value) => device.PollSetTime(value));
                trilist.SetStringSigAction(joinMap.Protocol.JoinNumber, (s) => device.SetProtocol(s));



                Debug.Console(2, "Linking Feedbacks");

                if (device.PollEnabledFeedback == null)
                    Debug.Console(2, "Poll Enalbed Feedback is null");
                else
                    Debug.Console(2, "Poll Enabled Feedback: {0}", device.PollEnabledFeedback.ToString());

                device.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn.JoinNumber]);
                device.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PowerOff.JoinNumber]);
                device.PollEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PollStart.JoinNumber]);
                device.PollEnabledFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PollStop.JoinNumber]);
                device.InputActiveFeedback.LinkInputSig(trilist.UShortInput[joinMap.Input.JoinNumber]);


                device.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.OnlineFb.JoinNumber]);



                Debug.Console(2, "Linking Complete");
            }

            catch (NullReferenceException nEx)
            {
                Debug.Console(2, Debug.ErrorLogLevel.Error, "Error linking Bridge. Device is Null: {0}", nEx.ToString());
            }

        }

        public static void UpdateBridge(this SharpDisplayPluginDevice device, BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            SharpDisplayPluginBridgeJoinMap joinMap = new SharpDisplayPluginBridgeJoinMap(joinStart);
            joinMap.Init();

            trilist.SetString(joinMap.DeviceName.JoinNumber, device.Name);
            for (uint join = 0; join <= joinMap.InputLabels.JoinSpan; join++)
            {
                trilist.SetString(joinMap.InputLabels.JoinNumber + join, device.InputLabels[(int)join]);
            }
        }
    }


	public class SharpDisplayPluginBridgeJoinMap : JoinMapBaseAdvanced
	{
        [JoinName("deviceName")]
        public JoinDataComplete DeviceName = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Label = "Device Name",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("powerOn")]
        public JoinDataComplete PowerOn = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Label = "Power On",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("powerOf")]
        public JoinDataComplete PowerOff = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata
            {
                Label = "Power Off",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("powerToggle")]
        public JoinDataComplete PowerToggle = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata
            {
                Label = "Power Toggle",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("pollStart")]
        public JoinDataComplete PollStart = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata
            {
                Label = "Poll Start",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("pollStop")]
        public JoinDataComplete PollStop = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata
            {
                Label = "Poll Stop",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("pollNext")]
        public JoinDataComplete PollNext = new JoinDataComplete(new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata
            {
                Label = "Poll Next",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("OnlineFb")]
        public JoinDataComplete OnlineFb = new JoinDataComplete(new JoinData { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata
            {
                Label = "OnlineFb",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("input")]
        public JoinDataComplete Input = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Label = "Input",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Analog
            });

        [JoinName("pollTime")]
        public JoinDataComplete PollTime = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata
            {
                Label = "Poll Time",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Analog
            });

        [JoinName("protocol")]
        public JoinDataComplete Protocol = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Label = "Protocol",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("inputLabels")]
        public JoinDataComplete InputLabels = new JoinDataComplete(new JoinData { JoinNumber = 7, JoinSpan = 10 },
            new JoinMetadata
            {
                Label = "Input Labels",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });



   
		public SharpDisplayPluginBridgeJoinMap(uint joinStart) 
            :base(joinStart)
		{
		}

        public void Init()
        {
            // This will add the joins defined above
            AddJoins(typeof(SharpDisplayPluginBridgeJoinMap));
        }

	}
}