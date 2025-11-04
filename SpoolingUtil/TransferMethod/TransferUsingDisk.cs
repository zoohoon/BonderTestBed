using System;
using LogModule;
using System.Reflection;
using System.IO;

namespace SpoolingUtil.TransferMethod
{
    /// <summary>
    /// ResultMAP파일을 disk에 이동 처리 하는 class(local disk)
    /// </summary>
    public class TransferUsingLocalDisk : ITransferMethods
    {
        /// <summary>
        /// Resuls MAP 파일을 가져올 path를 반환
        /// </summary>
        /// <param name="serverpath"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string GetResultMapPath(string serverpath, string filename)
        {
            string foldername = Path.GetFileNameWithoutExtension(filename);
            serverpath = Path.Combine(serverpath, filename.Substring(0, 4));
            return serverpath + '/' + foldername;
        }
      
        /// <summary>
        /// Result MAP파일을 다운로드
        /// </summary>
        /// <param name="serverpath"> Upload할 path(destination path)</param>
        /// <param name="sourcepath"> source path</param>
        /// <param name="filename"> file name</param>
        /// <param name="user_name">user name</param>
        /// <param name="password">pass word</param>
        /// <returns></returns>
        public bool UploadFileData(string serverpath, string sourcepath, string filename, string user_name = "", string password = "", bool odtp = false)
        {            
            string server_dest_path = GetResultMapPath(serverpath, filename);
            string server_dest_fullpath = server_dest_path + '/' + filename;          
            return UploadFileProc(server_dest_fullpath, sourcepath, user_name, password);            
        }

        /// <summary>
        /// file 전송(복사)
        /// </summary>
        /// <param name="destFullPath"> 목적지 full path</param>
        /// <param name="srcFullPath"> 올릴 파일 full path</param>
        /// <param name="username"> username</param>
        /// <param name="password"> password</param>
        /// <returns>true - 성공, false - 실패 </returns>
        public bool UploadFileProc(string destFullPath, string srcFullPath, string username, string password)
        {
            try
            {
                string destPath = Path.GetDirectoryName(destFullPath);
                if (Directory.Exists(destPath) == false)
                {
                    Directory.CreateDirectory(destPath);
                }
                else
                {
                    if (File.Exists(destFullPath))
                    {
                        File.Delete(destFullPath);
                    }
                }

                if (File.Exists(srcFullPath))
                {
                    File.Copy(srcFullPath, destFullPath);
                }

                return true;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"UploadFileData(Using local disk): Error occurred. Err = {err.Message}, source ={srcFullPath}, dest={destFullPath}");               
            }

            return false;
        }

        /// <summary>
        /// server path에 있는 파일을 localpath로 복사 처리한다.
        /// </summary>
        /// <param name="serverpath"> 접근해야할 path</param>
        /// <param name="filename"> target file name </param>
        /// <param name="downpath"> 저장할 path</param>
        /// <param name="user_name"> user name</param>
        /// <param name="password"> pass word</param>
        /// <returns></returns>
        public bool DownloadFileData(string serverpath, string filename, string downpath, string user_name = "", string password = "")
        {
            string server_dest_path = GetResultMapPath(serverpath, filename);
            string server_dest_fullpath = server_dest_path + '/' + filename;

            string directory = Path.GetFileNameWithoutExtension(server_dest_fullpath);
            downpath = Path.Combine(downpath, directory);
            string download_fullPath = Path.Combine(downpath, filename);

            try
            {
                if (false == Directory.Exists(downpath))
                {
                    Directory.CreateDirectory(downpath);
                }

                File.Copy(server_dest_fullpath, download_fullPath, true);
                return true;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{MethodBase.GetCurrentMethod().Name} - err_msg = {err.Message}");
                return false;
            }
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
            string target_path = GetResultMapPath(serverpath, sub_folder);
            try
            {
                if (false == Directory.Exists(target_path))
                {
                    LoggerManager.Debug($"have to check that network connect between server and loader(connection path={target_path})");
                    return false;
                }
                else
                {
                    LoggerManager.Debug($"Connection Check Succeed, Path: {target_path}");
                    return true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"{MethodBase.GetCurrentMethod().Name} - err_msg = {err.Message}");
                return false;
            }
        }
    }
}
