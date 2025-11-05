using System;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;

namespace LoaderCore
{
    using LogModule;
    using PreAlignStates;

    public class PreAlign : ILoaderProcessModule
    {
        public PreAlignState StateObj { get; set; }
        
        public PreAlignProcParam Param { get; private set; }

        public Autofac.IContainer Container { get; private set; }

        public void Init(Autofac.IContainer container, ILoaderProcessParam param)
        {
            try
            {
                this.Container = container;

                this.Param = param as PreAlignProcParam;

                this.StateObj = new IdleState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool CanExecute(ILoaderProcessParam info)
        {
            bool retval = false;

            try
            {
                var mypa = info as PreAlignProcParam;

                retval = mypa != null &&
                mypa.UsePA is IPreAlignModule &&
                mypa.UseARM is IARMModule;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public LoaderProcStateEnum State => StateObj.State;

        public ReasonOfSuspendedEnum ReasonOfSuspended => StateObj.ReasonOfSuspended;

        public void Execute()
        {
            try
            {
                StateObj.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Awake()
        {
            try
            {
                StateObj.Resume();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SelfRecovery()
        {
            try
            {
                StateObj.SelfRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        internal PreAlignData Data { get; set; }
    }

    public class PreAlignData
    {
        public EnumProberCam Cam { get; set; }
        
        public EnumMotorDedicatedIn NotchHomeInput { get; set; }

        public PreAlignAccessParam AccessParam { get; set; }

        public PreAlignProcessingParam ProcessingParam { get; set; }

        public double Radius { get; set; }

        public int CenteringTriedCount { get; set; }
                
        public double CenOffsetAngle { get; set; }

        public double CenOffsetDist { get; set; }
        
    }
}
