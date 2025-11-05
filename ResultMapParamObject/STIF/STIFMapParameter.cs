using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ResultMap;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ResultMapParamObject.STIF
{
    public enum RefCalcEnum
    {
        MANUAL,
        AUTO
    }

    [Serializable]
    public class STIFMapParameter : MapConverterParamBase, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Element<double> _Version = new Element<double>();
        public Element<double> Version
        {
            get { return _Version; }
            set
            {
                if (value != _Version)
                {
                    _Version = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Auto 계산이 안정화 되기 전, 임시로 메뉴얼로 입력 받아 사용,
        /// </summary>
        private Element<RefCalcEnum> _RefCalcMode = new Element<RefCalcEnum>();
        public Element<RefCalcEnum> RefCalcMode
        {
            get { return _RefCalcMode; }
            set
            {
                if (value != _RefCalcMode)
                {
                    _RefCalcMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _XRefManualValue = new Element<double>();
        public Element<double> XRefManualValue
        {
            get { return _XRefManualValue; }
            set
            {
                if (value != _XRefManualValue)
                {
                    _XRefManualValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _YRefManualValue = new Element<double>();
        public Element<double> YRefManualValue
        {
            get { return _YRefManualValue; }
            set
            {
                if (value != _YRefManualValue)
                {
                    _YRefManualValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Version.Value = 1.1;
                ReaderType.Value = FileReaderType.STREAM;
                NamerAlias.Value = "STIF";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Tuple<double, double> GetWaferCenterToRefDieCenterDistance(int refDieX, int refDieY, int cenDieX, int cenDieY, double dieSizeX, double dieSizeY, AxisDirectionEnum axisDirectionEnum)
        {
            Tuple<double, double> retval = null;

            try
            {
                LoggerManager.Debug($"GetWaferCenterToRefDieCenterDistance() : refDieX = {refDieX}, refDieY = {refDieY}, cenDieX = {cenDieX}, cenDieY = {cenDieY}, dieSizeX = {dieSizeX}, dieSizeY = {dieSizeY}");
                int XdistCenToRef = refDieX - cenDieX;
                int YdistCenToRef = refDieY - cenDieY;

                LoggerManager.Debug($"GetWaferCenterToRefDieCenterDistance() : XdistCenToRef = {XdistCenToRef}, YdistCenToRef = {YdistCenToRef}");

                int SignX = 0;
                int SignY = 0;

                getQuadCoordwithDistance(axisDirectionEnum, XdistCenToRef, YdistCenToRef, out SignX, out SignY);
                LoggerManager.Debug($"GetWaferCenterToRefDieCenterDistance() : axisDirectionEnum = {axisDirectionEnum}, SignX = {SignY}, SignX = {SignY}");


                retval = new Tuple<double, double>((Math.Abs(XdistCenToRef) * dieSizeX * SignX), (Math.Abs(YdistCenToRef) * dieSizeY * SignY));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Tuple<double, double> GetWaferCenterToRefDieCenterDistance(double refDieX, double refDieY, double cenDieX, double cenDieY)
        {
            Tuple<double, double> retval = null;

            try
            {
                // refDie , cenDie 둘다 WaferCoordinate 값 계산이기 때문에 차이 값으로 확인? 
                int XdistCenToRef = Convert.ToInt32(refDieX - cenDieX);
                int YdistCenToRef = Convert.ToInt32(refDieY - cenDieY);

                int SignX = -1;
                int SignY = 1;

                retval = new Tuple<double, double>((Math.Abs(XdistCenToRef) * SignX), (Math.Abs(YdistCenToRef) * SignY));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void getQuadCoordwithDistance(AxisDirectionEnum axisDirectionEnum, int xdistCenToRef, int ydistCenToRef, out int signX, out int signY)
        {
            int quadVal = 0;

            signX = 0;
            signY = 0;

            try
            {
                switch (axisDirectionEnum)
                {
                    case AxisDirectionEnum.UpRight:
                        quadVal = 3;
                        break;
                    case AxisDirectionEnum.DownRight:
                        quadVal = 2;
                        break;
                    case AxisDirectionEnum.UpLeft:
                        quadVal = 4;
                        break;
                    case AxisDirectionEnum.DownLeft:
                        quadVal = 1;
                        break;
                    default:
                        break;
                }

                if (quadVal == 1)
                {
                    if (xdistCenToRef > 0 && ydistCenToRef > 0)
                    {
                        signX = -1;
                        signY = 1;
                    }
                    else if (xdistCenToRef < 0 && ydistCenToRef > 0)
                    {
                        signX = 1;
                        signY = 1;
                    }
                    else if (xdistCenToRef < 0 && ydistCenToRef < 0)
                    {
                        signX = 1;
                        signY = -1;
                    }
                    else if (xdistCenToRef > 0 && ydistCenToRef < 0)
                    {
                        signX = -1;
                        signY = -1;
                    }
                    else
                    {
                        signX = 1;
                        signY = 1;
                    }
                }
                else if (quadVal == 2)
                {
                    if (xdistCenToRef > 0 && ydistCenToRef > 0)
                    {
                        signX = 1;
                        signY = 1;
                    }
                    else if (xdistCenToRef < 0 && ydistCenToRef > 0)
                    {
                        signX = -1;
                        signY = 1;
                    }
                    else if (xdistCenToRef < 0 && ydistCenToRef < 0)
                    {
                        signX = -1;
                        signY = -1;
                    }
                    else if (xdistCenToRef > 0 && ydistCenToRef < 0)
                    {
                        signX = 1;
                        signY = -1;
                    }
                    else
                    {
                        signX = 1;
                        signY = 1;
                    }
                }
                else if (quadVal == 3)
                {
                    if (xdistCenToRef > 0 && ydistCenToRef > 0)
                    {
                        signX = 1;
                        signY = -1;
                    }
                    else if (xdistCenToRef < 0 && ydistCenToRef > 0)
                    {
                        signX = -1;
                        signY = -1;
                    }
                    else if (xdistCenToRef < 0 && ydistCenToRef < 0)
                    {
                        signX = -1;
                        signY = 1;
                    }
                    else if (xdistCenToRef > 0 && ydistCenToRef < 0)
                    {
                        signX = 1;
                        signY = 1;
                    }
                    else
                    {
                        signX = 1;
                        signY = 1;
                    }
                }
                else if (quadVal == 4)
                {
                    if (xdistCenToRef > 0 && ydistCenToRef > 0)
                    {
                        signX = -1;
                        signY = -1;
                    }
                    else if (xdistCenToRef < 0 && ydistCenToRef > 0)
                    {
                        signX = 1;
                        signY = -1;
                    }
                    else if (xdistCenToRef < 0 && ydistCenToRef < 0)
                    {
                        signX = 1;
                        signY = 1;
                    }
                    else if (xdistCenToRef > 0 && ydistCenToRef < 0)
                    {
                        signX = -1;
                        signY = 1;
                    }
                    else
                    {
                        signX = 1;
                        signY = 1;
                    }
                }
                else
                {
                    LoggerManager.Error($"[STIFMapParameter] getQuadCoordwithDistance() : quadVal = {quadVal}, Undefined.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
