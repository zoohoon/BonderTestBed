using LogModule;
using ProberErrorCode;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces.ResultMap.BaseMapFormat.Header.Extensions.STIF
{
    public class STIFMapHeaderExtension : MapHeaderExtensionBase, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private double _RefDieCenterToFDITargetCenterDistanceX;
        [ProberMapPropertyAttribute(EnumProberMapProperty.REFDIECENTERTOFDITARGETCENTERDISTANCEX)]
        public double RefDieCenterToFDITargetCenterDistanceX
        {
            get { return _RefDieCenterToFDITargetCenterDistanceX; }
            set
            {
                if (_RefDieCenterToFDITargetCenterDistanceX != value)
                {
                    _RefDieCenterToFDITargetCenterDistanceX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _RefDieCenterToFDITargetCenterDistanceY;
        [ProberMapPropertyAttribute(EnumProberMapProperty.REFDIECENTERTOFDITARGETCENTERDISTANCEY)]
        public double RefDieCenterToFDITargetCenterDistanceY
        {
            get { return _RefDieCenterToFDITargetCenterDistanceY; }
            set
            {
                if (_RefDieCenterToFDITargetCenterDistanceY != value)
                {
                    _RefDieCenterToFDITargetCenterDistanceY = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override Type ExtensionType { get; set; }

        public STIFMapHeaderExtension()
        {
            ExtensionType = this.GetType();
        }
        public override EventCodeEnum AssignProperties()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SetRefDieCenterToFDITargetCenterDistance();

                if(retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Error("[STIFMapHeaderExtension], AssignProperties() : SetRefDieCenterToFDITargetCenterDistance() Failed.");
                    return retval;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum SetRefDieCenterToFDITargetCenterDistance()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                RefDieCenterToFDITargetCenterDistanceX = 1;
                RefDieCenterToFDITargetCenterDistanceY = 1;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
