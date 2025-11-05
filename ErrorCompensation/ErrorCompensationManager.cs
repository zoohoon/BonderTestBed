using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using ProberErrorCode;
using LogModule;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace ErrorCompensation
{
    public class ErrorCompensationManager : IErrorCompensationManager, INotifyPropertyChanged
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

        public IParam ErrorCompensationDescriptorParam_IParam { get; set; }
        //public ErrorCompensationDescriptorParam _ErrorDescriptor { get { return SysParam as ErrorCompensationDescriptorParam; } }

        private ErrorCompensationDescriptorParam _ErrorCompensationDescriptorParam_Clone;
        public ErrorCompensationDescriptorParam ErrorCompensationDescriptorParam_Clone
        {
            get { return _ErrorCompensationDescriptorParam_Clone; }
            set
            {
                if (value != _ErrorCompensationDescriptorParam_Clone)
                {
                    _ErrorCompensationDescriptorParam_Clone = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ICompensationModule _CompensationModule;
        public ICompensationModule CompensationModule
        {
            get { return _CompensationModule; }
            set
            {
                if (value != _CompensationModule)
                {
                    _CompensationModule = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<ProbeAxisObject> _AssociatedAxes;
        [XmlIgnore, JsonIgnore]
        public ObservableCollection<ProbeAxisObject> AssociatedAxes
        {
            get { return _AssociatedAxes; }
            set
            {
                if (value != _AssociatedAxes)
                {
                    _AssociatedAxes = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<EnumAxisConstants> _AssociatedAxisTypeList;
        public List<EnumAxisConstants> AssociatedAxisTypeList
        {
            get { return _AssociatedAxisTypeList; }
            set
            {
                if (value != _AssociatedAxisTypeList)
                {
                    _AssociatedAxisTypeList = value;
                    RaisePropertyChanged();
                }
            }
        }


        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new ErrorCompensationDescriptorParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retval = this.LoadParameter(ref tmpParam, typeof(ErrorCompensationDescriptorParam), null, null, Extensions_IParam.FileType.XML);

                ErrorCompensationDescriptorParam_IParam = tmpParam;
                ErrorCompensationDescriptorParam_Clone = ErrorCompensationDescriptorParam_IParam as ErrorCompensationDescriptorParam;

                AssociatedAxisTypeList = ErrorCompensationDescriptorParam_Clone.AssociatedAxisTypeList.Value.ToList();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public EventCodeEnum LoadParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            RetVal = LoadSysParameter();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        private EventCodeEnum SetCompeCompensationModule()
        {
            CompensationModule = new ErrorCompensationModule();

            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                if (ErrorCompensationDescriptorParam_Clone == null)
                {
                    retval = EventCodeEnum.PARAM_ERROR;

                    return retval;
                }
                else
                {
                    foreach (ErrorModuleDescriptor errordesc in ErrorCompensationDescriptorParam_Clone.ErrorModuleDescriptors)
                    {
                        if (errordesc.Type.Value == ErrorModuleType.First)
                        {
                            CompensationModule.Enable1D = errordesc.Enable.Value;
                                CompensationModule.EnableLinearComp = ErrorCompensationDescriptorParam_Clone.EnableLinear.Value;
                                CompensationModule.EnableStraightnessComp = ErrorCompensationDescriptorParam_Clone.EnableStraightness.Value;
                                CompensationModule.EnableAngularComp = ErrorCompensationDescriptorParam_Clone.EnableAngular.Value;

                            }
                            else if (errordesc.Type.Value == ErrorModuleType.Second)
                        {
                            CompensationModule.Enable2D = errordesc.Enable.Value;
                        }

                    }
           
                        retval = (CompensationModule as IHasSysParameterizable).LoadSysParameter();

                        if (retval == EventCodeEnum.NONE)
                        {
                        }
                        else
                        {
                            LoggerManager.Error("Compensation Module Init Failed.");
                        }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retval;
        }


        private CompensationValue ResultCompValue = new CompensationValue();
        public CompensationValue GetErrorComp(CompensationPos CPos)
        {
            try
            {
                ResultCompValue.XValue = 0;
                ResultCompValue.YValue = 0;
                ResultCompValue.CValue = 0;

                CompensationValue compValue;

                if (CompensationModule.Enable1D == true || CompensationModule.Enable2D == true)
                {
                    compValue = CompensationModule.GetErrorComp(CPos);

                    ResultCompValue.XValue += compValue.XValue;
                    ResultCompValue.YValue += compValue.YValue;
                    ResultCompValue.CValue += compValue.CValue;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            
            return ResultCompValue;
        }

        public int CalcErrorComp()
        {
            int retVal = 0;
            try
            {
            if (CompensationModule.Enable1D == true || CompensationModule.Enable2D == true)
            {
                retVal = CompensationModule.GetErrorComp(AssociatedAxes);
            }
            if (retVal != 0) { return retVal; }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    retval = SetCompeCompensationModule();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"SetCompeCompensationModule Failed");
                    }

                    AssociatedAxes = new ObservableCollection<ProbeAxisObject>();

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
        public void DeInitModule()
        {

        }
    }
}
