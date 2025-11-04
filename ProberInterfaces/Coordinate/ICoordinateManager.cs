
using Newtonsoft.Json;
using ProberInterfaces.Param;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;
using LogModule;


namespace ProberInterfaces
{
    public delegate void OverlayUpdate();
    [ServiceContract]
    public interface ICoordinateManager : IFactoryModule, IHasSysParameterizable
    { 
        EnumAxisConstants ProberPinAxis { get; }
        void SetPinAxisAs(EnumAxisConstants axis);
        OverlayUpdate OverlayUpdateDelegate { get; set; }

        EnumProberCam CamType { get; set; }
        UserIndex CurUserIndex { get; set; }
        WaferHighChuckCoordConvert WaferHighChuckConvert { get; set; }
        WaferLowChuckCoordConvert WaferLowChuckConvert { get; set; }
        PinHighPinCoordConvert PinHighPinConvert { get; set; }
        PinLowPinCoordinateConvert PinLowPinConvert { get; set; }
        WaferHighNCPadCoordConvert WaferHighNCPadConvert { get; set; }
        WaferLowNCPadCoordinate WaferLowNCPadConvert { get; set; }
        CatCoordinates CurrentCoordinate { get; set; }
        StageCoords StageCoord { get; set; }

        //bool ReverseManualMoveX { get; set; }
        //bool ReverseManualMoveY { get; set; }

        //IndexCoord GetMachineDieIndex(IndexCoord ui);
        //IndexCoord GetUserDieIndex(IndexCoord mi);
        //IndexCoord ReadCurMIIndex(CatCoordinates Relpos);
        //UserIndex ReadCurUIIndex(CatCoordinates Relpos);
        //CatCoordinates GetDieTargetPos(EnumDieViewPoint viewPoint, IndexCoord coord, CatCoordinates RelPos);

        #region Coordinate Old System
        //CatCoordinates GetCoordinate(EnumProberCam camtype);
        //CatCoordinates GetCoordinate(EnumProberCam camtype, CatCoordinates actualPos);
        //CatCoordinates GetPinMapCoordinate(EnumProberCam camtype, double width, double height, double sizeX, double sizeY, CatCoordinates offset);
        //CatCoordinates GetPinMapCoordinate(EnumProberCam camtype, CatCoordinates CardOriginPos, double width, double height, double sizeX, double sizeY, CatCoordinates CurPinPos);
        //CatCoordinates GetCoordinate(CatCoordinates coords, EnumProberCam camtype);
        #endregion
        double ChuckCoordXPos { get; set; }
        double ChuckCoordYPos { get; set; }
        double ChuckCoordZPos { get; set; }
        //void SetDieViewPoint(EnumDieViewPoint viewPoint);
        bool IsCoordToChuckContinus { get; set; }
        [OperationContract]
        void StageCoordConvertToChuckCoord();
        [OperationContract]
        void StopStageCoordConvertToChuckCoord();
        [OperationContract]
        bool GetReverseManualMoveX();
        [OperationContract]
        bool GetReverseManualMoveY();
        void Dispose();
        double DistanceOfPoints(double x1, double y1, double x2, double y2);
        double CalcP2PAngle(double x1, double y1, double x2, double y2);
        //double RefUserDieCenterXPos { get; set; }
        //double RefUserDieCenterYPos { get; set; }
        MachineCoordinate GetRotatedPoint(MachineCoordinate point, MachineCoordinate pivotPoint, double degrees);
        //[OperationContract]
        double[,] GetAnyDieCornerPos(UserIndex ui);
        //[OperationContract]
        double[,] GetAnyDieCornerPos(UserIndex ui, EnumProberCam camtype);
        [OperationContract]
        void InitService();
        [OperationContract]
        bool IsServiceAvailable();
        //CatCoordinates GetAnyDieCenterPos(UserIndex ui);
        [OperationContract]
        CatCoordinates StageCoordConvertToUserCoord(EnumProberCam camtype);
        [OperationContract]
        UserIndex GetCurUserIndex(CatCoordinates Pos);
        [OperationContract]
        MachineIndex UserIndexConvertToMachineIndex(UserIndex UI);
        [OperationContract]
        UserIndex MachineIndexConvertToUserIndex(MachineIndex MI);
        [OperationContract]
        MachineIndex WUIndexConvertWMIndex(long uindexX, long uindexY);
        [OperationContract]
        MachineIndex GetCurMachineIndex(WaferCoordinate Pos);
        [OperationContract]
        UserIndex WMIndexConvertWUIndex(long mindexX, long mindexY);
        void InitCoordinateManager();
        [OperationContract]
        CatCoordinates PmResultConverToUserCoord(PMResult pmresult);
        [OperationContract]
        MachineCoordinate RelPosToAbsPos(MachineCoordinate RelPos);

