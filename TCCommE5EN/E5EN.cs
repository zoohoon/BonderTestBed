using System;

namespace Temperature.Temp
{
    using EasyModbus;
    using LogModule;
    using ProberInterfaces.Temperature.TempManager;
    using System.Threading;
    ////using ProberInterfaces.ThreadSync;

    public class E5EN : IDisposable
    {

        public delegate void DataHandler(TempUpdateEventArgs args);
        public event DataHandler DataEvent;

        AutoResetEvent areUpdateIntv = new AutoResetEvent(false);
        //System.Threading.Timer _monitoringTimer;

        //Thread UpdateThread;
        //bool bStopUpdateThread;
        public ModbusClient TCClinet { get; set; }
        //private static readonly LockKey lockObject = new LockKey("E5EN");
        private static object lockObject = new object();

        #region // Properties
        private long _PV;

        public long PV
        {
            get { return _PV; }
            set
            {
                if (_PV != value)
                {
                    _PV = value;
                }
            }
        }

        private long _SV;
        public long SV
        {
            get { return _SV; }
            set
            {
                if (_SV != value)
                {
                    _SV = value;
                }
            }
        }

        private long _MV;
        public long MV
        {
            get { return _MV; }
            set
            {
                if (_MV != value)
                {
                    _MV = value;
                }
            }
        }

        #endregion

        public E5EN()
        {

        }
        ~E5EN()
        {
            Disconnect();
        }

