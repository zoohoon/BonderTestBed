using System;

namespace LogModule.LoggerController
{
    using LogModule;
    using LogModule.LoggerParam;
    using System.Windows.Media.Imaging;

    public class ImageLoggerController : LoggerController
    {
        private ImageLoggerParam _CognexLoggerParam;
        public ImageLoggerController(ImageLoggerParam cognexLoggerParam) : base(cognexLoggerParam)
        {
            if (cognexLoggerParam == null)
            {
                return;
            }

            _LogFilePrefix = "Image_";
            _LogFileExtension = ".bmp";

            _CognexLoggerParam = cognexLoggerParam;

            UpdateCurrentFileTargetPath();
        }
        public override String BuildLogFileName()
        {
            return _LogFilePrefix + DateTime.Now.ToString("HHmmss") + _LogFileExtension;
        }

        public bool SaveImage(BitmapImage bitmapImage)
        {
            bool saveSuccess = true;

            try
            {
                UpdateCurrentFileTargetPath();
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                using (var fileStream = new System.IO.FileStream(CurFileTargetPath, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                saveSuccess = false;
            }

            return saveSuccess;
        }
    }
}
