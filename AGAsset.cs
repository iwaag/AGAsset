using System.Collections.Generic;
using System.Runtime.Serialization;
using AGDev;
using AGBLang;
namespace AGAsset {
	#region asset mediator
	public interface AssetMediator {
		AssetType PickImplementedAsset<AssetType>(GrammarBlock gBlock);
		void SeekAsset<AssetType>(GrammarBlock gBlock, AsyncCollector<AssetType> collLis);
		IEnumerable<AssetType> GetImplementedAssets<AssetType>();
	}
	#endregion
	#region asset implementer
	public interface AssetSeekSupportListener<AssetType> {
		Picker<AssetUnitInterface, AssetRequestUnit> auInterfacePicker { get; }
		AsyncCollector<AssetType> collectorOnImplement { get; }
	}
	public class AssetImplementParam {
		public AssetRequestUnit assetRrequest;
		public AssetUnitInterface auInterface;
	}
	public interface AssetImplementer<AssetType> {
		AssetType PickImplementedAsset(GrammarBlock gBlock);
		void SeekAsset(GrammarBlock gBlock, AssetSeekSupportListener<AssetType> collect);
		//return : did success?
		void ImplementAsset(AssetImplementParam param, SimpleProcessListener listener);
		IEnumerable<AssetType> implementedAssets { get; }
	}
	public interface AssetImplementerGetter {
		AssetImplementer<AssetType> GetAssetImplementer<AssetType>(GrammarBlock gBlock);
	}
	#endregion
	#region asset request
	public interface AssetRequestHolder {
		AssetRequestUnit AddOrMergeRequest_GetAdded(AssetRequestUnit newReqUnit);
		AssetRequest request { get; }
		ImmediatePicker<AssetRequestUnit, int> unitPicker { get; }
	}
	[System.Serializable]
	[DataContract]
	public class AssetRequestUnit {
		static int stubID = 1;
		public int ID = stubID++;
		[DataMember(EmitDefaultValue = false)]
		public string sname;
		[DataMember(EmitDefaultValue = false)]
		public string creatorref;
		[DataMember(EmitDefaultValue = false)]
		public string assettype;
		[DataMember(EmitDefaultValue = false)]
		public List<string> attributes = new List<string>();
	}
	[System.Serializable]
	public class AssetRequest {
		public List<AssetRequestUnit> units = new List<AssetRequestUnit>();
	}
	#endregion
	#region asset referer
	public struct VersionControlSystemRef {
		public string repository;
		public string systemName;
		public string version;
		public string targetPath;
	}
	public interface AssetReferenceListener {
		void OnBeginRefering();
		void EnsureDependencyLocalized(AssetRequestUnit assetRequest);
		void OnBasicIOObtained(AssetUnitBasicIO referInterface);
		void OnInterfaceObtained(AssetUnitInterface referInterface);
		void OnAssetInfoObtained(AssetUnitInfo obtained, AssetReferer referer);
		void OnAssetContentObtained<AssetContentType>(AssetContentType asset, string contentPath);
		void OnObjectObtained(object asset, string contentPath);
		void OnRawAssetContentObtained(byte[] rawData, string contentPath);
		void OnStdArchiveObtained(byte[] archiveData);
		void OnVCSReferenceObtained(VersionControlSystemRef vcsRef);
		void OnFinish();
	}
	public interface AssetReferer {
		void ReferAsset(AssetUnitInfo assetUnitInfo, AssetReferenceListener listener);
	}
	public interface AssetInfoDatabase : ImmediatePicker<AssetUnitInfo, AssetUnitInfo>, Collector<AssetUnitInfo>, AssetUnitSupplier { }
	#endregion
	#region asset unit Integrator 
	public interface AssetUnitIntegrateListener {
		AssetUnitIntegrateSupport OnBeginIntegrate();
	}
	public interface AssetUnitIntegrateSupport {
		Picker<AssetUnitInterface, AssetRequestUnit> integrantPicker { get; }
		AssetUnitInterface generatedAssetInterface { get; }
		void OnSucceed();
		void OnFail();
	}
	public interface AssetUnitIntegrator {
		void IntegrateAssetUnit(AssetRequestUnit reqUnit, AssetUnitIntegrateListener listener);
	}
	#endregion
	#region asset supplier
	public interface AssetSupplyListener {
		AsyncCollector<AssetPick> supplyCollector { get; }
	}
	public interface AssetSupplier {
		void SupplyAsset(AssetRequest assetRequest, AssetSupplyListener listener);
	}

#endregion
	#region asset unit supplier
	public interface AssetUnitSupplyListener {
		Picker<AssetUnitInterface, AssetRequestUnit> integrantPicker { get; }
		AsyncCollector<AssetUnitInfo> supplyCollector { get; }
	}
	public interface AssetUnitSupplier {
		void SupplyAssetUnit(AssetRequestUnit assetRequest, AssetUnitSupplyListener listener);
	}
	#endregion
	#region basic asset IO
	public interface AssetInResultListener<ContentType> {
		void OnSuccess();
		void OnCopyContent(ContentType coppied);
		string OnRequestPathChange(string suggetion);
		void OnOverwrite();
		void OnFail();
	}
	public interface AssetBasicCollector<AssetType> {
		void CollectAsset(AssetType assetType);
		void CollectRawAsset(byte[] assetType);
		void OnFinish();
	}
	public interface AssetUnitBasicOut {
		void PickAssetAtPath<ContentType>(string path, AssetBasicCollector<ContentType> processor);
	}
	public interface AssetUnitBasicIn {
		void SetContent<ContentType>(BasicAssetInParam<ContentType> settingParam, AssetInResultListener<ContentType> listener);
	}
	public interface AssetUnitBasicIO {
		AssetUnitBasicIn assetIn { get; }
		AssetUnitBasicOut assetOut { get; }
		AssetUnitInfo baseAssetUnitInfo { get; }
	}
	public interface AssetBasicIO {
		ImmediatePicker<AssetUnitBasicIO, AssetUnitInfo> assetPicker { get; }
		string LocalizedAssetRef(AssetUnitInfo sourceAsset);
	}
	public class BasicAssetInParam<ContentType> {
		public ContentType content;
		public string contentPath;
		public bool doOverwrite = true;
	}
	#endregion
	#region asset interface
	public interface AssetReferInterface {
		void PickContent<ContentType>(string path, AsyncCollector<ContentType> collector);
	}
	public interface AssetModifyInterface {
		void SetContent<ContentType>(AssetContentSettingParam<ContentType> setParam, AssetInResultListener<ContentType> listener);
	}
	public class AssetContentSettingParam<ContentType> : BasicAssetInParam<ContentType> { }
	public interface AssetUnitInterface {
		AssetReferInterface referer { get; }
		AssetModifyInterface modifier { get; }
		AssetUnitInfo baseAssetInfo { get; }
	}
	public class StdAssetContentType {
		public readonly static string MAIN_CONTENT = "MainContent";
		public readonly static string ERWC = "ERWC";
		public readonly static string DICTOINARY = "Dictoinary";
		public readonly static string CUSTOM = "Custom";
	}
	#endregion
	#region asset request publisher
	public interface AssetRequestPublisher {
		void PublishAssetRequest(AssetRequest assetReq, SimpleProcessListener listener);
	}
	#endregion
	#region asset pick
	[System.Serializable]
	public class AssetPickUnit {
		public int reqID;
		public AssetInfo assetInfo;
	}
	[System.Serializable]
	public class AssetPick {
		public List<AssetPickUnit> units = new List<AssetPickUnit>();
	}
	#endregion
	#region asset info
	[System.Serializable]
	[DataContract]
	public class AssetUnitInfo {
		public int id;
		[DataMember(EmitDefaultValue = false)]
		public string reference;
		[DataMember(EmitDefaultValue = false)]
		public string distributor;
		[DataMember(EmitDefaultValue = false)]
		public string shortname;
		[DataMember(EmitDefaultValue = false)]
		public string assettype;
		[DataMember(EmitDefaultValue = false)]
		public string attributes;
		[DataMember(EmitDefaultValue = false)]
		public string lastupdate;
	}
	[System.Serializable]
	public class AssetInfo {
		public string packageType;
		public List<AssetUnitInfo> units;
	}
	#endregion
}