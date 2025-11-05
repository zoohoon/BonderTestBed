using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SECS_Host.Help
{
    public static class Help_ExcelParsing
    {
        public static List<String> PasingFromExel_xls(String Path, String SheetName, char delimiter)
        {
            List<string> retStrList = new List<string>();
            string szConn = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 8.0;HDR=No'", Path);
            OleDbConnection conn = null;

            try
            {
                conn = new OleDbConnection(szConn);
                conn.Open();

                OleDbCommand cmd = new OleDbCommand(string.Format("SELECT * FROM [{0}$]", SheetName), conn);
                OleDbDataAdapter adpt = new OleDbDataAdapter(cmd);
                DataSet ds = new DataSet();
                adpt.Fill(ds);

                foreach(DataRow dr in ds.Tables[0].Rows)
                {
                    StringBuilder setStr = new StringBuilder();
                    foreach(var v in dr.ItemArray)
                    {
                        setStr.Append(string.Format("{0}{1}", v.ToString(), delimiter));
                    }
                    retStrList.Add(setStr.ToString());
                }
            }
            catch
            {
            }
            finally
            {
                conn.Close();
            }

            return retStrList;
        }
    }
}
