using System;

namespace Cognex.Win32Display
{
    using Cognex.InSight.Controls.Display;
    //using LogModule;
    using System.Windows.Forms;

    public class InsightDisplayController : IDisposable
    {
        public CvsInSightDisplay CvsDisplay
        {
            get;
            private set;
        }
        private CheckBox _Online;
        private CheckBox _LiveMode;
        private CheckBox _Graphics;

        public InsightDisplayController()
        {
            try
            {
                CvsDisplay = new CvsInSightDisplay();

                _Online = new CheckBox();
                _LiveMode = new CheckBox();
                _Graphics = new CheckBox();

                //CvsDisplay.LoadStandardTheme();
                CvsDisplay.Edit.SoftOnline.Bind(_Online);
                CvsDisplay.Edit.LiveAcquire.Bind(_LiveMode);
                CvsDisplay.Edit.ShowGraphics.Bind(_Graphics);
                CvsDisplay.ShowGraphics = false;
                _Graphics.Checked = false;

            }
            catch (Exception)
            {
                //LoggerManager.Exception(err);
                throw;
            }
        }

        public bool Connect(string hostName, string userName, string password)
        {
            if (CvsDisplay == null)
                return false;
            bool connected = true;
            try
            {
                CvsDisplay.Disconnect();
                CvsDisplay.Connect(hostName, userName, password, false);
            }
            catch
            {
                connected = false;
            }

            return connected;
        }
        public void DisConnect()
        {
            try
            {
                if (CvsDisplay == null)
                    return;

                if (CvsDisplay.Connected == false)
                    return;

                CvsDisplay.Disconnect();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void SwitchOnlineMode()
        {
            try
            {
                if (_Online.Enabled == false)
                    return;

                _Online.Checked = !_Online.Checked;
            }
            catch (Exception)
            {
                //LoggerManager.Exception(err);
                throw;
            }
        }
        public void SwitchLiveMode()
        {
            try
            {
                if (_LiveMode.Enabled == false)
                    return;

                _LiveMode.Checked = !_LiveMode.Checked;
            }
            catch (Exception)
            {
                //LoggerManager.Exception(err);
                throw;
            }
        }
        public void SwitchGraphics(bool flag)
        {
            try
            {
                //_Graphics.Checked = flag;
                CvsDisplay.ShowGraphics = flag;
            }
            catch (Exception)
            {
                //LoggerManager.Exception(err);
                throw;
            }
        }
        public void SwitchSpreadSheet()
        {
            CvsDisplay.ShowGrid = !CvsDisplay.ShowGrid;
        }
        public void Dispose()
        {
            DisConnect();
        }
    }
}
