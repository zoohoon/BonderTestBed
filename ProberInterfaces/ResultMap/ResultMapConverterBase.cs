using LogModule;
using ProberErrorCode;
using ProberInterfaces.ResultMap.Script;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ProberInterfaces.ResultMap
{
    public interface IMapConverterBase
    {
        MapHeaderObject HeaderObj { get; set; }
        ResultMapData ResultMapObj { get; set; }

        IMapScripter Scripter { get; set; }

        ResultMapConvertType ConverterType { get;}
    }

    public static class PropertyExtension
    {
        public static EventCodeEnum GetPropertyValue(object targetobj, string targetname, out object value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            value = null;

            try
            {
                if (targetobj != null)
                {
                    Type t = targetobj.GetType();

                    PropertyInfo[] props = t.GetProperties();

                    foreach (var prop in props)
                    {
                        if (prop.Name == targetname)
                        {
                            value = prop.GetValue(targetobj);

                            retval = EventCodeEnum.NONE;
                            break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public static EventCodeEnum SetChildPropertyValue(object targetobj, string targetname, object value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (targetobj != null)
                {
                    Type t = targetobj.GetType();

                    PropertyInfo[] props = t.GetProperties();

                    // FullName 사용 시. Type 제외
                    if (targetname.Contains('.'))
                    {
                        targetname = targetname.Split('.').Last();
                    }

                    foreach (var prop in props)
                    {
                        if (prop.Name.ToUpper() == targetname.ToUpper())
                        {
                            // TODO : Enum 예외처리
                            // 변환 타입이 Enum이지만, 입력받은 값이 string인 경우 처리 가능.
                            if (prop.PropertyType.IsEnum == true && value.GetType().IsEnum == false)
                            {
                                var enumdata = System.Enum.Parse(prop.PropertyType, value.ToString(), true);
                                value = enumdata;
                            }

                            prop.SetValue(targetobj, Convert.ChangeType(value, prop.PropertyType), null);

                            retval = EventCodeEnum.NONE;
                            break;
                        }
                    }
                }

                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[MapDataConvert], SetPropertyValue() : Set value is faild. PropertyName = {targetname}, PropertyValue = {value}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public static EventCodeEnum SetPropertyValue(object targetobj, object value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (targetobj != null)
                {
                    targetobj = value;
                    //Type t = targetobj.GetType();

                    //PropertyInfo[] props = t.GetProperties();

                    //// FullName 사용 시. Type 제외
                    //if (targetname.Contains('.'))
                    //{
                    //    targetname = targetname.Split('.').Last();
                    //}

                    //foreach (var prop in props)
                    //{
                    //    if (prop.Name.ToUpper() == targetname.ToUpper())
                    //    {
                    //        // TODO : Enum 예외처리
                    //        // 변환 타입이 Enum이지만, 입력받은 값이 string인 경우 처리 가능.
                    //        if (prop.PropertyType.IsEnum == true && value.GetType().IsEnum == false)
                    //        {
                    //            var enumdata = System.Enum.Parse(prop.PropertyType, value.ToString(), true);
                    //            value = enumdata;
                    //        }

                    //        prop.SetValue(targetobj, Convert.ChangeType(value, prop.PropertyType), null);

                    //        retval = EventCodeEnum.NONE;
                    //        break;
                    //    }
                    //}
                }

                //if (retval != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Error($"[MapDataConvert], SetPropertyValue() : Set value is faild. PropertyName = {targetname}, PropertyValue = {value}");
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    // TODO : 데이터 정리
    // MapConverter : Header와 ResultMap을 이용
    // Scripter를 통해, 맵의 형태를 정의
    public abstract class MapConverterBase : IMapConverterBase
    {
        public abstract EventCodeEnum InitConverter(MapConverterParamBase param);
        public abstract EventCodeEnum ConvertMapDataFromBaseMap(MapHeaderObject _header, ResultMapData _resultMap, ref object _mapfile);
        public abstract EventCodeEnum ConvertMapDataToBaseMap(object _mapfile, ref MapHeaderObject _header, ref ResultMapData _resultMap);

        public abstract ResultMapConvertType ConverterType { get;}

        public MapConverterParamBase ConverterParam { get; set; }

        public MapHeaderObject HeaderObj { get; set; }
        public ResultMapData ResultMapObj { get; set; }
        public IMapScripter Scripter { get; set; }
        public object ResultMap { get; set; }

        public abstract object GetResultMap();

        // TODO : 삭제 
        public abstract char[,] GetASCIIMap();

        // TODO : 삭제
        public char[,] ASCIIMap { get; set; }

    }

    [Serializable]
    public abstract class MapConverterParamBase : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Element<FileReaderType> _readerType = new Element<FileReaderType>();
        public Element<FileReaderType> ReaderType
        {
            get { return _readerType; }
            set
            {
                if (value != _readerType)
                {
                    _readerType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _namerAlias = new Element<string>();
        public Element<string> NamerAlias
        {
            get { return _namerAlias; }
            set
            {
                if (value != _namerAlias)
                {
                    _namerAlias = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
