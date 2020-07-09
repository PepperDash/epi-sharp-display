using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using Epi.Display.Sharp.SharpDisplayProtocolCmdStyleClasses;

namespace Epi.Display.Sharp
{
    public static class SharpDisplayPluginProtocolCmdStyleFactory
    {
        internal static SharpDisplayProtocolCmdStyleBase BuildSharpDislplay(SharpDisplayPluginDevice display, SharpDisplayPluginConfigObject displayConfig)
        {
            // Get config value for protocol style and return protocol

            switch (displayConfig.Protocol)
            {
                case ("ProtocolStyle01"):
                    return new SharpDisplayProtocolCmdStyle01(display);
                default:
                    Debug.Console(0, "No Protocol Style Exists for Type");
                    return new SharpDisplayProtocolCmdStyleNotDefined(display);
            }
        }
    }
}