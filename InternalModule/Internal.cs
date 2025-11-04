using System;
using System.Collections.Generic;
using ProberInterfaces;
using System.ComponentModel;
using InternalParamObject;

namespace InternalModule
{
    using System.Reflection;
    using System.Collections.ObjectModel;
    using ProberInterfaces.Command;
    using ProberInterfaces.Utility;
    using ProberErrorCode;
    using System.Runtime.CompilerServices;
    using LogModule;

    public class Internal : IInternal, INotifyPropertyChanged, IHasSysParameterizable, IHasCommandRecipe
    {
        public bool Initialized { get; set; } = false;

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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = true;

                    retval = EventCodeEnum.NONE;
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

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        //private IParam _DevParam;
        //[ParamIgnore]
        //public IParam DevParam
        //{
        //    get { return _DevParam; }
        //    set
        //    {
        //        if (value != _DevParam)
        //        {
        //            _DevParam = value;
        //            NotifyPropertyChanged("Parameter");
        //        }
        //    }
        //}
        //private IParam _SysParam;
        //[ParamIgnore]
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set
        //    {
        //        if (value != _SysParam)
        //        {
        //            _SysParam = value;
        //            NotifyPropertyChanged("SysParam");
        //        }
        //    }
        //}

        private IInternalParameters _InternalParameterObject;
        public IInternalParameters InternalParameterObject
        {
            get { return _InternalParameterObject; }
            set
            {
                if (value != _InternalParameterObject)
                {
                    _InternalParameterObject = value;
                    RaisePropertyChanged();
                }
            }
        }

        private InternalParameters _InternalParam = new InternalParameters();
        public InternalParameters InternalParam
        {
            get { return _InternalParam; }
            set
            {
                if (value != _InternalParam)
                {
                    _InternalParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<TransitionInfo> _TransitionInfo = new ObservableCollection<TransitionInfo>();
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

        public string FilePath = null;

        public string FileName = null;

        public Type[] GetParmaeterTypes()
        {
            //Type[] types = new Type[]
            //{
            //    typeof(NoCommandParam),
            //    typeof(CallbackParam),
            //    typeof(AcknowledgeParam),
            //};
            List<Type> commandTypes = new List<Type>();
            try
            {
                foreach (Assembly assem in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var types = ReflectionEx.GetAssignableTypes(assem, typeof(IProbeCommandParameter));

                    commandTypes.AddRange(types);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return commandTypes.ToArray();
        }


        public EventCodeEnum SetCommandRecipe()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                var cm = this.CommandManager();

                cm.AddCommandParameters(InternalParam.CommandRecipe.Descriptors);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new InternalParameters.InternalCommandRecipe();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(InternalParameters.InternalCommandRecipe));

                if (RetVal == EventCodeEnum.NONE)
                {
                    InternalParam.CommandRecipe = tmpParam as InternalParameters.InternalCommandRecipe;
                }

                RetVal = this.LoadParameter(ref tmpParam, typeof(InternalParameters.ConsoleCommandRecipe));

                if (RetVal == EventCodeEnum.NONE)
                {
                    InternalParam.ConsoleRecipe = tmpParam as InternalParameters.ConsoleCommandRecipe;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = this.SaveParameter(InternalParam.CommandRecipe);
                RetVal = this.SaveParameter(InternalParam.ConsoleRecipe);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RetVal;
        }

    }
}
