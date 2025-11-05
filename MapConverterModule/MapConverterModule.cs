using System;
using ProberErrorCode;
using LogModule;
using ProberInterfaces.ResultMap;
using ProberInterfaces;
using ResultMapParamObject;
using ProberInterfaces.ResultMap.Attributes;
using SerializerUtil;

namespace MapConverterModule
{
    public class ResultMapConverter : IFactoryModule, IHasDevParameterizable
    {
        private MapConverterBase Converter { get; set; }
        private MapConverterParamBase ConverterParam { get; set; }

        private ResultMapConverterParameter resultMapConverterParameter { get; set; }

        public IParam resultMapConverterIParameter { get; private set; }

        private FileReaderType GetReaderType(ResultMapConvertType convtype)
        {
            FileReaderType retval = FileReaderType.UNDEFINED;

            try
            {
                var convparam = GetConverterParam(convtype);

                if (convparam != null)
                {
                    retval = convparam.ReaderType.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public FileReaderType GetUploadReaderType()
        {
            FileReaderType retval = FileReaderType.UNDEFINED;

            try
            {
                ResultMapConvertType convtype = resultMapConverterParameter.UploadType.Value;

                retval = GetReaderType(convtype);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public FileReaderType GetDownloadReaderType()
        {
            FileReaderType retval = FileReaderType.UNDEFINED;

            try
            {
                ResultMapConvertType convtype = resultMapConverterParameter.DownloadType.Value;

                retval = GetReaderType(convtype);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private MapConverterParamBase GetConverterParam(ResultMapConvertType type)
        {
            MapConverterParamBase retval = null;

            try
            {
                if (resultMapConverterParameter != null)
                {
                    retval = resultMapConverterParameter.GetConvParam(type);
                }
                else
                {
                    LoggerManager.Error($"[ResultMapConverter], GetConvParam() : Converter parameter is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Type GetConverterType(ResultMapConvertType type)
        {
            Type retval = default(Type);

            try
            {
                Type[] types = this.GetType().Assembly.GetTypes();

                foreach (var t in types)
                {
                    ResultMapConvertType convtype = t.GetAttributeValue((MapConverterAttribute dna) => dna.CoverterType);

                    if (convtype == type)
                    {
                        retval = t;
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Type GetResultMapObjectType()
        {
            Type retval = default(Type);

            try
            {
                ResultMapConvertType convtype = resultMapConverterParameter.UploadType.Value;

                EventCodeEnum ret = EventCodeEnum.UNDEFINED;

                bool needCreateConverter = false;

                if (Converter != null)
                {
                    if (Converter.ConverterType != convtype)
                    {
                        needCreateConverter = true;
                    }

                }
                else
                {
                    needCreateConverter = true;
                }

                if (needCreateConverter == true)
                {
                    ret = CreateConverter(convtype);
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }

                if (ret == EventCodeEnum.NONE)
                {
                    retval = Converter.GetType().GetAttributeValue((MapConverterAttribute dna) => dna.ResultMapObjectType);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }



        private EventCodeEnum CreateConverter(ResultMapConvertType type)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ConverterParam = GetConverterParam(type);

                if (ConverterParam != null)
                {
                    var convtype = GetConverterType(type);

                    if (convtype != null)
                    {
                        Converter = (MapConverterBase)Activator.CreateInstance(convtype);
                    }
                    else
                    {
                        LoggerManager.Error($"[ResultMapConverter], MapConvert() : Converter type is null.");
                    }
                }
                else
                {
                    LoggerManager.Error($"[ResultMapConverter], MapConvert() : Converter parameter can not get.");
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum Convert(MapHeaderObject header, ResultMapData resultMap, ResultMapConvertType type, ref object mapfile)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = CreateConverter(type);

                if (retval == EventCodeEnum.NONE)
                {
                    if (Converter != null && ConverterParam != null)
                    {
                        retval = Converter.InitConverter(ConverterParam);

                        if (retval == EventCodeEnum.NONE)
                        {
                            retval = Converter.ConvertMapDataFromBaseMap(header, resultMap, ref mapfile);
                        }
                        else
                        {
                            LoggerManager.Error($"[ResultMapConverter], MapConvert() : Failed converter initialize.");
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[ResultMapConverter], MapConvert() : Converter is not created.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retval;
        }

        public EventCodeEnum ConvertBack(object basedata, ResultMapConvertType type, ref MapHeaderObject _header, ref ResultMapData _resultMap)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = CreateConverter(type);

                if (retval == EventCodeEnum.NONE)
                {
                    if (Converter != null && ConverterParam != null)
                    {
                        retval = Converter.InitConverter(ConverterParam);

                        if (retval == EventCodeEnum.NONE)
                        {
                            retval = Converter.ConvertMapDataToBaseMap(basedata, ref _header, ref _resultMap);
                        }
                        else
                        {
                            LoggerManager.Error($"[ResultMapConverter], ConvertBack() : Failed converter initialize.");
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"[ResultMapConverter], ConvertBack() : Converter is not created.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public char[,] GetASCIIMap()
        {
            char[,] retval = null;

            try
            {
                if (Converter != null)
                {
                    retval = Converter.GetASCIIMap();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //public object GetResultMap()
        //{
        //    object retval = null;

        //    try
        //    {
        //        if (Converter != null)
        //        {
        //            retval = Converter.GetResultMap();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        public ResultMapConvertType GetUploadConverterType()
        {
            ResultMapConvertType retval = ResultMapConvertType.OFF;

            try
            {
                if (resultMapConverterParameter != null)
                {
                    retval = resultMapConverterParameter.UploadType.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ResultMapConvertType GetDownloadConverterType()
        {
            ResultMapConvertType retval = ResultMapConvertType.OFF;

            try
            {
                if (resultMapConverterParameter != null)
                {
                    retval = resultMapConverterParameter.DownloadType.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam iparam = null;
                retVal = this.LoadParameter(ref iparam, typeof(ResultMapConverterParameter));

                if (retVal == EventCodeEnum.NONE)
                {
                    resultMapConverterIParameter = iparam;
                    this.resultMapConverterParameter = resultMapConverterIParameter as ResultMapConverterParameter;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(resultMapConverterIParameter);
            }

            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            throw new NotImplementedException();
        }

        public void SetResultMapConvIParam(byte[] param)
        {
            try
            {
                object target = null;

                var result = SerializeManager.DeserializeFromByte(param, out target, typeof(ResultMapConverterParameter));

                if (target != null)
                {
                    resultMapConverterParameter = target as ResultMapConverterParameter;
                    resultMapConverterIParameter = resultMapConverterParameter as ResultMapConverterParameter;

                    this.SaveDevParameter();
                }
                else
                {
                    LoggerManager.Error($"SetPolishWaferIParam function is faild.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// UI 디스플레이를 위한 결과맵 데이터 포맷을 얻어 옴.
        /// </summary>
        /// <returns></returns>
        public object GetOrgResultMap()
        {
            object retval = null;

            try
            {
                if (Converter != null)
                {
                    retval = Converter.GetResultMap();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public string GetNamerAlias(FileTrasnferType trasnferType)
        {
            string retval = string.Empty;

            try
            {
                if (trasnferType == FileTrasnferType.UPLOAD)
                {
                    if (resultMapConverterParameter.UploadType.Value == ResultMapConvertType.STIF)
                    {
                        retval = resultMapConverterParameter.STIFParam.NamerAlias.Value;
                    }
                    else
                    {
                        // TODO : 
                    }
                }
                else if (trasnferType == FileTrasnferType.DOWNLOAD)
                {
                    if (resultMapConverterParameter.DownloadType.Value == ResultMapConvertType.STIF)
                    {
                        retval = resultMapConverterParameter.STIFParam.NamerAlias.Value;
                    }
                    else
                    {
                        // TODO : 
                    }
                }
                else
                {
                    // TOOD : 
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

    }
}