        public bool Connect(string serialPort = "COM1", byte UnitIdentifier = 1)
        {
            bool retVal = false;

            TCClinet = new ModbusClient(serialPort);
            TCClinet.UnitIdentifier = 1;
            TCClinet.Connect();

            try
            {
                SV = ReadSetV();

                if (TCClinet.Connected)
                    retVal = true;
                else
                    retVal = false;
            }
            catch
            {
                Disconnect();
                retVal = false;
            }

            return retVal;
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void Disconnect()
        {
            TCClinet?.Disconnect();
        }

        private void OnTempUpdateEvent(TempUpdateEventArgs arg)
        {
            DataEvent?.Invoke(arg);
        }

        /// <summary>
        /// ..Cur temp(R)
        /// </summary>
        public TempUpdateEventArgs Get_Cur_Temp(int dataGetdelay = 0)
        {
            TempUpdateEventArgs retArgs = null;
            try
            {
                long mv = 0;
                long pv = 0;
                int[] cur_temp = ReadHoldingRegister(ModBusE5ENEnum.PV);
                System.Threading.Thread.Sleep(dataGetdelay);
                int[] h_mv = ReadHoldingRegister(ModBusE5ENEnum.MV_Monitor_Heating);
                System.Threading.Thread.Sleep(dataGetdelay);

                if (cur_temp != null & h_mv != null)
                {
                    if (cur_temp.Length == 2 && h_mv.Length == 2)
                    {
                        byte[] upperByte = BitConverter.GetBytes(cur_temp[0]);
                        byte[] lowerByte = BitConverter.GetBytes(cur_temp[1]);
                        byte[] tempInByte = new byte[8];
                        // Low nibble
                        for (int i = 0; i < 4; i++)
                        {
                            tempInByte[i] = lowerByte[i];
                        }
                        // Upper nibble
                        for (int i = 0; i < 4; i++)
                        {
                            tempInByte[i + 4] = upperByte[i];
                        }
                        pv = BitConverter.ToInt64(tempInByte, 0);
                        //pv = cur_temp[1] + cur_temp[0] * 65535;
                        //if(cur_temp[0] < 0)
                        //{
                        //    pv = cur_temp[1] + (cur_temp[0] + 1) * 65535 * -1;
                        //}
                        //else
                        //{
                        //    pv = cur_temp[1] + cur_temp[0] * 65535;
                        //}
                        
                        mv = h_mv[1];
                    }
                    else
                    {
                        throw new SystemException();
                    }
                    PV = pv;
                    MV = mv;
                    SV = ReadSetV();
                    //retArgs = new TempUpdateEventArgs(1, PV, SV, MV);
                    retArgs = new TempUpdateEventArgs(PV, SV, MV);
                }
                //OnTempUpdateEvent(new TempUpdateEventArgs(1, PV, SV, MV));
            }
            catch (SystemException err)
            {
                LoggerManager.Exception(err);
                throw new Exception("System exception occurred while read PV.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw new Exception("Exception error while read PV.");
            }

            return retArgs;
        }


        #region Read
        public long ReadSetV()
        {
            int[] set_temp = new int[2];
            int sv = 0;

            try
            {
                set_temp = ReadHoldingRegister(ModBusE5ENEnum.R_Set_Point);
                sv = set_temp[1];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return sv;
        }
        public long ReadPV()
        {
            int[] cur_temp = new int[2];
            int pv = 0;
            try
            {
                cur_temp = ReadHoldingRegister(ModBusE5ENEnum.PV);
                // 32Bit conversion
                if(cur_temp[1] < 0)
                {
                    cur_temp[1] -= 0xFFFE; 
                }
                
                pv = cur_temp[1];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return pv;
        }
        public long ReadMV()
        {
            int[] h_mv = new int[2];
            int mv = 0;
            try
            {
                h_mv = ReadHoldingRegister(ModBusE5ENEnum.MV_Monitor_Heating);
                mv = h_mv[1];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return mv;
        }
        public long ReadStatus() //
        {
            int[] status = new int[2];
            int stus = 0;
            try
            {
                status = ReadHoldingRegister(ModBusE5ENEnum.Status);
                stus = status[1];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return stus;
        }
        public long Get_OutPut_State()
        {
            int[] output_state = new int[2];
            long retVal = -1;
            try
            {
                output_state = ReadHoldingRegister(ModBusE5ENEnum.Status);
                retVal = ~((output_state[0] >> 8) & 0x01) & 0x01;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region Write
        //.. Set SV
        public void SetSV(double value)
        {
            try
            {
                //int setValue;

                //setValue = (int)(value * (double)10.0);
                WriteSingleRegister(ModBusE5ENEnum.W_Set_Point, (int)value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //.. Set Remote_ON
        public void SetRemote_ON(object notUsed)
        {
            try
            {
                WriteSingleRegister(ModBusE5ENEnum.Set_OP_Com_Wrt, (int)ModBusE5ENEnum.ON);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //.. Set Remote OFF
        public void SetRemote_OFF(object notUsed)
        {
            try
            {
                WriteSingleRegister(ModBusE5ENEnum.Set_OP_Com_Wrt, (int)ModBusE5ENEnum.OFF);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //..Set Run
        public void Set_OutPut_ON(object notUsed)
        {
            try
            {
                WriteSingleRegister(ModBusE5ENEnum.Set_OP, (int)ModBusE5ENEnum.RUN);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //..Set Stop
        public void Set_OutPut_OFF(object notUsed)
        {
            try
            {
                WriteSingleRegister(ModBusE5ENEnum.Set_OP, (int)ModBusE5ENEnum.STOP);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //..Set offset
        public void Set_Offset(double value)
        {
            try
            {
                WriteSingleRegister(ModBusE5ENEnum.Temp_Offset, (int)value);
                LoggerManager.Debug($"Set_Offset(): Set temp. offset to {value}(raw value).");    
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //..Set PB
        public void Set_PB(double value)
        {
            try
            {
                //int setValue;

                WriteSingleRegister(ModBusE5ENEnum.Set_PID_PB, (int)value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //..Set IT
        public void Set_IT(double value)
        {
            try
            {
                //int setValue;

                WriteSingleRegister(ModBusE5ENEnum.SET_PID_IT, (int)value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //..Set DT
        public void Set_DT(double value)
        {
            try
            {
                //int setValue;

                WriteSingleRegister(ModBusE5ENEnum.SET_PID_DT, (int)value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        //Read
        private int[] ReadHoldingRegister(ModBusE5ENEnum enumdata)
        {
            //using (Locker locker = new Locker(lockObject))
            //{
            lock (lockObject)
            {
                try
                {
                    int[] result = TCClinet.ReadHoldingRegisters((int)enumdata, 2);
                    return result;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw err;
                }
            }
        }

        //Write
        public void WriteSingleRegister(ModBusE5ENEnum enumdata, int value)
        {
            //using (Locker locker = new Locker(lockObject))
            //{
            lock (lockObject)
            {
                try
                {
                    TCClinet.WriteSingleRegister((int)enumdata, value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw err;
                }
            }
        }

        public void WriteSingleRegister(int data, int value)
        {
            //using (Locker locker = new Locker(lockObject))
            //{
            lock (lockObject)
            {
                try
                {
                    TCClinet.WriteSingleRegister(data, value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw err;
                }
            }
        }

        public void WriteMuliteRegister(ModBusE5ENEnum enumdata, int[] value)
        {
            //using (Locker locker = new Locker(lockObject))
            //{
            lock (lockObject)
            {
                try
                {
                    TCClinet.WriteMultipleRegisters((int)enumdata, value);
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw err;
                }
            }
        }
    }


    public class TempUpdateEventArgs : ITempUpdateEventArgs
    {
        private double _PV;

        public double PV
        {
            get { return _PV; }
            set { _PV = value; }
        }

        private double _SV;

        public double SV
        {
            get { return _SV; }
            set { _SV = value; }
        }

        private double _MV;

        public double MV
        {
            get { return _MV; }
            set { _MV = value; }
        }
        
        public TempUpdateEventArgs(double pv, double sv, double mv)
        {
            _PV = pv;
            _SV = sv;
            _MV = mv;
        }

    }

}