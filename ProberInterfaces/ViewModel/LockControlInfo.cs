using System;

namespace ProberInterfaces
{
    using LogModule;
    using System.Windows.Controls;
    public class LockControlInfo
    {
        private int _HashCode;

        public int HashCode
        {
            get { return _HashCode; }
            set { _HashCode = value; }
        }


        private UserControl _ViewControl;

        public UserControl ViewControl
        {
            get { return _ViewControl; }
            set { _ViewControl = value; }
        }

        private UserControl _TopBarControl;

        public UserControl TopBarControl
        {
            get { return _TopBarControl; }
            set { _TopBarControl = value; }
        }

        public LockControlInfo(int hashcode, UserControl viewcontrol, UserControl topbarcontrol)
        {
            try
            {
                HashCode = hashcode;
                ViewControl = viewcontrol;
                TopBarControl = topbarcontrol;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
    }
}
