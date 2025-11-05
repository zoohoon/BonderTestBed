using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberInterfaces.Communication.Tester;
using NotifyEventModule;
using ProberInterfaces.Event;
using ProberInterfaces.Communication.Scenario;

namespace TesterCommunicationModule
{
    public class TesterCommunicationManager : ITesterCommunicationManager, INotifyPropertyChanged, IModule, IFactoryModule, IHasSysParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        public void DeInitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ITesterScenarioManager ScenarioManager { get; set; }

        private IParam _TesterCommunicationSysParam_IParam;
        public IParam TesterCommunicationSysParam_IParam
        {
            get { return _TesterCommunicationSysParam_IParam; }
            set
            {
                if (value != _TesterCommunicationSysParam_IParam)
                {
                    _TesterCommunicationSysParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private TesterCommunicationSysParam _TesterCommunicationSysParam;
        public TesterCommunicationSysParam TesterCommunicationSysParam
        {
            get { return _TesterCommunicationSysParam; }
            set
            {
                if (value != _TesterCommunicationSysParam)
                {
                    _TesterCommunicationSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        //public event ValueChangedEventHandler IsCreateValueChanged;
        //public delegate void ValueChangedEventHandler(bool newValue);

        //private bool _IsCreated = false;
        //public bool IsCreated
        //{
        //    get { return _IsCreated; }
        //    set
        //    {
        //        if (value != _IsCreated)
        //        {
        //            _IsCreated = value;
        //            RaisePropertyChanged();

        //            if (IsCreateValueChanged != null)
        //            {
        //                IsCreateValueChanged(value);
        //            }
        //        }
        //    }
        //}

        //public void IsCreatedChanged(bool newValue)
        //{
        //    try
        //    {
        //        // TODO : Loader에게 테스터 사용 할 수 있는 상태라는 정보 전달.   
        //        this.LoaderController().UpdateTesterCOMInfo(this.IsCreated);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //public IUseTesterDriver UseTesterDriverModule { get; private set; }

        public bool GetTesterAvailable()
        {
            bool retval = false;

            try
            {
                // 1) TesterCommunicationSysParam.EnumTesterComType.Value 이 UNDEFINED가 아니어야 됨.
                // 2) 타입에 맞는 모듈에게 Tester와 연결 가능한지 물어 본 후, 결과값 사용

                if (TesterCommunicationSysParam.EnumTesterComType.Value != EnumTesterComType.UNDEFINED)
                {
                    switch (TesterCommunicationSysParam.EnumTesterComType.Value)
                    {
                        case EnumTesterComType.UNDEFINED:
                            break;

                        case EnumTesterComType.GPIB:

                            retval = this.GPIB().GetTesterAvailable();
                            break;

                        case EnumTesterComType.TCPIP:

                            retval = this.TCPIPModule().GetTesterAvailable();
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum CreateTesterComInstance(EnumTesterComType comType)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (comType == TesterCommunicationSysParam.EnumTesterComType.Value)
                {
                    switch (TesterCommunicationSysParam.EnumTesterComType.Value)
                    {
                        case EnumTesterComType.UNDEFINED:

                            retval = EventCodeEnum.NONE;

                            break;

                        case EnumTesterComType.GPIB:

                            retval = this.GPIB().CreateTesterComDriver();
                            break;

                        case EnumTesterComType.TCPIP:

                            retval = this.TCPIPModule().CreateTesterComDriver();
                            break;

                        default:
                            break;
                    }
                }

                if (retval == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[TesterCommunicationManager], CreateTesterComInstance() : Module = {TesterCommunicationSysParam.EnumTesterComType.Value}, The tester communication driver created successfully.");
                    
                    if(ScenarioManager == null)
                    {
                        ScenarioManager = new TesterScenarioManager();
                    }
                    
                    ScenarioManager.InitModule(TesterCommunicationSysParam.EnumTesterComType.Value);
                }
                else
                {
                    LoggerManager.Debug($"[TesterCommunicationManager], CreateTesterComInstance() : Module = {TesterCommunicationSysParam.EnumTesterComType.Value}, The tester communication driver is not created successfully.");
                }
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    //retval = CreateTesterComInstance();

                    retval = this.EventManager().RegisterEvent(typeof(TesterConnectedEvent).FullName, "ProbeEventSubscibers", EventFired);
                    retval = this.EventManager().RegisterEvent(typeof(TesterDisonnectedEvent).FullName, "ProbeEventSubscibers", EventFired);

                    if (TesterCommunicationSysParam != null)
                    {
                        TesterCommunicationSysParam.EnumTesterComType.ValueChangedEvent -= EnumTesterComType_ValueChangedEvent;
                        TesterCommunicationSysParam.EnumTesterComType.ValueChangedEvent += EnumTesterComType_ValueChangedEvent;

                        retval = CreateTesterComInstance(TesterCommunicationSysParam.EnumTesterComType.Value);
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
                retval = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Debug($"[{this.GetType().Name}.InitModule()] : Error occurred while InitModule() in GpibManager. Err = {err.Message}");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void EnumTesterComType_ValueChangedEvent(object oldValue, object newValue, object valueChangedParam = null)
        {
            try
            {
                CreateTesterComInstance(TesterCommunicationSysParam.EnumTesterComType.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void EventFired(object sender, ProbeEventArgs e)
        {
            try
            {
                if (sender is TesterConnectedEvent)
                {
                    // TODO : 로더에게 테스터와의 연결이 되었음을 알림.
                    this.LoaderController()?.UpdateTesterConnectedStatus(true);
                }

                if (sender is TesterDisonnectedEvent)
                {
                    // TODO : 로더에게 테스터와의 연결이 끊겼음을 알림.
                    this.LoaderController()?.UpdateTesterConnectedStatus(false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;

                retVal = this.LoadParameter(ref tmpParam, typeof(TesterCommunicationSysParam));

                if (retVal == EventCodeEnum.NONE)
                {
                    TesterCommunicationSysParam_IParam = tmpParam;
                    TesterCommunicationSysParam = TesterCommunicationSysParam_IParam as TesterCommunicationSysParam;
                }
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
                retVal = this.SaveParameter(TesterCommunicationSysParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SetInitializedToTrue()
        {
            this.Initialized = false;
        }
    }
}
