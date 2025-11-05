using System;
using System.IO;

namespace SerializerUtil.SerializeControls
{
    public class ProtobufSerializeControl : SerializerBase
    {
        public override bool Serialize(string path, object serializeObj, Type serializeObjType = null, Type[] serializeTypes = null)
        {
            bool retVal = false;
            Stream stream = null;
            try
            {
                stream = new FileStream(path, FileMode.Create);
                ProtoBuf.Serializer.Serialize<object>(stream, serializeObj);
                stream.Dispose();

                retVal = true;
            }
            catch (Exception err)
            {
                stream?.Dispose();
                throw err;
            }

            return retVal;
        }

        public override bool Deserialize(string path, out object deserializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            bool retVal = false;
            Stream stream = null;

            deserializeObj = null;

            try
            {
                stream = new FileStream(path, FileMode.Open);
                deserializeObj = ProtoBuf.Serializer.Deserialize(deserializeObjType, stream);
                stream.Dispose();

                retVal = true;
            }
            catch (Exception err)
            {
                stream?.Close();
                stream?.Dispose();
                throw err;
            }

            return retVal;
        }
        public override byte[] SerializeToByte(object serializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            byte[] retVal = null;
            try
            {
                //내용 무
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public override bool DeserializeFromByte(byte[] vs, out object deserializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            bool retVal = false;
            deserializeObj = null;
            try
            {
                //내용 무
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
    }
}
