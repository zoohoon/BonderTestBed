using System;
using System.Collections.Generic;

namespace NotifyModule.Parameter
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class NotifySystemParameter : IParam, INotifyPropertyChanged, ISystemParameterizable
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IParam Property

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [JsonIgnore, ParamIgnore]
        public List<object> Nodes { get; set; }
        [JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [JsonIgnore, ParamIgnore]
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
        [JsonIgnore, ParamIgnore]
        public string FilePath { get; } = "Notify";
        [JsonIgnore, ParamIgnore]
        public string FileName { get; } = "NotifySysParam.Json";

        #endregion

        #region Property

        private ObservableCollection<EventCodeParam> _NoticeEventCodeParam
            = new ObservableCollection<EventCodeParam>();
        public ObservableCollection<EventCodeParam> NoticeEventCodeParam
        {
            get { return _NoticeEventCodeParam; }
            set
            {
                if (value != _NoticeEventCodeParam)
                {
                    _NoticeEventCodeParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public NotifySystemParameter()
        {

        }

        #region IParam Method

        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SetDefaultParam();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (NoticeEventCodeParam == null)
                    NoticeEventCodeParam = new ObservableCollection<EventCodeParam>();

                EventCodeParam ccrecoveryfail = new EventCodeParam();
                ccrecoveryfail.EventCode = EventCodeEnum.GP_CardChange_RECOVERY_FAIL;
                ccrecoveryfail.Message = "Error occurd while card change recovery";
                ccrecoveryfail.Title = "Card change recovery fail";
                ccrecoveryfail.EnableNotifyMessageDialog = true;
                NoticeEventCodeParam?.Add(ccrecoveryfail);

                EventCodeParam notifylatchparam = new EventCodeParam();
                notifylatchparam.EventCode = EventCodeEnum.GP_CardChange_CHECK_TO_LATCH;
                notifylatchparam.Message = "Have to check that probe card latch";
                notifylatchparam.Title = "latch is lock";
                notifylatchparam.EnableNotifyMessageDialog = true;
                NoticeEventCodeParam?.Add(notifylatchparam);

                #region <!-- ALCD 5 -->

                EventCodeParam probingsequenceinvaliderror = new EventCodeParam();
                probingsequenceinvaliderror.EventCode = EventCodeEnum.PROBING_SEQUENCE_INVALID_ERROR;
                probingsequenceinvaliderror.ProberErrorKind = EnumProberErrorKind.INVALID;
                probingsequenceinvaliderror.Title = "";
                probingsequenceinvaliderror.Message = "";
                probingsequenceinvaliderror.GemAlaramNumber = 1001;
                probingsequenceinvaliderror.EnableNotifyMessageDialog = false;
                probingsequenceinvaliderror.EnableNotifyEventlog = true;
                probingsequenceinvaliderror.EnableNotifyProlog = false;
                probingsequenceinvaliderror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(probingsequenceinvaliderror);

                EventCodeParam markalignfocusingfailed = new EventCodeParam();
                markalignfocusingfailed.EventCode = EventCodeEnum.MARK_ALIGN_FOCUSING_FAILED;
                markalignfocusingfailed.ProberErrorKind = EnumProberErrorKind.INVALID;
                markalignfocusingfailed.Title = "";
                markalignfocusingfailed.Message = "";
                markalignfocusingfailed.GemAlaramNumber = 13001;
                markalignfocusingfailed.EnableNotifyMessageDialog = false;
                markalignfocusingfailed.EnableNotifyEventlog = true;
                markalignfocusingfailed.EnableNotifyProlog = false;
                markalignfocusingfailed.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(markalignfocusingfailed);

                EventCodeParam markalignpatternmatchfailed = new EventCodeParam();
                markalignpatternmatchfailed.EventCode = EventCodeEnum.MARK_ALGIN_PATTERN_MATCH_FAILED;
                markalignpatternmatchfailed.ProberErrorKind = EnumProberErrorKind.INVALID;
                markalignpatternmatchfailed.Title = "";
                markalignpatternmatchfailed.Message = "";
                markalignpatternmatchfailed.GemAlaramNumber = 13051;
                markalignpatternmatchfailed.EnableNotifyMessageDialog = false;
                markalignpatternmatchfailed.EnableNotifyEventlog = true;
                markalignpatternmatchfailed.EnableNotifyProlog = false;
                markalignpatternmatchfailed.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(markalignpatternmatchfailed);

                EventCodeParam markalignfailed = new EventCodeParam();
                markalignfailed.EventCode = EventCodeEnum.MARK_ALIGN_FAIL;
                markalignfailed.ProberErrorKind = EnumProberErrorKind.INVALID;
                markalignfailed.Title = "";
                markalignfailed.Message = "";
                markalignfailed.GemAlaramNumber = 13101;
                markalignfailed.EnableNotifyMessageDialog = false;
                markalignfailed.EnableNotifyEventlog = true;
                markalignfailed.EnableNotifyProlog = false;
                markalignfailed.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(markalignfailed);


                EventCodeParam waferalignfail = new EventCodeParam();
                waferalignfail.EventCode = EventCodeEnum.WAFER_ALING_FAIL;
                waferalignfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                waferalignfail.Title = "";
                waferalignfail.Message = "";
                waferalignfail.GemAlaramNumber = 14001;
                waferalignfail.EnableNotifyMessageDialog = false;
                waferalignfail.EnableNotifyEventlog = true;
                waferalignfail.EnableNotifyProlog = false;
                waferalignfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(waferalignfail);

                EventCodeParam pinalignfailed = new EventCodeParam();
                pinalignfailed.EventCode = EventCodeEnum.PIN_ALIGN_FAILED;
                pinalignfailed.ProberErrorKind = EnumProberErrorKind.INVALID;
                pinalignfailed.Title = "";
                pinalignfailed.Message = "";
                pinalignfailed.GemAlaramNumber = 15001;
                pinalignfailed.EnableNotifyMessageDialog = false;
                pinalignfailed.EnableNotifyEventlog = true;
                pinalignfailed.EnableNotifyProlog = false;
                pinalignfailed.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pinalignfailed);

                EventCodeParam cardchangefail = new EventCodeParam();
                cardchangefail.EventCode = EventCodeEnum.CARD_CHANGE_FAIL;
                cardchangefail.ProberErrorKind = EnumProberErrorKind.INVALID;
                cardchangefail.Title = "";
                cardchangefail.Message = "";
                cardchangefail.GemAlaramNumber = 18001;
                cardchangefail.EnableNotifyMessageDialog = false;
                cardchangefail.EnableNotifyEventlog = true;
                cardchangefail.EnableNotifyProlog = false;
                cardchangefail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(cardchangefail);

                EventCodeParam heaterpowersupplyfail = new EventCodeParam();
                heaterpowersupplyfail.EventCode = EventCodeEnum.HEATER_POWER_SUPPLY_FAIL;
                heaterpowersupplyfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                heaterpowersupplyfail.Title = "";
                heaterpowersupplyfail.Message = "";
                heaterpowersupplyfail.GemAlaramNumber = 23001;
                heaterpowersupplyfail.EnableNotifyMessageDialog = false;
                heaterpowersupplyfail.EnableNotifyEventlog = true;
                heaterpowersupplyfail.EnableNotifyProlog = false;
                heaterpowersupplyfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(heaterpowersupplyfail);

                EventCodeParam dewpointhigh = new EventCodeParam();
                dewpointhigh.EventCode = EventCodeEnum.DEW_POINT_HIGH_ERR;
                dewpointhigh.ProberErrorKind = EnumProberErrorKind.INVALID;
                dewpointhigh.Title = "Error Message";
                dewpointhigh.Message = $"ERROR CODE : {dewpointhigh.EventCode}.\rDew Point is higher than coolant temperature. Please check dew point of cell and dry air state.";
                dewpointhigh.GemAlaramNumber = 23101;
                dewpointhigh.EnableNotifyMessageDialog = true;
                dewpointhigh.EnableNotifyEventlog = true;
                dewpointhigh.EnableNotifyProlog = false;
                dewpointhigh.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(dewpointhigh);

                EventCodeParam monitoringmainpwr = new EventCodeParam();
                monitoringmainpwr.EventCode = EventCodeEnum.MONITORING_MAIN_POWER_ERROR;
                monitoringmainpwr.ProberErrorKind = EnumProberErrorKind.INVALID;
                monitoringmainpwr.Title = "";
                monitoringmainpwr.Message = "";
                monitoringmainpwr.GemAlaramNumber = 50001;
                monitoringmainpwr.EnableNotifyMessageDialog = false;
                monitoringmainpwr.EnableNotifyEventlog = true;
                monitoringmainpwr.EnableNotifyProlog = false;
                monitoringmainpwr.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(monitoringmainpwr);

                EventCodeParam emoerror = new EventCodeParam();
                emoerror.EventCode = EventCodeEnum.EMO_ERROR;
                emoerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                emoerror.Title = "Error Message";
                emoerror.Message = "EMO error occured.";
                emoerror.GemAlaramNumber = 50002;
                emoerror.EnableNotifyMessageDialog = true;
                emoerror.EnableNotifyEventlog = true;
                emoerror.EnableNotifyProlog = false;
                emoerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(emoerror);

                EventCodeParam emoerrorform = new EventCodeParam();
                emoerrorform.EventCode = EventCodeEnum.EMO_ERROR_FORM_TESTER;
                emoerrorform.ProberErrorKind = EnumProberErrorKind.INVALID;
                emoerrorform.Title = "Error Message";
                emoerrorform.Message = "EMO error occured from tester";
                emoerrorform.GemAlaramNumber = 50003;
                emoerrorform.EnableNotifyMessageDialog = true;
                emoerrorform.EnableNotifyEventlog = true;
                emoerrorform.EnableNotifyProlog = false;
                emoerrorform.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(emoerrorform);

                EventCodeParam stagemainairerror = new EventCodeParam();
                stagemainairerror.EventCode = EventCodeEnum.STAGE_MAIN_AIR_ERROR;
                stagemainairerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                stagemainairerror.Title = "Error Message";
                stagemainairerror.Message = "Main air error in stage has occured. Please check the status of the vacuum.";
                stagemainairerror.GemAlaramNumber = 50011;
                stagemainairerror.EnableNotifyMessageDialog = true;
                stagemainairerror.EnableNotifyEventlog = true;
                stagemainairerror.EnableNotifyProlog = false;
                stagemainairerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(stagemainairerror);

                EventCodeParam stagemainvacerror = new EventCodeParam();
                stagemainvacerror.EventCode = EventCodeEnum.STAGE_MAIN_VAC_ERROR;
                stagemainvacerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                stagemainvacerror.Title = "Error Message";
                stagemainvacerror.Message = "Stage main vacuum error in stage has occurred. Please check the status of the vacuum.";
                stagemainvacerror.GemAlaramNumber = 50012;
                stagemainvacerror.EnableNotifyMessageDialog = true;
                stagemainvacerror.EnableNotifyEventlog = true;
                stagemainvacerror.EnableNotifyProlog = false;
                stagemainvacerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(stagemainvacerror);

                EventCodeParam loadermainairerror = new EventCodeParam();
                loadermainairerror.EventCode = EventCodeEnum.LOADER_MAIN_AIR_ERROR;
                loadermainairerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                loadermainairerror.Title = "Error Message";
                loadermainairerror.Message = "Main air error in loader has occurred. Please check the status of the air.";
                loadermainairerror.GemAlaramNumber = 50013;
                loadermainairerror.EnableNotifyMessageDialog = true;
                loadermainairerror.EnableNotifyEventlog = true;
                loadermainairerror.EnableNotifyProlog = false;
                loadermainairerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(loadermainairerror);

                EventCodeParam loadermainvacerror = new EventCodeParam();
                loadermainvacerror.EventCode = EventCodeEnum.LOADER_MAIN_VAC_ERROR;
                loadermainvacerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                loadermainvacerror.Title = "Error Message";
                loadermainvacerror.Message = "Loader main vacuum error in stage has occurred. Please check the status of the vacuum.";
                loadermainvacerror.GemAlaramNumber = 50014;
                loadermainvacerror.EnableNotifyMessageDialog = true;
                loadermainvacerror.EnableNotifyEventlog = true;
                loadermainvacerror.EnableNotifyProlog = false;
                loadermainvacerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(loadermainvacerror);

                EventCodeParam loaderrightdooropen = new EventCodeParam();
                loaderrightdooropen.EventCode = EventCodeEnum.LOADER_RIGHT_DOOR_OPEN;
                loaderrightdooropen.ProberErrorKind = EnumProberErrorKind.INVALID;
                loaderrightdooropen.Title = "Error Message";
                loaderrightdooropen.Message = "The right door of Loader is open. Please check.";
                loaderrightdooropen.GemAlaramNumber = 50015;
                loaderrightdooropen.EnableNotifyMessageDialog = true;
                loaderrightdooropen.EnableNotifyEventlog = true;
                loaderrightdooropen.EnableNotifyProlog = false;
                loaderrightdooropen.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(loaderrightdooropen);

                EventCodeParam loaderleftdooropen = new EventCodeParam();
                loaderleftdooropen.EventCode = EventCodeEnum.LOADER_LEFT_DOOR_OPEN;
                loaderleftdooropen.ProberErrorKind = EnumProberErrorKind.INVALID;
                loaderleftdooropen.Title = "Error Message";
                loaderleftdooropen.Message = "The left door of Loader is open. Please check.";
                loaderleftdooropen.GemAlaramNumber = 50016;
                loaderleftdooropen.EnableNotifyMessageDialog = true;
                loaderleftdooropen.EnableNotifyEventlog = true;
                loaderleftdooropen.EnableNotifyProlog = false;
                loaderleftdooropen.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(loaderleftdooropen);

                EventCodeParam loaderpcwleak = new EventCodeParam();
                loaderpcwleak.EventCode = EventCodeEnum.LOADER_PCW_LEAK_ALARM;
                loaderpcwleak.ProberErrorKind = EnumProberErrorKind.INVALID;
                loaderpcwleak.Title = "Information Message";
                loaderpcwleak.Message = "Leak detected on main PCW. Please check the main PCW line status.";
                loaderpcwleak.GemAlaramNumber = 50019;
                loaderpcwleak.EnableNotifyMessageDialog = true;
                loaderpcwleak.EnableNotifyEventlog = false;
                loaderpcwleak.EnableNotifyProlog = false;
                loaderpcwleak.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(loaderpcwleak);

                EventCodeParam systemerror = new EventCodeParam();
                systemerror.EventCode = EventCodeEnum.SYSTEM_ERROR;
                systemerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                systemerror.Title = "";
                systemerror.Message = "";
                systemerror.GemAlaramNumber = 50101;
                systemerror.EnableNotifyMessageDialog = false;
                systemerror.EnableNotifyEventlog = true;
                systemerror.EnableNotifyProlog = false;
                systemerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(systemerror);

                EventCodeParam systemfilemissing = new EventCodeParam();
                systemfilemissing.EventCode = EventCodeEnum.SYSTEM_FILE_MISSING;
                systemfilemissing.ProberErrorKind = EnumProberErrorKind.INVALID;
                systemfilemissing.Title = "";
                systemfilemissing.Message = "";
                systemfilemissing.GemAlaramNumber = 50151;
                systemfilemissing.EnableNotifyMessageDialog = false;
                systemfilemissing.EnableNotifyEventlog = true;
                systemfilemissing.EnableNotifyProlog = false;
                systemfilemissing.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(systemfilemissing);

                EventCodeParam harddiskfull = new EventCodeParam();
                harddiskfull.EventCode = EventCodeEnum.HARD_DISK_FULL;
                harddiskfull.ProberErrorKind = EnumProberErrorKind.INVALID;
                harddiskfull.Title = "";
                harddiskfull.Message = "";
                harddiskfull.GemAlaramNumber = 50201;
                harddiskfull.EnableNotifyMessageDialog = false;
                harddiskfull.EnableNotifyEventlog = true;
                harddiskfull.EnableNotifyProlog = false;
                harddiskfull.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(harddiskfull);

                EventCodeParam directorypatherror = new EventCodeParam();
                directorypatherror.EventCode = EventCodeEnum.DIRECTORY_PATH_ERROR;
                directorypatherror.ProberErrorKind = EnumProberErrorKind.INVALID;
                directorypatherror.Title = "";
                directorypatherror.Message = "";
                directorypatherror.GemAlaramNumber = 50251;
                directorypatherror.EnableNotifyMessageDialog = false;
                directorypatherror.EnableNotifyEventlog = true;
                directorypatherror.EnableNotifyProlog = false;
                directorypatherror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(directorypatherror);

                EventCodeParam invalidparameterfind = new EventCodeParam();
                invalidparameterfind.EventCode = EventCodeEnum.INVALID_PARAMETER_FIND;
                invalidparameterfind.ProberErrorKind = EnumProberErrorKind.INVALID;
                invalidparameterfind.Title = "";
                invalidparameterfind.Message = "";
                invalidparameterfind.GemAlaramNumber = 50301;
                invalidparameterfind.EnableNotifyMessageDialog = false;
                invalidparameterfind.EnableNotifyEventlog = true;
                invalidparameterfind.EnableNotifyProlog = false;
                invalidparameterfind.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(invalidparameterfind);

                EventCodeParam monitoringchuckvacerror = new EventCodeParam();
                monitoringchuckvacerror.EventCode = EventCodeEnum.MONITORING_CHUCK_6VAC_ERROR;
                monitoringchuckvacerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                monitoringchuckvacerror.Title = "";
                monitoringchuckvacerror.Message = "";
                monitoringchuckvacerror.GemAlaramNumber = 50351;
                monitoringchuckvacerror.EnableNotifyMessageDialog = false;
                monitoringchuckvacerror.EnableNotifyEventlog = true;
                monitoringchuckvacerror.EnableNotifyProlog = false;
                monitoringchuckvacerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(monitoringchuckvacerror);

                EventCodeParam monitoringchuck8vacerror = new EventCodeParam();
                monitoringchuck8vacerror.EventCode = EventCodeEnum.MONITORING_CHUCK_8VAC_ERROR;
                monitoringchuck8vacerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                monitoringchuck8vacerror.Title = "";
                monitoringchuck8vacerror.Message = "";
                monitoringchuck8vacerror.GemAlaramNumber = 50351;
                monitoringchuck8vacerror.EnableNotifyMessageDialog = false;
                monitoringchuck8vacerror.EnableNotifyEventlog = true;
                monitoringchuck8vacerror.EnableNotifyProlog = false;
                monitoringchuck8vacerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(monitoringchuck8vacerror);

                EventCodeParam monitoringchuck12vacerror = new EventCodeParam();
                monitoringchuck12vacerror.EventCode = EventCodeEnum.MONITORING_CHUCK_12VAC_ERROR;
                monitoringchuck12vacerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                monitoringchuck12vacerror.Title = "";
                monitoringchuck12vacerror.Message = "";
                monitoringchuck12vacerror.GemAlaramNumber = 50351;
                monitoringchuck12vacerror.EnableNotifyMessageDialog = false;
                monitoringchuck12vacerror.EnableNotifyEventlog = true;
                monitoringchuck12vacerror.EnableNotifyProlog = false;
                monitoringchuck12vacerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(monitoringchuck12vacerror);

                EventCodeParam monitoringmachineiniterror = new EventCodeParam();
                monitoringmachineiniterror.EventCode = EventCodeEnum.MONITORING_MACHINE_INIT_ERROR;
                monitoringmachineiniterror.ProberErrorKind = EnumProberErrorKind.INVALID;
                monitoringmachineiniterror.Title = "";
                monitoringmachineiniterror.Message = "";
                monitoringmachineiniterror.GemAlaramNumber = 50221;
                monitoringmachineiniterror.EnableNotifyMessageDialog = false;
                monitoringmachineiniterror.EnableNotifyEventlog = true;
                monitoringmachineiniterror.EnableNotifyProlog = false;
                monitoringmachineiniterror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(monitoringmachineiniterror);


                EventCodeParam wafernotexisterror = new EventCodeParam();
                wafernotexisterror.EventCode = EventCodeEnum.WAFER_NOT_EXIST_EROOR;
                wafernotexisterror.ProberErrorKind = EnumProberErrorKind.INVALID;
                wafernotexisterror.Title = "";
                wafernotexisterror.Message = "";
                wafernotexisterror.GemAlaramNumber = 50451;
                wafernotexisterror.EnableNotifyMessageDialog = false;
                wafernotexisterror.EnableNotifyEventlog = true;
                wafernotexisterror.EnableNotifyProlog = false;
                wafernotexisterror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(wafernotexisterror);

                EventCodeParam cassettelockerror = new EventCodeParam();
                cassettelockerror.EventCode = EventCodeEnum.CASSETTE_LOCK_ERROR;
                cassettelockerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                cassettelockerror.Title = "";
                cassettelockerror.Message = "";
                cassettelockerror.GemAlaramNumber = 60001;
                cassettelockerror.EnableNotifyMessageDialog = false;
                cassettelockerror.EnableNotifyEventlog = true;
                cassettelockerror.EnableNotifyProlog = false;
                cassettelockerror.ProLogType = PrologType.UNDEFINED;
                cassettelockerror.MessageOccurTitle = "Load Port";
                NoticeEventCodeParam?.Add(cassettelockerror);

                EventCodeParam cassetteunlockerror = new EventCodeParam();
                cassetteunlockerror.EventCode = EventCodeEnum.CASSETTE_UNLOCK_ERROR;
                cassetteunlockerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                cassetteunlockerror.Title = "";
                cassetteunlockerror.Message = "";
                cassetteunlockerror.GemAlaramNumber = 60011;
                cassetteunlockerror.EnableNotifyMessageDialog = false;
                cassetteunlockerror.EnableNotifyEventlog = true;
                cassetteunlockerror.EnableNotifyProlog = false;
                cassetteunlockerror.ProLogType = PrologType.UNDEFINED;
                cassetteunlockerror.MessageOccurTitle = "Load Port";
                NoticeEventCodeParam?.Add(cassetteunlockerror);

                EventCodeParam foupopenerror = new EventCodeParam();
                foupopenerror.EventCode = EventCodeEnum.FOUP_OPEN_ERROR;
                foupopenerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                foupopenerror.Title = "";
                foupopenerror.Message = "";
                foupopenerror.GemAlaramNumber = 60021;
                foupopenerror.EnableNotifyMessageDialog = false;
                foupopenerror.EnableNotifyEventlog = true;
                foupopenerror.EnableNotifyProlog = false;
                foupopenerror.ProLogType = PrologType.UNDEFINED;
                foupopenerror.MessageOccurTitle = "Load Port";
                NoticeEventCodeParam?.Add(foupopenerror);

                EventCodeParam foupcloseerror = new EventCodeParam();
                foupcloseerror.EventCode = EventCodeEnum.FOUP_CLOSE_ERROR;
                foupcloseerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                foupcloseerror.Title = "";
                foupcloseerror.Message = "";
                foupcloseerror.GemAlaramNumber = 60031;
                foupcloseerror.EnableNotifyMessageDialog = false;
                foupcloseerror.EnableNotifyEventlog = true;
                foupcloseerror.EnableNotifyProlog = false;
                foupcloseerror.ProLogType = PrologType.UNDEFINED;
                foupcloseerror.MessageOccurTitle = "Load Port";
                NoticeEventCodeParam?.Add(foupcloseerror);

                EventCodeParam fouploaderror = new EventCodeParam();
                fouploaderror.EventCode = EventCodeEnum.FOUP_LOAD_ERROR;
                fouploaderror.ProberErrorKind = EnumProberErrorKind.INVALID;
                fouploaderror.Title = "";
                fouploaderror.Message = "";
                fouploaderror.GemAlaramNumber = 60041;
                fouploaderror.EnableNotifyMessageDialog = false;
                fouploaderror.EnableNotifyEventlog = true;
                fouploaderror.EnableNotifyProlog = false;
                fouploaderror.ProLogType = PrologType.UNDEFINED;
                fouploaderror.MessageOccurTitle = "Load Port";
                NoticeEventCodeParam?.Add(fouploaderror);

                EventCodeParam foupunloaderror = new EventCodeParam();
                foupunloaderror.EventCode = EventCodeEnum.FOUP_UNLOAD_ERROR;
                foupunloaderror.ProberErrorKind = EnumProberErrorKind.INVALID;
                foupunloaderror.Title = "";
                foupunloaderror.Message = "";
                foupunloaderror.GemAlaramNumber = 60051;
                foupunloaderror.EnableNotifyMessageDialog = false;
                foupunloaderror.EnableNotifyEventlog = true;
                foupunloaderror.EnableNotifyProlog = false;
                foupunloaderror.ProLogType = PrologType.UNDEFINED;
                foupunloaderror.MessageOccurTitle = "Load Port";
                NoticeEventCodeParam?.Add(foupunloaderror);

                EventCodeParam foupscanfailed = new EventCodeParam();
                foupscanfailed.EventCode = EventCodeEnum.FOUP_SCAN_FAILED;
                foupscanfailed.ProberErrorKind = EnumProberErrorKind.INVALID;
                foupscanfailed.Title = "";
                foupscanfailed.Message = "";
                foupscanfailed.GemAlaramNumber = 60061;
                foupscanfailed.EnableNotifyMessageDialog = false;
                foupscanfailed.EnableNotifyEventlog = true;
                foupscanfailed.EnableNotifyProlog = false;
                foupscanfailed.ProLogType = PrologType.UNDEFINED;
                foupscanfailed.MessageOccurTitle = "Load Port";
                NoticeEventCodeParam?.Add(foupscanfailed);

                EventCodeParam loaderpafailed = new EventCodeParam();
                loaderpafailed.EventCode = EventCodeEnum.LOADER_PA_FAILED;
                loaderpafailed.ProberErrorKind = EnumProberErrorKind.INVALID;
                loaderpafailed.Title = "";
                loaderpafailed.Message = "";
                loaderpafailed.GemAlaramNumber = 61001;
                loaderpafailed.EnableNotifyMessageDialog = false;
                loaderpafailed.EnableNotifyEventlog = true;
                loaderpafailed.EnableNotifyProlog = false;
                loaderpafailed.ProLogType = PrologType.UNDEFINED;
                loaderpafailed.MessageOccurTitle = "Pre Aligner";
                NoticeEventCodeParam?.Add(loaderpafailed);

                EventCodeParam loaderpavacerror = new EventCodeParam();
                loaderpavacerror.EventCode = EventCodeEnum.LOADER_PA_VAC_ERROR;
                loaderpavacerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                loaderpavacerror.Title = "";
                loaderpavacerror.Message = "";
                loaderpavacerror.GemAlaramNumber = 61011;
                loaderpavacerror.EnableNotifyMessageDialog = false;
                loaderpavacerror.EnableNotifyEventlog = true;
                loaderpavacerror.EnableNotifyProlog = false;
                loaderpavacerror.ProLogType = PrologType.UNDEFINED;
                loaderpavacerror.MessageOccurTitle = "Pre Aligner";
                NoticeEventCodeParam?.Add(loaderpavacerror);

                EventCodeParam loaderfindnotchfail = new EventCodeParam();
                loaderfindnotchfail.EventCode = EventCodeEnum.LOADER_FIND_NOTCH_FAIL;
                loaderfindnotchfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                loaderfindnotchfail.Title = "";
                loaderfindnotchfail.Message = "";
                loaderfindnotchfail.GemAlaramNumber = 61021;
                loaderfindnotchfail.EnableNotifyMessageDialog = false;
                loaderfindnotchfail.EnableNotifyEventlog = true;
                loaderfindnotchfail.EnableNotifyProlog = false;
                loaderfindnotchfail.ProLogType = PrologType.UNDEFINED;
                loaderfindnotchfail.MessageOccurTitle = "Pre Aligner";
                NoticeEventCodeParam?.Add(loaderfindnotchfail);

                EventCodeParam loaderpawafmissed = new EventCodeParam();
                loaderpawafmissed.EventCode = EventCodeEnum.LOADER_PA_WAF_MISSED;
                loaderpawafmissed.ProberErrorKind = EnumProberErrorKind.INVALID;
                loaderpawafmissed.Title = "";
                loaderpawafmissed.Message = "";
                loaderpawafmissed.GemAlaramNumber = 61041;
                loaderpawafmissed.EnableNotifyMessageDialog = false;
                loaderpawafmissed.EnableNotifyEventlog = true;
                loaderpawafmissed.EnableNotifyProlog = false;
                loaderpawafmissed.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(loaderpawafmissed);

                EventCodeParam loaderwafermissedonarm = new EventCodeParam();
                loaderwafermissedonarm.EventCode = EventCodeEnum.LOADER_WAFER_MISSED_ON_ARM;
                loaderwafermissedonarm.ProberErrorKind = EnumProberErrorKind.INVALID;
                loaderwafermissedonarm.Title = "";
                loaderwafermissedonarm.Message = "";
                loaderwafermissedonarm.GemAlaramNumber = 62001;
                loaderwafermissedonarm.EnableNotifyMessageDialog = false;
                loaderwafermissedonarm.EnableNotifyEventlog = true;
                loaderwafermissedonarm.EnableNotifyProlog = false;
                loaderwafermissedonarm.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(loaderwafermissedonarm);

                EventCodeParam loaderfixedtraywafmissed = new EventCodeParam();
                loaderfixedtraywafmissed.EventCode = EventCodeEnum.LOADER_FIXED_TRAY_WAF_MISSED;
                loaderfixedtraywafmissed.ProberErrorKind = EnumProberErrorKind.INVALID;
                loaderfixedtraywafmissed.Title = "";
                loaderfixedtraywafmissed.Message = "";
                loaderfixedtraywafmissed.GemAlaramNumber = 63001;
                loaderfixedtraywafmissed.EnableNotifyMessageDialog = false;
                loaderfixedtraywafmissed.EnableNotifyEventlog = true;
                loaderfixedtraywafmissed.EnableNotifyProlog = false;
                loaderfixedtraywafmissed.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(loaderfixedtraywafmissed);

                EventCodeParam loaderbufferwafmissed = new EventCodeParam();
                loaderbufferwafmissed.EventCode = EventCodeEnum.LOADER_BUFFER_WAF_MISSED;
                loaderbufferwafmissed.ProberErrorKind = EnumProberErrorKind.INVALID;
                loaderbufferwafmissed.Title = "";
                loaderbufferwafmissed.Message = "";
                loaderbufferwafmissed.GemAlaramNumber = 64001;
                loaderbufferwafmissed.EnableNotifyMessageDialog = false;
                loaderbufferwafmissed.EnableNotifyEventlog = true;
                loaderbufferwafmissed.EnableNotifyProlog = false;
                loaderbufferwafmissed.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(loaderbufferwafmissed);

                EventCodeParam Loader_ARM_TO_BUFFER_Transfer = new EventCodeParam();
                Loader_ARM_TO_BUFFER_Transfer.EventCode = EventCodeEnum.LOADER_ARM_TO_BUFFER_TRANSFER_ERROR;
                Loader_ARM_TO_BUFFER_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_ARM_TO_BUFFER_Transfer.Title = "Information Message";
                Loader_ARM_TO_BUFFER_Transfer.Message = "Arm To Buffer Transfer Error.";
                Loader_ARM_TO_BUFFER_Transfer.GemAlaramNumber = 65000;
                Loader_ARM_TO_BUFFER_Transfer.EnableNotifyMessageDialog = false;
                Loader_ARM_TO_BUFFER_Transfer.EnableNotifyEventlog = false;
                Loader_ARM_TO_BUFFER_Transfer.EnableNotifyProlog = false;
                Loader_ARM_TO_BUFFER_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_ARM_TO_BUFFER_Transfer);

                EventCodeParam Loader_ARM_TO_STAGE_Transfer = new EventCodeParam();
                Loader_ARM_TO_STAGE_Transfer.EventCode = EventCodeEnum.LOADER_ARM_TO_STAGE_TRANSFER_ERROR;
                Loader_ARM_TO_STAGE_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_ARM_TO_STAGE_Transfer.Title = "Information Message";
                Loader_ARM_TO_STAGE_Transfer.Message = "Arm To Stage Transfer Error.";
                Loader_ARM_TO_STAGE_Transfer.GemAlaramNumber = 65001;
                Loader_ARM_TO_STAGE_Transfer.EnableNotifyMessageDialog = false;
                Loader_ARM_TO_STAGE_Transfer.EnableNotifyEventlog = false;
                Loader_ARM_TO_STAGE_Transfer.EnableNotifyProlog = false;
                Loader_ARM_TO_STAGE_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_ARM_TO_STAGE_Transfer);

                EventCodeParam Loader_ARM_TO_FIXED_Transfer = new EventCodeParam();
                Loader_ARM_TO_FIXED_Transfer.EventCode = EventCodeEnum.LOADER_ARM_TO_FIXED_TRANSFER_ERROR;
                Loader_ARM_TO_FIXED_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_ARM_TO_FIXED_Transfer.Title = "Information Message";
                Loader_ARM_TO_FIXED_Transfer.Message = "Arm To Fixed Transfer Error.";
                Loader_ARM_TO_FIXED_Transfer.GemAlaramNumber = 65002;
                Loader_ARM_TO_FIXED_Transfer.EnableNotifyMessageDialog = false;
                Loader_ARM_TO_FIXED_Transfer.EnableNotifyEventlog = false;
                Loader_ARM_TO_FIXED_Transfer.EnableNotifyProlog = false;
                Loader_ARM_TO_FIXED_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_ARM_TO_FIXED_Transfer);

                EventCodeParam Loader_ARM_TO_INSP_Transfer = new EventCodeParam();
                Loader_ARM_TO_INSP_Transfer.EventCode = EventCodeEnum.LOADER_ARM_TO_INSP_TRANSFER_ERROR;
                Loader_ARM_TO_INSP_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_ARM_TO_INSP_Transfer.Title = "Information Message";
                Loader_ARM_TO_INSP_Transfer.Message = "Arm To InspectionTray Transfer Error.";
                Loader_ARM_TO_INSP_Transfer.GemAlaramNumber = 65003;
                Loader_ARM_TO_INSP_Transfer.EnableNotifyMessageDialog = false;
                Loader_ARM_TO_INSP_Transfer.EnableNotifyEventlog = false;
                Loader_ARM_TO_INSP_Transfer.EnableNotifyProlog = false;
                Loader_ARM_TO_INSP_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_ARM_TO_INSP_Transfer);

                EventCodeParam Loader_ARM_TO_PREALIGN_Transfer = new EventCodeParam();
                Loader_ARM_TO_PREALIGN_Transfer.EventCode = EventCodeEnum.LOADER_ARM_TO_PREALIGN_TRANSFER_ERROR;
                Loader_ARM_TO_PREALIGN_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_ARM_TO_PREALIGN_Transfer.Title = "Information Message";
                Loader_ARM_TO_PREALIGN_Transfer.Message = "Arm To PreAlign Transfer Error.";
                Loader_ARM_TO_PREALIGN_Transfer.GemAlaramNumber = 65004;
                Loader_ARM_TO_PREALIGN_Transfer.EnableNotifyMessageDialog = false;
                Loader_ARM_TO_PREALIGN_Transfer.EnableNotifyEventlog = false;
                Loader_ARM_TO_PREALIGN_Transfer.EnableNotifyProlog = false;
                Loader_ARM_TO_PREALIGN_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_ARM_TO_PREALIGN_Transfer);

                EventCodeParam Loader_ARM_TO_SLOT_Transfer = new EventCodeParam();
                Loader_ARM_TO_SLOT_Transfer.EventCode = EventCodeEnum.LOADER_ARM_TO_SLOT_TRANSFER_ERROR;
                Loader_ARM_TO_SLOT_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_ARM_TO_SLOT_Transfer.Title = "Information Message";
                Loader_ARM_TO_SLOT_Transfer.Message = "Arm To Slot Transfer Error.";
                Loader_ARM_TO_SLOT_Transfer.GemAlaramNumber = 65005;
                Loader_ARM_TO_SLOT_Transfer.EnableNotifyMessageDialog = false;
                Loader_ARM_TO_SLOT_Transfer.EnableNotifyEventlog = false;
                Loader_ARM_TO_SLOT_Transfer.EnableNotifyProlog = false;
                Loader_ARM_TO_SLOT_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_ARM_TO_SLOT_Transfer);

                EventCodeParam Loader_BUFFER_TO_ARM_Transfer = new EventCodeParam();
                Loader_BUFFER_TO_ARM_Transfer.EventCode = EventCodeEnum.LOADER_BUFFER_TO_ARM_TRANSFER_ERROR;
                Loader_BUFFER_TO_ARM_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_BUFFER_TO_ARM_Transfer.Title = "Information Message";
                Loader_BUFFER_TO_ARM_Transfer.Message = "Buffer To Arm Transfer Error.";
                Loader_BUFFER_TO_ARM_Transfer.GemAlaramNumber = 65006;
                Loader_BUFFER_TO_ARM_Transfer.EnableNotifyMessageDialog = false;
                Loader_BUFFER_TO_ARM_Transfer.EnableNotifyEventlog = false;
                Loader_BUFFER_TO_ARM_Transfer.EnableNotifyProlog = false;
                Loader_BUFFER_TO_ARM_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_BUFFER_TO_ARM_Transfer);

                EventCodeParam Loader_CARM_TO_CBUFFER_Transfer = new EventCodeParam();
                Loader_CARM_TO_CBUFFER_Transfer.EventCode = EventCodeEnum.LOADER_CARM_TO_CBUFFER_TRANSFER_ERROR;
                Loader_CARM_TO_CBUFFER_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_CARM_TO_CBUFFER_Transfer.Title = "Information Message";
                Loader_CARM_TO_CBUFFER_Transfer.Message = "Card Arm To Card Buffer Transfer Error.";
                Loader_CARM_TO_CBUFFER_Transfer.GemAlaramNumber = 65007;
                Loader_CARM_TO_CBUFFER_Transfer.EnableNotifyMessageDialog = false;
                Loader_CARM_TO_CBUFFER_Transfer.EnableNotifyEventlog = false;
                Loader_CARM_TO_CBUFFER_Transfer.EnableNotifyProlog = false;
                Loader_CARM_TO_CBUFFER_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_CARM_TO_CBUFFER_Transfer);

                EventCodeParam Loader_CARM_TO_STAGE_Transfer = new EventCodeParam();
                Loader_CARM_TO_STAGE_Transfer.EventCode = EventCodeEnum.LOADER_CARM_TO_STAGE_TRANSFER_ERROR;
                Loader_CARM_TO_STAGE_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_CARM_TO_STAGE_Transfer.Title = "Information Message";
                Loader_CARM_TO_STAGE_Transfer.Message = "Card Arm To Stage Transfer Error.";
                Loader_CARM_TO_STAGE_Transfer.GemAlaramNumber = 65008;
                Loader_CARM_TO_STAGE_Transfer.EnableNotifyMessageDialog = false;
                Loader_CARM_TO_STAGE_Transfer.EnableNotifyEventlog = false;
                Loader_CARM_TO_STAGE_Transfer.EnableNotifyProlog = false;
                Loader_CARM_TO_STAGE_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_CARM_TO_STAGE_Transfer);

                EventCodeParam Loader_CARM_TO_CARDTRAY_Transfer = new EventCodeParam();
                Loader_CARM_TO_CARDTRAY_Transfer.EventCode = EventCodeEnum.LOADER_CARM_TO_CARDTRAY_TRANSFER_ERROR;
                Loader_CARM_TO_CARDTRAY_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_CARM_TO_CARDTRAY_Transfer.Title = "Information Message";
                Loader_CARM_TO_CARDTRAY_Transfer.Message = "Card Arm To CardTray Transfer Error.";
                Loader_CARM_TO_CARDTRAY_Transfer.GemAlaramNumber = 65009;
                Loader_CARM_TO_CARDTRAY_Transfer.EnableNotifyMessageDialog = false;
                Loader_CARM_TO_CARDTRAY_Transfer.EnableNotifyEventlog = false;
                Loader_CARM_TO_CARDTRAY_Transfer.EnableNotifyProlog = false;
                Loader_CARM_TO_CARDTRAY_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_CARM_TO_CARDTRAY_Transfer);

                EventCodeParam Loader_CBUFFER_TO_CARM_Transfer = new EventCodeParam();
                Loader_CBUFFER_TO_CARM_Transfer.EventCode = EventCodeEnum.LOADER_CBUFFER_TO_CARM_TRANSFER_ERROR;
                Loader_CBUFFER_TO_CARM_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_CBUFFER_TO_CARM_Transfer.Title = "Information Message";
                Loader_CBUFFER_TO_CARM_Transfer.Message = "Card Buffer To Card Arm Transfer Error.";
                Loader_CBUFFER_TO_CARM_Transfer.GemAlaramNumber = 65010;
                Loader_CBUFFER_TO_CARM_Transfer.EnableNotifyMessageDialog = false;
                Loader_CBUFFER_TO_CARM_Transfer.EnableNotifyEventlog = false;
                Loader_CBUFFER_TO_CARM_Transfer.EnableNotifyProlog = false;
                Loader_CBUFFER_TO_CARM_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_CBUFFER_TO_CARM_Transfer);

                EventCodeParam Loader_STAGE_TO_CARM_Transfer = new EventCodeParam();
                Loader_STAGE_TO_CARM_Transfer.EventCode = EventCodeEnum.LOADER_STAGE_TO_CARM_TRANSFER_ERROR;
                Loader_STAGE_TO_CARM_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_STAGE_TO_CARM_Transfer.Title = "Information Message";
                Loader_STAGE_TO_CARM_Transfer.Message = "Stage To Card Arm Transfer Error.";
                Loader_STAGE_TO_CARM_Transfer.GemAlaramNumber = 65011;
                Loader_STAGE_TO_CARM_Transfer.EnableNotifyMessageDialog = false;
                Loader_STAGE_TO_CARM_Transfer.EnableNotifyEventlog = false;
                Loader_STAGE_TO_CARM_Transfer.EnableNotifyProlog = false;
                Loader_STAGE_TO_CARM_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_STAGE_TO_CARM_Transfer);

                EventCodeParam Loader_TRAY_TO_CARM_Transfer = new EventCodeParam();
                Loader_TRAY_TO_CARM_Transfer.EventCode = EventCodeEnum.LOADER_TRAY_TO_CARM_TRANSFER_ERROR;
                Loader_TRAY_TO_CARM_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_TRAY_TO_CARM_Transfer.Title = "Information Message";
                Loader_TRAY_TO_CARM_Transfer.Message = "Tray To Card Arm Transfer Error.";
                Loader_TRAY_TO_CARM_Transfer.GemAlaramNumber = 65012;
                Loader_TRAY_TO_CARM_Transfer.EnableNotifyMessageDialog = false;
                Loader_TRAY_TO_CARM_Transfer.EnableNotifyEventlog = false;
                Loader_TRAY_TO_CARM_Transfer.EnableNotifyProlog = false;
                Loader_TRAY_TO_CARM_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_TRAY_TO_CARM_Transfer);

                EventCodeParam Loader_STAGE_TO_ARM_Transfer = new EventCodeParam();
                Loader_STAGE_TO_ARM_Transfer.EventCode = EventCodeEnum.LOADER_STAGE_TO_ARM_TRANSFER_ERROR;
                Loader_STAGE_TO_ARM_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_STAGE_TO_ARM_Transfer.Title = "Information Message";
                Loader_STAGE_TO_ARM_Transfer.Message = "Stage To Arm Transfer Error.";
                Loader_STAGE_TO_ARM_Transfer.GemAlaramNumber = 65013;
                Loader_STAGE_TO_ARM_Transfer.EnableNotifyMessageDialog = false;
                Loader_STAGE_TO_ARM_Transfer.EnableNotifyEventlog = false;
                Loader_STAGE_TO_ARM_Transfer.EnableNotifyProlog = false;
                Loader_STAGE_TO_ARM_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_STAGE_TO_ARM_Transfer);

                EventCodeParam Loader_FIXED_TO_ARM_Transfer = new EventCodeParam();
                Loader_FIXED_TO_ARM_Transfer.EventCode = EventCodeEnum.LOADER_FIXED_TO_ARM_TRANSFER_ERROR;
                Loader_FIXED_TO_ARM_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_FIXED_TO_ARM_Transfer.Title = "Information Message";
                Loader_FIXED_TO_ARM_Transfer.Message = "Fixed To Arm Transfer Error.";
                Loader_FIXED_TO_ARM_Transfer.GemAlaramNumber = 65014;
                Loader_FIXED_TO_ARM_Transfer.EnableNotifyMessageDialog = false;
                Loader_FIXED_TO_ARM_Transfer.EnableNotifyEventlog = false;
                Loader_FIXED_TO_ARM_Transfer.EnableNotifyProlog = false;
                Loader_FIXED_TO_ARM_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_FIXED_TO_ARM_Transfer);

                EventCodeParam Loader_INSP_TO_ARM_Transfer = new EventCodeParam();
                Loader_INSP_TO_ARM_Transfer.EventCode = EventCodeEnum.LOADER_INSP_TO_ARM_TRANSFER_ERROR;
                Loader_INSP_TO_ARM_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_INSP_TO_ARM_Transfer.Title = "Information Message";
                Loader_INSP_TO_ARM_Transfer.Message = "InspectionTray To Arm Transfer Error.";
                Loader_INSP_TO_ARM_Transfer.GemAlaramNumber = 65015;
                Loader_INSP_TO_ARM_Transfer.EnableNotifyMessageDialog = false;
                Loader_INSP_TO_ARM_Transfer.EnableNotifyEventlog = false;
                Loader_INSP_TO_ARM_Transfer.EnableNotifyProlog = false;
                Loader_INSP_TO_ARM_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_INSP_TO_ARM_Transfer);

                EventCodeParam Loader_PREALIGN_TO_ARM_Transfer = new EventCodeParam();
                Loader_PREALIGN_TO_ARM_Transfer.EventCode = EventCodeEnum.LOADER_PREALIGN_TO_ARM_TRANSFER_ERROR;
                Loader_PREALIGN_TO_ARM_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_PREALIGN_TO_ARM_Transfer.Title = "Information Message";
                Loader_PREALIGN_TO_ARM_Transfer.Message = "PreAlign To Arm Transfer Error.";
                Loader_PREALIGN_TO_ARM_Transfer.GemAlaramNumber = 65016;
                Loader_PREALIGN_TO_ARM_Transfer.EnableNotifyMessageDialog = false;
                Loader_PREALIGN_TO_ARM_Transfer.EnableNotifyEventlog = false;
                Loader_PREALIGN_TO_ARM_Transfer.EnableNotifyProlog = false;
                Loader_PREALIGN_TO_ARM_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_PREALIGN_TO_ARM_Transfer);

                EventCodeParam Loader_SLOT_TO_ARM_Transfer = new EventCodeParam();
                Loader_SLOT_TO_ARM_Transfer.EventCode = EventCodeEnum.LOADER_SLOT_TO_ARM_TRANSFER_ERROR;
                Loader_SLOT_TO_ARM_Transfer.ProberErrorKind = EnumProberErrorKind.INVALID;
                Loader_SLOT_TO_ARM_Transfer.Title = "Information Message";
                Loader_SLOT_TO_ARM_Transfer.Message = "Slot To Arm Transfer Error.";
                Loader_SLOT_TO_ARM_Transfer.GemAlaramNumber = 65017;
                Loader_SLOT_TO_ARM_Transfer.EnableNotifyMessageDialog = false;
                Loader_SLOT_TO_ARM_Transfer.EnableNotifyEventlog = false;
                Loader_SLOT_TO_ARM_Transfer.EnableNotifyProlog = false;
                Loader_SLOT_TO_ARM_Transfer.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(Loader_SLOT_TO_ARM_Transfer);

                EventCodeParam LoadPortAccessViolation = new EventCodeParam();
                LoadPortAccessViolation.EventCode = EventCodeEnum.E84_LOAD_PORT_ACCESS_VIOLATION;
                LoadPortAccessViolation.ProberErrorKind = EnumProberErrorKind.INVALID;
                LoadPortAccessViolation.Title = "Warning Message";
                LoadPortAccessViolation.Message = "";
                LoadPortAccessViolation.GemAlaramNumber = 60601;
                LoadPortAccessViolation.EnableNotifyMessageDialog = true;
                LoadPortAccessViolation.EnableNotifyEventlog = false;
                LoadPortAccessViolation.EnableNotifyProlog = false;
                LoadPortAccessViolation.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(LoadPortAccessViolation);

                EventCodeParam visioncamerachangeerror = new EventCodeParam();
                visioncamerachangeerror.EventCode = EventCodeEnum.VISION_CAMERA_CHANGE_ERROR;
                visioncamerachangeerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                visioncamerachangeerror.Title = "";
                visioncamerachangeerror.Message = "";
                visioncamerachangeerror.GemAlaramNumber = 70001;
                visioncamerachangeerror.EnableNotifyMessageDialog = false;
                visioncamerachangeerror.EnableNotifyEventlog = true;
                visioncamerachangeerror.EnableNotifyProlog = false;
                visioncamerachangeerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(visioncamerachangeerror);

                EventCodeParam motionconfigfileloadingfail = new EventCodeParam();
                motionconfigfileloadingfail.EventCode = EventCodeEnum.MOTION_CONFIG_FILE_LOADING_FAIL;
                motionconfigfileloadingfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                motionconfigfileloadingfail.Title = "";
                motionconfigfileloadingfail.Message = "";
                motionconfigfileloadingfail.GemAlaramNumber = 73001;
                motionconfigfileloadingfail.EnableNotifyMessageDialog = false;
                motionconfigfileloadingfail.EnableNotifyEventlog = true;
                motionconfigfileloadingfail.EnableNotifyProlog = false;
                motionconfigfileloadingfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(motionconfigfileloadingfail);

                EventCodeParam motionhomingerror = new EventCodeParam();
                motionhomingerror.EventCode = EventCodeEnum.MOTION_HOMING_ERROR;
                motionhomingerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                motionhomingerror.Title = "";
                motionhomingerror.Message = "";
                motionhomingerror.GemAlaramNumber = 73021;
                motionhomingerror.EnableNotifyMessageDialog = false;
                motionhomingerror.EnableNotifyEventlog = true;
                motionhomingerror.EnableNotifyProlog = false;
                motionhomingerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(motionhomingerror);

                EventCodeParam motionmotiondoneerror = new EventCodeParam();
                motionmotiondoneerror.EventCode = EventCodeEnum.MOTION_MOTIONDONE_ERROR;
                motionmotiondoneerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                motionmotiondoneerror.Title = "";
                motionmotiondoneerror.Message = "";
                motionmotiondoneerror.GemAlaramNumber = 73041;
                motionmotiondoneerror.EnableNotifyMessageDialog = false;
                motionmotiondoneerror.EnableNotifyEventlog = true;
                motionmotiondoneerror.EnableNotifyProlog = false;
                motionmotiondoneerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(motionmotiondoneerror);

                EventCodeParam motionposswlimiterror = new EventCodeParam();
                motionposswlimiterror.EventCode = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                motionposswlimiterror.ProberErrorKind = EnumProberErrorKind.INVALID;
                motionposswlimiterror.Title = "";
                motionposswlimiterror.Message = "";
                motionposswlimiterror.GemAlaramNumber = 73061;
                motionposswlimiterror.EnableNotifyMessageDialog = false;
                motionposswlimiterror.EnableNotifyEventlog = true;
                motionposswlimiterror.EnableNotifyProlog = false;
                motionposswlimiterror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(motionposswlimiterror);

                EventCodeParam motionnegswlimiterror = new EventCodeParam();
                motionnegswlimiterror.EventCode = EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                motionnegswlimiterror.ProberErrorKind = EnumProberErrorKind.INVALID;
                motionnegswlimiterror.Title = "";
                motionnegswlimiterror.Message = "";
                motionnegswlimiterror.GemAlaramNumber = 73061;
                motionnegswlimiterror.EnableNotifyMessageDialog = false;
                motionnegswlimiterror.EnableNotifyEventlog = true;
                motionnegswlimiterror.EnableNotifyProlog = false;
                motionnegswlimiterror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(motionnegswlimiterror);

                EventCodeParam monitoringthreelegerror = new EventCodeParam();
                monitoringthreelegerror.EventCode = EventCodeEnum.MONITORING_THREELEG_ERROR;
                monitoringthreelegerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                monitoringthreelegerror.Title = "";
                monitoringthreelegerror.Message = "";
                monitoringthreelegerror.GemAlaramNumber = 73081;
                monitoringthreelegerror.EnableNotifyMessageDialog = false;
                monitoringthreelegerror.EnableNotifyEventlog = true;
                monitoringthreelegerror.EnableNotifyProlog = false;
                monitoringthreelegerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(monitoringthreelegerror);

                EventCodeParam probingzlimiterror = new EventCodeParam();
                probingzlimiterror.EventCode = EventCodeEnum.PROBING_Z_LIMIT_ERROR;
                probingzlimiterror.ProberErrorKind = EnumProberErrorKind.INVALID;
                probingzlimiterror.Title = "";
                probingzlimiterror.Message = "";
                probingzlimiterror.GemAlaramNumber = 73101;
                probingzlimiterror.EnableNotifyMessageDialog = false;
                probingzlimiterror.EnableNotifyEventlog = true;
                probingzlimiterror.EnableNotifyProlog = false;
                probingzlimiterror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(probingzlimiterror);

                EventCodeParam cardMainVac = new EventCodeParam();
                cardMainVac.EventCode = EventCodeEnum.GP_CardChange_CARD_MAINVAC_ERROR;
                cardMainVac.ProberErrorKind = EnumProberErrorKind.INVALID;
                cardMainVac.Title = "Information Message";
                cardMainVac.Message = "Card vacuum error. Please check pogo vaccum or latch.";
                cardMainVac.GemAlaramNumber = 73141;
                cardMainVac.EnableNotifyMessageDialog = true;
                cardMainVac.EnableNotifyEventlog = false;
                cardMainVac.EnableNotifyProlog = false;
                cardMainVac.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(cardMainVac);

                EventCodeParam testerVac = new EventCodeParam();
                testerVac.EventCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_VAC_OFF_ERROR;
                testerVac.ProberErrorKind = EnumProberErrorKind.INVALID;
                testerVac.Title = "Information Message";
                testerVac.Message = "Tester vacuum error. Please check for tester vacuum status";
                testerVac.GemAlaramNumber = 73161;
                testerVac.EnableNotifyMessageDialog = true;
                testerVac.EnableNotifyEventlog = false;
                testerVac.EnableNotifyProlog = false;
                testerVac.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(testerVac);

                EventCodeParam testerhead_purge_air_err = new EventCodeParam();
                testerhead_purge_air_err.EventCode = EventCodeEnum.TESTERHEAD_PURGE_AIR_ERROR;
                testerhead_purge_air_err.ProberErrorKind = EnumProberErrorKind.INVALID;
                testerhead_purge_air_err.Title = "Error Message";
                testerhead_purge_air_err.Message = "Please check TesterHead Purge Air.";
                testerhead_purge_air_err.GemAlaramNumber = 33001; //h사 내용 공유 해야 함
                testerhead_purge_air_err.EnableNotifyMessageDialog = false;
                testerhead_purge_air_err.EnableNotifyEventlog = true;
                testerhead_purge_air_err.EnableNotifyProlog = false;
                testerhead_purge_air_err.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(testerhead_purge_air_err);

                if( SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    EventCodeParam chillernotconnected = new EventCodeParam();
                    chillernotconnected.EventCode = EventCodeEnum.CHILLER_REMOTE_NOT_CONNECTED;
                    chillernotconnected.ProberErrorKind = EnumProberErrorKind.INVALID;
                    chillernotconnected.Title = "Warning Message";
                    chillernotconnected.Message = $"ERROR CODE : { chillernotconnected.EventCode}.\rChiller is not connected. Check the chiller connection staus.";
                    chillernotconnected.GemAlaramNumber = 27001;
                    chillernotconnected.EnableNotifyMessageDialog = true;
                    chillernotconnected.EnableNotifyEventlog = true;
                    chillernotconnected.EnableNotifyProlog = false;
                    chillernotconnected.ProLogType = PrologType.UNDEFINED;
                    NoticeEventCodeParam?.Add(chillernotconnected);
                }
                else
                {
                    EventCodeParam chillernotconnected = new EventCodeParam();
                    chillernotconnected.EventCode = EventCodeEnum.CHILLER_NOT_CONNECTED;
                    chillernotconnected.ProberErrorKind = EnumProberErrorKind.INVALID;
                    chillernotconnected.Title = "Warning Message";
                    chillernotconnected.Message = $"ERROR CODE : { chillernotconnected.EventCode}.\rChiller is not connected. Check the chiller connection staus.";
                    chillernotconnected.GemAlaramNumber = 27061;
                    chillernotconnected.EnableNotifyMessageDialog = true;
                    chillernotconnected.EnableNotifyEventlog = true;
                    chillernotconnected.EnableNotifyProlog = false;
                    chillernotconnected.ProLogType = PrologType.UNDEFINED;
                    NoticeEventCodeParam?.Add(chillernotconnected);
                }

                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    EventCodeParam chillererroroccur = new EventCodeParam();
                    chillererroroccur.EventCode = EventCodeEnum.CHILLER_ERROR_OCCURRED;
                    chillererroroccur.ProberErrorKind = EnumProberErrorKind.INVALID;
                    chillererroroccur.Title = "";
                    chillererroroccur.Message = "";
                    chillererroroccur.GemAlaramNumber = 27021;
                    chillererroroccur.EnableNotifyMessageDialog = false;
                    chillererroroccur.EnableNotifyEventlog = true;
                    chillererroroccur.EnableNotifyProlog = false;
                    chillererroroccur.ProLogType = PrologType.UNDEFINED;
                    NoticeEventCodeParam?.Add(chillererroroccur);
                }
                else
                {
                    EventCodeParam chillererroroccur = new EventCodeParam();
                    chillererroroccur.EventCode = EventCodeEnum.CHILLER_ERROR_OCCURRED;
                    chillererroroccur.ProberErrorKind = EnumProberErrorKind.INVALID;
                    chillererroroccur.Title = "";
                    chillererroroccur.Message = "";
                    chillererroroccur.GemAlaramNumber = -1;
                    chillererroroccur.EnableNotifyMessageDialog = false;
                    chillererroroccur.EnableNotifyEventlog = true;
                    chillererroroccur.EnableNotifyProlog = false;
                    chillererroroccur.ProLogType = PrologType.UNDEFINED;
                    NoticeEventCodeParam?.Add(chillererroroccur);
                }
                #endregion

                #region <!-- ALCD 6 -->

                EventCodeParam cardcontactlimit = new EventCodeParam();
                cardcontactlimit.EventCode = EventCodeEnum.CARD_CONTACT_LIMIT;
                cardcontactlimit.ProberErrorKind = EnumProberErrorKind.INVALID;
                cardcontactlimit.Title = "";
                cardcontactlimit.Message = "";
                cardcontactlimit.GemAlaramNumber = 1031;
                cardcontactlimit.EnableNotifyMessageDialog = false;
                cardcontactlimit.EnableNotifyEventlog = true;
                cardcontactlimit.EnableNotifyProlog = false;
                cardcontactlimit.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(cardcontactlimit);


                EventCodeParam stgaeerroroccur = new EventCodeParam();
                stgaeerroroccur.EventCode = EventCodeEnum.STAGE_ERROR_OCCUR;
                stgaeerroroccur.ProberErrorKind = EnumProberErrorKind.INVALID;
                stgaeerroroccur.Title = "";
                stgaeerroroccur.Message = "";
                stgaeerroroccur.GemAlaramNumber = 1201;
                stgaeerroroccur.EnableNotifyMessageDialog = false;
                stgaeerroroccur.EnableNotifyEventlog = true;
                stgaeerroroccur.EnableNotifyProlog = false;
                stgaeerroroccur.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(stgaeerroroccur);


                EventCodeParam pmifail = new EventCodeParam();
                pmifail.EventCode = EventCodeEnum.PMI_FAIL;
                pmifail.ProberErrorKind = EnumProberErrorKind.INVALID;
                pmifail.Title = "";
                pmifail.Message = "";
                pmifail.GemAlaramNumber = 8001;
                pmifail.EnableNotifyMessageDialog = false;
                pmifail.EnableNotifyEventlog = true;
                pmifail.EnableNotifyProlog = false;
                pmifail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pmifail);

                EventCodeParam devicechangefail = new EventCodeParam();
                devicechangefail.EventCode = EventCodeEnum.DEVICE_CHANGE_FAIL;
                devicechangefail.ProberErrorKind = EnumProberErrorKind.INVALID;
                devicechangefail.Title = "";
                devicechangefail.Message = "";
                devicechangefail.GemAlaramNumber = 9001;
                devicechangefail.EnableNotifyMessageDialog = false;
                devicechangefail.EnableNotifyEventlog = true;
                devicechangefail.EnableNotifyProlog = false;
                devicechangefail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(devicechangefail);

                EventCodeParam waferedgenotfound = new EventCodeParam();
                waferedgenotfound.EventCode = EventCodeEnum.WAFER_EDGE_NOT_FOUND;
                waferedgenotfound.ProberErrorKind = EnumProberErrorKind.INVALID;
                waferedgenotfound.Title = "";
                waferedgenotfound.Message = "";
                waferedgenotfound.GemAlaramNumber = 14101;
                waferedgenotfound.EnableNotifyMessageDialog = false;
                waferedgenotfound.EnableNotifyEventlog = true;
                waferedgenotfound.EnableNotifyProlog = false;
                waferedgenotfound.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(waferedgenotfound);

                EventCodeParam waferindexalignfail = new EventCodeParam();
                waferindexalignfail.EventCode = EventCodeEnum.WAFER_INDEX_ALIGN_FAIL;
                waferindexalignfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                waferindexalignfail.Title = "";
                waferindexalignfail.Message = "";
                waferindexalignfail.GemAlaramNumber = 14151;
                waferindexalignfail.EnableNotifyMessageDialog = false;
                waferindexalignfail.EnableNotifyEventlog = true;
                waferindexalignfail.EnableNotifyProlog = false;
                waferindexalignfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(waferindexalignfail);

                EventCodeParam waferlowpatternnotfound = new EventCodeParam();
                waferlowpatternnotfound.EventCode = EventCodeEnum.WAFER_LOW_PATTERN_NOT_FOUND;
                waferlowpatternnotfound.ProberErrorKind = EnumProberErrorKind.INVALID;
                waferlowpatternnotfound.Title = "";
                waferlowpatternnotfound.Message = "";
                waferlowpatternnotfound.GemAlaramNumber = 14201;
                waferlowpatternnotfound.EnableNotifyMessageDialog = false;
                waferlowpatternnotfound.EnableNotifyEventlog = true;
                waferlowpatternnotfound.EnableNotifyProlog = false;
                waferlowpatternnotfound.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(waferlowpatternnotfound);

                EventCodeParam waferhighpatternnotfound = new EventCodeParam();
                waferhighpatternnotfound.EventCode = EventCodeEnum.WAFER_HIGH_PATTERN_NOT_FOUND;
                waferhighpatternnotfound.ProberErrorKind = EnumProberErrorKind.INVALID;
                waferhighpatternnotfound.Title = "";
                waferhighpatternnotfound.Message = "";
                waferhighpatternnotfound.GemAlaramNumber = 14201;
                waferhighpatternnotfound.EnableNotifyMessageDialog = false;
                waferhighpatternnotfound.EnableNotifyEventlog = true;
                waferhighpatternnotfound.EnableNotifyProlog = false;
                waferhighpatternnotfound.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(waferhighpatternnotfound);

                EventCodeParam waferinexalignpatternnotfound = new EventCodeParam();
                waferinexalignpatternnotfound.EventCode = EventCodeEnum.WAFER_INDEX_ALIGN_PATTERN_NOT_FOUND;
                waferinexalignpatternnotfound.ProberErrorKind = EnumProberErrorKind.INVALID;
                waferinexalignpatternnotfound.Title = "";
                waferinexalignpatternnotfound.Message = "";
                waferinexalignpatternnotfound.GemAlaramNumber = 14201;
                waferinexalignpatternnotfound.EnableNotifyMessageDialog = false;
                waferinexalignpatternnotfound.EnableNotifyEventlog = true;
                waferinexalignpatternnotfound.EnableNotifyProlog = false;
                waferinexalignpatternnotfound.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(waferinexalignpatternnotfound);

                EventCodeParam waferlowfocusingfail = new EventCodeParam();
                waferlowfocusingfail.EventCode = EventCodeEnum.WAFER_LOW_FOCUSING_FAIL;
                waferlowfocusingfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                waferlowfocusingfail.Title = "";
                waferlowfocusingfail.Message = "";
                waferlowfocusingfail.GemAlaramNumber = 14251;
                waferlowfocusingfail.EnableNotifyMessageDialog = false;
                waferlowfocusingfail.EnableNotifyEventlog = true;
                waferlowfocusingfail.EnableNotifyProlog = false;
                waferlowfocusingfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(waferlowfocusingfail);

                EventCodeParam waferhighfocusingfail = new EventCodeParam();
                waferhighfocusingfail.EventCode = EventCodeEnum.WAFER_HIGH_FOCUSING_FAIL;
                waferhighfocusingfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                waferhighfocusingfail.Title = "";
                waferhighfocusingfail.Message = "";
                waferhighfocusingfail.GemAlaramNumber = 14251;
                waferhighfocusingfail.EnableNotifyMessageDialog = false;
                waferhighfocusingfail.EnableNotifyEventlog = true;
                waferhighfocusingfail.EnableNotifyProlog = false;
                waferhighfocusingfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(waferhighfocusingfail);

                EventCodeParam verifyalignoverflowlimit = new EventCodeParam();
                verifyalignoverflowlimit.EventCode = EventCodeEnum.VERIFYALIGN_OVERFLOW_LIMIT;
                verifyalignoverflowlimit.ProberErrorKind = EnumProberErrorKind.INVALID;
                verifyalignoverflowlimit.Title = "";
                verifyalignoverflowlimit.Message = "";
                verifyalignoverflowlimit.GemAlaramNumber = 14301;
                verifyalignoverflowlimit.EnableNotifyMessageDialog = false;
                verifyalignoverflowlimit.EnableNotifyEventlog = true;
                verifyalignoverflowlimit.EnableNotifyProlog = false;
                verifyalignoverflowlimit.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(verifyalignoverflowlimit);

                EventCodeParam waferalignthetacompensationfail = new EventCodeParam();
                waferalignthetacompensationfail.EventCode = EventCodeEnum.WAFER_ALIGN_THETA_COMPENSATION_FAIL;
                waferalignthetacompensationfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                waferalignthetacompensationfail.Title = "";
                waferalignthetacompensationfail.Message = "";
                waferalignthetacompensationfail.GemAlaramNumber = 14351;
                waferalignthetacompensationfail.EnableNotifyMessageDialog = false;
                waferalignthetacompensationfail.EnableNotifyEventlog = true;
                waferalignthetacompensationfail.EnableNotifyProlog = false;
                waferalignthetacompensationfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(waferalignthetacompensationfail);

                EventCodeParam pinteachfailed = new EventCodeParam();
                pinteachfailed.EventCode = EventCodeEnum.PIN_TEACH_FAILED;
                pinteachfailed.ProberErrorKind = EnumProberErrorKind.INVALID;
                pinteachfailed.Title = "";
                pinteachfailed.Message = "";
                pinteachfailed.GemAlaramNumber = 15101;
                pinteachfailed.EnableNotifyMessageDialog = false;
                pinteachfailed.EnableNotifyEventlog = true;
                pinteachfailed.EnableNotifyProlog = false;
                pinteachfailed.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pinteachfailed);

                EventCodeParam pinpadmatchfail = new EventCodeParam();
                pinpadmatchfail.EventCode = EventCodeEnum.PIN_PAD_MATCH_FAIL;
                pinpadmatchfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                pinpadmatchfail.Title = "";
                pinpadmatchfail.Message = "";
                pinpadmatchfail.GemAlaramNumber = 15151;
                pinpadmatchfail.EnableNotifyMessageDialog = false;
                pinpadmatchfail.EnableNotifyEventlog = true;
                pinpadmatchfail.EnableNotifyProlog = false;
                pinpadmatchfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pinpadmatchfail);

                EventCodeParam pinalignverifyerror = new EventCodeParam();
                pinalignverifyerror.EventCode = EventCodeEnum.PIN_ALIGN_VERIFY_ERROR;
                pinalignverifyerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                pinalignverifyerror.Title = "";
                pinalignverifyerror.Message = "";
                pinalignverifyerror.GemAlaramNumber = 15201;
                pinalignverifyerror.EnableNotifyMessageDialog = false;
                pinalignverifyerror.EnableNotifyEventlog = true;
                pinalignverifyerror.EnableNotifyProlog = false;
                pinalignverifyerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pinalignverifyerror);

                EventCodeParam pinfocusfailed = new EventCodeParam();
                pinfocusfailed.EventCode = EventCodeEnum.PIN_FOCUS_FAILED;
                pinfocusfailed.ProberErrorKind = EnumProberErrorKind.INVALID;
                pinfocusfailed.Title = "";
                pinfocusfailed.Message = "";
                pinfocusfailed.GemAlaramNumber = 15251;
                pinfocusfailed.EnableNotifyMessageDialog = false;
                pinfocusfailed.EnableNotifyEventlog = true;
                pinfocusfailed.EnableNotifyProlog = false;
                pinfocusfailed.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pinfocusfailed);

                EventCodeParam pinlowpaternfailed = new EventCodeParam();
                pinlowpaternfailed.EventCode = EventCodeEnum.PIN_LOW_PATTERN_FAILED;
                pinlowpaternfailed.ProberErrorKind = EnumProberErrorKind.INVALID;
                pinlowpaternfailed.Title = "";
                pinlowpaternfailed.Message = "";
                pinlowpaternfailed.GemAlaramNumber = 15301;
                pinlowpaternfailed.EnableNotifyMessageDialog = false;
                pinlowpaternfailed.EnableNotifyEventlog = true;
                pinlowpaternfailed.EnableNotifyProlog = false;
                pinlowpaternfailed.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pinlowpaternfailed);

                EventCodeParam pinnotenough = new EventCodeParam();
                pinnotenough.EventCode = EventCodeEnum.PIN_NOT_ENOUGH;
                pinnotenough.ProberErrorKind = EnumProberErrorKind.INVALID;
                pinnotenough.Title = "";
                pinnotenough.Message = "";
                pinnotenough.GemAlaramNumber = 15401;
                pinnotenough.EnableNotifyMessageDialog = false;
                pinnotenough.EnableNotifyEventlog = true;
                pinnotenough.EnableNotifyProlog = false;
                pinnotenough.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pinnotenough);

                EventCodeParam pinfindcenterfail = new EventCodeParam();
                pinfindcenterfail.EventCode = EventCodeEnum.PIN_FIND_CENTER_FAIL;
                pinfindcenterfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                pinfindcenterfail.Title = "";
                pinfindcenterfail.Message = "";
                pinfindcenterfail.GemAlaramNumber = 15451;
                pinfindcenterfail.EnableNotifyMessageDialog = false;
                pinfindcenterfail.EnableNotifyEventlog = true;
                pinfindcenterfail.EnableNotifyProlog = false;
                pinfindcenterfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pinfindcenterfail);

                EventCodeParam pinexeedpostolerance = new EventCodeParam();
                pinexeedpostolerance.EventCode = EventCodeEnum.PIN_EXEED_POS_TOLERANCE;
                pinexeedpostolerance.ProberErrorKind = EnumProberErrorKind.INVALID;
                pinexeedpostolerance.Title = "";
                pinexeedpostolerance.Message = "";
                pinexeedpostolerance.GemAlaramNumber = 15501;
                pinexeedpostolerance.EnableNotifyMessageDialog = false;
                pinexeedpostolerance.EnableNotifyEventlog = true;
                pinexeedpostolerance.EnableNotifyProlog = false;
                pinexeedpostolerance.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pinexeedpostolerance);

                EventCodeParam pinexeedminmaxtolerance = new EventCodeParam();
                pinexeedminmaxtolerance.EventCode = EventCodeEnum.PIN_EXEED_MINMAX_TOLERANCE;
                pinexeedminmaxtolerance.ProberErrorKind = EnumProberErrorKind.INVALID;
                pinexeedminmaxtolerance.Title = "";
                pinexeedminmaxtolerance.Message = "";
                pinexeedminmaxtolerance.GemAlaramNumber = 15551;
                pinexeedminmaxtolerance.EnableNotifyMessageDialog = false;
                pinexeedminmaxtolerance.EnableNotifyEventlog = true;
                pinexeedminmaxtolerance.EnableNotifyProlog = false;
                pinexeedminmaxtolerance.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pinexeedminmaxtolerance);

                EventCodeParam pincancelpinalignment = new EventCodeParam();
                pincancelpinalignment.EventCode = EventCodeEnum.PIN_CANCEL_PINALIGNMENT;
                pincancelpinalignment.ProberErrorKind = EnumProberErrorKind.INVALID;
                pincancelpinalignment.Title = "";
                pincancelpinalignment.Message = "";
                pincancelpinalignment.GemAlaramNumber = 15601;
                pincancelpinalignment.EnableNotifyMessageDialog = false;
                pincancelpinalignment.EnableNotifyEventlog = true;
                pincancelpinalignment.EnableNotifyProlog = false;
                pincancelpinalignment.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pincancelpinalignment);

                EventCodeParam pinfailpercenttolerance = new EventCodeParam();
                pinfailpercenttolerance.EventCode = EventCodeEnum.PIN_FAIL_PERCENT_TOLERANCE;
                pinfailpercenttolerance.ProberErrorKind = EnumProberErrorKind.INVALID;
                pinfailpercenttolerance.Title = "";
                pinfailpercenttolerance.Message = "";
                pinfailpercenttolerance.GemAlaramNumber = 15651;
                pinfailpercenttolerance.EnableNotifyMessageDialog = false;
                pinfailpercenttolerance.EnableNotifyEventlog = true;
                pinfailpercenttolerance.EnableNotifyProlog = false;
                pinfailpercenttolerance.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pinfailpercenttolerance);

                EventCodeParam pingroupfail = new EventCodeParam();
                pingroupfail.EventCode = EventCodeEnum.PIN_GROUP_FAIL;
                pingroupfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                pingroupfail.Title = "";
                pingroupfail.Message = "";
                pingroupfail.GemAlaramNumber = 15701;
                pingroupfail.EnableNotifyMessageDialog = false;
                pingroupfail.EnableNotifyEventlog = true;
                pingroupfail.EnableNotifyProlog = false;
                pingroupfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pingroupfail);

                EventCodeParam pincenterpositionexceedtolerance = new EventCodeParam();
                pincenterpositionexceedtolerance.EventCode = EventCodeEnum.PIN_CENTER_POSITION_EXCEED_TOLERANCE;
                pincenterpositionexceedtolerance.ProberErrorKind = EnumProberErrorKind.INVALID;
                pincenterpositionexceedtolerance.Title = "";
                pincenterpositionexceedtolerance.Message = "";
                pincenterpositionexceedtolerance.GemAlaramNumber = 15751;
                pincenterpositionexceedtolerance.EnableNotifyMessageDialog = false;
                pincenterpositionexceedtolerance.EnableNotifyEventlog = true;
                pincenterpositionexceedtolerance.EnableNotifyProlog = false;
                pincenterpositionexceedtolerance.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pincenterpositionexceedtolerance);

                EventCodeParam pinblobposexceedtol = new EventCodeParam();
                pinblobposexceedtol.EventCode = EventCodeEnum.PIN_BLOB_POSITION_EXCEED_TOLERANC;
                pinblobposexceedtol.ProberErrorKind = EnumProberErrorKind.INVALID;
                pinblobposexceedtol.Title = "";
                pinblobposexceedtol.Message = "";
                pinblobposexceedtol.GemAlaramNumber = 15801;
                pinblobposexceedtol.EnableNotifyMessageDialog = false;
                pinblobposexceedtol.EnableNotifyEventlog = true;
                pinblobposexceedtol.EnableNotifyProlog = false;
                pinblobposexceedtol.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pinblobposexceedtol);

                EventCodeParam pintipsizevalidation = new EventCodeParam();
                pintipsizevalidation.EventCode = EventCodeEnum.PIN_TIP_SIZE_VALIDATION_FAIL;
                pintipsizevalidation.ProberErrorKind = EnumProberErrorKind.INVALID;
                pintipsizevalidation.Title = "";
                pintipsizevalidation.Message = "";
                pintipsizevalidation.GemAlaramNumber = 15851;
                pintipsizevalidation.EnableNotifyMessageDialog = false;
                pintipsizevalidation.EnableNotifyEventlog = true;
                pintipsizevalidation.EnableNotifyProlog = false;
                pintipsizevalidation.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pintipsizevalidation);

                EventCodeParam foupscannotdetect = new EventCodeParam();
                foupscannotdetect.EventCode = EventCodeEnum.FOUP_SCAN_NOTDETECT;
                foupscannotdetect.ProberErrorKind = EnumProberErrorKind.INVALID;
                foupscannotdetect.Title = "";
                foupscannotdetect.Message = "";
                foupscannotdetect.GemAlaramNumber = 60101;
                foupscannotdetect.EnableNotifyMessageDialog = false;
                foupscannotdetect.EnableNotifyEventlog = true;
                foupscannotdetect.EnableNotifyProlog = false;
                foupscannotdetect.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(foupscannotdetect);

                EventCodeParam cassettenotready = new EventCodeParam();
                cassettenotready.EventCode = EventCodeEnum.CASSETTE_NOT_READY;
                cassettenotready.ProberErrorKind = EnumProberErrorKind.INVALID;
                cassettenotready.Title = "";
                cassettenotready.Message = "";
                cassettenotready.GemAlaramNumber = 60111;
                cassettenotready.EnableNotifyMessageDialog = false;
                cassettenotready.EnableNotifyEventlog = true;
                cassettenotready.EnableNotifyProlog = false;
                cassettenotready.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(cassettenotready);

                EventCodeParam ocrreadfail = new EventCodeParam();
                ocrreadfail.EventCode = EventCodeEnum.OCR_READ_FAIL;
                ocrreadfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                ocrreadfail.Title = "";
                ocrreadfail.Message = "";
                ocrreadfail.GemAlaramNumber = 60141;
                ocrreadfail.EnableNotifyMessageDialog = false;
                ocrreadfail.EnableNotifyEventlog = true;
                ocrreadfail.EnableNotifyProlog = false;
                ocrreadfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(ocrreadfail);

                EventCodeParam ocrchecksumfail = new EventCodeParam();
                ocrchecksumfail.EventCode = EventCodeEnum.OCR_CHECKSUM_FAIL;
                ocrchecksumfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                ocrchecksumfail.Title = "";
                ocrchecksumfail.Message = "";
                ocrchecksumfail.GemAlaramNumber = 60151;
                ocrchecksumfail.EnableNotifyMessageDialog = false;
                ocrchecksumfail.EnableNotifyEventlog = true;
                ocrchecksumfail.EnableNotifyProlog = false;
                ocrchecksumfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(ocrchecksumfail);

                EventCodeParam ocrretryfail = new EventCodeParam();
                ocrretryfail.EventCode = EventCodeEnum.OCR_RETRY_FAIL;
                ocrretryfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                ocrretryfail.Title = "";
                ocrretryfail.Message = "";
                ocrretryfail.GemAlaramNumber = 60161;
                ocrretryfail.EnableNotifyMessageDialog = false;
                ocrretryfail.EnableNotifyEventlog = true;
                ocrretryfail.EnableNotifyProlog = false;
                ocrretryfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(ocrretryfail);

                EventCodeParam ocrautocalibrationfail = new EventCodeParam();
                ocrautocalibrationfail.EventCode = EventCodeEnum.OCR_AUTO_CALIBRATION_FAIL;
                ocrautocalibrationfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                ocrautocalibrationfail.Title = "";
                ocrautocalibrationfail.Message = "";
                ocrautocalibrationfail.GemAlaramNumber = 60171;
                ocrautocalibrationfail.EnableNotifyMessageDialog = false;
                ocrautocalibrationfail.EnableNotifyEventlog = true;
                ocrautocalibrationfail.EnableNotifyProlog = false;
                ocrautocalibrationfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(ocrautocalibrationfail);

                EventCodeParam ocrreadfailmanual = new EventCodeParam();
                ocrreadfailmanual.EventCode = EventCodeEnum.OCR_READ_FAIL_MANUAL;
                ocrreadfailmanual.ProberErrorKind = EnumProberErrorKind.INVALID;
                ocrreadfailmanual.Title = "";
                ocrreadfailmanual.Message = "";
                ocrreadfailmanual.GemAlaramNumber = 60181;
                ocrreadfailmanual.EnableNotifyMessageDialog = false;
                ocrreadfailmanual.EnableNotifyEventlog = true;
                ocrreadfailmanual.EnableNotifyProlog = false;
                ocrreadfailmanual.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(ocrreadfailmanual);

                EventCodeParam ocrresultalreadyprobedwaferid = new EventCodeParam();
                ocrresultalreadyprobedwaferid.EventCode = EventCodeEnum.OCR_RESULT_ALREADY_PROBED_WAFERID;
                ocrresultalreadyprobedwaferid.ProberErrorKind = EnumProberErrorKind.INVALID;
                ocrresultalreadyprobedwaferid.Title = "";
                ocrresultalreadyprobedwaferid.Message = "";
                ocrresultalreadyprobedwaferid.GemAlaramNumber = 60191;
                ocrresultalreadyprobedwaferid.EnableNotifyMessageDialog = false;
                ocrresultalreadyprobedwaferid.EnableNotifyEventlog = true;
                ocrresultalreadyprobedwaferid.EnableNotifyProlog = false;
                ocrresultalreadyprobedwaferid.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(ocrresultalreadyprobedwaferid);

                EventCodeParam soakingerror = new EventCodeParam();
                soakingerror.EventCode = EventCodeEnum.SOAKING_ERROR;
                soakingerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                soakingerror.Title = "";
                soakingerror.Message = "";
                soakingerror.GemAlaramNumber = 17001;
                soakingerror.EnableNotifyMessageDialog = false;
                soakingerror.EnableNotifyEventlog = true;
                soakingerror.EnableNotifyProlog = false;
                soakingerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(soakingerror);

                EventCodeParam polishwafercannotperformed = new EventCodeParam();
                polishwafercannotperformed.EventCode = EventCodeEnum.POLISHWAFER_CAN_NOT_PERFORMED;
                polishwafercannotperformed.ProberErrorKind = EnumProberErrorKind.INVALID;
                polishwafercannotperformed.Title = "";
                polishwafercannotperformed.Message = "";
                polishwafercannotperformed.GemAlaramNumber = 19001;
                polishwafercannotperformed.EnableNotifyMessageDialog = false;
                polishwafercannotperformed.EnableNotifyEventlog = true;
                polishwafercannotperformed.EnableNotifyProlog = false;
                polishwafercannotperformed.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(polishwafercannotperformed);

                EventCodeParam polishwaferfocusingerror = new EventCodeParam();
                polishwaferfocusingerror.EventCode = EventCodeEnum.POLISHWAFER_FOCUSING_ERROR;
                polishwaferfocusingerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                polishwaferfocusingerror.Title = "";
                polishwaferfocusingerror.Message = "";
                polishwaferfocusingerror.GemAlaramNumber = 19051;
                polishwaferfocusingerror.EnableNotifyMessageDialog = false;
                polishwaferfocusingerror.EnableNotifyEventlog = true;
                polishwaferfocusingerror.EnableNotifyProlog = false;
                polishwaferfocusingerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(polishwaferfocusingerror);

                EventCodeParam polishwafergetnextposfail = new EventCodeParam();
                polishwafergetnextposfail.EventCode = EventCodeEnum.POLISHWAFER_GET_NEXT_POSITION_FAIL;
                polishwafergetnextposfail.ProberErrorKind = EnumProberErrorKind.INVALID;
                polishwafergetnextposfail.Title = "";
                polishwafergetnextposfail.Message = "";
                polishwafergetnextposfail.GemAlaramNumber = 19101;
                polishwafergetnextposfail.EnableNotifyMessageDialog = false;
                polishwafergetnextposfail.EnableNotifyEventlog = true;
                polishwafergetnextposfail.EnableNotifyProlog = false;
                polishwafergetnextposfail.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(polishwafergetnextposfail);

                EventCodeParam polishwafercleaningerror = new EventCodeParam();
                polishwafercleaningerror.EventCode = EventCodeEnum.POLISHWAFER_CLEAING_ERROR;
                polishwafercleaningerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                polishwafercleaningerror.Title = "";
                polishwafercleaningerror.Message = "";
                polishwafercleaningerror.GemAlaramNumber = 19151;
                polishwafercleaningerror.EnableNotifyMessageDialog = false;
                polishwafercleaningerror.EnableNotifyEventlog = true;
                polishwafercleaningerror.EnableNotifyProlog = false;
                polishwafercleaningerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(polishwafercleaningerror);

                EventCodeParam polishwafercenteringerror = new EventCodeParam();
                polishwafercenteringerror.EventCode = EventCodeEnum.POLISHWAFER_CENTERING_ERROR;
                polishwafercenteringerror.ProberErrorKind = EnumProberErrorKind.INVALID;
                polishwafercenteringerror.Title = "";
                polishwafercenteringerror.Message = "";
                polishwafercenteringerror.GemAlaramNumber = 19201;
                polishwafercenteringerror.EnableNotifyMessageDialog = false;
                polishwafercenteringerror.EnableNotifyEventlog = true;
                polishwafercenteringerror.EnableNotifyProlog = false;
                polishwafercenteringerror.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(polishwafercenteringerror);

                EventCodeParam temperatureoutrangehot = new EventCodeParam();
                temperatureoutrangehot.EventCode = EventCodeEnum.TEMPERATURE_OUT_RANGE_HOT;
                temperatureoutrangehot.ProberErrorKind = EnumProberErrorKind.INVALID;
                temperatureoutrangehot.Title = "";
                temperatureoutrangehot.Message = "";
                temperatureoutrangehot.GemAlaramNumber = 23151;
                temperatureoutrangehot.EnableNotifyMessageDialog = false;
                temperatureoutrangehot.EnableNotifyEventlog = true;
                temperatureoutrangehot.EnableNotifyProlog = false;
                temperatureoutrangehot.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(temperatureoutrangehot);

                EventCodeParam temperatureinrangehot = new EventCodeParam();
                temperatureinrangehot.EventCode = EventCodeEnum.TEMPERATURE_IN_RANGE_HOT;
                temperatureinrangehot.ProberErrorKind = EnumProberErrorKind.INVALID;
                temperatureinrangehot.Title = "";
                temperatureinrangehot.Message = "";
                temperatureinrangehot.GemAlaramNumber = 23201;
                temperatureinrangehot.EnableNotifyMessageDialog = false;
                temperatureinrangehot.EnableNotifyEventlog = true;
                temperatureinrangehot.EnableNotifyProlog = false;
                temperatureinrangehot.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(temperatureinrangehot);

                EventCodeParam tempoutrangecold = new EventCodeParam();
                tempoutrangecold.EventCode = EventCodeEnum.TEMPERATURE_OUT_RANGE_COLD;
                tempoutrangecold.ProberErrorKind = EnumProberErrorKind.INVALID;
                tempoutrangecold.Title = "";
                tempoutrangecold.Message = "";
                tempoutrangecold.GemAlaramNumber = 23251;
                tempoutrangecold.EnableNotifyMessageDialog = false;
                tempoutrangecold.EnableNotifyEventlog = true;
                tempoutrangecold.EnableNotifyProlog = false;
                tempoutrangecold.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(tempoutrangecold);

                EventCodeParam tempinrangecold = new EventCodeParam();
                tempinrangecold.EventCode = EventCodeEnum.TEMPERATURE_IN_RANGE_COLD;
                tempinrangecold.ProberErrorKind = EnumProberErrorKind.INVALID;
                tempinrangecold.Title = "";
                tempinrangecold.Message = "";
                tempinrangecold.GemAlaramNumber = 23301;
                tempinrangecold.EnableNotifyMessageDialog = false;
                tempinrangecold.EnableNotifyEventlog = true;
                tempinrangecold.EnableNotifyProlog = false;
                tempinrangecold.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(tempinrangecold);

                EventCodeParam loadererroroccur = new EventCodeParam();
                loadererroroccur.EventCode = EventCodeEnum.LOADER_ERROR_OCCUR;
                loadererroroccur.ProberErrorKind = EnumProberErrorKind.INVALID;
                loadererroroccur.Title = "";
                loadererroroccur.Message = "";
                loadererroroccur.GemAlaramNumber = 50017;
                loadererroroccur.EnableNotifyMessageDialog = false;
                loadererroroccur.EnableNotifyEventlog = true;
                loadererroroccur.EnableNotifyProlog = false;
                loadererroroccur.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(loadererroroccur);

                EventCodeParam z_torque_tol_err = new EventCodeParam();
                z_torque_tol_err.EventCode = EventCodeEnum.MOTION_THREE_POD_LOAD_UNBALANCE_ERROR;
                z_torque_tol_err.ProberErrorKind = EnumProberErrorKind.INVALID;
                z_torque_tol_err.Title = "";
                z_torque_tol_err.Message = "";
                z_torque_tol_err.GemAlaramNumber = 73181;
                z_torque_tol_err.EnableNotifyMessageDialog = false;
                z_torque_tol_err.EnableNotifyEventlog = true;
                z_torque_tol_err.EnableNotifyProlog = false;
                z_torque_tol_err.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(z_torque_tol_err);
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    EventCodeParam chillerwarningoccur = new EventCodeParam();
                    chillerwarningoccur.EventCode = EventCodeEnum.CHILLER_WARNING_OCCURRED;
                    chillerwarningoccur.ProberErrorKind = EnumProberErrorKind.INVALID;
                    chillerwarningoccur.Title = "";
                    chillerwarningoccur.Message = "";
                    chillerwarningoccur.GemAlaramNumber = 27041;
                    chillerwarningoccur.EnableNotifyMessageDialog = false;
                    chillerwarningoccur.EnableNotifyEventlog = true;
                    chillerwarningoccur.EnableNotifyProlog = false;
                    chillerwarningoccur.ProLogType = PrologType.UNDEFINED;
                    NoticeEventCodeParam?.Add(chillerwarningoccur);
                }
                else
                {
                    EventCodeParam chillerwarningoccur = new EventCodeParam();
                    chillerwarningoccur.EventCode = EventCodeEnum.CHILLER_WARNING_OCCURRED;
                    chillerwarningoccur.ProberErrorKind = EnumProberErrorKind.INVALID;
                    chillerwarningoccur.Title = "";
                    chillerwarningoccur.Message = "";
                    chillerwarningoccur.GemAlaramNumber = -1;
                    chillerwarningoccur.EnableNotifyMessageDialog = false;
                    chillerwarningoccur.EnableNotifyEventlog = true;
                    chillerwarningoccur.EnableNotifyProlog = false;
                    chillerwarningoccur.ProLogType = PrologType.UNDEFINED;
                    NoticeEventCodeParam?.Add(chillerwarningoccur);
                }
                    


                #endregion

                #region <!-- ALCD 7 -->

                EventCodeParam lotend = new EventCodeParam();
                lotend.EventCode = EventCodeEnum.LOT_END;
                lotend.ProberErrorKind = EnumProberErrorKind.INVALID;
                lotend.Title = "";
                lotend.Message = "";
                lotend.GemAlaramNumber = 21;// cell에서는 -1
                lotend.EnableNotifyMessageDialog = false;
                lotend.EnableNotifyEventlog = true;// cell에서는 false
                lotend.EnableNotifyProlog = false;
                lotend.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(lotend);

                EventCodeParam lotstart = new EventCodeParam();
                lotstart.EventCode = EventCodeEnum.LOT_START;
                lotstart.ProberErrorKind = EnumProberErrorKind.INVALID;
                lotstart.Title = "";
                lotstart.Message = "";
                lotstart.GemAlaramNumber = -1;
                lotstart.EnableNotifyMessageDialog = false;
                lotstart.EnableNotifyEventlog = false;
                lotstart.EnableNotifyProlog = false;
                lotstart.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(lotstart);

                EventCodeParam stopbeforeprobing = new EventCodeParam();
                stopbeforeprobing.EventCode = EventCodeEnum.STOP_BEFORE_PROBING;
                stopbeforeprobing.ProberErrorKind = EnumProberErrorKind.INVALID;
                stopbeforeprobing.Title = "";
                stopbeforeprobing.Message = "";
                stopbeforeprobing.GemAlaramNumber = 1051;
                stopbeforeprobing.EnableNotifyMessageDialog = false;
                stopbeforeprobing.EnableNotifyEventlog = true;
                stopbeforeprobing.EnableNotifyProlog = false;
                stopbeforeprobing.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(stopbeforeprobing);


                EventCodeParam stopafterprobing = new EventCodeParam();
                stopafterprobing.EventCode = EventCodeEnum.STOP_AFTER_PROBING;
                stopafterprobing.ProberErrorKind = EnumProberErrorKind.INVALID;
                stopafterprobing.Title = "";
                stopafterprobing.Message = "";
                stopafterprobing.GemAlaramNumber = 1071;
                stopafterprobing.EnableNotifyMessageDialog = false;
                stopafterprobing.EnableNotifyEventlog = true;
                stopafterprobing.EnableNotifyProlog = false;
                stopafterprobing.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(stopafterprobing);

                EventCodeParam pinpadmatchedtimeout = new EventCodeParam();
                pinpadmatchedtimeout.EventCode = EventCodeEnum.PIN_PAD_MATCHED_TEST_TIMEOUT;
                pinpadmatchedtimeout.ProberErrorKind = EnumProberErrorKind.INVALID;
                pinpadmatchedtimeout.Title = "";
                pinpadmatchedtimeout.Message = "";
                pinpadmatchedtimeout.GemAlaramNumber = 1091;
                pinpadmatchedtimeout.EnableNotifyMessageDialog = false;
                pinpadmatchedtimeout.EnableNotifyEventlog = true;
                pinpadmatchedtimeout.EnableNotifyProlog = false;
                pinpadmatchedtimeout.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(pinpadmatchedtimeout);

                EventCodeParam celllotpausetimeout = new EventCodeParam();
                celllotpausetimeout.EventCode = EventCodeEnum.LOT_PAUSE_TIMEOUT;
                celllotpausetimeout.ProberErrorKind = EnumProberErrorKind.INVALID;
                celllotpausetimeout.Title = "";
                celllotpausetimeout.Message = "";
                celllotpausetimeout.GemAlaramNumber = 1111;
                celllotpausetimeout.EnableNotifyMessageDialog = false;
                celllotpausetimeout.EnableNotifyEventlog = true;
                celllotpausetimeout.EnableNotifyProlog = false;
                celllotpausetimeout.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(celllotpausetimeout);


                EventCodeParam loaderlotpausetimeout = new EventCodeParam();
                loaderlotpausetimeout.EventCode = EventCodeEnum.LOT_PAUSE_TIMEOUT_LOADER;
                loaderlotpausetimeout.ProberErrorKind = EnumProberErrorKind.INVALID;
                loaderlotpausetimeout.Title = "Information Message";
                loaderlotpausetimeout.Message = "LOT_PAUSE_TIMEOUT_LOADER. Please check lot pause reason.";
                loaderlotpausetimeout.GemAlaramNumber = 50018;
                loaderlotpausetimeout.EnableNotifyMessageDialog = true;
                loaderlotpausetimeout.EnableNotifyEventlog = false;
                loaderlotpausetimeout.EnableNotifyProlog = false;
                loaderlotpausetimeout.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(loaderlotpausetimeout);


                #endregion

                EventCodeParam dewpointtimeout = new EventCodeParam();
                dewpointtimeout.EventCode = EventCodeEnum.DEW_POINT_TIMEOUT;
                dewpointtimeout.ProberErrorKind = EnumProberErrorKind.INVALID;
                dewpointtimeout.Title = "";
                dewpointtimeout.Message = "";
                dewpointtimeout.GemAlaramNumber = -1;
                dewpointtimeout.EnableNotifyMessageDialog = false;
                dewpointtimeout.EnableNotifyEventlog = true;
                dewpointtimeout.EnableNotifyProlog = false;
                dewpointtimeout.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(dewpointtimeout);

                EventCodeParam testingtimeout = new EventCodeParam();
                testingtimeout.EventCode = EventCodeEnum.PROBING_TESTING_TIMEOUT;
                testingtimeout.ProberErrorKind = EnumProberErrorKind.INVALID;
                testingtimeout.Title = "Error Message";
                testingtimeout.Message = "Testing timeout in stage has occured.";
                testingtimeout.GemAlaramNumber = -1;
                testingtimeout.EnableNotifyMessageDialog = true;
                testingtimeout.EnableNotifyEventlog = true;
                testingtimeout.EnableNotifyProlog = false;
                testingtimeout.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(testingtimeout);

                EventCodeParam repeattestingtimeout = new EventCodeParam();
                repeattestingtimeout.EventCode = EventCodeEnum.REPEATED_TESTING_TIMEOUT;
                repeattestingtimeout.ProberErrorKind = EnumProberErrorKind.INVALID;
                repeattestingtimeout.Title = "";
                repeattestingtimeout.Message = "";
                repeattestingtimeout.GemAlaramNumber = -1;
                repeattestingtimeout.EnableNotifyMessageDialog = false;
                repeattestingtimeout.EnableNotifyEventlog = true;
                repeattestingtimeout.EnableNotifyProlog = false;
                repeattestingtimeout.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(repeattestingtimeout);

                EventCodeParam idle_soaking_pin_align_err = new EventCodeParam();
                idle_soaking_pin_align_err.EventCode = EventCodeEnum.SOAKING_ERROR_IDLE_PINALIGN;
                idle_soaking_pin_align_err.ProberErrorKind = EnumProberErrorKind.INVALID;
                idle_soaking_pin_align_err.Title = "";
                idle_soaking_pin_align_err.Message = "";
                idle_soaking_pin_align_err.GemAlaramNumber = -1;
                idle_soaking_pin_align_err.EnableNotifyMessageDialog = false;
                idle_soaking_pin_align_err.EnableNotifyEventlog = true;
                idle_soaking_pin_align_err.EnableNotifyProlog = false;
                idle_soaking_pin_align_err.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(idle_soaking_pin_align_err);

                EventCodeParam idle_soaking_wafer_align_err = new EventCodeParam();
                idle_soaking_wafer_align_err.EventCode = EventCodeEnum.SOAKING_ERROR_IDLE_WAFERALIGN;
                idle_soaking_wafer_align_err.ProberErrorKind = EnumProberErrorKind.INVALID;
                idle_soaking_wafer_align_err.Title = "";
                idle_soaking_wafer_align_err.Message = "";
                idle_soaking_wafer_align_err.GemAlaramNumber = -1;
                idle_soaking_wafer_align_err.EnableNotifyMessageDialog = false;
                idle_soaking_wafer_align_err.EnableNotifyEventlog = true;
                idle_soaking_wafer_align_err.EnableNotifyProlog = false;
                idle_soaking_wafer_align_err.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(idle_soaking_wafer_align_err);

                EventCodeParam chiller_not_activate_err = new EventCodeParam();
                chiller_not_activate_err.EventCode = EventCodeEnum.CHILLER_ACTIVATE_ERROR;
                chiller_not_activate_err.ProberErrorKind = EnumProberErrorKind.INVALID;
                chiller_not_activate_err.Title = "Warning Message";
                chiller_not_activate_err.Message = $"ERROR CODE : { chiller_not_activate_err.EventCode}.\rChiller is not activate. Check the chiller connection staus.";
                chiller_not_activate_err.GemAlaramNumber = -1;
                chiller_not_activate_err.EnableNotifyMessageDialog = true;
                chiller_not_activate_err.EnableNotifyEventlog = true;
                chiller_not_activate_err.EnableNotifyProlog = false;
                chiller_not_activate_err.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(chiller_not_activate_err);

                EventCodeParam evn_valve_state_err = new EventCodeParam();
                evn_valve_state_err.EventCode = EventCodeEnum.ENV_VALVE_STATE_ERROR;
                evn_valve_state_err.ProberErrorKind = EnumProberErrorKind.INVALID;
                evn_valve_state_err.Title = "Warning Message";
                evn_valve_state_err.Message = $"ERROR CODE : { evn_valve_state_err.EventCode}.\rSV is in CoolantInTemp, but the coolant valve is closed. Please check the valve status.";
                evn_valve_state_err.GemAlaramNumber = -1;
                evn_valve_state_err.EnableNotifyMessageDialog = true;
                evn_valve_state_err.EnableNotifyEventlog = true;
                evn_valve_state_err.EnableNotifyProlog = false;
                evn_valve_state_err.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(evn_valve_state_err);

                EventCodeParam temperature_state_err = new EventCodeParam();
                temperature_state_err.EventCode = EventCodeEnum.ENV_TEMPERATURE_STATE_ERROR;
                temperature_state_err.ProberErrorKind = EnumProberErrorKind.INVALID;
                temperature_state_err.Title = "Warning Message";
                temperature_state_err.Message = $"ERROR CODE : { temperature_state_err.EventCode}.\rTemperature control is in error state. Please check the chiller screen for error state.";
                temperature_state_err.GemAlaramNumber = -1;
                temperature_state_err.EnableNotifyMessageDialog = true;
                temperature_state_err.EnableNotifyEventlog = true;
                temperature_state_err.EnableNotifyProlog = false;
                temperature_state_err.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(temperature_state_err);

                EventCodeParam temperature_out_of_range = new EventCodeParam();
                temperature_out_of_range.EventCode = EventCodeEnum.ENV_TEMPARATURE_OUT_OF_RANGE;
                temperature_out_of_range.ProberErrorKind = EnumProberErrorKind.INVALID;
                temperature_out_of_range.Title = "Warning Message";
                temperature_out_of_range.Message = $"ERROR CODE : { temperature_out_of_range.EventCode}.\rTemperature is out of range (processing deviation). Please check the temperature state.";
                temperature_out_of_range.GemAlaramNumber = -1;
                temperature_out_of_range.EnableNotifyMessageDialog = false;
                temperature_out_of_range.EnableNotifyEventlog = false;
                temperature_out_of_range.EnableNotifyProlog = false;
                temperature_out_of_range.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(temperature_out_of_range);

                EventCodeParam env_temperature_not_matched_err = new EventCodeParam();
                env_temperature_not_matched_err.EventCode = EventCodeEnum.ENV_TEMPERAUTRE_NOT_MATCHED;
                env_temperature_not_matched_err.ProberErrorKind = EnumProberErrorKind.INVALID;
                env_temperature_not_matched_err.Title = "";
                env_temperature_not_matched_err.Message ="";
                env_temperature_not_matched_err.GemAlaramNumber = -1;
                env_temperature_not_matched_err.EnableNotifyMessageDialog = false;
                env_temperature_not_matched_err.EnableNotifyEventlog = false;
                env_temperature_not_matched_err.EnableNotifyProlog = false;
                env_temperature_not_matched_err.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(env_temperature_not_matched_err);

                EventCodeParam env_temperature_wait_done = new EventCodeParam();
                env_temperature_wait_done.EventCode = EventCodeEnum.ENV_TEMPERATURE_WAIT_DONE;
                env_temperature_wait_done.ProberErrorKind = EnumProberErrorKind.INVALID;
                env_temperature_wait_done.Title = "";
                env_temperature_wait_done.Message = "";
                env_temperature_wait_done.GemAlaramNumber = -1;
                env_temperature_wait_done.EnableNotifyMessageDialog = false;
                env_temperature_wait_done.EnableNotifyEventlog = false;
                env_temperature_wait_done.EnableNotifyProlog = false;
                env_temperature_wait_done.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(env_temperature_wait_done);

                EventCodeParam env_chiller_not_connected = new EventCodeParam();
                env_chiller_not_connected.EventCode = EventCodeEnum.ENV_CHILLER_NOT_CONNECTED;
                env_chiller_not_connected.ProberErrorKind = EnumProberErrorKind.INVALID;
                env_chiller_not_connected.Title = "";
                env_chiller_not_connected.Message = "";
                env_chiller_not_connected.GemAlaramNumber = -1;
                env_chiller_not_connected.EnableNotifyMessageDialog = false;
                env_chiller_not_connected.EnableNotifyEventlog = false;
                env_chiller_not_connected.EnableNotifyProlog = false;
                env_chiller_not_connected.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(env_chiller_not_connected);

                EventCodeParam lot_assignedwafer_missmatch = new EventCodeParam();
                lot_assignedwafer_missmatch.EventCode = EventCodeEnum.LOT_ASSIGNED_WAFER_MISSMATCH;
                lot_assignedwafer_missmatch.ProberErrorKind = EnumProberErrorKind.INVALID;
                lot_assignedwafer_missmatch.Title = "";
                lot_assignedwafer_missmatch.Message = "";
                lot_assignedwafer_missmatch.GemAlaramNumber = -1;
                lot_assignedwafer_missmatch.EnableNotifyMessageDialog = false;
                lot_assignedwafer_missmatch.EnableNotifyEventlog = true;
                lot_assignedwafer_missmatch.EnableNotifyProlog = false;
                lot_assignedwafer_missmatch.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(lot_assignedwafer_missmatch);

                EventCodeParam lot_assignedwafer_wafer_remain = new EventCodeParam();
                lot_assignedwafer_wafer_remain.EventCode = EventCodeEnum.LOT_ASSIGNED_WAFER_REMAIN;
                lot_assignedwafer_wafer_remain.ProberErrorKind = EnumProberErrorKind.INVALID;
                lot_assignedwafer_wafer_remain.Title = "";
                lot_assignedwafer_wafer_remain.Message = "";
                lot_assignedwafer_wafer_remain.GemAlaramNumber = -1;
                lot_assignedwafer_wafer_remain.EnableNotifyMessageDialog = false;
                lot_assignedwafer_wafer_remain.EnableNotifyEventlog = true;
                lot_assignedwafer_wafer_remain.EnableNotifyProlog = false;
                lot_assignedwafer_wafer_remain.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(lot_assignedwafer_wafer_remain);

                EventCodeParam wafer_change_autofeed_validation_error = new EventCodeParam();
                lot_assignedwafer_wafer_remain.EventCode = EventCodeEnum.WAFER_CHANGE_AUTOFEED_VALIDATION_FAULURE;
                lot_assignedwafer_wafer_remain.ProberErrorKind = EnumProberErrorKind.INVALID;
                lot_assignedwafer_wafer_remain.Title = "";
                lot_assignedwafer_wafer_remain.Message = "";
                lot_assignedwafer_wafer_remain.GemAlaramNumber = -1;
                lot_assignedwafer_wafer_remain.EnableNotifyMessageDialog = false;
                lot_assignedwafer_wafer_remain.EnableNotifyEventlog = false;
                lot_assignedwafer_wafer_remain.EnableNotifyProlog = false;
                lot_assignedwafer_wafer_remain.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(wafer_change_autofeed_validation_error);

                EventCodeParam wafer_change_autofeed_loader_handling_error = new EventCodeParam();
                lot_assignedwafer_wafer_remain.EventCode = EventCodeEnum.WAFER_CHANGE_AUTOFEED_LOADER_HANDLING_ERROR;
                lot_assignedwafer_wafer_remain.ProberErrorKind = EnumProberErrorKind.INVALID;
                lot_assignedwafer_wafer_remain.Title = "";
                lot_assignedwafer_wafer_remain.Message = "";
                lot_assignedwafer_wafer_remain.GemAlaramNumber = -1;
                lot_assignedwafer_wafer_remain.EnableNotifyMessageDialog = false;
                lot_assignedwafer_wafer_remain.EnableNotifyEventlog = false;
                lot_assignedwafer_wafer_remain.EnableNotifyProlog = false;
                lot_assignedwafer_wafer_remain.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(wafer_change_autofeed_loader_handling_error);

                EventCodeParam sensor_alarm_detected = new EventCodeParam();
                sensor_alarm_detected.EventCode = EventCodeEnum.SENSOR_ALARM_DETECTED;
                sensor_alarm_detected.ProberErrorKind = EnumProberErrorKind.INVALID;
                sensor_alarm_detected.Title = "Error Message";
                sensor_alarm_detected.Message = "[Emergency] Smoke sensor alarm detected! Please check machine.";
                sensor_alarm_detected.GemAlaramNumber = 60301;
                sensor_alarm_detected.EnableNotifyMessageDialog = false;
                sensor_alarm_detected.EnableNotifyEventlog = true;
                sensor_alarm_detected.EnableNotifyProlog = false;
                sensor_alarm_detected.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(sensor_alarm_detected);

                EventCodeParam sensor_disconnected = new EventCodeParam();
                sensor_disconnected.EventCode = EventCodeEnum.SENSOR_DISCONNECTED;
                sensor_disconnected.ProberErrorKind = EnumProberErrorKind.INVALID;
                sensor_disconnected.Title = "Error Message";
                sensor_disconnected.Message = "Please check the sensor's connection status!";
                sensor_disconnected.GemAlaramNumber = 60401;
                sensor_disconnected.EnableNotifyMessageDialog = true;
                sensor_disconnected.EnableNotifyEventlog = true;
                sensor_disconnected.EnableNotifyProlog = false;
                sensor_disconnected.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(sensor_disconnected);

                EventCodeParam invalid_cassettetype = new EventCodeParam();
                invalid_cassettetype.EventCode = EventCodeEnum.INVALID_CASSETTE_TYPE;
                invalid_cassettetype.ProberErrorKind = EnumProberErrorKind.INVALID;
                invalid_cassettetype.Title = "Error Message";
                invalid_cassettetype.Message = "Please check the cassette type!";
                invalid_cassettetype.GemAlaramNumber = 60501;
                invalid_cassettetype.EnableNotifyMessageDialog = true;
                invalid_cassettetype.EnableNotifyEventlog = true;
                invalid_cassettetype.EnableNotifyProlog = false;
                invalid_cassettetype.ProLogType = PrologType.UNDEFINED;
                NoticeEventCodeParam?.Add(invalid_cassettetype);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetElementMetaData()
        {

        }
        #endregion
    }


}
