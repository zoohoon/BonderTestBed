using ProberErrorCode;
using ProberInterfaces.ResultMap;

namespace MapFileManagerModule
{

    public abstract class MapFileManagerBase
    {
        public FileManagerType _fileManagerType { get; set; }
        public Namer Namer { get; set; }
        public abstract EventCodeEnum Upload(string sourcePath, bool overwrite = false);
        public abstract EventCodeEnum Upload(string sourcePath, string destPath, bool overwirte = false);

        public abstract EventCodeEnum Download(string destPath, bool overwrite = true);
        public abstract EventCodeEnum Download(string sourcePath, string destPath, bool overwrite = true);

        //public abstract EventCodeEnum Download(string filepath, ref object _param, Type deserializeObjType = null);
        //public abstract EventCodeEnum Download(ref object _param, Type deserializeObjType = null);
        //public abstract EventCodeEnum Upload(string _filepath, object _param, Type serializeObjType = null, bool IsOverwrite = false);
        //public abstract EventCodeEnum Upload(object _param, Type serializeObjType = null, bool IsOverwrite = false);
    }
}
