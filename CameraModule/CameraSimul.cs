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
    using ProberInterfaces.Vision;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    [Serializable]
    public class CameraSimul : CameraBase
    {

        public CameraSimul(CameraParameter param)
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
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override EventCodeEnum SetLight(EnumLightType type, ushort intensity)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                LightChannelType channel = LightsChannels.Find(light => light.Type.Value == type);

                if (channel != null)
                {
                    channel.SetLightDevOutput(intensity);
                    LightValueChanged = true;
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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
            int retval = -1;

            try
            {
                (this.VisionManager().DigitizerService[Param.DigiNumber.Value].GrabberService as IGrabberSimul).SwitchCamera();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public override async void ImageGrabbed(ImageBuffer img)
        {
            double xpos = 0.0;
            double ypos = 0.0;
            double zpos = 0.0;
            double tpos = 0.0;

            Task aquireSemaphoreTask = null;

            try
            {
                if (img != null)
                {
                    this.MotionManager()?.GetRefPos(ref xpos, ref ypos, ref zpos, ref tpos);

                    MachineCoordinate machinecoord = new MachineCoordinate(xpos, ypos, zpos, tpos);
                    CatCoordinates camcoord = new CatCoordinates();

                    camcoord = this.CoordinateManager()?.StageCoordConvertToUserCoord(Param.ChannelType.Value);

                    CancellationToken cancellationToken = new CancellationToken();
                    aquireSemaphoreTask = semaphore.WaitAsync(cancellationToken);
                    await aquireSemaphoreTask;

                    img.CamType = Param.ChannelType.Value;
                    img.MachineCoordinates = machinecoord;
                    img.CatCoordinates = camcoord;
                    img.RatioX.Value = Param.RatioX.Value;
                    img.RatioY.Value = Param.RatioY.Value;
                    img.Band = Param.Band.Value;
                    img.MachineIdx = GetCurCoordMachineIndex();
                    img.UserIdx = GetCurCoordIndex();
                    img.UpdateOverlayFlag = UpdateOverlayFlag;

                    img.CopyTo(curImage);

                    if (EnableGetFocusValueFlag == Visibility.Visible)
                    {
                        img.FocusLevelValue = this.VisionManager().GetFocusValue(img);
                        img.CopyTo(CamCurImage);
                    }

                    if (DrawDisplayDelegate != null)
                    {
                        GrabCoord = new MachineCoordinate(
                            Math.Round(machinecoord.GetX(), 0),
                            Math.Round(machinecoord.GetY(), 0),
                            Math.Round(machinecoord.GetZ(), 0),
                            Math.Round(machinecoord.GetT(), 0));

                        if (GrabCoord.GetX() != PreGrabCoord.GetX() ||
                            GrabCoord.GetY() != PreGrabCoord.GetY() ||
                            GrabCoord.GetZ() != PreGrabCoord.GetZ() ||
                            GrabCoord.GetT() != PreGrabCoord.GetT() || UpdateOverlayFlag || DisplayService.needUpdateOverlayCanvasSize
                                )
                        {

                            if (DrawDisplayDelegate != null)
                            {
                                await Application.Current.Dispatcher.BeginInvoke((Action) delegate
                                {
                                    try
                                    {
                                        if (DrawDisplayDelegate != null)
                                        {
                                            img.DrawOverlayContexts.Clear();

                                            DrawDisplayDelegate(img, this);
                                            DisplayService.Draw(img);
                                            UpdateOverlayFlag = true;
                                        }

                                        if (DisplayService.needUpdateOverlayCanvasSize)
                                            DisplayService.needUpdateOverlayCanvasSize = false;
                                    }
                                    catch (Exception err)
                                    {
                                        LoggerManager.Exception(err);
                                    }
                                });

                                img.DrawOverlayContexts = DisplayService.DrawOverlayContexts;
                            }

                            UpdateOverlayFlag = false;
                        }
                    }
                    else
                    {
                        if (img.DrawOverlayContexts.Count != 0)
                        {
                            img.DrawOverlayContexts.Clear();
                        }

                        UpdateOverlayFlag = false;
                    }

                    DisplayService.SetImage(this, img);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (aquireSemaphoreTask?.Status == TaskStatus.RanToCompletion)
                {
                    semaphore.Release();
                }
            }
        }
    }


    /// <summary>
    /// Allows a semaphore to release with the IDisposable pattern
    /// </summary>
    /// <remarks>
    /// Solves an issue where using the pattern:
    /// <code>
    /// try { await sem.WaitAsync(cancellationToken); }
    /// finally { sem.Release(); }
    /// </code>
    /// Can result in SemaphoreFullException if the token is cancelled and the
    /// the semaphore is not incremented.
    /// </remarks>
    public static class CancellableSemaphore
    {
        public static async Task<IDisposable> WaitWithCancellationAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            var task = semaphore.WaitAsync(cancellationToken);
            await task.ConfigureAwait(false);
            return new CancellableSemaphoreInternal(semaphore, task);
        }

        private class CancellableSemaphoreInternal : IDisposable
        {
            private readonly SemaphoreSlim _semaphoreSlim;
            private Task _awaitTask;

            public CancellableSemaphoreInternal(SemaphoreSlim semaphoreSlim, Task awaitTask)
            {
                _semaphoreSlim = semaphoreSlim;
                _awaitTask = awaitTask;
            }

            public void Dispose()
            {
                if (_awaitTask?.Status == TaskStatus.RanToCompletion)
                {
                    _semaphoreSlim.Release();
                    _awaitTask = null;
                }
            }
        }
    }
}
