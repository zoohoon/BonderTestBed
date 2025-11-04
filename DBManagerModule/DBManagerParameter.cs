using ProberErrorCode;
using System;

namespace DBManagerModule
{
    [Serializable]
    public class DBManagerParameter
    {
        public String DBName { get; set; }
        public String DBConnectionStr { get; set; }
        public String ServerConnectionStr { get; set; }
        public String ProviderName { get; set; }

        public String DevParamUpdateDate { get; set; }
        public String SysParamUpdateDate { get; set; }
        public String ComParamUpdateDate { get; set; }
        public bool EnableDBSourceServer { get; set; }
        public bool EnableUpdateDBFile { get; set; }

        public EventCodeEnum SetDefaultParam()
        {
            DBName = "ProberDB";
            ServerConnectionStr = "server=(local)\\SQLEXPRESS;Trusted_Connection=yes";
            DBConnectionStr = $"Data Source=(local)\\SQLEXPRESS;Integrated Security=SSPI;Initial Catalog={DBName}";
            ProviderName = "System.Data.SqlClient";
            EnableDBSourceServer = false;
            EnableUpdateDBFile = true;

            return EventCodeEnum.NONE;
        }
    }
}
