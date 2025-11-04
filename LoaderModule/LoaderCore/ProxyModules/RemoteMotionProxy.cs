using LoaderBase;
using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using ProberErrorCode;
using ProberInterfaces;
using LogModule;
using Motion;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LoaderCore.ProxyModules
{
    public class RemoteMotionProxy : IMotionManagerProxy, IHasSysParameterizable, IFactoryModule, IModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        bool bStopUpdateThread;
        Thread UpdateThread;
        ManualResetEvent mre = new ManualResetEvent(true);

        public Autofac.IContainer Container { get; set; }
        public ILoaderModule Loader => Container.Resolve<ILoaderModule>();
        public ProbeAxes LoaderAxes => RemoteAxes;
        public IMotionBase MotionProvider { get; set; }
        private LoaderAxes _RemoteAxes;
        public LoaderAxes RemoteAxes
        {
            get { return _RemoteAxes; }
            set
            {
                if (value != _RemoteAxes)
                {
                    _RemoteAxes = value;
                    RaisePropertyChanged();
                }
            }
        }
        public InitPriorityEnum InitPriority => InitPriorityEnum.LEVEL3;
        public bool Initialized { get; set; } = false;
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new LoaderAxes();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(LoaderAxes));
                tmpParam.Owner = this;
                if (RetVal == EventCodeEnum.NONE)
                {
                    RemoteAxes = tmpParam as LoaderAxes;
                }
            }
            catch (Exception err)
            {
                throw err;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            RetVal = this.SaveParameter(RemoteAxes);

            return RetVal;
        }
        public EventCodeEnum AbsMove(EnumAxisConstants axis, double pos)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum AbsMove(EnumAxisConstants axis, double pos, double vel, double acc)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum AbsMoveAsync(EnumAxisConstants axis, double pos, double vel, double acc)
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {
            bStopUpdateThread = true;
            if(UpdateThread != null)
            {
                UpdateThread.Join();
            }
            MotionProvider.DeInitMotionService();
        }

        public ProbeAxisObject GetAxis(EnumAxisConstants axis)
        {
            //RemoteAxes.ProbeAxisProviders[]
            return RemoteAxes?
                    .ProbeAxisProviders
                    .Where(item => item is ProbeAxisObject && (item as ProbeAxisObject).AxisType.Value == axis)
                    .FirstOrDefault() as ProbeAxisObject;
        }

        public bool GetIOHome(ProbeAxisObject axis)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum HomeMove()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum errCode = EventCodeEnum.UNDEFINED;
            try
            {
                MotionProvider = new ADSRemoteMotion();
                
                errCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return errCode;
        }

        public EventCodeEnum InitModule(Autofac.IContainer container)
        {
            EventCodeEnum errCode = EventCodeEnum.UNDEFINED;
            try
            {
                Container = container;
                bStopUpdateThread = false;
                UpdateThread = new Thread(new ThreadStart(UpdateMotionProc));
                UpdateThread.Name = this.GetType().Name;

                UpdateThread.Start();
                Initialized = true;
                errCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return errCode;
        }
        ~RemoteMotionProxy()
        {
            DeInitModule();
        }
        private void UpdateMotionProc()
        {
            try
            {
                while (bStopUpdateThread == false)
                {
                    try
                    {
                        Parallel.For(0, RemoteAxes.ProbeAxisProviders.Count, new ParallelOptions { MaxDegreeOfParallelism = 3 }, i =>
                        {
                            RemoteAxes.ProbeAxisProviders[i].Status.Pulse.Actual =
                                MotionProvider.AxisStatusList[RemoteAxes.ProbeAxisProviders[i].AxisIndex.Value].Pulse.Actual;
                            RemoteAxes.ProbeAxisProviders[i].Status.Pulse.Command =
                                MotionProvider.AxisStatusList[RemoteAxes.ProbeAxisProviders[i].AxisIndex.Value].Pulse.Command;
                            RemoteAxes.ProbeAxisProviders[i].Status.StatusCode =
                                MotionProvider.AxisStatusList[RemoteAxes.ProbeAxisProviders[i].AxisIndex.Value].StatusCode;
                            RemoteAxes.ProbeAxisProviders[i].Status.RawPosition.Actual =
                              RemoteAxes.ProbeAxisProviders[i].PtoD(RemoteAxes.ProbeAxisProviders[i].Status.Pulse.Actual);
                            RemoteAxes.ProbeAxisProviders[i].Status.RawPosition.Command =
                              RemoteAxes.ProbeAxisProviders[i].PtoD(RemoteAxes.ProbeAxisProviders[i].Status.Pulse.Command);

                            RemoteAxes.ProbeAxisProviders[i].Status.Position.Actual = RemoteAxes.ProbeAxisProviders[i].Status.RawPosition.Actual;
                        });

                        Thread.Sleep(150);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Debug($"Exception occurred. Err. = {err.Message}");
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("UpdateIOProc(): Error occurred while update io proc. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum NotchFindMove(EnumAxisConstants axis, EnumMotorDedicatedIn input)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum RelMove(EnumAxisConstants axis, double pos)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum RelMove(EnumAxisConstants axis, double pos, double vel, double acc)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum RelMoveAsync(EnumAxisConstants axis, double pos)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum RelMoveAsync(EnumAxisConstants axis, double pos, double vel, double acc)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SaveLoaderAxesObject()
        {
            return SaveSysParameter();
        }

        public EventCodeEnum StartScanPosCapt(EnumAxisConstants axis, EnumMotorDedicatedIn MotorDedicatedIn)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum StopScanPosCapt(EnumAxisConstants axis)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum WaitForAxisMotionDone(EnumAxisConstants axis)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum WaitForAxisMotionDone(EnumAxisConstants axis, Func<bool> GetSourceLevel, bool resumeLevel, long timeout = 0)
        {
            throw new NotImplementedException();
        }
    }

}
