using System;
using System.Collections.Generic;
using System.Linq;

namespace NotifyModule
{
    using LogModule;
    using NotifyModule.Parameter;
    using NotifyParamObject;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public class NotifyManager : INotifyManager, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Property
        public bool Initialized { get; set; } = false;
        private object Lockobj;
        private NotifySystemParameter _NotifySysParam;
        public IParam NotifySysParam
        {
            get { return _NotifySysParam; }
            set
            {
                if (_NotifySysParam != value)
                {
                    _NotifySysParam = value as NotifySystemParameter;
                }
            }
        }

        private List<EventCodeParam> _NoticeParams;

        public List<EventCodeParam> NoticeParams
        {
            get { return _NoticeParams; }
            set { _NoticeParams = value; }
        }

        public InitPriorityEnum InitPriority { get; set; }

        #endregion

        private string LastStageMSG { get; set; }

        #region Creator & Init
        public NotifyManager()
        {

        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                NoticeParams = new List<EventCodeParam>();
                Lockobj = new object();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            return InitModule();
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region DevParam
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new NotifySystemParameter();
                retVal = this.LoadParameter(ref tmpParam, typeof(NotifySystemParameter));

                if (retVal == EventCodeEnum.NONE)
                {
                    NotifySysParam = tmpParam as NotifySystemParameter;
                }
                retVal = EventCodeEnum.NONE;
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
                this.SaveParameter(NotifySysParam);
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region Method
        /// <param name="indexOffset"></param> : Foup, PreAlign 등 Index 를 가지고 있는경우 Parameter 에 offset Index 를 적용해 Gem Alarm 을 발생시켜야 한다.
        public EventCodeEnum Notify(EventCodeEnum errorCode, int indexOffset = 1, bool isStack = false)
        {
            EventCodeEnum retval = EventCodeEnum.NOTIFY_ERROR;

            try
            {
                var noticeInfo = _NotifySysParam.NoticeEventCodeParam.ToList<EventCodeParam>().Find(param => param?.EventCode == errorCode);

                if (noticeInfo != null)
                {
                    retval = Notify(noticeInfo, indexOffset, isStack);
                }
                else
                {
                    LoggerManager.Debug($"NotifySysParam Undefinded error Code: {errorCode}");

                    EventCodeParam param = new EventCodeParam();
                    param.GemAlaramNumber = -1;
                    param.EventCode = errorCode;
                    param.EnableNotifyProlog = false;

                    retval = Notify(param, indexOffset, isStack);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.NOTIFY_ERROR;
            }

            return retval;
        }
        public EventCodeEnum Notify(EventCodeEnum errorCode, string message, int indexOffset = 1, bool isStack = false)
        {
            EventCodeEnum retval = EventCodeEnum.NOTIFY_ERROR;

            try
            {
                var noticeInfo = _NotifySysParam.NoticeEventCodeParam.ToList<EventCodeParam>().Find(param => param.EventCode == errorCode);

                if (noticeInfo != null)
                {
                    if (message != "")
                    {
                        noticeInfo.Message = message;
                    }
                    retval = Notify(noticeInfo, indexOffset);
                }
                else
                {
                    retval = errorCode;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.NOTIFY_ERROR;
            }

            return retval;
        }
        public EventCodeEnum Notify(EventCodeParam noticeparam, int indexOffset = 1, bool isStack = false)
        {
            EventCodeEnum retval = EventCodeEnum.NOTIFY_ERROR;
            try
            {
                retval = NotifyMessage(noticeparam, isStack: isStack);//stage 쪽 

                //Cell 의 Gem Alram 은 위의 NotifyMessage 에서 실행되므로, Loader 와 Single 일 경우에만 동작하도록.
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote || (SystemManager.SysteMode == SystemModeEnum.Single && SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober))
                {
                    if (noticeparam.GemAlaramNumber != -1)
                    {
                        if (noticeparam.Index != 0)
                        {
                            indexOffset = noticeparam.Index;
                        }

                        var alarmnumber = noticeparam.GemAlaramNumber + (indexOffset - 1);//loader 쪽
                        this.GEMModule().SetAlarm(alarmnumber, (long)ProberInterfaces.GEM.GemAlarmState.SET, noticeparam.Index);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.NOTIFY_ERROR;
            }

            return retval;
        }
        public EventCodeEnum NotifyMessage(EventCodeParam param, int indexOffset = 1, bool isStack = false)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (param.OccurTime.CompareTo(DateTime.Now) < 0)
                {
                    param.OccurTime = DateTime.Now;
                }

                long gemalaramnumber = param.GemAlaramNumber;

                // CELL
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober && SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    param.Index = this.LoaderController().GetChuckIndex();

                    if (param.GemAlaramNumber != -1)
                    {
                        if (param.Index != 0)
                        {
                            gemalaramnumber = param.GemAlaramNumber + (param.Index - 1);
                        }
                        else
                        {
                            gemalaramnumber = param.GemAlaramNumber + (indexOffset - 1);
                        }
                    }

                    LoggerManager.Debug($"[{this.GetType().Name}], NotifyMessage() : Index = {param.Index}, OccurTime = {param.OccurTime}, EventCode = {param.EventCode}, ALID = {gemalaramnumber}, Message = {param.Message}, EnableNotifyEventlog = {param.EnableNotifyEventlog}, EnableNotifyProlog = {param.EnableNotifyProlog}, EnableNotifyMessageDialog = {param.EnableNotifyMessageDialog}", isInfo: true);

                    if (!isStack)
                    {
                        var connectedNotifyCode = this.VisionManager().GetConnectedNotifyCode();
                        
                        if (param.EventCode == connectedNotifyCode)
                        {
                            var (currentModuleType, lastModuleStartTime, lastHashCode) = this.VisionManager().GetImageDataSetIdentifiers();
                            
                            Task.Run(() => this.VisionManager().SaveModuleImagesAsync(currentModuleType));

                            param.ModuleType = (EnumProberModule)currentModuleType;
                            param.ModuleStartTime = lastModuleStartTime;
                            param.ImageDatasHashCode = lastHashCode;
                        }
                        else
                        {
                            param.ImageDatasHashCode = string.Empty;
                            param.ModuleStartTime = string.Empty;
                        }
                    }

                    retval = this.LoaderController().NotifyStageAlarm(param);//로더에 notify 불러줌.

                    if (retval != EventCodeEnum.NONE)
                    {
                        if (NoticeParams == null)
                        {
                            NoticeParams = new List<EventCodeParam>();
                        }
                        lock (Lockobj)
                        {
                            NoticeParams.Add(param);
                        }
                    }

                    if (param.EnableNotifyEventlog)
                    {
                        LoggerManager.Event(param.Index, param.OccurTime, param.EventCode, param.Message, true, moduleType: param.ModuleType, moduleStartTime:param.ModuleStartTime, imageDatasHashCode: param.ImageDatasHashCode);
                    }

                    if (param.EnableNotifyProlog)
                    {
                        LoggerManager.Prolog(param.ProLogType, param.EventCode, param.EventCode, param.Message);
                    }
                    retval = EventCodeEnum.NONE;
                }
                // LOADER
                else if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote ||
                        (SystemManager.SysteMode == SystemModeEnum.Single && SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober))
                {
                    if (param.GemAlaramNumber != -1)
                    {
                        if (param.Index != 0)
                        {
                            gemalaramnumber = param.GemAlaramNumber + (param.Index - 1);
                        }
                        else
                        {
                            gemalaramnumber = param.GemAlaramNumber + (indexOffset - 1);
                        }
                    }

                    LoggerManager.Debug($"[{this.GetType().Name}], NotifyMessage() : Index = {param.Index}, OccurTime = {param.OccurTime}, EventCode = {param.EventCode}, ALID = {gemalaramnumber}, Message = {param.Message}, EnableNotifyEventlog = {param.EnableNotifyEventlog}, EnableNotifyProlog = {param.EnableNotifyProlog}, EnableNotifyMessageDialog = {param.EnableNotifyMessageDialog}", isInfo: true);

                    if (param.EnableNotifyEventlog)
                    {
                        if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote && param.Index != 0)
                        {
                            LoggerManager.Event(param.Index, param.OccurTime, param.EventCode, param.Message, false, true, moduleType: param.ModuleType, moduleStartTime: param.ModuleStartTime, imageDatasHashCode: param.ImageDatasHashCode);
                        }
                        else
                        {
                            LoggerManager.Event(param.Index, param.OccurTime, param.EventCode, param.Message, true, true, moduleType: param.ModuleType, moduleStartTime: param.ModuleStartTime, imageDatasHashCode: param.ImageDatasHashCode);
                        }
                    }

                    if (param.EnableNotifyProlog)
                    {
                        if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote && param.Index != 0)
                        {
                            LoggerManager.Prolog(param.ProLogType, param.EventCode, param.EventCode, param.Message);
                        }
                        else
                        {
                            LoggerManager.Prolog(param.ProLogType, param.EventCode, param.EventCode, param.Message);
                        }
                    }

                    if (param.EnableNotifyMessageDialog)
                    {
                        if (param.Index != 0)
                        {
                            string cellstr = string.Format("{0}{1}", "CELL", param.Index);
                            param.Title = $"{param.Title} in [{cellstr}]";
                        }

                        this.MetroDialogManager().ShowMessageDialog(param.Title, param.Message, MetroDialogInterfaces.EnumMessageStyle.Affirmative);
                    }

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retval = EventCodeEnum.NOTIFY_ERROR;
            }

