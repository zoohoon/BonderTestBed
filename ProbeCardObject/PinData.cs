using LogModule;
using Newtonsoft.Json;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign.ProbeCardData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ProbeCardObject
{
    [Serializable]
    public class PinData : IPinData, INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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


        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        // 사용자가 처음 등록했던 위치로부터 변경된 총 위치 옵셋. 디바이스가 바뀌거나 핀을 재등록할 때 초기화한다.
        private PinCoordinate _AlignedOffset = new PinCoordinate();
        public PinCoordinate AlignedOffset
        {
            get
            {
                return _AlignedOffset;
            }
            set
            {
                //_UpdatePinsHistory.Add(value);
                _AlignedOffset = value;
            }
        }

        private PinCoordinate _LowCompensatedOffset = new PinCoordinate();
        public PinCoordinate LowCompensatedOffset
        {
            get
            {
                return _LowCompensatedOffset;
            }
            set
            {
                _LowCompensatedOffset = value;
            }
        }

        private PinCoordinate _MarkCumulativeCorrectionValue = new PinCoordinate();
        public PinCoordinate MarkCumulativeCorrectionValue
        {
            get
            {
                return _MarkCumulativeCorrectionValue;
            }
            set
            {
                _MarkCumulativeCorrectionValue = value;
            }
        }

        private PinCoordinate _AbsPosOrg = new PinCoordinate(0, 0, -9000);
        public PinCoordinate AbsPosOrg
        {
            get
            {
                return _AbsPosOrg;
            }
            set
            {
                //_UpdatePinsHistory.Add(value);
                _AbsPosOrg = value;
            }
        }

        [XmlIgnore, JsonIgnore]
        public PinCoordinate AbsPos
        {
            get
            {
                PinCoordinate tmpPos = new PinCoordinate();

                tmpPos.X.Value = _AbsPosOrg.X.Value + _AlignedOffset.X.Value;
                tmpPos.Y.Value = _AbsPosOrg.Y.Value + _AlignedOffset.Y.Value;
                tmpPos.Z.Value = _AbsPosOrg.Z.Value + _AlignedOffset.Z.Value;
                
                return tmpPos;
            }
            //set
            //{
            //    //_UpdatePinsHistory.Add(value);
            //    AbsPosOrg = value;
            //}
        }

        [XmlIgnore, JsonIgnore]

        private MachineCoordinate _MachineCoordPos = new MachineCoordinate();
        public MachineCoordinate MachineCoordPos
        {
            get
            {
                return _MachineCoordPos;
            }
            set
            {
                //_UpdatePinsHistory.Add(value);
                _MachineCoordPos = value;
            }
        }

        private ObservableCollection<PinCoordinate> _UpdatePinsHistory;
        [DataMember]
        public ObservableCollection<PinCoordinate> UpdatePinsHistory
        {
            get { return _UpdatePinsHistory; }
            set
            {
                if (value != _UpdatePinsHistory)
                {
                    _UpdatePinsHistory = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PinSearchParameter _PinSearchParam;
        [DataMember]
        public PinSearchParameter PinSearchParam
        {
            get { return _PinSearchParam; }
            set
            {
                if (value != _PinSearchParam)
                {
                    _PinSearchParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _IsAlignPin = new Element<bool>();
        [DataMember]
        public Element<bool> IsAlignPin
        {
            get
            {
                return _IsAlignPin;
            }
            set
            {
                _IsAlignPin = value;
            }
        }

        private Element<bool> _IsRefPin = new Element<bool>();
        [DataMember]
        public Element<bool> IsRefPin
        {
            get
            {
                return _IsRefPin;
            }
            set
            {
                _IsRefPin = value;
            }
        }

        private Element<bool> _IsRegisteredPin = new Element<bool>();
        [DataMember]
        public Element<bool> IsRegisteredPin
        {
            get
            {
                return _IsRegisteredPin;
            }
            set
            {
                _IsRegisteredPin = value;
            }
        }

        private Element<int> _GroupNum = new Element<int>();
        [DataMember]
        public Element<int> GroupNum
        {
            get
            {
                return _GroupNum;
            }
            set
            {
                _GroupNum = value;
            }
        }
        private Element<PINALIGNRESULT> _Result = new Element<PINALIGNRESULT>();
        [DataMember]
        public Element<PINALIGNRESULT> Result
        {
            get
            {
                return _Result;
            }
            set
            {
                _Result = value;
            }
        }
        
        private Element<PINALIGNRESULT> _PinTipResult = new Element<PINALIGNRESULT>();
        [DataMember]
        public Element<PINALIGNRESULT> PinTipResult
        {
            get
            {
                return _PinTipResult;
            }
            set
            {
                _PinTipResult = value;
            }
        }

        private Element<PINMODE> _PinMode = new Element<PINMODE>();
        [DataMember]
        public Element<PINMODE> PinMode
        {
            get
            {
                return _PinMode;
            }
            set
            {
                _PinMode = value;
            }
        }

        //private PinCoordinate _ThetaAbsPos;
        //[XmlIgnore, JsonIgnore]
        //public PinCoordinate ThetaAbsPos
        //{
        //    get
        //    {
        //        double degree = _RelPos.T.Value;
        //        double radian = Math.PI * degree / 180.0;
        //        double cosq = Math.Cos(radian);
        //        double sinq = Math.Sin(radian);
        //        double sx = _RelPos.X.Value;
        //        double sy = _RelPos.Y.Value;
        //        double rx = (sx * cosq - sy * sinq); // 결과 좌표 x
        //        double ry = (sx * sinq + sy * cosq); // 결과 좌표 y

        //        _ThetaAbsPos.X.Value = rx + DutInfo.RefCorner.X.Value;
        //        _ThetaAbsPos.Y.Value = ry + DutInfo.RefCorner.Y.Value;
        //        _ThetaAbsPos.Z.Value = _RelPos.Z.Value;
        //        _ThetaAbsPos.T.Value = _RelPos.T.Value;
        //        return _ThetaAbsPos;
        //    }
        //}

        private PinCoordinate _RelPos;
        [DataMember]
        public PinCoordinate RelPos
        {

            get { return _RelPos; }
            set
            {
                if (value != _RelPos)
                {
                    _RelPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _PinNum = new Element<int>();
        [DataMember]
        public Element<int> PinNum
        {
            get { return _PinNum; }
            set
            {
                if (value != _PinNum)
                {
                    _PinNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _DutNumber = new Element<int>();
        [DataMember]
        public Element<int> DutNumber
        {
            get { return _DutNumber; }
            set
            {
                if (value != _DutNumber)
                {
                    _DutNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<PinCoordinate> _DutLLconerPos = new Element<PinCoordinate>();
        [DataMember]
        public Element<PinCoordinate> DutLLconerPos
        {

            get { return _DutLLconerPos; }
            set
            {
                if (value != _DutLLconerPos)
                {
                    _DutLLconerPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<MachineIndex> _DutMacIndex = new Element<MachineIndex>();
        [DataMember]
        public Element<MachineIndex> DutMacIndex
        {
            get { return _DutMacIndex; }
            set
            {
                if (value != _DutMacIndex)
                {
                    _DutMacIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private MachineIndex _PadMachineIndex;
        [DataMember]
        public MachineIndex PadMachineIndex
        {
            get { return _PadMachineIndex; }
            set
            {
                if (value != _PadMachineIndex)
                {
                    _PadMachineIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<Guid> _PadGuid = new Element<Guid>();
        [DataMember]
        public Element<Guid> PadGuid
        {
            get { return _PadGuid; }
            set
            {
                if (value != _PadGuid)
                {
                    _PadGuid = value;
                    RaisePropertyChanged();
                }
            }
        }
        

        public PinData()
        {
            try
            {
                //_AbsPos = new PinCoordinate();
                //todo - must remove!! jake.
                //_AbsPos = new PinCoordinate(0, 0, -4000, 0);
                _PinNum.Value = new int();
                //_UpdatePins = new ObservableCollection<UpdatePinPos>();
                //_ThetaAbsPos = new PinCoordinate();
                //_UpdatePinsHistory = new ObservableCollection<PinCoordinate>();
                //_BelongPinGroup = new List<PinData>();
                _PinSearchParam = new PinSearchParameter();

                //_PinSizeX = _PinSizeX = 104;
                //_PinSizeY = _PinSizeY = 122;
                //_PinSizeWidth = _PinSizeWidth = 272;
                //_PinSizeHeight = _PinSizeHeight = 236;

                //_PinRoiX = _PinSizeX = 104;
                //_PinRoiY = _PinSizeY = 122;
                //_PinRoiWidth = _PinSizeWidth = 272;
                //_PinRoiHeight = _PinSizeHeight = 236;
                _PadGuid.Value = new Guid();
                _PadMachineIndex = new MachineIndex();
                _DutLLconerPos = new Element<PinCoordinate>();

                _DutMacIndex = new Element<MachineIndex>();

                _DutLLconerPos.Value = new PinCoordinate();
                _DutMacIndex.Value = new MachineIndex();


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public PinData(Dut dut, PinCoordinate orgPin) : this()
        //{
        //    _DutInfo = dut;
        //    _RelPos = orgPin;
        //}

        public PinData(IPinData pinData) : this()
        {
            try
            {
                _PinSearchParam = new PinSearchParameter();
                _PadGuid.Value = new Guid();
                _PadMachineIndex = new MachineIndex();
                _DutLLconerPos = new Element<PinCoordinate>();
                _DutMacIndex = new Element<MachineIndex>();

                if (pinData.AbsPosOrg != null)
                {
                    _AbsPosOrg = new PinCoordinate(pinData.AbsPosOrg.X.Value, pinData.AbsPosOrg.Y.Value, pinData.AbsPosOrg.Z.Value);
                }

                if (pinData.AlignedOffset != null)
                {
                    _AlignedOffset = new PinCoordinate(pinData.AlignedOffset.X.Value, pinData.AlignedOffset.Y.Value, pinData.AlignedOffset.Z.Value);
                }

                // TODO : 추가 해야 됨?
                // 1. _GroupNum.Value = pinData.GroupNum.value;
                // 2. Result
                // 3. PinMode
                // 4. DutMacIndex

                //if (pinData.AbsPos != null)
                //{
                //    AbsPos = new PinCoordinate(pinData.AbsPos);
                //}

                if (pinData.UpdatePinsHistory != null)
                {
                    _UpdatePinsHistory = new ObservableCollection<PinCoordinate>();
                    foreach (var pin in pinData.UpdatePinsHistory)
                        _UpdatePinsHistory.Add(new PinCoordinate(pin));
                }

                //if (pinData.UpdatePins != null)
                //{
                //    _UpdatePins = new ObservableCollection<UpdatePinPos>();
                //    foreach (var pin in pinData.UpdatePins)
                //        _UpdatePins.Add(new UpdatePinPos(pin));
                //}

                if (pinData.RelPos != null)
                {
                    _RelPos = new PinCoordinate(pinData.RelPos);
                }

                //if (pinData.UpdatePins != null)
                //{
                //    foreach (var pin in pinData.UpdatePins)
                //        _UpdatePins.Add(new UpdatePinPos(pin));
                //}

                //if (pinData.DutInfo != null)
                //{
                //    _DutInfo = pinData.DutInfo;
                //}

                if (pinData.PinSearchParam != null)
                {
                    _PinSearchParam = new PinSearchParameter(pinData.PinSearchParam);
                }

                //_BelongPinGroup = pinData.BelongPinGroup;

                //_Result = pinData.Result;
                //_PinSizeX = pinData._PinSizeX;
                //_PinSizeY = pinData._PinSizeY;
                //_PinSizeWidth = pinData.PinSizeWidth;
                //_PinRoiHeight = pinData.PinRoiHeight;
                //_PinRoiX = pinData.PinRoiX;
                //_PinRoiY = pinData.PinRoiY;
                //_PinRoiWidth = pinData.PinRoiWidth;
                //_PinRoiHeight = pinData.PinRoiHeight;
                _PinNum.Value = pinData.PinNum.Value;
                _PadGuid.Value = pinData.PadGuid.Value;
                _PadMachineIndex = pinData.PadMachineIndex;
                _IsAlignPin.Value = pinData.IsAlignPin.Value;
                _IsRefPin.Value = pinData.IsRefPin.Value;
                _IsRegisteredPin.Value = pinData.IsRegisteredPin.Value;

                _DutLLconerPos = new Element<PinCoordinate>();

                if (pinData.DutLLconerPos != null)
                {
                    if (pinData.DutLLconerPos.Value != null)
                    {
                        if (_DutLLconerPos.Value == null) _DutLLconerPos.Value = new PinCoordinate();
                        _DutLLconerPos.Value.X.Value = pinData.DutLLconerPos.Value.X.Value;
                        _DutLLconerPos.Value.Y.Value = pinData.DutLLconerPos.Value.Y.Value;
                        _DutLLconerPos.Value.Z.Value = pinData.DutLLconerPos.Value.Z.Value;
                    }
                }
                if (pinData.DutLLconerPos != null)
                {
                    if (pinData.DutMacIndex.Value != null)
                    {
                        if (_DutMacIndex.Value == null) _DutMacIndex.Value = new MachineIndex();
                        _DutMacIndex.Value.XIndex = pinData.DutMacIndex.Value.XIndex;
                        _DutMacIndex.Value.YIndex = pinData.DutMacIndex.Value.YIndex;
                    }
                }
                _DutNumber.Value = pinData.DutNumber.Value;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //public PinData(PinData pinData) : this()
        //{
        //    if (pinData.AbsPos != null)
        //    {
        //        _AbsPos = new PinCoordinate(pinData.AbsPos);
        //    }

        //    if (pinData.UpdatePinsHistory != null)
        //    {
        //        _UpdatePinsHistory = new ObservableCollection<PinCoordinate>();
        //        foreach (var pin in pinData.UpdatePinsHistory)
        //            _UpdatePinsHistory.Add(new PinCoordinate(pin));
        //    }

        //    //if (pinData.UpdatePins != null)
        //    //{
        //    //    _UpdatePins = new ObservableCollection<UpdatePinPos>();
        //    //    foreach (var pin in pinData.UpdatePins)
        //    //        _UpdatePins.Add(new UpdatePinPos(pin));
        //    //}

        //    if (pinData.RelPos != null)
        //    {
        //        _RelPos = new PinCoordinate(pinData.RelPos);
        //    }

        //    //if (pinData.UpdatePins != null)
        //    //{
        //    //    foreach (var pin in pinData.UpdatePins)
        //    //        _UpdatePins.Add(new UpdatePinPos(pin));
        //    //}

        //    if (pinData.DutInfo != null)
        //    {
        //        _DutInfo = pinData.DutInfo;
        //    }

        //    if (pinData.PinSearchParam != null)
        //    {
        //        _PinSearchParam = new PinSearchParameter(pinData.PinSearchParam);
        //    }

        //    //_BelongPinGroup = pinData.BelongPinGroup;

        //    //_Result = pinData.Result;
        //    //_PinSizeX = pinData._PinSizeX;
        //    //_PinSizeY = pinData._PinSizeY;
        //    //_PinSizeWidth = pinData.PinSizeWidth;
        //    //_PinRoiHeight = pinData.PinRoiHeight;
        //    //_PinRoiX = pinData.PinRoiX;
        //    //_PinRoiY = pinData.PinRoiY;
        //    //_PinRoiWidth = pinData.PinRoiWidth;
        //    //_PinRoiHeight = pinData.PinRoiHeight;
        //    _SerialNum.Value = pinData.SerialNum.Value;
        //    _PadGuid.Value = pinData._PadGuid.Value;
        //    _PadMachineIndex = pinData._PadMachineIndex;
        //    _IsAlignPin.Value = pinData._IsAlignPin.Value;
        //    _IsRefPin.Value = pinData._IsRefPin.Value;
        //    _IsRegistredPin.Value = pinData._IsRegistredPin.Value;
        //}
    }

}
