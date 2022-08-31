using System;
using Newtonsoft.Json;

namespace Krisp.Core.Internals
{
	public class SDKModel
	{
		[JsonProperty(PropertyName = "name", Required = Required.Always)]
		public string modelName;

		[JsonProperty(PropertyName = "config", Required = Required.Always)]
		public string modelConf;

		[JsonProperty(PropertyName = "default", Required = Required.Always)]
		public bool isDefault;

		[JsonProperty(PropertyName = "minRate", Required = Required.Always)]
		public uint minRate;

		[JsonProperty(PropertyName = "maxRate", Required = Required.Always)]
		public uint maxRate;

		[JsonProperty(PropertyName = "dereverb")]
		public bool dereverb;

		[JsonProperty(PropertyName = "openOffice")]
		public bool openOffice;

		public bool IsBad;
	}
}
