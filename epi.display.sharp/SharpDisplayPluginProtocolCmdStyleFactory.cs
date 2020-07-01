using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Epi.Display.Sharp.SharpDisplayProtocolCmdStyleClasses;

namespace Epi.Display.Sharp
{
    public static class SharpDisplayPluginProtocolCmdStyleFactory
    {
        internal static SharpDisplayProtocolCmdStyleBase BuildSharpDislplay(SharpDisplayPluginDevice display, SharpDisplayPluginConfigObject displayConfig)
        {
            // Get config value for protocol style and return protocol
            if (displayConfig.Protocol == "ProtocolStyle01")
                return new SharpDisplayProtocolCmdStyle01();

            return null;
        }
    }
}