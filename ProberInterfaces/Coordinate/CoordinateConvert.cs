namespace ProberInterfaces
{
    using LogModule;
    using System;
    using ProberInterfaces.Param;
    using Autofac;

    public abstract class CoordinateConvertBase<T> : ICoordinateConvert<T>, IFactoryModule
    {

        public abstract T Convert(MachineCoordinate machinecoord);
        public abstract MachineCoordinate ConvertBack(T coord);
        public abstract T CurrentPosConvert();
        public EnumAxisConstants ProberPinAxis;

    }

    public class WaferHighChuckCoordConvert : CoordinateConvertBase<WaferCoordinate>
    {
        public int InitModule()
        {
            int retVal = -1;
            return retVal;
        }
        public override WaferCoordinate Convert(MachineCoordinate machinecoord)
        {
            try
            {
                WaferCoordinate resultWaferCoord = new WaferCoordinate();

                MachineCoordinate wcenInMac = new MachineCoordinate();
                wcenInMac = ConvertBack(new WaferCoordinate(0, 0, 0));

                resultWaferCoord.X.Value = (wcenInMac.GetX() - machinecoord.GetX()) * 1d;
                resultWaferCoord.Y.Value = (wcenInMac.GetY() - machinecoord.GetY()) * 1d;
                resultWaferCoord.Z.Value = (wcenInMac.GetZ() - machinecoord.GetZ()) * 1d;

                return resultWaferCoord;

                //WaferCoordinate wafercoord = new WaferCoordinate();

                //wafercoord.X.Value = (machinecoord.X.Value
                //    - this.CoordinateManager().StageCoord.RefMarkPos.X.Value
                //    - this.CoordinateManager().StageCoord.MarkPosInChuckCoord.X.Value) * -1d;

                //wafercoord.Y.Value = (machinecoord.Y.Value
                //    - this.CoordinateManager().StageCoord.RefMarkPos.Y.Value
                //    - this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Y.Value) * -1d;

                //wafercoord.Z.Value = (machinecoord.Z.Value
                //    - this.CoordinateManager().StageCoord.RefMarkPos.Z.Value
                //    - this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Z.Value) * -1d;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            //return wafercoord;
        }

        public override MachineCoordinate ConvertBack(WaferCoordinate coord)
        {
            MachineCoordinate mccoord = new MachineCoordinate();
            try
            {
                ICoordinateManager CoordinateManager = this.CoordinateManager();
                if(CoordinateManager.StageCoord != null)
                {
                    mccoord.X.Value =
                            CoordinateManager.StageCoord.RefMarkPos.X.Value
                        + CoordinateManager.StageCoord.MarkPosInChuckCoord.X.Value
                        - coord.X.Value;

                    mccoord.Y.Value =
                          CoordinateManager.StageCoord.RefMarkPos.Y.Value
                        + CoordinateManager.StageCoord.MarkPosInChuckCoord.Y.Value
                        - coord.Y.Value;

                    mccoord.Z.Value =
                          CoordinateManager.StageCoord.RefMarkPos.Z.Value
                        + CoordinateManager.StageCoord.MarkPosInChuckCoord.Z.Value
                        - coord.Z.Value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return mccoord;
        }

        public override WaferCoordinate CurrentPosConvert()
        {
            //WaferCoordinate wafercoordCenter = new WaferCoordinate();
            //WaferCoordinate waferCoordInv = new WaferCoordinate();
            WaferCoordinate resultWaferCoord = new WaferCoordinate();
            try
            {

                //var axisx = this.MotionManager().GetAxis(EnumAxisConstants.X);
                //var axisy = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                //var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                double actualposx = this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref;
                double actualposy = this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref;
                double actualposz = this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Ref;


                MachineCoordinate wcenInMac = new MachineCoordinate();
                wcenInMac = ConvertBack(new WaferCoordinate(0, 0, 0));

                //resultWaferCoord =  Convert(wcenInMac);
                resultWaferCoord.X.Value = (wcenInMac.GetX() - actualposx) * 1d;
                resultWaferCoord.Y.Value = (wcenInMac.GetY() - actualposy) * 1d;
                resultWaferCoord.Z.Value = (wcenInMac.GetZ() - actualposz) * 1d;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return resultWaferCoord;
        }

        public MachineCoordinate GetWaferPinAlignedPosition(WaferCoordinate wafercoord, PinCoordinate pincoord)
        {
            MachineCoordinate mccoord = new MachineCoordinate();
            try
            {
                ICoordinateManager CoordinateManager = this.CoordinateManager();

                mccoord.X.Value = CoordinateManager.StageCoord.RefMarkPos.X.Value
                    - CoordinateManager.StageCoord.WHOffset.X.Value
                    + CoordinateManager.StageCoord.MarkPosInChuckCoord.X.Value
                    - wafercoord.X.Value
                    + pincoord.X.Value;

                mccoord.Y.Value = CoordinateManager.StageCoord.RefMarkPos.Y.Value
                    - CoordinateManager.StageCoord.WHOffset.Y.Value
                    + CoordinateManager.StageCoord.MarkPosInChuckCoord.Y.Value
                    - wafercoord.Y.Value
                    + pincoord.Y.Value;

                mccoord.Z.Value = CoordinateManager.StageCoord.RefMarkPos.Z.Value
                    - CoordinateManager.StageCoord.WHOffset.Z.Value
                    + CoordinateManager.StageCoord.MarkPosInChuckCoord.Z.Value
                    - wafercoord.Z.Value
                    + pincoord.Z.Value;

                mccoord.T.Value = wafercoord.T.Value;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return mccoord;
        }
        public WaferCoordinate GetWaferPosFromAlignedPos(MachineCoordinate mccoord, PinCoordinate pincoord)
        {
            WaferCoordinate wafercoord = new WaferCoordinate();

            try
            {
                ICoordinateManager CoordinateManager = this.CoordinateManager();

                wafercoord.X.Value = CoordinateManager.StageCoord.RefMarkPos.X.Value
                    - CoordinateManager.StageCoord.WHOffset.X.Value
                    + CoordinateManager.StageCoord.MarkPosInChuckCoord.X.Value
                    - mccoord.X.Value
                    + pincoord.X.Value;

                wafercoord.Y.Value = CoordinateManager.StageCoord.RefMarkPos.Y.Value
                    - CoordinateManager.StageCoord.WHOffset.Y.Value
                    + CoordinateManager.StageCoord.MarkPosInChuckCoord.Y.Value
                    - mccoord.Y.Value
                    + pincoord.Y.Value;

                wafercoord.Z.Value = CoordinateManager.StageCoord.RefMarkPos.Z.Value
                    - CoordinateManager.StageCoord.WHOffset.Z.Value
                    + CoordinateManager.StageCoord.MarkPosInChuckCoord.Z.Value
                    - mccoord.Z.Value
                    + pincoord.Z.Value;

                wafercoord.T.Value = wafercoord.T.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return wafercoord;
        }
        //GetWaferPosFromAlignedPos

    }

    public class WaferLowChuckCoordConvert : CoordinateConvertBase<WaferCoordinate>
    {
        public int InitModule()
        {
            int retVal = -1;
            return retVal;
        }

        public override WaferCoordinate Convert(MachineCoordinate machinecoord)
        {
            try
            {
                WaferCoordinate resultWaferCoord = new WaferCoordinate();

                MachineCoordinate wcenInMac = new MachineCoordinate();
                wcenInMac = ConvertBack(new WaferCoordinate(0, 0, 0));

                resultWaferCoord.X.Value = (wcenInMac.GetX() - machinecoord.GetX()) * 1d;
                resultWaferCoord.Y.Value = (wcenInMac.GetY() - machinecoord.GetY()) * 1d;
                resultWaferCoord.Z.Value = (wcenInMac.GetZ() - machinecoord.GetZ()) * 1d;

                return resultWaferCoord;

                //WaferCoordinate wafercoord = new WaferCoordinate();

                //wafercoord.X.Value = (machinecoord.X.Value
                //    - this.CoordinateManager().StageCoord.RefMarkPos.X.Value
                //    - this.CoordinateManager().StageCoord.MarkPosInChuckCoord.X.Value
                //    - this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value) * -1d;

                //wafercoord.Y.Value = (machinecoord.Y.Value
                //    - this.CoordinateManager().StageCoord.RefMarkPos.Y.Value
                //    - this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Y.Value
                //    - this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value) * -1d;

                //wafercoord.Z.Value = (machinecoord.Z.Value
                //    - this.CoordinateManager().StageCoord.RefMarkPos.Z.Value
                //    - this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Z.Value
                //    - this.CoordinateManager().StageCoord.WLCAMFromWH.Z.Value) * -1d;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            //return wafercoord;
        }

        public override MachineCoordinate ConvertBack(WaferCoordinate coord)
        {
            MachineCoordinate mccoord = new MachineCoordinate();
            try
            {

                mccoord.X.Value =
                    this.CoordinateManager().StageCoord.RefMarkPos.X.Value
                    + this.CoordinateManager().StageCoord.MarkPosInChuckCoord.X.Value
                    + this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value
                    - coord.X.Value;

                mccoord.Y.Value =
                     this.CoordinateManager().StageCoord.RefMarkPos.Y.Value
                    + this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Y.Value
                    + this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value
                    - coord.Y.Value;

                mccoord.Z.Value =
                     this.CoordinateManager().StageCoord.RefMarkPos.Z.Value
                    + this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Z.Value
                    + this.CoordinateManager().StageCoord.WLCAMFromWH.Z.Value
                    - coord.Z.Value;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return mccoord;
        }

        public override WaferCoordinate CurrentPosConvert()
        {
            WaferCoordinate wafercoord = new WaferCoordinate();
            try
            {
                MachineCoordinate macCoord = new MachineCoordinate();
                double actualposx = this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref;

                double actualposy = this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref;
                double actualposz = this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Ref;

                macCoord = ConvertBack(new WaferCoordinate(0, 0, 0));
                //wafercoord = Convert(macCoord);
                wafercoord.X.Value = (macCoord.GetX() - actualposx) * 1d;
                wafercoord.Y.Value = (macCoord.GetY() - actualposy) * 1d;
                wafercoord.Z.Value = (macCoord.GetZ() - actualposz) * 1d;

                //wafercoord.X.Value = (actualposx
                //    - this.CoordinateManager().StageCoord.RefMarkPos.X.Value
                //    - this.CoordinateManager().StageCoord.MarkPosInChuckCoord.X.Value
                //    - this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value) * -1d;

                //wafercoord.Y.Value = (actualposy
                //    - this.CoordinateManager().StageCoord.RefMarkPos.Y.Value
                //    - this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Y.Value
                //    - this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value) * -1d;

                //wafercoord.Z.Value = (actualposz
                //    - this.CoordinateManager().StageCoord.RefMarkPos.Z.Value
                //    - this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Z.Value
                //    - this.CoordinateManager().StageCoord.WLCAMFromWH.Z.Value) * -1d;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return wafercoord;
        }
    }

    public class PinHighPinCoordConvert : CoordinateConvertBase<PinCoordinate>
    {
        private Autofac.IContainer _Container;
        private ICoordinateManager CoordinateManager
        {
            get
            {
                return _Container.Resolve<ICoordinateManager>();
            }
        }
        private IMotionManager MotionManager
        {
            get
            {
                return _Container.Resolve<IMotionManager>();
            }
        }
        public int InitModule(IContainer container)
        {
            int retVal = -1;
            try
            {
                this._Container = container;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override PinCoordinate Convert(MachineCoordinate machinecoord)
        {
            try
            {

                PinCoordinate resultPinCoord = new PinCoordinate();

                MachineCoordinate wcenInMac = new MachineCoordinate();
                wcenInMac = ConvertBack(new PinCoordinate(0, 0, 0));

                resultPinCoord.X.Value = (wcenInMac.GetX() - machinecoord.GetX()) * -1d;
                resultPinCoord.Y.Value = (wcenInMac.GetY() - machinecoord.GetY()) * -1d;
                resultPinCoord.Z.Value = (wcenInMac.GetZ() - machinecoord.GetZ()) * -1d;

                return resultPinCoord;

                //PinCoordinate coordinate = new PinCoordinate();

                //coordinate.X.Value = machinecoord.X.Value
                //    - (this.CoordinateManager().StageCoord.RefMarkPos.X.Value
                //    - this.CoordinateManager().StageCoord.WHOffset.X.Value
                //    - this.CoordinateManager().StageCoord.PHOffset.X.Value);

                //coordinate.Y.Value = machinecoord.Y.Value
                //    - (this.CoordinateManager().StageCoord.RefMarkPos.Y.Value
                //    - this.CoordinateManager().StageCoord.WHOffset.Y.Value
                //    - this.CoordinateManager().StageCoord.PHOffset.Y.Value);

                //coordinate.Z.Value = machinecoord.Z.Value
                //    - (this.CoordinateManager().StageCoord.RefMarkPos.Z.Value
                //    - this.CoordinateManager().StageCoord.WHOffset.Z.Value
                //    - this.CoordinateManager().StageCoord.PHOffset.Z.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            //return coordinate;
        }

        public override MachineCoordinate ConvertBack(PinCoordinate coord)
        {
            MachineCoordinate mccoord = new MachineCoordinate();
            try
            {

                mccoord.X.Value = 1d * (this.CoordinateManager().StageCoord.RefMarkPos.X.Value
                        - this.CoordinateManager().StageCoord.WHOffset.X.Value
                        - this.CoordinateManager().StageCoord.PHOffset.X.Value
                        + coord.X.Value);

                mccoord.Y.Value = 1d * (this.CoordinateManager().StageCoord.RefMarkPos.Y.Value
                        - this.CoordinateManager().StageCoord.WHOffset.Y.Value
                        - this.CoordinateManager().StageCoord.PHOffset.Y.Value
                        + coord.Y.Value);

                mccoord.Z.Value = (this.CoordinateManager().StageCoord.RefMarkPos.Z.Value
                        - this.CoordinateManager().StageCoord.WHOffset.Z.Value
                        - this.CoordinateManager().StageCoord.PHOffset.Z.Value
                        + coord.Z.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return mccoord;
        }

        public override PinCoordinate CurrentPosConvert()
        {
            try
            {
                PinCoordinate coordinate = new PinCoordinate();
                MachineCoordinate macCoord = new MachineCoordinate();

                double actualposx = this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref;
                double actualposy = this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref;
                double actualposz = this.MotionManager().GetAxis(ProberPinAxis).Status.Position.Ref;

                macCoord = ConvertBack(new PinCoordinate(0, 0, 0));
                //coordinate = Convert(macCoord);
                coordinate.X.Value = (macCoord.GetX() - actualposx) * -1d;
                coordinate.Y.Value = (macCoord.GetY() - actualposy) * -1d;
                coordinate.Z.Value = (macCoord.GetZ() - actualposz) * -1d;

                return coordinate;

                //coordinate.X.Value = actualposx
                //    - (this.CoordinateManager().StageCoord.RefMarkPos.X.Value
                //    - this.CoordinateManager().StageCoord.WHOffset.X.Value
                //    - this.CoordinateManager().StageCoord.PHOffset.X.Value);

                //coordinate.Y.Value = actualposy
                //    - (this.CoordinateManager().StageCoord.RefMarkPos.Y.Value
                //    - this.CoordinateManager().StageCoord.WHOffset.Y.Value
                //    - this.CoordinateManager().StageCoord.PHOffset.Y.Value);

                //coordinate.Z.Value = actualposz
                //    - (this.CoordinateManager().StageCoord.RefMarkPos.Z.Value
                //    - this.CoordinateManager().StageCoord.WHOffset.Z.Value
                //    - this.CoordinateManager().StageCoord.PHOffset.Z.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            //return coordinate;
        }
    }

    public class PinLowPinCoordinateConvert : CoordinateConvertBase<PinCoordinate>
    {
        private Autofac.IContainer _Container;
        private ICoordinateManager CoordinateManager
        {
            get
            {
                return _Container.Resolve<ICoordinateManager>();
            }
        }
        private IMotionManager MotionManager
        {
            get
            {
                return _Container.Resolve<IMotionManager>();
            }
        }
        public int InitModule(IContainer container)
        {
            int retVal = -1;
            try
            {
                this._Container = container;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override PinCoordinate Convert(MachineCoordinate machinecoord)
        {
            try
            {
                PinCoordinate resultPinCoord = new PinCoordinate();

                MachineCoordinate wcenInMac = new MachineCoordinate();
                wcenInMac = ConvertBack(new PinCoordinate(0, 0, 0));

                resultPinCoord.X.Value = (wcenInMac.GetX() - machinecoord.GetX()) * -1d;
                resultPinCoord.Y.Value = (wcenInMac.GetY() - machinecoord.GetY()) * -1d;
                resultPinCoord.Z.Value = (wcenInMac.GetZ() - machinecoord.GetZ()) * -1d;

                return resultPinCoord;

                //PinCoordinate coordinate = new PinCoordinate();

                //coordinate.X.Value = machinecoord.X.Value
                //    - (this.CoordinateManager().StageCoord.RefMarkPos.X.Value
                //    - this.CoordinateManager().StageCoord.WHOffset.X.Value
                //    - this.CoordinateManager().StageCoord.PHOffset.X.Value
                //    - this.CoordinateManager().StageCoord.PLCAMFromPH.X.Value);

                //coordinate.Y.Value = machinecoord.Y.Value
                //    - (this.CoordinateManager().StageCoord.RefMarkPos.Y.Value
                //    - this.CoordinateManager().StageCoord.WHOffset.Y.Value
                //    - this.CoordinateManager().StageCoord.PHOffset.Y.Value
                //    - this.CoordinateManager().StageCoord.PLCAMFromPH.Y.Value);

                //coordinate.Z.Value = machinecoord.Z.Value
                //    - (this.CoordinateManager().StageCoord.RefMarkPos.Z.Value
                //    - this.CoordinateManager().StageCoord.WHOffset.Z.Value
                //    - this.CoordinateManager().StageCoord.PHOffset.Z.Value
                //    - this.CoordinateManager().StageCoord.PLCAMFromPH.Z.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            //return coordinate;
        }

        public override MachineCoordinate ConvertBack(PinCoordinate coord)
        {
            MachineCoordinate mccoord = new MachineCoordinate();
            try
            {

                mccoord.X.Value = this.CoordinateManager().StageCoord.RefMarkPos.X.Value
                    - this.CoordinateManager().StageCoord.WHOffset.X.Value
                    - this.CoordinateManager().StageCoord.PHOffset.X.Value
                    - this.CoordinateManager().StageCoord.PLCAMFromPH.X.Value
                    + coord.X.Value;

                mccoord.Y.Value = this.CoordinateManager().StageCoord.RefMarkPos.Y.Value
                    - this.CoordinateManager().StageCoord.WHOffset.Y.Value
                    - this.CoordinateManager().StageCoord.PHOffset.Y.Value
                    - this.CoordinateManager().StageCoord.PLCAMFromPH.Y.Value
                    + coord.Y.Value;

                mccoord.Z.Value = this.CoordinateManager().StageCoord.RefMarkPos.Z.Value
                    - this.CoordinateManager().StageCoord.WHOffset.Z.Value
                    - this.CoordinateManager().StageCoord.PHOffset.Z.Value
                    - this.CoordinateManager().StageCoord.PLCAMFromPH.Z.Value
                    + coord.Z.Value;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return mccoord;
        }

        public override PinCoordinate CurrentPosConvert()
        {
            try
            {

                PinCoordinate coordinate = new PinCoordinate();
                MachineCoordinate macCoord = new MachineCoordinate();

                double actualposx = this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref;
                double actualposy = this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref;
                double actualposz = this.MotionManager().GetAxis(ProberPinAxis).Status.Position.Ref;

                macCoord = ConvertBack(new PinCoordinate(0, 0, 0));
                //coordinate = Convert(macCoord);
                coordinate.X.Value = (macCoord.GetX() - actualposx) * -1d;
                coordinate.Y.Value = (macCoord.GetY() - actualposy) * -1d;
                coordinate.Z.Value = (macCoord.GetZ() - actualposz) * -1d;
                return coordinate;

                //PinCoordinate coordinate = new PinCoordinate();

                //double actualposx = this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref;
                //double actualposy = this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref;
                //double actualposz = this.MotionManager().GetAxis(EnumAxisConstants.Z).Status.Position.Ref;

                //coordinate.X.Value = actualposx
                //    - (this.CoordinateManager().StageCoord.RefMarkPos.X.Value
                //    - this.CoordinateManager().StageCoord.WHOffset.X.Value
                //    - this.CoordinateManager().StageCoord.PHOffset.X.Value
                //    - this.CoordinateManager().StageCoord.PLCAMFromPH.X.Value);

                //coordinate.Y.Value = actualposy
                //    - (this.CoordinateManager().StageCoord.RefMarkPos.Y.Value
                //    - this.CoordinateManager().StageCoord.WHOffset.Y.Value
                //    - this.CoordinateManager().StageCoord.PHOffset.Y.Value
                //    - this.CoordinateManager().StageCoord.PLCAMFromPH.Y.Value);

                //coordinate.Z.Value = actualposz
                //    - (this.CoordinateManager().StageCoord.RefMarkPos.Z.Value
                //    - this.CoordinateManager().StageCoord.WHOffset.Z.Value
                //    - this.CoordinateManager().StageCoord.PHOffset.Z.Value
                //    - this.CoordinateManager().StageCoord.PLCAMFromPH.Z.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            //return coordinate;
        }
    }

    public class WaferHighNCPadCoordConvert : CoordinateConvertBase<NCCoordinate>
    {
        private Autofac.IContainer _Container;
        private ICoordinateManager CoordinateManager
        {
            get
            {
                return _Container.Resolve<ICoordinateManager>();
            }
        }
        private IMotionManager MotionManager
        {
            get
            {
                return _Container.Resolve<IMotionManager>();
            }
        }
        public int InitModule(IContainer container)
        {
            int retVal = -1;
            try
            {
                this._Container = container;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override NCCoordinate Convert(MachineCoordinate machinecoord)
        {
            NCCoordinate nccoord = new NCCoordinate();
            try
            {

                nccoord.X.Value = -machinecoord.X.Value
                    + (this.CoordinateManager().StageCoord.RefMarkPos.X.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.X.Value);

                nccoord.Y.Value = -machinecoord.Y.Value
                    + (this.CoordinateManager().StageCoord.RefMarkPos.Y.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Y.Value);

                nccoord.Z.Value = -machinecoord.Z.Value
                    + (this.CoordinateManager().StageCoord.RefMarkPos.Z.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Z.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return nccoord;
        }
        public override MachineCoordinate ConvertBack(NCCoordinate coord)
        {
            MachineCoordinate mccoord = new MachineCoordinate();
            try
            {

                mccoord.X.Value =
                      ((this.CoordinateManager().StageCoord.RefMarkPos.X.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.X.Value)
                    - (coord.X.Value));

                mccoord.Y.Value =
                      (this.CoordinateManager().StageCoord.RefMarkPos.Y.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Y.Value)
                    - (coord.Y.Value);

                mccoord.Z.Value =
                      (this.CoordinateManager().StageCoord.RefMarkPos.Z.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Z.Value)
                    - (coord.Z.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return mccoord;
        }
        public override NCCoordinate CurrentPosConvert()
        {
            // (*) 주의!! 이건 웨이퍼 카메라 좌표계를 기준으로 현재 위치를 반환함.
            // 프로빙 좌표계가 아님

            NCCoordinate nccoord = new NCCoordinate();
            try
            {
                double actualposx = this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref;
                double actualposy = this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref;
                double actualposz = this.MotionManager().GetAxis(this.NeedleCleaner().NCAxis.AxisType.Value).Status.Position.Ref;

                nccoord.X.Value = -actualposx
                    + (this.CoordinateManager().StageCoord.RefMarkPos.X.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.X.Value);

                nccoord.Y.Value = -actualposy
                    + (this.CoordinateManager().StageCoord.RefMarkPos.Y.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Y.Value);

                nccoord.Z.Value = -actualposz
                    + (this.CoordinateManager().StageCoord.RefMarkPos.Z.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Z.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return nccoord;
        }

        public NCCoordinate CurrentCleaningPosConvert(PinCoordinate pincoord)
        {
            // 주어진 핀 높이를 기준으로 했을 때 상대적으로 현재 NC의 높이를 구하는 함수

            NCCoordinate nccoord = new NCCoordinate();
            try
            {
                double actualposx = this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref;
                double actualposy = this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref;
                double actualposz = this.MotionManager().GetAxis(this.NeedleCleaner().NCAxis.AxisType.Value).Status.Position.Ref;

                nccoord.X.Value = (actualposx
                                  - this.CoordinateManager().StageCoord.RefMarkPos.X.Value
                                  + this.CoordinateManager().StageCoord.WHOffset.X.Value
                                  - this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.X.Value
                                  - pincoord.X.Value) * -1;

                nccoord.Y.Value = (actualposy
                                  - this.CoordinateManager().StageCoord.RefMarkPos.Y.Value
                                  + this.CoordinateManager().StageCoord.WHOffset.Y.Value
                                  - this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Y.Value
                                  - pincoord.Y.Value) * -1;

                nccoord.Z.Value = actualposz
                                  - this.CoordinateManager().StageCoord.RefMarkPos.Z.Value
                                  + this.CoordinateManager().StageCoord.WHOffset.Z.Value
                                  - this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Z.Value
                                  - pincoord.Z.Value;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return nccoord;
        }

        public MachineCoordinate GetNCPadPinAlignedPosition(NCCoordinate nccoord, PinCoordinate pincoord)
        {
            MachineCoordinate mccoord = new MachineCoordinate();
            try
            {

                mccoord.X.Value = this.CoordinateManager().StageCoord.RefMarkPos.X.Value
                    - this.CoordinateManager().StageCoord.WHOffset.X.Value
                    + this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.X.Value
                    - nccoord.X.Value
                    + pincoord.X.Value;

                mccoord.Y.Value = this.CoordinateManager().StageCoord.RefMarkPos.Y.Value
                    - this.CoordinateManager().StageCoord.WHOffset.Y.Value
                    + this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Y.Value
                    - nccoord.Y.Value
                    + pincoord.Y.Value;

                mccoord.Z.Value = this.CoordinateManager().StageCoord.RefMarkPos.Z.Value
                    - this.CoordinateManager().StageCoord.WHOffset.Z.Value
                    + this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Z.Value
                    - nccoord.Z.Value
                    + pincoord.Z.Value;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return mccoord;
        }
    }

    public class WaferLowNCPadCoordinate : CoordinateConvertBase<NCCoordinate>
    {
        private Autofac.IContainer _Container;
        private ICoordinateManager CoordinateManager
        {
            get
            {
                return _Container.Resolve<ICoordinateManager>();
            }
        }
        private IMotionManager MotionManager
        {
            get
            {
                return _Container.Resolve<IMotionManager>();
            }
        }
        public int InitModule(IContainer container)
        {
            int retVal = -1;
            try
            {
                this._Container = container;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override NCCoordinate Convert(MachineCoordinate machinecoord)
        {
            NCCoordinate nccoord = new NCCoordinate();
            try
            {

                nccoord.X.Value = -machinecoord.X.Value
                    + (this.CoordinateManager().StageCoord.RefMarkPos.X.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.X.Value)
                    + (this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value);

                nccoord.Y.Value = -machinecoord.Y.Value
                    + (this.CoordinateManager().StageCoord.RefMarkPos.Y.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Y.Value)
                    + (this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value);

                nccoord.Z.Value = -machinecoord.Z.Value
                    + (this.CoordinateManager().StageCoord.RefMarkPos.Z.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Z.Value)
                    + (this.CoordinateManager().StageCoord.WLCAMFromWH.Z.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return nccoord;
        }

        public override MachineCoordinate ConvertBack(NCCoordinate coord)
        {
            MachineCoordinate mccoord = new MachineCoordinate();
            try
            {

                mccoord.X.Value =
                      this.CoordinateManager().StageCoord.RefMarkPos.X.Value
                    + this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.X.Value
                    + this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value
                    - coord.X.Value;

                mccoord.Y.Value =
                      this.CoordinateManager().StageCoord.RefMarkPos.Y.Value
                    + this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Y.Value
                    + this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value
                    - coord.Y.Value;

                mccoord.Z.Value =
                      this.CoordinateManager().StageCoord.RefMarkPos.Z.Value
                    + this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Z.Value
                    + this.CoordinateManager().StageCoord.WLCAMFromWH.Z.Value
                    - coord.Z.Value;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return mccoord;
        }

        public override NCCoordinate CurrentPosConvert()
        {
            NCCoordinate nccoord = new NCCoordinate();
            try
            {
                double actualposx = this.MotionManager().GetAxis(EnumAxisConstants.X).Status.Position.Ref;
                double actualposy = this.MotionManager().GetAxis(EnumAxisConstants.Y).Status.Position.Ref;
                double actualposz = this.MotionManager().GetAxis(this.NeedleCleaner().NCAxis.AxisType.Value).Status.Position.Ref;

                nccoord.X.Value = -actualposx
                    + (this.CoordinateManager().StageCoord.RefMarkPos.X.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.X.Value)
                    + (this.CoordinateManager().StageCoord.WLCAMFromWH.X.Value);

                nccoord.Y.Value = -actualposy
                    + (this.CoordinateManager().StageCoord.RefMarkPos.Y.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Y.Value)
                    + (this.CoordinateManager().StageCoord.WLCAMFromWH.Y.Value);

                nccoord.Z.Value = -actualposz
                    + (this.CoordinateManager().StageCoord.RefMarkPos.Z.Value)
                    + (this.CoordinateManager().StageCoord.MarkPosInDiskPosCoord.Z.Value)
                    + (this.CoordinateManager().StageCoord.WLCAMFromWH.Z.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return nccoord;
        }
    }

}
