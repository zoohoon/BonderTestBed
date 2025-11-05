using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;

namespace LotOP
{


    public class SystemInfo : ISystemInfo, INotifyPropertyChanged, IFactoryModule
    {
        string filePath = @"C:\ProberSystem\SystemInfo\SystemInfo.txt";
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private long _WaferCount;
        public long WaferCount
        {
            get { return _WaferCount; }
            set
            {
                if (value != _WaferCount)
                {
                    _WaferCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _DieCount;
        public long DieCount
        {
            get { return _DieCount; }
            set
            {
                if (value != _DieCount)
                {
                    _DieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _ProcessedWaferCountUntilBeforeCardChange;
        public long ProcessedWaferCountUntilBeforeCardChange
        {
            get { return _ProcessedWaferCountUntilBeforeCardChange; }
            set
            {
                if (value != _ProcessedWaferCountUntilBeforeCardChange)
                {
                    _ProcessedWaferCountUntilBeforeCardChange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _MarkedWaferCountLastPolishWaferCleaning;
        public long MarkedWaferCountLastPolishWaferCleaning
        {
            get { return _MarkedWaferCountLastPolishWaferCleaning; }
            set
            {
                if (value != _MarkedWaferCountLastPolishWaferCleaning)
                {
                    _MarkedWaferCountLastPolishWaferCleaning = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _TouchDownCountUntilBeforeCardChange;
        public long TouchDownCountUntilBeforeCardChange
        {
            get { return _TouchDownCountUntilBeforeCardChange; }
            set
            {
                if (value != _TouchDownCountUntilBeforeCardChange)
                {
                    _TouchDownCountUntilBeforeCardChange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _MarkedTouchDownCountLastPolishWaferCleaning;
        public long MarkedTouchDownCountLastPolishWaferCleaning
        {
            get { return _MarkedTouchDownCountLastPolishWaferCleaning; }
            set
            {
                if (value != _MarkedTouchDownCountLastPolishWaferCleaning)
                {
                    _MarkedTouchDownCountLastPolishWaferCleaning = value;
                    RaisePropertyChanged();
                }
            }
        }

        public SystemInfo()
        {

        }

        public SystemInfo(string filepath)
        {
            try
            {
                this.filePath = filepath;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void IncreaseWaferCount()
        {
            try
            {
                WaferCount++;
                //WriteInfo(WaferCount, DieCount);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void IncreaseDieCount()
        {
            try
            {
                DieCount++;
                //WriteInfo(WaferCount, DieCount);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void IncreaseProcessedWaferCountUntilBeforeCardChange()
        {
            try
            {
                ProcessedWaferCountUntilBeforeCardChange++;

                this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.PROCESSEDWAFERCOUNTUNTILBEFORECARDCHANGE, ProcessedWaferCountUntilBeforeCardChange.ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ResetProcessedWaferCountUntilBeforeCardChange()
        {
            try
            {
                ProcessedWaferCountUntilBeforeCardChange = 0;
                MarkedWaferCountLastPolishWaferCleaning = 0;

                this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.PROCESSEDWAFERCOUNTUNTILBEFORECARDCHANGE, ProcessedWaferCountUntilBeforeCardChange.ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        
        public void SetMarkedWaferCountLastPolishWaferCleaning()
        {
            try
            {
                MarkedWaferCountLastPolishWaferCleaning = ProcessedWaferCountUntilBeforeCardChange;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void IncreaseTouchDownCountUntilBeforeCardChange()
        {
            try
            {
                TouchDownCountUntilBeforeCardChange++;

                this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.TOUCHDOWNCOUNTUNTILBEFORECARDCHANGE, TouchDownCountUntilBeforeCardChange.ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ResetTouchDownCountUntilBeforeCardChange()
        {
            try
            {
                TouchDownCountUntilBeforeCardChange = 0;

                this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.TOUCHDOWNCOUNTUNTILBEFORECARDCHANGE, TouchDownCountUntilBeforeCardChange.ToString());

                MarkedTouchDownCountLastPolishWaferCleaning = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetMarkedTouchDownCountLastPolishWaferCleaning()
        {
            try
            {
                MarkedTouchDownCountLastPolishWaferCleaning = TouchDownCountUntilBeforeCardChange;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LoadInfo()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            StreamReader file = null;
            FileStream fs = null;

            try
            {
                
                if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                bool ExistFile = false;

                ExistFile = File.Exists(filePath);

                if (ExistFile == false)
                {
                    // Create File

                    FileInfo fileInfo = new FileInfo(filePath);

                    try
                    {
                        fs = fileInfo.Create();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                        throw;
                    }
                    finally
                    {
                        if (fs != null)
                        {
                            fs.Close();
                        }
                    }

                    WriteLotInfo();
                }

                //FileInfo fileInfo = new FileInfo(filePath);

                //if (!fileInfo.Exists)
                //{
                //    SaveLotInfo();
                //}

                List<long> ValueList = new List<long>();

                file = new System.IO.StreamReader(filePath);

                while (file.EndOfStream == false)
                {
                    var line = file.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    long pos = 0;
                    if (long.TryParse(line, out pos) == false)
                    {
                        retVal = EventCodeEnum.PARAM_ERROR;
                        break;

                    }

                    ValueList.Add(pos);
                }

                if (ValueList.Count == 2)
                {
                    WaferCount = ValueList[0];
                    DieCount = ValueList[1];
                }
                else if (ValueList.Count == 6)
                {
                    WaferCount = ValueList[0];
                    DieCount = ValueList[1];
                    
                    ProcessedWaferCountUntilBeforeCardChange = ValueList[2];
                    MarkedWaferCountLastPolishWaferCleaning = ValueList[3];
                    
                    TouchDownCountUntilBeforeCardChange = ValueList[4];
                    MarkedTouchDownCountLastPolishWaferCleaning = ValueList[5];
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }

            return retVal;
        }

        public void SaveLotInfo()
        {
            try
            {
                if(this.ProbingModule().ProbingDryRunFlag == false)
                {
                    Thread writerThread = new Thread(new ThreadStart(WriteLotInfo));
                    writerThread.Start();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void WriteLotInfo()
        {
            StreamWriter file = null;

            try
            {
                file = new System.IO.StreamWriter(filePath, false);

                file.WriteLine(WaferCount);
                file.WriteLine(DieCount);
                
                // 2020-06-12
                file.WriteLine(ProcessedWaferCountUntilBeforeCardChange);
                file.WriteLine(MarkedWaferCountLastPolishWaferCleaning);

                file.WriteLine(TouchDownCountUntilBeforeCardChange);
                file.WriteLine(MarkedTouchDownCountLastPolishWaferCleaning);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
        }
    }

    public class DeviceInfo : IDeviceInfo, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private double _WaferCount;
        public double WaferCount
        {
            get { return _WaferCount; }
            set
            {
                if (value != _WaferCount)
                {
                    _WaferCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DieCount;
        public double DieCount
        {
            get { return _DieCount; }
            set
            {
                if (value != _DieCount)
                {
                    _DieCount = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
    public class LotInfo : ILotInfo, IHasComParameterizable, INotifyPropertyChanged
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

        public List<object> Nodes { get; set; }
        //public int _ProcessDieCount;

        //p long _TouchDownCount = { get; set;}

        private long _TouchDownCount;
        public long TouchDownCount
        {
            get { return _TouchDownCount; }
            set
            {
                if (value != _TouchDownCount)
                {
                    _TouchDownCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ProcessedWaferCnt = 0;
        public int ProcessedWaferCnt
        {
            get { return _ProcessedWaferCnt; }
            set
            {
                if (value != _ProcessedWaferCnt)
                {
                    _ProcessedWaferCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _ProcessedDieCnt = 0;
        public int ProcessedDieCnt
        {
            get { return _ProcessedDieCnt; }
            set
            {
                if (value != _ProcessedDieCnt)
                {
                    _ProcessedDieCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// LotMode가 변경되었을 때, ProbingSequenceModule의 Inner State가 함께 변경되어야 한다. 
        /// </summary>
        private Element<LotModeEnum> _LotMode = new Element<LotModeEnum>();
        public Element<LotModeEnum> LotMode
        {
            get { return _LotMode; }
            set
            {
                if (value != _LotMode)
                {
                    _LotMode = value;

                    //this.ProbingSequenceModule().SetProbingInnerState(_LotMode);
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<WaferSummary> _WaferSummarys;
        public ObservableCollection<WaferSummary> WaferSummarys
        {
            get { return _WaferSummarys; }
            set
            {
                if (value != _WaferSummarys)
                {
                    _WaferSummarys = value;
                    RaisePropertyChanged();
                }
            }
        }

        private LotAccumulateInfo _AccumulateInfo = new LotAccumulateInfo();
        public LotAccumulateInfo AccumulateInfo
        {
            get { return _AccumulateInfo; }
            set
            {
                if (value != _AccumulateInfo)
                {
                    _AccumulateInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _LoadedWaferCountUntilBeforeLotStart;
        public int LoadedWaferCountUntilBeforeLotStart
        {
            get { return _LoadedWaferCountUntilBeforeLotStart; }
            set
            {
                _LoadedWaferCountUntilBeforeLotStart = value;
                RaisePropertyChanged();
            }
        }

        private int _LoadedWaferCountUntilBeforeDeviceChange;
        public int LoadedWaferCountUntilBeforeDeviceChange
        {
            get { return _LoadedWaferCountUntilBeforeDeviceChange; }
            set
            {
                _LoadedWaferCountUntilBeforeDeviceChange = value;
                RaisePropertyChanged();
            }
        }

        private Element<string> _LotName = new Element<string>();
        public Element<string> LotName
        {
            get { return _LotName; }
            set
            {
                if (value != _LotName)
                {
                    _LotName = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _DeviceName = new Element<string>();
        public Element<string> DeviceName
        {
            get { return _DeviceName; }
            set
            {
                if (value != _DeviceName)
                {
                    _DeviceName = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<string> _OperatorID = new Element<string>();
        public Element<string> OperatorID
        {
            get { return _OperatorID; }
            set
            {
                if (value != _OperatorID)
                {
                    _OperatorID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _FoupNumber = new Element<int>();
        public Element<int> FoupNumber
        {
            get { return _FoupNumber; }
            set
            {
                if (value != _FoupNumber)
                {
                    _FoupNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CSTHashCode;
        public string CSTHashCode
        {
            get { return _CSTHashCode; }
            set
            {
                if (value != _CSTHashCode)
                {
                    _CSTHashCode = value;
                    RaisePropertyChanged();
                }
            }
        }


        private bool _LotStartTimeEnable;
        public bool LotStartTimeEnable
        {
            get { return _LotStartTimeEnable; }
            set
            {
                if (value != _LotStartTimeEnable)
                {
                    _LotStartTimeEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _LotEndTimeEnable;
        public bool LotEndTimeEnable
        {
            get { return _LotEndTimeEnable; }
            set
            {
                if (value != _LotEndTimeEnable)
                {
                    _LotEndTimeEnable = value;
                    RaisePropertyChanged();
                }
            }
        }


        private DateTime _LotStartTime;
        public DateTime LotStartTime
        {
            get { return _LotStartTime; }
            set
            {
                if (value != _LotStartTime)
                {
                    _LotStartTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DateTime _LotEndTime;
        public DateTime LotEndTime
        {
            get { return _LotEndTime; }
            set
            {
                if (value != _LotEndTime)
                {
                    _LotEndTime = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _StopAfterScanCSTFlag;
        public bool StopAfterScanCSTFlag
        {
            get { return _StopAfterScanCSTFlag; }
            set
            {
                if (value != _StopAfterScanCSTFlag)
                {
                    _StopAfterScanCSTFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _StopBeforeProbeFlag;
        public bool StopBeforeProbeFlag
        {
            get { return _StopBeforeProbeFlag; }
            set
            {
                if (value != _StopBeforeProbeFlag)
                {
                    _StopBeforeProbeFlag = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool? _ContinueLot;
        public bool? ContinueLot
        {
            get { return _ContinueLot; }
            set
            {
                if (value != _ContinueLot)
                {
                    _ContinueLot = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _NeedLotDeallocated = false;
        public bool NeedLotDeallocated
        {
            get { return _NeedLotDeallocated; }
            set
            {
                if (value != _NeedLotDeallocated)
                {
                    _NeedLotDeallocated = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ResultMapDownloadTriggerEnum _ResultMapDownloadTrigger;
        public ResultMapDownloadTriggerEnum ResultMapDownloadTrigger
        {
            get { return _ResultMapDownloadTrigger; }
            set
            {
                if (value != _ResultMapDownloadTrigger)
                {
                    _ResultMapDownloadTrigger = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<DynamicModeEnum> _DynamicMode = new Element<DynamicModeEnum>();
        [DataMember]
        public Element<DynamicModeEnum> DynamicMode
        {
            get { return _DynamicMode; }
            set
            {
                if (value != _DynamicMode)
                {
                    _DynamicMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<StageLotInfo> _StageLotInfos = new List<StageLotInfo>();

        public List<StageLotInfo> StageLotInfos
        {
            get { return _StageLotInfos; }
            set { _StageLotInfos = value; }
        }

        private bool _isNewLot = false;
        public bool isNewLot
        {
            get { return _isNewLot; }
            set
            {
                if (value != _isNewLot)
                {
                    _isNewLot = value;
                    RaisePropertyChanged();
                }
            }
        }

        public object lockobject = new object();

        public object GetPIVDataLockObject()
        {
            return lockobject;
        }
        #region <!-- StageLotInfo 관련 함수들 -->

        public List<StageLotInfo> GetLotInfos()
        {
            return StageLotInfos;
        }

        public void CreateLotInfo(int foupnumber, string recipeid = "", string lotid = "", bool isAssignLot = false, string cstHashCode = "", string carrierid = null)
        {
            try
            {
                StageLotInfo info = null;

                if (DynamicMode.Value == DynamicModeEnum.DYNAMIC)
                {
                    info = StageLotInfos.Find(
                        stageinfo => stageinfo.FoupIndex == foupnumber && stageinfo.LotID.Equals(lotid)
                        && stageinfo.CassetteHashCode.Equals(cstHashCode));
                }
                else
                {
                    info = StageLotInfos.Find(
                        stageinfo => stageinfo.FoupIndex == foupnumber);
                }


                if (info == null)
                {
                    if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING || isAssignLot)
                    {
                        if (recipeid == "")
                        {
                            recipeid = this.FileManager().GetDeviceName();
                        }
                        StageLotInfos.Add(new StageLotInfo(foupnumber, lotid, recipeid, cstHashCode, carrierid));
                        LoggerManager.Debug($"Create StageLotInfo foupnumber : {foupnumber}, lot id : {lotid}, cst hashcode : {cstHashCode}, carrierid: {carrierid}");
                        SetStageLotAssignState(LotAssignStateEnum.ASSIGNED, lotid, cstHashCode);
                    }
                    else
                    {
                        LoggerManager.Debug($"Not Create StageLotInfo foupnumber : {foupnumber}, lot id : {lotid}, cst hashcode : {cstHashCode}, carrierid: {carrierid}" +
                            $". LOT state is {this.LotOPModule().ModuleState.GetState()}, AssignLot State is {isAssignLot}");
                    }
                }
                else
                {
                    //LOT Id
                    if (lotid == " ")
                    {
                        info.LotID = "";
                    }
                    else if (lotid != "")
                        info.LotID = lotid;

                    LoggerManager.Debug($"Update LotID of StageLotInfo foupnumber : {foupnumber}, lot id : {lotid}, cst hashcode : {cstHashCode}" +
                           $". LOT state is {this.LotOPModule().ModuleState.GetState()}, AssignLot State is {isAssignLot}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void RemoveLotInfo(int foupnumber, string lotid = "", string cstHashCode = "")
        {
            try
            {
                //LotID.Value = "";
                StageLotInfo info;

                if (lotid.Equals(""))
                    lotid = this.LotOPModule().LotInfo.LotName.Value;
                if (cstHashCode.Equals(""))
                    cstHashCode = CSTHashCode;
                if (string.IsNullOrEmpty(cstHashCode))
                {
                    info = StageLotInfos.Find(stageinfo => stageinfo.FoupIndex == foupnumber
                                                        && stageinfo.LotID.Equals(lotid));
                }
                else
                {
                    info = StageLotInfos.Find(stageinfo => stageinfo.FoupIndex == foupnumber
                                                        && stageinfo.LotID.Equals(lotid)
                                                        && stageinfo.CassetteHashCode.Equals(cstHashCode));
                }
                if (info != null)
                {
                    if(this.LotOPModule().LotInfo.DynamicMode.Value != DynamicModeEnum.DYNAMIC)
                    {
                        StageLotInfos.Remove(info);
                        LoggerManager.Debug($"Remove StageLotInfo foupnumber : {foupnumber}");
                    }                    
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ClearLotInfos()
        {
            try
            {
                if (StageLotInfos != null)
                {
                    foreach (var info in StageLotInfos)
                    {
                        LoggerManager.Debug($"ClearStageLotInfos() LOTID : {info.LotID}, FOUPIDX : {info.FoupIndex}");
                    }

                    StageLotInfos.Clear();

                    this.LotOPModule().IsNeedLotEnd = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetCassetteHashCode(int foupnumber, string lotid, string cstHashCode)
        {
            try
            {
                var lotInfos = StageLotInfos.FindAll(info => info.FoupIndex == foupnumber && info.LotID.Equals(lotid));
                if (lotInfos != null)
                {
                    foreach (var lotinfo in lotInfos)
                    {
                        if (lotinfo.CassetteHashCode != null)
                        {
                            if (lotinfo.CassetteHashCode.Equals(""))
                            {
                                lotinfo.CassetteHashCode = cstHashCode;
                                LoggerManager.Debug($"Set StageLotInfo foupnumber : {lotinfo.FoupIndex}, lot id : {lotinfo.LotID}, cst hashcode : {lotinfo.CassetteHashCode}");
                                break;
                            }
                            else
                            {
                                LoggerManager.Debug($"Already exist cassette hash code foupnumber : {lotinfo.FoupIndex}, lot id : {lotinfo.LotID}, cst hashcode : {lotinfo.CassetteHashCode}");
                            }
                        }
                        else
                        {
                            lotinfo.CassetteHashCode = cstHashCode;
                            LoggerManager.Debug($"Set StageLotInfo foupnumber : {lotinfo.FoupIndex}, lot id : {lotinfo.LotID}, cst hashcode : {lotinfo.CassetteHashCode}");
                            break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public void SetLotStarted(int foupnumber, string lotid, string cstHashCode)
        {
            try
            {
                var lotInfo = StageLotInfos.FindAll(info => info.FoupIndex == foupnumber && info.LotID.Equals(lotid) && info.CassetteHashCode == cstHashCode).FirstOrDefault();
                if (lotInfo != null)
                {
                    if (lotInfo.IsLotStarted == false)
                    {
                        lotInfo.IsLotStarted = true;
                        LoggerManager.Debug($"Set SetLotStarted lotinfo foupnumber : {foupnumber}, lot id : {lotid}, cst hashcode : {cstHashCode}");
                    }
                }
                else
                {
                    LoggerManager.Debug($"Set SetLotStarted failed. cannot find lotinfo foupnumber : {foupnumber}, lot id : {lotid}, cst hashcode : {cstHashCode}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }



        public void SetCassetteHashCode(string csthashcode)
        {
            try
            {
                LoggerManager.Debug($"SetCassetteHashCode Before: {CSTHashCode}");
                CSTHashCode = csthashcode;
                LoggerManager.Debug($"SetCassetteHashCode After: {CSTHashCode}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetDevDownResult(bool result, int foupnumber, string deviceid, string lotid)
        {
            try
            {
                this.GEMModule().GetPIVContainer().SetDevDownResult(result);

                var stgObj = StageLotInfos.Find(info => info.FoupIndex == foupnumber && info.LotID.Equals(lotid));
                if (stgObj != null)
                {
                    stgObj.DevDownResult = result;
                    stgObj.ProcDevOP = true;

                    if (string.IsNullOrEmpty(stgObj.RecipeID))
                    {
                        stgObj.RecipeID = deviceid;
                    }
                    else if (stgObj.RecipeID.Equals(deviceid) == false)
                    {
                        stgObj.RecipeID = deviceid;
                    }
                    LoggerManager.Debug($"SetDevDownResult(). Foup Number : {foupnumber}, Device : {deviceid}, LOT Id : {lotid}, Result : {result}");
                }
                else
                {
                    //SetDeviceName(deviceid);
                    LoggerManager.Debug($"SetDevDownResult() StageLotInfo is null. Foup Number : {foupnumber}, Device : {deviceid}, LOT Id : {lotid}, Result : {result}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetDevLoadResult(bool result, int foupnumber = 0, string deviceid = "", string lotid = "")
        {
            try
            {
                this.GEMModule().GetPIVContainer().SetDevDownResult(result);

                var stgObj = StageLotInfos.Find(info => info.FoupIndex == foupnumber && info.LotID.Equals(lotid));
                if (stgObj != null)
                {
                    // ProcDevOP  가 True 라는건 Download 받은적이 있다는 것.
                    if (stgObj.ProcDevOP)
                    {
                        stgObj.DevLoadResult = result;

                        if (deviceid.Equals("") == false)
                        {
                            if (stgObj.RecipeID.Equals(deviceid) == false)
                            {
                                stgObj.RecipeID = deviceid;
                            }
                        }

                        LoggerManager.Debug($"SetDevLoadResult(). Foup Number : {foupnumber}, Device : {deviceid}, LOT Id : {lotid}, Result : {result}");
                    }
                }
                else
                {
                    LoggerManager.Debug($"SetDevLoadResult() StageLotInfo is null. Foup Number : {foupnumber}, Device : {deviceid}, LOT Id : {lotid}, Result : {result}");
                }
                this.GEMModule().GetPIVContainer().SetDeviceName(deviceid);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool GetDevResult(int foupnumber = 0, string lotid = "", bool getdownresult = false, bool getloadresult = false)
        {
            bool retVal = false;

            try
            {
                StageLotInfo stgObj = null;

                if (foupnumber != 0 && lotid != "")
                {
                    stgObj = StageLotInfos.Find(info => info.FoupIndex == foupnumber && info.LotID.Equals(lotid));
                }
                else if (foupnumber != 0)
                {
                    stgObj = StageLotInfos.Find(info => info.FoupIndex == foupnumber);
                }
                else if (lotid != "")
                {
                    stgObj = StageLotInfos.Find(info => info.LotID.Equals(lotid));
                }

                if (stgObj != null)
                {
                    if (stgObj.ProcDevOP)
                    {
                        if (getdownresult && getloadresult)
                        {
                            retVal = stgObj.DevDownResult && stgObj.DevLoadResult;
                            LoggerManager.Debug($"GetDevResult() Foup Index#{foupnumber}, Lot ID#{lotid}" +
                                $" : DevDownResult is {stgObj.DevDownResult}, DevLoadResult is {stgObj.DevLoadResult}");
                        }
                        else if (getdownresult)
                        {
                            retVal = stgObj.DevDownResult;
                            LoggerManager.Debug($"GetDevResult() Foup Index#{foupnumber}, Lot ID#{lotid}" +
                                $" : DevDownResult is {stgObj.DevDownResult}");
                        }
                        else if (getloadresult)
                        {
                            retVal = stgObj.DevLoadResult;
                            LoggerManager.Debug($"GetDevResult() Foup Index#{foupnumber}, Lot ID#{lotid}" +
                                $" : DevLoadResult is {stgObj.DevLoadResult}");
                        }
                    }
                    else
                    {
                        if (this.GEMModule().GetPIVContainer().DevDownResult.Value == 1)
                            retVal = true;
                        else if (this.GEMModule().GetPIVContainer().DevDownResult.Value == 0)
                            retVal = false;

                        LoggerManager.Debug($"GetDevResult() : DevDownResult is {retVal}");
                    }
                }
                else
                {
                    if (this.GEMModule().GetPIVContainer().DevDownResult.Value == 1)
                        retVal = true;
                    else if (this.GEMModule().GetPIVContainer().DevDownResult.Value == 0)
                        retVal = false;

                    LoggerManager.Debug($"GetDevResult() : DevDownResult is {retVal}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //if (LoadPortDevDownResults.ContainsKey(foupnumber) != null)
            //{
            //    LoadPortDevDownResults.TryGetValue(foupnumber, out retVal);
            //}

            return retVal;
        }

        public string GetLotIDAtStageLotInfos(int foupnumber)
        {
            string lotid = "";
            try
            {
                var lotinfo = StageLotInfos.Find(info => info.FoupIndex == foupnumber);
                if (lotinfo != null)
                {
                    if (lotinfo.LotID != null)
                    {
                        lotid = lotinfo.LotID;
                    }
                    else
                    {
                        lotid = "";
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return lotid;
        }

        public int GetFoupNumbetAtStageLotInfos(string lotid)
        {
            int foupnumber = 0;
            try
            {
                var lotinfo = StageLotInfos.Find(info => info.LotID.Equals(lotid));
                if (lotinfo != null)
                {
                    foupnumber = lotinfo.FoupIndex;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return foupnumber;
        }

        public void SetStageLotAssignState(LotAssignStateEnum assignStateEnum, string lotid ="", string cstHashCode = "")
        {
            try
            {
                StageLotInfo lotinfo = null;

                if (String.IsNullOrEmpty(lotid) && !String.IsNullOrEmpty(this.LotName.Value))
                {
                    lotid = this.LotName.Value;
                }
                if (String.IsNullOrEmpty(cstHashCode) && !String.IsNullOrEmpty(this.CSTHashCode))
                {
                    cstHashCode = this.CSTHashCode;
                }

                if (!String.IsNullOrEmpty(lotid) && !String.IsNullOrEmpty(cstHashCode))
                {
                    lotinfo = StageLotInfos.Find(info => info.LotID.Equals(lotid) && info.CassetteHashCode.Equals(cstHashCode));
                }
                else if (!String.IsNullOrEmpty(cstHashCode))
                {
                    lotinfo = StageLotInfos.Find(info => info.CassetteHashCode.Equals(cstHashCode));
                }
                else if (!String.IsNullOrEmpty(lotid))
                {
                    lotinfo = StageLotInfos.Find(info => info.LotID.Equals(lotid));
                }

                if(lotinfo != null)
                {
                    if (lotinfo.StageLotAssignState != assignStateEnum)
                    {
                        if (assignStateEnum == LotAssignStateEnum.PROCESSING)
                        {
                            if (lotinfo.StageLotAssignState == LotAssignStateEnum.ASSIGNED)
                            {
                                lotinfo.StageLotAssignState = assignStateEnum;
                                LoggerManager.Debug($"[LotInfo] SetStageLotAssignState(). LOT ID : {lotinfo.LotID}, CST HASHCODE : {lotinfo.CassetteHashCode}, FOUP NUM : {lotinfo.FoupIndex}, STG LOT ASSING STAGE : {lotinfo.StageLotAssignState}");
                            }
                            else
                            {
                                LoggerManager.Debug($"[LotInfo] SetStageLotAssignState() set to PROCESSING stage fail. LOT ID : {lotinfo.LotID}, CST HASHCODE : {lotinfo.CassetteHashCode}, FOUP NUM : {lotinfo.FoupIndex}, STG LOT ASSING STAGE : {lotinfo.StageLotAssignState}");
                            }
                        }
                        else
                        {
                            lotinfo.StageLotAssignState = assignStateEnum;
                            LoggerManager.Debug($"[LotInfo] SetStageLotAssignState(). LOT ID : {lotinfo.LotID}, CST HASHCODE : {lotinfo.CassetteHashCode}, FOUP NUM : {lotinfo.FoupIndex}, STG LOT ASSING STAGE : {lotinfo.StageLotAssignState}");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public LotAssignStateEnum GetStageLotAssignState(string lotid = "", string cstHashCode = "")
        {
            LotAssignStateEnum lotAssignState = LotAssignStateEnum.UNASSIGNED;
            try
            {
                if(String.IsNullOrEmpty(lotid) && !String.IsNullOrEmpty(this.LotName.Value))
                {
                    lotid = this.LotName.Value;
                }
                if(String.IsNullOrEmpty(cstHashCode) && !String.IsNullOrEmpty(this.CSTHashCode))
                {
                    cstHashCode = this.CSTHashCode;
                }

                if (!String.IsNullOrEmpty(lotid) && !String.IsNullOrEmpty(cstHashCode))
                {
                    var lotinfo = StageLotInfos.Find(info => info.LotID.Equals(lotid) && info.CassetteHashCode.Equals(cstHashCode));
                    if(lotinfo != null)
                    {
                        lotAssignState = lotinfo.StageLotAssignState;
                    }
                }
                else if(!String.IsNullOrEmpty(cstHashCode))
                {
                    var lotinfo = StageLotInfos.Find(info => info.CassetteHashCode.Equals(cstHashCode));
                    if (lotinfo != null)
                    {
                        lotAssignState = lotinfo.StageLotAssignState;
                    }
                }
                else if(!String.IsNullOrEmpty(lotid))
                {
                    var lotinfo = StageLotInfos.Find(info => info.LotID.Equals(lotid));
                    if (lotinfo != null)
                    {
                        lotAssignState = lotinfo.StageLotAssignState;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return lotAssignState;
        }

        #endregion

        /// <summary>
        /// 한번 이상 웨이퍼와 핀이 터치한 웨이퍼 갯(장)수
        /// </summary>
        /// <returns></returns>
        public int ProcessWaferCount()
        {
            return WaferSummarys.Count(item => item.WaferStatus == EnumSubsStatus.EXIST
            && (item.WaferState == EnumWaferState.PROCESSED | item.WaferState == EnumWaferState.PROBING | item.WaferState == EnumWaferState.TESTED | item.WaferHolder.Equals("CHUCK")));
        }
        public int ProcessedWaferCount()
        {
            return WaferSummarys.Count(item => item.WaferStatus == EnumSubsStatus.EXIST && item.WaferState == EnumWaferState.PROCESSED);
        }
        public int UnProcessedWaferCount()
        {
            return WaferSummarys.Count(item => item.DoProceedProbing == true && item.WaferStatus == EnumSubsStatus.EXIST && item.WaferState == EnumWaferState.UNPROCESSED);
        }

        //public int ProcessedDieCount()
        //{
        //    return _ProcessDieCount;
        //}

        public void IncreaseTouchDownCount()
        {
            _TouchDownCount++;
        }
        //public long TouchDownCount()
        //{
        //    return _TouchDownCount;
        //}   

        public void UpdateWafer(IWaferObject waferObject)
        {
            try
            {
                int slotcount = 25;

                var Wafer = WaferSummarys.Where(item => item.SlotNumber == (waferObject.GetSubsInfo().SlotIndex.Value % slotcount == 0 ? 25 : waferObject.GetSubsInfo().SlotIndex.Value % slotcount)).FirstOrDefault();

                if (Wafer != null)
                {
                    Wafer.Yield = waferObject.GetSubsInfo().Yield;
                    Wafer.RetestYield = waferObject.GetSubsInfo().RetestYield;
                    Wafer.ProbingStartTime = (waferObject.GetSubsInfo() as SubstrateInfo).ProbingStartTime;
                    Wafer.ProbingEndTime = (waferObject.GetSubsInfo() as SubstrateInfo).ProbingEndTime;
                    Wafer.WaferState = waferObject.GetState();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void UpdateWafer(WaferSummary waferSummary)
        {
            try
            {
                var Wafer = WaferSummarys.Where(item => item.SlotNumber == waferSummary.SlotNumber).FirstOrDefault();
                if (Wafer != null)
                {
                    Wafer.WaferID = waferSummary.WaferID;
                    Wafer.Yield = waferSummary.Yield;
                    Wafer.RetestYield = waferSummary.RetestYield;
                    Wafer.ProbingStartTime = waferSummary.ProbingStartTime;
                    Wafer.ProbingEndTime = waferSummary.ProbingEndTime;
                    Wafer.WaferState = waferSummary.WaferState;
                    Wafer.WaferStatus = waferSummary.WaferStatus;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //public void UpdateSlotState(List<int> slots)
        //{
        //    try
        //    {
        //        for(int i =0; i< slots.Count; i++)
        //        {
        //            WaferSummary Wafer = WaferSummarys.Where(item => item.SlotNumber == i + 1).FirstOrDefault();
        //            if (Wafer != null)
        //            {
        //                if (slots[i] == 1)
        //                {
        //                    Wafer.WaferState = EnumWaferState.UNPROCESSED;
        //                }
        //                else
        //                {
        //                    Wafer.WaferState = EnumWaferState.UNDEFINED;
        //                }
                            
        //            }
        //        }                
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}

        public void SetWaferID(int slotNum, string waferID)
        {
            try
            {
                var Wafer = WaferSummarys.Where(item => item.SlotNumber == slotNum).FirstOrDefault();
                if (Wafer != null)
                {
                    Wafer.WaferID = waferID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public string GetWaferID(int slotNum)
        {
            string retWaferID = string.Empty;
            try
            {
                var Wafer = WaferSummarys.Where(item => item.SlotNumber == slotNum).FirstOrDefault();
                if (Wafer != null)
                {
                    retWaferID = Wafer.WaferID;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retWaferID;
        }
        public void SetHolder(int slotNum, string holder)
        {
            try
            {
                var Wafer = WaferSummarys.Where(item => item.SlotNumber == slotNum).FirstOrDefault();
                if (Wafer != null)
                {
                    Wafer.WaferHolder = holder;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void SetWaferState(int slotNum, EnumSubsStatus waferStatus, EnumWaferState waferState)
        {
            try
            {
                var Wafer = WaferSummarys.Where(item => item.SlotNumber == slotNum).FirstOrDefault();
                if (Wafer != null)
                {
                    Wafer.WaferStatus = waferStatus;
                    Wafer.WaferState = waferState;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void WaferSwapChanged(int originSlotNum, int changeSlotNum, bool isInit = false)
        {
            try
            {
                var originWafer = WaferSummarys.Where(item => item.SlotNumber == originSlotNum).FirstOrDefault();
                var changeWafer = WaferSummarys.Where(item => item.SlotNumber == changeSlotNum).FirstOrDefault();
                if (originWafer != null && changeWafer != null)
                {
                    changeWafer.WaferID = originWafer.WaferID;
                    changeWafer.ProbingEndTime = originWafer.ProbingEndTime;
                    changeWafer.ProbingStartTime = originWafer.ProbingStartTime;
                    changeWafer.WaferState = originWafer.WaferState;
                    changeWafer.WaferStatus = originWafer.WaferStatus;
                    changeWafer.Yield = originWafer.Yield;
                    changeWafer.RetestYield = originWafer.RetestYield;

                    if (isInit)
                    {
                        originWafer.WaferID = string.Empty;
                        originWafer.ProbingEndTime = null;
                        originWafer.ProbingStartTime = null;
                        originWafer.WaferState = EnumWaferState.UNDEFINED;
                        originWafer.WaferStatus = EnumSubsStatus.NOT_EXIST;
                        originWafer.Yield = 0;
                        originWafer.RetestYield = 0;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public bool IsLoadingWafer(int slotNum)
        {
            bool retVal = false;
            try
            {
                var Wafer = WaferSummarys.Where(item => item.SlotNumber == slotNum).FirstOrDefault();
                if (Wafer != null)
                {
                    if (Wafer.DoProceedProbing != null)
                    {
                        retVal = Wafer.DoProceedProbing.Value;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public void ClearWaferSummary()
        {
            try
            {
                foreach (var wafer in WaferSummarys)
                {
                    wafer.WaferID = String.Empty;
                    wafer.Yield = -1;
                    wafer.RetestYield = -1;
                    wafer.ProbingStartTime = null;
                    wafer.ProbingEndTime = null;
                    wafer.WaferState = EnumWaferState.UNDEFINED;
                    wafer.WaferStatus = EnumSubsStatus.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetLotName(string lotName)
        {
            try
            {
                if (LotName == null)
                {
                    LotName = new Element<string>();
                }
                LotName.Value = lotName;
                LoggerManager.Debug($"[LotInfo] SetLotName. LotName : {LotName.Value}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetFoupInfo(int foupnumber, string cstHashCode)
        {
            try
            {
                if(FoupNumber == null)
                {
                    FoupNumber = new Element<int>();
                }
                FoupNumber.Value = foupnumber;

                if(String.IsNullOrEmpty(cstHashCode) == false)
                {
                    CSTHashCode = cstHashCode;
                }

                LoggerManager.Debug($"[LotInfo] SetFoupInfo. FoupNumber : {FoupNumber.Value}, CSTHashCode : {CSTHashCode}.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetElementMetaData()
        {

        }

        private void Init()
        {
            try
            {
                _LotStartTime = DateTime.MinValue;
                _LotEndTime = _LotStartTime;

                //_StopBeforeWaferSet = new List<bool>();
                //_StopAfterWaferSet = new List<bool>();
                this.LotName = new Element<string>();
                LotName.Value = $"ID" + $"{DateTime.Now.Year}" + $"{DateTime.Now.Month}" + $"{DateTime.Now.Day}" + $"{DateTime.Now.Hour}" + $"{DateTime.Now.Minute}" + $"{DateTime.Now.Second}";
                //LotName.Value = $"YNOT2884";
                DeviceName.Value = "DEF08";
                OperatorID.Value = "Semics";
                _ContinueLot = false;

                LotMode.Value = LotModeEnum.CP1;

                WaferSummarys = new ObservableCollection<WaferSummary>();

                for (int i = 0; i < 25; i++)
                {
                    WaferSummary tmp = new WaferSummary();

                    tmp.SlotNumber = (i + 1);
                    tmp.WaferID = string.Empty;
                    WaferSummarys.Add(tmp);
                }
                //for (int i = 0; i < 25; i++)
                //{
                //    _StopBeforeWaferSet.Add(false);
                //    _StopAfterWaferSet.Add(false);
                //}

                //YieldList = new ObservableCollection<LotInfoYieldStructure>();

                //for (int i = 0; i < 25; i++)
                //{
                //    LotInfoYieldStructure tmp = new LotInfoYieldStructure();

                //    tmp.SlotNumber = i + 1;

                //    YieldList.Add(tmp);
                //}

                // TEST CODE
                ResultMapDownloadTrigger = ResultMapDownloadTriggerEnum.LOT_NAME_INPUT_TRIGGER;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public LotInfo()
        {
            Init();
        }
    }
}
