using System;
using System.Collections.Generic;

namespace LightManager
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Xml.Serialization;


    [Serializable]
    public class LightParameter : ISystemParameterizable, IParamNode, IParam
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }

        

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                

                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        [XmlIgnore, JsonIgnore]
        public string FilePath { get; } = "LightParameter";
        [XmlIgnore, JsonIgnore]
        public string FileName { get; } = "LightChannelMapping.Json";
        [XmlIgnore, JsonIgnore]
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

        private List<LightChannel> _LightList = new List<LightChannel>();
        public List<LightChannel> LightList
        {
            get { return _LightList; }
            set { _LightList = value; }
        }

        
        private Element<int> _LightIntensityBitSize = new Element<int>();
        public Element<int> LightIntensityBitSize
        {
            get { return _LightIntensityBitSize; }
            set { _LightIntensityBitSize = value; }
        }


        public LightParameter()
        {

        }

        public LightParameter(int controllernum)
        {
            try
            {
            _LightIntensityBitSize.Value = 12;
            for (int i = 0; i < controllernum; i++)
            {
                _LightList.Add(new LightChannel(0, i));
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public List<LightChannel> GetLightChannels()
        {
            return LightList;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                LightList.Add(new LightChannel(0, 0));
                LightList.Add(new LightChannel(0, 1));
                LightList.Add(new LightChannel(0, 2));
                LightList.Add(new LightChannel(0, 3));
                LightList.Add(new LightChannel(0, 4));
                LightList.Add(new LightChannel(0, 5));
                LightList.Add(new LightChannel(0, 6));
                LightList.Add(new LightChannel(0, 7));

                LightList.Add(new LightChannel(1, 0));
                LightList.Add(new LightChannel(1, 1));
                LightList.Add(new LightChannel(1, 2));
                LightList.Add(new LightChannel(1, 3));
                LightList.Add(new LightChannel(1, 4));
                LightList.Add(new LightChannel(1, 5));
                LightList.Add(new LightChannel(1, 6));
                LightList.Add(new LightChannel(1, 7));

                LightIntensityBitSize.Value = 15;
                RetVal = EventCodeEnum.NONE;
            }
            catch(Exception err)
            {
                throw new Exception($"Error during Setting Default Param From LightParameter. {err.Message}");
            }
            return RetVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                //LightList.Add(new LightChannel(1, 2, 0)); //DeviceIndex, 
                //LightList.Add(new LightChannel(1, 2, 1));
                //LightList.Add(new LightChannel(1, 2, 2));
                //LightList.Add(new LightChannel(1, 2, 3));
                //LightList.Add(new LightChannel(1, 2, 4));
                //LightList.Add(new LightChannel(1, 2, 5));
                //LightList.Add(new LightChannel(1, 2, 6));
                //LightList.Add(new LightChannel(1, 2, 7));
                ////LightList.Add(new LightChannel(0,1));
                ////LightList.Add(new LightChannel(0,2));
                ////LightList.Add(new LightChannel(0,3));
                ////LightList.Add(new LightChannel(0,4));
                ////LightList.Add(new LightChannel(0,5));
                ////LightList.Add(new LightChannel(0,6));
                ////LightList.Add(new LightChannel(0,7));

                ////LightList.Add(new LightChannel(1, 0));
                ////LightList.Add(new LightChannel(1, 1));
                ////LightList.Add(new LightChannel(1, 2));
                ////LightList.Add(new LightChannel(1, 3));
                ////LightList.Add(new LightChannel(1, 4));
                ////LightList.Add(new LightChannel(1, 5));
                ////LightList.Add(new LightChannel(1, 6));
                ////LightList.Add(new LightChannel(1, 7));

                //LightIntensityBitSize.Value = 15;
                //RetVal = SetDefaultParamBSCI1();
                RetVal = SetDefaultLoaderOPUSV();
                //RetVal = SetDefaultParamOPUSV_Machine3();
            }
            catch(Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw new Exception("Error during Setting Default Param From LightParameter.");
            }
            return RetVal;
        }

        public EventCodeEnum SetDefaultLoaderOPUSV()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
            //LightList.Add(new LightChannel(0, 6, 0)); //DeviceIndex,                 //WaferLow
            //LightList.Add(new LightChannel(0, 6, 1)); //DeviceIndex,                 //WaferLow oblique
            //LightList.Add(new LightChannel(0, 6, 2)); //DeviceIndex,                 //OCR2
            //LightList.Add(new LightChannel(0, 6, 3)); //DeviceIndex,                 //OCR3
            //LightList.Add(new LightChannel(0, 6, 4)); //DeviceIndex,                 //WaferLow
            //LightList.Add(new LightChannel(0, 6, 5)); //DeviceIndex,                 //WaferHigh Oblique
            //LightList.Add(new LightChannel(0, 6, 6)); //DeviceIndex,                 //OCR2
            //LightList.Add(new LightChannel(0, 6, 7)); //DeviceIndex,                 //OCR3

            //// 8
            //LightList.Add(new LightChannel(0, 10, 1)); //DeviceIndex,                 //PA
            //LightList.Add(new LightChannel(0, 10, 0)); //DeviceIndex,                 //PA
            //LightList.Add(new LightChannel(0, 10, 2)); //DeviceIndex,                 //OCR1
            //LightList.Add(new LightChannel(0, 10, 2)); //DeviceIndex,                 //OCR2
            //LightList.Add(new LightChannel(0, 10, 2)); //DeviceIndex,                 //OCR3

            LightList.Add(new LightChannel(0, 6, 2)); //DeviceIndex,                 //WaferHigh
            LightList.Add(new LightChannel(0, 6, 1)); //DeviceIndex,                 //WaferHigh oblique
            LightList.Add(new LightChannel(0, 6, 0)); //DeviceIndex,                 //WaferLow
            LightList.Add(new LightChannel(0, 6, 3)); //DeviceIndex,                 //WaferLow Oblique
            LightList.Add(new LightChannel(0, 6, 7)); //DeviceIndex,                 //pinlow
            LightList.Add(new LightChannel(0, 6, 4)); //DeviceIndex,                 //pinhigh ref
            LightList.Add(new LightChannel(0, 6, 5)); //DeviceIndex,                 //pinhigh Oblique
            LightList.Add(new LightChannel(0, 6, 6)); //DeviceIndex,                 //pinlow oblique

            // 8
            LightList.Add(new LightChannel(0, 10, 1)); //DeviceIndex,                 //PA
            LightList.Add(new LightChannel(0, 10, 0)); //DeviceIndex,                 //PA
            LightList.Add(new LightChannel(0, 10, 2)); //DeviceIndex,                 //OCR1
            LightList.Add(new LightChannel(0, 10, 2)); //DeviceIndex,                 //OCR2
            LightList.Add(new LightChannel(0, 10, 2)); //DeviceIndex,                 //OCR3


            LightIntensityBitSize.Value = 15;
            ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return ret;
        }

        public EventCodeEnum SetDefaultParamBSCI1()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
            LightList.Add(new LightChannel(0, 7, 0)); //DeviceIndex,                 //WaferHigh            0
            LightList.Add(new LightChannel(0, 7, 1)); //DeviceIndex,                 //WaferHigh Oblique    1
            LightList.Add(new LightChannel(0, 7, 2)); //DeviceIndex,                 //WaferLow             2
            LightList.Add(new LightChannel(0, 7, 3)); //DeviceIndex,                 //WaferLow Oblique     3

            LightList.Add(new LightChannel(0, 7, 4)); //DeviceIndex,                 //PinLow Oblique       4
            LightList.Add(new LightChannel(0, 7, 5)); //DeviceIndex,                 //Mark                 5
            LightList.Add(new LightChannel(0, 7, 6)); //DeviceIndex,                 //PinHigh              6
            LightList.Add(new LightChannel(0, 7, 7)); //DeviceIndex,                 //PinLow               7

            LightList.Add(new LightChannel(0, 10, 0)); //DeviceIndex,                 //PA 8                8
            LightList.Add(new LightChannel(0, 10, 1)); //DeviceIndex,                 //PA 6                9

            LightIntensityBitSize.Value = 15;
            ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return ret;

        }

        public EventCodeEnum SetDefaultParamOPUSV_Machine3()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
            LightList.Add(new LightChannel(0, 7, 0)); //DeviceIndex,                 //WaferHigh            0
            LightList.Add(new LightChannel(0, 7, 1)); //DeviceIndex,                 //WaferHigh Oblique    1
            LightList.Add(new LightChannel(0, 7, 2)); //DeviceIndex,                 //WaferLow             2
            LightList.Add(new LightChannel(0, 7, 3)); //DeviceIndex,                 //WaferLow Oblique     3

            LightList.Add(new LightChannel(0, 7, 4)); //DeviceIndex,                 //PinLow Oblique       4
            LightList.Add(new LightChannel(0, 7, 5)); //DeviceIndex,                 //Mark                 5
            LightList.Add(new LightChannel(0, 7, 6)); //DeviceIndex,                 //PinHigh              6
            LightList.Add(new LightChannel(0, 7, 7)); //DeviceIndex,                 //PinLow               7

            LightList.Add(new LightChannel(0, 10, 0)); //DeviceIndex,                 //PA 8                8
            LightList.Add(new LightChannel(0, 10, 1)); //DeviceIndex,                 //PA 6                9

            LightIntensityBitSize.Value = 15;
            ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return ret;

        }
    }
}
