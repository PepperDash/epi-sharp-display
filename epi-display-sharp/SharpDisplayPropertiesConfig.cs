using Newtonsoft.Json;

namespace PepperDash.Plugins.SharpDisplay
{
	public class SharpDisplayPropertiesConfig
	{
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("volumeUpperLimit")]
        public int volumeUpperLimit { get; set; }

        [JsonProperty("volumeLowerLimit")]
        public int volumeLowerLimit { get; set; }

        [JsonProperty("pollIntervalMs")]
        public long pollIntervalMs { get; set; }

        [JsonProperty("coolingTimeMs")]
        public uint coolingTimeMs { get; set; }

        [JsonProperty("warmingTimeMs")]
        public uint warmingTimeMs { get; set; }

		[JsonProperty("pollVolume")]
		public bool PollVolume { get; set; }
	}
}