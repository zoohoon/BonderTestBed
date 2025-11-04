using LogModule;
using ProberInterfaces.Temperature.TempManager;
using System;

namespace Temperature.Temp
{
    public class E5ENTempModule : ITempModule
    {
        private E5EN _e5en = new E5EN();

        bool disposed = false;

        public E5ENTempModule() { }

        public void Dispose()
        {
            Dispose(true);
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

                _e5en?.Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        ~E5ENTempModule()
        {
            Dispose(false);
        }

        public ITempUpdateEventArgs Get_Cur_Temp(int dataGetdelay = 0)
        {
            TempUpdateEventArgs tempArgs = null;

            try
            {
                tempArgs = _e5en?.Get_Cur_Temp(dataGetdelay);
                
                if(tempArgs != null)
                {
                    tempArgs.PV *= 0.1;
                    tempArgs.SV *= 0.1;
                    tempArgs.MV *= 0.1;

                    tempArgs.PV = Math.Round(tempArgs.PV, 2);
                    tempArgs.SV = Math.Round(tempArgs.SV, 2);
                    tempArgs.MV = Math.Round(tempArgs.MV, 2);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return tempArgs;
        }

        public double ReadMV()
        {
            long? result = _e5en?.ReadMV();
            long retVal = result == null ? -1 : (long)result;
            return (double)retVal / 10.0;
        }

        public double ReadPV()
        {
            long? result = _e5en?.ReadPV();
            long retVal = result == null ? -1 : (long)result;
            return (double)retVal / 10.0;
        }

        public double ReadSetV()
        {
            long? result = _e5en?.ReadSetV();
            long retVal = result == null ? -1 : (long)result;
            return (double)retVal / 10.0;
        }

        public long ReadStatus()
        {
            long? result = _e5en?.ReadStatus();
            long retVal = result == null ? -1 : (long)result;
            return retVal;
        }

        public long Get_OutPut_State()
        {
            long? result = _e5en?.Get_OutPut_State();
            long retVal = result == null ? -1 : (long)result;
            return retVal;
        }

        public void SetRemote_OFF(object notUsed)
        {
            try
            {
                _e5en?.SetRemote_OFF(notUsed);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetRemote_ON(object notUsed)
        {
            try
            {
                _e5en?.SetRemote_ON(notUsed);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetSV(double value)
        {
            try
            {
                double setValue = value * 10.0;
                _e5en?.SetSV(setValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Set_PB(double value)
        {
            try
            {
                double setValue = value * 10.0;
                _e5en?.Set_PB(setValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void Set_DT(double value)
        {
            try
            {
                double setValue = value * 10.0;
                _e5en?.Set_DT(setValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Set_IT(double value)
        {
            try
            {
                double setValue = value * 10.0;
                _e5en?.Set_IT(setValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Set_Offset(double value)
        {
            try
            {
                double setValue = Math.Round(value * 100) * -1.0;
                _e5en?.Set_Offset(setValue);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Set_OutPut_OFF(object notUsed)
        {
            try
            {
                _e5en?.Set_OutPut_OFF(notUsed);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public void Set_OutPut_ON(object notUsed)
        {
            try
            {
                _e5en?.Set_OutPut_ON(notUsed);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }


        public void WriteSingleRegister(int data, int value)
        {
            try
            {
                _e5en?.WriteSingleRegister(data, value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool Connect(string serialPort, byte UnitIdentifier)
        {
            bool? result = _e5en?.Connect(serialPort, UnitIdentifier);
            return result ?? false;
        }

        public void ChangeCurTemp(double temp)
        {
            //Not Using.
        }
    }
}
