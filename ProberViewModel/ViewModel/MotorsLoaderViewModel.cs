using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MotorsLoaderVM
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using RecipeEditorControl.RecipeEditorParamEdit;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class MotorsLoaderViewModel : IMainScreenViewModel, IParamScrollingViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
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

        #region ==> SelectedItem
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

                    if (_SelectedItem == null)
                        return;
                    SelectedItemFiltering(_SelectedItem);
                }
            }
        }
        #endregion

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

        private readonly Guid _ViewModelGUID = new Guid("615077c7-e0c9-49e0-ba5c-cc74b1fe6920");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
        }

        public bool Initialized { get; set; } = false;
        private Dictionary<string, string> _AxisPropertyPathMap = new Dictionary<string, string>();
        private List<IElement> _SystemParamElementList;
        private int _CategoryID = 00010016;


        public Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();

                // 순서 중요! category filtering 하기 전에 Combo Box를 채워준다.
                // Category filtering 할때 masking level로 걸러지기 때문에 Label 값을 얻어 올 수 없다.     
                _Label.Value = "Label";

                if (this.ParamManager() != null)
                {
                    _SystemParamElementList = this.ParamManager().GetSysElementList();

                    List<IElement> stageAxisElemList = _SystemParamElementList?.Where(item => item.PropertyPath.Contains("LoaderAxes")).ToList();

                    List<IElement> stageAxisLabelElemList = stageAxisElemList?.Where(item => item.PropertyPath.Contains("AxisType")).ToList();

                    foreach (IElement elem in stageAxisLabelElemList)
                    {
                        AxisTypeList.Add(elem.GetValue().ToString());

                        int endDotIdx = elem.PropertyPath.LastIndexOf("AxisType");
                        String keyPropPath = elem.PropertyPath.Remove(endDotIdx);

                        _AxisPropertyPathMap.Add(elem.GetValue().ToString(), keyPropPath);
                    }

                    if (Enum.GetNames(typeof(EnumAxisConstants)).Length > 0)
                        AxisTypeList.Add("All");

                    SelectedItem = AxisTypeList.Last();
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
                SelectedItemFiltering(SelectedItem);

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
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
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
        //public EventCodeEnum RollBackParameter()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    return retVal;
        //}
        //public bool HasParameterToSave()
        //{
        //    return true;
        //}

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
        private void SelectedItemFiltering(string selectItem)
        {
            try
            {
                RecipeEditorParamEdit = new RecipeEditorParamEditViewModel();
                RecipeEditorParamEdit.HardCategoryFiltering(_CategoryID);

                List<IElement> filter = new List<IElement>();
                foreach (KeyValuePair<string, string> kv in _AxisPropertyPathMap)
                {
                    String axisType = kv.Key;
                    String propertyPath = kv.Value;
                    filter = _SystemParamElementList.Where(x => x.PropertyPath.Contains(propertyPath)).ToList();
                    RecipeEditorParamEdit.AddDataNameInfo(axisType, filter);
                }

                if (selectItem == "All")
                    return;

                if (AxisTypeList.Count() < 1)
                    return;

                if (_AxisPropertyPathMap.ContainsKey(selectItem) == false)
                    return;

                foreach (KeyValuePair<string, string> kv in _AxisPropertyPathMap)
                {
                    String axisType = kv.Key;
                    String propertyPath = kv.Value;

                    if (axisType == selectItem)
                    {
                        filter = _SystemParamElementList.Where(x => x.PropertyPath.Contains(propertyPath)).ToList();
                        break;
                    }
                }
                RecipeEditorParamEdit.HardElementFiltering(filter);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
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
        //        throw;
        //    }
        //    return retVal;
        //}
    }
}
