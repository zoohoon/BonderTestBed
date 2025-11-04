using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using LoaderBase.LoaderLog;
using System.IO;
using Autofac;
using System.Reflection;

namespace LoaderLogSpliterModule
{
    public class LoaderLogSplitManager : ILoaderLogSplitManager, INotifyPropertyChanged, IFactoryModule, IDisposable, ILoaderFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public bool Initialized { get; set; }
        public InitPriorityEnum InitPriority { get; set; }
        private Autofac.IContainer _Container => this.GetLoaderContainer();

        public ILoaderLogManagerModule LoaderLogModule => _Container.Resolve<ILoaderLogManagerModule>();

        private readonly string ftpPattern = @"^ftp://|FTP:// ";
        private readonly string networkPattern = @"^\\(\\(\w+\s*)+)";
        private readonly string localPattern = @"^[A-Z]:(\\(\w+\s*)+)";

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void DeInitModule()
        {
            return;
        }
        public void Dispose()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private List<string> _reasonOferror = new List<string>();

        public List<string> reasonOferror
        {
            get { return _reasonOferror; }
            set { _reasonOferror = value; }
        }

        private string _showErrorMsg = "";

        public string showErrorMsg
        {
            get { return _showErrorMsg; }
            set { _showErrorMsg = value; }
        }

