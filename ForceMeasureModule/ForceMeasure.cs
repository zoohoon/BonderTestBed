using System;
using System.Runtime.CompilerServices;
using ProberErrorCode;
using LogModule;
using System.ComponentModel;
using ProberInterfaces;

namespace ForceMeasureModule
{
    public class ForceMeasure : INotifyPropertyChanged, IForceMeasure, IFactoryModule,
                                IHasSysParameterizable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]String info = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        public bool Initialized { get; set; } = false;


        private IParam _ForceMeasureParam_IParam;
        public IParam ForceMeasureParam_IParam
        {
            get { return _ForceMeasureParam_IParam; }
            set
            {
                if (value != _ForceMeasureParam_IParam)
                {
                    _ForceMeasureParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ForceMeasureParam ForceMeasureSysParamRef
        {
            get { return ForceMeasureParam_IParam as ForceMeasureParam; }
        }

        #region // Force Measurement Properties


        private ProbeAxisObject _ZAxis;
        public ProbeAxisObject ZAxis
        {
            get { return _ZAxis; }
            set
            {
                if (value != _ZAxis)
                {
                    _ZAxis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _Z0Axis;
        public ProbeAxisObject Z0Axis
        {
            get { return _Z0Axis; }
            set
            {
                if (value != _Z0Axis)
                {
                    _Z0Axis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _Z1Axis;
        public ProbeAxisObject Z1Axis
        {
            get { return _Z1Axis; }
            set
            {
                if (value != _Z1Axis)
                {
                    _Z1Axis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ProbeAxisObject _Z2Axis;
        public ProbeAxisObject Z2Axis
        {
            get { return _Z2Axis; }
            set
            {
                if (value != _Z2Axis)
                {
                    _Z2Axis = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _FSensorOrg0;
        public double FSensorOrg0
        {
            get { return _FSensorOrg0; }
            set
            {
                if (value != _FSensorOrg0)
                {
                    _FSensorOrg0 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _FSensorOrg1;
        public double FSensorOrg1
        {
            get { return _FSensorOrg1; }
            set
            {
                if (value != _FSensorOrg1)
                {
                    _FSensorOrg1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _FSensorOrg2;
        public double FSensorOrg2
        {
            get { return _FSensorOrg2; }
            set
            {
                if (value != _FSensorOrg2)
                {
                    _FSensorOrg2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _ForceValue;
        public double ForceValue
        {
            get { return _ForceValue; }
            set
            {
                if (value != _ForceValue)
                {
                    _ForceValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _Z0ForceValue;
        public double Z0ForceValue
        {
            get { return _Z0ForceValue; }
            set
            {
                if (value != _Z0ForceValue)
                {
                    _Z0ForceValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _Z1ForceValue;
        public double Z1ForceValue
        {
            get { return _Z1ForceValue; }
            set
            {
                if (value != _Z1ForceValue)
                {
                    _Z1ForceValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _Z2ForceValue;
        public double Z2ForceValue
        {
            get { return _Z2ForceValue; }
            set
            {
                if (value != _Z2ForceValue)
                {
                    _Z2ForceValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _ZFOrgRawPos;
        public double ZFOrgRawPos
        {
            get { return _ZFOrgRawPos; }
            set
            {
                if (value != _ZFOrgRawPos)
                {
                    _ZFOrgRawPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        private object measurementlockObject = new object();

        public void ResetMeasurement()
        {
            try
            {
                if (ForceMeasureSysParamRef.EnableForceMeasure.Value == true)
                {
                    lock (measurementlockObject)
                    {
                        System.Threading.Thread.Sleep(100);

                        int auxPos = 0;
                        this.MotionManager().GetAuxPulse(Z0Axis, ref auxPos);
                        //FSensorOrg0 = Z0Axis.Status.AuxPosition;
                        FSensorOrg0 = auxPos;
                        this.MotionManager().GetAuxPulse(Z1Axis, ref auxPos);
                        //FSensorOrg1 = Z1Axis.Status.AuxPosition;
                        FSensorOrg1 = auxPos;
                        this.MotionManager().GetAuxPulse(Z2Axis, ref auxPos);
                        //FSensorOrg2 = Z2Axis.Status.AuxPosition;
                        FSensorOrg2 = auxPos;


                        fcal_actpos = ZAxis.Status.RawPosition.Actual;

                        fcal_Tpos = 0;
                        fcal_Z0Delt = 0;
                        fcal_Z1Delt = 0;
                        fcal_Z2Delt = 0;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        double fcal_Tpos;
        double fcal_Z0Delt;
        double fcal_Z1Delt;
        double fcal_Z2Delt;
        //double fcal_temp;
        double fcal_actpos = 0.0;

        public void MeasureProbingForce()
        {
            double fcal_Z0VAl;
            double fcal_Z1VAl;
            double fcal_Z2VAl;
            if(ForceMeasureSysParamRef.EnableForceMeasure.Value == true)
            {
                var axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                //ResetMeasurement();
                lock (measurementlockObject)
                {
                    System.Threading.Thread.Sleep(700);
                    //fcal_Tpos = (axisZ.Status.RawPosition.Actual - fcal_actpos) * 524.288;
                    fcal_Tpos = (axisZ.Status.RawPosition.Actual - fcal_actpos) * ForceMeasureSysParamRef.AuxEncDtoPRatio.Value;

                    fcal_Z0VAl = FSensorOrg0 + fcal_Tpos;
                    fcal_Z1VAl = FSensorOrg1 + fcal_Tpos;
                    fcal_Z2VAl = FSensorOrg2 + fcal_Tpos;

                    int auxPos = 0;
                    this.MotionManager().GetAuxPulse(Z0Axis, ref auxPos);
                    fcal_Z0Delt = (auxPos - fcal_Z0VAl) * ForceMeasureSysParamRef.Z0RigidityCoeff.Value;
                    this.MotionManager().GetAuxPulse(Z1Axis, ref auxPos);
                    fcal_Z1Delt = (auxPos - fcal_Z1VAl) * ForceMeasureSysParamRef.Z1RigidityCoeff.Value;
                    this.MotionManager().GetAuxPulse(Z2Axis, ref auxPos);
                    fcal_Z2Delt = (auxPos - fcal_Z2VAl) * ForceMeasureSysParamRef.Z2RigidityCoeff.Value;
                    //fcal_Z0Delt = (Z0Axis.Status.AuxPosition - fcal_Z0VAl) * ForceMeasureSysParamRef.Z0RigidityCoeff.Value;
                    //fcal_Z1Delt = (Z1Axis.Status.AuxPosition - fcal_Z1VAl) * ForceMeasureSysParamRef.Z1RigidityCoeff.Value;
                    //fcal_Z2Delt = (Z2Axis.Status.AuxPosition - fcal_Z2VAl) * ForceMeasureSysParamRef.Z2RigidityCoeff.Value;

                    //ForceValue = (fcal_Z0Delt + fcal_Z1Delt + fcal_Z2Delt) / 1000.0;
                    ForceValue = (fcal_Z0Delt + fcal_Z1Delt + fcal_Z2Delt);   // / 1000.0;

                    Z0ForceValue = fcal_Z0Delt;
                    Z1ForceValue = fcal_Z1Delt;
                    Z2ForceValue = fcal_Z2Delt;

                   // Z0ForceValue = fcal_Z0Delt / 1000.0;
                    //Z1ForceValue = fcal_Z1Delt / 1000.0;
                    //Z2ForceValue = fcal_Z2Delt / 1000.0;

                    ForceValue = Math.Round(ForceValue, 1);

                    Z0ForceValue = Math.Round(Z0ForceValue, 2);
                    Z1ForceValue = Math.Round(Z1ForceValue, 2);
                    Z2ForceValue = Math.Round(Z2ForceValue, 2);
                }
            }
            
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;

            try
            {
                Z0Axis = this.MotionManager().GetAxis(EnumAxisConstants.Z0);
                Z1Axis = this.MotionManager().GetAxis(EnumAxisConstants.Z1);
                Z2Axis = this.MotionManager().GetAxis(EnumAxisConstants.Z2);
                ZAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                
                ResetMeasurement();
                MeasureProbingForce();

                Initialized = true;

                errorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return errorCode;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(ForceMeasureParam));
                if (RetVal == EventCodeEnum.NONE)
                {
                    ForceMeasureParam_IParam = tmpParam;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(ForceMeasureParam_IParam);
                this.LoadSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

    }
}
