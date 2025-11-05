using ErrorParam;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using XMLConverter;
using Autofac;
using ProberErrorCode;
using LogModule;
using System.Runtime.CompilerServices;

namespace ErrorCompensation
{
    public class ErrorCompensationModule : INotifyPropertyChanged, ICompensationModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private ErrorTableConverter ErrConvert = new ErrorTableConverter();
        private FirstErrorTable ErrorTable1D;
        private SecondErrorTable ErrorTable2D;

        private bool _Enable1D;
        public bool Enable1D
        {
            get { return _Enable1D; }
            set
            {
                if (value != _Enable1D)
                {
                    _Enable1D = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _Enable2D;
        public bool Enable2D
        {
            get { return _Enable2D; }
            set
            {
                if (value != _Enable2D)
                {
                    _Enable2D = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _EnableLinearComp;
        public bool EnableLinearComp
        {
            get { return _EnableLinearComp; }
            set
            {
                if (value != _EnableLinearComp)
                {
                    _EnableLinearComp = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _EnableStraightnessComp;
        public bool EnableStraightnessComp
        {
            get { return _EnableStraightnessComp; }
            set
            {
                if (value != _EnableStraightnessComp)
                {
                    _EnableStraightnessComp = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _EnableAngularComp;
        public bool EnableAngularComp
        {
            get { return _EnableAngularComp; }
            set
            {
                if (value != _EnableAngularComp)
                {
                    _EnableAngularComp = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _m_sqr_angle;
        public double m_sqr_angle
        {
            get { return _m_sqr_angle; }
            set
            {
                if (value != this._m_sqr_angle)
                {
                    _m_sqr_angle = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        private double GetErrorComp_X(double x_ref, double y_ref)
        {
            double comp_x = 0;

            try
            {
                //comp_x = GetLinearErrorX(x_ref) + GetStraightnessErrorX(y_ref);
                if (Enable2D)
                {
                    comp_x = GetLinearErrorXExt(x_ref, y_ref) + GetStraightnessErrorX(y_ref);// + GetSqaurenessErrorX(x_ref, y_ref);
                }
                else
                {
                    comp_x = GetLinearErrorX(x_ref) + GetStraightnessErrorX(y_ref);// + GetSqaurenessErrorX(x_ref, y_ref);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return comp_x;
        }

        private double GetErrorComp_Y(double x_ref, double y_ref)
        {
            double comp_y = 0;

            try
            {

                //comp_y = GetLinearErrorY(y_ref) + GetStraightnessErrorY(x_ref) + GetSqaurenessErrorY(x_ref, y_ref);
                if (Enable2D)
                {
                    //comp_y = GetLinearErrorY(y_ref) + GetStraightnessErrorYExt(x_ref, y_ref);
                    comp_y = GetLinearErrorY(y_ref) + GetStraightnessErrorYExt(x_ref, y_ref) + GetSqaurenessErrorY(x_ref, y_ref);
                }
                else
                {
                    comp_y = GetLinearErrorY(y_ref) + GetStraightnessErrorY(x_ref) + GetSqaurenessErrorY(x_ref, y_ref);
                    //comp_y = GetLinearErrorY(y_ref) + GetStraightnessErrorY(x_ref);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return comp_y;
        }

        public SecondErrorTable GetErrorTable2D()
        {
            return ErrorTable2D;
        }
        //private double GetErrorComp_C(double x_ref, double y_ref, double z_ref, double c_ref)
        private double GetErrorComp_C(double x_ref, double y_ref)
        {

            double comp_c = 0;
            try
            {
                comp_c = GetAngularErrorX(x_ref) +
                         GetAngularErrorY(y_ref);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return comp_c;
        }
        private double GetLinearErrorX(double a_x_ref)
        {
            double Result = 0;
            try
            {
                int tbl_index;
                double step;
                double tmp;
                if (EnableLinearComp == true)
                {
                    tbl_index = GetErrTableIndex(FlgCOMP_MODE.COMP_MODE_X_LIN, a_x_ref);

                    if (tbl_index == -1)
                    {
                        return 0;
                    }

                    step = GetErrStep(FlgCOMP_MODE.COMP_MODE_X_LIN, tbl_index);

                    tmp = a_x_ref - ErrorTable1D.TBL_X_LINEAR[tbl_index].Position;

                    Result = (ErrorTable1D.TBL_X_LINEAR[tbl_index + 1].ErrorValue - ErrorTable1D.TBL_X_LINEAR[tbl_index].ErrorValue) * (tmp / step)
                    + ErrorTable1D.TBL_X_LINEAR[tbl_index].ErrorValue;
                }
                else
                {
                    Result = 0.0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }

        private double GetLinearErrorY(double a_y_ref)
        {
            double Result = 0;
            try
            {
                int tbl_index;
                double step;
                double tmp;
                if (EnableLinearComp == true)
                {
                    tbl_index = GetErrTableIndex(FlgCOMP_MODE.COMP_MODE_Y_LIN, a_y_ref);

                    if (tbl_index == -1)
                    {
                        return 0;
                    }

                    step = GetErrStep(FlgCOMP_MODE.COMP_MODE_Y_LIN, tbl_index);

                    tmp = a_y_ref - ErrorTable1D.TBL_Y_LINEAR[tbl_index].Position;

                    Result = (ErrorTable1D.TBL_Y_LINEAR[tbl_index + 1].ErrorValue - ErrorTable1D.TBL_Y_LINEAR[tbl_index].ErrorValue) * (tmp / step)
                    + ErrorTable1D.TBL_Y_LINEAR[tbl_index].ErrorValue;
                }
                else
                {
                    Result = 0.0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }

        private double GetStraightnessErrorX(double a_y_ref)
        {
            double Result = 0;
            try
            {
                int tbl_index;
                double step;
                double tmp;
                if (EnableStraightnessComp == true)
                {
                    tbl_index = GetErrTableIndex(FlgCOMP_MODE.COMP_MODE_Y_STR, a_y_ref);

                    if (tbl_index == -1)
                    {
                        return 0;
                    }

                    step = GetErrStep(FlgCOMP_MODE.COMP_MODE_Y_STR, tbl_index);

                    tmp = a_y_ref - ErrorTable1D.TBL_Y_STRAIGHT[tbl_index].Position;

                    Result = (ErrorTable1D.TBL_Y_STRAIGHT[tbl_index + 1].ErrorValue - ErrorTable1D.TBL_Y_STRAIGHT[tbl_index].ErrorValue) * (tmp / step)
                    + ErrorTable1D.TBL_Y_STRAIGHT[tbl_index].ErrorValue;
                }
                else
                {
                    Result = 0.0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }

        private double GetStraightnessErrorY(double a_x_ref)
        {
            double Result = 0;
            try
            {
                int tbl_index;
                double step;
                double tmp;
                if (EnableStraightnessComp == true)
                {
                    tbl_index = GetErrTableIndex(FlgCOMP_MODE.COMP_MODE_X_STR, a_x_ref);

                    if (tbl_index == -1)
                    {
                        return 0;
                    }

                    step = GetErrStep(FlgCOMP_MODE.COMP_MODE_X_STR, tbl_index);

                    tmp = a_x_ref - ErrorTable1D.TBL_X_STRAIGHT[tbl_index].Position;

                    Result = (ErrorTable1D.TBL_X_STRAIGHT[tbl_index + 1].ErrorValue - ErrorTable1D.TBL_X_STRAIGHT[tbl_index].ErrorValue) * (tmp / step)
                    + ErrorTable1D.TBL_X_STRAIGHT[tbl_index].ErrorValue;
                }
                else
                {
                    Result = 0.0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }
        private double GetAngularErrorX(double a_x_ref)
        {
            double Result = 0;
            try
            {
                int tbl_index;
                double step;
                double tmp;
                //if (true)
                if (EnableAngularComp == true)
                {
                    tbl_index = GetErrTableIndex(FlgCOMP_MODE.COMP_MODE_X_ANG, a_x_ref);

                    if (tbl_index == -1)
                    {
                        return 0;
                    }

                    step = GetErrStep(FlgCOMP_MODE.COMP_MODE_X_ANG, tbl_index);

                    tmp = a_x_ref - ErrorTable1D.TBL_X_ANGULAR[tbl_index].Position;

                    Result = (ErrorTable1D.TBL_X_ANGULAR[tbl_index + 1].ErrorValue - ErrorTable1D.TBL_X_ANGULAR[tbl_index].ErrorValue) * (tmp / step)
                    + ErrorTable1D.TBL_X_ANGULAR[tbl_index].ErrorValue;
                }
                else
                {
                    Result = 0.0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }

        private double GetAngularErrorY(double a_y_ref)
        {
            double Result = 0;
            try
            {
                int tbl_index;
                double step;
                double tmp;
                if (EnableAngularComp == true)
                {
                    tbl_index = GetErrTableIndex(FlgCOMP_MODE.COMP_MODE_Y_ANG, a_y_ref);

                    if (tbl_index == -1)
                    {
                        return 0;
                    }

                    step = GetErrStep(FlgCOMP_MODE.COMP_MODE_Y_ANG, tbl_index);

                    tmp = a_y_ref - ErrorTable1D.TBL_Y_ANGULAR[tbl_index].Position;

                    Result = (ErrorTable1D.TBL_Y_ANGULAR[tbl_index + 1].ErrorValue - ErrorTable1D.TBL_Y_ANGULAR[tbl_index].ErrorValue) * (tmp / step)
                    + ErrorTable1D.TBL_Y_ANGULAR[tbl_index].ErrorValue;
                }
                else
                {
                    Result = 0.0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }
        /// <summary>
        /// Get sqaureness error for specific Y position.
        /// Remove Y angular compensated X deviations.
        /// </summary>
        /// <param name="x_ref"></param>
        /// <param name="y_ref"></param>
        /// <returns></returns>
        double GetSqaurenessErrorY(double x_ref, double y_ref)
        {
            double comp_y;
            double ang_err = 0.0;
            ang_err = GetAngularErrorY(y_ref);
            if (this.CoordinateManager() != null)
            {
                m_sqr_angle = this.CoordinateManager().StageCoord.MachineSequareness.Value;
            }
            if (EnableAngularComp == true)
            {
                comp_y = (x_ref) * Math.Sin(DegreeToRadian((m_sqr_angle + ang_err) / 10000.0));
            }
            else
            {
                comp_y = 0.0;
            }

            return comp_y;
        }


        double GetSqaurenessErrorX(double x_ref, double y_ref)
        {
            double comp_x;
            double ang_err = 0.0;
            ang_err = GetAngularErrorX(y_ref);
            if (this.CoordinateManager() != null)
            {
                m_sqr_angle = this.CoordinateManager().StageCoord.MachineSequareness.Value;
            }
            if (EnableAngularComp == true)
            {
                comp_x = (y_ref) * Math.Sin(DegreeToRadian((m_sqr_angle + ang_err) / 10000.0));
            }
            else
            {
                comp_x = 0.0;
            }
            return (comp_x);
        }

        private int GetErrTableIndex(FlgCOMP_MODE Type, double ref_pos)
        {
            int Index = 0;
            try
            {

                int LastIndex = 0;

                // Calc

                switch (Type)
                {
                    case FlgCOMP_MODE.COMP_MODE_X_LIN:
                        {
                            if (ErrorTable1D.TBL_X_LINEAR.Count > 0)
                            {
                                LastIndex = ErrorTable1D.TBL_X_LINEAR.Count - 1;

                                for (Index = 0; Index < LastIndex; Index++)
                                {
                                    if ((ErrorTable1D.TBL_X_LINEAR[Index].Position <= ref_pos) && (ErrorTable1D.TBL_X_LINEAR[Index + 1].Position > ref_pos))
                                    {
                                        break;
                                    }
                                }

                                if (ErrorTable1D.TBL_X_LINEAR[0].Position > ref_pos)
                                {
                                    Index = 0;
                                }

                                if (ErrorTable1D.TBL_X_LINEAR[LastIndex].Position < ref_pos)
                                {
                                    Index = LastIndex - 1;
                                }
                            }
                            else
                            {
                                Index = -1;
                            }

                            break;
                        }

                    case FlgCOMP_MODE.COMP_MODE_X_ANG:
                        {
                            if (ErrorTable1D.TBL_X_ANGULAR.Count > 0)
                            {
                                LastIndex = ErrorTable1D.TBL_X_ANGULAR.Count - 1;

                                for (Index = 0; Index < LastIndex; Index++)
                                {
                                    if ((ErrorTable1D.TBL_X_ANGULAR[Index].Position <= ref_pos) && (ErrorTable1D.TBL_X_ANGULAR[Index + 1].Position > ref_pos))
                                    {
                                        break;
                                    }
                                }

                                if (ErrorTable1D.TBL_X_ANGULAR[0].Position > ref_pos)
                                {
                                    Index = 0;
                                }

                                if (ErrorTable1D.TBL_X_ANGULAR[LastIndex].Position < ref_pos)
                                {
                                    Index = LastIndex - 1;
                                }
                            }
                            else
                            {
                                Index = -1;
                            }

                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_X_STR:
                        {
                            if (ErrorTable1D.TBL_X_STRAIGHT.Count > 0)
                            {
                                LastIndex = ErrorTable1D.TBL_X_STRAIGHT.Count - 1;

                                for (Index = 0; Index < LastIndex; Index++)
                                {
                                    if ((ErrorTable1D.TBL_X_STRAIGHT[Index].Position <= ref_pos) && (ErrorTable1D.TBL_X_STRAIGHT[Index + 1].Position > ref_pos))
                                    {
                                        break;
                                    }
                                }

                                if (ErrorTable1D.TBL_X_STRAIGHT[0].Position > ref_pos)
                                {
                                    Index = 0;
                                }

                                if (ErrorTable1D.TBL_X_STRAIGHT[LastIndex].Position < ref_pos)
                                {
                                    Index = LastIndex - 1;
                                }
                            }
                            else
                            {
                                Index = -1;
                            }

                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_Y_LIN:
                        {
                            if (ErrorTable1D.TBL_Y_LINEAR.Count > 0)
                            {
                                LastIndex = ErrorTable1D.TBL_Y_LINEAR.Count - 1;

                                for (Index = 0; Index < LastIndex; Index++)
                                {
                                    if ((ErrorTable1D.TBL_Y_LINEAR[Index].Position <= ref_pos) && (ErrorTable1D.TBL_Y_LINEAR[Index + 1].Position > ref_pos))
                                    {
                                        break;
                                    }
                                }

                                if (ErrorTable1D.TBL_Y_LINEAR[0].Position > ref_pos)
                                {
                                    Index = 0;
                                }

                                if (ErrorTable1D.TBL_Y_LINEAR[LastIndex].Position < ref_pos)
                                {
                                    Index = LastIndex - 1;
                                }
                            }
                            else
                            {
                                Index = -1;
                            }

                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_Y_ANG:
                        {
                            if (ErrorTable1D.TBL_Y_ANGULAR.Count > 0)
                            {
                                LastIndex = ErrorTable1D.TBL_Y_ANGULAR.Count - 1;

                                for (Index = 0; Index < LastIndex; Index++)
                                {
                                    if ((ErrorTable1D.TBL_Y_ANGULAR[Index].Position <= ref_pos) && (ErrorTable1D.TBL_Y_ANGULAR[Index + 1].Position > ref_pos))
                                    {
                                        break;
                                    }
                                }

                                if (ErrorTable1D.TBL_Y_ANGULAR[0].Position > ref_pos)
                                {
                                    Index = 0;
                                }

                                if (ErrorTable1D.TBL_Y_ANGULAR[LastIndex].Position < ref_pos)
                                {
                                    Index = LastIndex - 1;
                                }
                            }
                            else
                            {
                                Index = -1;
                            }

                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_Y_STR:
                        {
                            if (ErrorTable1D.TBL_Y_STRAIGHT.Count > 0)
                            {

                                LastIndex = ErrorTable1D.TBL_Y_STRAIGHT.Count - 1;

                                for (Index = 0; Index < LastIndex; Index++)
                                {
                                    if ((ErrorTable1D.TBL_Y_STRAIGHT[Index].Position <= ref_pos) && (ErrorTable1D.TBL_Y_STRAIGHT[Index + 1].Position > ref_pos))
                                    {
                                        break;
                                    }
                                }
                                if (ErrorTable1D.TBL_Y_STRAIGHT[0].Position > ref_pos)
                                {
                                    Index = 0;
                                }
                                if (ErrorTable1D.TBL_Y_STRAIGHT[LastIndex].Position < ref_pos)
                                {
                                    Index = LastIndex - 1;
                                }
                            }
                            else
                            {
                                Index = -1;
                            }

                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                if ((Index < 0) || (Index >= LastIndex))
                {
                    return -1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Index;
        }

        private double GetErrStep(FlgCOMP_MODE comp_mode, int index)
        {
            double step;
            try
            {

                step = 0;

                switch (comp_mode)
                {
                    case FlgCOMP_MODE.COMP_MODE_X_LIN:
                        {
                            step = ErrorTable1D.TBL_X_LINEAR[index + 1].Position - ErrorTable1D.TBL_X_LINEAR[index].Position;
                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_X_ANG:
                        {
                            step = ErrorTable1D.TBL_X_ANGULAR[index + 1].Position - ErrorTable1D.TBL_X_ANGULAR[index].Position;
                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_X_STR:
                        {
                            step = ErrorTable1D.TBL_X_STRAIGHT[index + 1].Position - ErrorTable1D.TBL_X_STRAIGHT[index].Position;
                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_Y_LIN:
                        {
                            step = ErrorTable1D.TBL_Y_LINEAR[index + 1].Position - ErrorTable1D.TBL_Y_LINEAR[index].Position;
                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_Y_ANG:
                        {
                            step = ErrorTable1D.TBL_Y_ANGULAR[index + 1].Position - ErrorTable1D.TBL_Y_ANGULAR[index].Position;
                            break;
                        }
                    case FlgCOMP_MODE.COMP_MODE_Y_STR:
                        {
                            step = ErrorTable1D.TBL_Y_STRAIGHT[index + 1].Position - ErrorTable1D.TBL_Y_STRAIGHT[index].Position;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                if (step < 0)
                {
                    step = step * -1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return step;
        }

        //public EventCodeEnum LoadErrorTable(string Filepath)
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        object deserializedObj;

        //        if (Directory.Exists(System.IO.Path.GetDirectoryName(Filepath)) == false)
        //        {
        //            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Filepath));
        //        }
        //        if (File.Exists(Filepath) == false)
        //        {
        //            RetVal = ErrConvert.ConvertError1DTable(Filepath);
        //        }

        //        IParam tmpParam = null;
        //        RetVal = this.LoadParameter(ref tmpParam, typeof(FirstErrorTable));
        //        if (RetVal == EventCodeEnum.NONE)
        //        {
        //            ErrorTable1D = tmpParam as FirstErrorTable;
        //        }
        //        else
        //        {
        //            RetVal = EventCodeEnum.PARAM_ERROR;

        //            LoggerManager.DebugError($"[FirstCompensationModule] LoadSysParam(): DeSerialize Error");
        //            return RetVal;
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        RetVal = EventCodeEnum.UNDEFINED;
        //        //LoggerManager.Error($String.Format("LoadMotionParam(): Error occurred while loading parameters. Err = {0}", err.Message));
        //        LoggerManager.Exception(err);

        //        throw;
        //    }

        //    return RetVal;
        //}

        CompensationValue Result = new CompensationValue();
        public CompensationValue GetErrorComp(CompensationPos CPos)
        {
            try
            {
                Result.XValue = 0;
                Result.YValue = 0;
                Result.CValue = 0;

                Result.XValue = GetErrorComp_X(CPos.XPos, CPos.YPos);
                Result.YValue = GetErrorComp_Y(CPos.XPos, CPos.YPos);
                Result.CValue = GetErrorComp_C(CPos.XPos, CPos.YPos);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return Result;
        }

        public int GetErrorComp(ObservableCollection<ProbeAxisObject> ProbeAxes)
        {
            int retVal = 0;
            try
            {
                ProbeAxisObject axisX = ProbeAxes.ToList().Find(axis => axis.AxisType.Value == ProberInterfaces.EnumAxisConstants.X);
                if (axisX == null) return -1;
                ProbeAxisObject axisY = ProbeAxes.ToList().Find(axis => axis.AxisType.Value == ProberInterfaces.EnumAxisConstants.Y);
                if (axisY == null) return -1;
                ProbeAxisObject axisC = ProbeAxes.ToList().Find(axis => axis.AxisType.Value == ProberInterfaces.EnumAxisConstants.C);
                if (axisC == null) return -1;
                var xComp = GetErrorComp_X(axisX.Status.Position.Ref, axisY.Status.Position.Ref);

                axisX.Status.RawPosition.Ref = axisX.Status.Position.Ref - GetErrorComp_X(axisX.Status.Position.Ref, axisY.Status.Position.Ref);
                axisY.Status.RawPosition.Ref = axisY.Status.Position.Ref - GetErrorComp_Y(axisX.Status.Position.Ref, axisY.Status.Position.Ref);
                axisC.Status.RawPosition.Ref = axisC.Status.Position.Ref - GetErrorComp_C(axisX.Status.Position.Ref, axisY.Status.Position.Ref);

                //LoggerManager.Debug($"Error comp x({GetErrorComp_X(axisX.Status.Position.Ref, axisY.Status.Position.Ref)}), y({GetErrorComp_Y(axisX.Status.Position.Ref, axisY.Status.Position.Ref)})");

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Debug($"GetErrorComp error");
                retVal = -1;
            }
            return retVal;
        }



        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
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

        public EventCodeEnum InitModule(Autofac.IContainer container, object param)
        {
            throw new NotImplementedException();
        }
        public void DeInitModule()
        {

        }
        public object GetErrorTable()
        {
            return ErrorTable1D;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                RetVal = LoadFirstErrorTable();
                RetVal = LoadSecondErrorTable();


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        SecondErrorTable tmpSecondTable = null;
        public EventCodeEnum ErrorMappingDataConvert(double dieSizeX, double dieSizeY, double positionX, double positionY)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                ParameterBackUp();
                ConvertYData(dieSizeY, positionY);
                SaveFirstTable();
                tmpSecondTable = Convert2DTable();
                ConvertExtYData();
                ConvertExtXData(dieSizeX, positionX);
                SaveFirstTable();
                SaveSecondErrorTable();
                RetVal = LoadFirstErrorTable();
                RetVal = LoadSecondErrorTable();
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.UNDEFINED;
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Debug($"[CompensationModule] ErrorMappingDataConvert(): ErrorMappingDataConvert Error");
            }
            return RetVal;
        }
        public SecondErrorTable Convert2DTable()
        {

            int tmpxCnt_X2 = 0;

            int tmpxCnt_Y2 = 0;

            int Cnt_1D_X;

            int Cnt_1D_Y;

            var SecondErrTable = new SecondErrorTable();
            try
            {
                Cnt_1D_X = ErrorTable1D.TBL_X_LINEAR.Count;


                Cnt_1D_Y = ErrorTable1D.TBL_Y_LINEAR.Count;

                for (tmpxCnt_Y2 = 0; tmpxCnt_Y2 < Cnt_1D_Y; tmpxCnt_Y2++)
                {
                    SecondErrTable.TBL_OX_LINEAR.Add(new ErrorParameter2D());
                    SecondErrTable.TBL_OX_LINEAR[SecondErrTable.TBL_OX_LINEAR.Count - 1].ListY = new List<ErrorParameter2D_X>();
                    SecondErrTable.TBL_OX_LINEAR[SecondErrTable.TBL_OX_LINEAR.Count - 1].PositionY = ErrorTable1D.TBL_Y_LINEAR[tmpxCnt_Y2].Position;


                    SecondErrTable.TBL_OX_STRAIGHT.Add(new ErrorParameter2D());
                    SecondErrTable.TBL_OX_STRAIGHT[SecondErrTable.TBL_OX_STRAIGHT.Count - 1].ListY = new List<ErrorParameter2D_X>();
                    SecondErrTable.TBL_OX_STRAIGHT[SecondErrTable.TBL_OX_STRAIGHT.Count - 1].PositionY = ErrorTable1D.TBL_Y_STRAIGHT[tmpxCnt_Y2].Position;

                    for (tmpxCnt_X2 = 0; tmpxCnt_X2 < Cnt_1D_X; tmpxCnt_X2++)
                    {
                        var xLinear = new ErrorParameter2D_X(ErrorTable1D.TBL_X_LINEAR[tmpxCnt_X2].Position, ErrorTable1D.TBL_X_LINEAR[tmpxCnt_X2].ErrorValue);
                        SecondErrTable.TBL_OX_LINEAR[SecondErrTable.TBL_OX_LINEAR.Count - 1].ListY.Add(xLinear);
                        var xStraight = new ErrorParameter2D_X(ErrorTable1D.TBL_X_STRAIGHT[tmpxCnt_X2].Position, ErrorTable1D.TBL_X_STRAIGHT[tmpxCnt_X2].ErrorValue);
                        SecondErrTable.TBL_OX_STRAIGHT[SecondErrTable.TBL_OX_STRAIGHT.Count - 1].ListY.Add(xStraight);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return SecondErrTable;
        }
        public EventCodeEnum ParameterBackUp()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                RetVal = Extensions_IParam.SaveParameter(null, ErrorTable1D, null, @"C:\ProberSystem\Backup\ErrorTable1D.json");
                RetVal = Extensions_IParam.SaveParameter(null, ErrorTable2D, null, @"C:\ProberSystem\Backup\ErrorTable2D.json");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }


        public EventCodeEnum LoadFirstErrorTable()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                var manager = this.MotionManager().ErrorManager as ErrorCompensationManager;
                var param = manager.ErrorCompensationDescriptorParam_Clone.ErrorModuleDescriptors.Where(item => item.Type.Value == ErrorModuleType.First).FirstOrDefault();
                ErrorTable1D = new FirstErrorTable();
                bool isSysParam = false;
                string FullPath = Extensions_IParam.GetFullPath(ErrorTable1D, null, null, ref isSysParam, typeof(FirstErrorTable));
                if (File.Exists(FullPath) == false)
                {
                    RetVal = ErrConvert.ConvertError1DTable(FullPath);
                }
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(FirstErrorTable));

                if (RetVal == EventCodeEnum.NONE)
                {
                    ErrorTable1D = tmpParam as FirstErrorTable;
                }
                else
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;

                    LoggerManager.Error($"[CompensationModule] LoadFirstErrorTable(): DeSerialize Error");
                    return RetVal;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[FirstCompensationModule] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }



        public EventCodeEnum ForcedLoadFirstErrorTable()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                var manager = this.MotionManager().ErrorManager as ErrorCompensationManager;
                var param = manager.ErrorCompensationDescriptorParam_Clone.ErrorModuleDescriptors.Where(item => item.Type.Value == ErrorModuleType.First).FirstOrDefault();
                ErrorTable1D = new FirstErrorTable();
                bool isSysParam = false;
                string FullPath = Extensions_IParam.GetFullPath(ErrorTable1D, null, null, ref isSysParam, typeof(FirstErrorTable));

                RetVal = ErrConvert.ConvertError1DTable(FullPath);
                IParam tmpParam = null;

                RetVal = this.LoadParameter(ref tmpParam, typeof(FirstErrorTable));
                bool isEnable = false;
                if (RetVal == EventCodeEnum.NONE)
                {
                    if (Enable1D == true)
                    {
                        Enable1D = false;
                        isEnable = true;
                    }
                    ErrorTable1D = tmpParam as FirstErrorTable;

                    if (isEnable == true)
                    {
                        Enable1D = true;
                    }
                }
                else
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;

                    LoggerManager.Error($"[CompensationModule] LoadFirstErrorTable(): DeSerialize Error");
                    return RetVal;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[FirstCompensationModule] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }
        public EventCodeEnum LoadSecondErrorTable()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(SecondErrorTable));

                if (RetVal == EventCodeEnum.NONE)
                {
                    ErrorTable2D = tmpParam as SecondErrorTable;
                }
                else
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;

                    LoggerManager.Error($"[CompensationModule] LoadSecondErrorTable(): DeSerialize Error");
                    return RetVal;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[FirstCompensationModule] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }
            return RetVal;
        }



        public SecondErrorTable GetCurrentSecondErrorTable()
        {
            SecondErrorTable table = null;
            try
            {
                EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(SecondErrorTable));
                if (RetVal == EventCodeEnum.NONE)
                {
                    table = tmpParam as SecondErrorTable;
                }
                else
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;

                    LoggerManager.Error($"[CompensationModule] LoadSecondErrorTable(): DeSerialize Error");
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("[FirstCompensationModule] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }
            return table;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                RetVal = SaveFirstTable();
                RetVal = SaveSecondErrorTable();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SaveSecondErrorTable(SecondErrorTable table)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                ErrorTable2D = table;

                RetVal = SaveSecondErrorTable();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SaveSecondErrorTable()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                var manager = this.MotionManager().ErrorManager as ErrorCompensationManager;
                var param = manager.ErrorCompensationDescriptorParam_Clone.ErrorModuleDescriptors.Where(item => item.Type.Value == ErrorModuleType.Second).FirstOrDefault();
                if (param != null)
                {
                    RetVal = Extensions_IParam.SaveParameter(null, ErrorTable2D);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveFirstTable()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                var manager = this.MotionManager().ErrorManager as ErrorCompensationManager;
                var param = manager.ErrorCompensationDescriptorParam_Clone.ErrorModuleDescriptors.Where(item => item.Type.Value == ErrorModuleType.First).FirstOrDefault();
                if (param != null)
                {
                    RetVal = Extensions_IParam.SaveParameter(null, ErrorTable1D);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        private int GetErrTableXIndexExt(int indexY, double refX)
        {
            int Index = 0;
            try
            {

                int LastIndex;

                LastIndex = ErrorTable2D.TBL_OX_LINEAR[indexY].ListY.Count - 1;

                for (Index = 0; Index < LastIndex; Index++)
                {
                    if ((ErrorTable2D.TBL_OX_LINEAR[indexY].ListY[Index].PositionX <= refX) && (ErrorTable2D.TBL_OX_LINEAR[indexY].ListY[Index + 1].PositionX > refX))
                    {
                        break;
                    }
                }

                if ((Index < 0) || (Index > LastIndex))
                {
                    return -1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Index;
        }

        private int GetErrTableYIndexExt(double refY)
        {
            int Index = 0;
            try
            {

                int LastIndex;

                LastIndex = ErrorTable2D.TBL_OX_LINEAR.Count - 1;

                for (Index = 0; Index < LastIndex; Index++)
                {
                    if ((ErrorTable2D.TBL_OX_LINEAR[Index].PositionY <= refY) && (ErrorTable2D.TBL_OX_LINEAR[Index + 1].PositionY > refY))
                    {
                        break;
                    }
                }

                if ((Index < 0) || (Index > LastIndex))
                {
                    return -1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Index;
        }

        private double GetErrStepXExt(int Xindex, int Yindex)
        {
            double step;
            try
            {

                step = 0;
                if (Xindex > ErrorTable2D.TBL_OX_LINEAR[Yindex].ListY.Count - 2)
                {
                    Xindex = ErrorTable2D.TBL_OX_LINEAR[Yindex].ListY.Count - 2;
                }
                step = ErrorTable2D.TBL_OX_LINEAR[Yindex].ListY[Xindex + 1].PositionX - ErrorTable2D.TBL_OX_LINEAR[Yindex].ListY[Xindex].PositionX;

                //if (FlagExtraTableOnPin == 1 && FlagUseExtraTableForPin == 1)
                //{
                //    if (Xindex < 0)
                //    {
                //        step = POS_TBL_X_EXT_FOR_PIN[1] - POS_TBL_X_EXT_FOR_PIN[0];
                //        return step;
                //    }
                //    else if (Xindex > Table_Constants.MAX_ERR_TABLE)
                //    {
                //        step = POS_TBL_X_EXT_FOR_PIN[Table_Constants.MAX_ERR_TABLE] - POS_TBL_X_EXT_FOR_PIN[Table_Constants.MAX_ERR_TABLE - 1];
                //        return step;
                //    }
                //    else
                //    {
                //        step = POS_TBL_X_EXT_FOR_PIN[Xindex + 1] - POS_TBL_X_EXT_FOR_PIN[Xindex];
                //    }
                //}
                //else
                //{
                //    if (Xindex < 0)
                //    {
                //        step = POS_TBL_X_EXT[1] - POS_TBL_X_EXT[0];
                //        return step;
                //    }
                //    else if (Xindex > Table_Constants.MAX_ERR_TABLE)
                //    {
                //        step = POS_TBL_X_EXT[Table_Constants.MAX_ERR_TABLE] - POS_TBL_X_EXT[Table_Constants.MAX_ERR_TABLE - 1];
                //        return step;
                //    }
                //    else
                //    {
                //        step = POS_TBL_X_EXT[Xindex + 1] - POS_TBL_X_EXT[Xindex];
                //    }

                //}

                if (step < 0)
                {
                    step = step * -1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return step;
        }

        private double GetErrStepYExt(int Xindex, int Yindex)
        {
            double step;
            try
            {

                step = 0;


                //if (Xindex > ErrorTable2D.TBL_OX_LINEAR[Yindex].ListY.Count - 2)
                //{
                //    Xindex = ErrorTable2D.TBL_OX_LINEAR[Yindex].ListY.Count - 2;
                //}
                var indexYplus = Yindex + 1;
                if (Yindex >= ErrorTable2D.TBL_OX_LINEAR.Count)
                {
                    indexYplus = ErrorTable2D.TBL_OX_LINEAR.Count - 1;
                }
                step = ErrorTable2D.TBL_OX_LINEAR[Yindex].PositionY - ErrorTable2D.TBL_OX_LINEAR[indexYplus].PositionY;

                if (step < 0)
                {
                    step = step * -1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return step;
        }

        private double GetLinearErrorXExt(double a_x_ref, double a_y_ref)
        {
            double Result = 0;
            try
            {
                int tbl_indexY;
                int tbl_indexX;

                double step;

                double tmp;
                double tmpPoint1;
                double tmpPoint2;

                // Get Table Index X and Y
                tbl_indexY = GetErrTableYIndexExt(a_y_ref);

                if (tbl_indexY == -1)
                {
                    return 0;
                }

                tbl_indexX = GetErrTableXIndexExt(tbl_indexY, a_x_ref);

                if (tbl_indexX == -1)
                {
                    return 0;
                }

                step = GetErrStepXExt(tbl_indexX, tbl_indexY);
                if (tbl_indexY < ErrorTable2D.TBL_OX_STRAIGHT.Count - 1)
                {
                    tmp = a_y_ref - ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].PositionY; //ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].PositionX;
                    var indexYplus = tbl_indexY + 1;
                    var indexXplus = tbl_indexX + 1;
                    if (tbl_indexY >= ErrorTable2D.TBL_OX_LINEAR.Count - 1)
                    {
                        indexYplus = ErrorTable2D.TBL_OX_LINEAR.Count - 1;
                    }
                    if (tbl_indexX >= ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY.Count - 1)
                    {
                        indexXplus = ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY.Count - 1;
                    }
                    tmpPoint1 = (ErrorTable2D.TBL_OX_LINEAR[indexYplus].ListY[tbl_indexX].ErrorValue - ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].ErrorValue) * (tmp / step)
                                + ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].ErrorValue;


                    tmpPoint2 = (ErrorTable2D.TBL_OX_LINEAR[indexYplus].ListY[indexXplus].ErrorValue - ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[indexXplus].ErrorValue) * (tmp / step)
                                + ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[indexXplus].ErrorValue;

                    //tmp = a_x_ref - ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].PositionX;

                    //tmpPoint1 = (ErrorTable2D.TBL_OX_LINEAR[tbl_indexY + 1].ListY[tbl_indexX + 1].ErrorValue - ErrorTable2D.TBL_OX_LINEAR[tbl_indexY + 1].ListY[tbl_indexX].ErrorValue) * (tmp / step)
                    //            + ErrorTable2D.TBL_OX_LINEAR[tbl_indexY + 1].ListY[tbl_indexX].ErrorValue;

                    //tmpPoint2 = (ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX + 1].ErrorValue - ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].ErrorValue) * (tmp / step)
                    //            + ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].ErrorValue;

                    step = GetErrStepYExt(tbl_indexX, tbl_indexY);

                    //if (FlagExtraTableOnPin == 1 && FalgUseExtraTableForPin == 1)
                    //{
                    //    tmp = a_x_ref - POS_TBL_X_EXT_FOR_PIN[tbl_indexX];
                    //}
                    //else
                    //{
                    //    tmp = a_x_ref - POS_TBL_X_EXT[tbl_indexX];
                    //}

                    tmp = a_x_ref - ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].PositionX;  //ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].PositionY;
                                                                                                         //tmp = a_y_ref - ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].PositionY;

                    Result = (tmpPoint2 - tmpPoint1) * (tmp / step) + tmpPoint1;
                    //Result = (tmpPoint2 - tmpPoint1) * (tmp / step) + tmpPoint2;
                }
                else
                {
                    Result = ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].ErrorValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }

        private double GetStraightnessErrorYExt(double a_x_ref, double a_y_ref)
        {
            double Result = 0;
            try
            {
                int tbl_indexY;
                int tbl_indexX;

                double step;

                double tmp;
                double tmpPoint1;
                double tmpPoint2;

                // Get Table Index X and Y
                tbl_indexY = GetErrTableYIndexExt(a_y_ref);

                if (tbl_indexY == -1)
                {
                    return 0;
                }

                tbl_indexX = GetErrTableXIndexExt(tbl_indexY, a_x_ref);

                if (tbl_indexX == -1)
                {
                    return 0;
                }

                step = GetErrStepXExt(tbl_indexX, tbl_indexY);

                tmp = a_y_ref - ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].PositionY; // ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX].PositionY;
                                                                                    //tmp = a_x_ref - ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX].PositionX;
                if (tbl_indexY < ErrorTable2D.TBL_OX_STRAIGHT.Count - 1)
                {


                    var indexYplus = tbl_indexY + 1;
                    var indexXplus = tbl_indexX + 1;
                    if (tbl_indexY >= ErrorTable2D.TBL_OX_STRAIGHT.Count - 1)
                    {
                        indexYplus = ErrorTable2D.TBL_OX_STRAIGHT.Count - 1;
                    }
                    if (tbl_indexX >= ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY.Count - 1)
                    {
                        indexXplus = ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY.Count - 1;
                    }

                    tmpPoint1 = (ErrorTable2D.TBL_OX_STRAIGHT[indexYplus].ListY[tbl_indexX].ErrorValue - ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX].ErrorValue) * (tmp / step)
                                            + ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX].ErrorValue;

                    tmpPoint2 = (ErrorTable2D.TBL_OX_STRAIGHT[indexYplus].ListY[indexXplus].ErrorValue - ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[indexXplus].ErrorValue) * (tmp / step)
                                + ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[indexXplus].ErrorValue;

                    //tmpPoint1 = (ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY + 1].ListY[tbl_indexX + 1].ErrorValue - ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY + 1].ListY[tbl_indexX].ErrorValue) * (tmp / step)
                    //            + ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY + 1].ListY[tbl_indexX].ErrorValue;

                    //tmpPoint2 = (ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX + 1].ErrorValue - ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX].ErrorValue) * (tmp / step)
                    //            + ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX].ErrorValue;

                    step = GetErrStepYExt(tbl_indexX, tbl_indexY);

                    //if (FlagExtraTableOnPin == 1 && FalgUseExtraTableForPin == 1)
                    //{
                    //    tmp = a_x_ref - POS_TBL_X_EXT_FOR_PIN[tbl_indexX];
                    //}
                    //else
                    //{
                    //    tmp = a_x_ref - POS_TBL_X_EXT[tbl_indexX];
                    //}

                    tmp = a_x_ref - ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX].PositionX;  //ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexX].PositionX;
                                                                                                           //tmp = a_y_ref - ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].PositionY;

                    Result = (tmpPoint2 - tmpPoint1) * (tmp / step) + tmpPoint1;
                    //Result = (tmpPoint2 - tmpPoint1) * (tmp / step) + tmpPoint2;

                }
                else
                {
                    var indexXplus = tbl_indexX + 1;
                    if (tbl_indexX >= ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY.Count - 1)
                    {
                        indexXplus = ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY.Count - 1;
                    }
                    Result = ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[indexXplus].ErrorValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }

        public void ConvertYData(double dieSizeY, double yPos)
        {
            try
            {
                var straight_y = ErrorTable1D.TBL_Y_STRAIGHT.ToList();
                var linear_y = ErrorTable1D.TBL_Y_LINEAR.ToList();
                var angular_y = ErrorTable1D.TBL_Y_ANGULAR.ToList();
                ErrorParamList Con_straight = new ErrorParamList();
                ErrorParamList Con_linear = new ErrorParamList();
                ErrorParamList Con_angular = new ErrorParamList();
                int convertYcnt = 0;
                int cnt = 0;
                double minValue = 0;
                while (true)
                {
                    if (straight_y[0].Position < yPos - dieSizeY * convertYcnt)
                    {
                        convertYcnt++;
                    }
                    else
                    {
                        break;
                    }
                }
                cnt = convertYcnt;
                minValue = yPos - dieSizeY * cnt;
                convertYcnt = 0;
                while (true)
                {
                    if (straight_y[straight_y.Count - 1].Position > yPos + dieSizeY * convertYcnt)
                    {
                        convertYcnt++;
                    }
                    else
                    {
                        break;
                    }
                }
                cnt += convertYcnt;
                convertYcnt = 0;
                for (int i = 0; i < cnt + 1; i++)
                {
                    Con_straight.Add(new ErrorParameter(minValue + dieSizeY * i, CalcConvertErrPosY(ErrorCompEnum.STRAIGHT, minValue + dieSizeY * i)));
                    Con_linear.Add(new ErrorParameter(minValue + dieSizeY * i, CalcConvertErrPosY(ErrorCompEnum.LINEAR, minValue + dieSizeY * i)));
                    Con_angular.Add(new ErrorParameter(minValue + dieSizeY * i, CalcConvertErrPosY(ErrorCompEnum.ANGULAR, minValue + dieSizeY * i)));
                }
                ErrorTable1D.TBL_Y_LINEAR = Con_linear;
                ErrorTable1D.TBL_Y_STRAIGHT = Con_straight;
                ErrorTable1D.TBL_Y_ANGULAR = Con_angular;



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ConvertExtXData(double dieSizeX, double xPos)
        {
            try
            {

                int convertYcnt = 0;
                int cnt = 0;
                double minValue = 0;
                while (true)
                {
                    if (ErrorTable2D.TBL_OX_STRAIGHT[0].ListY[0].PositionX < xPos - dieSizeX * convertYcnt)
                    {
                        convertYcnt++;
                    }
                    else
                    {
                        break;
                    }
                }
                cnt = convertYcnt;
                minValue = xPos - dieSizeX * cnt;
                convertYcnt = 0;
                while (true)
                {
                    if (ErrorTable2D.TBL_OX_STRAIGHT[0].ListY[ErrorTable2D.TBL_OX_STRAIGHT[0].ListY.Count - 1].PositionX > xPos + dieSizeX * convertYcnt)
                    {
                        convertYcnt++;
                    }
                    else
                    {
                        break;
                    }
                }
                cnt += convertYcnt;
                convertYcnt = 0;
                for (int i = 0; i < ErrorTable2D.TBL_OX_STRAIGHT.Count; i++)
                {
                    List<ErrorParameter2D_X> tmpStraightList = new List<ErrorParameter2D_X>();
                    List<ErrorParameter2D_X> tmpLinearList = new List<ErrorParameter2D_X>();
                    for (int j = 0; j < cnt + 1; j++)
                    {
                        tmpStraightList.Add(new ErrorParameter2D_X(minValue + dieSizeX * j, CalcConvertErrPosX2D(ErrorCompEnum.STRAIGHT, j, minValue + dieSizeX * j)));
                        tmpLinearList.Add(new ErrorParameter2D_X(minValue + dieSizeX * j, CalcConvertErrPosX2D(ErrorCompEnum.LINEAR, j, minValue + dieSizeX * j)));
                    }
                    ErrorTable2D.TBL_OX_STRAIGHT[i].ListY = tmpStraightList;
                    ErrorTable2D.TBL_OX_LINEAR[i].ListY = tmpLinearList;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void ConvertExtYData()
        {
            try
            {

                int tmpxCnt_X2 = 0;

                int tmpxCnt_Y2 = 0;

                int Cnt_1D_X;

                int Cnt_1D_Y;
                SecondErrorTable errorTable = new SecondErrorTable();
                Cnt_1D_X = ErrorTable2D.TBL_OX_LINEAR[0].ListY.Count;

                Cnt_1D_Y = ErrorTable1D.TBL_Y_LINEAR.Count;

                for (tmpxCnt_Y2 = 0; tmpxCnt_Y2 < Cnt_1D_Y; tmpxCnt_Y2++)
                {
                    errorTable.TBL_OX_LINEAR.Add(new ErrorParameter2D());
                    errorTable.TBL_OX_LINEAR[errorTable.TBL_OX_LINEAR.Count - 1].ListY = new List<ErrorParameter2D_X>();
                    errorTable.TBL_OX_LINEAR[errorTable.TBL_OX_LINEAR.Count - 1].PositionY = ErrorTable1D.TBL_Y_LINEAR[tmpxCnt_Y2].Position;


                    errorTable.TBL_OX_STRAIGHT.Add(new ErrorParameter2D());
                    errorTable.TBL_OX_STRAIGHT[errorTable.TBL_OX_STRAIGHT.Count - 1].ListY = new List<ErrorParameter2D_X>();
                    errorTable.TBL_OX_STRAIGHT[errorTable.TBL_OX_STRAIGHT.Count - 1].PositionY = ErrorTable1D.TBL_Y_STRAIGHT[tmpxCnt_Y2].Position;
                    var pos = errorTable.TBL_OX_STRAIGHT[errorTable.TBL_OX_STRAIGHT.Count - 1].PositionY;
                    for (tmpxCnt_X2 = 0; tmpxCnt_X2 < Cnt_1D_X; tmpxCnt_X2++)
                    {
                        var xLinear = new ErrorParameter2D_X(ErrorTable2D.TBL_OX_LINEAR[0].ListY[tmpxCnt_X2].PositionX, CalcConvertErrPosX(ErrorCompEnum.LINEAR, errorTable.TBL_OX_STRAIGHT[errorTable.TBL_OX_STRAIGHT.Count - 1].PositionY, ErrorTable2D.TBL_OX_LINEAR[FindExtIndex(pos)].ListY[tmpxCnt_X2].PositionX));
                        errorTable.TBL_OX_LINEAR[errorTable.TBL_OX_LINEAR.Count - 1].ListY.Add(xLinear);
                        var xStraight = new ErrorParameter2D_X(ErrorTable2D.TBL_OX_STRAIGHT[0].ListY[tmpxCnt_X2].PositionX, CalcConvertErrPosX(ErrorCompEnum.STRAIGHT, errorTable.TBL_OX_STRAIGHT[errorTable.TBL_OX_STRAIGHT.Count - 1].PositionY, ErrorTable2D.TBL_OX_STRAIGHT[FindExtIndex(pos)].ListY[tmpxCnt_X2].PositionX));
                        errorTable.TBL_OX_STRAIGHT[errorTable.TBL_OX_STRAIGHT.Count - 1].ListY.Add(xStraight);
                    }

                }
                ErrorTable2D = errorTable;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        } 

        private int FindExtIndex(double pos)
        {
            int retVal = 0;
            try
            {
                //     if (ErrorTable2D.TBL_OX_LINEAR[retVal].PositionY <=
                int cnt = 0;
                while (true)
                {

                    if (ErrorTable2D.TBL_OX_LINEAR[0].PositionY >= pos)
                    {
                        retVal = 0;
                        break;
                    }
                    else if (cnt + 1 > ErrorTable2D.TBL_OX_LINEAR.Count - 1)
                    {
                        retVal = ErrorTable2D.TBL_OX_LINEAR.Count - 1;
                        break;
                    }
                    else if (ErrorTable2D.TBL_OX_LINEAR[cnt].PositionY <= pos && pos <= ErrorTable2D.TBL_OX_LINEAR[cnt + 1].PositionY)
                    {
                        retVal = cnt;
                        break;
                    }
                    else
                    {
                        cnt++;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private double CalcConvertErrPosX2D(ErrorCompEnum comp, int index, double position)
        {
            List<ErrorParameter2D_X> errorTable = null;
            if (comp == ErrorCompEnum.LINEAR)
            {
                errorTable = ErrorTable2D.TBL_OX_LINEAR[index].ListY;
            }
            else if (comp == ErrorCompEnum.STRAIGHT)
            {
                errorTable = ErrorTable2D.TBL_OX_STRAIGHT[index].ListY;
            }
            int cnt = 0;
            double retVal = 0.0;
            try
            {
                while (true)
                {
                    if (errorTable[0].PositionX >= position)
                    {
                        retVal = errorTable[0].ErrorValue;
                        break;
                    }
                    else if (cnt + 1 > errorTable.Count - 1)
                    {
                        retVal = errorTable[errorTable.Count - 1].ErrorValue;
                        break;
                    }
                    else if (errorTable[cnt].PositionX <= position && position <= errorTable[cnt + 1].PositionX)
                    {
                        retVal = errorTable[cnt].ErrorValue +
                            ((errorTable[cnt + 1].ErrorValue - errorTable[cnt].ErrorValue) / (errorTable[cnt + 1].PositionX - errorTable[cnt].PositionX)) *
                            (position - errorTable[cnt].PositionX);
                        break;
                    }
                    else
                    {
                        cnt++;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;

        }
        private double CalcConvertErrPosX(ErrorCompEnum comp, double positionY, double positionX)
        {
            double retVal = 0.0;
            try
            {
                ObservableCollection<ErrorParameter2D> errorTable = null;
                if (comp == ErrorCompEnum.LINEAR)
                {
                    errorTable = ErrorTable2D.TBL_OX_LINEAR;
                }
                else if (comp == ErrorCompEnum.STRAIGHT)
                {
                    errorTable = ErrorTable2D.TBL_OX_STRAIGHT;
                }
                int cnt = 0;
                while (true)
                {
                    if (errorTable[0].PositionY >= positionY)
                    {
                        int Xcnt = 0;
                        while (true)
                        {
                            if (errorTable[0].ListY[0].PositionX >= positionX)
                            {
                                retVal = errorTable[0].ListY[0].ErrorValue;
                                break;
                            }
                            else if (Xcnt + 1 > errorTable[0].ListY.Count - 1)
                            {
                                int idx = errorTable[0].ListY.Count - 1;
                                retVal = errorTable[0].ListY[idx].ErrorValue;
                                //errorTable[errorTable.Count - 1].PositionY;
                                break;
                            }
                            else if (errorTable[0].ListY[Xcnt].PositionX <= positionX && positionX < errorTable[0].ListY[Xcnt + 1].PositionX)
                            {
                                retVal = errorTable[0].ListY[Xcnt].ErrorValue;
                                break;
                            }
                            else
                            {
                                Xcnt++;
                            }
                        }
                        break;
                    }
                    else if (cnt + 1 > errorTable.Count - 1)
                    {
                        int Xcnt = 0;
                        int idx = errorTable.Count - 1;
                        while (true)
                        {
                            if (errorTable[idx].ListY[0].PositionX >= positionX)
                            {
                                retVal = errorTable[idx].ListY[0].ErrorValue;
                                break;
                            }
                            else if (Xcnt + 1 > errorTable[idx].ListY.Count - 1)
                            {
                                int xidx = errorTable[idx].ListY.Count - 1;
                                retVal = errorTable[idx].ListY[xidx].ErrorValue;
                                //errorTable[errorTable.Count - 1].PositionY;
                                break;
                            }
                            else if (errorTable[idx].ListY[Xcnt].PositionX <= positionX && positionX < errorTable[idx].ListY[Xcnt + 1].PositionX)
                            {
                                retVal = errorTable[idx].ListY[Xcnt].ErrorValue;
                                break;
                            }
                            else
                            {
                                Xcnt++;
                            }
                        }
                        break;
                    }
                    else if (errorTable[cnt].PositionY <= positionY && positionY <= errorTable[cnt + 1].PositionY)
                    {
                        int Xcnt = 0;
                        while (true)
                        {
                            if (errorTable[cnt].ListY[0].PositionX >= positionX)
                            {
                                retVal = errorTable[cnt].ListY[0].ErrorValue;
                                break;
                            }
                            else if (Xcnt + 1 > errorTable[cnt].ListY.Count - 1)
                            {
                                int idx = errorTable[cnt].ListY.Count - 1;
                                retVal = errorTable[cnt].ListY[idx].ErrorValue +
                                      ((errorTable[cnt + 1].ListY[idx].ErrorValue - errorTable[cnt].ListY[idx].ErrorValue) / (errorTable[cnt + 1].PositionY - errorTable[cnt].PositionY)) *
                                          (positionY - errorTable[cnt].PositionY);
                                //errorTable[errorTable.Count - 1].PositionY;
                                break;
                            }
                            else if (errorTable[cnt].ListY[Xcnt].PositionX <= positionX && positionX < errorTable[cnt].ListY[Xcnt + 1].PositionX)
                            {
                                retVal = errorTable[cnt].ListY[Xcnt].ErrorValue +
                                          ((errorTable[cnt + 1].ListY[Xcnt].ErrorValue - errorTable[cnt].ListY[Xcnt].ErrorValue) / (errorTable[cnt + 1].PositionY - errorTable[cnt].PositionY)) *
                                          (positionY - errorTable[cnt].PositionY);
                                break;
                            }
                            else
                            {
                                Xcnt++;
                            }
                        }
                        //retVal = errorTable[cnt].ErrorValue +
                        //    ((errorTable[cnt + 1].ErrorValue - errorTable[cnt].ErrorValue) / (errorTable[cnt + 1].Position - errorTable[cnt].Position)) *
                        //    (position - errorTable[cnt].Position);
                        break;
                    }
                    else
                    {
                        cnt++;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            return retVal;
        }

        private double CalcConvertErrPosY(ErrorCompEnum comp, double position)
        {
            ErrorParamList errorTable = null;
            if (comp == ErrorCompEnum.LINEAR)
            {
                errorTable = ErrorTable1D.TBL_Y_LINEAR;
            }
            else if (comp == ErrorCompEnum.STRAIGHT)
            {
                errorTable = ErrorTable1D.TBL_Y_STRAIGHT;
            }
            else if (comp == ErrorCompEnum.ANGULAR)
            {
                errorTable = ErrorTable1D.TBL_Y_ANGULAR;
            }
            int cnt = 0;
            double retVal = 0.0;
            try
            {
                while (true)
                {
                    if (errorTable[0].Position >= position)
                    {
                        retVal = errorTable[0].ErrorValue;
                        break;
                    }
                    else if (cnt + 1 > errorTable.Count - 1)
                    {
                        retVal = errorTable[errorTable.Count - 1].ErrorValue;
                        break;
                    }
                    else if (errorTable[cnt].Position <= position && position <= errorTable[cnt + 1].Position)
                    {
                        retVal = errorTable[cnt].ErrorValue +
                            ((errorTable[cnt + 1].ErrorValue - errorTable[cnt].ErrorValue) / (errorTable[cnt + 1].Position - errorTable[cnt].Position)) *
                            (position - errorTable[cnt].Position);
                        break;
                    }
                    else
                    {
                        cnt++;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;

        }

    }

    public enum ErrorCompEnum
    {
        LINEAR,
        STRAIGHT,
        ANGULAR
    }

    //public class SecondCompensationModule : INotifyPropertyChanged, ICompensationModule
    //{
    //    #region ==> PropertyChanged
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion


    //    public bool Initialized { get; set; } = false;

    //    private ErrorTableConverter ErrConvert = new ErrorTableConverter();
    //    private FirstErrorTable ErrorTable1D;
    //    private SecondErrorTable ErrorTable2D;

    //    private bool _Enable;
    //    public bool Enable
    //    {
    //        get { return _Enable; }
    //        set
    //        {
    //            if (value != _Enable)
    //            {
    //                _Enable = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private double _m_sqr_angle;
    //    public double m_sqr_angle
    //    {
    //        get { return _m_sqr_angle; }
    //        set
    //        {
    //            if (value != this._m_sqr_angle)
    //            {
    //                _m_sqr_angle = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    //public EventCodeEnum LoadErrorTable(string Filepath)
    //    //{
    //    //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

    //    //    try
    //    //    {

    //    //        if (Directory.Exists(System.IO.Path.GetDirectoryName(Filepath)) == false)
    //    //        {
    //    //            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Filepath));
    //    //        }

    //    //        if (File.Exists(Filepath) == false)
    //    //        {
    //    //            RetVal = ErrConvert.ConvertError2DTable(Filepath);

    //    //            if (RetVal != EventCodeEnum.NONE)
    //    //            {
    //    //                return RetVal;
    //    //            }
    //    //        }

    //    //        IParam tmpParam = null;
    //    //        RetVal = this.LoadParameter(ref tmpParam, typeof(SecondErrorTable), null, Filepath);
    //    //        ErrorTable2D = tmpParam as SecondErrorTable;
    //    //    }
    //    //    catch (Exception err)
    //    //    {
    //    //        RetVal = EventCodeEnum.UNDEFINED;

    //    //        //LoggerManager.Error($String.Format("LoadMotionParam(): Error occurred while loading parameters. Err = {0}", err.Message));
    //    //        LoggerManager.Exception(err);

    //    //        throw;
    //    //    }

    //    //    return RetVal;
    //    //}

    //    private double GetErrorComp_X_Ext(double x_ref, double y_ref)
    //    {
    //        double comp_x = 0;

    //        comp_x = GetLinearErrorXExt(x_ref, y_ref);

    //        return comp_x;
    //    }
    //    private double GetErrorComp_Y_Ext(double x_ref, double y_ref)
    //    {
    //        double comp_y = 0;

    //        comp_y = GetStraightnessErrorYExt(x_ref, y_ref);

    //        return comp_y;
    //    }

    //    private int GetErrTableXIndexExt(int indexY, double refX)
    //    {
    //        int Index = 0;

    //        int LastIndex;

    //        LastIndex = ErrorTable2D.TBL_OX_LINEAR[indexY].ListY.Count - 1;

    //        for (Index = 0; Index < LastIndex; Index++)
    //        {
    //            if ((ErrorTable2D.TBL_OX_LINEAR[indexY].ListY[Index].PositionX <= refX) && (ErrorTable2D.TBL_OX_LINEAR[indexY].ListY[Index + 1].PositionX > refX))
    //            {
    //                break;
    //            }
    //        }

    //        if ((Index < 0) || (Index > LastIndex))
    //        {
    //            return -1;
    //        }

    //        return Index;
    //    }

    //    private int GetErrTableYIndexExt(double refY)
    //    {
    //        int Index = 0;

    //        int LastIndex;

    //        LastIndex = ErrorTable2D.TBL_OX_LINEAR.Count - 1;

    //        for (Index = 0; Index < LastIndex; Index++)
    //        {
    //            if ((ErrorTable2D.TBL_OX_LINEAR[Index].PositionY <= refY) && (ErrorTable2D.TBL_OX_LINEAR[Index + 1].PositionY > refY))
    //            {
    //                break;
    //            }
    //        }

    //        if ((Index < 0) || (Index > LastIndex))
    //        {
    //            return -1;
    //        }

    //        return Index;
    //    }

    //    private double GetErrStepXExt(int Xindex, int Yindex)
    //    {
    //        double step;

    //        step = 0;

    //        step = ErrorTable2D.TBL_OX_LINEAR[Yindex].ListY[Xindex + 1].PositionX - ErrorTable2D.TBL_OX_LINEAR[Yindex].ListY[Xindex].PositionX;

    //        //if (FlagExtraTableOnPin == 1 && FlagUseExtraTableForPin == 1)
    //        //{
    //        //    if (Xindex < 0)
    //        //    {
    //        //        step = POS_TBL_X_EXT_FOR_PIN[1] - POS_TBL_X_EXT_FOR_PIN[0];
    //        //        return step;
    //        //    }
    //        //    else if (Xindex > Table_Constants.MAX_ERR_TABLE)
    //        //    {
    //        //        step = POS_TBL_X_EXT_FOR_PIN[Table_Constants.MAX_ERR_TABLE] - POS_TBL_X_EXT_FOR_PIN[Table_Constants.MAX_ERR_TABLE - 1];
    //        //        return step;
    //        //    }
    //        //    else
    //        //    {
    //        //        step = POS_TBL_X_EXT_FOR_PIN[Xindex + 1] - POS_TBL_X_EXT_FOR_PIN[Xindex];
    //        //    }
    //        //}
    //        //else
    //        //{
    //        //    if (Xindex < 0)
    //        //    {
    //        //        step = POS_TBL_X_EXT[1] - POS_TBL_X_EXT[0];
    //        //        return step;
    //        //    }
    //        //    else if (Xindex > Table_Constants.MAX_ERR_TABLE)
    //        //    {
    //        //        step = POS_TBL_X_EXT[Table_Constants.MAX_ERR_TABLE] - POS_TBL_X_EXT[Table_Constants.MAX_ERR_TABLE - 1];
    //        //        return step;
    //        //    }
    //        //    else
    //        //    {
    //        //        step = POS_TBL_X_EXT[Xindex + 1] - POS_TBL_X_EXT[Xindex];
    //        //    }

    //        //}

    //        if (step < 0)
    //        {
    //            step = step * -1;
    //        }

    //        return step;
    //    }

    //    private double GetErrStepYExt(int Xindex, int Yindex)
    //    {
    //        double step;

    //        step = 0;

    //        step = ErrorTable2D.TBL_OX_LINEAR[Yindex].PositionY - ErrorTable2D.TBL_OX_LINEAR[Yindex + 1].PositionY;

    //        if (step < 0)
    //        {
    //            step = step * -1;
    //        }

    //        return step;
    //    }

    //    private double GetLinearErrorXExt(double a_x_ref, double a_y_ref)
    //    {
    //        double Result = 0;
    //        int tbl_indexY;
    //        int tbl_indexX;

    //        double step;

    //        double tmp;
    //        double tmpPoint1;
    //        double tmpPoint2;

    //        // Get Table Index X and Y
    //        tbl_indexY = GetErrTableYIndexExt(a_y_ref);

    //        if (tbl_indexY == -1)
    //        {
    //            return 0;
    //        }

    //        tbl_indexX = GetErrTableXIndexExt(tbl_indexY, a_x_ref);

    //        if (tbl_indexX == -1)
    //        {
    //            return 0;
    //        }

    //        step = GetErrStepXExt(tbl_indexX, tbl_indexY);

    //        tmp = a_x_ref - ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].PositionX;

    //        tmpPoint1 = (ErrorTable2D.TBL_OX_LINEAR[tbl_indexY + 1].ListY[tbl_indexX + 1].ErrorValue - ErrorTable2D.TBL_OX_LINEAR[tbl_indexY + 1].ListY[tbl_indexX].ErrorValue) * (tmp / step)
    //                    + ErrorTable2D.TBL_OX_LINEAR[tbl_indexY + 1].ListY[tbl_indexX].ErrorValue;

    //        tmpPoint2 = (ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX + 1].ErrorValue - ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].ErrorValue) * (tmp / step)
    //                    + ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].ErrorValue;

    //        step = GetErrStepYExt(tbl_indexX, tbl_indexY);

    //        //if (FlagExtraTableOnPin == 1 && FalgUseExtraTableForPin == 1)
    //        //{
    //        //    tmp = a_x_ref - POS_TBL_X_EXT_FOR_PIN[tbl_indexX];
    //        //}
    //        //else
    //        //{
    //        //    tmp = a_x_ref - POS_TBL_X_EXT[tbl_indexX];
    //        //}

    //        tmp = a_y_ref - ErrorTable2D.TBL_OX_LINEAR[tbl_indexY].PositionY;

    //        Result = (tmpPoint2 - tmpPoint1) * (tmp / step) + tmpPoint1;

    //        return Result;
    //    }

    //    private double GetStraightnessErrorYExt(double a_x_ref, double a_y_ref)
    //    {
    //        double Result = 0;
    //        int tbl_indexY;
    //        int tbl_indexX;

    //        double step;

    //        double tmp;
    //        double tmpPoint1;
    //        double tmpPoint2;

    //        // Get Table Index X and Y
    //        tbl_indexY = GetErrTableYIndexExt(a_y_ref);

    //        if (tbl_indexY == -1)
    //        {
    //            return 0;
    //        }

    //        tbl_indexX = GetErrTableXIndexExt(tbl_indexY, a_x_ref);

    //        if (tbl_indexX == -1)
    //        {
    //            return 0;
    //        }

    //        step = GetErrStepXExt(tbl_indexX, tbl_indexY);

    //        tmp = a_x_ref - ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX].PositionX;

    //        tmpPoint1 = (ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY + 1].ListY[tbl_indexX + 1].ErrorValue - ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY + 1].ListY[tbl_indexX].ErrorValue) * (tmp / step)
    //                    + ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY + 1].ListY[tbl_indexX].ErrorValue;

    //        tmpPoint2 = (ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX + 1].ErrorValue - ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX].ErrorValue) * (tmp / step)
    //                    + ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX].ErrorValue;

    //        step = GetErrStepYExt(tbl_indexX, tbl_indexY);

    //        //if (FlagExtraTableOnPin == 1 && FalgUseExtraTableForPin == 1)
    //        //{
    //        //    tmp = a_x_ref - POS_TBL_X_EXT_FOR_PIN[tbl_indexX];
    //        //}
    //        //else
    //        //{
    //        //    tmp = a_x_ref - POS_TBL_X_EXT[tbl_indexX];
    //        //}

    //        tmp = a_y_ref - ErrorTable2D.TBL_OX_STRAIGHT[tbl_indexY].PositionY;

    //        Result = (tmpPoint2 - tmpPoint1) * (tmp / step) + tmpPoint1;

    //        return Result;
    //    }

    //    CompensationValue Result = new CompensationValue();
    //    public CompensationValue GetErrorComp(CompensationPos CPos)
    //    {
    //        Result.XValue = 0;
    //        Result.YValue = 0;

    //        Result.XValue = GetErrorComp_X_Ext(CPos.XPos, CPos.YPos);
    //        Result.YValue = GetErrorComp_Y_Ext(CPos.XPos, CPos.YPos);

    //        // Result.CValue = GetErrorComp_C(CPos.Pos[(int)EnumAxisConstants.X], CPos.Pos[(int)EnumAxisConstants.Y]);

    //        return Result;
    //    }

    //    public EventCodeEnum InitModule()
    //    {
    //        EventCodeEnum retval = EventCodeEnum.UNDEFINED;

    //        try
    //        {
    //            if (Initialized == false)
    //            {
    //                Initialized = true;

    //                retval = EventCodeEnum.NONE;
    //            }
    //            else
    //            {
    //                LoggerManager.DebugError($"DUPLICATE_INVOCATION");

    //                retval = EventCodeEnum.DUPLICATE_INVOCATION;
    //            }
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }

    //        return retval;
    //    }
    //    public void DeInitModule()
    //    {

    //    }
    //    public int GetErrorComp(ObservableCollection<ProbeAxisObject> ProbeAxes)
    //    {
    //        return 1;
    //    }

    //    public EventCodeEnum InitModule(Autofac.IContainer container, object param)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public object GetErrorTable()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum LoadSysParameter()
    //    {
    //        EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

    //        //ErrorTable2D = new SecondErrorTable();

    //        //object deserializedObj;

    //        //string FullPath;

    //        //string RootPath = this.FileManager().FileManagerParam.SystemParamRootDirectory;
    //        //string FilePath = ErrorTable2D.FilePath;
    //        //string FileName = ErrorTable2D.FileName;

    //        //if (FilePath != "")
    //        //{
    //        //    FullPath = RootPath + "\\" + FilePath + "\\" + FileName;
    //        //}
    //        //else
    //        //{
    //        //    FullPath = RootPath + "\\" + FileName;
    //        //}

    //        try
    //        {
    //            //if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
    //            //{
    //            //    Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
    //            //}

    //            //if (File.Exists(FullPath) == false)
    //            //{
    //            //    RetVal = ErrConvert.ConvertError2DTable(FullPath);

    //            //    if (RetVal == EventCodeEnum.NODATA)
    //            //    {
    //            //        LoggerManager.DebugError($"[SecondCompensationModule] LoadSysParam(): Serialize Error");

    //            //        return RetVal;
    //            //    }
    //            //}

    //            IParam tmpParam = null;
    //            //RetVal = this.LoadParameter(ref tmpParam, typeof(SecondErrorTable), null, FullPath);
    //            RetVal = this.LoadParameter(ref tmpParam, typeof(SecondErrorTable));
    //            ErrorTable2D = tmpParam as SecondErrorTable;

    //            RetVal = EventCodeEnum.NONE;
    //        }
    //        catch (Exception err)
    //        {
    //            RetVal = EventCodeEnum.PARAM_ERROR;
    //            //LoggerManager.Error($String.Format("[SecondCompensationModule] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
    //            LoggerManager.Exception(err);

    //            throw;
    //        }

    //        return RetVal;
    //    }

    //    public EventCodeEnum SaveSysParameter()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    //public EventCodeEnum LoadParameter()
    //    //{
    //    //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

    //    //    RetVal = LoadSysParameter();

    //    //    return RetVal;
    //    //}

    //    public EventCodeEnum SaveParameter()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public CatCoordinates GetErrorComp(CatCoordinates Pos)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    //public class SecondPinCompensationModule : INotifyPropertyChanged, ICompensationModule
    //{
    //    #region ==> PropertyChanged
    //    public event PropertyChangedEventHandler PropertyChanged;

    //    protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion


    //    public bool Initialized { get; set; } = false;

    //    private ErrorTableConverter ErrConvert = new ErrorTableConverter();
    //    private SecondErrorTable ErrorTablePin2D;

    //    private bool _Enable;
    //    public bool Enable
    //    {
    //        get { return _Enable; }
    //        set
    //        {
    //            if (value != _Enable)
    //            {
    //                _Enable = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    private double _m_sqr_angle;
    //    public double m_sqr_angle
    //    {
    //        get { return _m_sqr_angle; }
    //        set
    //        {
    //            if (value != this._m_sqr_angle)
    //            {
    //                _m_sqr_angle = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    //public EventCodeEnum LoadErrorTable(string Filepath)
    //    //{
    //    //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

    //    //    try
    //    //    {
    //    //        object deserializedObj;
    //    //        if (Directory.Exists(System.IO.Path.GetDirectoryName(Filepath)) == false)
    //    //        {
    //    //            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Filepath));
    //    //        }
    //    //        if (File.Exists(Filepath) == false)
    //    //        {
    //    //            RetVal = ErrConvert.ConvertError2DPinTable(Filepath);
    //    //        }

    //    //        IParam tmpParam = null;
    //    //        RetVal = this.LoadParameter(ref tmpParam, typeof(SecondErrorTable), null, Filepath);
    //    //        ErrorTablePin2D = tmpParam as SecondErrorTable;
    //    //    }
    //    //    catch (Exception err)
    //    //    {
    //    //        RetVal = EventCodeEnum.UNDEFINED;

    //    //        //LoggerManager.Error($String.Format("LoadMotionParam(): Error occurred while loading parameters. Err = {0}", err.Message));
    //    //        LoggerManager.Exception(err);

    //    //        throw;
    //    //    }

    //    //    return RetVal;
    //    //}

    //    private double GetErrorComp_X_Ext(double x_ref, double y_ref)
    //    {
    //        double comp_x = 0;

    //        comp_x = GetLinearErrorXExt(x_ref, y_ref);

    //        return comp_x;
    //    }
    //    private double GetErrorComp_Y_Ext(double x_ref, double y_ref)
    //    {
    //        double comp_y = 0;

    //        comp_y = GetStraightnessErrorYExt(x_ref, y_ref);

    //        return comp_y;
    //    }

    //    private int GetErrTableXIndexExt(int indexY, double refX)
    //    {
    //        int Index = 0;

    //        int LastIndex;

    //        LastIndex = ErrorTablePin2D.TBL_OX_LINEAR[indexY].ListY.Count - 1;

    //        for (Index = 0; Index < LastIndex; Index++)
    //        {
    //            if ((ErrorTablePin2D.TBL_OX_LINEAR[indexY].ListY[Index].PositionX <= refX) && (ErrorTablePin2D.TBL_OX_LINEAR[indexY].ListY[Index + 1].PositionX > refX))
    //            {
    //                break;
    //            }
    //        }

    //        if ((Index < 0) || (Index > LastIndex))
    //        {
    //            return -1;
    //        }

    //        return Index;
    //    }

    //    private int GetErrTableYIndexExt(double refY)
    //    {
    //        int Index = 0;

    //        int LastIndex;

    //        LastIndex = ErrorTablePin2D.TBL_OX_LINEAR.Count - 1;

    //        for (Index = 0; Index < LastIndex; Index++)
    //        {
    //            if ((ErrorTablePin2D.TBL_OX_LINEAR[Index].PositionY <= refY) && (ErrorTablePin2D.TBL_OX_LINEAR[Index + 1].PositionY > refY))
    //            {
    //                break;
    //            }
    //        }

    //        if ((Index < 0) || (Index > LastIndex))
    //        {
    //            return -1;
    //        }

    //        return Index;
    //    }

    //    private double GetErrStepXExt(int Xindex, int Yindex)
    //    {
    //        double step;

    //        step = 0;

    //        step = ErrorTablePin2D.TBL_OX_LINEAR[Yindex].ListY[Xindex + 1].PositionX - ErrorTablePin2D.TBL_OX_LINEAR[Yindex].ListY[Xindex].PositionX;

    //        //if (FlagExtraTableOnPin == 1 && FlagUseExtraTableForPin == 1)
    //        //{
    //        //    if (Xindex < 0)
    //        //    {
    //        //        step = POS_TBL_X_EXT_FOR_PIN[1] - POS_TBL_X_EXT_FOR_PIN[0];
    //        //        return step;
    //        //    }
    //        //    else if (Xindex > Table_Constants.MAX_ERR_TABLE)
    //        //    {
    //        //        step = POS_TBL_X_EXT_FOR_PIN[Table_Constants.MAX_ERR_TABLE] - POS_TBL_X_EXT_FOR_PIN[Table_Constants.MAX_ERR_TABLE - 1];
    //        //        return step;
    //        //    }
    //        //    else
    //        //    {
    //        //        step = POS_TBL_X_EXT_FOR_PIN[Xindex + 1] - POS_TBL_X_EXT_FOR_PIN[Xindex];
    //        //    }
    //        //}
    //        //else
    //        //{
    //        //    if (Xindex < 0)
    //        //    {
    //        //        step = POS_TBL_X_EXT[1] - POS_TBL_X_EXT[0];
    //        //        return step;
    //        //    }
    //        //    else if (Xindex > Table_Constants.MAX_ERR_TABLE)
    //        //    {
    //        //        step = POS_TBL_X_EXT[Table_Constants.MAX_ERR_TABLE] - POS_TBL_X_EXT[Table_Constants.MAX_ERR_TABLE - 1];
    //        //        return step;
    //        //    }
    //        //    else
    //        //    {
    //        //        step = POS_TBL_X_EXT[Xindex + 1] - POS_TBL_X_EXT[Xindex];
    //        //    }

    //        //}

    //        if (step < 0)
    //        {
    //            step = step * -1;
    //        }

    //        return step;
    //    }

    //    private double GetErrStepYExt(int Xindex, int Yindex)
    //    {
    //        double step;

    //        step = 0;

    //        step = ErrorTablePin2D.TBL_OX_LINEAR[Yindex].PositionY - ErrorTablePin2D.TBL_OX_LINEAR[Yindex + 1].PositionY;

    //        if (step < 0)
    //        {
    //            step = step * -1;
    //        }

    //        return step;
    //    }

    //    private double GetLinearErrorXExt(double a_x_ref, double a_y_ref)
    //    {
    //        double Result = 0;
    //        int tbl_indexY;
    //        int tbl_indexX;

    //        double step;

    //        double tmp;
    //        double tmpPoint1;
    //        double tmpPoint2;

    //        // Get Table Index X and Y
    //        tbl_indexY = GetErrTableYIndexExt(a_y_ref);

    //        if (tbl_indexY == -1)
    //        {
    //            return 0;
    //        }

    //        tbl_indexX = GetErrTableXIndexExt(tbl_indexY, a_x_ref);

    //        if (tbl_indexX == -1)
    //        {
    //            return 0;
    //        }

    //        step = GetErrStepXExt(tbl_indexX, tbl_indexY);

    //        tmp = a_x_ref - ErrorTablePin2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].PositionX;

    //        tmpPoint1 = (ErrorTablePin2D.TBL_OX_LINEAR[tbl_indexY + 1].ListY[tbl_indexX + 1].ErrorValue - ErrorTablePin2D.TBL_OX_LINEAR[tbl_indexY + 1].ListY[tbl_indexX].ErrorValue) * (tmp / step)
    //                    + ErrorTablePin2D.TBL_OX_LINEAR[tbl_indexY + 1].ListY[tbl_indexX].ErrorValue;

    //        tmpPoint2 = (ErrorTablePin2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX + 1].ErrorValue - ErrorTablePin2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].ErrorValue) * (tmp / step)
    //                    + ErrorTablePin2D.TBL_OX_LINEAR[tbl_indexY].ListY[tbl_indexX].ErrorValue;

    //        step = GetErrStepYExt(tbl_indexX, tbl_indexY);

    //        //if (FlagExtraTableOnPin == 1 && FalgUseExtraTableForPin == 1)
    //        //{
    //        //    tmp = a_x_ref - POS_TBL_X_EXT_FOR_PIN[tbl_indexX];
    //        //}
    //        //else
    //        //{
    //        //    tmp = a_x_ref - POS_TBL_X_EXT[tbl_indexX];
    //        //}

    //        tmp = a_y_ref - ErrorTablePin2D.TBL_OX_LINEAR[tbl_indexY].PositionY;

    //        Result = (tmpPoint2 - tmpPoint1) * (tmp / step) + tmpPoint1;

    //        return Result;
    //    }

    //    private double GetStraightnessErrorYExt(double a_x_ref, double a_y_ref)
    //    {
    //        double Result = 0;
    //        int tbl_indexY;
    //        int tbl_indexX;

    //        double step;

    //        double tmp;
    //        double tmpPoint1;
    //        double tmpPoint2;

    //        // Get Table Index X and Y
    //        tbl_indexY = GetErrTableYIndexExt(a_y_ref);

    //        if (tbl_indexY == -1)
    //        {
    //            return 0;
    //        }

    //        tbl_indexX = GetErrTableXIndexExt(tbl_indexY, a_x_ref);

    //        if (tbl_indexX == -1)
    //        {
    //            return 0;
    //        }

    //        step = GetErrStepXExt(tbl_indexX, tbl_indexY);

    //        tmp = a_x_ref - ErrorTablePin2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX].PositionX;

    //        tmpPoint1 = (ErrorTablePin2D.TBL_OX_STRAIGHT[tbl_indexY + 1].ListY[tbl_indexX + 1].ErrorValue - ErrorTablePin2D.TBL_OX_STRAIGHT[tbl_indexY + 1].ListY[tbl_indexX].ErrorValue) * (tmp / step)
    //                    + ErrorTablePin2D.TBL_OX_STRAIGHT[tbl_indexY + 1].ListY[tbl_indexX].ErrorValue;

    //        tmpPoint2 = (ErrorTablePin2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX + 1].ErrorValue - ErrorTablePin2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX].ErrorValue) * (tmp / step)
    //                    + ErrorTablePin2D.TBL_OX_STRAIGHT[tbl_indexY].ListY[tbl_indexX].ErrorValue;

    //        step = GetErrStepYExt(tbl_indexX, tbl_indexY);

    //        //if (FlagExtraTableOnPin == 1 && FalgUseExtraTableForPin == 1)
    //        //{
    //        //    tmp = a_x_ref - POS_TBL_X_EXT_FOR_PIN[tbl_indexX];
    //        //}
    //        //else
    //        //{
    //        //    tmp = a_x_ref - POS_TBL_X_EXT[tbl_indexX];
    //        //}

    //        tmp = a_y_ref - ErrorTablePin2D.TBL_OX_STRAIGHT[tbl_indexY].PositionY;

    //        Result = (tmpPoint2 - tmpPoint1) * (tmp / step) + tmpPoint1;

    //        return Result;
    //    }

    //    CompensationValue Result = new CompensationValue();

    //    public CompensationValue GetErrorComp(CompensationPos CPos)
    //    {
    //        Result.XValue = 0;
    //        Result.YValue = 0;

    //        Result.XValue = GetErrorComp_X_Ext(CPos.XPos, CPos.YPos);
    //        Result.YValue = GetErrorComp_Y_Ext(CPos.XPos, CPos.YPos);

    //        // Result.CValue = GetErrorComp_C(CPos.Pos[(int)EnumAxisConstants.X], CPos.Pos[(int)EnumAxisConstants.Y]);

    //        return Result;
    //    }

    //    public EventCodeEnum InitModule()
    //    {
    //        EventCodeEnum retval = EventCodeEnum.UNDEFINED;

    //        try
    //        {
    //            if (Initialized == false)
    //            {
    //                Initialized = true;

    //                retval = EventCodeEnum.NONE;
    //            }
    //            else
    //            {
    //                LoggerManager.DebugError($"DUPLICATE_INVOCATION");

    //                retval = EventCodeEnum.DUPLICATE_INVOCATION;
    //            }
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //        }

    //        return retval;
    //    }
    //    public void DeInitModule()
    //    {

    //    }
    //    public int GetErrorComp(ObservableCollection<ProbeAxisObject> ProbeAxes)
    //    {
    //        return 1;
    //    }

    //    public EventCodeEnum InitModule(Autofac.IContainer container, object param)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public object GetErrorTable()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public EventCodeEnum LoadSysParameter()
    //    {
    //        EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

    //        ErrorTablePin2D = new SecondErrorTable();

    //        object deserializedObj;

    //        string FullPath;

    //        string RootPath = this.FileManager().FileManagerParam.SystemParamRootDirectory;
    //        string FilePath = ErrorTablePin2D.FilePath;
    //        string FileName = ErrorTablePin2D.FileName;

    //        if (FilePath != "")
    //        {
    //            FullPath = RootPath + "\\" + FilePath + "\\" + FileName;
    //        }
    //        else
    //        {
    //            FullPath = RootPath + "\\" + FileName;
    //        }

    //        try
    //        {
    //            if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
    //            {
    //                Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
    //            }

    //            if (File.Exists(FullPath) == false)
    //            {
    //                RetVal = ErrConvert.ConvertError2DPinTable(FullPath);

    //                if (RetVal == EventCodeEnum.NODATA)
    //                {
    //                    LoggerManager.DebugError($"[SecondPinCompensationModule] LoadSysParam(): Serialize Error");
    //                    return RetVal;
    //                }
    //            }

    //            IParam tmpParam = null;
    //            RetVal = this.LoadParameter(ref tmpParam, typeof(SecondErrorTable), null, FullPath);
    //            ErrorTablePin2D = tmpParam as SecondErrorTable;
    //        }
    //        catch (Exception err)
    //        {
    //            RetVal = EventCodeEnum.PARAM_ERROR;
    //            //LoggerManager.Error($String.Format("[SecondPinCompensationModule] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
    //            LoggerManager.Exception(err);

    //            throw;
    //        }

    //        return RetVal;
    //    }

    //    public EventCodeEnum SaveSysParameter()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    //public EventCodeEnum LoadParameter()
    //    //{
    //    //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

    //    //    RetVal = LoadSysParameter();

    //    //    return RetVal;
    //    //}

    //    public EventCodeEnum SaveParameter()
    //    {
    //        EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
    //        return retVal;
    //    }

    //    public CatCoordinates GetErrorComp(CatCoordinates Pos)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}


//// Made by Yang 140124
//int tbl_indexY;
//int tbl_indexX;
//double step;
//double error, tmp;

//// Add by Yang 140918
//double tmpPoint1;
//double tmppoint2;

//tbl_indexY = GetErrTableYIndexExt(a_y_ref);
//tbl_indexX = GetErrTableXIndexExt(a_x_ref);

//	// Modified by Yang 140918

//	if (tbl_indexX>(MAX_ERR_TABLE-2) || tbl_indexX<0) return 0.0;	
//	if (tbl_indexY>(MAX_ERR_TABLE-2) || tbl_indexY<0) return 0.0;

//	step = GetErrStep(COMP_MODE_Y_LIN, tbl_indexY);

//	tmp = a_y_ref - POS_TBL_Y_EXT[tbl_indexY];

//	tmpPoint1 = (TBL_X_LINEAR_EXT[tbl_indexY + 1][tbl_indexX]-TBL_X_LINEAR_EXT[tbl_indexY][tbl_indexX])*(tmp/step)
//		+ TBL_X_LINEAR_EXT[tbl_indexY][tbl_indexX]; ok

//	tmppoint2 = (TBL_X_LINEAR_EXT[tbl_indexY + 1][tbl_indexX + 1]-TBL_X_LINEAR_EXT[tbl_indexY][tbl_indexX + 1])*(tmp/step)
//		+ TBL_X_LINEAR_EXT[tbl_indexY][tbl_indexX + 1];

//	step = GetErrStepExt(tbl_indexX, tbl_indexY);

//	tmp = a_x_ref - POS_TBL_X_EXT[tbl_indexX];

//	error = (tmppoint2-tmpPoint1)*(tmp/step) + tmpPoint1;

//	return error;
