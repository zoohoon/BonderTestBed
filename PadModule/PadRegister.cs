using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace PadModule
{
    using ProberInterfaces;
    using ProberInterfaces.Pad;
    using ProberInterfaces.Param;
    using System.Collections.ObjectModel;
    using System.Windows;
    using ProberInterfaces.Enum;
    using SubstrateObjects;
    using ProberErrorCode;
    using ProberInterfaces.Vision;
    using LogModule;
    using System.Runtime.CompilerServices;

    public class PadRegistrationAssistanceModule : IPadRegist, INotifyPropertyChanged
    {

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property

        public bool Initialized { get; set; } = false;

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

        private ICamera Camera;
        #endregion

        private WaferObject Wafer { get { return (WaferObject)this.StageSupervisor().WaferObject; } }

        public ObservableCollection<GraphicsParam> Graphicsparam { get; set; }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Graphicsparam = new ObservableCollection<GraphicsParam>();

                    Camera = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

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
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public int CheckPadPosition(PadObject padparam)
        {
            int Result = 0, i = 0;
            try
            {
                bool XInPos = false, YInPos = false;

                double DistXPos = 0.0;
                double DistYPos = 0.0;

                for (i = 0; i < Wafer.GetSubsInfo().Pads.DutPadInfos.Count; i++)
                {
                    XInPos = false;
                    YInPos = false;

                    DistXPos = Math.Abs(Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadCenter.X.Value - padparam.PadCenter.X.Value);
                    DistYPos = Math.Abs(Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadCenter.Y.Value - padparam.PadCenter.Y.Value);

                    if (DistXPos <= (padparam.PadSizeX.Value / 2 + Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadSizeX.Value / 2))
                    {
                        XInPos = true;
                    }
                    else
                    {
                        XInPos = false;
                    }

                    if (DistYPos <= (padparam.PadSizeY.Value / 2 + Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadSizeY.Value / 2))
                    {
                        YInPos = true;
                    }
                    else
                    {
                        YInPos = false;
                    }

                    if ((XInPos == true) && (YInPos == true))
                    {
                        break;
                    }
                }

                if ((XInPos == true) && (YInPos == true))
                {
                    Result = -1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }

        public int CheckPadTolerance(PadObject padparam)
        {
            int Result = 0;
            try
            {
                bool flag = false;

                double pt1_x;
                double pt1_y;
                double pt2_x;
                double pt2_y;

                double padcorner1_x;
                double padcorner1_y;
                double padcorner2_x;
                double padcorner2_y;

                double[,] DieConrerPos = new double[2, 2];

                //double xpos = 0.0;
                //double ypos = 0.0;

                //this.MotionManager().GetActualPos(EnumAxisConstants.X, out xpos);
                //this.MotionManager().GetActualPos(EnumAxisConstants.Y, out ypos);

                //WaferCoordinate padwafercoord = this.CoordinateManager().GetChuckCoordinate(new MachineCoordinate(xpos, ypos), cam.CameraParameters.ChannelType);
                WaferCoordinate padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                //
                MachineIndex MachinDieIndex = this.CoordinateManager().GetCurMachineIndex(padwafercoord);
                //WaferCoordinate CurDieCenter = WaferAlinger.MIndexToWPos(Convert.ToInt32(MachinDieIndex.XIndex), Convert.ToInt32(MachinDieIndex.YIndex));
                UserIndex CurDieUI = this.CoordinateManager().WMIndexConvertWUIndex(MachinDieIndex.XIndex, MachinDieIndex.YIndex);

                DieConrerPos = this.CoordinateManager().GetAnyDieCornerPos(CurDieUI);

                pt1_x = DieConrerPos[0, 0] - (Wafer.GetSubsInfo().DieXClearance.Value / 2);
                pt1_y = DieConrerPos[0, 1] - (Wafer.GetSubsInfo().DieYClearance.Value / 2);

                pt2_x = DieConrerPos[1, 0] + (Wafer.GetSubsInfo().DieXClearance.Value / 2);
                pt2_y = DieConrerPos[1, 1] + (Wafer.GetSubsInfo().DieYClearance.Value / 2);

                padcorner1_x = padwafercoord.X.Value - padparam.PadSizeX.Value / 2;
                padcorner1_y = padwafercoord.Y.Value - padparam.PadSizeY.Value / 2;
                padcorner2_x = padwafercoord.X.Value + padparam.PadSizeX.Value / 2;
                padcorner2_y = padwafercoord.Y.Value + padparam.PadSizeY.Value / 2;

                if (pt1_x <= padcorner1_x && pt1_y >= padcorner1_y && pt2_x >= padcorner2_x && pt2_y <= padcorner2_y)
                {
                    flag = false;
                }
                else
                {
                    flag = true;
                }

                if ((flag == true))
                {
                    Result = -1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Result;
        }

        public void AddPad(PadObject padparameter, WaferCoordinate curcoord, ROIParameter roiparam)
        {
            try
            {
                WaferCoordinate CurDieLeftCorner = new WaferCoordinate();
                Point pt = this.WaferAligner().GetLeftCornerPosition(curcoord.X.Value, curcoord.Y.Value);

                CurDieLeftCorner.X.Value = pt.X;
                CurDieLeftCorner.Y.Value = pt.Y;

                padparameter.PadCenter.X.Value = padparameter.PadCenter.GetX() - CurDieLeftCorner.GetX();
                padparameter.PadCenter.Y.Value = padparameter.PadCenter.GetY() - CurDieLeftCorner.GetY();


                //PadObject padparam = new PadObject();
                //padparam.PadCenter.X.Value = padparameter.PadCenter.GetX() - CurDieLeftCorner.GetX();
                //padparam.PadCenter.Y.Value = padparameter.PadCenter.GetY() - CurDieLeftCorner.GetY();

                //padparam.PadColor = padparameter.PadColor;
                //padparam.PadShape = padparameter.PadShape;
                //padparam.PadSizeX = padparameter.PadSizeX;
                //padparam.PadSizeY = padparameter.PadSizeY;
                //padparam.MachineIndex = padparameter.MachineIndex;

                int Invaildate;

                Invaildate = Invaildation(padparameter);
                if (Invaildate == 0)
                {
                    int CheckPad;
                    int CheckTolerance;
                    // Check Pad Pos
                    CheckPad = CheckPadPosition(padparameter);
                    CheckTolerance = CheckPadTolerance(padparameter);

                    if (CheckPad == 0)
                    {

                        Wafer.GetSubsInfo().Pads.DutPadInfos.Add((DUTPadObject)padparameter);
                    }
                }

                SortPadPositionList(Wafer.GetSubsInfo().Pads.DutPadInfos.ToList());

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "AddPad() : Error occured.");
                LoggerManager.Exception(err);
            }
        }

        private int GetPadparamToPadIndex()
        {
            int nearindex = 0;
            try
            {
                WaferCoordinate padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                Point pt = this.WaferAligner().GetLeftCornerPosition(padwafercoord.X.Value, padwafercoord.Y.Value);

                double minX = Int32.MaxValue;

                if (Wafer.GetSubsInfo().Pads.DutPadInfos.Count() != 0)
                {
                    for (int index = 0; index < Wafer.GetSubsInfo().Pads.DutPadInfos.Count(); index++)
                    {
                        double dist1 = 0.0;

                        dist1 = Math.Sqrt(Math.Pow(Wafer.GetSubsInfo().Pads.DutPadInfos[index].PadCenter.X.Value - (padwafercoord.X.Value - pt.X), 2) +
                            Math.Pow(Wafer.GetSubsInfo().Pads.DutPadInfos[index].PadCenter.Y.Value - (padwafercoord.Y.Value - pt.Y), 2));

                        if (dist1 < minX)
                        {
                            minX = dist1;
                            nearindex = index;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "GetPadparamToPadIndex() : Error occured.");
                LoggerManager.Exception(err);
            }
            return nearindex;

        }
        public void DeletePad(PadObject padparameter, WaferCoordinate curcoord, ROIParameter roiparam)
        {
            try
            {
                //Pads.PadInfo.OrderBy(a => a);
                //string ParamPath = string.Empty;
                //Container.Resolve<IFileManager>().GetXMLFilePath(typeof(PadGroup), out ParamPath);
                //AllPadSearch(padparam);

                //double xpos = 0.0;
                //double ypos = 0.0;

                WaferCoordinate CurDieLeftCorner = new WaferCoordinate();
                Point pt = this.WaferAligner().GetLeftCornerPosition(curcoord.X.Value, curcoord.Y.Value);

                CurDieLeftCorner.X.Value = pt.X;
                CurDieLeftCorner.Y.Value = pt.Y;


                ImageBuffer GetCurGrabImage = new ImageBuffer();

                //double startX = 0.0;
                //double startY = 0.0;

                //startX = pt.X + (Camera.GetGrabSizeWidth() / 2 * Camera.GetRatioX());
                //startY = pt.Y + Wafer.SubsInfo.ActualIndexSize.Height
                //    - Wafer.SubsInfo.DieYClearance - (Camera.GetGrabSizeHeight() / 2 * Camera.GetRatioY());

                PadObject padparam = new PadObject();

                padparam.PadCenter.X.Value = padparameter.PadCenter.GetX() - CurDieLeftCorner.GetX();
                padparam.PadCenter.Y.Value = padparameter.PadCenter.GetY() - CurDieLeftCorner.GetY();

                int Invaildate;

                int delpadindex = -1;
                Invaildate = Invaildation(padparam, ref delpadindex);
                if (Invaildate != 0)
                {
                    //int removeIdx = GetPadparamToPadIndex();
                    Wafer.GetSubsInfo().Pads.DutPadInfos.RemoveAt(delpadindex);
                    //Wafer.GetSubsInfo().Devices[0].Pads.PadInfos.RemoveAt(removeIdx);

                }

                //int CheckPad;
                //int CheckTolerance;
                //// Check Pad Pos
                //CheckPad = CheckPadPosition(padparameter);
                //CheckTolerance = CheckPadTolerance(padparameter);

                //if (CheckPad == 0 && CheckTolerance == 0)
                //{

                //    GraphicsParam Test = new GraphicsParam();

                //    Test.X = padparameter.PadCenter.X.Value;
                //    Test.Y = padparameter.PadCenter.Y.Value;
                //    Test.SizeX = padparameter.PadSizeX;
                //    Test.SizeY = padparameter.PadSizeY;

                //    Test.Shape = padparameter.PadShape;

                //    UpdateDrawList(Test);
                //}


                SortPadPositionList(Wafer.GetSubsInfo().Pads.DutPadInfos.ToList());

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "DeletePad() : Error occured.");
                LoggerManager.Exception(err);

            }
        }
        public void UpdateDrawList(GraphicsParam r)
        {
            try
            {
                Graphicsparam.Add(r);
                //public ObservableCollection<GraphicsParam> Graphicsparam { get; set; }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void AddPad(PadObject padparam, double xpos, double ypos)
        {
            try
            {
                SortPadPositionList(Wafer.GetSubsInfo().Devices[0].Pads.DutPadInfos.ToList());
                //WaferCoordinate padwafercoord = this.CoordinateManager().GetChuckCoordinate(new MachineCoordinate(xpos, ypos), Camera.GetChannelType());

                //padparam.PadCenter.X.Value = padwafercoord.X.Value;
                //padparam.PadCenter.Y.Value = padwafercoord.Y.Value;

                //Pads.PadInfo.Add(padparam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void MoveToPad(PadObject padparam, ROIParameter roiparam)
        {
            try
            {
                BlobResult blobresult = this.VisionManager().Blob(
                    Camera.GetChannelType(),
                    padparam.BlobParam,
                    roiparam);

                double halfsizex = blobresult.ResultBuffer.SizeX / 2;
                double halfsizey = blobresult.ResultBuffer.SizeY / 2;

                double offsetx = 0.0;
                double offsety = 0.0;

                double xpos = 0.0;
                double ypos = 0.0;

                //PadParameter padparam = new PadParameter();

                if (blobresult.DevicePositions.Count == 1)
                {
                    offsetx = halfsizex - Math.Abs(blobresult.DevicePositions[0].PosX);
                    offsety = halfsizey - Math.Abs(blobresult.DevicePositions[0].PosY);

                    offsetx *= Camera.GetRatioX();
                    offsety *= Camera.GetRatioY();

                    this.MotionManager().GetActualPos(EnumAxisConstants.X, ref xpos);
                    this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref ypos);

                    xpos += offsetx;
                    ypos += offsety;

                    //WaferCoordinate padwafercoord = this.CoordinateManager().GetChuckCoordinate(new MachineCoordinate(xpos, ypos), Camera.GetChannelType());

                    //cam.CameraMoveMethod.MoveAsync(padwafercoord.X.Value, padwafercoord.Y.Value, 1826);

                    this.MotionManager().WaitForMotionDoneAsync();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Task<EventCodeEnum> AllPadSearch()
        {
            //string ParamPath = string.Empty;
            //Container.Resolve<IFileManager>().GetXMLFilePath(typeof(PadGroup), out ParamPath);

            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                ROIParameter roiparam = new ROIParameter();

                //this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM).StopGrab();

                double MoveXNum;
                double MoveYNum;

                //double curXpos = 0.0;
                //double curYpos = 0.0;

                double NextPosX = 0.0;
                double NextPosY = 0.0;

                double DefaultMovingPosX;
                double DefaultMovingPosY;

                // Pixel
                double EdgeMarginX = 5;
                double EdgeMarginY = 5;

                ICamera cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);

                //현재는 패드 탐색 영역이 가변적으로 변하게 되어있지만, 480*480 영역에 패드 사이즈만큼을 더 주어 동일한 크기로 이동하도록 코드 수정해야함.(패드 사이즈를 알고있다는 가정하에)
                DefaultMovingPosX = (cam.GetGrabSizeWidth() * cam.GetRatioX()) - Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeX.Value - (EdgeMarginX * cam.GetRatioX());
                DefaultMovingPosY = (cam.GetGrabSizeHeight() * cam.GetRatioY()) - Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeY.Value - (EdgeMarginY * cam.GetRatioY());

                // Current Position
                WaferCoordinate padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                WaferCoordinate DieEndPos = new WaferCoordinate();
                ImageBuffer GetCurGrabImage = new ImageBuffer();

                DieEndPos.X.Value = padwafercoord.X.Value - Wafer.GetSubsInfo().ActualDieSize.Width.Value;
                DieEndPos.Y.Value = padwafercoord.Y.Value - Wafer.GetSubsInfo().ActualDieSize.Height.Value;

                MoveXNum = Math.Round((Wafer.GetSubsInfo().ActualDieSize.Width.Value / (cam.GetGrabSizeWidth() * cam.GetRatioX())));
                MoveYNum = Math.Round((Wafer.GetSubsInfo().ActualDieSize.Height.Value / (cam.GetGrabSizeHeight() * cam.GetRatioY())));

                //int Xsign = -1;
                //int Ysign = -1;

                //padwafercoord.X.Value -= (cam.CameraParameters.GrabSizeX.Value * cam.CameraParameters.RatioX.Value) / 2;
                //padwafercoord.Y.Value -= (cam.CameraParameters.GrabSizeY.Value * cam.CameraParameters.RatioY.Value) / 2;

                roiparam.OffsetX.Value = 0;
                roiparam.OffsetY.Value = 0;

                roiparam.Width.Value = (int)cam.GetGrabSizeWidth();
                roiparam.Height.Value = (int)cam.GetGrabSizeHeight();

                //this.VisionManager().DigitizerService[cam.CameraParameters.DigiNumber].BlockGrabEvent();

                // Start
                LoggerManager.Debug($"Die Corner Start pos : {padwafercoord.X.Value}, {padwafercoord.Y.Value}");

                // End
                LoggerManager.Debug($"Die Corner End pos : {DieEndPos.X.Value}, {DieEndPos.Y.Value}");


                //WaferAlign이 안되서 임시방편으로 만듬
                WaferCoordinate CurDieLeftCorner = new WaferCoordinate();
                Point pt = this.WaferAligner().GetLeftCornerPosition(padwafercoord.X.Value, padwafercoord.Y.Value);

                CurDieLeftCorner.X.Value = pt.X;
                CurDieLeftCorner.Y.Value = pt.Y;


                double limitX = 0.0;
                double limitY = 0.0;
                // CurDieCenter.Y.Value = -5721;  

                //CurDieCenter = this.CoordinateManager().GetCurDieCenterPos();
                //double movex = 0.0;
                //double movey = 0.0;

                //오른쪽 방향
                limitX = CurDieLeftCorner.X.Value +
                    Wafer.GetSubsInfo().ActualDieSize.Width.Value - Wafer.GetSubsInfo().DieXClearance.Value;
                do
                {
                    padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    GetCurGrabImage = this.VisionManager().SingleGrab(cam.GetChannelType(), this);
                    ObservableCollection<GrabDevPosition> position = new ObservableCollection<GrabDevPosition>();
                    position = this.VisionManager().Detect_Pad(GetCurGrabImage, Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeX.Value, Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeY.Value, roiparam);

                    NextPosX = padwafercoord.X.Value + DefaultMovingPosX;
                    NextPosY = DefaultMovingPosY;

                    this.StageSupervisor().StageModuleState.WaferHighViewMove(NextPosX, padwafercoord.Y.Value, Wafer.GetPhysInfo().Thickness.Value);

                    //if (position.Count == 0)
                    //{
                    //    NextPosX = padwafercoord.X.Value + DefaultMovingPosX;
                    //    NextPosY = DefaultMovingPosY;

                    //    StageSupervisor.StageModuleState.WaferHighViewMove(NextPosX, padwafercoord.Y.Value, Wafer.GetSubsInfo().Devices[0].Thickness);

                    //}

                    //else if (position.Count >= 1)
                    //{                       
                    //    double offsetx = position[0].PosX - (cam.CameraParameters.GrabSizeX.Value / 2);

                    //    if(position[0].PosX > (cam.CameraParameters.GrabSizeX.Value / 2))
                    //    {
                    //        offsetx = position[0].PosX - (cam.CameraParameters.GrabSizeX.Value / 2);
                    //    }
                    //    else
                    //    {
                    //       offsetx = (cam.CameraParameters.GrabSizeX.Value / 2) - position[0].PosX;
                    //       offsetx *= -1;
                    //    }

                    //    movex = padwafercoord.X.Value + (offsetx * cam.CameraParameters.RatioX.Value) +
                    //    ((cam.CameraParameters.GrabSizeX.Value / 2) * cam.CameraParameters.RatioX.Value);

                    //    StageSupervisor.StageModuleState.WaferHighViewMove(movex, padwafercoord.Y.Value, Wafer.GetSubsInfo().Devices[0].Thickness);              
                    //}

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

                        padparam.PadCenter.X.Value = pixelpadx - CurDieLeftCorner.X.Value;
                        padparam.PadCenter.Y.Value = pixelpady - CurDieLeftCorner.Y.Value;

                        //Pads.PadInfo.Add(new PadParameter(position[count].SizeX, position[count].SizeY, 
                        //    new PadCoordinate(pixelpadx - CurDieLeftCorner.X.Value, pixelpady - CurDieLeftCorner.Y.Value)));
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            int Invaildate;
                            Invaildate = Invaildation(padparam);
                            if (Invaildate == 0)
                            {
                                Wafer.GetSubsInfo().Devices[0].Pads.DutPadInfos.Add((DUTPadObject)padparam);
                                //SortPadPositionList(Pads.PadInfo, 50);
                            }
                        });


                        int CheckPad;
                        int CheckTolerance;
                        // Check Pad Pos
                        CheckPad = CheckPadPosition(padparam);
                        CheckTolerance = CheckPadTolerance(padparam);

                        if (CheckPad == 0 && CheckTolerance == 0)
                        {

                            //Pads.PadInfo.OrderBy(a => a.PadCenter.X.Value);
                            //RWDeviceFile();

                            GraphicsParam Test = new GraphicsParam();

                            Test.X = padparam.PadCenter.X.Value;
                            Test.Y = padparam.PadCenter.Y.Value;
                            Test.SizeX = padparam.PadSizeX.Value;
                            Test.SizeY = padparam.PadSizeY.Value;

                            Test.Shape = padparam.PadShape.Value;

                            UpdateDrawList(Test);
                        }
                    }

                    //SaveDevParameter();
                }
                while (NextPosX + ((cam.GetGrabSizeWidth()) * (cam.GetRatioX())) < limitX);

                //아래 방향
                limitY = CurDieLeftCorner.Y.Value;
                do
                {
                    padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    GetCurGrabImage = this.VisionManager().SingleGrab(cam.GetChannelType(), this);
                    ObservableCollection<GrabDevPosition> position = new ObservableCollection<GrabDevPosition>();
                    position = this.VisionManager().Detect_Pad(GetCurGrabImage, Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeX.Value, Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeY.Value, roiparam);

                    NextPosX = DefaultMovingPosX;
                    NextPosY = padwafercoord.Y.Value - DefaultMovingPosY;

                    this.StageSupervisor().StageModuleState.WaferHighViewMove(padwafercoord.X.Value, NextPosY, Wafer.GetPhysInfo().Thickness.Value);

                    //if (position.Count == 0)
                    //{
                    //    NextPosX = DefaultMovingPosX;
                    //    NextPosY = padwafercoord.Y.Value - DefaultMovingPosY;

                    //    StageSupervisor.StageModuleState.WaferHighViewMove(padwafercoord.X.Value, NextPosY, Wafer.GetSubsInfo().Devices[0].Thickness);
                    //}

                    //else if (position.Count >= 1) 
                    //{
                    //    double offsety = 0.0;

                    //    if(position[position.Count - 1].PosY > (cam.CameraParameters.GrabSizeY.Value / 2))
                    //    {
                    //        offsety = position[position.Count - 1].PosY - (cam.CameraParameters.GrabSizeY.Value / 2);
                    //    }
                    //    else
                    //    {
                    //        offsety = (cam.CameraParameters.GrabSizeY.Value / 2) - position[position.Count - 1].PosY;
                    //        offsety *= -1;
                    //    }

                    //    movey = padwafercoord.Y.Value - (offsety * cam.CameraParameters.RatioY.Value) -
                    //    ((cam.CameraParameters.GrabSizeY.Value / 2) * cam.CameraParameters.RatioY.Value);

                    //    StageSupervisor.StageModuleState.WaferHighViewMove(padwafercoord.X.Value, movey, Wafer.GetSubsInfo().Devices[0].Thickness);               
                    //}

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

                        padparam.PadCenter.X.Value = pixelpadx - CurDieLeftCorner.X.Value;
                        padparam.PadCenter.Y.Value = pixelpady - CurDieLeftCorner.Y.Value;

                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            int Invaildate;
                            Invaildate = Invaildation(padparam);
                            if (Invaildate == 0)
                            {
                                Wafer.GetSubsInfo().Devices[0].Pads.DutPadInfos.Add((DUTPadObject)padparam);
                                //SortPadPositionList(Pads.PadInfo, 50);
                            }
                        });
                        //Pads.PadInfo.Add(new PadParameter(position[count].SizeX, position[count].SizeY, 
                        //    new PadCoordinate(pixelpadx - CurDieLeftCorner.X.Value, pixelpady - CurDieLeftCorner.Y.Value)));

                        int CheckPad;
                        int CheckTolerance;
                        // Check Pad Pos
                        CheckPad = CheckPadPosition(padparam);
                        CheckTolerance = CheckPadTolerance(padparam);

                        if (CheckPad == 0 && CheckTolerance == 0)
                        {
                            //Pads.PadInfo.Add(padparam);
                            //Pads.PadInfo.OrderBy(a => a.PadCenter.Y.Value);
                            //RWDeviceFile();

                            GraphicsParam Test = new GraphicsParam();

                            Test.X = padparam.PadCenter.X.Value;
                            Test.Y = padparam.PadCenter.Y.Value;
                            Test.SizeX = padparam.PadSizeX.Value;
                            Test.SizeY = padparam.PadSizeY.Value;

                            Test.Shape = padparam.PadShape.Value;

                            UpdateDrawList(Test);
                        }
                    }

                    //SaveDevParameter();
                }
                while (NextPosY - (cam.GetGrabSizeHeight() / 2) > limitY);

                //왼쪽 방향
                limitX = CurDieLeftCorner.X.Value;
                do
                {
                    padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    GetCurGrabImage = this.VisionManager().SingleGrab(cam.GetChannelType(),this);
                    ObservableCollection<GrabDevPosition> position = new ObservableCollection<GrabDevPosition>();
                    position = this.VisionManager().Detect_Pad(GetCurGrabImage, Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeX.Value, Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeY.Value, roiparam);

                    NextPosX = padwafercoord.X.Value - DefaultMovingPosX;
                    NextPosY = DefaultMovingPosY;

                    this.StageSupervisor().StageModuleState.WaferHighViewMove(NextPosX, padwafercoord.Y.Value, Wafer.GetPhysInfo().Thickness.Value);

                    //if (position.Count == 0)
                    //{
                    //    NextPosX = padwafercoord.X.Value - DefaultMovingPosX;
                    //    NextPosY = DefaultMovingPosY;

                    //    StageSupervisor.StageModuleState.WaferHighViewMove(NextPosX, padwafercoord.Y.Value, Wafer.GetSubsInfo().Devices[0].Thickness);

                    //}

                    //else if (position.Count >= 1)
                    //{
                    //    double offsetx = 0.0;

                    //    if (position[position.Count - 1].PosX < cam.CameraParameters.GrabSizeX.Value / 2)
                    //    {
                    //        offsetx = (cam.CameraParameters.GrabSizeX.Value / 2) - position[position.Count - 1].PosX;
                    //        offsetx *= -1;
                    //    }
                    //    else
                    //    {
                    //        offsetx = position[position.Count - 1].PosX - (cam.CameraParameters.GrabSizeX.Value / 2);

                    //    }

                    //    movex = padwafercoord.X.Value + (offsetx * cam.CameraParameters.RatioX.Value) -
                    //    ((cam.CameraParameters.GrabSizeX.Value / 2) * cam.CameraParameters.RatioX.Value);

                    //    StageSupervisor.StageModuleState.WaferHighViewMove(movex, padwafercoord.Y.Value, Wafer.GetSubsInfo().Devices[0].Thickness);               
                    //}

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

                        padparam.PadCenter.X.Value = pixelpadx - CurDieLeftCorner.X.Value;
                        padparam.PadCenter.Y.Value = pixelpady - CurDieLeftCorner.Y.Value;

                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            int Invaildate;
                            Invaildate = Invaildation(padparam);
                            if (Invaildate == 0)
                            {
                                Wafer.GetSubsInfo().Devices[0].Pads.DutPadInfos.Add((DUTPadObject)padparam);
                                //SortPadPositionList(Pads.PadInfo, 50);
                            }
                        });
                        //Pads.PadInfo.Add(new PadParameter(position[count].SizeX, position[count].SizeY, 
                        //    new PadCoordinate(pixelpadx - CurDieLeftCorner.X.Value, pixelpady - CurDieLeftCorner.Y.Value)));

                        int CheckPad;
                        int CheckTolerance;
                        // Check Pad Pos
                        CheckPad = CheckPadPosition(padparam);
                        CheckTolerance = CheckPadTolerance(padparam);

                        if (CheckPad == 0 && CheckTolerance == 0)
                        {
                            //Pads.PadInfo.Add(padparam);
                            //Pads.PadInfo.OrderByDescending(a => a.PadCenter.X.Value);
                            //RWDeviceFile();

                            GraphicsParam Test = new GraphicsParam();

                            Test.X = padparam.PadCenter.X.Value;
                            Test.Y = padparam.PadCenter.Y.Value;
                            Test.SizeX = padparam.PadSizeX.Value;
                            Test.SizeY = padparam.PadSizeY.Value;

                            Test.Shape = padparam.PadShape.Value;

                            UpdateDrawList(Test);
                        }
                    }

                    //SaveDevParameter();
                }
                while (NextPosX - (cam.GetGrabSizeWidth() / 2) > limitX);

                //위 방향
                limitY = CurDieLeftCorner.Y.Value +
                    Wafer.GetSubsInfo().ActualDieSize.Width.Value
                    - Wafer.GetSubsInfo().DieXClearance.Value;
                do
                {
                    padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    GetCurGrabImage = this.VisionManager().SingleGrab(cam.GetChannelType(),this);
                    ObservableCollection<GrabDevPosition> position = new ObservableCollection<GrabDevPosition>();
                    position = this.VisionManager().Detect_Pad(GetCurGrabImage, Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeX.Value, Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeY.Value, roiparam);

                    NextPosX = DefaultMovingPosX;
                    NextPosY = padwafercoord.Y.Value + DefaultMovingPosY;

                    this.StageSupervisor().StageModuleState.WaferHighViewMove(padwafercoord.X.Value, NextPosY, Wafer.GetPhysInfo().Thickness.Value);

                    //if (position.Count == 0)
                    //{
                    //    NextPosX = DefaultMovingPosX;
                    //    NextPosY = padwafercoord.Y.Value + DefaultMovingPosY;

                    //    StageSupervisor.StageModuleState.WaferHighViewMove(padwafercoord.X.Value, NextPosY, Wafer.GetSubsInfo().Devices[0].Thickness);

                    //}

                    //else if (position.Count >= 1)
                    //{                               
                    //    double offsety = 0.0;
                    //    if (position[0].PosY < (cam.CameraParameters.GrabSizeY.Value / 2))
                    //    {
                    //        offsety = position[0].PosY - (cam.CameraParameters.GrabSizeY.Value / 2);
                    //        offsety *= -1;

                    //    }
                    //    else
                    //    {
                    //        offsety = (cam.CameraParameters.GrabSizeY.Value / 2) - position[0].PosY ;                        
                    //    }

                    //    movey = padwafercoord.Y.Value + (offsety * cam.CameraParameters.RatioY.Value) +
                    //    ((cam.CameraParameters.GrabSizeY.Value / 2) * cam.CameraParameters.RatioY.Value);

                    //    StageSupervisor.StageModuleState.WaferHighViewMove(padwafercoord.X.Value, movey, Wafer.GetSubsInfo().Devices[0].Thickness);               
                    //}

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

                        padparam.PadCenter.X.Value = pixelpadx - CurDieLeftCorner.X.Value;
                        padparam.PadCenter.Y.Value = pixelpady - CurDieLeftCorner.Y.Value;


                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            int Invaildate;
                            Invaildate = Invaildation(padparam);
                            if (Invaildate == 0)
                            {
                                Wafer.GetSubsInfo().Devices[0].Pads.DutPadInfos.Add((DUTPadObject)padparam);
                                //SortPadPositionList(Pads.PadInfo, 50);
                            }
                        });

                        //Pads.PadInfo.Add(new PadParameter(position[count].SizeX, position[count].SizeY, 
                        //    new PadCoordinate(pixelpadx - CurDieLeftCorner.X.Value, pixelpady - CurDieLeftCorner.Y.Value)));

                        int CheckPad;
                        int CheckTolerance;
                        // Check Pad Pos
                        CheckPad = CheckPadPosition(padparam);
                        CheckTolerance = CheckPadTolerance(padparam);

                        if (CheckPad == 0 && CheckTolerance == 0)
                        {
                            //Pads.PadInfo.Add(padparam);
                            //Pads.PadInfo.OrderByDescending(a => a.PadCenter.Y.Value);
                            //RWDeviceFile();

                            GraphicsParam Test = new GraphicsParam();

                            Test.X = padparam.PadCenter.X.Value;
                            Test.Y = padparam.PadCenter.Y.Value;
                            Test.SizeX = padparam.PadSizeX.Value;
                            Test.SizeY = padparam.PadSizeY.Value;

                            Test.Shape = padparam.PadShape.Value;

                            UpdateDrawList(Test);
                        }
                    }
                }
                while (padwafercoord.Y.Value + (cam.GetGrabSizeHeight()) < limitY);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public void PrevPad()
        {
            try
            {
                WaferCoordinate padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                Point pt = this.WaferAligner().GetLeftCornerPosition(padwafercoord.X.Value, padwafercoord.Y.Value);

                double offsetx = 0.0;
                double offsety = 0.0;

                if (Wafer.GetSubsInfo().Pads.Flag >= 0)
                {
                    Wafer.GetSubsInfo().Pads.Flag -= 1;

                    if (Wafer.GetSubsInfo().Pads.Flag <= 0)
                    {
                        Wafer.GetSubsInfo().Pads.Flag = 0;
                    }
                    else if (Wafer.GetSubsInfo().Pads.Flag >= Wafer.GetSubsInfo().Pads.DutPadInfos.Count())
                    {
                        Wafer.GetSubsInfo().Pads.Flag = Wafer.GetSubsInfo().Pads.DutPadInfos.Count();
                    }
                    offsetx = pt.X + Math.Abs(Wafer.GetSubsInfo().Pads.DutPadInfos[Wafer.GetSubsInfo().Pads.Flag].PadCenter.X.Value);
                    offsety = pt.Y + Math.Abs(Wafer.GetSubsInfo().Pads.DutPadInfos[Wafer.GetSubsInfo().Pads.Flag].PadCenter.Y.Value);

                    this.StageSupervisor().StageModuleState.WaferHighViewMove(offsetx, offsety, Wafer.GetPhysInfo().Thickness.Value);
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "PrevPad() : Error occured.");
                LoggerManager.Exception(err);

            }

        }

        public void NextPad()
        {
            try
            {
                WaferCoordinate padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                Point pt = this.WaferAligner().GetLeftCornerPosition(padwafercoord.X.Value, padwafercoord.Y.Value);

                double offsetx = 0.0;
                double offsety = 0.0;

                //double xpos = 0.0;
                //double ypos = 0.0;

                if (Wafer.GetSubsInfo().Pads.Flag <= Wafer.GetSubsInfo().Pads.DutPadInfos.Count())
                {
                    Wafer.GetSubsInfo().Pads.Flag += 1;

                    if (Wafer.GetSubsInfo().Pads.Flag <= 0)
                    {
                        Wafer.GetSubsInfo().Pads.Flag = 0;
                    }
                    else if (Wafer.GetSubsInfo().Pads.Flag >= Wafer.GetSubsInfo().Pads.DutPadInfos.Count())
                    {
                        Wafer.GetSubsInfo().Pads.Flag = Wafer.GetSubsInfo().Pads.DutPadInfos.Count() - 1;
                    }

                    offsetx = pt.X + Math.Abs(Wafer.GetSubsInfo().Pads.DutPadInfos[Wafer.GetSubsInfo().Pads.Flag].PadCenter.X.Value);
                    offsety = pt.Y + Math.Abs(Wafer.GetSubsInfo().Pads.DutPadInfos[Wafer.GetSubsInfo().Pads.Flag].PadCenter.Y.Value);

                    this.StageSupervisor().StageModuleState.WaferHighViewMove(offsetx, offsety, Wafer.GetPhysInfo().Thickness.Value);
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "NextPad() : Error occured.");
                LoggerManager.Exception(err);
            }
        }

        public void FindPad()
        {
            try
            {
                WaferCoordinate padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                Point pt = this.WaferAligner().GetLeftCornerPosition(padwafercoord.X.Value, padwafercoord.Y.Value);

                double minX = Int32.MaxValue;
                int nearindex = 0;

                if (Wafer.GetSubsInfo().Devices[0].Pads.DutPadInfos.Count() != 0)
                {
                    for (int index = 0; index < Wafer.GetSubsInfo().Pads.DutPadInfos.Count(); index++)
                    {
                        double dist1 = 0.0;

                        dist1 = Math.Sqrt(Math.Pow(Wafer.GetSubsInfo().Pads.DutPadInfos[index].PadCenter.X.Value - (padwafercoord.X.Value - pt.X), 2) + Math.Pow(Wafer.GetSubsInfo().Devices[0].Pads.DutPadInfos[index].PadCenter.Y.Value - (padwafercoord.Y.Value - pt.Y), 2));

                        if (dist1 < minX)
                        {
                            minX = dist1;
                            nearindex = index;
                        }
                    }

                    double offsetX = pt.X + Math.Abs(Wafer.GetSubsInfo().Pads.DutPadInfos[nearindex].PadCenter.X.Value);
                    double offsetY = pt.Y + Math.Abs(Wafer.GetSubsInfo().Pads.DutPadInfos[nearindex].PadCenter.Y.Value);

                    this.StageSupervisor().StageModuleState.WaferHighViewMove(offsetX, offsetY, Wafer.GetPhysInfo().Thickness.Value);
                }

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "FindPad() : Error occured.");
                LoggerManager.Exception(err);
            }

        }

        public Task<EventCodeEnum> DieAllSearch()
        {
            //string ParamPath = string.Empty;
            //Container.Resolve<IFileManager>().GetXMLFilePath(typeof(PadGroup), out ParamPath);

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
                double limitY = 0.0;

                double movex = 0.0;
                //double movey = 0.0;

                double NextPosX = 0.0;
                double NextPosY = 0.0;

                double DefaultMovingPosX;
                double DefaultMovingPosY;

                double CurlimitX = 0.0;
                double CurlimitY = 0.0;

                // Pixel
                double EdgeMarginX = 5;
                double EdgeMarginY = 5;

                DefaultMovingPosX = (cam.GetGrabSizeWidth() * cam.GetRatioX()) - Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeX.Value - (EdgeMarginX * cam.GetRatioX());
                DefaultMovingPosY = (cam.GetGrabSizeHeight() * cam.GetRatioY()) - Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeY.Value - (EdgeMarginY * cam.GetRatioY());

                do
                {
                    padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                    GetCurGrabImage = this.VisionManager().SingleGrab(cam.GetChannelType(), this);
                    ObservableCollection<GrabDevPosition> position = new ObservableCollection<GrabDevPosition>();
                    position = this.VisionManager().Detect_Pad(GetCurGrabImage, Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeX.Value, Wafer.GetSubsInfo().Devices[0].Pads.SetPadObject.PadSizeY.Value, roiparam);

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

                        padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                        CurlimitX = CurDieLeftCorner.X.Value + (cam.GetGrabSizeHeight() / 2); ;
                        limitX = padwafercoord.X.Value - (cam.GetGrabSizeWidth() / 2);

                        CurlimitY = CurDieLeftCorner.Y.Value + (cam.GetGrabSizeHeight() / 2);
                        limitY = padwafercoord.Y.Value - (cam.GetGrabSizeWidth() / 2);

                        CurlimitY = CurDieLeftCorner.Y.Value + (cam.GetGrabSizeHeight() / 2);
                        limitY = padwafercoord.Y.Value - (cam.GetGrabSizeWidth());

                        if (limitX < CurlimitX)
                        {
                            if (limitY > CurlimitY)
                            {
                                NextPosY = padwafercoord.Y.Value - DefaultMovingPosY;
                                this.StageSupervisor().StageModuleState.WaferHighViewMove(padwafercoord.X.Value, NextPosY, Wafer.GetPhysInfo().Thickness.Value);

                                moveDir = EnumMoveDirection.Right;
                                limitX = limitX + (cam.GetGrabSizeWidth());
                            }
                            else
                            {
                                limitX = limitX - (cam.GetGrabSizeWidth());
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

                        padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();

                        //limitX = CurDieLeftCorner.X.Value + this.WaferAligner().WAInfoFile.DieSizeActualX 
                        //    - this.WaferAligner().WAInfoFile.DieXClearance;
                        //CurlimitX = padwafercoord.X.Value + (cam.GetGrabSizeWidth() / 2);

                        //limitY = CurDieLeftCorner.Y.Value + this.WaferAligner().WAInfoFile.DieSizeActualY 
                        //    - this.WaferAligner().WAInfoFile.DieYClearance;

                        limitX = CurDieLeftCorner.X.Value + Wafer.GetSubsInfo().ActualDieSize.Width.Value
                            - Wafer.GetSubsInfo().DieXClearance.Value;
                        CurlimitX = padwafercoord.X.Value + (cam.GetGrabSizeWidth() / 2);

                        limitY = CurDieLeftCorner.Y.Value + Wafer.GetSubsInfo().ActualDieSize.Height.Value
                            - Wafer.GetSubsInfo().DieYClearance.Value;


                        CurlimitY = padwafercoord.Y.Value + (cam.GetGrabSizeWidth() / 2);

                        if (CurlimitX > limitX)
                        {
                            if (limitY > CurlimitY)
                            {
                                NextPosY = padwafercoord.Y.Value - DefaultMovingPosY;
                                this.StageSupervisor().StageModuleState.WaferHighViewMove(padwafercoord.X.Value, NextPosY, Wafer.GetPhysInfo().Thickness.Value);

                                moveDir = EnumMoveDirection.Left;
                                CurlimitX = CurlimitX - (cam.GetGrabSizeWidth());
                            }

                            else
                            {
                                CurlimitX = CurlimitX + (cam.GetGrabSizeWidth() / 2);
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
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Wafer.GetSubsInfo().Devices[0].Pads.DutPadInfos.Add((DUTPadObject)padparam);
                        });

                        int CheckPad;
                        int CheckTolerance;
                        // Check Pad Pos
                        CheckPad = CheckPadPosition(padparam);
                        CheckTolerance = CheckPadTolerance(padparam);

                        if (CheckPad == 0 && CheckTolerance == 0)
                        {

                            //Pads.PadInfo.OrderBy(a => a.PadCenter.X.Value);
                            //RWDeviceFile();

                            GraphicsParam Test = new GraphicsParam();

                            Test.X = padparam.PadCenter.X.Value;
                            Test.Y = padparam.PadCenter.Y.Value;
                            Test.SizeX = padparam.PadSizeX.Value;
                            Test.SizeY = padparam.PadSizeY.Value;

                            Test.Shape = padparam.PadShape.Value;

                            UpdateDrawList(Test);
                        }
                    }

                    //SaveDevParameter();

                } while (CurlimitX < limitX);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public void SortPadPositionList(List<DUTPadObject> padinfo)
        {
            try
            {
                List<PadObject> padlist = new List<PadObject>();

                //WaferCoordinate padwafercoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();            
                //Point pt = this.WaferAligner().PositionFormLeftBottomCorner(padwafercoord.X.Value, padwafercoord.Y.Value);
                //double dist1 = 0.0;
                //double dist2 = 0.0;

                foreach (PadObject item in padinfo)
                {
                    padlist.Add(item);
                }

                for (int index = padlist.Count() - 1; index > 0; index--)
                {
                    for (int j = 0; j < index; j++)
                    {
                        if (padlist[j].PadCenter.X.Value > padlist[j + 1].PadCenter.X.Value)
                        {
                            WaferCoordinate xtemp = padlist[j].PadCenter;
                            padlist[j].PadCenter = padlist[j + 1].PadCenter;
                            padlist[j + 1].PadCenter = xtemp;
                            if (Math.Abs(padlist[j].PadCenter.X.Value - padlist[j + 1].PadCenter.X.Value) < 10)
                            {
                                if (padlist[j].PadCenter.Y.Value > padlist[j + 1].PadCenter.Y.Value)
                                {
                                    WaferCoordinate ytemp = padlist[j].PadCenter;
                                    padlist[j].PadCenter = padlist[j + 1].PadCenter;
                                    padlist[j + 1].PadCenter = ytemp;
                                }
                            }
                        }
                        else
                        {
                            if (Math.Abs(padlist[j].PadCenter.X.Value - padlist[j + 1].PadCenter.X.Value) < 10)
                            {
                                if (padlist[j].PadCenter.Y.Value > padlist[j + 1].PadCenter.Y.Value)
                                {
                                    WaferCoordinate ytemp = padlist[j].PadCenter;
                                    padlist[j].PadCenter = padlist[j + 1].PadCenter;
                                    padlist[j + 1].PadCenter = ytemp;
                                }
                            }
                        }
                    }
                }
                padinfo.Clear();

                for (int index = 0; index < padlist.Count(); index++)
                {
                    padlist[index].Index.Value = index;
                    padinfo.Add((DUTPadObject)padlist[index]);
                }
                if (Wafer.GetSubsInfo().Pads.DutPadInfos != null)
                {
                    if (Wafer.GetSubsInfo().Pads.DutPadInfos.Count() != 0)
                    {
                        Wafer.GetSubsInfo().Pads.RefPad = new PadObject();
                        Wafer.GetSubsInfo().Pads.RefPad = padlist[0];
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private int Invaildation(PadObject padparam, ref int padindex)
        {
            int result = 0;
            try
            {
                bool isXflag = false;
                bool isYflag = false;

                double distX = 0.0;
                double distY = 0.0;

                for (int index = 0; index < Wafer.GetSubsInfo().Pads.DutPadInfos.Count(); index++)
                {
                    isXflag = false;
                    isYflag = false;

                    distX = Math.Abs(Wafer.GetSubsInfo().Pads.DutPadInfos[index].PadCenter.X.Value - padparam.PadCenter.X.Value);
                    distY = Math.Abs(Wafer.GetSubsInfo().Pads.DutPadInfos[index].PadCenter.Y.Value - padparam.PadCenter.Y.Value);

                    if (distX < 10)
                    {
                        isXflag = true;
                    }
                    else
                    {
                        isXflag = false;
                    }

                    if (distY < 10)
                    {
                        isYflag = true;
                    }
                    else
                    {
                        isYflag = false;
                    }

                    if (isXflag == true && isYflag == true)
                    {
                        padindex = index;
                        break;
                    }

                }

                if (isXflag == true && isYflag == true)
                {
                    //Pads.PadInfo.Remove(padparam);
                    result = -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return result;
        }
        private int Invaildation(PadObject padparam)
        {
            int result = 0;
            try
            {
                bool isXflag = false;
                bool isYflag = false;

                double distX = 0.0;
                double distY = 0.0;

                for (int index = 0; index < Wafer.GetSubsInfo().Pads.DutPadInfos.Count(); index++)
                {
                    isXflag = false;
                    isYflag = false;

                    distX = Math.Abs(Wafer.GetSubsInfo().Pads.DutPadInfos[index].PadCenter.X.Value - padparam.PadCenter.X.Value);
                    distY = Math.Abs(Wafer.GetSubsInfo().Pads.DutPadInfos[index].PadCenter.Y.Value - padparam.PadCenter.Y.Value);

                    if (distX < 10)
                    {
                        isXflag = true;
                    }
                    else
                    {
                        isXflag = false;
                    }

                    if (distY < 10)
                    {
                        isYflag = true;
                    }
                    else
                    {
                        isYflag = false;
                    }

                    if (isXflag == true && isYflag == true)
                    {
                        break;
                    }

                }

                if (isXflag == true && isYflag == true)
                {
                    //Pads.PadInfo.Remove(padparam);
                    result = -1;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return result;
        }


        public EventCodeEnum DefaultParameterSet(int Index)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (Index == 0)
                {
                    Pads = new PadGroup();

                    BlobParameter mblobparam = new BlobParameter();

                    mblobparam.BlobMinRadius.Value = 3;
                    mblobparam.BlobThreshHold.Value = 120;
                    mblobparam.MinBlobArea.Value = 50;

                    //Pads.BlobParam = mblobparam;

                    Pads.SetPadObject.PadSizeX.Value = 80;
                    Pads.SetPadObject.PadSizeY.Value = 80;
                    Pads.Flag = 0;

                    Pads.DutPadInfos.Clear();
                }
                else if (Index == 1)
                {


                }
                else if (Index == 2)
                {

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        //public EventCodeEnum LoadDevParameter()
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

        //    IParam tmpParam = null;
        //    tmpParam = new PadGroup();
        //    tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
        //    RetVal = Extensions_IParam.LoadParameter(ref tmpParam, typeof(PadGroup));

        //    if (RetVal == EventCodeEnum.NONE)
        //    {
        //        Pads = tmpParam as PadGroup;
        //    }

        //    DevParam = new IParamEmpty();

        //    return RetVal;
        //}

        //public EventCodeEnum SaveDevParameter()
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

        //    RetVal = Extensions_IParam.SaveParameter(Pads);

        //    return RetVal;
        //}

    }
}
