using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Shared.Interops;

namespace Krisp.Core.Internals
{
	public class SDKModelManager
	{
		public SDKModelManager(string modelsCfg, string modelsFolder)
		{
			this.LoadConfig(modelsCfg);
		}

		public SDKModel FindModel(EnStreamDirection dir, SPFeature feature, uint sRate)
		{
			SDKModel sdkmodel = null;
			foreach (SDKModel sdkmodel2 in ((dir == EnStreamDirection.Speaker) ? this._inboundModels : this._outboundModels))
			{
				if (feature == SPFeature.Feature_OpenOffice && sdkmodel2.openOffice)
				{
					sdkmodel = sdkmodel2;
					break;
				}
				if (feature == SPFeature.Feature_Dereverb && sdkmodel2.dereverb)
				{
					if (sdkmodel2.minRate <= sRate && sRate < sdkmodel2.maxRate)
					{
						sdkmodel = sdkmodel2;
						break;
					}
				}
				else if (sdkmodel2.isDefault)
				{
					sdkmodel = sdkmodel2;
				}
			}
			return sdkmodel;
		}

		public bool GetModelsList(out List<SDKModel> modelsList)
		{
			modelsList = new List<SDKModel>();
			modelsList.AddRange(this._inboundModels);
			modelsList.AddRange(this._outboundModels);
			return modelsList.Count > 1;
		}

		private void LoadConfig(string modelsCfg)
		{
			SDKModelManager.ModelConfig modelConfig = null;
			try
			{
				using (StreamReader streamReader = File.OpenText(modelsCfg))
				{
					modelConfig = JsonConvert.DeserializeObject<SDKModelManager.ModelConfig>(streamReader.ReadToEnd());
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error on reading ModelConfig.", ex);
			}
			try
			{
				this._inboundModels = modelConfig.models.inboundModels;
				this._outboundModels = modelConfig.models.outboundModels;
				if (this._inboundModels.Find((SDKModel itm) => itm.isDefault) == null)
				{
					throw new Exception("Missing default inbound model in configuration.");
				}
				if (this._outboundModels.Find((SDKModel itm) => itm.isDefault) == null)
				{
					throw new Exception("Missing default outbound model in configuration.");
				}
			}
			catch (Exception ex2)
			{
				throw new Exception("Error on loading ModelConfig.", ex2);
			}
		}

		private List<SDKModel> _inboundModels;

		private List<SDKModel> _outboundModels;

		internal class SDKModels
		{
			[JsonProperty(PropertyName = "inbound", Required = 2)]
			public List<SDKModel> inboundModels;

			[JsonProperty(PropertyName = "outbound", Required = 2)]
			public List<SDKModel> outboundModels;
		}

		internal class ModelConfig
		{
			[JsonProperty(PropertyName = "models", Required = 2)]
			public SDKModelManager.SDKModels models;
		}
	}
}
