using System.Collections.Generic;
namespace AGAsset.StdUtil {
	public class RepositoryReferer : AssetReferer {
		void AssetReferer.ReferAsset(AssetUnitInfo assetUnitInfo, AssetReferenceListener listener) {
			var strings = assetUnitInfo.reference.Split(' ');
			if (strings[0].EndsWith(".git")) {
				var vcsRef = new VersionControlSystemRef { repository = strings[0], systemName = "git" };
				if (strings.Length > 1) {
					vcsRef.targetPath = strings[1];
				}
				listener.OnBeginRefering();
				listener.OnVCSReferenceObtained(vcsRef);
				listener.OnFinish();
			}
		}
	}
	public class ClusterAssetReferer : AssetReferer {
		public IEnumerable<AssetReferer> referers = new List<AssetReferer>();
		void AssetReferer.ReferAsset(AssetUnitInfo assetUnitInfo, AssetReferenceListener listener) {
			foreach (var referer in referers) {
				var listenerWrapper = new PrvtAssetReferenceListener { clientListener = listener };
				referer.ReferAsset(assetUnitInfo, listenerWrapper);
				if (listenerWrapper.didBegin)
					return;
			}
		}
		class PrvtAssetReferenceListener : AssetReferenceListener {
			public AssetReferenceListener clientListener;
			public bool didBegin = false;
			void AssetReferenceListener.OnAssetInfoObtained(AssetUnitInfo obtained, AssetReferer referer) {
				clientListener.OnAssetInfoObtained(obtained, referer);
			}
			void AssetReferenceListener.OnBeginRefering() {
				didBegin = true;
				clientListener.OnBeginRefering();
			}

			void AssetReferenceListener.OnFinish() {
				clientListener.OnFinish();
			}

			void AssetReferenceListener.OnStdArchiveObtained(byte[] obtained) {
				clientListener.OnStdArchiveObtained(obtained);
			}

			void AssetReferenceListener.OnInterfaceObtained(AssetUnitInterface referInterface) {
				clientListener.OnInterfaceObtained(referInterface);
			}

			void AssetReferenceListener.OnVCSReferenceObtained(VersionControlSystemRef reason) {
				clientListener.OnVCSReferenceObtained(reason);
			}

			void AssetReferenceListener.OnRawAssetContentObtained(byte[] archiveData, string contentType) {
				clientListener.OnRawAssetContentObtained(archiveData, contentType);
			}

			void AssetReferenceListener.EnsureDependencyLocalized(AssetRequestUnit assetRequest) {
				clientListener.EnsureDependencyLocalized(assetRequest);
			}

			void AssetReferenceListener.OnAssetContentObtained<AssetContentType>(AssetContentType asset, string contentPath) {
				clientListener.OnAssetContentObtained(asset, contentPath);
			}

			void AssetReferenceListener.OnBasicIOObtained(AssetUnitBasicIO referInterface) {
				clientListener.OnBasicIOObtained(referInterface);
			}

			void AssetReferenceListener.OnObjectObtained(object asset, string contentPath) {
				clientListener.OnObjectObtained(asset, "");
			}
		}
	}
	public class ObservedAssetReferer : AssetReferer {
		public ObservedProcessHelper helper;
		public AssetReferer clientReferer;
		void AssetReferer.ReferAsset(AssetUnitInfo assetUnitInfo, AssetReferenceListener listener) {
			clientReferer.ReferAsset(assetUnitInfo, new PrvtListener { clientListener = listener, helper = helper });
		}
		public class PrvtListener : AssetReferenceListener {
			public ObservedProcessHelper helper;
			public AssetReferenceListener clientListener;
			void AssetReferenceListener.EnsureDependencyLocalized(AssetRequestUnit assetRequest) {
				clientListener.EnsureDependencyLocalized(assetRequest);
			}

			void AssetReferenceListener.OnAssetContentObtained<AssetContentType>(AssetContentType asset, string contentPath) {
				clientListener.OnAssetContentObtained(asset, contentPath);
			}

			void AssetReferenceListener.OnAssetInfoObtained(AssetUnitInfo obtained, AssetReferer referer) {
				clientListener.OnAssetInfoObtained(obtained, referer);
			}

			void AssetReferenceListener.OnBasicIOObtained(AssetUnitBasicIO referInterface) {
				clientListener.OnBasicIOObtained(referInterface);
			}

			void AssetReferenceListener.OnBeginRefering() {
				helper.CountUp();
				clientListener.OnBeginRefering();
			}

			void AssetReferenceListener.OnFinish() {
				clientListener.OnFinish();
				helper.CountDown();
			}

			void AssetReferenceListener.OnInterfaceObtained(AssetUnitInterface referInterface) {
				clientListener.OnInterfaceObtained(referInterface);
			}

			void AssetReferenceListener.OnObjectObtained(object asset, string contentPath) {
				clientListener.OnObjectObtained(asset, contentPath);
			}

			void AssetReferenceListener.OnRawAssetContentObtained(byte[] archiveData, string contentPath) {
				clientListener.OnRawAssetContentObtained(archiveData, contentPath);
			}

			void AssetReferenceListener.OnStdArchiveObtained(byte[] archiveData) {
				clientListener.OnStdArchiveObtained(archiveData);
			}

			void AssetReferenceListener.OnVCSReferenceObtained(VersionControlSystemRef vcsRef) {
				clientListener.OnVCSReferenceObtained(vcsRef);
			}
		}
	}
}
