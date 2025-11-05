using LogModule;
using ProberErrorCode;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces.ResultMap.BaseMapFormat.Header.Extensions.STIF
{
    public class E142MapHeaderExtension : MapHeaderExtensionBase, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private double _StepSizeX;
        [ProberMapPropertyAttribute(EnumProberMapProperty.STEPSIZEX)]
        public double StepSizeX
        {
            get { return _StepSizeX; }
            set
            {
                if (_StepSizeX != value)
                {
                    _StepSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _StepSizeY;
        [ProberMapPropertyAttribute(EnumProberMapProperty.STEPSIZEY)]
        public double StepSizeY
        {
            get { return _StepSizeY; }
            set
            {
                if (_StepSizeY != value)
                {
                    _StepSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }
        public override Type ExtensionType { get; set; }

        public E142MapHeaderExtension()
        {
            ExtensionType = this.GetType();
        }

        public override EventCodeEnum AssignProperties()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SetStepSize();

                if (retval != EventCodeEnum.NONE)
                {
                    LoggerManager.Error("[STIFMapHeaderExtension], AssignProperties() : SetStepSize() Failed.");
                    return retval;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum SetStepSize()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ISubstrateInfo waferSubstracteInfo = this.GetParam_Wafer()?.GetSubsInfo();

                if (waferSubstracteInfo != null)
                {
                    // Step Size = (Street Size + Die Size)

                    StepSizeX = waferSubstracteInfo.DieXClearance.Value + waferSubstracteInfo.ActualDieSize.Width.Value;
                    StepSizeY = waferSubstracteInfo.DieYClearance.Value + waferSubstracteInfo.ActualDieSize.Height.Value;
                    
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        
    }
}
