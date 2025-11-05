using System;
using System.Linq;

namespace CameraModule
{
    using ProberInterfaces;
    using Autofac;
    using System.Xml.Serialization;
    using Vision.DisplayModule;
    using ProberInterfaces.Param;
    using ProberErrorCode;
    using VisionParams.Camera;
    using LogModule;
    using Newtonsoft.Json;

    //using ProberInterfaces.ThreadSync;

    [Serializable]
    //[XmlInclude(typeof(Camera))]
    public class Camera : CameraBase
    {
      

        #region // Grab continous delegate
        public delegate void GrabContDelegate();
        [XmlIgnore, JsonIgnore]
        public GrabContDelegate GrabCont;
        #endregion

        public enum EnumPixelDirection
        {
            REVERSE = -1,
            FORWARD = 1
        }

        public Camera()
        {

        }
        public Camera(CameraParameter param)
        {
            Param = param;
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

                    if(Param.LightsChannels.Value != null)
                    {
                        LightsChannels = Param.LightsChannels.Value.ToList<LightChannelType>();
                        retval = InitLights();

                        if (retval != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"InitLights() Failed");
                        }
                    }

                    CameraChannel = new CameraChannelType((EnumProberCam)Param.ChannelType.Value, Param.ChannelNumber.Value);
                    retval = InitCameraChannels();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"InitLights() Failed");
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

        public override void InitGrab()
        {
            try
            {
                IVisionManager vm;

                if (this.GetContainer().TryResolve<IVisionManager>(out vm) == true)
                {
                    GrabCont = () =>
                    {
                        vm.DigitizerService[Param.DigiNumber.Value].GrabberService.ContinuousGrab();
                    };
                }
                else
                {
                    GrabCont = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int GetLight(EnumLightType type)
        {
            int retVal = -1;
            try
            {
                LightChannelType channel = LightsChannels.Find(light => light.Type.Value == type);
                if(channel != null)
                {
                    retVal = channel.GetLightVal();
                }
                
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "GetLight() Error occurred.");
                LoggerManager.Exception(err);

                throw;
            }
            return retVal;
        }

        public override EventCodeEnum SetLight(EnumLightType type, UInt16 intensity)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LightValueChanged = false;
                LightChannelType channel = LightsChannels.Find(light => light.Type.Value == type);
                if (channel != null)
                {
                    channel.SetLightDevOutput(intensity);
                    LightValueChanged = true;
                    //if (this.StageSupervisor().IsRegisteDataViewModelContainer() & this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.RUNNING & this.LotOPModule().ModuleState.GetState() != ModuleStateEnum.PAUSING)
                        //this.StageDataManager().ServiceCallBack.PNPLightJogUpdated(type, intensity);
                }
                else
                {
                    LoggerManager.Debug($"{type} channel not supported on {this.Param.ChannelType} camera.");
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (NullReferenceException err)
            {
                //LoggerManager.Error($string.Format("SetLight({0}, {1}): Error occurred. Type not defined. Err = {2}",type, intensity, nullerr.Message));
                LoggerManager.Exception(err);

                throw err;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("SetLight({0}, {1}): Error occurred. Err = {2}", type, intensity, err.Message));
                LoggerManager.Exception(err);

                throw;
            }
            return retVal;
        }

        public override int SetLightNoLUT(EnumLightType type, UInt16 intensity)
        {
            int retVal = -1;
            try
            {
                LightChannelType channel = LightsChannels.Find(light => light.Type.Value == type);
                channel.SetLightDevOutputNoLUT(intensity);

                retVal = 1;
            }
            catch (NullReferenceException err)
            {
                //LoggerManager.Error($string.Format("SetLightNoLUT({0}, {1}): Error occurred. Type not defined. Err = {2}", type, intensity, nullerr.Message));
                LoggerManager.Exception(err);

                retVal = -1;

                throw err;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("SetLightNoLUT({0}, {1}): Error occurred. Err = {2}", type, intensity, err.Message));
                LoggerManager.Exception(err);

                retVal = -1;
                throw;
            }

            return retVal;
        }

        public override int SetupLightLookUpTable(EnumLightType type, IntListParam lightLUT)
        {
            int retVal = -1;
            try
            {
                LightChannelType channel = LightsChannels.Find(light => light.Type.Value == type);
                channel.SetupLUT(lightLUT);
                retVal = 1;
            }
            catch (NullReferenceException err)
            {
                //LoggerManager.Error($string.Format("SetupLookUpTable({0}): Error occurred. Type not defined. Err = {2}", type, nullerr.Message));
                LoggerManager.Exception(err);

                retVal = -1;
                throw err;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("SetupLookUpTable({0}): Error occurred. Err = {2}", type, err.Message));
                LoggerManager.Exception(err);

                retVal = -1;
                throw;
            }

            return retVal;
        }


        public override int SwitchCamera()
        {
            int retVal = -1;
            try
            {
                CameraChannel.SetCameraChannelDevOutput();
                retVal = 1;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("SwitchCamera(): Error occurred. Err = {0}", err));
                LoggerManager.Exception(err);

                retVal = -1;
                throw;
            }

            return retVal;
        }

        public override ImageBuffer WaitSingleShot()
        {
            return null;
        }
    }
}
