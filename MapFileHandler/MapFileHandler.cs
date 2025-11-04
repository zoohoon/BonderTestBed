using System;
using MapFileManagerModule;
using LogModule;
using ProberErrorCode;
using ProberInterfaces.ResultMap;

namespace MapFileHandlerModule
{
    public class MapFileHandler
    {
        public MapFileManagerBase _fileManager;

        public MapFileHandler(FileManagerType managerType, FileTransferInfoBase transferInfoBase)
        {
            setFileManagerType(managerType, transferInfoBase);
        }

        public void setFileManagerType(FileManagerType tatgetManagerType, FileTransferInfoBase transferInfoBase)
        {
            try
            {
                switch (tatgetManagerType)
                {
                    case FileManagerType.HDD:
                        _fileManager = new HDDFileManager(transferInfoBase);
                        break;
                    case FileManagerType.FTP:
                        _fileManager = new FTPFileManager(transferInfoBase);
                        break;
                    default:
                        _fileManager = null;
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void setNamer(Namer namer)
        {
            try
            {
                if (_fileManager != null)
                {
                    _fileManager.Namer = namer;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public EventCodeEnum Upload(string sourcePath, bool overwrite = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (_fileManager != null)
                {
                    retval = _fileManager.Upload(sourcePath, overwrite);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum ManualUpload(string sourcePath, string destPath, bool overwrite = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (_fileManager != null)
                {
                    retval = _fileManager.Upload(sourcePath, destPath, overwrite);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum Download(string destPath, bool overwrite = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (_fileManager != null)
                {
                    retval = _fileManager.Download(destPath, overwrite);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum ManualDownload(string sourcePath, string destPath, bool overwrite = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (_fileManager != null)
                {
                    retval = _fileManager.Download(sourcePath, destPath, overwrite);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

    }
}
