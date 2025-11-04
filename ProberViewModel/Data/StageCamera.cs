using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberViewModel.Data
{
    public enum enumStageCamType
    {
        UNDEFINED,
        WaferHigh,
        WaferLow,
        PinHigh,
        PinLow,
        WaferHighNC,
        MAP_REF,
    }

    public enum StageCamEnum
    {
        WAFER_HIGH_CAM,
        WAFER_LOW_CAM,
        PIN_HIGH_CAM,
        PIN_LOW_CAM,
    }

    public enum LocationEnum
    {
        UpperLeft,
        Up,
        UpperRight,
        Left,
        Center,
        Right,
        LowerLeft,
        Down,
        LowerRight,
    }

    public class StageCamera : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public enumStageCamType CamType { get; set; }
        public StageCamera(enumStageCamType camtype)
        {
            CamType = camtype;
        }
        public override string ToString()
        {
            return CamType.ToString();
        }
    }
}
