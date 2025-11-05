using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DBManagerModule.Table
{
    using System.Data;
    public class EventInfoTable : DBTable
    {
        public EventInfoTable(String tableName, DBController dbController)
            : base(tableName, dbController)
        {
        }

        public override void InitRecordColumTemplate()
        {
            try
            {
                //==> 컬럼 템플릿 생성
                ColumTemplate = new Dictionary<String, TableColumn>();
                ColumTemplate.Add("EventType", new TableColumn(DbType.String, "EventType"));
                ColumTemplate.Add("EventNumber", new TableColumn(DbType.Int32, "EventNumber"));
                ColumTemplate.Add("Category", new TableColumn(DbType.String, "Category"));

                //==> Primary Key 지정 해야 함
                PrimaryKeyColumn = ColumTemplate.FirstOrDefault().Value.ColumnCaption;
                PrimaryKeyColumnType = ColumTemplate.FirstOrDefault().Value.Type;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
