using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign.ProbeCardData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using static ProberInterfaces.Param.PinSearchParameter;

namespace ProbeCardObject
{
    [Serializable]
    public class ProbeCardDevObject : IProbeCardDevObject, INotifyPropertyChanged
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

        #region ==> DeviceParameterizable
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FilePath { get; } = "";

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "ProbeCardParameter.Json";

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        [XmlIgnore, JsonIgnore]
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
        #endregion

        #region ==> Property


        #region ==> DutIndexSizeX
        private int _DutIndexSizeX;
        public int DutIndexSizeX
        {
            get { return _DutIndexSizeX; }
            set { _DutIndexSizeX = value; }
        }
        #endregion

        #region ==> DutIndexSizeY 
        private int _DutIndexSizeY;
        public int DutIndexSizeY
        {
            get { return _DutIndexSizeY; }
            set { _DutIndexSizeY = value; }
        }
        #endregion

        // 등록된 핀의 무게 중심. 핀 등록시, 디바이스 로딩 시, 핀 얼라인 시 다시 계산한다.
        #region ==> PinCenX    
        private double _PinCenX = new double();
        public double PinCenX
        {
            get { return _PinCenX; }
            set { _PinCenX = value; }
        }
        #endregion

        #region ==> PinCenY    
        private double _PinCenY = new double();
        public double PinCenY
        {
            get { return _PinCenY; }
            set { _PinCenY = value; }
        }
        #endregion

        // 현재 더트의 무게 중심. 프로빙 용도로는 쓰지 않는다. 니들 클리닝이나 폴리쉬 웨이퍼 등에서만 사용한다. 핀 센터와 동일한 타이밍이 같이 업데이트 한다.
        #region ==> DutCenX    
        private double _DutCenX = new double();
        public double DutCenX
        {
            get { return _DutCenX; }
            set { _DutCenX = value; }
        }
        #endregion

        #region ==> DutCenY    
        private double _DutCenY = new double();
        public double DutCenY
        {
            get { return _DutCenY; }
            set { _DutCenY = value; }
        }
        #endregion

