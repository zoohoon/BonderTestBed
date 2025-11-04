using LogModule;
using ProberInterfaces;
using ProberInterfaces.Temperature;
using ProberInterfaces.Temperature.TempManager;
using System;

namespace Temperature.Temp
{
    public class EmulTempModule : ITempModule
    {
        bool disposed = false;

        private double _TargetTemp = 40.0;
        private double TargetTemp
        {
            get { return _TargetTemp; }
            set
            {
                if (_TargetTemp != value)
                {
                    _TargetTemp = value;
                }
            }
        }
        public bool OutputEnable { get; set; }
        private bool IsFirstCurTemp = true;
        public bool RemoteEnable { get; set; }

        public EmulTempModule() { }

        public void Dispose()
        {
            try
            {
                Dispose(true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void Dispose(bool disposing)
        {
            try
            {
                if (disposed)
                {
                    return;
                }

                if (disposing)
                {
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        ~EmulTempModule()
        {
            Dispose(false);
        }

        public ITempUpdateEventArgs Get_Cur_Temp(int dataGetdelay = 0)
        {
            double PV = 0;
            double SV = 0;
            double CurTemp = 0;
            double mv = 0.0;
            try
            {
                if (!IsFirstCurTemp)
                {
                    double nowCurTemp = this.TempController().TempInfo.CurTemp.Value;
                    //double nowCurTemp = GoalCurTempForEmul;
                    double diffValue = Math.Abs(TargetTemp - nowCurTemp);
                    Random rand = new Random();
                    mv = rand.NextDouble() * this.TempController().TempController().GetDeviaitionValue();

                    if (OutputEnable == true)
                    {
                        if (this.TempController().TempController().GetDeviaitionValue() * 3 < diffValue)
                        {
                            if (TargetTemp > nowCurTemp)
                            {
                                CurTemp = nowCurTemp + mv;
                            }
                            else
                            {
                                CurTemp = nowCurTemp - mv;
                            }
                        }
                        else if (this.TempController().TempController().GetDeviaitionValue() * 3 >= diffValue)
                        {
                            if (nowCurTemp < TargetTemp)
                            {
                                CurTemp = nowCurTemp + mv * 0.2;
                            }
                            else
                            {
                                CurTemp = nowCurTemp - mv * 0.2;
                            }

                        }
                    }
                    else
                    {
                        if ((this.EnvControlManager()?.GetChillerModule()?.ChillerParam?.ChillerModuleMode?.Value
                                     ?? EnumChillerModuleMode.NONE) != EnumChillerModuleMode.NONE)
                        {
                            if (this.EnvControlManager().ChillerManager.GetChillerModule().ChillerInfo.ChillerInternalTemp < nowCurTemp)
                            {
                                var chillingMV = nowCurTemp - this.EnvControlManager().ChillerManager.GetChillerModule().ChillerInfo.ChillerInternalTemp;
                                CurTemp = nowCurTemp - (chillingMV * 0.1 * mv);
                            }
                        }
                        else
                        {
                            nowCurTemp = 0;
                            CurTemp = TargetTemp;
                            nowCurTemp = this.TempController().TempInfo.CurTemp.Value;

                            if (CurTemp == nowCurTemp)
                            {
                                IsFirstCurTemp = false;
                            }
                        }
                    }
                }
                else
                {
                    double nowCurTemp = 0;
                    CurTemp = TargetTemp;
                    nowCurTemp = this.TempController().TempInfo.CurTemp.Value;

                    if (CurTemp == nowCurTemp)
                    {
                        IsFirstCurTemp = false;
                    }
                }

                //CurTemp = Math.Round(CurTemp, 0);
                PV = (double)(CurTemp);
                //if (this.TempController().TempInfo.SetTemp.Value ==
                //    (this.TempController().TempControllerDevParam as ITempControllerDevParam).SetTemp.Value)
                //{
                //    SV = (double)(this.TempController().TempInfo.SetTemp.Value);
                //}
                //else
                //{
                //    SV = (double)(this.TempController().TempControllerDevParam as ITempControllerDevParam).SetTemp.Value;
                //}
                SV = TargetTemp;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return new TempUpdateEventArgs(PV, SV, 0);
        }

        public double ReadMV()
        {
            return 0;
        }

        public double ReadPV()
        {
            return this.TempController().TempInfo.SetTemp.Value;
        }

        public double ReadSetV()
        {
            return this.TempController().TempInfo.CurTemp.Value;
        }

        public long ReadStatus()
        {
            return 0;
        }
        public long Get_OutPut_State()
        {
            if (OutputEnable == true)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        public void SetRemote_OFF(object notUsed)
        {
            RemoteEnable = false;
        }

        public void SetRemote_ON(object notUsed)
        {
            RemoteEnable = true;
        }

        public void SetSV(double value)
        {
            try
            {
                ChangeCurTemp(value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Set_DT(double value)
        {
        }

        public void Set_IT(double value)
        {
        }

        public void Set_Offset(double value)
        {
        }

        public void Set_OutPut_OFF(object notUsed)
        {
            if (OutputEnable == true)
            {
                LoggerManager.Debug($"[EmulTempModule] Temp. controller output disabled.");
            }
            OutputEnable = false;
        }

        public void Set_OutPut_ON(object notUsed)
        {
            if (OutputEnable == false)
            {
                LoggerManager.Debug($"[EmulTempModule] Temp. controller output enabled.");
            }
            OutputEnable = true;
        }

        public void Set_PB(double value)
        {
        }

        public void WriteSingleRegister(int data, int value)
        {
        }

        public bool Connect(string serialPort, byte UnitIdentifier)
        {
            return true;
        }

        public void ChangeCurTemp(double temp)
        {
            try
            {
                TargetTemp = temp;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
