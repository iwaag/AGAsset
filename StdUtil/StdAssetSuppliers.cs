using AGDev;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AGAsset.StdUtil {
	/*public class VoiceGeneratorSupplier : AssetUnitSupplier {
		public string 
		void AssetUnitSupplier.SupplyAssetUnit(AssetRequestUnit assetRequest, AssetUnitSupplyListener listener) {
			if (assetRequest.assettype != "VoiceGen") {
				listener.supplyTaker.OnFinish();
				return;
			}
			//check cache
			var temporaryAUInfo = new AssetUnitInfo {
				assettype = assetRequest.assettype,
				attributes = assetRequest.attributes[0],
				distributor = "VoiceGen",
				reference = "VoiceGen:" + assetRequest.attributes[0],
				shortname = "VoiceGen-" + System.Guid.NewGuid()
			};
			listener.supplyTaker.Take(temporaryAUInfo);
			listener.supplyTaker.OnFinish();
		}
	}
	public class StubSpeechGeneratorInterfaceGiver : ImmediateGiver<AssetUnitInterface, AssetUnitInfo> {
		public AssetUnitInterface voiceAssetInterface;
		AssetUnitInterface ImmediateGiver<AssetUnitInterface, AssetUnitInfo>.PickBestElement(AssetUnitInfo key) {
			if () {
				
			}
		}
		public class PrvtAssetInterface : AssetUnitInterface, AssetReferInterface, AssetModifyInterface {
			public AssetUnitInfo auInfo;
			AssetReferInterface AssetUnitInterface.referer => this;
			AssetModifyInterface AssetUnitInterface.modifier => this;
			AssetUnitInfo AssetUnitInterface.baseAssetInfo => auInfo;
			void AssetReferInterface.PickContent<ContentType>(string path, Taker<ContentType> collector) {
				if () {
				}
			}

			void AssetModifyInterface.SetContent<ContentType>(AssetContentSettingParam<ContentType> setParam, AssetInResultListener<ContentType> listener) {
				throw new System.NotImplementedException();
			}
		}
	}*/
}
