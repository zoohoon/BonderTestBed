using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Reflection;

namespace FtpUtil
{
    using LogModule;
    using StringUtil;
    /// <summary>
    /// FTP 관련한 간단한 처리 모음 클래스
    /// </summary>
    public class SimpleFTPFunctions
    {
        public string FTPUserName { get; set; }
        public string FTPPassword { get; set; }

        public bool UseBinary { get; set; } = true;
        public bool UsePassive { get; set; } = true;
        
        public int Timeout { get; set; } = 5000;
        public int ReadWriteStreamTimeout { get; set; } = 5000;
        public bool KeepAlive { get; set; } = false;
        public string last_err_info { get; set; }
        
        public SimpleFTPFunctions(string username, string password, bool usePassiveMode, bool useBinary = true, int timeout = 5000, int readWriteStreamTimeout = 5000, bool keepAlive = false )
        {
            this.FTPUserName = username;
            this.FTPPassword = password;
            this.UseBinary = useBinary;
            this.UsePassive = usePassiveMode;
            this.Timeout = timeout;
            this.ReadWriteStreamTimeout = readWriteStreamTimeout;
            this.KeepAlive = keepAlive;            
        }

        /// <summary>
        /// FtpWebRequest class에 기본 정보 Set
        /// </summary>
        /// <param name="ftpWebReq"> FtpWebRequest 정보를 채워 반환</param>
        /// <param name="dirpath"> FTP 주소로 외부에 확인 하고 호출 필요</param>
        /// <param name="methods"> FTP Methods </param>
        private void SetBasicFtpWebRequestInf(out FtpWebRequest ftpWebReq, string dirpath, string methods)
        {
            ftpWebReq = (FtpWebRequest)FtpWebRequest.Create(dirpath);
            ftpWebReq.Method = methods;
            ftpWebReq.Credentials = new NetworkCredential(FTPUserName, FTPPassword);
            ftpWebReq.UsePassive = UsePassive;
            ftpWebReq.UseBinary = UseBinary;
            ftpWebReq.KeepAlive = KeepAlive;
            ftpWebReq.Timeout = Timeout;
            ftpWebReq.ReadWriteTimeout = ReadWriteStreamTimeout;
        }

