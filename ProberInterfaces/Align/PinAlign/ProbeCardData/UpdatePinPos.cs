using LogModule;
using ProberInterfaces.Param;
using System;
using System.ComponentModel;

namespace ProberInterfaces.PinAlign.ProbeCardData
{

    public interface IUpdatePinPos
    {
        PinCoordinate RealPinPos { get; set; }
        PinCoordinate ProbeCardCenterOffset { get; set; }
    }

    [Serializable]
    public class UpdatePinPos : IUpdatePinPos, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public UpdatePinPos()
        {
            try
            {
            _RealPinPos = new PinCoordinate();
            _ProbeCardCenterOffset = new PinCoordinate();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public UpdatePinPos(PinCoordinate realPin) : this()
        {
            _RealPinPos = realPin == null ? null : new PinCoordinate(realPin);
        }

        public UpdatePinPos(PinCoordinate realPin, PinCoordinate estPin) : this()
        {
            try
            {
            _RealPinPos = realPin == null ? null : new PinCoordinate(realPin);
            _EstimatePinPos = estPin == null ? null : new PinCoordinate(estPin);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public UpdatePinPos(PinCoordinate realPin, PinCoordinate estPin, PinCoordinate probeCardCenterOffset) : this()
        {
            try
            {
            _RealPinPos = realPin == null ? null :  new PinCoordinate(realPin);
            _EstimatePinPos = estPin == null ? null : new PinCoordinate(estPin);
            _ProbeCardCenterOffset = probeCardCenterOffset == null ? null : new PinCoordinate(probeCardCenterOffset);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public UpdatePinPos(UpdatePinPos updatePinData)
        {
            try
            {
            _RealPinPos = new PinCoordinate(updatePinData.RealPinPos);
            _EstimatePinPos = new PinCoordinate(updatePinData.EstimatePinPos);
            _ProbeCardCenterOffset = new PinCoordinate(updatePinData.ProbeCardCenterOffset);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private PinCoordinate _RealPinPos;
        public PinCoordinate RealPinPos
        {
            get { return _RealPinPos; }
            set
            {
                if (value != _RealPinPos)
                {
                    _RealPinPos = value;
                    NotifyPropertyChanged("RealPinPos");
                }
            }
        }

        private PinCoordinate _EstimatePinPos;
        public PinCoordinate EstimatePinPos
        {
            get { return _EstimatePinPos; }
            set
            {
                if (value != _EstimatePinPos)
                {
                    _EstimatePinPos = value;
                    NotifyPropertyChanged("EstimatePinPos");
                }
            }
        }


        private PinCoordinate _ProbeCardCenterOffset;
        public PinCoordinate ProbeCardCenterOffset
        {
            get { return _ProbeCardCenterOffset; }
            set
            {
                if (value != _ProbeCardCenterOffset)
                {
                    _ProbeCardCenterOffset = value;
                    NotifyPropertyChanged("ProbeCardCenterOffset");
                }
            }
        }

        // [TODO] 온도, 시간 추가
    }
}
