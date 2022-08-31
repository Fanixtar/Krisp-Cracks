using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Krisp.AppHelper;
using Newtonsoft.Json;
using Shared.Interops;

namespace Krisp.Core.Internals
{
	public class SDKModelManager
	{
		private Logger logger
		{
			get
			{
				Logger logger;
				if ((logger = this._logger) == null)
				{
					logger = (this._logger = LogWrapper.GetLogger("SDKModelManager"));
				}
				return logger;
			}
		}

		public SDKModelManager(string modelsCfgFile, string modelsFolder)
		{
			if (string.IsNullOrWhiteSpace(modelsCfgFile))
			{
				throw new ArgumentException("", "modelsCfgFile");
			}
			if (string.IsNullOrWhiteSpace(modelsFolder))
			{
				throw new ArgumentException("", "modelsFolder");
			}
			if (!Directory.Exists(modelsFolder))
			{
				throw new DirectoryNotFoundException("directory " + modelsFolder + " not found.");
			}
			this._modelsFolder = modelsFolder;
			this.LoadConfig(modelsCfgFile);
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

		public List<SDKModel> GetModelsList(EnStreamDirection? streamDirection = null)
		{
			if (streamDirection == null)
			{
				List<SDKModel> list = new List<SDKModel>();
				list.AddRange(this._inboundModels);
				list.AddRange(this._outboundModels);
				return list;
			}
			EnStreamDirection? enStreamDirection = streamDirection;
			EnStreamDirection enStreamDirection2 = EnStreamDirection.Speaker;
			if ((enStreamDirection.GetValueOrDefault() == enStreamDirection2) & (enStreamDirection != null))
			{
				return this._inboundModels;
			}
			return this._outboundModels;
		}

		private void LoadConfig(string modelsCfg)
		{
			SDKModelManager.ModelConfig modelConfig = null;
			string text;
			using (StreamReader streamReader = File.OpenText(modelsCfg))
			{
				text = streamReader.ReadToEnd();
			}
			try
			{
				modelConfig = JsonConvert.DeserializeObject<SDKModelManager.ModelConfig>(text);
			}
			catch (Exception ex)
			{
				this.logger.LogError(ex);
				throw new Exception("Syntax error in ModelConfig", ex);
			}
			this._inboundModels = modelConfig.models.inboundModels;
			this._outboundModels = modelConfig.models.outboundModels;
			this.CheckConfig();
			if (this._inboundModels.Count == 0)
			{
				throw new Exception("No inbound model in config");
			}
			if (this._outboundModels.Count == 0)
			{
				throw new Exception("No inbound model in config");
			}
			if (this._inboundModels.Find((SDKModel itm) => itm.isDefault) == null)
			{
				this.logger.LogError("Missing default inbound model. Setting first as default.");
				this._inboundModels.First<SDKModel>().isDefault = true;
			}
			if (this._outboundModels.Find((SDKModel itm) => itm.isDefault) == null)
			{
				this.logger.LogError("Missing default outbound model. Setting first as default.");
				this._outboundModels.First<SDKModel>().isDefault = true;
			}
		}

		private void CheckConfig()
		{
			bool flag = false;
			foreach (SDKModel sdkmodel in this._inboundModels)
			{
				if (!this.CheckTHW(sdkmodel.modelConf))
				{
					sdkmodel.IsBad = true;
					flag = true;
				}
			}
			if (flag)
			{
				this._inboundModels = this._inboundModels.Where((SDKModel x) => !x.IsBad).ToList<SDKModel>();
			}
			flag = false;
			foreach (SDKModel sdkmodel2 in this._outboundModels)
			{
				if (!this.CheckTHW(sdkmodel2.modelConf))
				{
					sdkmodel2.IsBad = true;
					flag = true;
				}
			}
			if (flag)
			{
				this._inboundModels = this._inboundModels.Where((SDKModel x) => !x.IsBad).ToList<SDKModel>();
			}
		}

		private bool CheckTHW(string name)
		{
			string text = this._modelsFolder + "\\" + name;
			if (!File.Exists(text))
			{
				this.logger.LogError("No " + name + " in models directory. Discrading.");
				return false;
			}
			string text2;
			using (StreamReader streamReader = File.OpenText(text))
			{
				text2 = streamReader.ReadLine();
			}
			text2 = this._modelsFolder + "\\" + text2.Remove(0, 7);
			if (!File.Exists(text2))
			{
				this.logger.LogError("No " + text2 + " in models directory. Discrading.");
				return false;
			}
			return true;
		}

		private List<SDKModel> _inboundModels;

		private List<SDKModel> _outboundModels;

		private string _modelsFolder;

		private Logger _logger;

		private class SDKModels
		{
			[JsonProperty(PropertyName = "inbound", Required = Required.Always)]
			public List<SDKModel> inboundModels;

			[JsonProperty(PropertyName = "outbound", Required = Required.Always)]
			public List<SDKModel> outboundModels;
		}

		private class ModelConfig
		{
			[JsonProperty(PropertyName = "models", Required = Required.Always)]
			public SDKModelManager.SDKModels models;
		}
	}
}
