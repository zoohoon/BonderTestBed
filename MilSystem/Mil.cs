
namespace Utils.MilSystem
{
    using ProberInterfaces;
    using ProberInterfaces.Vision;
    using Matrox.MatroxImagingLibrary;
    using System;
    using ProberErrorCode;
    using SystemExceptions.VisionException;
    using LogModule;

    public class Mil : IMil , IDisposable
    {
        
        private int GrabberIdentifier = -1;
        private int ProcesserIdentifier = -1;

        public MIL_ID MilApplication = MIL.M_NULL;

        MIL_INT LicenseModules = -1;

        bool disposed = false;

        private MIL_ID MilSystem = MIL.M_NULL;

        public bool _IsDongle;
        public bool IsDongle
        {
            get { return _IsDongle; }
            set
            {
                _IsDongle = value;
            }
        }

        public Mil()
        {
            // InitMilObjects();
        }
        ~Mil()
        {
            Dispose(false);
        }

        public EventCodeEnum InitMilObjects(EnumGrabberRaft graberType)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                MIL.MappAlloc(MIL.M_NULL, MIL.M_TRACE_LOG_DISABLE, ref MilApplication);
                MIL.MappControl(MilApplication, MIL.M_ERROR, MIL.M_THROW_EXCEPTION);
                MIL.MappInquire(MilApplication, MIL.M_LICENSE_MODULES, ref LicenseModules);

                if ((LicenseModules & MIL.M_LICENSE_IM) != 0)
                {
                    _IsDongle = true;
                    if (graberType == EnumGrabberRaft.MILMORPHIS)
                    {
                        MIL.MsysAlloc(MIL.M_SYSTEM_MORPHIS, MIL.M_DEV0, MIL.M_COMPLETE, ref MilSystem);
                    }
                    else if (graberType == EnumGrabberRaft.MILGIGE)
                    {
                        MIL.MsysAlloc(MIL.M_SYSTEM_GIGE_VISION, MIL.M_DEV0, MIL.M_COMPLETE, ref MilSystem);
                    }
                    else if (graberType == EnumGrabberRaft.EMULGRABBER)
                    {
                        MIL.MsysAlloc(MIL.M_DEFAULT, MIL.M_SYSTEM_HOST, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilSystem);
                    }
                    GrabberIdentifier = (int)MilSystem;
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                }
            }
            catch (MILException err)
            {
                //LoggerManager.Error($err, $"InitMilObjects() : Error occurred. graber={graberType}, err={err.Message}");
                LoggerManager.Exception(err);

                throw new VisionException("InitMilObjects Error", err, EventCodeEnum.VISION_EXCEPTION, this);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, $"InitMilObjects() : Error occurred. graber={graberType}, err={err.Message}");
                LoggerManager.Exception(err);

