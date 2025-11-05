using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Enum;
using ProberInterfaces.Pad;
using ProberInterfaces.Param;
using ProberInterfaces.Vision;
using SubstrateObjects;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace PadProcModule
{
    [Serializable]
    public class PadSearchDieAll : PadSearchBase, INotifyPropertyChanged
    {
        [XmlIgnore, JsonIgnore]
        public override bool Initialized { get; set; } = false;

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        [XmlIgnore, JsonIgnore]
        private WaferObject Wafer { get { return (WaferObject)this.StageSupervisor().WaferObject; } }

        private PadGroup _Pads;
        public PadGroup Pads
        {
            get { return _Pads; }
            set
            {
                if (value != _Pads)
                {
                    _Pads = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumMoveDirection _moveDir;
        public EnumMoveDirection moveDir
        {
            get { return _moveDir; }
            set
            {
                if (value != _moveDir)
                {
                    _moveDir = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return retval;
        }

        //Die 전체 Pad Search
        public override Task<EventCodeEnum> Run()
        {
            //string ParamPath = string.Empty;
            //_Container.Resolve<IFileManager>().GetXMLFilePath(typeof(PadGroup), out ParamPath);

            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                moveDir = EnumMoveDirection.Right;

                ICamera cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                ROIParameter roiparam = new ROIParameter();

                roiparam.OffsetX.Value = 0;
                roiparam.OffsetY.Value = 0;

                roiparam.Width.Value = (int)cam.GetGrabSizeWidth();
                roiparam.Height.Value = (int)cam.GetGrabSizeHeight();

                WaferCoordinate padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                WaferCoordinate CurDieLeftCorner = new WaferCoordinate();
                Point pt = this.WaferAligner().GetLeftCornerPosition(padwafercoord.X.Value, padwafercoord.Y.Value);

                CurDieLeftCorner.X.Value = pt.X;
                CurDieLeftCorner.Y.Value = pt.Y;

                ImageBuffer GetCurGrabImage = new ImageBuffer();

                double limitX = 0.0;
                //double limitY = 0.0;

                double movex = 0.0;
                //double movey = 0.0;

                double NextPosX = 0.0;
                double NextPosY = 0.0;

                double DefaultMovingPosX;
                double DefaultMovingPosY;

                double CurlimitX = 0.0;

                // Pixel
                double EdgeMarginX = 5;
                double EdgeMarginY = 5;

                DefaultMovingPosX = (cam.GetGrabSizeWidth() * cam.GetRatioX()) - Pads.SetPadObject.PadSizeX.Value - (EdgeMarginX * cam.GetRatioX());
                DefaultMovingPosY = (cam.GetGrabSizeHeight() * cam.GetRatioY()) - Pads.SetPadObject.PadSizeY.Value - (EdgeMarginY * cam.GetRatioY());

                do
                {
                    padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    
                    GetCurGrabImage = this.VisionManager().SingleGrab(cam.GetChannelType(), this);

                    ObservableCollection<GrabDevPosition> position = new ObservableCollection<GrabDevPosition>();
                    position = this.VisionManager().Detect_Pad(GetCurGrabImage, Pads.SetPadObject.PadSizeX.Value, Pads.SetPadObject.PadSizeY.Value, roiparam);

                    if (moveDir == EnumMoveDirection.Left)
                    {
                        if (position.Count == 0)
                        {
                            NextPosX = padwafercoord.X.Value - DefaultMovingPosX;
                            NextPosY = DefaultMovingPosY;

                            this.StageSupervisor().StageModuleState.WaferHighViewMove(NextPosX, padwafercoord.Y.Value, Wafer.GetPhysInfo().Thickness.Value);
                        }
                        else if (position.Count >= 1)
                        {
                            double offsetx = 0.0;

                            if (position[position.Count - 1].PosX < cam.GetGrabSizeWidth() / 2)
                            {
                                offsetx = (cam.GetGrabSizeWidth() / 2) - position[position.Count - 1].PosX;
                                offsetx *= -1;
                            }
                            else
                            {
                                offsetx = position[position.Count - 1].PosX - (cam.GetGrabSizeWidth() / 2);

                            }

                            movex = padwafercoord.X.Value + (offsetx * cam.GetRatioX()) -
                            ((cam.GetGrabSizeWidth() / 2) * cam.GetRatioX());

                            this.StageSupervisor().StageModuleState.WaferHighViewMove(movex, padwafercoord.Y.Value, Wafer.GetPhysInfo().Thickness.Value);
                        }

                        limitX = padwafercoord.X.Value - (cam.GetGrabSizeWidth());
                        CurlimitX = CurDieLeftCorner.X.Value + (cam.GetGrabSizeWidth() / 2);

                        if (CurlimitX > limitX)
                        {
                            padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                            NextPosY = padwafercoord.Y.Value - DefaultMovingPosY;
                            this.StageSupervisor().StageModuleState.WaferHighViewMove(padwafercoord.X.Value, NextPosY, Wafer.GetPhysInfo().Thickness.Value);

                            moveDir = EnumMoveDirection.Right;
                            if (CurDieLeftCorner.X.Value < limitX)
                            {
                                limitX = -limitX;
                            }
                        }
                    }
                    else if (moveDir == EnumMoveDirection.Right)
                    {
                        if (position.Count == 0)
                        {
                            NextPosX = padwafercoord.X.Value + DefaultMovingPosX;
                            NextPosY = DefaultMovingPosY;

                            this.StageSupervisor().StageModuleState.WaferHighViewMove(NextPosX, padwafercoord.Y.Value, Wafer.GetPhysInfo().Thickness.Value);
                        }
                        else if (position.Count >= 1)
                        {
                            double offsetx = position[0].PosX - (cam.GetGrabSizeWidth() / 2);

                            if (position[0].PosX > (cam.GetGrabSizeWidth() / 2))
                            {
                                offsetx = position[0].PosX - (cam.GetGrabSizeWidth() / 2);
                            }
                            else
                            {
                                offsetx = (cam.GetGrabSizeWidth() / 2) - position[0].PosX;
                                offsetx *= -1;
                            }

                            movex = padwafercoord.X.Value + (offsetx * cam.GetRatioX()) +
                            ((cam.GetGrabSizeWidth() / 2) * cam.GetRatioX());

                            this.StageSupervisor().StageModuleState.WaferHighViewMove(movex, padwafercoord.Y.Value, Wafer.GetPhysInfo().Thickness.Value);
                        }

                        //limitX = CurDieLeftCorner.X.Value + WaferAlinger.WAInfoFile.DieSizeActualX - WaferAlinger.WAInfoFile.DieXClearance;
                        limitX = CurDieLeftCorner.X.Value + Wafer.GetSubsInfo().ActualDieSize.Width.Value
                            - Wafer.GetSubsInfo().DieXClearance.Value;

                        CurlimitX = padwafercoord.X.Value + (cam.GetGrabSizeWidth());

                        if (CurlimitX > limitX)
                        {
                            padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                            NextPosY = padwafercoord.Y.Value - DefaultMovingPosY;
                            this.StageSupervisor().StageModuleState.WaferHighViewMove(padwafercoord.X.Value, NextPosY, Wafer.GetPhysInfo().Thickness.Value);

                            moveDir = EnumMoveDirection.Left;
                            if (CurDieLeftCorner.X.Value < CurlimitX)
                            {
                                limitX = -limitX;
                            }
                        }
                    }

                    for (int count = 0; count < position.Count(); count++)
                    {
                        PadObject padparam = new PadObject();

                        padparam.PadSizeX.Value = position[count].SizeX;
                        padparam.PadSizeY.Value = position[count].SizeY;

                        double pixeloffsetx = 0.0;
                        double pixeloffsety = 0.0;

                        if (position[count].PosX > (cam.GetGrabSizeWidth() / 2))
                        {
                            pixeloffsetx = position[count].PosX - (cam.GetGrabSizeWidth() / 2);
                        }
                        else
                        {
                            pixeloffsetx = (cam.GetGrabSizeWidth() / 2) - position[count].PosX;
                            pixeloffsetx *= -1;
                        }

                        if (position[count].PosY < (cam.GetGrabSizeHeight() / 2))
                        {
                            pixeloffsety = position[count].PosY - (cam.GetGrabSizeHeight() / 2);
                            pixeloffsety *= -1;
                        }
                        else
                        {
                            pixeloffsety = (cam.GetGrabSizeHeight() / 2) - position[count].PosY;
                        }

                        double pixelpadx = padwafercoord.X.Value + (pixeloffsetx * cam.GetRatioX());
                        double pixelpady = padwafercoord.Y.Value + (pixeloffsety * cam.GetRatioY());

                        padparam.PadCenter.X.Value = pixelpadx - pt.X;
                        padparam.PadCenter.Y.Value = pixelpady - pt.Y;

                        //Pads.PadInfo.Add(new PadParameter(position[count].SizeX, position[count].SizeY, 
                        //    new PadCoordinate(pixelpadx - CurDieLeftCorner.X.Value, pixelpady - CurDieLeftCorner.Y.Value)));

                        //Pads.PadInfo.Add(padparam);

                        int CheckPad;
                        int CheckTolerance;
                        // Check Pad Pos
                        CheckPad = this.PadRegist().CheckPadPosition(padparam);
                        CheckTolerance = this.PadRegist().CheckPadTolerance(padparam);

                        if (CheckPad == 0 && CheckTolerance == 0)
                        {
                            Pads.DutPadInfos.Add((DUTPadObject)padparam);
                            //Pads.PadInfo.OrderBy(a => a.PadCenter.X.Value);
                            //RWDeviceFile();

                            GraphicsParam Test = new GraphicsParam();

                            Test.X = padparam.PadCenter.X.Value;
                            Test.Y = padparam.PadCenter.Y.Value;
                            Test.SizeX = padparam.PadSizeX.Value;
                            Test.SizeY = padparam.PadSizeY.Value;

                            Test.Shape = padparam.PadShape.Value;

                            this.PadRegist().UpdateDrawList(Test);
                        }
                    }

                    //this.PadRegist().SavePadParam(ParamPath);
                    //this.PadRegist().SavePadParam();
                    if(this.PadRegist() is IHasDevParameterizable)
                    {
                        (this.PadRegist() as IHasDevParameterizable).SaveDevParameter();
                    }

                } while (CurlimitX < limitX);

                //NextPosY = padwafercoord.Y.Value - DefaultMovingPosY;
                //StageSupervisor.StageModuleState.WaferHighViewMove(padwafercoord.X.Value, NextPosY, Wafer.PhysInfo.Thickness);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override void DeInitModule()
        {
        }
    }
}
