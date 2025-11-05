using System;
using System.Collections.Generic;
using System.Linq;

namespace ProberInterfaces.PinAlign
{
    using ProberInterfaces.Param;
    using System.Collections.ObjectModel;
    using ProberInterfaces.Align;
    using ProberInterfaces.Enum;
    using ProberInterfaces.PnpSetup;
    using ProberErrorCode;
    using ProberInterfaces.Template;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

    [ServiceContract]
    public interface IPinAligner : IStateModule, IPnpSetupScreen, ITemplateStateModule, IHasDevParameterizable, IManualOPReadyAble
    {
        //        AlignState PinAlignState { get; set; }
        //PinAlignParameters PinAlignParam { get; set; }
        IParam PinAlignDevParam { get; set; }
        //PinAlignInfo AlignInfo { get; set; }

        PARecoveryModule RecoveryModules { get; set; }

        //bool IsChangedSource { get; set; }

        PINALIGNSOURCE PinAlignSource { get; set; }
        bool UseSoakingSamplePinAlign { get; set; }

        DateTime LastAlignDoneTime { get; set; }
        IPinAlignInfo PinAlignInfo { get; set; }
        EventCodeEnum SavePinAlignDevParam();
        //EventCodeEnum SavePinAlignParamObject();
        //Task<FocusingRet> FocusingAsync(EnumProberCam cam, Rect roi, CancellationToken token);
        //    EnumPinSeekRet PinCenterDetect(int sizeX, int sizeY, out Point resultPoint, Rect roi = new Rect());
        void GetTransformationPin(PinCoordinate crossPin, PinCoordinate orgPin, double degree, out PinCoordinate updatePin);
        double GetDegree(PinCoordinate pivot, PinCoordinate pointOld, PinCoordinate pointNew);
        bool IsExistParamFile(String paramPath);
        //EventCodeEnum DrawPinOverlay(ICamera cam);
        EventCodeEnum DrawDutOverlay(ICamera cam);
        //EventCodeEnum StopDrawPinOverlay(ICamera cam);
        EventCodeEnum StopDrawDutOverlay(ICamera cam);

        EventCodeEnum StateTranstionSetup();
        PinCoordinate SoakingPinTolerance { get; set; }
        PinAlignInnerStateEnum GetPAInnerStateEnum();
        //bool[] CheckDoWaferInterval { get; set; }
        //int NextWaferOrder { get; set; }
        ISinglePinAlign SinglePinAligner { get; set; }
        //EventCodeEnum DoPinAlignProcess();
        bool IsRecoveryStarted { get; set; }
        bool GetPlaneAdjustEnabled();
        ITemplateFileParam SinglePinTemplateParameter { get; set; }
        TemplateStateCollection SinglePinTemplate { get; set; }

        [OperationContract]
        bool IsServiceAvailable();

        [OperationContract]
        IParam GetPinAlignerIParam();

        [OperationContract]
        void SetPinAlignerIParam(byte[] param);

        [OperationContract]
        byte[] GetPinAlignerParam();

        EventCodeEnum ChangeAlignKeySetupControlFlag(PinSetupMode mode, bool flag);
        bool GetAlignKeySetupControlFlag(PinSetupMode mode);
        void ConnectValueChangedEventHandler();

        string MakeFailDescription();

        bool WaferTransferRunning { get; set; }
        bool PIN_ALIGN_Failure { get; set; }
        bool Each_Pin_Failure { get; set; }
        EventCodeEnum CollectElement();

        TIP_SIZE_VALIDATION_RESULT Validation_Pin_Tip_Size(BlobResult singlealignpin, ISinglePinAlign singlepinalign, double ratio_x, double ratio_y);
        List<PinSizeValidateResult> ValidPinTipSize_Original_List { get; set; }
        List<PinSizeValidateResult> ValidPinTipSize_OutOfSize_List { get; set; }
        List<PinSizeValidateResult> ValidPinTipSize_SizeInRange_List { get; set; }
        EventCodeEnum PinAlignResultServerUpload(List<PinSizeValidateResult> originList, List<PinSizeValidateResult> outList, List<PinSizeValidateResult> inList);
        bool CheckSubModulesInTheSkipstate(IProcessingModule subModule);
    }

