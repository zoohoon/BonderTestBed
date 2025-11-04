using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CameraModule
{
    using LogModule;
    using ProberInterfaces;
    using System.Xml.Serialization;
    using ProberInterfaces.Vision;
    using ProberErrorCode;
    using VisionParams.Camera;
    using Newtonsoft.Json;

    [Serializable]
    [XmlInclude(typeof(CameraDescriptor))]
    public class CameraDescriptor : ICameraDescriptor, ISystemParameterizable, IParamNode
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
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

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

        [NonSerialized]
        private ObservableCollection<ICamera> _Cams
            = new ObservableCollection<ICamera>();
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public ObservableCollection<ICamera> Cams
        {
            get { return _Cams; }
            set
            {
                if (value != _Cams)
                {
                    _Cams = value;
                    NotifyPropertyChanged("Cams");
                }
            }
        }

        private ObservableCollection<CameraParameter> _CameraParams
             = new ObservableCollection<CameraParameter>();

        public ObservableCollection<CameraParameter> CameraParams
        {
            get { return _CameraParams; }
            set { _CameraParams = value; }
        }

        //   [NonSerialized]
        //   private Element<ObservableCollection<CameraParameter>> _CameraParameters
        //= new Element<ObservableCollection<CameraParameter>>();
        //   [ParamIgnore]

        //   public Element<ObservableCollection<CameraParameter>> CameraParameters
        //   {
        //       get { return _CameraParameters; }
        //       set { _CameraParameters = value; }
        //   }

        //ObservableCollection<CameraBase> _Cameras =
        //    new ObservableCollection<CameraBase>();

        //public ObservableCollection<CameraBase> Cameras
        //{
        //    get { return _Cameras; }
        //    set { _Cameras = value; }
        //}


        //[NonSerialized]
        //private VisionDigiParameters _DigiParam;
        //[XmlIgnore, JsonIgnore]
        //[ParamIgnore]
        //public VisionDigiParameters DigiParam
        //{
        //    get { return _DigiParam; }
        //    set { _DigiParam = value; }
        //}

        [XmlIgnore, JsonIgnore]
        public string FilePath { get; } = "Vision";
        [XmlIgnore, JsonIgnore]
        public string FileName { get; } = "CameraParameter.Json";

        //VisionType visiontype;
        //EnumGrabberType grabberType;

        //public void SetDigiParameter(VisionDigiParameters digiparams)
        //{
        //    DigiParam = digiparams;
        //}
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                SetEmulGige();
                //SetDefaultPramGigeBSCI1();
                //SetDefaultPramMorphis();
                //SetDefaultPramGigeVisionMapping();

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {



                // SetDefaultPramMorphis();
                // SetDefaultPramGige();
                //SetDefaultPramGigeVisionMapping();
                //SetDefaultPramGigeBSCI1();
                SetEmulGige();
                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        private void SetDefaultPramMorphis()
        {
            try
            {

                Array values = Enum.GetValues(typeof(EnumProberCam));
                int enumindex = 0;

                for (int index = 0; index < values.Length; index++)
                {
                    if ((EnumProberCam)values.GetValue(index) != EnumProberCam.INVALID & (EnumProberCam)values.GetValue(index) != EnumProberCam.UNDEFINED)
                    {
                        break;
                    }
                    enumindex++;
                }

                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILMORPHIS, 0, 0));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILMORPHIS, 0, 1));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILMORPHIS, 0, 2));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILMORPHIS, 0, 3));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILMORPHIS, 1, 0));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILMORPHIS, 1, 1));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILMORPHIS, 1, 2));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILMORPHIS, 1, 3));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILMORPHIS, 1, 4));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILMORPHIS, 1, 5));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILMORPHIS, 1, 6));
                #region // TIS
                //CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.TIS, 0, 0));
                //CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.TIS, 0, 0));
                //CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.TIS, 1, 0));
                //CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.TIS, 1, 0));
                //CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.TIS, 1, 0));
                //CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.TIS, 1, 0));
                //CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.TIS, 1, 0));
                //CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.TIS, 1, 0));
                //CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.TIS, 1, 0));
                //CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.TIS, 1, 0));
                //CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.TIS, 1, 0));

                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetDeafultTISLoader()
        {
            try
            {
                CameraParams.Clear();
                Array values = Enum.GetValues(typeof(EnumProberCam));
                int enumindex = 0;

                for (int index = 0; index < values.Length; index++)
                {
                    if ((EnumProberCam)values.GetValue(index) != EnumProberCam.INVALID & (EnumProberCam)values.GetValue(index) != EnumProberCam.UNDEFINED)
                    {
                        break;
                    }
                    enumindex++;
                }

                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILGIGE, 1, 0));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILGIGE, 2, 0));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILGIGE, 3, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PACL8_CAM, EnumGrabberRaft.TIS, 4, 0));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetDefaultPramGige()
        {
            try
            {
                Array values = Enum.GetValues(typeof(EnumProberCam));
                int enumindex = 0;

                for (int index = 0; index < values.Length; index++)
                {
                    if ((EnumProberCam)values.GetValue(index) != EnumProberCam.INVALID & (EnumProberCam)values.GetValue(index) != EnumProberCam.UNDEFINED)
                    {
                        break;
                    }
                    enumindex++;
                }

                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILGIGE, 1, 0));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILGIGE, 2, 0));
                CameraParams.Add(new CameraParameter((EnumProberCam)values.GetValue(enumindex++), EnumGrabberRaft.MILGIGE, 3, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PACL8_CAM, EnumGrabberRaft.TIS, 4, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PACL6_CAM, EnumGrabberRaft.TIS, 5, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.MAP_REF_CAM, EnumGrabberRaft.MILGIGE, 6, 0));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private void SetDefaultPramGigeVisionMapping()
        {
            try
            {
                Array values = Enum.GetValues(typeof(EnumProberCam));
                CameraParams.Add(new CameraParameter(EnumProberCam.WAFER_HIGH_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.WAFER_LOW_CAM, EnumGrabberRaft.MILGIGE, 1, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PIN_HIGH_CAM, EnumGrabberRaft.MILGIGE, 2, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PIN_LOW_CAM, EnumGrabberRaft.MILGIGE, 3, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PACL6_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PACL8_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PACL12_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.ARM_6_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.ARM_8_12_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.OCR1_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.OCR2_CAM, EnumGrabberRaft.MILGIGE, 0, 0));

                for (int mapIndex = 0; mapIndex < 6; mapIndex++)
                {
                    var digiNum = (int)EnumProberCam.MAP_1_CAM + mapIndex;
                    var camTag = (EnumProberCam)values.GetValue(digiNum);
                    CameraParams.Add(new CameraParameter(camTag, EnumGrabberRaft.MILGIGE, mapIndex + 7, 0));

                }
                CameraParams.Add(new CameraParameter(EnumProberCam.MAP_REF_CAM, EnumGrabberRaft.MILGIGE, 6, 0));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void SetDefaultPramGigeBSCI1()
        {
            try
            {
                Array values = Enum.GetValues(typeof(EnumProberCam));
                CameraParams.Add(new CameraParameter(EnumProberCam.WAFER_HIGH_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.WAFER_LOW_CAM, EnumGrabberRaft.MILGIGE, 1, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PIN_HIGH_CAM, EnumGrabberRaft.MILGIGE, 2, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PIN_LOW_CAM, EnumGrabberRaft.MILGIGE, 3, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PACL6_CAM, EnumGrabberRaft.TIS, 4, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PACL8_CAM, EnumGrabberRaft.TIS, 5, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PACL12_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.ARM_6_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.ARM_8_12_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.OCR1_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.MAP_REF_CAM, EnumGrabberRaft.MILGIGE, 6, 0));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetEmulGige()
        {
            try
            {
                Array values = Enum.GetValues(typeof(EnumProberCam));
                CameraParams.Add(new CameraParameter(EnumProberCam.WAFER_HIGH_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.WAFER_LOW_CAM, EnumGrabberRaft.MILGIGE, 1, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PIN_HIGH_CAM, EnumGrabberRaft.MILGIGE, 2, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PIN_LOW_CAM, EnumGrabberRaft.MILGIGE, 3, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PACL6_CAM, EnumGrabberRaft.MILMORPHIS, 4, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PACL8_CAM, EnumGrabberRaft.MILMORPHIS, 4, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.PACL12_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.ARM_6_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.ARM_8_12_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                CameraParams.Add(new CameraParameter(EnumProberCam.OCR1_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
                //CameraParams.Add(new CameraParameter(EnumProberCam.MAP_REF_CAM, EnumGrabberRaft.MILGIGE, 0, 0));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public string GetFilePath()
        {
            return FilePath;
        }

        public string GetFileName()
        {
            return FileName;
        }

        public CameraDescriptor()
        {
            //CameraParams = new ObservableCollection<CameraParameter>();
        }

    }
}
