using System.Collections.Generic;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Plugins.SharpDisplay
{
    public class SharpDisplayControllerFactory:EssentialsPluginDeviceFactory<SharpDisplayController>
    {
        public SharpDisplayControllerFactory()
        {
            TypeNames = new List<string> {"SharpDisplay", "SharpPlugin", "Sharp"};

            MinimumEssentialsFrameworkVersion = "1.9.1";
        }

        #region Overrides of EssentialsDeviceFactory<SharpDisplayController>

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var comms = CommFactory.CreateCommForDevice(dc);

            if (comms == null) return null;

            var config = dc.Properties.ToObject<SharpDisplayPropertiesConfig>();

            return config == null ? null : new SharpDisplayController(dc.Key, dc.Name, config, comms);
        }

        #endregion
    }
}