using System;
using System.Collections.Generic;
using System.Linq;

namespace DBManagerModule.Table
{
    using AccountModule;
    using ProberInterfaces;
    using System.Data;
    using System.Runtime.CompilerServices;

    public class LoginDataListTable : DBTable
    {

        public LoginDataListTable(String tableName, DBController dbController) :
            base(tableName, dbController)
        {
        }
        public override void InitRecordColumTemplate()
        {
            //==> 컬럼 템플릿 생성
            ColumTemplate = new Dictionary<String, TableColumn>();
            ColumTemplate.Add(LoginDataList.UserName, new TableColumn(DbType.String, LoginDataList.UserName));
            ColumTemplate.Add(LoginDataList.Password, new TableColumn(DbType.String, LoginDataList.Password));
            ColumTemplate.Add(LoginDataList.UserLevel, new TableColumn(DbType.Int32, LoginDataList.UserLevel));
            ColumTemplate.Add(LoginDataList.ImageSource, new TableColumn(DbType.String, LoginDataList.ImageSource));

            //==> Primary Key 지정 해야 함
            PrimaryKeyColumn = ColumTemplate.FirstOrDefault().Value.ColumnCaption;
            PrimaryKeyColumnType = ColumTemplate.FirstOrDefault().Value.Type;
        }
        public void SetupSuperUser()
        {
            List<String> accountList = GetAllFieldByColumn(DBManager.LoginDataList.PrimaryKeyColumn);
            if(accountList.Contains(AccountManager.DefaultUserName) == false)
            {
                AddRecord((AccountManager.DefaultUserName), "admin", AccountManager.DefaultUserLevel,AccountManager.DefaultImageSource);
            }
        }
        public void AddRecord(String userName, String password, int userLevel, String imageSource)
        {
            bool isExistRecord = false;
            if (IsExistRecord(userName, out isExistRecord) == false)
            {
                //==> query 실패
                return;
            }

            if (isExistRecord)
                return;

            InsertRecord(userName);
            ModifyRecord(userName, password, userLevel, imageSource);
        }
        public void ModifyRecord(String userName, String password, int userLevel, String imageSource)
        {
            UpdateField(userName, LoginDataList.Password, DbType.String, password);
            UpdateField(userName, LoginDataList.UserLevel, DbType.Int32, userLevel);
            UpdateField(userName, LoginDataList.ImageSource, DbType.String, imageSource);
        }
        public void RemoveRecord(String userName)
        {
            DeleteRecord(userName);
        }

       public Account GetUserInfoFromDB(string userName)
        {
            Account userinfo = new Account();

            Dictionary<String, String> accountData = DBManager.LoginDataList.GetRecord(userName);

            string password = accountData[LoginDataList.Password];
            string userlevel = accountData[LoginDataList.UserLevel];
            string imagesource = accountData[LoginDataList.ImageSource];

            userinfo.UserName = userName;
            userinfo.Password = password;

            int intUserLevel = 0;
            int.TryParse(userlevel, out intUserLevel);
            userinfo.UserLevel = intUserLevel;
            userinfo.ImageSource = imagesource;

            return userinfo;
        }
        private Dictionary<String, String> GetRecord(String userName)
        {
            string password = DBManager.LoginDataList.ReadField(userName, LoginDataList.Password);
            string userlevel = DBManager.LoginDataList.ReadField(userName, LoginDataList.UserLevel);
            string imageSource = DBManager.LoginDataList.ReadField(userName, LoginDataList.ImageSource);

            Dictionary<String, String> dic = new Dictionary<String, String>();
            dic.Add(LoginDataList.UserName, userName);
            dic.Add(LoginDataList.Password, password);
            dic.Add(LoginDataList.UserLevel, userlevel);
            dic.Add(LoginDataList.ImageSource, imageSource);

            return dic;
        }
    }
    public static class LoginDataList
    {
        public static String UserName { get { return GetPropertyName(); } }
        public static String Password { get { return GetPropertyName(); } }
        public static String UserLevel { get { return GetPropertyName(); } }
        public static String ImageSource { get { return GetPropertyName(); } }
        private static String GetPropertyName([CallerMemberName]string propertyName = "")
        {
            return propertyName;
        }
    }
}
