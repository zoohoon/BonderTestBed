namespace EventProcessModule
{
    using Autofac;
    using LoaderBase;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Event.EventProcess;
    using System;
    
    public class DynamicFoupEnableChecker : EventConditionCheckBase
    {
        public DynamicFoupEnableChecker() { }
        public override bool DoCkeck(ref string checkFailReason)
        {
            bool retVal = false;
            try
            {
                ILoaderSupervisor LoaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                if(LoaderMaster != null)
                {
                    if(LoaderMaster.DynamicMode == DynamicModeEnum.DYNAMIC)
                    {
                        retVal = true;
                    }
                    else
                    {
                        checkFailReason = $"DynamicMode is {LoaderMaster.DynamicMode}.";
                    }
                }
                else
                {
                    checkFailReason = $"Loader Module is null";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class FirstContactChecker : EventConditionCheckBase
    {
        public FirstContactChecker() { }
        public override bool DoCkeck(ref string checkFailReason)
        {
            bool retVal = false;
            try
            {
                if (this.ProbingModule().IsFirstZupSequence == false)
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    public class NotFirstContactChecker: EventConditionCheckBase
    {
        public NotFirstContactChecker() { }
        public override bool DoCkeck(ref string checkFailReason)
        {
            bool retVal = false;
            try
            {
                if(this.ProbingModule().IsFirstZupSequence == true)
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}
