using System;
using System.IO;
using LogModule;
using FtpUtil;
using System.Reflection;


namespace SpoolingUtil.TransferMethod
{
    /// <summary>
    ///  파일을 Upload하는 class
    /// </summary>
    public class TransferUsingFTP : ITransferMethods
    {
        #region ==>Variable init

        bool InitFlag { get; set; } = false;
        public SimpleFTPFunctions FtpFunctionObj { get; set; }

        public string LocalPATH  { get; set;} // = @"C:\ProberSystem\LoaderSystem\EMUL\Parameters\Resultmaps\Download\";
        #endregion

        public TransferUsingFTP(string username, string password, bool usePassiveMode, bool useBinary = true, int timeout = 30000, int readWriteStreamTimeout = 30000, bool keepAlive = false)
        {
            FtpFunctionObj = new SimpleFTPFunctions(username, password, usePassiveMode, useBinary, timeout, readWriteStreamTimeout, keepAlive);
            if (null != FtpFunctionObj)
                InitFlag = true;
        }

        /// <summary>
        /// user name과 pwd를 갱신한다
        /// </summary>
        /// <param name="username"> 접속할 user name</param>
        /// <param name="pwd"> 접속 password </param>
        private void UpdateUserNamePwd(string username, string pwd)
        {
            if (username.Length > 0)
                FtpFunctionObj.FTPUserName = username;
            if(pwd.Length > 0)
                FtpFunctionObj.FTPPassword = pwd;
        }

        /// <summary>
        /// download할 source path를 반환
        /// </summary>
        /// <param name="serverpath"> server path</param>
        /// <param name="filename"> target file name</param>
        /// <returns></returns>
        public string ResultMapPathOnFTP(string serverpath, string filename)
        {
            string foldername = Path.GetFileNameWithoutExtension(filename);
            if (serverpath[serverpath.Length - 1] == '/')
                serverpath = serverpath + filename.Substring(0, 4);
            else
                serverpath = serverpath + '/' + filename.Substring(0, 4);

            string server_dest_path = serverpath + '/' + foldername;
            return server_dest_path;
        }

        /// <summary>
        /// Result Map 파일을 FTP 상에 Upload 처리
        /// </summary>
        /// <param name="targetPath"> 목적지 경로 </param>
        /// <param name="sourcePath"> upload할 file full path</param>
        /// <param name="filename"> 전송할 파일이름</param>
        /// <param name="user_name"> user name</param>
        /// <param name="password"> password</param>
        /// <returns> ture - 성공, false - 실패 </returns>
        public bool UploadFileData(string serverpath, string sourcepath, string filename, string user_name = "", string password = "", bool odtp = false)
        {
            if (false == InitFlag)
            {
                LoggerManager.Error($"{MethodBase.GetCurrentMethod().Name} - Failed to initialize.");
                return false;
            }
            string server_dest_path;
            if (odtp == true) 
            {
                server_dest_path = serverpath;
            }
            else 
            {
                server_dest_path = ResultMapPathOnFTP(serverpath, filename);
            }
            string server_dest_fullpath = server_dest_path + '/' + filename;

            return UploadFileProc(server_dest_fullpath, sourcepath, user_name, password);
        }