    public class PinSizeValidateResult
    {
        public int pinNum { get; set; }
        public string filePath { get; set; }
        public PinSizeValidateResult(int pinnum)
        {
            this.pinNum = pinnum;
        }
    }
    public interface IPinAlignInfo
    {
        PinAlignResultes AlignResult { get; set; }
    }

    public interface IPinAlignResultes
    {
        long TotalAlignPinCount { get; set; }
        long TotalPassPinCount { get; set; }
        long TotalFailPinCount { get; set; }
        int PassPercentage { get; set; }
        EventCodeEnum Result { get; set; }
        List<EachPinResult> EachPinResultes { get; set; }
        EachPinResult GetResult(int pinNum);
        List<double> PlaneOffset { get; set; }
    }

    [Serializable]
    public class EachPinResult
    {
        public EachPinResult(
            int dutno,
            double diffX,
            double diffY,
            double diffZ,
            double height,
            MachineIndex dutmachineindex,
            int pinnum,
            int group,
            EventCodeEnum status,
            PINALIGNRESULT result,
            bool alignToken)
        {
            _DutNo = dutno;

            _DiffX = diffX;
            _DiffY = diffY;
            _DiffZ = diffZ;
            _Height = height;
            _DutMachineIndex = dutmachineindex;
            _PinNum = pinnum;

            _Group = group;
            _Status = status;
            _PinResult = result;
            _AlignToken = alignToken;
        }
        public EachPinResult(
            int dutno,
            double diffX,
            double diffY,
            double diffZ,
            double height,
            MachineIndex dutmachineindex,
            int pinnum,
            int group,
            EventCodeEnum status,
            PINALIGNRESULT result,
            bool alignToken,
            PINALIGNRESULT tipoptresult)
        {
            _DutNo = dutno;

            _DiffX = diffX;
            _DiffY = diffY;
            _DiffZ = diffZ;
            _Height = height;
            _DutMachineIndex = dutmachineindex;
            _PinNum = pinnum;

            _Group = group;
            _Status = status;
            _PinResult = result;
            _AlignToken = alignToken;
            PinTipOptResult = tipoptresult;
        }
        private int _DutNo;
        public int DutNo
        {
            get { return _DutNo; }
            set { _DutNo = value; }
        }
        private MachineIndex _DutMachineIndex;
        public MachineIndex DutMachineIndex
        {
            get { return _DutMachineIndex; }
            set { _DutMachineIndex = value; }
        }
        private int _PinNum;
        public int PinNum
        {
            get { return _PinNum; }
            set { _PinNum = value; }
        }

        private PINALIGNRESULT _PinResult;
        public PINALIGNRESULT PinResult
        {
            get { return _PinResult; }
            set { _PinResult = value; }
        }

        // Key blob�� pin tip�� ����� �����ϴ� ������Ƽ��
        private PINALIGNRESULT _PinTipOptResult;
        public PINALIGNRESULT PinTipOptResult
        {
            get { return _PinTipOptResult; }
            set { _PinTipOptResult = value; }
        }

        private EventCodeEnum _Status;
        public EventCodeEnum Status
        {
            get { return _Status; }
            set { _Status = value; }
        }

        private double _DiffX;
        public double DiffX
        {
            get { return _DiffX; }
            set { _DiffX = value; }
        }

        private double _DiffY;
        public double DiffY
        {
            get { return _DiffY; }
            set { _DiffY = value; }
        }

        private double _DiffZ;
        public double DiffZ
        {
            get { return _DiffZ; }
            set { _DiffZ = value; }
        }

        private double _Height;
        public double Height
        {
            get { return _Height; }
            set { _Height = value; }
        }



        private int _Group;
        public int Group
        {
            get { return _Group; }
            set { _Group = value; }
        }

        private bool _AlignToken;
        public bool AlignToken
        {
            get { return _AlignToken; }
            set { _AlignToken = value; }
        }
    }