            return retval;
        }
        public EventCodeEnum ClearNotify(EventCodeEnum errorCode, int indexOffset = 1)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var noticeInfo = _NotifySysParam.NoticeEventCodeParam.ToList<EventCodeParam>().Find(
               param => param.EventCode == errorCode);

                if (noticeInfo != null)
                {
                    if (noticeInfo.GemAlaramNumber != -1)
                    {
                        if (noticeInfo.Index != 0)
                        {
                            indexOffset = noticeInfo.Index;
                        }
                        var alarmnumber = noticeInfo.GemAlaramNumber + (indexOffset - 1);
                        this.GEMModule().SetAlarm(alarmnumber, (long)ProberInterfaces.GEM.GemAlarmState.CLEAR, indexOffset - 1);
                    }
                }
                else
                {
                    //파라미터 없는데 Event codes 일 경우 prolog 띄우기.
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetLastStageMSG(string msg)
        {
            try
            {
                LastStageMSG = msg;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public string GetLastStageMSG()
        {
            return LastStageMSG;
        }
        public void SendLastMSGToLoader()
        {
            try
            {
                int index = this.LoaderController().GetChuckIndex();

                if (LastStageMSG != string.Empty)
                {
                    //this.StageSupervisor()?.ServiceCallBack?.SetTitleMessage(index, LastStageMSG);
                    this.LoaderController().SetTitleMessage(index, LastStageMSG);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum NotifyStackParams()
        {
            EventCodeEnum retval = EventCodeEnum.NOTIFY_ERROR;
            try
            {
                EventCodeParam a = new EventCodeParam();

                lock (Lockobj)
                {
                    List<EventCodeParam> removeparamlist = new List<EventCodeParam>();

                    if (NoticeParams?.Count > 0)
                    {
                        foreach (var param in NoticeParams)
                        {
                            retval = Notify(param.EventCode, param.Index, true);

                            if (retval == EventCodeEnum.NONE)
                            {
                                removeparamlist.Add(param);
                            }
                        }

                        foreach (var removeparam in removeparamlist)
                        {
                            NoticeParams.Remove(removeparam);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return retval;
        }
        public bool IsCriticalError(EventCodeEnum eventcode)
        {
            bool retval = false;

            try
            {
                EventCodeParam eventcodeparam = _NotifySysParam.NoticeEventCodeParam.FirstOrDefault(e => e.EventCode == eventcode);

                if (eventcodeparam != null)
                {
                    if (eventcodeparam.ProberErrorKind == EnumProberErrorKind.CRITICAL)
                    {
                        retval = true;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeParam GetNotifyParam(EventCodeEnum errorCode)
        {
            EventCodeParam param = null;
            try
            {
                if (_NotifySysParam != null && _NotifySysParam.NoticeEventCodeParam != null)
                {
                    param = _NotifySysParam.NoticeEventCodeParam.FirstOrDefault(notifyparam => notifyparam.EventCode == errorCode);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return param;
        }
        #endregion
    }
}
