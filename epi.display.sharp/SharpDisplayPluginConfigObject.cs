using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace Epi.Display.Sharp
{
	public class SharpDisplayPluginConfigObject
	{
        public ControlPropertiesConfig Control { get; set; }

        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string Protocol { get; set; }
	}


}