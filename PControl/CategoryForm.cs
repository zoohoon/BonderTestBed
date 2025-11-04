using System;
using System.Threading.Tasks;

namespace PnPControl
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.State;
    using System.Collections.ObjectModel;
    public class CategoryForm : PNPSetupBase, IPnpCategoryForm
    {
        public override Guid ScreenGUID { get; } = new Guid();
        public CategoryForm()
        {
            try
            {
                SetupState = new NotCompletedState(this);
                Categories = new ObservableCollection<ITemplateModule>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public CategoryForm(string name)
        {
            try
            {
                InitPnpModule();
                Header = name;
                SetupState = new NotCompletedState(this);
                Categories = new ObservableCollection<ITemplateModule>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            if(parameter is EventCodeEnum)
            {
                if((EventCodeEnum)parameter == EventCodeEnum.NONE)
                {
                    return await base.Cleanup(parameter);
                }
            }
            if (SetupState.GetState() != EnumMoudleSetupState.NONE)
                return await base.Cleanup(parameter);
            else
                return EventCodeEnum.NONE;
        }
      
        public EventCodeEnum ValidationCategoryStep(object parameter)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            //UNDEFINED = PASS
            //NONE = 현재 Form 내에 일치하는 파라미터 있음. Step이동 가능함
            //PNP_EXCEPTION = 이동 못함.
            try
            {
                foreach(var module in Categories)
                {
                    if (module == parameter)
                    {
                        retVal = EventCodeEnum.NONE;
                        break;
                    }
                    else
                    {
                        if ((module as ICategoryNodeItem).StateRecoverySetup == EnumMoudleSetupState.VERIFY
                            || (module as ICategoryNodeItem).StateRecoverySetup == EnumMoudleSetupState.COMPLETE)
                        {
                            retVal = EventCodeEnum.NONE;
                            break;
                        }
                        if ((module as ICategoryNodeItem).StateSetup == EnumMoudleSetupState.NONE
                          || (module as ICategoryNodeItem).StateSetup == EnumMoudleSetupState.COMPLETE)
                            continue;
                        else
                        {
                            retVal = EventCodeEnum.PNP_EXCEPTION;
                            break;
                        }

                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

            return retVal;
        }


        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override void SetStepSetupState(string header = null)
        {
            try
            {
                if (Categories != null)
                {
                    int nonecount = 0;
                    for (int index = 0; index < Categories.Count; index++)
                    {
                        if ((Categories[index] as ICategoryNodeItem)?.StateSetup == EnumMoudleSetupState.NONE)
                            nonecount++;
                    }
                    if (nonecount == Categories.Count)
                    {
                        SetNodeSetupState(EnumMoudleSetupState.NONE);
                    }
                    else
                    {
                        for (int index = 0; index < Categories.Count; index++)
                        {
                            if(header !=null)
                                (Categories[index] as ICategoryNodeItem).SetStepSetupState();

                            if ((Categories[index] as ICategoryNodeItem)?.StateSetup == EnumMoudleSetupState.COMPLETE)
                                SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                            else
                            {
                                SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                                break;
                            }
                        }
                    }

                    //Test Code
                    //SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public override Task<EventCodeEnum> InitViewModel()
        {
            if (Categories != null)
            {
                int nonecount = 0;
                for (int index = 0; index < Categories.Count; index++)
                {
                    if ((Categories[index] as ICategoryNodeItem)?.StateSetup == EnumMoudleSetupState.NONE)
                        nonecount ++;
                }

                if(nonecount == Categories.Count)
                {
                    SetNodeSetupState(EnumMoudleSetupState.NONE);
                }
                else
                {
                    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                }

            }

            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public override Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public EventCodeEnum SaveParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(Categories.Count !=0)
                {
                    foreach(var module in Categories)
                    {
                        if (module is IHasDevParameterizable)
                            retVal = (module as IHasDevParameterizable).SaveDevParameter();
                        if (module is IHasSysParameterizable)
                            retVal = (module as IHasSysParameterizable).SaveSysParameter();
                        if (module is IPnpCategoryForm)
                        {
                            retVal = (module as IPnpCategoryForm).SaveParameter();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }    
            return retVal;
        }

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }
    }


}

    
