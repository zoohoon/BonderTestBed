using LogModule;
using System;
using System.Collections.Generic;

namespace ProberInterfaces.WaferAlignEX
{
    using Newtonsoft.Json;
    using ProberInterfaces.Param;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Xml.Serialization;

    public enum HeightProfilignPosEnum
    {
        CENTER,
        LEFT,
        RIGHT,
        UPPER,
        BOTTOM,
        LEFTUPPER,
        RIGHTUPPER,
        LEFTBOTTOM,
        RIGHTBOTTOM,
        UNDEFINED
    }

    [Serializable]
    public class WAHeightPositionParams : INotifyPropertyChanged , IParamNode
    {
        [field:NonSerialized, JsonIgnore]
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

        public List<object> Nodes { get; set; }

        private ObservableCollection<WAHeightPositionParam> _HeightPosParams
            = new ObservableCollection<WAHeightPositionParam>();
        public ObservableCollection<WAHeightPositionParam> HeightPosParams
        {
            get { return _HeightPosParams; }
            set
            {
                if (value != _HeightPosParams)
                {
                    _HeightPosParams = value;
                    NotifyPropertyChanged("HeightPosParams");
                }
            }
        }
    }

    [Serializable]
    public class WAHeightPositionParam : INotifyPropertyChanged , IParamNode
    {
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

        public List<object> Nodes { get; set; }

        private WaferCoordinate _Position
             = new WaferCoordinate();
        public WaferCoordinate Position
        {
            get { return _Position; }
            set
            {
                if (value != _Position)
                {
                    _Position = value;
                    NotifyPropertyChanged("Position");
                }
            }
        }

        private MachineIndex _MIndex
             = new MachineIndex();
        public MachineIndex MIndex
        {
            get { return _MIndex; }
            set
            {
                if (value != _MIndex)
                {
                    _MIndex = value;
                    NotifyPropertyChanged("MIndex");
                }
            }
        }

        private HeightProfilignPosEnum _PosEnum;
        public HeightProfilignPosEnum PosEnum
        {
            get { return _PosEnum; }
            set
            {
                if (value != _PosEnum)
                {
                    _PosEnum = value;
                    NotifyPropertyChanged("PosEnum");
                }
            }
        }


        private ObservableCollection<LightValueParam> _LightParams;
        public ObservableCollection<LightValueParam> LightParams
        {
            get { return _LightParams; }
            set
            {
                if (value != _LightParams)
                {
                    _LightParams = value;
                    NotifyPropertyChanged("LightParams");
                }
            }
        }


        [NonSerialized]
        private double _HeightProfilingVal = -1;
        [ParamIgnore, JsonIgnore]
        public double HeightProfilingVal
        {
            get { return _HeightProfilingVal; }
            set
            {
                if (value != _HeightProfilingVal)
                {
                    _HeightProfilingVal = value;
                    NotifyPropertyChanged("HeightProfilingVal");
                }
            }
        }

        //private FocusParameter _FocusParam
        //     = new FocusParameter();
        //public FocusParameter FocusParam
        //{
        //    get { return _FocusParam; }
        //    set
        //    {
        //        if (value != _FocusParam)
        //        {
        //            _FocusParam = value;
        //            NotifyPropertyChanged("FocusParam");
        //        }
        //    }
        //}

        public WAHeightPositionParam()
        {

        }
        public WAHeightPositionParam(WaferCoordinate position)
        {
            this.Position = position;
        }
        public WAHeightPositionParam(WaferCoordinate position, HeightProfilignPosEnum heightProfilignPos)
        {
            try
            {
            this.Position = position;
            PosEnum = heightProfilignPos;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public WAHeightPositionParam(WaferCoordinate position, ObservableCollection<LightValueParam> lightParams)
        {
            try
            {
                this.Position = position;
                LightParams = new ObservableCollection<LightValueParam>();
                foreach (var light in lightParams)
                {
                    LightParams.Add(new LightValueParam(light.Type.Value, light.Value.Value));
                }
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WAHeightPositionParam(WaferCoordinate position, ObservableCollection<LightValueParam> lightParams, HeightProfilignPosEnum heightProfilignPos)
        {
            try
            {
                this.Position = position;
                LightParams = new ObservableCollection<LightValueParam>();
                foreach (var light in lightParams)
                {
                    LightParams.Add(new LightValueParam(light.Type.Value, light.Value.Value));
                }
                PosEnum = heightProfilignPos;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public WAHeightPositionParam(double xpos, double ypos)
        {
            try
            {
            Position.X.Value = xpos;
            Position.Y.Value = ypos;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public WAHeightPositionParam(double xpos, double ypos, HeightProfilignPosEnum heightProfilignPos)
        {
            try
            {
            Position.X.Value = xpos;
            Position.Y.Value = ypos;
            PosEnum = heightProfilignPos;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }


        public WAHeightPositionParam(WaferCoordinate position, MachineIndex mindex)
        {
            try
            {
            this.Position = position;
            this.MIndex = mindex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public WAHeightPositionParam(double xpos, double ypos, MachineIndex mindex)
        {
            try
            {
            Position.X.Value = xpos;
            Position.Y.Value = ypos;
            this.MIndex = mindex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public void CopyTo(WAHeightPositionParam target)
        {
            try
            {
            if (target.Position == null)
                target.Position = new WaferCoordinate();
            target.Position.X.Value = this.Position.GetX();
            target.Position.Y.Value = this.Position.GetY();
            target.Position.Z.Value = this.Position.GetZ();

            if (MIndex == null)
                MIndex = new MachineIndex();
            target.MIndex.XIndex = this.MIndex.XIndex;
            target.MIndex.YIndex = this.MIndex.YIndex;

            if (LightParams == null)
                LightParams = new ObservableCollection<LightValueParam>();
            foreach(var lparam in LightParams)
            {
                target.LightParams.Add(lparam);
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }
}
