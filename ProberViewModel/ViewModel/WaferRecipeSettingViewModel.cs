using System;
using System.Threading.Tasks;

namespace WaferRecipeSettingVM
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using CUIServices;
    using LogModule;
    using MetroDialogInterfaces;
    using ProberErrorCode;
    using ProberInterfaces;
    using RelayCommandBase;
    using VirtualKeyboardControl;

    public class WaferRecipeSettingViewModel : IMainScreenViewModel
    {
        private readonly Guid _ViewModelGUID = new Guid("870A076C-A427-28AA-729A-D50D3A19509E");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public bool Initialized { get; set; } = false;
        public IWaferObject Wafer { get { return this.GetParam_Wafer(); } }
        public IPhysicalInfo physicalInfo { get { return this.GetParam_Wafer().GetPhysInfo(); } }
        private int _SelectedItmeIndex;

        public int SelectedItmeIndex
        {
            get { return _SelectedItmeIndex; }
            set { _SelectedItmeIndex = value; }
        }


        private ObservableCollection<string> _TemplateEntries
             = new ObservableCollection<string>();
        public ObservableCollection<string> TemplateEntries
        {
            get { return _TemplateEntries; }
            set
            {
                if (value != _TemplateEntries)
                {
                    _TemplateEntries = value;
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
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> InitViewModel()
        {

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        private double OriginThicknessVal = 0.0;
        private int OriginSelectedItmeIndex = -1;
        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ObservableCollection<string> templates = new ObservableCollection<string>();

                if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                {
                    for (int index = 0; index < this.WaferAligner().TemplateParameter.Param.TemplateInfos.Count; index++)
                    {
                        templates.Add(this.WaferAligner().TemplateParameter.Param.TemplateInfos[index].Name.Value);
                        if (this.WaferAligner().TemplateParameter.Param.SeletedTemplate.Name.Value.Equals
                            (this.WaferAligner().TemplateParameter.Param.TemplateInfos[index].Name.Value))
                        {
                            SelectedItmeIndex = index;
                            OriginSelectedItmeIndex = SelectedItmeIndex;
                            break;
                        }
                    }

                    TemplateEntries = templates;
                }
                else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    //await this.ViewModelManager().ViewTransitionAsync(new Guid("5CCAD8EF-1255-F3CE-5118-C245943F1993"));
                }

                OriginThicknessVal = Wafer.GetPhysInfo().Thickness.Value;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        //public EventCodeEnum RollBackParameter()
        //{
        //    return EventCodeEnum.NONE;
        //}

        public async void SaveParameter()
        {
            try
            {
                await Task.Run(async () =>
                {
                    //await this.WaitCancelDialogService().ShowDialog("Wait");
                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Wait");

                    if (OriginThicknessVal != Wafer.GetPhysInfo().Thickness.Value)
                    {
                        Wafer.GetSubsInfo().ActualThickness = Wafer.GetPhysInfo().Thickness.Value;
                        Wafer.SaveDevParameter();
                    }
                    if (OriginSelectedItmeIndex != SelectedItmeIndex)
                    {
                        this.TemplateManager().SaveTemplate(this.WaferAligner());
                    }

                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.WaitCancelDialogService().CloseDialog();
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }
        public bool HasParameterToSave()
        {
            bool retVal = false;

            try
            {
                if (OriginThicknessVal != Wafer.GetPhysInfo().Thickness.Value)
                    retVal = true;
                if (OriginSelectedItmeIndex != SelectedItmeIndex)
                    retVal = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }



        private AsyncCommand _TemplateSelectionChangedCommand;
        public ICommand TemplateSelectionChangedCommand
        {
            get
            {
                if (null == _TemplateSelectionChangedCommand) _TemplateSelectionChangedCommand = new AsyncCommand(TemplateSelectionChanged);
                return _TemplateSelectionChangedCommand;
            }
        }

        private async Task TemplateSelectionChanged()
        {
            try
            {
                EventCodeEnum ret = this.TemplateManager().CheckTemplate(this.WaferAligner(), true, SelectedItmeIndex);

                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("", "", EnumMessageStyle.Affirmative);
                }

                SaveParameter();
            }
            catch (Exception err)
            {
                //LoggerManager.Debug(err);
                LoggerManager.Exception(err);
            }
        }
        private AsyncCommand<CUI.Button> _CreateWaferMapViewCommand;
        public ICommand CreateWaferMapViewCommand
        {
            get
            {
                if (null == _CreateWaferMapViewCommand) _CreateWaferMapViewCommand = new AsyncCommand<CUI.Button>(FuncCreateWaferMapViewCommand);
                return _CreateWaferMapViewCommand;
            }
        }

        private async Task FuncCreateWaferMapViewCommand(CUI.Button cuiparam)
        {
            try
            {
                Guid ViewGUID = CUIService.GetTargetViewGUID(cuiparam.GUID);
                this.ViewModelManager().ViewTransitionAsync(ViewGUID);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _ThicknessTextBoxClickCommand;
        public ICommand ThicknessTextBoxClickCommand
        {
            get
            {
                if (null == _ThicknessTextBoxClickCommand) _ThicknessTextBoxClickCommand = new RelayCommand<Object>(ThicknessTextBoxClickCommandFunc);
                return _ThicknessTextBoxClickCommand;
            }
        }

        private void ThicknessTextBoxClickCommandFunc(Object param)
        {
            try
            {
                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                //tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL,Convert.ToInt32(this.Wafer.GetPhysInfo().Thickness.GetLowerLimit()),
                //   Convert.ToInt32(this.Wafer.GetPhysInfo().Thickness.GetUpperLimit()));
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 1000);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                SaveParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        //public EventCodeEnum CheckParameterToSave()
        //{
        //    return EventCodeEnum.NONE;
        //}
    }
}
