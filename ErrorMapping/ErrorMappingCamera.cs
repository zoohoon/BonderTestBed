using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ErrorMapping
{
    [Serializable]
    public class ErrorMappingCamera : INotifyPropertyChanged, IFactoryModule, IModule, IParam
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public bool Initialized { get; set; } = false;

        public ErrorMappingCamera()
        {
            try
            {
            _CamPos = new CatCoordinates();
            _VMPos_X = new CatCoordinates();
            _VMPos_Y = new CatCoordinates();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public ErrorMappingCamera(int camNum, CatCoordinates camPos, CatCoordinates vmPosX, CatCoordinates vmPosY, EnumProberCam camType)
        {
            try
            {
            _CamNumber = CamNumber;
            _CamPos = camPos;
            _VMPos_X = vmPosX;
            _VMPos_Y = vmPosY;
            _CamType = camType;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private IMotionManager MotionManager;

        private int _CamNumber;
        public int CamNumber
        {
            get { return _CamNumber; }
            set
            {
                if (value != _CamNumber)
                {
                    _CamNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _CamPos;
        public CatCoordinates CamPos
        {
            get { return _CamPos; }
            set
            {
                if (value != _CamPos)
                {
                    _CamPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _VMPos_X;
        public CatCoordinates VMPos_X
        {
            get { return _VMPos_X; }
            set
            {
                if (value != _VMPos_X)
                {
                    _VMPos_X = value;
                    RaisePropertyChanged();
                }
            }
        }
        private CatCoordinates _VMPos_Y;
        public CatCoordinates VMPos_Y
        {
            get { return _VMPos_Y; }
            set
            {
                if (value != _VMPos_Y)
                {
                    _VMPos_Y = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CatCoordinates _RelPos_X = new CatCoordinates();
        [XmlIgnore, JsonIgnore]
        public CatCoordinates RelPos_X
        {
            get { return _RelPos_X; }
            set
            {
                if (value != _RelPos_X)
                {
                    _RelPos_X = value;
                    RaisePropertyChanged();
                }
            }
        }
        private CatCoordinates _RelPos_Y = new CatCoordinates();
        [XmlIgnore, JsonIgnore]
        public CatCoordinates RelPos_Y
        {
            get { return _RelPos_Y; }
            set
            {
                if (value != _RelPos_Y)
                {
                    _RelPos_Y = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ErrorDataTable _ErrorDataX = new ErrorDataTable();
        [XmlIgnore, JsonIgnore]
        public ErrorDataTable ErrorDataX
        {
            get { return _ErrorDataX; }
            set
            {
                if (value != _ErrorDataX)
                {
                    _ErrorDataX = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ErrorDataTable _ErrorDataY = new ErrorDataTable();
        [XmlIgnore, JsonIgnore]
        public ErrorDataTable ErrorDataY
        {
            get { return _ErrorDataY; }
            set
            {
                if (value != _ErrorDataY)
                {
                    _ErrorDataY = value;
                    RaisePropertyChanged();
                }
            }
        }




        private EnumProberCam _CamType;
        public EnumProberCam CamType
        {
            get { return _CamType; }
            set
            {
                if (value != _CamType)
                {
                    _CamType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _MinXCnt;
        public int MinXCnt
        {
            get { return _MinXCnt; }
            set
            {
                if (value != _MinXCnt)
                {
                    _MinXCnt = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _MinYCnt;
        public int MinYCnt
        {
            get { return _MinYCnt; }
            set
            {
                if (value != _MinYCnt)
                {
                    _MinYCnt = value;
                    RaisePropertyChanged();
                }
            }
        }
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
         = new List<object>();

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    MotionManager = this.MotionManager();
                    retval = EventCodeEnum.NONE;

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public void DeInitModule()
        {
        }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }
    }
}
