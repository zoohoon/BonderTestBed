using System;
using System.IO;  // 파일 입출력 스트림
using LogModule;
using ProberErrorCode;
using ProberInterfaces.ResultMap;
using FtpUtil;
using System.IO.Compression;

namespace MapFileManagerModule
{
    public class FTPFileManager : MapFileManagerBase
    {
        //public string mFTPPath;
        //public static readonly string FTP_INFO_FILE_PATH = "이 곳에 경로가 들어가겠지?";
        public FTPController _fTPController;


        public FTPFileManager(FileTransferInfoBase ftpinfo)
        {
            this._fileManagerType = FileManagerType.FTP;
            this.ftpinfo = ftpinfo as FTPTransferInfo;

            _fTPController = new FTPController(
                          this.ftpinfo.UserName.Value,
                          this.ftpinfo.UserPassword.Value,
                          this.ftpinfo.IP.ToString(),
                          this.ftpinfo.Port.Value);

        }

        private FTPTransferInfo ftpinfo { get; set; }

        private new Namer Namer { get; set; }

        public override EventCodeEnum Upload(string sourcePath, bool overwrite = false)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            if (Namer != null)
            {
                string filename = string.Empty;

                retval = Namer.Run(out filename);

                if (retval == EventCodeEnum.NONE && string.IsNullOrEmpty(filename) == false)
                {
                    string FullPath = Path.Combine(sourcePath, filename);
                    string uploadZipFullPath = FullPath + ".zip";
                    EnumFtpResult ftpresult;

                    _fTPController = new FTPController(
                                 this.ftpinfo.UserName.Value,
                                 this.ftpinfo.UserPassword.Value,
                                 this.ftpinfo.IP.ToString(),
                                 this.ftpinfo.Port.Value);

                    retval = Namer.Run(out filename);
                    if (retval == EventCodeEnum.NONE && string.IsNullOrEmpty(filename) == false)
                    {
                        if (Directory.Exists(sourcePath) == true)
                        {
                            if (File.Exists(uploadZipFullPath))
                            {
                                File.Delete(uploadZipFullPath);
                            }

                            ZipFile.CreateFromDirectory(sourcePath, uploadZipFullPath);

                            ftpresult = _fTPController.Upload(sourcePath, filename, overwrite);
                            if (ftpresult != EnumFtpResult.NONE)
                            {
                                retval = EventCodeEnum.EVENT_ERROR;
                            }
                            else
                            {
                                File.Delete(uploadZipFullPath);
                                retval = EventCodeEnum.NONE;
                            }

                        }
                    }
                }
            }
            

            return retval;
        }

        public override EventCodeEnum Upload(string sourcePath, string destPath, bool overwirte = false)
        {
            throw new NotImplementedException();
        }

        public override EventCodeEnum Download(string destPath, bool overwrite = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            EnumFtpResult ftpresult;
            string filename = string.Empty;
            retval = Namer.Run(out filename);
            filename = Path.GetFileNameWithoutExtension(filename);
            if (retval == EventCodeEnum.NONE && string.IsNullOrEmpty(filename) == false)
            {
                String downloadFileFullPath = Path.Combine(destPath, filename);
                if (BuildDirectory(downloadFileFullPath) == false)
                {
                    return EventCodeEnum.DIRECTORY_PATH_ERROR;
                }

                ftpresult = _fTPController.Download(destPath, filename, overwrite);
                if (ftpresult != EnumFtpResult.NONE)
                {
                    retval = EventCodeEnum.EVENT_ERROR;
                }
            }
            return retval;

        }

        public override EventCodeEnum Download(string sourcePath, string destPath, bool overwrite = true)
        {
            throw new NotImplementedException();
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

    }
}
