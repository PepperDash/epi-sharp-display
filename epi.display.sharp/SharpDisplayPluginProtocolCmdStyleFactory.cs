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

                case "PN-UH501":
                    return new SharpDisplayProtocolCmdStylePnUh501(display);
                default:
                    Debug.Console(0, "No Protocol Style Exists for Type");
                    return new SharpDisplayProtocolCmdStyleNotDefined(display);
            }
        }
    }
}