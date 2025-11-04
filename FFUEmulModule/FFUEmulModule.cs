using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using ProberInterfaces.Temperature.FFU;
using ProberErrorCode;
using LogModule;


namespace FFUEmulModule
{
    public class FFUEmulModule : IFFUModule, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public IFFUParameter FFUParam { get; set; }
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

        private int _StageIdx;

        public int StageIdx
        {
            get { return _StageIdx; }
            set { _StageIdx = value; }
        }

        public bool Initialized { get; set; } = false;

        public void DeInitModule()
        {
            LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            Initialized = false;
        }
        public EventCodeEnum InitModule()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                CurFFUInfo = new FFUInfo();
                PreFFUInfo = new FFUInfo();
                Initialized = true;
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public EventCodeEnum DisConnect()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public string GetFFUInfo(FFUInfo info)
        {
            string retVal = "";
            string alarmMessage = "";
            List<EventCodeEnum> retErrorCodes = new List<EventCodeEnum>();
            int stageIndex = -1;

            try
            {
                if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                {
                    stageIndex = this.LoaderController().GetChuckIndex();
                }
                ushort[] result = { 499, 500, 25, 1 };  //CurSpeed, SetSpeed, Alarm, Speed
                if (stageIndex == -1)
                {
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
                            alarmMessage = "(EMUL)Loader FFU# Alarm/Details\n";

                            for (int i = 0; i < retErrorCodes.Count; i++)
                            {
                                alarmMessage += retErrorCodes[i].ToString() + "\n";
                                this.NotifyManager().Notify(retErrorCodes[i]);
                            }
                        }
                    }
                }
                retVal = alarmMessage;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public string GetFFUInfo(FFUInfo info, ushort startaddress, ushort numregisters)
        {
            throw new NotImplementedException();
        }
    }
}
