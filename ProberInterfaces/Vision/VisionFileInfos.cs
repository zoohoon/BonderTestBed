using LogModule;
using System;

namespace ProberInterfaces.Vision
{
    public class VisionFileInfos
    {

        private EnumProberCam _CamType;

        public EnumProberCam CamType
        {
            get { return _CamType; }
            set { _CamType = value; }
        }


        private string[] _FilePaths = { String.Empty };

        public string[] FilePaths
        {
            get { return _FilePaths; }
            set { _FilePaths = value; }
        }

        public VisionFileInfos()
        {

        }
        public VisionFileInfos(EnumProberCam camtype, string[] filepaths)
        {
            try
            {
            this.CamType = camtype;
            this.FilePaths = filepaths;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

    }
}
