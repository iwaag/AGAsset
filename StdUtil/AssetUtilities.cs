using AGDev;
using System.Collections.Generic;

namespace AGAsset.StdUtil {
	public class SideListeningAUSupplyListener : AssetSupplyListener, AsyncCollector<AssetPick> {
		public AssetSupplyListener mainListener;
		public AssetSupplyListener sideListener;
		AsyncCollector<AssetPick> AssetSupplyListener.supplyCollector => this;
		void Collector<AssetPick>.Collect(AssetPick item) {
			mainListener.supplyCollector.Collect(item);
			sideListener.supplyCollector.Collect(item);
		}
		void AsyncCollector<AssetPick>.OnFinish() {
			mainListener.supplyCollector.OnFinish();
			sideListener.supplyCollector.OnFinish();
		}
	}
	public class ClusterAUInterface : AssetUnitInterface, AssetModifyInterface, AssetReferInterface {
		public IEnumerable<AssetUnitInterface> cluster;
		public AssetUnitInfo auInfo;
		AssetReferInterface AssetUnitInterface.referer => this;

		AssetModifyInterface AssetUnitInterface.modifier => this;

		AssetUnitInfo AssetUnitInterface.baseAssetInfo => auInfo;

		void AssetReferInterface.PickContent<ContentType>(string path, AsyncCollector<ContentType> collector) {
			var enumerator = cluster.GetEnumerator();
			if (enumerator.MoveNext()) {
				enumerator.Current.referer.PickContent(path, new PrvtColl <ContentType> { path = path, clientCollector = collector, enumerator = enumerator });
			}
		}

		void AssetModifyInterface.SetContent<ContentType>(AssetContentSettingParam<ContentType> setParam, AssetInResultListener<ContentType> listener) {
			//stub
			listener.OnFail();
		}
		public class PrvtColl<ContentType> : AsyncCollector<ContentType> {
			public string path;
			public bool didCollect = false;
			public AsyncCollector<ContentType> clientCollector;
			public IEnumerator<AssetUnitInterface> enumerator;
			void Collector<ContentType>.Collect(ContentType item) {
				clientCollector.Collect(item);
				didCollect = true;
			}
			void AsyncCollector<ContentType>.OnFinish() {
				if (!didCollect && enumerator.MoveNext()) {
					enumerator.Current.referer.PickContent(path, this);
				} else {
					clientCollector.OnFinish();
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

		void AssetReferInterface.PickContent<ContentType>(string path, AsyncCollector<ContentType> collector) {
			local.referer.PickContent(path, new FirstColl<ContentType> { clientColl = collector, parent = this, path = path});
		}
		public class FirstColl<ContentType> : AsyncCollector<ContentType> {
			public AsyncCollector<ContentType> clientColl;
			public CachingAUInterface parent;
			public string path;
			bool didCollect = false;
			void Collector<ContentType>.Collect(ContentType item) {
				didCollect = true;
				clientColl.Collect(item);
			}

			void AsyncCollector<ContentType>.OnFinish() {
				if (!didCollect) {
					parent.remote.referer.PickContent(path, new SecondColl { clientColl = clientColl, parent = parent, path = path });
				}
			}
			public class SecondColl : AsyncCollector<ContentType> {
				public AsyncCollector<ContentType> clientColl;
				public CachingAUInterface parent;
				public string path;
				void Collector<ContentType>.Collect(ContentType item) {
					clientColl.Collect(item);
					parent.local.modifier.SetContent(new AssetContentSettingParam<ContentType> { content = item, contentPath = path, doOverwrite = true }, new StubAssetInResultListener<ContentType>());
				}

				void AsyncCollector<ContentType>.OnFinish() {
					clientColl.OnFinish();
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
				listener.supplyCollector.OnFinish();
			}
		}
		class PrvtSupLis : AssetSupplyListener, AsyncCollector<AssetPick> {
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
					clientListener.supplyCollector.OnFinish();
				}
			}
			AsyncCollector<AssetPick> AssetSupplyListener.supplyCollector {
				get { return this; }
			}

			void Collector<AssetPick>.Collect(AssetPick newElement) {
				didCollect = true;
				clientListener.supplyCollector.Collect(newElement);
			}

			void AsyncCollector<AssetPick>.OnFail(string reason) {
				clientListener.supplyCollector.OnFail(reason);
			}

			void AsyncCollector<AssetPick>.OnFinish() {
				if (!didCollect)
					//passed
					OnPass();
				else
					//collectod
					clientListener.supplyCollector.OnFinish();
			}

			AsyncCollector<RequestAndCollector<AssetUnitInfo>> AssetSupplyListener.OnRequestIntegrants() {
				return clientListener.OnRequestIntegrants();
			}
		}
	}
#endif
}