        /// <summary>
        /// FTP 상에 인자로 들어온 경로가 존재하는지 반환
        /// </summary>
        /// <param name="path"> 확인할 경로 </param>
        /// <returns>
        /// first : 함수 성공 유무
        /// second: Path 존재 유무
        /// </returns>
        public (bool/*sucess*/, bool/*exist*/) DoesPathExist(string path)
        {
            string dirpath;
            if (path.Last() == '/')
                dirpath = path;
            else
                dirpath = path + '/';

            FtpWebRequest requestDir;

            try
            {
                SetBasicFtpWebRequestInf(out requestDir, dirpath, WebRequestMethods.Ftp.ListDirectory);
                if (null == requestDir)
                {
                    last_err_info = "Failed to set basic info(information is null)";
                    throw new Exception($"{MethodBase.GetCurrentMethod().Name} : {last_err_info}, dir={dirpath}");
                }

                using (FtpWebResponse res = (FtpWebResponse)requestDir.GetResponse()) { }
                return (true, true);
            }
            catch (WebException ex)
            {
                LoggerManager.Error($"[FTP]  Error occured while {MethodBase.GetCurrentMethod().Name}, {ex.Message} ");
                FtpWebResponse res = (FtpWebResponse)ex.Response;
                if(null != res)
                {
                    if (res.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        return (true, false);
                    }
                    else
                    {
                        string StatusDesc = (null != res.StatusDescription) ? res.StatusDescription.Replace("\r\n", "") : "";
                        last_err_info = $"Detail:{ex.Message}({StatusDesc} :{res.StatusCode})";
                        LoggerManager.Error($"[FTP] Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:{res.StatusCode} " +
                                    $"response description:{StatusDesc}, url={dirpath}, msg={ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                last_err_info = $"Detail: {ex.Message}";
                LoggerManager.Error($"[FTP] Error occured while {MethodBase.GetCurrentMethod().Name} err_msg = {ex.Message}, url={dirpath}");
            }

            return (false, false);
        }

        /// <summary>
        /// 인자로 들어온 Path를 생성한다.
        /// </summary>
        /// <param name="path"> 생성할 path </param>
        /// <returns>true -  성공, false - 실패 </returns>
        public bool CreateDir(string path)
        {
            string dirpath;
            if (path.Last() == '/')
                dirpath = path;
            else
                dirpath = path + '/';

            try
            {
                FtpWebRequest request = null;
                SetBasicFtpWebRequestInf(out request, dirpath, WebRequestMethods.Ftp.MakeDirectory);
                if (null == request)
                {
                    last_err_info = "Failed to set basic info(information is null)";
                    throw new Exception($"[FTP] {MethodBase.GetCurrentMethod().Name} : {last_err_info}");
                }

                using (FtpWebResponse res = (FtpWebResponse)request.GetResponse()) { }

                return true;
            }
            catch (WebException ex)
            {
                LoggerManager.Error($"[FTP]  Error occured while {MethodBase.GetCurrentMethod().Name}, {ex.Message} ");
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (null != response)
                {
                    string StatusDesc = (null != response.StatusDescription) ? response.StatusDescription.Replace("\r\n", "") : "";
                    last_err_info = $"Detail:{ex.Message}({StatusDesc} :{response.StatusCode})";
                    LoggerManager.Error($"[FTP] Error occured while {MethodBase.GetCurrentMethod().Name} " +
                        $"response statuscode:{response.StatusCode} " +
                        $"response description:{StatusDesc}, url={dirpath}, msg={ex.Message}");
                }
            }
            catch (Exception ex)
            {
                last_err_info = $"Detail: {ex.Message}";
                LoggerManager.Error($"[FTP] Error occured while {MethodBase.GetCurrentMethod().Name} err_msg = {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// 인자로 들어온 Path를 확인하여 해당 Path대로 Directory를 구성한다.
        /// </summary>
        /// <param name="path"> 생성할 Path</param>
        /// <returns> true - 성공, false - 실패 </returns>
        public bool RecursiveCreateDir(string path)
        {
            /// 1. Parsing & Get Path List
            string preFixKeyword = "ftp://";
            int SearchPos_Idx = preFixKeyword.Length; 

            List<string> SubFullPathList = new List<string>();
            StringTool.ParsingForPathTypeToSubPath(ref SubFullPathList, path, "/", SearchPos_Idx); //6
            foreach(var subPath in SubFullPathList)
            {
                (bool success, bool exist) = DoesPathExist(subPath);
                if (success)
                {
                    if (false == exist)
                    {
                        if (false == CreateDir(subPath))
                            return false;
                    }                        
                }
                else
                    return false;
            }

            return true;
        }

        /// <summary>
        /// FTP상에 있는 파일 제거 
        /// </summary>
        /// <param name="filepath"> 삭제할 file full path</param>
        /// <returns>true - 성공, false - 실패 </returns>
        public bool DeleteFile(string filepath)
        {
            try
            {
                FtpWebRequest request;
                SetBasicFtpWebRequestInf(out request, filepath, WebRequestMethods.Ftp.DeleteFile);
                using (FtpWebResponse res = (FtpWebResponse)request.GetResponse()) { }
                return true;
            }
            catch (WebException ex)
            {
                LoggerManager.Error($"[FTP]  Error occured while {MethodBase.GetCurrentMethod().Name}, {ex.Message} ");
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if(null != response)
                {
                    string StatusDesc = (null != response.StatusDescription) ? response.StatusDescription.Replace("\r\n", "") : "";
                    last_err_info = $"Detail:{ex.Message}({StatusDesc} :{response.StatusCode})";
                    LoggerManager.Error($"[FTP] Error occured while {MethodBase.GetCurrentMethod().Name} " +
                       $"response statuscode:{response.StatusCode} " +
                       $"response description:{StatusDesc}" +
                       $"path{filepath}, msg={ex.Message}");
                }
            }
            catch (Exception err)
            {
                last_err_info = $"Detail: {err.Message}";
                LoggerManager.Error($"[FTP] Error occured while {MethodBase.GetCurrentMethod().Name} : Failed to delete file on FTP: Error occurred. Err = {err.Message}, path={filepath}");
            }

            return false;
        }

        /// <summary>
        ///  FTP server로 파일을 업로드 처리한다.
        /// </summary>
        /// <param name="sourceFilePath"> 전송할 파일 full path</param>
        /// <param name="destPathOnFTP"> FTP상 full path</param>
        /// <param name="readUtf8"> 파일을 ut</param>
        /// <returns>true - 성공, false - 실패</returns>
        public bool UploadFileTo_FTP(string sourceFilePath, string destPathOnFTP)
        {
            try
            {
                byte[] ReadData;

                FtpWebRequest request;
                SetBasicFtpWebRequestInf(out request, destPathOnFTP, WebRequestMethods.Ftp.UploadFile);
                ReadData = File.ReadAllBytes(sourceFilePath);

                request.ContentLength = ReadData.Length;
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(ReadData, 0, ReadData.Length);
                }

                using (FtpWebResponse res = (FtpWebResponse)request.GetResponse()) { }
                return true;
            }
            catch (WebException ex)
            {
                LoggerManager.Error($"[FTP]  Error occured while {MethodBase.GetCurrentMethod().Name}, {ex.Message} ");
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if(null != response)
                {
                    string StatusDesc = (null != response.StatusDescription) ? response.StatusDescription.Replace("\r\n", "") : "";
                    last_err_info = $"Detail:{ex.Message}({StatusDesc} :{response.StatusCode})";
                    LoggerManager.Error($"[FTP] Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:{response.StatusCode} " +
                                    $"response description:{StatusDesc}" + $"src={sourceFilePath}, dest={destPathOnFTP},msg={ex.Message}");
                }
            }
            catch (Exception err)
            {
                last_err_info = $"Detail: {err.Message}";
                LoggerManager.Error($"[FTP] {MethodBase.GetCurrentMethod().Name} : Error occurred. Err = {err.Message}");
            }

            return false;
        }

        /// <summary>
        /// FTP를 통해 파일을 download 한다.
        /// </summary>
        /// <param name="sourceFilePath"> source file path </param>
        /// <param name="destFilePath"> destination file path </param>
        /// <returns>true - 성공, false - 실패</returns>
        public bool DownloadFileFromFTP(string sourceFilePath, string destFilePath)
        {
            try
            {
                string dirPath = Path.GetDirectoryName(destFilePath);
                if (false == Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                FtpWebRequest request;
                SetBasicFtpWebRequestInf(out request, sourceFilePath, WebRequestMethods.Ftp.DownloadFile);
                Stream sourceStream;
                Stream targetStream ;
                using (FtpWebResponse res = (FtpWebResponse)request.GetResponse())
                {
                    using (sourceStream = res.GetResponseStream())
                    {
                        using (targetStream = File.Create(destFilePath))
                        {
                            byte[] buffer = new byte[10240];
                            int read;
                            while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                targetStream.Write(buffer, 0, read);
                            }
                        }
                    }

                    LoggerManager.Debug($"[FTP] {MethodBase.GetCurrentMethod().Name} : Downloaded file. src ={sourceFilePath}, dest={destFilePath}");
                }

                return true;
            }
            catch (WebException ex)
            {
                LoggerManager.Error($"[FTP]  Error occured while {MethodBase.GetCurrentMethod().Name}, {ex.Message} ");
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if(null != response)
                {
                    string StatusDesc = (null != response.StatusDescription) ? response.StatusDescription.Replace("\r\n", "") : "";
                    last_err_info = $"Detail:{ex.Message}({StatusDesc} :{response.StatusCode})";
                    LoggerManager.Error($"[FTP] Error occured while {MethodBase.GetCurrentMethod().Name} " +
                        $"response statuscode:{response.StatusCode} " +
                        $"response description:{StatusDesc}" +
                        $"path{destFilePath}, msg={ex.Message}");
                }

            }
            catch (Exception err)
            {
                last_err_info = $"Detail: {err.Message}";
                LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} : Failed to download file on FTP: Error occurred. Err = {err.Message}, src={sourceFilePath}, dest={destFilePath}");
            }

            return false;
        }
    }
}
