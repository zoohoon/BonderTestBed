using SerializerUtil.SerializeControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SerializerUtil
{
    public static class SerializeManager
    {
        static SerializerBase JsonSerializer = new JsonSerializeControl();
        static SerializerBase XmlSerializer = new XmlSerializeControl();
        static SerializerBase ExtendedXmlSerializer = new ExtendedXmlSerializeControl();
        static SerializerBase BinSerializer = new BinSerializeControl();
        static SerializerBase ProtobufSerializer = new ProtobufSerializeControl();

        private static Dictionary<string, object> LockDict = new Dictionary<string, object>();

        //private static object SerializeLockObject = new object();

        //[MethodImpl(MethodImplOptions.Synchronized)]
        public static bool Serialize(string path, 
                                     object serializeObj, 
                                     Type serializeObjType = null, 
                                     SerializerType serializerType = SerializerType.JSON, 
                                     Type[] serializeTypes = null)
        {
            bool retVal = false;
            try
            {
                SerializerBase serializer = SelectSerializer(serializerType);

                bool isExistsPath = false;
                isExistsPath = Directory.Exists(Path.GetDirectoryName(path));
                if (!isExistsPath)
                {
                    Directory.CreateDirectory(path);
                }

                object lockobj = null;

                bool ExistLockObject = LockDict.TryGetValue(path, out lockobj);

                if (ExistLockObject != true)
                {
                    lockobj = new object();

                    LockDict.Add(path, lockobj);
                }

                lock (lockobj)
                {
                    retVal = serializer.Serialize(path, serializeObj, serializeObjType, serializeTypes);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
                throw;
            }
            return retVal;
        }

        /// <summary>
        /// deserializeObjType : 객체의 타입. JSON Deserialize 할 때 필요함.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="deserializeObj"></param>
        /// <param name="deserializeObjType"></param>
        /// <param name="deserializerType"> 
        /// deserialize 객체의 타입. JSON Deserialize 할 때 필요함.
        /// </param>
        /// <param name="deserializeTypes">
        /// deserializeObjType : 객체의 타입. JSON Deserialize 할 때 필요함.
        /// </param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public static bool Deserialize(string path, out object deserializeObj,
            Type deserializeObjType = null,
            SerializerType deserializerType = SerializerType.JSON,
            Type[] deserializeTypes = null)
        {
            bool retVal = false;

            try
            {
                SerializerBase serializer = SelectSerializer(deserializerType);

                object lockobj = null;

                bool ExistLockObject = LockDict.TryGetValue(path, out lockobj);

                if (ExistLockObject != true)
                {
                    lockobj = new object();

                    LockDict.Add(path, lockobj);
                }

                lock (lockobj)
                {
                    retVal = serializer.Deserialize(path, out deserializeObj, deserializeObjType, deserializeTypes);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err.Message);
                throw;
            }
            return retVal;
        }

        private static SerializerBase SelectSerializer(SerializerType serializerType)
        {
            SerializerBase serializer = JsonSerializer;

            try
            {
                if (serializerType == SerializerType.JSON)
                {
                    serializer = JsonSerializer;
                }
                else if (serializerType == SerializerType.XML)
                {
                    serializer = XmlSerializer;
                }
                else if (serializerType == SerializerType.EXTENDEDXML)
                {
                    serializer = ExtendedXmlSerializer;
                }
                else if (serializerType == SerializerType.BIN)
                {
                    serializer = BinSerializer;
                }
                else if (serializerType == SerializerType.PROTOBUF)
                {
                    serializer = ProtobufSerializer;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
                throw;
            }

            return serializer;
        }

        public static byte[] SerializeToByte(object serializeObj,
            Type serializeObjType = null, Type[] serializeTypes = null)
        {
            byte[] vs = null;
            try
            {
                if (serializeObj != null)
                {
                    vs = JsonSerializer.SerializeToByte(serializeObj, serializeObjType, serializeTypes);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
                throw;
            }
            return vs;
        }

        /// <summary>
        /// deserializeObjType : 객체의 타입. JSON Deserialize 할 때 필요함.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="deserializeObj"></param>
        /// <param name="deserializeObjType"></param>
        /// <param name="deserializerType"> 
        /// deserialize 객체의 타입. JSON Deserialize 할 때 필요함.
        /// </param>
        /// <param name="deserializeTypes">
        /// deserializeObjType : 객체의 타입. JSON Deserialize 할 때 필요함.
        /// </param>
        /// <returns></returns>
        public static bool DeserializeFromByte(byte[] vs, out object deserializeObj,
            Type deserializeObjType = null, Type[] deserializeTypes = null)
        {
            bool retVal = false;

            try
            {
                retVal = JsonSerializer.DeserializeFromByte(vs, out deserializeObj, deserializeObjType, deserializeTypes);

            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }

        public static object ByteToObject(byte[] buffer)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    stream.Position = 0;
                    return binaryFormatter.Deserialize(stream);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return null;
        }
        public static byte[] ObjectToByte(object obj)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(stream, obj);
                    return stream.ToArray();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return null;
        }
    }

    public static class ObjectSerialize
    {
        public static byte[] Serialize(this Object obj)
        {
            byte[] retVal = null;
            try
            {
                if (obj == null)
                {
                    return retVal;
                }

                using (var memoryStream = new MemoryStream())
                {
                    var binaryFormatter = new BinaryFormatter();

                    binaryFormatter.Serialize(memoryStream, obj);

                    var compressed = Compress(memoryStream.ToArray());
                    retVal = compressed;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
            }
            return retVal;
        }

        public static Object DeSerialize(this byte[] arrBytes)
        {
            object retVal = null;
                
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    var binaryFormatter = new BinaryFormatter();
                    var decompressed = Decompress(arrBytes);

                    memoryStream.Write(decompressed, 0, decompressed.Length);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    retVal = binaryFormatter.Deserialize(memoryStream);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
            }

            return retVal;
        }

        private static byte[] Compress(byte[] input)
        {
            byte[] compressesData = null;

            try
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var zip = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        zip.Write(input, 0, input.Length);
                    }

                    compressesData = outputStream.ToArray();
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
            }

            return compressesData;
        }

        private static byte[] Decompress(byte[] input)
        {
            byte[] decompressedData = null;

            try
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var inputStream = new MemoryStream(input))
                    {
                        using (var zip = new GZipStream(inputStream, CompressionMode.Decompress))
                        {
                            zip.CopyTo(outputStream);
                        }
                    }

                    decompressedData = outputStream.ToArray();
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
            }

            return decompressedData;
        }
    }

}
