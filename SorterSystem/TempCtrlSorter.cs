using LogModule;
using SciChart.Charting.Model.DataSeries;
using SequenceService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SorterSystem.TempCtrl
{
    using Temperature;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Command.Internal;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Event;
    using ProberInterfaces.State;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.Temperature.TempManager;
    using SerializerUtil;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Threading.Tasks;
    using TempControl;
    using NotifyEventModule;
    using TempControllerParameter;
    using Temperature.Temp;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TempCotrlSorter : TempController, ITempController,  IHasDevParameterizable 
    {
        public new EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (TempInfo == null)
                {
                    TempInfo = new TemperatureInfo();
                    Extensions_IParam.CollectCommonElement(TempInfo, this.GetType().Name);
                }
                TempControllerDevParameter.SetTemp.ConvertToReportTypeEvent += SetTempDevParam_ConvertToReportTypeEvent;
                TempControllerDevParameter.SetTemp.ValueChangedEvent += SetTempDevParam_ValueChangedEvent;
                TempInfo.MonitoringInfo.MonitoringEnable = TempControllerDevParameter.TemperatureMonitorEnable.Value;
                TempInfo.MonitoringInfo.TempMonitorRange = TempControllerDevParameter.TempMonitorRange.Value;
                TempInfo.MonitoringInfo.WaitMonitorTimeSec = TempControllerDevParameter.WaitMonitorTimeSec.Value;
                TempInfo.DeviceSetTemp = TempControllerDevParameter.SetTemp.Value;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public new void InitState(bool callClearState = false)
        {
            InnerStateTransition(new TCIdleState(this));
        }

        public new TempBackupInfo LoadBackupTempInfo()
        {
            try
            {
                TempBackupInfo backup = new TempBackupInfo();
                string cellNo = "00";
                string folderPath = this.FileManager().FileManagerParam.LogRootDirectory + $@"\Backup\{cellNo}";
                string fileName = backup.FileName;
                string fullPath = Path.Combine(folderPath, fileName);
                if (Directory.Exists(folderPath) == false)
                {
                    Directory.CreateDirectory(folderPath);
                }

                if (File.Exists(fullPath))
                {
                    IParam tmpParam = null;
                    EventCodeEnum RetVal = this.LoadParameter(ref tmpParam, typeof(TempBackupInfo), null, fullPath);
                    if (RetVal == EventCodeEnum.NONE)
                    {
                        backup = tmpParam as TempBackupInfo;
                        this.TempBackupInfo = backup;
                    }
                    //SerializeManager.Deserialize(fullPath, out var param, deserializerType: SerializerType.JSON);
                    //TempBackupInfo = JsonConvert.DeserializeObject<TempBackupInfo>(param.ToString());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return TempBackupInfo;
        }

        public new TempBackupInfo SaveBackupTempInfo()
        {
            try
            {
                if (TempBackupInfo.TempInfoHistory != null)
                {
                    string cellNo = "00";
                    string folderPath = this.FileManager().FileManagerParam.LogRootDirectory + $@"\Backup\{cellNo}";
                    string fileName = TempBackupInfo.FileName;
                    string fullPath = Path.Combine(folderPath, fileName);
                    if (Directory.Exists(folderPath) == false)
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    //SerializeManager.Serialize(fullPath, this.TempBackupInfo, serializerType: SerializerType.JSON);
                    this.SaveParameter(this.TempBackupInfo, fixFullPath: fullPath);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return TempBackupInfo;
        }


        private void MakeTempModules()
        {
            #region => Temperature
            if (TempControllerParam.TempModuleMode.Value == TempModuleMode.E5EN)
            {
                TempManager = new Temperature.Temp.TempManager(TempModuleMode.E5EN);
            }
            else if (TempControllerParam.TempModuleMode.Value == TempModuleMode.EMUL)
            {
                TempManager = new Temperature.Temp.TempManager(TempModuleMode.EMUL);
            }
            else
            {
                TempManager = new Temperature.Temp.TempManager(TempModuleMode.EMUL);
            }
            #endregion

        }

        private EventCodeEnum InitTempModules()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = InitHelpFunc(TempManager);

            return retVal;

            EventCodeEnum InitHelpFunc(IModule tempModule)
            {
                EventCodeEnum isSucess = EventCodeEnum.UNDEFINED;

                if (tempModule != null)
                {
                    if (tempModule is IHasSysParameterizable)
                    {
                        isSucess = (tempModule as IHasSysParameterizable)?.LoadSysParameter() ?? EventCodeEnum.UNDEFINED;
                        if (isSucess != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"{tempModule.GetType().Name}.LoadSysParameter() Failed");
                        }
                    }

                    tempModule.InitModule();

                    if (isSucess != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"{tempModule.GetType().Name}.InitModule() Failed");
                    }
                }

                return isSucess;
            }

        }

        object temp_backup_lock = null;

        private void UpdateLastestInfoTempInfoHistory(TemperatureChangeSource source, double targetTemp)//TemperatureInfo latest_tempInfo)
        {
            try
            {
                TempEventInfo latest_tempInfo = new TempEventInfo(source, targetTemp);

                //string history = "TempInfoHistory: ";
                lock (temp_backup_lock)
                {
                    if (TempBackupInfo.TempInfoHistory.Count() > 0)
                    {
                        if (TempBackupInfo.TempInfoHistory[0].SetTemp == targetTemp && TempBackupInfo.TempInfoHistory[0].TempChangeSource == source)
                        {
                            return;
                        }
                    }


                    this.TempBackupInfo.TempInfoHistory.Insert(0, latest_tempInfo);
                    if (this.TempBackupInfo.TempInfoHistory.Count > 100)
                    {
                        TempBackupInfo.TempInfoHistory = TempBackupInfo.TempInfoHistory.GetRange(0, 100);
                    }

                    SaveBackupTempInfo();

                    //for (int i = 0; i < this.TempBackupInfo.TempInfoHistory.Count; i++)
                    //{
                    //    history += $"({i}) {this.TempBackupInfo.TempInfoHistory[i].TempChangeSource}, {this.TempBackupInfo.TempInfoHistory[i].SetTemp.Value}";
                    //    if (i + 1 < this.TempBackupInfo.TempInfoHistory.Count)
                    //    {
                    //        history += " | ";
                    //    }


                    //}
                    //LoggerManager.Debug($"{history}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public new void SetSV(TemperatureChangeSource source, double changeSetTemp, bool willYouSaveSetValue = true, bool forcedSetValue = false, double overHeating = 0.0, double Hysteresis = 0.0)
        {
            try
            {
                try
                {
                    lock (TempChangeLockObj)
                    {
                        var backup = GetCurrentTempInfoInHistory();
                        if (willYouSaveSetValue && GetApplySVChangesBasedOnDeviceValue())
                        {
                            TempControllerDevParameter.SetTemp.Value = (double)(changeSetTemp); // ValueChangeEvnet 때문에 두번 돌게 됨. Lock 때문에 문제는 없지만 
                            LoggerManager.Debug($"Set SetTemp for DevParameter : {TempControllerDevParameter.SetTemp.Value}");
                            SaveDevParameter();
                        }

                        if (TempInfo.SetTemp.Value != changeSetTemp & TempInfo.PreSetTemp.Value != TempInfo.SetTemp.Value)
                        {
                            TempInfo.PreSetTemp.Value = TempInfo.SetTemp.Value;
                        }


                        if (forcedSetValue)
                        {
                            SetForcedSetValue(forcedSetValue);
                        }

                        if (TempInfo.TargetTemp.Value != changeSetTemp)
                        {
                            TempInfo.TargetTemp.Value = changeSetTemp;
                            LoggerManager.Debug($"[TempController] SetSV({backup?.TempChangeSource}). TargetTemp set to {TempInfo.TargetTemp.Value}, input source:{source}");

                            TempInfo.OverHeatingOffset = overHeating;
                            LoggerManager.Debug($"[TempController] SetSV({backup?.TempChangeSource}). OverHeatingOffset set to {TempInfo.OverHeatingOffset}");
                            TempInfo.OverHeatingHysteresis = Hysteresis;
                            LoggerManager.Debug($"[TempController] SetSV({backup?.TempChangeSource}). OverHeatingHysteresis set to {TempInfo.OverHeatingHysteresis}");
                        }
                        UpdateLastestInfoTempInfoHistory(source, changeSetTemp);
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw err;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        // TempCopntrol에 SV를 Set을 안한다.
        // 실제로 Set은 전의 Set값과 현재의 Set값을 비교하여 ControlState안의 TempManager.SetTempWithOption에서 일어난다.
        public new void SetSV(double changeSetTemp, bool willYouSaveSetValue = true, bool forcedSetValue = false)
        {
            try
            {
                lock (TempChangeLockObj)
                {
                    if (willYouSaveSetValue && GetApplySVChangesBasedOnDeviceValue())
                    {
                        TempControllerDevParameter.SetTemp.Value = (double)(changeSetTemp);
                        LoggerManager.Debug($"Set SetTemp for DevParameter : {TempControllerDevParameter.SetTemp.Value}");
                        SaveDevParameter();
                    }

                    if (TempInfo.SetTemp.Value != changeSetTemp & TempInfo.PreSetTemp.Value != TempInfo.SetTemp.Value)
                    {
                        TempInfo.PreSetTemp.Value = TempInfo.SetTemp.Value;
                    }

                    if (forcedSetValue)
                    {
                        SetForcedSetValue(forcedSetValue);
                    }

                    if (TempInfo.TargetTemp.Value != changeSetTemp)
                    {
                        TempInfo.TargetTemp.Value = changeSetTemp;
                        LoggerManager.Debug($"[TempController] SetSV(). TargetTemp set to {TempInfo.TargetTemp.Value}");

                        TempControllerParam.LastSetTargetTemp.Value = TempInfo.TargetTemp.Value;
                        LoggerManager.Debug("[TempController] SetSV(). LastSetTargetTemp system parameter set to {this.TempControllerParam.LastSetTargetTemp.Value}.");
                        SaveSysParameter();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }


        private ModuleStateBase _ModuleState;
        public new ModuleStateBase ModuleState
        {
            get { return _ModuleState; }
            private set { _ModuleState = value; }
        }

        public new EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    CommandRecvSlot = new CommandSlot();
                    RunTokenSet = new CommandTokenSet();

                    ModuleState = new ModuleIdleState(this);
                    temp_backup_lock = new object();

                    InitState();

                    if (TempManager == null)
                    {
                        MakeTempModules();
                        InitTempModules();

                        LoadBackupTempInfo();
                        if (this.TempBackupInfo.TempInfoHistory?.Count > 0)
                        {
                            var reloadBack = this.GetCurrentTempInfoInHistory();
                            if (reloadBack != null)
                                SetSV(reloadBack.TempChangeSource, reloadBack.SetTemp, willYouSaveSetValue: false);
                        }
                        else
                        {
                            InitDevParameter();

                            this.SetSV(TemperatureChangeSource.TEMP_DEVICE, (double)this.TempControllerDevParameter.SetTemp.Value, willYouSaveSetValue: true);
                        }

                        retval = TempManager.StartModule();
                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"TempManager.StartModule() Failed");
                        }
                    }

                    if (TempControllerDevParam != null)
                    {
                        //기존 파라미터에서 새로 생성이 되면 0이고 default 값 set 해주기 위함.
                        if (TempControllerDevParameter.EmergencyAbortTempTolerance.Value == 0)
                        {
                            TempControllerDevParameter.EmergencyAbortTempTolerance.Value = 10;
                            SaveDevParameter();
                        }
                    }

                    if (TempControllerParam != null && TempInfo != null)
                    {
                        var backuptemp = GetCurrentTempInfoInHistory();
                        if (backuptemp?.TempChangeSource == TemperatureChangeSource.UNDEFINED)//TODO: 이 기능이 처음 들어갔을 떄 초기화 해주기위함.
                        {

                            SetSV(TemperatureChangeSource.TEMP_DEVICE, TempControllerDevParameter.SetTemp.Value, willYouSaveSetValue: false);

                        }
                        else if (backuptemp != null)
                        {

                            SetSV(backuptemp.TempChangeSource, backuptemp.SetTemp, willYouSaveSetValue: false);
                        }
                    }

                    Initialized = true;


                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public new void SetActivatedState(bool forced = false)
        {
            try
            {
                ReserveNextState = new Activated(this, forced);
                LoggerManager.Debug($"[TempCtrlSorter] call SetActivatedState()");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //private AutoCooling AutoCooling;
        private DateTime lastUpdateTime;
        private int maxTempHistoryCount = 1 * 60 * 60 * 12; // For 12 hours
        static int logElapsed = 0;
        public override ModuleStateEnum SequenceRun()
        {
            ModuleStateEnum RetVal = ModuleStateEnum.UNDEFINED;
            try
            {
                DateTime time = DateTime.Now;

                var timeFromLastUpdate = (time - lastUpdateTime);
                TempInfo.CurTemp.Value = (double)TempManager.PV;
                TempInfo.MV.Value = (double)TempManager.MV;

                TempInfo.SetTemp.Value = (double)TempManager.SV;

                double updateIntervalInSec = 1;

                if (timeFromLastUpdate.TotalSeconds >= updateIntervalInSec)
                {
                    var CurTemp = (double)TempManager.PV;
                    dataSeries_CurTemp.Append(time, CurTemp / 1.0);
                    dataSeries_SetTemp.Append(time, (double)TempManager.SV / 1.0);
                    if (dataSeries_CurTemp.Count > maxTempHistoryCount | dataSeries_SetTemp.Count > maxTempHistoryCount)
                    {
                        Task.Run(() =>
                        {
                            dataSeries_CurTemp.RemoveAt(0);
                            dataSeries_SetTemp.RemoveAt(0);
                        }).Wait();
                    }
                    if (logElapsed >= TempControllerParam.LoggingInterval.Value * updateIntervalInSec)
                    {
                        LoggerManager.SetTempInfo(TempManager.SV, TempManager.PV, TempInfo.DewPoint.Value, TempInfo.MV.Value);
                        LoggerManager.TempLog($"Temp.: SV = {TempManager.SV / 1.0,5:0.00}, PV = {TempManager.PV / 1.0,5:0.00}, DP = {TempInfo.DewPoint.Value,5:0.00}, MV = {TempInfo.MV.Value,5:0.00}");
                        logElapsed = 0;
                    }
                    else
                    {
                        logElapsed++;
                    }

                    lastUpdateTime = time;
                }

                //Temp를 보고 Purge Air On/ Off(고온에서는 Perge Air를 킬 필요가 없어서)
                bool isPurgeAir = ControlTopPurgeAir();
                if (isPurgeAir != IsPurgeAirBackUpValue)
                {
                    IsPurgeAirBackUpValue = isPurgeAir;
                    var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTESTERHEAD_PURGE, IsPurgeAirBackUpValue);
                    if (ioret != IORet.NO_ERR)
                    {
                        LoggerManager.Debug($"[TempController]SequenceRun(), Fail Purge Air changing. Output Value: {IsPurgeAirBackUpValue}");
                    }
                    else
                    {
                        LoggerManager.Debug($"[TempController]SequenceRun(), Changed Purge Air. Output Value: {IsPurgeAirBackUpValue}");
                    }

                }

                // Fail safe
                if (this.IOManager().IO.Outputs.DOTESTERHEAD_PURGE.IOOveride.Value != EnumIOOverride.EMUL)
                {
                    bool purgeAirState = false;
                    var ioRet = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Outputs.DOTESTERHEAD_PURGE, out purgeAirState);
                    if (purgeAirState != IsPurgeAirBackUpValue)
                    {
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTESTERHEAD_PURGE, IsPurgeAirBackUpValue);
                    }
                }

                RetVal = Execute();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }
    }
}
