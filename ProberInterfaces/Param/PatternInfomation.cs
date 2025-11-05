using LogModule;

using System.Collections.Generic;

namespace ProberInterfaces.Param
{
    using Newtonsoft.Json;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Vision;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable,DataContract]
    public class PatternInfomation : CatCoordinates, INotifyPropertyChanged, IParamNode
    {
        [XmlIgnore, JsonIgnore]
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
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
        public new List<object> Nodes { get; set; }


        private Element<EnumProberCam> _CamType
            = new Element<EnumProberCam>();
        [DataMember]
        public Element<EnumProberCam> CamType
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

        private PMParameter _PMParameter = new PMParameter();
        [DataMember]
        public PMParameter PMParameter
        {
            get { return _PMParameter; }
            set
            {
                if (value != _PMParameter)
                {
                    _PMParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<LightValueParam> _LightParams;
        [DataMember]
        public ObservableCollection<LightValueParam> LightParams
        {
            get { return _LightParams; }
            set
            {
                if (value != _LightParams)
                {
                    _LightParams = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 패턴 등록 당시 화면의 GrayLevel 
        /// </summary>
        private int _GrayLevel;
        [DataMember]
        public int GrayLevel
        {
            get { return _GrayLevel; }
            set
            {
                if (value != _GrayLevel)
                {
                    _GrayLevel = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<PatternStateEnum> _PatternState = new Element<PatternStateEnum>();
        [DataMember]
        public Element<PatternStateEnum> PatternState
        {
            get { return _PatternState; }
            set
            {
                if (value != _PatternState)
                {
                    _PatternState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ImageBuffer _Imagebuffer;
        [JsonIgnore]
        public ImageBuffer Imagebuffer
        {
            get { return _Imagebuffer; }
            set
            {
                if (value != _Imagebuffer)
                {
                    _Imagebuffer = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Processing 성공시에 전체 화면에 대한 GrayLevel 값.
        /// 다음에 실패했을 때 해당 GrayLevel 로 조명을 맞추기 위한 목적
        /// </summary>
        [NonSerialized]
        private int _SuccessVisionGrayLevel = 0;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public int SuccessVisionGrayLevel
        {
            get { return _SuccessVisionGrayLevel; }
            set
            {
                if (value != _SuccessVisionGrayLevel)
                {
                    _SuccessVisionGrayLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private ObservableCollection<LightValueParam> _SuccessVisionLightParams
            = new ObservableCollection<LightValueParam>();
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public ObservableCollection<LightValueParam> SuccessVisionLightParams
        {
            get { return _SuccessVisionLightParams; }
            set
            {
                if (value != _SuccessVisionLightParams)
                {
                    _SuccessVisionLightParams = value;
                    RaisePropertyChanged();
                }
            }
        }




        public PatternInfomation(string path)
        {
            PMParameter.ModelFilePath.Value = path;
        }
            public PatternInfomation(double x, double y)
        {
            try
            {
            this.X.Value = x;
            this.Y.Value = y;
            this.Z.Value = 0;
            this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public PatternInfomation(double x, double y, PMParameter pmparam)
        {
            try
            {
            this.X.Value = x;
            this.Y.Value = y;
            this.Z.Value = 0;
            this.T.Value = 0;
            PMParameter = pmparam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public PatternInfomation(double x, double y, PMParameter pmparam , string path)
        {
            try
            {
            this.X.Value = x;
            this.Y.Value = y;
            this.Z.Value = 0;
            this.T.Value = 0;
            PMParameter = pmparam;
            PMParameter.ModelFilePath.Value = path;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public PatternInfomation(double x, double y, PMParameter pmparam, string path,EnumProberCam camtype)
        {
            try
            {
            this.X.Value = x;
            this.Y.Value = y;
            this.Z.Value = 0;
            this.T.Value = 0;
            PMParameter = pmparam;
            PMParameter.ModelFilePath.Value = path;
            CamType.Value = camtype;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public PatternInfomation(double x, double y, PMParameter pmparam,
            string path, EnumProberCam camtype, ObservableCollection<LightValueParam> lightparam)
        {
            this.X.Value = x;
            this.Y.Value = y;
            this.Z.Value = 0;
            this.T.Value = 0;

            PMParameter = pmparam;
            PMParameter.ModelFilePath.Value = path;
            CamType.Value = camtype;
            this.LightParams = lightparam;
        }
        public PatternInfomation(double x, double y, PMParameter pmparam,
            string path,string extenstion, EnumProberCam camtype, ObservableCollection<LightValueParam> lightparam)
        {
            this.X.Value = x;
            this.Y.Value = y;
            this.Z.Value = 0;
            this.T.Value = 0;

            if (PMParameter != null)
                PMParameter = pmparam;
           
            PMParameter.ModelFilePath.Value = path;
            PMParameter.PatternFileExtension.Value = extenstion;
            CamType.Value = camtype;
            this.LightParams = lightparam;
        }

        public PatternInfomation(string path, double x, double y)
        {
            try
            {
            PMParameter.ModelFilePath.Value = path;
            this.X.Value = x;
            this.Y.Value = y;
            this.Z.Value = 0;
            this.T.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }


        public PatternInfomation(double x, double y, double z, double t)
        {
            try
            {
            this.X.Value = x;
            this.Y.Value = y;
            this.Z.Value = z;
            this.T.Value = t;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public PatternInfomation(EnumProberCam camtype, PMParameter pmparam)
        {
            try
            {
            this.CamType.Value = camtype;
            this.PMParameter = pmparam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public PatternInfomation(EnumProberCam camtype, PMParameter pmparam, string pmpath)
        {
            try
            {
            this.CamType.Value = camtype;
            this.PMParameter = pmparam;
            this.PMParameter.ModelFilePath.Value = pmpath;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public PatternInfomation(PatternInfomation ptinfo)
        {
            try
            {
            this.X.Value = ptinfo.X.Value;
            this.Y.Value = ptinfo.Y.Value;
            this.Z.Value = ptinfo.Z.Value;
            this.T.Value = ptinfo.T.Value;

            this.PMParameter.ModelFilePath = ptinfo.PMParameter.ModelFilePath;
            this.LightParams = ptinfo.LightParams;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public PatternInfomation()
        {
        }
    }

   
}
