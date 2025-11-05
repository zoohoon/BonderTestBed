using System;
using System.Collections.Generic;
using LoaderBase;
using ProberInterfaces;
using System.Windows;

namespace LoaderCore
{
    using CameraScanCassetteStates;
    using LoaderParameters;
    using LogModule;

    public class CameraScanCassette : ILoaderProcessModule
    {
        public CameraScanCassetteState StateObj { get; set; }

        public ScanProcParam Param { get; private set; }

        public int PlaceDownTriedCount { get; set; }

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
                mypa.UseScanable is IScanCameraModule;
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

        internal CameraScanCassetteData Data { get; set; }

    }

    public class CameraScanCassetteData
    {
        public CassetteSlot1AccessParam Slot1AccessParam { get; set; }

        public ScanCameraParam ScanParam { get; set; }

        public List<ISlotModule> SlotsOrderByBottom { get; set; }

        public List<ISlotModule> SlotsOrderByTop { get; set; }

        public EnumProberCam Cam { get; set; }

        public System.Windows.Size GrabSize { get; set; }

        public Rect ROI { get; set; }

        public Dictionary<ISlotModule, ImageBuffer> UpScanDataDic { get; set; }

        public Dictionary<ISlotModule, ImageBuffer> DownScanDataDic { get; set; }

        public string DumpPrefixStr { get; set; }
    }
}
