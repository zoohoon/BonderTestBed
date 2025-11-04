using LogModule;
using PinAlignIntervalSettingViewProject.UC;
using ProberErrorCode;
using ProberInterfaces;
using RecipeEditorControl.RecipeEditorParamEdit;
using RelayCommandBase;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using VirtualKeyboardControl;
//using MahApps.Metro.Controls;
//using MahApps.Metro.Controls.Dialogs;
using ProbeCardObject;

namespace PinAlignIntervalSettingVM
{
    public class PinAlignIntervalSettingViewModel : IMainScreenViewModel, IParamScrollingViewModel
    {
        private Guid _ViewModelGUID = new Guid("17EC126F-7F6E-4797-8407-37F58862626D");

        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        private WaferIntervalSettingUIViewModel DialogControl;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public bool Initialized { get; set; } = false;

        #region ==> RecipeEditorParamEdit
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
        #endregion

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

        private int _SelectedIndex;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                if (value != _SelectedIndex)
                {
                    _SelectedIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PinAlignIntervalParameter _BackupData = new PinAlignIntervalParameter();
        public PinAlignIntervalParameter BackupData
        {
            get { return _BackupData; }
            set
            {
                if (value != _BackupData)
                {
                    _BackupData = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    if(this.PinAligner() != null)
                    {
                        PinAlignDevParameters PinAlignParam = (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);

                        if (PinAlignParam.PinAlignInterval == null)
                        {
                            _BackupData = new PinAlignIntervalParameter();
                        }
                        else
                        {
                            _BackupData = new PinAlignIntervalParameter(PinAlignParam.PinAlignInterval);
                        }
                    }

                    DialogControl = new WaferIntervalSettingUIViewModel();

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

        public Task<EventCodeEnum> InitViewModel()
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                RecipeEditorParamEdit.HardCategoryFiltering(10024001);

                _BackupData = null;

                PinAlignDevParameters PinAlignParam = (this.PinAligner().PinAlignDevParam as PinAlignDevParameters);

                if (PinAlignParam.PinAlignInterval == null)
                {
                    _BackupData = new PinAlignIntervalParameter();
                }
                else
                {
                    _BackupData = new PinAlignIntervalParameter(PinAlignParam.PinAlignInterval);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }


        private RelayCommand<object> _DataIncreaseCommand;
        public ICommand DataIncreaseCommand
        {
            get
            {
                if (null == _DataIncreaseCommand) _DataIncreaseCommand = new RelayCommand<object>(DataIncrease);

                return _DataIncreaseCommand;
            }
        }
        public void DataIncrease(object obj)
        {
            try
            {
                IElement element = obj as IElement;

                if (element != null)
                {
                    //string valueStr = element.GetValueByString();
                    //if (!string.IsNullOrEmpty(valueStr))
                    //{
                    //    double parseData = double.Parse(valueStr);
                    //    parseData += 1;
                    //    element.SetValueByString(parseData.ToString());
                    //}

                    double data = (double)element.GetValue();
                    data += 1;
                    element.SetValue(data);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _DataDecreaseCommand;
        public ICommand DataDecreaseCommand
        {
            get
            {
                if (null == _DataDecreaseCommand) _DataDecreaseCommand = new RelayCommand<object>(DataDecrease);

                return _DataDecreaseCommand;
            }
        }
        public void DataDecrease(object obj)
        {
            try
            {
                IElement element = obj as IElement;

                if (element != null)
                {
                    //string valueStr = element.GetValueByString();
                    //if (!string.IsNullOrEmpty(valueStr))
                    //{
                    //    double parseData = double.Parse(valueStr);
                    //    parseData -= 1;
                    //    if (parseData < 0)
                    //    {
                    //        parseData = 0;
                    //    }
                    //    element.SetValueByString(parseData.ToString());
                    //}

                    double data = (double)element.GetValue();
                    data -= 1;
                    if (data < 0)
                        data = 0;

                    element.SetValue(data);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _KeyBoardCommand;
        public ICommand KeyBoardCommand
        {
            get
            {
                if (null == _KeyBoardCommand) _KeyBoardCommand = new RelayCommand<object>(KeyBoard);

                return _KeyBoardCommand;
            }
        }
        public void KeyBoard(object obj)
        {
            try
            {
                IElement element = obj as IElement;

                if (element != null)
                {
                    //string valueStr = element.GetValueByString();
                    //if (!string.IsNullOrEmpty(valueStr))
                    //{
                    //    double parseData = double.Parse(valueStr);
                    //    string setData = VirtualKeyboard.Show(parseData.ToString(), KB_TYPE.DECIMAL, 0, 100);
                    //    if (double.Parse(setData) < 0)
                    //    {
                    //        element.SetValueByString("0");
                    //    }
                    //    else if (double.Parse(setData) > 1000)
                    //    {
                    //        element.SetValueByString("1000");
                    //    }
                    //    else
                    //    {
                    //        element.SetValueByString(setData);
                    //    }

                    //}

                    double data = (double)element.GetValue();
                    string setData = VirtualKeyboard.Show(data.ToString(), KB_TYPE.DECIMAL, 0, 100);

                    double setDataDbl = double.Parse(setData);
                    if (setDataDbl < 0)
                        element.SetValue(0);
                    else if (setDataDbl > 1000)
                        element.SetValue(1000);
                    else
                        element.SetValue(setData);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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

        public EventCodeEnum CheckParameterToSave()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum UpProc()
        {
            RecipeEditorParamEdit.PrevPageCommandFunc();
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DownProc()
        {
            RecipeEditorParamEdit.NextPageCommandFunc();
            return EventCodeEnum.NONE;
        }
        private AsyncCommand _PinAlignSetupCommand;
        public ICommand PinAlignSetupCommand
        {
            get
            {
                if (null == _PinAlignSetupCommand) _PinAlignSetupCommand = new AsyncCommand(FuncPinAlignSetup);
                return _PinAlignSetupCommand;
            }
        }

        private async Task<EventCodeEnum> FuncPinAlignSetup()
        {
            await DialogControl.ShowDialogControl();
            return EventCodeEnum.NONE;
        }

        //public EventCodeEnum SaveParameter()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {

        //        retVal = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return retVal;
        //}
    }
}
