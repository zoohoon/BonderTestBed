using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Encryption;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace SerializerUtil.SerializeControls
{
    public class JsonSerializeControl : SerializerBase
    {
        private JsonSerializer serializerForByte;
        private JsonSerializer serializer;
        //private JsonSerializerSettings settings;
        EncryptionFactory factory = new EncryptionFactory();

        public JsonSerializeControl()
        {

            serializer = new JsonSerializer();
            serializer.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
            //serializer.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            // TODO : 
            //serializer.ObjectCreationHandling = ObjectCreationHandling.Replace;

            serializerForByte = new JsonSerializer();
            serializerForByte.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
            //serializer.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            serializerForByte.Formatting = Newtonsoft.Json.Formatting.Indented;
            serializerForByte.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializerForByte.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            serializerForByte.ObjectCreationHandling = ObjectCreationHandling.Replace;
            //serializerForByte.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override bool Deserialize(string path, out object deserializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            bool retVal = false;

            deserializeObj = null;

            try
            {
                StreamReader file = File.OpenText(path);

                try
                {
                    deserializeObj = serializer.Deserialize(file, deserializeObjType);

                    if (deserializeObj == null)
                    {
                        retVal = false;
                        deserializeObj = null;
                        throw new JsonSerializationException($"[{deserializeObjType}] LoadSysParam(): DeSerialize Error");
                    }
                    else
                    {
                        retVal = true;
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Debug.WriteLine(err);
                    retVal = false;
                }
                finally
                {
                    file?.Dispose();
                }

                if (retVal == false)
                    throw new Exception();
            }
            catch (Exception err)
            {
                retVal = false;
                throw err;
            }

            retVal = true;
            return retVal;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override bool Serialize(string path, object serializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            bool retVal = false;

            try
            {
                // serialize JSON directly to a file
                StreamWriter file = File.CreateText(path);

                try
                {
                    serializer.Serialize(file, serializeObj);
                }
                catch (Exception err)
                {
                    System.Diagnostics.Debug.WriteLine(err);
                    throw;
                }
                finally
                {
                    file?.Dispose();
                }
                retVal = true;
            }
            catch (JsonSerializationException err)
            {
                retVal = false;
                throw err;
            }
            catch (JsonException err)
            {
                retVal = false;
                throw err;
            }
            catch (Exception err)
            {
                retVal = false;
                throw err;
            }

            return retVal;
        }

        public override byte[] SerializeToByte(object serializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            byte[] vs = null;

            try
            {
                // serialize JSON directly to a file
                MemoryStream stream = new MemoryStream();
                BsonDataWriter writer = new BsonDataWriter(stream);
                try
                {
                    var jsonSerializerSettings = new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(serializeObj, jsonSerializerSettings));

                    serializerForByte.Serialize(writer, serializeObj);

                    stream.Flush();
                    vs = stream.ToArray();
                }
                catch (Exception err)
                {
                    System.Diagnostics.Debug.WriteLine(err);
                    throw;
                }
                finally
                {
                    writer?.Close();
                    stream?.Dispose();
                }
            }
            catch (JsonSerializationException err)
            {
                throw err;
            }
            catch (JsonException err)
            {
                throw err;
            }
            catch (Exception err)
            {
                throw err;
            }
            return vs;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override bool DeserializeFromByte(byte[] vs, out object deserializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            bool retVal = false;

            deserializeObj = null;

            try
            {
                var stream = new MemoryStream(vs);
                BsonDataReader reader = new BsonDataReader(stream);

                try
                {
                    object instance = null;

                    if (deserializeObjType != null)
                    {
                        instance = Activator.CreateInstance(deserializeObjType);
                    }

                    instance = serializerForByte.Deserialize(reader, deserializeObjType);

                    deserializeObj = instance;

                    if (deserializeObj == null)
                    {
                        retVal = false;
                        deserializeObj = null;
                        throw new JsonSerializationException($"[{deserializeObjType}] DeserializeFromByte(): DeSerialize Error");
                    }
                    else
                    {
                        retVal = true;
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Debug.WriteLine(err);
                    retVal = false;
                }
                finally
                {
                    reader?.Close();
                    stream?.Dispose();
                }
            }
            catch (Exception err)
            {
                retVal = false;
                throw err;
            }

            return retVal;
        }
    }
}
