using System;
using System.Collections.Generic;

namespace LightManager
{
    using ProberInterfaces;
    using ECATIO;
    using ProberErrorCode;
    using EmulIOModule;

    using LogModule;

    public class LightAdmin : ILightAdmin
    {
        public bool Initialized { get; set; } = false;

        private bool IsInfo = false;

        public LightParameter LightParam { get; set; }
        //private IParam _SysParam;
        //[ParamIgnore]
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set
        //    {
        //        if (value != _SysParam)
        //        {
        //            _SysParam = value;
        //        }
        //    }
        //}
        public LightAdmin()
        {

        }

        public EventCodeEnum InitLight(List<ILightDeviceControl> lightiodevices)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                int lightMaxIntensity = Convert.ToInt32(Math.Pow(2, LightParam.LightIntensityBitSize.Value)) - 1;


                // 파라미터로 컨테이너 받아서 각 컨트롤러에 라이트 디바이스 전달
                foreach (LightChannel controller in LightParam.LightList)
                {
                    if (controller.DevIndex.Value >= 0
                        & controller.DevIndex.Value < LightParam.LightList.Count)
                    {
                        controller.FrontSystemFilePath = this.FileManager().FileManagerParam.SystemParamRootDirectory;
                        controller.InitLightController(lightiodevices[controller.DevIndex.Value], lightMaxIntensity);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                //LoggerManager.Error($err, "LightAdmin - InitLight() : Error occurred.");

            }
            
           

            RetVal = EventCodeEnum.NONE;

            return RetVal;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    //==> Autopac을 통해 IOService를 받아와서 ECATIOProvider 객체를 lightiodevices에 초기화 한다.
                    IIOManager iostate = this.IOManager();

                    List<ILightDeviceControl> lightiodevices = new List<ILightDeviceControl>();

                    foreach (IIOBase item in iostate.IOServ.IOList)
                    {
                        if (item is ECATIOProvider || item is EmulIOProvider)
                        {
                            lightiodevices.Add((ILightDeviceControl)item);
                        }
                    }

                    //==> Light Service에 Light Controller들을 초기화 한다.
                    if (lightiodevices.Count == 0)
                    {
                        retval = EventCodeEnum.PARAM_ERROR;
                    }
                    else
                    {
                        retval = InitLight(lightiodevices);
                    }

                    Initialized = true;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public int GetLight(int channelMapIdx)
        {
            return LightParam.LightList[channelMapIdx].GetLight();
        }
        public int GetLightChannelCount()
        {
            return LightParam.LightList.Count;
        }

        public void SetLight(int channelMapIdx, UInt16 intensity, EnumProberCam camType, EnumLightType lightType = EnumLightType.UNDEFINED)
        {
            LoggerManager.Debug($"[{this.GetType().Name}] SetLight() channelMapIndex: {channelMapIdx}, intensity: {intensity}", isInfo: IsInfo);
            LightParam.LightList[channelMapIdx].SetLight(intensity, camType, lightType);
        }
        public void SetLightNoLUT(int channelMapIdx, UInt16 intensity)
        {
            LightParam.LightList[channelMapIdx].SetLightNoLUT(intensity);
        }

        public void SetupLightLookUpTable(int channelMapIdx, IntListParam setupLUT)
        {
            LightParam.LightList[channelMapIdx].SetupLightLookUpTable(setupLUT);
        }
        public ILightChannel GetLightChannel(int channelMapIdx)
        {
            return LightParam.LightList[channelMapIdx];
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            IParam tmpParam = null;
            tmpParam = new LightParameter();
            tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";

            RetVal = this.LoadParameter(ref tmpParam, typeof(LightParameter));

            if (RetVal == EventCodeEnum.NONE)
            {
                LightParam = tmpParam as LightParameter;
            }

            //SysParam = LightParam;

            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            RetVal = this.SaveParameter(LightParam);
            if (RetVal != EventCodeEnum.NONE)
            {
                throw new Exception($"[{this.GetType().Name} - SaveSysParameter] Faile SaveParameter");
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public void LoadLUT()
        {
            try
            {
                foreach (LightChannel controller in LightParam.LightList)
                {
                    if (controller.DevIndex.Value >= 0
                        & controller.DevIndex.Value < LightParam.LightList.Count)
                    {
                        controller.LoadLUT();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
