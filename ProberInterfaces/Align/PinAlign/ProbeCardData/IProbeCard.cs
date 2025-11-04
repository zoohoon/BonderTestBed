using ProberErrorCode;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign.ProbeCardData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static ProberInterfaces.Param.PinSearchParameter;

namespace ProberInterfaces
{
    public interface IProbeCard : IHasDevParameterizable, IHasComParameterizable, IAlignModule,
                                  IFactoryModule, IModuleParameter, IParamNode, IHasSysParameterizable
    {
        /// <summary>
        /// ProbeCard의 Device Parameter 정보를 담고 있는 속성. ProbeCardDevObject 동일.
        /// </summary>
        IProbeCardDevObject ProbeCardDevObjectRef { get; }
        IProbeCardSysObject ProbeCardSysObjectRef { get; }
        MachineIndex SelectedCoordM { get; set; }
        MachineIndex FirstDutM { get; set; }
        ObservableCollection<IDut> CandidateDutList { get; set; }
        List<IGroupData> CandidatePinGroupList { get; set; }
        List<IPinData> DisplayPinList { get; set; }
        IDut[] DisplayDutArr { get; set; }
        IPinData SelectedPin { get; set; }
        Element<bool> ProbeCardChangedToggle { get; set; }
        //int ContactCnt { get; set; }
        double LineOffsetX { get; set; }
        double LineOffsetY { get; set; }

        MachineIndex GetRefOffset(int siteindex);
        PinCoordinate GetProbeCardCenterPos();
        List<IPinData> GetPinList();
        AlignStateEnum GetPinPadAlignState();
        IPinData GetPin(int pinNum);
        IDut GetDutFromPinNum(int pinNum);
        double CalcHighestPin();
        double CalcLowestPin();
        double CalcPinAverageHeight();
        double GetPinAverageHeight();        
        int GetPinArrayIndex(int pinnum);
        int GetPinCount();
        void CalcPinCen(out double PosX, out double PosY, out double DutX, out double DutY);
        void CheckValidPinParameters();
        void GetArrayIndex(int pinNum, out int dut_arrayNum, out int pin_arrayNum);
        void SetPinPadAlignState(AlignStateEnum state);

        EventCodeEnum ShiftPindata(double offsetX, double offsetY, double offsetZ);
        EventCodeEnum GetPinDataFromPads();
        EventCodeEnum CheckPinPadParameterValidity();
        Element<bool> PinSetupChangedToggle { get; set; }

        //Element<double> PinPadMatchTolerenceX { get; set;}
        //Element<double> PinPadMatchTolerenceY { get; set; }
        //Element<double> PinPadOptimizeAngleLimit { get; set; }
        //double ProbeCardAngle { get; set; }
        //double LowestPin { get; }
        //double HighestPin { get; }
    }

    public interface IProbeCardDevObject : IDeviceParameterizable
    {
        AsyncObservableCollection<IDut> DutList { get; set; }
        Element<double> DutSizeX { get; set; }
        Element<double> DutSizeY { get; set; }
        PinCoordinate ProbeCardCenterOffset { get; set; }
        List<IGroupData> PinGroupList { get; set; }
        Element<int> RefPinNum { get; set; }
        Element<double> PinDefaultHeight { get; set; }
        double PinHeight { get; set; }
        double DutCenX { get; set; }
        double DutCenY { get; set; }
        double PinCenX { get; set; }
        double PinCenY { get; set; }
        double DutAngle { get; set; }
        double DutDiffX { get; set; }
        double DutDiffY { get; set; }
        double DiffX { get; set; }
        double DiffY { get; set; }
        double DiffAngle { get; set; }
        int DutIndexSizeX { get; set; }
        int DutIndexSizeY { get; set; }
        Element<PROBECARD_TYPE> ProbeCardType { get; set; }
        List<AlignKeyLibPack> AlignKeyHighLib { get; set; }
        List<AlignKeyLibPack> AlignKeyLowLib { get; set; }
        List<AlignKeyInfo> AlignKeyLow { get; set; }
        Element<double> AlignKeyLowCenX { get; set; }
        Element<double> AlignKeyLowCenY { get; set; }
        Element<double> AlignKeyLowCenZ { get; set; }
        Element<double> AlignKeyLowAngle { get; set; }
        Element<double> LowAlignKeyFormPinZoffet { get; set; }
        //Element<string> ProbeCardID { get; set; }
        Element<int> TouchdownCount { get; set; }
    }
    public interface IProbeCardSysObject : ISystemParameterizable
    {
        ProbercardBackupinfo ProbercardBackupinfo { get; set; }
        Element<bool> UsePinPosWithCardID { get; set; }

        EventCodeEnum ProbercardinfoClear();
    }
    public class ProbercardBackupinfo
    {
        private string _ProbeCardID;
        public string ProbeCardID
        {
            get { return _ProbeCardID; }
            set { _ProbeCardID = value; }
        }

        private int _TotalDutCnt;
        public int TotalDutCnt
        {
            get { return _TotalDutCnt; }
            set { _TotalDutCnt = value; }
        }
        private int _TotalPinCnt;
        public int TotalPinCnt
        {
            get { return _TotalPinCnt; }
            set { _TotalPinCnt = value; }
        }

        private MachineIndex _FirstDutMI;
        public MachineIndex FirstDutMI
        {
            get { return _FirstDutMI; }
            set { _FirstDutMI = value; }
        }
        
        private List<BackupPinData> _BackupPinDataList;
        public List<BackupPinData> BackupPinDataList
        {
            get { return _BackupPinDataList; }
            set { _BackupPinDataList = value; }
        }
        
        public ProbercardBackupinfo() 
        {
        }
    }

    public class BackupPinData
    {
        private int _DutNo;
        public int DutNo
        {
            get { return _DutNo; }
            set { _DutNo = value; }
        }

        private int _PinNo;
        public int PinNo
        {
            get { return _PinNo; }
            set { _PinNo = value; }
        }

        private PinCoordinate _BackupAbsPos;
        public PinCoordinate BackupAbsPos
        {
            get { return _BackupAbsPos; }
            set { _BackupAbsPos = value; }
        }
    }
}
