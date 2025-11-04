using System;
using System.Collections.Generic;

namespace ModuleStateView
{
    using DXControlBase;
    using SharpDX.Direct2D1;
    using SharpDX.Mathematics.Interop;
    using SharpDX.DirectWrite;
    using System.ComponentModel;
    using System.Windows;
    using ProberInterfaces;
    using LoaderController;
    using LogModule;

    //public enum EnumNotchType
    //{
    //    UNDEFINED = -1,
    //    NOTCH = 1,
    //    FLAT = 2
    //}

    public class ModuleStateViewControl : D2dControl, INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

        }

        public int stateNum = Enum.GetNames(typeof(ModuleStateEnum)).Length;

        public List<ModuleStateEnum> stateList;

        #region // Properties
        private float _maxZoomLevel;

        public float MaxZoomLevel
        {
            get { return _maxZoomLevel; }
            set { _maxZoomLevel = value; }
        }

        //private float _zoomLevel;

        //public float ZoomLevel
        //{
        //    get { return _zoomLevel; }
        //    set {
        //        MouseDownPosX = TimeMapStartX;
        //        _zoomLevel = value;
        //    }
        //}

        private float _rectWidth;

        public float RectWidth
        {
            get { return _rectWidth; }
            set { _rectWidth = value; }
        }

        private float _rectHeight;

        public float RectHeight
        {
            get { return _rectHeight; }
            set { _rectHeight = value; }
        }
        private float _recSize;

        public float RecSize
        {
            get { return _recSize; }
            set { _recSize = value; }
        }

        private float MouseDownPosX = 0;





        #endregion

        #region // Event handlers


        private void ModuleStateViewControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                IsMouseDown = false;
                IsTimeBarEnable = false;
                viewCurrentCenPos.X = viewCenPos.X;
                viewCurrentCenPos.Y = viewCenPos.Y;
                TempTimeLeft = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void ModuleStateViewControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            try
            {
                if (timeBarWidth != TimeRightBtnStartX - TimeLeftBtnEndX)
                {
                    if (e.Delta > 0)
                    {
                        timeBarLeft += 2;
                    }
                    else
                    {
                        timeBarLeft -= 2;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        bool IsTimeBarEnable = false;
        bool IsTimeBarView = false;
        float TimeDownStartPosX { get; set; }
        float TimeDownEndPosX { get; set; }
        float TempTimeLeft = 0;
        private void ModuleStateViewControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                System.Windows.Point downPos = new System.Windows.Point();
                downPos = e.GetPosition(this);
                MouseDownPos = downPos;
                IsMouseDown = true;
                if (IsTimeBarView && isContainRect(timeBarLeft, TimeBarStartY + 10, timeBarLeft + timeBarWidth, TimeBarEndY - 10))
                {
                    IsTimeBarEnable = true;
                    TimeDownStartPosX = (float)downPos.X;
                    TempTimeLeft = timeBarLeft;
                }
                if (MouseDownPos.X + timeBarWidth > TimeMapEndX)
                {
                    MouseDownPosX = (float)TimeMapEndX - timeBarWidth;
                }
                else
                {
                    MouseDownPosX = (float)MouseDownPos.X;
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void ModuleStateViewControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                IsMouseDown = false;
                IsTimeBarEnable = false;
                viewCurrentCenPos.X = viewCenPos.X;
                viewCurrentCenPos.Y = viewCenPos.Y;
                TempTimeLeft = 0;



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        System.Windows.Point currPos;
        float TimePosOffset = 0;
        private void ModuleStateViewControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                currPos = new System.Windows.Point();
                currPos = e.GetPosition(this);
                if (IsTimeBarView && IsTimeBarEnable)
                {
                    TimeDownEndPosX = (float)currPos.X;
                    TimePosOffset = TimeDownEndPosX - TimeDownStartPosX;
                }
                else
                {
                    TimePosOffset = 0;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion




        public static readonly DependencyProperty ServiceProperty =
            DependencyProperty.Register("Service", typeof(List<ISequenceEngineService>), typeof(ModuleStateViewControl), new FrameworkPropertyMetadata(null));
        public List<ISequenceEngineService> Service
        {
            get { return (List<ISequenceEngineService>)this.GetValue(ServiceProperty); }
            set
            {
                this.SetValue(ServiceProperty, value);
            }
        }


        public static readonly DependencyProperty IsLotModuleProperty =
            DependencyProperty.Register("IsLotModule", typeof(bool), typeof(ModuleStateViewControl), new FrameworkPropertyMetadata(null));
        public bool IsLotModule
        {
            get { return (bool)this.GetValue(IsLotModuleProperty); }
            set
            {
                this.SetValue(IsLotModuleProperty, value);
            }
        }

        public static readonly DependencyProperty IsLoaderModuleProperty =
            DependencyProperty.Register("IsLoaderModule", typeof(bool), typeof(ModuleStateViewControl), new FrameworkPropertyMetadata(null));
        public bool IsLoaderModule
        {
            get { return (bool)this.GetValue(IsLoaderModuleProperty); }
            set
            {
                this.SetValue(IsLoaderModuleProperty, value);
            }
        }

        public static readonly DependencyProperty IsIModuleProperty =
           DependencyProperty.Register("IsIModule", typeof(bool), typeof(ModuleStateViewControl), new FrameworkPropertyMetadata(null));
        public bool IsIModule
        {
            get { return (bool)this.GetValue(IsIModuleProperty); }
            set
            {
                this.SetValue(IsIModuleProperty, value);
            }
        }

        public static readonly DependencyProperty ZoomLevelProperty =
          DependencyProperty.Register("ZoomLevel", typeof(float), typeof(ModuleStateViewControl), new FrameworkPropertyMetadata(null));
        public float ZoomLevel
        {
            get { return (float)this.GetValue(ZoomLevelProperty); }
            set
            {
                this.SetValue(ZoomLevelProperty, value);
            }
        }



        public System.Windows.Point MouseDownPos { get; set; }
        public float GutterSize { get; set; }
        public bool IsMouseDown { get; set; }
        // public bool IsMouseUp { get; set; }






        /// <summary>
        /// Initializing events
        /// </summary>
        public ModuleStateViewControl()
        {
            try
            {
                InitEvents();
                //  _zoomLevel = 1;
                _maxZoomLevel = 6;

                //   RecSize = 5 * _zoomLevel / 5;
                _rectWidth = RecSize;
                _rectHeight = RecSize;
                GutterSize = 2;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        /// <summary>
        /// Event handler adding method
        /// </summary>
        private void InitEvents()
        {
            try
            {
                this.MouseMove += ModuleStateViewControl_MouseMove;
                this.MouseDown += ModuleStateViewControl_MouseDown;
                this.MouseLeave += ModuleStateViewControl_MouseLeave;
                this.MouseWheel += ModuleStateViewControl_MouseWheel;
                this.MouseUp += ModuleStateViewControl_MouseUp;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }




        private void InitColorPallette(RenderTarget target)
        {
            try
            {



                System.Drawing.Color c = System.Drawing.Color.BlueViolet;

                resCache.Add("UndifinedBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(105 / 255f, 105 / 255f, 105 / 255f, 255 / 255f)));
                resCache.Add("InitBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(255 / 255f, 99 / 255f, 71 / 255f, 255 / 255f)));
                resCache.Add("IdleBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(255 / 255f, 165 / 255f, 0 / 255f, 255 / 255f)));
                resCache.Add("RunningBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(50 / 255f, 205 / 255f, 50 / 255f, 255 / 255f)));
                resCache.Add("SuspendedBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(188 / 255f, 143 / 255f, 143 / 255f, 255 / 255f)));
                resCache.Add("AbortBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(186 / 255f, 85 / 255f, 211 / 255f, 255 / 255f)));
                resCache.Add("PausedBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(139 / 255f, 0 / 255f, 0 / 255f, 255 / 255f)));
                resCache.Add("DoneBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(0 / 255f, 128 / 255f, 0 / 255f, 255 / 255f)));
                resCache.Add("RecoveryBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(0 / 255f, 0 / 255f, 255 / 255f, 255 / 255f)));

                resCache.Add("ErrorBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(255 / 255f, 0 / 255f, 0 / 255f, 255 / 255f)));
                resCache.Add("PendingBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(30 / 255f, 144 / 255f, 255 / 255f, 255 / 255f)));
                resCache.Add("TimeBarReadyBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(104 / 255f, 104 / 255f, 104 / 255f, 255 / 255f)));
                resCache.Add("TimeBarBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(80 / 255f, 80 / 255f, 80 / 255f, 255 / 255f)));
                resCache.Add("WhiteBrush", t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(255 / 255f, 255 / 255f, 255 / 255f, 255 / 255f)));



                resCache.Add("ModuleBrush",
                   t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(255 / 255f, 255 / 255f, 255 / 255f, 255 / 255f)));
                resCache.Add("textBrush",
                  t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(255 / 255f, 255 / 255f, 255 / 255f, 1)));
                resCache.Add("lineBrush",
                     t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(70 / 255f, 130 / 255f, 180 / 255f, 255 / 255f)));
                resCache.Add("EdgeBrush",
                  t => new SharpDX.Direct2D1.SolidColorBrush(t, new RawColor4(245 / 255f, 245 / 255f, 245 / 255f, 1)));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //PathGeometry geometry;
        //GeometrySink sink;

        static SharpDX.DirectWrite.Factory fontFactory = new SharpDX.DirectWrite.Factory();

        TextFormat textModuleStateFormat = new TextFormat(fontFactory, "Franklin Gothic", 18.0f);
        TextFormat textFormat = new TextFormat(fontFactory, "Franklin Gothic", 20.0f);
        TextFormat textTimeFormat = new TextFormat(fontFactory, "Franklin Gothic", 20.0f);

        RawVector2 viewCenPos = new RawVector2();
        RawVector2 viewCurrentCenPos = new RawVector2();
        int idx = 0;
        float margin = 0;
        SharpDX.Direct2D1.Brush moduleBrush = null;
        float ModuleRectX = 0;
        float ModuleRectY = 0;
        float ModuleRectWidth = 0;
        float ModuleRectHeight = 0;
        double fovWidth = 0;
        double fovHeight = 0;

        float StateMapStartX = 0;
        float StateMapStartY = 0;
        float StateMapEndX = 0;
        float StateMapEndY = 0;

        float LotOPMapStartX = 0;
        float LotOPMapStartY = 0;
        float LotOPMapEndX = 0;
        float LotOPMapEndY = 0;


        float TimeMapStartX = 0;
        float TimeMapStartY = 0;
        float TimeMapEndX = 0;
        float TimeMapEndY = 0;


        float TimeBarStartX = 0;
        float TimeBarStartY = 0;
        float TimeBarEndX = 0;
        float TimeBarEndY = 0;

        float TimeLeftBtnStartX = 0;
        float TimeLeftBtnStartY = 0;
        float TimeLeftBtnEndX = 0;
        float TimeLeftBtnEndY = 0;

        float TimeRightBtnStartX = 0;
        float TimeRightBtnStartY = 0;
        float TimeRightBtnEndX = 0;
        float TimeRightBtnEndY = 0;


        float MaxTimelength = 0;
        float timeBarLeft = 0;
        float TimeBarOffsetX = 0;
        float timeBarWidth = 0;
        float tmpZoomLevel = 0;
        List<IStateModule> moduleList = new List<IStateModule>();
        /// <summary>
        /// Does the actual rendering. 
        /// BeginDraw and EndDraw are already called by the caller. 
        /// </summary>
        public override void Render(RenderTarget target)
        {
            try
            {
                moduleList.Clear();
                MaxTimelength = 0;
                idx = 0;
                moduleBrush = null;
                margin = (float)(this.ActualHeight * 0.005);

                StateMapStartX = margin;
                StateMapStartY = margin;
                StateMapEndX = (float)this.ActualWidth - margin;
                StateMapEndY = (float)(this.ActualHeight * 0.05);

                LotOPMapStartX = margin;
                LotOPMapStartY = margin;
                LotOPMapEndX = (float)this.ActualWidth / 6;
                LotOPMapEndY = (float)this.ActualHeight - margin;


                TimeMapStartX = LotOPMapEndX + margin;
                TimeMapStartY = margin;
                TimeMapEndX = (float)this.ActualWidth - margin;
                TimeMapEndY = (float)this.ActualHeight - margin * 10;


                TimeBarStartX = LotOPMapEndX + margin;
                TimeBarStartY = TimeMapEndY;
                TimeBarEndX = (float)this.ActualWidth - margin;
                TimeBarEndY = (float)this.ActualHeight - margin;



                TimeLeftBtnStartX = margin;
                TimeLeftBtnStartY = TimeBarStartY;
                TimeLeftBtnEndX = margin * 7;
                TimeLeftBtnEndY = (float)this.ActualHeight - margin;

                TimeRightBtnStartX = (float)this.ActualWidth - margin * 8;
                TimeRightBtnStartY = TimeBarStartY;
                TimeRightBtnEndX = (float)this.ActualWidth - margin;
                TimeRightBtnEndY = (float)this.ActualHeight - margin;

                if (ZoomLevel == 0)
                {
                    ZoomLevel = 1;
                }
                if (tmpZoomLevel != ZoomLevel)
                {
                    MouseDownPosX = TimeLeftBtnStartX;
                    tmpZoomLevel = ZoomLevel;
                    timeBarLeft = TimeLeftBtnStartX;
                    TimeBarOffsetX = 0;
                }

                float min = 0;
                int iterCount = 0;
                int increaseNum = 0;
                string Strtime = null;
                double SecondWidth = 0;
                if (ZoomLevel == 1)
                {
                    min = 1;
                    Strtime = "s";
                    iterCount = 6;
                    increaseNum = 10;
                }
                else if (ZoomLevel == 2)
                {
                    min = 5;
                    iterCount = 5;
                    Strtime = "m";
                    increaseNum = 1;
                }
                else if (ZoomLevel == 3)
                {
                    min = 10;
                    iterCount = 10;
                    Strtime = "m";
                    increaseNum = 1;
                }
                else if (ZoomLevel == 4)
                {
                    min = 30;
                    iterCount = 6;
                    Strtime = "m";
                    increaseNum = 5;
                }
                else if (ZoomLevel == 5)
                {
                    min = 60;
                    iterCount = 6;
                    Strtime = "m";
                    increaseNum = 10;
                }
                else if (ZoomLevel == 6)
                {

                    min = 240;
                    iterCount = 4;
                    Strtime = "h";
                    increaseNum = 1;
                }




                SecondWidth = 60 * min;
                fovWidth = (TimeMapEndX - TimeMapStartX) / SecondWidth;
                fovHeight = TimeMapEndY / SecondWidth;



                target.Clear(new RawColor4(30 / 255f, 30 / 255f, 30 / 255f, 1));

                if (stateList == null)
                {
                    stateList = new List<ModuleStateEnum>();

                    foreach (ModuleStateEnum enumItem in Enum.GetValues(typeof(ModuleStateEnum)))
                    {
                        stateList.Add(enumItem);
                    }
                }

                InitColorPallette(target);

                try
                {
                    if (Service != null)
                    {
                        float stateRectX = StateMapEndX / (stateList.Count + 1);
                        float stateIntervalX = StateMapEndX / (stateList.Count);

                        Brush lineBrush = resCache["lineBrush"] as SharpDX.Direct2D1.Brush;
                        RawVector2 EdgeLineLT = new RawVector2();
                        EdgeLineLT.X = margin;
                        EdgeLineLT.Y = margin;

                        RawVector2 EdgeLineLineRB = new RawVector2();
                        EdgeLineLineRB.X = (float)this.ActualWidth - margin;
                        EdgeLineLineRB.Y = (float)this.ActualHeight - margin;

                        RawVector2 EdgeLineLineRT = new RawVector2();
                        EdgeLineLineRT.X = (float)this.ActualWidth - margin;
                        EdgeLineLineRT.Y = margin;

                        RawVector2 EdgeLineLineLB = new RawVector2();
                        EdgeLineLineLB.X = margin;
                        EdgeLineLineLB.Y = (float)this.ActualHeight - margin;

                        target.DrawLine(EdgeLineLT, EdgeLineLineLB, lineBrush);
                        target.DrawLine(EdgeLineLT, EdgeLineLineRT, lineBrush);
                        target.DrawLine(EdgeLineLineRT, EdgeLineLineRB, lineBrush);
                        target.DrawLine(EdgeLineLineLB, EdgeLineLineRB, lineBrush);



                        RawVector2 StateLineL = new RawVector2();
                        StateLineL.X = margin;
                        StateLineL.Y = TimeBarStartY;

                        RawVector2 StateLineR = new RawVector2();
                        StateLineR.X = (float)this.ActualWidth - margin;
                        StateLineR.Y = TimeBarStartY;




                        target.DrawLine(StateLineL, StateLineR, lineBrush);
                        textTimeFormat = new TextFormat(fontFactory, "Franklin Gothic", 10.0f);
                        textFormat = new TextFormat(fontFactory, "Franklin Gothic", 10.0f);
                        DateTime time = DateTime.Now;
                        for (int i = 0; i < iterCount; i++)// Time Line and String
                        {
                            RawVector2 currentPosLineTop = new RawVector2();
                            currentPosLineTop.X = TimeMapStartX + (TimeMapEndX - TimeMapStartX) / iterCount * i;
                            currentPosLineTop.Y = TimeMapStartY;

                            RawVector2 currentPosLineBottom = new RawVector2();
                            currentPosLineBottom.X = TimeMapStartX + (TimeMapEndX - TimeMapStartX) / iterCount * i;
                            currentPosLineBottom.Y = TimeMapEndY - 25;
                            target.DrawLine(currentPosLineTop, currentPosLineBottom, resCache["ModuleBrush"] as SharpDX.Direct2D1.Brush);
                            int interval = i * increaseNum;
                            if (i == 0)
                            {
                                // target.DrawText(string.Format("{0}", time), textTimeFormat, new RawRectangleF(TimeMapStartX + (TimeMapEndX - TimeMapStartX) / iterCount * i, TimeMapStartY - 10 - margin, TimeMapStartX + (TimeMapEndX - TimeMapStartX) / iterCount * i + 100, TimeMapStartY), resCache["ModuleBrush"] as SharpDX.Direct2D1.Brush);
                            }
                            target.DrawText(string.Format("{0}", interval.ToString() + Strtime), textFormat, new RawRectangleF(TimeMapStartX + (TimeMapEndX - TimeMapStartX) / iterCount * i - 10, TimeMapEndY - 20 - margin, TimeMapStartX + (TimeMapEndX - TimeMapStartX) / iterCount * i + 50, TimeMapEndY), resCache["ModuleBrush"] as SharpDX.Direct2D1.Brush);
                        }


                        ModuleRectHeight = 0;
                        float fonSize = 0;
                        int ModuleListCnt = GetModuleCount(Service);
                        foreach (ISequenceEngineService Sequence in Service)
                        {
                            if (ModuleListCnt < 20)
                            {
                                ModuleListCnt = 20;
                            }
                            ModuleRectX = LotOPMapStartX + margin;
                            ModuleRectY = LotOPMapStartY + margin;
                            ModuleRectWidth = LotOPMapEndX - margin;
                            ModuleRectHeight = (LotOPMapEndY - LotOPMapStartY - 200) / (ModuleListCnt);
                            fonSize = 40 - ModuleListCnt;

                            textFormat = new TextFormat(fontFactory, "Franklin Gothic", fonSize);
                            textFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;

                            if (Sequence is ILotOPModule)
                            {
                                if (!IsLotModule) continue;
                                ILotOPModule lotModule = (ILotOPModule)Sequence;
                                List<IStateModule> ModuleList = lotModule.RunList;

                                if (ModuleList != null)
                                {
                                    DrawModule(target, lotModule);
                                    foreach (IStateModule module in ModuleList)
                                    {
                                        DrawModule(target, module);
                                    }


                                }
                            }
                            else if (Sequence is ILoaderOPModule)
                            {
                                if (!IsLoaderModule) continue;
                                ILoaderOPModule LoaderModule = (ILoaderOPModule)Sequence;
                                List<IStateModule> ModuleList = LoaderModule.RunList;

                                if (ModuleList != null)
                                {
                                    DrawModule(target, LoaderModule);
                                    foreach (IStateModule module in ModuleList)
                                    {
                                        DrawModule(target, module);
                                        var loadercontrollor = module as LoaderController;
                                        //ModuleStateEnum=  loadercontrollor.LoaderInfo.ModuleInfo.ModuleState;
                                    }
                                }
                            }
                            else if (Sequence is IStateModule && IsIModule)
                            {
                                IStateModule module = (IStateModule)Sequence;
                                DrawModule(target, module);
                            }
                        }







                        timeBarWidth = TimeRightBtnStartX - TimeLeftBtnEndX;

                        if (MaxTimelength > 0)
                        {
                            if (TempTimeLeft != 0)
                            {
                                timeBarLeft = TempTimeLeft + TimePosOffset;
                            }
                            timeBarWidth = timeBarWidth * (timeBarWidth / (MaxTimelength - TimeLeftBtnEndX));

                            TimeBarOffsetX = (float)(((timeBarLeft - TimeLeftBtnEndX) * (TimeRightBtnStartX - TimeLeftBtnEndX)) / timeBarWidth);

                        }

                        textFormat = new TextFormat(fontFactory, "Franklin Gothic", 30.0f);
                        if (timeBarLeft < TimeLeftBtnEndX)
                        {
                            timeBarLeft = TimeLeftBtnEndX;
                        }
                        else if (timeBarLeft + timeBarWidth > TimeRightBtnStartX)
                        {
                            timeBarLeft = TimeRightBtnStartX - timeBarWidth;
                        }
                        if (timeBarWidth != TimeRightBtnStartX - TimeLeftBtnEndX)
                        {
                            string DirLeftStr = "";
                            string DirRightStr = "";
                            if (isContainRect(TimeLeftBtnStartX, TimeLeftBtnStartY, TimeLeftBtnEndX, TimeLeftBtnEndY))
                            {
                                if (IsMouseDown)
                                {
                                    timeBarLeft -= 1;
                                }
                                DirLeftStr = "◀";
                            }
                            else
                            {
                                DirLeftStr = "◁";
                            }

                            if (isContainRect(TimeRightBtnStartX, TimeRightBtnStartY, TimeRightBtnEndX, TimeRightBtnEndY))
                            {
                                if (IsMouseDown)
                                {
                                    timeBarLeft += 1;
                                }
                                DirRightStr = "▶";
                            }
                            else
                            {
                                DirRightStr = "▷";
                            }
                            target.DrawText(DirLeftStr, textFormat, new RawRectangleF(TimeLeftBtnStartX, TimeLeftBtnStartY - margin, TimeLeftBtnEndX, TimeLeftBtnEndY), resCache["ModuleBrush"] as SharpDX.Direct2D1.Brush);
                            target.DrawText(DirRightStr, textFormat, new RawRectangleF(TimeRightBtnStartX, TimeRightBtnStartY - margin, TimeRightBtnEndX, TimeRightBtnEndY), resCache["ModuleBrush"] as SharpDX.Direct2D1.Brush);
                            Brush brush = resCache["TimeBarBrush"] as SharpDX.Direct2D1.Brush;
                            if (isContainRect(timeBarLeft, TimeBarStartY + 10, timeBarLeft + timeBarWidth, TimeBarEndY - 10))
                            {
                                brush = resCache["TimeBarReadyBrush"] as SharpDX.Direct2D1.Brush;
                            }
                            if (IsTimeBarEnable && IsTimeBarView)
                            {
                                brush = resCache["ModuleBrush"] as SharpDX.Direct2D1.Brush;
                            }

                            target.FillRectangle(new RawRectangleF(timeBarLeft, TimeBarStartY + 10, timeBarLeft + timeBarWidth, TimeBarEndY - 10), brush);
                            IsTimeBarView = true;
                        }
                        else
                        {
                            IsTimeBarView = false;
                        }
                    }

                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public int GetModuleCount(List<ISequenceEngineService> Service)
        {
            int moduleCnt = 0;
            try
            {
                foreach (ISequenceEngineService Sequence in Service)
                {

                    if (Sequence is ILotOPModule)
                    {
                        ILotOPModule lotModule = (ILotOPModule)Sequence;
                        List<IStateModule> ModuleList = lotModule.RunList;
                        moduleCnt += ModuleList.Count + 1;
                    }
                    else if (Sequence is ILoaderOPModule)
                    {
                        ILoaderOPModule LoaderModule = (ILoaderOPModule)Sequence;
                        List<IStateModule> ModuleList = LoaderModule.RunList;
                        moduleCnt += ModuleList.Count + 1;
                    }
                    else if (Sequence is IStateModule)
                    {
                        moduleCnt++;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return moduleCnt;
        }
        public Brush GetStateBrush(ModuleStateEnum state)
        {
            Brush moduleBrush = null;
            try
            {
                ModuleStateEnum convertState = (ModuleStateEnum)((int)state % 0x100);
                switch (convertState)
                {
                    case ModuleStateEnum.UNDEFINED:
                        moduleBrush = resCache["UndifinedBrush"] as SharpDX.Direct2D1.Brush;
                        break;
                    case ModuleStateEnum.INIT:
                        moduleBrush = resCache["InitBrush"] as SharpDX.Direct2D1.Brush;
                        break;
                    case ModuleStateEnum.IDLE:
                        moduleBrush = resCache["IdleBrush"] as SharpDX.Direct2D1.Brush;
                        break;
                    case ModuleStateEnum.RUNNING:
                        moduleBrush = resCache["RunningBrush"] as SharpDX.Direct2D1.Brush;
                        break;
                    case ModuleStateEnum.SUSPENDED:
                        moduleBrush = resCache["SuspendedBrush"] as SharpDX.Direct2D1.Brush;
                        break;
                    case ModuleStateEnum.DONE:
                        moduleBrush = resCache["DoneBrush"] as SharpDX.Direct2D1.Brush;
                        break;
                    case ModuleStateEnum.ERROR:
                        moduleBrush = resCache["ErrorBrush"] as SharpDX.Direct2D1.Brush;
                        break;
                    case ModuleStateEnum.ABORT:
                        moduleBrush = resCache["AbortBrush"] as SharpDX.Direct2D1.Brush;
                        break;
                    case ModuleStateEnum.PAUSED:
                        moduleBrush = resCache["PausedBrush"] as SharpDX.Direct2D1.Brush;
                        break;
                    case ModuleStateEnum.PENDING:
                        moduleBrush = resCache["PendingBrush"] as SharpDX.Direct2D1.Brush;
                        break;
                    case ModuleStateEnum.RECOVERY:
                        moduleBrush = resCache["RecoveryBrush"] as SharpDX.Direct2D1.Brush;
                        break;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return moduleBrush;
        }

        public void DrawModule(RenderTarget target, IStateModule module)
        {
            try
            {
                if (module.ModuleState != null)
                {

                    moduleList.Add(module);
                    idx++;
                    float moduleLeft = ModuleRectX;
                    float moduleRight = moduleLeft + ModuleRectWidth;
                    float moduleTop = ModuleRectY + (ModuleRectHeight * (idx - 1)) - margin;
                    float moduleBottom = moduleTop + ModuleRectHeight;
                    moduleBrush = GetStateBrush(module.ModuleState.GetState());
                    string str = module.ToString().Substring(module.ToString().IndexOf('.') + 1);
                    if (str.Length > 17)
                    {
                        str = str.Substring(0, 17) + "..";
                    }
                    if (module.ModuleState.GetState() == ModuleStateEnum.RUNNING)
                    {
                        target.FillRectangle(new RawRectangleF(moduleLeft, moduleTop, moduleRight - margin, moduleBottom - margin * 2), moduleBrush);
                        target.DrawText(string.Format("{0}", str), textFormat, new RawRectangleF(moduleLeft, moduleTop, moduleRight, moduleBottom), resCache["WhiteBrush"] as SharpDX.Direct2D1.Brush);
                    }
                    else
                    {
                        target.DrawText(string.Format("{0}", str), textFormat, new RawRectangleF(moduleLeft, moduleTop, moduleRight, moduleBottom), moduleBrush); //Module Text
                    }
                    float prevRight = ModuleRectX + ModuleRectWidth;
                    for (int i = module.TransitionInfo.Count - 1; i >= 0; i--)
                    {
                        if (module.ModuleState != null)
                        {
                            double offset = 1;
                            double firstOffset = 1;
                            if (module.TransitionInfo.Count == 1) //1개일 경우
                            {
                                TimeSpan FirstelsTime = DateTime.Now - module.TransitionInfo[i].TransitionTime;
                                firstOffset = FirstelsTime.TotalMilliseconds / 1000;
                            }
                            else
                            {
                                if (i != module.TransitionInfo.Count - 1)
                                {
                                    TimeSpan elsTime = module.TransitionInfo[i + 1].TransitionTime - module.TransitionInfo[i].TransitionTime;
                                    offset = elsTime.TotalMilliseconds / 1000;
                                }
                                else
                                {
                                    TimeSpan FirstelsTime = DateTime.Now - module.TransitionInfo[i].TransitionTime;
                                    firstOffset = FirstelsTime.TotalMilliseconds / 1000;

                                }
                            }

                            float left = prevRight;
                            float top = moduleTop + margin * 2;//ModuleRectY * idx - margin;
                            float right = left + ((float)fovWidth * (float)firstOffset * (float)offset);
                            float bottom = moduleBottom - margin * 2;// top + stateRectHeight;
                            if (right > TimeMapEndX)
                            {
                                if (MaxTimelength < right)
                                {
                                    MaxTimelength = right;
                                }
                            }
                            prevRight = right;
                            left -= TimeBarOffsetX;
                            right -= TimeBarOffsetX;
                            if (left <= TimeMapStartX)
                            {
                                left = TimeMapStartX;

                            }
                            if (left > TimeMapEndX)
                            {
                                left = TimeMapEndX;
                            }
                            if (right <= TimeMapStartX)
                            {
                                right = TimeMapStartX;
                            }

                            //right -= TimeBarOffsetX;

                            if (right > TimeMapEndX)
                            {
                                right = TimeMapEndX;
                            }

                            RawVector2 currentPosLineLeft = new RawVector2();
                            currentPosLineLeft.X = margin;
                            currentPosLineLeft.Y = (float)(bottom + 10);

                            RawVector2 currentPosLineRight = new RawVector2();
                            currentPosLineRight.X = (float)this.ActualWidth - margin;
                            currentPosLineRight.Y = bottom + 10;

                            Brush brush = null;
                            brush = GetStateBrush(module.TransitionInfo[i].state);
                            if (isContainRect(left, top, right, bottom))
                            {
                                float timeLeft = right;
                                if (timeLeft + 100 > this.TimeMapEndX)
                                {
                                    timeLeft = TimeMapEndX - 100;
                                }
                                if (module.TransitionInfo[i].Reason != null)
                                {
                                    target.DrawText(string.Format("{0}", module.TransitionInfo[i].TransitionTime + "{Reason : " + module.TransitionInfo[i].Reason + " }"), textTimeFormat, new RawRectangleF(timeLeft, bottom, timeLeft + 100, bottom + 30), brush);
                                }
                                else
                                {
                                    target.DrawText(string.Format("{0}", module.TransitionInfo[i].TransitionTime), textTimeFormat, new RawRectangleF(timeLeft, bottom, timeLeft + 100, bottom + 30), brush);
                                }
                            }
                            target.FillRectangle(new RawRectangleF(left, top, right, bottom), brush);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public void DrawCommand()
        {

        }
        public bool isContainRect(float left, float top, float right, float bottom)
        {
            bool retVal = false;
            try
            {
                if (left <= currPos.X && currPos.X <= right && top <= currPos.Y && currPos.Y <= bottom)
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
}
