using System;
using AGAsset;
using AGDev;

namespace StdUnityAGDev.StdUtil {
    public class SQLiteLocalAssetPicker : AssetInfoDatabase {
        void Collector<AssetUnitInfo>.Collect(AssetUnitInfo item) {
            throw new NotImplementedException();
        }

        AssetUnitInfo ImmediatePicker<AssetUnitInfo, AssetUnitInfo>.PickBestElement(AssetUnitInfo key) {
            throw new NotImplementedException();
        }

        void AssetUnitSupplier.SupplyAssetUnit(AssetRequestUnit assetRequest, AssetUnitSupplyListener listener) {
            throw new NotImplementedException();
        }
    }
#if false
        [System.Serializable]
	public class SQLiteLocalAssetPicker : AssetInfoDatabase {
		//public string defaultFileTemplatePath;
		public string connectionString;
		IDbConnection dbconn {
			get {
				if (_dbconn == null)
					OpenDatabase();
				return _dbconn;
			}
		}
		IDbConnection _dbconn;
		string error;
		~SQLiteLocalAssetPicker() {
			CloseDatabase();
		}
		AssetUnitInfo ImmediatePicker<AssetUnitInfo, AssetUnitInfo>.PickBestElement(AssetUnitInfo key) {
			var sqlQueryBuilder = new System.Text.StringBuilder();
			sqlQueryBuilder.Append("SELECT * FROM assets WHERE sname = \'" + key.shortname + "\' And creatorref = \'" + key.distributor + "\'");
			bool hasCondition = false;
			IDbCommand dbcmd = dbconn.CreateCommand();
			dbcmd.CommandText = sqlQueryBuilder.ToString();
			IDataReader reader = dbcmd.ExecuteReader();
			AssetUnitInfo newAssetInfo = null;
			if (reader.Read()) {
				newAssetInfo = EditTimeAssetUtils.EntityToAssetInfo(reader).units[0];
			}
			reader.Close();
			reader = null;
			dbcmd.Dispose();
			dbcmd = null;
			return newAssetInfo;
		}
		public IDbConnection ConnectInernal(string databaseFilePath) {
			/*if (!Directory.Exists(Path.GetDirectoryName(databaseFilePath))) {
				Directory.CreateDirectory(Path.GetDirectoryName(databaseFilePath));
			}
			if (!File.Exists(databaseFilePath)) {
				File.Copy(databaseFileTemplatePath, databaseFilePath);
			}*/
			CloseDatabase();
			//return (IDbConnection) new SqliteConnection("URI=file:" + databaseFilePath);
			return null;
		}
		private void OpenDatabase() {
			_dbconn = ConnectInernal(connectionString);
			_dbconn.Open();
		}
		public void CloseDatabase() {
			//SqliteConnection.ClearAllPools();
			if (_dbconn != null) {
				_dbconn.Close();
				_dbconn = null;
				GC.Collect();
			}
		}
		AssetUnitInfo RequestToAssetUnit(AssetRequestUnit reqUnit) {
			var sqlQueryBuilder = new System.Text.StringBuilder();
			sqlQueryBuilder.Append("SELECT * FROM assets WHERE ");
			bool hasCondition = false;
			Action ActionBeforeAddCondition = () => {
				if (hasCondition == false)
					hasCondition = true;
				else
					sqlQueryBuilder.Append(" And ");
			};
			if (reqUnit.attributes != null) {
				foreach (var attrbiute in reqUnit.attributes) {
					ActionBeforeAddCondition();
					sqlQueryBuilder.Append("(");
					sqlQueryBuilder.Append("attributes = \'" + attrbiute + "\' OR ");
					sqlQueryBuilder.Append("attributes = \'" + attrbiute + ";%\' OR ");
					sqlQueryBuilder.Append("attributes = \'%;" + attrbiute + ";%\' OR ");
					sqlQueryBuilder.Append("attributes = \'%;" + attrbiute + "\'");
					sqlQueryBuilder.Append(")");
				}
			}
			if (reqUnit.assettype != null) {
				ActionBeforeAddCondition();
				sqlQueryBuilder.Append("assettype LIKE \'%" + reqUnit.assettype + "%\'");
			}
			if (reqUnit.creatorref != null) {
				ActionBeforeAddCondition();
				sqlQueryBuilder.Append("creatorref LIKE \'%" + reqUnit.creatorref + "%\'");
			}
			if (reqUnit.sname != null) {
				ActionBeforeAddCondition();
				sqlQueryBuilder.Append("sname LIKE \'%" + reqUnit.sname + "%\'");
			}
			IDbCommand dbcmd = dbconn.CreateCommand();
			dbcmd.CommandText = sqlQueryBuilder.ToString();
			IDataReader reader = dbcmd.ExecuteReader();
			AssetUnitInfo newAssetInfo = null;
			//asset found
			if (reader.Read()) {
				newAssetInfo = EditTimeAssetUtils.EntityToAssetUnitInfo(reader);
			}
			reader.Close();
			reader = null;
			dbcmd.Dispose();
			dbcmd = null;
			return newAssetInfo;
		}
		public void AddAssetToDatabase(AssetUnitInfo localizedAssetUnitInfo) {
			try {
				var equivelent = (this as ImmediatePicker<AssetUnitInfo, AssetUnitInfo>).PickBestElement(localizedAssetUnitInfo);
				if(equivelent != null)
					return;
				var sqlInsertBuilder = new System.Text.StringBuilder();
				var sqlInsertVarBuilder = new System.Text.StringBuilder();
				var sqlInsertValueBuilder = new System.Text.StringBuilder();
				sqlInsertVarBuilder.Append("localref, creatorref, sname, lastupdate");
				sqlInsertValueBuilder.Append("\'" + localizedAssetUnitInfo.reference + "\'");
				sqlInsertValueBuilder.Append(",\'" + localizedAssetUnitInfo.distributor + "\'");
				sqlInsertValueBuilder.Append(",\'" + localizedAssetUnitInfo.shortname + "\'");
				sqlInsertValueBuilder.Append(",\'" + localizedAssetUnitInfo.lastupdate + "\'");
				if (localizedAssetUnitInfo.assettype != null) {
					sqlInsertVarBuilder.Append(", assettype");
					sqlInsertValueBuilder.Append(",\'" + localizedAssetUnitInfo.assettype + "\'");
				}
				if (localizedAssetUnitInfo.attributes != null) {
					sqlInsertVarBuilder.Append(", attributes");
					sqlInsertValueBuilder.Append(",\'" + localizedAssetUnitInfo.attributes + "\'");
				}
				sqlInsertBuilder.Append(
					"INSERT INTO assets(" + sqlInsertVarBuilder.ToString() + ") VALUES (" + sqlInsertValueBuilder.ToString() + ")"
				);
				string insertString = sqlInsertBuilder.ToString();
				Debug.Log(insertString);
				IDbCommand dbcmd = dbconn.CreateCommand();
				dbcmd.CommandText = insertString;
				dbcmd.ExecuteNonQuery();
				dbcmd.Dispose();
				dbcmd = null;
			}
			catch (Exception e) {
				error = e.ToString();
			}
		}

		void Collector<AssetUnitInfo>.Collect(AssetUnitInfo newElement) {
			AddAssetToDatabase(newElement);
		}

		void AssetUnitSupplier.SupplyAssetUnit(AssetRequestUnit assetRequest, AssetUnitSupplyListener listener) {
			var assetUnit = RequestToAssetUnit(assetRequest);
			if(assetUnit != null)
				listener.supplyCollector.Collect(assetUnit);
			listener.supplyCollector.OnFinish();
		}
	}
#endif
}