        [OperationContract]
        void UpdateCenM();

        void CalculateOffsetFromCurrentZ(EnumProberCam camchannel);
    }

    public struct IndexKey
    {
        public long X { get; set; }
        public long Y { get; set; }

        public IndexKey(long x, long y)
        {
            X = x;
            Y = y;
        }
    }


    [Serializable, DataContract]
    public abstract class IndexCoord : IParamNode, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public virtual string Genealogy { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public virtual Object Owner { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        //public long XIndex { get; set; }
        //public long YIndex { get; set; }

        private long _XIndex;
        [DataMember]
        public long XIndex
        {
            get { return _XIndex; }
            set
            {
                if (value != _XIndex)
                {
                    _XIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private long _YIndex;
        [DataMember]
        public long YIndex
        {
            get { return _YIndex; }
            set
            {
                if (value != _YIndex)
                {
                    _YIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IndexCoord()
        {

        }
        public IndexCoord(long x, long y)
        {
            try
            {
                XIndex = x;
                YIndex = y;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            bool isEqual = false;

            try
            {

                if (obj is IndexCoord index)
                {
                    if ((XIndex == index.XIndex) && (YIndex == index.YIndex))
                    {
                        isEqual = true;
                    }
                    else
                    {
                        isEqual = false;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return isEqual;
        }
        public abstract IndexCoord Add(object obj);

        public void CopyTo(IndexCoord target)
        {
            try
            {
                target.XIndex = this.XIndex;
                target.YIndex = this.YIndex;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
    [Serializable, DataContract]
    public class UserIndex : IndexCoord
    {
        public UserIndex() : base()
        {

        }
        public UserIndex(long ux, long uy) : base(ux, uy)
        {

        }

        public override IndexCoord Add(object obj)
        {
            IndexCoord result = null;
            try
            {

                if (obj is IndexCoord | obj is UserIndex)
                {
                    IndexCoord index = (IndexCoord)obj;
                    result = new UserIndex(index.XIndex + XIndex, index.YIndex + YIndex);
                }
                else
                {
                    result = null;
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
            return result;
        }
    }
    [Serializable, DataContract]
    public class MachineIndex : IndexCoord
    {
        public MachineIndex() : base()
        {

        }
        public MachineIndex(long mx, long my) : base(mx, my)
        {
            try
            {
                XIndex = mx;
                YIndex = my;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
        public MachineIndex(MachineIndex mindex)
        {
            try
            {
                XIndex = mindex.XIndex;
                YIndex = mindex.YIndex;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
        public override IndexCoord Add(object obj)
        {
            IndexCoord result = null;
            try
            {

                if (obj is IndexCoord || obj is MachineIndex)
                {
                    IndexCoord index = (IndexCoord)obj;

                    result = new MachineIndex(index.XIndex + XIndex, index.YIndex + YIndex);
                }
                else
                {
                    result = null;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return result;
        }
    }
    [Serializable]
    public class ElemUserIndex: IParamNode
    {
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public virtual string Genealogy { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public virtual Object Owner { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }
        #region ==> XIndex
        Element<long> _XIndex = new Element<long>();
        public Element<long> XIndex
        {
            get { return _XIndex; }
            set { _XIndex = value; }
        }
        #endregion

        #region ==> YIndex
        Element<long> _YIndex = new Element<long>();
        public Element<long> YIndex
        {
            get { return _YIndex; }
            set { _YIndex = value; }
        }

    
        #endregion

        public ElemUserIndex()
        {

        }
        public ElemUserIndex(long ux, long uy)
        {
            try
            {
                XIndex.Value = ux;
                YIndex.Value = uy;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
    [Serializable]
    public class ElemMachineIndex: IParamNode
    {
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public virtual string Genealogy { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public virtual Object Owner { get; set; }
        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }
        #region ==> XIndex
        Element<long> _XIndex = new Element<long>();
        public Element<long> XIndex
        {
            get { return _XIndex; }
            set { _XIndex = value; }
        }
        #endregion

        #region ==> YIndex
        Element<long> _YIndex = new Element<long>();
        public Element<long> YIndex
        {
            get { return _YIndex; }
            set { _YIndex = value; }
        }
        #endregion

        public ElemMachineIndex()
        {

        }
        public ElemMachineIndex(long mx, long my)
        {
            try
            {
                XIndex.Value = mx;
                YIndex.Value = my;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}

