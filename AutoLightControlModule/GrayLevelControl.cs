using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoLightControlModule
{
    using LogModule;
    using ProberInterfaces;

    public class GrayLevelControl
    {
        private EnumProberCam _CameraType;
        public EnumProberCam CameraType
        {
            get { return _CameraType; }
            set { _CameraType = value; }
        }

        private EnumLightType _LightType;

        public EnumLightType LightType
        {
            get { return _LightType; }
            set { _LightType = value; }
        }
        //==> Gray Level 한계 범위
        private const int _Diviation = 5;

        private IVisionManager _VisionManager;
        private ICamera _Camera;

        public GrayLevelControl(EnumProberCam cameraType, EnumLightType lightType, IVisionManager visionManager)
        {
            try
            {
                _CameraType = cameraType;
                _LightType = lightType;
                _VisionManager = visionManager;
                _Camera = _VisionManager.GetCam(_CameraType);
                //_Camera.SwitchCamera();

                LightChannelType light = _Camera.LightsChannels.Find(channel => channel.Type.Value == _LightType);
                if (light == null)
                {
                    throw new ArgumentNullException();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Task<int> SetGrayLevel(int grayLevel)
        {
            int retVal = -1;
            try
            {
                //_Camera.SwitchCamera();
                this._VisionManager.SwitchCamera(_Camera.Param, this);

                int backupGrayLevel = GetGrayLevel();
                int addValue = backupGrayLevel > grayLevel ? -1 : +1;
                int intensity = grayLevel;

                while (true)
                {
                    int curGrayLevel = GetGrayLevel();
                    //==> grayLevel - ? < curGrayLevel < grayLevel + ?
                    if (grayLevel - _Diviation < curGrayLevel && curGrayLevel < grayLevel + _Diviation)
                        break;
                    if (intensity < 0 || intensity > 255)
                        break;

                    _Camera.SetLight(_LightType, (ushort)intensity);
                    intensity += addValue;

                    //_delays.DelayFor(1);
                     System.Threading.Thread.Sleep(1);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<int>(retVal);
        }

        //==> 조명 세기를 변경함에 따른 Gray Level을 List로 반환함
        public void SetupGraylLevelLUT()
        {
            try
            {
                IntListParam graylLevelLUT = new IntListParam(256);
                int maxLightIntensity = 4095;//==> !!! [HARDCODING] !!

                _VisionManager.StopGrab(_CameraType);
                OffOtherLight();

                for (ushort i = 0; i < maxLightIntensity + 1; i += 5)
                {
                    //==> Change Light Intensity
                    _Camera.SetLightNoLUT(_LightType, i);
                    int grayLevel = GetGrayLevel();
                    graylLevelLUT[grayLevel] = i;
                }

                _Camera.SetupLightLookUpTable(_LightType, graylLevelLUT);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //==> 카메라가 잡고 있는 이미지의 Gray Level 획득
        public int GetGrayLevel(object assembly = null)
        {
            _VisionManager.SetCaller(_CameraType, assembly);
            ImageBuffer imageBuffer = _VisionManager.SingleGrab(_CameraType, assembly);
            _VisionManager.VisionProcessing.GetGrayLevel(ref imageBuffer);

            return imageBuffer.GrayLevelValue;
        }

        //==> Camera에 달린 조명들 중 _LightType이 아닌 다른 조명들은 끈다
        private void OffOtherLight()
        {
            try
            {
                List<LightChannelType> otherLightes = _Camera.LightsChannels.FindAll(channel => channel.Type.Value != _LightType);
                foreach (LightChannelType light in otherLightes)
                    _Camera.SetLight(light.Type.Value, 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
