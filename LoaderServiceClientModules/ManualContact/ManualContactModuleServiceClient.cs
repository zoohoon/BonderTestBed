using System;
using System.Threading.Tasks;

namespace LoaderServiceClientModules
{
    using Autofac;
    using LogModule;
    using ProberInterfaces;
    using ProberErrorCode;
    using System.Windows;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using LoaderBase.Communication;
    using ManualContact;
    using StageStateEnum = ProberInterfaces.StageStateEnum;

    public class ManualContactModuleServiceClient : IManualContact, INotifyPropertyChanged, ILoaderFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ILoaderCommunicationManager loaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
        private IRemoteMediumProxy remoteMediumProx => loaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

        private ICoordinateManager _CoordinateManager;
        public ICoordinateManager CoordinateManager
        {
            get { return _CoordinateManager; }
            set
            {
                if (value != _CoordinateManager)
                {
                    _CoordinateManager = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCallbackEntry;
        public bool IsCallbackEntry
        {
            get { return _IsCallbackEntry; }
            set
            {
                if (value != _IsCallbackEntry)
                {
                    _IsCallbackEntry = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Point _MXYIndex;
        public Point MXYIndex
        {
            get { return _MXYIndex; }
            set
            {
                if (value != _MXYIndex || CellIsZClearedState())
                {
                    //brett// IsCallbackEntry는 셀에서 위치이동을 호출했는지를 구분하기 위해 추가한 것으로 보임
                    //Cell에서 위치이동한 경우에는 다시 Cell로 wcf 호출(SetMXIndex)하지 않고 내부 변수 값만 업데이트 하고 있음
                    if (IsCallbackEntry == false)
                    {
#pragma warning disable 4014
                        // 시간이 오래걸리는 작업이라 Await를 걸지 않았다.
                        // 향후, 커맨드 처리로 변경 검토 필요 by brett.
                        SetMXIndex(value);
#pragma warning restore 4014
                    }
                    else
                    {
                        _MXYIndex = value;

                        IsCallbackEntry = false;
                    }

                    RaisePropertyChanged();
                }
                else
                {
                    //brett//ISSD-4009 zup,zdown의 경우에도 setter가 call 되고 있었으며 이경우 IsCallbackEntry가 true에서 false로 변경하지 못하는 문제가 있어 수정함
                    if (value == _MXYIndex && IsCallbackEntry)
                    {
                        IsCallbackEntry = false;
                    }
                }
            }
        }

        private object _ViewTarget ;
        public object ViewTarget
        {
            get { return _ViewTarget; }
            set
            {
                if (value != _ViewTarget)
                {
                    _ViewTarget = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _OverDrive;
        public double OverDrive
        {
            get
            {
                return _OverDrive;
            }
            set
            {
                if (value != _OverDrive)
                {
                    _OverDrive = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _CPC_Z0;
        public double CPC_Z0
        {
            get
            {
                return _CPC_Z0;
            }
            set
            {
                if (value != _CPC_Z0)
                {
                    _CPC_Z0 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CPC_Z1;
        public double CPC_Z1
        {
            get
            {
                return _CPC_Z1;
            }
            set
            {
                if (value != _CPC_Z1)
                {
                    _CPC_Z1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CPC_Z2;
        public double CPC_Z2
        {
            get
            {
                return _CPC_Z2;
            }
            set
            {
                if (value != _CPC_Z2)
                {
                    _CPC_Z2 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SelectedContactPosition;
        public double SelectedContactPosition
        {
            get { return _SelectedContactPosition; }
            set
            {
                if (value != _SelectedContactPosition)
                {
                    _SelectedContactPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _CPC_Visibility;
        public Visibility CPC_Visibility
        {
            get { return _CPC_Visibility; }
            set
            {
                if (value != _CPC_Visibility)
                {
                    _CPC_Visibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ManualContactStateBase _ManualContactZUpState;
        public ManualContactStateBase ManualContactZAxisState
        {
            get { return _ManualContactZUpState; }
            set
            {
                if (value != _ManualContactZUpState)
                {
                    _ManualContactZUpState = value;
                    RaisePropertyChanged(nameof(ManualContactZAxisState));
                }
            }
        }

        private bool _IsZUpState;
        public bool IsZUpState
        {
            get { return _IsZUpState; }
            set
            {
                if (value != _IsZUpState)
                {
                    _IsZUpState = value;
                    RaisePropertyChanged();
                }
            }
        }


        private MachinePosition _MachinePosition;
        public MachinePosition MachinePosition
        {
            get { return _MachinePosition; }
            set
            {
                if (value != _MachinePosition)
                {
                    _MachinePosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _AlawaysMoveToFirstDut;
        public bool AlawaysMoveToTeachDie
        {
            get { return _AlawaysMoveToFirstDut; }
            set
            {
                if (value != _AlawaysMoveToFirstDut)
                {
                    _AlawaysMoveToFirstDut = value;
                    RaisePropertyChanged();
                    //remoteMediumProx.MCMSetAlawaysMoveToTeachDie(_AlawaysMoveToFirstDut);
                }
            }
        }

        public bool Initialized { get; set; }

        public InitPriorityEnum InitPriority => throw new NotImplementedException();

        public bool IsMovingStage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task<bool> AllContactSet()
        {
            throw new NotImplementedException();
        }

        private bool CellIsZClearedState()
        {
            bool retVal = false;
            try
            {
                var stgObj = loaderCommunicationManager.GetStage();
                if(stgObj != null)
                {
                    StageStateEnum stageState = StageStateEnum.Z_CLEARED;
                    var stageMoveStage = stgObj.StageInfo.LotData.StageMoveState;
                    if (stageMoveStage.Equals(stageState.ToString()))
                    {
                        retVal = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private async Task SetMXIndex(System.Windows.Point mxyIndex)
        {
            try
            {
                await Task.Run(() =>
                {
                    remoteMediumProx.SetMCM_XYInex(mxyIndex);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                
            }
        }

        public void ChangeOverDrive(string OverDriveValue)
        {
            bool parseSuccessResult = false;
            double parseValue = 0;

            parseSuccessResult = ParsingToDouble(OverDriveValue, out parseValue);

            if (parseSuccessResult == true)
            {
                this.OverDrive = parseValue;
            }
        }
        public void ChangeCPC_Z1(string CPC_Z1)
        {
            bool parseSuccessResult = false;
            double parseValue = 0;

            parseSuccessResult = ParsingToDouble(CPC_Z1, out parseValue);

            if (parseSuccessResult == true)
            {
                this.CPC_Z1 = parseValue;
            }
        }
        public void ChangeCPC_Z2(string CPC_Z2)
        {
            bool parseSuccessResult = false;
            double parseValue = 0;

            parseSuccessResult = ParsingToDouble(CPC_Z2, out parseValue);

            if (parseSuccessResult == true)
            {
                this.CPC_Z2 = parseValue;
            }
        }
        private bool ParsingToDouble(string parseData, out double parseValue)
        {
            bool retVal = false;
            parseValue = 0;
            if (!string.IsNullOrEmpty(parseData))
            {
                retVal = double.TryParse(parseData, out parseValue);
            }

            return retVal;
        }
        public Task ChangeToZDownState()
        {
            //ManualContactZAxisStateTransition(new ManualContactZDown());

            //IsZUpState = false;

            return null;
        }

        public void ChangeToZUpState()
        {
            //ManualContactZAxisStateTransition(new ManualContactZUp());
        }

        public void ManualContactZAxisStateTransition(ManualContactStateBase changeState)
        {
            try
            {
                //if (this.StageSupervisor().StageMoveState == StageStateEnum.Z_UP)
                //{
                //    changeState = new ManualContactZUp();
                //}

                this.ManualContactZAxisState = changeState;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }



        public void DeInitModule()
        {
            
        }

        public Task<bool> FirstContactSet()
        {
            throw new NotImplementedException();
        }

        public void GetOverDriveFromProbingModule()
        {
            throw new NotImplementedException();
        }


        public void DecreaseX()
        {
            remoteMediumProx.MCMDecreaseX();
        }

        public void DecreaseY()
        {
            remoteMediumProx.MCMDecreaseY();
        }

        public void IncreaseX()
        {
            remoteMediumProx.MCMIncreaseX();
        }

        public void IncreaseY()
        {
            remoteMediumProx.MCMIncreaseY();

        }

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
            //throw new NotImplementedException();
        }

        public void InitSelectedContactPosition()
        {
            throw new NotImplementedException();
        }

        public void MachinePositionUpdate()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum MoveToWannaZIntervalMinus(double wantToMoveZInterval)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum MoveToWannaZIntervalPlus(double wantToMoveZInterval)
        {
            throw new NotImplementedException();
        }

        public void OverDriveValueDown()
        {
            throw new NotImplementedException();
        }

        public void OverDriveValueUp()
        {
            throw new NotImplementedException();
        }

        public void ResetContactStartPosition()
        {
            throw new NotImplementedException();
        }

        public void SetContactStartPosition()
        {
            throw new NotImplementedException();
        }

        public void ZDownMode(bool needZCleared = false)
        {
            throw new NotImplementedException();
        }

        public void ZoomIn()
        {
            throw new NotImplementedException();
        }

        public void ZoomOut()
        {
            throw new NotImplementedException();
        }

        public void ZUpMode()
        {
            throw new NotImplementedException();
        }
        public EventCodeEnum ZUpMode(long xIndex, long yIndex, double OD)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            return EventCodeEnum.NONE;
        }

        public void SetIndex(EnumMovingDirection xdir, EnumMovingDirection ydir)
        {
            remoteMediumProx.MCMSetIndex(xdir,ydir);
        }
        public void ManualContactZDownStateTransition()
        {
            throw new NotImplementedException();
        }
    }
}
