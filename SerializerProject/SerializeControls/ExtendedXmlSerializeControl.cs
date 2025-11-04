using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace SerializerUtil.SerializeControls
{
    public class ExtendedXmlSerializeControl : SerializerBase
    {
        public override bool Serialize(string path, object serializeObj, Type deserializeObjType = null, Type[] serializeTypes = null)
        {
            bool retVal = false;

            try
            {
                IExtendedXmlSerializer serializer;

                if (serializeTypes == null)
                {
                    serializer = new ConfigurationContainer().Create();
                }
                else
                {
                    serializer = new ConfigurationContainer().EnableImplicitTyping(serializeTypes).Create();
                }

                using (var mutex = new Mutex(false, "SHARED_BY_ALL_PROCESSES"))
                {
                    mutex.WaitOne();

                    var contents = serializer.Serialize(new XmlWriterSettings { Indent = true }, serializeObj);

                    File.WriteAllText(path, contents);

                    mutex.ReleaseMutex();
                }

                retVal = true;
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

            try
            {
                IExtendedXmlSerializer serializer;

                if (serializeTypes == null)
                {
                    serializer = new ConfigurationContainer().IncludeConfiguredMembers().Create();
                }
                else
                {
                    serializer = new ConfigurationContainer().EnableImplicitTyping(serializeTypes).Create();
                }                

                using (var mutex = new Mutex(false, "SHARED_BY_ALL_PROCESSES"))
                {
                    mutex.WaitOne();

                    using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            file.CopyToAsync(stream).Wait();

                            stream.Seek(0, SeekOrigin.Begin);
                            var contents = new StreamReader(stream).ReadToEnd();
                            using (var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
                            {
                                using (var reader = XmlReader.Create(contentStream))
                                {
                                    deserializeObj = (object)serializer.Deserialize(reader);
                                }
                            }                            
                        }                            
                    }

                    mutex.ReleaseMutex();
                }

                retVal = true;
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
}
