using PepperDash.Core;
using PepperDash.Essentials.Sharp.SharpDisplayProtocolCmdStyleClasses;

namespace PepperDash.Essentials.Sharp
{
    public static class SharpDisplayPluginProtocolCmdStyleFactory
    {
        internal static SharpDisplayProtocolCmdStyleBase BuildSharpDislplay(SharpDisplayPluginDevice display, string displayConfig)
        {
            // Get config value for protocol style and return protocol

            switch (displayConfig)
            {

                case "PN-UH501":
                    return new SharpDisplayProtocolCmdStylePnUh501(display);
                default:
                    Debug.Console(0, "No Protocol Style Exists for Type");
                    return new SharpDisplayProtocolCmdStyleNotDefined(display);
            }
        }
    }
}