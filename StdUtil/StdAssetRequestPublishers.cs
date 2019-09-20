using System.Collections.Generic;
using AGDev;

namespace AGAsset.StdUtil {
	#region Taiga IO
	[System.Serializable]
	public class TaigaIOAuth {
		public string username;
		public string password;
		public string type;
	}
	[System.Serializable]
	public class TaigaIOUserInfo {
		public string auth_token;
	}
	[System.Serializable]
	public class TaigaIOUserStory {
		public string subject;
	}
	[System.Serializable]
	public class TaigaIOUserStoryAdd {
		public string subject;
		public int project;
	}
	[System.Serializable]
	public class TaigaIOAssetRequestPublisher : AssetRequestPublisher {
		public string username;
		public string password;
		public int projectID;
		// Use this for initialization
		void AssetRequestPublisher.PublishAssetRequest(AssetRequest assetReq, SimpleProcessListener listener) {
			RequiredFuncs.ProcessHTTP(
				"https://api.taiga.io/api/v1/auth",
				(response) => {
					var auth = RequiredFuncs.FromJson<TaigaIOUserInfo>(response);
					var headerWithAuth = new KeyValuePair<string, string>[] {
						new KeyValuePair<string, string>("Content-Type", "application/json"),
						new KeyValuePair<string, string>("Authorization", "Bearer " + auth.auth_token)
					};
					RequiredFuncs.ProcessHTTP(
						"https://api.taiga.io/api/v1/userstories?project=" + projectID,
						(storiesJson) => {
							var stories = RequiredFuncs.FromJsonToArray<TaigaIOUserStory>(storiesJson);
							foreach(var reqUnit in assetReq.units) {
								if (reqUnit.attributes.Count == 0)
									continue;
								var storySubjForReq = "Asset Wanted: " + reqUnit.attributes[0] + " " + reqUnit.assettype;
								bool shouldPublishAssetReq = true;
								foreach (var story in stories) {
									if (story.subject == storySubjForReq) {
										shouldPublishAssetReq = false;
										break;
									}
								}
								if (shouldPublishAssetReq) {
									RequiredFuncs.ProcessHTTP(
										"https://api.taiga.io/api/v1/userstories",
										(addStoryResponseText) => {
											RequiredFuncs.Log(RequiredFuncs.ToJsonString(new TaigaIOUserStoryAdd { subject = storySubjForReq, project = projectID }));
										},
										headerWithAuth,
										RequiredFuncs.ToJson(new TaigaIOUserStoryAdd { subject = storySubjForReq, project = projectID })
									);
								}
							}
							listener.OnFinish(true);
						},
						headerWithAuth
					);
				},
				new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("Content-Type", "application/json") },
				RequiredFuncs.ToJson(new TaigaIOAuth {
					username = username, password = password, type = "normal"
				})
			);
		}
	}
	#endregion
}
