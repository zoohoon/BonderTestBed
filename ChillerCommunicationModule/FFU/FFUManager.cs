using System;
using System.Collections.Generic;

namespace ControlModules
{
    using FFUCommModule;
    using FFUEmulModule;
    using FFUIOModule;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Temperature.FFU;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Temperature.Temp.FFU;

    public class FFUManager : INotifyPropertyChanged, IFactoryModule, IFFUManager
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion

        #region .. Property
        public int reConnectDelay = 10000;
        public int getDataDelay = 2000;
        public int moduleDelay = 500; 
        private List<IFFUModule> _FFUSerialModuleList = new List<IFFUModule>();
        public List<IFFUModule> FFUSerialModuleList
        {
            get { return _FFUSerialModuleList; }
            set
            {
                if (value != _FFUSerialModuleList)
                {
                    _FFUSerialModuleList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<IFFUModule> _FFUIOModuleList = new List<IFFUModule>(); 
        public List<IFFUModule> FFUIOModuleList
        {
            get { return _FFUIOModuleList; }
            set
            {
                if (value != _FFUIOModuleList)
                {
                    _FFUIOModuleList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFFUModule _EmulModule;
        public IFFUModule EmulModule
        {
            get { return _EmulModule; }
            set
            {
                if (value != _EmulModule)
                {
                    _EmulModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IFFUComm _FFUCommModule;
        public IFFUComm FFUCommModule
        {
            get { return _FFUCommModule; }
            set
            {
                if (value != _FFUCommModule)
                {
                    _FFUCommModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FFUSysParameter _FFUCommParam
             = new FFUSysParameter();

        public FFUSysParameter FFUCommParam
        {
            get { return _FFUCommParam; }
            set { _FFUCommParam = value; }
        }

        private bool bIsUpdating = true;
        Thread UpdateThread = null;

        public bool Initialized { get; set; } = false;

        #endregion

        public FFUManager()
        {

        }

        public FFUManager(FFUSysParameter ffusysparam)
        {
            FFUCommParam = ffusysparam;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (!Initialized)
                {
                    if (FFUCommParam.FFUModuleType == EnumFFUModuleMode.SERIAL)
                    {
                        FFUSerialModuleList.Clear();
                        FFUCommModule = new FFUSerialCommModule(FFUCommParam.Address);
                        FFUCommModule.InitModule();
                        for (int index = 0; index < FFUCommParam.NodeNum.Count; index++)
                        {
                            FFUSerialModuleList.Add(new FFUSerialModule(FFUCommModule, FFUCommParam.NodeNum[index]));
                            retVal = FFUSerialModuleList[index].InitModule();
                        }
                        FFUUpdateThread();
                    }
                    else if (FFUCommParam.FFUModuleType == EnumFFUModuleMode.IO)
                    {
                        FFUIOModuleList.Clear();
                        if (this.IOManager().IO.Inputs.DI_FFU_ONLINES != null)
                        {
                            for (int index = 0; index < this.IOManager().IO.Inputs.DI_FFU_ONLINES.Count; index++)
                            {
                                FFUIOModuleList.Add(new FFUIOModule(this.IOManager().IO.Inputs.DI_FFU_ONLINES[index].Description.Value.ToString(), index));
                                retVal = FFUIOModuleList[index].InitModule();
                            }
                            FFUUpdateThread();
                        }
                    }
                    else if (FFUCommParam.FFUModuleType == EnumFFUModuleMode.EMUL)
                    {
                        EmulModule = new FFUEmulModule();
                        retVal = EmulModule.InitModule();
                        //FFUUpdateThread();
                    }
                    Initialized = true;
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void DeInitModule()
        {
            try
            {
                if (FFUCommParam.FFUModuleType == EnumFFUModuleMode.SERIAL)
                {
                    foreach (var SerialModule in FFUSerialModuleList)
                    {
                        SerialModule?.DeInitModule();
                    }
                }
                else if (FFUCommParam.FFUModuleType == EnumFFUModuleMode.IO)
                {
                    foreach (var IOModule in _FFUIOModuleList)
                    {
                        IOModule?.DeInitModule();
                    }
                }
                else if (FFUCommParam.FFUModuleType == EnumFFUModuleMode.EMUL)
                {
                    EmulModule?.DeInitModule();
                }
                StopUpdateData();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
        private void StopUpdateData()
        {
            if (UpdateThread != null)
            {
                bIsUpdating = false;
            }
        }

        public EventCodeEnum Connect()
        {
            try
            {
                return FFUCommModule?.Connect() ?? EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return EventCodeEnum.UNDEFINED;
            }
        }

        public EventCodeEnum Disconnect()
        {
            return EventCodeEnum.NONE;
        }

        private void FFUUpdateThread()
        {
            UpdateThread = new Thread(new ThreadStart(FFUUpdateData));
            UpdateThread.Name = this.GetType().Name;
            UpdateThread.Start();
        }
        private void FFUUpdateData()
        {
            string alarmMessage;
            FFUInfo tempinfo = new FFUInfo();

            try
            {
                while (bIsUpdating)
                {
                    try
                    {
                        if (Extensions_IParam.LoadProgramFlag == true)
                        {
                            if (FFUCommParam.FFUModuleType == EnumFFUModuleMode.SERIAL)
                            {
                                if (!FFUCommModule.GetCommState())
                                {
                                    FFUCommModule?.Connect();
                                    //delay.DelayFor(reConnectDelay);
                                    Thread.Sleep(reConnectDelay);
                                }
                                else
                                {
                                    for (int index = 0; index < _FFUCommParam.NodeNum.Count; index++)
                                    {
                                        alarmMessage = FFUSerialModuleList[index].GetFFUInfo(tempinfo, _FFUCommParam.StartAddress, _FFUCommParam.NumRegisters);
                                        if (alarmMessage != "")
                                        {
                                            FFUAlarmOccurred(alarmMessage);
                                        }
                                        //delay.DelayFor(moduleDelay);
                                        Thread.Sleep(moduleDelay);
                                    }
                                }
                            }
                            else if (FFUCommParam.FFUModuleType == EnumFFUModuleMode.IO)
                            {
                                for (int index = 0; index < this.IOManager().IO.Inputs.DI_FFU_ONLINES.Count; index++)
                                {
                                    alarmMessage = FFUIOModuleList[index].GetFFUInfo(tempinfo);
                                    if (alarmMessage != "")
                                    {
                                        FFUAlarmOccurred(alarmMessage);
                                    }
                                    //delay.DelayFor(moduleDelay);
                                    Thread.Sleep(moduleDelay);
                                }
                            }
                            else if (FFUCommParam.FFUModuleType == EnumFFUModuleMode.EMUL)
                            {
                                alarmMessage = EmulModule.GetFFUInfo(tempinfo);
                                if (alarmMessage != "")
                                {
                                    FFUAlarmOccurred(alarmMessage);
                                }
                            }
                        }
                        //delay.DelayFor(getDataDelay);
                        Thread.Sleep(getDataDelay);
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void FFUAlarmOccurred(string alarmmessage)
        {
            string replaceAlarmMessage = alarmmessage.Replace("\n", ",");
            LoggerManager.Debug($"{replaceAlarmMessage}");
            this.EnvControlManager().RaiseFFUAlarm(alarmmessage);

        }
    }
}
