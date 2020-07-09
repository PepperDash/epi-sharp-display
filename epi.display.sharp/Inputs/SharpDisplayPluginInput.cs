using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Epi.Display.Sharp.Inputs
{
    public class SharpDisplayPluginInput
    {
        public string Name { get; private set; }
        public string InputCode { get; private set; }

        public SharpDisplayPluginInput(string name, string inputCode)
        {
            Name = name;
            InputCode = inputCode;
        }
    }
}