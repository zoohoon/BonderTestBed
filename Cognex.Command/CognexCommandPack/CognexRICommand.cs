using System;

namespace Cognex.Command.CognexCommandPack
{
    using LogModule;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using System.Xml;
    public class CognexRICommand
    {
        private String _Command = null;
        public String Command
        {
            get { return "RI " + _Command; }
        }

        public String Status { get; set; } = String.Empty;
        public String Size { get; set; } = String.Empty;
        public String Data { get; set; } = String.Empty;
        public String CRC { get; set; } = String.Empty;
        public byte[] ByteData { get; set; } = null;
        public bool ParseResponse(String response)
        {
            if (String.IsNullOrEmpty(response))
                return false;
            bool result = false;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);

                using (XmlNodeReader xmlReader = new XmlNodeReader(xmlDoc))
                {
                    Status = GetNextNodeValue(xmlReader, nameof(Status));

                    xmlReader.ReadToFollowing(nameof(Data));
                    Size = xmlReader.GetAttribute(nameof(Size));
                    xmlReader.Read();
                    Data = xmlReader.Value;
                    Data = Data.Trim(new char[] { ' ', '\r', '\n' });
                    Data = Data.Replace("\r\n", String.Empty);

                    CRC = GetNextNodeValue(xmlReader, nameof(CRC));
                }


                do
                {
                    if (Status == "1")
                    {
                        ByteData = ToByteArray(Data);
                        result = true;
                        break;
                    }
                    if (Status == "0")//==> Unrecognized command.
                        break;
                    if (Status == "-2")//==> The command could not be executed.
                        break;
                    if (Status == "6")//==>User does not have Full Access to execute the command.
                        break;
                } while (false);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return result;
        }
        protected String GetNextNodeValue(XmlNodeReader xmlReader, String nodeName)
        {
            try
            {
                xmlReader.ReadToFollowing(nodeName);
                xmlReader.Read();
                return xmlReader.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        protected String GetNextNodeAttribute(XmlNodeReader xmlReader, String nodeName, String attribute)
        {
            try
            {
                xmlReader.ReadToFollowing(nodeName);
                return xmlReader.GetAttribute(attribute);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private byte[] ToByteArray(String hex)
        {
            byte[] bytes = new byte[hex.Length / 2];
            try
            {
                for (int i = 0; i < hex.Length / 2; i++)
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return bytes;
        }
        public Size GetImageSize()
        {
            try
            {
                BitmapImage bitmapImage = GetBitmapImage();
                return new Size(bitmapImage.Width, bitmapImage.Height);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public BitmapImage GetBitmapImage()
        {
            if (ByteData?.Length < 1)
                return null;

            BitmapImage image = new BitmapImage();
            try
            {
                using (var ms = new System.IO.MemoryStream(ByteData))
                {
                    image.BeginInit();
                    image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.StreamSource = null;
                    image.Freeze();
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
