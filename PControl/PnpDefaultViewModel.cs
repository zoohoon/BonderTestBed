using ProberInterfaces;
using System.Windows;

namespace PnPControl
{
    public class PnpDefaultViewModel : PNPSetupBase
    {
        private Visibility _VisibilityZoomIn;
        public Visibility VisibilityZoomIn
        {
            get { return _VisibilityZoomIn; }
            set
            {
                if (value != _VisibilityZoomIn)
                {
                    _VisibilityZoomIn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibilityZoomOut;
        public Visibility VisibilityZoomOut
        {
            get { return _VisibilityZoomOut; }
            set
            {
                if (value != _VisibilityZoomOut)
                {
                    _VisibilityZoomOut = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _VisibilityMoveToCenter;
        public Visibility VisibilityMoveToCenter
        {
            get { return _VisibilityMoveToCenter; }
            set
            {
                if (value != _VisibilityMoveToCenter)
                {
                    _VisibilityMoveToCenter = value;
                    RaisePropertyChanged();
                }
            }
        }
      
        public override void SetStepSetupState(string header = null)
        {
            return;
        }
    }
}
