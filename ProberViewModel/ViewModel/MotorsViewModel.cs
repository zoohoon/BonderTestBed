using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MotorsVM
{
    using Autofac;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RecipeEditorControl.RecipeEditorParamEdit;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;

    public class MotorsViewModel : IMainScreenViewModel, IParamScrollingViewModel, IRecipeEditorSelectAble
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

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

        #region ==> AxisTypeList
        private ObservableCollection<String> _AxisTypeList = new ObservableCollection<string>();
        public ObservableCollection<String> AxisTypeList
        {
            get { return _AxisTypeList; }
            set
            {
                if (value != _AxisTypeList)
                {
                    _AxisTypeList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private string _SelectedItem;
        public string SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (value != _SelectedItem)
                {
                    _SelectedItem = value;
                    RaisePropertyChanged();
                }
            }
        }

        private AsyncCommand<object> _SelectedItemChangedCommand;
        public ICommand SelectedItemChangedCommand
        {
            get
            {
                if (null == _SelectedItemChangedCommand)
                {
                    _SelectedItemChangedCommand = new AsyncCommand<object>(SelectedItemChangedCommandFunc);
                }

                return _SelectedItemChangedCommand;
            }
        }

        private async Task SelectedItemChangedCommandFunc(object param)
        {
            try
            {
                SelectedItemFiltering(_SelectedItem);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region ==> Label
        // Combo Box 에 보여줄 item이 저장되어 있는 프로퍼티 이름
        // ex) MotionManager.LoaderAxes.ProbeAxisObject[0].Label.Value = "X"
        private Element<string> _Label = new Element<string>();
        public Element<string> Label
        {
            get
            {
                return _Label;
            }
            set
            {
                if (value != _Label)
                {
                    _Label = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private readonly Guid _ViewModelGUID = new Guid("85fae7d6-723f-490e-a163-0bf24f8acb03");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public bool Initialized { get; set; } = false;
        private Dictionary<string, string> _AxisPropertyPathMap = new Dictionary<string, string>();
        private List<IElement> _SystemParamElementList;
        private int _CategoryID = 00010005;



        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retval);
        }

        private async Task PopulatePageContents()
        {
            try
            {
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                {
                    _SystemParamElementList = this.ParamManager().GetSysElementList();
                }
                else
                {
                    _SystemParamElementList = this.GetLoaderContainer().Resolve<ILoaderParamManager>().GetSysElementList();
                }

                // 순서 중요! category filtering 하기 전에 Combo Box를 채워준다.
                // Category filtering 할때 masking level로 걸러지기 때문에 Label 값을 얻어 올 수 없다.     
                _Label.Value = "Label";

                List<IElement> stageAxisElemList = _SystemParamElementList.Where(item => item.PropertyPath.Contains("StageAxes")).ToList();

                List<IElement> stageAxisLabelElemList = stageAxisElemList.Where(item => item.PropertyPath.Contains("AxisType")).ToList();

                foreach (IElement elem in stageAxisLabelElemList)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (AxisTypeList.Contains(elem.GetValue().ToString()) == false)
                        {
                            AxisTypeList.Add(elem.GetValue().ToString());
                            int endDotIdx = elem.PropertyPath.LastIndexOf("AxisType");
                            String keyPropPath = elem.PropertyPath.Remove(endDotIdx);
                            _AxisPropertyPathMap.Add(elem.GetValue().ToString(), keyPropPath);
                        }
                    });
                }

                foreach (IElement elem in stageAxisLabelElemList)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (AxisTypeList.Contains("etc") == false)
                        {
                            AxisTypeList.Add("etc");
                            _AxisPropertyPathMap.Add("etc", "");
                        }
                    });
                }

                if (AxisTypeList.Contains("All") == false)
                {
                    if (Enum.GetNames(typeof(EnumAxisConstants)).Length > 0)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            AxisTypeList.Add("All");
                            SelectedItem = AxisTypeList.Last();
                        });
                    }
                }
                else
                {
                    SelectedItem = AxisTypeList.Last();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"MotorsViewModel.PopulatePageContents(): Error occurred. Err = {err.Message}");
            }
        }
        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                await Task.Run(() => PopulatePageContents());
                await Task.Run(() => SelectedItemFiltering(SelectedItem));

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
            SelectedItem = string.Empty;

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }
        public async Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            Task task = new Task(() =>
            {

                try
                {
                    LoggerManager.Debug($"DeInitViewModel() in {GetType().Name}");

                    retval = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            });
            task.Start();
            await task;

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

        private async Task SelectedItemFiltering(string selectedItem)
        {
            try
            {
                List<IElement> filter = new List<IElement>();

                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                RecipeEditorParamEdit.HardCategoryFiltering(_CategoryID);

                foreach (KeyValuePair<string, string> axisPropertyPath in _AxisPropertyPathMap)
                {
                    string axisType = axisPropertyPath.Key;
                    string propertyPath = axisPropertyPath.Value;

                    if (axisType == "etc")
                    {
                        filter = _SystemParamElementList.Where(item => item.PropertyPath.Contains("ProbeAxisObject") == false && item.PropertyPath.Contains("MotionManager.StageAxes")).ToList();
                    }
                    else
                    {
                        filter = _SystemParamElementList.Where(x => x.PropertyPath.Contains(propertyPath)).ToList();
                    }

                    RecipeEditorParamEdit.AddDataNameInfo(axisType, filter);
                }

                if (_AxisPropertyPathMap.ContainsKey(selectedItem))
                {
                    string propertyPath = _AxisPropertyPathMap[selectedItem];
                    if (selectedItem == "etc")
                    {
                        filter = _SystemParamElementList.Where(item => item.PropertyPath.Contains("ProbeAxisObject") == false && item.PropertyPath.Contains("MotionManager.StageAxes")).ToList();
                    }
                    else
                    {
                        filter = _SystemParamElementList.Where(x => x.PropertyPath.Contains(propertyPath)).ToList();
                    }
                    RecipeEditorParamEdit.HardElementFiltering(filter);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
