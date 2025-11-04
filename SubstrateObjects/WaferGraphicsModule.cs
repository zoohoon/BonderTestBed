
using System;
using System.Collections.Generic;
using System.Linq;

namespace SubstrateObjects
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml.Serialization;
    using Vision.GraphicsContext;

    public class WaferAlignSetupControl
    {
        private bool _CenterDie;
        public bool CenterDie
        {
            get { return _CenterDie; }
            set
            {
                if (value != _CenterDie)
                {
                    _CenterDie = value;
                }
            }
        }

        private bool _Edge_Center;
        public bool Edge_Center
        {
            get { return _Edge_Center; }
            set
            {
                if (value != _Edge_Center)
                {
                    _Edge_Center = value;
                }
            }
        }

        private int _Edge_Center_Area;
        public int Edge_Center_Area
        {
            get { return _Edge_Center_Area; }
            set
            {
                if (value != _Edge_Center_Area)
                {
                    _Edge_Center_Area = value;
                }
            }
        }

        private bool _Align_Centerr;
        public bool Align_Center
        {
            get { return _Align_Centerr; }
            set
            {
                if (value != _Align_Centerr)
                {
                    _Align_Centerr = value;
                }
            }
        }

        private bool _Centerofthecenterdie;
        public bool Centerofthecenterdie
        {
            get { return _Centerofthecenterdie; }
            set
            {
                if (value != _Centerofthecenterdie)
                {
                    _Centerofthecenterdie = value;
                }
            }
        }
        
    }


    [Serializable]
    public class WaferGraphicsModule : IFactoryModule, IModule
    {
        [System.Diagnostics.DebuggerBrowsable(
            System.Diagnostics.DebuggerBrowsableState.Never)]

        private WaferObject Wafer
        {
            get
            {
                return (WaferObject)this.StageSupervisor().WaferObject;
            }
        }

        [XmlIgnore, JsonIgnore]
        public bool Initialized { get; set; } = false;

        //private CatCoordinates LUcorner = new CatCoordinates();
        //private CatCoordinates RDcorner = new CatCoordinates();
        private WaferCoordinate LUcorner = new WaferCoordinate();
        private WaferCoordinate RDcorner = new WaferCoordinate();
        ObservableCollection<DrawRectangleModule> DrawDieRectangles = new ObservableCollection<DrawRectangleModule>();
        ObservableCollection<DrawRectangleModule> DrawPadRectangles = new ObservableCollection<DrawRectangleModule>();

         
        DrawRectangleModule drawRectangle = new DrawRectangleModule();
        DrawRectangleModule drawCenterDieRectangle = new DrawRectangleModule();
        DrawSVGPathModule drawAlignCenter = new DrawSVGPathModule();
        DrawSVGPathModule drawEdgeCenter = new DrawSVGPathModule();
        DrawLineModule drawCenterLineofthecenterdie_X = new DrawLineModule();
        DrawLineModule drawCenterLineofthecenterdie_Y = new DrawLineModule();
        DrawEllipseModule drawEdgeCenterArea = new DrawEllipseModule();

        //DrawRectangleModule drawPadRectangle = new DrawRectangleModule();
        List<DrawLineModule> drawLines = new List<DrawLineModule>();
        DrawLineModule drawLine = new DrawLineModule();
        List<DrawTextModule> drawTexts = new List<DrawTextModule>();
        DrawTextModule drawText = new DrawTextModule();
        private IDisplay Display;

        public WaferAlignSetupControl WaferAlignSetupControl { get; set; }

        public WaferGraphicsModule() 
        {
            if (WaferAlignSetupControl == null)
            {
                WaferAlignSetupControl = new WaferAlignSetupControl();
            }
        }

        public void DeInitModule()
        {
        }

        public EventCodeEnum InitModule()
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
        public EventCodeEnum SetDisplayModule(IDisplay display)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Display = display;
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum DrawDieOverlay(ImageBuffer img, ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (cam.DisplayService.DrawOverlayContexts.Count != 0)
                {
                    cam.DisplayService.DrawOverlayContexts.Clear();
                }
                #region ..//Die Overlay

                double pt1_x;
                double pt1_y;
                double pt2_x;
                double pt2_y;


                ////==> Mil 화면의 사각의 점들의 Wafer 좌표계 좌표
                double milScreenLeft = img.CatCoordinates.X.Value - (img.SizeX / 2 * img.RatioX.Value);
                double milScreenTop = img.CatCoordinates.Y.Value + (img.SizeY / 2 * img.RatioY.Value);
                double milScreenRight = img.CatCoordinates.X.Value + (img.SizeX / 2 * img.RatioX.Value);
                double milScreenBottom = img.CatCoordinates.Y.Value - (img.SizeY / 2 * img.RatioY.Value);

                double[,] DieCornerPos = new double[2, 2];
                double[,] DisplayCornerPos = new double[2, 2];

                DisplayCornerPos[0, 0] = img.CatCoordinates.X.Value - (img.RatioX.Value * (img.SizeX / 2));
                DisplayCornerPos[0, 1] = img.CatCoordinates.Y.Value - (img.RatioY.Value * (img.SizeY / 2));

                DisplayCornerPos[1, 0] = img.CatCoordinates.X.Value + (img.RatioX.Value * (img.SizeX / 2));
                DisplayCornerPos[1, 1] = img.CatCoordinates.Y.Value + (img.RatioY.Value * (img.SizeY / 2));

                LUcorner.X.Value = DisplayCornerPos[0, 0];
                LUcorner.Y.Value = DisplayCornerPos[0, 1];

                RDcorner.X.Value = DisplayCornerPos[1, 0];
                RDcorner.Y.Value = DisplayCornerPos[1, 1];


                //UserIndex LUcornerUI = this.CoordinateManager().GetCurUserIndex(LUcorner);
                //UserIndex RDcornerUI = this.CoordinateManager().GetCurUserIndex(RDcorner);
                MachineIndex LUcornerUI = this.CoordinateManager().GetCurMachineIndex(LUcorner);
                MachineIndex RDcornerUI = this.CoordinateManager().GetCurMachineIndex(RDcorner);

                //MachineIndex CurDieUI = this.CoordinateManager().GetCurMachineIndex(img.CatCoordinates);
                MachineIndex CurDieUI = cam.GetCurCoordMachineIndex();

                MachineIndex tmpUI = new MachineIndex();

                double DieStartXIndex = (LUcornerUI.XIndex > RDcornerUI.XIndex)
                ? RDcornerUI.XIndex : LUcornerUI.XIndex;
                double DieStartYIndex = (LUcornerUI.YIndex > RDcornerUI.YIndex)
                ? RDcornerUI.YIndex : LUcornerUI.YIndex;

                double DieEndXIndex = (LUcornerUI.XIndex > RDcornerUI.XIndex)
                ? LUcornerUI.XIndex : RDcornerUI.XIndex;
                double DieEndYIndex = (LUcornerUI.YIndex > RDcornerUI.YIndex)
                ? LUcornerUI.YIndex : RDcornerUI.YIndex;


                for (int y = (int)DieStartYIndex; y <= (int)DieEndYIndex; y++)
                {
                    for (int x = (int)DieStartXIndex; x <= (int)DieEndXIndex; x++)
                    {

                        tmpUI.XIndex = x;
                        tmpUI.YIndex = y;

                        //DieCornerPos = this.CoordinateManager().GetAnyDieCornerPos(tmpUI, img.CamType);

                        WaferCoordinate CurPosition = new WaferCoordinate();
                        WaferCoordinate CurDieLeftCorner = new WaferCoordinate();

                        Point ptt = this.WaferAligner().GetLeftCornerPosition(CurPosition.X.Value, CurPosition.Y.Value);

                        MachineIndex MachinDieIndex = this.CoordinateManager().GetCurMachineIndex(CurPosition);
                        
                        WaferCoordinate CurDieCenter = this.WaferAligner().MachineIndexConvertToDieLeftCorner_NonCalcZ(Convert.ToInt32(MachinDieIndex.XIndex), Convert.ToInt32(MachinDieIndex.YIndex));
                        
                        CurDieCenter = this.WaferAligner().MachineIndexConvertToDieLeftCorner_NonCalcZ(Convert.ToInt32(x), Convert.ToInt32(y));
                        ptt = this.WaferAligner().GetLeftCornerPosition(CurDieCenter.X.Value, CurDieCenter.Y.Value);

                        CurDieLeftCorner.X.Value = ptt.X;
                        CurDieLeftCorner.Y.Value = ptt.Y;


                        Point pt = new Point();
                        pt.X = this.WaferAligner().MachineIndexConvertToDieLeftCorner_NonCalcZ(x, y).GetX();
                        pt.Y = this.WaferAligner().MachineIndexConvertToDieLeftCorner_NonCalcZ(x, y).GetY();

                        //DieCornerPos[0, 0] : 좌측X || DieCornerPos[1, 1] : 좌측 Y  || DieCornerPos[1, 0] : 우측 X  || DieCornerPos[1, 1] : 우측 Y 
                        DieCornerPos[0, 0] = pt.X + (Wafer.GetSubsInfo().DieXClearance.Value / 2);
                        DieCornerPos[1, 1] = pt.Y + (Wafer.GetSubsInfo().DieYClearance.Value / 2);
                        DieCornerPos[0, 1] = DieCornerPos[1, 1] + (Wafer.GetSubsInfo().ActualDieSize.Height.Value - (Wafer.GetSubsInfo().DieYClearance.Value));
                        DieCornerPos[1, 0] = DieCornerPos[0, 0] + (Wafer.GetSubsInfo().ActualDieSize.Width.Value - (Wafer.GetSubsInfo().DieXClearance.Value));

                        pt1_x = (DieCornerPos[0, 0] - milScreenLeft) / img.RatioX.Value;
                        pt1_y = (milScreenTop - DieCornerPos[0, 1]) / img.RatioY.Value;

                        pt2_x = (DieCornerPos[1, 0] - milScreenLeft) / img.RatioX.Value;
                        pt2_y = (milScreenTop - (DieCornerPos[1, 1])) / img.RatioY.Value;


                        drawRectangle = new DrawRectangleModule(Math.Truncate(pt1_x + ((pt2_x - pt1_x) / 2)), Math.Truncate(pt1_y - ((pt1_y - pt2_y) / 2)),
                        Math.Abs(pt2_x - pt1_x), Math.Abs(pt1_y - pt2_y),
                        verflip: this.VisionManager().GetDispVerFlip(), horflip: this.VisionManager().GetDispHorFlip(),
                        left: 0, right: img.SizeX, top: 0, bottom: img.SizeY
                        ) ;
                        drawRectangle.Color = Colors.BlueViolet;
                        drawRectangle.Thickness = 3;


                        cam.DisplayService.DrawOverlayContexts.Add(drawRectangle);

                        if (WaferAlignSetupControl != null)
                        {
                            bool iscenterdie = x == Wafer.GetPhysInfo().CenU.XIndex.Value && y == Wafer.GetPhysInfo().CenU.YIndex.Value;
                            if (WaferAlignSetupControl.CenterDie && iscenterdie)
                            {
                                #region ..//CenterDie
                                drawCenterDieRectangle = new DrawRectangleModule(Math.Truncate(pt1_x + ((pt2_x - pt1_x) / 2)), Math.Truncate(pt1_y - ((pt1_y - pt2_y) / 2)),
                                Math.Abs(pt2_x - pt1_x), Math.Abs(pt1_y - pt2_y));
                                drawCenterDieRectangle.Color = Colors.OrangeRed;
                                drawCenterDieRectangle.Thickness = 3;

                                cam.DisplayService.DrawOverlayContexts.Add(drawCenterDieRectangle);

                                drawText = new DrawTextModule(pt1_x, pt1_y, "Center DIE");

                                drawText.BackColor = Colors.WhiteSmoke;
                                drawText.Fontcolor = Colors.Black;
                                drawText.FontSize = 12;
                                cam.DisplayService.DrawOverlayContexts.Add(drawText);
                                #endregion
                            }

                            if (WaferAlignSetupControl.Edge_Center)
                            {
                                #region ..//Edge_Center
                                double wafercenterx = Wafer.GetSubsInfo().WaferCenterOriginatEdge.GetX();
                                double wafercentery = Wafer.GetSubsInfo().WaferCenterOriginatEdge.GetY();

                                if (wafercenterx > milScreenLeft && wafercenterx < milScreenRight &&
                                    wafercentery < milScreenTop && wafercentery > milScreenBottom)
                                {
                                    double pt_x = (wafercenterx - milScreenLeft) / img.RatioX.Value;
                                    double pt_y = (milScreenTop - wafercentery) / img.RatioY.Value;

                                    //string data = "M10 4 L14 4 M20 10 L20 14 M10 20 L14 20 M4 10 L4 14 M20 12 H4 M12 4 V20 M12 4 L12 20 M4 12 L20 12 M16 12 A4 4 0 1 1 8 12 A4 4 0 1 1 16 12";
                                    ////string data = "m13,8l-2,0l0,3l-3,0l0,2l3,0l0,3l2,0l0,-3l3,0l0,-2l-3,0m-1,-8c-4.96,0 -9,4.04 -9,9c0,4.96 4.04,9 9,9c4.96,0 9,-4.04 9,-9c0,-4.96 -4.04,-9 -9,-9m0,16c-3.86,0 -7,-3.14 -7,-7c0,-3.86 3.14,-7 7,-7c3.86,0 7,3.14 7,7c0,3.86 -3.14,7 -7,7z";
                                    //drawAlignCenter = new DrawSVGPathModule(pt_x, pt_y, 86, 86, data);
                                    //drawAlignCenter.Color = Colors.Red;
                                    //drawAlignCenter.Thickness = 1;
                                    //cam.DisplayService.DrawOverlayContexts.Add(drawAlignCenter);

                                    //drawText = new DrawTextModule(pt_x + 20, pt_y - 20, "Wafer Align Center");
                                    //drawText.BackColor = Colors.WhiteSmoke;
                                    //drawText.Fontcolor = Colors.Black;
                                    //drawText.FontSize = 12;
                                    //cam.DisplayService.DrawOverlayContexts.Add(drawText);
                                    if (WaferAlignSetupControl.Edge_Center_Area > 0) 
                                    {
                                        int radius = WaferAlignSetupControl.Edge_Center_Area;

                                        drawEdgeCenterArea = new DrawEllipseModule(pt_x, pt_y, 
                                            (Wafer.GetSubsInfo().ActualDieSize.Width.Value * radius/100) / img.RatioX.Value,
                                            (Wafer.GetSubsInfo().ActualDieSize.Height.Value * radius / 100) / img.RatioY.Value);

                                        drawEdgeCenterArea.Color = Colors.OrangeRed;
                                        drawEdgeCenterArea.Thickness = 1;
                                        drawEdgeCenterArea.StrokeDashOffset = 3;
                                        cam.DisplayService.DrawOverlayContexts.Add(drawEdgeCenterArea);
                                    }
                                }


                                #endregion
                            }

                            if (WaferAlignSetupControl.Align_Center && iscenterdie == true)
                            {
                                #region ..//Align_Center
                                double wafercenterx = Wafer.GetSubsInfo().WaferCenter.GetX();
                                double wafercentery = Wafer.GetSubsInfo().WaferCenter.GetY();

                                if (wafercenterx > milScreenLeft && wafercenterx < milScreenRight &&
                                    wafercentery < milScreenTop && wafercentery > milScreenBottom)
                                {
                                    double pt_x = (wafercenterx - milScreenLeft) / img.RatioX.Value;
                                    double pt_y = (milScreenTop - wafercentery) / img.RatioY.Value;

                                    //string data = "M10 4 L14 4 M20 10 L20 14 M10 20 L14 20 M4 10 L4 14 M20 12 H4 M12 4 V20 M12 4 L12 20 M4 12 L20 12 M16 12 A4 4 0 1 1 8 12 A4 4 0 1 1 16 12";
                                    string data = "m13,8l-2,0l0,3l-3,0l0,2l3,0l0,3l2,0l0,-3l3,0l0,-2l-3,0m-1,-8c-4.96,0 -9,4.04 -9,9c0,4.96 4.04,9 9,9c4.96,0 9,-4.04 9,-9c0,-4.96 -4.04,-9 -9,-9m0,16c-3.86,0 -7,-3.14 -7,-7c0,-3.86 3.14,-7 7,-7c3.86,0 7,3.14 7,7c0,3.86 -3.14,7 -7,7z";
                                    drawAlignCenter = new DrawSVGPathModule(pt_x, pt_y, 86, 86, data);
                                    drawAlignCenter.Color = Colors.Green;
                                    drawAlignCenter.Thickness = 1;
                                    cam.DisplayService.DrawOverlayContexts.Add(drawAlignCenter);

                                    //drawText = new DrawTextModule(pt_x + 20, pt_y - 20, "Wafer Align Center");
                                    //drawText.BackColor = Colors.WhiteSmoke;
                                    //drawText.Fontcolor = Colors.Black;
                                    //drawText.FontSize = 12;
                                    //cam.DisplayService.DrawOverlayContexts.Add(drawText);
                                }

                                #endregion
                            }

                            if (WaferAlignSetupControl.Centerofthecenterdie && iscenterdie == true)
                            {
                                #region ..//Align_Center
                                double pt_x = Math.Truncate(pt1_x + ((pt2_x - pt1_x) / 2));
                                double pt_y = Math.Truncate(pt1_y - ((pt1_y - pt2_y) / 2));

                                drawCenterLineofthecenterdie_X = new DrawLineModule();
                                drawCenterLineofthecenterdie_X.SetParameter(pt_x, pt_y - (Math.Abs(pt1_y - pt2_y) / 2), pt_x, pt_y + (Math.Abs(pt1_y - pt2_y) / 2), Colors.Red, 1, StrokeDashOffset: 2);
                                cam.DisplayService.DrawOverlayContexts.Add(drawCenterLineofthecenterdie_X);

                                drawCenterLineofthecenterdie_Y = new DrawLineModule();
                                drawCenterLineofthecenterdie_Y.SetParameter(pt_x - (Math.Abs(pt2_x - pt1_x) /2), pt_y, pt_x + (Math.Abs(pt2_x - pt1_x) / 2), pt_y, Colors.Red, 1, StrokeDashOffset: 2);
                                cam.DisplayService.DrawOverlayContexts.Add(drawCenterLineofthecenterdie_Y);
                                #endregion
                            }

                        }
                    }
                }
                //cam.DisplayService.Draw(img);

                #endregion
            }

            catch (Exception err)
            {
                //LoggerManager.Error($err, "OverlayDie() Error occurred.");
                LoggerManager.Exception(err);

            }
            return retVal;
        }

        public EventCodeEnum DrawPadOverlay(ImageBuffer img, ICamera cam)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (cam.GetChannelType() == EnumProberCam.WAFER_HIGH_CAM ||
              cam.GetChannelType() == EnumProberCam.WAFER_LOW_CAM)
                {

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Display = cam.DisplayService;

                            for (int i = 0; i < Wafer.GetSubsInfo().Pads.DutPadInfos.Count(); i++)
                            {
                                double[,] DieConrerPos = new double[2, 2];
                                double[,] DisplayConerePos = new double[2, 2];

                                // Draw Current Die Corner

                                DisplayConerePos[0, 0] = img.CatCoordinates.X.Value - (img.RatioX.Value * (img.SizeX / 2));
                                DisplayConerePos[0, 1] = img.CatCoordinates.Y.Value - (img.RatioY.Value * (img.SizeY / 2));

                                DisplayConerePos[1, 0] = img.CatCoordinates.X.Value + (img.RatioX.Value * (img.SizeX / 2));
                                DisplayConerePos[1, 1] = img.CatCoordinates.Y.Value + (img.RatioY.Value * (img.SizeY / 2));

                                double pt1_x;
                                double pt1_y;
                                double pt2_x;
                                double pt2_y;


                                CatCoordinates LUcorner = new CatCoordinates();
                                CatCoordinates RDcorner = new CatCoordinates();

                                LUcorner.X.Value = DisplayConerePos[0, 0];
                                LUcorner.Y.Value = DisplayConerePos[0, 1];

                                RDcorner.X.Value = DisplayConerePos[1, 0];
                                RDcorner.Y.Value = DisplayConerePos[1, 1];

                                UserIndex LUcornerUI = this.CoordinateManager().GetCurUserIndex(LUcorner);
                                UserIndex RDcornerUI = this.CoordinateManager().GetCurUserIndex(RDcorner);

                                UserIndex CurDieUI = this.CoordinateManager().GetCurUserIndex(img.CatCoordinates);

                                UserIndex tmpUI = new UserIndex();

                                double DieStartX = (LUcornerUI.XIndex > RDcornerUI.XIndex)
                                ? RDcornerUI.XIndex : LUcornerUI.XIndex;
                                double DieStartY = (LUcornerUI.YIndex > RDcornerUI.YIndex)
                                ? RDcornerUI.YIndex : LUcornerUI.YIndex;

                                double DieEndX = (LUcornerUI.XIndex > RDcornerUI.XIndex)
                                ? LUcornerUI.XIndex : RDcornerUI.XIndex;
                                double DieEndY = (LUcornerUI.YIndex > RDcornerUI.YIndex)
                                ? LUcornerUI.YIndex : RDcornerUI.YIndex;

                                for (int y = (int)DieStartY; y <= (int)DieEndY; y++)
                                {
                                    for (int x = (int)DieStartX; x <= (int)DieEndX; x++)
                                    {

                                        tmpUI.XIndex = x;
                                        tmpUI.YIndex = y;

                                        DieConrerPos = this.CoordinateManager().GetAnyDieCornerPos(tmpUI, img.CamType);

                                        double CurPositionX = img.CatCoordinates.X.Value;
                                        double CurPositionY = img.CatCoordinates.Y.Value;
                                        int idx = 0;

                                        //==> Mil 화면의 사각의 점들의 Wafer 좌표계 좌표
                                        double milScreenLeft = img.CatCoordinates.X.Value - (img.SizeX / 2 * img.RatioX.Value);
                                        double milScreenTop = img.CatCoordinates.Y.Value + (img.SizeY / 2 * img.RatioY.Value);
                                        double milScreenRight = img.CatCoordinates.X.Value + (img.SizeX / 2 * img.RatioX.Value);
                                        double milScreenBottom = img.CatCoordinates.Y.Value - (img.SizeY / 2 * img.RatioY.Value);


                                        double padstartX = (DieConrerPos[0, 0] + (Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadCenter.X.Value)
                                                + (Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadSizeX.Value / 2));
                                        double padstartY = (DieConrerPos[1, 1] + (Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadCenter.Y.Value)
                                            + (Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadSizeY.Value / 2));
                                        double padendX = padstartX + Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadSizeX.Value;
                                        double padendY = padstartY - Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadSizeY.Value;

                                        if (padstartX > milScreenLeft &&
                                              padstartX < milScreenRight &&
                                              padstartY < milScreenTop &&
                                              padstartY > milScreenBottom)
                                        {
                                            pt1_x = (padstartX - milScreenLeft) / img.RatioX.Value;
                                            pt1_y = (milScreenTop - padstartY) / img.RatioY.Value;

                                            pt2_x = (padendX - milScreenLeft) / img.RatioX.Value;
                                            pt2_y = (milScreenTop - padendY) / img.RatioY.Value;


                                            double padcetnerx = pt1_x - ((pt2_x - pt1_x) / 2);
                                            double padcentery = pt1_y - ((pt1_y - pt2_y) / 2);

                                            idx = Wafer.GetSubsInfo().Pads.DutPadInfos[i].Index.Value;
                                            drawRectangle =
                                            new DrawRectangleModule(
                                                padcetnerx, padcentery, Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadSizeX.Value /
                                                cam.GetRatioX(), Wafer.GetSubsInfo().Pads.DutPadInfos[i].PadSizeY.Value /
                                                cam.GetRatioY());
                                            drawText = new DrawTextModule(pt1_x, pt1_y, idx.ToString());

                                            string str = "Ref Pad";
                                            if (idx == 0)
                                            {
                                                drawText = new DrawTextModule(pt1_x, pt1_y, str);
                                            }
                                            drawText.BackColor = Colors.WhiteSmoke;
                                            drawText.Fontcolor = Colors.Black;
                                            drawText.FontSize = 12;

                                            drawRectangle.Color = Colors.Yellow;

                                            cam.DisplayService.DrawOverlayContexts.Add(drawRectangle);
                                            cam.DisplayService.DrawOverlayContexts.Add(drawText);

                                        }
                                    }
                                }
                            }
                        
                        //cam.DisplayService.Draw(img);
                    }));

                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "OverlayPad() Error occurred.");
                LoggerManager.Exception(err);

            }
            return retVal;
        }

    }
}
