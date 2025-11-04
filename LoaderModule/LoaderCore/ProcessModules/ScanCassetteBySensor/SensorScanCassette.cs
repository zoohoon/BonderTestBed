using System;
using System.Collections.Generic;
using LoaderBase;
using LoaderParameters;

namespace LoaderCore
{
    using LogModule;
    using SensorScanCassetteStates;

    public partial class SensorScanCassette : ILoaderProcessModule
    {
        public SensorScanCassetteState StateObj { get; set; }

        public ScanProcParam Param { get; private set; }

        public Autofac.IContainer Container { get; private set; }

        public void Init(Autofac.IContainer container, ILoaderProcessParam param)
        {
            try
            {

                this.Container = container;

                this.Param = param as ScanProcParam;

                this.StateObj = new IdleState(this);
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
                System.Threading.Thread.Sleep(100);
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
        internal SensorScanCassetteData Data { get; set; }

    }

    public class SensorScanCassetteData
    {
        public List<ISlotModule> SlotsOrderByBottom { get; set; }

        public List<ISlotModule> SlotsOrderByTop { get; set; }

        private List<double> _UPCapPositions = new List<double>();
        public List<double> UPCapPositions
        {
            get { return _UPCapPositions; }
            set { _UPCapPositions = value; }
        }

        private List<double> _DownCapPositions = new List<double>();
        public List<double> DownCapPositions
        {
            get { return _DownCapPositions; }
            set { _DownCapPositions = value; }
        }
    }
}
