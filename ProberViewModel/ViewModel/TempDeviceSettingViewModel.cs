using Autofac;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Temperature;
using RelayCommandBase;
using SciChart.Charting.Model.ChartSeries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using VirtualKeyboardControl;
using MetroDialogInterfaces;
using LoaderBase.Communication;
using Temperature;

namespace TempDeviceSettingVM
{
    public class TempDeviceSettingViewModel : IMainScreenViewModel
    {
        public Guid ScreenGUID { get; set; } = new Guid("2FBAD965-A727-4392-9CF0-E80252EE08C5");

        public bool Initialized { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public ObservableCollection<IRenderableSeriesViewModel> RenderableSeriesViewModels { get; set; }


        private double _ValueForDeviceTemp;
        public double ValueForDeviceTemp
        {
            get { return _ValueForDeviceTemp; }
            set
            {
                if (value != _ValueForDeviceTemp)
                {
                    _ValueForDeviceTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _ValueForSettingTemp;
        public double ValueForSettingTemp
        {
            get { return _ValueForSettingTemp; }
            set
            {
                if (value != _ValueForSettingTemp)
                {
                    _ValueForSettingTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private object _TempOffsetSelectedItem;
        public object TempOffsetSelectedItem
        {
            get { return _TempOffsetSelectedItem; }
            set
            {
                if (value != _TempOffsetSelectedItem)
                {
                    _TempOffsetSelectedItem = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<KeyValuePair<double, double>> _TempCalOffsets;
        public ObservableCollection<KeyValuePair<double, double>> TempCalOffsets
        {
            get { return _TempCalOffsets; }
            set
            {
                if (value != _TempCalOffsets)
                {
                    _TempCalOffsets = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _NotApplySVChangesBasedOnDevice;
        public bool NotApplySVChangesBasedOnDevice
        {
            get { return _NotApplySVChangesBasedOnDevice; }
            set
            {
                if (value != _NotApplySVChangesBasedOnDevice)
                {
                    _NotApplySVChangesBasedOnDevice = value;
                    RaisePropertyChanged();
                }
            }
        }

   


        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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

        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }

        public Task<EventCodeEnum> InitViewModel()
        {
            ITempController TempController = this.TempController();
            try
            {
                ValueForDeviceTemp = TempController.GetDevSetTemp();

                if (RenderableSeriesViewModels == null)
                {
                    if (TempController != null)
                    {
                        RenderableSeriesViewModels = new ObservableCollection<IRenderableSeriesViewModel>()
                            {
                                new LineRenderableSeriesViewModel{
                                    DataSeries = TempController.dataSeries_CurTemp,
                                    StyleKey = "LineSeriesStyle",
                                    Stroke = Colors.Red},
                                new LineRenderableSeriesViewModel{
                                    DataSeries = TempController.dataSeries_SetTemp,
                                    StyleKey = "LineSeriesStyle",
                                    Stroke = Colors.Green},
                            };
                    }
                }

                TempCalOffsets = new ObservableCollection<KeyValuePair<double, double>>();

                if (TempController != null && TempController.TempManager != null)
                {
                    foreach (var offset in TempController.TempManager.Dic_HeaterOffset)
                    {
                        TempCalOffsets.Add(offset);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                // TODO : 스테이지 정보를 얻어오는지 확인.
                ITempController TempController = this.TempController();

                TempController.LoadDevParameter();


                ValueForSettingTemp = TempController.TempInfo?.SetTemp?.Value ?? 9999;
                // TODO: TempInfo.SetTemp는 Controller에서 돌기로한 SetTemp, 여기서는 Recipe의 SetTemp값을 수정해야하므로 TempControllerDevParameter.SetTemp값을 바꾸도록 해야함.
                if (ValueForSettingTemp != 9999)
                {
                    TempController.TempInfo.SetTemp.Value = TempController.GetSetTemp();
                    ValueForSettingTemp = TempController.TempInfo?.SetTemp?.Value ?? 9999;
                }

                ValueForSettingTemp = TempController.TempInfo?.SetTemp?.Value ?? 9999;

                TempControllerDevParam tempDevParam = TempController.TempControllerDevParam as TempControllerDevParam;
                ValueForDeviceTemp = tempDevParam.SetTemp.Value;

                // TODO: TempInfo.SetTemp는 Controller에서 돌기로한 SetTemp, 여기서는 Recipe의 SetTemp값을 수정해야하므로 TempControllerDevParameter.SetTemp값을 바꾸도록 해야함.

                NotApplySVChangesBasedOnDevice = !TempController.GetApplySVChangesBasedOnDeviceValue();
                //if(NotApplySVChangesBasedOnDevice && TempController.TempControllerDevParam != null)
                //{
                //    TempControllerDevParam tempDevParam = TempController.TempControllerDevParam as TempControllerDevParam;
                //    ValueForSettingTemp = tempDevParam.SetTemp.Value;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(FuncTextBoxClickCommand);
                return _TextBoxClickCommand;
            }
        }

        private void FuncTextBoxClickCommand(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL | KB_TYPE.FLOAT, 0, 200);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private AsyncCommand<object> _SettingSVCommand;
        public ICommand SettingSVCommand
        {
            get
            {
                if (null == _SettingSVCommand) _SettingSVCommand = new AsyncCommand<object>(SettingSVFunc);
                return _SettingSVCommand;
            }
        }

        private async Task SettingSVFunc(object obj)
        {
            try
            {
                EnumMessageDialogResult result = EnumMessageDialogResult.UNDEFIND;
                if (SystemManager.SysteMode == SystemModeEnum.Multiple | SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                {
                    //Maintanance 모드이고 해당 스테이지의 valve 가 잠겨있을시에는 온도 변경 가능.
                    //-maintanance 모드를 빠져나가 online 이 된다면 원래 Lot 돌던 Recipe로 엎어쳐져서 돌아야한다.
                    //(원래 - 20->setup에서 - 10으로 변경->online-> - 20 Device 로 변경->온도 설정 후 Lot 재개)


                    Task task = new Task(() =>
                    {
                        this.MetroDialogManager().ShowMessageDialog("Warning Message",
                      "This change chiller target temperature depending on recipe set temperature.\n" +
                      "It can affact to other cells which use same chiller.",
                      EnumMessageStyle.AffirmativeAndNegative);
                    });
                    task.Start();

                }

                ITempController TempController = this.TempController();//ApplyDevice 파라미터가 false이면? 
                TempController.SetSV(TemperatureChangeSource.TEMP_DEVICE, ValueForSettingTemp, forcedSetValue:true, willYouSaveSetValue: true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private AsyncCommand<object> _SettingDeviceSetTempCommand;
        public ICommand SettingDeviceSetTempCommand
        {
            get
            {
                if (null == _SettingDeviceSetTempCommand) _SettingDeviceSetTempCommand = new AsyncCommand<object>(SettingDeviceSetTempCommandFunc);
                return _SettingDeviceSetTempCommand;
            }
        }

        private async Task SettingDeviceSetTempCommandFunc(object obj)
        {
            try
            {
                this.TempController().SetDevSetTemp(ValueForDeviceTemp);


                if (NotApplySVChangesBasedOnDevice == false)
                {
                    ValueForSettingTemp = ValueForDeviceTemp;
                    ITempController TempController = this.TempController();//ApplyDevice 파라미터가 false이면? 
                    TempController.SetSV(TemperatureChangeSource.TEMP_DEVICE, ValueForSettingTemp, forcedSetValue: true, willYouSaveSetValue: true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _LoadParamCommand;
        public ICommand LoadParamCommand
        {
            get
            {
                if (null == _LoadParamCommand) _LoadParamCommand = new RelayCommand<object>(LoadParamFunc);
                return _LoadParamCommand;
            }
        }
        //ModifyValueCommand
        private RelayCommand<object> _ModifyValueCommand;
        public ICommand ModifyValueCommand
        {
            get
            {
                if (null == _ModifyValueCommand) _ModifyValueCommand = new RelayCommand<object>(ModifyValueMethod);
                return _ModifyValueCommand;
            }
        }

        private void ModifyValueMethod(object obj)
        {
            if (TempOffsetSelectedItem != null)
            {
                if (TempOffsetSelectedItem is double)
                {
                    double offset = (double)TempOffsetSelectedItem;
                    string setData = VirtualKeyboard.Show(offset.ToString(), KB_TYPE.DECIMAL, 0, 100);
                    double modifiedValue = 0.0;
                    if (double.TryParse(setData, out modifiedValue))
                    {
                        offset = modifiedValue;
                    }
                }
            }
        }

        //ApplyParamCommand
        private RelayCommand<object> _ApplyParamCommand;
        public ICommand ApplyParamCommand
        {
            get
            {
                if (null == _ApplyParamCommand) _ApplyParamCommand = new RelayCommand<object>(ApplyParamMethod);
                return _ApplyParamCommand;
            }
        }

        private void ApplyParamMethod(object obj)
        {
            try
            {
                ITempController TempController = this.TempController();

                if (TempCalOffsets != null)
                {
                    TempController.TempManager.Dic_HeaterOffset.Clear();
                    foreach (var item in TempCalOffsets)
                    {
                        TempController.TempManager.Dic_HeaterOffset.Add(item.Key, item.Value);
                        LoggerManager.Debug($"Temp. Offset for {item.Key}: {item.Value}");
                    }
                }

                var loadResult = TempController.TempManager.SaveSysParameter();
                loadResult = TempController.TempManager.LoadSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void LoadParamFunc(object obj)
        {
            try
            {
                ITempController TempController = this.TempController();
                var loadResult = TempController.TempManager.LoadSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
