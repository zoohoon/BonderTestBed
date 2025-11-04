using System;

namespace SerializerUtil
{
    public abstract class SerializerBase
    {
        public abstract bool Serialize(string path, object serializeObj, Type serializeObjType = null, Type[] serializeTypes = null);
        public abstract bool Deserialize(string path, out object deserializeObj, Type deserializeObjType = null, Type[] serializeTypes = null);
        public abstract byte[] SerializeToByte(object serializeObj, Type deserializeObjType = null, Type[] serializeTypes = null);
        public abstract bool DeserializeFromByte(byte[] vs, out object deserializeObj, Type deserializeObjType = null, Type[] serializeTypes = null);
    }
}