                throw new Exception(err.Message, err);
            }
            return retVal;
        }

        public int GetMilSystem(EnumGrabberRaft grabberRaft)
        {
            try
            {
                if (grabberRaft == EnumGrabberRaft.EMULGRABBER || grabberRaft == EnumGrabberRaft.GIGE_EMULGRABBER)
                    return -1;

                if (GrabberIdentifier == -1)
                {
                    if (ConfirmCreateApplication() != -1)
                    {
                        if ((LicenseModules & MIL.M_LICENSE_IM) != 0)
                        {
                            _IsDongle = true;
                            MilSystem = 0;

                            if (grabberRaft == EnumGrabberRaft.MILMORPHIS)
                            {
                                MIL.MsysAlloc(MIL.M_SYSTEM_MORPHIS, MIL.M_DEV0, MIL.M_COMPLETE, ref MilSystem);
                            }
                            else if (grabberRaft == EnumGrabberRaft.MILGIGE)
                            {
                                MIL.MsysAlloc(MIL.M_SYSTEM_GIGE_VISION, MIL.M_DEV0, MIL.M_COMPLETE, ref MilSystem);
                            }
                            else if(grabberRaft == EnumGrabberRaft.SIMUL_GRABBER)
                            {
                                // Nothing
                            }

                            GrabberIdentifier = (int)MilSystem;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                }
            }
            catch (MILException err)
            {
                //LoggerManager.Error($err, "GetMilSystem(EnumGrabberRaft) : Error occurred.");
                LoggerManager.Exception(err);

                throw new VisionException("InitMilObjects Error", err, EventCodeEnum.VISION_EXCEPTION, this);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "GetMilSystem(EnumGrabberRaft) : Error occurred.");
                LoggerManager.Exception(err);

                throw new Exception(err.Message, err);
            }
            return GrabberIdentifier;
        }

        public int GetMilSystem(EnumVisionProcRaft visionProcRaft)
        {
            try
            {
                if (visionProcRaft == EnumVisionProcRaft.EMUL)
                    return -1;
                if (visionProcRaft == EnumVisionProcRaft.OPENCV)
                    return -1;

                if (ConfirmCreateApplication() != -1)
                {
                    if ((LicenseModules & MIL.M_LICENSE_IM) != 0)
                    {
                        _IsDongle = true;
                        if (visionProcRaft == EnumVisionProcRaft.MIL)
                        {
                            MIL.MsysAlloc(MIL.M_DEFAULT, MIL.M_SYSTEM_HOST, MIL.M_DEFAULT, MIL.M_DEFAULT, ref MilSystem);
                        }
                        else if (visionProcRaft == EnumVisionProcRaft.EMUL)
                        {
                            MilSystem = -1;
                        }
                        ProcesserIdentifier = (int)MilSystem;
                    }
                    else
                    {
                        return -1;
                    }
                }
                
            }
            catch (MILException err)
            {
                //LoggerManager.Error($err, "GetMilSystem(EnumVisionProcRaft) : Error occurred.");
                LoggerManager.Exception(err);

                throw new VisionException("InitMilObjects Error", err, EventCodeEnum.VISION_EXCEPTION, this);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "GetMilSystem(EnumVisionProcRaft) : Error occurred.");
                LoggerManager.Exception(err);

                throw new Exception(err.Message, err);
            }
            return ProcesserIdentifier;
        }

        public int ConfirmCreateApplication()
        {
            int retVal = -1;
            try
            {
                if (MilApplication == 0 && LicenseModules == -1)
                {
                    MIL.MappAlloc(MIL.M_NULL, MIL.M_TRACE_LOG_DISABLE, ref MilApplication);
                    MIL.MappControl(MilApplication, MIL.M_ERROR, MIL.M_THROW_EXCEPTION);
                    MIL.MappInquire(MilApplication, MIL.M_LICENSE_MODULES, ref LicenseModules);
                }
                retVal = 1;
            }
            catch (MILException err)
            {
                //LoggerManager.Error($err, "ConfirmCreateApplication() : Error occurred.");
                LoggerManager.Exception(err);

                throw new VisionException("InitMilObjects Error", err, EventCodeEnum.VISION_EXCEPTION, this);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "ConfirmCreateApplication() : Error occurred.");
                LoggerManager.Exception(err);

                throw new Exception(err.Message, err);
            }
            return retVal;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposed)
                    return;

                if (disposing)
                {
                    // Code to dispose the managed resources of the class

                    try
                    {

                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }

                }

                // Code to dispose the un-managed resources of the class

                if (MilSystem != MIL.M_NULL)
                {
                    MIL.MsysFree(MilSystem);
                    MilSystem = MIL.M_NULL;
                }

                if (MilApplication != MIL.M_NULL)
                {
                    MIL.MappFree(MilApplication);
                    MilApplication = MIL.M_NULL;
                }

                disposed = true;
            }
            catch (MILException err)
            {
                //LoggerManager.Error($err, $"Dispose Error");
                LoggerManager.Exception(err);

                throw new VisionException("Dispose Error", err, EventCodeEnum.VISION_EXCEPTION, this);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, $"Dispose Error");
                LoggerManager.Exception(err);
                    
                throw new Exception(err.Message, err);
            }
        }
    }
}