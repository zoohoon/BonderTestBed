using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using ProberInterfaces.Temperature.FFU;
using ProberErrorCode;
using LogModule;

namespace FFUIOModule
{
    public class FFUIOModule : IFFUModule, INotifyPropertyChanged
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

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        private int _IOIndex;
        public int IOIndex
        {
            get { return _IOIndex; }
            set { _IOIndex = value; }
        }

        private int _StageIdx;

        public int StageIdx
        {
            get { return _StageIdx; }
            set { _StageIdx = value; }
        }

        public bool Initialized { get; set; } = false;

        public IIOService IOService { get; set; }

        public FFUIOModule(string desc, int ioindex)
        {
            Description = desc;
            IOIndex = ioindex;
        }
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
                IOService = this.IOManager().IOServ;
                CurFFUInfo = new FFUInfo();
                PreFFUInfo = new FFUInfo();
                Initialized = true;
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
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
            catch (Exception)
            {

            }
            return ret;
        }

        public string GetFFUInfo(FFUInfo info)
        {
            string retVal="";
            string alarMmessage = "";
            int stageIndex = -1;
            bool DI_FFU_ONLINES = false;

            try
            {
                
                if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober
                    && SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    //GroupProber - Cell
                    stageIndex = this.LoaderController().GetChuckIndex();
                    DI_FFU_ONLINES = this.IOManager().IO.Inputs.DI_FFU_ONLINES[IOIndex].Value;
                    if (DI_FFU_ONLINES)
                    {
                        info.IOCheck = true;
                        PreFFUInfo.IOCheck = CurFFUInfo.IOCheck;
                        CurFFUInfo.IOCheck = info.IOCheck;
                        if (CurFFUInfo.IOCheck == true && PreFFUInfo.IOCheck == false && (CurFFUInfo.IOCheck != PreFFUInfo.IOCheck))
                        {
                            alarMmessage = "Cell#" + stageIndex + " FFU Alarm/Details\n" + Description + " Recorvery";
                            if (IOIndex == 0)
                            {
                                this.NotifyManager().Notify(EventCodeEnum.FFU_RECOVERY_FRONT);
                            }
                            else if (IOIndex == 1)
                            {
                                this.NotifyManager().Notify(EventCodeEnum.FFU_RECOVERY_BACK);
                            }
                        }
                    }
                    else
                    {
                        info.IOCheck = false;
                        PreFFUInfo.IOCheck = CurFFUInfo.IOCheck;
                        CurFFUInfo.IOCheck = info.IOCheck;
                        if (CurFFUInfo.IOCheck != PreFFUInfo.IOCheck)
                        {
                            alarMmessage = "Cell#" + stageIndex + " FFU Alarm/Details\n" + Description + " Error";
                            if (IOIndex == 0)
                            {
                                this.NotifyManager().Notify(EventCodeEnum.FFU_OFFLINE_FRONT);
                            }
                            else if (IOIndex == 1)
                            {
                                this.NotifyManager().Notify(EventCodeEnum.FFU_OFFLINE_BACK);
                            }
                        }
                    }
                }
                else if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober
                    && SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    //SingleProber
                    //DI_FFU_ONLINES = this.IOManager().IO.Inputs.DI_FFU_ONLINES[IOIndex].Value;
                    //if (DI_FFU_ONLINES)
                    //{
                    //    info.IOCheck = true;
                    //    PreFFUInfo.IOCheck = CurFFUInfo.IOCheck;
                    //    CurFFUInfo.IOCheck = info.IOCheck;
                    //    if (CurFFUInfo.IOCheck == true && PreFFUInfo.IOCheck == false && (CurFFUInfo.IOCheck != PreFFUInfo.IOCheck))
                    //    {
                    //        alarMmessage = "FFU Alarm/Details\n" + Description + " Recorvery";
                    //        this.NotifyManager().Notify(EventCodeEnum.FFU_RECOVERY);
                    //    }
                    //}
                    //else
                    //{
                    //    info.IOCheck = false;
                    //    PreFFUInfo.IOCheck = CurFFUInfo.IOCheck;
                    //    CurFFUInfo.IOCheck = info.IOCheck;
                    //    if (CurFFUInfo.IOCheck != PreFFUInfo.IOCheck)
                    //    {
                    //        alarMmessage = "FFU Alarm/Details\n" + Description + " Error";
                    //        this.NotifyManager().Notify(EventCodeEnum.FFU_OFFLINE);
                    //    }
                    //}
                }
                else if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                {
                    //LoaderSystem
                }
                retVal = alarMmessage;
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
