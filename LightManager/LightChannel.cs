using System;
using System.Collections.Generic;

namespace LightManager
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;



    [Serializable]
    public class LightChannel : IParamNode, INotifyPropertyChanged, ILightChannel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
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
        public List<object> Nodes { get; set; }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public ILightDeviceControl LightIODevice { get; private set; }

        private Element<int> _DevIndex = new Element<int>();
        public Element<int> DevIndex
        {
            get { return _DevIndex; }
            set { _DevIndex = value; }
        }

        private Element<int> _Channel = new Element<int>();
        public Element<int> Channel
        {
            get { return _Channel; }
            set { _Channel = value; }
        }

        private Element<int> _Node = new Element<int>();
        public Element<int> Node
        {
            get { return _Node; }
            set { _Node = value; }
        }

        private int _MyProperty;

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public int CurLightValue
        {
            get { return _MyProperty; }
            set
            {
                if (value != _MyProperty)
                {
                    _MyProperty = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<int> _LUT;

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<int> LUT
        {
            get { return _LUT; }
            set
            {
                if (value != _LUT)
                {
                    _LUT = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _IOValue;

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public int IOValue
        {
            get { return _IOValue; }
            set
            {
                if (value != _IOValue)
                {
                    _IOValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private String _LightLookUpFilePath;

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public String LightLookUpFilePath
        {
            get { return _LightLookUpFilePath; }
            set
            {
                if (value != _LightLookUpFilePath)
                {
                    _LightLookUpFilePath = value;
                    RaisePropertyChanged();
                }
            }
        }
        //public int CurLightValue { get; set; }


        private int _LightMaxIntensity;
        //private String _LightLookUpFilePath;
        private IntListParam _LightLookUpTable;
        public string FrontSystemFilePath = @"C:\ProberSystem\Default\Parameters\SystemParam";


        public LightChannel()
        {

        }

        public LightChannel(int devIndx, int channel)
        {
            try
            {
                _DevIndex.Value = devIndx;
                _Channel.Value = channel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public LightChannel(int devIndx, int node, int channel)
        {
            try
            {
                _DevIndex.Value = devIndx;
                _Node.Value = node;
                _Channel.Value = channel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void InitLightController(ILightDeviceControl lightiodev, int lightMaxIntensity)
        {
            try
            {
                LightLookUpFilePath = FrontSystemFilePath + @"\LightParameter\" + "Light_LUT_D" + _DevIndex.Value.ToString() + "N" + _Node.Value.ToString() + "CH" + _Channel.Value.ToString() + ".json";
                //_LightLookUpFilePath = @"C:\ProberSystem\Parameters\SystemParam\LightParameter\" + "Light_LUT_D" + _DevIndex.ToString() + "N" + _Node.ToString() + "CH" + _Channel.ToString() + ".xml";
                LightIODevice = lightiodev;
                _LightMaxIntensity = lightMaxIntensity;
                InitLightLookUpTable(out _LightLookUpTable);
                LUT = _LightLookUpTable;
                if (CurLightValue != 0)
                {
                    IOValue = LUT[CurLightValue - 1];
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "LightChannel - InitLightController () : Error occurred.");
                LoggerManager.Exception(err);
            }

        }

        public void LoadLUT()
        {
            try
            {
                InitLightLookUpTable(out _LightLookUpTable);
                LUT = _LightLookUpTable;
                if (CurLightValue != 0)
                {
                    IOValue = LUT[CurLightValue - 1];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private int InitLightLookUpTable(out IntListParam lightLookUpTable)
        {
            int retVal = -1;
            EventCodeEnum saveResult = EventCodeEnum.NONE;

            try
            {
                //==> 하위 디렉터리까지 생성할 수 있음
                if (Directory.Exists(Path.GetDirectoryName(LightLookUpFilePath)) == false)
                    Directory.CreateDirectory(Path.GetDirectoryName(LightLookUpFilePath));

                //XmlSerializer serializer = new XmlSerializer(typeof(List<int>));
                //==> 디바이스 맵핑 파일이 없는 경우 기본 파일을 생성함
                if (File.Exists(LightLookUpFilePath) == false)
                {
                    IntListParam devconfig = new IntListParam();

                    for (int i = 0; i < 256; i++)
                        devconfig.Add(i * (_LightMaxIntensity / 255));

                    saveResult = Extensions_IParam.SaveParameter(null, devconfig, null, LightLookUpFilePath);
                }

                IParam tmpParam = null;
                Extensions_IParam.LoadParameter(null, ref tmpParam, typeof(IntListParam), null, LightLookUpFilePath, owner: this.Owner);

                if (saveResult == EventCodeEnum.NONE)
                {
                    lightLookUpTable = tmpParam as IntListParam;
                }
                else
                {
                    lightLookUpTable = null;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = -1;
                throw;
            }

            return retVal;
        }

        public int SetupLightLookUpTable(IntListParam setupLUT)
        {
            int retVal = -1;
            EventCodeEnum saveResult = EventCodeEnum.NONE;
            try 
            {
                //==> 디바이스 맵핑 파일이 없는 경우 기본 파일을 생성함
                saveResult = Extensions_IParam.SaveParameter(null, setupLUT, null, LightLookUpFilePath);
                _LightLookUpTable = setupLUT;
                LUT = _LightLookUpTable;
                if (CurLightValue != 0)
                {
                    IOValue = LUT[CurLightValue - 1];
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = -1;
                throw;
            }
            return retVal;
        }

        //==> 조명 세기값이 정상범위 밖에 값이 들어왔을 때는 조명을 최대치로 출력
        public void SetLight(int grayLevel, EnumProberCam camType, EnumLightType lightType = EnumLightType.UNDEFINED)
        {
            try
            {
                if (_LightLookUpTable == null)
                {
                    return;
                }

                if (grayLevel > _LightLookUpTable.Count - 1 || grayLevel < 0)
                {
                    grayLevel = _LightLookUpTable.Count - 1;
                }

                LightIODevice.SetLight(Node.Value, Channel.Value, _LightLookUpTable[grayLevel]);
                CurLightValue = grayLevel;

                VirtualStageConnector.VirtualStageConnector.Instance.SetLight(camType, lightType, grayLevel);

                if (CurLightValue != 0)
                {
                    IOValue = LUT[CurLightValue - 1];
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetLightNoLUT(int intensity)
        {
            try
            {
                if (intensity > _LightMaxIntensity || intensity < 0)
                    intensity = _LightMaxIntensity;

                LightIODevice.SetLight(Node.Value, Channel.Value, intensity);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public int GetLight()
        {
            return CurLightValue;
        }
    }
}
