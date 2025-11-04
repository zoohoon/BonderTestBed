using System;
using System.Linq;

namespace CameraModule
{
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using Vision.DisplayModule;
    using ProberErrorCode;
    using VisionParams.Camera;
    using LogModule;
    //using ProberInterfaces.ThreadSync;

    [Serializable]
    public class CameraEmul : CameraBase
    {

        public CameraEmul(CameraParameter param)
        {
            Param = param;
        }

        public override void InitGrab()
        {
            return;
        }
        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    DisplayService = new ModuleVisionDisplay();
                    curImage = new ImageBuffer();
                    CamSystemPos = new CatCoordinates();
                    CamSystemUI = new UserIndex();
                    CamSystemMI = new MachineIndex();

                    LightsChannels = Param.LightsChannels.Value.ToList<LightChannelType>();
                    CameraChannel = new CameraChannelType((EnumProberCam)Param.ChannelType.Value, Param.ChannelNumber.Value);

                    InitLights();
                    InitCameraChannels();

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "InitModule() Error occurred.");
                LoggerManager.Exception(err);

                throw err;
            }

            return retval;
        }
        public override EventCodeEnum SetLight(EnumLightType type, ushort intensity)
        {
            LightChannelType channel = LightsChannels.Find(light => light.Type.Value == type);
            if (channel != null)
            {
                channel.SetLightDevOutput(intensity);
                LightValueChanged = true;
            }
            return EventCodeEnum.NONE;
        }
        public override int SetLightNoLUT(EnumLightType type, ushort intensity)
        {
            return -1;
        }
        public override int SetupLightLookUpTable(EnumLightType type, IntListParam lightLUT)
        {
            return -1;
        }
        public override int GetLight(EnumLightType type)
        {

            int retVal = 0;

            try
            {
                LightChannelType channel = LightsChannels.Find(light => light.Type.Value == type);

                if (channel != null && channel.GetLightVal != null)
                {
                    retVal = channel.GetLightVal();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public override ImageBuffer WaitSingleShot()
        {

            ImageBuffer retimg = null;
            try
            {
                retimg = this.VisionManager().DigitizerService[Param.DigiNumber.Value].GrabberService.WaitSingleShot().Result;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retimg;

        }
        public override int SwitchCamera()
        {
            return -1;
        }
    }
}