        #region ==> DutSizeX
        private Element<double> _DutSizeX = new Element<double>();
        public Element<double> DutSizeX
        {
            get
            {
                if (_DutSizeX == null || _DutSizeX.Value < 1)
                {
                    _DutSizeX.Value = (double)this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeX.GetValue();
                }
                return _DutSizeX;
            }
            set
            {
                if (value != _DutSizeX)
                {
                    _DutSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> DutSizeY    
        private Element<double> _DutSizeY = new Element<double>();
        public Element<double> DutSizeY
        {
            get
            {
                if (_DutSizeY == null || _DutSizeY.Value < 1)
                {
                    _DutSizeY.Value = (double)this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeY.GetValue();
                }
                return _DutSizeY;
            }
            set
            {
                if (value != _DutSizeY)
                {
                    _DutSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ProbeCardCenterOffset
        private PinCoordinate _ProbeCardCenterOffset = new PinCoordinate();
        public PinCoordinate ProbeCardCenterOffset
        {
            get
            {
                return _ProbeCardCenterOffset;
            }
            set
            {
                if (value != _ProbeCardCenterOffset)
                {
                    _ProbeCardCenterOffset = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ProbeCardID
        private Element<string> _ProbeCardID = new Element<string>(); //안씀...
        public Element<string> ProbeCardID
        {
            get { return _ProbeCardID; }
            set
            {
                if (value != _ProbeCardID)
                {
                    _ProbeCardID = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region CardType
        // 현재 카드의 타입
        private Element<PROBECARD_TYPE> _ProbeCardType;
        public Element<PROBECARD_TYPE> ProbeCardType
        {
            get { return _ProbeCardType; }
            set
            {
                if (value != _ProbeCardType)
                {
                    _ProbeCardType = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region LowAlignKey
        private List<AlignKeyInfo> _AlignKeyLow
             = new List<AlignKeyInfo>();
        public List<AlignKeyInfo> AlignKeyLow
        {
            get { return _AlignKeyLow; }
            set
            {
                if (value != _AlignKeyLow)
                {
                    _AlignKeyLow = value;
                    RaisePropertyChanged();
                }
            }
        }

        // 패턴들의 무게 중심과 더트 중심과의 상대 거리
        private Element<double> _AlignKeyLowCenX = new Element<double>();
        public Element<double> AlignKeyLowCenX
        {
            get { return _AlignKeyLowCenX; }
            set { _AlignKeyLowCenX = value; }
        }
        private Element<double> _AlignKeyLowCenY = new Element<double>();
        public Element<double> AlignKeyLowCenY
        {
            get { return _AlignKeyLowCenY; }
            set { _AlignKeyLowCenY = value; }
        }
        private Element<double> _AlignKeyLowCenZ = new Element<double>();
        public Element<double> AlignKeyLowCenZ
        {
            get { return _AlignKeyLowCenZ; }
            set { _AlignKeyLowCenZ = value; }
        }

        // Low Align 키 등록당시 DUT 각도
        private Element<double> _AlignKeyLowAngle = new Element<double>();
        public Element<double> AlignKeyLowAngle
        {
            get { return _AlignKeyLowAngle; }
            set { _AlignKeyLowAngle = value; }
        }

        //Low Key - Ref Pin Z offset
        private Element<double> _LowKeyFormPinZoffet = new Element<double>() { Value = 0};

        public Element<double> LowAlignKeyFormPinZoffet
        {
            get { return _LowKeyFormPinZoffet; }
            set { _LowKeyFormPinZoffet = value; }
        }

        #endregion

        private Element<int> _TouchdownCount = new Element<int>() { Value = 0 };

        public Element<int> TouchdownCount
        {
            get { return _TouchdownCount; }
            set { _TouchdownCount = value; }
        }


        #region ==> DutList
        private AsyncObservableCollection<IDut> _DutList = new AsyncObservableCollection<IDut>();
        public AsyncObservableCollection<IDut> DutList
        {
            get { return _DutList; }
            set
            {
                if (value != _DutList)
                {
                    _DutList = value;
                    UpdateDutList();
                    RaisePropertyChanged();
                }
            }
        }

        private void UpdateDutList()
        {
            if (DutList.Count < 1)
            {
                DutIndexSizeX = 0;
                DutIndexSizeY = 0;
                return;
            }

            long minX = DutList.Min(x => x.MacIndex.XIndex);
            long maxX = DutList.Max(x => x.MacIndex.XIndex);
            long minY = DutList.Min(y => y.MacIndex.YIndex);
            long maxY = DutList.Max(y => y.MacIndex.YIndex);

            DutIndexSizeX = (int)(maxX - minX + 1);
            DutIndexSizeY = (int)(maxY - minY + 1);
        }
        #endregion

        #region ==> PinGroupList
        private List<IGroupData> _PinGroupList = new List<IGroupData>();
        public List<IGroupData> PinGroupList
        {
            get { return _PinGroupList; }
            set { _PinGroupList = value; }
        }
        #endregion       

        #region ==> HighAlignkeyOffsetX
        private Element<double> _HighAlignkeyOffsetX;
        public Element<double> HighAlignkeyOffsetX
        {
            get { return _HighAlignkeyOffsetX; }
            set
            {
                if (value != _HighAlignkeyOffsetX)
                {
                    _HighAlignkeyOffsetX = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> HighAlignkeyOffsetY
        private Element<double> _HighAlignkeyOffsetY;
        public Element<double> HighAlignkeyOffsetY
        {
            get { return _HighAlignkeyOffsetY; }
            set
            {
                if (value != _HighAlignkeyOffsetY)
                {
                    _HighAlignkeyOffsetY = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> HighAlignkeyOffsetZ
        private Element<double> _HighAlignkeyOffsetZ;
        public Element<double> HighAlignkeyOffsetZ
        {
            get { return _HighAlignkeyOffsetZ; }
            set
            {
                if (value != _HighAlignkeyOffsetZ)
                {
                    _HighAlignkeyOffsetZ = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PinHeight
        private double _PinHeight;
        public double PinHeight
        {
            get
            {
                return _PinHeight;
            }
            set
            {
               _PinHeight = value;
            }
        }
        #endregion

        #region ==> Ref Pin Serialnum
        private Element<int> _RefPinNum = new Element<int>();
        public Element<int> RefPinNum
        {
            get { return _RefPinNum; }
            set
            {
                if (value != _RefPinNum)
                {
                    _RefPinNum = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        //==> Probe Card HW Spec, PinAlign 할때마다 갱신

        #region ==> DiffAngle
        private double _DiffAngle;
        public double DiffAngle
        {
            get { return _DiffAngle; }
            set
            {
                if (value != _DiffAngle)
                {
                    _DiffAngle = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> DiffX
        private double _DiffX;
        public double DiffX
        {
            get { return _DiffX; }
            set
            {
                if (value != _DiffX)
                {
                    _DiffX = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> DiffY
        private double _DiffY;
        public double DiffY
        {
            get { return _DiffY; }
            set
            {
                if (value != _DiffY)
                {
                    _DiffY = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        #region ==> DutAngle
        private double _DutAngle;
        public double DutAngle
        {
            get { return _DutAngle; }
            set
            {
                if (value != _DutAngle)
                {
                    _DutAngle = value;
                }
            }
        }
        #endregion

        #region ==> DutDiffX
        private double _DutDiffX;
        public double DutDiffX
        {
            get { return _DutDiffX; }
            set
            {
                if (value != _DutDiffX)
                {
                    _DutDiffX = value;
                }
            }
        }
        #endregion

        #region ==> DutDiffY
        private double _DutDiffY;
        public double DutDiffY
        {
            get { return _DutDiffY; }
            set
            {
                if (value != _DutDiffY)
                {
                    _DutDiffY = value;
                }
            }
        }
        #endregion

        #region ==> PinDefaultHeight
        private Element<double> _PinDefaultHeight = new Element<double>();
        public Element<double> PinDefaultHeight
        {
            get { return _PinDefaultHeight; }
            set
            {
                if (value != _PinDefaultHeight)
                {
                    _PinDefaultHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

       

        


        #endregion


        #region CardLibraryData        
        // High 카메라에서 실제 핀 대신 사용할 패턴 리스트
        private List<AlignKeyLibPack> _AlignKeyHighLib = new List<AlignKeyLibPack>();
        public List<AlignKeyLibPack> AlignKeyHighLib
        {
            get { return _AlignKeyHighLib; }
            set
            {
                if (value != _AlignKeyHighLib)
                {
                    _AlignKeyHighLib = value;
                }
            }
        }

        // High 카메라에서 실제 핀 대신 사용할 패턴 리스트
        private List<AlignKeyLibPack> _AlignKeyLowLib = new List<AlignKeyLibPack>();
        public List<AlignKeyLibPack> AlignKeyLowLib
        {
            get { return _AlignKeyLowLib; }
            set
            {
                if (value != _AlignKeyLowLib)
                {
                    _AlignKeyLowLib = value;
                }
            }
        }
        #endregion



        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if(this.ProbeCardType == null)
                {
                    ProbeCardType = new Element<PROBECARD_TYPE>();
                    ProbeCardType.Value = PROBECARD_TYPE.Cantilever_Standard;
                }
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                //DutSizeX.Value = 10;
                //DutSizeY.Value = 10;

                Dut dut1 = new Dut(new MachineIndex(0, 0));
                dut1.UserIndex = new UserIndex(0, 0);
                dut1.DutEnable = true;
                //dut1.PinList = new List<PinData>();
                dut1.PinList = new List<IPinData>();
                dut1.RefCorner = new PinCoordinate(-4254, 7975, -11488);
                dut1.DutNumber = 1;
                DutIndexSizeX = 1;
                DutIndexSizeY = 1;

                DutList.Add(dut1);

                GroupData groupData = new GroupData();
                groupData.PinNumList.Add(0);
                groupData.GroupResult = PINGROUPALIGNRESULT.CONTINUE;
                PinGroupList.Add(groupData);

                ProbeCardType = new Element<PROBECARD_TYPE>();

                //PinPadMatchTolerenceX.Value = 20;
                //PinPadMatchTolerenceY.Value = 20;
                //PinPadOptimizeAngleLimit.Value = 1.5;

                //SettingAlignKeyHighLib();

                #region ==> Default

                /*
                _XSize.Value = 1;
                _YSize.Value = 1;
                _SizeNum.Value = 1;
                _Location.Value = "Loc";
                _CardID.Value = "2-1";
                _DutSizeX.Value = (double)this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeX.GetValue()*2;
                _DutSizeY.Value = (double)this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeY.GetValue()*2;

                PinData pin = new PinData();
                pin.AbsPos = new PinCoordinate(-85366, 104519, -9302);
                pin.RelPos = new PinCoordinate();
                pin.PinSearchParam = new PinSearchParameter();
                pin.PinSearchParam.BlobThreshold.Value = 140;
                pin.PinSearchParam.MaxBlobSizeX.Value = 50;
                pin.PinSearchParam.MaxBlobSizeY.Value = 50;
                pin.PinSearchParam.MinBlobSizeX.Value = 15;
                pin.PinSearchParam.MinBlobSizeY.Value = 15;
                pin.PinSearchParam.SearchArea = new Element<Rect>();
                pin.PinSearchParam.SearchArea.Value = new Rect(new Size(240, 240));
                pin.SerialNum.Value = 0.ToString();
                dut1.PinList.Add(new PinData(pin));
                pin.AbsPos = new PinCoordinate(-85366, 109679, -9284);
                pin.RelPos = new PinCoordinate();
                pin.PinSearchParam = new PinSearchParameter();
                pin.PinSearchParam.BlobThreshold.Value = 140;
                pin.PinSearchParam.MaxBlobSizeX.Value = 50;
                pin.PinSearchParam.MaxBlobSizeY.Value = 50;
                pin.PinSearchParam.MinBlobSizeX.Value = 15;
                pin.PinSearchParam.MinBlobSizeY.Value = 15;
                pin.PinSearchParam.SearchArea = new Element<Rect>();
                pin.PinSearchParam.SearchArea.Value = new Rect(new Size(240, 240));
                pin.SerialNum.Value = 1.ToString();
                dut1.PinList.Add(new PinData(pin));
                pin.AbsPos = new PinCoordinate(-80156, 109679, -9294);
                pin.RelPos = new PinCoordinate();
                pin.PinSearchParam = new PinSearchParameter();
                pin.PinSearchParam.BlobThreshold.Value = 140;
                pin.PinSearchParam.MaxBlobSizeX.Value = 50;
                pin.PinSearchParam.MaxBlobSizeY.Value = 50;
                pin.PinSearchParam.MinBlobSizeX.Value = 15;
                pin.PinSearchParam.MinBlobSizeY.Value = 15;
                pin.PinSearchParam.SearchArea = new Element<Rect>();
                pin.PinSearchParam.SearchArea.Value = new Rect(new Size(240, 240));
                pin.SerialNum.Value = 2.ToString();
                dut1.PinList.Add(new PinData(pin));
                pin.AbsPos = new PinCoordinate(-80156, 104519, -9287);
                pin.RelPos = new PinCoordinate();
                pin.PinSearchParam = new PinSearchParameter();
                pin.PinSearchParam.BlobThreshold.Value = 140;
                pin.PinSearchParam.MaxBlobSizeX.Value = 50;
                pin.PinSearchParam.MaxBlobSizeY.Value = 50;
                pin.PinSearchParam.MinBlobSizeX.Value = 15;
                pin.PinSearchParam.MinBlobSizeY.Value = 15;
                pin.PinSearchParam.SearchArea = new Element<Rect>();
                pin.PinSearchParam.SearchArea.Value = new Rect(new Size(240, 240));
                pin.SerialNum.Value = 3.ToString();
                dut1.PinList.Add(new PinData(pin));

                pin.AbsPos = new PinCoordinate(-79944, 109867, -9298);
                pin.RelPos = new PinCoordinate();
                pin.PinSearchParam = new PinSearchParameter();
                pin.PinSearchParam.BlobThreshold.Value = 140;
                pin.PinSearchParam.MaxBlobSizeX.Value = 50;
                pin.PinSearchParam.MaxBlobSizeY.Value = 50;
                pin.PinSearchParam.MinBlobSizeX.Value = 15;
                pin.PinSearchParam.MinBlobSizeY.Value = 15;
                pin.PinSearchParam.SearchArea = new Element<Rect>();
                pin.PinSearchParam.SearchArea.Value = new Rect(new Size(240, 240));
                pin.SerialNum.Value = 4.ToString();
                dut1.PinList.Add(new PinData(pin));
                pin.AbsPos = new PinCoordinate(-79944, 115056, -9314);
                pin.RelPos = new PinCoordinate();
                pin.PinSearchParam = new PinSearchParameter();
                pin.PinSearchParam.BlobThreshold.Value = 140;
                pin.PinSearchParam.MaxBlobSizeX.Value = 50;
                pin.PinSearchParam.MaxBlobSizeY.Value = 50;
                pin.PinSearchParam.MinBlobSizeX.Value = 15;
                pin.PinSearchParam.MinBlobSizeY.Value = 15;
                pin.PinSearchParam.SearchArea = new Element<Rect>();
                pin.PinSearchParam.SearchArea.Value = new Rect(new Size(240, 240));
                pin.SerialNum.Value = 5.ToString();
                dut1.PinList.Add(new PinData(pin));
                pin.AbsPos = new PinCoordinate(-74734, 115056, -9284);
                pin.RelPos = new PinCoordinate();
                pin.PinSearchParam = new PinSearchParameter();
                pin.PinSearchParam.BlobThreshold.Value = 140;
                pin.PinSearchParam.MaxBlobSizeX.Value = 50;
                pin.PinSearchParam.MaxBlobSizeY.Value = 50;
                pin.PinSearchParam.MinBlobSizeX.Value = 15;
                pin.PinSearchParam.MinBlobSizeY.Value = 15;
                pin.PinSearchParam.SearchArea = new Element<Rect>();
                pin.PinSearchParam.SearchArea.Value = new Rect(new Size(240, 240));
                pin.SerialNum.Value = 6.ToString();
                dut1.PinList.Add(new PinData(pin));
                pin.AbsPos = new PinCoordinate(-74734, 109896, -9289);
                pin.RelPos = new PinCoordinate();
                pin.PinSearchParam = new PinSearchParameter();
                pin.PinSearchParam.BlobThreshold.Value = 140;
                pin.PinSearchParam.MaxBlobSizeX.Value = 50;
                pin.PinSearchParam.MaxBlobSizeY.Value = 50;
                pin.PinSearchParam.MinBlobSizeX.Value = 15;
                pin.PinSearchParam.MinBlobSizeY.Value = 15;
                pin.PinSearchParam.SearchArea = new Element<Rect>();
                pin.PinSearchParam.SearchArea.Value = new Rect(new Size(240, 240));
                pin.SerialNum.Value = 7.ToString();
                dut1.PinList.Add(new PinData(pin));

                DutList.Add(dut1);

                GroupData tmpGroup1 = new GroupData();
                DutPinPair data = new DutPinPair();
                data.DutMachineIndex.XIndex.Value = 0;
                data.DutMachineIndex.XIndex.Value = 0;
                data.PinSerialNum = 0.ToString();
                tmpGroup1.DutPinPairList.Add(new DutPinPair(data));
                data.PinSerialNum = 1.ToString();
                tmpGroup1.DutPinPairList.Add(new DutPinPair(data));
                data.PinSerialNum = 2.ToString();
                tmpGroup1.DutPinPairList.Add(new DutPinPair(data));
                data.PinSerialNum = 3.ToString();
                tmpGroup1.DutPinPairList.Add(new DutPinPair(data));
                data.PinSerialNum = 4.ToString();
                tmpGroup1.DutPinPairList.Add(new DutPinPair(data));
                data.PinSerialNum = 5.ToString();
                tmpGroup1.DutPinPairList.Add(new DutPinPair(data));
                data.PinSerialNum = 6.ToString();
                tmpGroup1.DutPinPairList.Add(new DutPinPair(data));
                data.PinSerialNum = 7.ToString();
                tmpGroup1.DutPinPairList.Add(new DutPinPair(data));

                PinGroupList.Add(tmpGroup1);
                */

                ////Dut dut2 = new Dut(new MachineIndex(1, 1));
                ////dut2.UserIndex = new UserIndex(1, 1);
                ////dut2.DutEnable.Value = true;
                ////dut2.RefCorner = new PinCoordinate(dut1.RefCorner.GetX() + this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeX.Value, dut1.RefCorner.GetY() + this.StageSupervisor().WaferObject.GetPhysInfo().DieSizeY.Value, -9301);
                ////dut2.LTCorner = new PinCoordinate(dut2.RefCorner.GetX(), dut2.RefCorner.GetY() + _DutSizeY.Value, dut1.RefCorner.GetZ());
                ////dut2.RTCorner = new PinCoordinate(dut2.RefCorner.GetX() + _DutSizeX.Value, dut2.RefCorner.GetY() + _DutSizeY.Value, dut1.RefCorner.GetZ());
                ////dut2.RBCorner = new PinCoordinate(dut2.RefCorner.GetX() + _DutSizeX.Value, dut2.RefCorner.GetY(), dut1.RefCorner.GetZ());

                //pin.AbsPos = new PinCoordinate(-79944, 109867, -9298);
                //pin.RelPos = new PinCoordinate();
                //pin.PinSearchParam = new PinSearchParameter();
                //pin.PinSearchParam.BlobThreshold.Value = 140;
                //pin.PinSearchParam.MaxBlobSizeX.Value = 50;
                //pin.PinSearchParam.MaxBlobSizeY.Value = 50;
                //pin.PinSearchParam.MinBlobSizeX.Value = 15;
                //pin.PinSearchParam.MinBlobSizeY.Value = 15;
                //pin.PinSearchParam.SearchArea = new Element<Rect>();
                //pin.PinSearchParam.SearchArea.Value = new Rect(new Size(240, 240));
                //pin.SerialNum.Value = 4.ToString();
                //dut2.PinList.Add(new PinData(pin));
                //pin.AbsPos = new PinCoordinate(-79944, 115056, -9314);
                //pin.RelPos = new PinCoordinate();
                //pin.PinSearchParam = new PinSearchParameter();
                //pin.PinSearchParam.BlobThreshold.Value = 140;
                //pin.PinSearchParam.MaxBlobSizeX.Value = 50;
                //pin.PinSearchParam.MaxBlobSizeY.Value = 50;
                //pin.PinSearchParam.MinBlobSizeX.Value = 15;
                //pin.PinSearchParam.MinBlobSizeY.Value = 15;
                //pin.PinSearchParam.SearchArea = new Element<Rect>();
                //pin.PinSearchParam.SearchArea.Value = new Rect(new Size(240, 240));
                //pin.SerialNum.Value = 5.ToString();
                //dut2.PinList.Add(new PinData(pin));
                //pin.AbsPos = new PinCoordinate(-74734, 115056, -9284);
                //pin.RelPos = new PinCoordinate();
                //pin.PinSearchParam = new PinSearchParameter();
                //pin.PinSearchParam.BlobThreshold.Value = 140;
                //pin.PinSearchParam.MaxBlobSizeX.Value = 50;
                //pin.PinSearchParam.MaxBlobSizeY.Value = 50;
                //pin.PinSearchParam.MinBlobSizeX.Value = 15;
                //pin.PinSearchParam.MinBlobSizeY.Value = 15;
                //pin.PinSearchParam.SearchArea = new Element<Rect>();
                //pin.PinSearchParam.SearchArea.Value = new Rect(new Size(240, 240));
                //pin.SerialNum.Value = 6.ToString();
                //dut2.PinList.Add(new PinData(pin));
                //pin.AbsPos = new PinCoordinate(-74734, 109896, -9289);
                //pin.RelPos = new PinCoordinate();
                //pin.PinSearchParam = new PinSearchParameter();
                //pin.PinSearchParam.BlobThreshold.Value = 140;
                //pin.PinSearchParam.MaxBlobSizeX.Value = 50;
                //pin.PinSearchParam.MaxBlobSizeY.Value = 50;
                //pin.PinSearchParam.MinBlobSizeX.Value = 15;
                //pin.PinSearchParam.MinBlobSizeY.Value = 15;
                //pin.PinSearchParam.SearchArea = new Element<Rect>();
                //pin.PinSearchParam.SearchArea.Value = new Rect(new Size(240, 240));
                //pin.SerialNum.Value = 7.ToString();
                //dut2.PinList.Add(new PinData(pin));
                //DutList.Add(dut2);

                ////GroupData tmpGroup1 = new GroupData();
                ////DutPinPair data = new DutPinPair();
                ////data.DutMachineIndex.XIndex.Value = 0;
                ////data.DutMachineIndex.XIndex.Value = 0;
                ////data.PinSerialNum = 0.ToString();
                ////tmpGroup1.DutPinPairList.Add(new DutPinPair(data));
                ////data.PinSerialNum = 1.ToString();
                ////tmpGroup1.DutPinPairList.Add(new DutPinPair(data));

                ////PinGroupList.Add(tmpGroup1);

                ////GroupData tmpGroup2 = new GroupData();
                ////data.PinSerialNum = 2.ToString();
                ////tmpGroup2.DutPinPairList.Add(new DutPinPair(data));
                ////data.PinSerialNum = 3.ToString();
                ////tmpGroup2.DutPinPairList.Add(new DutPinPair(data));

                ////PinGroupList.Add(tmpGroup2);

                ////GroupData tmpGroup3 = new GroupData();
                ////data = new DutPinPair();
                ////data.DutMachineIndex.XIndex.Value = 1;
                ////data.DutMachineIndex.YIndex.Value = 1;
                ////data.PinSerialNum = 4.ToString();
                ////tmpGroup3.DutPinPairList.Add(new DutPinPair(data));
                ////data.PinSerialNum = 5.ToString();
                ////tmpGroup3.DutPinPairList.Add(new DutPinPair(data));

                ////PinGroupList.Add(tmpGroup3);

                ////GroupData tmpGroup4 = new GroupData();
                ////data.PinSerialNum = 6.ToString();
                ////tmpGroup4.DutPinPairList.Add(new DutPinPair(data));
                ////data.PinSerialNum = 7.ToString();
                ////tmpGroup4.DutPinPairList.Add(new DutPinPair(data));

                ////PinGroupList.Add(tmpGroup4);



                //Dut dut1 = new Dut(new MachineIndex(0, 0));
                //dut1.UserIndex = new UserIndex(0, 0);
                //dut1.DutEnable.Value = true;
                //dut1.PinList = new List<PinData>();
                //dut1.PinList.Add(new PinData()
                //{
                //    PinSearchParam = new PinSearchParameter()
                //    {
                //        SearchArea = new Element<Rect>()
                //        {
                //            Value = new Rect
                //            {
                //                X = 10,
                //                Y = 10,
                //                Width = 10,
                //                Height = 10
                //            }
                //        },
                //        PinSize = new Element<Rect>()
                //        {
                //            Value = new Rect
                //            {
                //                X = 10,
                //                Y = 10,
                //                Width = 10,
                //                Height = 10
                //            }
                //        }
                //    }
                //});
                //dut1.PinList.Add(new PinData()
                //{
                //    PinSearchParam = new PinSearchParameter()
                //    {
                //        SearchArea = new Element<Rect>()
                //        {
                //            Value = new Rect
                //            {
                //                X = 11,
                //                Y = 11,
                //                Width = 11,
                //                Height = 11
                //            }
                //        },
                //        PinSize = new Element<Rect>()
                //        {
                //            Value = new Rect
                //            {
                //                X = 10,
                //                Y = 10,
                //                Width = 10,
                //                Height = 10
                //            }
                //        }
                //    }
                //});
                //DutList.Add(dut1);

                //Dut dut2 = new Dut(new MachineIndex(1, 1));
                //dut2.UserIndex = new UserIndex(1, 1);
                //dut2.DutEnable.Value = true;
                //dut2.PinList.Add(new PinData()
                //{
                //    PinSearchParam = new PinSearchParameter()
                //    {
                //        SearchArea = new Element<Rect>()
                //        {
                //            Value = new Rect
                //            {
                //                X = 12,
                //                Y = 12,
                //                Width = 12,
                //                Height = 12
                //            }
                //        },
                //        PinSize = new Element<Rect>()
                //        {
                //            Value = new Rect
                //            {
                //                X = 10,
                //                Y = 10,
                //                Width = 10,
                //                Height = 10
                //            }
                //        }
                //    }
                //});
                //dut2.PinList.Add(new PinData()
                //{
                //    PinSearchParam = new PinSearchParameter()
                //    {
                //        SearchArea = new Element<Rect>()
                //        {
                //            Value = new Rect
                //            {
                //                X = 13,
                //                Y = 13,
                //                Width = 13,
                //                Height = 13
                //            }
                //        },
                //        PinSize = new Element<Rect>()
                //        {
                //            Value = new Rect
                //            {
                //                X = 10,
                //                Y = 10,
                //                Width = 10,
                //                Height = 10
                //            }
                //        }
                //    }
                //});
                //DutList.Add(dut2);

                //GroupData tmpGroup = new GroupData();

                //tmpGroup.DutPinPairList.Add(new DutPinPair());
                //tmpGroup.DutPinPairList.Add(new DutPinPair());
                //tmpGroup.DutPinPairList.Add(new DutPinPair());
                //tmpGroup.DutPinPairList.Add(new DutPinPair());

                //PinGroupList.Add(tmpGroup);
                #endregion

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        //public void SetEventToElement()
        //{
        //    if(ProbeCardType != null)
        //    {
        //        ProbeCardType.ValueChangedEvent += ProbeCardType_ValueChangedEvent;
        //    }
        //}

        public void SettingAlignKeyHighLib()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            if (ProbeCardType.Value == PROBECARD_TYPE.MEMS_Dual_AlignKey)
            {
                AlignKeyLibPack tmpLibPack = new AlignKeyLibPack();

                AlignKeyHighLib.Clear();
                // 마이크론 프로브 카드의 초기 설정값. 실제로는 파일에 쓰여 있어야 하고 파일로부터 로드 해야 한다.
                retVal = LoadAlignKeyLibPack(ref tmpLibPack);

                if (retVal != EventCodeEnum.UNDEFINED && tmpLibPack != null)
                {
                    AlignKeyHighLib.Add(tmpLibPack);

                    foreach (var dut in this.DutList)
                    {
                        foreach (var pin in dut.PinList)
                        {
                            pin.PinSearchParam.AlignKeyHigh = new List<PinSearchParameter.AlignKeyInfo>();
                            for (int num = 0; num < this.AlignKeyHighLib[0].AlignKeyLib.Count; num++)
                            {
                                pin.PinSearchParam.AlignKeyHigh.Add(new PinSearchParameter.AlignKeyInfo());
                            }


                            if (this.AlignKeyHighLib != null && 0 < this.AlignKeyHighLib.Count)
                            {
                                var source_AlignKeyHigh = this.AlignKeyHighLib[0];
                                var dest_AlignKeyHigh = pin.PinSearchParam.AlignKeyHigh;

                                int count = source_AlignKeyHigh.AlignKeyLib.Count;

                                if (dest_AlignKeyHigh.Count == count)
                                {
                                    for (int i = 0; i < count; i++)  ///TEST CODE TODO:REMOVE
                                    {
                                        var source_AlignKeyLib = source_AlignKeyHigh.AlignKeyLib[i];

                                        dest_AlignKeyHigh[i].AlignKeyPos.X.Value = source_AlignKeyLib.AlignKeyPosition.X.Value;
                                        dest_AlignKeyHigh[i].AlignKeyPos.Y.Value = source_AlignKeyLib.AlignKeyPosition.Y.Value;
                                        dest_AlignKeyHigh[i].AlignKeyPos.Z.Value = source_AlignKeyLib.AlignKeyPosition.Z.Value;

                                        dest_AlignKeyHigh[i].BlobSizeX.Value = source_AlignKeyLib.BlobSizeX;
                                        dest_AlignKeyHigh[i].BlobSizeY.Value = source_AlignKeyLib.BlobSizeY;
                                        dest_AlignKeyHigh[i].AlignKeyAngle.Value = 0;
                                        dest_AlignKeyHigh[i].BlobSizeTolX.Value = source_AlignKeyLib.BlobSizeTolX;
                                        dest_AlignKeyHigh[i].BlobSizeTolY.Value = source_AlignKeyLib.BlobSizeTolY;
                                        dest_AlignKeyHigh[i].BlobThreshold.Value = source_AlignKeyLib.BlobThreshold;
                                        dest_AlignKeyHigh[i].FocusingAreaSizeX.Value = source_AlignKeyLib.FocusingAreaX;
                                        dest_AlignKeyHigh[i].FocusingAreaSizeY.Value = source_AlignKeyLib.FocusingAreaY;
                                        dest_AlignKeyHigh[i].FocusingRange.Value = source_AlignKeyLib.FocusingRange;
                                        dest_AlignKeyHigh[i].ImageBlobType = source_AlignKeyLib.ImageBlobType;
                                        dest_AlignKeyHigh[i].PatternIfo.LightParams = new ObservableCollection<LightValueParam>();
                                        dest_AlignKeyHigh[i].BlobRoiSizeX.Value = source_AlignKeyLib.BlobRoiSizeX;
                                        dest_AlignKeyHigh[i].BlobRoiSizeY.Value = source_AlignKeyLib.BlobRoiSizeY;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private EventCodeEnum LoadAlignKeyLibPack(ref AlignKeyLibPack tmpLibPack)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            IParam tmpParam = null;

            try
            {
                retVal = this.LoadParameter(ref tmpParam, typeof(AlignKeyLibPack));
                if(tmpParam != null)
                {
                    tmpLibPack = tmpParam as AlignKeyLibPack;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                retVal = EventCodeEnum.UNDEFINED;
            }

            return retVal;
        }

        //private void ProbeCardType_ValueChangedEvent(object oldValue, object newValue)
        //{
        //    SettingAlignKeyHighLib();

        //    this.PinAligner().ConnectValueChangedEventHandler();
        //}
    }
}
