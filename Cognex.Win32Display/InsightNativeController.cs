using System;

namespace Cognex.Win32Display
{
    using Cognex.InSight.NativeMode;
    //using LogModule;

    public class InsightNativeController : IDisposable
    {
        private CvsNativeModeClient _CvsNative;
        public InsightNativeController()
        {
            _CvsNative = new CvsNativeModeClient();
        }
        public bool Connect(string hostName, string userName, string password)
        {
            if (_CvsNative == null)
                return false;

            //==> Cognex Module에 연결
            _CvsNative.Connect(hostName, userName, password);

            return _CvsNative.Connected;
        }
        public void DisConnect()
        {
            try
            {
                if (_CvsNative == null)
                    return;

                if (_CvsNative.Connected == false)
                    return;

                _CvsNative.Disconnect();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Dispose()
        {
            try
            {
                DisConnect();
            }
            catch(Exception)
            {
                //LoggerManager.Exception(err);
                throw;
            }
        }
        public String SendCommand(String command)
        {
            String response = String.Empty;
            try
            {
                if (_CvsNative.Connected == false)
                    return response;

                if (command.Length < 1)
                    return response;

                //==> Cognex Module에 명령을 보냄
                response = _CvsNative.SendCommand(command.Trim());

            }
            catch (Exception)
            {
                //LoggerManager.Exception(err);
                throw;
            }
            return response;
        }
    }
}
