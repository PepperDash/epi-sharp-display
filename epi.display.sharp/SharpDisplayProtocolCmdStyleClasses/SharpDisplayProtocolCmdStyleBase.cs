using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using Epi.Display.Sharp;
using System.Text.RegularExpressions;


namespace Epi.Display.Sharp.SharpDisplayProtocolCmdStyleClasses
{
    public abstract class SharpDisplayProtocolCmdStyleBase
    {

        public Dictionary<eCommands, string> Commands;
        public Dictionary<eInputParams, string> InputParams;
        public Dictionary<ePowerParams, string> PowerParams;
        public Dictionary<eCommMethod, string> CommandSettingParams;

        protected string ParamMatchRegexPattern = string.Empty;

        protected string Command;
        protected string Parameter;
        
        public string PollString = string.Empty;
        public string Delimiter = "\x0D";


        #region IHasProtocolStyle Members

        public abstract string FormatCommandFromString(string command, string parameter);

        public abstract string FormatPowerCommand(eCommands command, ePowerParams parameter);

        public abstract string FormatInputCommand(eCommands command, eInputParams parameter);

        public abstract string FormatCommandSettingCommand(eCommands command, eCommMethod parameter);


        #endregion

        public Object HandleResponse(string response)
        {
            Regex ParamRegex = new Regex(ParamMatchRegexPattern);

            Debug.Console(2, "Handling Response: {0}", response);

            if (response.ToUpper().Contains("OK"))
                return new SharpDisplayPluginResponseOk();

            if (response.ToUpper().Contains("ERR"))
                return new SharpDisplayPluginResponseError();

            Match Param = ParamRegex.Match(response);
            Debug.Console(2, "Param match: {0}", Param.Value);
            if (!Param.Success)
            {
                Debug.Console(2, "Param !success");
                return new SharpDisplayPluginResponseError();
            }

            return new SharpDisplayPluginResponse(Param.ToString());
        }

        public override string ToString()
        {
            string type = this.GetType().Name.ToString();
            return "Device Type: " + type;
        }
    }
}