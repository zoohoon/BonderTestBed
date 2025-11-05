using System;
using System.Collections.Generic;

namespace Temperature.Temp.FFU
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Temperature.FFU;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    public class FFUSerialModule : IFFUModule, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public bool Initialized { get; set; } = false;

        private IFFUComm _FFUSerialComm;

        public IFFUComm FFUSerialComm
        {
            get { return _FFUSerialComm; }
            set { _FFUSerialComm = value; }
        }

        private FFUInfo _PreFFUInfo;

        public FFUInfo PreFFUInfo
        {
            get { return _PreFFUInfo; }
            set { _PreFFUInfo = value; }
        }

        private FFUInfo _CurFFUInfo;

        public FFUInfo CurFFUInfo
        {
            get { return _CurFFUInfo; }
            set { _CurFFUInfo = value; }
        }

        private int _FFUnodeNum;

        public int FFUnodeNum
        {
            get { return _FFUnodeNum; }
            set { _FFUnodeNum = value; }
        }

        private int _StageIdx;

        public int StageIdx
        {
            get { return _StageIdx; }
            set { _StageIdx = value; }
        }

        public List<EventCodeEnum> errorCodes = new List<EventCodeEnum>
        {
            EventCodeEnum.FFU_DIRECTION_MOTOR_ROTATION_ERROR,
            EventCodeEnum.FFU_PFC_ERROR_WITHOUT_VOLTAGE,
            EventCodeEnum.FFU_MOTOR_OVER_CURRENT,
            EventCodeEnum.FFU_SHORT_CURRENT,
            EventCodeEnum.FFU_MOTOR_SENSOR_ERROR,
            EventCodeEnum.FFU_MOTOR_DID_LOCK,
            EventCodeEnum.FFU_OVER_CURRENT,
            EventCodeEnum.FFU_IPM_MODULE_ERROR,
            EventCodeEnum.FFU_OVER_CURRENT_PROTECTION,
            EventCodeEnum.FFU_TEMPERATURE_IPM_ERROR,
            EventCodeEnum.FFU_TEMPERATURE_CONTROLLER_ERROR,
            EventCodeEnum.FFU_TEMPERATURE_MOTER_ERROR,
            EventCodeEnum.FFU_PFC_OVER_VOLTAGE,
            EventCodeEnum.FFU_PFC_UNDER_VOLTAGE,
            EventCodeEnum.FFU_AC_FREQUENCY_ERROR,
            EventCodeEnum.FFU_AC_POWER_INPUT_ERROR,
        };

        #region .. Creator & Init & DeInit
        public FFUSerialModule()
        {
        }
        public FFUSerialModule(IFFUComm ffucommmodule, int nodenum)
        {
            FFUSerialComm = ffucommmodule;
            FFUnodeNum = nodenum;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (!Initialized)
                {
                    CurFFUInfo = new FFUInfo();
                    PreFFUInfo = new FFUInfo();


                    Initialized = true;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public void DeInitModule()
        {
            LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            this.Dispose();
            Initialized = false;
        }

        #endregion

        public EventCodeEnum DisConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                FFUSerialComm.DisConnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private bool isDisposed = false;
        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool isDispossing)
        {
            if (!this.isDisposed)
            {
                if (isDispossing)
                {
                    FFUSerialComm?.Dispose();
                }
            }
        }

        public string GetFFUInfo(FFUInfo info, ushort startaddress, ushort numregisters)
        {
            string retVal = "";
            string alarmMessage = "";
            List<EventCodeEnum> retErrorCodes = new List<EventCodeEnum>();

            try
            {
                ushort[] result = FFUSerialComm.GetData(FFUnodeNum, startaddress, numregisters);
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                {
                    //LoaderSystem
                    if (result != null)
                    {
                        info.CurSpeed = result[0];
                        info.SetSpeed = result[1];
                        info.Alarm = result[2];
                        info.STS = result[3];

                        PreFFUInfo.Alarm = CurFFUInfo.Alarm;
                        CurFFUInfo.Alarm = info.Alarm;
                        if (CurFFUInfo.Alarm != 0 && PreFFUInfo.Alarm == 0 && (CurFFUInfo.Alarm != PreFFUInfo.Alarm))
                        {
                            string alarmBinary = Convert.ToString(info.Alarm, 2).PadLeft(16, '0');
                            for (int index = 0; index < alarmBinary.Length; index++)
                            {
                                if (alarmBinary[index] == '1')
                                {
                                    retErrorCodes.Add(errorCodes[index]);
                                }
                            }
                            alarmMessage = "Loader FFU#" + FFUnodeNum + " Alarm/Details\n";

                            for (int i = 0; i < retErrorCodes.Count; i++)
                            {
                                alarmMessage += retErrorCodes[i].ToString() + "\n";
                                this.NotifyManager().Notify(retErrorCodes[i]);
                            }
                        }
                        else if (CurFFUInfo.Alarm == 0 && PreFFUInfo.Alarm == -1 && (CurFFUInfo.Alarm != PreFFUInfo.Alarm))
                        {
                            string[] splitMsg = alarmMessage.Split(new string[] { "/" }, StringSplitOptions.None);
                            splitMsg[0] += "/Details\n" + EventCodeEnum.FFU_AC_RECOVERY;
                            alarmMessage = splitMsg[0];
                            this.NotifyManager().Notify(EventCodeEnum.FFU_AC_RECOVERY);
                        }
                    }
                    else
                    {
                        info.Alarm = -1;
                        PreFFUInfo.Alarm = CurFFUInfo.Alarm;
                        CurFFUInfo.Alarm = info.Alarm;
                        if(CurFFUInfo.Alarm != PreFFUInfo.Alarm)
                        {
                            alarmMessage = "Loader FFU#" + FFUnodeNum + " Alarm/Details\n" + EventCodeEnum.FFU_TIMEOUT_ERROR.ToString();
                            this.NotifyManager().Notify(EventCodeEnum.FFU_TIMEOUT_ERROR);
                        }
                    }
                }
                else if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober
                    && SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    //GroupProber
                }
                else if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober
                    && SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    //SingleProber
                }
                retVal = alarmMessage;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public string GetFFUInfo(FFUInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
