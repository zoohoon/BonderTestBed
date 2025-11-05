using System;
using System.Collections.Generic;
using System.Linq;

namespace MonitoringModule
{
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    //using ProberInterfaces.ThreadSync;
    using LogModule;

    public abstract class HWPartChecker : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> CurrentErrorLevel
        //LockKey _LockObj = new LockKey("HW Part Checker");
        private object _LockObj = new object();
        private EnumHWPartErrorLevel _CurrentErrorLevel;
        public EnumHWPartErrorLevel CurrentErrorLevel
        {
            get { return _CurrentErrorLevel; }
            set
            {
                //using (Locker locker = new Locker(_LockObj))
                //{
                lock (_LockObj)
                {
                    try
                    {
                    _CurrentErrorLevel = value;
                    RaisePropertyChanged();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                         throw;
                    }
                }
            }
        }
        #endregion

        public HWPartChecker ResumeDepHWPartChecker { get; set; }

        protected IMonitoringManager _MonitoringManager;

        public List<EnumAxisConstants> AxisList { get; set; }

        public HWPartChecker(IMonitoringManager monitoringManager, List<EnumAxisConstants> axisList)
        {
            try
            {
            _MonitoringManager = monitoringManager;
            AxisList = axisList;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        protected abstract EnumHWPartErrorLevel Check();
        public EnumHWPartErrorLevel GetCheckResult()
        {
            CurrentErrorLevel = Check();
            return CurrentErrorLevel;
        }
        public void SetEmergency()
        {
            SetAxisStatus(AxisList, _MonitoringManager.MotionManager().Abort);
        }
        public void LockAxis()
        {
            try
            {
            foreach (EnumAxisConstants axis in AxisList)
            {
                ProbeAxisObject axisObj = _MonitoringManager.MotionManager().GetAxis(axis);
                axisObj.LockAxis();
            }
            SetAxisStatus(AxisList, _MonitoringManager.MotionManager().Stop);//==> therer is a Checking about axis IsMoving in Stop Function
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public void UnlockAxis()
        {
            try
            {
            foreach (EnumAxisConstants axis in AxisList)
            {
                ProbeAxisObject axisObj = _MonitoringManager.MotionManager().GetAxis(axis);
                axisObj.UnLockAxis();
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public void ResumeAxis()
        {
            try
            {
            if (ResumeDepHWPartChecker != null)
                ResumeDepHWPartChecker.ResumeAxis();
            SetAxisStatus(AxisList, _MonitoringManager.MotionManager().Resume);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private void SetAxisStatus(List<EnumAxisConstants> axisList, Func<ProbeAxisObject, int> setAxisStatusDel)
        {
            try
            {
            Func<ProbeAxisObject, int> SetAxisStatusDel = setAxisStatusDel;

            //==> Z 축부터 먼저 내림
            EnumAxisConstants zAxis = axisList.FirstOrDefault(item => item == EnumAxisConstants.Z);
            if (axisList.Contains(EnumAxisConstants.Z))
            {
                SetAxisStatusDel(_MonitoringManager.MotionManager().GetAxis(EnumAxisConstants.Z));
                axisList.Remove(EnumAxisConstants.Z);
            }

            Thread.Sleep(100);

            foreach (EnumAxisConstants axis in axisList)
            {
                ProbeAxisObject axisObj = _MonitoringManager.MotionManager().GetAxis(axis);
                SetAxisStatusDel(axisObj);
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }

    public enum EnumHWPartErrorLevel { NONE, WARN, LOCK, EMERGENCY }

}
