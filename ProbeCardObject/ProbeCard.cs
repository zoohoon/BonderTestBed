using System;
using System.Collections.Generic;
using System.Linq;

namespace ProbeCardObject
{
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using System.Windows;
    using ProberErrorCode;
    using LogModule;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using ProberInterfaces.State;
    using Autofac;
    using ProberInterfaces.PinAlign;

    [Serializable]
    public class ProbeCard : IProbeCard, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        public bool IsInitialized { get; set; }

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; }

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public object Owner { get; set; }

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public List<Object> Nodes { get; set; }

        public ProbeCard()
        {
        }

        #region ==> ProbeCardDevObject                                                                                          
        private ProbeCardDevObject _ProbeCardDevOjbect;
        /// <summary>
        /// ProbeCard의 Device Parameter 정보를 담고 있는 속성. ProbeCardDevObjectRef 동일.
        /// </summary>
        public ProbeCardDevObject ProbeCardDevObject
        {
            get { return _ProbeCardDevOjbect; }
            set
            {
                if (value != _ProbeCardDevOjbect)
                {
                    _ProbeCardDevOjbect = value;
                    RaisePropertyChanged(nameof(ProbeCardDevObject));
                }
            }
        }
        #endregion

        /// <summary>
        /// ProbeCard의 Device Parameter 정보를 담고 있는 속성. ProbeCardDevObject 동일.
        /// </summary>
        public IProbeCardDevObject ProbeCardDevObjectRef
        {
            get { return ProbeCardDevObject; }
        }

        #region ==> ProbeCardSysOjbect
        private ProbeCardSysObject _ProbeCardSysObject;
        /// <summary>
        /// ProbeCard의 Device Parameter 정보를 담고 있는 속성. ProbeCardSysOjbect 동일.
        /// </summary>
        public ProbeCardSysObject ProbeCardSysObject
        {
            get { return _ProbeCardSysObject; }
            set
            {
                if (value != _ProbeCardSysObject)
                {
                    _ProbeCardSysObject = value;
                    RaisePropertyChanged(nameof(ProbeCardSysObject));
                }
            }
        }
        public IProbeCardSysObject ProbeCardSysObjectRef
        {
            get { return ProbeCardSysObject; }
        }
        #endregion

        #region ==> Common Parameter

        #region ==> SelectedCoordM
        [NonSerialized]
        private MachineIndex _SelectedCoordM = new MachineIndex();
        /// <summary>
        /// 뷰에서 사용자가 선택한 위치를 뷰모델에 연결시켜 주기 위해 사용하는 좌표 
        /// </summary>
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public MachineIndex SelectedCoordM
        {
            get { return _SelectedCoordM; }
            set
            {
                if (value != _SelectedCoordM)
                {
                    _SelectedCoordM = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> FirstDutM
        [NonSerialized]
        private MachineIndex _FirstDutM = new MachineIndex();
        /// <summary>
        /// 뷰에서 그려진 더트에서 첫번째 더트가 위치한 좌표값
        /// </summary>
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public MachineIndex FirstDutM
        {
            get { return _FirstDutM; }
            set
            {
                if (value != _FirstDutM)
                {
                    _FirstDutM = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CandidateDutList        
        [NonSerialized]
        private ObservableCollection<IDut> _CandidateDutList = new ObservableCollection<IDut>();
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public ObservableCollection<IDut> CandidateDutList
        {
            get { return _CandidateDutList; }
            set
            {
                if (value != _CandidateDutList)
                {
                    _CandidateDutList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> DisplayPinList
        [NonSerialized]
        private List<IPinData> _DisplayPinList = new List<IPinData>();
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<IPinData> DisplayPinList
        {
            get { return _DisplayPinList; }
            set
            {
                if (value != _DisplayPinList)
                {
                    _DisplayPinList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        //==> ViewModel에 있어야할 것 같은 속성들

        #region ==> SelectedPin
        [NonSerialized]
        private IPinData _SelectedPin;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public IPinData SelectedPin
        {
            get { return _SelectedPin; }
            set
            {
                if (value != _SelectedPin)
                {
                    _SelectedPin = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ProberCardCompensation
        [NonSerialized]
        private PinCoordinate _ProberCardCompensation = new PinCoordinate();
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public PinCoordinate ProberCardCompensation
        {
            get { return _ProberCardCompensation; }
            set
            {
                if (value != _ProberCardCompensation)
                {
                    _ProberCardCompensation = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CandidatePinGroupList
        [NonSerialized]
        private List<IGroupData> _CandidatePinGroupList = new List<IGroupData>();
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<IGroupData> CandidatePinGroupList
        {
            get { return _CandidatePinGroupList; }
            set { _CandidatePinGroupList = value; }
        }
        #endregion

        #region ==> LineOffsetX
        [NonSerialized]
        private double _LineOffsetX;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public double LineOffsetX
        {
            get { return _LineOffsetX; }
            set
            {
                if (value != _LineOffsetX)
                {
                    _LineOffsetX = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> LineOffsetY
        [NonSerialized]
        private double _LineOffsetY;
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public double LineOffsetY
        {
            get { return _LineOffsetY; }
            set
            {
                if (value != _LineOffsetY)
                {
                    _LineOffsetY = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        //#region ==> ContactCnt
        //[NonSerialized]
        //private int _ContactCnt;
        //[XmlIgnore, JsonIgnore]
        //public int ContactCnt
        //{
        //    get { return _ContactCnt; }
        //    set
        //    {
        //        if (value != _ContactCnt)
        //        {
        //            _ContactCnt = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //#endregion


        #region ==> AlignState
        [NonSerialized]
        private Element<AlignStateEnum> _AlignState
             = new Element<AlignStateEnum>();
        [JsonIgnore]
        public Element<AlignStateEnum> AlignState
        {
            get { return _AlignState; }
            set
            {
                if (value != _AlignState)
                {
                    _AlignState = value;
                    RaisePropertyChanged();
                }
            }
        }
        public AlignStateEnum GetAlignState()
        {
            AlignStateEnum state = AlignStateEnum.IDLE;
            try
            {
                if (AlignState.DoneState == ElementStateEnum.DONE)
                    state = AlignStateEnum.DONE;
                else
                    state = AlignStateEnum.IDLE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return state;
            //return AlignState.Value;
        }

        public void SetAlignState(AlignStateEnum state)
        {
            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}], SetAlignState() : {AlignState.Value} => {state}", isInfo: true);

                if (state != AlignState.Value)
                {
                    this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.PINALIGNSTATE, state.ToString());
                }

                AlignState.Value = state;

                switch (state)
                {
                    case AlignStateEnum.IDLE:
                        AlignState.DoneState = ElementStateEnum.DEFAULT;

                        if (this.GetContainer().IsRegistered<IPinAligner>() == true)
                        {
                            if (this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset.Count != 3)
                            {
                                this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset.Clear();

                                this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset.Add(0);
                                this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset.Add(0);
                                this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset.Add(0);
                            }
                            else
                            {
                                this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset[0] = 0;
                                this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset[1] = 0;
                                this.PinAligner().PinAlignInfo.AlignResult.PlaneOffset[2] = 0;
                            }
                        }

                        break;
                    case AlignStateEnum.DONE:
                        AlignState.DoneState = ElementStateEnum.DONE; break;
                }



                //if (this.LoaderRemoteMediator()?.GetServiceCallBack() != null)
                //{
                //    //this.StageSupervisor().ServiceCallBack?.UpdateWaferAlignState(this.LoaderController().GetChuckIndex(), AlignTypeEnum.Pin, AlignState);
                //}
                //else
                //{
                //    //LoggerManager.Debug($"Can not update pin align state");
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> PinPadAlignState
        [NonSerialized]
        private AlignStateEnum _PinPadAlignState;
        [XmlIgnore, JsonIgnore]
        public AlignStateEnum PinPadAlignState
        {
            get { return _PinPadAlignState; }
            private set
            {
                if (value != _PinPadAlignState)
                {
                    _PinPadAlignState = value;
                    RaisePropertyChanged();
                }
            }
        }
        public AlignStateEnum GetPinPadAlignState()
        {
            return PinPadAlignState;
        }
        public void SetPinPadAlignState(AlignStateEnum state)
        {
            LoggerManager.Debug($"[{this.GetType().Name}], SetAlignState(), PinPadAlignState : {PinPadAlignState} => {state}", isInfo: true);

            PinPadAlignState = state;
        }
        #endregion

        #region ==> DisplayDutArr
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public IDut[] DisplayDutArr { get; set; }
        #endregion

        #region ==> ProbeCardChangedToggle
        [NonSerialized]
        private Element<bool> _ProbeCardChangedToggle
             = new Element<bool>();
        [XmlIgnore, JsonIgnore]
        public Element<bool> ProbeCardChangedToggle
        {
            get { return _ProbeCardChangedToggle; }
            set
            {
                if (value != _ProbeCardChangedToggle)
                {
                    _ProbeCardChangedToggle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _PinSetupChangedToggle
             = new Element<bool>();

        public Element<bool> PinSetupChangedToggle
        {
            get { return _PinSetupChangedToggle; }
            set { _PinSetupChangedToggle = value; }
        }

        #endregion

        #endregion

        #region ==> Function

        /// <summary>
        /// 핀 넘버 -1 값을 기반으로 핀넘버에 해당하는 핀을 반환하는 코드. 
        /// 모든 더트에서 핀넘버를 뒤진다.
        /// </summary>
        /// <param name="pinNum">핀이 아니라 핀인덱스</param>
        /// <returns></returns>
        public IPinData GetPin(int pinNum)
        {
            // 데이터 배열 상의 핀 번호는 0번부터 시작하지만, 실제 보이는 핀 번호는 0부터 시작한다. 따라서 넘겨온 번호에 1을 더해서 찾는다.
            IPinData pinData = null;
            try
            {
                foreach (IDut dut in this.ProbeCardDevObject.DutList)
                {
                    if(dut.PinList.Count > 0)
                    {
                        pinData = dut.PinList.FirstOrDefault(pin => pin.PinNum.Value == (pinNum + 1));
                        if (pinData != null)
                            break;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pinData;
        }
        public List<IPinData> GetPinList()
        {
            List<IPinData> pinList = new List<IPinData>();
            try
            {

                IEnumerable<List<IPinData>> pinListes = this.ProbeCardDevObject.DutList.Select(dut => dut.PinList);
                foreach (List<IPinData> pines in pinListes)
                {
                    pinList.AddRange(pines);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pinList;
        }

        public int GetPinCount()
        {
            return GetPinList().Count;
        }

        public int GetPinArrayIndex(int pinNum)
        {
            // 데이터 배열 상의 핀 번호는 0번부터 시작하지만, 실제 보이는 핀 번호는 0부터 시작한다. 따라서 넘겨온 번호에 1을 더해서 찾는다.
            int idx = -1;
            try
            {
                foreach (IDut dut in this.ProbeCardDevObject.DutList)
                {
                    idx = dut.PinList.FindIndex(pin => pin.PinNum.Value == (pinNum + 1));
                    if (idx != -1)
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return idx;
        }


        /// <summary>
        /// 핀넘버 기준으로 더트를 반환한다
        /// </summary>
        /// <param name="pinNum"></param>
        /// <returns></returns>
        public IDut GetDutFromPinNum(int pinNum)
        {
            IDut retDut = null;
            try
            {
                foreach (IDut dut in this.ProbeCardDevObject.DutList)
                {
                    var tpin = dut.PinList.Find(pin => pin.PinNum.Value == pinNum);
                    if (tpin != null)
                    {
                        retDut = dut;
                        break;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retDut;
        }

        public void GetArrayIndex(int pinNum, out int dut_arrayNum, out int pin_arrayNum)
        {
            // 데이터 배열 상의 핀 번호는 0번부터 시작하지만, 실제 보이는 핀 번호는 0부터 시작한다. 따라서 넘겨온 번호에 1을 더해서 찾는다.
            int idx = -1;
            dut_arrayNum = -1;
            pin_arrayNum = -1;

            foreach (IDut dut in this.ProbeCardDevObject.DutList)
            {
                idx = dut.PinList.FindIndex(pin => pin.PinNum.Value == (pinNum + 1));
                if (idx != -1)
                {
                    dut_arrayNum = this.ProbeCardDevObject.DutList.IndexOf(dut);
                    pin_arrayNum = idx;
                    break;
                }
            }
        }

        public EventCodeEnum ShiftPindata(double offsetX, double offsetY, double offsetZ)
        {
            // 마크얼라인 후 핀 데이터 전체를 시프트하기 위하여 사용한다. 주의할것!!!!

            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                double tmpcenx = 0;
                double tmpceny = 0;
                double tmpdutcenx = 0;
                double tmpdutceny = 0;

                try
                {
                    LoggerManager.Debug($"Shift pin data... ({offsetX}, {offsetY}, {offsetZ})", isInfo: true);

                    for (int i = 0; i <= this.ProbeCardDevObject.DutList.Count() - 1; i++)
                    {
                        for (int j = 0; j <= this.ProbeCardDevObject.DutList[i].PinList.Count() - 1; j++)
                        {
                            this.ProbeCardDevObject.DutList[i].PinList[j].AbsPosOrg.X.Value += offsetX;
                            this.ProbeCardDevObject.DutList[i].PinList[j].AbsPosOrg.Y.Value += offsetY;
                            this.ProbeCardDevObject.DutList[i].PinList[j].AbsPosOrg.Z.Value += offsetZ;

                            this.ProbeCardDevObject.DutList[i].PinList[j].MarkCumulativeCorrectionValue.X.Value += offsetX;
                            this.ProbeCardDevObject.DutList[i].PinList[j].MarkCumulativeCorrectionValue.Y.Value += offsetY;
                            this.ProbeCardDevObject.DutList[i].PinList[j].MarkCumulativeCorrectionValue.Z.Value += offsetZ;

                            if (this.ProbeCardDevObject.DutList[i].PinList[j].DutLLconerPos.Value != null)
                            {
                                this.ProbeCardDevObject.DutList[i].PinList[j].DutLLconerPos.Value.X.Value += offsetX;
                                this.ProbeCardDevObject.DutList[i].PinList[j].DutLLconerPos.Value.Y.Value += offsetY;
                            }
                        }
                    }

                    CalcPinCen(out tmpcenx, out tmpceny, out tmpdutcenx, out tmpdutceny);
                    this.ProbeCardDevObject.PinCenX = tmpcenx;
                    this.ProbeCardDevObject.PinCenY = tmpceny;
                    this.ProbeCardDevObject.DutCenX = tmpdutcenx;
                    this.ProbeCardDevObject.DutCenY = tmpdutceny;
                    LoggerManager.Debug($"ShiftPindata() : pin cen = ({this.ProbeCardDevObject.PinCenX}, {this.ProbeCardDevObject.PinCenY}), " +
                                                         $"dut cen = ({this.ProbeCardDevObject.DutCenX}, {this.ProbeCardDevObject.DutCenY})");

                    retval = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    SetAlignState(AlignStateEnum.IDLE);

                    retval = EventCodeEnum.UNDEFINED;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        /// <summary>
        /// 1번 더트(티치다이)와의 옵셋 계산하기 위함. 
        /// 티치다이는 항상 Dutnumber 1인 더트를 사용해야하는데
        /// 그 이유는 웨이퍼의 위치와 카드의 위치를 매칭할때 1번 더트 기준으로 매칭하기 때문 
        /// </summary>
        /// <param name="siteindex"></param>
        /// <returns></returns>
        public MachineIndex GetRefOffset(int siteindex)
        {
            MachineIndex indexOffset = new MachineIndex();

            try
            {
                indexOffset.XIndex = this.ProbeCardDevObject.DutList[siteindex].MacIndex.XIndex - this.ProbeCardDevObject.DutList[0].MacIndex.XIndex;
                indexOffset.YIndex = this.ProbeCardDevObject.DutList[siteindex].MacIndex.YIndex - this.ProbeCardDevObject.DutList[0].MacIndex.YIndex;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return indexOffset;
        }

        public EventCodeEnum CheckPinPadParameterValidity()
        {
            try
            {
                int padNum = 0;
                //int pinIndex = 0;
                //int dutIndex = 0;
                int pin_total = 0;
                int pad_total = 0;


                foreach (DUTPadObject padinfo in this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.ToList())
                {
                    padNum = padinfo.PadNumber.Value;
                    //GetArrayIndex(padNum - 1, out dutIndex, out pinIndex);
                    var tmpPin = this.GetParam_ProbeCard().GetPinList().Find(tmppin => tmppin.DutNumber.Value == (int)padinfo.DutNumber);
                    if (tmpPin == null)
                    {
                        return EventCodeEnum.NODATA;
                    }
                    var tmpDut = this.GetParam_ProbeCard().GetDutFromPinNum(tmpPin.PinNum.Value);

                    if (padNum <= 0)
                    {
                        LoggerManager.Debug($"Pad data has wrong number = {padNum}");
                        return EventCodeEnum.PTPA_WRONG_PAD_NUMBER;
                    }

                    if (tmpDut != null && tmpPin != null)
                    {
                        // 맞는 핀 찾음. 정상.
                    }
                    else
                    {
                        // 서로 매칭되는 핀 패드가 없음. 에러
                        LoggerManager.Debug($"Could not find matched pin number. Pad number = {padNum}");
                        return EventCodeEnum.PIN_PAD_MATCH_FAIL;
                    }

                    if (tmpDut.DutNumber <= 0)
                    {
                        LoggerManager.Debug($"Pin data is in wrong DUT.  DUT number = {tmpDut.DutNumber}");
                        return EventCodeEnum.PTPA_WRONG_DUT_NUMBER;
                    }

                    if (padinfo.DutNumber <= 0)
                    {
                        LoggerManager.Debug($"Pad data is in wrong DUT.  DUT number = {padinfo.DutNumber}");
                        return EventCodeEnum.PTPA_WRONG_DUT_NUMBER;
                    }
                }

                if (this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos.Count() != this.StageSupervisor().ProbeCardInfo.GetPinCount())
                {
                    LoggerManager.Debug($"Pin and Pad count is different.  Pad count = {pad_total}, pin count = {pin_total}");
                    return EventCodeEnum.PIN_PAD_MATCH_FAIL;
                }

                return EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNDEFINED;
            }

        }

        public EventCodeEnum GetPinDataFromPads()
        {
            LoggerManager.Debug($"[ProbeCard] GetPinDataFromPads() : Start GetPinDataFromPads");

            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            double tmpcenx = 0;
            double tmpceny = 0;
            double tmpdutcenx = 0;
            double tmpdutceny = 0;
            GroupData tmpGroup = null;
            int tmpDutIndex;

            IPinData tmpPinData;

            double orgPosX = 0;
            double orgPosY = 0;
            double orgPosZ = 0;
            double shiftOffsetX = 0;
            double shiftOffsetY = 0;
            double shiftOffsetZ = 0;

            try
            {
                ISubstrateInfo substrateInfo = this.StageSupervisor().WaferObject.GetSubsInfo();
                IPhysicalInfo physicalInfo = this.StageSupervisor().WaferObject.GetPhysInfo();

                double WaferCenterX = substrateInfo.WaferCenter.X.Value;
                double WaferCenterY = substrateInfo.WaferCenter.Y.Value;

                if (substrateInfo.Pads.DutPadInfos.Count <= 0)
                {
                    retVal = EventCodeEnum.NODATA;
                }
                else
                {
                    if (this.ProbeCardDevObject.PinDefaultHeight.Value >= 0)
                    {
                        // 예외 처리. 
                        this.ProbeCardDevObject.PinDefaultHeight.Value = -10000;
                    }

                    // 초기화 할때 패드 데이터 기반으로 핀 데이터를 초기화 하되, Ref 핀 위치는 바꾸지 않는다.
                    // (초기화하고 나서 다시 찾으러 가기 너무 귀찮음....)

                    // 데이터 클리어 하기 전에 현재 Ref핀 위치 저장
                    tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(this.ProbeCardDevObject.RefPinNum.Value - 1);

                    if (tmpPinData != null)
                    {
                        orgPosX = tmpPinData.AbsPos.X.Value;
                        orgPosY = tmpPinData.AbsPos.Y.Value;
                        orgPosZ = tmpPinData.AbsPos.Z.Value;
                    }

                    foreach (Dut DutDataFromProbeCard in this.ProbeCardDevObject.DutList)
                    {
                        DutDataFromProbeCard.PinList.Clear();
                    }

                    this.ProbeCardDevObject.DutSizeX = new Element<double>();
                    this.ProbeCardDevObject.DutSizeY = new Element<double>();

                    this.ProbeCardDevObject.DutSizeX.Value = (double)physicalInfo.DieSizeX.GetValue();
                    this.ProbeCardDevObject.DutSizeY.Value = (double)physicalInfo.DieSizeY.GetValue();

                    // 그룹 정보 클리어
                    if (this.ProbeCardDevObject.PinGroupList == null)
                    {
                        this.ProbeCardDevObject.PinGroupList = new List<IGroupData>();
                        this.ProbeCardDevObject.PinGroupList.Add(new GroupData());
                    }

                    if (this.ProbeCardDevObject.PinGroupList.Count == 0)
                    {
                        this.ProbeCardDevObject.PinGroupList.Add(new GroupData());
                    }

                    if (this.ProbeCardDevObject.PinGroupList[0].PinNumList == null)
                    {
                        this.ProbeCardDevObject.PinGroupList[0].PinNumList = new List<int>();
                    }

                    this.ProbeCardDevObject.PinGroupList[0].PinNumList.Clear();

                    tmpDutIndex = substrateInfo.Pads.GetPadArrayIndex(0);   // 첫번째 패드를 가지고 있는 배열의 인덱스

                    // 웨이퍼의 Center die의 중심점과 0번 패드를 가지고 있던 다이의 LL 사이의 거리 (Center die가 얼마나 어긋나 있는가)
                    // 이 값을 더해주면 해당 위치가 1번 더트의 중심점을 패드 등록 당시 웨이퍼 센터에 맞추었다고 가정했을때의 값으로 시프트된다.
                    double cen_die_offsetX = 0;
                    double cen_die_offsetY = 0;

                    cen_die_offsetX = WaferCenterX - (((physicalInfo.CenM.XIndex.Value - substrateInfo.Pads.DutPadInfos[tmpDutIndex].MachineIndex.XIndex) * substrateInfo.ActualDieSize.Width.Value) + substrateInfo.Pads.DutPadInfos[tmpDutIndex].MIndexLCWaferCoord.X.Value - (double)((substrateInfo.ActualDieSize.Width.Value / 2.0)));
                    cen_die_offsetY = WaferCenterY - (((physicalInfo.CenM.YIndex.Value - substrateInfo.Pads.DutPadInfos[tmpDutIndex].MachineIndex.YIndex) * substrateInfo.ActualDieSize.Height.Value) + substrateInfo.Pads.DutPadInfos[tmpDutIndex].MIndexLCWaferCoord.Y.Value - (double)((substrateInfo.ActualDieSize.Height.Value / 2.0)));

                    // 핀 기본 그룹 생성
                    tmpGroup = new GroupData();
                    tmpGroup.GroupNum = 1;

                    this.ProbeCardDevObject.PinGroupList.Clear();
                    this.ProbeCardDevObject.PinGroupList.Add(tmpGroup);

                    if(tmpPinData != null)
                    {
                        // 1번 패드를 기준으로 미리 시프트할 양을 계산하여 옵셋을 구해둔다.
                        foreach (DUTPadObject DutDataFromPads in substrateInfo.Pads.DutPadInfos)
                        {
                            if (DutDataFromPads.PadNumber.Value == 1)
                            {
                                // Ref Pad
                                shiftOffsetX = orgPosX - (DutDataFromPads.MIndexLCWaferCoord.X.Value + DutDataFromPads.PadCenter.X.Value + cen_die_offsetX);
                                shiftOffsetY = orgPosY - (DutDataFromPads.MIndexLCWaferCoord.Y.Value + DutDataFromPads.PadCenter.Y.Value + cen_die_offsetY);
                                shiftOffsetZ = orgPosZ - this.ProbeCardDevObject.PinDefaultHeight.Value;

                                break;
                            }
                        }
                    }

                    // 패드를 등록하던 당시의 좌표가 웨이퍼 센터가 아니었을 수 있으므로, 패드 위치의 Abs값을 그대로 사용할 수 없다.
                    // 따라서 등록 당시 1번 더트의 LL 위치로부터 당시 더트 센터를 구하고, 그 값을 각 패드 위치에서 각각 빼줌으로써 모든 패드 위치를 웨이퍼 센터로 끌어온다.
                    // 그 후 그 위치를 핀 위치로 등록한다. (즉, 프로브 카드가 카드 영역에 중심에 존재하는 것으로 믿고 초기화한다)

                    // 주의 : 패드 리스트에 첫 번째가 반드시 1번 패드가 들어 있어야 함!!!
                    foreach (DUTPadObject DutDataFromPads in substrateInfo.Pads.DutPadInfos)
                    {
                        PinData NewPin = new PinData();

                        double PadCenterX = DutDataFromPads.PadCenter.GetX();
                        double PadCenterY = DutDataFromPads.PadCenter.GetY();

                        NewPin.RelPos = new PinCoordinate(PadCenterX, PadCenterY, this.ProbeCardDevObject.PinDefaultHeight.Value);

                        double AbsPosOrgX = DutDataFromPads.MIndexLCWaferCoord.X.Value + PadCenterX + cen_die_offsetX;
                        double AbsPosOrgY = DutDataFromPads.MIndexLCWaferCoord.Y.Value + PadCenterY + cen_die_offsetY;
                        double AbsPosOrgZ = NewPin.RelPos.GetZ();

                        if (tmpPinData != null)
                        {
                            NewPin.AbsPosOrg = new PinCoordinate(AbsPosOrgX + shiftOffsetX, AbsPosOrgY + shiftOffsetY, AbsPosOrgZ + shiftOffsetZ);
                        }
                        else
                        {
                            NewPin.AbsPosOrg = new PinCoordinate(AbsPosOrgX, AbsPosOrgY, AbsPosOrgZ);
                        }

                        NewPin.AlignedOffset = new PinCoordinate(0, 0, 0);

                        if (DutDataFromPads.DutNumber <= 0)
                        {
                            DutDataFromPads.DutNumber = 1;  // It must be bigger than zero
                        }

                        var tmpIndex = this.ProbeCardDevObject.DutList.ToList().FindIndex(dut => dut.DutNumber == (int)DutDataFromPads.DutNumber);
                        NewPin.DutNumber.Value = (int)DutDataFromPads.DutNumber; // DutDataFromProbeCard;
                        NewPin.DutLLconerPos.Value = new PinCoordinate();
                        NewPin.DutLLconerPos.Value.X.Value = DutDataFromPads.MIndexLCWaferCoord.GetX();
                        NewPin.DutLLconerPos.Value.Y.Value = DutDataFromPads.MIndexLCWaferCoord.GetY();
                        NewPin.DutMacIndex.Value = new MachineIndex();
                        NewPin.DutMacIndex.Value.XIndex = this.ProbeCardDevObject.DutList[tmpIndex].MacIndex.XIndex;
                        NewPin.DutMacIndex.Value.YIndex = this.ProbeCardDevObject.DutList[tmpIndex].MacIndex.YIndex;
                        NewPin.PadGuid.Value = (Guid)DutDataFromPads.PadGuid.GetValue();
                        NewPin.PadMachineIndex = new MachineIndex(DutDataFromPads.MachineIndex.XIndex, DutDataFromPads.MachineIndex.YIndex);
                        NewPin.PinMode.Value = PINMODE.UPDATE_ONLY;
                        NewPin.PinSearchParam = new PinSearchParameter();
                        NewPin.PinSearchParam.AddLight(new LightValueParam(EnumLightType.COAXIAL, 70));
                        NewPin.PinSearchParam.AddLight(new LightValueParam(EnumLightType.AUX, 0));
                        NewPin.PinSearchParam.PinSize = new Element<Rect>();
                        NewPin.PinSearchParam.PinSize.Value = new Rect(0, 0, DutDataFromPads.PadSizeX.Value / 4, DutDataFromPads.PadSizeY.Value / 4);
                        NewPin.Result.Value = PINALIGNRESULT.PIN_NOT_PERFORMED;

                        NewPin.PinNum.Value = DutDataFromPads.PadNumber.Value;

                        NewPin.UpdatePinsHistory = new ObservableCollection<PinCoordinate>();

                        if (DutDataFromPads.PadNumber.Value == 1)
                        {
                            NewPin.IsRefPin.Value = true;
                            this.ProbeCardDevObject.RefPinNum.Value = 1;
                        }

                        LoggerManager.Debug($"[ProbeCard] GetPinDataFromPads() : #{ NewPin.PinNum.Value} X = {NewPin.AbsPos.X.Value} Y = {NewPin.AbsPos.Y.Value} Z = {NewPin.AbsPos.Z.Value})");

                        this.ProbeCardDevObject.DutList[tmpIndex].PinList.Add(NewPin);

                        // 핀 그룹 정보에 핀데이터 초기화, 아직은 그룹을 한개만 써서 0번에 집어 넣는다.
                        this.ProbeCardDevObject.PinGroupList[0].PinNumList.Add(NewPin.PinNum.Value);

                    }
                }

                int NumOfPin = 0;

                foreach (Dut dut in this.ProbeCardDevObject.DutList)
                {
                    NumOfPin += dut.PinList.Count;
                }

                LoggerManager.Debug($"[ProbeCard] GetPinDataFromPads() : Number of pin = {NumOfPin}");

                this.ProbeCardDevObject.DutAngle = 0;

                CalcPinCen(out tmpcenx, out tmpceny, out tmpdutcenx, out tmpdutceny);

                this.ProbeCardDevObject.PinCenX = tmpcenx;
                this.ProbeCardDevObject.PinCenY = tmpceny;

                this.ProbeCardDevObject.DutCenX = tmpdutcenx;
                this.ProbeCardDevObject.DutCenY = tmpdutceny;

                LoggerManager.Debug($"UpdataPinData() : pin cen = ({this.ProbeCardDevObject.PinCenX}, {this.ProbeCardDevObject.PinCenY}), dut cen = ({this.ProbeCardDevObject.DutCenX}, {this.ProbeCardDevObject.DutCenY})");

                retVal = EventCodeEnum.NONE;

                if (NumOfPin != substrateInfo.Pads.DutPadInfos.Count)
                {
                    LoggerManager.Debug($"[ProbeCard] GetPinDataFromPads() : GetPinDataFromPads Fail. Pin and Pad data is not matched.");

                    foreach (Dut dut in this.ProbeCardDevObject.DutList)
                    {
                        dut.PinList.Clear();
                        retVal = EventCodeEnum.PIN_INVALID_PAD_DATA;
                    }
                }

                LoggerManager.Debug($"[ProbeCard] GetPinDataFromPads() : End GetPinDataFromPads");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void CheckValidPinParameters()
        {
            try
            {
                int tmpAreaSize = 120;

                ICamera cam = this.VisionManager().GetCam(EnumProberCam.PIN_HIGH_CAM);

                Rect tmpRect = new Rect((cam.Param.GrabSizeX.Value / 2) - (tmpAreaSize / 2), (cam.Param.GrabSizeX.Value / 2) - (tmpAreaSize / 2), tmpAreaSize, tmpAreaSize);
                Rect tmpTipRect = new Rect((cam.Param.GrabSizeX.Value / 2) - (tmpAreaSize / 4), (cam.Param.GrabSizeX.Value / 2) - (tmpAreaSize / 4), tmpAreaSize / 2, tmpAreaSize / 2);

                LightValueParam tmpCoaxLight = new LightValueParam(EnumLightType.COAXIAL, 35);
                LightValueParam tmpAuxLight = new LightValueParam(EnumLightType.AUX, 0);

                // 핀 파라미터들의 현재 값을 확인하고 범주를 벗어나는 값이 있으면 기본값으로 초기화

                foreach (Dut dut in this.ProbeCardDevObject.DutList)
                {
                    foreach (PinData pin in dut.PinList)
                    {
                        if (pin.PinSearchParam.BlobThreshold.Value <= 0) pin.PinSearchParam.BlobThreshold.Value = 70;

                        if (pin.PinSearchParam.MaxBlobSizeX.Value <= 0) pin.PinSearchParam.MaxBlobSizeX.Value = 80;

                        if (pin.PinSearchParam.MaxBlobSizeY.Value <= 0) pin.PinSearchParam.MaxBlobSizeY.Value = 80;

                        if (pin.PinSearchParam.MinBlobSizeX.Value <= 0) pin.PinSearchParam.MinBlobSizeX.Value = 20;

                        if (pin.PinSearchParam.MinBlobSizeY.Value <= 0) pin.PinSearchParam.MinBlobSizeY.Value = 20;

                        if (pin.PinSearchParam.PinFocusingArea.Value.Width <= 0 || pin.PinSearchParam.PinFocusingArea.Value.Height <= 0)
                            pin.PinSearchParam.PinFocusingArea.Value = tmpRect;

                        if (pin.PinSearchParam.PinSize.Value.Width <= 0 || pin.PinSearchParam.PinSize.Value.Height <= 0)
                            pin.PinSearchParam.PinSize.Value = tmpTipRect;

                        if (pin.PinSearchParam.SearchArea.Value.Width <= 0 || pin.PinSearchParam.SearchArea.Value.Height <= 0)
                            pin.PinSearchParam.SearchArea.Value = tmpRect;


                        if (pin.PinSearchParam.LightForTip.Count > 2)
                        {
                            var tmpLight = pin.PinSearchParam.LightForTip.FindAll(l => l.Type.Value == tmpCoaxLight.Type.Value);
                            tmpCoaxLight.Value.Value = tmpLight.LastOrDefault().Value.Value;
                            tmpLight = pin.PinSearchParam.LightForTip.FindAll(l => l.Type.Value == tmpAuxLight.Type.Value);
                            tmpAuxLight.Value.Value = tmpLight.LastOrDefault().Value.Value;


                            pin.PinSearchParam.LightForTip.Clear();
                        }

                        // pin.PinSearchParam.LightForTip.Clear();

                        if (pin.PinSearchParam.LightForTip.Count() <= 0)
                        {
                            pin.PinSearchParam.LightForTip.Add(tmpCoaxLight);
                            pin.PinSearchParam.LightForTip.Add(tmpAuxLight);
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CalcPinCen(out double PosX, out double PosY, out double DutX, out double DutY)
        {
            long pincount = 0;
            double sumx = 0;
            double sumy = 0;


            // 현재 핀 위치들로부터 핀 센터와 더트 센터를 계산하어 리턴한다.

            try
            {
                // 핀 센터 계산
                foreach (Dut dut in this.ProbeCardDevObject.DutList)
                {
                    foreach (PinData pin in dut.PinList)
                    {
                        //LoggerManager.Debug($"pin pos = ({pin.AbsPos.X.Value} {pin.AbsPos.Y.Value})");
                        sumx += pin.AbsPos.X.Value;
                        sumy += pin.AbsPos.Y.Value;

                        pincount += 1;
                    }
                }

                if (pincount > 0)
                {
                    PosX = sumx / pincount;
                    PosY = sumy / pincount;
                }
                else
                {
                    PosX = 0;
                    PosY = 0;
                }

                this.WaferAligner().UpdatePadCen();

                double cen_diffX = 0;
                double cen_diffY = 0;
                int tmpDutIndex = -1;
                IPinData tmpPinData;

                // 더트 센터 계산
                // 더트의 센터는 핀 위치만으로는 알 수 없다. 웨이퍼 얼라인 및 패드 등록이 완료된 시점에서만 정상적으로 계산이 가능하다.
                if (pincount > 0)
                {
                    var tmpPinIndex = this.StageSupervisor().WaferObject.GetSubsInfo().Pads.GetPadArrayIndex(0);   // 첫번째 패드를 가지고 있는 배열의 인덱스

                    IDut dutObj = GetDutFromPinNum(tmpPinIndex + 1);
                    if (dutObj != null)
                        tmpDutIndex = dutObj.DutNumber - 1;

                    var refDut = new int();
                    refDut = tmpDutIndex;

                    tmpPinData = this.StageSupervisor().ProbeCardInfo.GetPin(0);

                    if (tmpDutIndex >= 0 && tmpPinData != null)
                    {
                        // DUT 좌표계 좌측하단 원점에서부터 첫번째 패드가 포함된 더트까지의 거리
                        // 위의 값을
                        // (1번 패드와 대응 되는 핀의 측정된 위치를 넣어 둠)
                        cen_diffX = tmpPinData.AbsPos.X.Value;
                        // 핀 위치값에 패드를 포함한 더트의 좌측 하단에서부터 패드 까지의 거리를 더해서 이 핀을 포함한 더트의 좌측 하단을 구한다.
                        cen_diffX -= this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[tmpPinIndex].PadCenter.X.Value; // + this.StageSupervisor().WaferObject.GetSubsInfo().DieXClearance.Value/2);    
                        // 더트좌표계 원점(좌하)으로 옮긴다.
                        cen_diffX += (0 - this.ProbeCardDevObject.DutList[refDut].MacIndex.XIndex) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;
                        // 더트 전체 사이즈의 절반만큼 시프트 시켜 중심점을 구한다.
                        cen_diffX += (double)(this.ProbeCardDevObject.DutIndexSizeX / 2.0) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Width.Value;

                        cen_diffY = tmpPinData.AbsPos.Y.Value;
                        cen_diffY -= this.StageSupervisor().WaferObject.GetSubsInfo().Pads.DutPadInfos[tmpPinIndex].PadCenter.Y.Value; // + this.StageSupervisor().WaferObject.GetSubsInfo().DieYClearance.Value/2);                        
                        cen_diffY += (0 - this.ProbeCardDevObject.DutList[refDut].MacIndex.YIndex) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;  // 더트좌표계 원점(좌하)으로 옮긴다.
                        cen_diffY += (double)(this.ProbeCardDevObject.DutIndexSizeY / 2.0) * this.StageSupervisor().WaferObject.GetSubsInfo().ActualDieSize.Height.Value;

                        cen_diffX = (cen_diffX - tmpPinData.AbsPos.X.Value) * Math.Cos(Math.PI * this.ProbeCardDevObject.DutAngle / 180.0)
                            - (cen_diffY - tmpPinData.AbsPos.Y.Value) * Math.Sin(Math.PI * this.ProbeCardDevObject.DutAngle / 180.0)
                            + tmpPinData.AbsPos.X.Value;
                        cen_diffY = (cen_diffX - tmpPinData.AbsPos.X.Value) * Math.Sin(Math.PI * this.ProbeCardDevObject.DutAngle / 180.0)
                            + (cen_diffY - tmpPinData.AbsPos.Y.Value) * Math.Cos(Math.PI * this.ProbeCardDevObject.DutAngle / 180.0)
                            + tmpPinData.AbsPos.Y.Value;
                        DutX = cen_diffX;
                        DutY = cen_diffY;

                        var dutCen = this.CoordinateManager().GetRotatedPoint(
                            new MachineCoordinate(cen_diffX, cen_diffY),
                            new MachineCoordinate(tmpPinData.AbsPos.X.Value, tmpPinData.AbsPos.Y.Value),
                            this.ProbeCardDevObject.DutAngle);
                        DutX = dutCen.X.Value;
                        DutY = dutCen.Y.Value;
                    }
                    else
                    {
                        DutX = 0;
                        DutY = 0;
                    }
                }
                else
                {
                    DutX = 0;
                    DutY = 0;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                PosX = 0;
                PosY = 0;
                DutX = 0;
                DutY = 0;
            }
        }

        public double CalcHighestPin()
        {
            double highest = -10000000;

            List<int> PinNumberlist = new List<int>();

            try
            {
                int pincnt = 0;

                foreach (Dut dut in this.ProbeCardDevObject.DutList)
                {
                    foreach (PinData pin in dut.PinList)
                    {
                        //if (pin.Result.Value == PINALIGNRESULT.PIN_PASSED)
                        //{
                        //    if (highest < pin.AbsPos.Z.Value)
                        //    {
                        //        highest = pin.AbsPos.Z.Value;
                        //    }

                        //    PinNumberlist.Add(pin.PinNum.Value);
                        //    pincnt++;
                        //}

                        if (highest < pin.AbsPos.Z.Value)
                        {
                            highest = pin.AbsPos.Z.Value;
                        }

                        PinNumberlist.Add(pin.PinNum.Value);
                        pincnt++;
                    }
                }

                //string numbers = string.Empty;

                //if (pincnt > 0)
                //{
                //    numbers = string.Join(", ", PinNumberlist);
                //}
                //else
                //{
                //    highest = -9000;
                //}

                //LoggerManager.Debug($"Used Pin No. {numbers}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return highest;
        }

        public double CalcLowestPin()
        {
            int pincnt = 0;
            double Lowest = 10000000;

            List<int> PinNumberlist = new List<int>();

            try
            {
                LoggerManager.Debug($"Calculate Pin Height(Lowest)");

                foreach (Dut dut in this.ProbeCardDevObject.DutList)
                {
                    foreach (PinData pin in dut.PinList)
                    {
                        //if (pin.Result.Value == PINALIGNRESULT.PIN_PASSED)
                        //{
                        //    if (Lowest > pin.AbsPos.Z.Value)
                        //    {
                        //        Lowest = pin.AbsPos.Z.Value;
                        //    }

                        //    PinNumberlist.Add(pin.PinNum.Value);
                        //    pincnt++;
                        //}

                        if (Lowest > pin.AbsPos.Z.Value)
                        {
                            Lowest = pin.AbsPos.Z.Value;
                        }

                        PinNumberlist.Add(pin.PinNum.Value);
                        pincnt++;

                    }
                }

                //string numbers = string.Empty;

                //if (pincnt > 0)
                //{
                //    numbers = string.Join(", ", PinNumberlist);
                //}
                //else
                //{
                //    Lowest = -9000;
                //}

                //LoggerManager.Debug($"Used Pin No. {numbers}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return Lowest;

        }

        public double CalcPinAverageHeight()
        {
            double sum = 0;
            double avg = 0;

            List<int> PinNumberlist = new List<int>();

            try
            {
                int pincnt = 0;

                foreach (Dut myDutList in this.ProbeCardDevObject.DutList)
                {
                    foreach (PinData pin in myDutList.PinList)
                    {
                        //if (pin.Result.Value == PINALIGNRESULT.PIN_PASSED)
                        //{
                        //    sum += pin.AbsPos.Z.Value;

                        //    PinNumberlist.Add(pin.PinNum.Value);
                        //    pincnt++;
                        //}

                        sum += pin.AbsPos.Z.Value;

                        PinNumberlist.Add(pin.PinNum.Value);
                        pincnt++;

                    }
                }

                //if (pincnt == 0)
                //{
                //    foreach (Dut tmpDutList in this.ProbeCardDevObject.DutList)
                //    {
                //        foreach (PinData pin in tmpDutList.PinList)
                //        {
                //            if (pin.Result.Value != PINALIGNRESULT.PIN_BLOB_FAILED && pin.Result.Value != PINALIGNRESULT.PIN_BLOB_TOLERANCE && pin.Result.Value != PINALIGNRESULT.PIN_FOCUS_FAILED &&
                //                pin.Result.Value != PINALIGNRESULT.PIN_OVER_TOLERANCE)
                //            {
                //                sum += pin.AbsPos.Z.Value;
                //                pincnt++;
                //            }
                //        }
                //    }
                //}

                //string numbers = string.Empty;

                if (pincnt > 0)
                {
                    avg = sum / pincnt;

                    //numbers = string.Join(", ", PinNumberlist);
                }
                else
                {
                    avg = this.CoordinateManager().StageCoord.PinReg.PinRegMin.Value;
                }

                //LoggerManager.Debug($"CalcPinAverageHeight() : Used Pin Count : {pincnt}");

                //if (pincnt < 1)
                //{
                //    avg = -9000;
                //}
                //else
                //{
                //    avg = sum / pincnt;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return avg;
        }

        public PinCoordinate GetProbeCardCenterPos()
        {
            try
            {
                int PinCnt = 0;
                PinCoordinate SumPinCoord = new PinCoordinate(0, 0, 0);
                PinCoordinate PinCenCoord = new PinCoordinate(0, 0, 0);

                foreach (Dut dut in this.ProbeCardDevObject.DutList)
                {
                    PinCnt += dut.PinList.Count;
                    foreach (PinData pin in dut.PinList)
                    {
                        SumPinCoord.X.Value += pin.AbsPos.X.Value;
                        SumPinCoord.Y.Value += pin.AbsPos.Y.Value;
                        SumPinCoord.Z.Value += pin.AbsPos.Z.Value;
                    }
                }
                if (PinCnt < 1)
                {
                    PinCenCoord = new PinCoordinate(0, 0, -9000);
                }
                else
                {
                    PinCenCoord = new PinCoordinate((SumPinCoord.GetX() / PinCnt), (SumPinCoord.GetY() / PinCnt), (SumPinCoord.GetZ() / PinCnt));
                }
                return new PinCoordinate(PinCenCoord.GetX(), PinCenCoord.GetY(), PinCenCoord.GetZ());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public PinCoordinate GetDutCenterPos()
        {
            try
            {
                long minx = 9999999;
                long miny = 9999999;
                long maxx = -999999;
                long maxy = -999999;

                double maxposX = 0;
                double maxposY = 0;

                double minposX = 0;
                double minposY = 0;

                double cenX = 0;
                double cenY = 0;

                foreach (Dut dut in this.ProbeCardDevObject.DutList)
                {
                    if (dut.MacIndex.XIndex < minx)
                    {
                        minx = dut.MacIndex.XIndex;
                    }
                    if (dut.MacIndex.XIndex > maxx)
                    {
                        maxx = dut.MacIndex.XIndex;
                    }
                    if (dut.MacIndex.YIndex < miny)
                    {
                        miny = dut.MacIndex.YIndex;
                    }
                    if (dut.MacIndex.YIndex > maxy)
                    {
                        maxy = dut.MacIndex.YIndex;
                    }
                }

                foreach (Dut dut in this.ProbeCardDevObject.DutList)
                {
                    if (dut.MacIndex.XIndex == minx && dut.MacIndex.YIndex == miny)
                    {
                        minposX = dut.RefCorner.GetX();
                        minposY = dut.RefCorner.GetY();
                    }
                    if (dut.MacIndex.XIndex == maxx && dut.MacIndex.YIndex == maxy)
                    {
                        maxposX = dut.RefCorner.GetX() + this.ProbeCardDevObject.DutSizeX.Value;
                        maxposY = dut.RefCorner.GetY() + this.ProbeCardDevObject.DutSizeY.Value;
                    }
                }

                cenX = (minposX + maxposX) / 2;
                cenY = (minposY + maxposY) / 2;

                return new PinCoordinate(cenX, cenY, this.GetPinAverageHeight());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public double GetPinAverageHeight()
        {
            double avg = 0;
            try
            {
                double sum = 0;
                int pincnt = 0;
                foreach (Dut dut in this.ProbeCardDevObject.DutList)
                {
                    foreach (PinData pin in dut.PinList)
                    {
                        if (pin.Result.Value == PINALIGNRESULT.PIN_PASSED)
                        {
                            sum += pin.AbsPos.Z.Value;
                            pincnt++;
                        }
                    }
                }
                if (pincnt == 0)
                {
                    foreach (Dut dut in this.ProbeCardDevObject.DutList)
                    {
                        foreach (PinData pin in dut.PinList)
                        {
                            if (pin.Result.Value != PINALIGNRESULT.PIN_BLOB_FAILED && pin.Result.Value != PINALIGNRESULT.PIN_BLOB_TOLERANCE && pin.Result.Value != PINALIGNRESULT.PIN_FOCUS_FAILED &&
                                pin.Result.Value != PINALIGNRESULT.PIN_OVER_TOLERANCE)
                            {
                                sum += pin.AbsPos.Z.Value;
                                pincnt++;
                            }
                        }
                    }
                }
                if (pincnt < 1)
                {
                    avg = -9000;
                }
                else
                {
                    avg = sum / pincnt;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return avg;
        }

        #endregion

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            if (IsInitialized)
                return retval;

            try
            {
                _DisplayPinList = new List<IPinData>();
                _ProberCardCompensation = new PinCoordinate();
                _SelectedPin = new PinData();

                _CandidatePinGroupList = new List<IGroupData>();

                //ContactCnt = 0;
                ProbeCardChangedToggle = new Element<bool>();
                AlignState = new Element<AlignStateEnum>();

                retval = EventCodeEnum.NONE;

                IsInitialized = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            SetAlignState(AlignStateEnum.IDLE);

            return retval;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;

                retVal = this.LoadParameter(ref tmpParam, typeof(ProbeCardDevObject));

                this.ProbeCardDevObject = tmpParam as ProbeCardDevObject;

                //this.ProbeCardDevObject?.SetEventToElement();

                this.PinAligner()?.ConnectValueChangedEventHandler();

                retVal = this.Init();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                if (this.ProbeCardSysObjectRef == null)
                {
                    this.ProbeCardSysObject = new ProbeCardSysObject();
                }

                retVal = this.LoadParameter(ref tmpParam, typeof(ProbeCardSysObject));
                this.ProbeCardSysObject = tmpParam as ProbeCardSysObject;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(this.ProbeCardDevObject);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                string fullPath = this.FileManager().GetSystemParamFullPath(this.ProbeCardSysObjectRef.FilePath, this.ProbeCardSysObjectRef.FileName);
                retVal = this.SaveParameter(this.ProbeCardSysObject, fixFullPath: fullPath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            retVal = EventCodeEnum.NONE;
            return retVal;
        }

        public void SetElementMetaData()
        {
            ProbeCardChangedToggle.AssociateElementID = "C2,D896";
        }


        // #Hynix_Merge: 검토 필요, 이부분 CardChange SystemParam으로 변경된걸로 알고 있는데 주석하는게 맞는지 확인 필요.
        //public void SetProbeCardID(string probeCardID)
        //{
        //    if (ProbeCardDevObject != null && ProbeCardDevObject.ProbeCardID != null)
        //    {
        //        ProbeCardDevObject.ProbeCardID.Value = probeCardID;
        //        SaveDevParameter();
        //    }
        //    else if (ProbeCardDevObject != null && ProbeCardDevObject.ProbeCardID == null)
        //    {
        //        ProbeCardDevObject.ProbeCardID = new Element<string>();
        //        ProbeCardDevObject.ProbeCardID.Value = probeCardID;
        //        SaveDevParameter();
        //    }
        //    SaveSysParameter();
        //    this.GEMModule().GetPIVContainer().SetProberCardID(ProbeCardDevObject.ProbeCardID.Value);
        //}

        //public string GetProbeCardID()
        //{
        //    if (ProbeCardDevObject.ProbeCardID != null)
        //    {
        //        return ProbeCardDevObject.ProbeCardID.Value;
        //    }
        //    else
        //    {
        //        ProbeCardDevObject.ProbeCardID = new Element<string>();
        //        return "";
        //    }
        //}

        // ==========================================


        #region ==> 주석 모음

        //private Element<double> _PinPadOptimizeAngleLimit = new Element<double>();
        //public Element<double> PinPadOptimizeAngleLimit
        //{
        //    get { return _PinPadOptimizeAngleLimit; }
        //    set
        //    {
        //        if (value != _PinPadOptimizeAngleLimit)
        //        {
        //            _PinPadOptimizeAngleLimit = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //#region ==> PinPadMatchTolerenceX
        //private Element<double> _PinPadMatchTolerenceX = new Element<double>();
        //public Element<double> PinPadMatchTolerenceX
        //{
        //    get { return _PinPadMatchTolerenceX; }
        //    set
        //    {
        //        if (value != _PinPadMatchTolerenceX)
        //        {
        //            _PinPadMatchTolerenceX = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //#endregion

        //#region ==> PinPadMatchTolerenceY
        //private Element<double> _PinPadMatchTolerenceY = new Element<double>();
        //public Element<double> PinPadMatchTolerenceY
        //{
        //    get { return _PinPadMatchTolerenceY; }
        //    set
        //    {
        //        if (value != _PinPadMatchTolerenceY)
        //        {
        //            _PinPadMatchTolerenceY = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //#endregion

        //#region ==> HighestPin
        //[NonSerialized]
        //private double _HighestPin;
        //[XmlIgnore, JsonIgnore]
        //[ParamIgnore]
        //public double HighestPin
        //{
        //    get
        //    {
        //        double highest = -10000000;
        //        int pincnt = 0;
        //        foreach (Dut dut in DutList)
        //        {
        //            foreach (PinData pin in dut.PinList)
        //            {
        //                if (highest < pin.AbsPos.Z.Value)
        //                {
        //                    highest = pin.AbsPos.Z.Value;
        //                }
        //                pincnt++;
        //            }
        //        }
        //        if (pincnt < 1)
        //        {
        //            highest = -9000;
        //        }
        //        return highest;
        //    }
        //}
        //#endregion

        //#region ==> LowestPin
        //[NonSerialized]
        //private double _LowestPin;
        //[XmlIgnore, JsonIgnore]
        //[ParamIgnore]
        //public double LowestPin
        //{
        //    get
        //    {
        //        int pincnt = 0;
        //        double Lowest = 10000000;
        //        foreach (Dut dut in DutList)
        //        {
        //            foreach (PinData pin in dut.PinList)
        //            {
        //                if (Lowest > pin.AbsPos.Z.Value)
        //                {
        //                    Lowest = pin.AbsPos.Z.Value;
        //                }
        //                pincnt++;
        //            }
        //        }
        //        if (pincnt < 1)
        //        {
        //            Lowest = -9000;
        //        }
        //        return Lowest;
        //    }
        //}
        //#endregion

        ////==> 사용하지만 필요 없어 보이는 데이터

        //#region ==> ProbeCardAngle
        //private double _ProbeCardAngle;
        //public double ProbeCardAngle
        //{
        //    get { return _ProbeCardAngle; }
        //    set
        //    {
        //        if (value != _ProbeCardAngle)
        //        {
        //            _ProbeCardAngle = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //#endregion
        //==> 사용하지 않지만 필요해 보이는 데이터

        //#region ==> AlignedPinCenterPos
        //private PinCoordinate _AlignedPinCenterPos = new PinCoordinate();
        //public PinCoordinate AlignedPinCenterPos
        //{
        //    get
        //    {
        //        int PinCnt = 0;
        //        PinCoordinate SumPinCoord = new PinCoordinate(0, 0, 0);
        //        foreach (Dut dut in DutList)
        //        {
        //            PinCnt += dut.PinList.Count;
        //            foreach (PinData pin in dut.PinList)
        //            {
        //                SumPinCoord.X.Value += pin.AbsPos.X.Value;
        //                SumPinCoord.Y.Value += pin.AbsPos.Y.Value;
        //                SumPinCoord.Z.Value += pin.AbsPos.Z.Value;
        //            }
        //        }
        //        return new PinCoordinate((SumPinCoord.GetX() / PinCnt), (SumPinCoord.GetY() / PinCnt), (SumPinCoord.GetZ() / PinCnt));
        //    }
        //}
        //#endregion

        #endregion

    }
}
