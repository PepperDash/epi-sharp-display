using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Epi.Display.Sharp.SharpDisplayPluginDeviceClasses;

namespace Epi.Display.Sharp
{
    public static class SharpDisplayPluginProtocolStyleFactory
    {
        internal static SharpDisplayBase BuildSharpDislplay(SharpDisplayPluginDevice display, SharpDisplayPluginConfigObject displayConfig)
        {
            // Get config value for protocol style and return protocol
            if (displayConfig.Protocol == "ProtocolStyle01")
                return new SharpDisplayProtocolStyle01(display, displayConfig);
            if (displayConfig.Protocol == "ProtocolStyle02")
                return new SharpDisplayProtocolStyle02(display, displayConfig);
            if (displayConfig.Protocol == "ProtocolStyle03")
                return new SharpDisplayProtocolStyle03(display, displayConfig);
            if (displayConfig.Protocol == "ProtocolStyle04")
                return new SharpDisplayProtocolStyle04(display, displayConfig);
            return null;
        }
    }
}