using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using RelayCommandBase;
using System.Windows.Input;
using VirtualKeyboardControl;
using LogModule;
using ProbeCardObject;

namespace PinAlignSettingVM
{
    public class PinAlignSettingVM : IMainScreenViewModel
    {
        private Guid _ViewModelGUID = new Guid("154ADDAE-BC21-4747-8DC5-06BF3A53CFBA");

        public Guid ScreenGUID { get { return _ViewModelGUID; } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

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

        private List<PinAlignSettignParameter> _BackupData = new List<PinAlignSettignParameter>();
        public List<PinAlignSettignParameter> BackupData
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

        private PinAlignSettignParameter _DisplayData = new PinAlignSettignParameter();
        public PinAlignSettignParameter DisplayData
        {
            get { return _DisplayData; }
            set
            {
                if (value != _DisplayData)
                {
                    _DisplayData = value;
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
                        PinAlignDevParameters PinAlignParam = this.PinAligner().PinAlignDevParam as PinAlignDevParameters;

                        if (PinAlignParam.PinAlignSettignParam == null)
                        {
                            _BackupData.Add(new PinAlignSettignParameter());
                            _BackupData.Add(new PinAlignSettignParameter());
                            _BackupData.Add(new PinAlignSettignParameter());
                            _BackupData.Add(new PinAlignSettignParameter());
                            _BackupData.Add(new PinAlignSettignParameter());
                            _BackupData.Add(new PinAlignSettignParameter());
                            _BackupData.Add(new PinAlignSettignParameter());
                        }
                        else
                        {
                            foreach (PinAlignSettignParameter param in PinAlignParam.PinAlignSettignParam)
                            {
                                _BackupData.Add(new PinAlignSettignParameter(param));
                            }
                        }
                    }

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
                _BackupData.Clear();

                PinAlignDevParameters PinAlignParam = this.PinAligner().PinAlignDevParam as PinAlignDevParameters;

                if (PinAlignParam.PinAlignSettignParam == null)
                {
                    _BackupData.Add(new PinAlignSettignParameter());
                    _BackupData.Add(new PinAlignSettignParameter());
                    _BackupData.Add(new PinAlignSettignParameter());
                    _BackupData.Add(new PinAlignSettignParameter());
                    _BackupData.Add(new PinAlignSettignParameter());
                    _BackupData.Add(new PinAlignSettignParameter());
                    _BackupData.Add(new PinAlignSettignParameter());
                }
                else
                {
                    foreach (PinAlignSettignParameter param in PinAlignParam.PinAlignSettignParam)
                    {
                        _BackupData.Add(new PinAlignSettignParameter(param));
                    }
                }
                _SelectedIndex = 0;
                _DisplayData = BackupData[_SelectedIndex];
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
        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                PinAlignDevParameters PinAlignParam = this.PinAligner().PinAlignDevParam as PinAlignDevParameters;

                PinAlignParam.PinAlignSettignParam.Clear();

                foreach (PinAlignSettignParameter param in _BackupData)
                {
                    PinAlignParam.PinAlignSettignParam.Add(new PinAlignSettignParameter(param));
                }

                retVal = this.PinAligner().SavePinAlignDevParam();

                if (retVal != EventCodeEnum.NONE)
                {
                    LoggerManager.Error("[PinAlignSettingVM] Parameter Save Fail!");
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        private RelayCommand _ChangedItem;
        public ICommand ChangedItem
        {
            get
            {
                if (null == _ChangedItem) _ChangedItem = new RelayCommand(DisPlayDataChange);

                return _ChangedItem;
            }
        }
        public void DisPlayDataChange()
        {
            _DisplayData = BackupData[_SelectedIndex];
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
                    //if(!string.IsNullOrEmpty(valueStr))
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
                    //    if(parseData < 0)
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
                    //    else if(double.Parse(setData) > 1000)
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
    }
}
