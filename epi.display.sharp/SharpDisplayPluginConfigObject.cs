
using PepperDash.Core;

namespace Epi.Display.Sharp
{
	public class SharpDisplayPluginConfigObject
	{
        public ControlPropertiesConfig Control { get; set; }

        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string Protocol { get; set; }

        public CustomCommand[] CustomCommands { get; set; }

	}

    public class CustomCommand
    {

        public string Key { get; set; }


        public string Name { get; set; }


        public string Command { get; set; }


        public string ResponseHandler { get; set; }
    }

}