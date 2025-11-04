using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SerializerUtil.SerializeControls
{
    public class BinSerializeControl : SerializerBase
    {
        public override bool Serialize(string path, object serializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            bool retVal = false;

            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream stream = new MemoryStream())
                {
                    formatter.Serialize(stream, serializeObj);

                    using (var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        stream.WriteTo(file);
                    }

                    using (var file = new FileStream(path, FileMode.Open))
                    {
                        file.CopyToAsync(stream).Wait();
                    }
                    stream.Seek(0, SeekOrigin.Begin);

                    retVal = true;
                }                    
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }

        public override bool Deserialize(string path, out object deserializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            bool retVal = false;

            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                deserializeObj = null;
                using (MemoryStream stream = new MemoryStream())
                {
                    using (var file = new FileStream(path, FileMode.Open))
                    {
                        file.CopyToAsync(stream).Wait();
                    }

                    stream.Seek(0, SeekOrigin.Begin);
                    deserializeObj = formatter.Deserialize(stream);

                    retVal = true;
                }                
            }
            catch (Exception err)
            {
                throw err;
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
}
