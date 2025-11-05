using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SerializerUtil.SerializeControls
{
    public class XmlSerializeControl : SerializerBase
    {
        public override bool Serialize(string path, object serializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            bool retVal = false;

            try
            {
                XmlSerializer xser = null;

                if (deserializeObjType == null)
                {
                    retVal = false;
                }
                else
                {
                    xser = new XmlSerializer(deserializeObjType);

                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        xser.Serialize(fs, serializeObj);
                        fs.Close();
                    }

                    retVal = true;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
                throw;
            }
            return retVal;
        }

        public override bool Deserialize(string path, out object deserializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            bool retVal = false;
            deserializeObj = null;

            try
            {
                XmlSerializer xser = null;

                if (deserializeObjType == null)
                {
                    retVal = false;
                }
                else
                {
                    xser = new XmlSerializer(deserializeObjType);

                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        deserializeObj = xser.Deserialize(fs);
                        fs.Close();
                    }

                    retVal = true;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
                throw;
            }
            return retVal;
        }

        public override byte[] SerializeToByte(object serializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            throw new NotImplementedException();
        }
        public override bool DeserializeFromByte(byte[] vs, out object deserializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            throw new NotImplementedException();
        }
    }

    public class XMLSerializer
    {
        public static Byte[] StringToUTF8ByteArray(String xmlString)
        {
            return new UTF8Encoding().GetBytes(xmlString);
        }

        public static String SerializeToXML<T>(T objectToSerialize)
        {
            StringBuilder sb = new StringBuilder();

            XmlWriterSettings settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };

            using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
            {
                if (xmlWriter != null)
                {
                    new XmlSerializer(typeof(T)).Serialize(xmlWriter, objectToSerialize);
                }
            }

            return sb.ToString();
        }

        public static void DeserializeFromXML<T>(string xmlString, out T deserializedObject) where T : class
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));

            using (MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(xmlString)))
            {
                deserializedObject = xs.Deserialize(memoryStream) as T;
            }
        }
    }

}
