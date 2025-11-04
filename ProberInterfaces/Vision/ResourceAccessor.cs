using LogModule;
using System;


namespace ProberInterfaces
{
    using System.IO;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public static class ResourceAccessor
    {
        public static ImageSource Get(System.Drawing.Bitmap bitmap)
        {
            BitmapImage image = new BitmapImage();
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    (bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    image.BeginInit();
                    ms.Seek(0, SeekOrigin.Begin);
                    image.StreamSource = ms;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.StreamSource = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return image;
        }
    }
}