        /// <summary>
        /// FTP file upload 처리
        /// </summary>
        /// <param name="destFullPath"> 목적지 full path</param>
        /// <param name="srcFullPath"> 올릴 파일 full path</param>
        /// <param name="username"> username</param>
        /// <param name="password"> password</param>
        /// <returns>true - 성공, false - 실패 </returns>
        public bool UploadFileProc(string destFullPath, string srcFullPath, string username, string password)
        {
            UpdateUserNamePwd(username, password); /// update user name and password

            string destPath = destFullPath.Substring(0, destFullPath.LastIndexOf("/"));
            /// 1. check the folder(on FTP), we should check sub path and will make it.
            /// 
            if (false == FtpFunctionObj.RecursiveCreateDir(destPath))
            {
                LoggerManager.Error($"{MethodBase.GetCurrentMethod().Name} - Failed to check & create for sub path list");
                return false;
            }

            /// 2. If the same file exist we should delete.      
            /// 
            (bool sucess, bool exist) = FtpFunctionObj.DoesPathExist(destFullPath);
            if (false == sucess)
            {
                LoggerManager.Error($"{MethodBase.GetCurrentMethod().Name} - Failed to check for path (path = {destFullPath})");
                return false;
            }

            if (exist)
            {
                if (false == FtpFunctionObj.DeleteFile(destFullPath))
                {
                    return false;
                }
            }

            /// 3. Result MAP 파일 Upload
            /// 
            if (false == FtpFunctionObj.UploadFileTo_FTP(srcFullPath, destFullPath))
            {
                LoggerManager.Error($"{MethodBase.GetCurrentMethod().Name} - UploadFileTo_FTP (source path = {srcFullPath}, dest Path ={destFullPath})");
                return false;
            }

            return true;
        }

        /// <summary>
        /// FTP상에 있는 file을 download 처리한다.
        /// </summary>
        /// <param name="serverpath"> server path </param>
        /// <param name="filename"> target file name </param>
        /// <param name="downpath"> destination path </param>
        /// <param name="user_name"> user name</param>
        /// <param name="password"> password</param>
        /// <returns> true - 성공, false - 실패 </returns>
        public bool DownloadFileData(string serverpath, string filename, string downpath, string user_name = "", string password = "")
        {
            string targetPathOnFTP = ResultMapPathOnFTP(serverpath, filename);
            string sourceFullPath = targetPathOnFTP + '/' + filename;

            string destSubPath = Path.GetFileNameWithoutExtension(filename);
            string destPath = Path.Combine(downpath, destSubPath);
            string destFullPath = Path.Combine(destPath, filename);

            UpdateUserNamePwd(user_name, password); /// update user name and password

            /// 1. FTP상에 파일 존재 여부 체크
            ///
            (bool sucess, bool exist) = FtpFunctionObj.DoesPathExist(sourceFullPath);
            if(false == sucess)
            {
                LoggerManager.Error($"{MethodBase.GetCurrentMethod().Name} - Failed to check for path (path = {sourceFullPath})");
                return false;
            }

            /// 2. Result Map을 받을 경로 생성
            try
            {
                if (Directory.Exists(destPath) == false)
                    Directory.CreateDirectory(destPath);
                else
                {
                    if (File.Exists(destFullPath))
                        File.Delete(destFullPath);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{MethodBase.GetCurrentMethod().Name} - Failed to create dir(err = {err.Message}, path={destPath}, file={filename})");
                return false;
            }

            /// 3. Result Map파일 download
            /// 
            if( false == FtpFunctionObj.DownloadFileFromFTP(sourceFullPath, destFullPath) )
            {
                LoggerManager.Error($"{MethodBase.GetCurrentMethod().Name} - Failed to download file. src={sourceFullPath}, dest={destFullPath}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 인자로 들어온 path가 존재하는지 체크
        /// </summary>
        /// <param name="serverpath"> server path</param>
        /// <param name="sub_folder"> 접근할 sub folder</param>
        /// <param name="user_name"> user name </param>
        /// <param name="password"> password </param>
        /// <returns></returns>
        public bool PathCheck(string serverpath, string sub_folder, string user_name = "", string password = "")
        {
            UpdateUserNamePwd(user_name, password); /// update user name and password

            string checkTargetPath = ResultMapPathOnFTP(serverpath, sub_folder); 

            (bool sucess, bool exist) = FtpFunctionObj.DoesPathExist(checkTargetPath);
            if ( false == sucess )
            {
                LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name} - have to check that network connect between server and loader(path={checkTargetPath})");
                return false;
            }
            else
            {
                LoggerManager.Debug($"{MethodBase.GetCurrentMethod().Name} - Connetion success - exist path{exist}");
            }

            return exist;
        }
    }
}
