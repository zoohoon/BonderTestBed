using System;

namespace ProberInterfaces.Param
{
    using Newtonsoft.Json;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public interface IPinBaseFiducialMarkParameter : INotifyPropertyChanged
    {
        PinCoordinate FiducialMarkPos { get; set; }
        PinCoordinate CardCenterOffset { get; set; }
    }

    [Serializable]
    public class PinBaseFiducialMarkParameter : IPinBaseFiducialMarkParameter
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private PinCoordinate _FiducialMarkPos = new PinCoordinate();
        public PinCoordinate FiducialMarkPos
        {
            get { return _FiducialMarkPos; }
            set
            {
                if (value != _FiducialMarkPos)
                {
                    _FiducialMarkPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PinCoordinate _CardCenterOffset = new PinCoordinate();
        public PinCoordinate CardCenterOffset
        {
            get { return _CardCenterOffset; }
            set
            {
                if (value != _CardCenterOffset)
                {
                    _CardCenterOffset = value;
                    RaisePropertyChanged();
                }
            }
        }

    }
}
