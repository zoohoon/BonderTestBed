using System;
using System.Collections.Generic;
using System.Linq;

namespace FtpUtil
{
    /*
     * FtpStatusCode
     * Undefined : FTP Server에 접속 안됨
     * ActionNotTakenFileUnavailable : File을 못 찾음
     */
    using LogModule;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;

    public enum EnumFtpResult
    {
        NONE,
        CONNECTION_ERROR,
        DIRECTORY_ERROR,
        PERFORM_ERROR
    };

    public class FTPController
    {
        #region ==> README
        /*
        * Kaperski 방화벽에 의해 FTP 접속이 차단될 수 있다. 
        *  -> 이를 위해 FTP Manager를 사용하는 프로그램을 신뢰할 수 있는 프로그램으로 두어야 한다.
        * 
        * Active Mode : 
        *          FTP SERVER                   FTP Client
        *      20(DATA) 21(COMMAND)            5150    5151
        *          |       |                    |       |
        *          |       |<-----(1:port5151)--|       |
        *          |       |                    |       |
        *          |       |------(   2:OK   )->|       |
        *          |       |                    |       |
        *          |---------(3:DATA CHANNEL)---------->|
        *          |       |                    |       |
        *          |       |                    |       |
        *          |<----------(    4:OK    )-----------|
        *          
        *          Data Transfer (20)<====>(5151)
        *          
        *      - 동작하기 위해 FTP Manager가 있는 HOST PC(OPUS 장비)의 방화벽을 설정해 주저야 함.
        *      - Active Mode로 동작시 방화벽을 허용할 것인지 묻는 창이 뜰 것이다.
        *      
        * Passive Mode : 
        *          FTP SERVER                   FTP Client
        *      20(DATA) 21(COMMAND)            5150    5151
        *          |       |                    |       |
        *          |       |<-----(  1:PASV  )--|       |
        *          |  3267 |                    |       |
        *          |   |   |---(  2: OK 3267 )->|       |
        *          |   |   |                    |       |
        *              |<----------(DATA CHANNEL)-------|
        *          |   |   |                    |       |
        *          |   |   |                    |       |
        *          |   |-------(      OK    )---------->|
        *          
        *          Data Transfer (3267)<====>(5151)
        *          
        *      - 방확벽이 OUT PORT 검사도 하게 된다면 역시 설정해 주어야 함.
        * 
        * 
        */
        #endregion

        public String ServerIP { get; set; }
        public int ServerPort { get; set; }
        public String UserName { get; set; }
        public String Password { get; set; }

        private NetworkCredential _Credential;
        private bool _UsePassive;
        private int _DataBuffSize = 2048;
        private const int _TimeOut = 5000;

        public Func<long, long, bool> DataTransferEvent { get; set; }

        public Stack<String> SubDirectoryStack { get; set; }
        public String CurrentFtpWorkPath
        {
            get
            {
                String uri = "ftp://" + ServerIP + ":" + ServerPort + "/";
                foreach (var subDir in SubDirectoryStack.Reverse())
                {
                    uri += subDir + "/";
                }
                return uri;
            }
        }

        public FTPController(String userName, String password, String serverIP, int serverPort = 21)
        {
            SubDirectoryStack = new Stack<string>();
            _UsePassive = true;

            Setting(userName, password, serverIP, serverPort);
        }
        public void Setting(String userName, String password, String serverIP, int serverPort = 21)
        {
            UserName = userName;
            Password = password;
            ServerIP = serverIP;
            ServerPort = serverPort;
            _Credential = new NetworkCredential(UserName, Password);
        }

