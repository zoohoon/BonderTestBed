using System;
using System.Collections.Generic;
using System.Linq;

namespace DBManagerModule.Table
{
    using System.Data;
    using System.Runtime.CompilerServices;
    public class ParamElementTable : DBTable
    {
        //==> Element ID 충돌 확인용 Hash
        private HashSet<String> _ElementIDSet;
        private int _LastAddElementID;
        public ParamElementTable(string tableName, DBController dbController, int elementIDBand)
            : base(tableName, dbController)
        {
            _LastAddElementID = elementIDBand + 1;
        }
        public override DBStatus Open()
        {
            DBStatus status = base.Open();

            if (status == DBStatus.TableError)
                return status;

            _ElementIDSet = new HashSet<String>();
            List<String> elementIDList = GetAllFieldByColumn(PramElementColumn.ElementID);
            foreach (String strElementID in elementIDList)
            {
                _ElementIDSet.Add(strElementID);
            }

            return status;
        }
        public override void InitRecordColumTemplate()
        {
            //==> 컬럼 템플릿 생성
            ColumTemplate = new Dictionary<String, TableColumn>();
            ColumTemplate.Add(PramElementColumn.PropertyPath, new TableColumn(DbType.String, PramElementColumn.PropertyPath, 255));
            ColumTemplate.Add(PramElementColumn.ElementID, new TableColumn(DbType.Int32, PramElementColumn.ElementID));
            ColumTemplate.Add(PramElementColumn.ElementName, new TableColumn(DbType.String, PramElementColumn.ElementName, 100));
            ColumTemplate.Add(PramElementColumn.ElementAdmin, new TableColumn(DbType.String, PramElementColumn.ElementAdmin, 50));
            ColumTemplate.Add(PramElementColumn.AssociateElementID, new TableColumn(DbType.String, PramElementColumn.AssociateElementID, 255));
            ColumTemplate.Add(PramElementColumn.CategoryID, new TableColumn(DbType.String, PramElementColumn.CategoryID, 50));
            ColumTemplate.Add(PramElementColumn.Unit, new TableColumn(DbType.String, PramElementColumn.Unit, 20));
            ColumTemplate.Add(PramElementColumn.LowerLimit, new TableColumn(DbType.Double, PramElementColumn.LowerLimit));
            ColumTemplate.Add(PramElementColumn.UpperLimit, new TableColumn(DbType.Double, PramElementColumn.UpperLimit));
            ColumTemplate.Add(PramElementColumn.ReadMaskingLevel, new TableColumn(DbType.Int32, PramElementColumn.ReadMaskingLevel));
            ColumTemplate.Add(PramElementColumn.WriteMaskingLevel, new TableColumn(DbType.Int32, PramElementColumn.WriteMaskingLevel));
            ColumTemplate.Add(PramElementColumn.Description, new TableColumn(DbType.String, PramElementColumn.Description, 255));
            ColumTemplate.Add(PramElementColumn.VID, new TableColumn(DbType.Int32, PramElementColumn.VID, 255));
            //==> Primary Key 지정 해야 함
            PrimaryKeyColumn = ColumTemplate.FirstOrDefault().Value.ColumnCaption;
            PrimaryKeyColumnType = ColumTemplate.FirstOrDefault().Value.Type;
        }
        public bool InsertRecord(
            String propertyPath,
            String elementID,
            String elementName,
            String elementAdmin,
            String associateElementID,
            String categoryID,
            String unit,
            String lowerLimit,
            String upperLimit,
            String readMaskingLevel,
            String writeMaskingLevel,
            String description,
            String vid)
        {
            bool isExistRecord = false;
            if (IsExistRecord(propertyPath, out isExistRecord) == false)
                return false;

            if (isExistRecord)
                return false;

            String[] columnArr = new String[]
            {
                PramElementColumn.PropertyPath,
                PramElementColumn.ElementID,
                PramElementColumn.ElementName,
                PramElementColumn.ElementAdmin,
                PramElementColumn.AssociateElementID,
                PramElementColumn.CategoryID,
                PramElementColumn.Unit,
                PramElementColumn.LowerLimit,
                PramElementColumn.UpperLimit,
                PramElementColumn.ReadMaskingLevel,
                PramElementColumn.WriteMaskingLevel,
                PramElementColumn.Description,
                PramElementColumn.VID
            };

            int intElementId = 0;
            int.TryParse(elementID, out intElementId);

            double dblLowerLimit = 0.0;
            double.TryParse(lowerLimit, out dblLowerLimit);

            double dblUpperLimit = 0.0;
            double.TryParse(upperLimit, out dblUpperLimit);

            int intReadMaskingLevel = 100;
            if (int.TryParse(readMaskingLevel, out intReadMaskingLevel) == false)
            {
                intReadMaskingLevel = 100;
            }

            int intwriteMaskingLevel = 100;
            if (int.TryParse(writeMaskingLevel, out intwriteMaskingLevel) == false)
            {
                intwriteMaskingLevel = 100;
            }

            int intvid = 0;
            int.TryParse(vid, out intvid);

            Object[] valueArr = new Object[]
            {
                propertyPath,
                intElementId,
                elementName,
                elementAdmin,
                associateElementID,
                categoryID,
                unit,
                dblLowerLimit,
                dblUpperLimit,
                intReadMaskingLevel,
                intwriteMaskingLevel,
                description,
                intvid
            };

            return InsertRecord(propertyPath, columnArr, valueArr);
        }

