using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Autofac;
using ProberInterfaces;
using ProberInterfaces.Vision;
using ProberInterfaces.Enum;
using RelayCommandBase;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PMIModuleParameter;
using ProberInterfaces.DialogControl;
using LogModule;
using System.Runtime.CompilerServices;
using ProberErrorCode;
using ProberInterfaces.PMI;
using WaferSelectSetup;
using System.Collections.ObjectModel;
using SubstrateObjects;
using SerializerUtil;

namespace PMISetup.UC
{
    public class WaferPMIMapSetupControlService : WaferSelectSetupBase, INotifyPropertyChanged, IFactoryModule, IPnpAdvanceSetupViewModel
    {
        #region == > PropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public WaferPMIMapSetupControl DialogControl;
        //private PMIModuleDevParam ModuleParam;

        //private WaferObject _WaferObject;
        //public WaferObject WaferObject
        //{
        //    get { return _WaferObject; }
        //    set
        //    {
        //        if (value != _WaferObject)
        //        {
        //            _WaferObject = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private ObservableCollection<ButtonDescriptor> Wafers;

        //private IStateModule _PMIModule;
        //public IStateModule PMIModule
        //{
        //    get { return _PMIModule; }
        //    set
        //    {
        //        if (value != _PMIModule)
        //        {
        //            _PMIModule = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private IPMIInfo _PMIInfo;
        public IPMIInfo PMIInfo
        {
            get { return _PMIInfo; }
            set
            {
                if (value != _PMIInfo)
                {
                    _PMIInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _WaferIntervalVisibility;
        public Visibility WaferIntervalVisibility
        {
            get { return _WaferIntervalVisibility; }
            set
            {
                if (value != _WaferIntervalVisibility)
                {
                    _WaferIntervalVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }


        private RelayCommand<object> _SelectWaferTemplateCommand;
        public ICommand SelectWaferTemplateCommand
        {
            get
            {
                if (null == _SelectWaferTemplateCommand) _SelectWaferTemplateCommand = new RelayCommand<object>(SelectWaferTemplateCmd);
                return _SelectWaferTemplateCommand;
            }
        }

        private void SelectWaferTemplateCmd(object wafertemplate)
        {
            try
            {
                WaferTemplate tmp = wafertemplate as WaferTemplate;

                if (tmp != null)
                {
                    var q = PMIInfo.WaferTemplateInfo.IndexOf(PMIInfo.WaferTemplateInfo.Where(X => X == tmp).FirstOrDefault());

                    if (q > -1)
                    {
                        SelectedWaferIndex = q;
                        //PMIInfo.SelectedWaferTemplateIndex = q;
                        // do stuff
                    }
                    else
                    {
                        // do other stuff
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _AddNormalPMIMapCommand;
        public ICommand AddNormalPMIMapCommand
        {
            get
            {
                if (null == _AddNormalPMIMapCommand) _AddNormalPMIMapCommand = new RelayCommand(AddNormalPMIMapCmd);
                return _AddNormalPMIMapCommand;
            }
        }

        private void AddNormalPMIMapCmd()
        {
            try
            {
                long maxX, minX, maxY, minY;
                long xNum, yNum;

                maxX = this.StageSupervisor().WaferObject.GetSubsInfo().Devices.Max(d => d.DieIndexM.XIndex);
                minX = this.StageSupervisor().WaferObject.GetSubsInfo().Devices.Min(d => d.DieIndexM.XIndex);
                maxY = this.StageSupervisor().WaferObject.GetSubsInfo().Devices.Max(d => d.DieIndexM.YIndex);
                minY = this.StageSupervisor().WaferObject.GetSubsInfo().Devices.Min(d => d.DieIndexM.YIndex);

                xNum = maxX - minX + 1;
                yNum = maxY - minY + 1;

                DieMapTemplate tmpMap = new DieMapTemplate((int)xNum, (int)yNum);

                if (tmpMap != null)
                {
                    PMIInfo.NormalPMIMapTemplateInfo.Add(tmpMap);

                    PMIInfo.SelectedNormalPMIMapTemplateIndex = PMIInfo.NormalPMIMapTemplateInfo.Count - 1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private RelayCommand _DeleteNormalPMIMapCommand;
        public ICommand DeleteNormalPMIMapCommand
        {
            get
            {
                if (null == _DeleteNormalPMIMapCommand) _DeleteNormalPMIMapCommand = new RelayCommand(DeleteNormalPMIMapCmd);
                return _DeleteNormalPMIMapCommand;
            }
        }

        private void DeleteNormalPMIMapCmd()
        {
            try
            {
                if (PMIInfo.NormalPMIMapTemplateInfo.Count > 1)
                {
                    int removeindex = PMIInfo.SelectedNormalPMIMapTemplateIndex;

                    PMIInfo.NormalPMIMapTemplateInfo.RemoveAt(PMIInfo.SelectedNormalPMIMapTemplateIndex);

                    if (PMIInfo.SelectedNormalPMIMapTemplateIndex > PMIInfo.NormalPMIMapTemplateInfo.Count)
                    {
                        PMIInfo.SelectedNormalPMIMapTemplateIndex = PMIInfo.NormalPMIMapTemplateInfo.Count - 1;
                    }
                    else
                    {
                        if (PMIInfo.SelectedNormalPMIMapTemplateIndex > 0)
                        {
                            PMIInfo.SelectedNormalPMIMapTemplateIndex--;
                        }
                    }

                    foreach (var wafertemplate in PMIInfo.WaferTemplateInfo)
                    {
                        if (wafertemplate.SelectedMapIndex.Value == removeindex)
                        {
                            wafertemplate.SelectedMapIndex.Value = 0;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //private WaferTemplate _SelectedItem;
        //public WaferTemplate SelectedItem
        //{
        //    get { return _SelectedItem; }
        //    set
        //    {
        //        if (value != _SelectedItem)
        //        {
        //            _SelectedItem = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        public ICamera CurCam { get; set; }
        //public IProberStation Prober { get; set; }
        public IStageSupervisor StageSupervisor { get => this.StageSupervisor(); }

        //public ICoordinateManager CoordinateManager { get; set; }

        public IProbingModule ProbingModule { get => this.ProbingModule(); }
        public ICoordinateManager CoordManager { get => this.CoordinateManager(); }
        public float ZoomLevel { get; set; } = 30;

        public void ConstructorInit()
        {
            DialogControl = new WaferPMIMapSetupControl();
            DialogControl.DataContext = this;
            //PMIModule = this.PMIModule();
            //CoordinateManager = this.CoordinateManager();
            //Prober = this.ProberStation();

            //SelectedWaferIndex = 0;
            //CurMapIndex = 0;
            //DisplayMapIndex = PMIInfo.SelectedNormalPMIMapTemplateIndex + 1;
        }

        //public WaferPMIMapSetupControlService()
        //{
        //    //Wafer = this.StageSupervisor().WaferObject as WaferObject;
        //    ConstructorInit();

        //    InitWaferSelectionSetup();

        //    //SelectedWaferIndex = 0;
        //    //CurMapIndex = 0;
        //    //DisplayMapIndex = PMIInfo.SelectedNormalPMIMapTemplateIndex + 1;

        //    PMIInfo.SelectedNormalPMIMapTemplateIndex = 0;
        //}

        public WaferPMIMapSetupControlService()
        {
            try
            {
                //Wafer = this.StageSupervisor().WaferObject as WaferObject;
                ConstructorInit();

                //NormalPMIMapSetupModule = normalPMIMapSetupModule;
                //ModuleParam = NormalPMIMapSetupModule.PMIDevParam;

                //WaferObject = normalPMIMapSetupModule.Wafer as WaferObject;

                InitWaferSelectionSetup();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #region ==> Properties

        //private int _SelectedWaferIndex = 0;
        //public int SelectedWaferIndex
        //{
        //    get { return _SelectedWaferIndex; }
        //    set
        //    {
        //        if (value != _SelectedWaferIndex)
        //        {
        //            _SelectedWaferIndex = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private int _SelectedWaferIndex = 0;
        public int SelectedWaferIndex
        {
            get { return _SelectedWaferIndex; }
            set
            {
                if (value != _SelectedWaferIndex)
                {
                    _SelectedWaferIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private int _CurMapIndex = 0;
        //public int CurMapIndex
        //{
        //    get { return _CurMapIndex; }
        //    set
        //    {
        //        if (value != _CurMapIndex)
        //        {
        //            _CurMapIndex = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private int _DisplayMapIndex;
        //public int DisplayMapIndex
        //{
        //    get { return _DisplayMapIndex; }
        //    set
        //    {
        //        if (value != _DisplayMapIndex)
        //        {
        //            _DisplayMapIndex = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private int _CurWaferMapIndex;
        //public int CurWaferMapIndex
        //{
        //    get { return _CurWaferMapIndex; }
        //    set
        //    {
        //        if (value != _CurWaferMapIndex)
        //        {
        //            _CurWaferMapIndex = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}


        #endregion

        #region ==> Command
        private AsyncCommand _CmdExitClick;
        public ICommand CmdExitClick
        {
            get
            {
                if (null == _CmdExitClick) _CmdExitClick = new AsyncCommand(ExitDialogCommand);
                return _CmdExitClick;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task ExitDialogCommand()
        {
            try
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                //retval = this.PMIModule().SaveDevParameter();

                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"Parameter save is failed.");
                }

                // Close current pop-up Window
                //await HiddenDialogControl();
                await this.PnPManager().ClosePnpAdavanceSetupWindow();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task ShowDialogControl()
        {
            try
            {
                await this.MetroDialogManager().ShowWindow(DialogControl);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [ShowDialogControl()] : {err}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task HiddenDialogControl()
        {
            try
            {
                await this.MetroDialogManager().CloseWindow(DialogControl);
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [HiddenDialogControl()] : {err}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitWaferSelectionSetup()
        {
            try
            {
                ////////////////////////////////////////////////////////////////////////////////////
                //if (PMIInfo.GetWaferTemplateInfo() == null)
                //{
                //    PMIInfo.WaferInfo = new ObservableCollection<WaferTemplate>();
                //}

                //if (PMIInfo.WaferInfo.Count == 0)
                //{
                //    for (int i = 0; i < 25; i++)
                //    {
                //        PMIInfo.WaferInfo.Add(new WaferTemplate());
                //    }
                //}
                ////////////////////////////////////////////////////////////////////////////////////

                Wafers = new ObservableCollection<ButtonDescriptor>();

                for (int i = 0; i < 25; i++)
                {
                    Wafers.Add(new ButtonDescriptor());
                }

                WaferSelectBtn = new ObservableCollection<ButtonDescriptor>(Wafers);

                for (int i = 0; i < WaferSelectBtn.Count; i++)
                {
                    WaferSelectBtn[i].Command = new RelayCommand<Object>(SelectWaferIndexCommand);
                    WaferSelectBtn[i].CommandParameter = i;
                    //WaferSelectBtn[i].isChecked = PMIInfo.GetWaferTemplate(i).PMIEnable.Value;
                }

                SelectAllBtn.Command = new RelayCommand<Object>(SetAllWaferUsingCommand);
                SelectAllBtn.CommandParameter = true;

                ClearAllBtn.Command = new RelayCommand<Object>(SetAllWaferUsingCommand);
                ClearAllBtn.CommandParameter = false;


                // For GP
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    WaferIntervalVisibility = Visibility.Hidden;
                }
                else
                {
                    WaferIntervalVisibility = Visibility.Visible;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [InitWaferSelectionSetup()] : {err}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void SelectWaferIndexCommand(Object index)
        {
            try
            {
                if (index is int)
                {
                    PMIInfo.WaferTemplateInfo[(int)index].PMIEnable.Value = WaferSelectBtn[(int)index].isChecked;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [SelectWaferIndexCommand()] : {err}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetAllWaferUsingCommand(Object Using)
        {
            try
            {
                bool flag = (bool)Using;

                for (int i = 0; i < 25; i++)
                {
                    PMIInfo.GetWaferTemplate(i).PMIEnable.Value = flag;
                    WaferSelectBtn[i].isChecked = flag;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [SetAllWaferUsingCommand()] : {err}");
            }
        }

        private RelayCommand<SETUP_DIRECTION> _ChangeMapIndexCommand;
        public ICommand ChangeMapIndexCommand
        {
            get
            {
                if (null == _ChangeMapIndexCommand) _ChangeMapIndexCommand = new RelayCommand<SETUP_DIRECTION>(ChangeMapIndexFunc);
                return _ChangeMapIndexCommand;
            }
        }
        /// <summary>
        /// 선택된 PMI Map index를 변경하는 함수
        /// </summary>
        /// <param name="direction"></param>
        public void ChangeMapIndexFunc(SETUP_DIRECTION direction)
        {
            try
            {
                switch (direction)
                {
                    case SETUP_DIRECTION.PREV:

                        if (PMIInfo.SelectedNormalPMIMapTemplateIndex > 0)
                        {
                            PMIInfo.SelectedNormalPMIMapTemplateIndex--;
                        }
                        else
                        {
                            PMIInfo.SelectedNormalPMIMapTemplateIndex = PMIInfo.NormalPMIMapTemplateInfo.Count - 1;
                        }
                        break;

                    case SETUP_DIRECTION.NEXT:

                        if (PMIInfo.SelectedNormalPMIMapTemplateIndex < PMIInfo.NormalPMIMapTemplateInfo.Count - 1)
                        {
                            PMIInfo.SelectedNormalPMIMapTemplateIndex++;
                        }
                        else
                        {
                            PMIInfo.SelectedNormalPMIMapTemplateIndex = 0;
                        }
                        break;

                    default:
                        break;
                }

                this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo().SelectedNormalPMIMapTemplateIndex = PMIInfo.SelectedNormalPMIMapTemplateIndex;

                //PMIInfo.SetSelectedNormalMapTemplateIndex(CurMapIndex);
                //DisplayMapIndex = CurMapIndex + 1;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [ChangeMapIndexFunc()] : {err}");
            }
        }

        private RelayCommand _ApplyMapToWaferCommand;
        public ICommand ApplyMapToWaferCommand
        {
            get
            {
                if (null == _ApplyMapToWaferCommand) _ApplyMapToWaferCommand = new RelayCommand(ApplyMapToWaferFunc);
                return _ApplyMapToWaferCommand;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ApplyMapToWaferFunc()
        {
            try
            {
                PMIInfo.WaferTemplateInfo[SelectedWaferIndex].SelectedMapIndex.Value = PMIInfo.SelectedNormalPMIMapTemplateIndex;

                //PMIInfo.SelectedWaferTemplate.SelectedMapIndex.Value = PMIInfo.SelectedNormalPMIMapTemplateIndex;
                //CurWaferMapIndex = PMIInfo.SelectedWaferTemplate.SelectedMapIndex.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [ApplyMapToWaferFunc()] : {err}");
            }
        }

        private RelayCommand _ApplyMapToAllWaferCommand;
        public ICommand ApplyMapToAllWaferCommand
        {
            get
            {
                if (null == _ApplyMapToAllWaferCommand) _ApplyMapToAllWaferCommand = new RelayCommand(ApplyMapToAllWaferFunc);
                return _ApplyMapToAllWaferCommand;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ApplyMapToAllWaferFunc()
        {
            try
            {
                for (int i = 0; i < 25; i++)
                {
                    PMIInfo.WaferTemplateInfo[i].SelectedMapIndex.Value = PMIInfo.SelectedNormalPMIMapTemplateIndex;
                }

                //CurWaferMapIndex = PMIInfo.SelectedWaferTemplate.SelectedMapIndex.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[WaferPMIMapSetupControlService] [ApplyMapToAllWaferFunc()] : {err}");
            }
        }


        #endregion

        #region //.. IPnpAdvanceSetupViewModel Method
        public void SetParameters(List<byte[]> datas)
        {
            try
            {
                if (datas != null)
                {
                    foreach (var param in datas)
                    {
                        object target;
                        SerializeManager.DeserializeFromByte(param, out target, typeof(PMIInfo));
                        if (target != null)
                        {
                            PMIInfo info = (this.StageSupervisor().WaferObject.GetSubsInfo() as SubstrateInfo).PMIInfo;
                            info = (PMIInfo)target;
                            
                            PMIInfo = info;

                            //PMIInfo.SelectedWaferTemplateIndex = 0;
                            //PMIInfo.SelectedPadTemplateIndex = 0;
                            //PMIInfo.SelectedNormalPMIMapTemplateIndex = 0;
                            //PMIInfo.SelectedPadTableTemplateIndex = 0;

                            for (int i = 0; i < WaferSelectBtn.Count; i++)
                            {
                                WaferSelectBtn[i].isChecked = PMIInfo.GetWaferTemplate(i).PMIEnable.Value;
                            }

                            //PMIInfo.SelectedNormalPMIMapTemplateIndex = 0;

                            break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<byte[]> GetParameters()
        {
            List<byte[]> parameters = new List<byte[]>();

            try
            {
                if (PMIInfo != null)
                    parameters.Add(SerializeManager.SerializeToByte(PMIInfo));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return parameters;
        }

        public void Init()
        {
            return;
        }

        #endregion
    }
}
