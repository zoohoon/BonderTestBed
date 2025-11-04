using System;
using ProberInterfaces;
using ProberInterfaces.Event;
using System.ComponentModel;
using NotifyParamObject;

namespace NotifyModule
{
    using DllImporter;
    using System.Reflection;
    using System.Collections.ObjectModel;
    using ProberErrorCode;
    using LogModule;
    using System.Runtime.CompilerServices;

    public class Notify : INotify, IEventRecipe, INotifyPropertyChanged, IHasSysParameterizable
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private INotifyParameters _NotifyParameterObject;
        public INotifyParameters NotifyParameterObject
        {
            get { return _NotifyParameterObject; }
            set
            {
                if (value != _NotifyParameterObject)
                {
                    _NotifyParameterObject = value;
                    RaisePropertyChanged();
                }
            }
        }


        private NotifyParam _NotifyParam = new NotifyParam();
        public NotifyParam NotifyParam
        {
            get { return _NotifyParam; }
            set
            {
                if (value != _NotifyParam)
                {
                    _NotifyParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ModuleStateBase ModuleState =null;

        private ObservableCollection<TransitionInfo> _TransitionInfo;
        public ObservableCollection<TransitionInfo> TransitionInfo
        {
            get { return _TransitionInfo; }
            set
            {
                if (value != _TransitionInfo)
                {
                    _TransitionInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum CreateEventRecipe()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                DllImporter DLLImporter = new DllImporter();

                foreach (EventComponent_Notify EventComponent in NotifyParam.NotifyEventRecipeParams.EventRecipe_Notify.EventList)
                {
                    // Load
                    Tuple<bool, Assembly> ret = DLLImporter.LoadDLL(NotifyParam.NotifyEventRecipeParams.EventRecipe_Notify.DLLPath, EventComponent.AssemblyInfo);

                    if (ret != null && ret.Item1 == true)
                    {
                        NotifyParam.NotifyEventRecipeParams.EventRecipe_Notify.EventRecipeList_Notify.Add(DLLImporter.Assignable<INotifyEvent>(ret.Item2, EventComponent.EventName, this.GetContainer()));
                    }
                }

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum GetEventList()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            foreach (IProbeEvent evt in NotifyParam.NotifyEventRecipeParams.EventRecipe_Notify.EventRecipeList_Notify)
            {
                this.EventManager().EventList.Add(evt);
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    retval = CreateEventRecipe();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"CreateEventRecipe() Failed");
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

        public EventCodeEnum LoadNotifyRecipeParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            NotifyParam.NotifyRecipeParams = new NotifyParam.NotifyRecipeParam();

            string fullPath = this.FileManager().GetDeviceParamFullPath(NotifyParam.NotifyRecipeParams.FilePath, NotifyParam.NotifyRecipeParams.FileName);

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(NotifyParam.NotifyRecipeParam));
                if (RetVal == EventCodeEnum.NONE)
                {
                    NotifyParam.NotifyRecipeParams = tmpParam as NotifyParam.NotifyRecipeParam;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[Notify] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }

        public EventCodeEnum SaveNotifyRecipeParams()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            string fullPath = this.FileManager().GetDeviceParamFullPath(NotifyParam.NotifyRecipeParams.FilePath, NotifyParam.NotifyRecipeParams.FileName);

            try
            {
                RetVal = this.SaveParameter(NotifyParam.NotifyRecipeParams, fixFullPath: fullPath);
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.UNDEFINED;
                //LoggerManager.Error($String.Format("[Notify] SaveNotifyRecipeParams(): Serialize Error. Err = {0}", err.Message));
                LoggerManager.Exception(err);


                throw;
            }

            return RetVal;
        }

        public EventCodeEnum LoadNotifyEventRecipeParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            NotifyParam.NotifyEventRecipeParams = new NotifyParam.NotifyEventRecipeParam();

            string fullPath = this.FileManager().GetDeviceParamFullPath(NotifyParam.NotifyEventRecipeParams.FilePath, NotifyParam.NotifyEventRecipeParams.FileName);

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(NotifyParam.NotifyEventRecipeParam));

                if(RetVal == EventCodeEnum.NONE)
                {
                    NotifyParam.NotifyEventRecipeParams = tmpParam as NotifyParam.NotifyEventRecipeParam;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[Notify] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }

        public EventCodeEnum SaveNotifyEventRecipeParams()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            string fullPath = this.FileManager().GetDeviceParamFullPath(NotifyParam.NotifyEventRecipeParams.FilePath, NotifyParam.NotifyEventRecipeParams.FileName);

            try
            {
                RetVal = this.SaveParameter(NotifyParam.NotifyEventRecipeParams, fixFullPath: fullPath);
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.UNDEFINED;
                //LoggerManager.Error($String.Format("[Notify] SaveNotifyEventRecipeParams(): Serialize Error. Err = {0}", err.Message));
                LoggerManager.Exception(err);


                throw;
            }

            return RetVal;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            IParam tmpParam = null;
            tmpParam = new NotifyParam.NotifyRecipeParam();
            tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
            RetVal = this.LoadParameter(ref tmpParam, typeof(NotifyParam.NotifyRecipeParam));

            if (RetVal == EventCodeEnum.NONE)
            {
                NotifyParam.NotifyRecipeParams = tmpParam as NotifyParam.NotifyRecipeParam;
            }

            RetVal = this.LoadParameter(ref tmpParam, typeof(NotifyParam.NotifyEventRecipeParam));

            if (RetVal == EventCodeEnum.NONE)
            {
                NotifyParam.NotifyEventRecipeParams = tmpParam as NotifyParam.NotifyEventRecipeParam;
            }

            NotifyParameterObject = NotifyParam;

            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            RetVal = this.SaveParameter(NotifyParam.NotifyRecipeParams);
            RetVal = this.SaveParameter(NotifyParam.NotifyEventRecipeParams);

            return RetVal;
        }
    }
}
