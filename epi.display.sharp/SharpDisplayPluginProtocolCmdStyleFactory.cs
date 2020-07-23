using Epi.Display.Sharp.SharpDisplayProtocolCmdStyleClasses;
using PepperDash.Core;

namespace Epi.Display.Sharp
{
    public static class SharpDisplayPluginProtocolCmdStyleFactory
    {
        internal static SharpDisplayProtocolCmdStyleBase BuildSharpDislplay(SharpDisplayPluginDevice display, string displayConfig)
        {
            // Get config value for protocol style and return protocol

            switch (displayConfig)
            {
                case "1":
                    return new SharpDisplayProtocolCmdStyle01(display);
                case "2":
                    return new SharpDisplayProtocolCmdStyle02(display);
                case "3":
                    return new SharpDisplayProtocolCmdStyle03(display);
                case "4":
                    return new SharpDisplayProtocolCmdStyle04(display);
                case "5":
                    return new SharpDisplayProtocolCmdStyle05(display);
                case "PN-UH501":
                    return new SharpDisplayProtocolCmdStyle_PN_UH501(display);
                case "PN-UH701":
                    return new SharpDisplayProtocolCmdStyle_PN_UH701(display);
                default:
                    Debug.Console(0, "No Protocol Style Exists for Type");
                    return new SharpDisplayProtocolCmdStyleNotDefined(display);
            }
        }
    }
}