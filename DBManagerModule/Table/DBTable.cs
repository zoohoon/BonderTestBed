using LogModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBManagerModule.Table
{
    using System.Data;
    using System.Data.Common;
    public abstract class DBTable
    {
        public DBTable(String tableName, DBController dbController)
        {
            TableName = tableName;
            Controller = dbController;
            InitRecordColumTemplate();//==> PrimaryKey 지정하기 전에 초기화 해야 함
        }
        //==> Column Key : Column Caption
        public Dictionary<String, TableColumn> ColumTemplate { get; set; }
        public DBController Controller { get; set; }
        public String TableName { get; set; }
        public String PrimaryKeyColumn { get; set; }
        public DbType PrimaryKeyColumnType { get; set; }
        public abstract void InitRecordColumTemplate();
        public virtual DBStatus Open()
        {
            if (IsExistsTable())
                return DBStatus.TableExist;

            if (CreateTable() == false)
                return DBStatus.TableError;

            return DBStatus.TableExist;
        }
        //==> 
        public bool CreateTable()
        {
            StringBuilder query = new StringBuilder();
            query.Append($"CREATE TABLE {TableName} (");
            foreach (var columnTemp in ColumTemplate)
            {
                TableColumn tableColumn = columnTemp.Value;

                String type = String.Empty;
                switch (tableColumn.Type)
                {
                    case DbType.String:
                        type = $"NVARCHAR({tableColumn.Length})";
                        break;
                    case DbType.Int32:
                        type = "INT";
                        break;
                    case DbType.Double:
                        type = "FLOAT";
                        break;
                }
                //==> [TODO] 특정 데이터 공급자에 종속적인 쿼리문 같다 ( [] 사용 )
                //query.Append(String.Format("[{0}] {1}, ", columnTemp.Value.ColumnCaption, type));
                query.Append($"[{tableColumn.ColumnCaption}] {type} ");
                if (tableColumn.AutoIncrement)
                    query.Append($"IDENTITY(1,1)");
                query.Append(",");
            }
            query.Append($"PRIMARY KEY ([{PrimaryKeyColumn}]))");
            #region ==> QUERY SAMPLE
            //"CREATE TABLE TestTable (
            //  `DataID` varchar(50), `Index` varchar(50), `VariableName` varchar(50), `VariableName2` varchar(50), 
            //  `Category` varchar(50), `Data Name` varchar(50), `Description` varchar(50), 	`AvailableValue` varchar(50), 
            //  `Unit` varchar(50), `Permission1` varchar(50), `Permission2` varchar(50), `Permission3` varchar(50), `Masking` varchar(50), 
            //  `Status` varchar(50), `FormID` varchar(50), `FileName` varchar(50), `Section` varchar(50), `Key` varchar(50), 
            //  `Parse` varchar(50), `FileName2` varchar(50), `Section2` varchar(50), `Key2` varchar(50), `Parse2` varchar(50), 
            //  `ReadType` varchar(50), `VariableType` varchar(50), `ReturnLength` varchar(50), `ReturnUnit` varchar(50), `DataValue` varchar(50), 
            //  PRIMARY KEY(`DataID`))"
            #endregion

            //==> 기존 Table Name을 가진 Table이 존재할 경우 Error
            String createTableQuery = query.ToString();
            return Controller.DoNonQuery(createTableQuery);
        }
        public bool IsExistsTable()
        {
            bool isExistTable = false;
            String query = $"SELECT * FROM sys.tables";
            //==> Execute Query
            using (DbDataReader reader = Controller.DoReaderQuery(query))
            {
                if (reader == null)
                {
                    isExistTable = false;
                }
                else
                {
                    //==> Check Table Exists
                    while (reader.Read())
                    {
                        //==> DB에 있는 Table 이름들 열거
                        if (reader.GetString(0).ToLower() == TableName.ToLower())
                        {
                            isExistTable = true;
                            break;
                        }
                    }
                }
            }
            return isExistTable;
        }
        public bool DropTable()
        {
            String query = String.Format($"DROP TABLE {TableName}");
            #region ==> QUERY SAMPLE
            //"DROP TABLE TestTable"
            #endregion

            return Controller.DoNonQuery(query);
        }
        public bool IsExistRecord(Object primaryKey, out bool isExist)
        {
            return IsExistFiled(PrimaryKeyColumn, primaryKey, out isExist);
        }
        public bool IsExistFiled(String searchColumnName, Object searchColumnValue, out bool isExist)
        {
            isExist = false;

            //==> Table에 이미 똑같은 Record ID를 가진 Record 개수를 얻어옴, 일반적으로 0(존재X) 아니면 1(존재O)을 반환
            String checkExistsQuery = $"SELECT COUNT({searchColumnName}) FROM {TableName} WHERE {searchColumnName} = @ColumnValue";
            #region ==> QUERY SAMPLE
            //"SELECT COUNT(DataID) FROM TestTable WHERE DataID = @PrimaryKey"
            #endregion

            String fieldValue = String.Empty;
            DbParameter primaryKeyParam = Controller.CreateQueryParameter();
            primaryKeyParam.ParameterName = "@ColumnValue";
            primaryKeyParam.Value = searchColumnValue;
            primaryKeyParam.DbType = PrimaryKeyColumnType;

            Object intValue = Controller.DoExecuteScalar(checkExistsQuery, primaryKeyParam);
            if (intValue == null)
                return false;

            int matchQueryCount = 0;
            if (int.TryParse(intValue.ToString(), out matchQueryCount) == false)
                return false;

            isExist = matchQueryCount > 0;

            return true;
        }
        public bool GetColumnMax(String columnName, out int maxValue)
        {
            maxValue = 0;

            String checkExistsQuery = $"SELECT MAX({columnName}) FROM {TableName}";

            Object intValue = Controller.DoExecuteScalar(checkExistsQuery, new DbParameter[] { });
            if (intValue == null)
                return false;

            int matchQueryCount = 0;
            if (int.TryParse(intValue.ToString(), out matchQueryCount) == false)
                return false;

            maxValue = matchQueryCount;
            return true;

        }
        public bool InsertRecord(Object primaryKey)
        {
            String query = $"INSERT INTO {TableName} ({PrimaryKeyColumn}) VALUES ('{primaryKey}')";

            #region ==> QUERY SAMPLE
            //"INSERT INTO TestTable (DataID) VALUES ('7') "
            #endregion

            //==> 기존 recordID를 가진 Record가 존재하는 경우 Error
            return Controller.DoNonQuery(query);
        }
        public bool InsertRecord(Object primaryKey, String[] columnArr, Object[] valueArr)
        {
            String columnPart = String.Empty;
            foreach (Object column in columnArr)
            {
                columnPart += column + " ,";
            }
            columnPart = columnPart.Remove(columnPart.Length - 1);


            String valuePart = String.Empty;
            foreach (Object value in valueArr)
            {
                valuePart += $"'{value}' ,";
            }
            valuePart = valuePart.Remove(valuePart.Length - 1);
            

            String query = $"INSERT INTO {TableName} ({columnPart}) VALUES ({valuePart})";

            #region ==> QUERY SAMPLE
            //"INSERT INTO TestTable (DataID) VALUES ('7') "
            #endregion

            //==> 기존 recordID를 가진 Record가 존재하는 경우 Error
            return Controller.DoNonQuery(query);
        }
        public String ReadField(Object primaryKey, String columnName)
        {
            String query = $"SELECT {columnName} FROM {TableName} WHERE {PrimaryKeyColumn} = @PrimaryKey";
            #region ==> QUERY SAMPLE
            //"SELECT columnName FROM TestTable WHERE DataID = "ID00001""
            #endregion

            String fieldValue = String.Empty;
            DbParameter primaryKeyParam = Controller.CreateQueryParameter();
            primaryKeyParam.ParameterName = "@PrimaryKey";
            primaryKeyParam.Value = primaryKey;
            primaryKeyParam.DbType = PrimaryKeyColumnType;

            using (DbDataReader dr = Controller.DoReaderQuery(query, primaryKeyParam))
            {
                if (dr == null)
                {
                    fieldValue = String.Empty;
                }
                else
                {
                    if (dr.Read() == false)
                        fieldValue = String.Empty;
                    else
                        fieldValue = dr[columnName].ToString();
                }
            }

            #region ==> Non Param Query Way
            //String query = String.Format("SELECT {0} FROM {1} WHERE {2} = '{3}'",
            //    column, TableName, PrimaryKeyColumn, recordID);
            //#region ==> QUERY SAMPLE
            ////"SELECT * FROM TestTable WHERE DataID = "ID00001""
            //#endregion

            //String fieldValue = String.Empty;
            //using (DbDataReader dr = Controller.DoReaderQuery(query))
            //{
            //    dr.Read();
            //    fieldValue = dr[column].ToString();
            //}
            //return fieldValue;
            #endregion

            return fieldValue;
        }
        public String ReadField(String searchColumnName, Object searchColumnValue, String readColumnName)
        {
            String query = $"SELECT {readColumnName} FROM {TableName} WHERE {searchColumnName} = @ColumnValue";
            #region ==> QUERY SAMPLE
            //"SELECT columnName FROM TestTable WHERE DataID = "ID00001""
            #endregion

            String fieldValue = String.Empty;
            DbParameter primaryKeyParam = Controller.CreateQueryParameter();
            primaryKeyParam.ParameterName = "@ColumnValue";
            primaryKeyParam.Value = searchColumnValue;
            primaryKeyParam.DbType = PrimaryKeyColumnType;

            using (DbDataReader dr = Controller.DoReaderQuery(query, primaryKeyParam))
            {
                if (dr == null)
                {
                    fieldValue = String.Empty;
                }
                else
                {
                    if (dr.Read() == false)
                        fieldValue = String.Empty;
                    else
                        fieldValue = dr[readColumnName].ToString();
                }

            }

            #region ==> Non Param Query Way
            //String query = String.Format("SELECT {0} FROM {1} WHERE {2} = '{3}'",
            //    column, TableName, PrimaryKeyColumn, recordID);
            //#region ==> QUERY SAMPLE
            ////"SELECT * FROM TestTable WHERE DataID = "ID00001""
            //#endregion

            //String fieldValue = String.Empty;
            //using (DbDataReader dr = Controller.DoReaderQuery(query))
            //{
            //    dr.Read();
            //    fieldValue = dr[column].ToString();
            //}
            //return fieldValue;
            #endregion

            return fieldValue;
        }
        public Dictionary<String, String> ReadField(Object primaryKey, params String[] columnNameList)
        {
            String columnName = String.Empty;
            foreach (String column in columnNameList)
                columnName += $"{column} ,";
            int lastCommaIdx = columnName.LastIndexOf(',');
            columnName = columnName.Remove(lastCommaIdx);

            String query = $"SELECT {columnName} FROM {TableName} WHERE {PrimaryKeyColumn} = @PrimaryKey";
            #region ==> QUERY SAMPLE
            //"SELECT columnName1, columnName2 FROM TestTable WHERE DataID = "ID00001""
            #endregion

            Dictionary<String, String> fieldValue = new Dictionary<String, String>();
            DbParameter primaryKeyParam = Controller.CreateQueryParameter();
            primaryKeyParam.ParameterName = "@PrimaryKey";
            primaryKeyParam.Value = primaryKey;
            primaryKeyParam.DbType = PrimaryKeyColumnType;

            using (DbDataReader dr = Controller.DoReaderQuery(query, primaryKeyParam))
            {
                if (dr == null)
                {
                    fieldValue = null;
                }
                else
                {
                    if (dr.Read() == false)
                        fieldValue = null;
                    else
                    {
                        foreach (String column in columnNameList)
                            fieldValue.Add(column, dr[column].ToString());
                    }
                }
                dr?.Close();
            }
            return fieldValue;
        }
        public DbDataReader GetDataReaderByQuery()
        {
            String query = $"SELECT * FROM {TableName}";
            DbDataReader dr = Controller.DoReaderQuery(query);
            return dr;
        }
        public bool UpdateField(Object primaryKey, String columnName, DbType columnType, Object fieldValue)
        {
            String query = $"UPDATE {TableName} SET {columnName} = @Field WHERE {PrimaryKeyColumn} = @PrimaryKey";
            #region ==> QUERY SAMPLE
            //"UPDATE TestTable SET VariableName = @Column WHERE DataID = "ID00001""
            #endregion

            DbParameter primaryKeyParam = Controller.CreateQueryParameter();
            primaryKeyParam.ParameterName = "@PrimaryKey";
            primaryKeyParam.Value = primaryKey;
            primaryKeyParam.DbType = PrimaryKeyColumnType;

            DbParameter columnParam = Controller.CreateQueryParameter();
            columnParam.ParameterName = "@Field";
            columnParam.Value = fieldValue;
            columnParam.DbType = columnType;

            return Controller.DoNonQuery(query, primaryKeyParam, columnParam);

            #region ==> Non Param Query Way
            //String query = String.Format("UPDATE {0} SET {1} = '{2}' WHERE {3} = '{4}'",
            //    TableName, column, fieldValue, PrimaryKeyColumn, recordID);
            //#region ==> QUERY SAMPLE
            ////"UPDATE TestTable SET VariableName = @Column WHERE DataID = "ID00001""
            //#endregion
            //return Controller.DoNonQuery(query);
            #endregion
        }

        public bool UpdateTargetInfoValue(String keyField, String keyValue , String targetField,  String setValue)
        {
            //UPDATE SystemParameter
            //SET UpperLimit = 100
            //WHERE PropertyPath = 'MotionManager.LoaderAxes.ProbeAxisObject[0].AxisIndex'

            
            String query = $"UPDATE {TableName} SET {targetField} = {setValue} WHERE {keyField} = '{keyValue}'";
            #region ==> QUERY SAMPLE
            //"UPDATE TestTable SET VariableName = @Column WHERE DataID = "ID00001""
            #endregion

            return Controller.DoNonQuery(query);

            #region ==> Non Param Query Way
            //String query = String.Format("UPDATE {0} SET {1} = '{2}' WHERE {3} = '{4}'",
            //    TableName, column, fieldValue, PrimaryKeyColumn, recordID);
            //#region ==> QUERY SAMPLE
            ////"UPDATE TestTable SET VariableName = @Column WHERE DataID = "ID00001""
            //#endregion
            //return Controller.DoNonQuery(query);
            #endregion
        }


        public bool DeleteRecord(Object primaryKey)
        {
            String query = $"DELETE FROM {TableName} WHERE {PrimaryKeyColumn} = @PrimaryKey";
            #region ==> QUERY SAMPLE
            //"DELETE FROM TestTable WHERE DataID = "ID00001""
            #endregion

            DbParameter primaryKeyParam = Controller.CreateQueryParameter();
            primaryKeyParam.ParameterName = "@PrimaryKey";
            primaryKeyParam.Value = primaryKey;
            primaryKeyParam.DbType = PrimaryKeyColumnType;

            return Controller.DoNonQuery(query, primaryKeyParam);
        }

        //==> 지정된 컬럼의 Field들을 List로 반환
        public List<String> GetAllFieldByColumn(String columnName)
        {
            String query = $"SELECT {columnName} FROM {TableName}";
            List<String> keyList = new List<String>();
            using (DbDataReader dr = Controller.DoReaderQuery(query))
            {
                if (dr == null)
                {
                    keyList = null;
                }
                else
                {
                    while (dr.Read())
                    {
                        String key = dr[columnName].ToString();
                        keyList.Add(key);
                    }
                }
            }
            return keyList;
        }
    }

    public class TableColumn
    {
        public DbType Type { get; set; }
        public String ColumnCaption { get; set; }
        public int Length { get; set; }
        public bool AutoIncrement { get; set; }
        public TableColumn(DbType type, String columnCaption, int length = 50, bool autoIncrement = false)
        {
            try
            {
                Type = type;
                ColumnCaption = columnCaption;
                Length = length;
                AutoIncrement = autoIncrement;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
