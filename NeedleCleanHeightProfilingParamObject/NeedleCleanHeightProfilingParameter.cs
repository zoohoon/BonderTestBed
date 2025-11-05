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

namespace NeedleCleanHeightProfilingParamObject
{
    [Serializable]
    public class PZErrorParameter : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public PZErrorParameter()
        {
            try
            {
            if (Positions == null)
            {
                Positions = new List<PinCoordinate>();
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private Element<double> _ZHeightOfPlane = new Element<double>();
        public Element<double> ZHeightOfPlane
        {
            get { return _ZHeightOfPlane; }
            set
            {
                if (value != _ZHeightOfPlane)
                {
                    _ZHeightOfPlane = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<PinCoordinate> _Positions = new List<PinCoordinate>();
        public List<PinCoordinate> Positions
        {
            get { return _Positions; }
            set
            {
                if (value != _Positions)
                {
                    _Positions = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class NeedleCleanHeightProfilingParameter : ISystemParameterizable, IParamNode, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public NeedleCleanHeightProfilingParameter()
        {

        }
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [ParamIgnore]
        public List<object> Nodes { get; set; }
        [ParamIgnore]
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

        [field: NonSerialized, JsonIgnore]
        [ParamIgnore]
        public string FilePath { get; } = "NeedleCleanModule";
        [field: NonSerialized, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "NeedleCleanHeightProfile.Json";

        private Element<bool> _Enable = new Element<bool>();
        public Element<bool> Enable
        {
            get { return _Enable; }
            set
            {
                if (value != _Enable)
                {
                    _Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MaxLimit = new Element<double>();
        public Element<double> MaxLimit
        {
            get { return _MaxLimit; }
            set
            {
                if (value != _MaxLimit)
                {
                    _MaxLimit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<PZErrorParameter> _ErrorTable = new List<PZErrorParameter>();
        public List<PZErrorParameter> ErrorTable
        {
            get { return _ErrorTable; }
            set
            {
                if (value != _ErrorTable)
                {
                    _ErrorTable = value;
                    RaisePropertyChanged();
                }
            }
        }


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
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                SetDefaultParam();

                

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                PZErrorParameter tmp = null;

                if (_ErrorTable == null)
                {
                    _ErrorTable = new List<PZErrorParameter>();
                }

                MaxLimit.Value = 20;

                //tmp = new PZErrorParameter();
                //tmp.ZHeightOfPlane.Value = -70000;

                //// Center Position = (0,0) , Up +, Right +, Width : 200,000, Height : 100,000

                //// Left Up
                //tmp.Positions.Add(new PinCoordinate(-100000, +50000, tmp.ZHeightOfPlane.Value));

                //// Left Down
                //tmp.Positions.Add(new PinCoordinate(-100000, -50000, tmp.ZHeightOfPlane.Value));

                //// Center
                //tmp.Positions.Add(new PinCoordinate(0, 0, tmp.ZHeightOfPlane.Value));

                //// Right UP
                //tmp.Positions.Add(new PinCoordinate(+100000, +50000, tmp.ZHeightOfPlane.Value));

                //// Right Down
                //tmp.Positions.Add(new PinCoordinate(+100000, -50000, tmp.ZHeightOfPlane.Value));

                //_ErrorTable.Add(tmp);

                //tmp = new PZErrorParameter();
                //tmp.ZHeightOfPlane.Value = -60000;

                //// Left Up
                //tmp.Positions.Add(new PinCoordinate(-100000, +50000, tmp.ZHeightOfPlane.Value));

                //// Left Down
                //tmp.Positions.Add(new PinCoordinate(-100000, -50000, tmp.ZHeightOfPlane.Value));

                //// Center
                //tmp.Positions.Add(new PinCoordinate(0, 0, tmp.ZHeightOfPlane.Value));

                //// Right UP
                //tmp.Positions.Add(new PinCoordinate(+100000, +50000, tmp.ZHeightOfPlane.Value));

                //// Right Down
                //tmp.Positions.Add(new PinCoordinate(+100000, -50000, tmp.ZHeightOfPlane.Value));

                //_ErrorTable.Add(tmp);

                tmp = new PZErrorParameter();
                tmp.ZHeightOfPlane.Value = -60000;

                // Center Position = (0,0) , Up +, Right +, Width : 200,000, Height : 100,000

                // Left Up
                tmp.Positions.Add(new PinCoordinate(-100000, +50000, 0));

                // Left Down
                tmp.Positions.Add(new PinCoordinate(-100000, -50000, 0));

                // Center
                tmp.Positions.Add(new PinCoordinate(0, 0, 0));

                // Right UP
                tmp.Positions.Add(new PinCoordinate(+100000, +50000, 0));

                // Right Down
                tmp.Positions.Add(new PinCoordinate(+100000, -50000, 0));

                _ErrorTable.Add(tmp);

                tmp = new PZErrorParameter();
                tmp.ZHeightOfPlane.Value = -10000;

                // Left Up
                tmp.Positions.Add(new PinCoordinate(-100000, +50000, 0));

                // Left Down
                tmp.Positions.Add(new PinCoordinate(-100000, -50000, 0));

                // Center
                tmp.Positions.Add(new PinCoordinate(0, 0, 0));

                // Right UP
                tmp.Positions.Add(new PinCoordinate(+100000, +50000, 0));

                // Right Down
                tmp.Positions.Add(new PinCoordinate(+100000, -50000, 0));

                _ErrorTable.Add(tmp);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
