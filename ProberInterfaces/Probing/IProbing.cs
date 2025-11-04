using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProberInterfaces.Param;
using ProberErrorCode;
using ProberInterfaces.State;
using ProberInterfaces.BinData;
using System.ServiceModel;
using System;
using ProberInterfaces.Loader.RemoteDataDescription;

namespace ProberInterfaces
{
    [ServiceContract]
    public interface IProbingModule :   IStateModule,
                                        IHasSysParameterizable,
                                        IHasDevParameterizable
    {

        [OperationContract]
        IParam GetProbingDevIParam(int idx=-1);
        [OperationContract]
        byte[] GetProbingDevParam(int idx = -1);
        //[OperationContract]
        //void SetProbingDevParam(byte[] param);
        [OperationContract]
        void SetProbingDevParam(IParam param, int idx = -1);

        EventCodeEnum SaveDevParameter(IParam param, int idx = -1);

        [OperationContract]
        byte[] GetBinDevParam();
      
        IParam              ProbingModuleDevParam_IParam    { get; set; }
        IParam              ProbingModuleSysParam_IParam    { get; set; }
        PinCoordinate       PinZeroPos                      { get; set; }
        ProbingInfo         ProbingProcessStatus            { get; set; }
        EnumProbingState    ProbingStateEnum                { get; }
        MachineIndex        ProbingLastMIndex               { get; }
        EnumProbingState    PreProbingStateEnum             { get; set; }

        double              ZClearence                      { get; set; }
        double              OverDrive                       { get; set; }
        double              FirstContactHeight              { get; set; }
        double              AllContactHeight                { get; set; }
        bool                IsFirstZupSequence              { get; set; }
        int                 ProbingMXIndex                  { get; }
        int                 ProbingMYIndex                  { get; }

        bool IsReservePause { get; set; }
        bool ProbingDryRunFlag { get; set; }
        bool StilProbingZUpFlag { get; set; }

        List<IDeviceObject> GetUnderDutList(MachineIndex mCoord);
        CatCoordinates      GetPMShifhtValue();
        EventCodeEnum       GetUnderDutDices(MachineIndex mCoord);
        EventCodeEnum       ProbingSequenceTransfer(int moveIdx);
        EventCodeEnum       ProbingSequenceTransfer(MachineIndex curMI, int remainSeqCnt);
        double              CalculateZClearenceUsingOD(double overDrive, double zClearence);
        bool                IsLotHasSuspendedState();
        void                ProbingRestart();
        EventCodeEnum       InnerStateTransition(IInnerState state);

        double GetDeflectX();
        double GetDeflectY();
        double GetInclineZHor();
        double GetInclineZVer();
        double GetSquarenceValue();
        double GetTwistValue();
        OverDriveStartPositionType GetOverDriveStartPosition();

        void SetProbingUnderdut(ObservableCollection<IDeviceObject> UnderDutDevs);
        void SetDeflectX(double value);
        void SetDeflectY(double value);
        void SetInclineZHor(double value);
        void SetInclineZVer(double value);
        void SetProbeShiftValue(CatCoordinates shiftvalue);
        void SetProbeTempShiftValue(CatCoordinates shiftvalue);
        void SetProbingMachineIndexToCenter();
        void SetSquarenceValue(double value);

        void SetLastProbingMIndex(MachineIndex index);
        void SetTwistValue(double value);
        void GetCPCValues(double position, out double z0, out double z1, out double z2);
        CatCoordinates GetSetTemperaturePMShifhtValue();

        Dictionary<double, CatCoordinates> GetTemperaturePMShifhtTable();

        //IRetest RetestModule { get; set; }
        //IBinDeviceParam GetBinDevParam();
        //Dictionary<int, EnableEnum> GetSortedBinInfos();
        ObservableCollection<IChuckPlaneCompParameter> GetCPCValues();
        void AddCPCParameter(IChuckPlaneCompParameter cpcparam);
        ProbingEndReason ProbingEndReason { get; }
        EventCodeEnum GetStagePIVProbingData(ref long XIndex, ref long YIndex, UserIndex userindex, ref string full_site);
        bool DutIsInRange(long xindex, long yindex);
        bool GetEnablePTPAEnhancer();

        bool NeedRetest(long xindex, long yindex);
        bool NeedRetestbyBIN(RetestTypeEnum type, long xindex, long yindex);
        //bool GetRetestEnable(RetestTypeEnum type, long xindex, long yindex);
        List<IBINInfo> GetBinInfos();
        EventCodeEnum SaveBinDevParam();
        EventCodeEnum SetBinInfos(List<IBINInfo> binInfos);

        bool IsTestedDIE(long xindex, long yindex);

        EventCodeEnum ResetOnWaferInformation();
        EventCodeEnum ClearUnderDutDevs();
        void SetProbingEndState(ProbingEndReason probingEndReason, EnumWaferState WaferState = EnumWaferState.UNDEFINED);
        DateTime GetZupStartTime();
        MarkShiftValues GetUserSystemMarkShiftValue();
    }

    public interface IChuckPlaneCompParameter:IParamNode
    {
        double Position { get; set; }
        double Z0 { get; set; }
        double Z1 { get; set; }
        double Z2 { get; set; }
    }
}
