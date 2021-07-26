

namespace PepperDash.Essentials.Sharp.Inputs
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