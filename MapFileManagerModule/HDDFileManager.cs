using System;
using System.IO;  // 파일 입출력 스트림
using LogModule;
using ProberErrorCode;
using ProberInterfaces.ResultMap;

namespace MapFileManagerModule
{
    public class HDDFileManager : MapFileManagerBase
    {
        public HDDFileManager(FileTransferInfoBase transferInfoBase)
        {
            this._fileManagerType = FileManagerType.HDD;
            this.transferInfo = transferInfoBase as HDDTransferInfo;
        }

        public HDDTransferInfo transferInfo { get; set; }

        public override EventCodeEnum Upload(string sourcePath, bool overwrite = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Namer != null)
                {
                    string filename = string.Empty;

                    retval = Namer.Run(out filename);

                    if (retval == EventCodeEnum.NONE && string.IsNullOrEmpty(filename) == false)
                    {
                        bool isExist = false;
                        object obj = null;

                        isExist = Namer.ProberMapDictionary.TryGetValue(EnumProberMapProperty.LOTID, out obj);

                        if (isExist == true && obj != null)
                        {
                            string sourcehFullpath = Path.Combine(sourcePath, obj.ToString(), filename);
                            string destPath = Path.Combine(transferInfo.UploadPath.Value, obj.ToString(), filename);

                            retval = Upload(sourcehFullpath, destPath, overwrite);
                        }
                        else
                        {
                            // TODO : ERROR

                            LoggerManager.Error($"[HDDFileManager], Upload() : Can not use LOTID.");
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[HDDFileManager], Upload() : Namer.Run() failed. retval = {retval}, filename = {filename}");
                    }
                }
                else
                {
                    LoggerManager.Error($"[HDDFileManager], Upload() : Namer is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum Upload(string sourcePath, string destPath, bool overwirte = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (string.IsNullOrEmpty(sourcePath) == false)
                {
                    bool sourceExist = false;

                    sourceExist = File.Exists(sourcePath);

                    if (sourceExist == true)
                    {
                        DirectoryInfo targetDir = new DirectoryInfo(Path.GetDirectoryName(destPath));

                        if (targetDir.Exists == false)
                        {
                            targetDir.Create();
                        }

                        File.Copy(sourcePath, destPath, overwirte);

                        LoggerManager.Debug($"[HDDFileManager], Upload() : Copy success. Source => {sourcePath}, Dest => {destPath}");

                        retval = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Error($"[HDDFileManager], Upload() : source file not exist.");
                    }
                }
                else
                {
                    LoggerManager.Error($"[HDDFileManager], Upload() : sourcePath is empty.");
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }

            return retval;
        }



        public override EventCodeEnum Download(string destPath, bool overwrite = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Namer != null)
                {
                    if (Namer.IsLotNameChangedTriggered == true)
                    {
                        bool isExist = false;
                        object obj = null;

                        isExist = Namer.ProberMapDictionary.TryGetValue(EnumProberMapProperty.LOTID, out obj);

                        if (isExist == true && obj != null)
                        {
                            string sourcePath = Path.Combine(transferInfo.DownloadPath.Value, obj.ToString());

                            if(Directory.Exists(sourcePath))
                            {
                                string[] files = Directory.GetFiles(sourcePath);

                                // Copy the files and overwrite destination files if they already exist.
                                foreach (string s in files)
                                {
                                    // Use static Path methods to extract only the file name from the path.
                                    var fileName = Path.GetFileName(s);
                                    string destFullPath = Path.Combine(destPath, fileName);

                                    retval = Download(s, destFullPath, overwrite);
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                    else
                    {
                        string filename = string.Empty;
                        retval = Namer.Run(out filename);

                        if (string.IsNullOrEmpty(filename) == false)
                        {
                            bool isExist = false;
                            object obj = null;

                            isExist = Namer.ProberMapDictionary.TryGetValue(EnumProberMapProperty.LOTID, out obj);

                            if (isExist == true && obj != null)
                            {
                                string sourcePath = Path.Combine(transferInfo.DownloadPath.Value, obj.ToString(), filename);
                                string destFullPath = Path.Combine(destPath, obj.ToString(), filename);

                                retval = Download(sourcePath, destFullPath, overwrite);
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 하나의 파일 단위
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="overwirte"></param>
        /// <returns></returns>
        public override EventCodeEnum Download(string sourcePath, string destPath, bool overwirte = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (string.IsNullOrEmpty(sourcePath) == false)
                {
                    bool sourceExist = false;

                    sourceExist = File.Exists(sourcePath);

                    if (sourceExist == true)
                    {
                        DirectoryInfo targetDir = new DirectoryInfo(Path.GetDirectoryName(destPath));

                        if (targetDir.Exists == false)
                        {
                            targetDir.Create();
                        }

                        File.Copy(sourcePath, destPath, overwirte);

                        LoggerManager.Debug($"[HDDFileManager], Download() : Copy success. Source => {sourcePath}, Dest => {destPath}");

                        retval = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Error($"[HDDFileManager], Download() : source file not exist. path = {sourcePath}");
                    }
                }
                else
                {
                    LoggerManager.Error($"[HDDFileManager], Download() : sourcePath is empty.");
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