        public EventCodeEnum ConnectCheck(string path, string username, string password)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (path != null)
                {
                    if (path.Length > 0)
                    {
                        if (Regex.IsMatch(path, ftpPattern))
                        {
                            try
                            {
                                reasonOferror.Clear();
                                ret = Connect(path);
                                if (ret != EventCodeEnum.NONE)
                                {
                                    // Path 에 디렉토리가 포함되어 있고, 서버에 해당 디렉토리가 없을 경우 Connect Fail 발생
                                    // 앞에 주소까지만 잘라서 Connect 할 경우 Connect Check 가능함.
                                    var u = new Uri(path);
                                    path = u.AbsoluteUri.Replace(u.AbsolutePath, "");
                                    //Retry
                                    ret = Connect(path);
                                    if (ret != EventCodeEnum.NONE)
                                    {                                        
                                        if (ret == EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT)
                                        {
                                            // user name or password incorrect
                                            ret = EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT;
                                            this.NotifyManager().Notify(EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT);
                                        }
                                        else
                                        {
                                            // Parent Folder Connection Check
                                            string tempPath = path.TrimEnd('/');
                                            string connectPath = tempPath.Substring(0, tempPath.LastIndexOf('/'));
                                            ret = Connect(connectPath);
                                            if (ret != EventCodeEnum.NONE)
                                            {                                                
                                                ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                                                this.NotifyManager().Notify(EventCodeEnum.LOGUPLOAD_CONNECT_FAIL);                                                
                                            }
                                        }                                                                                                                            
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                //Retry
                                LoggerManager.Error($"ConnectCheck(): Error occurred. Err = {err.Message}");
                                ret = Connect(string.Empty);
                                if (ret != EventCodeEnum.NONE)
                                {
                                    ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                                    this.NotifyManager().Notify(EventCodeEnum.LOGUPLOAD_CONNECT_FAIL);
                                }
                            }

                            EventCodeEnum Connect(string connPath)
                            {
                                WebResponse response = null;
                                FtpWebRequest requestDir = null;
                                showErrorMsg = null;
                                try
                                {
                                    string connectPath = string.Empty;
                                    if (string.IsNullOrEmpty(connPath))
                                    {
                                        connectPath = path;
                                    }
                                    else
                                    {
                                        connectPath = connPath;
                                    }

                                    requestDir = (FtpWebRequest)FtpWebRequest.Create(connectPath);

                                    requestDir.Method = WebRequestMethods.Ftp.ListDirectory;
                                    requestDir.Credentials = new NetworkCredential(username, password);
                                    requestDir.UsePassive = LoaderLogModule.LoaderLogParam.FTPUsePassive.Value;
                                    requestDir.KeepAlive = false;
                                    requestDir.Timeout = 5000;
                                    requestDir.ReadWriteTimeout = 5000;

                                    response = requestDir.GetResponse();
                                    response.Close();
                                    response.Dispose();
                                    ret = EventCodeEnum.NONE;
                                }
                                catch (WebException ex)
                                {
                                    FtpWebResponse responsed = (FtpWebResponse)ex.Response;
                                    if (responsed != null)
                                    {                                        
                                        if(responsed.StatusCode == FtpStatusCode.SendPasswordCommand ||
                                           responsed.StatusCode == FtpStatusCode.SendUserCommand ||
                                           responsed.StatusCode == FtpStatusCode.NotLoggedIn)
                                        {
                                            ret = EventCodeEnum.LOGUPLOAD_LOGIN_OR_PASSWORD_INCORRECT;                                            
                                        }
                                        else
                                        {
                                            ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                                        }
                                        LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                        $"response statuscode:{responsed.StatusCode} " +
                                        $"response description:{responsed.StatusDescription}");    
                                        if(responsed.StatusDescription == "" || responsed.StatusDescription == null)
                                        {
                                            reasonOferror.Add(responsed.StatusCode + " -" + " No decription.");
                                        }
                                        else
                                        {
                                            reasonOferror.Add(responsed.StatusCode + ":" + responsed.StatusDescription);
                                        }
                                        reasonOferror = reasonOferror.Distinct().ToList();
                                        foreach (var item in reasonOferror)
                                        {
                                            showErrorMsg += item.ToString();
                                        }
                                    }
                                    else
                                    {
                                        LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                                    $"response statuscode:unknown msg={ex.Message}");
                                        ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                                    }
                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Error($"ConnectCheck().Connect(): Error occurred. Err = {err.Message}");
                                    bool msg = reasonOferror.Contains("Undefined - No decription.");
                                    if (msg)
                                    {
                                        reasonOferror.Add($"\nInvalid URI: The Authority/Host could not be parsed.");
                                    }
                                    reasonOferror = reasonOferror.Distinct().ToList();
                                    foreach (var item in reasonOferror)
                                    {
                                        showErrorMsg += item.ToString();
                                    }
                                    ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                                }
                                finally
                                {
                                    requestDir?.Abort();
                                    response?.Close();
                                    response?.Dispose();
                                }
                                return ret;
                            }
                        }
                        else if (Regex.IsMatch(path, networkPattern))
                        {
                            NetworkCredential credential = new NetworkCredential(username, password);
                            var isconnected = NetworkConnection.ConnectValidate(path, credential);
                            if (isconnected)
                            {
                                ret = EventCodeEnum.NONE;
                            }
                            else
                            {
                                //Retry
                                isconnected = NetworkConnection.ConnectValidate(path, credential);
                                if (isconnected == false)
                                {
                                    // Parent Folder Connection Check
                                    string tempPath = path.TrimEnd('\\');
                                    string connectPath = tempPath.Substring(0, tempPath.LastIndexOf('\\'));

                                    isconnected = NetworkConnection.ConnectValidate(connectPath, credential);
                                    if (isconnected == false)
                                    {
                                        ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                                        this.NotifyManager().Notify(EventCodeEnum.LOGUPLOAD_CONNECT_FAIL);
                                    }
                                }
                                else
                                {
                                    ret = EventCodeEnum.NONE;
                                }
                            }
                        }
                        else if (Regex.IsMatch(path, localPattern))
                        {
                            ret = EventCodeEnum.NONE;
                        }
                        else
                        {
                            LoggerManager.Debug($"LoaderLogSplitManager.ConnectCheck() ServerPath dose not contain an Path Pattern {path}");
                            // Not support protocol
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"ConnectCheck(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
                reasonOferror.Clear();
            }

            return ret;
        }

        public EventCodeEnum CreateDicrectory(string path, string username, string password)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                string dirpath = null;
                if (Regex.IsMatch(path, ftpPattern))
                {
                    FtpWebResponse response;
                    try
                    {
                        if (path.Last() == '/')
                        {
                            dirpath = path;
                        }
                        else
                        {
                            dirpath = path + '/';
                        }
                        //create the directory
                        FtpWebRequest requestDir = (FtpWebRequest)FtpWebRequest.Create(dirpath);
                        requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
                        requestDir.Credentials = new NetworkCredential(username, password);
                        requestDir.UsePassive = LoaderLogModule.LoaderLogParam.FTPUsePassive.Value;
                        requestDir.UseBinary = true;
                        requestDir.KeepAlive = false;
                        requestDir.Timeout = 5000;
                        requestDir.ReadWriteTimeout = 5000;
                        response = (FtpWebResponse)requestDir.GetResponse();

                        //ftpStream.Close();
                        response.Close();
                        response.Dispose();
                        ret = EventCodeEnum.NONE;
                    }
                    catch (WebException ex)
                    {
                        LoggerManager.Error($"CreateDicrectory({path}): Error occurred. Err = {ex.Message}");
                        response = (FtpWebResponse)ex.Response;
                        if (response != null)
                        {
                            if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                            {
                                LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:{response.StatusCode} " +
                                    $"response description:{response.StatusDescription}");
                                response.Close();
                                ret = EventCodeEnum.LOGUPLOAD_CREATE_DIRECTORY_FAIL;
                            }
                            else if (ex.Status == WebExceptionStatus.ProtocolError)
                            {
                                LoggerManager.Debug($"Status Code : {((HttpWebResponse)ex.Response).StatusCode}");
                                LoggerManager.Debug($"Status Description : {((HttpWebResponse)ex.Response).StatusDescription}");
                                ret = EventCodeEnum.LOGUPLOAD_CREATE_DIRECTORY_FAIL;
                            }
                            else
                            {
                                LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:{response.StatusCode} " +
                                    $"response description:{response.StatusDescription}");
                                ret = EventCodeEnum.LOGUPLOAD_CREATE_DIRECTORY_FAIL;
                            }
                        }
                    }
                    finally
                    {
                    }
                }
                else if (Regex.IsMatch(path, networkPattern))
                {
                    try
                    {
                        if (path.Last() == '/')
                        {
                            dirpath = path; ;
                        }
                        else
                        {
                            dirpath = path + '/';
                        }
                        NetworkCredential credentials = new NetworkCredential(username, password);
                        using (new NetworkConnection(LoaderLogModule.LoaderLogParam.DeviceUpLoadPath.Value, credentials))
                        {
                            if (!Directory.Exists(dirpath))
                            {
                                Directory.CreateDirectory(dirpath);
                            }
                        }
                        ret = EventCodeEnum.NONE;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"CreateDicrectory(): Error occurred. Err = {err.Message}");
                        ret = EventCodeEnum.LOGUPLOAD_CREATE_DIRECTORY_FAIL;
                    }
                }
                else if (Regex.IsMatch(path, localPattern))
                {
                    try
                    {
                        if (path.Last() == '/')
                        {
                            dirpath = path; ;
                        }
                        else
                        {
                            dirpath = path + '/';
                        }
                        if (!Directory.Exists(dirpath))
                        {
                            Directory.CreateDirectory(dirpath);
                        }
                        ret = EventCodeEnum.NONE;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"CreateDicrectory(): Error occurred. Err = {err.Message}");
                        ret = EventCodeEnum.LOGUPLOAD_CREATE_DIRECTORY_FAIL;
                    }
                }
                else
                {
                    LoggerManager.Debug($"LoaderLogSplitManager.CreateDicrectory() ServerPath dose not contain an Path Pattern {path}");
                    // Not support protocol
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"CreateDicrectory(): Error occurred. Err = {err.Message}");
            }

            return ret;
        }
        public EventCodeEnum CheckFolderExist(string path, string username, string password)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                string dirpath = null;

                if (Regex.IsMatch(path, ftpPattern))
                {
                    try
                    {
                        if (path.Last() == '/')
                        {
                            dirpath = path;
                        }
                        else
                        {
                            dirpath = path + '/';
                        }

                        //create the directory
                        FtpWebRequest requestDir = (FtpWebRequest)FtpWebRequest.Create(dirpath);
                        requestDir.Method = WebRequestMethods.Ftp.ListDirectory;
                        requestDir.Credentials = new NetworkCredential(username, password);
                        requestDir.UsePassive = LoaderLogModule.LoaderLogParam.FTPUsePassive.Value;
                        requestDir.UseBinary = true;
                        requestDir.KeepAlive = false;
                        requestDir.Timeout = 5000;
                        requestDir.ReadWriteTimeout = 5000;
                        FtpWebResponse response = (FtpWebResponse)requestDir.GetResponse();

                        //ftpStream.Close();
                        response.Close();
                        response.Dispose();
                        ret = EventCodeEnum.NONE;
                    }
                    catch (WebException ex)
                    {
                        LoggerManager.Error($"CheckFolderExist({path}): Error occurred. Err = {ex.Message}");
                        FtpWebResponse response = (FtpWebResponse)ex.Response;
                        if (response != null)
                        {
                            if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                            {
                                response.Close();
                                response.Dispose();
                                LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:{response.StatusCode} " +
                                    $"response description:{response.StatusDescription}");
                                ret = EventCodeEnum.LOGUPLOAD_FOLDER_NOT_EXIST;
                            }
                            else if (ex.Status == WebExceptionStatus.ProtocolError)
                            {
                                LoggerManager.Debug($"Status Code : {((HttpWebResponse)ex.Response).StatusCode}");
                                LoggerManager.Debug($"Status Description : {((HttpWebResponse)ex.Response).StatusDescription}");
                                ret = EventCodeEnum.LOGUPLOAD_FOLDER_NOT_EXIST;
                            }
                            else
                            {
                                LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:{response.StatusCode} " +
                                    $"response description:{response.StatusDescription}");
                                ret = EventCodeEnum.LOGUPLOAD_FOLDER_NOT_EXIST;
                            }
                        }
                        else
                        {
                            LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:unknown msg={ex.Message}");
                            ret = EventCodeEnum.LOGUPLOAD_FOLDER_NOT_EXIST;
                        }
                    }
                    finally
                    {
                    }
                }
                else if (Regex.IsMatch(path, networkPattern))
                {
                    try
                    {
                        if (Directory.Exists(path) == false)
                        {
                            Directory.CreateDirectory(path);
                            ret = EventCodeEnum.NONE;
                        }
                        else
                        {
                            ret = EventCodeEnum.NONE;

                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"CheckFolderExist(): Error occurred. Err = {err.Message}");
                        ret = EventCodeEnum.LOGUPLOAD_CREATE_DIRECTORY_FAIL;
                    }

                }
                else if (Regex.IsMatch(path, localPattern))
                {
                    if (Directory.Exists(path) == false)
                    {
                        Directory.CreateDirectory(path);
                        ret = EventCodeEnum.NONE;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;

                    }
                }
                else
                {
                    LoggerManager.Debug($"LoaderLogSplitManager.CheckFolderExist() ServerPath dose not contain an Path Pattern {path}");
                    // Not support protocol
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return ret;
        }
        public EventCodeEnum GetLoaderDatesFromServer(string path, string username, string password, ref List<string> dateslist)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string loaderpath = null;
            if (path.Last() == '/')
            {
                loaderpath = path;
            }
            else
            {
                loaderpath = path + '/';
            }
            try
            {
                if (loaderpath?.Length > 0)
                {
                    if (Regex.IsMatch(loaderpath, ftpPattern))
                    {
                        try
                        {
                            ret = CheckFolderExist(loaderpath, username, password);
                            if (ret != EventCodeEnum.NONE)
                            {
                                ret = CreateDicrectory(loaderpath, username, password);
                            }

                            if (ret == EventCodeEnum.NONE)
                            {
                                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(loaderpath);
                                request.Method = WebRequestMethods.Ftp.ListDirectory;
                                request.UsePassive = LoaderLogModule.LoaderLogParam.FTPUsePassive.Value;
                                request.Credentials = new NetworkCredential(username, password);
                                request.KeepAlive = false;
                                request.Timeout = 30000;
                                request.ReadWriteTimeout = 30000;
                                List<string> lines = new List<string>();
                                FtpWebResponse listResponse = null;
                                Stream listStream = null;
                                StreamReader listReader = null;
                                using (listResponse = (FtpWebResponse)request.GetResponse())
                                {
                                    using (listStream = listResponse.GetResponseStream())
                                    {
                                        using (listReader = new StreamReader(listStream))
                                        {
                                            while (!listReader.EndOfStream)
                                            {
                                                lines.Add(listReader.ReadLine());
                                            }
                                            listReader.Close();
                                        }
                                        listStream.Close();
                                    }
                                    listResponse.Close();
                                }

                                foreach (string line in lines)
                                {
                                    Match m = GetMatchingRegex(line);
                                    if (m == null)
                                    {
                                        throw new Exception("Unable to parse line: " + line);
                                    }

                                    string name = m.Groups["name"].Value;
                                    dateslist.Add(name);
                                }

                                ret = EventCodeEnum.NONE;
                            }
                            else
                            {
                                ret = EventCodeEnum.LOGUPLOAD_FOLDER_NOT_EXIST;
                            }
                        }
                        catch (WebException ex)
                        {
                            FtpWebResponse response = (FtpWebResponse)ex.Response;
                            if (response != null)
                            {
                                LoggerManager.Error($"Error occured while{MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:{response.StatusCode} " +
                                    $"response description:{response.StatusDescription}" +
                                    $"path{path}");
                            }
                            else
                            {
                                LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:unknown msg={ex.Message}" +
                                    $"path{path}");
                            }
                            ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"GetLoaderDatesFromServer(): Error occurred. Err = {err.Message}");
                            LoggerManager.Error($"Error Occured while GetLoaderDatesFromServer() Path:{path}");
                            ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                        }
                    }
                    else if (Regex.IsMatch(path, networkPattern))
                    {
                        var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");
                        NetworkCredential credentials = null;
                        credentials = new NetworkCredential(username, password);
                        using (new NetworkConnection(loaderpath, credentials))
                        {
                            var serverpath = loaderpath;
                            if (Directory.Exists(serverpath))
                            {
                                string[] files = System.IO.Directory.GetFiles(serverpath, "*.log");
                                foreach (var s in files)
                                {
                                    System.IO.FileInfo fi = null;
                                    try
                                    {
                                        fi = new System.IO.FileInfo(s);
                                        foreach (Match m in reg.Matches(fi.Name))
                                        {
                                            dateslist.Add(m.Value);
                                            //LoaderDebugDatesFromServer.Add(m.Value);
                                        }
                                    }
                                    catch (System.IO.FileNotFoundException e)
                                    {
                                        LoggerManager.Exception(e);
                                        continue;
                                    }
                                    catch (Exception err)
                                    {
                                        LoggerManager.Exception(err);
                                    }
                                }
                            }
                            ret = EventCodeEnum.NONE;
                        }
                    }
                    else if (Regex.IsMatch(path, localPattern))
                    {
                        var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");
                        var serverpath = loaderpath;
                        if (Directory.Exists(serverpath))
                        {
                            string[] files = System.IO.Directory.GetFiles(serverpath, "*.log");
                            foreach (var s in files)
                            {
                                System.IO.FileInfo fi = null;
                                try
                                {
                                    fi = new System.IO.FileInfo(s);
                                    foreach (Match m in reg.Matches(fi.Name))
                                    {
                                        dateslist.Add(m.Value);
                                        //LoaderDebugDatesFromServer.Add(m.Value);
                                    }
                                }
                                catch (System.IO.FileNotFoundException e)
                                {
                                    LoggerManager.Exception(e);
                                    continue;
                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Exception(err);
                                }
                            }
                        }
                        ret = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Debug($"LoaderLogSplitManager.GetLoaderDatesFromServer() ServerPath dose not contain an Path Pattern {path}");
                        // Not support protocol
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetLoaderDatesFromServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public EventCodeEnum GetLoaderOCRDatesFromServer(string path, string username, string password, ref List<string> dateslist)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string loaderpath = null;
            loaderpath = path + '/';
            try
            {
                if (loaderpath?.Length > 0)
                {
                    if (Regex.IsMatch(loaderpath, ftpPattern))
                    {
                        try
                        {
                            ret = CheckFolderExist(loaderpath, username, password);
                            if (ret != EventCodeEnum.NONE)
                            {
                                ret = CreateDicrectory(loaderpath, username, password);
                            }

                            if (ret == EventCodeEnum.NONE)
                            {
                                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(loaderpath);
                                request.Method = WebRequestMethods.Ftp.ListDirectory;
                                request.UsePassive = LoaderLogModule.LoaderLogParam.FTPUsePassive.Value;
                                request.Credentials = new NetworkCredential(username, password);
                                request.KeepAlive = false;
                                request.Timeout = 30000;
                                request.ReadWriteTimeout = 30000;
                                List<string> lines = new List<string>();
                                FtpWebResponse listResponse = null;
                                Stream listStream = null;
                                StreamReader listReader = null;
                                using (listResponse = (FtpWebResponse)request.GetResponse())
                                {
                                    using (listStream = listResponse.GetResponseStream())
                                    {
                                        using (listReader = new StreamReader(listStream))
                                        {
                                            while (!listReader.EndOfStream)
                                            {
                                                lines.Add(listReader.ReadLine());
                                            }
                                            listReader.Close();
                                        }
                                        listStream.Close();
                                    }
                                    listResponse.Close();
                                }

                                foreach (string line in lines)
                                {
                                    Match m = GetMatchingRegex(line);
                                    if (m == null)
                                    {
                                        throw new Exception("Unable to parse line: " + line);
                                    }

                                    string name = m.Groups["name"].Value;
                                    dateslist.Add(name);
                                }

                                ret = EventCodeEnum.NONE;
                            }
                            else
                            {
                                ret = EventCodeEnum.LOGUPLOAD_FOLDER_NOT_EXIST;
                            }
                        }
                        catch (WebException ex)
                        {
                            FtpWebResponse response = (FtpWebResponse)ex.Response;
                            if (response != null)
                            {
                                LoggerManager.Error($"Error occured while{MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:{response.StatusCode} " +
                                    $"response description:{response.StatusDescription}" +
                                    $"path{path}");
                            }
                            else
                            {
                                LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:unknown msg={ex.Message}" +
                                    $"path{path}");
                            }
                            ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"GetLoaderOCRDatesFromServer(): Error occurred. Err = {err.Message}");

                        }
                    }
                    else if (Regex.IsMatch(path, networkPattern))
                    {
                        var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");
                        try
                        {
                            NetworkCredential credentials = null;
                            credentials = new NetworkCredential(username, password);
                            using (new NetworkConnection(loaderpath, credentials))
                            {
                                if (Directory.Exists(loaderpath))
                                {
                                    DirectoryInfo directory = new DirectoryInfo(loaderpath);
                                    var dir = directory.GetDirectories();
                                    foreach (var folder in dir)
                                    {
                                        foreach (Match m in reg.Matches(folder.Name))
                                        {
                                            dateslist.Add(m.Value);
                                        }
                                    }
                                }
                            }
                            ret = EventCodeEnum.NONE;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"GetLoaderOCRDatesFromServer(): Error occurred. Err = {err.Message}");
                        }
                    }
                    else if (Regex.IsMatch(path, localPattern))
                    {
                        ret = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Debug($"LoaderLogSplitManager.GetLoaderOCRDatesFromServer() ServerPath dose not contain an Path Pattern {loaderpath}");
                        // Not support protocol
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetLoaderOCRDatesFromServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public EventCodeEnum GetStageDatesFromServer(string cellindex, string path, string username, string password, ref List<string> dateslist)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string cellpath = null;
            //if (path[path.Length - 1] == '/')
            //{
            //    cellpath = path + cellindex + '/';
            //}
            //else
            //{
            //    cellpath = path + "/" + cellindex + '/';
            //}

            cellpath = path;

            try
            {
                if (cellpath?.Length > 0)
                {
                    if (Regex.IsMatch(cellpath, ftpPattern))
                    {
                        try
                        {
                            ret = CheckFolderExist(cellpath, username, password);
                            if (ret != EventCodeEnum.NONE)
                            {
                                ret = CreateDicrectory(cellpath, username, password);
                            }

                            if (ret == EventCodeEnum.NONE)
                            {
                                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(cellpath);
                                request.Method = WebRequestMethods.Ftp.ListDirectory;
                                request.UsePassive = LoaderLogModule.LoaderLogParam.FTPUsePassive.Value;
                                request.Credentials = new NetworkCredential(username, password);
                                request.Timeout = 30000;
                                request.ReadWriteTimeout = 30000;
                                List<string> lines = new List<string>();
                                FtpWebResponse listResponse = null;
                                Stream listStream = null;
                                StreamReader listReader = null;
                                using (listResponse = (FtpWebResponse)request.GetResponse())
                                {
                                    using (listStream = listResponse.GetResponseStream())
                                    {
                                        using (listReader = new StreamReader(listStream))
                                        {
                                            while (!listReader.EndOfStream)
                                            {
                                                lines.Add(listReader.ReadLine());
                                            }
                                            listReader.Close();
                                        }
                                        listStream.Close();
                                    }
                                    listResponse.Close();
                                }

                                foreach (string line in lines)
                                {
                                    Match m = GetMatchingRegex(line);
                                    if (m == null)
                                    {
                                        throw new Exception("Unable to parse line: " + line);
                                    }

                                    string name = m.Groups["name"].Value;
                                    dateslist.Add(name);
                                }

                                ret = EventCodeEnum.NONE;
                            }
                            else
                            {
                                ret = EventCodeEnum.LOGUPLOAD_FOLDER_NOT_EXIST;
                            }
                        }
                        catch (WebException ex)
                        {
                            FtpWebResponse response = (FtpWebResponse)ex.Response;
                            if (response != null)
                            {
                                LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:{response.StatusCode} " +
                                    $"response description:{response.StatusDescription}" +
                                    $"path{path}");
                            }
                            else
                            {
                                LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:unknown msg={ex.Message}" +
                                    $"path{path}");
                            }
                            ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"GetStageDatesFromServer(): Error occurred. Err = {err.Message}");
                        }
                    }
                    else if (Regex.IsMatch(path, networkPattern))
                    {
                        var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");
                        NetworkCredential credentials = null;
                        credentials = new NetworkCredential(username, password);
                        using (new NetworkConnection(path, credentials))
                        {
                            var serverpath = path + "\\" + cellindex;
                            if (Directory.Exists(serverpath))
                            {
                                string[] files = System.IO.Directory.GetFiles(serverpath, "*.log");
                                foreach (var s in files)
                                {
                                    System.IO.FileInfo fi = null;
                                    try
                                    {
                                        fi = new System.IO.FileInfo(s);
                                        foreach (Match m in reg.Matches(fi.Name))
                                        {
                                            dateslist.Add(m.Value);
                                            //StageDebugDatesFromServer.Add(m.Value);
                                        }
                                    }
                                    catch (System.IO.FileNotFoundException e)
                                    {
                                        LoggerManager.Error($"GetStageDatesFromServer(): Error occurred. Err = {e.Message}");
                                        LoggerManager.Exception(e);
                                        continue;
                                    }
                                    catch (Exception err)
                                    {
                                        LoggerManager.Error($"GetStageDatesFromServer(): Error occurred. Err = {err.Message}");
                                        LoggerManager.Exception(err);
                                    }
                                }

                            }
                        }
                        ret = EventCodeEnum.NONE;
                    }
                    else if (Regex.IsMatch(path, localPattern))
                    {
                        try
                        {
                            var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");
                            var serverpath = path + "\\" + cellindex;
                            if (Directory.Exists(serverpath))
                            {
                                string[] files = System.IO.Directory.GetFiles(serverpath, "*.log");
                                foreach (var s in files)
                                {
                                    System.IO.FileInfo fi = null;
                                    try
                                    {
                                        fi = new System.IO.FileInfo(s);
                                        foreach (Match m in reg.Matches(fi.Name))
                                        {
                                            dateslist.Add(m.Value);
                                            //StageDebugDatesFromServer.Add(m.Value);
                                        }
                                    }
                                    catch (System.IO.FileNotFoundException e)
                                    {
                                        LoggerManager.Error($"GetStageDatesFromServer(): Error occurred. Err = {e.Message}");
                                        LoggerManager.Exception(e);
                                        continue;
                                    }
                                    catch (Exception err)
                                    {
                                        LoggerManager.Error($"GetStageDatesFromServer(): Error occurred. Err = {err.Message}");
                                        LoggerManager.Exception(err);
                                    }
                                }
                            }
                            ret = EventCodeEnum.NONE;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"GetStageDatesFromServer(): Error occurred. Err = {err.Message}");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetStageDatesFromServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
      
        public byte[] ConvertImageToByteArray(string path)
        {
            byte[] imageByteArray = null;
            try
            {
                FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    imageByteArray = new byte[reader.BaseStream.Length];
                    for (int i = 0; i < reader.BaseStream.Length; i++)
                        imageByteArray[i] = reader.ReadByte();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return imageByteArray;
        }
        public EventCodeEnum LoaderLogUploadToServer(string sourcepath, string destpath, string username, string password, EnumUploadLogType logtype)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            string uploadlogtype = logtype.ToString();
            string finalDestpath = null;
            string localsourcepath = null;
            switch (logtype)
            {
                case EnumUploadLogType.Debug:
                    localsourcepath = sourcepath + '/' + "Debug";
                    localsourcepath = Path.GetFullPath(localsourcepath);
                    break;
                case EnumUploadLogType.Temp:
                    localsourcepath = sourcepath + '/' + "TEMP";
                    break;
                case EnumUploadLogType.PMI:
                    localsourcepath = sourcepath + '/' + "PMI";
                    break;
                case EnumUploadLogType.PIN:
                    localsourcepath = sourcepath + '/' + "PIN";
                    break;
                case EnumUploadLogType.LoaderDebug:
                    localsourcepath = sourcepath + '/' + "LOT";
                    break;
                case EnumUploadLogType.LoaderOCR:
                    localsourcepath = sourcepath + '/' + "Cognex";
                    break;
                default:
                    break;
            }
            
            try
            {
                if (string.IsNullOrEmpty(destpath))
                {
                    return EventCodeEnum.LOGUPLOAD_UPLOAD_PATH_NULL;
                }

                if (destpath[destpath.Length - 1] == '/')
                {
                    finalDestpath = destpath;
                }
                else
                {
                    finalDestpath = destpath + '/';
                }

                if (Regex.IsMatch(finalDestpath, ftpPattern))
                {
                    try
                    {
                        byte[] data;

                        if (logtype == EnumUploadLogType.LoaderDebug)
                        {
                            var localfiles = Directory.GetFiles(localsourcepath);
                            foreach (var file in localfiles)
                            {
                                FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(finalDestpath + '/' + Path.GetFileName(file));
                                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                                ftpRequest.UsePassive = LoaderLogModule.LoaderLogParam.FTPUsePassive.Value;
                                ftpRequest.Timeout = 30000;
                                ftpRequest.ReadWriteTimeout = 30000;
                                ftpRequest.Credentials = new NetworkCredential(username, password);
                                ftpRequest.KeepAlive = false;

                                using (StreamReader reader = new StreamReader(file))
                                {
                                    data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                                    reader.Close();
                                }
                                //data = ConvertImageToByteArray(file);
                                ftpRequest.ContentLength = data.Length;
                                Stream reqStream = null;
                                using (reqStream = ftpRequest.GetRequestStream())
                                {
                                    reqStream.Write(data, 0, data.Length);
                                    reqStream.Close();
                                }

                                FtpWebResponse resp = null;
                                using (resp = (FtpWebResponse)ftpRequest.GetResponse())
                                {
                                    // FTP 결과 상태 출력
                                    LoggerManager.Debug($"[LoaderLogSplitManager]Upload File: {Path.GetFileName(file)}");
                                    resp.Close();
                                }
                            }
                        }
                        else if (logtype == EnumUploadLogType.LoaderOCR)
                        {
                            var ocrLocaldir = Directory.GetDirectories(localsourcepath);
                            var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");
                            foreach (var dir in ocrLocaldir)
                            {
                                var date = reg.Match(dir);
                                ret = CheckFolderExist(finalDestpath + date, username, password);
                                if (ret != EventCodeEnum.NONE)
                                { 
                                    ret = CreateDicrectory(finalDestpath + date, username, password);
                                }

                                if (ret == EventCodeEnum.NONE)
                                {
                                    var ocrfile = Directory.GetFiles(Path.GetFullPath(dir));
                                    foreach (var file in ocrfile)
                                    {
                                        FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(finalDestpath + date + '/' + Path.GetFileName(file));
                                        ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                                        ftpRequest.UsePassive = LoaderLogModule.LoaderLogParam.FTPUsePassive.Value;
                                        ftpRequest.Credentials = new NetworkCredential(username, password);
                                        ftpRequest.UseBinary = true;
                                        ftpRequest.KeepAlive = false;
                                        ftpRequest.Timeout = 30000;
                                        ftpRequest.ReadWriteTimeout = 30000;
                                        //using (StreamReader reader = new StreamReader(file))
                                        //{
                                        //    data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                                        //}
                                        data = ConvertImageToByteArray(file);
                                        ftpRequest.ContentLength = data.Length;
                                        Stream reqStream = null;
                                        using (reqStream = ftpRequest.GetRequestStream())
                                        {
                                            reqStream.Write(data, 0, data.Length);
                                            reqStream.Close();
                                        }

                                        FtpWebResponse resp = null;
                                        using ( resp = (FtpWebResponse)ftpRequest.GetResponse())
                                        {
                                            // FTP 결과 상태 출력
                                            LoggerManager.Debug($"[LoaderLogSplitManager]Upload File: {Path.GetFileName(file)}");
                                            resp.Close();
                                        }
                                    }
                                }
                                else
                                {
                                    ret = EventCodeEnum.LOGUPLOAD_FOLDER_NOT_EXIST;
                                }
                            }
                        }
                        else
                        {

                        }

                        if (LoaderLogModule.LoaderLogParam.AutoLogUploadIntervalMinutes.Value > 0)
                        {
                            LoaderLogModule.SetIntervalForLogUpload(LoaderLogModule.LoaderLogParam.AutoLogUploadIntervalMinutes.Value);
                        }
                        ret = EventCodeEnum.NONE;
                    }
                    catch (WebException ex)
                    {
                        FtpWebResponse response = (FtpWebResponse)ex.Response;
                        if (response != null)
                        {
                            LoggerManager.Error($"[LoaderLogSplitManager]Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:{response.StatusCode} " +
                                    $"response description:{response.StatusDescription}" +
                                    $"path{finalDestpath}");
                        }
                        else
                        {
                            LoggerManager.Error($"[LoaderLogSplitManager]Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                    $"response statuscode:unknown msg={ex.Message}" +
                                    $"path{finalDestpath}");
                        }
                        ret = EventCodeEnum.LOGUPLOAD_UPLOAD_LOG_FAIL;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"[LoaderLogSplitManager]LoaderLogUploadToServer(): Error occurred. Err = {err.Message}");
                        LoggerManager.Debug($"[LoaderLogSplitManager]Fail LoaderLog Upload ");
                        ret = EventCodeEnum.LOGUPLOAD_UPLOAD_LOG_FAIL;
                    }
                    finally
                    {

                    }
                }
                else if (Regex.IsMatch(finalDestpath, networkPattern))
                {
                    try
                    {
                        if (logtype == EnumUploadLogType.LoaderDebug)
                        {
                            NetworkCredential credentials = new NetworkCredential(username, password);
                            using (new NetworkConnection(LoaderLogModule.LoaderLogParam.DeviceUpLoadPath.Value, credentials))
                            {
                                var localfiles = Directory.GetFiles(localsourcepath);
                                foreach (var file in localfiles)
                                {
                                    File.Copy(Path.GetFullPath(file), finalDestpath + '/' + Path.GetFileName(file));
                                    LoggerManager.Debug($"[LoaderLogSplitManager]Upload File: {Path.GetFileName(file)}");
                                }
                            }
                        }
                        else if (logtype == EnumUploadLogType.LoaderOCR)
                        {
                            var ocrLocaldir = Directory.GetDirectories(localsourcepath);
                            var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");
                            foreach (var dir in ocrLocaldir)
                            {
                                var date = reg.Match(dir);
                                ret = CreateDicrectory(finalDestpath + date, username, password);
                                if (ret == EventCodeEnum.NONE)
                                {
                                    var ocrfile = Directory.GetFiles(Path.GetFullPath(dir));
                                    NetworkCredential credentials = new NetworkCredential(username, password);
                                    using (new NetworkConnection(LoaderLogModule.LoaderLogParam.DeviceUpLoadPath.Value, credentials))
                                    {
                                        foreach (var file in ocrfile)
                                        {
                                            File.Copy(Path.GetFullPath(file), finalDestpath + date + '/' + Path.GetFileName(file));
                                            LoggerManager.Debug($"[LoaderLogSplitManager]Upload File: {Path.GetFileName(file)}");
                                        }
                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                        else
                        {

                        }

                        ret = EventCodeEnum.NONE;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"[LoaderLogSplitManager]LoaderLogUploadToServer(): Error occurred. Err = {err.Message}");
                        LoggerManager.Debug($"[LoaderLogSplitManager]Fail LoaderLog Upload ");
                        ret = EventCodeEnum.LOGUPLOAD_UPLOAD_LOG_FAIL;
                    }
                }
                string localPattern = @"^[A-Z]:(\\(\w+\s*)+)";
                if (Regex.IsMatch(finalDestpath, localPattern))
                {
                    try
                    {
                        var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");
                        string todaydate = $"{DateTime.Today.Year}-{DateTime.Today.Month.ToString().PadLeft(2, '0')}-{DateTime.Today.Day.ToString().PadLeft(2, '0')}";
                        if (logtype == EnumUploadLogType.LoaderDebug)
                        {
                            var localfiles = Directory.GetFiles(localsourcepath);
                            foreach (var file in localfiles)
                            {
                                foreach (Match m in reg.Matches(file))
                                {
                                    if (m.Value == todaydate)
                                    {
                                        File.Copy(Path.GetFullPath(file), finalDestpath + '/' + Path.GetFileName(file), true);
                                    }
                                    else
                                    {
                                        File.Copy(Path.GetFullPath(file), finalDestpath + '/' + Path.GetFileName(file));
                                    }
                                }
                                LoggerManager.Debug($"[LoaderLogSplitManager]Upload File: {Path.GetFileName(file)}");
                            }
                        }
                        else if(logtype == EnumUploadLogType.LoaderOCR)
                        {
                            var ocrLocaldir = Directory.GetDirectories(localsourcepath);
                            foreach (var dir in ocrLocaldir)
                            {
                                var date = reg.Match(dir);
                                ret = CreateDicrectory(finalDestpath + date, username, password);
                                if (ret == EventCodeEnum.NONE)
                                {
                                    var ocrfile = Directory.GetFiles(Path.GetFullPath(dir));
                                    
                                    foreach (var file in ocrfile)
                                    {
                                        File.Copy(Path.GetFullPath(file), finalDestpath + date + '/' + Path.GetFileName(file));
                                        LoggerManager.Debug($"[LoaderLogSplitManager]Upload File: {Path.GetFileName(file)}");
                                    }
                                }
                                else
                                {

                                }
                            }

                        }
                        else
                        {

                        }

                        ret = EventCodeEnum.NONE;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"[LoaderLogSplitManager]LoaderDeviceUploadToServer(): Error occurred. Err = {err.Message}");
                        LoggerManager.Debug($"[LoaderLogSplitManager]Fail Device Upload ");
                        ret = EventCodeEnum.LOGUPLOAD_DEVICE_UPLOAD_FAIL;
                        this.NotifyManager().Notify(ret);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"[LoaderLogSplitManager]LoaderLogUploadToServer(): Error occurred. Err = {err.Message}");
            }
            //finally
            //{
            //    this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            //}
            return ret;
        }
        public EventCodeEnum CellLogUploadToServer(int cellindex, string sourcepath, string destpath, string username, string password, EnumUploadLogType logtype, params string[] sub_paths)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string cellpath = null;
            string localsourcepath = null;
            string ftpPattern = @"^ftp://|FTP:// ";
            string networkPattern = @"^\\(\\(\w+\s*)+)";

            switch (logtype)
            {
                case EnumUploadLogType.Debug:
                    localsourcepath = sourcepath + '/' + "Debug";
                    localsourcepath = Path.GetFullPath(localsourcepath);
                    break;
                case EnumUploadLogType.Temp:
                    localsourcepath = sourcepath + '/' + "TEMP";
                    break;
                case EnumUploadLogType.PMI:
                    localsourcepath = sourcepath + '/' + "PMI";
                    break;
                case EnumUploadLogType.PIN:
                    localsourcepath = sourcepath + '/' + "PIN";
                    break;
                case EnumUploadLogType.LOT:
                    localsourcepath = sourcepath + '/' + "LOT";
                    break;
                case EnumUploadLogType.PMIImage:
                    localsourcepath = sourcepath;
                    break;
                case EnumUploadLogType.PINImage:
                    localsourcepath = sourcepath;
                    break;
                default:
                    break;
            }

            try
            {
                if (string.IsNullOrEmpty(destpath))
                {
                    return EventCodeEnum.LOGUPLOAD_UPLOAD_PATH_NULL;
                }

                if(LoaderLogModule.LoaderLogParam.CanUseStageLogParam.Value == false)
                {
                    if (destpath[destpath.Length - 1] == '/')
                    {
                        cellpath = destpath + $"Cell{cellindex.ToString().PadLeft(2, '0')}" + '/';
                    }
                    else
                    {
                        cellpath = destpath + '/' + $"Cell{cellindex.ToString().PadLeft(2, '0')}" + '/';
                    }
                    string retval_str = string.Empty;

                    if (CheckFolderExist(cellpath, username, password) != EventCodeEnum.NONE)
                    {
                        CreateDicrectory(cellpath, username, password);
                    }

                    retval_str = CombinePath(new[] { cellpath }.Concat(sub_paths).ToArray());

                    if (!retval_str.EndsWith("/"))
                    {
                        retval_str += "/";
                        cellpath = retval_str;
                    }
                }
                else
                {
                    cellpath = destpath;
                }
                //this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), logtype + "Log Uploading...");
                if (CheckFolderExist(cellpath, username, password) != EventCodeEnum.NONE)
                {
                    CreateDicrectory(cellpath, username, password);
                }

                if (Regex.IsMatch(cellpath, ftpPattern))
                {
                    try
                    {
                        byte[] data;
                        var localfiles = Directory.GetFiles(localsourcepath);
                        foreach (var item in localfiles)
                        {
                            FileInfo fi = new FileInfo(item);
                            FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(cellpath + '/' + Path.GetFileName(item));
                            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                            ftpRequest.UsePassive = LoaderLogModule.LoaderLogParam.FTPUsePassive.Value;
                            ftpRequest.Credentials = new NetworkCredential(username, password);
                            ftpRequest.KeepAlive = false;
                            ftpRequest.Timeout = 30000;
                            ftpRequest.ReadWriteTimeout = 30000;
                            ftpRequest.UseBinary = true;
                            using (StreamReader reader = new StreamReader(item))
                            {
                                data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                                reader.Close();
                            }
                            if (fi.Extension == ".mmo" || fi.Extension == ".bmp" || fi.Extension == ".jpg")
                            {
                                ftpRequest.UseBinary = true;
                                data = ConvertImageToByteArray(item);
                            }
                            ftpRequest.ContentLength = data.Length;
                            Stream reqStream = null;
                            using (reqStream = ftpRequest.GetRequestStream())
                            {
                                reqStream.Write(data, 0, data.Length);
                                reqStream.Close();
                            }
                            FtpWebResponse resp = null;
                            using (resp = (FtpWebResponse)ftpRequest.GetResponse())
                            {
                                // FTP 결과 상태 출력
                                LoggerManager.Debug($"[LoaderLogSplitManager]Upload File: {Path.GetFileName(item)}");
                                resp.Close();
                            }
                        }

                        if (LoaderLogModule.LoaderLogParam.AutoLogUploadIntervalMinutes.Value > 0)
                        {
                            LoaderLogModule.SetIntervalForLogUpload(LoaderLogModule.LoaderLogParam.AutoLogUploadIntervalMinutes.Value);
                        }
                        ret = EventCodeEnum.NONE;
                    }

                    catch (WebException ex)
                    {
                        FtpWebResponse response = (FtpWebResponse)ex.Response;
                        if(response != null)
                        {
                            LoggerManager.Error($"[LoaderLogSplitManager]Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                $"response statuscode:{response.StatusCode} " +
                                $"response description:{response.StatusDescription}" +
                                $"path{cellpath}");
                        }
                        else
                        {
                            LoggerManager.Error($"[LoaderLogSplitManager]Error occured while CellLogUploadToServer " +
                                $"Cell Index = {cellindex}, Source = {sourcepath}, Dest = {destpath}, Log Type = {logtype}" +
                                $"Response is Null, Path = " +
                                $"path{cellpath}");
                        }
                        ret = EventCodeEnum.LOGUPLOAD_UPLOAD_LOG_FAIL;
                    }
                    catch (SystemException syserr)
                    {
                        LoggerManager.Error($"[LoaderLogSplitManager]Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                $"Error Message: {syserr.Message} " +
                                $"path{cellpath}");
                        ret = EventCodeEnum.LOGUPLOAD_UPLOAD_LOG_FAIL;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"[LoaderLogSplitManager]CellLogUploadToServer(): Error occurred. Err = {err.Message}");
                        LoggerManager.Debug($"[LoaderLogSplitManager]Fail Upload Cell{cellindex} LogType:{logtype}");
                        ret = EventCodeEnum.LOGUPLOAD_UPLOAD_LOG_FAIL;
                    }
                }
                else if (Regex.IsMatch(cellpath, networkPattern))
                {
                    try
                    {
                        NetworkCredential credentials = new NetworkCredential(username, password);
                        using (new NetworkConnection(LoaderLogModule.LoaderLogParam.DeviceUpLoadPath.Value, credentials))
                        {
                            var localfiles = Directory.GetFiles(localsourcepath);
                            foreach (var file in localfiles)
                            {
                                File.Copy(Path.GetFullPath(file), cellpath + '/' + Path.GetFileName(file), true);
                                LoggerManager.Debug($"[LoaderLogSplitManager]Upload File: {Path.GetFileName(file)}");
                            }
                        }
                        ret = EventCodeEnum.NONE;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"[LoaderLogSplitManager]CellLogUploadToServer(): Error occurred. Err = {err.Message}");
                        LoggerManager.Debug($"[LoaderLogSplitManager]Fail Upload Cell{cellindex} LogType:{logtype}");
                        ret = EventCodeEnum.LOGUPLOAD_UPLOAD_LOG_FAIL;
                    }

                }
                else if (Regex.IsMatch(cellpath, localPattern))
                {
                    try
                    {
                        string todaydate = $"{DateTime.Today.Year}-{DateTime.Today.Month.ToString().PadLeft(2, '0')}-{DateTime.Today.Day.ToString().PadLeft(2, '0')}";
                        var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");
                        var localfiles = Directory.GetFiles(localsourcepath);
                        foreach (var file in localfiles)
                        {
                            foreach (Match m in reg.Matches(file))
                            {
                                if(m.Value == todaydate)
                                {
                                    File.Copy(Path.GetFullPath(file), cellpath + '/' + Path.GetFileName(file), true);
                                }
                                else
                                {
                                    File.Copy(Path.GetFullPath(file), cellpath + '/' + Path.GetFileName(file));
                                }
                            }
                            LoggerManager.Debug($"[LoaderLogSpliterModule]Upload File: {Path.GetFileName(file)}");
                        }
                        ret = EventCodeEnum.NONE;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"[LoaderLogSpliterModule]LoaderDeviceUploadToServer(): Error occurred. Err = {err.Message}");
                        LoggerManager.Debug($"[LoaderLogSpliterModule]Fail Device Upload ");
                        ret = EventCodeEnum.LOGUPLOAD_DEVICE_UPLOAD_FAIL;
                        this.NotifyManager().Notify(ret);
                    }
                }
                else
                {
                    // Not support protocol
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"[LoaderLogSplitManager]CellLogUploadToServer(): Error occurred. Err = {err.Message}");
            }
            //finally
            //{
            //    this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            //}
            return ret;
        }
        private string CombinePath(params string[] paths)
        {
            try
            {
                return Path.Combine(paths.Select(p => p.Trim('\\')).ToArray());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return string.Empty;
            }
        }
        public EventCodeEnum LoaderDeviceUploadToServer(string localzippath, string destpath, string username, string password)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (Regex.IsMatch(destpath, ftpPattern))
                {
                    recursiveUploadDirectory_FTP(localzippath, destpath, username, password);
                    ret = EventCodeEnum.NONE;
                }
                else if (Regex.IsMatch(destpath, networkPattern))
                {
                    recursiveUploadDirectory_NetWork(localzippath, destpath, username, password);
                    ret = EventCodeEnum.NONE;
                }
                else if (Regex.IsMatch(destpath, localPattern))
                {

                }
                else
                {
                    LoggerManager.Debug($"LoaderLogSplitManager.LoaderDeviceUploadToServer() ServerPath dose not contain an Path Pattern {destpath}");
                    // Not support protocol
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"LoaderDeviceUploadToServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Debug($"Fail Device Upload ");
                ret = EventCodeEnum.LOGUPLOAD_DEVICE_UPLOAD_FAIL;
                this.NotifyManager().Notify(ret);
            }

            return ret;
        }
        public EventCodeEnum LoaderDeviceDownloadFromServer(string serverpath, string localpath, string username, string password)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (Regex.IsMatch(serverpath, ftpPattern))
                {
                    ret = DownloadFtpDirectory(serverpath, localpath);
                }
                else if (Regex.IsMatch(serverpath, networkPattern))
                {
                    ret = recursiveDownloadDirectory_NetWork(serverpath, localpath, username, password);
                }
                else if (Regex.IsMatch(serverpath, localPattern))
                {

                }
                else
                {
                    LoggerManager.Debug($"LoaderLogSplitManager.LoaderDeviceDownloadFromServer() ServerPath dose not contain an Path Pattern {serverpath}");
                    // Not Support protocol
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"LoaderDeviceDownloadFromServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Debug($"Fail Device Download ");
                ret = EventCodeEnum.LOGUPLOAD_DEVICE_DOWNLOAD_FAIL;
                this.NotifyManager().Notify(ret);
            }

            return ret;
        }
        private Match GetMatchingRegex(string line)
        {
            Regex rx;
            Match m;
            for (int i = 0; i <= _ParseFormats.Length - 1; i++)
            {
                rx = new Regex(_ParseFormats[i]);
                m = rx.Match(line);
                if (m.Success)
                {
                    return m;
                }
            }
            return null;
        }

        #region "Regular expressions for parsing LIST results"
        /// <summary>
        /// List of REGEX formats for different FTP server listing formats
        /// </summary>
        /// <remarks>
        /// The first three are various UNIX/LINUX formats, fourth is for MS FTP
        /// in detailed mode and the last for MS FTP in 'DOS' mode.
        /// I wish VB.NET had support for Const arrays like C# but there you go
        /// </remarks>
        private static string[] _ParseFormats = new string[] {
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\w+\\s+\\w+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)",
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)",
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{1,2}:\\d{2})\\s+(?<name>.+)",
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\w+\\s+\\w+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{1,2}:\\d{2})\\s+(?<name>.+)",
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})(\\s+)(?<size>(\\d+))(\\s+)(?<ctbit>(\\w+\\s\\w+))(\\s+)(?<size2>(\\d+))\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{2}:\\d{2})\\s+(?<name>.+)",
            "(?<timestamp>\\d{2}\\-\\d{2}\\-\\d{2}\\s+\\d{2}:\\d{2}[Aa|Pp][mM])\\s+(?<dir>\\<\\w+\\>){0,1}(?<size>\\d+){0,1}\\s+(?<name>.+)",
            "(?<name>.+)"};
        #endregion

        private EventCodeEnum DownloadFtpDirectory(string downloadpath, string localPath)
        {
            EventCodeEnum ret = EventCodeEnum.LOGUPLOAD_DEVICE_DOWNLOAD_FAIL;
            try
            {
                NetworkCredential credential = new NetworkCredential(LoaderLogModule.LoaderLogParam.UserName.Value,
                    LoaderLogModule.LoaderLogParam.Password.Value);
                FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(downloadpath);
                listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                listRequest.UsePassive = LoaderLogModule.LoaderLogParam.FTPUsePassive.Value;
                listRequest.Credentials = credential;
                listRequest.KeepAlive = false;
                listRequest.Timeout = 30000;
                listRequest.ReadWriteTimeout = 30000;
                List<string> lines = new List<string>();
                FtpWebResponse listResponse = null;
                Stream listStream = null;
                StreamReader listReader = null;
                using (listResponse = (FtpWebResponse)listRequest.GetResponse())
                {
                    using (listStream = listResponse.GetResponseStream())
                    {
                        using (listReader = new StreamReader(listStream))
                        {
                            while (!listReader.EndOfStream)
                            {
                                lines.Add(listReader.ReadLine());
                            }
                            listReader.Close();
                        }
                        listStream.Close();
                    }
                    listResponse.Close();
                }

                if (lines.Count() != 0)
                {
                    if (!Directory.Exists(localPath))
                    {
                        Directory.CreateDirectory(localPath);
                    }
                }

                EventCodeEnum bRecursiveRet = EventCodeEnum.NONE;
                foreach (string line in lines)
                {
                    Match m = GetMatchingRegex(line);
                    if (m == null)
                    {
                        throw new Exception("Unable to parse line: " + line);
                    }

                    string name = m.Groups["name"].Value;
                    string dir = m.Groups["dir"].Value;

                    string localFilePath = Path.Combine(localPath, name);
                    string fileUrl = downloadpath + name;
                    if (downloadpath[downloadpath.Length - 1] == '/')
                    {
                        fileUrl = downloadpath + name;
                    }
                    else
                    {
                        fileUrl = downloadpath + '/' + name;
                    }

                    if (dir != "" && dir != "-")
                    {
                        if (!Directory.Exists(localFilePath))
                        {
                            Directory.CreateDirectory(localFilePath);
                        }
                        bRecursiveRet = DownloadFtpDirectory(fileUrl + "/", localFilePath);
                        if (bRecursiveRet != EventCodeEnum.NONE)
                            break;
                    }
                    else
                    {
                        FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(fileUrl);
                        downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                        downloadRequest.Credentials = credential;
                        downloadRequest.UsePassive = LoaderLogModule.LoaderLogParam.FTPUsePassive.Value;
                        downloadRequest.KeepAlive = false;
                        downloadRequest.Timeout = 30000;
                        downloadRequest.ReadWriteTimeout = 30000;
                        FtpWebResponse downloadResponse = null;
                        Stream sourceStream = null;
                        Stream targetStream = null;
                        using (downloadResponse = (FtpWebResponse)downloadRequest.GetResponse())
                        {
                            using (sourceStream = downloadResponse.GetResponseStream())
                            {
                                using (targetStream = File.Create(localFilePath))
                                {
                                    byte[] buffer = new byte[10240];
                                    int read;
                                    while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        targetStream.Write(buffer, 0, read);
                                        //LoggerManager.Debug($"Downloaded file{localFilePath}");
                                    }
                                    targetStream.Close();
                                }
                                sourceStream.Close();
                            }
                            downloadResponse.Close();
                        }
                    }
                }
                ret = bRecursiveRet;
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response != null)
                {
                    LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                $"response statuscode:{response.StatusCode} " +
                                $"response description:{response.StatusDescription}" +
                                $"path{downloadpath}" +
                                $"path{localPath}");
                }
                else
                {
                    LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                $"response statuscode:unknown msg={ex.Message}" +
                                $"path{downloadpath}" +
                                $"path{localPath}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"DownloadFtpDirectory(): Error occurred. Err = {err.Message}");
                LoggerManager.Error($"Error occured while device download");
            }
            return ret;
        }

        private EventCodeEnum recursiveDownloadDirectory_NetWork(string serverpath, string localpath, string username, string password)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string[] subDirectories = null;
            string lastPartOfCurrentDirectoryNamebyFolder = null;
            //filePaths = Directory.GetFiles(serverpath, "*.*");
            subDirectories = Directory.GetDirectories(serverpath);
            var serverfiles = Directory.GetFiles(serverpath);

            try
            {
                if (Directory.Exists(localpath))
                {
                    Directory.Delete(localpath, true);
                }

                foreach (var file in serverfiles)
                {
                    FileInfo fi = new FileInfo(file);
                    ret = CheckFolderExist(localpath, username, password);
                    if (ret != EventCodeEnum.NONE)
                    {
                        ret = CreateDicrectory(localpath, username, password);
                        if (ret != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"Fail CrateDirectory: {localpath}");
                        }
                    }
                    File.Copy(fi.FullName, localpath + '/' + Path.GetFileName(file));

                    // FTP 결과 상태 출력
                    LoggerManager.Debug($"Download File: {Path.GetFileName(file)}");
                }

                EventCodeEnum bRecursiveRet = EventCodeEnum.NONE;
                foreach (string subDir in subDirectories)
                {
                    DirectoryInfo di = new DirectoryInfo(subDir);
                    lastPartOfCurrentDirectoryNamebyFolder = di.Name;
                    string path = localpath + '/' + lastPartOfCurrentDirectoryNamebyFolder;
                    ret = CheckFolderExist(path, username, password);
                    if (ret != EventCodeEnum.NONE)
                    {
                        ret = CreateDicrectory(path, username, password);
                        if (ret != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"Fail CrateDirectory: {localpath + '/' }");
                        }
                    }
                    string subpath = serverpath + '/' + lastPartOfCurrentDirectoryNamebyFolder;
                    bRecursiveRet = recursiveDownloadDirectory_NetWork(subpath, localpath + '/' + lastPartOfCurrentDirectoryNamebyFolder, username, password);
                    if (bRecursiveRet != EventCodeEnum.NONE)
                        break;

                }
                ret = bRecursiveRet;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"recursiveDownloadDirectory_NetWork(): Error occurred. Err = {err.Message}");
                LoggerManager.Error($"Device Download Fail in recursiveDirectory");
            }
            return ret;
        }

        private EventCodeEnum recursiveUploadDirectory_FTP(string directoryPath, string despath, string username, string password)
        {
            EventCodeEnum ret = EventCodeEnum.LOGUPLOAD_DEVICE_UPLOAD_FAIL;
            string[] filePaths = null;
            string[] subDirectories = null;
            string lastPartOfCurrentDirectoryNamebyFile = null;
            string lastPartOfCurrentDirectoryNamebyFolder = null;
            filePaths = Directory.GetFiles(directoryPath, "*.*");
            subDirectories = Directory.GetDirectories(directoryPath);
            var localfiles = Directory.GetFiles(directoryPath);
            byte[] data;
            try
            {
                foreach (var file in localfiles)
                {
                    FileInfo fi = new FileInfo(file);
                    lastPartOfCurrentDirectoryNamebyFile = fi.Directory.Name;
                    ret = CheckFolderExist(despath + '/' + lastPartOfCurrentDirectoryNamebyFile, username, password);
                    if (ret != EventCodeEnum.NONE)
                    {
                        ret = CreateDicrectory(despath + '/' + lastPartOfCurrentDirectoryNamebyFile, username, password);
                        if (ret != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"Fail CrateDirectory: {despath + '/' + lastPartOfCurrentDirectoryNamebyFile}");
                        }
                    }
                    FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(despath + '/' + lastPartOfCurrentDirectoryNamebyFile +
                        '/' + Path.GetFileName(file));
                    ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                    ftpRequest.UsePassive = LoaderLogModule.LoaderLogParam.FTPUsePassive.Value;
                    ftpRequest.Credentials = new NetworkCredential(username, password);
                    ftpRequest.KeepAlive = false;
                    ftpRequest.UseBinary = false; // 필수!! resultmap 은 Binary 쓰면 안됨.
                    ftpRequest.Timeout = 30000;
                    ftpRequest.ReadWriteTimeout = 30000;
                    using (StreamReader reader = new StreamReader(file))
                    {
                        data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                        reader.Close();
                    }
                    if (fi.Extension == ".mmo" || fi.Extension == ".bmp")
                    {
                        ftpRequest.UseBinary = true;
                        data = ConvertImageToByteArray(file);
                    }
                    ftpRequest.ContentLength = data.Length;
                    Stream reqStream = null;
                    using (reqStream = ftpRequest.GetRequestStream())
                    {
                        reqStream.Write(data, 0, data.Length);
                        reqStream.Close();
                    }
                    FtpWebResponse resp = null;
                    using (resp = (FtpWebResponse)ftpRequest.GetResponse())
                    {
                        // FTP 결과 상태 출력
                        LoggerManager.Debug($"Upload File: {Path.GetFileName(file)}");
                        resp.Close();
                    }
                }

                EventCodeEnum bRecursiveRet = EventCodeEnum.NONE;
                foreach (string subDir in subDirectories)
                {
                    DirectoryInfo di = new DirectoryInfo(subDir);
                    lastPartOfCurrentDirectoryNamebyFolder = di.Name;
                    string fullPath = Path.GetFullPath(directoryPath).TrimEnd(Path.DirectorySeparatorChar);
                    string lastfolder = fullPath.Split(Path.DirectorySeparatorChar).Last();
                    lastPartOfCurrentDirectoryNamebyFile = lastfolder;
                    string path = despath + '/' + lastPartOfCurrentDirectoryNamebyFile + '/'
                        + lastPartOfCurrentDirectoryNamebyFolder;
                    ret = CheckFolderExist(path, username, password);
                    if (ret != EventCodeEnum.NONE)
                    {
                        ret = CreateDicrectory(path, username, password);
                        if (ret != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"Fail CrateDirectory: {despath + '/' }");
                        }
                    }
                    string subpath = directoryPath + '/' + lastPartOfCurrentDirectoryNamebyFolder;
                    bRecursiveRet = recursiveUploadDirectory_FTP(subpath, despath + '/' + lastPartOfCurrentDirectoryNamebyFile + '/', username, password);
                    if (bRecursiveRet != EventCodeEnum.NONE)
                        break;

                }
                ret = bRecursiveRet;
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response != null)
                {
                    LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                $"response statuscode:{response.StatusCode} " +
                                $"response description:{response.StatusDescription}" +
                                $"path{directoryPath}" +
                                $"path{despath}");
                }
                else
                {
                    LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                            $"response statuscode:unknown msg={ex.Message}" +
                                $"path{directoryPath}" +
                                $"path{despath}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"recursiveUploadDirectory_FTP(): Error occurred. Err = {err.Message}");
                LoggerManager.Error($"Device Upload Fail in recursiveDirectory");
            }
            return ret;
        }
        private EventCodeEnum recursiveUploadDirectory_NetWork(string directoryPath, string despath, string username, string password)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string[] filePaths = null;
            string[] subDirectories = null;
            string lastPartOfCurrentDirectoryNamebyFile = null;
            string lastPartOfCurrentDirectoryNamebyFolder = null;
            filePaths = Directory.GetFiles(directoryPath, "*.*");
            subDirectories = Directory.GetDirectories(directoryPath);
            var localfiles = Directory.GetFiles(directoryPath);

            try
            {
                foreach (var file in localfiles)
                {
                    FileInfo fi = new FileInfo(file);
                    lastPartOfCurrentDirectoryNamebyFile = fi.Directory.Name;
                    ret = CheckFolderExist(despath + '/' + lastPartOfCurrentDirectoryNamebyFile, username, password);
                    if (ret != EventCodeEnum.NONE)
                    {
                        ret = CreateDicrectory(despath + '/' + lastPartOfCurrentDirectoryNamebyFile, username, password);
                        if (ret != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"Fail CrateDirectory: {despath + '/' + lastPartOfCurrentDirectoryNamebyFile}");
                        }
                    }
                    File.Copy(fi.FullName, despath + '/' + lastPartOfCurrentDirectoryNamebyFile +
                        '/' + Path.GetFileName(file), true);

                    LoggerManager.Debug($"Upload File: {Path.GetFileName(file)}");
                }

                EventCodeEnum bRecursiveRet = EventCodeEnum.NONE;
                foreach (string subDir in subDirectories)
                {
                    DirectoryInfo di = new DirectoryInfo(subDir);
                    lastPartOfCurrentDirectoryNamebyFolder = di.Name;
                    string path = despath + '/' + lastPartOfCurrentDirectoryNamebyFile + '/'
                        + lastPartOfCurrentDirectoryNamebyFolder;
                    ret = CheckFolderExist(path, username, password);
                    if (ret != EventCodeEnum.NONE)
                    {
                        ret = CreateDicrectory(path, username, password);
                        if (ret != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"Fail CrateDirectory: {despath + '/' }");
                        }
                    }
                    string subpath = directoryPath + '/' + lastPartOfCurrentDirectoryNamebyFolder;
                    bRecursiveRet = recursiveUploadDirectory_NetWork(subpath, despath + '/' + lastPartOfCurrentDirectoryNamebyFile + '/', username, password);
                    if (bRecursiveRet != EventCodeEnum.NONE)
                        break;

                }
                ret = bRecursiveRet;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"recursiveUploadDirectory_NetWork(): Error occurred. Err = {err.Message}");
                LoggerManager.Error($"Device Upload Fail in recursiveDirectory");
            }
            return ret;
        }

        public EventCodeEnum GetFolderListFromServer(string path, string username, string password, ref List<string> folderlist)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                if (path?.Length > 0)
                {
                    if (Regex.IsMatch(path, ftpPattern))
                    {
                        string tempPath = path.TrimEnd('/');
                        string connectPath = tempPath + "/";

                        try
                        {
                            ret = CheckFolderExist(connectPath, username, password);
                            if (ret != EventCodeEnum.NONE)
                            {
                                ret = CreateDicrectory(connectPath, username, password);
                            }

                            if (ret == EventCodeEnum.NONE)
                            {
                                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(connectPath);
                                request.Method = WebRequestMethods.Ftp.ListDirectory;
                                request.UsePassive = LoaderLogModule.LoaderLogParam.FTPUsePassive.Value;
                                request.Credentials = new NetworkCredential(username, password);
                                request.KeepAlive = false;
                                request.Timeout = 30000;
                                request.ReadWriteTimeout = 30000;
                                List<string> lines = new List<string>();
                                FtpWebResponse listResponse = null;
                                Stream listStream = null;
                                StreamReader listReader = null;
                                using (listResponse = (FtpWebResponse)request.GetResponse())
                                {
                                    using (listStream = listResponse.GetResponseStream())
                                    {
                                        using (listReader = new StreamReader(listStream))
                                        {
                                            while (!listReader.EndOfStream)
                                            {
                                                lines.Add(listReader.ReadLine());
                                            }
                                            listReader.Close();
                                        }
                                        listStream.Close();
                                    }
                                    listResponse.Close();
                                }

                                foreach (string line in lines)
                                {
                                    Match m = GetMatchingRegex(line);
                                    if (m == null)
                                    {
                                        throw new Exception("Unable to parse line: " + line);
                                    }

                                    string name = m.Groups["name"].Value;
                                    folderlist.Add(name);
                                }

                                ret = EventCodeEnum.NONE;
                            }
                            else
                            {
                                ret = EventCodeEnum.LOGUPLOAD_FOLDER_NOT_EXIST;
                            }
                        }
                        catch (WebException ex)
                        {
                            FtpWebResponse response = (FtpWebResponse)ex.Response;
                            if (response != null)
                            {
                                LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                $"response statuscode:{response.StatusCode} " +
                                $"response description:{response.StatusDescription}" +
                                $"connectPath{connectPath}");
                            }
                            else
                            {
                                LoggerManager.Error($"Error occured while {MethodBase.GetCurrentMethod().Name} " +
                                            $"response statuscode:unknown msg={ex.Message}" +
                                            $"connectPath{connectPath}");
                            }
                            ret = EventCodeEnum.LOGUPLOAD_DEVICE_UPLOAD_FAIL;
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"GetFolderListFromServer(): Error occurred. Err = {err.Message}");
                            LoggerManager.Error($"Error Occured while GetLoaderDatesFromServer() Path:{connectPath}");
                            ret = EventCodeEnum.LOGUPLOAD_CONNECT_FAIL;
                        }
                    }
                    else if (Regex.IsMatch(path, networkPattern))
                    {
                        NetworkCredential credentials = null;
                        credentials = new NetworkCredential(username, password);
                        using (new NetworkConnection(path, credentials))
                        {
                            var serverpath = path;
                            if (Directory.Exists(serverpath))
                            {
                                DirectoryInfo dirinfo = new DirectoryInfo(serverpath);
                                var dirs = dirinfo.GetDirectories();
                                foreach (var dir in dirs)
                                {
                                    folderlist.Add(dir.Name);
                                }
                            }
                        }
                        ret = EventCodeEnum.NONE;
                    }
                    else if (Regex.IsMatch(path, localPattern))
                    {
                        ret = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Debug($"LoaderLogSplitManager.GetFolderListFromServer() ServerPath dose not contain an Path Pattern {path}");
                        // Not support protocol
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetFolderListFromServer(): Error occurred. Err = {err.Message}");
                LoggerManager.Exception(err);
            }
            return ret;
        }
    }
}
