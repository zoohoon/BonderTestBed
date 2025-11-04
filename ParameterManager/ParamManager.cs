using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParameterManager
{
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using System.Reflection;
    using System.Collections;
    using ProberErrorCode;
    using DBManagerModule;
    using DBManagerModule.Table;
    using CsvUtil;
    using System.Data;
    using System.IO;
    using LogModule;
    using System.Data.Common;
    using System.ServiceModel;
    using SerializerUtil;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Windows.Forms;
    using System.Security.Cryptography;
    using System.Runtime.InteropServices;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ParamManager : IParamManager
    {
        public bool Initialized { get; set; } = false;

        private ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> _DevDBElementDictionary = new ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>>(StringComparer.OrdinalIgnoreCase);
        public ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> DevDBElementDictionary
        {
            get { return _DevDBElementDictionary; }
            set { _DevDBElementDictionary = value; }
        }
        private ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> _SysDBElementDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<String, IElement>>(StringComparer.OrdinalIgnoreCase);
        public ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> SysDBElementDictionary
        {
            get { return _SysDBElementDictionary; }
            set { _SysDBElementDictionary = value; }
        }
        private ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> _CommonDBElementDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<String, IElement>>(StringComparer.OrdinalIgnoreCase);
        public ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> CommonDBElementDictionary
        {
            get { return _CommonDBElementDictionary; }
            set { _CommonDBElementDictionary = value; }
        }

        private ParamDevParameter _ParamDevParam;

        public ParamDevParameter ParamDevParam
        {
            get { return _ParamDevParam; }
            set { _ParamDevParam = value; }
        }

        private AlternateSysParameter _AltSysParam;

        public AlternateSysParameter AltSysParam
        {
            get { return _AltSysParam; }
            set { _AltSysParam = value; }
        }
        public HashSet<String> CustomizeElementSet { get; set; } = new HashSet<String>();

        public event EventHandler OnLoadElementInfoFromDB = null;

        public bool ChangedDeviceParam = true;
        public bool ChangedSystemParam = true;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //==> DB File 이 Update 되었을 경우에만 호출 된다.
        public void RegistElementToDB()
        {
            LoggerManager.Debug($"Register Element Option : {DBManager.DBManagerParam.EnableDBSourceServer}");

            if (DBManager.DBManagerParam.EnableDBSourceServer == false)
                return;

            Func<ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>>, ParamElementTable, bool> insertRecord = (elementDic, table) =>
            {
                foreach (var item in elementDic)
                {
                    String dbPropertyPath = item.Key;

                    //==> Shared Property path를 가지는 Element는 첫번째 Element만 메타 데이터를 채우면 된다.
                    IElement firstElement = item.Value.FirstOrDefault().Value;

                    bool isExistRecord = false;
                    if (table.IsExistRecord(dbPropertyPath, out isExistRecord) == false)
                    {
                        //==> query 실패
                        continue;
                    }

                    if (isExistRecord)
                    {
                        //==> TODO : 이 처리는 Element의 메타 데이터를 소스 코드에서 모두 기록 했을 때 실용적이다.
                        //==> 지금은 많은 Element들이 소스 코드에 메타 데이터를 추가하지 않았 음으로 DB Table을 지워서 다시 생성하게 되면
                        //==> 항상 Table이 EMPTY로 날라가 버린다. 따라서 많은 사람들이 소스 코드에 메타 데이터를 기록 하였을 때
                        //==> 이 코드의 주석을 해제해 주면 소스 코드에서 메타 데이터가 수정 되었을 때 바로 DB에 적용되어서 유용하게 사용할 수 있다.

                        //String elementID = table.ReadField(propertyPath, PramElementColumn.ElementID);

                        //==> 기존에 Table에 존재하는 Element Record를 지운다
                        //table.DeleteRecord(propertyPath);

                        //==> 사용자가 지정한 Element Record를 Table에 새로 채운다.
                        //table.InsertRecord(
                        //    propertyPath,
                        //    elementID.ToString(),
                        //    firstElement.ElementName,
                        //    firstElement.ElementAdmin,
                        //    firstElement.AssociateElementID,
                        //    firstElement.CategoryID,
                        //    firstElement.Unit,
                        //    firstElement.LowerLimit.ToString(),
                        //    firstElement.UpperLimit.ToString(),
                        //    firstElement.ReadMaskingLevel.ToString(),
                        //    firstElement.WriteMaskingLevel.ToString(),
                        //    firstElement.Description);
                    }
                    else
                    {
                        //==> 소스 코드에서 추가된 Element를 ID 자동 부여해서 등록
                        //==> DB Table에 존재하는 Element ID는 피해서 ID를 자동 할당 한다.

                        if (firstElement.ElementID == -1)
                        {
                            table.InsertRecordAutoIncID(
                            dbPropertyPath,
                            firstElement.ElementName,
                            firstElement.ElementAdmin,
                            firstElement.AssociateElementID,
                            firstElement.CategoryID,
                            firstElement.Unit,
                            firstElement.LowerLimit.ToString(),
                            firstElement.UpperLimit.ToString(),
                            firstElement.ReadMaskingLevel.ToString(),
                            firstElement.WriteMaskingLevel.ToString(),
                            firstElement.Description,
                            firstElement.VID.ToString());
                        }
                        else if (firstElement.ElementID != -1)
                        {
                            table.InsertRecord(
                            dbPropertyPath,
                            firstElement.ElementID.ToString(),
                            firstElement.ElementName,
                            firstElement.ElementAdmin,
                            firstElement.AssociateElementID,
                            firstElement.CategoryID,
                            firstElement.Unit,
                            firstElement.LowerLimit.ToString(),
                            firstElement.UpperLimit.ToString(),
                            firstElement.ReadMaskingLevel.ToString(),
                            firstElement.WriteMaskingLevel.ToString(),
                            firstElement.Description,
                            firstElement.VID.ToString());
                        }

                        LoggerManager.Debug($"[Insert Element] : {dbPropertyPath}");
                    }
                }

                return true;
            };

            try
            {
                LoggerManager.Debug($"Register - System");
                insertRecord(SysDBElementDictionary, DBManager.SystemParameter);

                LoggerManager.Debug($"Register - Device");
                insertRecord(DevDBElementDictionary, DBManager.DeviceParameter);

                LoggerManager.Debug($"Register - Common");
                insertRecord(CommonDBElementDictionary, DBManager.CommonParameter);
            }
            catch (Exception err)
            {
                // 오류가 발생 시, db update 실패 메시지를 띄워주록 처리
                MessageBox.Show("DB parameter update failed.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoggerManager.Exception(err);
            }
        }
        public void SyncDBTableByCSV()
        {
            #region ==> Differential Import Algorithm
            //
            //- DB Table을 조사하면서 Csv에 저장된 Element Meta data와 다른 부분이 있으면 갱신하는 작업
            //
            //+------------------------------------------------------------------------------------+
            //| 1) DB File에 존재하는 Element의 ID들은 어떤 경우에서도 중복되지 않는다는 보장이 있다는 전제 하에
            //| 알고리즘이 동작한다.
            //|
            //| 2) Element ID는 DB Source Server에서 자동 할당 된다
            //|
            //| 3) Modify List : 사용자가 Customize 한 Element List
            //+------------------------------------------------------------------------------------+
            //
            //- Element ID 충돌 검사
            //
            //  [DB]                [Csv]
            //	- Not Collision -
            //		[A.A.A][0999]
            //		[A.A.B][1000] <- [A.A.B][1000]
            //		[A.A.C][1001]
            //      
            //    - DB 와 CSV가 같은 Meta Data를 가지고 있기에 Sync가 되어 있는 상황이다. 
            //    - 추가적인 작업이 필요 없으며 Skip

            //	- Collision Case 1 (Property Path까지 같이 충돌)
            //		[A.A.A][0999]
            //		[A.A.B][1000]   <- [A.A.B][1001]
            //	  ->[A.A.C][1001]<-
            //      
            //    - 쎄믹스 정책상 A.A.B의 Element ID가 1000 에서 1001로 바뀐 것이다.
            //    - A.A.B의 ID를 1001로 새로 갱신
            //    - A.A.C의 Element ID와 중복되는 문제는 CSV에 A.A.C의 새로운 ElementID가 분명 
            //    존재하고 이는 충돌되지 않은 Element ID이기에 A.A.C도 Update 되면서 문제 해결


            //	- Collision Case 2 (Property Path까지는 같이 충돌 되지 않음)
            //		[A.A.A][0999]
            //	  ->[A.A.B][1000]<-
            //		[A.A.C][1001]
            //					    <- [A.A.X][1000]
            //      
            //    - A.A.B가 더 이상 사용되지 않는 Element가 된 것이다.
            //    - A.A.B를 DB에서 삭제 한다.

            //- Record 충돌 검사
            //  - Record를 추가 하려고 하는데 Property Path가 일치하는 Record 존재함 O. 
            //  	[A.A.A]
            //  	[A.A.B] <- [A.A.B]
            //  	[A.A.C]

            //  	- 일치하는 Record를 Update해야 하는 상황.

            //  	- Update 하려는 Record가 Modify List에 존재하지 않음
            //  		- CSV에 있는 Record를 DB에 Update

            //  	- Update 하려는 Record가 Modify List에 존재
            //  		- DB File에 있는 Record를 DB에 Update 하지 않음
            //  		- 단 Element ID는 갱신 할 수 있기에 Update 해주어야 함.

            //  - Record를 추가 하려고 하는데 Property Path가 일치하는 Record가 존재하지 않음 X.
            //  	[A.A.A]
            //  	[A.A.B]
            //  	[A.A.C]
            //  		  	<- [A.A.X]

            //  	- 일치하는 Record가 없기에 Insert 해야 하는 상황
            //  	- Record가 DB에 없기에 Modify List에도 당연히 없다.
            //  	- Record를 추가하면 된다.

            //	- 충돌된 Element ID가 발생되는 이유
            //      - 주된 이유는 Property Path가 바뀌어서 이다, Property Path가 바뀜으로서 Element ID를 다시 재 조정하는 과정을 거친다.
            //		- 충돌된 Record가 더 이상 사용하지 않는 Element이다.
            //		- 충돌된 Record의 Element ID가 바뀌었다. 
            //			- 삭제 된것으로 간주한다. 그래서 장비 PC에서도 Element ID를 잃은 Record는 Customize 유무에 상관없이 삭제를 원칙으로 한다.
            //			- Element ID가 바뀐다는 것은 고객의 DB에 있는 Customize 된 Record가 삭제 될 수도 있다는 뜻이다.

            //	- DB Source Server의 DB에 Record가 추가될 때는 모든 Elemnet ID가 유일성을 보장 받는다(개발자가 아닌 자동 부여이기에...)	
            //	- Element ID가 충돌되는 시점은 DB Source Server에서 DB File을 Export하고 이를 다른 PC에 배포해서 Differential Import 하는 시점이다.
            //	- 다른 PC에서 구버전의 DB를 사용하기 때문이다. 구버전의 DB에는 찌꺼기가 있을 수 있다.

            //	- Element ID가 충돌되면 충돌이 발생한 Record를 삭제하고 DB File에 존재하는 최신의 Record를 추가 해야 한다.


            //- Element ID를 자주 바꿀 일은 없다, 그리고 "자주 바뀌어서도 안된다".
            //- 어떤 Record의 Element ID를 바꿈으로서 충돌이 발생하여 사용자가 Customize한 Record를 삭제한다
            //- Record가 없는 깡통 DB File을 넘기면 어떻게 될까?? 
            //	- 아무일도 안 일어날 것이다.
            //	- 이유는 DB File로 부터 Property Path가 존재하는 Record만 얻어와 DB와 비교해서 작업을 처리 한다.
            //- Property Path 는 Source Code 상에서의 Key 역할을 하며 Element ID 는 사용자나 개발자 입장에서 Key 역할을 한다.
            #endregion

            //==> 실행 PC가 DB Update 모드일 때만 가능.
            LoggerManager.Debug($"Sync Update DB option : {DBManager.DBManagerParam.EnableUpdateDBFile}");
            if (DBManager.DBManagerParam.EnableUpdateDBFile == false)
                return;

            Func<ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>>, ParamElementTable, CsvDicTypeReader, bool> ImportDB = (dbElementDictionary, table, csvReader) =>
            {
                Func<String, bool> updateRecord = (propPath) =>
                {
                    //==> Csv 파일(csvReader)를 사용하여 테이블의 필드 데이터를 갱신 
                    Func<String, String, DbType, bool> updateFieldByCsv = (key, column, dbType) =>
                    {
                        String csvFieldData = String.Empty;
                        if (csvReader.Data.ContainsKey(key) == false)
                        {
                            return false;
                        }
                        if (csvReader.Data[key].ContainsKey(column) == false)
                        {
                            return false;
                        }

                        csvFieldData = csvReader.Data[key][column];

                        //==> DB에 문자열 Empty로 Update 하지 않는다.
                        if (String.IsNullOrEmpty(csvFieldData))
                            return false;

                        //==> 이미 Excel의 데이터와 DB의 데이터가 같아서 Update 할 필요 없음.
                        String tableData = table.ReadField(key, column);
                        if (tableData == csvFieldData)
                            return false;

                        return table.UpdateField(key, column, dbType, csvFieldData);
                    };

                    bool isExistRecord = false;
                    if (table.IsExistRecord(propPath, out isExistRecord) == false)
                    {
                        //==> query 실패
                        return false;
                    }
                    if (csvReader.Data.ContainsKey(propPath) == false)
                    {
                        return false;
                    }

                    //==> CSV 로 부터 Record 데이터를 가져 옴
                    String elementId =
                        csvReader.Data[propPath][PramElementColumn.ElementID];
                    String elementName =
                        csvReader.Data[propPath][PramElementColumn.ElementName];
                    String elementAdmin =
                        csvReader.Data[propPath][PramElementColumn.ElementAdmin];
                    String associateElementID =
                        csvReader.Data[propPath][PramElementColumn.AssociateElementID];
                    String categoryID =
                        csvReader.Data[propPath][PramElementColumn.CategoryID];
                    String unit =
                        csvReader.Data[propPath][PramElementColumn.Unit];
                    String lowerLimit =
                        csvReader.Data[propPath][PramElementColumn.LowerLimit];
                    String upperLimit =
                        csvReader.Data[propPath][PramElementColumn.UpperLimit];
                    String readMaskingLevel =
                        csvReader.Data[propPath][PramElementColumn.ReadMaskingLevel];
                    String writeMaskingLevel =
                        csvReader.Data[propPath][PramElementColumn.WriteMaskingLevel];
                    String description =
                        csvReader.Data[propPath][PramElementColumn.Description];
                    String vid =
                        csvReader.Data[propPath][PramElementColumn.VID];

                    String collisonElemIdPropPath = table.ReadField(
                        searchColumnName: PramElementColumn.ElementID,
                        searchColumnValue: elementId,
                        readColumnName: PramElementColumn.PropertyPath);

                    //==> 알고리즘이 이렇게 복잡한 이유는 Local DB를 Customize 할 수 있다는 가정이 있기 때문이다.

                    if (collisonElemIdPropPath == String.Empty)
                    {
                        //==> Element ID가 충돌되지 않았다.
                        //(Not Collision)
                        //  (DB)              (CSV)
                        //	[A.A.A][0999]
                        //	[A.A.B][1000] 
                        //	[A.A.C][1001]
                        //                 <- [1002]
                    }
                    else if (collisonElemIdPropPath == propPath)
                    {
                        //==> Element ID가 충돌되지 않았다.
                        //(Not Collision)
                        //  (DB)              (CSV)
                        //	[A.A.A][0999]
                        //	[A.A.B][1000]  <- [A.A.B][1000]
                        //	[A.A.C][1001]
                    }
                    else
                    {
                        //==> Element ID가 충돌 되었다.

                        if (csvReader.Data.ContainsKey(collisonElemIdPropPath))
                        {
                            //(Collision Case 1)
                            //  (DB)              (CSV)
                            //	[A.A.A][0999]
                            //	[A.A.B][1000]  <- [A.A.B][1001]
                            //->[A.A.C][1001]<-

                            //==> 충돌된 Record의 Property Path가 CSV 에 존재함 (Case 1)

                            // CSV에 존재하는 충돌된 Element의 Elemenet ID를 Update 해줌으로서 충돌 회피
                            //  [A.A.C][1001] -> [A.A.C][1002]
                            updateFieldByCsv(collisonElemIdPropPath, PramElementColumn.ElementID, DbType.Int32);
                        }
                        else
                        {
                            //(Collision Case 2)
                            //  (DB)              (CSV)
                            //	[A.A.A][0999]
                            //->[A.A.B][1000]<-
                            //	[A.A.C][1001]
                            //				   <- [A.A.X][1000]

                            //==> 충돌된 Record의 Property Path가 CSV 에 존재 하지 않음 (Case 2)

                            // DB Source Server에서 삭제된 것이다. 따라서 충돌된 Record를 "DELETE" 처리 한다.
                            // [A.A.B][1000] <- DEL
                            table.DeleteRecord(collisonElemIdPropPath);
                        }
                    }


                    //==> Element ID 충돌 검사랑 나눠서 처리하는것이 편리
                    if (isExistRecord)
                    {
                        //==> Record를 추가 하려고 하는데 Property Path가 일치하는 Record 존재.
                        //(DB)       (CSV)
                        //[A.A.A]
                        //[A.A.B] <- [A.A.B]
                        //[A.A.C]

                        // 일치하는 Record를 Update 함

                        if (DBManager.CustomizedElementListParam.CheckElementExist(propPath))
                        {
                            //==> 사용자가 Customize 한 것이기에 Meta Data update 하면 안된다.

                            //==> 단 Element ID는 사용자가 Customize 못하고 DB File에서 바뀔 수 있기에 Element ID만 Update 해줌
                            updateFieldByCsv(propPath, PramElementColumn.ElementID, DbType.Int32);
                        }
                        else
                        {
                            //==> 사용자가 customize 하지 않았다.

                            //==> CSV 에 존재하는 Record의 Meta Data로 DB에 Update 한다.
                            table.DeleteRecord(propPath);
                            table.InsertRecord(
                                propPath,
                                elementId,
                                elementName,
                                elementAdmin,
                                associateElementID,
                                categoryID,
                                unit,
                                lowerLimit,
                                upperLimit,
                                readMaskingLevel,
                                writeMaskingLevel,
                                description,
                                vid);
                        }
                    }
                    else
                    {
                        //==> DB Table에 추가하려는 Record가 없기에 CSV에 존재하는 Record를 DB Table에 추가.
                        //(DB)       (CSV)
                        //[A.A.A]
                        //[A.A.B]
                        //[A.A.C]
                        //        <- [A.A.X]

                        // Record가 DB에 없기에 Customize Element List 에도 당연히 없다.

                        //==> Record 추가
                        table.InsertRecord(
                                propPath,
                                elementId,
                                elementName,
                                elementAdmin,
                                associateElementID,
                                categoryID,
                                unit,
                                lowerLimit,
                                upperLimit,
                                readMaskingLevel,
                                writeMaskingLevel,
                                description,
                                vid);
                    }
                    return true;
                };


                int totalCount = csvReader.Data.Keys.Count();
                int count = 0;
                foreach (String propPath in csvReader.Data.Keys)
                {
                    updateRecord(propPath);
                    ++count;
                    LoggerManager.Debug($"Progress : {count}/{totalCount}, {propPath}");
                }

                return true;
            };

            try
            {
                String devUpdateDate = File.GetLastWriteTime(CsvDicTypeReader.DEVPARAMETERDATA_FILENAME).ToString();
                String sysUpdateDate = File.GetLastWriteTime(CsvDicTypeReader.SYSPARAMETERDATA_FILENAME).ToString();
                String comUpdateDate = File.GetLastWriteTime(CsvDicTypeReader.COMMONPARAMETERDATA_FILENAME).ToString();

                bool isDBFileModify = false;

                LoggerManager.Debug($"[Device DB File Update] : {DBManager.DBManagerParam.DevParamUpdateDate}, [Last Update] : {devUpdateDate}");

                if (DBManager.DBManagerParam.DevParamUpdateDate != devUpdateDate)
                {
                    LoggerManager.Debug("-> Sync Device Table");
                    CsvDicTypeReader devCsvReader = new CsvDicTypeReader(CsvDicTypeReader.DEVPARAMETERDATA_FILENAME, PramElementColumn.PropertyPath);

                    DBManager.DeviceParameter.DropTable();
                    DBManager.DeviceParameter.CreateTable();

                    if (devCsvReader.Import())
                    {
                        ImportDB(DevDBElementDictionary, DBManager.DeviceParameter, devCsvReader);
                    }
                    else
                    {
                        LoggerManager.Debug("-> Sync Device Table : Fail");
                    }

                    DBManager.DBManagerParam.DevParamUpdateDate = devUpdateDate;
                    isDBFileModify = true;
                }

                LoggerManager.Debug($"[System DB File Update] : {DBManager.DBManagerParam.SysParamUpdateDate}, [Last Update] : {sysUpdateDate}");

                if (DBManager.DBManagerParam.SysParamUpdateDate != sysUpdateDate)
                {
                    LoggerManager.Debug("-> Sync System Table");
                    CsvDicTypeReader sysCsvReader = new CsvDicTypeReader(CsvDicTypeReader.SYSPARAMETERDATA_FILENAME, PramElementColumn.PropertyPath);

                    DBManager.SystemParameter.DropTable();
                    DBManager.SystemParameter.CreateTable();

                    if (sysCsvReader.Import())
                    {
                        ImportDB(SysDBElementDictionary, DBManager.SystemParameter, sysCsvReader);
                    }
                    else
                    {
                        LoggerManager.Debug("-> Sync System Table : Fail");
                    }

                    DBManager.DBManagerParam.SysParamUpdateDate = sysUpdateDate;
                    isDBFileModify = true;
                }

                LoggerManager.Debug($"[Common DB File Update] : {DBManager.DBManagerParam.ComParamUpdateDate}, [Last Update] : {comUpdateDate}");

                if (DBManager.DBManagerParam.ComParamUpdateDate != comUpdateDate)
                {
                    LoggerManager.Debug("-> Sync Common Table");

                    CsvDicTypeReader commonCsvReader = new CsvDicTypeReader(CsvDicTypeReader.COMMONPARAMETERDATA_FILENAME, PramElementColumn.PropertyPath);

                    DBManager.CommonParameter.DropTable();
                    DBManager.CommonParameter.CreateTable();

                    if (commonCsvReader.Import())
                    {
                        ImportDB(CommonDBElementDictionary, DBManager.CommonParameter, commonCsvReader);
                    }
                    else
                    {
                        LoggerManager.Debug("-> Sync Common Table : Fail");
                    }

                    DBManager.DBManagerParam.ComParamUpdateDate = comUpdateDate;
                    isDBFileModify = true;
                }

                if (isDBFileModify)
                {
                    // DB Update 완료 후에 EnableUpdateDBFile = false로 변경                    
                    DBManager.DBManagerParam.EnableUpdateDBFile = false;
                    DBManager.SaveDBManagerParameter();
                }
            }
            catch (Exception err)
            {
                // 오류가 발생 시, db update 실패 메시지를 띄워주록 처리.
                MessageBox.Show("DB parameter update failed.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoggerManager.Exception(err);
            }
        }
        public void ExportDBtoCSV()
        {
            LoggerManager.Debug($"Export DB to CSV : {DBManager.DBManagerParam.EnableDBSourceServer}");
            if (DBManager.DBManagerParam.EnableDBSourceServer == false)
                return;

            Func<DBTable, String, bool> exportDBFile = (table, dbFilePath) =>
            {
                if (File.Exists(dbFilePath))
                {
                    File.Delete(dbFilePath);
                }
                using (DbDataReader dataReader = table.GetDataReaderByQuery())
                {
                    using (StreamWriter sw = new StreamWriter(dbFilePath))
                    {
                        String column = $"{PramElementColumn.PropertyPath};{PramElementColumn.ElementID};{PramElementColumn.ElementName};{PramElementColumn.ElementAdmin};{PramElementColumn.AssociateElementID};{PramElementColumn.CategoryID};{PramElementColumn.Unit};{PramElementColumn.LowerLimit};{PramElementColumn.UpperLimit};{PramElementColumn.ReadMaskingLevel};{PramElementColumn.WriteMaskingLevel};{PramElementColumn.Description};{PramElementColumn.VID}";
                        sw.WriteLine(column);

                        while (dataReader.Read())
                        {
                            String propertyPath = dataReader[PramElementColumn.PropertyPath].ToString();
                            if (String.IsNullOrEmpty(propertyPath))
                                continue;

                            int elementId = 0;
                            int.TryParse(dataReader[PramElementColumn.ElementID].ToString(), out elementId);

                            String elementName = dataReader[PramElementColumn.ElementName].ToString();
                            String elementAdmin = dataReader[PramElementColumn.ElementAdmin].ToString();
                            String associateElementID = dataReader[PramElementColumn.AssociateElementID].ToString();
                            String categoryID = dataReader[PramElementColumn.CategoryID].ToString();
                            String unit = dataReader[PramElementColumn.Unit].ToString();

                            //==> Lower Limit
                            double lowerLimit = 0.0;
                            double.TryParse(dataReader[PramElementColumn.LowerLimit].ToString(), out lowerLimit);

                            //==> Upper Limit
                            double upperLimit = 0.0;
                            double.TryParse(dataReader[PramElementColumn.UpperLimit].ToString(), out upperLimit);

                            //==> Set ReadMaskingLevel
                            int readMaskingLevel;
                            int.TryParse(dataReader[PramElementColumn.ReadMaskingLevel].ToString(), out readMaskingLevel);

                            //==> Set WriteMaskingLevel
                            int writeMaskingLevel;
                            int.TryParse(dataReader[PramElementColumn.WriteMaskingLevel].ToString(), out writeMaskingLevel);

                            String description = dataReader[PramElementColumn.Description].ToString();
                            String vid = dataReader[PramElementColumn.VID].ToString();

                            String record = $"{propertyPath};{elementId};{elementName};{elementAdmin};{associateElementID};{categoryID};{unit};{lowerLimit};{upperLimit};{readMaskingLevel};{writeMaskingLevel};{description};{vid}";

                            sw.WriteLine(record);
                        }
                    }
                }

                return true;
            };

            exportDBFile(DBManager.DeviceParameter, Path.Combine(DBManager.DBDirectoryPath, CsvDicTypeReader.DEVPARAMETERDATA_FILENAME));
            exportDBFile(DBManager.SystemParameter, Path.Combine(DBManager.DBDirectoryPath, CsvDicTypeReader.SYSPARAMETERDATA_FILENAME));
            exportDBFile(DBManager.CommonParameter, Path.Combine(DBManager.DBDirectoryPath, CsvDicTypeReader.COMMONPARAMETERDATA_FILENAME));
            // install path에 있는 csv파일에 대해서 Hash 떠서 파일로 저장(C:\Program Files (x86)\Maestro)
            CreateHash();

            // DB의 데이터들을 Csv로 Export한 후에 EnableDBSourceServer = false로 변경    
            DBManager.DBManagerParam.EnableDBSourceServer = false;
            DBManager.SaveDBManagerParameter();
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        private void CreateHash()
        {
            //폴더 존재 여부 체크
            var currDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            if (!Directory.Exists(currDirectory))
            {
                return;
            }

            var csvFiles = Directory.GetFiles(currDirectory, $"*ParameterData.csv");
            foreach (var file in csvFiles)
            {
                using (var md5 = MD5.Create())
                {
                    using (FileStream fileStream = File.OpenRead(file))
                    {
                        var comHash = md5.ComputeHash(fileStream);
                        var hashValue = BitConverter.ToString(comHash).Replace("-", String.Empty).ToLower();
                        var fileName = Path.GetFileName(file).Replace(".csv", "");
                        WritePrivateProfileString("Hash", fileName, hashValue, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CsvHashFile.ini"));
                    }
                }
            }
        }
        public void LoadElementInfoFromDB()
        {
            LoggerManager.Debug("Load System Element");
            LoadSysElementInfoFromDB();

            LoggerManager.Debug("Load Device Element");
            LoadDevElementInfoFromDB();

            LoggerManager.Debug("Load Common Element");
            LoadComElementInfoFromDB();

            OnLoadElementInfoFromDB?.Invoke(null, null);

            ApplyAltParamToElement();
        }
        public void LoadSysElementInfoFromDB()
        {
            LoadElementDicFromDB(SysDBElementDictionary, DBManager.SystemParameter);
            this.ChangedSystemParam = true;
        }
        public ParamElementTable GetParamElementTable(string propertypath)
        {
            ParamElementTable retVal = null;
            try
            {
                if (GetElementDic(propertypath)?.ToString() == SysDBElementDictionary.ToString())
                {
                    return DBManager.SystemParameter;
                }
                else if (GetElementDic(propertypath)?.ToString() == DevDBElementDictionary.ToString())
                {
                    return DBManager.DeviceParameter;
                }
                else if (GetElementDic(propertypath)?.ToString() == CommonDBElementDictionary.ToString())
                {
                    return DBManager.CommonParameter;
                }
                else
                {
                    //retVal = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void LoadDevElementInfoFromDB()
        {
            try
            {
                LoadElementDicFromDB(DevDBElementDictionary, DBManager.DeviceParameter);
                this.ChangedDeviceParam = true;

                if(Extensions_IParam.LoadProgramFlag)
                {
                    ApplyAltParamToElement();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void LoadComElementInfoFromDB(bool isOccureLoadeEvent = false)
        {
            try
            {
                LoadElementDicFromDB(CommonDBElementDictionary, DBManager.CommonParameter);
                if (isOccureLoadeEvent)
                {
                    OnLoadElementInfoFromDB?.Invoke(null, null);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void LoadElementDicFromDB(ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> elementDic, DBTable table, string geology = null)
        {
            using (DbDataReader dataReader = table.GetDataReaderByQuery())
            {
                while (dataReader.Read())
                {
                    String propertyPath = dataReader[PramElementColumn.PropertyPath] as String;
                    if (String.IsNullOrEmpty(propertyPath))
                        continue;

                    if (elementDic.ContainsKey(propertyPath) == false)
                        continue;


                    foreach (IElement elem in elementDic[propertyPath].Values)
                    {
                        int elementId = 0;
                        int.TryParse(dataReader[PramElementColumn.ElementID].ToString(), out elementId);

                        elem.ElementID = elementId;
                        elem.ElementName = dataReader[PramElementColumn.ElementName] as String;
                        elem.ElementAdmin = dataReader[PramElementColumn.ElementAdmin] as String;
                        elem.AssociateElementID = dataReader[PramElementColumn.AssociateElementID] as String;
                        elem.CategoryID = dataReader[PramElementColumn.CategoryID] as String;
                        elem.Unit = dataReader[PramElementColumn.Unit] as String;

                        //==> Lower Limit
                        double dblLimit = 0.0;
                        double.TryParse(dataReader[PramElementColumn.LowerLimit].ToString(), out dblLimit);
                        elem.LowerLimit = dblLimit;

                        //==> Upper Limit
                        double.TryParse(dataReader[PramElementColumn.UpperLimit].ToString(), out dblLimit);
                        elem.UpperLimit = dblLimit;

                        //==> Set ReadMaskingLevel
                        int intMaskingLevel;
                        int.TryParse(dataReader[PramElementColumn.ReadMaskingLevel].ToString(), out intMaskingLevel);
                        elem.ReadMaskingLevel = intMaskingLevel;

                        //==> Set WriteMaskingLevel
                        int.TryParse(dataReader[PramElementColumn.WriteMaskingLevel].ToString(), out intMaskingLevel);
                        elem.WriteMaskingLevel = intMaskingLevel;

                        elem.Description = dataReader[PramElementColumn.Description] as String;
                        //int vid = 0;
                        //int.TryParse(dataReader[PramElementColumn.VID].ToString(), out vid);
                        //elem.VID = vid;

                        if (elem.PropertyPath.Equals("EMPTY"))
                        {
                            elem.PropertyPath = propertyPath;
                        }
                        elem.Setup();

                        elem.SetValue(elem.GetValue());
                    }
                }
                dataReader.Close();
            }
        }
        public void LoadElementInfoFromDB(ParamType paramType, IElement elem, String dbPropertyPath)
        {
            try
            {
                DBTable dbTable = null;
                switch (paramType)
                {
                    case ParamType.SYS:
                        dbTable = DBManager.SystemParameter;
                        break;
                    case ParamType.DEV:
                        dbTable = DBManager.DeviceParameter;
                        break;
                    case ParamType.COMMON:
                        dbTable = DBManager.CommonParameter;
                        break;
                    default:
                        //==> THROUGHOUT
                        break;
                }

                if (dbTable == null)
                    return;

                Dictionary<String, String> record = null;
                try
                {
                    record = dbTable.ReadField(
                       dbPropertyPath,
                       PramElementColumn.ElementID,
                       PramElementColumn.ElementName,
                       PramElementColumn.ElementAdmin,
                       PramElementColumn.AssociateElementID,
                       PramElementColumn.CategoryID,
                       PramElementColumn.Unit,
                       PramElementColumn.ReadMaskingLevel,
                       PramElementColumn.WriteMaskingLevel,
                       PramElementColumn.Description,
                       PramElementColumn.LowerLimit,
                       PramElementColumn.UpperLimit
                       );
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }


                if (record == null)
                {
                    elem.ElementID = -1;
                    return;
                }

                //==> ElementID
                String strElementID = record[PramElementColumn.ElementID];
                int intElementID;
                if (int.TryParse(strElementID, out intElementID) == false)
                    intElementID = -1;

                elem.ElementID = intElementID;

                //==> ElementName
                elem.ElementName = record[PramElementColumn.ElementName];

                //==> ElementAdmin
                elem.ElementAdmin = record[PramElementColumn.ElementAdmin];

                //==> AssociateElementID
                elem.AssociateElementID = record[PramElementColumn.AssociateElementID];

                //==> 
                elem.CategoryID = record[PramElementColumn.CategoryID];

                //==> Unit
                elem.Unit = record[PramElementColumn.Unit];

                //==> Lower Limit
                double dblLimit;
                double.TryParse(record[PramElementColumn.LowerLimit], out dblLimit);
                elem.LowerLimit = dblLimit;

                //==> Upper Limit
                double.TryParse(record[PramElementColumn.UpperLimit], out dblLimit);
                elem.UpperLimit = dblLimit;

                //==> MaskingLevel
                int intMaskingLevel;
                int.TryParse(record[PramElementColumn.ReadMaskingLevel], out intMaskingLevel);
                elem.ReadMaskingLevel = intMaskingLevel;

                int.TryParse(record[PramElementColumn.WriteMaskingLevel], out intMaskingLevel);
                elem.WriteMaskingLevel = intMaskingLevel;

                //==> Description
                elem.Description = record[PramElementColumn.Description];

                elem.Setup();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        public void UpdateSingleElementInfo(string propertyPath, String targetField, string setValue)
        {
            try
            {
                //먼저 property path 가 있는 Table 을 가져온다.
                var table = GetParamElementTable(propertyPath);
                if (table != null)
                {
                    table.UpdateTargetInfoValue("PropertyPath", propertyPath, targetField, setValue);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void UpdateUpperLimit(string propertyPath, string setValue)
        {
            try
            {

                //먼저 property path 가 있는 Table 을 가져온다.
                GetElement(propertyPath).UpperLimit = double.Parse(setValue);
                UpdateSingleElementInfo(propertyPath, "UpperLimit", setValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void UpdateLowerLimit(string propertyPath, string setValue)
        {
            try
            {
                //먼저 property path 가 있는 Table 을 가져온다.
                GetElement(propertyPath).LowerLimit = double.Parse(setValue);
                UpdateSingleElementInfo(propertyPath, "LowerLimit", setValue);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public void SetKeyValue(object instance, string key)
        {
            try
            {
                // key += strDot;
                if (instance is IList)
                {
                    var genericArgType = instance.GetType().GenericTypeArguments.FirstOrDefault();
                    if (typeof(IParamNode).IsAssignableFrom(genericArgType) ||
                        typeof(IElement).IsAssignableFrom(genericArgType))
                    {
                        IList list = instance as IList;

                        foreach (var item in list)
                        {
                            SetKeyValue(item, key + '.' + item.GetType().Name);
                        }
                    }
                }
                else if (instance is IParamNode)
                {
                    var properties = GetPropertyTypes(instance, typeof(IParamNode), typeof(IElement));

                    foreach (var prop in properties)
                    {
                        var paramIgnore = prop.GetCustomAttribute(typeof(ParamIgnore));
                        if (paramIgnore == null)
                        {
                            var nodeInstance = prop.GetValue(instance);
                            SetKeyValue(nodeInstance, key + '.' + prop.Name);
                        }
                    }
                    var propertiesList = GetPropertyTypes(instance, typeof(IList));
                    foreach (var prop in propertiesList)
                    {
                        var paramIgnore = prop.GetCustomAttribute(typeof(ParamIgnore));
                        if (paramIgnore == null)
                        {
                            var nodeInstance = prop.GetValue(instance);
                            SetKeyValue(nodeInstance, key);
                        }
                    }
                }
                else if (instance is IElement)
                {
                    string keyValue = key;
                    var prop = instance.GetType().GetProperty(PramElementColumn.PropertyPath);
                    prop.SetValue(instance, keyValue);
                }
                else
                {
                    if (instance != null)
                    {

                        var props = instance.GetType().GetProperties();

                        foreach (PropertyInfo info in props)
                        {
                            var paramIgnore = info.GetCustomAttribute(typeof(ParamIgnore));
                            if (paramIgnore == null)
                            {
                                var nodeInstance = info.GetValue(instance);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        private List<PropertyInfo> GetPropertyTypes(object instance, params Type[] interfaceTypes)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            try
            {

                var type = instance.GetType();
                var properties = type.GetProperties();
                foreach (var prop in properties)
                {
                    foreach (var interfaceType in interfaceTypes)
                    {
                        if (interfaceType.IsAssignableFrom(prop.PropertyType))
                        {
                            list.Add(prop);
                            break;
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return list;
        }
        public void SaveElement(IElement elem, bool isNeedValidation = false)//, string source_classname = null)
        {
            try
            {
                ///TODO: 검토된 내용: 이부분은 GP에서 사용되고 있지 않은 코드로 보임. 셀에서만 호출할 때 정도 의미 있어 보임.
                IParam param = null;
                Object owner = elem.Owner;
                while (owner != null)
                {
                    if (owner is IParam)
                    {
                        param = owner as IParam;
                        break;
                    }
                    else if (owner is IParamNode)
                    {
                        IParamNode node = owner as IParamNode;
                        owner = node.Owner;
                    }
                    else
                    {
                        LoggerManager.Debug($"{elem.ElementName} owner is null");
                        break;
                    }
                }

                EventCodeEnum errrCode = EventCodeEnum.UNDEFINED;
                if (param is ISystemParameterizable && param.Owner is IHasSysParameterizable)
                {
                    var module = param.Owner as IHasSysParameterizable;
                    errrCode = module.SaveSysParameter();
                }
                else if (param is IDeviceParameterizable && param.Owner is IHasDevParameterizable)
                {
                    var module = param.Owner as IHasDevParameterizable;
                    errrCode = module.SaveDevParameter();
                }
                LoggerManager.Debug($"{elem.ElementName} save result : {errrCode}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void SaveElement(string categoryID)
        {
            try
            {
                IElement findElement = null;
                bool isFound = false;
                long lCategoryID = 0;

                isFound = long.TryParse(categoryID.ToString(), out lCategoryID);
                if (isFound)
                {
                    findElement = GetElementList().FirstOrDefault(f => f.CategoryID == lCategoryID.ToString());
                    if (findElement != null)
                        SaveElement(findElement);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private List<IElement> GetElementList()
        {
            List<IElement> elementList = new List<IElement>();
            try
            {
                List<IElement> devEemenetList = GetDevElementList();
                List<IElement> sysEemenetList = GetSysElementList();

                elementList.AddRange(devEemenetList);
                elementList.AddRange(sysEemenetList);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return elementList;
        }
        public List<IElement> GetElementList(List<IElement> emlList, ref byte[] bulkElem)
        {
            object target;
            var dataPack = bulkElem;
            var result = SerializeManager.DeserializeFromByte(dataPack, out target, typeof(ElementPacks), new Type[] { typeof(ElementPack) });

            ElementPacks packs = (ElementPacks)target;

            foreach (var element in packs.Elements)
            {
                if (emlList.Count() > 0)
                {
                    var item = emlList.FirstOrDefault(x => x.ElementID == element.ElementID && x.CategoryID.Contains(element.CategoryID));
                    if (item == null)
                    {
                        emlList.Add((IElement)element);
                    }
                    else
                    {
                        item.SetValue(element.GetValue());
                    }
                }
                else
                {
                    emlList.Add((IElement)element);
                }
            }

            return emlList;
        }
        public List<IElement> GetDevElementList()
        {
            return GetElementListByDic(DevDBElementDictionary);
        }
        public List<IElement> GetSysElementList()
        {
            return GetElementListByDic(SysDBElementDictionary);
        }
        public List<IElement> GetCommonElementList()
        {
            return GetElementListByDic(CommonDBElementDictionary);
        }
        private List<IElement> GetElementListByDic(ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> elementDic)
        {
            List<IElement> elementList = new List<IElement>();
            try
            {
                foreach (var item in elementDic)
                {
                    ConcurrentDictionary<String, IElement> elemDic = item.Value;
                    elementList.AddRange(elemDic.Values);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return elementList;
        }
        public string GetElementPath(int elementID)
        {
            string path = null;
            try
            {
                path = GetElementKeyByDic(elementID, DevDBElementDictionary);
                if (path != null)
                    return path;
                path = GetElementKeyByDic(elementID, SysDBElementDictionary);
                if (path != null)
                    return path;
                path = GetElementKeyByDic(elementID, CommonDBElementDictionary);
                if (path != null)
                    return path;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return path;
        }
        public IElement GetElement(int elementID)
        {
            IElement findElement = null;
            try
            {
                findElement = GetDevElement(elementID);

                if (findElement != null)
                    return findElement;

                findElement = GetSysElement(elementID);
                if (findElement != null)
                    return findElement;

                findElement = GetCommElement(elementID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return findElement;
        }
        public ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> GetElementDic(string propertyPath)
        {
            ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> dic = null;
            try
            {
                ConcurrentDictionary<String, IElement> tempval = null;
                bool isfind = DevDBElementDictionary.TryGetValue(propertyPath, out tempval);
                if (isfind)
                {
                    return DevDBElementDictionary;
                }


                isfind = SysDBElementDictionary.TryGetValue(propertyPath, out tempval);
                if (isfind)
                {
                    return SysDBElementDictionary;
                }


                isfind = CommonDBElementDictionary.TryGetValue(propertyPath, out tempval);
                if (isfind)
                {
                    return CommonDBElementDictionary;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //throw;
            }
            return dic;
        }
        public string GetPropertyPathFromVID(long vid)
        {
            IElement tempVal = null;

            try
            {

                foreach (var item in DevDBElementDictionary)
                {
                    tempVal = item.Value.Values.Where(v => v.VID == vid).FirstOrDefault();
                    if (tempVal != null)
                        return item.Value.Values.FirstOrDefault().PropertyPath;
                }


                foreach (var item in SysDBElementDictionary)
                {
                    tempVal = item.Value.Values.Where(v => v.VID == vid).FirstOrDefault();
                    if (tempVal != null)
                        return item.Value.Values.FirstOrDefault().PropertyPath;
                }


                foreach (var item in CommonDBElementDictionary)
                {
                    tempVal = item.Value.Values.Where(v => v.VID == vid).FirstOrDefault();
                    if (tempVal != null)
                        return item.Value.Values.FirstOrDefault().PropertyPath;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //throw;
            }
            return "";
        }
        public long GetElementIDFormVID(long vid)
        {
            long elementid = -1;
            try
            {
                foreach (var item in DevDBElementDictionary)
                {
                    ConcurrentDictionary<String, IElement> elemDic = item.Value;
                    var element = elemDic.Values.FirstOrDefault(elem => elem.VID == vid);
                    if (element != null)
                    {
                        elementid = element.ElementID;
                        return elementid;
                    }
                }

                foreach (var item in SysDBElementDictionary)
                {
                    ConcurrentDictionary<String, IElement> elemDic = item.Value;
                    var element = elemDic.Values.FirstOrDefault(elem => elem.VID == vid);
                    if (element != null)
                    {
                        elementid = element.ElementID;
                        return elementid;
                    }

                }


                foreach (var item in CommonDBElementDictionary)
                {
                    ConcurrentDictionary<String, IElement> elemDic = item.Value;
                    var element = elemDic.Values.FirstOrDefault(elem => elem.VID == vid);
                    if (element != null)
                    {
                        elementid = element.ElementID;
                        return elementid;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return elementid;
        }
        public IElement GetElement(string propertyPath)
        {
            IElement findElement = null;
            try
            {
                using (IEnumerator<ConcurrentDictionary<String, IElement>> enumerator = DevDBElementDictionary.Values.GetEnumerator())
                {
                    bool moveNext = enumerator.MoveNext();
                    while (moveNext)
                    {
                        findElement = enumerator.Current.Values.ToList<IElement>().Find(elem => elem.PropertyPath.Equals(propertyPath));
                        if (findElement != null)
                            break;

                        moveNext = enumerator.MoveNext();
                    }
                }

                if (findElement != null)
                    return findElement;

                using (IEnumerator<ConcurrentDictionary<String, IElement>> enumerator = SysDBElementDictionary.Values.GetEnumerator())
                {
                    bool moveNext = enumerator.MoveNext();
                    while (moveNext)
                    {
                        findElement = enumerator.Current.Values.ToList<IElement>().Find(elem => elem.PropertyPath.Equals(propertyPath));
                        if (findElement != null)
                            break;

                        moveNext = enumerator.MoveNext();
                    }
                }

                if (findElement != null)
                    return findElement;

                using (IEnumerator<ConcurrentDictionary<String, IElement>> enumerator = CommonDBElementDictionary.Values.GetEnumerator())
                {
                    bool moveNext = enumerator.MoveNext();
                    while (moveNext)
                    {
                        findElement = enumerator.Current.Values.ToList<IElement>().Find(elem => elem.PropertyPath.Equals(propertyPath));
                        if (findElement != null)
                            break;

                        moveNext = enumerator.MoveNext();
                    }
                }

                if (findElement != null)
                    return findElement;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return findElement;
        }
        private IElement GetDevElement(int elementID)
        {
            return GetElementByDic(elementID, DevDBElementDictionary);
        }
        private IElement GetSysElement(int elementID)
        {
            return GetElementByDic(elementID, SysDBElementDictionary);
        }
        private IElement GetCommElement(int elementID)
        {
            return GetElementByDic(elementID, CommonDBElementDictionary);
        }
        private IElement GetElementByDic(int elementID, ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> elementDic)
        {
            IElement findElement = null;

            try
            {
                using (IEnumerator<ConcurrentDictionary<String, IElement>> enumerator = elementDic.Values.GetEnumerator())
                {
                    bool moveNext = enumerator.MoveNext();
                    while (moveNext)
                    {
                        findElement = enumerator.Current.Values.ToList<IElement>().Find(elem => elem.ElementID == elementID);
                        if (findElement != null)
                            break;

                        moveNext = enumerator.MoveNext();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return findElement;
        }
        private string GetElementKeyByDic(int elementID, ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> elementDic)
        {
            string path = null;

            try
            {
                using (IEnumerator<ConcurrentDictionary<String, IElement>> enumerator = elementDic.Values.GetEnumerator())
                {
                    bool moveNext = enumerator.MoveNext();
                    while (moveNext)
                    {
                        var element = enumerator.Current.Values.ToList<IElement>().Find(elem => elem.ElementID == elementID);
                        if (element != null)
                        {
                            //path = enumerator.Current.Keys.tos
                            break;
                        }


                        moveNext = enumerator.MoveNext();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return path;
        }
        public IElement GetAssociateElement(string associateID)
        {
            IElement findElement = null;
            try
            {
                int id = 0;
                char firstChar = associateID.FirstOrDefault();
                associateID = associateID.Substring(1);
                if (int.TryParse(associateID, out id))
                {
                    if (firstChar.Equals('D'))
                        findElement = GetDevElement(id);
                    else if (firstChar.Equals('S'))
                        findElement = GetSysElement(id);
                    else if (firstChar.Equals('C'))
                        findElement = GetCommElement(id);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return findElement;
        }
        public void SaveCategory(string categoryid)
        {
            SaveElement(categoryid);
        }
        public void LoadElementInfosFromDB()
        {
            LoadElementInfoFromDB();
        }
        public bool IsServiceAvailable()
        {
            return true;
        }
        public void InitService()
        {
            LoggerManager.Debug($"ParamManager.InitService(); Service initialized.");
        }
        public byte[] GetDevParamElementsBulk(string parmName)
        {
            // TODO : 함수 사용 확인 필요
            byte[] bulkElements = new byte[1];

            var devElements = DevDBElementDictionary.Where(i => i.Key.Contains(parmName));
            ElementPacks packs = new ElementPacks();
            List<IElement> iElemenetList = new List<IElement>();

            try
            {
                foreach (var item in devElements)
                {
                    ElementPack pack;

                    ConcurrentDictionary<String, IElement> elemDic = item.Value;
                    iElemenetList.AddRange(elemDic.Values);
                    foreach (var elem in elemDic)
                    {
                        pack = new ElementPack();
                        elem.Value.CopyTo(pack);
                        packs.Elements.Add(pack);
                    }
                }
                bulkElements = SerializeManager.SerializeToByte(packs, typeof(ElementPacks));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return bulkElements;
        }
        public byte[] GetDevElementsBulk()
        {
            return GetElementsBulk(DevDBElementDictionary);
        }
        public byte[] GetSysElementsBulk()
        {
            return GetElementsBulk(SysDBElementDictionary);
        }
        private byte[] GetElementsBulk(ConcurrentDictionary<string, ConcurrentDictionary<string, IElement>> elementsDictionary)
        {
            byte[] bulkElements = new byte[1];
            ElementPacks packs = new ElementPacks();
            try
            {
                foreach (var item in elementsDictionary)
                {
                    ElementPack pack;
                    ConcurrentDictionary<string, IElement> elemDic = item.Value;

                    foreach (var elem in elemDic)
                    {
                        pack = new ElementPack();
                        elem.Value.CopyTo(pack);
                        packs.Elements.Add(pack);
                    }
                }

                bulkElements = SerializeManager.SerializeToByte(packs, typeof(ElementPacks));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return bulkElements;
        }
        public void SetElement(string propPath, Object setval)
        {
            EventCodeEnum errrCode = EventCodeEnum.UNDEFINED;
            try
            {
                Object OldValue = null;

                ConcurrentDictionary<String, IElement> elemDict;
                IElement elem = null;

                if (DevDBElementDictionary.ContainsKey(propPath) == true)
                {
                    DevDBElementDictionary.TryGetValue(propPath, out elemDict);
                    elemDict.TryGetValue(propPath, out elem);
                }
                else if (SysDBElementDictionary.ContainsKey(propPath) == true)
                {
                    SysDBElementDictionary.TryGetValue(propPath, out elemDict);
                    elemDict.TryGetValue(propPath, out elem);
                }
                else if (CommonDBElementDictionary.ContainsKey(propPath) == true)
                {
                    CommonDBElementDictionary.TryGetValue(propPath, out elemDict);
                    elemDict.TryGetValue(propPath, out elem);//여기서 ElementPack을 Element로 바꿔줌. 
                }
                if (elem != null)
                {
                    IParam param = null;
                    Object owner = elem.Owner;
                    while (owner != null)
                    {
                        if (owner is IParam)
                        {
                            param = owner as IParam;
                            break;
                        }
                        else if (owner is IParamNode)
                        {
                            IParamNode node = owner as IParamNode;
                            owner = node.Owner;
                        }
                        else
                        {
                            LoggerManager.Debug($"{elem.ElementName} owner is null");
                            break;
                        }
                    }

                    OldValue = elem.GetValue();

                    elem.SetValue(setval);

                    if (param is ISystemParameterizable && param.Owner is IHasSysParameterizable)
                    {
                        var module = param.Owner as IHasSysParameterizable;
                        errrCode = module.SaveSysParameter();

                        ChangedSystemParam = true;

                        LoggerManager.Debug($"SaveElementPack(), System Parameter is changed. {ChangedSystemParam}");
                    }
                    else if (param is IDeviceParameterizable && param.Owner is IHasDevParameterizable)
                    {
                        var module = param.Owner as IHasDevParameterizable;
                        errrCode = module.SaveDevParameter();

                        ChangedDeviceParam = true;

                        LoggerManager.Debug($"SaveElementPack(), Device Parameter is changed. {ChangedDeviceParam}");
                    }
                }

                if (OldValue != null)
                {
                    LoggerManager.Debug($"{elem.ElementName}, Old value = {OldValue.ToString()}, New value = {elem.ToString()}, save result : {errrCode}");
                }
                else
                {
                    LoggerManager.Debug($"{elem.ElementName} save result : {errrCode}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        /// <summary>
        /// 파라미터를 Set해준 Class의 ClassName을 받아 IModule Object로 만들어주는 함수
        /// </summary>
        /// <param name="source_classname"></param>
        /// <returns></returns>
        public (bool needValidation, IModule source) ClassNameConverter(string source_classname)
        {
            (bool needValidation, IModule source) retVal = (false, null);
            try
            {
                Type type = Type.GetType(source_classname);// TODO: classname으로 singleton object 가지고 올수 있도록 근데이거 SetupPage는 로더랑 이름 다르니까... 문제가 되겠네..

                if (source_classname != null && source_classname != "")
                {
                    retVal.needValidation = true;
                    if (source_classname == this.GEMModule().GetType().FullName)
                    {
                        retVal.source = this.GEMModule();
                    }
                    else if (source_classname == this.TCPIPModule().GetType().FullName)
                    {
                        retVal.source = this.TCPIPModule();
                    }
                    else if (source_classname == this.GPIB().GetType().FullName)
                    {
                        retVal.source = this.GPIB();
                    }
                    else
                    {
                        //nothing, but need to validation
                    }
                }
                else
                {
                    retVal.needValidation = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public EventCodeEnum CheckSetValueAvailable(string propertypath, object val)
        {
            string erromsg = "";
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IElement element = this.ParamManager().GetElement(propertypath);              
                if (element != null)
                {
                    retVal = element.CheckSetValueAvailable(propertypath, val, out erromsg);
                }
                else
                {
                    erromsg = $"[LoaderController].CheckSetValueAvailable(): Cannot find element. {propertypath}";
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //errorlog = erromsg;
            }
            return retVal;
        }

        public EventCodeEnum CheckOriginSetValueAvailable(string propertypath, object val)
        {
            string erromsg = "";
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IElement element = this.ParamManager().GetElement(propertypath);
                if (element.PropertyPath != element.OriginPropertyPath)
                {
                    val = element.ConvertToOriginType(val);
                    element = this.ParamManager().GetElement(element.OriginPropertyPath);
                    
                }

                if (element != null)
                {
                    retVal = element.CheckSetValueAvailable(propertypath, val, out erromsg);
                }
                else
                {
                    erromsg = $"[LoaderController].CheckSetValueAvailable(): Cannot find element. {propertypath}";
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //errorlog = erromsg;
            }
            return retVal;
        }

        public EventCodeEnum SetOriginValue(string propertypath, Object val, bool isNeedValidation = false, bool isEqualsValue = true, object valueChangedParam = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {                
                IElement element = this.ParamManager().GetElement(propertypath);

                if (element.PropertyPath != element.OriginPropertyPath)
                {
                    val = element.ConvertToOriginType(val);
                    element = this.ParamManager().GetElement(element.OriginPropertyPath);
                    
                }

                LoggerManager.Debug($"[ParamManager] SetOriginValue(): Try prop:{element.PropertyPath}, origin:{element.PropertyPath},  newVal:{val} oldVal:{element.GetValue()} ");
                if (element != null)
                {
                    retVal = element.SetValue(val, isNeedValidation, isEqualsValue, valueChangedParam);//, convertModule);                  
                    if(retVal == EventCodeEnum.NONE)
                    {
                        element.SaveElement(false, true);
                    }
                    else if (retVal == EventCodeEnum.PARAM_SET_EQUAL_VALUE)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                }
                LoggerManager.Debug($"[ParamManager] SetOriginValue(): End, result:{retVal} origin:{element.PropertyPath},  Val:{element.GetValue()} ");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetValue(string propertypath, Object val, bool isNeedValidation = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {                
                IElement element = this.ParamManager().GetElement(propertypath);
          
                LoggerManager.Debug($"[ParamManager] SetValue(): Try prop:{element.PropertyPath}, origin:{element.PropertyPath},  newVal:{val} oldVal:{element.GetValue()} ");
                if (element != null)
                {
                    retVal = element.SetValue(val, isNeedValidation);//, convertModule);
                    if (retVal == EventCodeEnum.PARAM_SET_EQUAL_VALUE)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                }
                LoggerManager.Debug($"[ParamManager] SetValue(): End, result:{retVal} origin:{element.PropertyPath},  Val:{element.GetValue()} ");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SaveElementPack(byte[] bytepack, bool isNeedValidation = false)//, string source_classname = null)
        {
            EventCodeEnum errrCode = EventCodeEnum.UNDEFINED;
            try
            {
                //(bool needToValidation, IModule convertModule) = ClassNameConverter(source_classname);

                ElementPacks packs;
                object target;
                var result = SerializeManager.DeserializeFromByte(
                    bytepack, out target, typeof(ElementPacks), new Type[] { typeof(ElementPack) });

                Object OldValue = null;

                packs = (ElementPacks)target;
                if (packs.Elements.Count > 0)
                {
                    var pack = packs.Elements.FirstOrDefault();
                    var propPath = pack.PropertyPath;
                    ConcurrentDictionary<String, IElement> elemDict;
                    IElement elem = null;
                    if (DevDBElementDictionary.ContainsKey(propPath) == true)
                    {
                        DevDBElementDictionary.TryGetValue(propPath, out elemDict);
                        elemDict.TryGetValue(propPath, out elem);
                    }
                    else if (SysDBElementDictionary.ContainsKey(propPath) == true)
                    {
                        SysDBElementDictionary.TryGetValue(propPath, out elemDict);
                        elemDict.TryGetValue(propPath, out elem);
                    }
                    else if (CommonDBElementDictionary.ContainsKey(propPath) == true)
                    {
                        CommonDBElementDictionary.TryGetValue(propPath, out elemDict);
                        elemDict.TryGetValue(propPath, out elem);//여기서 ElementPack을 Element로 바꿔줌. 
                    }
                    if (elem != null)
                    {
                        IParam param = null;
                        Object owner = elem.Owner;
                        while (owner != null)
                        {
                            if (owner is IParam)
                            {
                                param = owner as IParam;
                                break;
                            }
                            else if (owner is IParamNode)
                            {
                                IParamNode node = owner as IParamNode;
                                owner = node.Owner;
                            }
                            else
                            {
                                LoggerManager.Debug($"{elem.ElementName} owner is null");
                                break;
                            }
                        }

                        OldValue = elem.GetValue();

                        elem.SetValue(pack.Value, isNeedValidation);//, convertModule);


                        if (param is ISystemParameterizable && param.Owner is IHasSysParameterizable)
                        {
                            var module = param.Owner as IHasSysParameterizable;
                            errrCode = module.SaveSysParameter();

                            ChangedSystemParam = true;

                            LoggerManager.Debug($"SaveElementPack(), System Parameter is changed. {ChangedSystemParam}");
                        }
                        else if (param is IDeviceParameterizable && param.Owner is IHasDevParameterizable)
                        {
                            var module = param.Owner as IHasDevParameterizable;
                            errrCode = module.SaveDevParameter();

                            ChangedDeviceParam = true;

                            LoggerManager.Debug($"SaveElementPack(), Device Parameter is changed. {ChangedDeviceParam}");
                        }
                    }

                    if (OldValue != null)
                    {
                        LoggerManager.Debug($"{elem.ElementName}, Old value = {OldValue.ToString()}, New value = {elem.ToString()}, save result : {errrCode}");
                    }
                    else
                    {
                        LoggerManager.Debug($"{elem.ElementName} save result : {errrCode}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public bool IsAvailable()
        {
            return true;
        }
        public void SetChangedDeviceParam(bool flag)
        {
            ChangedDeviceParam = flag;
        }
        public void SetChangedSystemParam(bool flag)
        {
            ChangedSystemParam = flag;
        }
        public bool GetChangedDeviceParam()
        {
            return ChangedDeviceParam;
        }
        public bool GetChangedSystemParam()
        {
            return ChangedSystemParam;
        }
        public EventCodeEnum VerifyLotVIDsCheckBeforeLot()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                string message = "";
                if (ParamDevParam.VerifyParameterBeforeStartLotEnable == false)
                {
                    return EventCodeEnum.NONE;
                }

                var verifyInfo = ParamDevParam.VerifyParamInfos;
                bool isSuccess = true;
                if (verifyInfo != null)
                {

                    VerifyParamInfo paraminfo = verifyInfo.Find(info => info.EnumProperty == VerifyPropertyEnum.OVER_DRIVE);
                    if (paraminfo != null)
                    {
                        double overdrive = this.ProbingModule().OverDrive;
                        CheckValue(overdrive, paraminfo.MinValue, paraminfo.MaxValve);

                        string msg = $"[Verify Param] " +
                            $"Property Name : {paraminfo.PropertyName}, MinValue : {paraminfo.MinValue}, " +
                            $"MaxValue : {paraminfo.MaxValve}, OrginalValue : {overdrive}";
                        message += msg;
                        LoggerManager.Debug(msg);
                    }

                    paraminfo = verifyInfo.Find(info => info.EnumProperty == VerifyPropertyEnum.TEMPERATURE);
                    if (paraminfo != null)
                    {
                        double temp = this.TempController().GetSetTemp();
                        CheckValue(temp, paraminfo.MinValue, paraminfo.MaxValve);

                        string msg = $"[Verify Param] " +
                            $"Property Name : {paraminfo.PropertyName}, MinValue : {paraminfo.MinValue}, " +
                            $"MaxValue : {paraminfo.MaxValve}, OrginalValue : {temp}";
                        message += "\r\n" + msg;
                        LoggerManager.Debug(msg);
                    }

                    var polishWaferparam = this.PolishWaferModule().GetPolishWaferIntervalParameters();
                    if (polishWaferparam != null)
                    {
                        foreach (var item in polishWaferparam)
                        {
                            if (item.CleaningParameters != null)
                            {
                                foreach (var cleanparam in item.CleaningParameters)
                                {
                                    paraminfo = verifyInfo.Find(info => info.EnumProperty == VerifyPropertyEnum.CLEAN_OVER_DRIVE);
                                    if (paraminfo != null)
                                    {
                                        double clean_overdrive = cleanparam.OverdriveValue.Value;
                                        CheckValue(clean_overdrive, paraminfo.MinValue, paraminfo.MaxValve);

                                        string msg = $"[Verify Param] " +
                                            $"Property Name : {paraminfo.PropertyName}, MinValue : {paraminfo.MinValue}, " +
                                            $"MaxValue : {paraminfo.MaxValve}, OrginalValue : {clean_overdrive}";
                                        message += "\r\n" + msg;
                                        LoggerManager.Debug(msg);
                                    }
                                }

                                paraminfo = verifyInfo.Find(info => info.EnumProperty == VerifyPropertyEnum.CLEAN_INTERNAL);
                                if (paraminfo != null)
                                {
                                    double clean_interval = item.IntervalCount.Value;
                                    CheckValue(clean_interval, paraminfo.MinValue, paraminfo.MaxValve);

                                    string msg = $"[Verify Param] " +
                                        $"Property Name : {paraminfo.PropertyName}, MinValue : {paraminfo.MinValue}, " +
                                        $"MaxValue : {paraminfo.MaxValve}, OrginalValue : {clean_interval}";
                                    message += "\r\n" + msg;
                                    LoggerManager.Debug(msg);
                                }


                                paraminfo = verifyInfo.Find(info => info.EnumProperty == VerifyPropertyEnum.CLEAN_TOUCH_COUNT);
                                if (paraminfo != null)
                                {
                                    double clean_timecount = item.TouchdownCount.Value;
                                    CheckValue(clean_timecount, paraminfo.MinValue, paraminfo.MaxValve);

                                    string msg = $"[Verify Param] " +
                                        $"Property Name : {paraminfo.PropertyName}, MinValue : {paraminfo.MinValue}, " +
                                        $"MaxValue : {paraminfo.MaxValve}, OrginalValue : {clean_timecount}";
                                    message += "\r\n" + msg;
                                    LoggerManager.Debug(msg);
                                }
                            }
                        }
                    }

                    if (isSuccess == true)
                    {
                        retVal = EventCodeEnum.NONE;
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //this.EventManager().RaisingEvent(typeof(ParameterVerifySuccessEvent).FullName, new ProbeEventArgs(this, semaphore));
                        //semaphore.Wait();
                    }
                    else
                    {
                        retVal = EventCodeEnum.UNDEFINED;
                        this.MetroDialogManager().ShowMessageDialog("Error Message",
                            $"Parameter verify fail. please check value. \r\n {message}", MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                        //SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        //this.EventManager().RaisingEvent(typeof(ParameterVerifyFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                        //semaphore.Wait();

                    }
                }

                void CheckValue(double orginvalue, double minvalue, double maxvalue)
                {
                    if (minvalue == maxvalue)
                    {
                        if (orginvalue == minvalue)
                        {
                            if (isSuccess != false)
                            {
                                isSuccess = true;
                            }
                        }
                        else
                        {
                            isSuccess = false;
                        }
                    }
                    else
                    {
                        if (orginvalue >= minvalue && orginvalue <= maxvalue)
                        {
                            if (isSuccess != false)
                            {
                                isSuccess = true;
                            }
                        }
                        else
                        {
                            isSuccess = false;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public ObservableCollection<int> GetVerifyLotVIDs()
        {
            return ParamDevParam?.VerifyLotDevVIDs ?? null;
        }
        public void SetVerifyLotVIDs(ObservableCollection<int> vids)
        {
            try
            {
                if (ParamDevParam != null)
                {
                    ParamDevParam.VerifyLotDevVIDs = vids;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void UpdateVerifyParam()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool GetVerifyParameterBeforeStartLotEnable()
        {
            bool retVal = false;
            try
            {
                retVal = ParamDevParam.VerifyParameterBeforeStartLotEnable;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public List<VerifyParamInfo> GetVerifyParamInfo()
        {
            List<VerifyParamInfo> retVal = null;
            try
            {
                retVal = ParamDevParam.VerifyParamInfos;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetVerifyParameterBeforeStartLotEnable(bool flag)
        {
            try
            {
                ParamDevParam.VerifyParameterBeforeStartLotEnable = flag;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetVerifyParamInfo(List<VerifyParamInfo> infos)
        {
            try
            {
                ParamDevParam.VerifyParamInfos = infos;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum ApplyAltParamToElement()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (AltSysParam != null)
                {
                    if(AltSysParam.AltParamPropertyPathList != null)
                    {
                        foreach (var altParam in AltSysParam.AltParamPropertyPathList)
                        {
                            if(altParam != null && String.IsNullOrEmpty(altParam.PropertyPath) == false && altParam.Value != null)
                            {
                                if (altParam.PropertyPath.Contains("[]") == false)
                                {
                                    var element = GetElement(altParam.PropertyPath);
                                    if (element != null)
                                    {
                                        element.SetAltValue(altParam.Value);
                                    }
                                    else
                                    {
                                        LoggerManager.Debug($"SetAltValue() fail because not exist element. PropertyPath : {altParam.PropertyPath.ToString()}, Value : {altParam.Value.ToString()}");
                                    }
                                }
                                else
                                {
                                    string propertyPath = altParam.PropertyPath;
                                    string[] words = propertyPath.Split('[');
                                    //첫번째 path 로 element list 가져오기
                                    if(words != null && words.Length > 0)
                                    {
                                        var elements = GetElementContainPropteyPath(words[0]);
                                        if(elements != null)
                                        {
                                            //첫번째 path 로 이후 path 일치 확인
                                            for (int index = 1; index < words.Length; index++)
                                            {
                                                elements = elements.FindAll(element => element.PropertyPath.Contains(words[index]));
                                            }

                                            if(elements != null)
                                            {
                                                foreach (var element in elements)
                                                {
                                                    if (element != null)
                                                    {
                                                        element.SetAltValue(altParam.Value);
                                                    }
                                                    else
                                                    {
                                                        LoggerManager.Debug($"SetAltValue() fail because not exist element. PropertyPath : {altParam.PropertyPath.ToString()}, Value : {altParam.Value.ToString()}");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                string logInfo = "";
                                if(altParam != null)
                                {
                                    logInfo = $" PropertyPath : {altParam.PropertyPath.ToString()}, Value : {altParam.Value.ToString()}";
                                }
                                LoggerManager.Debug($"SetAltValue() fail because have incorrect information parameter.{logInfo}");
                            }
                        }
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool CheckExistAltParamInParameterObject(object paramFileObj, ref string msg)
        {
            bool retVal = false;
            try
            {
                if (paramFileObj != null)
                {
                    var type = paramFileObj.GetType();
                    var properties = GetPropertyTypes(paramFileObj, typeof(IParamNode), typeof(IElement), typeof(IList));
                    foreach (var prop in properties)
                    {
                        var propObj = prop.GetValue(paramFileObj);
                        if (propObj is IElement)
                        {
                            comparePropertyPath((IElement)propObj, ref msg);
                        }
                        else if(propObj is IList)
                        {
                            IList list = propObj as IList;

                            foreach (var item in list)
                            {
                                if (propObj is IElement)
                                {
                                    comparePropertyPath((IElement)propObj, ref msg);
                                }
                            }
                        }
                        else if(propObj is IParamNode && !(propObj is IParam))
                        {
                            bool retval = CheckExistAltParamInParameterObject(propObj as IParamNode, ref msg);
                            if(retval)
                            {
                                retVal = retval;
                                msg = $"{msg}";
                            }
                        }
                    }
                }

                void comparePropertyPath(IElement element, ref string _msg)
                {
                    if (AltSysParam != null && AltSysParam.AltParamPropertyPathList != null)
                    {
                        var  matchedObj = AltSysParam.AltParamPropertyPathList.Find(param => param.PropertyPath.Equals(element.PropertyPath));
                        bool isMatched = matchedObj == null ? false : true;
                        if(isMatched)
                        {
                            _msg += $"\nProperty Path : {element.PropertyPath}, Alt Value : {matchedObj.Value}.";
                            retVal = true;
                        }
                    }
                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public List<IElement> GetElementContainPropteyPath(string propertyPath)
        {
            List<IElement> findElements = new List<IElement>();
            IElement findElement = null;
            try
            {
                using (IEnumerator<ConcurrentDictionary<String, IElement>> enumerator = DevDBElementDictionary.Values.GetEnumerator())
                {
                    bool moveNext = enumerator.MoveNext();
                    while (moveNext)
                    {
                        findElement = enumerator.Current.Values.ToList<IElement>().Find(elem => elem.PropertyPath.Contains(propertyPath));
                        if (findElement != null)
                        {
                            findElements.Add(findElement);
                        }
                        moveNext = enumerator.MoveNext();
                    }
                }

                if(findElements.Count != 0)
                {
                    return findElements;
                }

                using (IEnumerator<ConcurrentDictionary<String, IElement>> enumerator = SysDBElementDictionary.Values.GetEnumerator())
                {
                    bool moveNext = enumerator.MoveNext();
                    while (moveNext)
                    {
                        findElement = enumerator.Current.Values.ToList<IElement>().Find(elem => elem.PropertyPath.Contains(propertyPath));
                        if (findElement != null)
                        {
                            findElements.Add(findElement);
                        }
                        moveNext = enumerator.MoveNext();
                    }
                }

                if (findElements.Count != 0)
                {
                    return findElements;
                }

                using (IEnumerator<ConcurrentDictionary<String, IElement>> enumerator = CommonDBElementDictionary.Values.GetEnumerator())
                {
                    bool moveNext = enumerator.MoveNext();
                    while (moveNext)
                    {
                        findElements = enumerator.Current.Values.ToList<IElement>().FindAll(elem => elem.PropertyPath.Contains(propertyPath));
                        if (findElement != null)
                        {
                            findElements.Add(findElement);
                        }
                        moveNext = enumerator.MoveNext();
                    }
                }

                if (findElements.Count != 0)
                {
                    return findElements;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return findElements;
        }

        #region <remarks> IHasDevParameterizable Methods<remarks>
        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new ParamDevParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(ParamDevParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    ParamDevParam = tmpParam as ParamDevParameter;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.SaveParameter(ParamDevParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
        #endregion

        #region <remarks> IHasSysParameterizable Methods<remarks>
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new AlternateSysParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(AlternateSysParameter));
                AltSysParam = (AlternateSysParameter)tmpParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = this.SaveParameter(AltSysParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return RetVal;
        }
        #endregion
    }
}
