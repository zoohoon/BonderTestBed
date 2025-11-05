using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using RecipeEditorControl.RecipeEditorParamEdit;
using RelayCommandBase;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Autofac;

namespace TouchSensorVM
{
    public class TouchSensorViewModel : IMainScreenViewModel, IParamScrollingViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
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

        private readonly Guid _ViewModelGUID = new Guid("8D28C9F7-35C2-48DD-96C1-5BB8E48FE083");
        public Guid ScreenGUID
        {
            get { return _ViewModelGUID; }
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
                RecipeEditorParamEdit.HardCategoryFiltering(00020003);
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
        public Task<EventCodeEnum> DeInitViewModel(object parameter = null)
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


        public EventCodeEnum RollBackParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
                //retVal = GPIB.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool HasParameterToSave()
        {
            return true;
        }

        public EventCodeEnum UpProc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                RecipeEditorParamEdit.PrevPageCommandFunc();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum DownProc()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                RecipeEditorParamEdit.NextPageCommandFunc();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private AsyncCommand<CUI.Button> _TouchSensorSetupCommand;
        public ICommand TouchSensorSetupCommand
        {
            get
            {
                if (null == _TouchSensorSetupCommand) _TouchSensorSetupCommand = new AsyncCommand<CUI.Button>(TouchSensorSetupCommandFunc);
                return _TouchSensorSetupCommand;
            }
        }

        private async Task TouchSensorSetupCommandFunc(CUI.Button cuiparam)
        {
            try
            {
                if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    Guid viewguid = new Guid();
                    List<Guid> pnpsteps = new List<Guid>();

                    this.PnPManager().GetCuiBtnParam(this.NeedleCleaner(), cuiparam.GUID, out viewguid, out pnpsteps);

                    if (pnpsteps.Count != 0)
                    {
                        this.PnPManager().SetNavListToGUIDs(this.NeedleCleaner(), pnpsteps);
                        await this.ViewModelManager().ViewTransitionAsync(viewguid);
                    }
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                    {
                        Guid viewguid = new Guid();
                        List<Guid> pnpsteps = new List<Guid>();
                        this.PnPManager().GetCuiBtnParam(this.NeedleCleaner(), cuiparam.GUID, out viewguid, out pnpsteps);
                        if (pnpsteps.Count != 0)
                        {
                            this.PnPManager().SetNavListToGUIDs(this.NeedleCleaner(), pnpsteps);
                            await this.ViewModelManager().ViewTransitionAsync(viewguid);
                        }
                    }
                    else
                    {
                        //    List<Guid> pnpsteps = new List<Guid>();
                        //    pnpsteps.Add(new Guid("36972B01-CC9A-6E37-10F4-73228D50521C")); //TouchSensorSetup
                        //    pnpsteps.Add(new Guid("72AC2B4C-2CF9-6D37-D8E7-F91C8D87CAF7")); //TouchSensorBaseSetup
                        //    pnpsteps.Add(new Guid("D1205560-3025-DD6C-C29C-A5C158F8BF80")); //TouchSensorPadRefSetup
                        //    pnpsteps.Add(new Guid("2AA2C7F4-8171-1A9D-0EBD-C8ED49A3E6A0")); //TouchSensorOffsetSetup
                        //    this.PnPManager().SetNavListToGUIDs(this.NeedleCleaner(), pnpsteps);

                        await this.ViewModelManager().ViewTransitionAsync(new Guid("1B96AA21-1613-108A-71D6-9BCE684A4DD0"));
                        await this.PnPManager().SettingRemotePNP("NeedleCleanerModule", "INeedleCleanModule", cuiparam.GUID);

                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
