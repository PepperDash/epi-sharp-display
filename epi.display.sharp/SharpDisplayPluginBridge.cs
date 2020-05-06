using System;
using System.Linq;
using System.Text.RegularExpressions;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Bridges;
using PepperDash.Essentials.Core;

using Epi.Display.Sharp.SharpDisplayPluginDeviceClasses;


namespace Epi.Display.Sharp
{
	public static class SharpDisplayPluginBridge
	{
		public static void LinkToApiExt(this SharpDisplayPluginDevice device, BasicTriList trilist, uint joinStart, string joinMapKey, EiscApi bridge)
		{
			SharpDisplayPluginBridgeJoinMap joinMap = new SharpDisplayPluginBridgeJoinMap(joinStart);
            joinMap.Init();

            // This adds the join map to the collection on the bridge
            bridge.AddJoinMap(device.Key, joinMap);

            var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);

            if (customJoins != null)
            {
                joinMap.SetCustomJoinData(customJoins);
            }

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to Bridge Type {0}", device.GetType().Name.ToString());


            trilist.OnlineStatusChange += new Crestron.SimplSharpPro.OnlineStatusChangeEventHandler((o, a) =>
            {
                if (a.DeviceOnLine)
                {
                    trilist.SetString(joinMap.DeviceName.JoinNumber, device.Name);
                }
            });

            Debug.Console(2, "Linking device functions");

            trilist.SetSigTrueAction(joinMap.PowerOn.JoinNumber, () => device.PowerOn());
            trilist.SetSigTrueAction(joinMap.PowerOff.JoinNumber, () => device.PowerOff());
            trilist.SetSigTrueAction(joinMap.PowerToggle.JoinNumber, () => device.PowerToggle());
            trilist.SetUShortSigAction(joinMap.Input.JoinNumber, (value) => device.SelectInput(value));

            Debug.Console(2, "Linking Feedbacks");
            device.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn.JoinNumber]);
            device.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PowerOff.JoinNumber]);
            device.InputActiveFeedback.LinkInputSig(trilist.UShortInput[joinMap.Input.JoinNumber]);

            Debug.Console(2, "Linking Complete");
            
         
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

        [JoinName("input")]
        public JoinDataComplete Input = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Label = "Input",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Analog
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