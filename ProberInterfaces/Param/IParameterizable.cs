using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using ProberErrorCode;
using System.Reflection;
using System.Xml.Serialization;
using LogModule;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Security;
using ProberInterfaces.NeedleClean;
using System.IO.Compression;
using SerializerUtil;
using System.ServiceModel;
using System.Collections.Concurrent;
using System.IO;

namespace ProberInterfaces
{
    public enum RunMode
    {
        DEFAULT,
        EMUL
    }
    public enum ExecuteMode
    {
        DEFAULT,
        ENGINEER
    }
    public enum ParamType
    {
        COMMON,
        SYS,
        DEV,
    }

    public class ElementArgs : EventArgs
    {
        public IElement elementData;
    }

    public static class Extensions_IParam
    {
        private static bool IsInfo = false;

        public static IWaferObject GetParam_Wafer(this IFactoryModule module)

        {
            return module.StageSupervisor().WaferObject;
        }

        public static IProbeCard GetParam_ProbeCard(this IFactoryModule module)
        {
            return module.StageSupervisor().ProbeCardInfo;
        }
        public static INeedleCleanObject GetParam_NcObject(this IFactoryModule module)
        {
            return module.StageSupervisor().NCObject;
        }


        public static ITouchSensorObject GetParam_TouchSensor(this IFactoryModule module)
        {
            return module.StageSupervisor().TouchSensorObject;
        }

        public enum FileType
        {
            NONE,
            JSON,
            BIN,
            XML,
            EXTENDEDXML
        }

        //public static int ConnectTimerUpdateInterval = 240000;
        public static int ConnectTimerUpdateInterval = 4000;

        public static RunMode ProberRunMode;
        public static ExecuteMode ProberExecuteMode;
        private static IParamManager ParamManager;
        public static bool ProgramShutDown; // 프로그램을 종료시킨게 Shutdown 버튼을 사용한건지 알지위해.
        public static bool LoadProgramFlag { get; set; } = false;
        //public static event EventHandler OnFoundProperty;

