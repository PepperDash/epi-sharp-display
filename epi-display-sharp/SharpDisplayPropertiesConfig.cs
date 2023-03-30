using Newtonsoft.Json;

namespace PepperDash.Plugins.SharpDisplay
{
	public class SharpDisplayPropertiesConfig
	{
		/// <summary>
		/// Character used to pad the command, defaults to " " (\x20) if not defined
		/// </summary>
		[JsonProperty("zeroPadCommands")]
        public bool ZeroPadCommands { get; set; }

		/// <summary>
		/// Device volume upper limit, 100
		/// </summary>
        [JsonProperty("volumeUpperLimit")]
        public int volumeUpperLimit { get; set; }

		/// <summary>
		/// Device volume lower limit
		/// </summary>
        [JsonProperty("volumeLowerLimit")]
        public int VolumeLowerLimit { get; set; }

		/// <summary>
		/// Poll interval in miliseconds, defaults 30,000ms (30-seconds)
		/// </summary>
        [JsonProperty("pollIntervalMs")]
        public long PollIntervalMs { get; set; }

		/// <summary>
		/// Device cooling time, defaults to 15,000ms (15-seconds)
		/// </summary>
        [JsonProperty("coolingTimeMs")]
        public uint CoolingTimeMs { get; set; }

		/// <summary>
		/// Device warming time, defaults to 15,000ms (15-seconds)
		/// </summary>
        [JsonProperty("warmingTimeMs")]
        public uint WarmingTimeMs { get; set; }

		/// <summary>
		/// Enbales whether volume and mute are polled
		/// </summary>
		[JsonProperty("pollVolume")]
		public bool PollVolume { get; set; }

        /// <summary>
        /// Set Left(true) or Right(false) justification for command parameters
        /// </summary>
        [JsonProperty("isLeftJustified")]
        public bool IsLeftJustified { get; set; }
	}
}