    [Serializable]
    public class PinAlignResultes : IPinAlignResultes
    {
        private long _TotalAlignPinCount;
        public long TotalAlignPinCount
        {
            get { return _TotalAlignPinCount; }
            set { _TotalAlignPinCount = value; }
        }
        private long _TotalPassPinCount;
        public long TotalPassPinCount
        {
            get { return _TotalPassPinCount; }
            set { _TotalPassPinCount = value; }
        }
        private long _TotalFailPinCount;
        public long TotalFailPinCount
        {
            get { return _TotalFailPinCount; }
            set { _TotalFailPinCount = value; }
        }
        private int _PassPercentage;
        public int PassPercentage
        {
            get { return _PassPercentage; }
            set { _PassPercentage = value; }
        }
        private EventCodeEnum _Result;
        public EventCodeEnum Result
        {
            get { return _Result; }
            set { _Result = value; }
        }
        private List<EachPinResult> _EachPinResultes = new List<EachPinResult>();
        public List<EachPinResult> EachPinResultes
        {
            get { return _EachPinResultes; }
            set { _EachPinResultes = value; }
        }
        private List<double> _PlaneOffset = new List<double>();
        public List<double> PlaneOffset
        {
            get { return _PlaneOffset; }
            set { _PlaneOffset = value; }
        }

        private double _MinMaxZDiff;
        public double MinMaxZDiff
        {
            get { return _MinMaxZDiff; }
            set
            {
                if (value != _MinMaxZDiff)
                {
                    _MinMaxZDiff = value;
                }
            }
        }

        private double _CardCenterDiffX;
        public double CardCenterDiffX
        {
            get { return _CardCenterDiffX; }
            set
            {
                if (value != _CardCenterDiffX)
                {
                    _CardCenterDiffX = value;
                }
            }
        }

        private double _CardCenterDiffY;
        public double CardCenterDiffY
        {
            get { return _CardCenterDiffY; }
            set
            {
                if (value != _CardCenterDiffY)
                {
                    _CardCenterDiffY = value;
                }
            }
        }

        private double _CardCenterDiffZ;
        public double CardCenterDiffZ
        {
            get { return _CardCenterDiffZ; }
            set
            {
                if (value != _CardCenterDiffZ)
                {
                    _CardCenterDiffZ = value;
                }
            }
        }

        private PINALIGNSOURCE _AlignSource;
        public PINALIGNSOURCE AlignSource
        {
            get { return _AlignSource; }
            set
            {
                if (value != _AlignSource)
                {
                    _AlignSource = value;
                }
            }
        }


        public PinAlignResultes()
        {
            try
            {
                _TotalAlignPinCount = 0;
                _TotalPassPinCount = 0;
                _TotalFailPinCount = 0;
                _Result = EventCodeEnum.UNDEFINED;
                _PassPercentage = 0;
                //_ProcSampleAlign = SAMPLEPINALGINRESULT.CONTINUE;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public PinAlignResultes(PinAlignResultes result)
        {
            try
            {
                _TotalAlignPinCount = result.TotalAlignPinCount;
                _TotalPassPinCount = result.TotalPassPinCount;
                _TotalFailPinCount = result.TotalFailPinCount;
                _Result = result.Result;
                _PassPercentage = result.PassPercentage;
                //_ProcSampleAlign = result.ProcSampleAlign;
                foreach (EachPinResult item in result.EachPinResultes)
                {
                    _EachPinResultes.Add(new EachPinResult(item.DutNo, item.DiffX, item.DiffY, item.DiffZ, item.Height, item.DutMachineIndex, item.PinNum, item.Group, item.Status, item.PinResult, item.AlignToken));
                }
                //foreach (int item in result.RequiredAlignList)
                //{
                //    _RequiredAlignList.Add(item);
                //}
            }
            catch (Exception)
            {
                throw;
            }
        }

        public EachPinResult GetResult(int pinNum)
        {
            EachPinResult FindResult = null;
            try
            {
                FindResult = EachPinResultes.FirstOrDefault(pin => pin.PinNum == pinNum + 1);

            }
            catch (Exception)
            {
                throw;
            }
            return FindResult;
        }
    }

    public class PARecoveryModule
    {
        private ObservableCollection<ISubModule> _InvaliedModules
            = new ObservableCollection<ISubModule>();

        public ObservableCollection<ISubModule> InvaliedModules
        {
            get { return _InvaliedModules; }
            set { _InvaliedModules = value; }
        }

        public PARecoveryModule()
        {

        }
    }
}