        public static IParam Copy(this IParam param)
        {
            IParam retparam = null;

            try
            {
                if (param != null)
                {
                    if (!param.GetType().IsSerializable)
                    {
                        throw new ArgumentException("The type must be serializable.", "source");
                    }

                    // Don't serialize a null object, simply return the default for that object
                    if (Object.ReferenceEquals(param, null))
                    {
                        return default(IParam);
                    }

                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new MemoryStream();

                    try
                    {
                        formatter.Serialize(stream, param);
                        stream.Seek(0, SeekOrigin.Begin);
                        retparam = (IParam)formatter.Deserialize(stream);
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    }
                    finally
                    {
                        stream.Dispose();
                    }

                    if (retparam != null)
                    {
                        retparam.Owner = param.Owner;
                        retparam.Genealogy = param.Genealogy;
                        retparam.Nodes = param.Nodes;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retparam;
        }

        public static T Copy<T>(T param)
        {
            T retVal;
            try
            {
                if (!param.GetType().IsSerializable)
                {
                    throw new ArgumentException("The type must be serializable.", "source");
                }

                // Don't serialize a null object, simply return the default for that object
                if (Object.ReferenceEquals(param, null))
                {
                    return default(T);
                }

                IFormatter formatter = new BinaryFormatter();
                Stream stream = new MemoryStream();
                //using (stream)
                //{
                try
                {
                    formatter.Serialize(stream, param);
                    stream.Seek(0, SeekOrigin.Begin);
                    retVal = (T)formatter.Deserialize(stream);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
                finally
                {
                    stream.Dispose();
                }
                //}

                if (retVal is IParam && param is IParam)
                {
                    ((IParam)retVal).Owner = ((IParam)param)?.Owner;
                    ((IParam)retVal).Genealogy = ((IParam)param)?.Genealogy;
                    ((IParam)retVal).Nodes = ((IParam)param)?.Nodes;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public static object CopyTest(this object objSource)
        {
            Type typeSource = objSource.GetType();
            object objTarget = Activator.CreateInstance(typeSource);
            PropertyInfo[] propertyInfo = typeSource.GetProperties();

            foreach (PropertyInfo property in propertyInfo)
            {
                if (property.CanWrite)
                {
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    {
                        property.SetValue(objTarget, property.GetValue(objSource, null), null);
                    }
                    else
                    {
                        object objPropertyValue = property.GetValue(objSource, null);
                        bool isAssignableFrom = typeof(IParamNode).IsAssignableFrom(property.PropertyType) ||
                            typeof(INode).IsAssignableFrom(property.PropertyType);

                        if (objPropertyValue == null)
                        {
                            property.SetValue(objTarget, null, null);
                        }
                        else if (isAssignableFrom)
                        {
                            property.SetValue(objTarget, objPropertyValue.CopyTest(), null);
                        }
                    }
                }
            }
            return objTarget;
        }

        //public static IParam JCopy(this IParam param)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    IParam retparam = null;

        //    try
        //    {
        //        if (!param.GetType().IsSerializable)
        //        {
        //            throw new ArgumentException("The type must be serializable.", "source");
        //        }

        //        // Don't serialize a null object, simply return the default for that object
        //        if (Object.ReferenceEquals(param, null))
        //        {
        //            return default(IParam);
        //        }

        //        try
        //        {
        //            retparam = SerializeManager.SerializerCopy(param) as IParam;

        //            Test(param, retparam);
        //        }
        //        catch (Exception err)
        //        {
        //            throw;
        //        }
        //        finally
        //        {
        //        }

        //        if (retparam != null)
        //        {
        //            retparam.Owner = param.Owner;
        //            retparam.Genealogy = param.Genealogy;
        //            retparam.Nodes = param.Nodes;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }


        //    return retparam;
        //}

        private static void Test(object source, object dest)
        {
            Type paramType = source.GetType();
            var properties = paramType.GetProperties();

            for (int i = 0; i < (properties?.Length ?? 0); i++)
            {
                PropertyInfo prop = properties[i];
                bool isAssignableFrom = false;
                object sourceTmp = null;
                object destTmp = null;

                sourceTmp = prop.GetValue(source);
                destTmp = prop.GetValue(dest);

                if (sourceTmp == null)
                    continue;

                isAssignableFrom = typeof(IParamNode).IsAssignableFrom(prop.PropertyType);
                if (isAssignableFrom)
                {
                    Test(sourceTmp, destTmp); ;
                }

                isAssignableFrom = (prop.CustomAttributes.ToList().Find(attribute => attribute.AttributeType == typeof(JsonIgnoreAttribute)) != null)
                                    || (sourceTmp != null && destTmp == null);
                if (isAssignableFrom)
                {
                    prop.SetValue(dest, source);
                }
            }
        }

        private static EventCodeEnum JsonDeserialize(Type type, string jsonFullPath, out object deserializeObj)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool deserializeResult = false;
                LoggerManager.Debug($"[S] Deserialize - {type.Name}");
                deserializeResult = SerializeManager.Deserialize(jsonFullPath, out deserializeObj, type);
                LoggerManager.Debug($"[E] Deserialize - {type.Name}");
                retVal = deserializeResult ? EventCodeEnum.NONE : EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                deserializeObj = null;
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err, $"{jsonFullPath} Deserialize failed.");
            }

            return retVal;
        }

        private static EventCodeEnum JsonSerialize(object tempInstance, string jsonFullPath)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool serializeResult = false;
                //LoggerManager.Debug($"Start Serialize - {tempInstance.GetType().Name}");
                serializeResult = SerializeManager.Serialize(jsonFullPath, tempInstance);
                //LoggerManager.Debug($"End Serialize - {tempInstance.GetType().Name}");
                retVal = serializeResult ? EventCodeEnum.NONE : EventCodeEnum.UNDEFINED;

                if (tempInstance is IParam)
                    (tempInstance as IParam).IsParamChanged = false;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private static EventCodeEnum BinSerialize(object tempInstance, string binFullPath)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool serializeResult = false;
                serializeResult = SerializeManager.Serialize(binFullPath, tempInstance, serializerType : SerializerType.BIN);
                retVal = serializeResult ? EventCodeEnum.NONE : EventCodeEnum.UNDEFINED;

                if (tempInstance is IParam)
                    (tempInstance as IParam).IsParamChanged = false;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private static void CheckAndCreateDirectoryExists(string fullPath)
        {
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(fullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private static void GetParamManager(object instance)
        {
            try
            {
                if (ParamManager == null)
                {
                    ParamManager = (instance as IFactoryModule)?.ParamManager();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static string GetFullPath(this IFactoryModule module, object obj)
        {
            string FullPath = null;
            string tmpPath = string.Empty;
            string tmpName = string.Empty;
            IParam param = null;
            try
            {
                if (obj is IParam)
                    param = obj as IParam;
                else
                    return FullPath;


                if (!string.IsNullOrEmpty(param.FileName))
                {
                    tmpPath = param.FilePath;
                    tmpName = param.FileName;
                }
                //else
                //{
                //    object instance = Activator.CreateInstance(param.GetType());
                //    IParam paramForGetName = instance as IParam;

                //    tmpPath = paramForGetName.FilePath;
                //    tmpName = extension + paramForGetName.FileName;
                //}


                if (param is ISystemParameterizable)
                {
                    FullPath = param.FileManager().GetSystemParamFullPath(tmpPath, tmpName);
                }
                else if (param is IDeviceParameterizable)
                {
                    FullPath = param.FileManager().GetDeviceParamFullPath(tmpPath, tmpName);
                }
                else
                {
                    throw new Exception($"SaveParameter() - GetFullPathUsingIParamInstance() : param isn't ISystemParameterizable or IDeviceParameterizable.");
                }

                FullPath = Path.ChangeExtension(FullPath, ".json");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return FullPath;
        }

        public static string GetFullPath(IParam tempInstance, string fixFullPath, string prefix, ref bool isSysParam, Type type)
        {
            string retFullPath = string.Empty;

            try
            {
                if (fixFullPath != null)
                {
                    retFullPath = fixFullPath;

                    if (tempInstance is ISystemParameterizable)
                    {
                        isSysParam = true;
                    }
                    else if (tempInstance is IDeviceParameterizable)
                    {
                        isSysParam = false;
                    }
                }
                else
                {
                    try
                    {
                        prefix = prefix != null ? prefix + '_' : null;
                        retFullPath = GetFullPathUsingIParamInstance(tempInstance, prefix, ref isSysParam);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Error($"[{type}] " + err.Message);
                        throw err;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retFullPath;
        }

        private static string GetFullPathUsingIParamInstance(IParam tempInstance, string prefix, ref bool isSysParam)
        {
            string retFilePath = string.Empty;

            try
            {
                if (tempInstance is ISystemParameterizable)
                {
                    retFilePath = tempInstance.FileManager().GetSystemParamFullPath(tempInstance.FilePath, prefix + tempInstance.FileName);
                    isSysParam = true;
                }
                else if (tempInstance is IDeviceParameterizable)
                {
                    retFilePath = tempInstance.FileManager().GetDeviceParamFullPath(tempInstance.FilePath, prefix + tempInstance.FileName);
                    isSysParam = false;
                }
                else
                {
                    throw new Exception($"LoadSysParam(): param isn't ISystemParameterizable or IDeviceParameterizable.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retFilePath;
        }

        public static object GetOwner(IFactoryModule module, Object owner)
        {
            object retOwner = null;

            try
            {
                if (owner != null)
                {
                    retOwner = owner;
                }
                else if (module != null)
                {
                    retOwner = module;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retOwner;
        }

        private static EventCodeEnum ParamFileMove(string source, string dest)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                File.Move(source, dest);

                //File.Copy(source, dest);
                //File.Delete(source);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public static EventCodeEnum LoadParamForUnitTest(this IFactoryModule module,
                                                  ref IParam param, Type type,
                                                  string prefix = null,
                                                  string fixFullPath = null,
                                                  Object owner = null,
                                                  string extention = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                object deserializedObj = null;
                object instance = Activator.CreateInstance(type);

                IParam tempInstance = instance as IParam;
                tempInstance.Owner = GetOwner(module, owner);

                bool isSysParam = false;

                if (tempInstance != null)
                {
                    string FullPath = GetFullPath(tempInstance, fixFullPath, prefix, ref isSysParam, type);

                    try
                    {
                        CheckAndCreateDirectoryExists(FullPath);

                        try
                        {
                            var JsonFullPath = Path.ChangeExtension(FullPath, ".json");

                            retval = JsonDeserialize(type, JsonFullPath, out deserializedObj);
                            param = deserializedObj as IParam;
                        }
                        catch (JsonSerializationException err)
                        {
                            retval = EventCodeEnum.PARAM_ERROR;
                            LoggerManager.Exception(err);
                        }
                        catch (JsonException err)
                        {
                            retval = EventCodeEnum.PARAM_ERROR;
                            LoggerManager.Exception(err);
                        }
                        catch (Exception err)
                        {
                            retval = EventCodeEnum.PARAM_ERROR;
                            LoggerManager.Exception(err);
                        }

                        retval = param.Init();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        retval = EventCodeEnum.PARAM_ERROR;
                    }
                }

                param.Owner = GetOwner(module, owner);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        static string BackupParamFullpath = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"> 
        ///     Load 후, 반환하고픈 Parameter 
        /// </param>
        /// <param name="type"> 
        ///     Parameter의 Type 
        /// </param>
        /// <param name="prefix"> 
        ///     parameter name에 prefix로 고정 하고픈 string. 
        ///     만약 fixFullpath가 null이 아니라면 효과 없음.
        /// </param>
        /// <param name="fixFullPath"> 
        ///     고정 Full Path.
        /// </param>
        /// <returns></returns>
        public static EventCodeEnum LoadParameter(this IFactoryModule module,
                                                  ref IParam param, Type type,
                                                  string prefix = null,
                                                  string fixFullPath = null,
                                                  Object owner = null,
                                                  string extention = null)
        {
            object deserializedObj = null;
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"[S] [Load Parameter] {type.Name}", isInfo: IsInfo);

                object instance = Activator.CreateInstance(type);

                IParam tempInstance = instance as IParam;
                tempInstance.Owner = GetOwner(module, owner);

                bool isSysParam = false;

                RetVal = SetDefaultParam(tempInstance);

                if (RetVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"{tempInstance}'s SetDefaultParam Method Error");
                }

                GetParamManager(tempInstance);

                if (tempInstance != null)
                {
                    string FullPath = GetFullPath(tempInstance, fixFullPath, prefix, ref isSysParam, type);

                    try
                    {
                        CheckAndCreateDirectoryExists(FullPath);

                        // Convert Logic

                        var XmlFullPath = Path.ChangeExtension(FullPath, ".xml");
                        var BinFullPath = Path.ChangeExtension(FullPath, ".bin");
                        var JsonFullPath = Path.ChangeExtension(FullPath, ".json");

                        bool ExistXml = false;
                        bool ExistBin = false;
                        bool ExistJson = false;

                        bool NeedConvertToJason = false;
                        bool ErrorOccurredFlag = false;
                        bool NeedBackupParam = false;

                        int ExistFileCount = 0;

                        ExistXml = File.Exists(XmlFullPath);
                        ExistBin = File.Exists(BinFullPath);
                        ExistJson = File.Exists(JsonFullPath);

                        if (ExistXml == true)
                        {
                            ExistFileCount++;
                        }

                        if (ExistBin == true)
                        {
                            ExistFileCount++;
                        }

                        if (ExistJson == true)
                        {
                            ExistFileCount++;
                        }

                        if (ExistFileCount == 0)
                        {
                            NeedConvertToJason = false;
                        }
                        else if (ExistFileCount == 1)
                        {
                            if (ExistXml)
                            {
                                NeedConvertToJason = true;
                            }
                            else if (ExistBin)
                            {
                                NeedConvertToJason = true;
                            }
                            else if (ExistJson)
                            {
                                NeedConvertToJason = false;
                            }
                            else
                            {
                                NeedConvertToJason = false;
                                ErrorOccurredFlag = true;
                                LoggerManager.Error("Unknown.");
                            }
                        }
                        else if (ExistFileCount == 2)
                        {
                            if ((ExistXml && ExistBin))
                            {
                                NeedConvertToJason = false;
                                ErrorOccurredFlag = true;

                                throw new Exception("Need to Check : To be used paramerter file.");
                            }
                            else if ((ExistXml && ExistJson))
                            {
                                NeedConvertToJason = false;
                                NeedBackupParam = true;
                            }
                            else if ((ExistBin && ExistJson))
                            {
                                NeedConvertToJason = false;
                                NeedBackupParam = true;
                            }
                            else
                            {
                                NeedConvertToJason = false;
                                ErrorOccurredFlag = true;

                                LoggerManager.Error("Unknown.");
                            }
                        }
                        else if (ExistFileCount == 3)
                        {
                            if (ExistXml && ExistBin && ExistJson)
                            {
                                NeedConvertToJason = false;
                                NeedBackupParam = true;
                            }
                            else
                            {
                                NeedConvertToJason = false;
                                ErrorOccurredFlag = true;

                                LoggerManager.Error("Exist File Count is unknown.");
                            }
                        }
                        else
                        {
                            NeedConvertToJason = false;
                            ErrorOccurredFlag = true;

                            LoggerManager.Error("Exist File Count is unknown.");
                        }

                        if (ErrorOccurredFlag == true)
                        {
                            RetVal = EventCodeEnum.PARAM_ERROR;
                            LoggerManager.Error($"[{type}] LoadSysParam(): Error occurred while loading parameters. Err = {0}");
                            return RetVal;
                        }

                        // Backup Logic

                        if (NeedBackupParam == true)
                        {
                            if (BackupParamFullpath == null)
                            {
                                BackupParamFullpath = module.FileManager().GetRootParamPath() + @"\Parameters\Backup_";
                                BackupParamFullpath = BackupParamFullpath + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                            }

                            if (Directory.Exists(BackupParamFullpath) == false)
                            {
                                Directory.CreateDirectory(BackupParamFullpath);
                            }

                            try
                            {
                                string source = null;
                                string dest = null;

                                string filename = null;

                                if (ExistXml == true)
                                {
                                    source = XmlFullPath;

                                    filename = Path.ChangeExtension(Path.GetFileName(source), ".xml");
                                    dest = BackupParamFullpath + "\\" + filename;

                                    bool hasFile = File.Exists(dest);
                                    if (hasFile)
                                    {
                                        string[] filePaths = source.Split('\\');

                                        if (3 < filePaths.Length)
                                        {
                                            dest = Path.ChangeExtension(
                                                $"{Path.GetFileNameWithoutExtension(source)}_{filePaths[filePaths.Length - 2]}_{filePaths[filePaths.Length - 3]}", ".xml");

                                            int index = 0;
                                            hasFile = File.Exists(dest);
                                            while (hasFile)
                                            {
                                                dest = Path.ChangeExtension(
                                                $"{Path.GetFileNameWithoutExtension(source)}_{filePaths[filePaths.Length - 2]}_{filePaths[filePaths.Length - 3]}_{index++}", ".xml");
                                                hasFile = File.Exists(dest);
                                            }
                                        }
                                    }

                                    LoggerManager.Debug($"[S] ParamFileMove.");
                                    ParamFileMove(source, dest);
                                    LoggerManager.Debug($"[E] ParamFileMove.");
                                }

                                if (ExistBin == true)
                                {
                                    source = BinFullPath;
                                    filename = Path.ChangeExtension(Path.GetFileName(source), ".bin");
                                    dest = BackupParamFullpath + "\\" + filename;

                                    bool hasFile = File.Exists(dest);
                                    if (hasFile)
                                    {
                                        string[] filePaths = source.Split('\\');

                                        if (3 < filePaths.Length)
                                        {
                                            dest = Path.ChangeExtension(
                                                $"{Path.GetFileNameWithoutExtension(source)}_{filePaths[filePaths.Length - 2]}_{filePaths[filePaths.Length - 3]}", ".bin");
                                        }
                                    }

                                    ParamFileMove(source, dest);
                                }
                            }
                            catch (Exception err)
                            {
                                LoggerManager.Exception(err);
                            }
                        }

                        if (NeedConvertToJason == true)
                        {
                            Type[] types = null;

                            if (tempInstance is IHasAbstactClassSerialized)
                            {
                                types = (tempInstance as IHasAbstactClassSerialized).GetImplTypes();
                            }

                            if (ExistXml == true)
                            {
                                deserializedObj = ParamDeSerialize(FileType.XML, XmlFullPath, types);

                                LoggerManager.Debug($"[S] Delete File");
                                File.Delete(XmlFullPath);
                                LoggerManager.Debug($"[E] Delete File");
                            }
                            else
                            {
                                deserializedObj = ParamDeSerialize(FileType.BIN, BinFullPath, types);

                                LoggerManager.Debug($"[S] Delete File");
                                File.Delete(BinFullPath);
                                LoggerManager.Debug($"[E] Delete File");
                            }

                            if (deserializedObj == null)
                            {
                                RetVal = EventCodeEnum.PARAM_ERROR;

                                LoggerManager.Error($"[{type}] LoadSysParam(): DeSerialize Error");
                            }
                            else
                            {
                                // Convert To Json Object

                                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();

                                jsonSerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                                //jsonSerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                                jsonSerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                                jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

                                //string JsonObj = JsonConvert.SerializeObject(deserializedObj, new Newtonsoft.Json.Converters.StringEnumConverter());
                                LoggerManager.Debug($"[S] {module?.ToString()} Json SerializeObject.");
                                string JsonObj = JsonConvert.SerializeObject(deserializedObj, jsonSerializerSettings);
                                LoggerManager.Debug($"[E] {module?.ToString()} Json SerializeObject.");

                                try
                                {
                                    LoggerManager.Debug($"[S] WriteAllText.");
                                    File.WriteAllText(JsonFullPath, JsonObj);
                                    LoggerManager.Debug($"[E] WriteAllText.");
                                }
                                catch (SecurityException err)
                                {
                                    LoggerManager.Exception(err);
                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Exception(err);
                                }
                            }
                        }

                        if (deserializedObj == null)
                        {
                            // TODO : Check Logic
                            
                            if(File.Exists(JsonFullPath) == false)
                            {
                                tempInstance.Owner = null;
                                JsonSerialize(tempInstance, JsonFullPath);
                            }
                        }

                        try
                        {
                            RetVal = JsonDeserialize(type, JsonFullPath, out deserializedObj);
                            param = deserializedObj as IParam;

                            // todo : remove
                            // temp code 181105
                            //JsonSerialize(deserializedObj, JsonFullPath);
                        }
                        catch (JsonSerializationException err)
                        {
                            RetVal = EventCodeEnum.PARAM_ERROR;
                            LoggerManager.Exception(err);
                        }
                        catch (JsonException err)
                        {
                            RetVal = EventCodeEnum.PARAM_ERROR;
                            LoggerManager.Exception(err);
                        }
                        catch (Exception err)
                        {
                            RetVal = EventCodeEnum.PARAM_ERROR;
                            LoggerManager.Exception(err);
                        }

                        //String paramGenealogy = param?.Genealogy;
                        //if (paramGenealogy != null)
                        //    param.Genealogy = paramGenealogy;

                        //bool isPramChanged = false; //지워도 되지 않을까..
                        //RetVal = param.NullPropertyFill(tempInstance, out isPramChanged);

                        if (RetVal != EventCodeEnum.NONE)
                        {
                            RetVal = EventCodeEnum.PARAM_ERROR;
                            LoggerManager.Error($"[{type}] LoadSysParam(): Error occurred while loading file. ({JsonFullPath})");
                            return RetVal;
                        }

                        LoggerManager.Debug($"[S] Parameter ({type.Name}) Init()");
                        RetVal = param.Init();
                        LoggerManager.Debug($"[E] Parameter ({type.Name}) Init()");

                        if (RetVal != EventCodeEnum.NONE)
                        {
                            RetVal = EventCodeEnum.PARAM_ERROR;
                            LoggerManager.Error($"[{type}] LoadSysParam(): Error occurred while loading parameters. Err = {0}");
                            return RetVal;
                        }

                        CollectElement(module, param, owner);

                        RetVal = EventCodeEnum.NONE;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        RetVal = EventCodeEnum.PARAM_ERROR;
                        throw;
                    }
                }
                else
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    LoggerManager.Error($"[{type}] LoadSysParam(): param isn't IParam");
                    throw new Exception($"[{type} - {instance}] LoadSysParam(): param isn't IParam");
                }

                param.Owner = GetOwner(module, owner);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            LoggerManager.Debug($"[E] [Load Parameter] {type.Name}", isInfo: IsInfo);

            return RetVal;
        }

        public static void CollectElement(this IFactoryModule module, IParam param, Object owner = null)
        {
            try
            {
                bool isSysParam = false;

                if (module != null)
                {
                    SetBaseGenealogy(param, GetOwner(module, owner).GetType().Name);
                }

                if (param is ISystemParameterizable)
                {
                    isSysParam = true;
                }
                else if (param is IDeviceParameterizable)
                {
                    isSysParam = false;
                }

                ParamType paramType = isSysParam ? ParamType.SYS : ParamType.DEV;

                param.SetElementMetaData();
                CollectElement(param, paramType);

                param.Owner = GetOwner(module, owner);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void SetBaseGenealogy(IParamNode param, String ownerModuleName)
        {
            param.Genealogy = ownerModuleName + "." + param.GetType().Name + ".";
        }

        public static EventCodeEnum LoadDataFromJson(ref object param, Type type, string fixFullPath, string extension = null, FileType fileType = FileType.NONE, Object owner = null)
        {
            object deserializedObj = null;
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                object instance = Activator.CreateInstance(type);


                if (instance != null)
                {
                    try
                    {
                        CheckAndCreateDirectoryExists(fixFullPath);

                        // Convert Logic
                        var JsonFullPath = Path.ChangeExtension(fixFullPath, ".json");

                        if (File.Exists(JsonFullPath) == false)
                        {
                            if (File.Exists(fixFullPath) == false)
                            {
                                JsonSerialize(instance, JsonFullPath);
                            }
                        }

                        try
                        {
                            RetVal = JsonDeserialize(type, JsonFullPath, out deserializedObj);
                        }
                        catch (JsonSerializationException err)
                        {
                            LoggerManager.Exception(err);
                        }
                        catch (JsonException err)
                        {
                            LoggerManager.Exception(err);
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }

                        param = deserializedObj;
                        RetVal = EventCodeEnum.NONE;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);

                        RetVal = EventCodeEnum.PARAM_ERROR;

                        throw;
                    }
                }
                else
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    LoggerManager.Error($"[{type}] LoadSysParam(): param isn't IParam");
                    throw new Exception($"[{type} - {instance}] LoadSysParam(): param isn't IParam");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }
        public abstract class SerializeHelper
        {
            public abstract EventCodeEnum Serialize();
            public abstract EventCodeEnum DeSerialize();
        }

        public class JsonSerializeHelper : SerializeHelper
        {
            public override EventCodeEnum DeSerialize()
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                return retVal;
            }

            public override EventCodeEnum Serialize()
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                return retVal;
            }
        }

        public class XmlSerializeHelper : SerializeHelper
        {
            public override EventCodeEnum DeSerialize()
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                return retVal;
            }

            public override EventCodeEnum Serialize()
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                return retVal;
            }
        }

        public class BinSerializeHelper : SerializeHelper
        {
            public override EventCodeEnum DeSerialize()
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                return retVal;
            }

            public override EventCodeEnum Serialize()
            {
                EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

                return retVal;
            }
        }

        private static EventCodeEnum SetDefaultParam(object param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.DEFAULT)
                {
                    if (param is IParam)
                    {
                        retval = (param as IParam).SetDefaultParam();
                    }
                }
                else if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    if (param is IParam)
                    {
                        retval = (param as IParam).SetEmulParam();
                    }
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.UNDEFINED;
                LoggerManager.Debug("[Extentions_IParam] Fail Setting Parameter to Default Value.");
                LoggerManager.Exception(err);
            }

            return retval;
        }


        private static object ParamDeSerialize(FileType fileType, string FullPath, Type[] typelist = null)
        {
            object deserializedObj;
            bool IsSuccess = false;

            try
            {
                if (fileType == FileType.NONE || fileType == FileType.BIN)
                {
                    LoggerManager.Debug($"[S] BinaryDeSerialize.");
                    IsSuccess = SerializeManager.Deserialize(FullPath, out deserializedObj, deserializerType: SerializerType.BIN, deserializeTypes: typelist);
                    LoggerManager.Debug($"[E] BinaryDeSerialize.");
                }
                else
                {
                    LoggerManager.Debug($"[S] ExtendedXmlDeSerialize.");
                    IsSuccess = SerializeManager.Deserialize(FullPath, out deserializedObj, deserializerType: SerializerType.EXTENDEDXML, deserializeTypes: typelist);
                    LoggerManager.Debug($"[E] ExtendedXmlDeSerialize.");
                }

                if (IsSuccess == false)
                {
                    LoggerManager.Error($"ParamDeSerialize faild.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return deserializedObj;
        }

        public static EventCodeEnum SaveDataToJson(object param, string fixFullPath, string extension = null, FileType fileType = FileType.NONE, Type[] types = null)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            //BinarySerialize
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(fixFullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fixFullPath));
                }

                var JsonFullPath = Path.ChangeExtension(fixFullPath, ".json");
                JsonSerialize(param, JsonFullPath);

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);
                throw err;
            }

            return RetVal;
        }

        public static EventCodeEnum ChangeElementLog(IParamNode param, string fileName, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            List<PropertyInfo> propertyList = null;

            try
            {

                param.Nodes = new List<object>();
                propertyList = GetPropertyTypes(param, typeof(IParamNode), typeof(IElement), typeof(IList));

                foreach (PropertyInfo propInfo in propertyList)
                {
                    Attribute paramIgnore = null;
                    Attribute sharePropPath = null;
                    Object nodeInstance = null;

                    paramIgnore = propInfo.GetCustomAttribute(typeof(ParamIgnore));

                    if (paramIgnore != null)
                        continue;
                    if (typeof(IDeviceParameterizable).IsAssignableFrom(propInfo.PropertyType))
                        continue;
                    if (typeof(ISystemParameterizable).IsAssignableFrom(propInfo.PropertyType))
                        continue;

                    sharePropPath = propInfo.GetCustomAttribute(typeof(SharePropPath));
                    nodeInstance = propInfo.GetValue(param);

                    if (nodeInstance is IList)
                    {
                        Type genericArgType = nodeInstance.GetType().GenericTypeArguments.FirstOrDefault();

                        if (typeof(IParamNode).IsAssignableFrom(genericArgType))
                        {
                            IList list = null;

                            sharePropPath = genericArgType.GetCustomAttribute(typeof(SharePropPath));

                            list = nodeInstance as IList;

                            for (int i = 0; i < list.Count; i++)
                            {
                                var node = list[i] as IParamNode;

                                ChangeElementLog(node, fileName, index);
                            }
                        }

                    }
                    else if (nodeInstance is IParamNode)
                    {
                        IParamNode node = nodeInstance as IParamNode;

                        ChangeElementLog(node, fileName, index);
                    }
                    else if (nodeInstance is IElement)
                    {
                        IElement element = nodeInstance as IElement;

                        if (element.GetValue() is IList)
                        {
                            Type genericArgType = element.GetValue().GetType().GenericTypeArguments.FirstOrDefault();

                            if (typeof(IParamNode).IsAssignableFrom(genericArgType))
                            {
                                IList list = null;

                                sharePropPath = genericArgType.GetCustomAttribute(typeof(SharePropPath));

                                list = element.GetValue() as IList;

                                for (int i = 0; i < list.Count; i++)
                                {
                                    var node = list[i] as IParamNode;

                                    ChangeElementLog(node, fileName, index);
                                }
                            }
                        }
                        else if (element.GetValue() is IDictionary)
                        {
                            Type genericArgType = element.GetValue().GetType().GenericTypeArguments.FirstOrDefault();

                            if (typeof(IParamNode).IsAssignableFrom(genericArgType))
                            {
                                IDictionary list = null;

                                sharePropPath = genericArgType.GetCustomAttribute(typeof(SharePropPath));

                                list = element.GetValue() as IDictionary;

                                for (int i = 0; i < list.Count; i++)
                                {
                                    var node = list[i] as IParamNode;

                                    ChangeElementLog(node, fileName, index);
                                }
                            }
                        }
                        else if (element.GetValue() is IParamNode)
                        {
                            IParamNode node = element.GetValue() as IParamNode;

                            ChangeElementLog(node, fileName, index);
                        }
                        else if (element.IsChanged)
                        {

                            if (!element.GetValue().Equals(element.GetOriginValue()))
                            {
                                element.IsChanged = false;
                                LoggerManager.ParamLog(fileName, element.PropertyPath, element.GetOriginValue()?.ToString(), element.GetValue().ToString(), index);
                                element.SetOriginValue();
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public static EventCodeEnum SaveParameter(this IFactoryModule module, IParam param, string extension = null, string fixFullPath = null, FileType fileType = FileType.NONE, Type[] types = null, bool isNotSave_ChangeLog = false)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            string FullPath = string.Empty;
            string filePath = string.Empty;
            string fileName = string.Empty;

            //// 변경 된 파라미터가 없기 때문에 Save 할 필요 없음. 
            //// 변경 된 파라미터가 존재함에도 불구하고, 해당 값이 False라면 버그가 존재하는 것.
            //if (param.IsParamChanged != true)
            //{
            //    RetVal = EventCodeEnum.NONE;
            //    return RetVal;
            //}

            if (param == null)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;

                LoggerManager.Debug($"Caller : {module.ToString()}, Parameter is null.");

                return RetVal;
            }

            //경로지정
            if (fixFullPath != null)
            {
                FullPath = fixFullPath;
            }
            else
            {
                FullPath = GetFullPath();
            }
            //BinarySerialize
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
                }

                var JsonFullPath = Path.ChangeExtension(FullPath, ".json");

                if (isNotSave_ChangeLog == false)
                {
                    int idx = 0;

                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        if (module.LoaderController() != null)
                        {
                            if (AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                            {
                                idx = module.LoaderController().GetChuckIndex();
                            }
                        }
                    }

                    ChangeElementLog(param, JsonFullPath, idx);
                }

                long freeSpace = param.FileManager().GetFreeSpaceFromDrive(JsonFullPath);
                long fileSize = param.FileManager().GetFileSize(JsonFullPath);

                if (fileSize > -1 && freeSpace > -1)
                {
                    if (fileSize * 2 > freeSpace)
                    {
                        double filesize_KB = Math.Round((double)fileSize / 1024, 3);
                        double freeSpace_KB = Math.Round((double)freeSpace / 1024, 3);
                        string diskType = Path.GetPathRoot(JsonFullPath);

                        LoggerManager.Debug($"[Save Parameter] Save Parameter Failed. File Path: {JsonFullPath} \n" +
                            $"The remaining disk space is less than twice the size of the file to be saved.." +
                            $"File Size: {filesize_KB} KB, Disk({diskType}) free space: {freeSpace_KB} KB", isInfo: IsInfo);

                        RetVal = EventCodeEnum.PARAM_ERROR;
                    }
                    else
                    {
                        JsonSerialize(param, JsonFullPath);
                        LoggerManager.Debug($"[Save Parameter] Save Parameter Sucess Path:{JsonFullPath}", isInfo: IsInfo);

                        RetVal = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Debug($"[Save Parameter] Invalid freeSpace Ret: {freeSpace}, fileSize Ret: {fileSize}");
                    RetVal = EventCodeEnum.PARAM_ERROR;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);
                throw err;
            }

            return RetVal;

            string GetFullPath()
            {
                string retFullPath = string.Empty;

                try
                {
                    if (fixFullPath != null)
                    {
                        retFullPath = fixFullPath;
                    }
                    else
                    {
                        try
                        {
                            extension = extension != null ? extension + '_' : null;
                            retFullPath = GetFullPathUsingIParamInstance();
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"[{param.GetType().Name}] " + err.Message);
                            throw err;
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }

                return retFullPath;
            }

            string GetFullPathUsingIParamInstance()
            {
                string retFilePath = string.Empty;
                string tmpPath = string.Empty;
                string tmpName = string.Empty;


                try
                {
                    if (!string.IsNullOrEmpty(param.FileName))
                    {
                        tmpPath = param.FilePath;
                        tmpName = param.FileName;
                        if (extension != null)
                            tmpName = extension + tmpName;
                    }
                    else
                    {
                        object instance = Activator.CreateInstance(param.GetType());
                        IParam paramForGetName = instance as IParam;

                        tmpPath = paramForGetName.FilePath;
                        tmpName = extension + paramForGetName.FileName;
                    }


                    if (param is ISystemParameterizable)
                    {
                        retFilePath = param.FileManager().GetSystemParamFullPath(tmpPath, tmpName);
                    }
                    else if (param is IDeviceParameterizable)
                    {
                        retFilePath = param.FileManager().GetDeviceParamFullPath(tmpPath, tmpName);
                    }
                    else
                    {
                        throw new Exception($"SaveParameter() - GetFullPathUsingIParamInstance() : param isn't ISystemParameterizable or IDeviceParameterizable.");
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }

                return retFilePath;
            }

        }

        public static EventCodeEnum SaveParameterCallElement(this IFactoryModule module, IParam param, string extension = null, string fixFullPath = null)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            string FullPath = string.Empty;
            string filePath = string.Empty;
            string fileName = string.Empty;

            //경로지정
            if (fixFullPath != null)
            {
                FullPath = fixFullPath;
            }
            else
            {
                FullPath = GetFullPath();
            }

            //BinarySerialize
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
                }

                var JsonFullPath = Path.ChangeExtension(FullPath, ".json");

                JsonSerialize(param, JsonFullPath);

                LoggerManager.Debug($"[Save Parameter] Save Parameter By Caller Sucess Path:{JsonFullPath}");

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);
                throw err;
            }

            return RetVal;

            string GetFullPath()
            {
                string retFullPath = string.Empty;

                try
                {
                    if (fixFullPath != null)
                    {
                        retFullPath = fixFullPath;
                    }
                    else
                    {
                        try
                        {
                            extension = extension != null ? extension + '_' : null;
                            retFullPath = GetFullPathUsingIParamInstance();
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Error($"[{param.GetType().Name}] " + err.Message);
                            throw err;
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }

                return retFullPath;
            }

            string GetFullPathUsingIParamInstance()
            {
                string retFilePath = string.Empty;
                try
                {
                    string tmpPath = string.Empty;
                    string tmpName = string.Empty;


                    if (!string.IsNullOrEmpty(param.FileName))
                    {
                        tmpPath = param.FilePath;
                        tmpName = param.FileName;
                    }
                    else
                    {
                        object instance = Activator.CreateInstance(param.GetType());
                        IParam paramForGetName = instance as IParam;

                        tmpPath = paramForGetName.FilePath;
                        tmpName = extension + paramForGetName.FileName;
                    }


                    if (param is ISystemParameterizable)
                    {
                        retFilePath = param.FileManager().GetSystemParamFullPath(tmpPath, tmpName);
                    }
                    else if (param is IDeviceParameterizable)
                    {
                        retFilePath = param.FileManager().GetDeviceParamFullPath(tmpPath, tmpName);
                    }
                    else
                    {
                        throw new Exception($"SaveParameter() - GetFullPathUsingIParamInstance() : param isn't ISystemParameterizable or IDeviceParameterizable.");
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }

                return retFilePath;
            }
        }
        public static EventCodeEnum SaveElementParmeter(IParamNode param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var properties = GetPropertyTypes(param, typeof(IParamNode), typeof(IElement), typeof(IList));

                foreach (var prop in properties)
                {
                    var paramIgnore = prop.GetCustomAttribute(typeof(ParamIgnore));

                    if (paramIgnore == null)
                    {
                        var nodeInstance = prop.GetValue(param);

                        if (nodeInstance is IList)
                        {
                            var genericArgType = nodeInstance.GetType().GenericTypeArguments.FirstOrDefault();

                            if (typeof(IParamNode).IsAssignableFrom(genericArgType))
                            {
                                IList list = nodeInstance as IList;
                                for (int i = 0; i < list.Count; i++)
                                {
                                    var node = list[i] as IParamNode;

                                    retVal = SaveElementParmeter(node);
                                    if (retVal != EventCodeEnum.NONE)
                                        return retVal;
                                    if (prop.GetCustomAttribute(typeof(SharePropPath)) != null)
                                        return retVal;
                                }
                            }
                        }
                        else if (nodeInstance is IParamNode)
                        {
                            var node = nodeInstance as IParamNode;
                            retVal = SaveElementParmeter(node);
                            if (retVal != EventCodeEnum.NONE)
                                return retVal;
                        }
                        else if (nodeInstance is IElement)
                        {
                            IElement element = nodeInstance as IElement;
                            element.SaveElement(true);
                            retVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            if (prop is IElement)
                            {
                                (prop as IElement).SaveElement(true);
                                retVal = EventCodeEnum.NONE;
                            }
                        }

                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }
        public static void CollectCommonElement(IHasComParameterizable param, String ownerName)
        {
            param.SetElementMetaData();

            Extensions_IParam.SetBaseGenealogy(param, ownerName);
            Extensions_IParam.CollectElement(param, ParamType.COMMON);
        }

        public static EventCodeEnum ManualCollectElement(this IFactoryModule module, IParamNode param, ParamType paramType, bool sharePropPathFlag = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = CollectElement(param, paramType, sharePropPathFlag);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public static EventCodeEnum CollectElement(IParamNode param, ParamType paramType, bool sharePropPathFlag = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            List<PropertyInfo> propertyList = null;
            string genealogy = null;

            try
            {

                param.Nodes = new List<object>();
                propertyList = GetPropertyTypes(param, typeof(IParamNode), typeof(IElement), typeof(IList));

                foreach (PropertyInfo propInfo in propertyList)
                {
                    Attribute paramIgnore = null;
                    Attribute sharePropPath = null;
                    Object nodeInstance = null;

                    paramIgnore = propInfo.GetCustomAttribute(typeof(ParamIgnore));

                    if (paramIgnore != null)
                        continue;
                    if (typeof(IDeviceParameterizable).IsAssignableFrom(propInfo.PropertyType))
                        continue;
                    if (typeof(ISystemParameterizable).IsAssignableFrom(propInfo.PropertyType))
                        continue;

                    sharePropPath = propInfo.GetCustomAttribute(typeof(SharePropPath));
                    sharePropPathFlag = sharePropPath != null;
                    nodeInstance = propInfo.GetValue(param);

                    if (nodeInstance is IList)
                    {
                        Type genericArgType = nodeInstance.GetType().GenericTypeArguments.FirstOrDefault();

                        if (typeof(IParamNode).IsAssignableFrom(genericArgType))
                        {
                            IList list = null;

                            sharePropPath = genericArgType.GetCustomAttribute(typeof(SharePropPath));
                            if (sharePropPath != null)
                            {
                                sharePropPathFlag = true;
                            }
                            list = nodeInstance as IList;

                            for (int i = 0; i < list.Count; i++)
                            {
                                var node = list[i] as IParamNode;
                                node.Owner = param;
                                genealogy = null;

                                if (sharePropPathFlag)
                                {
                                    genealogy = "#" + node.GetType().Name + "[" + i + "].";
                                }
                                else
                                {
                                    genealogy = node.GetType().Name + "[" + i + "].";
                                }

                                node.Genealogy = param.Genealogy + genealogy;
                                CollectElement(node, paramType, sharePropPathFlag);
                            }
                            param.Nodes.Add(nodeInstance);
                        }
                        else if (typeof(IElement).IsAssignableFrom(genericArgType))
                        {
                            IList list = null;
                            list = nodeInstance as IList;
                            for (int index = 0; index < list.Count; index++)
                            {
                                if (list[index] is IElement)
                                {
                                    IElement elem = list[index] as IElement;
                                    AddElementDictionary(paramType, elem, param.ToString() + "." + propInfo.Name + "[" + index.ToString() + "]");
                                }

                            }

                            param.Nodes.Add(nodeInstance);

                        }
                    }
                    else if (nodeInstance is IParamNode)
                    {
                        IParamNode node = nodeInstance as IParamNode;
                        node.Owner = param;
                        genealogy = null;

                        if (sharePropPathFlag)
                        {
                            genealogy = "#" + propInfo.Name + ".";
                        }
                        else
                        {
                            genealogy = propInfo.Name + ".";
                        }
                        node.Genealogy = param.Genealogy + genealogy;
                        CollectElement(node, paramType, sharePropPathFlag);
                        param.Nodes.Add(node);
                    }
                    else if (nodeInstance is IElement)
                    {
                        IElement element = nodeInstance as IElement;
                        string dbPropertyPath = null;

                        if (element.GetValue() is IList)
                        {
                            Type genericArgType = element.GetValue().GetType().GenericTypeArguments.FirstOrDefault();

                            if (typeof(IParamNode).IsAssignableFrom(genericArgType))
                            {
                                IList list = null;

                                sharePropPath = genericArgType.GetCustomAttribute(typeof(SharePropPath));
                                if (sharePropPath != null)
                                {
                                    sharePropPathFlag = true;
                                }
                                list = element.GetValue() as IList;

                                for (int i = 0; i < list.Count; i++)
                                {
                                    var node = list[i] as IParamNode;
                                    node.Owner = param;
                                    genealogy = null;

                                    if (sharePropPathFlag)
                                    {
                                        genealogy = "#" + node.GetType().Name + "[" + i + "].";
                                    }
                                    else
                                    {
                                        genealogy = node.GetType().Name + "[" + i + "].";
                                    }

                                    node.Genealogy = param.Genealogy + genealogy;
                                    CollectElement(node, paramType, sharePropPathFlag);
                                }
                                param.Nodes.Add(element.GetValue());
                            }
                            else if (typeof(IElement).IsAssignableFrom(genericArgType))
                            {
                                IList list = null;
                                list = element.GetValue() as IList;
                                for (int index = 0; index < list.Count; index++)
                                {
                                    if (list[index] is IElement)
                                    {
                                        IElement elem = list[index] as IElement;
                                        AddElementDictionary(paramType, elem, param.ToString() + "." + propInfo.Name + "[" + index.ToString() + "]");
                                    }

                                }

                                param.Nodes.Add(element.GetValue());

                            }
                        }
                        else if (element.GetValue() is IDictionary)
                        {
                            Type genericArgType = element.GetValue().GetType().GenericTypeArguments.FirstOrDefault();

                            if (typeof(IParamNode).IsAssignableFrom(genericArgType))
                            {
                                IDictionary dic = null;

                                sharePropPath = genericArgType.GetCustomAttribute(typeof(SharePropPath));
                                if (sharePropPath != null)
                                {
                                    sharePropPathFlag = true;
                                }
                                dic = element.GetValue() as IDictionary;

                                for (int i = 0; i < dic.Count; i++)
                                {
                                    var node = dic[i] as IParamNode;
                                    node.Owner = param;
                                    genealogy = null;

                                    if (sharePropPathFlag)
                                    {
                                        genealogy = "#" + node.GetType().Name + "[" + i + "].";
                                    }
                                    else
                                    {
                                        genealogy = node.GetType().Name + "[" + i + "].";
                                    }

                                    node.Genealogy = param.Genealogy + genealogy;
                                    CollectElement(node, paramType, sharePropPathFlag);
                                }
                                param.Nodes.Add(element.GetValue());
                            }
                            else if (typeof(IElement).IsAssignableFrom(genericArgType))
                            {
                                IList list = null;
                                list = element.GetValue() as IList;
                                for (int index = 0; index < list.Count; index++)
                                {
                                    if (list[index] is IElement)
                                    {
                                        IElement elem = list[index] as IElement;
                                        AddElementDictionary(paramType, elem, param.ToString() + "." + propInfo.Name + "[" + index.ToString() + "]");
                                    }

                                }

                                param.Nodes.Add(nodeInstance);

                            }
                        }


                        if (element.GetValue() is IParamNode)
                        {
                            IParamNode node = element.GetValue() as IParamNode;
                            node.Owner = param;
                            genealogy = null;

                            if (sharePropPathFlag)
                            {
                                genealogy = "#" + propInfo.Name + ".";
                            }
                            else
                            {
                                genealogy = propInfo.Name + ".";
                            }
                            node.Genealogy = param.Genealogy + genealogy;
                            CollectElement(node, paramType, sharePropPathFlag);
                            param.Nodes.Add(node);
                        }
                        else
                        {
                            if (sharePropPathFlag)
                            {
                                genealogy = "#" + propInfo.Name;
                            }
                            else
                            {
                                genealogy = propInfo.Name;
                            }

                            FillPropInElement(element, param, genealogy);
                            element.SetOriginValue();
                            //Add Element to Dictionary
                            dbPropertyPath = GetDBPropertyPath(sharePropPathFlag, propInfo, param.Genealogy, genealogy);
                            AddElementDictionary(paramType, element, dbPropertyPath);

                            param.Nodes.Add(nodeInstance);
                        }
                        //SetEventForAfterFindingProperty(element);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private static void FillPropInElement(IElement element, IParamNode param, string genealogy)
        {
            element.Owner = param;
            if (element.GetValue() != null)
            {
                List<PropertyInfo> elementProperties = GetPropertyTypes(element.GetValue(), typeof(IParamNode));
                if (elementProperties.Count >= 1)
                {
                    foreach (PropertyInfo propertyInfo in elementProperties)
                    {
                        if ((element.GetValue() as IParamNode) != null)
                        {
                            Object node = propertyInfo.GetValue(element.GetValue());
                            MakeOwner(propertyInfo as IParamNode, node);
                        }
                        else if (element.GetValue() is IList)
                        {
                            Type genericArgType =
                                element.GetValue().GetType().GenericTypeArguments.FirstOrDefault();

                            if (typeof(IParamNode).IsAssignableFrom(genericArgType))
                            {
                                IList list = element.GetValue() as IList;
                                for (int i = 0; i < list.Count; i++)
                                {
                                    IParamNode node = list[i] as IParamNode;
                                    node.Owner = param;
                                    MakeOwner(node, node);
                                }
                            }
                        }
                    }
                }
            }
            element.PropertyPath = param.Genealogy + genealogy;
        }

        private static string GetDBPropertyPath(bool sharePropPathFlag, PropertyInfo propInfo, string paramGenealogy, string genealogy)
        {
            string retDBPropertyPath = null;
            string[] split = null;

            split = (paramGenealogy + genealogy).Split('.');

            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].Contains("#"))
                {
                    int start = split[i].IndexOf("[");
                    if (start > 0)
                    {
                        split[i] = split[i].Remove(start);
                    }
                }
                if (i != split.Length - 1)
                {
                    retDBPropertyPath += split[i] + ".";
                }
                else
                {
                    retDBPropertyPath += split[i];
                }
            }

            return retDBPropertyPath;
        }

        //private static void SetEventForAfterFindingProperty(IElement element)
        //{
        //    if (OnFoundProperty != null)
        //    {
        //        ElementArgs args = new ElementArgs();
        //        args.elementData = element;
        //        OnFoundProperty?.Invoke(null, args);
        //    }
        //}

        private static void AddElementDictionary(ParamType paramType, IElement element, String dbPropertyPath)
        {
            ConcurrentDictionary<String, ConcurrentDictionary<String, IElement>> elemDictionary = null;

            switch (paramType)
            {
                case ParamType.SYS:
                    elemDictionary = ParamManager?.SysDBElementDictionary;
                    break;
                case ParamType.DEV:
                    elemDictionary = ParamManager?.DevDBElementDictionary;
                    break;
                case ParamType.COMMON:
                    elemDictionary = ParamManager?.CommonDBElementDictionary;
                    break;
            }

            try
            {

                if (elemDictionary != null)
                {
                    if (elemDictionary.ContainsKey(dbPropertyPath) == false)//==> Element가 처음 추가 될 때
                    {
                        elemDictionary.TryAdd(dbPropertyPath, new ConcurrentDictionary<String, IElement>());
                        elemDictionary[dbPropertyPath].TryAdd(element.PropertyPath, element);
                    }
                    else
                    {
                        if (elemDictionary[dbPropertyPath].ContainsKey(element.PropertyPath) == false)//==> Element가 존재하지 않을 시 새로 추가
                        {
                            elemDictionary[dbPropertyPath].TryAdd(element.PropertyPath, element);
                        }
                        else//==> Element가 존재하여 기존의 Element를 갈아 끼움(주로 파일을 다시 Load 할 때 발생)
                        {
                            //==> DB Load 작업을 하지 않았다면 NULL이다.
                            if (elemDictionary[dbPropertyPath][element.PropertyPath].ValueType != null)
                                ParamManager.LoadElementInfoFromDB(paramType, element, dbPropertyPath);

                            if(elemDictionary[dbPropertyPath][element.PropertyPath].ApplyAltValue)
                            {
                                object altValue = elemDictionary[dbPropertyPath][element.PropertyPath].GetValue();
                                element.SetAltValue(altValue);
                            }

                            elemDictionary[dbPropertyPath][element.PropertyPath] = element;

                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        private static EventCodeEnum MakeOwner(IParamNode param, object owner)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (param == null)
                {
                    return EventCodeEnum.NONE;
                }
                param.Nodes = new List<object>();
                var properties = GetPropertyTypes(param, typeof(IParamNode), typeof(IElement), typeof(IList));

                foreach (var prop in properties)
                {

                    var nodeInstance = prop.GetValue(param);


                    if (nodeInstance is IList)
                    {
                        var genericArgType = nodeInstance.GetType().GenericTypeArguments.FirstOrDefault();

                        if (typeof(IParamNode).IsAssignableFrom(genericArgType))
                        {

                            IList list = nodeInstance as IList;
                            for (int i = 0; i < list.Count; i++)
                            {
                                var node = list[i] as IParamNode;
                                node.Owner = owner;
                                MakeOwner(node, owner);
                            }
                            param.Nodes.Add(nodeInstance);
                        }
                    }
                    else if (nodeInstance is IParamNode)
                    {
                        var node = nodeInstance as IParamNode;
                        node.Owner = owner;
                        MakeOwner(node, owner);
                        param.Nodes.Add(node);
                    }
                    else if (nodeInstance is IElement)
                    {
                        var element = nodeInstance as IElement;

                        element.Owner = owner;
                        if (element.GetValue() != null)
                        {
                            var elementProperties = GetPropertyTypes(element.GetValue(), typeof(IParamNode));
                            if (elementProperties.Count >= 1)
                            {
                                MakeOwner(element.GetValue() as IParamNode, owner);
                            }
                        }

                        param.Nodes.Add(nodeInstance);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
        private static List<PropertyInfo> GetPropertyTypes(object instance, params Type[] interfaceTypes)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();

            try
            {
                var type = instance.GetType();
                var properties = type.GetProperties();
                foreach (var prop in properties)
                {
                    foreach (var interfaceType in interfaceTypes)
                    {
                        if (interfaceType.IsAssignableFrom(prop.PropertyType))
                        {
                            list.Add(prop);
                            break;
                        }
                        else if (prop.PropertyType.Name == "IList`1")
                        {
                            list.Add(prop);
                            break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return list;
        }

        public static byte[] CompressFolderToByteArray(this IFactoryModule module, string extractPath)
        {
            byte[] arr = new byte[0];
            try
            {
                string zippath = extractPath + ".zip";
                if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    extractPath += Path.DirectorySeparatorChar;

                if (!File.Exists(zippath))
                    ZipFile.CreateFromDirectory(extractPath, zippath);

                arr = File.ReadAllBytes(zippath);

                File.Delete(extractPath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return arr;
        }
        public static byte[] CompressFileToStream(this IFactoryModule module, string extractPath)
        {
            return CompressFileToStream_Content(extractPath);
        }

        public static byte[] CompressFileToStream(string extractPath)
        {
            return CompressFileToStream_Content(extractPath);
        }

        private static byte[] CompressFileToStream_Content(string extractPath)
        {
            byte[] bytearray = null;

            try
            {
                if (extractPath != null)
                {
                    if (File.Exists(extractPath))
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            var bytes = File.ReadAllBytes(extractPath);
                            stream.Write(bytes, 0, bytes.Length);
                            stream.Position = 0;

                            byte[] buffer = new byte[16 * 1024];
                            using (MemoryStream ms = new MemoryStream())
                            {
                                int read;
                                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    ms.Write(buffer, 0, read);
                                }
                                bytearray = ms.ToArray();
                                //bytearray = new byte[300];
                            }
                        }

                    }
                }

                //return stream;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return bytearray;
        }

        public static void DecompressFilesFromByteArray(this IFactoryModule module, Stream stream, string filepath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var content = reader.ReadToEnd();
                    File.WriteAllText(filepath, content);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public static void DecompressFilesFromByteArray(this IFactoryModule module, byte[] param, string filepath)
        {
            try
            {
                using (Stream stream = new MemoryStream(param))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var content = reader.ReadToEnd();
                        File.WriteAllText(filepath, content);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public static byte[] ObjectToByteArray(this IFactoryModule module, Object obj)
        {
            if (obj == null)
                return null;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, obj);
                    byte[] arr = ms.ToArray();
                    return arr;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return null;
        }
        // Convert a byte array to an Object
        public static Object ByteArrayToObjectSync(this IFactoryModule module, byte[] arrBytes)
        {
            Object obj = null;
            try
            {
                if (arrBytes != null)
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStream.Write(arrBytes, 0, arrBytes.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        obj = (Object)binForm.Deserialize(memStream);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return obj;
        }
        // Convert a byte array to an Object
        public static async Task<Object> ByteArrayToObject(this IFactoryModule module, byte[] arrBytes)
        {
            Object obj = null;
            try
            {
                Task<object> task = new Task<object>(() =>
                {
                    Object streamObj = null;
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStream.Write(arrBytes, 0, arrBytes.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        streamObj = (Object)binForm.Deserialize(memStream);
                        return streamObj;
                    }
                });
                task.Start();
                obj = await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return obj;
        }

        public static byte[] CompressFile(this IFactoryModule module, Object obj)
        {
            byte[] filearry = null;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return filearry;
        }


        /// <summary>
        /// Update State 가 아닌 Property가 있는지 확인하는 함수. (Update State가 있으면 return PARAM_INSUFFICIENT)
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static EventCodeEnum ElementStateNeedSetupValidation(IParamNode param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                var properties = GetPropertyTypes(param, typeof(IParamNode), typeof(IElement), typeof(IList));
                foreach (var prop in properties)
                {
                    var paramIgnore = prop.GetCustomAttribute(typeof(ParamIgnore));
                    if (paramIgnore == null)
                    {
                        var nodeInstance = prop.GetValue(param);

                        if (nodeInstance is IList)
                        {
                            var genericArgType = nodeInstance.GetType().GenericTypeArguments.FirstOrDefault();

                            if (typeof(IParamNode).IsAssignableFrom(genericArgType))
                            {
                                IList list = nodeInstance as IList;
                                for (int i = 0; i < list.Count; i++)
                                {
                                    var node = list[i] as IParamNode;
                                    ElementStateNeedSetupValidation(node);
                                }
                            }
                        }
                        else if (nodeInstance is IParamNode)
                        {
                            var node = nodeInstance as IParamNode;
                            ElementStateNeedSetupValidation(node);
                        }
                        else if (nodeInstance is IElement)
                        {
                            if ((nodeInstance as IElement).DoneState == State.ElementStateEnum.NEEDSETUP)
                            {
                                retVal = EventCodeEnum.PARAM_INSUFFICIENT;
                                return retVal;
                            }
                        }
                    }
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {

                throw err;
            }
            return retVal;
        }
        public static EventCodeEnum ElementStateDefaultValidation(IParamNode param)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                var properties = GetPropertyTypes(param, typeof(IParamNode), typeof(IElement), typeof(IList));

                foreach (var prop in properties)
                {
                    var paramIgnore = prop.GetCustomAttribute(typeof(ParamIgnore));

                    if (paramIgnore == null)
                    {
                        var nodeInstance = prop.GetValue(param);

                        if (nodeInstance is IList)
                        {
                            var genericArgType = nodeInstance.GetType().GenericTypeArguments.FirstOrDefault();

                            if (typeof(IParamNode).IsAssignableFrom(genericArgType))
                            {
                                IList list = nodeInstance as IList;

                                for (int i = 0; i < list.Count; i++)
                                {
                                    var node = list[i] as IParamNode;

                                    retVal = ElementStateDefaultValidation(node);

                                    if (retVal != EventCodeEnum.NONE)
                                        return retVal;
                                }
                            }
                        }
                        else if (nodeInstance is IParamNode)
                        {
                            var node = nodeInstance as IParamNode;

                            retVal = ElementStateDefaultValidation(node);

                            if (retVal != EventCodeEnum.NONE)
                                return retVal;
                        }
                        else if (nodeInstance is IElement)
                        {
                            if ((nodeInstance as IElement).DoneState == State.ElementStateEnum.DEFAULT)
                            {
                                retVal = EventCodeEnum.PARAM_INSUFFICIENT;

                                return retVal;
                            }
                        }
                    }
                }
                //====================================
                //param.Nodes = new List<object>();
                //var properties = GetPropertyTypes(param, typeof(IElement));
                //foreach (var prop in properties)
                //{
                //    var nodeInstance = prop.GetValue(param);
                //    {
                //        if ((nodeInstance as IElement).SetupState.GetState() == State.ElementStateEnum.MODIFY)
                //        {
                //            retVal = EventCodeEnum.UNDEFINED;

                //            return retVal;
                //        }
                //    }
                //}

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }

            return retVal;
        }
        public static EventCodeEnum NullPropertyFill(this object moduleParam, object tempParam, out bool isParamChanged)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                BindingFlags masking = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                var paramFields = moduleParam.GetType().GetFields(masking);
                isParamChanged = false;

                foreach (var field in paramFields)
                {
                    try
                    {
                        var fieldValue = field.GetValue(moduleParam);
                        var tempParamField = tempParam.GetType().GetField(field.Name, masking);

                        if ((fieldValue == null) && tempParamField != null)
                        {
                            var tempParamFieldValue = tempParamField.GetValue(tempParam);

                            if (tempParamFieldValue != null)
                            {
                                if (isParamChanged == false && moduleParam is IParam)
                                {
                                    LoggerManager.Debug($"파라미터 희생자들....{(moduleParam as IParam)?.Genealogy ?? string.Empty}");
                                }
                                isParamChanged = true;
                                field.SetValue(moduleParam, tempParamFieldValue);
                            }
                        }
                        else
                        {

                        }
                        retVal = EventCodeEnum.NONE;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, ex);
                        retVal = EventCodeEnum.UNDEFINED;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }
    }

    public class IParamEmpty : IParam
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        public List<object> Nodes { get; set; }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public void SetElementMetaData()
        {

        }
    }

    [ServiceContract]
    public interface IHasParameterizable
    { }

    [ServiceContract]
    public interface IHasDevParameterizable : IHasParameterizable
    {
        //IParam DevParam { get; set; }
        [OperationContract]
        EventCodeEnum LoadDevParameter();
        [OperationContract]
        EventCodeEnum SaveDevParameter();
        [OperationContract]
        EventCodeEnum InitDevParameter();
    }

    [ServiceContract]
    public interface IHasSysParameterizable : IHasParameterizable
    {
        //IParam SysParam { get; set; }
        [OperationContract]
        EventCodeEnum LoadSysParameter();
        [OperationContract]
        EventCodeEnum SaveSysParameter();
    }

    public interface IPackagable
    {
        /// <summary>
        /// Loader 에게 Parameter 넘겨주기위해 (AdvanceSetup 등) Parameter 를 넣어놓는다.
        /// </summary>
        List<byte[]> PackagableParams { get; set; }
        void ApplyParams(List<byte[]> datas);
    }
    public interface IHasComParameterizable : IHasParameterizable, IElementMetaDataInitor, IParamNode
    { }

    public interface ISystemParameterizable : IParam
    {
    }
    public interface IDeviceParameterizable : IParam
    {
    }

    public interface IModuleParameter
    {
        bool IsInitialized { get; }
        EventCodeEnum Init();
    }

    public interface IParam : IFactoryModule, IParamNode, IElementMetaDataInitor
    {
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        string FilePath { get; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        string FileName { get; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        bool IsParamChanged { get; set; }
        EventCodeEnum Init();
        EventCodeEnum SetEmulParam();
        EventCodeEnum SetDefaultParam();
    }
    public interface IElementMetaDataInitor
    {
        void SetElementMetaData();
    }

    public static class ObjectCopy
    {
        // 1. Deep Clone 구현
        public static T DeepClone<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("Object cannot be null.");


            return (T)Process(obj, new Dictionary<object, object>() { });
        }

        private static object Process(object obj, Dictionary<object, object> circular)
        {
            try
            {
                if (obj == null)
                    return null;

                Type type = obj.GetType();

                if (type.IsValueType || type == typeof(string))
                {
                    return obj;
                }

                if (type.IsArray)
                {
                    if (circular.ContainsKey(obj))
                        return circular[obj];

                    string typeNoArray = type.FullName.Replace("[]", string.Empty);
                    Type elementType = Type.GetType(typeNoArray + ", " + type.Assembly.FullName);
                    var array = obj as Array;
                    Array arrCopied = Array.CreateInstance(elementType, array.Length);

                    circular[obj] = arrCopied;

                    for (int i = 0; i < array.Length; i++)
                    {
                        object element = array.GetValue(i);
                        object objCopy = null;

                        if (element != null && circular.ContainsKey(element))
                            objCopy = circular[element];
                        else
                            objCopy = Process(element, circular);

                        arrCopied.SetValue(objCopy, i);
                    }

                    return Convert.ChangeType(arrCopied, obj.GetType());
                }

                if (type.IsClass)
                {
                    if (circular.ContainsKey(obj))
                        return circular[obj];

                    object objValue = Activator.CreateInstance(obj.GetType());
                    circular[obj] = objValue;
                    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    foreach (FieldInfo field in fields)
                    {
                        object fieldValue = field.GetValue(obj);

                        if (fieldValue == null)
                            continue;

                        object objCopy = circular.ContainsKey(fieldValue) ? circular[fieldValue] : Process(fieldValue, circular);
                        field.SetValue(objValue, objCopy);
                    }

                    return objValue;
                }
                else
                    throw new ArgumentException("Unknown type");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }


}
