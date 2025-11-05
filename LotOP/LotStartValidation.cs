namespace LotOP
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using SequenceRunner;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class LotStartValidations
    {
        private List<LotStartValidation> _LotStartValidationList = new List<LotStartValidation>();
        public List<LotStartValidation> LotStartValidationList
        {
            get { return _LotStartValidationList; }
            set
            {
                if (value != _LotStartValidationList)
                {
                    _LotStartValidationList = value;
                }
            }
        }

        public bool Valid(out string errorMsg)
        {
            bool retval = true;
            errorMsg = string.Empty;
            string resultMsg = string.Empty;

            var validationMessages = new List<string>();

            try
            {
                int errorCount = 0;

                foreach (var valid in LotStartValidationList)
                {
                    bool isValid = valid.CheckValidation();

                    if (!isValid)
                    {
                        retval = false;
                        
                        errorCount++;
                        errorMsg += $"{errorCount}. {valid.ErrorMessage} \n";
                    }

                    validationMessages.Add($"{valid.Name} : {isValid}");
                }

                if (validationMessages.Count > 0)
                {
                    resultMsg = string.Join(", ", validationMessages);
                }

                LoggerManager.Debug($"[{this.GetType().Name}], Valid() : {resultMsg}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public LotStartValidations()
        {
            if (LotStartValidationList.Count == 0)
            {
                LotStartValidationList.Add(new ParameterValidation());
                LotStartValidationList.Add(new StageLockValidation());
                LotStartValidationList.Add(new RecoveryModeValidation());
                LotStartValidationList.Add(new CardPodDownValidation());
                LotStartValidationList.Add(new RunListValidation());
                LotStartValidationList.Add(new ChillerValidation());
                LotStartValidationList.Add(new ZifLockValidation());
                LotStartValidationList.Add(new StageModeValidation());
                LotStartValidationList.Add(new TempOffsetTableValidation());
                LotStartValidationList.Add(new TesterHeadPurgeAirValidation());
            }
        }
    }

    public abstract class LotStartValidation : IFactoryModule
    {
        public abstract string ErrorMessage { get; set; }
        public abstract bool CheckValidation();
        public abstract string Name { get; set; }
    }

    public class StageModeValidation : LotStartValidation
    {
        public StageModeValidation()
        {
            Name = this.GetType().Name;
        }

        private string _Name;
        public override string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                }
            }
        }

        private string _ErrorMessage = "StageMode Check Validation Failed.";
        public override string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (value != _ErrorMessage)
                {
                    _ErrorMessage = value;
                }
            }
        }

        public override bool CheckValidation()
        {
            bool retVal = false;
            try
            {
                retVal = this.StageSupervisor().IsModeChanging == false && this.StageSupervisor().StageMode == GPCellModeEnum.ONLINE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class ParameterValidation : LotStartValidation
    {
        public ParameterValidation()
        {
            Name = this.GetType().Name;
        }

        private string _Name;
        public override string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                }
            }
        }

        private string _ErrorMessage = "Parameter Validation Failed.";
        public override string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (value != _ErrorMessage)
                {
                    _ErrorMessage = value;
                }
            }
        }

        public override bool CheckValidation()
        {
            bool retVal = false;
            try
            {
                retVal = this.ParamManager().VerifyLotVIDsCheckBeforeLot() == EventCodeEnum.NONE ? true : false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    public class StageLockValidation : LotStartValidation
    {
        public StageLockValidation()
        {
            Name = this.GetType().Name;
        }

        private string _Name;
        public override string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                }
            }
        }

        private string _ErrorMessage = "Stage is Lock State.";
        public override string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (value != _ErrorMessage)
                {
                    _ErrorMessage = value;
                }
            }
        }

        public override bool CheckValidation()
        {
            bool retVal = false;
            try
            {
                retVal = this.StageSupervisor().GetStageLockMode() == StageLockMode.LOCK ? false : true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    public class RecoveryModeValidation : LotStartValidation
    {
        public RecoveryModeValidation()
        {
            Name = this.GetType().Name;
        }

        private string _Name;
        public override string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                }
            }
        }

        private string _ErrorMessage = "Check the wafer status of the stage.";
        public override string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (value != _ErrorMessage)
                {
                    _ErrorMessage = value;
                }
            }
        }

        public override bool CheckValidation()
        {
            bool retVal = false;
            try
            {
                retVal = this.StageSupervisor().IsRecoveryMode == true ? false : true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    public class CardPodDownValidation : LotStartValidation
    {
        public CardPodDownValidation()
        {
            Name = this.GetType().Name;
        }

        private string _Name;
        public override string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                }
            }
        }

        private string _ErrorMessage = "Check the CardPodDown status of the stage.";
        public override string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (value != _ErrorMessage)
                {
                    _ErrorMessage = value;
                }
            }
        }

        public override bool CheckValidation()
        {
            bool retVal = false;
            try
            {
                retVal = true;
                var cardPodUpCheckcommand = new GP_CheckPCardPodIsDown();
                var retVal_CardPod = this.CardChangeModule().BehaviorRun(cardPodUpCheckcommand).Result;
                if (retVal_CardPod != EventCodeEnum.NONE)
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    public class RunListValidation : LotStartValidation
    {
        public RunListValidation()
        {
            Name = this.GetType().Name;
        }

        private string _Name;
        public override string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                }
            }
        }

        private string _ErrorMessage;
        public override string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (value != _ErrorMessage)
                {
                    _ErrorMessage = value;
                }
            }
        }
        public override bool CheckValidation()
        {
            bool retVal = false;
            string msg = null;
            try
            {
                retVal = true;
                foreach (IStateModule module in this.LotOPModule().RunList)
                {
                    bool RetVal = true;

                    if (module.ForcedDone == EnumModuleForcedState.Normal)
                    {
                        RetVal = module.IsLotReady(out msg);
                    }

                    if (RetVal == false)
                    {
                        retVal = false;
                        string moduleName = module.GetType().Name;

                        ErrorMessage += $"[{moduleName}] {msg}.\n";

                        LoggerManager.Debug(msg);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    public class ChillerValidation : LotStartValidation
    {
        public ChillerValidation()
        {
            Name = this.GetType().Name;
        }

        private string _Name;
        public override string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                }
            }
        }

        private string _ErrorMessage = "Check the chiller status.";
        public override string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (value != _ErrorMessage)
                {
                    _ErrorMessage = value;
                }
            }
        }
        public override bool CheckValidation()
        {
            bool retVal = false;
            try
            {
                retVal = this.EnvControlManager().ChillerManager.CanRunningLot();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    public class ZifLockValidation : LotStartValidation
    {
        public ZifLockValidation()
        {
            Name = this.GetType().Name;
        }

        private string _Name;
        public override string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                }
            }
        }

        private string _ErrorMessage = "Check the zif lock status.";
        public override string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (value != _ErrorMessage)
                {
                    _ErrorMessage = value;
                }
            }
        }

        public override bool CheckValidation()
        {
            bool retVal = false;
            try
            {
                retVal = true;
                //if pcw lot running 중일 경우 무조건 IsZifLockStateValid 확인하지 않음.
                //셀이 로더의 값을 가지고 올수 있는 proxy 써야함. 

                ILotOPModule lotOPModule = this.LotOPModule();

                var activeWaferType = this.LoaderController().GetActiveLotWaferType(lotOPModule.LotInfo.LotName.Value);

                if (activeWaferType == EnumWaferType.STANDARD)
                {
                    //다른 조건이 다 IsCanProbingStart 상태여도 Zif lock 상태가 유효하지 않으면 Error 처리해야함.               
                    if (this.CardChangeModule().IsZifRequestedState(true, writelog: false) != EventCodeEnum.NONE)
                    {
                        lotOPModule.ReasonOfError.AddEventCodeInfo(EventCodeEnum.ZIF_STATE_NOT_READY, "Pause by invalid zif lock state.", this.GetType().Name);
                        lotOPModule.PauseSourceEvent = lotOPModule.ReasonOfError.GetLastEventCode();

                        retVal = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class TempOffsetTableValidation : LotStartValidation
    {
        public TempOffsetTableValidation()
        {
            Name = this.GetType().Name;
        }

        private string _Name;
        public override string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                }
            }
        }

        private string _ErrorMessage = "Check the temperature offset table";
        public override string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (value != _ErrorMessage)
                {
                    _ErrorMessage = value;
                }
            }
        }

        public override bool CheckValidation()
        {
            bool retVal = false;
            try
            {
                if (this.TempController().GetCheckingTCTempTable() == true && this.TempController().CheckIfTempIsIncluded() == false && this.StageSupervisor().Get_TCW_Mode() == TCW_Mode.OFF)
                {
                    ILotOPModule lotOPModule = this.LotOPModule();

                    string keys = string.Join(", ", this.TempController().GetHeaterOffsets().Keys);
                    double setTemp = this.TempController().TempInfo.SetTemp.Value;

                    this.LotOPModule().ReasonOfError.AddEventCodeInfo(EventCodeEnum.TEMPERATURE_EXCLUDED_TEMP_TABLE,
                       "The heater offset table does not include the device's set temperature value, so we cannot proceed with the lot. " +
                           "\n" +
                           $"\nCurrent set Temp: {setTemp} \nHeater Offset Table: {keys}", this.GetType().Name);
                    lotOPModule.PauseSourceEvent = lotOPModule.ReasonOfError.GetLastEventCode();

                }
                else
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class TesterHeadPurgeAirValidation : LotStartValidation
    {
        public TesterHeadPurgeAirValidation()
        {
            Name = this.GetType().Name;
        }

        private string _Name;
        public override string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                }
            }
        }

        private string _ErrorMessage = "Check the tester head purge air state";
        public override string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (value != _ErrorMessage)
                {
                    _ErrorMessage = value;
                }
            }
        }

        public override bool CheckValidation()
        {
            bool retVal = false;
            try
            {
                if (this.IOManager().IO.Inputs.DITESTERHEAD_PURGE.IOOveride.Value == EnumIOOverride.NONE)
                {
                    if (this.TempController().IsPurgeAirBackUpValue)
                    {
                        bool ditoppurge = false;
                        var ioreturnValue = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITESTERHEAD_PURGE, out ditoppurge);//들어오는지 확인

                        if (ditoppurge == true) // air 들어올 때 Input 1
                        {
                            retVal = true;
                        }
                        else //Tester Purge Air 유량 떨어진 경우 출력 Input 0
                        {
                            retVal = false;
                        }
                    }
                    else
                    {
                        var purgeret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITESTERHEAD_PURGE, false, 100, 1500, false);//안들어 오는지 확인
                        if (purgeret != 0)
                        {
                            retVal = false;
                        }
                        else
                        {
                            retVal = true;
                        }
                    }
                }
                else
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}
