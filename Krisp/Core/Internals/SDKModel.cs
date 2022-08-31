using System;
using Newtonsoft.Json;

namespace Krisp.Core.Internals
{
	public class SDKModel
	{
		[JsonProperty(PropertyName = "name", Required = 2)]
		public string modelName;

		[JsonProperty(PropertyName = "config", Required = 2)]
		public string modelConf;

		[JsonProperty(PropertyName = "default", Required = 2)]
		public bool isDefault;

		[JsonProperty(PropertyName = "minRate", Required = 2)]
		public uint minRate;

		[JsonProperty(PropertyName = "maxRate", Required = 2)]
		public uint maxRate;

		[JsonProperty(PropertyName = "dereverb")]
		public bool dereverb;

		[JsonProperty(PropertyName = "openOffice")]
		public bool openOffice;
	}
}
