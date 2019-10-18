using AGDev;
using System.Collections.Generic;

namespace AGAsset.StdUtil {
	public class StubAssetInResultListener<AssetType> : AssetInResultListener<AssetType> {
		AssetType copied;
		void AssetInResultListener<AssetType>.OnCopyContent(AssetType _copied) {
			copied = _copied;
		}

		void AssetInResultListener<AssetType>.OnFail() { }

		void AssetInResultListener<AssetType>.OnOverwrite() { }

		string AssetInResultListener<AssetType>.OnRequestPathChange(string suggetion) { return ""; }

		void AssetInResultListener<AssetType>.OnSuccess() { }
	}
	public class SideListeningAUSupplyListener : AssetSupplyListener, Taker<AssetPick> {
		public AssetSupplyListener mainListener;
		public AssetSupplyListener sideListener;
		Taker<AssetPick> AssetSupplyListener.supplyTaker => this;

		void Taker<AssetPick>.None() {
			mainListener.supplyTaker.None();
			sideListener.supplyTaker.None();
		}

		void Taker<AssetPick>.Take(AssetPick item) {
			mainListener.supplyTaker.Take(item);
			sideListener.supplyTaker.Take(item);
		}
	}
	public class ClusterAUInterface : AssetUnitInterface, AssetModifyInterface, AssetReferInterface {
		public IEnumerable<AssetUnitInterface> cluster;
		public AssetUnitInfo auInfo;
		AssetReferInterface AssetUnitInterface.referer => this;

		AssetModifyInterface AssetUnitInterface.modifier => this;

		AssetUnitInfo AssetUnitInterface.baseAssetInfo => auInfo;

		void AssetReferInterface.PickContent<ContentType>(string path, Taker<ContentType> collector) {
			var enumerator = cluster.GetEnumerator();
			if (enumerator.MoveNext()) {
				enumerator.Current.referer.PickContent(path, new PrvtColl <ContentType> { path = path, clientTaker = collector, enumerator = enumerator });
			}
		}

		void AssetModifyInterface.SetContent<ContentType>(AssetContentSettingParam<ContentType> setParam, AssetInResultListener<ContentType> listener) {
			//stub
			listener.OnFail();
		}
		public class PrvtColl<ContentType> : Taker<ContentType> {
			public string path;
			public Taker<ContentType> clientTaker;
			public IEnumerator<AssetUnitInterface> enumerator;
			void Taker<ContentType>.Take(ContentType item) {
				clientTaker.Take(item);
			}
			void Taker<ContentType>.None() {
				if (enumerator.MoveNext()) {
					enumerator.Current.referer.PickContent(path, this);
				}
				else {
					clientTaker.None();
				}
			}
		}
	}
	class ClusterAssetUnitIntegrator : AssetUnitIntegrator {
		public IEnumerable<AssetUnitIntegrator> Integrators;
		void AssetUnitIntegrator.IntegrateAssetUnit(AssetRequestUnit reqUnit, AssetUnitIntegrateListener listener) {
			foreach (var Integrator in Integrators) {
				Integrator.IntegrateAssetUnit(reqUnit, listener);
			}
		}
	}
	public class CachingAUInterface : AssetUnitInterface, AssetReferInterface, AssetModifyInterface {
		public AssetUnitInterface remote;
		public AssetUnitInterface local;
		AssetReferInterface AssetUnitInterface.referer => this;

		AssetModifyInterface AssetUnitInterface.modifier => this;

		AssetUnitInfo AssetUnitInterface.baseAssetInfo => local.baseAssetInfo;

		void AssetReferInterface.PickContent<ContentType>(string path, Taker<ContentType> collector) {
			local.referer.PickContent(path, new FirstColl<ContentType> { clientColl = collector, parent = this, path = path});
		}
		public class FirstColl<ContentType> : Taker<ContentType> {
			public Taker<ContentType> clientColl;
			public CachingAUInterface parent;
			public string path;
			void Taker<ContentType>.Take(ContentType item) {
				clientColl.Take(item);
			}
			void Taker<ContentType>.None() {
				parent.remote.referer.PickContent(path, new SecondColl { clientColl = clientColl, parent = parent, path = path });
			}

			public class SecondColl : Taker<ContentType> {
				public Taker<ContentType> clientColl;
				public CachingAUInterface parent;
				public string path;
				void Taker<ContentType>.Take(ContentType item) {
					clientColl.Take(item);
					parent.local.modifier.SetContent(new AssetContentSettingParam<ContentType> { content = item, contentPath = path, doOverwrite = true }, new StubAssetInResultListener<ContentType>());
				}
				void Taker<ContentType>.None() {
					clientColl.None();
				}
			}
		}
		void AssetModifyInterface.SetContent<ContentType>(AssetContentSettingParam<ContentType> setParam, AssetInResultListener<ContentType> listener) {
			//controversial: is it really OK to just setting content to both?
			local.modifier.SetContent(setParam, listener);
			remote.modifier.SetContent(setParam, listener);
		}
	}
#if false
	public class AssetSupplyLineup : AssetSupplier {
		public IList<AssetSupplier> suppliers = new List<AssetSupplier>();
		void AssetSupplier.SupplyAsset(AssetRequest assetRequest, AssetSupplyListener listener) {
			if (suppliers.Count > 0) {
				suppliers[0].SupplyAsset(assetRequest, new PrvtSupLis {
					clientListener = listener, suppliers = suppliers, assetRequest = assetRequest, integrantSupplier = this });
			} else {
				listener.supplyTaker.OnFinish();
			}
		}
		class PrvtSupLis : AssetSupplyListener, Taker<AssetPick> {
			public AssetSupplyListener clientListener;
			public AssetRequest assetRequest;
			public IList<AssetSupplier> suppliers;
			public AssetSupplier integrantSupplier;
			public int supplierIndex = 0;
			bool didCollect = false;
			void OnPass() {
				supplierIndex++;
				if (suppliers.Count > supplierIndex) {
					suppliers[supplierIndex].SupplyAsset(assetRequest, this);
				} else {
					// no one can supply
					clientListener.supplyTaker.OnFinish();
				}
			}
			Taker<AssetPick> AssetSupplyListener.supplyTaker {
				get { return this; }
			}

			void Taker<AssetPick>.Take(AssetPick newElement) {
				didCollect = true;
				clientListener.supplyTaker.Take(newElement);
			}

			void AsyncProcess.OnFail(string reason) {
				clientListener.supplyTaker.OnFail(reason);
			}

			void AsyncProcess.OnFinish() {
				if (!didCollect)
					//passed
					OnPass();
				else
					//collectod
					clientListener.supplyTaker.OnFinish();
			}

			Taker<RequestAndTaker<AssetUnitInfo>> AssetSupplyListener.OnRequestIntegrants() {
				return clientListener.OnRequestIntegrants();
			}
		}
	}
#endif
}
