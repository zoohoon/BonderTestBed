using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.Temperature;
using ProberInterfaces.Temperature.Chiller;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Temperature.Temp.Chiller
{
    public class ChillCommEmul : IChillerComm, INotifyPropertyChanged
    {
        #region ==> NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        public ChillerModule Module { get; set; }
        private int ChillerIndeex { get; set; }

        public ChillCommEmul(ChillerModule module)
        {
            Module = module;
            ChillerIndeex = Module?.ChillerInfo?.Index ?? -1;
        }

        private double _SV;
        public double SV
        {
            get { return _SV; }
            set
            {
                if (value != _SV)
                {
                    _SV = value;
                    NotifyPropertyChanged("SV");
                }
            }
        }
        private double _PV;
        public double PV
        {
            get { return _PV; }
            set
            {
                if (value != _PV)
                {
                    _PV = value;
                    NotifyPropertyChanged("PV");
                }
            }
        }
        private int _PumpPressure;
        public int PumpPressure
        {
            get { return _PumpPressure; }
            set
            {
                if (value != _PumpPressure)
                {
                    _PumpPressure = value;
                    NotifyPropertyChanged("PumpPressure");
                }
            }
        }

        Thread tempUpdateThread;
        bool bStopUpdate = false;

        private bool _IsActive;
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                if (value != _IsActive)
                {
                    _IsActive = value;
                    NotifyPropertyChanged("IsActive");
                }
            }
        }

        private bool _IsCirculation;
        public bool IsCirculation
        {
            get { return _IsCirculation; }
            set
            {
                if (value != _IsCirculation)
                {
                    _IsCirculation = value;
                    NotifyPropertyChanged("IsCirculation");
                }
            }
        }


        private bool _IsConnected = false;

        int ser = 0;
        public EventCodeEnum Connect(string address, int port)
        {
            ser = port;
            _IsConnected = true;
            tempUpdateThread = new Thread(new ThreadStart(TempEmulMethod));
            tempUpdateThread.Start();
            return EventCodeEnum.NONE;
        }

        private void TempEmulMethod()
        {
            Random random = new Random();
            double mv = 0.0;
            while (bStopUpdate == false)
            {
                if(IsActive == true)
                {
                    mv = random.Next(1,10) * 0.1;
                    if (SV != PV)
                    {
                        if (SV > PV)
                        {
                            PV += mv;
                        }
                        else
                        {
                            PV -= 0.1;
                        }
                    }
                    if (Math.Abs(PV - SV) < 0.5)
                    {
                        PV = SV;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        public void DisConnect()
        {
            try
            {
                _IsConnected = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Dispose()
        {
            bStopUpdate = true;
            if(tempUpdateThread != null)
            {
                tempUpdateThread.Join();
                tempUpdateThread = null;
            }
        }

        public EnumCommunicationState GetCommState(byte subModuleIndex = 0x00)
        {
            if (_IsConnected)
                return EnumCommunicationState.CONNECTED;
            else
                return EnumCommunicationState.DISCONNECT;
        }

        public ICommunicationMeans GetCommunicationObj()
        {
            return null;
        }
        public object GetCommLockObj()
        {
            return new object();
        }
        public int GetErrorReport(byte subModuleIndex = 0x00)
        {
            return 0;
        }


        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum CheckCanUseChiller(double sendVal, int stageindex = -1, bool offvalve = false, bool forcedSetValue = false)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            return retVal;
        }
        public EventCodeEnum SetTargetTemp(double sendVal, TempValueType sendTempValueType, byte subModuleIndex = 0x00, bool forcedSetValue = false)
        {
            if(SV != sendVal)
            {
                LoggerManager.Debug($"Set chiller target temp as {sendVal:0.0}℃");
            }
            SV = sendVal;
            if (IsActive == false)
                SetTempActiveMode(true);
            return EventCodeEnum.NONE;
        }

        public void SetTempActiveMode(bool bValue, byte subModuleIndex = 0x00)
        {
            if(IsActive == false & bValue == true)
            {
                LoggerManager.Debug($"Chiller activated.");
            }
            else if(IsActive == true & bValue == false)
            {
                LoggerManager.Debug($"Chiller Inactivated.");
            }
            IsActive = bValue;
        }

        public void SetSetTempPumpSpeed(int iValue, byte subModuleIndex = 0x00)
        {
            LoggerManager.Debug($"Set pump pressure as {iValue}");
            PumpPressure = iValue;
        }

        public void SetOperatingLock(bool bOperatinglock, bool bWatchdogBehavior, byte subModuleIndex = 0x00)
        {
            return;
        }

        public double GetSetTempValue(byte subModuleIndex = 0x00)
        {
            return SV;

        }
        public double GetInternalTempValue(byte subModuleIndex = 0x00)
        {
            return PV;
        }
        public double GetReturnTempVal(byte subModuleIndex = 0x00)
        {
            return PV;
        }

        public int GetPumpPressureVal(byte subModuleIndex = 0x00)
        {
            return PumpPressure;
        }


        public double GetProcessTempVal(byte subModuleIndex = 0x00)
        {
            return PV;
        }


        public int GetPumpSpeed(byte subModuleIndex = 0x00)
        {
            return PumpPressure;
        }

        public double GetMinSetTemp(byte subModuleIndex = 0x00)
        {
            return -90.0;
        }

        public double GetMaxSetTemp(byte subModuleIndex = 0x00)
        {
            return 200.0;
        }

        public int GetSetTempPumpSpeed(byte subModuleIndex = 0x00)
        {
            return PumpPressure;
        }

        public int GetWarningMessage(byte subModuleIndex = 0x00)
        {
            return 0;
        }

        public int GetStatusOfThermostat(byte subModuleIndex = 0x00)
        {
            return 0;
        }

        public bool IsAutoPID(byte subModuleIndex = 0x00)
        {
            return false;
        }

        public bool IsTempControlProcessMode(byte subModuleIndex = 0x00)
        {
            return false;
        }

        public bool IsTempControlActive(byte subModuleIndex = 0x00)
        {
            return IsActive;
        }

        public (bool, bool) GetProcTempActValSetMode(byte subModuleIndex = 0x00)
        {
            return (true, IsActive);
        }

        public int GetSerialNumLow(byte subModuleIndex = 0x00)
        {
            //return 0x01;
            return ser;
        }

        public int GetSerialNumHigh(byte subModuleIndex = 0x00)
        {
            return 0x00;
        }

        public int GetSerialNumber(byte subModuleIndex = 0x00)
        {
            return ser;
        }

        public bool Prev_IsCirculationActive { get; set; }


        public bool IsCirculationActive(byte subModuleIndex = 0x00)
        {
            if (Prev_IsCirculationActive != IsCirculation)
            {
                LoggerManager.Debug($"[Chiller #{ChillerIndeex}] IsCirculationActive(): change {Prev_IsCirculationActive} to {IsCirculation}");
                Prev_IsCirculationActive = IsCirculation;
            }
            return IsCirculation;
        }

        public void SetCircuationActive(bool bValue, byte subModuleIndex = 0x00)
        {
            string mode;
            if (bValue)
            {
                mode = "external";
            }
            else
            {
                mode = "internal";
            }

            LoggerManager.Debug($"[CHI][Chiller #{ChillerIndeex}] SetCircuationActive set to {mode}");
            IsCirculation = bValue;
            return;
        }

        public (bool, bool) IsOperatingLock(byte subModuleIndex = 0x00)
        {
            return (false, false);
        }

        public double GetUpperAlramInternalLimit(byte subModuleIndex = 0x00)
        {
            return 500;
        }

        public double GetLowerAlramInternalLimit(byte subModuleIndex = 0x00)
        {
            return -151;
        }

        public double GetUpperAlramProcessLimit(byte subModuleIndex = 0x00)
        {
            return 500;
        }

        public double GetLowerAlramProcessLimit(byte subModuleIndex = 0x00)
        {
            return -151;
        }

        public int GetCurrentPower(byte subModuleIndex = 0x00)
        {
            if(SV > 25.0)
            {
                return 1000;
            }
            else
            {
                return -1000;
            }
        }

        public double GetExtMoveVal(byte subModuleIndex = 0x00)
        {
            return 0;
        }


    }
}
