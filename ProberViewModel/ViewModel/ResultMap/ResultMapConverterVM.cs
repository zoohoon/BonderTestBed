using Autofac;
using LoaderBase.Communication;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RecipeEditorControl.RecipeEditorParamEdit;
using RelayCommandBase;
using ResultMapParamObject;
using SerializerUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using VirtualKeyboardControl;

namespace ProberViewModel.ViewModel.ResultMap
{
    public class ResultMapConverterVM : IMainScreenViewModel, IParamScrollingViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private readonly Guid _ViewModelGUID = new Guid("f32c9cdb-c432-4ec3-b38d-d52a485b0829");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public bool Initialized { get; set; } = false;
        private const int _ResultMapDevCatID = 10051001;

        private List<IElement> _DeviceParamElementList;

        public ILoaderCommunicationManager _LoaderCommunicationManager => this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();

        IRemoteMediumProxy _RemoteMediumProxy => _LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

        private RecipeEditorParamEditViewModel _RecipeEditorParamEdit;
        public RecipeEditorParamEditViewModel RecipeEditorParamEdit
        {
            get { return _RecipeEditorParamEdit; }
            set
            {
                if (value != _RecipeEditorParamEdit)
                {
                    _RecipeEditorParamEdit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ResultMapConverterParameter _resultMapConvParam;
        public ResultMapConverterParameter ResultMapConvParam
        {
            get { return _resultMapConvParam; }
            set
            {
                if (value != _resultMapConvParam)
                {
                    _resultMapConvParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double[] _STIFVersions = new double[] { 1.1, 1.2, 1.3 };
        public double[] STIFVersions
        {
            get { return _STIFVersions; }
            set
            {
                if (value != _STIFVersions)
                {
                    _STIFVersions = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string[] _namerAliasArray;
        public string[] NamerAliasArray
        {
            get { return _namerAliasArray; }
            set
            {
                if (value != _namerAliasArray)
                {
                    _namerAliasArray = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();

                if (this.ParamManager() != null)
                {
                    _DeviceParamElementList = this.ParamManager().GetDevElementList();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    _DeviceParamElementList = this.ParamManager().GetDevElementList();
                }
                else
                {
                    _DeviceParamElementList = this.GetLoaderContainer().Resolve<ILoaderParamManager>().GetDevElementList();
                }

                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                RecipeEditorParamEdit.HardCategoryFiltering(_ResultMapDevCatID);

                ResultMapConvParam = this.ResultMapManager().GetResultMapConvIParam() as ResultMapConverterParameter;

                NamerAliasArray = this.ResultMapManager().GetNamerAliaslist();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ParamSynchronization();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        public void DeInitModule()
        {
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

        //private double _MapVersion;
        //public double MapVersion
        //{
        //    get { return _MapVersion; }
        //    set
        //    {
        //        if (value != _MapVersion)
        //        {
        //            _MapVersion = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private RelayCommand<Object> _ChangedSTIFVersionCommand;
        public ICommand ChangedSTIFVersionCommand
        {
            get
            {
                if (null == _ChangedSTIFVersionCommand)
                    _ChangedSTIFVersionCommand = new RelayCommand<object>(ChangedSTIFVersionCommandFunc);
                return _ChangedSTIFVersionCommand;
            }
        }

        private void ChangedSTIFVersionCommandFunc(object obj)
        {
            try
            {
                ParamSynchronization();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _ChangedSTIFNamerAliasCommand;
        public ICommand ChangedSTIFNamerAliasCommand
        {
            get
            {
                if (null == _ChangedSTIFNamerAliasCommand)
                    _ChangedSTIFNamerAliasCommand = new RelayCommand<object>(ChangedSTIFNamerAliasCommandFunc);
                return _ChangedSTIFNamerAliasCommand;
            }
        }

        private void ChangedSTIFNamerAliasCommandFunc(object obj)
        {
            try
            {
                LoggerManager.Debug($"[ResultMapConverterVM], ChangedSTIFNamerAliasCommandFunc() : called");

                ParamSynchronization();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _ChangedRefCalcModeCommand;
        public ICommand ChangedRefCalcModeCommand
        {
            get
            {
                if (null == _ChangedRefCalcModeCommand)
                    _ChangedRefCalcModeCommand = new RelayCommand<object>(ChangedRefCalcModeCommandFunc);
                return _ChangedRefCalcModeCommand;
            }
        }

        private void ChangedRefCalcModeCommandFunc(object obj)
        {
            try
            {
                LoggerManager.Debug($"[ResultMapConverterVM], ChangedRefCalcModeCommandFunc() : called");

                ParamSynchronization();

                RefCalcClickCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _FloatTextBoxClickCommand;
        public ICommand FloatTextBoxClickCommand
        {
            get
            {
                if (null == _FloatTextBoxClickCommand) _FloatTextBoxClickCommand = new RelayCommand<Object>(FloatTextBoxClickCommandFunc);
                return _FloatTextBoxClickCommand;
            }
        }

        private void FloatTextBoxClickCommandFunc(object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.FLOAT | KB_TYPE.DECIMAL);

                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

                //ParamSynchronization();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void RefCalcClickCommandFunc()
        {
            try
            {
                // 랏드 안돌리고 ref die 값 로그로 남겨서 확인해보기 위함
                var waferPhysicalInfo = this.GetParam_Wafer()?.GetPhysInfo();
                MachineIndex RefDieMI = this.CoordinateManager().UserIndexConvertToMachineIndex(new UserIndex(waferPhysicalInfo.RefU.XIndex.Value, waferPhysicalInfo.RefU.YIndex.Value));
                var RefCenterPos = this.WaferAligner().MachineIndexConvertToDieCenter(RefDieMI.XIndex, RefDieMI.YIndex);
                LoggerManager.Debug($"[ResultMapConverterVM], RefCalcClickCommandFunc() : Auto X = {RefCenterPos.X.Value}, Auto Y = {RefCenterPos.Y.Value * (-1)}" +
                    $"Auto X = { RefCenterPos.X.Value / 25.4 * 0.1} (0.1) MIL, Auto Y = { RefCenterPos.Y.Value / 25.4 * 0.1 * (-1)} (0.1) MIL ");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ParamSynchronization()
        {
            try
            {
                if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                {
                    this.ResultMapManager().SaveDevParameter();
                }
                else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    byte[] param = null;

                    param = SerializeManager.SerializeToByte(ResultMapConvParam, typeof(ResultMapConverterParameter));

                    if (param != null)
                    {
                        SetResultMapConvIParam(param);
                    }
                    else
                    {
                        LoggerManager.Error($"ParamSynchronization() Failed");
                    }

                    var aaa = this.ResultMapManager().GetResultMapConvIParam() as ResultMapConverterParameter;

                    //ResultMapConvParam
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetResultMapConvIParam(byte[] param)
        {
            try
            {
                _RemoteMediumProxy.SetResultMapConvIParam(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum UpProc()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DownProc()
        {
            return EventCodeEnum.NONE;
        }
    }
}
