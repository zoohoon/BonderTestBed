using System;
using System.Collections.Generic;
using System.Linq;

namespace DeviceUpDownModule
{
    using FtpUtil;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.IO;
    using System.IO.Compression;

    public class DeviceUpDownManager : IDeviceUpDownManager
    {
        public bool Initialized { get; set; } = false;
        public DeviceUpDownSysParameter DeviceUpDownSysParam { get; set; }

        private FTPController _FtpController;

        private String GetDeviceRootDirectory
        {
            get
            {
                return this.FileManager().FileManagerParam.DeviceParamRootDirectory;
            }
        }

        public void ChangeUploadDirectory()
        {
            //==> DeviceUpDownSysParam 의 FTP 접속 정보는 실행중에 바뀔 수 있기에 동작하기 전에 Setting을 한다.
            _FtpController.Setting(
                DeviceUpDownSysParam.FtpUserAccount.Value,
                DeviceUpDownSysParam.FtpUserPassword.Value,
                DeviceUpDownSysParam.FtpServerIP.ToString(),
                DeviceUpDownSysParam.FtpServerPort.Value);

            _FtpController.ChangeHomeDir();
            _FtpController.ChangeDir(DeviceUpDownSysParam.FtpServerUploadPath.Value);
        }
        public void ChangeDownloadDirectory()
        {
            //==> DeviceUpDownSysParam 의 FTP 접속 정보는 실행중에 바뀔 수 있기에 동작하기 전에 Setting을 한다.
            _FtpController.Setting(
                DeviceUpDownSysParam.FtpUserAccount.Value,
                DeviceUpDownSysParam.FtpUserPassword.Value,
                DeviceUpDownSysParam.FtpServerIP.ToString(),
                DeviceUpDownSysParam.FtpServerPort.Value);

            _FtpController.ChangeHomeDir();
            _FtpController.ChangeDir(DeviceUpDownSysParam.FtpServerDownloadPath.Value);
        }
        public List<String> GetLocalDeviceNameList()
        {
            if (Directory.Exists(GetDeviceRootDirectory) == false)
            {
                Directory.CreateDirectory(GetDeviceRootDirectory);
            }

            DirectoryInfo localDirectoryInfo = new DirectoryInfo(GetDeviceRootDirectory);
            DirectoryInfo[] dirArr = localDirectoryInfo.GetDirectories();

            List<String> dirListNameList = dirArr.Select(item => item.Name).ToList();//==> Device 하나도 없을 때 빈 List 반환
            dirListNameList.Remove("WaferMap");
            dirListNameList.Remove("ProbeCard");
            return dirListNameList;
        }
        public EnumDeviceUpDownErr GetServerDeviceNameList(out List<String> fileList)
        {
            //==> DeviceUpDownSysParam 의 FTP 접속 정보는 실행중에 바뀔 수 있기에 동작하기 전에 Setting을 한다.
            _FtpController.Setting(
                DeviceUpDownSysParam.FtpUserAccount.Value,
                DeviceUpDownSysParam.FtpUserPassword.Value,
                DeviceUpDownSysParam.FtpServerIP.ToString(),
                DeviceUpDownSysParam.FtpServerPort.Value);

            fileList = new List<string>();

            List<String> fileNameList;
            if(_FtpController.GetFileNameList(new char[] { '-' }, out fileNameList) != EnumFtpResult.NONE)
            {
                fileNameList = new List<String>();
                return EnumDeviceUpDownErr.NETWORK_ERROR;
            }

            //==> EX) "DeviceParam.zip"
            //==> EX) "SubFolder.zip"
            IEnumerable<String> zipFileList = fileNameList
                .Where(file => file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                .Select(file => Path.GetFileNameWithoutExtension(file))
                .AsEnumerable();

            fileList.AddRange(zipFileList);

            return EnumDeviceUpDownErr.NONE;
        }
        private void DirectoryCopy(string sourceDirName, string destDirName)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirArr = directoryInfo.GetDirectories();
            if (Directory.Exists(destDirName) == false)
            {
                Directory.CreateDirectory(destDirName);
            }

            //==> 파일 복사 수행
            FileInfo[] fileArr = directoryInfo.GetFiles();
            foreach (FileInfo file in fileArr)
            {
                String copyPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(copyPath, true);
            }

            //==> 하위 폴더들 까지 복사 수행.
            foreach (DirectoryInfo subDirInfo in dirArr)
            {
                String copyPath = Path.Combine(destDirName, subDirInfo.Name);
                DirectoryCopy(subDirInfo.FullName, copyPath);
            }
        }
        public bool CheckDeviceExistInLocal(String deviceName)
        {
            String downloadDevFolderFullPath = Path.Combine(GetDeviceRootDirectory, deviceName);
            return Directory.Exists(downloadDevFolderFullPath);
        }
        public EnumDeviceUpDownErr CheckDeviceExistInServer(String deviceName, out bool isExists)
        {
            //==> DeviceUpDownSysParam 의 FTP 접속 정보는 실행중에 바뀔 수 있기에 동작하기 전에 Setting을 한다.
            _FtpController.Setting(
                DeviceUpDownSysParam.FtpUserAccount.Value,
                DeviceUpDownSysParam.FtpUserPassword.Value,
                DeviceUpDownSysParam.FtpServerIP.ToString(),
                DeviceUpDownSysParam.FtpServerPort.Value);

            String downloadDevZipName = deviceName + ".zip";

            EnumFtpResult result = _FtpController.CheckFileExists(downloadDevZipName, out isExists);

            if (result != EnumFtpResult.NONE)
            {
                return EnumDeviceUpDownErr.NETWORK_ERROR;
            }

            return EnumDeviceUpDownErr.NONE; ;
        }
        public void DeleteDeviceInLocal(String deviceName)
        {
            String downloadDevFolderFullPath = Path.Combine(GetDeviceRootDirectory, deviceName);
            Directory.Delete(downloadDevFolderFullPath, true);
        }
        //==> dataTransferEvent는 Progress Bar를 표시하기 위한 콜백 함수 이다. long : 전송한 데이터 크기, long : 파일 크기
        public EnumDeviceUpDownErr DownloadDevice(String deviceName, Func<long, long, bool> dataTransferEvent)
        {
            //==> DeviceUpDownSysParam 의 FTP 접속 정보는 실행중에 바뀔 수 있기에 동작하기 전에 Setting을 한다.
            _FtpController.Setting(
                DeviceUpDownSysParam.FtpUserAccount.Value,
                DeviceUpDownSysParam.FtpUserPassword.Value,
                DeviceUpDownSysParam.FtpServerIP.ToString(),
                DeviceUpDownSysParam.FtpServerPort.Value);

            String downloadDevZipName = deviceName + ".zip";
            String downloadDevZipFullPath = Path.Combine(GetDeviceRootDirectory, downloadDevZipName);
            String downloadDevFolderFullPath = Path.Combine(GetDeviceRootDirectory, deviceName);
            _FtpController.DataTransferEvent = dataTransferEvent;
            try
            {
                bool isExists;
                EnumDeviceUpDownErr existsCheckResult = CheckDeviceExistInServer(deviceName, out isExists);
                if (existsCheckResult != EnumDeviceUpDownErr.NONE)
                {
                    //==> 수행 중 Server에 무었인가 이상 생김
                    LoggerManager.Debug($"{deviceName} : Device Exists in server check is Error, {existsCheckResult}");
                    return existsCheckResult;
                }
                if (isExists == false)
                {
                    LoggerManager.Debug($"{deviceName} : Device is not exist in server");
                    return EnumDeviceUpDownErr.DEVICENOTEXIST_ERROR;
                }


                //==> Device 다운로드
                EnumFtpResult downloadResult = _FtpController.Download(GetDeviceRootDirectory, downloadDevZipName, true);
                if (downloadResult != EnumFtpResult.NONE)
                {
                    LoggerManager.Debug($"{deviceName} : Download fail, {downloadResult}");
                    return EnumDeviceUpDownErr.TRANSFER_ERROR;
                }
                //==> 모든 Device들을 삭제해 버림
                if (DeviceUpDownSysParam.ClearDeviceOption.Value)
                {
                    DirectoryInfo deviceRootDi = new DirectoryInfo(GetDeviceRootDirectory);
                    DirectoryInfo[] dirArr = deviceRootDi.GetDirectories();
                    foreach (DirectoryInfo subDirInfo in dirArr)
                    {
                        Directory.Delete(subDirInfo.FullName, true);
                    }
                }

                //==>  다운로드 한 Device 를 압축 해제
                ZipFile.ExtractToDirectory(
                    downloadDevZipFullPath, //==> source : zip file full path
                    downloadDevFolderFullPath); //==> destination : extract folder path

                //==> 압축 해제 확인
                if (CheckDeviceExistInLocal(deviceName) == false)
                {
                    LoggerManager.Debug($"{deviceName} : Unzip Fail");
                    return EnumDeviceUpDownErr.ZIP_ERROR;
                }

                if (CheckFolderOverlap(downloadDevFolderFullPath))
                {
                    MoveFoloderToParent(downloadDevFolderFullPath);
                }

                //==> 압축 파일은 삭제, 원하는 결과물은 디렉터리일 뿐,,,
                File.Delete(downloadDevZipFullPath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                _FtpController.DataTransferEvent = null;
            }
            String fileName = Path.GetFileName(downloadDevFolderFullPath);

            return EnumDeviceUpDownErr.NONE;
        }
        private bool CheckFolderOverlap(String dirPath)
        {
            String folderName = Path.GetFileName(dirPath);
            String overlapFolderPath = Path.Combine(dirPath, folderName);
            bool isOverlap = Directory.Exists(overlapFolderPath);

            return isOverlap;
        }
        private void MoveFoloderToParent(String dirPath)
        {
            String folderName = Path.GetFileName(dirPath);
            String overlapFolderPath = Path.Combine(dirPath, folderName);

            DirectoryCopy(overlapFolderPath, dirPath);//==> Device를 상위 폴더로 복사한 다음
            Directory.Delete(overlapFolderPath, true);//==> 폴더를 지움
        }
        //==> dataTransferEvent는 Progress Bar를 표시하기 위한 콜백 함수 이다. long : 전송한 데이터 크기, long : 파일 크기
        public EnumDeviceUpDownErr UploadDevice(String deviceName, Func<long, long, bool> dataTransferEvent)
        {
            //==> DeviceUpDownSysParam 의 FTP 접속 정보는 실행중에 바뀔 수 있기에 동작하기 전에 Setting을 한다.
            _FtpController.Setting(
                DeviceUpDownSysParam.FtpUserAccount.Value,
                DeviceUpDownSysParam.FtpUserPassword.Value,
                DeviceUpDownSysParam.FtpServerIP.ToString(),
                DeviceUpDownSysParam.FtpServerPort.Value);

            String uploadFolderFullPath = Path.Combine(GetDeviceRootDirectory, deviceName);
            String uploadDevZipName = deviceName + ".zip";
            String uploadDevZipFullPath = Path.Combine(GetDeviceRootDirectory, uploadDevZipName);
            _FtpController.DataTransferEvent = dataTransferEvent;
            try
            {

                //==> Local Direcotry에 Device 존재하는지 확인
                if (CheckDeviceExistInLocal(deviceName) == false)
                {
                    LoggerManager.Debug($"{deviceName} : Device is not exist in local");
                    return EnumDeviceUpDownErr.DEVICENOTEXIST_ERROR;
                }

                //==> Device 압축 파일이 이미 존재한다면, 이전에 수행했던 업로드의 잔재임으로 지워준다.
                if (File.Exists(uploadDevZipFullPath))
                {
                    File.Delete(uploadDevZipFullPath);
                }

                //==> Device 압축
                ZipFile.CreateFromDirectory(uploadFolderFullPath, uploadDevZipFullPath);

                EnumFtpResult uploadResult = _FtpController.Upload(GetDeviceRootDirectory, uploadDevZipName, true);
                if (uploadResult != EnumFtpResult.NONE)
                {
                    LoggerManager.Debug($"{deviceName} : Upload fail, {uploadResult}");
                    return EnumDeviceUpDownErr.TRANSFER_ERROR;
                }

                //==> 업로드를 다 수행 하였으니 남은 Zip 파일을 삭제 한다.
                File.Delete(uploadDevZipFullPath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                _FtpController.DataTransferEvent = null;
            }

            return EnumDeviceUpDownErr.NONE;
        }
        public bool CheckConnection()
        {
            //==> DeviceUpDownSysParam 의 FTP 접속 정보는 실행중에 바뀔 수 있기에 동작하기 전에 Setting을 한다.
            _FtpController.Setting(
                DeviceUpDownSysParam.FtpUserAccount.Value,
                DeviceUpDownSysParam.FtpUserPassword.Value,
                DeviceUpDownSysParam.FtpServerIP.ToString(),
                DeviceUpDownSysParam.FtpServerPort.Value);

            return _FtpController.CheckConnection(5000);
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (Initialized == false)
            {
                try
                {
                    retval = LoadSysParameter();

                    _FtpController = new FTPController(
                        DeviceUpDownSysParam.FtpUserAccount.Value,
                        DeviceUpDownSysParam.FtpUserPassword.Value,
                        DeviceUpDownSysParam.FtpServerIP.ToString(),
                        DeviceUpDownSysParam.FtpServerPort.Value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

                Initialized = true;
            }
            else
            {
                LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                retval = EventCodeEnum.DUPLICATE_INVOCATION;
            }

            return retval;
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new DeviceUpDownSysParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";

                RetVal = this.LoadParameter(ref tmpParam, typeof(DeviceUpDownSysParameter));

                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error(String.Format("[DeviceUpDown] Save system param: Serialize Error"));
                    return RetVal;
                }
                DeviceUpDownSysParam = tmpParam as DeviceUpDownSysParameter;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = this.SaveParameter(DeviceUpDownSysParam);

                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error(String.Format("[DeviceUpDown] Save system param: Serialize Error"));
                    return RetVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public void DeInitModule()
        {
        }
    }
}
