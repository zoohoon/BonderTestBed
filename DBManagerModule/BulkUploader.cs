using System;
using System.Collections.Generic;
using System.Linq;

namespace DBManagerModule
{
    using DBManagerModule.Table;
    using LogModule;
    using System.Data;
    using System.Data.SqlClient;
    public class BulkUploader : IDisposable
    {
        private DataTable _DataTable;
        private SqlConnection _Connection;
        private Dictionary<String, TableColumn> _ColumTemplate;
        private String _TableName;
        public String PrimaryKeyColumnName { get; set; }
        public int PrimaryKeyColumnIdx { get; set; }
        public int ColumnCount { get; set; }
        public BulkUploader(SqlConnection connection, String tableName, Dictionary<String, TableColumn> columTemplate, String primaryKeyColumn)
        {
            try
            {
                _DataTable = new DataTable(tableName);
                _TableName = tableName;
                _Connection = connection;
                _ColumTemplate = columTemplate;
                ColumnCount = _ColumTemplate.Keys.Count;

                PrimaryKeyColumnName = primaryKeyColumn;
                PrimaryKeyColumnIdx = _ColumTemplate.Keys.ToList().IndexOf(PrimaryKeyColumnName);


                BuildDataTableColumn();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void BuildDataTableColumn()
        {
            try
            {
                foreach (var item in _ColumTemplate)
                {
                    TableColumn tableColumn = item.Value;
                    DataColumn dataColumn = new DataColumn();

                    switch (tableColumn.Type)
                    {
                        case DbType.String:
                            dataColumn.DataType = typeof(String);
                            dataColumn.MaxLength = tableColumn.Length;
                            break;
                        case DbType.Int32:
                            dataColumn.DataType = typeof(Int32);
                            break;
                        case DbType.Double:
                            dataColumn.DataType = typeof(Double);
                            break;
                    }

                    dataColumn.ColumnName = tableColumn.ColumnCaption;
                    dataColumn.Caption = tableColumn.ColumnCaption;
                    dataColumn.AutoIncrement = tableColumn.AutoIncrement;

                    _DataTable.Columns.Add(dataColumn);
                }

                //==> set primary key column.
                DataColumn[] primaryKeyColumns = new DataColumn[1];
                primaryKeyColumns[0] = _DataTable.Columns[PrimaryKeyColumnName];
                _DataTable.PrimaryKey = primaryKeyColumns;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void InsertRow(params String[] record)
        {
            try
            {
                if (record.Length != ColumnCount)
                    return;

                DataRow row = _DataTable.NewRow();
                for (int i = 0; i < ColumnCount; i++)
                {
                    if (record[i] == null)
                    {
                        row[i] = DBNull.Value;
                        continue;
                    }

                    DbType type = _ColumTemplate.ElementAt(i).Value.Type;
                    switch (type)
                    {
                        case DbType.String:
                            row[i] = record[i];
                            break;
                        case DbType.Int32:
                            int intVal = 0;
                            int.TryParse(record[i], out intVal);
                            row[i] = intVal;
                            break;
                        case DbType.Double:
                            double dblVal = 0;
                            double.TryParse(record[i], out dblVal);
                            row[i] = dblVal;
                            break;
                    }
                }

                try
                {
                    if (_DataTable.Rows.Contains(record[PrimaryKeyColumnIdx]))
                        return;

                    _DataTable.Rows.Add(row);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void WriteToDB()
        {
            try
            {
                //==> make sure to enable triggers more on triggers in next post
                SqlBulkCopy bulkCopy = new SqlBulkCopy(
                        _Connection,
                        SqlBulkCopyOptions.TableLock |
                        SqlBulkCopyOptions.FireTriggers |
                        SqlBulkCopyOptions.UseInternalTransaction,
                        null);
                try
                {
                    bulkCopy.DestinationTableName = _TableName;
                    bulkCopy.WriteToServer(_DataTable);

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
                finally
                {
                    bulkCopy.Close();
                    _DataTable.Clear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void Dispose()
        {
            try
            {
                if (_DataTable == null)
                    return;
                _DataTable.Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