        //==> Element ID 자동 부여
        public void InsertRecordAutoIncID(
            String propertyPath,
            String elementName,
            String elementAdmin,
            String associateElementID,
            String categoryID,
            String unit,
            String lowerLimit,
            String upperLimit,
            String readMaskingLevel,
            String writeMaskingLevel,
            String description,
            String vid)
        {
            while (_ElementIDSet.Contains(_LastAddElementID.ToString()))
            {
                ++_LastAddElementID;
            }

            if (InsertRecord(
                propertyPath: propertyPath,
                elementID: _LastAddElementID.ToString(),
                elementName: elementName,
                elementAdmin: elementAdmin,
                associateElementID: associateElementID,
                categoryID: categoryID,
                unit: unit,
                lowerLimit: lowerLimit,
                upperLimit: upperLimit,
                readMaskingLevel: readMaskingLevel,
                writeMaskingLevel: writeMaskingLevel,
                description: description,
                vid : vid))
            {
                _ElementIDSet.Add(_LastAddElementID.ToString());
                ++_LastAddElementID;
            }
        }

    }

    public static class PramElementColumn
    {
        public static String PropertyPath { get { return GetPropertyName(); } }
        public static String ElementID { get { return GetPropertyName(); } }
        public static String ElementName { get { return GetPropertyName(); } }
        public static String ElementAdmin { get { return GetPropertyName(); } }
        public static String AssociateElementID { get { return GetPropertyName(); } }
        public static String CategoryID { get { return GetPropertyName(); } }
        public static String Unit { get { return GetPropertyName(); } }
        public static String LowerLimit { get { return GetPropertyName(); } }
        public static String UpperLimit { get { return GetPropertyName(); } }
        public static String ReadMaskingLevel { get { return GetPropertyName(); } }
        public static String WriteMaskingLevel { get { return GetPropertyName(); } }
        public static String Description { get { return GetPropertyName(); } }
        public static String VID { get { return GetPropertyName(); } }
        public static List<String> ColumnList = new List<string>()
        {
            PropertyPath,
            ElementID,
            ElementName,
            ElementAdmin,
            AssociateElementID,
            CategoryID,
            Unit,
            LowerLimit,
            UpperLimit,
            ReadMaskingLevel,
            WriteMaskingLevel,
            Description,
            VID
        };

        private static String GetPropertyName([CallerMemberName]string propertyName = "")
        {
            return propertyName;
        }
    }
}
