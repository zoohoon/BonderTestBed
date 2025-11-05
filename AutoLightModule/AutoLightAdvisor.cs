using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoLightModule
{
    using Autofac;
    using AutoLightControlModule;
    using ProberInterfaces;
    using ProberErrorCode;
    using LogModule;

    public class AutoLightAdvisor : IAutoLightAdvisor, IModule
    {
        
        private List<GrayLevelControl> _GrayLevelControls = new List<GrayLevelControl>();
        public List<GrayLevelControl> GrayLevelControls { get; }

        public bool Initialized { get; set; } = false;

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    _GrayLevelControls.Clear();

                    foreach (ICamera cam in this.VisionManager().GetCameras())
                    {

                        foreach (LightChannelType light in cam.LightsChannels)
                        {
                            if (cam.CameraChannel != null)
                            {
                                _GrayLevelControls.Add(new GrayLevelControl(cam.CameraChannel.Type, light.Type.Value, this.VisionManager()));
                            }
                        }
                    }

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
        public async Task<int> SetGrayLevel(EnumProberCam camType, EnumLightType lightType, int grayLevel)
        {
            try
            {
                GrayLevelControl grayLevelControl = _GrayLevelControls.Where(control => control.CameraType == camType && control.LightType == lightType).FirstOrDefault();
                if (grayLevelControl == null)
                {
                    LoggerManager.Error($"SetGrayLevel(): Find Match Gray Level Control Error CameraType : {camType.ToString()}, LightType : {lightType.ToString()}");
                    return -1;
                }

                await grayLevelControl.SetGrayLevel(grayLevel);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return 1;
        }

        public bool SetGrayLevel(EnumProberCam camType, int grayLevel)
        {
            Dictionary<int, int> graylevelTable = new Dictionary<int, int>();
            try
            {
                //==> Get Cur Camera Gray Level
                ICamera cam = this.VisionManager().GetCam(camType);
                int curCamLightSum = 0;
                foreach (var channel in cam.LightsChannels)
                {
                    curCamLightSum += cam.GetLight(channel.Type.Value);
                }

                int curCamLighAvg = curCamLightSum / cam.LightsChannels.Count;
                int intensity = curCamLighAvg;
                int diviation = 3;
                bool outOfRange = false;
                int lightChangeWidth = 10;
                bool searchDirection = GetGrayLevel(camType) > grayLevel;
                bool searchDirectionBack = searchDirection;
                int min = 255;
                int grayLevelTolerance = 10;
                while (true)
                {
                    int curCameraGrayLevel = GetGrayLevel(camType);

                    searchDirection = curCameraGrayLevel > grayLevel;
                    if (searchDirection != searchDirectionBack)
                    {
                        if (lightChangeWidth == 1)
                        {
                            int keyVal = 0;
                            foreach (var tableVal in graylevelTable)
                            {
                                int tabelGrayLevel = tableVal.Value;
                                if (Math.Abs(tabelGrayLevel - grayLevel) <= min)
                                {
                                    min = Math.Abs(tabelGrayLevel - grayLevel);
                                    keyVal = tableVal.Key;
                                }
                            }

                            if (grayLevelTolerance >= Math.Abs(min))
                            {
                                intensity = keyVal;
                                foreach (LightChannelType channel in cam.LightsChannels)
                                    cam.SetLight(channel.Type.Value, (ushort)intensity);
                            }
                            else
                                return false;

                            break;
                        }

                        lightChangeWidth /= 2;
                        if (lightChangeWidth < 1)
                            lightChangeWidth = 1;
                    }

                    int addValue = searchDirection ? -lightChangeWidth : +lightChangeWidth;

                    if (grayLevel - diviation <= curCameraGrayLevel && curCameraGrayLevel <= grayLevel + diviation)
                    {
                        LoggerManager.Debug($"[AutoLightAdvisor] Auto light sequence done (in range). " +
                            $"CamType : {camType}, Target GrayLevel : {grayLevel}, CurCam GrayLevel : {curCameraGrayLevel}, Deviation : {diviation}");
                        break;
                    }


                    if (intensity <= 0 || intensity >= 255)
                    {
                        outOfRange = true;
                        LoggerManager.Debug($"[AutoLightAdvisor] Auto light sequence done (out range). " +
                            $"CamType : {camType}, Target GrayLevel : {grayLevel}, CurCam GrayLevel : {curCameraGrayLevel}, Intensity : {intensity}");
                        break;
                    }
                    foreach (LightChannelType channel in cam.LightsChannels)
                        cam.SetLight(channel.Type.Value, (ushort)intensity);

                    if (!graylevelTable.ContainsKey(intensity))
                    {
                        curCameraGrayLevel = GetGrayLevel(camType);
                        graylevelTable.Add(intensity, curCameraGrayLevel);
                    }                    
                    intensity += addValue;
                    searchDirectionBack = searchDirection;

                    //_delays.DelayFor(1);
                    System.Threading.Thread.Sleep(1);
                }

                if (outOfRange)
                    return false;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            finally
            {
                graylevelTable.Clear();
            }

            return true;
        }

        public int GetGrayLevel(EnumProberCam camType, object assembly = null)
        {
            try
            {
                GrayLevelControl grayLevelControl = _GrayLevelControls.Where(control => control.CameraType == camType).FirstOrDefault();
                if (grayLevelControl == null)
                {
                    LoggerManager.Error($"SetGrayLevel(): Find Match Gray Level Control Error CameraType : {camType.ToString()}");
                    return -1;
                }

                if (assembly != null)
                    return grayLevelControl.GetGrayLevel(assembly);
                else
                    return grayLevelControl.GetGrayLevel(System.Reflection.Assembly.GetCallingAssembly());
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public int SetupGraylLevelLUT(EnumProberCam camType, EnumLightType lightType)
        {
            try
            {
                GrayLevelControl grayLevelControl = _GrayLevelControls.Where(control => control.CameraType == camType && control.LightType == lightType).FirstOrDefault();
                if (grayLevelControl == null)
                {
                    LoggerManager.Error($"SetGrayLevel(): Find Match Gray Level Control Error CameraType : {camType.ToString()}, LightType : {lightType.ToString()}");
                    return -1;
                }

                grayLevelControl.SetupGraylLevelLUT();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return 1;
        }

        public EventCodeEnum InitModule(IContainer container, object param)
        {
            throw new NotImplementedException();
        }
    }
}
