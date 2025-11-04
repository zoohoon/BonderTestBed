using LogModule;
using System;

namespace DBManagerModule
{
    using DBManagerModule.Table;
    using ProberErrorCode;
    using SerializerUtil;
    using System.IO;

    //==> Table 관리
    //==> 사용자의 요청에 적절한 Table을 선택하여 Table의 기능 호출

    /*
     * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
     * DBManager에서 ProberInterface를 상속을 제거하고 싶다면 Account
     * (LoginDataList Table에 존재하는) 타입을 제거하라.
     * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    */
    public static class DBManager
    {
        private static DBController _Controller;//==> Manager에서는 딱히 사용할 필요가 없을 듯
        public static DBController Controller
        {
            get { return _Controller; }
        }
        public static String DBDirectoryPath { get; } = @"C:\ProberSystem\DB";
        public static ParamElementTable SystemParameter { get; set; }
        public static ParamElementTable DeviceParameter { get; set; }
        public static ParamElementTable CommonParameter { get; set; }
        public static EventInfoTable EventInfo { get; set; }
        public static LoginDataListTable LoginDataList { get; set; }
        public static BulkUploader SystemBulkUploader { get; set; }
        public static BulkUploader DeviceBulkUploader { get; set; }
        public static BulkUploader CommonBulkUploader { get; set; }

        public static DBManagerParameter DBManagerParam { get; set; }
        public static CustomizedElementListParameter CustomizedElementListParam { get; set; }
        public static bool Open()
        {
            DBStatus dbStatus = DBStatus.IsOK;

            try
            {

                do
                {
                    LoadDBManagerParameter();
                    LoadCustomizedElementParameter();

                    _Controller = new DBController(
                        DBManagerParam.DBName,
                        DBManagerParam.ServerConnectionStr,
                        DBManagerParam.DBConnectionStr,
                        DBManagerParam.ProviderName);

                    dbStatus = _Controller.Open();
                    if (dbStatus == DBStatus.CreatDBError)
                        break;
                    if (dbStatus == DBStatus.ConnectDBError)
                        break;
                    //======== Don;t write the Code above this Line =====

                    //String tableName = System.Configuration.ConfigurationManager.AppSettings["TestTableName"].ToString();
                    //DataList = new DataListTable(tableName, _Controller);

                    //Parameter = new ParameterTable(nameof(Parameter), _Controller);
                    //dbStatus = Parameter.Open();
                    //if (dbStatus == DBStatus.CreateTableError)
                    //    break;

                    EventInfo = new EventInfoTable(nameof(EventInfo), _Controller);
                    dbStatus = EventInfo.Open();
                    if (dbStatus == DBStatus.TableError)
                        break;

                    SystemParameter = new ParamElementTable(nameof(SystemParameter), _Controller, 10000000);
                    dbStatus = SystemParameter.Open();
                    if (dbStatus == DBStatus.TableError)
                        break;

                    DeviceParameter = new ParamElementTable(nameof(DeviceParameter), _Controller, 20000000);
                    dbStatus = DeviceParameter.Open();
                    if (dbStatus == DBStatus.TableError)
                        break;

                    CommonParameter = new ParamElementTable(nameof(CommonParameter), _Controller, 30000000);
                    dbStatus = CommonParameter.Open();
                    if (dbStatus == DBStatus.TableError)
                        break;

                    LoginDataList = new LoginDataListTable(nameof(LoginDataList), _Controller);
                    dbStatus = LoginDataList.Open();
                    if (dbStatus == DBStatus.TableError)
                        break;
                    LoginDataList.SetupSuperUser();


                    //==> Bulk Add
                    //SqlConnection sqlConnection = _Controller.DbConnection as SqlConnection;
                    //if (sqlConnection != null)
                    //{
                    //    SystemBulkUploader = new BulkUploader(
                    //        sqlConnection,
                    //        nameof(SystemParameter),
                    //        SystemParameter.ColumTemplate,
                    //        SystemParameter.PrimaryKeyColumn);

                    //    DeviceBulkUploader = new BulkUploader(
                    //        sqlConnection,
                    //        nameof(DeviceParameter),
                    //        DeviceParameter.ColumTemplate,
                    //        DeviceParameter.PrimaryKeyColumn);

                    //    CommonBulkUploader = new BulkUploader(
                    //        sqlConnection,
                    //        nameof(CommonParameter),
                    //        CommonParameter.ColumTemplate,
                    //        CommonParameter.PrimaryKeyColumn);
                    //}
                } while (false);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return dbStatus == DBStatus.IsOK;
        }
        public static EventCodeEnum LoadDBManagerParameter()
        {
            DBManagerParameter defaultParamObject = new DBManagerParameter();
            defaultParamObject.SetDefaultParam();

            String filePath = Path.Combine(DBDirectoryPath, "DBManagerParam.json");

            DBManagerParam = LoadParameter(filePath, defaultParamObject, typeof(DBManagerParameter)) as DBManagerParameter;
            if (DBManagerParam == null)
            {
                LoggerManager.Error("Load DB Manager Parameter error");
                return EventCodeEnum.UNDEFINED;
            }

            return EventCodeEnum.NONE;
        }
        public static EventCodeEnum LoadCustomizedElementParameter()
        {
            CustomizedElementListParameter defaultParamObject = new CustomizedElementListParameter();
            defaultParamObject.SetDefaultParam();

            String filePath = Path.Combine(DBDirectoryPath, "CustomizedElementListParam.json");

            CustomizedElementListParam = LoadParameter(filePath, defaultParamObject, typeof(CustomizedElementListParameter)) as CustomizedElementListParameter;
            if (CustomizedElementListParam == null)
            {
                LoggerManager.Error("Load Customized Element List Parameter error");
                return EventCodeEnum.UNDEFINED;
            }

            CustomizedElementListParam.BuildCustomizeElementSet();

            return EventCodeEnum.NONE;
        }
        public static bool SaveDBManagerParameter()
        {
            String filePath = Path.Combine(DBDirectoryPath, "DBManagerParam.json");

            if (SaveParameter(filePath, DBManagerParam) != EventCodeEnum.NONE)
            {
                LoggerManager.Error("Save DB Manager Parameter error");
                return false;
            }

            return true;
        }
        public static bool SaveCustomizedElementParameter()
        {
            String filePath = Path.Combine(DBDirectoryPath, "CustomizedElementListParam.json");

            if (SaveParameter(filePath, CustomizedElementListParam) != EventCodeEnum.NONE)
            {
                LoggerManager.Error("Save Customized Element List Parameter error");
                return false;
            }

            return true;
        }

        private static Object LoadParameter(String filePath, Object defaultParamObject, Type type)
        {
            Object deserializedObj = null;
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                if (File.Exists(filePath) == false)
                {
                    if (SerializeManager.Serialize(filePath, defaultParamObject) == false)
                    {
                        return null;
                    }
                }

                if (SerializeManager.Deserialize(filePath, out deserializedObj, type) == false)
                {
                    return null;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return deserializedObj;
        }
        private static EventCodeEnum SaveParameter(String filePath, Object paramObject)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                if (SerializeManager.Serialize(filePath, paramObject) == false)
                {
                    return EventCodeEnum.PARAM_ERROR;
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }
        public static void Dispose()
        {
            if (_Controller == null)
                return;
            _Controller.Dispose();
        }
    }
}
