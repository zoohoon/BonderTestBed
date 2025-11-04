using LogModule;
using MapFileHandlerModule;
using NamerMudule;
using ProberErrorCode;
using ProberInterfaces;
using ResultMapParamObject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProberInterfaces.ResultMap;

namespace MapUpDownloadModule
{
    using FtpUtil;
    /// <summary>
    /// FileBase..
    /// </summary>
    public class ResultMapUpDownloader : IFactoryModule
    {
        // TODO : Upload 및 Download 기능
        // Spooling 기능 필요

        //private MapHeaderObject HeaderObj { get; set; }
        public ResultMapNamer MapNamer { get; set; }

        public List<MapFileHandler> UploadFileHandlers { get; set; }
        public MapFileHandler DownloadFileHandler { get; set; }

        public IParam MapFileManagingIParameter { get; private set; }

        //public FileTransferComponent UpoadParam { get; set; }
        //public FileTransferComponent DownloadParam { get; set; }

        public ResultMapManagerSysParameter _resultMapManagerSysParam
        {
            get => this.ResultMapManager()?.ResultMapManagerSysIParam as ResultMapManagerSysParameter;
        }


        private List<FileHandlerComponent> GetUploadHandlerInfo()
        {
            List<FileHandlerComponent> retval = new List<FileHandlerComponent>();

            try
            {
                FileHandlerComponent tmp = null;

                if (_resultMapManagerSysParam.UploadHDDEnable.Value == true)
                {
                    tmp = new FileHandlerComponent(FileManagerType.HDD);

                    retval.Add(tmp);
                }

                if (_resultMapManagerSysParam.UploadFTPEnable.Value == true)
                {
                    tmp = new FileHandlerComponent(FileManagerType.FTP);

                    retval.Add(tmp);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private FileHandlerComponent GetDownloadHandlerInfo()
        {
            FileHandlerComponent retval = null;

            try
            {
                switch (_resultMapManagerSysParam.DownloadType.Value)
                {
                    case FileManagerType.UNDEFINED:
                        break;
                    case FileManagerType.HDD:
                        retval = new FileHandlerComponent(FileManagerType.HDD);
                        break;
                    case FileManagerType.FTP:
                        retval = new FileHandlerComponent(FileManagerType.FTP);
                        break;
                    case FileManagerType.FDD:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum CreateFileHandlers()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // Upload handlers
                if (UploadFileHandlers == null)
                {
                    UploadFileHandlers = new List<MapFileHandler>();
                }
                UploadFileHandlers.Clear();

                var uploadHandlerInfo = GetUploadHandlerInfo();

                foreach (var info in uploadHandlerInfo)
                {
                    var hanlder = CreateFileHandler(info.FileManagerType);

                    if (hanlder != null)
                    {
                        UploadFileHandlers.Add(hanlder);
                    }
                }

                // Download handler
                var downloadHandlerInfo = GetDownloadHandlerInfo();

                if (downloadHandlerInfo != null)
                {
                    var hanlder = CreateFileHandler(downloadHandlerInfo.FileManagerType);

                    if (hanlder != null)
                    {
                        DownloadFileHandler = hanlder;
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private MapFileHandler CreateFileHandler(FileManagerType fileManagerType)
        {
            MapFileHandler retval = null;

            try
            {
                if (fileManagerType == FileManagerType.HDD)
                {
                    retval = new MapFileHandler(fileManagerType, _resultMapManagerSysParam.HDDInfo);
                }
                else if (fileManagerType == FileManagerType.FTP)
                {
                    retval = new MapFileHandler(fileManagerType, _resultMapManagerSysParam.FTPInfo);
                }
                else
                {
                    retval = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                // MAKE NAMER
                MapNamer = new ResultMapNamer();
                retval = MapNamer.LoadSysParameter();
                retval = CreateFileHandlers();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetHeaderData(MapHeaderObject _headerinfo)
        {
            try
            {
                this.MapNamer.SetHeaderData(_headerinfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum Upload(Namer namer)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if (namer != null)
                {
                    foreach (var handler in UploadFileHandlers)
                    {
                        handler.setNamer(namer);
                        retval = handler.Upload(this.ResultMapManager().LocalUploadPath);
                    }
                }
                else
                {
                    LoggerManager.Error($"[ResultMapUpDownloader], Upload() : Namer is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum ManualUpload(string sourcePath, string destPath, FileManagerType managertype)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                var handler = UploadFileHandlers.FirstOrDefault(x => x._fileManager._fileManagerType == managertype);

                if (handler != null)
                {
                    retval = handler.ManualUpload(sourcePath, destPath, true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum Download(string namerkey, bool IsLotNameChangedTriggered)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Namer namer = MapNamer.GetNamer(namerkey);

                if (namer != null)
                {
                    namer.IsLotNameChangedTriggered = IsLotNameChangedTriggered;

                    if (DownloadFileHandler != null)
                    {
                        DownloadFileHandler.setNamer(namer);
                        retval = DownloadFileHandler.Download(this.ResultMapManager().LocalDownloadPath);
                    }
                    else
                    {
                        LoggerManager.Error($"[ResultMapUpDownloader], Download() : DownloadFileHandler is null.");
                    }
                }
                else
                {
                    LoggerManager.Error($"[ResultMapUpDownloader], Download() : Namer is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool SetResultMapByFileName(byte[] device, string resultmapname)
        {
            bool retVal = false;

            try
            {
                string fullpath = Path.Combine(this.ResultMapManager().LocalDownloadPath, Path.GetFileNameWithoutExtension(resultmapname));
                string zippath = Path.Combine(fullpath , resultmapname);
                if (!Directory.Exists(fullpath))
                {
                    Directory.CreateDirectory(fullpath);
                }
                if (File.Exists(zippath))
                {
                    File.Delete(zippath);
                }
                File.WriteAllBytes(zippath, device);

                retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        
        //public EventCodeEnum ManualUpload(object param, FileManagerType managertype, string filepath, FileReaderType readertype, Type serializeObjtype = null)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        var mFileHandler = CreateFileHandler(managertype, readertype);

        //        // TEST CODE
        //        if (MapNamer != null)
        //        {
        //            string newfilePath = string.Empty;

        //            retval = MapNamer.GetNamer("STIF").Run(out newfilePath);

        //            if (retval == EventCodeEnum.NONE)
        //            {
        //                filepath = Path.Combine(filepath, newfilePath);
        //            }
        //        }

        //        retval = mFileHandler.Save(param, filepath, serializeObjtype);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        //public EventCodeEnum ManualDownload(FileManagerType managertype, string filepath, FileReaderType readertype, ref object param, Type deserializeObjtype = null)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        var mFileHandler = CreateFileHandler(managertype, readertype);

        //        retval = mFileHandler.Load(filepath, ref param, deserializeObjtype);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}
        public string[] GetNamerAliaslist()
        {
            string[] retval = null;

            try
            {
                retval = MapNamer.GetNamerAliaslist();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = MapNamer.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class FileHandlerComponent
    {
        public FileManagerType FileManagerType { get; set; }

        public FileHandlerComponent(FileManagerType fileManagerType)
        {
            try
            {
                this.FileManagerType = fileManagerType;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
