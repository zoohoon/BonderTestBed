using LogModule;
using System;
using TwinCAT.Ads;

namespace TwinCatHelper
{
    #region ADSRouter
    public enum EnumDataType
    {
        Type_Undefined = -1,
        BOOL = 0,
        BYTE,
        WORD,
        DWORD,
        SINT,
        INT,
        DINT,
        USINT,
        UINT,
        UDINT,
        REAL,
        LREAL,
        //STRUCT,
        Type_Last = LREAL
    }
    public enum EnumVariableType
    {
        Type_Undefined = -1,
        VAR = 0,
        ARR,
        STRUCTURE,
        Type_Last ,
        STRUCTUREDARRAY = Type_Last,
    }

    public class ADSRouter : IDisposable
    {
        

        private bool disposed = false;

        public TcAdsClient tcClient;
        public string NetID;
        //private bool mBoolTCConnected = false;
        public bool BoolTCConnected
        {
            get
            {
                if (tcClient != null)
                {
                    return tcClient.IsConnected;
                }
                else
                {
                    return false;
                }
            }
        }

        public ADSRouter(string netid)
        {
            try
            {
                NetID = netid;
                tcClient = new TcAdsClient();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public int InitComm()
        {
            int retVal = -1;
            try
            {
                //tcClient.Connect(801); 
                if (string.IsNullOrEmpty(NetID) == false && NetID.Equals("0") == false) //  NetID 가 null 이라도 Connect 호출하면 tcClient.IsConnected 가 true가 되면서 emul 에서 문제 생김
                {
                    tcClient.Connect(NetID, 851);
                }
                //return -1;      // ToRemove: Temporarily prevent connect to ADS target.
                if (tcClient.IsConnected == true)
                {
                    try
                    {
                        LoggerManager.Debug($"TC Client(AMS Net. ID#[{NetID}]) connected. ");
                        InitVariableHandles();
                    }
                    catch (Exception err)
                    {
                        //LoggerManager.Error($string.Format("ADSRouter.InitComm(): Exception occurred. Error description = {0}", err.Message));
                        LoggerManager.Exception(err);

                        retVal = -1;
                    }

                    retVal = 0;
                }
                else
                {
                    LoggerManager.Error($"TC Client connection failed to AMS Net. ID#[{NetID}]");
                    retVal = -1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"ADSRouter.InitComm(): Error occurred while init. communication. Err = {err.Message}");
                throw;
            }
            return retVal;
        }



        private void InitVariableHandles()
        {
            try
            {
                //hbAxisEnableArray = tcClient.CreateVariableHandle(".gbEnableAmps");
                //hbMachineStatusStructure = tcClient.CreateVariableHandle(".gStMachineStatus");
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("ADSRouter.InitVariableHandles(): Exception occurred. Error description = {0}", err.Message));
                LoggerManager.Exception(err);

            }
        }

        public void DeInitComm()
        {
            try
            {
            if (tcClient.IsConnected == true)
            {
                tcClient.Dispose();
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public void Dispose()
        {
            try
            {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            try
            {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    //tcClient.DeleteDeviceNotification(hbHeartBeat);
                    //tcClient.DeleteVariableHandle(hbAxisEnableArray);
                    DeInitComm();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                disposed = true;

            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
    }
    #endregion
}
