using System;

namespace LoaderCore
{
    using LoaderParameters;
    using LogModule;
    using LoaderBase;
    using LoaderCore.GP_SensorScanCassetteStates;

    public partial class GP_SensorScanCassette : ILoaderProcessModule
    {
        public GP_SensorScanCassetteState StateObj { get; set; }

        public ScanProcParam Param { get; private set; }

        internal SensorScanCassetteData Data { get; set; }

        public Autofac.IContainer Container { get; private set; }

        public void Init(Autofac.IContainer container, ILoaderProcessParam param)
        {
            try
            {
                this.Container = container;

                this.Param = param as ScanProcParam;

                StateObj = new IdleState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool CanExecute(ILoaderProcessParam param)
        {
            var mypa = param as ScanProcParam;

            return
                mypa != null &&
                mypa.Cassette is ICassetteModule &&
                mypa.UseScanable is IScanSensorModule;
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


    }
}
