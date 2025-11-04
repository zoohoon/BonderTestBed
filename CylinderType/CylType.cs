using LogModule;
using ProberInterfaces;
using ProberInterfaces.Enumeration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CylType
{

    public abstract class CylinderType : Enumeration, ICylinderType, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public List<CylinderStatus> Cylinder_Extend_StatisticsInfo { get; set; }
        public List<CylinderStatus> Cylinder_Retract_StatisticsInfo { get; set; }

        private string _Name;
        public string Name
        {
            get
            {
                return GetDisplayName();
            }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private CylinderStateEnum _State;
        public CylinderStateEnum State
        {
            get { return _State; }
            set
            {
                if (value != _State)
                {
                    _State = value;
                    RaisePropertyChanged();
                }
            }
        }
        private PoVEnum _PoV;
        public PoVEnum PoV
        {
            get { return _PoV; }
            set
            {
                if (value != _PoV)
                {
                    _PoV = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Func<bool> _ExtInterlock;
        public Func<bool> ExtInterlock
        {
            get { return _ExtInterlock; }
            set
            {
                if (value != _ExtInterlock)
                {
                    _ExtInterlock = value;
                    RaisePropertyChanged();
                }
            }
        }


        public CylinderType()
        {
        }
        public void Init()
        {
            try
            {

                if (Cylinder_Extend_StatisticsInfo == null)
                {
                    Cylinder_Extend_StatisticsInfo = new List<CylinderStatus>();
                }

                if (Cylinder_Retract_StatisticsInfo == null)
                {
                    Cylinder_Retract_StatisticsInfo = new List<CylinderStatus>();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private CylinderIOParameter _CylinderParam;
        public CylinderIOParameter CylinderParam
        {
            get { return _CylinderParam; }
            set
            {
                _CylinderParam = value;
            }
        }

        private long _CylinderLifeTime;
        public long CylinderLifeTime
        {
            get { return _CylinderLifeTime; }
            set
            {
                if (value != _CylinderLifeTime)
                {
                    _CylinderLifeTime = value;
                    RaisePropertyChanged();
                }
            }
        }



        public IEnumerable<ICylinderType> Members { get; set; }
        public string GetDisplayName()
        {
            return base.DisplayName;
        }

        #region Parameter Set Method
        public void SetExtend_OutPut(IOPortDescripter<bool> io)
        {
            try
            {
                if (CylinderParam == null)
                {
                    CylinderParam = new CylinderIOParameter();
                }

                CylinderParam.Extend_Output = io;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetRetract_OutPut(IOPortDescripter<bool> io)
        {
            try
            {
                if (CylinderParam == null)
                {
                    CylinderParam = new CylinderIOParameter();
                }

                CylinderParam.Retract_OutPut = io;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetExtend_Input(List<IOPortDescripter<bool>> io)
        {
            try
            {
                if (CylinderParam == null)
                {
                    CylinderParam = new CylinderIOParameter();
                }

                CylinderParam.Extend_Input.MultipleInput = io;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetRetract_Input(List<IOPortDescripter<bool>> io)
        {
            try
            {

                if (CylinderParam == null)
                {
                    CylinderParam = new CylinderIOParameter();
                }

                CylinderParam.Retract_Input.MultipleInput = io;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetInterlock_Input(List<IOPortDescripter<bool>> io)
        {
            try
            {

                if (CylinderParam == null)
                {
                    CylinderParam = new CylinderIOParameter();
                }
                if(CylinderParam.InterlockInputs == null)
                {
                    CylinderParam.InterlockInputs = new InputDescripters();
                }
                CylinderParam.InterlockInputs.MultipleInput = io;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        //public IEnumerable<IStatisticsElement> StatisticsMembers
        //{
        //    get => Members;
        //}

        //public IEnumerable<CylinderType> Members
        //{
        //    get;
        //    set;
        //}

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int Extend()
        {
            int RetVal = 0;
            try
            {

                State = CylinderStateEnum.RUNNING;

                DateTime starttime = DateTime.MinValue;
                DateTime endtime = DateTime.MinValue;

                List<string> ex_in_keys = new List<string>();
                List<string> re_in_keys = new List<string>();
                List<string> intlck_in_keys = new List<string>();
                bool interLocked = false;
                if (CylinderParam != null)
                {

                    foreach (var item in CylinderParam.InterlockInputs.MultipleInput.Select((value, i) => new { i, value }))
                    {
                        var value = item.value;
                        var index = item.i;

                        RetVal = value.MonitorForIO(false);

                        if (RetVal != 0)
                        {
                            interLocked = true;
                        }
                    }
                    if(ExtInterlock != null)
                    {
                        var intlk = ExtInterlock();
                        if (intlk == true)
                        {
                            LoggerManager.Debug($"{Name} Cylinder interlock failed while extanding.");
                            interLocked = true;
                        }
                    }
                    if(interLocked == false) // interlock for cc 
                    {
                        CylinderParam.Retract_OutPut.ResetValue();
                        CylinderParam.Extend_Output.SetValue();

                        // Timer Start

                        starttime = DateTime.Now;

                        // value.MonitorForIO() == 0 => NO ERROR 
                        // value.MonitorForIO() == -1 => NET_IO_ERROR
                        // value.MonitorForIO() == -2 => TIME_OUT_ERROR 


                        foreach (var item in CylinderParam.Retract_Input.MultipleInput.Select((value, i) => new { i, value }))
                        {
                            var value = item.value;
                            var index = item.i;

                            RetVal = value.MonitorForIO(false);

                            if (RetVal != 0)
                            {
                                re_in_keys.Add(value.Key.Value);
                            }
                        }

                        foreach (var item in CylinderParam.Extend_Input.MultipleInput.Select((value, i) => new { i, value }))
                        {
                            var value = item.value;
                            var index = item.i;

                            RetVal = value.MonitorForIO(true);

                            if (RetVal != 0)
                            {
                                ex_in_keys.Add(value.Key.Value);
                            }
                        }
                    }
                    else
                    {
                        StringBuilder stb = new StringBuilder();
                        stb.Append($"CylinderParam.InterlockInputs: ");

                        foreach (var item in CylinderParam.InterlockInputs.MultipleInput)
                        {
                            stb.Append($"({item.Description}: ch:{item.ChannelIndex},pt:{item.PortIndex})");
                            if (CylinderParam.InterlockInputs.MultipleInput.Last() != item)
                            {
                                stb.Append(", ");
                            }
                        }

                        LoggerManager.Debug($"CylType: {Name}, Interlock failed. Interlocks = {stb}");
                        RetVal = -3;
                    }
                }
                else
                {
                    RetVal = -99;
                }

                if (RetVal == 0) // NO ERROR
                {
                    State = CylinderStateEnum.EXTEND;
                }
                else if (RetVal == -1) // NET_IO_ERROR
                {
                    State = CylinderStateEnum.ERROR;

                }
                else if (RetVal == -2) // TIME_OUT_ERROR
                {
                    State = CylinderStateEnum.JAM;
                }
                else
                {
                    State = CylinderStateEnum.UNDEFINED;
                }

                // Timer End
                endtime = DateTime.Now;

                CylinderStatus stat = new CylinderStatus();

                stat.UpdateCylinderStatus(starttime, endtime, State, ex_in_keys, re_in_keys);
                //Cylinder_Extend_StatisticsInfo.Add(stat);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public int Retract()
        {
            int RetVal = 0;

            try
            {


                State = CylinderStateEnum.RUNNING;
                RaisePropertyChanged();

                DateTime starttime = DateTime.MinValue;
                DateTime endtime = DateTime.MinValue;

                List<string> ex_in_keys = new List<string>();
                List<string> re_in_keys = new List<string>();

                if(CylinderParam != null)
                {
                    CylinderParam.Extend_Output.ResetValue();
                    CylinderParam.Retract_OutPut.SetValue();

                    // Timer Start

                    starttime = DateTime.Now;

                    foreach (var item in CylinderParam.Extend_Input.MultipleInput.Select((value, i) => new { i, value }))
                    {
                        var value = item.value;
                        var index = item.i;
                        RetVal = value.MonitorForIO(false);

                        if (RetVal != 0)
                        {
                            re_in_keys.Add(value.Key.Value);
                        }
                    }

                    if (RetVal == 0)
                    {
                        foreach (var item in CylinderParam.Retract_Input.MultipleInput.Select((value, i) => new { i, value }))
                        {
                            var value = item.value;
                            var index = item.i;

                            RetVal = value.MonitorForIO(true);

                            if (RetVal != 0)
                            {
                                ex_in_keys.Add(value.Key.Value);
                            }
                        }
                    }

                    //Cylinder_Retract_StatisticsInfo.Add(stat);
                }
                else
                {
                    RetVal = -99;
                }

                if (RetVal == 0) // NO ERROR
                {
                    State = CylinderStateEnum.RETRACT;
                }
                else if (RetVal == -1) // NET_IO_ERROR
                {
                    State = CylinderStateEnum.ERROR;

                }
                else if (RetVal == -2) // TIME_OUT_ERROR
                {
                    State = CylinderStateEnum.JAM;
                }
                else
                {
                    State = CylinderStateEnum.UNDEFINED;
                }

                // Timer End
                endtime = DateTime.Now;

                CylinderStatus stat = new CylinderStatus();

                stat.UpdateCylinderStatus(starttime, endtime, State, ex_in_keys, re_in_keys);

                //if (State != CylinderStateEnum.ERROR)
                //{
                //    State = CylinderStateEnum.RETRACT;
                //}

            }
            catch (Exception err)
            {
                // Exception
                RetVal = -3;

                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public bool SetExtInterLock(Func<bool> extinterlock)
        {
            bool ret = false;
            try
            {
                ExtInterlock = extinterlock;
                ret = true;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"SetExtInterLock(): Error occurred. Err = {err.Message}");
            }
            return ret;
        }

        public CylinderType(int value, string displayName) : base(value, displayName) { }
    }

    public class StageCylinderType : CylinderType, IStageCylinderType
    {
        public static readonly StageCylinderType CardHolder = new StageCylinderType(0, "CardHolder");
        public static readonly StageCylinderType CHTube = new StageCylinderType(1, "CHTube");
        public static readonly StageCylinderType CHRotate = new StageCylinderType(2, "CHRotate");
        public static readonly StageCylinderType SwingPlate = new StageCylinderType(3, "SwingPlate");
        public static readonly StageCylinderType ThreeLEG = new StageCylinderType(4, "ThreeLEG");
        public static readonly StageCylinderType CleanUnit = new StageCylinderType(5, "CleanUnit");
        public static readonly StageCylinderType CH_ROT = new StageCylinderType(6, "CH_ROT");
        public static readonly StageCylinderType CH_TUB = new StageCylinderType(7, "CH_TUB");
        public static readonly StageCylinderType ZIF_REQ = new StageCylinderType(8, "ZIF_REQ");
        public static readonly StageCylinderType NeedleBrush = new StageCylinderType(9, "NeedleBrush");
        public static readonly StageCylinderType Front_Inner_Cover = new StageCylinderType(10, "Front_Inner_Cover");
        public static readonly StageCylinderType T2K_Cylinder = new StageCylinderType(11, "T2K_Cylinder");
        public static readonly StageCylinderType Tester_Head = new StageCylinderType(12, "Tester_Head");
        public static readonly StageCylinderType MoveWaferCam = new StageCylinderType(13, "MoveWaferCam");
        public static readonly StageCylinderType CardPod = new StageCylinderType(14, "CardPod");
        public static readonly StageCylinderType BernoulliHandlerUpDown = new StageCylinderType(15, "BernoulliHandlerUpDown");
        public static readonly StageCylinderType BernoulliHandlerAlign = new StageCylinderType(16, "BernoulliHandlerAlign");

        private StageCylinderType(int value, string displayName) : base(value, displayName) { }

        public StageCylinderType()
        {
            Members = GetAll<StageCylinderType>();
        }
    }
    public class LoaderCylinderType : CylinderType, ILoaderCylinderType
    {
        public static readonly LoaderCylinderType ArmExtender = new LoaderCylinderType(0, "ArmExtender");
        public static readonly LoaderCylinderType FramedPre = new LoaderCylinderType(1, "FramedPre");
        public static readonly LoaderCylinderType TesterHead = new LoaderCylinderType(2, "TesterHead");
        public static readonly LoaderCylinderType CardTray = new LoaderCylinderType(3, "CardTray");
        public static readonly LoaderCylinderType InspectionCover = new LoaderCylinderType(4, "InspectionCover");
        public static readonly LoaderCylinderType PreIN = new LoaderCylinderType(5, "PreIN");
        private LoaderCylinderType(int value, string displayName) : base(value, displayName) { }

        public LoaderCylinderType()
        {
            Members = GetAll<LoaderCylinderType>();
        }
    }
    public class FoupCylinderType : CylinderType, IFoupCylinderType
    {
        public static readonly FoupCylinderType FoupDockingPlate6 = new FoupCylinderType(0, "FoupDockingPlate6");
        public static readonly FoupCylinderType FoupDockingPlate8 = new FoupCylinderType(1, "FoupDockingPlate8");
        public static readonly FoupCylinderType FoupDockingPlate12 = new FoupCylinderType(2, "FoupDockingPlate12");
        public static readonly FoupCylinderType FoupDockingPort = new FoupCylinderType(3, "FoupDockingPort");
        public static readonly FoupCylinderType FoupDockingPort40 = new FoupCylinderType(4, "FoupDockingPort40");
        public static readonly FoupCylinderType FoupRotator = new FoupCylinderType(5, "FoupRotator");
        public static readonly FoupCylinderType FoupCover = new FoupCylinderType(6, "FoupCover");
        public static readonly FoupCylinderType FoupCassetteTilting = new FoupCylinderType(7, "FoupCassetteTilting");


        public FoupCylinderType()
        {
            try
            {
                Members = GetAll<FoupCylinderType>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private FoupCylinderType(int value, string displayName) : base(value, displayName)
        {
            try
            {
                this.Cylinder_Extend_StatisticsInfo = new List<CylinderStatus>();
                this.Cylinder_Retract_StatisticsInfo = new List<CylinderStatus>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
