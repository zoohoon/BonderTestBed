using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.PMI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace PMIModuleParameter
{
    [Serializable]
    public class PMITemplatePack : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        // Default 값으로만 쓰이는 Name
        private string _TemplateName;
        public string TemplateName
        {
            get { return _TemplateName; }
            set
            {
                _TemplateName = value;
                RaisePropertyChanged();
            }
        }

        private string _ImageSource;
        public string ImageSource
        {
            get { return _ImageSource; }
            set
            {
                _ImageSource = value;
                RaisePropertyChanged();
            }
        }


        private PAD_SHAPE _PadShape;
        public PAD_SHAPE PadShape
        {
            get { return _PadShape; }
            set
            {
                _PadShape = value;
                RaisePropertyChanged();
            }
        }
    }


    [Serializable]
    public class PMIModuleSysParam : ISystemParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public PMIModuleSysParam()
        {

        }

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; } = "PMIModule";
        public string FileName { get; } = "PMIModuleSysParameter.json";
        private string _Genealogy;
        public string Genealogy
        {
            get { return _Genealogy; }
            set { _Genealogy = value; }
        }
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

        #region param

        private bool _AutoThresholdEnable = new bool();
        public bool AutoThresholdEnable
        {
            get { return _AutoThresholdEnable; }
            set
            {
                if (value != _AutoThresholdEnable)
                {
                    _AutoThresholdEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ThresholdValue = new double();
        public double ThresholdValue
        {
            get { return _ThresholdValue; }
            set
            {
                if (value != _ThresholdValue)
                {
                    _ThresholdValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<PMITemplatePack> _TemplatePacklist = new ObservableCollection<PMITemplatePack>();
        public ObservableCollection<PMITemplatePack> TemplatePacklist
        {
            get { return _TemplatePacklist; }
            set
            {
                if (value != _TemplatePacklist)
                {
                    _TemplatePacklist = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _DoPMIDebugImages;
        public bool DoPMIDebugImages
        {
            get { return _DoPMIDebugImages; }
            set
            {
                if (value != _DoPMIDebugImages)
                {
                    _DoPMIDebugImages = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                MakeTemplatePacklist();
                
                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private void MakeTemplatePacklist()
        {
            try
            {
                if (TemplatePacklist == null)
                {
                    TemplatePacklist = new ObservableCollection<PMITemplatePack>();
                }

                TemplatePacklist.Clear();

                PMITemplatePack Rectangle = new PMITemplatePack();
                Rectangle.TemplateName = "Rectangle";
                Rectangle.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/rect-outline.png";
                Rectangle.PadShape = PAD_SHAPE.RECTANGLE;

                PMITemplatePack Circle = new PMITemplatePack();
                Circle.TemplateName = "Circle";
                Circle.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/circle-outline.png";
                Circle.PadShape = PAD_SHAPE.CIRCLE;

                PMITemplatePack Diamond = new PMITemplatePack();
                Diamond.TemplateName = "Diamond";
                Diamond.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/diamond-outline.png";
                Diamond.PadShape = PAD_SHAPE.DIAMOND;

                PMITemplatePack Oval = new PMITemplatePack();
                Oval.TemplateName = "Oval";
                Oval.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/oval-outline.png";
                Oval.PadShape = PAD_SHAPE.OVAL;

                PMITemplatePack Octagon = new PMITemplatePack();
                Octagon.TemplateName = "Octagon";
                Octagon.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/octagon-outline.png";
                Octagon.PadShape = PAD_SHAPE.OCTAGON;

                PMITemplatePack HalfOctagon = new PMITemplatePack();
                HalfOctagon.TemplateName = "Half-Octagon";
                HalfOctagon.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/halfoctagon-outline.png";
                HalfOctagon.PadShape = PAD_SHAPE.HALF_OCTAGON;

                PMITemplatePack RoundedRectangle = new PMITemplatePack();
                RoundedRectangle.TemplateName = "Rounded-Rectangle";
                RoundedRectangle.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/round-rect-outline.png";
                RoundedRectangle.PadShape = PAD_SHAPE.ROUNDED_RECTANGLE;

                TemplatePacklist.Add(Rectangle);
                TemplatePacklist.Add(Circle);
                TemplatePacklist.Add(Diamond);
                TemplatePacklist.Add(Oval);
                TemplatePacklist.Add(Octagon);
                TemplatePacklist.Add(HalfOctagon);
                TemplatePacklist.Add(RoundedRectangle);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetElementMetaData()
        {

        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                MakeTemplatePacklist();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }
            return retVal;
        }
    }
}