        //==> file type char
        //'d' : Directory
        //'-' : File
        public EnumFtpResult GetFileNameList(char[] fileTypeCharArr, out List<String> fileNameList)
        {

            fileNameList = new List<String>();
            if (fileTypeCharArr == null)
            {
                return EnumFtpResult.NONE;
            }

            if (fileTypeCharArr.Length == 0)
            {
                return EnumFtpResult.NONE;
            }

            try
            {
                FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(CurrentFtpWorkPath));
                //==> Sever에 Request를 보낸 후 완료되고 난 후 연결을 지속할 것인지 여부
                ftp.KeepAlive = false;
                ftp.Credentials = _Credential;
                //==> Specify the data type.
                ftp.UseBinary = true;
                //==> Specify the transfer type.
                ftp.UsePassive = _UsePassive;
                //==> Timeout
                ftp.Timeout = _TimeOut;

                ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails; /*WebRequestMethods.Ftp.ListDirectoryDetails*/

                using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()/*, System.Text.Encoding.Default*/))
                {
                    String line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        //==> EX) "-rw-r--r-- 1 ftp ftp          43998 Jan 24  2018 DeviceParam.zip"
                        String[] splitLine = line.Split(' ');
                        //==> EX) "-rw-r--r--"
                        String fileAttribute = splitLine[0];
                        //==> EX) "DeviceParam.zip"
                        String fileName = splitLine[splitLine.Length - 1];
                        //==> EX) '-'
                        char fileTypeChar = fileAttribute[0];
                        if (fileTypeCharArr.Contains(fileTypeChar))
                        {
                            fileNameList.Add(fileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //==> Connection 문제
                fileNameList = new List<String>();
                LoggerManager.Debug($"[FTP] : {EnumFtpResult.CONNECTION_ERROR}, {ex.Message}");
                return EnumFtpResult.CONNECTION_ERROR;
            }


            return EnumFtpResult.NONE;
        }
        public EnumFtpResult Download(String downloadPath, String fileName, bool isOverwrite = false)
        {
            String downloadFileFullPath = Path.Combine(downloadPath, fileName);
            String downloadUriFullPath = CurrentFtpWorkPath + fileName;

            if (BuildDirectory(downloadFileFullPath) == false)
            {
                return EnumFtpResult.DIRECTORY_ERROR;
            }

            if (File.Exists(downloadFileFullPath))
            {
                if (isOverwrite)
                {
                    File.Delete(downloadFileFullPath);
                }
                else
                {
                    return EnumFtpResult.NONE;
                }
            }

            try
            {
                long fileSize;
                FtpWebRequest fileSizeRequest = MakeFtpWebRequest(downloadUriFullPath, WebRequestMethods.Ftp.GetFileSize);
                using (FtpWebResponse response = (FtpWebResponse)fileSizeRequest.GetResponse())
                {
                    fileSize = response.ContentLength;
                }

                FtpWebRequest ftp = MakeFtpWebRequest(downloadUriFullPath, WebRequestMethods.Ftp.DownloadFile);

                using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
                using (Stream readerStream = response.GetResponseStream())
                using (FileStream outputStream = new FileStream(downloadFileFullPath, FileMode.Create))
                {


                    long totalReadCount = 0;
                    int readCount;
                    byte[] buffer = new byte[_DataBuffSize];

                    while ((readCount = readerStream.Read(buffer, 0, _DataBuffSize)) > 0)
                    {
                        totalReadCount += readCount;
                        outputStream.Write(buffer, 0, readCount);

                        if (DataTransferEvent != null)
                            DataTransferEvent(totalReadCount, fileSize);
                    }
                }

                #region ==> Simple Version, But not support zip file
                //using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
                //using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                //{
                //    String data = reader.ReadToEnd();
                //    File.WriteAllText(downloadFileFullPath, data);
                //}
                #endregion
            }
            catch (Exception ex)
            {
                LoggerManager.Debug($"[FTP] : {EnumFtpResult.CONNECTION_ERROR}, {ex.Message}");
                return EnumFtpResult.CONNECTION_ERROR;
            }

            if (File.Exists(downloadFileFullPath) == false)
            {
                //==> File 다운로드 실패
                LoggerManager.Debug($"[FTP] : {EnumFtpResult.PERFORM_ERROR}");
                return EnumFtpResult.PERFORM_ERROR;
            }

            return EnumFtpResult.NONE;
        }
        public EnumFtpResult Upload(String uploadPath, String fileName, bool isOverwrite = false)
        {
            EnumFtpResult result = EnumFtpResult.NONE;

            // 코드 단순화를 위해 하드코드함
            String uploadFileFullPath = Path.Combine(uploadPath, fileName);
            String uploadUriFullPath = CurrentFtpWorkPath + fileName;

            bool isExists;
            result = CheckFileExists(fileName, out isExists);

            if (result != EnumFtpResult.NONE)
                return result;

            if (isExists)
            {
                if (isOverwrite)
                {
                    DeleteFile(fileName);
                }
                else
                {
                    return EnumFtpResult.NONE;
                }
            }

            try
            {
                FtpWebRequest ftp = MakeFtpWebRequest(uploadUriFullPath, WebRequestMethods.Ftp.UploadFile);

                byte[] buff = new byte[_DataBuffSize];
                FileInfo fileInfo = new FileInfo(uploadFileFullPath);

                ftp.ContentLength = fileInfo.Length;

                using (Stream writeStream = ftp.GetRequestStream())
                using (FileStream fs = fileInfo.OpenRead())
                {
                    long fileSize = fileInfo.Length;
                    long totalWriteCount = 0;
                    int writeCount;
                    while ((writeCount = fs.Read(buff, 0, _DataBuffSize)) != 0)
                    {
                        writeStream.Write(buff, 0, writeCount);
                        totalWriteCount += writeCount;

                        if (DataTransferEvent != null)
                            DataTransferEvent(totalWriteCount, fileSize);
                    }
                }

                #region ==> Simple Version, But not support zip file
                //// 입력파일을 바이트 배열로 읽음
                //byte[] data = null;
                //using (StreamReader reader = new StreamReader(uploadFileFullPath))
                //{
                //    data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                //}

                //ftp.ContentLength = data.Length;
                //using (Stream reqStream = ftp.GetRequestStream())
                //{
                //    reqStream.Write(data, 0, data.Length);
                //}

                //using (FtpWebResponse resp = (FtpWebResponse)ftp.GetResponse())
                //{
                //    Console.WriteLine("Upload: {0}", resp.StatusDescription);
                //}
                #endregion
            }
            catch (Exception ex)
            {
                LoggerManager.Debug($"[FTP] : {EnumFtpResult.CONNECTION_ERROR}, {ex.Message}");
                return EnumFtpResult.CONNECTION_ERROR;
            }

            result = CheckFileExists(fileName, out isExists);

            if (result != EnumFtpResult.NONE)
                return result;

            if (isExists == false)
            {
                //==> File 업로드 실패
                LoggerManager.Debug($"[FTP] : {EnumFtpResult.PERFORM_ERROR}");
                return EnumFtpResult.PERFORM_ERROR;
            }

            return EnumFtpResult.NONE;
        }
        public EnumFtpResult DeleteFile(String fileName)
        {
            String deleteUriFullPath = CurrentFtpWorkPath + fileName;
            try
            {
                FtpWebRequest ftp = MakeFtpWebRequest(deleteUriFullPath, WebRequestMethods.Ftp.DeleteFile);
                using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
                {
                    //==> Server에 파일 삭제 요청
                }
            }
            catch (Exception ex)
            {
                LoggerManager.Debug($"[FTP] : {EnumFtpResult.CONNECTION_ERROR}, {ex.Message}");
                return EnumFtpResult.CONNECTION_ERROR;
            }

            EnumFtpResult result;
            bool isExists;
            result = CheckFileExists(fileName, out isExists);

            if (isExists)
            {
                //==> File 삭제 실패
                LoggerManager.Debug($"[FTP] : {EnumFtpResult.PERFORM_ERROR}");
                return EnumFtpResult.PERFORM_ERROR;
            }

            return EnumFtpResult.NONE;
        }
        public EnumFtpResult CheckFileExists(String fileName, out bool isExists)
        {
            isExists = true;
            String downloadUriFullPath = CurrentFtpWorkPath + fileName;

            try
            {
                FtpWebRequest ftp = MakeFtpWebRequest(downloadUriFullPath, WebRequestMethods.Ftp.GetDateTimestamp);
                using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
                {
                    //==> Just for Check File Exists
                }
            }
            catch (WebException ex)
            {
                isExists = false;

                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response != null && response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    //==> File이 존재하지 않음
                    return EnumFtpResult.NONE;
                }
                else
                {
                    //==> Connection 문제
                    LoggerManager.Debug($"[FTP] : {EnumFtpResult.CONNECTION_ERROR}, {ex.Message}");
                    return EnumFtpResult.CONNECTION_ERROR;
                }
            }

            return EnumFtpResult.NONE;
        }
        private FtpWebRequest MakeFtpWebRequest(String uri, String method)
        {
            // WebRequest.Create로 Http,Ftp,File Request 객체를 모두 생성할 수 있다.

            FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
            ftp.KeepAlive = false;
            ftp.Credentials = _Credential;
            ftp.UseBinary = true;//==>  Some servers require sending "TYPE I" before the SIZE command will work
            ftp.UsePassive = _UsePassive;
            ftp.Timeout = _TimeOut;

            ftp.Method = method;
            return ftp;
        }
        private bool BuildDirectory(String path)
        {
            if (String.IsNullOrEmpty(path))
                return false;

            String directoryPath = String.Empty;
            try
            {
                directoryPath = Path.GetDirectoryName(path);
                if (Directory.Exists(directoryPath))
                    return true;

                Directory.CreateDirectory(directoryPath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return false;
            }

            return Directory.Exists(directoryPath);
        }
        public void ChangeDir(String dirPath)
        {
            String[] dirSplit = dirPath.Split('/');
            foreach (var dirName in dirSplit)
            {
                if (dirName == String.Empty)
                    continue;
                SubDirectoryStack.Push(dirName);
            }
        }
        public void ChangeHomeDir()
        {
            SubDirectoryStack.Clear();

        }
        public void ChangeDirParent()
        {
            if (SubDirectoryStack.Count < 1)
                return;

            SubDirectoryStack.Pop();
        }
        public bool CheckConnection(int timeout)
        {
            //==> true : open
            //==> false : close
            IPAddress serverIpAddr;

            if (IPAddress.TryParse(ServerIP, out serverIpAddr) == false)
                return false;

            IPEndPoint ipEndPoint = new IPEndPoint(serverIpAddr, ServerPort);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            bool isOpen = false;
            try
            {
                IAsyncResult result = socket.BeginConnect(ipEndPoint, null, null);

                isOpen = result.AsyncWaitHandle.WaitOne(timeout, true);

                if (socket.Connected)
                {
                    socket.EndConnect(result);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[FTP] : {EnumFtpResult.CONNECTION_ERROR}, {err.Message}");
            }
            finally
            {
                socket.Close();
            }

            return isOpen;
        }
        public bool CheckConnection()
        {
            //==> true : open
            //==> false : close

            IPAddress serverIpAddr;

            if (IPAddress.TryParse(ServerIP, out serverIpAddr) == false)
                return false;

            bool isOpen = true;
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(serverIpAddr, ServerPort);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipEndPoint);
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 10061)
                    LoggerManager.Exception(ex);

                LoggerManager.Debug($"[FTP] : {EnumFtpResult.CONNECTION_ERROR}, {ex.Message}");
                isOpen = false;
            }
            return isOpen;
        }
        //private bool CheckIP()
        //{
        //    IPAddress serverIpAddr;
        //    if (IPAddress.TryParse(ServerIP, out serverIpAddr) == false)
        //        return false;
        //    Ping pingSender = new Ping();
        //    PingReply reply = pingSender.Send(serverIpAddr, 120);
        //    return reply.Status == IPStatus.Success;
        //}
    }
}