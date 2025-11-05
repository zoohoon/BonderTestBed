using System;

namespace DBManagerModule
{
    using LogModule;
    using System.Data;
    using System.Data.Common;

    /*
     * DB Query를 실행 시키는 역할 수행
     */
    public class DBController : IDisposable
    {
        private DbProviderFactory _DbFactory;
        private DbConnection _DbConnection;
        public DbConnection DbConnection => _DbConnection;
        //==> App.Config String Value
        private readonly String _DBName;
        private readonly String _DBConnectionStr;
        private readonly String _ServerConnectionStr;
        private readonly String _ProviderName;
        public DBController(String dbName, String serverConnectionStr, String dbConnectionStr, String providerName)
        {
            _DBName = dbName;
            _ServerConnectionStr = serverConnectionStr;
            _DBConnectionStr = dbConnectionStr;
            _ProviderName = providerName;
        }
        public DBStatus Open()
        {
            if (IsExistsDB() == false)
            {
                if (CreateDataBase() == false)
                    return DBStatus.CreatDBError;
            }
            if (ConnectDB() == false)
            {
                return DBStatus.ConnectDBError;
            }

            return DBStatus.IsOK;
        }

        #region ==> Open Function
        private bool IsExistsDB()
        {
            bool isExist = false;
            DbProviderFactory tmpDbFactory = null;
            DbConnection tmpConn = null;
            DbCommand cmd = null;
            try
            {
                //==> DB Factory Get
                tmpDbFactory = DbProviderFactories.GetFactory(_ProviderName);

                //==> DB Connection Set
                tmpConn = tmpDbFactory.CreateConnection();
                tmpConn.ConnectionString = _ServerConnectionStr;
                tmpConn.Open();

                //==> DB Command Set
                cmd = tmpDbFactory.CreateCommand();
                cmd.Connection = tmpConn;
                cmd.CommandText = "SELECT database_id FROM sys.databases WHERE Name = @DBName";

                //==> Parameter Set
                DbParameter param = tmpDbFactory.CreateParameter();
                param.ParameterName = "@DBName";
                param.Value = _DBName;
                param.DbType = DbType.String;
                cmd.Parameters.Add(param);

                //==> Check DB Exists
                int dbID = 0;
                object resultObj = cmd.ExecuteScalar();
                if (resultObj != null)
                {
                    int.TryParse(resultObj.ToString(), out dbID);
                }
                isExist = (dbID > 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                isExist = false;
            }
            finally
            {
                if (tmpConn != null)
                    tmpConn.Dispose();
                if (cmd != null)
                    cmd.Dispose();
            }

            return isExist;
        }
        private bool CreateDataBase()
        {
            bool isCreateSuccess = true;
            DbProviderFactory tmpDbFactory = null;
            DbConnection tmpConn = null;
            DbCommand cmd = null;
            try
            {
                //==> DB Factory Get
                tmpDbFactory = DbProviderFactories.GetFactory(_ProviderName);

                //==> DB Connection Set
                tmpConn = tmpDbFactory.CreateConnection();
                tmpConn.ConnectionString = _ServerConnectionStr;
                tmpConn.Open();

                //==> DB Command Set
                cmd = tmpDbFactory.CreateCommand();
                cmd.Connection = tmpConn;
                cmd.CommandText = "CREATE DATABASE " + _DBName;

                //==> Execute Query
                int ret = cmd.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[DBController] [Method = CreateDataBase] [Error = {err}]");
                isCreateSuccess = false;
            }
            finally
            {
                if (tmpConn != null)
                    tmpConn.Dispose();
                if (cmd != null)
                    cmd.Dispose();
            }

            return isCreateSuccess;
        }
        private bool ConnectDB()
        {
            bool isConnectSuccess = true;
            try
            {
                //==> DB Factory Get
                _DbFactory = DbProviderFactories.GetFactory(_ProviderName);//==> "System.Data.SqlClient";

                //==> DB Connect
                _DbConnection = _DbFactory.CreateConnection();
                _DbConnection.ConnectionString = _DBConnectionStr;//==>@"Data Source=(local)\SQLEXPRESS;Integrated Security=SSPI;Initial Catalog=AutoLot";
                _DbConnection.Open();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[DBController] [Method = ConnectDB] [Error = {err}]");

                isConnectSuccess = false;
                _DbConnection.Dispose();
            }

            return isConnectSuccess;
        }
        #endregion

        //==> 사용중인 DBMS에서 사용 가능한 DB Parameter를 생성한다.
        public DbParameter CreateQueryParameter()
        {
            return _DbFactory.CreateParameter();
        }
        //==> Non Query : 쿼리 실행 후 결과 값을 받지 않는다.
        public bool DoNonQuery(String query, params DbParameter[] paramList)
        {
            bool isSuccess = true;
            DbCommand cmd = null;
            try
            {
                //==> DB Command Set
                cmd = _DbFactory.CreateCommand();
                cmd.Connection = _DbConnection;
                cmd.CommandText = query;
                foreach (DbParameter param in paramList)
                {
                    cmd.Parameters.Add(param);
                }

                //==> Execute Query
                cmd.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[DBController] [Method = DoNonQuery] [Error = {err}]");
                LoggerManager.Debug($"[Query : {query}]");
                isSuccess = false;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }

            return isSuccess;
        }
        //==> Reader Query : 쿼리 실행 후 결과 값을 받는다.
        public DbDataReader DoReaderQuery(String query, params DbParameter[] paramList)
        {
            DbCommand cmd = null;
            DbDataReader reader = null;
            try
            {
                //==> DB Command Set
                cmd = _DbFactory.CreateCommand();
                cmd.Connection = _DbConnection;
                cmd.CommandText = query;
                foreach (DbParameter param in paramList)
                {
                    cmd.Parameters.Add(param);
                }
                //==> Execute Query
                reader = cmd.ExecuteReader();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[DBController] [Method = DoReaderQuery] [Error = {err}]");
                LoggerManager.Debug($"[Query : {query}]");
                reader = null;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }
            return reader;//==> reader는 Caller에서 Dispose 호출해 주어야 한다.
        }
        //==> Scalar : 수학적 계산 쿼리(Row 갯수, 컬럼 갯수...)
        public Object DoExecuteScalar(String query, params DbParameter[] paramList)
        {
            DbCommand cmd = null;
            Object result = null;
            try
            {
                //==> DB Command Set
                cmd = _DbFactory.CreateCommand();
                cmd.Connection = _DbConnection;
                cmd.CommandText = query;
                foreach (DbParameter param in paramList)
                {
                    cmd.Parameters.Add(param);
                }
                //==> Execute Query
                result = cmd.ExecuteScalar();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[DBController] [Method = DoExecuteScalar] [Error = {err}]");
                LoggerManager.Debug($"[Query : {query}]");
                result = null;
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
            }
            return result;//==> reader는 Caller에서 Dispose 호출해 주어야 한다.
        }
        public bool Close()
        {
            bool isCloseSuccess = true;
            try
            {
                Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[DBController] [Method = Close] [Error = {err}]");
                isCloseSuccess = false;
            }
            return isCloseSuccess;
        }
        public void Dispose()
        {
            if (_DbConnection == null)
                return;
            _DbConnection.Close();
            _DbConnection.Dispose();
        }
    }
}
